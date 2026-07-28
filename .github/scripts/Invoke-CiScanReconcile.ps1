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
      3. Logic level  — the pure core (`CiScanReconcile.Core.ps1`) can only ever emit the
                        decision 'candidate'; it has no vocabulary for "close".

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
        Executes a read-only `gh` invocation and returns parsed JSON (or $null).
    .DESCRIPTION
        Only ever used for GET-shaped calls. Failures return $null and increment
        ReadErrors; the caller must treat $null as "unknown", which always resolves in
        the conservative direction.
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
    try { return $text | ConvertFrom-Json -ErrorAction Stop }
    catch { $script:Counters.ReadErrors++; return $null }
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
        'enforce'; labels and comments require 'comment' or 'enforce'.
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
        $user = Get-CiScanJsonField -Object $c -Name 'user'
        if ($null -eq $user) { continue }
        $login = [string](Get-CiScanJsonField -Object $user -Name 'login')
        $type = [string](Get-CiScanJsonField -Object $user -Name 'type')
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
        [int[]]$ClaimedBuildIds = @()
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

        # Guarding the `records` COLLECTION above is not enough: the individual records are
        # a separate shape promise. A perfectly well-formed timeline — 200, `records`
        # present, an array — can still carry one entry without `name` or `result`, and
        # dotting into it throws for exactly the same StrictMode reason, aborting the run.
        # The `$null -ne`/`$null -eq` tests below already expressed the right intent; they
        # simply could not run, because the property READ throws before the guard evaluates.
        #
        # Skipping a malformed record is the conservative direction in all three positions:
        # each filter can only SHRINK, which can only make `$allLegsRan` false and drop the
        # build from `VerifiedAbsentBuilds`. A junk record can never help close an issue.
        $records = @($timelineRecords)
        $allLegsRan = $true
        foreach ($leg in $Legs) {
            $key = ($leg -split '—')[0].Trim()
            if ([string]::IsNullOrWhiteSpace($key)) { $allLegsRan = $false; break }
            $match = @($records | Where-Object {
                $recordName = Get-CiScanJsonField -Object $_ -Name 'name'
                $null -ne $recordName -and ([string]$recordName).IndexOf($key, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
            })
            if ((Get-CiScanCount $match) -eq 0) { $allLegsRan = $false; break }
            $ran = @($match | Where-Object {
                $recordResult = Get-CiScanJsonField -Object $_ -Name 'result'
                $r = if ($null -eq $recordResult) { '' } else { [string]$recordResult }
                $r -ne '' -and $d.NonRunningLegResults -notcontains $r
            })
            if ((Get-CiScanCount $ran) -eq 0) { $allLegsRan = $false; break }

            # Execution is not absence. The affected leg is precisely where this
            # signature surfaces, so a leg that RAN AND FAILED is the one outcome that
            # cannot distinguish "the signature is gone" from "the signature fired
            # again and the scanner did not record it". A build-level `failed` is still
            # accepted (some unrelated leg can fail while this one is clean) — but if
            # THIS leg failed, the build proves nothing and must not be counted.
            # Reachable-safe via `$ran` (anything here already produced a non-empty result),
            # but read through the accessor anyway so the shape invariant is local rather
            # than a two-hop proof — and so the unsafe idiom does not survive in this loop.
            $clean = @($ran | Where-Object {
                $d.CleanLegResults -contains ([string](Get-CiScanJsonField -Object $_ -Name 'result'))
            })
            if ((Get-CiScanCount $clean) -eq 0) { $allLegsRan = $false; break }
        }
        if (-not $allLegsRan) { continue }

        $verified += [int]$buildId
    }

    $result.VerifiedAbsentBuilds = @($verified | Sort-Object -Unique)
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
            $coverage = Get-CiScanBuildCoverage -Config $config -Pipeline $fp.Pipeline `
                -Legs (Get-CiScanAffectedLegs -Body $body) `
                -ClaimedBuildIds @($stateResult.State.absent_builds)
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

    # ---- Run-level fail-closed ------------------------------------------------
    $failClosed = $false
    $failReason = ''
    if (-not $prIndex.Complete) { $failClosed = $true; $failReason = 'pull-request-index-incomplete' }
    elseif ($script:Counters.ReadErrors -gt 0) { $failClosed = $true; $failReason = "read-errors:$($script:Counters.ReadErrors)" }
    elseif ((Get-CiScanCount $issues) -eq 0) { $failClosed = $true; $failReason = 'no-issues-fetched' }
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
