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
    $script:Counters = @{ Closes = 0; Reopens = 0; Comments = 0; LabelOps = 0; Writes = 0; ReadErrors = 0 }
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
    $verb = ($GhArgs | Select-Object -First 2) -join ' '
    if ($verb -cne 'api' -and $verb -cne 'pr list' -and $GhArgs[0] -cne 'api') {
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

        `Truncated` reports whether the bound actually elided anything, so a bounded
        batch can never be misread as "these are all the open tracking issues". That
        signal is only trustworthy if EVERY early exit is accounted for, so the page
        ceiling is derived from `Max` instead of being a constant: a constant ceiling
        silently caps the survey once `Max` exceeds ceiling x 100 and reports
        `Truncated` = $false while doing it. Hitting the ceiling is therefore reported
        as truncation, never as exhaustion.

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
        if ($null -eq $batch) { break }
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
    #>
    [CmdletBinding()]
    param([string]$Owner, [string]$Repo, [int]$Max)

    $fields = 'number,title,body,state,mergedAt,isDraft,baseRefName'
    $open = Invoke-GhRead -GhArgs @('pr', 'list', '--repo', "$Owner/$Repo", '--state', 'open',
        '--limit', "$Max", '--json', $fields)
    $fixes = Invoke-GhRead -GhArgs @('pr', 'list', '--repo', "$Owner/$Repo", '--state', 'all',
        '--search', 'in:title ci-fix', '--limit', "$Max", '--json', $fields)

    $complete = ($null -ne $open -and $null -ne $fixes)
    $all = @()
    $seen = @{}
    foreach ($pr in (@($open) + @($fixes))) {
        if ($null -eq $pr) { continue }
        $n = [int]$pr.number
        if ($seen.ContainsKey($n)) { continue }
        $seen[$n] = $true
        $all += $pr
    }
    return @{ PullRequests = @($all); Complete = $complete }
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
        if ($null -eq $c -or $null -eq $c.user) { continue }
        $login = [string]$c.user.login
        $type = if ($c.user.PSObject.Properties.Name -contains 'type') { [string]$c.user.type } else { '' }
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
            `## Affected Legs`, with a result that means the leg genuinely ran.

        The last check is what separates a real clean build from a build where the
        relevant leg was skipped, gated off, or never scheduled — the failure mode a
        wall-clock rule cannot see.

        Any error, any missing build, any unresolvable leg sets `Unverifiable = $true`,
        which the core converts to zero counted absences.
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

        if ([int]$build.definition.id -ne $definitionId) { $result.Unverifiable = $true; $result.Reason = "definition-mismatch:$buildId"; return $result }
        if ([string]$build.sourceBranch -cne $expectedBranch) { $result.Unverifiable = $true; $result.Reason = "branch-mismatch:$buildId"; return $result }
        if ([string]$build.status -ne 'completed') { continue }
        if ($d.AcceptedBuildResults -notcontains [string]$build.result) { continue }

        $timeline = Invoke-HttpGetJson -Url "https://dev.azure.com/dnceng-public/public/_apis/build/builds/$buildId/timeline`?api-version=7.1"
        if ($null -eq $timeline -or -not ($timeline.PSObject.Properties.Name -contains 'records')) {
            $result.Unverifiable = $true; $result.Reason = "timeline-fetch-failed:$buildId"; return $result
        }

        $records = @($timeline.records)
        $allLegsRan = $true
        foreach ($leg in $Legs) {
            $key = ($leg -split '—')[0].Trim()
            if ([string]::IsNullOrWhiteSpace($key)) { $allLegsRan = $false; break }
            $match = @($records | Where-Object {
                $null -ne $_.name -and ([string]$_.name).IndexOf($key, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
            })
            if ((Get-CiScanCount $match) -eq 0) { $allLegsRan = $false; break }
            $ran = @($match | Where-Object {
                $r = if ($null -eq $_.result) { '' } else { [string]$_.result }
                $r -ne '' -and $d.NonRunningLegResults -notcontains $r
            })
            if ((Get-CiScanCount $ran) -eq 0) { $allLegsRan = $false; break }
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
    $null = $sb.AppendLine("| Fail-closed | $(if ($Report.FailClosed) { "**yes — $($Report.FailClosedReason)**" } else { 'no' }) |")
    $null = $sb.AppendLine("| Issues evaluated | $($Report.IssueCount) |")
    $null = $sb.AppendLine("| Survey complete | $(if ($Report.IssuesTruncated) { '**no — issue listing was truncated by `-MaxIssues`; oldest issues shown**' } else { 'yes' }) |")
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

        $number = [int]$issue.number
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
        $body = if ($issue.PSObject.Properties.Name -contains 'body') { [string]$issue.body } else { '' }
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
    if ($script:MutationsAllowed -and -not $failClosed) {
        foreach ($v in $verdicts) {
            foreach ($action in @($v.ProposedActions)) {
                if ($action.StartsWith('label:', [System.StringComparison]::Ordinal)) {
                    $name = $action.Substring('label:'.Length)
                    if ($script:CiScanOwnedLabels -cnotcontains $name) {
                        Write-Warning "Refusing to apply non-reconciler label '$name'."
                    }
                    elseif (Invoke-GhWrite -Kind label -IssueNumber $v.Number -GhArgs @(
                            'issue', 'edit', "$($v.Number)", '--repo', "$Owner/$Repo", '--add-label', $name)) {
                        $script:Counters.LabelOps++
                    }
                }
                elseif ($action -ceq 'comment:candidate-notice') {
                    $notice = New-CiScanCandidateNotice -Verdict $v -Config $config
                    if (Invoke-GhWrite -Kind comment -IssueNumber $v.Number -GhArgs @(
                            'issue', 'comment', "$($v.Number)", '--repo', "$Owner/$Repo", '--body', $notice)) {
                        $script:Counters.Comments++
                    }
                }
                elseif ($action -ceq 'close') {
                    # Redundant with the check inside Invoke-GhWrite. Kept deliberately:
                    # two independent guards on the only irreversible operation.
                    if ($script:ClosuresAllowed) {
                        $closing = New-CiScanClosingNotice -Verdict $v -Config $config
                        if (Invoke-GhWrite -Kind close -IssueNumber $v.Number -GhArgs @(
                                'issue', 'close', "$($v.Number)", '--repo', "$Owner/$Repo",
                                '--reason', 'completed', '--comment', $closing)) {
                            $script:Counters.Closes++
                            if (Invoke-GhWrite -Kind label -IssueNumber $v.Number -GhArgs @(
                                    'issue', 'edit', "$($v.Number)", '--repo', "$Owner/$Repo",
                                    '--add-label', 'auto-closed-stale')) {
                                $script:Counters.LabelOps++
                            }
                        }
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
