#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Deterministic, non-agentic reconciler for `ci-scan` / `ci-scan-net11` tracking issues.

.DESCRIPTION
    Surveys the open scanner tracking issues for one twin, decides (deterministically)
    what should happen to each, and — ONLY in an explicitly requested mutating mode —
    applies labels, comments, and closures.

    MODES
      report   (DEFAULT)  Read-only. Emits a full evidence report. ZERO writes.
      comment             Report + apply reconciler-owned labels and at most one
                          candidate-notice comment per issue. Never closes.
      enforce             Everything in `comment`, plus closing eligible candidates and
                          reopening incorrectly-closed ones.

    Any -Mode value that is not the exact, case-sensitive string 'comment' or 'enforce'
    is normalized to 'report'. There is no "unknown mode" error path that could leave a
    caller believing a mutation happened.

    DEFENCE IN DEPTH
      1. Host level   — the workflow's report job is granted `issues: read` only, so its
                        token physically cannot mutate. The mutating job additionally
                        requires workflow_dispatch + a protected deployment environment.
      2. Script level — `Invoke-GhWrite` is the single choke point for every mutating
                        API call and re-checks the effective mode on every invocation.
      3. Logic level  — the pure core (`CiScanReconcile.Core.ps1`) has no vocabulary for
                        "close": the strongest thing it can say about an OPEN issue is
                        'candidate'. Its only other decision, 'reopen', is emitted for
                        already-CLOSED issues and is recoverable by construction — the
                        worst outcome of a wrong reopen is a maintainer re-closing an
                        issue, whereas a wrong close silently buries a live failure.
                        Both still travel through `Invoke-GhWrite`, and 'reopen' is
                        gated on the same `ClosuresAllowed` flag that permits closing.

    Issue numbers are only ever obtained from a GitHub listing filtered by label +
    creator + state and then re-verified by `Test-CiScanIssueProvenance`. No integer
    parsed out of any issue body, PR body, comment, or CI log can select an issue for
    mutation; such integers are used exclusively to BLOCK mutation.

.PARAMETER Mode
    'report' (default), 'comment', or 'enforce'. See above.

.PARAMETER Label
    'ci-scan' (main twin) or 'ci-scan-net11' (net11.0 twin).

.EXAMPLE
    pwsh -File .github/scripts/Invoke-CiScanReconcile.ps1 -Label ci-scan-net11
    # Read-only report for the net11 twin.

.NOTES
    Requires: gh CLI authenticated with read access. AzDO is queried anonymously.
#>

[CmdletBinding()]
param(
    [string]$Mode = 'report',
    [ValidateSet('ci-scan', 'ci-scan-net11')]
    [string]$Label = 'ci-scan',
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [string]$OutputPath = '',
    [string]$SummaryPath = '',
    [int]$MaxIssues = 300,
    # Sized against live counts with headroom: a read-only run on 2026-07-28 saw 52 open
    # `ci-scan` issues, 58 `ci-scan-net11`, and 296 open pull requests repo-wide.
    #
    # The PR bound is the one to watch. It indexes EVERY open PR, not just `[ci-fix]` ones,
    # because an open PR referencing an issue is a closure blocker whoever opened it. So it
    # tracks total repo PR volume, and 296/400 is only ~26% headroom. If open PRs ever
    # exceed this, `Get-CiScanPullRequestIndex` reports incomplete, the run fail-closes on
    # `pull-request-index-incomplete`, and NOTHING is ever closed again.
    #
    # That direction is safe — a short blocker index must never license a close — but it is
    # silent, so raise this bound rather than letting the reconciler quietly stop working.
    # The summary's `PR blocker index complete` row is what makes it visible at all.
    [int]$MaxPullRequests = 400,
    # Skips the AzDO round-trips. Coverage then reports as unverifiable, which fails
    # closed (no absences count), so this can never make the run more aggressive.
    [switch]$SkipAzdo
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
# `gh` legitimately exits non-zero for expected conditions (404, rate limit). Graceful
# degradation depends on inspecting $LASTEXITCODE instead of terminating at invocation.
$PSNativeCommandUseErrorActionPreference = $false

. (Join-Path $PSScriptRoot 'CiScanReconcile.Core.ps1')

#region Mode gate -------------------------------------------------------------

function Set-CiScanReconcileMode {
    <#
    .SYNOPSIS
        Normalizes a requested mode into the effective mode and permission flags.
    .DESCRIPTION
        Case-sensitive allow-list. 'Enforce', 'ENFORCE', 'enforce ', 'enforce;rm -rf /',
        '', $null, 'shadow', and every other value collapse to 'report'. There is no
        error path — an unrecognised mode must never abort in a way that leaves a caller
        unsure whether a mutation happened; it simply does nothing mutating.
    #>
    [CmdletBinding()]
    param([AllowNull()][AllowEmptyString()][string]$RequestedMode)

    $script:EffectiveMode = 'report'
    if ($RequestedMode -ceq 'comment') { $script:EffectiveMode = 'comment' }
    elseif ($RequestedMode -ceq 'enforce') { $script:EffectiveMode = 'enforce' }

    $script:MutationsAllowed = ($script:EffectiveMode -ceq 'comment' -or $script:EffectiveMode -ceq 'enforce')
    $script:ClosuresAllowed = ($script:EffectiveMode -ceq 'enforce')
    return $script:EffectiveMode
}

function Reset-CiScanCounters {
    [CmdletBinding()]
    param()
    $script:Counters = @{ Closes = 0; Reopens = 0; Comments = 0; LabelOps = 0; Writes = 0; ReadErrors = 0; WriteErrors = 0 }
    # Set to a human-readable description of the write that stopped the apply loop.
    # $null on a clean run. Surfaced in the result and the step summary so the operator
    # sees exactly which issue was left mid-sequence.
    $script:AbortedAt = $null
}

$script:SkipAzdo = [bool]$SkipAzdo
# Stands in for the author of a comment that cannot be attributed to any account —
# GitHub returns `user: null` once the account is deleted. Recorded instead of the
# (absent) login so `Get-CiScanHumanCommenters` can veto the issue without inventing a
# plausible-looking username.
#
# Two constraints, and they pull in opposite directions. It must be impossible to
# confuse with a real commenter: GitHub logins are alphanumerics and single interior
# hyphens only, so the parentheses here can never appear in one. And it must survive
# MARKDOWN rendering, because this value reaches the operator through the run's step
# summary (via the `human-comment:` verdict signal, rendered by `Format-...Summary`).
# An angle-bracketed form would satisfy the first constraint and fail the second — GFM
# would treat it as an HTML tag and swallow it, leaving the operator a bare
# `human-comment:` with the reason silently deleted.
$script:CiScanUnattributableCommenter = '(deleted-account)'
# How many builds newer than the state marker's horizon the coverage layer will fetch
# before it gives up and fails closed.
#
# This is a driver-layer AzDO budget, not a staleness policy, which is why it lives here
# rather than in `CiScanReconcile.Core.ps1` with the decision thresholds: it bounds HTTP
# calls (two per probed build), and Core has no AzDO concept at all.
#
# The value is a real trade-off in one direction only. Too low and a healthy twin whose
# scanner is merely a few builds behind fails closed to `needs-human` — noisy, but safe.
# Too high and a grossly stale marker gets probed for a long time before reaching the same
# answer, at real API cost. 20 covers roughly a day of `main`/`net11.0` CI activity, which
# is comfortably more than the lag between scanner runs and far less than the weeks-long
# gap that the "June marker read in August" failure describes.
$script:CiScanMaxNewerBuildsProbed = 20
# The repository every write must name, resolved once from this run's own parameters.
# Assigned with `$script:` rather than read back off the param variables inside the write
# choke point: a param variable resolves to whatever scope bound it, which is the caller's
# when the script is dot-sourced, so `Get-Variable -Scope Script` for `Owner` finds nothing
# under Pester and the choke point fails closed on every legitimate call. This is the same
# mechanism `$script:MutationsAllowed` already relies on -- written and read through the
# same scope by functions in this file.
$script:TargetRepo = "$Owner/$Repo"

<#
    The argument contract for every operation the reconciler may perform. Hoisted to
    script scope rather than left inline in Invoke-GhWrite for one reason: the
    declared-but-unshaped state has to be INDUCIBLE, or the refusal that handles it
    cannot be pinned.

    That was measured. With the table inline, deleting the named refusal left the suite
    at 290/290 green -- the state it guards is unreachable in unmutated code, so no test
    could bind to it, and the only thing distinguishing "refused by name" from "crashed
    on a property lookup" was unasserted. A test can now remove a key, observe which
    refusal fires, and put it back.

    This weakens nothing: the table was already effectively constant, it is written once
    here, and Invoke-GhWrite is the sole reader.
#>
$script:CiScanWriteShapes = @{
    label   = @{ Verb = 'edit'; Flag = '--add-label';    Allowed = @('--repo', '--add-label') }
    unlabel = @{ Verb = 'edit'; Flag = '--remove-label'; Allowed = @('--repo', '--remove-label') }
    body    = @{ Verb = 'edit'; Flag = '--body';         Allowed = @('--repo', '--body') }
    comment = @{ Verb = 'comment'; Flag = '--body';      Allowed = @('--repo', '--body') }
    close   = @{ Verb = 'close'; Flag = $null;           Allowed = @('--repo', '--reason', '--comment') }
    reopen  = @{ Verb = 'reopen'; Flag = $null;          Allowed = @('--repo', '--comment') }
}
$null = Set-CiScanReconcileMode -RequestedMode $Mode
Reset-CiScanCounters

#endregion

#region I/O primitives --------------------------------------------------------

function Test-CiScanRequestShapingArg {
    <#
    .SYNOPSIS
        True if a `gh` argument could turn a default-GET request into a write.
    .DESCRIPTION
        `gh api` documents two independent ways to leave GET:

          * an explicit method flag (`-X` / `--method`), and
          * ANY request parameter — "adding request parameters will automatically switch
            the request method to POST". That covers `-F`/`--field`, `-f`/`--raw-field`
            and `--input` alike, so `-f`/`--raw-field` is the same class as `-F`/`--field`
            and must be refused too.

        Matching is done on the flag's syntactic forms rather than on exact strings,
        because `pflag` accepts a value attached to the flag (`--method=POST`, `-XPOST`,
        `-fstate=closed`), and an exact-string list silently misses every attached form.

        Shorthand matching is deliberately anchored at `^-[XFf]` rather than scanning the
        whole cluster: `pflag` treats everything after a value-taking shorthand as that
        shorthand's value, so a scan would falsely reject innocuous arguments such as
        `-q.foo` (the `f` there is jq syntax, not a flag). Every shaping shorthand takes a
        value, so it can only ever appear first in a cluster.

        Case-sensitive on purpose: `-F` and `-f` are distinct gh flags, and both are
        refused here, but `-X` must not be conflated with an unrelated `-x`.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][AllowEmptyString()][string]$Argument)

    if ($Argument -cmatch '^--([^=]+)') {
        return ($Matches[1] -cin @('method', 'field', 'raw-field', 'input'))
    }
    return ($Argument -cmatch '^-[XFf]')
}

function Invoke-GhRead {
    <#
    .SYNOPSIS
        Executes a read-only `gh` invocation and returns the parsed JSON payload as an
        ARRAY, or $null if — and only if — the read did not succeed.
    .DESCRIPTION
        Only ever used for GET-shaped calls. Failures return $null and increment
        ReadErrors; the caller must treat $null as "unknown", which always resolves in
        the conservative direction.

        THE RETURN IS ALWAYS AN ARRAY ON SUCCESS, and that is load-bearing rather than
        stylistic. `$text | ConvertFrom-Json` writes each element of a JSON array to the
        pipeline separately, so an EMPTY listing (`[]`) writes nothing at all and arrives
        at the caller as `$null` — indistinguishable from the failure returns above.
        Every caller here tests `$null -eq $result` to mean "the read failed", so the
        healthy empty listing was being read as a failed read:

          * `Get-CiScanOpenIssues` marked a zero-open backlog `Truncated`, which fails
            the whole run closed and takes the REOPEN safety net down with it — on
            exactly the day the last tracker closes, which is the steady state this tool
            exists to reach.
          * `Get-CiScanPullRequestIndex` reported `Complete = $false` for a repo with no
            matching pull requests.
          * `Get-CiScanHumanCommenters` reported an issue with zero comments as an
            unreadable comment history.

        Collecting the pipeline's OUTPUT with `@( <pipeline> )` fixes all of them at the
        source: no output stays an empty array rather than collapsing to `$null`. The
        collection must stay in the direct-pipeline form and must be reached by
        ASSIGNMENT, never by `return @(...)`, because `return` unrolls a zero- or
        one-element array and would reinstate the very collapse this exists to prevent.
        The single `return ,$payload` below carries the wrapping comma for that reason.

        This is the same `[]`-to-`$null` gotcha already documented and handled on the
        AzDO side of the reconciler in `Get-CiScanBuildsAfter`; it is handled here, at
        the single seam every `gh` read passes through, rather than re-derived at each
        of the six call sites.

        A JSON OBJECT response therefore arrives as a one-element array. Every current
        caller reads a list, so nothing needs the scalar shape; a future object-shaped
        read must index the result rather than assume the object.

        A PAYLOAD CONTAINING A `$null` RECORD IS A FAILED READ, not a successful one.
        Collecting the pipeline is what makes that check necessary as well as possible:
        a JSON `null` — and equally `[null]`, or `[{...}, null]` — survives collection as
        a NON-NULL array holding `$null`, so it sails past every caller's
        `$null -eq $result` failure test. That is a fail-OPEN in a tool whose entire
        design is to fail closed on anything it cannot prove, and the consequences are
        not uniform:

          * `Get-CiScanPullRequestIndex` certifies `Complete = $true` over an EMPTY
            blocker index, skipping the `pull-request-index-incomplete` guard. A blocker
            index that is empty because it was never readable is the one shape that can
            let an issue close.
          * `Get-CiScanOpenIssues` and `Get-CiScanClosedReconcilerIssues` admit the
            `$null` itself into the surveyed backlog as an issue RECORD, so a decision
            loop that is supposed to read only well-formed issues is handed a record with
            no number, no body and no fingerprint.

        No GitHub list endpoint any of the six callers uses returns `null` in place of
        `[]`, so this is a hardening rather than a live bug — but "unreachable today" is
        the wrong basis on which to leave a fail-open at the one seam every read crosses.
        Rejecting is deliberately restricted to `$null` RECORDS: the deleted-account
        shape `Get-CiScanHumanCommenters` depends on is a well-formed comment object
        whose `user` is null, which is a real payload and must keep flowing.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string[]]$GhArgs, [switch]$Raw)

    # Defence in depth: this function shells out to `gh` with a caller-supplied argument
    # vector, so on its own it would be a second, unguarded mutation path. Constrain it to
    # the read shapes the reconciler actually uses. `gh api` defaults to GET, so the only
    # way to make it write is a method flag or a request parameter.
    #
    # The allow-list is expressed as whole command shapes rather than as a chain of
    # negated comparisons: `gh api <path>` is matched on its first token alone (the path
    # varies), while the list subcommands are matched on their first two tokens. A guard
    # this security-sensitive has to be readable at a glance, because the failure mode of
    # a subtly wrong condition is a silent write path.
    $allowed =
        $GhArgs[0] -ceq 'api' -or
        (($GhArgs | Select-Object -First 2) -join ' ') -cin @('pr list', 'label list')
    if (-not $allowed) {
        throw "BUG: Invoke-GhRead refuses non-read invocation 'gh $($GhArgs -join ' ')'."
    }
    foreach ($a in $GhArgs) {
        if (Test-CiScanRequestShapingArg -Argument $a) {
            throw "BUG: Invoke-GhRead refuses request-shaping flag '$a'."
        }
    }

    $out = & gh @GhArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        $script:Counters.ReadErrors++
        Write-Warning "gh read failed (exit $LASTEXITCODE): gh $($GhArgs -join ' ')"
        return $null
    }
    $text = ($out | Out-String)
    if ($Raw) { return $text }
    if ([string]::IsNullOrWhiteSpace($text)) { return $null }
    # See .DESCRIPTION: collecting the pipeline is what keeps an empty listing an empty
    # array instead of collapsing it into the $null that means "the read failed".
    try { $payload = @($text | ConvertFrom-Json -ErrorAction Stop) }
    catch { $script:Counters.ReadErrors++; return $null }
    foreach ($record in $payload) {
        # A $null record would otherwise ride out of here inside a NON-null array and pass
        # every caller's `$null -eq` failure test. See .DESCRIPTION: that is a fail-open,
        # so an unusable payload is reported as the failed read it effectively is.
        if ($null -eq $record) {
            $script:Counters.ReadErrors++
            Write-Warning "gh read returned a null record: gh $($GhArgs -join ' ')"
            return $null
        }
    }
    # The comma is load-bearing: bare `return $payload` unrolls a zero- or one-element
    # array and undoes the collection above.
    return ,$payload
}

function Invoke-HttpGetJson {
    <#
    .SYNOPSIS
        Anonymous read-only HTTPS GET against a fixed AzDO host, returning parsed JSON.
    .DESCRIPTION
        The host is asserted here rather than at the call site so that no caller can be
        tricked into pointing this at an arbitrary URL. Any failure returns $null, which
        the coverage layer converts into `Unverifiable` (fail closed).
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Url)

    if ($Url -notlike 'https://dev.azure.com/dnceng-public/public/_apis/*') {
        throw "Refusing to fetch non-allowlisted URL: $Url"
    }
    try {
        return Invoke-RestMethod -Uri $Url -Method Get -TimeoutSec 60 -ErrorAction Stop
    }
    catch {
        $script:Counters.ReadErrors++
        Write-Warning "AzDO read failed: $($_.Exception.Message)"
        return $null
    }
}

function Get-CiScanJsonField {
    <#
    .SYNOPSIS
        Reads one property off a parsed AzDO payload without trusting its shape.

    .DESCRIPTION
        This script runs under `Set-StrictMode -Version Latest`, where reading a property
        that does not exist is a TERMINATING error rather than `$null`. Every AzDO body
        reaches us through `Invoke-RestMethod`, which only guarantees the shape when the
        service returns the JSON we expect: a 200 carrying an error object, an HTML
        interstitial (parsed as a bare [string]), or an api-version change all produce an
        object with no `definition`/`sourceBranch`/`status` at all.

        Dotting into those directly throws out of the caller's per-issue loop and aborts
        the whole run — the precise opposite of the fail-closed contract the coverage
        layer documents. Returning `$null` lets each call site fall through to its own
        existing "unverifiable" branch instead. `Invoke-HttpGetJson` already fails closed
        for every NON-200; this is the same guarantee for a 200 whose body is not what we
        asked for.
    #>
    [CmdletBinding()]
    param([object]$Object, [Parameter(Mandatory)][string]$Name)

    if ($null -eq $Object) { return $null }
    # Index the property collection rather than testing `.Name -notcontains`: on an object
    # with NO properties the `.Name` member-enumeration itself throws under StrictMode,
    # which would reintroduce the very failure this helper exists to prevent. The indexer
    # returns $null for an absent name on every input shape, including a bare [string].
    $prop = $Object.PSObject.Properties[$Name]
    if ($null -eq $prop) { return $null }
    return $prop.Value
}

function Invoke-GhWrite {
    <#
    .SYNOPSIS
        THE single choke point for every mutating GitHub call.

    .DESCRIPTION
        Nothing else in this repository's reconciler path is permitted to invoke a
        mutating `gh` subcommand. Two consequences that the tests assert directly:

          * In report mode this function throws before touching the network, so a bug
            anywhere upstream surfaces as a loud failure rather than a silent write.
          * A test can prove "zero writes" by mocking exactly this one function and
            asserting it was invoked zero times.

        `Kind` is checked against the effective mode: closures and reopens require
        'enforce'; labels and comments require 'comment' or 'enforce'. `Kind` is then
        checked against `GhArgs` — it is a declaration, and until that check existed a
        close could run under a label declaration, in comment mode, against a different
        issue than the one validated.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][ValidateSet('label', 'unlabel', 'comment', 'close', 'reopen', 'body')]
        [string]$Kind,
        [Parameter(Mandatory)][int]$IssueNumber,
        [Parameter(Mandatory)][string[]]$GhArgs
    )

    if (-not $script:MutationsAllowed) {
        throw "BUG: mutating call '$Kind' attempted on issue #$IssueNumber while mode='$($script:EffectiveMode)'."
    }
    if (($Kind -eq 'close' -or $Kind -eq 'reopen') -and -not $script:ClosuresAllowed) {
        throw "BUG: '$Kind' attempted on issue #$IssueNumber while mode='$($script:EffectiveMode)'."
    }
    if ($IssueNumber -le 0) {
        throw "BUG: refusing to mutate non-positive issue number '$IssueNumber'."
    }

    <#
        `Kind` is a caller DECLARATION. `$GhArgs` is what actually runs. Nothing tied
        them together, so the gates above constrained a property the command need not
        honour. Measured in comment mode against a stubbed `gh`, before this block existed:

            -Kind close   -GhArgs @('issue','close','5')          -> BLOCKED  (honest call, gated)
            -Kind label   -GhArgs @('issue','close','5')          -> EXECUTED (the same close)
            -Kind comment -IssueNumber 5 -GhArgs @(...,'999',...) -> EXECUTED (mutated #999)

        The control matters: an honest close IS blocked, so this was never "the gate
        never worked" -- it was a gate on the wrong property. Consequences were
        cumulative. A mislabeled close skipped $ClosuresAllowed entirely, drew its budget
        from MaxLabelOps instead of MaxCloses (the budget table is keyed by the same
        declaration), and reported itself as a label op. The design requires issue-number
        provenance; #999 above is that requirement failing.

        Every tier test asserts on `Kind` and stayed green through all of it, which is
        why this is enforced here rather than pinned by another test.

        label/unlabel/body all spell `gh issue edit`, so the verb alone cannot separate
        them -- hence the flag, and the absence of its siblings. A `label` call that
        carried --remove-label would otherwise be able to strip a human veto label while
        declaring itself the additive kind.
    #>
    $shape = $script:CiScanWriteShapes[$Kind]

    # A kind admitted by the ValidateSet but absent from this table is refused here, by
    # name. Without this line it is still refused -- but by a StrictMode property crash on
    # `$shape.Verb` reporting "The property 'Verb' cannot be found on this object", which
    # names neither the kind nor the vocabulary. That is the third instance on this branch
    # of a crash standing in for a guard, and the most consequential: this table IS the
    # closed set of operations the reconciler may perform.
    #
    # The ordering matters. An untiered kind falls through to $MutationsAllowed -- the
    # COMMENT tier -- so a kind added to the ValidateSet is pre-authorized at the tier
    # meant to run for weeks in shadow. The only thing standing between that and a live
    # write is a shape-table entry, which is exactly what someone wiring a new kind adds
    # next. Failing by name here makes the missing tier decision visible at the moment it
    # is skipped, instead of surfacing as a property error someone "fixes" by adding the
    # entry.
    if ($null -eq $shape) {
        throw "BUG: '$Kind' is in the Kind vocabulary but has no shape entry, so its argument contract and tier were never decided."
    }

    if ($GhArgs.Count -lt 3 -or $GhArgs[0] -ne 'issue' -or $GhArgs[1] -ne $shape.Verb) {
        throw "BUG: '$Kind' must run 'gh issue $($shape.Verb)' on #$IssueNumber; got '$($GhArgs -join ' ')'."
    }
    if ($GhArgs[2] -ne "$IssueNumber") {
        throw "BUG: '$Kind' validated #$IssueNumber but the command targets '$($GhArgs[2])'."
    }
    if ($shape.Flag -and $shape.Flag -notin $GhArgs) {
        throw "BUG: '$Kind' must carry $($shape.Flag); got '$($GhArgs -join ' ')'."
    }

    <#
        An issue number does not identify an issue -- #5 exists in every repository. The
        checks above bind the verb and the number; the repository was the remaining half
        of the target identity, and it stayed unbound after they landed. Measured against
        a stubbed `gh` in comment mode, at the commit that added those checks:

            -Kind label -IssueNumber 5 -GhArgs @('issue','edit','5','--repo','attacker/evil',...)
                -> EXECUTED, against a repository the run never surveyed
            -Kind label -IssueNumber 5 -GhArgs @('issue','edit','5','--add-label',...)
                -> EXECUTED, and `gh` then falls back to whatever repo the CWD happens to be

        Both declare #5, both were accepted, and the "validated #5 but the command targets
        ..." message would have read as satisfied in each. Unlike the number, `--repo` is
        never derived from issue data -- every call site passes the run's own -Owner/-Repo
        -- so this pins current behaviour rather than fixing a reachable escape.

        Fail closed: if the run's own repository cannot be resolved, refuse the write
        instead of letting it through unchecked.
    #>
    <#
        Every check above reads ONE position for the target: `$GhArgs[2]`. `gh issue edit`
        reads ALL of them -- its grammar is `{<numbers> | <urls>}`, plural, while `close`,
        `reopen` and `comment` are singular. Measured read-only, arity errors only:

            gh issue edit    999999998 999999999  -> "field to edit flag required"  (accepted)
            gh issue close   999999998 999999999  -> "accepts 1 arg(s), received 2"
            gh issue comment 999999998 999999999  -> "accepts 1 arg(s), received 2"

        So cobra's arity check has been doing this validation for three of the four verbs,
        for free and invisibly -- and `edit` is the verb behind `label`, `unlabel` AND `body`.
        Positionals are also collected interspersed among flags and after a `--` terminator,
        so a second target can sit anywhere in the array:

            gh issue close 999999998 --repo dotnet/maui    999999999 -> received 2
            gh issue close 999999998 --repo dotnet/maui -- 999999999 -> received 2

        All six shapes executed against every other check in this function: verb correct,
        $GhArgs[2] correct, exactly one --repo, exactly one kind flag, no sibling flags.
        Nothing looked at the extra token.

        This runs BEFORE the repository binding below, and the order is load-bearing rather
        than cosmetic. Well-formedness is prior to binding: "index 3 is the repo flag" is
        only meaningful once the vector is known to be flag-shaped. Both orders refuse the
        same inputs, so only the DIAGNOSIS differs -- and for a bare token "gh would read it
        as a second target" is the accurate one, while "--repo is not the first flag" is
        true but describes a symptom.

        The worst shape is a URL, because a `gh` issue URL CARRIES ITS OWN REPOSITORY. It
        does not have to beat the --repo binding -- it goes around it, never touching --repo
        at all. That is why this is not an extension of the --repo work: it bypasses it.

        Same class as the repeated-flag defect one level down -- there, the validator read
        the first occurrence and the consumer read the last; here the validator reads one
        position and the consumer reads every one. A positional has no name, so there is no
        occurrence to count, and the shape must be constrained instead.

        Constrained rather than enumerated: the array must be `issue <verb> <number>`
        followed only by `--flag value` pairs or `--flag=value` singles. No bare tokens, no
        `--`. Enumerating the attacks would leave the next spelling open; this leaves only
        the shape every honest call site already emits. A future boolean flag makes this
        refuse loudly rather than mispair silently, which is the same fail-closed trade as
        the case-sensitive --repo comparison.
    #>
    $seenFlagNames = [System.Collections.Generic.List[string]]::new()
    for ($i = 3; $i -lt $GhArgs.Count; $i++) {
        $token = [string]$GhArgs[$i]
        if ($token -eq '--') {
            throw "BUG: '$Kind' must not carry a '--' terminator on issue #$IssueNumber; gh still collects positional targets after it. Got '$($GhArgs -join ' ')'."
        }
        if ($token -clike '--*') {
            <#
                The sibling-flag check this replaces was a hand-typed DENY list of three
                names -- --add-label, --remove-label, --body -- while `gh issue edit` ships
                eleven flags. Measured against the pre-fix head with -Kind label in comment
                mode, the enumerated --remove-label was correctly BLOCKED (so the check
                worked and was merely short) while all seven unenumerated ones EXECUTED:

                    --remove-assignee  --remove-milestone  --milestone
                    --title  --body-file  --add-assignee  --remove-project

                The first two are the veto strip, unmitigated. `Test-CiScanHumanTouched`
                vetoes on `assignee` and `milestone`; `gh issue edit` ships a --remove-* for
                each. So a `label` call at COMMENT tier could strip the exact signal that
                protects an issue, and a later ENFORCE run would close something that now
                looks untouched. --remove-label was gated; the two that matter more were not.
                --body-file is separately an arbitrary file read into an issue body, and it
                slipped the --body check purely because it is not spelled --body. `gh issue
                comment` has its own in --edit-last, which replaces a previous comment
                instead of adding one.

                Inverted rather than extended. A hand-typed deny list is the defect, not its
                length -- adding eight names leaves the next `gh` release short again. The
                allow list derives from the same $shape table the verb check uses, so a new
                flag is refused by default and the two lists cannot drift apart.

                Position-aware, and that is load-bearing rather than tidy. The deny list was
                a flat `-in $GhArgs` membership test, so it could not tell a FLAG from a
                VALUE that happens to spell one. Measured at the pre-fix head: `-Kind body`
                whose notice text is exactly '--remove-label' was REFUSED, and so was
                '--add-label'. Narrow -- `-in` compares whole elements, so a body merely
                CONTAINING the text was fine -- but inverting the polarity would have widened
                that latent false refusal from three names to eleven, and `--title` or
                `--milestone` are far likelier to appear alone in a notice than
                `--remove-label` is. Refusing the product rather than the attack. Checking
                inside this walk means a token is judged only where the walk has already
                established it is in flag position, so both spellings execute.

                Residual that used to live here, now closed: the `--repo` occurrence
                counter below was also a flat scan, so a notice body that was exactly
                '--repo' was refused as a duplicate. It now reads the flag names this
                walk collects, so both checks share one answer to "is this token a flag".

                Case-INsensitive on purpose, which is the one place this check deliberately
                gives ground. Comparing case-sensitively refused '--REPO' here and preempted
                the downstream provenance diagnosis, which is the more specific one: a case
                variant means the token came from somewhere other than the call sites, and
                that check says so by name. Nothing is gained by owning it -- a case variant
                of a DISALLOWED flag is still refused here, since it is absent from the list
                in every casing -- so the vocabulary check yields to the binding check rather
                than shadowing it. The sibling's own case-variant test is what pins that
                ordering; it fails if this comparison is tightened back to `-cnotin`.
            #>
            $name = ($token -split '=', 2)[0]
            if ($name -notin $shape.Allowed) {
                throw "BUG: '$Kind' on issue #$IssueNumber carries $name, which is not a flag this kind may use. Allowed: $($shape.Allowed -join ', '). Got '$($GhArgs -join ' ')'."
            }
            # Recorded HERE, at the one place in this function that has established the
            # token is in flag position. The duplication check below reads this list
            # instead of re-scanning the raw vector; see its docblock for why.
            $seenFlagNames.Add($name)
            if ($token -clike '--*=*') { continue }   # --flag=value consumes nothing
            $i++; continue                            # --flag consumes exactly its value
        }
        throw "BUG: '$Kind' on issue #$IssueNumber carries the bare argument '$token'; gh would read it as a SECOND target. Got '$($GhArgs -join ' ')'."
    }


    if (-not $script:TargetRepo) {
        throw "BUG: '$Kind' on issue #$IssueNumber cannot be checked against the run's own repository."
    }
    <#
        Searching the array for '--repo' asks "does this token appear", not "is this
        token a flag". Argument vectors are flat: a token in VALUE position is
        indistinguishable from the flag it spells. Measured against a stubbed `gh`,
        with the search form in place:

            -Kind comment -GhArgs @('issue','comment','5','--body','--repo','dotnet/maui')
                -> EXECUTED, and the command carries NO --repo at all

        The search found '--repo' at index 4 -- the BODY TEXT -- and read 'dotnet/maui'
        after it as the flag's value. Both reads are self-consistent; neither token is
        a flag. `gh` then falls back to the working directory, which is the exact
        redirect the repository binding exists to prevent, reached by satisfying it.

        Deciding which positions are flags requires knowing each flag's arity, i.e.
        re-implementing the consumer -- the same trap refused below. So the repository
        is pinned to a POSITION instead: it is part of the fixed prefix that indices
        0-2 already pin, and all five call sites emit exactly this shape. A token in
        value position cannot reach index 3 without breaking the verb/number checks
        that ran above.

        This does not subsume the duplication check below, and is not subsumed by it:
        a correct prefix followed by a LATER second --repo passes here and is honoured
        by `gh`. Both are load-bearing.

        Deliberately the two-token form only. `--repo=dotnet/maui` at index 3 is REFUSED
        here even though the walk above accepts `--flag=value` generally, and that is a
        narrowing of permitted SHAPE, not a second opinion about what the token is. The
        walk answers "is this a flag, and may this kind carry it"; this answers "does the
        prefix look exactly like what the call sites emit". All five emit the two-token
        form, so pinning it costs nothing and keeps the value at a known index instead of
        requiring this check to re-split the token -- a small re-parse, but re-parsing is
        how the readings drift apart. Stated because it was previously an accident of two
        checks written independently, and an unstated narrowing is one a later edit
        relaxes while "fixing an inconsistency". The test named for this pins it.
    #>
    $expectedRepo = $script:TargetRepo

    if ($GhArgs.Count -lt 5 -or $GhArgs[3] -cne '--repo') {
        throw "BUG: '$Kind' must carry --repo $expectedRepo as its first flag on issue #$IssueNumber; got '$($GhArgs -join ' ')'."
    }
    if ($GhArgs[4] -cne $expectedRepo) {
        throw "BUG: '$Kind' validated #$IssueNumber in $expectedRepo but the command targets '$($GhArgs[4])'."
    }

    <#
        The checks above resolve a flag by its FIRST occurrence. `gh` resolves by its
        LAST, and accepts `--flag=value` as well as `--flag value`. Measured read-only
        against live repositories -- newest issue was 36877 in dotnet/maui, 131512 in
        dotnet/runtime:

            --repo dotnet/maui  --repo  dotnet/runtime  -> 131512
            --repo dotnet/maui  --repo=dotnet/runtime   -> 131512
            --repo=dotnet/runtime  --repo dotnet/maui   ->  36877

        So a second --repo was validated by nothing and honoured by gh. Both forms
        executed against the repo-binding check: it read 'dotnet/maui' at index 4 and
        approved, while the write landed wherever the trailing value pointed.

        Ambiguity is refused rather than resolved. Teaching this to mimic gh's precedence
        would re-diverge the day gh changes it -- and the divergence would again be
        invisible, because the validator and the consumer would still be two independent
        readings of the same array. No honest call site emits a flag twice.

        This counts what the WALK ABOVE identified as flags, not tokens matching a flag's
        spelling anywhere in the vector. It used to do the latter, which made it the last
        flat reader in this function and gave it the same blind spot the allow list had
        before it moved into the walk: a flat scan cannot tell a FLAG from a VALUE that
        happens to spell one. Measured against the search form:

            -Kind comment --body '--repo'        -> BLOCKED as a duplicate --repo
            -Kind comment --body '--body'        -> BLOCKED as a duplicate --body
            -Kind label   --add-label '--add-label' -> BLOCKED as a duplicate

        while a body merely CONTAINING the text was fine, since these compare whole
        elements. Every one of those is a FALSE REFUSAL of an honest call: fail-closed,
        no write reaches the wrong place, and no notice this reconciler composes is
        exactly a flag name -- so this fixed no reachable bug and is not claimed to.

        What it removes is the second reading. The walk had already decided which
        positions are flags and threw that knowledge away; this re-derived it by a
        different rule, and two rules over one array is the shape of every argument
        defect on this branch -- first-vs-last occurrence, one-position-vs-all,
        deny-list-vs-value. Consolidating is the general fix rather than a third rule
        that happens to agree today.

        Compared case-insensitively BECAUSE the walk accepted them that way. If the walk
        admitted '--REPO' as the --repo flag, this must count it as one; the two cannot
        hold different opinions about what a token IS without reopening exactly the gap
        being closed. That also tightens a real edge the flat form missed: a LATER case
        variant was counted by neither, so it reached `gh` to fail there as an unknown
        flag. The index-3 provenance check still owns the case-variant diagnosis, because
        it runs before this and refuses on the position.
    #>
    foreach ($flag in @('--repo', $shape.Flag)) {
        if (-not $flag) { continue }
        $occurrences = @($seenFlagNames | Where-Object { $_ -eq $flag })
        if ($occurrences.Count -ne 1) {
            throw "BUG: '$Kind' must carry exactly one $flag on issue #$IssueNumber; got '$($GhArgs -join ' ')'."
        }
    }

    $script:Counters.Writes++
    $out = & gh @GhArgs 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Counted, not just warned. A close and its `auto-closed-stale` marker are two
        # calls: if the close lands and the label does not, the issue is closed WITHOUT
        # the marker that `Get-CiScanReopenVerdict` requires, so the reopen safety net no
        # longer recognizes it. Swallowing that left the run green over inconsistent
        # state. The caller turns any non-zero count into a non-zero exit.
        $script:Counters.WriteErrors++
        Write-Warning "gh write failed (exit $LASTEXITCODE) on #${IssueNumber}: $($out | Out-String)"
        return $false
    }
    return $true
}

#endregion

#region Data acquisition ------------------------------------------------------

function Get-CiScanOpenIssues {
    <#
    .SYNOPSIS
        Lists open tracking issues for a twin, oldest first.
    .DESCRIPTION
        This listing is the ONLY origin of issue numbers in the whole reconciler. It is
        constrained server-side by repo + state + exact label, and each result is
        re-validated client-side by `Test-CiScanIssueProvenance`.

        ORDERING IS LOAD-BEARING. `Max` bounds the batch, so whichever end of the
        eligible set the API returns first is the end that gets surveyed. GitHub's
        default issue order is newest-first, which would permanently strand the OLDEST
        issues once the open backlog exceeded `Max` — and the oldest issues are exactly
        the ones a staleness reconciler exists to evaluate (anything younger than
        `MinIssueAgeDays` cannot become a candidate anyway). `sort=created` +
        `direction=asc` inverts that so the bound drops the youngest, never the oldest.

        The same stranding bug class was fixed in the CI-fixer twins; see the
        `sort=created&direction=asc` prefetch in `ci-status-fix*.md`.

        `Truncated` means "this listing is NOT proven exhaustive", not "the bound
        provably elided something" — a full page that exactly consumed the remaining
        budget may or may not have more behind it, and the two cases are
        indistinguishable without another request. Reporting the ambiguous case as
        complete is the failure that matters, so it is reported as truncation. That
        signal is only trustworthy if EVERY early exit is accounted for, so the page
        ceiling is derived from `Max` instead of being a constant: a constant ceiling
        silently caps the survey once `Max` exceeds ceiling x 100 and reports
        `Truncated` = $false while doing it. Hitting the ceiling is therefore reported
        as truncation, never as exhaustion — and so is a failed page read, which leaves
        the remaining pages unknown rather than known-empty.

        Only a SHORT page is treated as proof of exhaustion — that is GitHub's documented
        pagination contract (fewer items than `per_page` means the last page).
    #>
    [CmdletBinding()]
    param([string]$Owner, [string]$Repo, [string]$Label, [int]$Max)

    $issues = @()
    $page = 1
    $truncated = $false
    $maxPages = [math]::Max(1, [int][math]::Ceiling($Max / 100.0))
    while ($issues.Count -lt $Max) {
        if ($page -gt $maxPages) { $truncated = $true; break }
        $perPage = [math]::Min(100, $Max - $issues.Count)
        $path = "repos/$Owner/$Repo/issues?state=open&labels=$([uri]::EscapeDataString($Label))" +
                "&sort=created&direction=asc&per_page=$perPage&page=$page"
        $batch = Invoke-GhRead -GhArgs @('api', $path)
        # A failed read is an unknown, not an exhausted list: the pages we never saw may
        # hold tracking issues. Reporting this exit as exhaustion would let a partial
        # survey claim completeness, which is the one thing `Truncated` exists to prevent.
        #
        # This test is only sound because `Invoke-GhRead` returns an empty listing as an
        # empty ARRAY: while it collapsed `[]` to `$null`, a healthy zero-open backlog
        # took this branch and reported itself as truncated, which failed the run closed
        # and disabled the reopen safety net at precisely the moment it became the only
        # thing left to run. The healthy-empty exit is the `Count -eq 0` break below.
        if ($null -eq $batch) { $truncated = $true; break }
        $batch = @($batch)
        if ($batch.Count -eq 0) { break }
        $issues += $batch
        # A full page that exactly consumed the remaining budget means the server may
        # still be holding more; anything short of a full page proves it is not.
        if ($batch.Count -lt $perPage) { break }
        if ($issues.Count -ge $Max) { $truncated = $true; break }
        $page++
    }
    return @{ Issues = @($issues); Truncated = $truncated }
}

function Get-CiScanClosedReconcilerIssues {
    <#
    .SYNOPSIS
        Lists issues this reconciler itself auto-closed, newest closure first.

    .DESCRIPTION
        The second, much smaller origin of issue numbers, and the one that makes
        `Get-CiScanReopenVerdict` reachable at all. Until this existed the reconciler only
        ever listed `state=open`, so the reopen verdict — implemented, tested, documented
        in the header as a supported capability — could never be called with a real issue.
        The safety net was a claim, not a mechanism.

        Constrained server-side to `state=closed` AND both labels: the twin label and
        `auto-closed-stale`. GitHub treats a comma-separated `labels=` as a conjunction,
        so the listing itself already excludes anything this automation did not close.
        That is a narrowing, not the guarantee — `Test-CiScanIssueProvenance` and
        `Get-CiScanReopenVerdict` both re-check the label client-side, because a
        server-side filter is a request parameter and this is the response.

        ORDERING IS INVERTED RELATIVE TO `Get-CiScanOpenIssues`, and the inversion is
        deliberate rather than an oversight. There, `Max` must drop the youngest, because
        staleness accrues with age and the oldest issues are the whole point. Here
        eligibility EXPIRES with age: a closure older than `ReopenWindowDays` cannot be
        reopened at all, so the newest closures are the only ones that can produce an
        action and newest-first is what keeps the bound from stranding them.

        `Truncated` carries the same meaning as in the open listing — "not proven
        exhaustive" — for the same reasons and by the same rules.
    #>
    [CmdletBinding()]
    param([string]$Owner, [string]$Repo, [string]$Label, [int]$Max)

    $issues = @()
    $page = 1
    $truncated = $false
    $maxPages = [math]::Max(1, [int][math]::Ceiling($Max / 100.0))
    $labels = [uri]::EscapeDataString("$Label,auto-closed-stale")
    while ($issues.Count -lt $Max) {
        if ($page -gt $maxPages) { $truncated = $true; break }
        $perPage = [math]::Min(100, $Max - $issues.Count)
        $path = "repos/$Owner/$Repo/issues?state=closed&labels=$labels" +
                "&sort=updated&direction=desc&per_page=$perPage&page=$page"
        $batch = Invoke-GhRead -GhArgs @('api', $path)
        if ($null -eq $batch) { $truncated = $true; break }
        $batch = @($batch)
        if ($batch.Count -eq 0) { break }
        $issues += $batch
        if ($batch.Count -lt $perPage) { break }
        if ($issues.Count -ge $Max) { $truncated = $true; break }
        $page++
    }
    return @{ Issues = @($issues); Truncated = $truncated }
}

function Get-CiScanPullRequestIndex {
    <#
    .SYNOPSIS
        Fetches the pull requests that could reference a tracking issue.
    .DESCRIPTION
        Two bounded queries instead of one query per issue:
          * every OPEN pull request (an open PR of any kind blocks closure), and
          * every `[ci-fix*]` pull request in any state (merged ones reset the clock).

        Returning an empty set on failure would silently REMOVE blockers, so a failed
        fetch is surfaced via `Complete` = $false and the caller aborts all mutations.

        The cap is probed, not assumed. `gh pr list --limit N` returning exactly N is
        indistinguishable from "there were exactly N" unless you ask for one more, and a
        blocker PR beyond a silently-truncated page is a blocker that cannot veto a
        close. Asking for `Max + 1` makes hitting the bound observable, and it is then
        reported as incomplete for the same reason a failed fetch is.
    #>
    [CmdletBinding()]
    param([string]$Owner, [string]$Repo, [int]$Max)

    $fields = 'number,title,body,state,mergedAt,isDraft,baseRefName'
    $probe = $Max + 1
    $open = Invoke-GhRead -GhArgs @('pr', 'list', '--repo', "$Owner/$Repo", '--state', 'open',
        '--limit', "$probe", '--json', $fields)
    $fixes = Invoke-GhRead -GhArgs @('pr', 'list', '--repo', "$Owner/$Repo", '--state', 'all',
        '--search', 'in:title ci-fix', '--limit', "$probe", '--json', $fields)

    # `$null` means the listing could not be read. An empty listing — a repo with no open
    # pull requests, or no `ci-fix` titles ever — arrives as an empty array and is a
    # COMPLETE index with nothing in it, so it must not be conflated with a failed read.
    $complete = ($null -ne $open -and $null -ne $fixes)
    if ($complete -and ((Get-CiScanCount $open) -gt $Max -or (Get-CiScanCount $fixes) -gt $Max)) {
        Write-Warning "Pull-request listing hit the -MaxPullRequests bound of $Max; the blocker index is NOT exhaustive."
        $complete = $false
    }
    $all = @()
    $seen = @{}
    foreach ($pr in (@($open) + @($fixes))) {
        if ($null -eq $pr) { continue }
        $n = 0
        if (-not [int]::TryParse([string](Get-CiScanJsonField -Object $pr -Name 'number'), [ref]$n) -or $n -le 0) {
            # Dropping an unreadable record silently would SHRINK the blocker index, and a
            # missing blocker is the one direction that can let an issue close. So this
            # marks the index inexhaustive rather than skipping quietly.
            Write-Warning 'A pull request record has no readable number; the blocker index is NOT exhaustive.'
            $complete = $false
            continue
        }
        if ($seen.ContainsKey($n)) { continue }
        $seen[$n] = $true
        $all += $pr
    }
    return @{ PullRequests = @($all); Complete = $complete }
}

function Test-CiScanOwnedLabels {
    <#
    .SYNOPSIS
        Returns the reconciler-owned labels that do NOT exist on the repository.
    .DESCRIPTION
        Preflight for mutating modes. Applying a label that does not exist is a hard
        `gh` failure, and the one that matters is `auto-closed-stale`: it is written
        AFTER the close, so a missing label produces a closed issue with no marker —
        and `Get-CiScanReopenVerdict` refuses to reopen anything it cannot recognize as
        its own. The result is an irreversible close, which is precisely the outcome the
        design forbids.

        A lookup that FAILS is reported as missing too: an unprovable label is not a
        present one, and this gate exists to fail closed. One list call rather than one
        probe per label, so a missing label is an absence in a successful response
        instead of an expected-404 that would pollute the read-error counter.
    #>
    [CmdletBinding()]
    param([string]$Owner, [string]$Repo, [int]$Max = 500)

    $labels = Invoke-GhRead -GhArgs @('label', 'list', '--repo', "$Owner/$Repo",
        '--limit', "$Max", '--json', 'name')
    if ($null -eq $labels) { return @($script:CiScanOwnedLabels) }

    $present = @{}
    foreach ($l in @($labels)) {
        # `$null -eq $l.name` reads as a guard for a label record without a name and
        # THROWS on exactly that record under StrictMode, because the property read runs
        # before the comparison. This preflight gates every mutating run, so an aborted
        # one is not a safe failure — it is an unhandled exception in the code whose job
        # is to decide whether mutation is allowed at all.
        $name = [string](Get-CiScanJsonField -Object $l -Name 'name')
        if ([string]::IsNullOrEmpty($name)) { continue }
        $present[$name] = $true
    }

    $missing = @()
    foreach ($name in $script:CiScanOwnedLabels) {
        if (-not $present.ContainsKey($name)) { $missing += $name }
    }
    return @($missing)
}

function Get-CiScanHumanCommenters {
    <#
    .SYNOPSIS
        Returns non-bot commenter logins for an issue, or Ok = $false when the comment
        history could not be read in full.
    .DESCRIPTION
        A human comment is a veto signal, so this function's job is to prove the ABSENCE
        of human comments. A single unpaginated page could only ever prove that about the
        first 100 comments — on a busier issue a later human comment would be invisible
        and the reconciler would act MORE aggressively than intended. So the history is
        paginated to exhaustion.

        Any outcome that leaves the history incomplete — a failed page, or more comments
        than the page ceiling allows — returns `Ok = $false` with an empty login set AND
        counts a read error, so the run-level fail-closed check suppresses mutations for
        the ENTIRE run rather than acting on a partial history.

        Both incomplete outcomes must count a read error, not just the failed page. A
        failed page gets one for free (`Invoke-GhRead` records it), but exhausting the
        page ceiling is the same epistemic state — the absence of a human comment is
        unproven — and it would otherwise be downgraded per-issue while leaving every
        OTHER issue in the run free to mutate.

        A third state sits between "history complete" and "history unreadable": a
        comment whose `user` is null, which is what GitHub returns once the author's
        account has been DELETED. The history is complete, so `Ok` stays `$true`, but
        that one comment cannot be attributed — and since bot accounts are not deleted,
        the overwhelmingly likely author is a human. It is therefore reported as a
        commenter (under a fixed sentinel login), which vetoes this one issue without
        suppressing the whole run.
    #>
    [CmdletBinding()]
    param([string]$Owner, [string]$Repo, [int]$Number)

    $incomplete = @{ Logins = @(); Ok = $false }
    $perPage = 100
    $maxPages = 20   # 2,000 comments; no ci-scan tracking issue is remotely near this.

    $comments = @()
    $complete = $false
    for ($page = 1; $page -le $maxPages; $page++) {
        $batch = Invoke-GhRead -GhArgs @('api',
            "repos/$Owner/$Repo/issues/$Number/comments?per_page=$perPage&page=$page")
        # `$null` is a failed page. An issue with no comments at all returns an empty
        # array, which is the commonest shape a tracking issue has and is a COMPLETE
        # history proving zero human commenters — not an unreadable one.
        if ($null -eq $batch) { return $incomplete }
        $batch = @($batch)
        $comments += $batch
        # A short (or empty) page is the only proof that no further comments exist.
        if ($batch.Count -lt $perPage) { $complete = $true; break }
    }
    if (-not $complete) {
        # Ceiling exhausted. Invoke-GhRead never failed, so nothing has counted this yet.
        $script:Counters.ReadErrors++
        Write-Warning ("Comment history for #$Number exceeded the $maxPages-page ceiling; " +
            'treating the run as unable to prove the absence of human comments.')
        return $incomplete
    }

    $logins = @()
    foreach ($c in $comments) {
        # `$null -eq $c.user` was the same dead guard as the timeline records: under
        # StrictMode the read throws before the comparison, so a comment payload without
        # `user` aborted the run rather than being skipped.
        #
        # But `continue` is the wrong landing spot for it, and it is wrong in the
        # dangerous direction. GitHub nulls `user` when the ACCOUNT WAS DELETED, and
        # deleted accounts are humans — bots are not deleted. So the single payload shape
        # that most strongly means "a human was here" was the one shape discarded, while
        # the loop fell through to `Ok = $true` and certified the history as proving the
        # absence of human comments. This function's whole contract (see .DESCRIPTION) is
        # to prove that absence; a comment it cannot attribute is precisely the evidence
        # it cannot prove it from. Unattributable therefore counts AS human.
        #
        # Recorded as a fixed sentinel, never as payload text: the login is echoed into a
        # `human-comment:` verdict signal, so an attacker-chosen string must not reach it.
        # The sentinel must be something no GitHub login can equal. Logins are letters,
        # digits and hyphens only, so the parentheses are what make it collision-proof —
        # not the '<' and '>' this comment used to name, which the sentinel has never
        # contained. The inversion mattered: the step-summary test requires the sentinel
        # to carry NO angle brackets, so anyone reconciling the sentinel to its stated
        # justification would have broken rendering. The property is pinned behaviourally
        # in 'a sentinel that is neither a login nor markup'.
        #
        # This is a PER-ISSUE veto, not a run-level read error. The history here is
        # complete — every page was fetched — so the run-wide `Ok = $false` suppression
        # reserved for an unreadable history would be disproportionate; the correct scope
        # is "this one issue has a comment that might be human, so leave it alone".
        $user = Get-CiScanJsonField -Object $c -Name 'user'
        $login = if ($null -eq $user) { '' } else { [string](Get-CiScanJsonField -Object $user -Name 'login') }
        $type = if ($null -eq $user) { '' } else { [string](Get-CiScanJsonField -Object $user -Name 'type') }
        if ([string]::IsNullOrWhiteSpace($login)) {
            $logins += $script:CiScanUnattributableCommenter
            continue
        }
        if ($type -eq 'Bot') { continue }
        # `-like '*[bot]'` would be a WILDCARD CHARACTER CLASS: it matches any login
        # ending in b, o or t (dropping humans such as `rmarinho`) while NOT matching
        # the literal `[bot]` suffix it was meant to catch. Anchor a regex instead.
        if ($login -match '\[bot\]$') { continue }
        if ($login -in @('github-actions', 'maui-bot', 'MauiBot', 'dotnet-bot', 'dotnet-policy-service')) { continue }
        $logins += $login
    }
    return @{ Logins = @($logins | Select-Object -Unique); Ok = $true }
}

#endregion

#region AzDO coverage ---------------------------------------------------------

function Get-CiScanLegOutcome {
    <#
    .SYNOPSIS
        Classifies one build's timeline against the issue's affected legs.

    .DESCRIPTION
        Returns exactly one of three values, and the distinction between the last two is
        the whole point of the function:

          'clean'  — every affected leg matched a record, ran, and EVERY matching ran
                     record completed cleanly. Only this counts as a verified absence.
          'failed' — some affected leg ran and at least one of its matching records did
                     not complete cleanly. This is positive evidence that the leg where
                     the signature surfaces went red.
          'no-run' — no usable observation: a leg matched nothing, matched only
                     non-running records, or carried an unreadable key. Says nothing in
                     either direction.

        The caller for CLAIMED builds treats 'failed' and 'no-run' identically (neither
        is an absence), which is why this used to be a single `$allLegsRan` boolean. The
        caller for builds NEWER than the marker's horizon cannot: for those, 'failed' is
        evidence the signature came back, while 'no-run' is merely silence. Collapsing
        them there would either veto on silence or ignore a recurrence, so the third
        state has to exist before the newer-build probe can be written at all.

        'failed' is deliberately checked per-leg and returned immediately: one red
        affected leg is enough, and continuing could downgrade it to 'no-run' on a later
        leg that simply did not run.

        Every field is read through `Get-CiScanJsonField`. Under
        `Set-StrictMode -Version Latest` a missing property is a TERMINATING error, so a
        well-formed `records` array carrying one entry without `name`/`result` would
        abort the caller's per-issue loop rather than quarantine this one build. Skipping
        a malformed record is conservative in all three positions: each filter can only
        SHRINK, which can only move the answer away from 'clean'. A junk record can never
        help close an issue.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][AllowEmptyCollection()][array]$Records,
        [Parameter(Mandatory)][AllowEmptyCollection()][string[]]$Legs,
        [Parameter(Mandatory)][hashtable]$Defaults
    )

    # No legs is not a clean build: with nothing to check, "every leg was clean" is
    # vacuously true and would credit an absence on zero evidence.
    if ((Get-CiScanCount $Legs) -eq 0) { return 'no-run' }

    foreach ($leg in $Legs) {
        $key = ($leg -split '—')[0].Trim()
        if ([string]::IsNullOrWhiteSpace($key)) { return 'no-run' }

        $match = @($Records | Where-Object {
            $recordName = Get-CiScanJsonField -Object $_ -Name 'name'
            $null -ne $recordName -and ([string]$recordName).IndexOf($key, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
        })
        if ((Get-CiScanCount $match) -eq 0) { return 'no-run' }

        $ran = @($match | Where-Object {
            $recordResult = Get-CiScanJsonField -Object $_ -Name 'result'
            $r = if ($null -eq $recordResult) { '' } else { [string]$recordResult }
            $r -ne '' -and $Defaults.NonRunningLegResults -notcontains $r
        })
        if ((Get-CiScanCount $ran) -eq 0) { return 'no-run' }

        # Execution is not absence. The affected leg is precisely where this signature
        # surfaces, so a leg that RAN AND FAILED is the one outcome that cannot
        # distinguish "the signature is gone" from "the signature fired again and the
        # scanner did not record it". A build-level `failed` is still accepted (some
        # unrelated leg can fail while this one is clean) — but if THIS leg failed, the
        # build proves nothing about absence.
        #
        # EVERY ran record must be clean, not merely one of them. `$key` is matched by
        # SUBSTRING (`IndexOf`, above), so a single leg key routinely selects more than
        # one timeline record: matrix legs sharing a name prefix, and — the case that
        # matters — a failed attempt plus its retry. Asking only whether a clean record
        # EXISTS turns `Leg = failed` + `Leg retry = succeeded` into a verified absence,
        # which is the exact inversion the paragraph above forbids: the signature fired,
        # and the build gets counted as proof that it did not. Comparing the two counts
        # asks "did anything fail?" instead of "did anything pass?", and it degrades the
        # right way — an ambiguous leg is reported red rather than credited clean.
        $clean = @($ran | Where-Object {
            $Defaults.CleanLegResults -contains ([string](Get-CiScanJsonField -Object $_ -Name 'result'))
        })
        if ((Get-CiScanCount $clean) -ne (Get-CiScanCount $ran)) { return 'failed' }
    }

    return 'clean'
}

function Get-CiScanBuildsAfter {
    <#
    .SYNOPSIS
        Lists completed builds on the twin's definition + branch newer than a horizon.

    .DESCRIPTION
        The reconciler's view of a signature's history is the state marker, and the
        marker only knows the builds the SCANNER recorded. `Get-CiScanBuildCoverage`
        enumerated exactly those build IDs, so the reconciler's window closed wherever
        the marker's did. A marker last written in June, carrying nine clean builds, is
        still nine clean builds in August — and the July build in which the signature
        recurred was never fetched, because nothing in the loop was ever going to ask for
        a build ID the marker did not already name. Absence of evidence was being read as
        evidence of absence, with the marker choosing the evidence.

        This asks AzDO directly instead: what has run on this branch since the newest
        build the marker accounts for? It is the only call in the reconciler whose result
        is not derived from the issue body.

        THE HORIZON IS A UNION ON THE CLIENT, AND IT HAS TO BE. `AfterBuildId` alone is
        not a "newer than" test: AzDO build IDs are assigned at QUEUE time, so a build
        queued before the marker's newest build but finishing after it carries a LOWER id
        and was silently dropped — which is precisely the recurrence the caller is looking
        for. A build therefore qualifies when its id is above the id horizon OR its
        `finishTime` is above the time horizon.

        The union is ASYMMETRIC once the wire is accounted for, and the asymmetry is
        deliberate. `minTime` narrows server-side on `finishTime` (see the request
        comment below), so the time horizon can only ever WIDEN the id horizon, never the
        reverse: a build whose id is above the id horizon but whose `finishTime` is below
        the time horizon is dropped upstream and never reaches the client's id branch.
        That build finished before the marker was last written, so the scanner's own next
        pass re-reads it and the following reconcile sees it above a moved watermark; the
        recurrence probe is additionally gated by `MinQuietDays`, so nothing closes in the
        interval. Widening `minTime` to cover it would refetch weeks of builds on every
        run to re-derive an answer that arrives on its own.

        `MinFinishTime` is applied on both sides of the wire for the usual reason: it is
        sent as `minTime` so the server can narrow the page, and re-checked here because
        the request is a parameter and this is the response. A build at or below the id
        horizon whose `finishTime` cannot be read is the one case that cannot be
        classified either way, so the whole listing fails closed rather than dropping it
        — dropping is how the queue-order hole behaved, and it is the unsafe direction.

        `Truncated` carries the same meaning as in `Get-CiScanOpenIssues`: "this listing
        is NOT proven exhaustive". `$top` is requested one HIGHER than the cap precisely
        so the two cases stay distinguishable — a page that comes back at or under the cap
        proves we saw every newer build, while one that overflows proves only that there
        are more than we are willing to fetch. The overflow case is exactly the grossly
        stale marker (a scanner stalled for weeks), so reporting it as truncation and
        letting the caller fail closed is the whole point rather than a limitation.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][int]$DefinitionId,
        [Parameter(Mandatory)][string]$Branch,
        [int]$AfterBuildId = 0,
        [datetime]$MinFinishTime = [datetime]::MinValue,
        [Parameter(Mandatory)][int]$Max
    )

    $top = $Max + 1
    $url = "https://dev.azure.com/dnceng-public/public/_apis/build/builds" +
           "?definitions=$DefinitionId" +
           "&branchName=$([uri]::EscapeDataString("refs/heads/$Branch"))" +
           "&statusFilter=completed&queryOrder=finishTimeDescending&`$top=$top&api-version=7.1"

    # `minTime` filters the field named by `queryOrder`, which is `finishTime` above. It
    # is a server-side narrowing only: the caller still fails closed on truncation, so a
    # server that ignored the parameter would produce a more conservative answer, never a
    # more permissive one. Sent in round-trip UTC form because AzDO reads an offsetless
    # timestamp in the ORGANIZATION's timezone, not ours.
    if ($MinFinishTime -gt [datetime]::MinValue) {
        $url += "&minTime=$([uri]::EscapeDataString($MinFinishTime.ToUniversalTime().ToString('o')))"
    }

    $page = Invoke-HttpGetJson -Url $url
    # `value` is tested for PRESENCE, not for null, and the difference is load-bearing.
    # An empty listing is the healthy steady state — the scanner is current and nothing
    # has run since — but `Get-CiScanJsonField` returns that empty array through a
    # function boundary, where PowerShell unrolls it to `$null`. A null test therefore
    # reads "no newer builds" as "the listing failed" and fails every healthy issue
    # closed to `needs-human`, which is a silent denial-of-service on the whole
    # reconciler rather than a safety property. Presence separates the two: a response
    # with no `value` at all really is the wrong shape.
    if ($null -eq $page -or -not (Test-CiScanHasField -Object $page -Name 'value')) {
        return @{ Builds = @(); Ok = $false; Truncated = $false }
    }
    $value = Get-CiScanJsonField -Object $page -Name 'value'

    $newer = @()
    foreach ($b in @($value)) {
        if ($null -eq $b) { continue }
        $id = 0
        # An unreadable id cannot be compared against the horizon, and silently dropping
        # it would let the one build we could not parse be the one that recurred.
        if (-not [int]::TryParse([string](Get-CiScanJsonField -Object $b -Name 'id'), [ref]$id)) {
            return @{ Builds = @(); Ok = $false; Truncated = $false }
        }
        if ($id -gt $AfterBuildId) { $newer += $id; continue }

        # Below the id horizon, so the only remaining question is when it FINISHED. A
        # caller with no time horizon (the recurrence probe passes `AfterBuildId = 0`
        # and never reaches here) keeps the id-only behaviour exactly.
        if ($MinFinishTime -le [datetime]::MinValue) { continue }
        $finished = ConvertFrom-CiScanTimestamp (Get-CiScanJsonField -Object $b -Name 'finishTime')
        if ($null -eq $finished) {
            return @{ Builds = @(); Ok = $false; Truncated = $false }
        }
        if ($finished -gt $MinFinishTime.ToUniversalTime()) { $newer += $id }
    }

    $newer = @($newer | Sort-Object -Unique -Descending)
    if ((Get-CiScanCount $newer) -gt $Max) {
        return @{ Builds = @($newer | Select-Object -First $Max); Ok = $true; Truncated = $true }
    }
    return @{ Builds = @($newer); Ok = $true; Truncated = $false }
}

function Get-CiScanBuildCoverage {
    <#
    .SYNOPSIS
        Independently re-derives which recorded absence build IDs are actually usable.

    .DESCRIPTION
        The recorded state marker is treated as a CLAIM, never as proof. For every build
        ID claimed as an absence this re-fetches the build from AzDO and requires:

          * the build exists and belongs to the twin's configured definition,
          * `sourceBranch` is exactly `refs/heads/<twin branch>`,
          * `status` is `completed` and `result` is one of the accepted results, and
          * the timeline contains a record for EVERY leg named in the issue's
            `## Affected Legs`, with a result that means the leg genuinely ran AND
            completed cleanly.

        The last check is what separates a real clean build from a build where the
        relevant leg was skipped, gated off, never scheduled, or ran and failed — the
        failure modes a wall-clock rule cannot see. A leg that ran and FAILED is
        specifically excluded: that is the one outcome where the signature may well
        have fired, so counting it would let a still-broken pipeline accumulate
        "verified absences".

        Any error, any missing build, any unresolvable leg sets `Unverifiable = $true`,
        which the core converts to zero counted absences. That includes a 200 whose body
        is not a build at all: every field is read through `Get-CiScanJsonField`, because
        under `Set-StrictMode -Version Latest` dotting a missing property would abort the
        caller's per-issue loop rather than quarantine this one build. The same applies
        one level down, to the individual timeline records: a well-formed `records` array
        may still contain an entry with no `name`/`result`, so those are read through the
        accessor too and an unreadable record is skipped rather than counted.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][hashtable]$Config,
        [Parameter(Mandatory)][string]$Pipeline,
        [Parameter(Mandatory)][string[]]$Legs,
        [int[]]$ClaimedBuildIds = @(),
        [int[]]$KnownBuildIds = @(),
        [datetime]$MarkerUpdatedAt = [datetime]::MinValue
    )

    $result = @{ VerifiedAbsentBuilds = @(); Unverifiable = $false; Reason = '' }

    if ($script:SkipAzdo) {
        $result.Unverifiable = $true
        $result.Reason = 'azdo-skipped'
        return $result
    }
    if ((Get-CiScanCount $ClaimedBuildIds) -eq 0) { return $result }
    if ((Get-CiScanCount $Legs) -eq 0) {
        $result.Unverifiable = $true
        $result.Reason = 'no-legs'
        return $result
    }

    $definition = @($Config.Pipelines | Where-Object { $_.Name -ceq $Pipeline })
    if ((Get-CiScanCount $definition) -ne 1) {
        $result.Unverifiable = $true
        $result.Reason = "unknown-pipeline:$Pipeline"
        return $result
    }
    $definitionId = [int]$definition[0].DefinitionId
    $expectedBranch = "refs/heads/$($Config.Branch)"
    $d = Get-CiScanDefaults

    $verified = @()
    foreach ($buildId in @($ClaimedBuildIds | Sort-Object -Unique)) {
        if ($buildId -le 0) { $result.Unverifiable = $true; $result.Reason = 'non-positive-build-id'; return $result }

        $build = Invoke-HttpGetJson -Url "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId`?api-version=7.1"
        if ($null -eq $build) { $result.Unverifiable = $true; $result.Reason = "build-fetch-failed:$buildId"; return $result }

        # Every field below is read through Get-CiScanJsonField, not dotted directly: under
        # `Set-StrictMode -Version Latest` a MISSING property is a terminating error, so a
        # 200 whose body is not a build (an error object, an HTML interstitial parsed as a
        # bare string, an api-version shape change) aborted the entire run here instead of
        # marking this one build unverifiable. Invoke-HttpGetJson already fails closed for
        # every non-200; this extends the same guarantee to a 200 with the wrong shape.
        #
        # `id` additionally needs TryParse rather than [int]: a present-but-non-numeric or
        # overflowing value is a terminating cast error even when the property exists. An
        # unreadable id is reported as its own reason rather than as a mismatch, which
        # would blame the wrong pipeline for what is really a malformed payload.
        $buildDefinitionId = 0
        $rawDefinitionId = Get-CiScanJsonField -Object (Get-CiScanJsonField -Object $build -Name 'definition') -Name 'id'
        if (-not [int]::TryParse([string]$rawDefinitionId, [ref]$buildDefinitionId)) {
            $result.Unverifiable = $true; $result.Reason = "definition-unparseable:$buildId"; return $result
        }
        if ($buildDefinitionId -ne $definitionId) { $result.Unverifiable = $true; $result.Reason = "definition-mismatch:$buildId"; return $result }
        if ([string](Get-CiScanJsonField -Object $build -Name 'sourceBranch') -cne $expectedBranch) { $result.Unverifiable = $true; $result.Reason = "branch-mismatch:$buildId"; return $result }
        if ([string](Get-CiScanJsonField -Object $build -Name 'status') -ne 'completed') { continue }
        if ($d.AcceptedBuildResults -notcontains [string](Get-CiScanJsonField -Object $build -Name 'result')) { continue }

        $timeline = Invoke-HttpGetJson -Url "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId/timeline`?api-version=7.1"
        # Same helper, same reason: the previous `$timeline.PSObject.Properties.Name -contains`
        # form is itself unsafe under StrictMode, because `.Name` member-enumeration throws
        # on a payload with NO properties (a 200 carrying `{}`). A present-but-null `records`
        # now also fails closed rather than degrading into a one-element array of $null.
        $timelineRecords = Get-CiScanJsonField -Object $timeline -Name 'records'
        if ($null -eq $timeline -or $null -eq $timelineRecords) {
            $result.Unverifiable = $true; $result.Reason = "timeline-fetch-failed:$buildId"; return $result
        }

        if ((Get-CiScanLegOutcome -Records @($timelineRecords) -Legs $Legs -Defaults $d) -ne 'clean') { continue }

        $verified += [int]$buildId
    }

    $result.VerifiedAbsentBuilds = @($verified | Sort-Object -Unique)

    <#
        Everything above re-derives the marker's OWN claims. That is necessary and not
        sufficient: it can only ever disprove a build the marker already named, so the
        reconciler's field of view ends exactly where the scanner's last write did. The
        reported failure mode is a marker last updated in June with nine clean builds
        still reading as nine clean builds in August — the July build in which the
        signature recurred is not rejected, it is never requested, because no build ID
        outside `absent_builds` is reachable from this loop.

        So ask AzDO what has happened since, and subject it to the same leg test. The
        horizon is the newest build the marker accounts for in EITHER direction: absences
        alone would understate it, since a recorded PRESENCE is also proof the scanner
        looked at that build.

        Only 'failed' vetoes. The three outcomes are not symmetric here:
          * 'failed'  — the affected leg ran and went red after our evidence window. That
                        is the recurrence, and certifying absence across it is the bug.
          * 'clean'   — a newer clean build agrees with the absence claim. It is NOT added
                        to `VerifiedAbsentBuilds`: the count threshold is calibrated
                        against scanner-recorded observations, and quietly inflating it
                        here would lower the bar to close using evidence the scanner never
                        vetted. Agreeing evidence should not accelerate a closure.
          * 'no-run'  — the leg was skipped or absent from the timeline. Silence, not
                        agreement, and vetoing on it would strand every issue whose leg is
                        conditionally scheduled.

        Fail closed on anything we could not enumerate: a failed listing leaves the newer
        builds unknown, and an overflowing one means the marker is further behind than we
        are willing to probe — which is the grossly-stale scanner the reported failure
        describes. Both are `Unverifiable`, which the core converts to zero counted
        absences.

        `MarkerUpdatedAt` supplies the TIME half of that horizon and is required, not
        optional. Build IDs are assigned at queue time, so an id-only horizon drops a
        build that was queued earlier but finished later — exactly the recurrence this
        probe exists to catch. A marker with no readable write timestamp cannot supply
        it, and probing with half a horizon would restore the hole, so that case is
        `Unverifiable` like every other unknown here.
    #>
    if ($MarkerUpdatedAt -le [datetime]::MinValue) {
        $result.Unverifiable = $true; $result.Reason = 'no-marker-timestamp'; return $result
    }

    $horizon = 0
    foreach ($k in (@($ClaimedBuildIds) + @($KnownBuildIds))) {
        if ($null -eq $k) { continue }
        $kid = 0
        if (-not [int]::TryParse([string]$k, [ref]$kid)) { continue }
        if ($kid -gt $horizon) { $horizon = $kid }
    }
    if ($horizon -le 0) {
        $result.Unverifiable = $true; $result.Reason = 'no-build-horizon'; return $result
    }

    $newer = Get-CiScanBuildsAfter -DefinitionId $definitionId -Branch $Config.Branch `
        -AfterBuildId $horizon -MinFinishTime $MarkerUpdatedAt -Max $script:CiScanMaxNewerBuildsProbed
    if (-not $newer.Ok) {
        $result.Unverifiable = $true; $result.Reason = "newer-build-listing-failed:$horizon"; return $result
    }
    if ($newer.Truncated) {
        $result.Unverifiable = $true
        $result.Reason = "marker-horizon-stale:more-than-$($script:CiScanMaxNewerBuildsProbed)-builds-since-$horizon"
        return $result
    }

    foreach ($buildId in @($newer.Builds)) {
        $build = Invoke-HttpGetJson -Url "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId`?api-version=7.1"
        if ($null -eq $build) { $result.Unverifiable = $true; $result.Reason = "newer-build-fetch-failed:$buildId"; return $result }

        # The listing was already filtered server-side by definition and branch, but it is
        # re-checked here for the same reason the claimed loop does it: the filter is a
        # request parameter, and this is the response. Only the branch can realistically
        # drift (a renamed twin branch), and reading it wrong in the permissive direction
        # would mean probing another branch's builds for a recurrence on this one.
        if ([string](Get-CiScanJsonField -Object $build -Name 'sourceBranch') -cne $expectedBranch) { continue }
        if ($d.AcceptedBuildResults -notcontains [string](Get-CiScanJsonField -Object $build -Name 'result')) { continue }

        $timeline = Invoke-HttpGetJson -Url "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId/timeline`?api-version=7.1"
        $timelineRecords = Get-CiScanJsonField -Object $timeline -Name 'records'
        if ($null -eq $timeline -or $null -eq $timelineRecords) {
            $result.Unverifiable = $true; $result.Reason = "newer-timeline-fetch-failed:$buildId"; return $result
        }

        if ((Get-CiScanLegOutcome -Records @($timelineRecords) -Legs $Legs -Defaults $d) -eq 'failed') {
            $result.Unverifiable = $true
            $result.Reason = "recurrence-after-horizon:$buildId"
            return $result
        }
    }

    return $result
}

function Test-CiScanRecurrenceSince {
    <#
    .SYNOPSIS
        Asks AzDO whether an affected leg went red on the twin since a point in time.

    .DESCRIPTION
        This is the evidence primitive for the reopen safety net, and it is the reason
        `Get-CiScanReopenVerdict` was previously unreachable: the verdict function has
        always demanded `-RecurrenceObserved` from "trusted validated scanner output",
        and nothing in the reconciler could produce that. The newer-build probe added for
        the stale-marker fix already knows how to ask AzDO what has run on a branch and
        how to classify a leg in a timeline; this reuses both against a TIME horizon
        instead of a build-ID horizon.

        The horizon is `closed_at`, deliberately not the state marker's newest build.
        Reopen means "we closed this, and then it came back". Anchoring on the closure:

          * needs no state marker, so a closed issue whose marker is missing or corrupt
            is still protected by the safety net rather than silently excluded from it;
          * cannot be moved by anything written into the issue body after the fact, since
            `closed_at` is GitHub-controlled timeline metadata;
          * is strictly narrower than a marker-anchored horizon, so the only errors it
            can make are failures to reopen, never spurious reopens.

        FAILING CLOSED HERE MEANS `Observed = $false`, which is the inverse of the
        coverage path and is deliberate. There, an unknown must not be allowed to justify
        closing an issue, so unknown blocks the action. Here the action IS the safety net,
        and the unknown must not be allowed to manufacture a mutation on an issue a human
        may have closed correctly. In both cases the unknown blocks the write. `Ok` is
        reported separately so the summary can say "we could not check" rather than
        implying a clean bill of health.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][hashtable]$Config,
        [Parameter(Mandatory)][string]$Pipeline,
        [Parameter(Mandatory)][string[]]$Legs,
        [Parameter(Mandatory)][datetime]$Since
    )

    $result = @{ Observed = $false; BuildId = 0; Ok = $true; Reason = 'no-recurrence-since-closure' }

    # Same escape hatch the coverage path honours. Without it, `-SkipAzdo` would disable
    # the closure evidence but leave the reopen evidence still calling AzDO — a switch
    # that means "do not touch AzDO" would then half-touch it.
    if ($script:SkipAzdo) { $result.Ok = $false; $result.Reason = 'azdo-skipped'; return $result }

    $d = Get-CiScanDefaults
    # `Pipelines` is a LIST of { Name; DefinitionId }, not a keyed map. Read it exactly the
    # way `Get-CiScanBuildCoverage` does — a second, differently-shaped accessor for the
    # same data is how the two paths drift apart.
    $definition = @($Config.Pipelines | Where-Object { $_.Name -ceq $Pipeline })
    if ((Get-CiScanCount $definition) -ne 1) {
        $result.Ok = $false; $result.Reason = "unknown-pipeline:$Pipeline"; return $result
    }
    $definitionId = [int]$definition[0].DefinitionId
    if ($definitionId -le 0) {
        $result.Ok = $false; $result.Reason = "unknown-pipeline:$Pipeline"; return $result
    }
    if ((Get-CiScanCount $Legs) -eq 0) {
        # No affected leg means there is no timeline record to look for, so no observation
        # is possible. Reported as not-checked rather than as a clean result.
        $result.Ok = $false; $result.Reason = 'no-affected-legs'; return $result
    }

    $listing = Get-CiScanBuildsAfter -DefinitionId $definitionId -Branch $Config.Branch `
        -MinFinishTime $Since -Max $script:CiScanMaxNewerBuildsProbed
    if (-not $listing.Ok) {
        $result.Ok = $false; $result.Reason = 'build-listing-failed'; return $result
    }
    # Truncation is not fatal here the way it is for coverage. We are looking for the
    # EXISTENCE of one red build, and a truncated page still contains real builds — if one
    # of them is red the evidence is genuine. What truncation costs is only the ability to
    # say "definitely none", so it is carried into the reason when nothing was found.
    $expectedBranch = "refs/heads/$($Config.Branch)"

    foreach ($buildId in @($listing.Builds)) {
        $build = Invoke-HttpGetJson -Url "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId`?api-version=7.1"
        if ($null -eq $build) { $result.Ok = $false; $result.Reason = "build-fetch-failed:$buildId"; return $result }
        if ([string](Get-CiScanJsonField -Object $build -Name 'sourceBranch') -cne $expectedBranch) { continue }
        if ($d.AcceptedBuildResults -notcontains [string](Get-CiScanJsonField -Object $build -Name 'result')) { continue }

        # `minTime` is a server-side filter on a parameter the server is free to
        # interpret, so the finish time is re-checked against the same horizon here. A
        # build that finished BEFORE the closure is not evidence that the signature came
        # back after it, and accepting one would reopen an issue on the very observations
        # that were already weighed when it was closed.
        $finished = ConvertFrom-CiScanTimestamp (Get-CiScanJsonField -Object $build -Name 'finishTime')
        if ($null -eq $finished) { $result.Ok = $false; $result.Reason = "unreadable-finish-time:$buildId"; return $result }
        if ($finished -le $Since.ToUniversalTime()) { continue }

        $timeline = Invoke-HttpGetJson -Url "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId/timeline`?api-version=7.1"
        $timelineRecords = Get-CiScanJsonField -Object $timeline -Name 'records'
        if ($null -eq $timeline -or $null -eq $timelineRecords) {
            $result.Ok = $false; $result.Reason = "timeline-fetch-failed:$buildId"; return $result
        }

        # Only 'failed' counts. 'no-run' is silence and 'clean' is the opposite of
        # evidence, exactly as in the coverage probe.
        if ((Get-CiScanLegOutcome -Records @($timelineRecords) -Legs $Legs -Defaults $d) -eq 'failed') {
            $result.Observed = $true
            $result.BuildId = $buildId
            $result.Reason = "affected-leg-failed-in-build:$buildId"
            return $result
        }
    }

    if ($listing.Truncated) {
        # Not an observation, and not a clean bill of health either. The listing is
        # finishTimeDescending, so an overflow drops the OLDEST builds since the closure
        # — and drops them permanently, since the probe re-runs from the same `closed_at`
        # horizon every time and never revisits them. Reporting that as "no recurrence"
        # would be the same absence-of-evidence error the coverage path exists to avoid,
        # so `Ok = $false` says "we could not check", which the summary renders as
        # not-verified instead of as silence. A recurrence found INSIDE the truncated
        # page is unaffected: the loop above returns before ever reaching here.
        $result.Ok = $false
        $result.Reason = 'no-recurrence-in-probed-builds-listing-truncated'
    }
    return $result
}

#endregion

#region Notices ---------------------------------------------------------------

function New-CiScanCandidateNotice {
    <#
    .SYNOPSIS
        Builds the one-time "this issue looks stale" comment.
    .DESCRIPTION
        Every value interpolated here is produced by trusted code in this repository
        (integers, thresholds, the twin's own configured names). No text originating from
        an agent, an issue body, a PR body, or a CI log is echoed, so the comment cannot
        become a relay for injected content.

        The veto gestures listed in the body MUST stay in sync with the signals
        `Test-CiScanHumanTouched` actually enforces. Removing the
        `ci-scan-stale-candidate` label is deliberately NOT offered: it is not a veto,
        because `Get-CiScanProposedActions` simply re-adds the label and re-notifies on
        the next run while the issue still qualifies as a candidate.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Verdict, [Parameter(Mandatory)][hashtable]$Config)

    $d = Get-CiScanDefaults
    return @"
🤖 **Automated staleness check**

The failure signature tracked by this issue has not been observed in the last **$($Verdict.VerifiedAbsences)** independently verified complete builds of ``$($Verdict.Pipeline)`` on ``$($Config.Branch)`` (threshold for this signature: **$($Verdict.RequiredAbsences)**).

If nothing changes, this issue becomes eligible for automatic closure.

**To stop that, do any one of these** — each is a signal the reconciler treats as human ownership and will not auto-act on:

- assign the issue to someone,
- set a milestone,
- add an area, priority, status, or partner label (``area-*``, ``p/*``, ``s/*``, ``partner/*``, ``legacy-area-*``), or
- just leave a comment here.

<sub>Posted by the ``ci-scan-reconcile`` workflow. Thresholds: min age $($d.MinIssueAgeDays)d, min quiet $($d.MinQuietDays)d, max wait $($d.MaxWaitDays)d.</sub>
"@
}

function New-CiScanClosingNotice {
    <#
    .SYNOPSIS
        Builds the closing-evidence comment. Same trusted-values-only rule as above.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Verdict, [Parameter(Mandatory)][hashtable]$Config)

    $builds = if (@($Verdict.AbsentBuildIds).Count -gt 0) { (@($Verdict.AbsentBuildIds) -join ', ') } else { 'n/a' }
    return @"
🤖 **Closing as stale**

- Pipeline: ``$($Verdict.Pipeline)`` on ``$($Config.Branch)``
- Verified clean builds since the observation clock started: **$($Verdict.VerifiedAbsences)** (required: $($Verdict.RequiredAbsences))
- Issue age: $($Verdict.AgeDays)d · quiet period: $($Verdict.QuietDays)d
- No open pull request references this issue.
- Build IDs: $builds

Each of those builds was re-fetched from Azure DevOps by this workflow and confirmed to be a completed build of the expected pipeline definition on ``$($Config.Branch)``, with every affected leg actually executed.

**If this failure recurs, the scanner will file a fresh issue** — closing here loses no coverage. Reopen this issue if you disagree.

<sub>Closed by the ``ci-scan-reconcile`` workflow (``auto-closed-stale``).</sub>
"@
}

function New-CiScanReopenNotice {
    <#
    .SYNOPSIS
        Builds the reopening-evidence comment. Same trusted-values-only rule as above.
    .DESCRIPTION
        Every interpolated value is either a workflow constant or an integer this script
        parsed itself — the build ID comes from an AzDO listing, never from issue text.
        Nothing an issue body, comment, or CI log can influence reaches this string, which
        is the same rule the candidate and closing notices follow and the reason none of
        them accept free text.
    #>
    [CmdletBinding()]
    param([Parameter(Mandatory)]$Verdict, [Parameter(Mandatory)][hashtable]$Config)

    $d = Get-CiScanDefaults
    $build = if ($Verdict.RecurrenceBuildId -gt 0) {
        "[$($Verdict.RecurrenceBuildId)](https://dev.azure.com/dnceng-public/public/_build/results?buildId=$($Verdict.RecurrenceBuildId))"
    }
    else { 'n/a' }

    return @"
🤖 **Reopening: this signature came back**

This issue was auto-closed as stale, and the failure it tracks has since been observed again on ``$($Config.Branch)``.

- Recurrence observed in build: $build
- An affected leg of ``$($Verdict.Pipeline)`` ran in that build and **failed**.
- The build finished **after** this issue was closed.
- Closure was inside the $($d.ReopenWindowDays)-day reopen window.

Closing this issue was therefore premature, and the automation is undoing its own action rather than leaving a real failure untracked.

<sub>Reopened by the ``ci-scan-reconcile`` workflow. Only issues carrying ``auto-closed-stale`` are eligible.</sub>
"@
}

#endregion

#region Reporting -------------------------------------------------------------

function Format-CiScanSummary {
    <#
    .SYNOPSIS
        Renders the human-review report: what would happen, and exactly why.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]$Report
    )

    $sb = [System.Text.StringBuilder]::new()
    $null = $sb.AppendLine("## ci-scan reconciler — ``$($Report.Label)`` (branch ``$($Report.Branch)``)")
    $null = $sb.AppendLine()
    $null = $sb.AppendLine("| | |")
    $null = $sb.AppendLine("|---|---|")
    $null = $sb.AppendLine("| Mode requested | ``$($Report.RequestedMode)`` |")
    $null = $sb.AppendLine("| Mode effective | **``$($Report.EffectiveMode)``** |")
    $null = $sb.AppendLine("| Mutations permitted | $(if ($Report.MutationsAllowed) { 'yes' } else { '**no — report only**' }) |")
    $null = $sb.AppendLine("| Closures permitted | $(if ($Report.ClosuresAllowed) { 'yes' } else { '**no**' }) |")
    $null = $sb.AppendLine("| Mutating API calls made | **$($Report.Counters.Writes)** |")
    $null = $sb.AppendLine("| Read errors | $($Report.Counters.ReadErrors) |")
    $null = $sb.AppendLine("| Write errors | $($Report.Counters.WriteErrors) |")
    if ($Report.AbortedAt) {
        $null = $sb.AppendLine("| **Apply ABORTED at** | **$($Report.AbortedAt)** |")
    }
    $null = $sb.AppendLine("| Fail-closed | $(if ($Report.FailClosed) { "**yes — $($Report.FailClosedReason)**" } else { 'no' }) |")
    $null = $sb.AppendLine("| Issues evaluated | $($Report.IssueCount) |")

    # Three independent ways the survey can fall short, deliberately reported apart.
    #
    # `IssuesTruncated` is a BOUND signal: the issue listing hit `-MaxIssues` (or a page
    # read failed), so there may be issues nobody looked at. `ReadErrors` is a COMPLETENESS
    # signal covering every other read — issue comments, label listing — any of which
    # failing means a verdict was computed from partial data. The PR index carries its own
    # BOUND signal: it can be read without error yet still be short of every open fix PR,
    # which is the one index whose incompleteness could hide a closure blocker.
    #
    # Collapsing them lets a run that read the whole issue list but failed three PR reads
    # print "Survey complete: yes" next to "Read errors: 3". Mutations are already
    # fail-closed in that state, but the report is what a human reads during the review
    # phase, and it must not over-claim.
    $null = $sb.AppendLine("| Issue listing bounded | $(if ($Report.IssuesTruncated) { '**yes — hit the `-MaxIssues` bound or a page read failed; oldest issues shown**' } else { 'no — listing read to exhaustion' }) |")
    $null = $sb.AppendLine("| All reads succeeded | $(if ($Report.Counters.ReadErrors -gt 0) { "**no — $($Report.Counters.ReadErrors) read(s) failed; verdicts below used partial data**" } else { 'yes' }) |")
    $null = $sb.AppendLine("| PR blocker index complete | $(if ($Report.PullRequestIndexComplete) { 'yes' } else { '**no — hit the `-MaxPullRequests` bound; an open fix PR may have been missed**' }) |")

    <#
        Headroom against the PR bound, reported every run because it DRIFTS on its own.

        The index covers every open pull request, not just `[ci-fix]` ones, because a PR
        referencing an issue is a closure blocker whoever opened it. So it tracks repo-wide
        PR volume and creeps toward the bound with nobody changing anything here.

        Crossing it is safe but silent: the index reports incomplete, the run fail-closes
        on `pull-request-index-incomplete`, and nothing is closed again until a human
        notices. A comment in the param block documents that; only a row DETECTS it.
        Warning early turns a silent stop into a scheduled bump.
    #>
    if ($Report.MaxPullRequests -gt 0) {
        $pct = [math]::Round(100 * $Report.PullRequestCount / $Report.MaxPullRequests)
        $headroom = "$($Report.PullRequestCount) / $($Report.MaxPullRequests) ($pct% of bound)"
        if (-not $Report.PullRequestIndexComplete) {
            $headroom = "**$headroom — BOUND REACHED; raise ``-MaxPullRequests``**"
        }
        elseif ($pct -ge 80) {
            $headroom = "**$headroom — approaching the bound; raise ``-MaxPullRequests`` before it fail-closes**"
        }
        $null = $sb.AppendLine("| PR index headroom | $headroom |")
    }

    $surveyComplete = (-not $Report.IssuesTruncated) -and ($Report.Counters.ReadErrors -eq 0) -and $Report.PullRequestIndexComplete
    $null = $sb.AppendLine("| Survey complete | $(if ($surveyComplete) { 'yes' } else { '**no — see the rows above**' }) |")
    $null = $sb.AppendLine("| Generated | $($Report.GeneratedAt) |")
    $null = $sb.AppendLine()

    if ($Report.MutationsAllowed) {
        $null = $sb.AppendLine("> **Mutating run.** $($Report.Counters.Writes) GitHub write call(s) were made: " +
            "$($Report.Counters.Closes) close(s), $($Report.Counters.Comments) comment(s), $($Report.Counters.LabelOps) label op(s).")
    }
    else {
        $null = $sb.AppendLine("> **Report only — no mutations were performed.** Everything under *Proposed actions* " +
            "is a preview of what ``enforce`` mode would do. GitHub write calls made: $($Report.Counters.Writes).")
    }
    $null = $sb.AppendLine()

    $null = $sb.AppendLine("### Decisions")
    $null = $sb.AppendLine()
    $null = $sb.AppendLine('| Decision | Count | Meaning |')
    $null = $sb.AppendLine('|---|---:|---|')
    $meanings = [ordered]@{
        'candidate'              = 'All gates passed — closable in `enforce` mode'
        'active'                 = 'An open PR references the issue — hands off'
        'watching'               = 'Tracked; absence/age/quiet threshold not yet met'
        'awaiting-canonical-data' = 'No canonical fingerprint / no observations — never closable'
        'needs-human'            = 'Escalated; automation will not act'
    }
    foreach ($k in $meanings.Keys) {
        $n = @($Report.Verdicts | Where-Object { $_.Decision -eq $k }).Count
        $null = $sb.AppendLine("| ``$k`` | $n | $($meanings[$k]) |")
    }
    $null = $sb.AppendLine()

    $buckets = $Report.Verdicts | Group-Object LegacyBucket | Sort-Object Name
    if (@($buckets).Count -gt 0) {
        $null = $sb.AppendLine('### Backlog buckets')
        $null = $sb.AppendLine()
        $null = $sb.AppendLine('| Bucket | Count |')
        $null = $sb.AppendLine('|---|---:|')
        foreach ($b in $buckets) { $null = $sb.AppendLine("| ``$($b.Name)`` | $($b.Count) |") }
        $null = $sb.AppendLine()
    }

    $actionable = @($Report.Verdicts | Where-Object { $_.Decision -in @('candidate', 'active', 'watching') -or @($_.ProposedActions).Count -gt 0 })
    $null = $sb.AppendLine("### Proposed actions ($(@($actionable).Count) issue(s))")
    $null = $sb.AppendLine()
    if (@($actionable).Count -eq 0) {
        $null = $sb.AppendLine('_No issue is currently tracked toward closure._')
    }
    else {
        $null = $sb.AppendLine('| Issue | Decision | Proposed action(s) | Reason | Verified absences | Required | Build IDs | Age (d) | Quiet (d) | Blocking PRs | Cap |')
        $null = $sb.AppendLine('|---:|---|---|---|---:|---:|---|---:|---:|---|---|')
        foreach ($v in ($actionable | Sort-Object -Property Decision, @{ Expression = 'VerifiedAbsences'; Descending = $true }, Number)) {
            $prs = if (@($v.BlockingPrs).Count -gt 0) { (@($v.BlockingPrs | ForEach-Object { "#$($_.Number)" }) -join ' ') } else { '—' }
            $acts = if (@($v.ProposedActions).Count -gt 0) { '`' + ((@($v.ProposedActions)) -join '`, `') + '`' } else { '—' }
            $builds = if (@($v.AbsentBuildIds).Count -gt 0) { (@($v.AbsentBuildIds) -join ' ') } else { '—' }
            $null = $sb.AppendLine(("| [#{0}](https://github.com/{1}/{2}/issues/{0}) | ``{3}`` | {4} | {5} | {6} | {7} | {8} | {9} | {10} | {11} | {12} |" -f
                $v.Number, $Report.Owner, $Report.Repo, $v.Decision, $acts, $v.Reason,
                $v.VerifiedAbsences, $v.RequiredAbsences, $builds, $v.AgeDays,
                $(if ($null -eq $v.QuietDays) { '—' } else { $v.QuietDays }), $prs, $v.CapDecision))
        }
    }
    $null = $sb.AppendLine()

    <#
        The reopen section is rendered UNCONDITIONALLY, including when it is empty and
        including in report mode. An empty section is the evidence that the safety net ran
        and found nothing; omitting it when there is nothing to say would make "the net is
        attached but quiet" and "the net was never invoked" render identically — which is
        precisely the state this whole change corrects, and the operator would have no way
        to tell the difference from the summary alone.

        A report that does not carry the field at all is a THIRD state, and it is rendered
        as its own warning rather than folded into either. `$Report.ReopenVerdicts` on a
        report lacking the key is a terminating error under StrictMode, and this function
        renders the audit trail for a run that may have already mutated issues — so a
        missing field must never be the thing that destroys the record of what was done.
        Degrading loudly keeps the summary total; degrading silently into "nothing to
        reopen" would be the same conflation one level up.
    #>
    $hasReopenField = if ($Report -is [System.Collections.IDictionary]) {
        $Report.Contains('ReopenVerdicts')
    }
    else { Test-CiScanHasField -Object $Report -Name 'ReopenVerdicts' }

    if (-not $hasReopenField) {
        $null = $sb.AppendLine('### Reopen review')
        $null = $sb.AppendLine()
        $null = $sb.AppendLine('> ⚠️ This report carries no reopen data. The false-close safety net did **not** run for this report.')
    }
    else {
        $reopens = @($Report.ReopenVerdicts)
        $toReopen = @($reopens | Where-Object { $_.Decision -eq 'reopen' })
        $null = $sb.AppendLine("### Reopen review ($(@($reopens).Count) auto-closed issue(s) examined, $(@($toReopen).Count) to reopen)")
        $null = $sb.AppendLine()
        if ($Report.ReopensTruncated) {
            $null = $sb.AppendLine('> ⚠️ The auto-closed listing was truncated; this reopen review is **not** exhaustive.')
            $null = $sb.AppendLine()
        }
        if (@($reopens).Count -eq 0) {
            $null = $sb.AppendLine('_No issue carries `auto-closed-stale`, so there is nothing this automation could have closed incorrectly._')
        }
        else {
            $null = $sb.AppendLine('| Issue | Decision | Reason | Recurrence build | Evidence | Applied |')
            $null = $sb.AppendLine('|---:|---|---|---|---|---|')
            foreach ($rv in ($reopens | Sort-Object Decision, Number)) {
                $rbuild = if ($rv.RecurrenceBuildId -gt 0) { "``$($rv.RecurrenceBuildId)``" } else { '—' }
                # An unchecked issue must not read as a clean one. `EvidenceOk = $false` means
                # we could not look, which is a different claim from "we looked and it is fine".
                $ev = if ($rv.EvidenceOk) { $rv.EvidenceReason } else { "⚠️ not-verified: $($rv.EvidenceReason)" }
                $null = $sb.AppendLine("| [#$($rv.Number)](https://github.com/$($Report.Owner)/$($Report.Repo)/issues/$($rv.Number)) | ``$($rv.Decision)`` | $($rv.Reason) | $rbuild | $ev | $(if ($rv.Applied) { 'yes' } else { 'no' }) |")
            }
        }
    }
    $null = $sb.AppendLine()
    $null = $sb.AppendLine('### Escalations and exclusions')
    $null = $sb.AppendLine()
    $null = $sb.AppendLine('| Issue | Decision | Reason | Detail |')
    $null = $sb.AppendLine('|---:|---|---|---|')
    $others = @($Report.Verdicts | Where-Object { $_.Decision -in @('needs-human', 'awaiting-canonical-data') })
    foreach ($v in ($others | Sort-Object Reason, Number | Select-Object -First 200)) {
        $detail = if (@($v.Detail).Count -gt 0) { (@($v.Detail) -join '; ') } else { '—' }
        $null = $sb.AppendLine("| #$($v.Number) | ``$($v.Decision)`` | $($v.Reason) | $detail |")
    }
    $null = $sb.AppendLine()
    if (-not $Report.MutationsAllowed) {
        $null = $sb.AppendLine('> **This run made no writes.** `Mutating API calls made` above is the direct counter from the single write choke point.')
    }
    return $sb.ToString()
}

#endregion

#region Main ------------------------------------------------------------------

function Invoke-CiScanReconcile {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Label,
        [Parameter(Mandatory)][string]$Owner,
        [Parameter(Mandatory)][string]$Repo,
        [int]$MaxIssues,
        [int]$MaxPullRequests,
        [string]$RequestedMode = 'report'
    )

    # Apply the mode gate here rather than relying on load-time state. This makes the
    # function self-contained: every entry point (script body, test, future caller) goes
    # through the same case-sensitive allow-list, and the post-condition at the bottom is
    # checked against the mode this call actually ran under.
    $null = Set-CiScanReconcileMode -RequestedMode $RequestedMode
    Reset-CiScanCounters

    $config = Get-CiScanTwinConfig -Label $Label
    $defaults = Get-CiScanDefaults
    $now = (Get-Date).ToUniversalTime()

    Write-Host "ci-scan reconciler | label=$Label branch=$($config.Branch) requested-mode=$RequestedMode effective-mode=$($script:EffectiveMode)"
    if (-not $script:MutationsAllowed) { Write-Host 'REPORT MODE: no GitHub mutations will be attempted.' }

    $issueIndex = Get-CiScanOpenIssues -Owner $Owner -Repo $Repo -Label $Label -Max $MaxIssues
    $issues = @($issueIndex.Issues)
    Write-Host "Fetched $($issues.Count) open issue(s) labelled '$Label' (oldest first); truncated=$($issueIndex.Truncated)."
    if ($issueIndex.Truncated) {
        Write-Warning ("Issue listing hit the -MaxIssues bound of $MaxIssues; this run surveyed only the " +
            "$($issues.Count) oldest open '$Label' issue(s). The report is NOT an exhaustive survey.")
    }

    $prIndex = Get-CiScanPullRequestIndex -Owner $Owner -Repo $Repo -Max $MaxPullRequests
    Write-Host "Fetched $(@($prIndex.PullRequests).Count) pull request(s); complete=$($prIndex.Complete)."

    $verdicts = @()
    foreach ($issue in @($issues)) {
        if ($null -eq $issue) { continue }

        $number = 0
        if (-not [int]::TryParse([string](Get-CiScanJsonField -Object $issue -Name 'number'), [ref]$number) -or $number -le 0) {
            # This loop has no try/catch, so a bare `[int]$issue.number` ended the whole
            # survey on one malformed record. Counting it as a read error skips the issue
            # AND fails the run closed, so a partial survey cannot look clean.
            Write-Warning 'An issue record has no readable number; skipping it and failing the run closed.'
            $script:Counters.ReadErrors++
            continue
        }
        $fixStatus = Get-CiScanFixPrStatus -IssueNumber $number -PullRequests $prIndex.PullRequests

        # Comments are only needed to detect human involvement; fetch them lazily for
        # issues that are otherwise plausible tracking issues.
        $commenters = @()
        $commentsOk = $true
        $prov = Test-CiScanIssueProvenance -Issue $issue -Config $config
        if ($prov.Ok) {
            $c = Get-CiScanHumanCommenters -Owner $Owner -Repo $Repo -Number $number
            $commenters = @($c.Logins)
            $commentsOk = $c.Ok
        }

        # Coverage is only meaningful once a canonical fingerprint and state marker
        # exist. Evaluating it for the legacy backlog would be ~200 pointless AzDO calls.
        $coverage = $null
        $body = [string](Get-CiScanJsonField -Object $issue -Name 'body')
        $fp = Get-CiScanFingerprintMarker -Body $body -Config $config
        $stateResult = Get-CiScanStateMarker -Body $body -Config $config
        if ($null -ne $fp -and $stateResult.Status -eq 'ok') {
            # The marker's write time is the TIME half of the coverage horizon. Stored as
            # a normalised string (or `$null` when the marker predates the field), and a
            # `[datetime]` parameter cannot bind `$null`, so the absent case is converted
            # explicitly — coverage reads MinValue as "no horizon" and fails closed.
            $markerUpdatedAt = ConvertFrom-CiScanTimestamp $stateResult.State.updated_at
            if ($null -eq $markerUpdatedAt) { $markerUpdatedAt = [datetime]::MinValue }
            $coverage = Get-CiScanBuildCoverage -Config $config -Pipeline $fp.Pipeline `
                -Legs (Get-CiScanAffectedLegs -Body $body) `
                -ClaimedBuildIds @($stateResult.State.absent_builds) `
                -KnownBuildIds @($stateResult.State.present_builds) `
                -MarkerUpdatedAt $markerUpdatedAt
        }

        $verdict = Get-CiScanIssueVerdict -Issue $issue -Config $config -Now $now `
            -FixPrStatus $fixStatus -Coverage $coverage -HumanCommenters $commenters

        if (-not $commentsOk -and $verdict.Decision -eq 'candidate') {
            # Could not prove the absence of human comments — refuse to treat as candidate.
            $verdict.Decision = 'needs-human'
            $verdict.Reason = 'comment-fetch-failed'
        }

        $labelNames = Get-CiScanIssueLabelNames -Issue $issue
        $verdict | Add-Member -NotePropertyName HasCandidateLabel `
            -NotePropertyValue ($labelNames -ccontains 'ci-scan-stale-candidate') -Force
        $verdict | Add-Member -NotePropertyName ExistingLabels -NotePropertyValue @($labelNames) -Force
        $verdicts += $verdict
    }

    # ---- Reopen survey (the false-close safety net) ---------------------------
    <#
        `Get-CiScanReopenVerdict` has existed, been tested, and been advertised in this
        script's own header since the first draft, but nothing ever called it: the only
        issue listing in the reconciler was `state=open`, so the one function whose job is
        to undo an incorrect closure could never be handed a closed issue. An untested
        safety net is a risk; a net that is fully tested and simply not attached is worse,
        because the tests make it look present.

        This survey is what attaches it. It is read-only in every mode — the verdicts are
        computed and reported unconditionally, and only the apply loop below (already
        gated on `enforce`) can act on them. A report run therefore says exactly what it
        would have reopened, which is the same contract the closure path offers.
    #>
    $reopenVerdicts = @()
    # Bounded by `MaxReopenSurvey`, NOT by `MaxCloses`. This is a read bound and
    # `MaxCloses` is a write budget; reusing the write budget capped the newest-first
    # listing at five closures while `ReopenWindowDays` is sixty, so a handful of busy
    # enforce runs pushed older still-eligible closures off the end of the page, where
    # nothing would ever probe them for recurrence again.
    $closedIndex = Get-CiScanClosedReconcilerIssues -Owner $Owner -Repo $Repo -Label $Label -Max $defaults.MaxReopenSurvey
    Write-Host "Fetched $(@($closedIndex.Issues).Count) auto-closed issue(s) for reopen review; truncated=$($closedIndex.Truncated)."

    foreach ($issue in @($closedIndex.Issues)) {
        if ($null -eq $issue) { continue }

        $rnumber = 0
        if (-not [int]::TryParse([string](Get-CiScanJsonField -Object $issue -Name 'number'), [ref]$rnumber) -or $rnumber -le 0) {
            Write-Warning 'A closed issue record has no readable number; skipping it and failing the run closed.'
            $script:Counters.ReadErrors++
            continue
        }

        # Same provenance gate the open path uses. A closed issue carrying our label is
        # still only a candidate for reopening if it is genuinely one of ours.
        $rprov = Test-CiScanIssueProvenance -Issue $issue -Config $config
        if (-not $rprov.Ok) { continue }

        # Evidence first, verdict second — and the evidence is only gathered for issues
        # that could still act on it. Probing AzDO for a closure that is already outside
        # the reopen window would spend real API calls to reach a foregone conclusion.
        $closedAt = ConvertFrom-CiScanTimestamp (Get-CiScanJsonField -Object $issue -Name 'closed_at')
        $recurrence = @{ Observed = $false; BuildId = 0; Ok = $true; Reason = 'not-probed' }
        $rpipeline = 'unknown'
        # `auto-closed-stale` is never removed — the open path reads it as the
        # `reopened-after-auto-close` needs-human gate — so it proves a PAST closure was
        # ours, not this one. An issue auto-closed, reopened, then closed again by a
        # person still carries it. `closed_by` is the only field that names the CURRENT
        # closer, and it is a free payload read, so it is checked here for the same
        # reason the window is: an issue that can never be reopened must not cost real
        # AzDO calls. The verdict re-checks it independently.
        $rclosedByUs = $script:CiScanAllowedCreators -ccontains (Get-CiScanClosureActor -Issue $issue)
        if ($rclosedByUs -and $null -ne $closedAt -and ($now - $closedAt).TotalDays -le $defaults.ReopenWindowDays) {
            $rbody = [string](Get-CiScanJsonField -Object $issue -Name 'body')
            $rfp = Get-CiScanFingerprintMarker -Body $rbody -Config $config
            if ($null -ne $rfp) {
                $rpipeline = [string]$rfp.Pipeline
                $recurrence = Test-CiScanRecurrenceSince -Config $config -Pipeline $rfp.Pipeline `
                    -Legs (Get-CiScanAffectedLegs -Body $rbody) -Since $closedAt
            }
            else {
                $recurrence.Ok = $false
                $recurrence.Reason = 'no-canonical-fingerprint'
            }
        }

        $rv = Get-CiScanReopenVerdict -Issue $issue -Config $config -Now $now `
            -RecurrenceObserved:([bool]$recurrence.Observed)
        $rv | Add-Member -NotePropertyName EvidenceOk -NotePropertyValue ([bool]$recurrence.Ok) -Force
        $rv | Add-Member -NotePropertyName EvidenceReason -NotePropertyValue ([string]$recurrence.Reason) -Force
        $rv | Add-Member -NotePropertyName RecurrenceBuildId -NotePropertyValue ([int]$recurrence.BuildId) -Force
        $rv | Add-Member -NotePropertyName Pipeline -NotePropertyValue $rpipeline -Force
        $rv | Add-Member -NotePropertyName Applied -NotePropertyValue $false -Force
        $reopenVerdicts += $rv
    }

    # ---- Run-level fail-closed ------------------------------------------------
    $failClosed = $false
    $failReason = ''
    if (-not $prIndex.Complete) { $failClosed = $true; $failReason = 'pull-request-index-incomplete' }
    elseif ($script:Counters.ReadErrors -gt 0) { $failClosed = $true; $failReason = "read-errors:$($script:Counters.ReadErrors)" }
    elseif ((Get-CiScanCount $issues) -eq 0 -and ($issueIndex.Truncated -or $MaxIssues -le 0)) {
        # An empty backlog has two meanings and only one of them is a failure.
        #
        # This used to fail closed on emptiness alone, which conflated "the listing could
        # not be read" with "every tracker is closed" — the HEALTHY steady state this
        # tool is designed to reach. It cost nothing on the close path (there is nothing
        # open to close), but it sits in front of the same gate as the REOPEN loop, so
        # the false-close safety net switched itself off on exactly the day it became the
        # only thing left running, and a wrongly-closed tracker stayed unreopenable for
        # as long as the backlog stayed empty.
        #
        # `Truncated` is the real signal: it is set when a page read failed or the page
        # ceiling was hit, i.e. when the backlog was genuinely not seen. `MaxIssues -le 0`
        # is checked separately because a zero budget never enters the paging loop and so
        # reports `Truncated = $false` while having surveyed nothing.
        $failClosed = $true
        $failReason = if ($MaxIssues -le 0) { 'no-issue-budget' } else { 'issue-listing-unproven' }
    }
    elseif ($script:MutationsAllowed) {
        # Every label this reconciler owns must already exist before it is allowed to
        # mutate anything. None of the three exist in dotnet/maui today, so without this
        # a first `enforce` run would close issues and then fail to stamp
        # `auto-closed-stale` on any of them — the exact partial state that makes the
        # closures unreopenable by `Get-CiScanReopenVerdict`. Checked once per run, in a
        # read-only call, and it fails the whole run closed rather than per-issue.
        $missing = @(Test-CiScanOwnedLabels -Owner $Owner -Repo $Repo)
        if ((Get-CiScanCount $missing) -gt 0) {
            $failClosed = $true
            $failReason = "missing-owned-labels:$($missing -join ',')"
        }
    }

    # ---- Plan actions (mode-independent) and apply caps ----------------------
    $budgets = @{ close = $defaults.MaxCloses; comment = $defaults.MaxComments; label = $defaults.MaxLabelOps }

    foreach ($v in ($verdicts | Sort-Object -Property @{ Expression = 'VerifiedAbsences'; Descending = $true }, Number)) {
        $desired = Get-CiScanProposedActions -Verdict $v `
            -AlreadyLabelledCandidate:($v.HasCandidateLabel) -ExistingLabels @($v.ExistingLabels)

        if ((Get-CiScanCount $desired) -eq 0) { $v.ProposedActions = @(); $v.CapDecision = 'n/a'; continue }
        if ($failClosed) { $v.ProposedActions = @(); $v.CapDecision = 'suppressed-fail-closed'; continue }

        $granted = @()
        $capped = @()
        foreach ($action in $desired) {
            $bucket = $null
            if ($action -ceq 'close') { $bucket = 'close' }
            elseif ($action.StartsWith('comment:', [System.StringComparison]::Ordinal)) { $bucket = 'comment' }
            elseif ($action.StartsWith('label:', [System.StringComparison]::Ordinal)) { $bucket = 'label' }
            if ($null -eq $bucket) { continue }
            if ($budgets[$bucket] -le 0) { $capped += $action; continue }
            $budgets[$bucket]--
            $granted += $action
        }
        $v.ProposedActions = @($granted)
        $v.CapDecision = if ((Get-CiScanCount $capped) -gt 0) { "cap-reached:$(@($capped) -join ',')" } else { 'within-cap' }
    }

    # ---- Apply (mutating modes only) -----------------------------------------
    # Every branch below routes through Invoke-GhWrite, which independently re-checks the
    # effective mode. This loop is unreachable in report mode, and would throw rather
    # than write if it somehow were reached.
    #
    # ABORT-ON-FIRST-FAILURE. The loop stops at the first failed write instead of carrying
    # on to the next issue. The motivating shape is close-then-label: a close that lands
    # whose `auto-closed-stale` marker does not leaves an issue closed WITHOUT the marker
    # `Get-CiScanReopenVerdict` requires, so the automation can no longer recognise or undo
    # its own irreversible action. Continuing would turn one such issue into many. Stopping
    # bounds the damage at a single known issue, which the summary names explicitly, and the
    # non-zero exit forces a human to look before anything runs again.
    if ($script:MutationsAllowed -and -not $failClosed) {
        :apply foreach ($v in $verdicts) {
            foreach ($action in @($v.ProposedActions)) {
                if ($action.StartsWith('label:', [System.StringComparison]::Ordinal)) {
                    $name = $action.Substring('label:'.Length)
                    if ($script:CiScanOwnedLabels -cnotcontains $name) {
                        Write-Warning "Refusing to apply non-reconciler label '$name'."
                        continue
                    }
                    if (Invoke-GhWrite -Kind label -IssueNumber $v.Number -GhArgs @(
                            'issue', 'edit', "$($v.Number)", '--repo', "$Owner/$Repo", '--add-label', $name)) {
                        $script:Counters.LabelOps++
                    }
                    else {
                        $script:AbortedAt = "label '$name' on #$($v.Number)"
                        break apply
                    }
                }
                elseif ($action -ceq 'comment:candidate-notice') {
                    $notice = New-CiScanCandidateNotice -Verdict $v -Config $config
                    if (Invoke-GhWrite -Kind comment -IssueNumber $v.Number -GhArgs @(
                            'issue', 'comment', "$($v.Number)", '--repo', "$Owner/$Repo", '--body', $notice)) {
                        $script:Counters.Comments++
                    }
                    else {
                        $script:AbortedAt = "comment on #$($v.Number)"
                        break apply
                    }
                }
                elseif ($action -ceq 'close') {
                    # Redundant with the check inside Invoke-GhWrite. Kept deliberately:
                    # two independent guards on the only irreversible operation.
                    if ($script:ClosuresAllowed) {
                        $closing = New-CiScanClosingNotice -Verdict $v -Config $config
                        if (-not (Invoke-GhWrite -Kind close -IssueNumber $v.Number -GhArgs @(
                                    'issue', 'close', "$($v.Number)", '--repo', "$Owner/$Repo",
                                    '--reason', 'completed', '--comment', $closing))) {
                            $script:AbortedAt = "close of #$($v.Number)"
                            break apply
                        }
                        $script:Counters.Closes++

                        # The marker half of the close. Losing this is the inconsistent
                        # state described above, so it aborts rather than warns.
                        if (-not (Invoke-GhWrite -Kind label -IssueNumber $v.Number -GhArgs @(
                                    'issue', 'edit', "$($v.Number)", '--repo', "$Owner/$Repo",
                                    '--add-label', 'auto-closed-stale'))) {
                            $script:AbortedAt = "auto-closed-stale marker on #$($v.Number) (issue is CLOSED WITHOUT its marker)"
                            break apply
                        }
                        $script:Counters.LabelOps++
                    }
                }
                else { Write-Warning "Unknown action '$action' ignored." }
            }
        }

        <#
            Reopens run after the close/label/comment pass, in their own loop, for a
            reason that is not cosmetic: the two collections are disjoint (open issues vs
            closed ones) but the abort semantics are not shared. `break apply` above stops
            the closure pass at the first failed write to bound the damage of a
            half-applied close. A reopen has no second half — it is one call, and it is
            the only mutation in this script that makes the world MORE conservative — so
            failing to reopen issue A is not a reason to skip issue B. It is counted,
            which fails the run closed, and the loop continues.

            Still gated on `$script:ClosuresAllowed`, and `Invoke-GhWrite` re-checks it
            independently, so this is two guards on the reopen path exactly as on close.
        #>
        if ($script:ClosuresAllowed) {
            $reopenBudget = $defaults.MaxCloses
            foreach ($rv in @($reopenVerdicts | Where-Object { $_.Decision -eq 'reopen' })) {
                if ($reopenBudget -le 0) { $rv.Reason = 'cap-reached:reopen'; continue }
                $reopenBudget--
                if (Invoke-GhWrite -Kind reopen -IssueNumber $rv.Number -GhArgs @(
                        'issue', 'reopen', "$($rv.Number)", '--repo', "$Owner/$Repo",
                        '--comment', (New-CiScanReopenNotice -Verdict $rv -Config $config))) {
                    $rv.Applied = $true
                    $script:Counters.Reopens++
                }
            }
        }
    }

    # ---- Post-condition: a non-mutating mode MUST have made zero mutating calls ------
    # Asserted here (rather than only at the end of the script) so the guarantee is
    # covered by unit tests and holds for every caller of this function.
    if (-not $script:MutationsAllowed -and $script:Counters.Writes -ne 0) {
        throw "SAFETY VIOLATION: $($script:Counters.Writes) mutating call(s) attempted in mode '$($script:EffectiveMode)'."
    }
    if (-not $script:ClosuresAllowed -and $script:Counters.Closes -ne 0) {
        throw "SAFETY VIOLATION: $($script:Counters.Closes) closure(s) attempted in mode '$($script:EffectiveMode)'."
    }
    # Reopens are asserted separately rather than folded into the closure check. They are
    # a distinct counter incremented on a distinct code path, so a single assertion over
    # `Closes` would leave the reopen path with no post-condition at all — the exact shape
    # of "tested but never reached" that this whole change exists to remove.
    if (-not $script:ClosuresAllowed -and $script:Counters.Reopens -ne 0) {
        throw "SAFETY VIOLATION: $($script:Counters.Reopens) reopen(s) attempted in mode '$($script:EffectiveMode)'."
    }

    return @{
        Label             = $Label
        Branch            = $config.Branch
        Owner             = $Owner
        Repo              = $Repo
        RequestedMode     = $RequestedMode
        EffectiveMode     = $script:EffectiveMode
        MutationsAllowed  = $script:MutationsAllowed
        ClosuresAllowed   = $script:ClosuresAllowed
        FailClosed        = $failClosed
        FailClosedReason  = $failReason
        IssueCount        = @($issues).Count
        IssuesTruncated   = [bool]$issueIndex.Truncated
        PullRequestCount  = @($prIndex.PullRequests).Count
        PullRequestIndexComplete = [bool]$prIndex.Complete
        # Carried so the summary can report headroom, not just the pass/fail bound check.
        MaxPullRequests   = $MaxPullRequests
        MaxIssues         = $MaxIssues
        WriteErrors       = $script:Counters.WriteErrors
        AbortedAt         = $script:AbortedAt
        Counters          = $script:Counters
        Thresholds        = $defaults
        Verdicts          = @($verdicts)
        ReopenVerdicts    = @($reopenVerdicts)
        ReopensTruncated  = [bool]$closedIndex.Truncated
        GeneratedAt       = $now.ToString('o')
    }
}

#endregion

# Dot-source guard: tests load this file to exercise the functions without running.
if ($MyInvocation.InvocationName -eq '.' -or $MyInvocation.Line -match '^\s*\.\s') { return }

$report = Invoke-CiScanReconcile -Label $Label -Owner $Owner -Repo $Repo `
    -MaxIssues $MaxIssues -MaxPullRequests $MaxPullRequests -RequestedMode $Mode

$markdown = Format-CiScanSummary -Report $report
Write-Host $markdown

if (-not $OutputPath) { $OutputPath = "ci-scan-reconcile-$Label.json" }
$report | ConvertTo-Json -Depth 8 | Set-Content -Path $OutputPath -Encoding utf8
Write-Host "Wrote JSON report to $OutputPath"

if (-not $SummaryPath -and $env:GITHUB_STEP_SUMMARY) { $SummaryPath = $env:GITHUB_STEP_SUMMARY }
if ($SummaryPath) { Add-Content -Path $SummaryPath -Value $markdown -Encoding utf8 }

# Final assertion: in a non-mutating mode the write counter must be exactly zero.
if (-not $script:MutationsAllowed -and $script:Counters.Writes -ne 0) {
    throw "SAFETY VIOLATION: $($script:Counters.Writes) mutating call(s) in mode '$($script:EffectiveMode)'."
}
Write-Host "Done. mode=$($script:EffectiveMode) writes=$($script:Counters.Writes) closes=$($script:Counters.Closes) labels=$($script:Counters.LabelOps)"

# A partially-applied mutating run must NOT report success. Reaching here with write
# errors means some subset of the planned actions landed and the rest did not — most
# dangerously a close whose `auto-closed-stale` marker failed, which the reopen path can
# no longer recognize. The report has already been written and uploaded, so the operator
# has the full picture; this only denies the green check.
if ($script:Counters.WriteErrors -gt 0) {
    $where = if ($script:AbortedAt) { " The run STOPPED at: $($script:AbortedAt)." } else { '' }
    throw "$($script:Counters.WriteErrors) GitHub write call(s) failed in mode '$($script:EffectiveMode)'; the run applied only part of its plan.$where See the warnings above and the JSON report."
}
