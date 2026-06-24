#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Assesses release readiness of a .NET MAUI Servicing Release (SR) branch.

.DESCRIPTION
    Produces a deterministic, evidence-backed answer to "Is release/X.Y.Zxx-srN
    ready to ship?" by:

      1. Computing what is NEW in the SR (commits + source PR refs + reverts)
      2. Querying open `regressed-in-*` issues, walking timelines, and classifying
         each candidate fix PR against SR contents
      3. Querying CI pipelines on the SR branch with freshness check

    All conclusions carry evidence (commit SHAs, PR numbers, ancestry checks)
    and confidence levels. See references/methodology.md for the algorithms
    and the three critical gotchas the skill encodes.

.PARAMETER SrBranch
    SR branch name (e.g. release/10.0.1xx-sr7). Required.

.PARAMETER RegressionLabels
    Comma-separated `regressed-in-*` label names. Required unless
    -InferRegressionLabels is set.

.PARAMETER InferRegressionLabels
    Auto-derive labels from the SR's version family. Agent should ALWAYS
    confirm the inferred labels with the user before using for automation.

.PARAMETER Repo
    Repository in owner/name form. Default: dotnet/maui.

.PARAMETER MainBranch
    Stable branch used for ancestry checks. Default: main.

.PARAMETER ExcludeBranches
    Comma-separated branches to exclude when computing SR-only commits.
    Default: origin/main. Do NOT add inflight/* refs — SR branches cut from
    main; comparing against inflight produces wrong "what's shipping" answers.

.PARAMETER Candidate
    Pre-flight / candidate mode. Use when the next SR branch doesn't exist
    yet but you want to know "what WOULD ship in SRn+1 if cut from main
    today?". With -Candidate, the script treats `origin/$MainBranch` as the
    SR-to-be and uses the named -SrBranch as the prior-SR exclude baseline.

.PARAMETER InheritFromPriorSr
    Only valid with -Candidate. Models the dotnet/maui release workflow where
    SRn+1 is cut from main AND then has SRn merged into it. The "what's
    shipping" set = (main commits since prior SR) ∪ (prior SR-only commits).
    Without this flag, candidate mode shows only main-since-priorSR.

.PARAMETER Phase
    Which phase to run: all (default), ci, commits, regressions, open-prs.

.PARAMETER OutputDir
    Directory for output files. If unset, prints to stdout.

.PARAMETER OutputFormat
    json, markdown, or both (default).

.PARAMETER MaxIssues
    Cap on regression issues to walk. Default: 100.

.PARAMETER NoFetch
    Skip `git fetch`. Use for re-runs with cached refs.

.PARAMETER RepoUrl
    Base URL of the repository web UI. Used to linkify commit SHAs and PR
    numbers in the markdown report. Default: https://github.com/dotnet/maui.

.PARAMETER TrackerKey
    Canonical key used to identify the corresponding tracker issue (e.g.
    `net10-sr7`). When set, the markdown report includes a hidden HTML
    comment marker `<!-- release-readiness-tracker: net10-sr7 -->` and a
    visible "Tracker: …" line so a workflow can match a single tracker
    issue per SR. Optional; omit for ad-hoc local reports.

.PARAMETER MaxBodyBytes
    Hard cap on the rendered markdown body. When the report exceeds this,
    the script truncates and appends a single-line "[Report truncated. See
    artifacts at <link>.]" message. Default: 60000 (≈60KB, well under
    GitHub's 65,536-byte issue body limit).

.EXAMPLE
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 `
        -RegressionLabels regressed-in-10.0.60,regressed-in-10.0.70 `
        -OutputDir CustomAgentLogsTmp/release-readiness/sr7

.EXAMPLE
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Phase commits

.EXAMPLE
    # Pre-flight: what would SR8 contain if cut from main today?
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Candidate `
        -RegressionLabels regressed-in-10.0.70,regressed-in-10.0.80 `
        -OutputDir CustomAgentLogsTmp/release-readiness/sr8-candidate

.EXAMPLE
    # Pre-flight SR8 modeling the SR7→SR8 merge workflow
    pwsh ./Get-ReleaseReadiness.ps1 -SrBranch release/10.0.1xx-sr7 -Candidate `
        -InheritFromPriorSr `
        -RegressionLabels regressed-in-10.0.70,regressed-in-10.0.80 `
        -OutputDir CustomAgentLogsTmp/release-readiness/sr8-candidate
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$SrBranch,
    [string]$RegressionLabels,
    [switch]$InferRegressionLabels,
    [string]$Repo = 'dotnet/maui',
    [string]$MainBranch = 'main',
    [string]$ExcludeBranches = 'origin/main',
    [ValidateSet('all', 'ci', 'commits', 'regressions', 'open-prs')]
    [string]$Phase = 'all',
    [string]$OutputDir,
    [ValidateSet('json', 'markdown', 'both')]
    [string]$OutputFormat = 'both',
    [int]$MaxIssues = 100,
    [switch]$NoFetch,
    # URL base for linkifying commit SHAs and PR numbers in the markdown
    # report. Defaults to the public dotnet/maui repo; override for forks.
    [string]$RepoUrl = 'https://github.com/dotnet/maui',
    # Canonical key for the tracker issue (e.g. net10-sr7). When set, the
    # markdown report embeds tracker + idempotency markers. Optional.
    [string]$TrackerKey,
    # Body-size cap (bytes) for markdown rendering. GitHub issue body limit
    # is 65,536 bytes; default 60,000 leaves headroom for marker comments.
    [int]$MaxBodyBytes = 60000,
    # Candidate / pre-flight mode: survey what WOULD ship in the next SR if cut
    # from main today. Requires -SrBranch to be the prior SR (used as the
    # exclude baseline). Treats origin/main as the "SR-to-be".
    [switch]$Candidate,
    # When set in -Candidate mode, model the dotnet/maui workflow where, after
    # cutting SRn+1 from main, the prior SR (-SrBranch) is merged in. The
    # candidate's "what's shipping" set = main-since-priorSR ∪ priorSR-only commits.
    # Without this flag, candidate mode shows only main-since-priorSR.
    [switch]$InheritFromPriorSr,
    # Skip Maestro/BAR operational checks (default-channel mapping + per-commit
    # BAR build lookup). These run via `darc` CLI and require BAR auth. When darc
    # isn't installed (e.g. minimal CI image), the checks auto-skip and emit
    # UNKNOWN status with verification commands — this switch lets a caller force
    # the skip even when darc IS available (e.g. known auth-failure environment).
    [switch]$SkipMaestroChecks,
    # Skip milestone hygiene checks (current+next milestone existence + stale-open
    # milestone detection). Useful for repos that don't use milestone-per-release.
    [switch]$SkipMilestoneChecks,
    # Query internal (dnceng/internal) AzDO pipelines in addition to the public
    # dnceng-public ones. Off by default: the public Actions runner has no
    # internal AzDO credentials, so the query always returns 401 — which in
    # turn permanently parks the verdict at 🟡 Conditionally Ready with a
    # bogus "unknown" Tier 2 reason. Enable when running locally with AzDO
    # auth (az login / PAT) and you actually want internal signal.
    [switch]$IncludeInternal
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

# Shared nightly-feed freshness helpers (Get-NightlyFeedFreshness / Format-NightlyFeedBanner).
# Defensive load: the banner is auxiliary signal, not part of the verdict, so a missing
# helper must degrade to "no banner" rather than crash the unattended nightly tracker job.
$Script:NightlyFeedHelperLoaded = $false
$nightlyFeedHelperPath = Join-Path $PSScriptRoot 'NightlyFeed.ps1'
if (Test-Path $nightlyFeedHelperPath) {
    . $nightlyFeedHelperPath
    $Script:NightlyFeedHelperLoaded = $true
} else {
    Write-Warning "NightlyFeed.ps1 helper not found at $nightlyFeedHelperPath — nightly-feed banner disabled." -WarningAction Continue
}

# DETERMINISTIC RULE — SR branches in dotnet/maui ALWAYS cut from `main`.
# Refuse to operate on any `inflight/*` or `staging/*` ref — those are
# integration branches, not SR sources. This guard exists because conflating
# the two leads to wrong "what's shipping" conclusions.
$Script:ForbiddenSrPatterns = @(
    '^inflight/'   # inflight/current, inflight/candidate, inflight/ai — NOT SR sources
    '^staging/'    # any staging area
    '^backport/'   # in-progress backport branches
)

# Public AzDO MAUI pipelines on dnceng-public
$Script:PublicPipelines = @(
    @{ Name = 'maui-pr';              DefinitionId = 302; Org = 'dnceng-public'; Project = 'public' }
    @{ Name = 'maui-pr-devicetests';  DefinitionId = 314; Org = 'dnceng-public'; Project = 'public' }
    @{ Name = 'maui-pr-uitests';      DefinitionId = 313; Org = 'dnceng-public'; Project = 'public' }
)
# Internal signed build (best-effort — requires AzDO auth)
$Script:InternalPipelines = @(
    @{ Name = 'dotnet-maui';          DefinitionId = 1095; Org = 'dnceng'; Project = 'internal' }
)

$Script:Warnings = [System.Collections.Generic.List[string]]::new()

function Write-Warn([string]$msg) {
    $Script:Warnings.Add($msg) | Out-Null
    Write-Host "warn: $msg" -ForegroundColor Yellow
}

function Invoke-Git([string]$Cmd) {
    $argList = $Cmd -split ' ' | Where-Object { $_ -ne '' }
    $out = & git @argList 2>$null
    if ($LASTEXITCODE -ne 0) { return $null }
    return $out
}

function Invoke-Gh([string[]]$GhArgs, [switch]$Quiet) {
    # -Quiet suppresses the non-zero-exit warning for callers that handle a
    # $null return themselves and don't want a raw `gh ... exited` line leaking
    # into $Script:Warnings (which is rendered into the tracker issue body).
    $errFile = [System.IO.Path]::GetTempFileName()
    try {
        $out = & gh @GhArgs 2>$errFile
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            if (-not $Quiet) {
                $err = Get-Content $errFile -Raw -ErrorAction SilentlyContinue
                Write-Warn "gh $($GhArgs -join ' ') exited $exitCode : $err"
            }
            return $null
        }
        return $out
    } finally {
        if (Test-Path $errFile) { Remove-Item $errFile -ErrorAction SilentlyContinue }
    }
}

function Get-FileFromRef {
    <#
    .SYNOPSIS
        Reads a file from the local repo at the given ref. Tries `git show` first (fast,
        offline); falls back to `gh api` if the local ref isn't available.
    #>
    param([string]$Path, [string]$Ref)
    $local = Invoke-Git "show ${Ref}:${Path}"
    if ($local) { return ($local -join "`n") }

    # Strip leading origin/ for gh api ref
    $apiRef = $Ref -replace '^origin/', ''
    $encodedRef = [System.Uri]::EscapeDataString($apiRef)
    $b64 = Invoke-Gh @('api', "repos/$($script:Repo)/contents/$Path`?ref=$encodedRef",
                       '--jq', '.content')
    if (-not $b64) { return $null }
    try {
        return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(($b64 -replace '\s', '')))
    } catch {
        return $null
    }
}

function Get-VersionsPropsState {
    <#
    .SYNOPSIS
        Parses eng/Versions.props at $Ref and returns the version-bump state.
    .DESCRIPTION
        Returns @{ Major; Minor; Patch; PreReleaseVersionLabel; PreReleaseVersionIteration;
                   StabilizePackageVersion; FullVersion } or $null if the file is
        unreadable. FullVersion is "<Major>.<Minor>.<Patch>" — the version that this
        branch's builds would emit.
    #>
    param([string]$Ref)
    $content = Get-FileFromRef -Path 'eng/Versions.props' -Ref $Ref
    if (-not $content) { return $null }

    function _Extract([string]$xml, [string]$tag) {
        if ($xml -match "<$tag(?:\s[^>]*)?>\s*([^<]*)\s*</$tag>") { return $Matches[1].Trim() }
        return $null
    }

    $major = _Extract $content 'MajorVersion'
    $minor = _Extract $content 'MinorVersion'
    $patch = _Extract $content 'PatchVersion'
    if (-not $major -or -not $minor -or $null -eq $patch) { return $null }

    @{
        Major                       = [int]$major
        Minor                       = [int]$minor
        Patch                       = [int]$patch
        PreReleaseVersionLabel      = (_Extract $content 'PreReleaseVersionLabel')
        PreReleaseVersionIteration  = (_Extract $content 'PreReleaseVersionIteration')
        StabilizePackageVersion     = (_Extract $content 'StabilizePackageVersion')
        FullVersion                 = "$major.$minor.$patch"
    }
}

function Get-BugTemplateVersions {
    <#
    .SYNOPSIS
        Reads the version-with-bug dropdown from .github/ISSUE_TEMPLATE/bug-report.yml
        at $Ref. See Get-PreviewReadiness for the matching helper.
    #>
    param([string]$Ref)

    $yaml = Get-FileFromRef -Path '.github/ISSUE_TEMPLATE/bug-report.yml' -Ref $Ref
    if ([string]::IsNullOrWhiteSpace($yaml)) { return @() }

    $lines = $yaml -split "`n"
    $inDropdown = $false
    $inOptions = $false
    $optionsIndent = -1
    $values = New-Object System.Collections.Generic.List[string]

    foreach ($rawLine in $lines) {
        $line = $rawLine.TrimEnd("`r")
        if (-not $inDropdown) {
            if ($line -match '^\s*id:\s*version-with-bug\s*$') { $inDropdown = $true }
            continue
        }
        if (-not $inOptions) {
            if ($line -match '^(\s*)options:\s*$') {
                $inOptions = $true
                $optionsIndent = $Matches[1].Length
            }
            if ($line -match '^\s*-\s*type:\s*') { break }
            continue
        }
        if ($line -match '^(\s*)-\s+(.+?)\s*$') {
            $indent = $Matches[1].Length
            if ($indent -gt $optionsIndent) {
                $value = $Matches[2].Trim().Trim("'").Trim('"')
                if (-not [string]::IsNullOrWhiteSpace($value)) { [void]$values.Add($value) }
                continue
            }
        }
        if ($line -match '^\s*$') { continue }
        if ($line -match '^(\s*)\S' -and $Matches[1].Length -le $optionsIndent) { break }
    }
    return @($values)
}

function Get-ExpectedShipDate {
    <#
    .SYNOPSIS
        Returns the expected ship date for a .NET MAUI release.
    .DESCRIPTION
        Cadence depends on the PatchVersion being shipped:
          - Multiples of 10 (80, 90, 100…) and previews → 2nd Tuesday of a month
            (cross-team .NET convention — used by dotnet/sdk, runtime, MAUI, VS, etc.)
          - Anything else (81, 82, 91…) → ASAP hotfix, no cadence

        Anchoring (which month's 2nd Tuesday?):
          - If `-MainBumpDate` is provided, the anchor is the month immediately
            AFTER main was bumped to this SR's cycle base PatchVersion. This is
            the deterministic mapping the team actually uses: main bumped 70→80
            on 2026-05-13 → SR8 ships 2nd Tuesday of June 2026 = June 9.
          - If no anchor: fall back to "next 2nd Tuesday from today". This is
            only correct when readying the *current* SR before its window;
            after the window passes, the fallback wrongly slides into the next
            SR's slot. Production callers should always pass MainBumpDate.

        Returns [PSCustomObject]@{
            Cadence       = 'second-tuesday' | 'second-tuesday-missed' | 'asap-hotfix'
            Date          = [DateTime] (UTC, 00:00) | $null when ASAP
            DaysFromNow   = [int] | $null  (negative when the window passed)
            FormattedLong = "Tuesday June 9, 2026" | "ASAP (hotfix patch)"
            MissedWindow  = [bool]
            AnchorSource  = 'main-bump' | 'fallback-current-month' | 'fallback-rolled'
            Note          = explanation string suitable for the report header
        }
    .NOTES
        $ReferenceDate is for testability — production callers pass [DateTime]::UtcNow.Date.
        $PatchVersion = $null → assume 2nd-Tuesday cadence (back-compat for callers
        that don't know the patch yet).
    #>
    param(
        [DateTime]$ReferenceDate = [DateTime]::UtcNow.Date,
        [Nullable[int]]$PatchVersion = $null,
        [Nullable[DateTime]]$MainBumpDate = $null
    )
    # Hotfix patch (not a multiple of 10) → no cadence, ship ASAP.
    if ($null -ne $PatchVersion -and ($PatchVersion % 10) -ne 0) {
        return [PSCustomObject]@{
            Cadence       = 'asap-hotfix'
            Date          = $null
            DaysFromNow   = $null
            FormattedLong = 'ASAP (hotfix patch)'
            MissedWindow  = $false
            AnchorSource  = 'asap'
            Note          = "PatchVersion ``$PatchVersion`` is a hotfix on top of an existing release — ships as soon as ready, no 2nd-Tuesday wait."
        }
    }

    # 2nd-Tuesday cadence.
    $today = $ReferenceDate.Date

    function _SecondTuesdayOf {
        param([int]$Year, [int]$Month)
        $first = [DateTime]::new($Year, $Month, 1)
        # DayOfWeek: Sunday=0, Monday=1, Tuesday=2. Offset to reach the first Tuesday.
        $offset = (2 - [int]$first.DayOfWeek + 7) % 7
        $firstTuesday = $first.AddDays($offset)
        return $firstTuesday.AddDays(7)
    }

    $anchorSource = $null
    if ($MainBumpDate) {
        # Anchor on the month AFTER main was bumped to this SR's cycle.
        # Convention: main bumped to N*10 in month M → SR_N ships month (M+1).
        $bumpedMonth = $MainBumpDate.Date.AddMonths(1)
        $candidate = _SecondTuesdayOf -Year $bumpedMonth.Year -Month $bumpedMonth.Month
        $anchorSource = 'main-bump'
    } else {
        # Fallback: "next 2nd Tuesday from today". Only safe BEFORE the window.
        $candidate = _SecondTuesdayOf -Year $today.Year -Month $today.Month
        $anchorSource = 'fallback-current-month'
        if ($candidate -lt $today) {
            $next = $today.AddMonths(1)
            $candidate = _SecondTuesdayOf -Year $next.Year -Month $next.Month
            $anchorSource = 'fallback-rolled'
        }
    }

    $daysFromNow = [int]($candidate - $today).TotalDays
    $missedWindow = ($anchorSource -eq 'main-bump' -and $daysFromNow -lt 0)

    if ($missedWindow) {
        $cadence = 'second-tuesday-missed'
        $note = "Scheduled ship date for this SR was the 2nd Tuesday of $($candidate.ToString('MMMM yyyy')) (anchored on the main-bump for this cycle). That date has passed — coordinate with the release captain on the next valid window."
    } else {
        $cadence = 'second-tuesday'
        $note = '.NET releases ship on the 2nd Tuesday of each month.'
    }

    [PSCustomObject]@{
        Cadence       = $cadence
        Date          = $candidate
        DaysFromNow   = $daysFromNow
        FormattedLong = $candidate.ToString('dddd MMMM d, yyyy')
        MissedWindow  = $missedWindow
        AnchorSource  = $anchorSource
        Note          = $note
    }
}

function Get-MainBumpDateForCycle {
    <#
    .SYNOPSIS
        Finds the date `origin/main` was bumped to a particular PatchVersion.
    .DESCRIPTION
        Walks `git log` for commits on main that ADDED `<PatchVersion>$CycleBase</PatchVersion>`
        in eng/Versions.props, returning the MOST RECENT such commit. The date
        of that commit anchors the SR's ship-date calculation: an SR with
        cycle base N*10 ships the 2nd Tuesday of the month AFTER main bumped
        to N*10.

        Critical caveats `git log -S` does NOT handle:
          1. `-S` matches commits where the count of the substring CHANGED —
             so it matches both the "add 80" commit (70→80) AND the "remove
             80" commit (80→90). We need only the ADD commit.
          2. The same PatchVersion value (e.g. 80) recurs across major-version
             cycles: MAUI 8.x, 9.x and 10.x each had a `<PatchVersion>80</PatchVersion>`
             line at different points in history. If MajorVersion is provided,
             we validate the commit had the matching `<MajorVersion>` value,
             which eliminates the cross-major ambiguity entirely.

        Returns [PSCustomObject]@{ Sha; Date (UTC); Subject } or $null.
    #>
    param(
        [Parameter(Mandatory)][int]$CycleBase,
        [Nullable[int]]$MajorVersion = $null,
        [string]$MainRef = 'origin/main'
    )
    $needle = "<PatchVersion>$CycleBase</PatchVersion>"
    try {
        # Default order is newest-first. Walk candidates and pick the most
        # recent one where the line was ADDED (not removed) AND, if requested,
        # the MajorVersion at that commit matches.
        $shas = git log -S $needle --pretty='%H' $MainRef -- eng/Versions.props 2>$null
        if (-not $shas) { return $null }
        foreach ($s in @($shas)) {
            $sTrim = $s.Trim(); if (-not $sTrim) { continue }

            # Verify the diff ADDED the line (the bump event), not removed it
            # (a subsequent re-bump that took us past this cycle).
            $diff = git show --no-color --format= $sTrim -- eng/Versions.props 2>$null
            $addedNeedle = $false
            foreach ($line in ($diff -split "`r?`n")) {
                if ($line -like "+*" -and $line -notlike "+++*" -and $line -match [regex]::Escape($needle)) {
                    $addedNeedle = $true; break
                }
            }
            if (-not $addedNeedle) { continue }

            # Validate MajorVersion at that commit (eliminates cross-major collisions).
            if ($null -ne $MajorVersion) {
                $content = git show "$($sTrim):eng/Versions.props" 2>$null
                if (-not $content) { continue }
                # `git show` returns an [Object[]] of lines. Join to a single
                # string so the regex match works against the whole file
                # rather than per-line (where the MajorVersion match would
                # never fire because each individual line doesn't contain it).
                if ($content -is [array]) { $content = $content -join "`n" }
                if ($content -notmatch "<MajorVersion>$MajorVersion</MajorVersion>") { continue }
            }

            $line2 = git show -s --format='%cI%x09%s' $sTrim 2>$null
            if (-not $line2) { continue }
            $parts = $line2 -split "`t", 2
            if ($parts.Count -lt 2) { continue }
            # Date is `git log --format=%cI` (committer date, ISO-8601 with
            # 'Z' / offset). Route through ConvertTo-Utc so culture-sensitive
            # [DateTime]::Parse doesn't silently shift the value on hosts
            # whose locale doesn't accept ISO-8601 directly.
            $dateUtc = ConvertTo-Utc -Value $parts[0]
            if (-not $dateUtc) { continue }
            return [PSCustomObject]@{
                Sha     = $sTrim
                Date    = $dateUtc
                Subject = $parts[1]
            }
        }
        return $null
    } catch {
        return $null
    }
}

function New-ReadinessCheck {
    <#
    .SYNOPSIS
        Constructs a readiness-check record used by the Blocking / Cleanup summaries
        at the top of the markdown report.
    .DESCRIPTION
        Status semantics:
          READY    — check passed
          WATCH    — soft signal worth eyeballing; doesn't block ship
          BLOCKED  — must be resolved before ship; escalates verdict to Tier 1 (Not Ready)
          CLEANUP  — known follow-up that doesn't prevent ship (stale milestones,
                     bug-template entries that need to be added soon, etc.). Surfaces
                     in a dedicated "🧹 Cleanup follow-ups" section so it doesn't get
                     lost, but does NOT escalate the overall verdict.
          UNKNOWN  — check couldn't run (missing tool, no data); surfaces as ⚪
    #>
    param(
        [string]$Area,
        [ValidateSet('READY', 'WATCH', 'BLOCKED', 'CLEANUP', 'UNKNOWN')][string]$Status,
        [string]$Details,
        [string]$NextAction
    )
    [PSCustomObject]@{
        Area       = $Area
        Status     = $Status
        Details    = $Details
        NextAction = $NextAction
    }
}

function Get-ReleaseShipChecks {
    <#
    .SYNOPSIS
        Runs the "ready to ship" checks for the SR/candidate report:
          - Versions.props bumped to match the SR cycle (Major.Minor.Patch in [N0..N9])
          - Bug template's version-with-bug dropdown contains the expected SR version

        In CANDIDATE mode the checks still run, but the messaging notes that
        the bumps + template updates happen AFTER the SR is cut, so a BLOCKED
        status in candidate mode is a soft heads-up rather than a hard blocker
        of the candidate itself.
    .OUTPUTS
        Array of check records (see New-ReadinessCheck).
    #>
    param($Ctx)

    $checks = @()
    $isCandidate = ($Ctx.mode -eq 'candidate')

    # Determine the SR number from the SR branch name. In live-SR mode (not
    # candidate), srBranch IS the release branch (release/X.Y.Zxx-srN). In
    # candidate mode, srBranch is main and the prior-SR name lives in
    # priorSrBranch — we want NEXT SR (= prior + 1).
    $srBranchName = if ($isCandidate) { $Ctx.priorSrBranch } else { $Ctx.srBranch }
    $srMatch = [regex]::Match($srBranchName, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
    if (-not $srMatch.Success) {
        $checks += New-ReadinessCheck -Area 'Versions.props bump' -Status 'UNKNOWN' `
            -Details "Could not parse SR number from '$srBranchName'." `
            -NextAction "Verify the branch matches release/X.Y.Zxx-srN."
        return $checks
    }
    $major = [int]$srMatch.Groups[1].Value
    $minor = [int]$srMatch.Groups[2].Value
    $priorSr = [int]$srMatch.Groups[3].Value
    $targetSr = if ($isCandidate) { $priorSr + 1 } else { $priorSr }
    $expectedPatchPrefix = $targetSr * 10   # SR8 → 80, SR9 → 90, SR10 → 100

    # Which ref do we read Versions.props from?
    # Shipped mode: the SR branch itself.
    # Candidate mode: main (which would carry the bump once SR-prior cuts).
    $versionsRef = if ($isCandidate) { "origin/$($Ctx.mainBranch)" } else { $Ctx.srRef }
    $vp = Get-VersionsPropsState -Ref $versionsRef

    if (-not $vp) {
        $checks += New-ReadinessCheck -Area 'Versions.props bump' -Status 'UNKNOWN' `
            -Details "Could not read eng/Versions.props from ``$versionsRef``." `
            -NextAction "Inspect the file manually."
    } else {
        $patchInRange = ($vp.Patch -ge $expectedPatchPrefix -and $vp.Patch -lt ($expectedPatchPrefix + 10))
        $majorMinorMatch = ($vp.Major -eq $major -and $vp.Minor -eq $minor)
        $area = if ($isCandidate) { "Versions.props bump (main → SR$targetSr)" } else { "Versions.props bump (SR$targetSr)" }
        if ($majorMinorMatch -and $patchInRange) {
            $checks += New-ReadinessCheck -Area $area -Status 'READY' `
                -Details "``$versionsRef`` reports ``$($vp.FullVersion)`` — within expected SR$targetSr range [$expectedPatchPrefix..$($expectedPatchPrefix + 9)]." `
                -NextAction "No bump needed."
        } else {
            $candidateHint = if ($isCandidate) {
                " (Expected after SR$priorSr cut: bump main's PatchVersion from $($vp.Patch) to $expectedPatchPrefix.)"
            } else { "" }
            $checks += New-ReadinessCheck -Area $area -Status 'BLOCKED' `
                -Details "``$versionsRef`` reports ``$($vp.FullVersion)``; expected ``$major.$minor.[$expectedPatchPrefix..$($expectedPatchPrefix + 9)]`` for SR$targetSr.$candidateHint" `
                -NextAction "Bump eng/Versions.props (MajorVersion/MinorVersion/PatchVersion) before shipping SR$targetSr."
        }
    }

    # === Servicing-release flip ===
    # When an SR branch is cut from main, two eng/Versions.props values MUST be
    # flipped to switch the branch from "CI build" mode to "stable release" mode:
    #   - PreReleaseVersionLabel:  ci.main  ->  servicing
    #   - StabilizePackageVersion: false    ->  true   (default value, may be unset)
    #
    # Without these flips, the SR branch still produces prerelease packages
    # (`1.2.3-servicing-…` or `1.2.3-ci-…`) — never a stable `1.2.3` package.
    # The build will succeed and CI will be green, so nothing else catches this:
    # the only symptom is that the released NuGet packages never actually become
    # stable. This is exactly the trap the check exists to slam shut.
    #
    # Skip in candidate mode — the candidate IS main, where these values are
    # SUPPOSED to read ci.main / false. The flip happens AFTER the SR is cut.
    if (-not $isCandidate -and $vp) {
        $flipArea = "Versions.props servicing flip (SR$targetSr)"
        $expectedLabel = 'servicing'
        $expectedStabilize = 'true'
        $actualLabel = if ($vp.PreReleaseVersionLabel) { $vp.PreReleaseVersionLabel } else { '<unset>' }
        $actualStabilize = if ($vp.StabilizePackageVersion) { $vp.StabilizePackageVersion } else { '<unset>' }
        $labelOk = ($vp.PreReleaseVersionLabel -eq $expectedLabel)
        $stabilizeOk = ($vp.StabilizePackageVersion -eq $expectedStabilize)
        if ($labelOk -and $stabilizeOk) {
            # Provenance: was the flip done by an SR-direct commit, or just
            # inherited from the previous SR via the catch-up merge?
            #
            # Walk NON-MERGE commits on this SR branch that aren't on the
            # previous SR branch and aren't on main. Look for one that ADDED
            # the `servicing` label line. If none → the flip is inherited via
            # merge from the previous SR (functionally fine, the branch WILL
            # produce stable packages, but worth surfacing so the release
            # captain knows there was no deliberate SR-direct flip PR).
            $prevSrBranch = "release/$major.$minor.1xx-sr$($targetSr - 1)"
            $prevSrRef    = "origin/$prevSrBranch"
            $mainRef      = if ($Ctx -is [hashtable]) {
                if ($Ctx.ContainsKey('mainBranch')) { "origin/$($Ctx['mainBranch'])" } else { 'origin/main' }
            } elseif ($Ctx.PSObject.Properties.Name -contains 'mainBranch') {
                "origin/$($Ctx.mainBranch)"
            } else { 'origin/main' }
            $flipDirectSha = $null
            try {
                $shas = git log --no-merges --pretty='%H' "origin/$($Ctx.srBranch)" "^$prevSrRef" "^$mainRef" -- eng/Versions.props 2>$null
                foreach ($s in @($shas)) {
                    $sTrim = $s.Trim(); if (-not $sTrim) { continue }
                    $diff = git show --no-color --format= $sTrim -- eng/Versions.props 2>$null
                    if ($diff -match '(?m)^\+\s*<PreReleaseVersionLabel>servicing</PreReleaseVersionLabel>') {
                        $flipDirectSha = $sTrim
                        break
                    }
                }
            } catch { }

            if ($flipDirectSha) {
                $shortSha = $flipDirectSha.Substring(0, [Math]::Min(10, $flipDirectSha.Length))
                $details = "``$versionsRef`` has ``PreReleaseVersionLabel=servicing`` and ``StabilizePackageVersion=true`` (set by SR-direct commit ``$shortSha``) — branch is configured to produce stable release packages."
            } else {
                # Find the merge commit on this SR branch that brought in `prevSrBranch`.
                $mergeShaShort = $null
                try {
                    $mergeSha = git log --merges --pretty='%H' --first-parent "origin/$($Ctx.srBranch)" -- eng/Versions.props 2>$null | Select-Object -First 1
                    if ($mergeSha) { $mergeShaShort = $mergeSha.Trim().Substring(0, 10) }
                } catch { }
                $provenance = if ($mergeShaShort) {
                    "inherited from ``$prevSrBranch`` via catch-up merge ``$mergeShaShort``"
                } else {
                    "inherited from ``$prevSrBranch``"
                }
                $details = "``$versionsRef`` has ``PreReleaseVersionLabel=servicing`` and ``StabilizePackageVersion=true`` — branch IS configured to produce stable release packages, but the values were $provenance, not from an SR-direct flip PR (no commit on ``$($Ctx.srBranch)`` alone has set ``PreReleaseVersionLabel=servicing``). Functionally fine; surfaced so the release captain knows the workflow deviated from the previous SR's pattern (e.g., SR$($targetSr-1)'s explicit flip PR)."
            }

            $checks += New-ReadinessCheck -Area $flipArea -Status 'READY' `
                -Details $details `
                -NextAction "No change needed."
        } else {
            $missing = @()
            if (-not $labelOk) { $missing += "``PreReleaseVersionLabel=$actualLabel`` (expected ``servicing``)" }
            if (-not $stabilizeOk) { $missing += "``StabilizePackageVersion=$actualStabilize`` (expected ``true``)" }
            $checks += New-ReadinessCheck -Area $flipArea -Status 'BLOCKED' `
                -Details "``$versionsRef`` is NOT flipped to servicing-release mode: $($missing -join '; '). Without these flips the branch builds prerelease packages and will not ship as a stable .NET release — CI stays green so nothing else catches it." `
                -NextAction "Edit eng/Versions.props on ``$($Ctx.srBranch)``: set ``<PreReleaseVersionLabel>servicing</PreReleaseVersionLabel>`` and ``<StabilizePackageVersion Condition=`"'`$(StabilizePackageVersion)' == ''`">true</StabilizePackageVersion>``. See ``release/$major.$minor.1xx-sr$($targetSr - 1)`` for the canonical diff."
        }
    }

    # === Main bumped to NEXT SR cycle ===
    # Convention: as soon as a release/X.Y.Zxx-srN branch is cut, main MUST bump
    # PatchVersion to (N+1)*10 so any new PRs landing on main during SR$N
    # stabilization correctly target the next SR cycle, not the SR being shipped.
    #
    # If main is still at the same PatchVersion as the SR-to-ship, it's a hard
    # ship-blocker: the moment SR$N tags, every "10.0.80" PR on main suddenly
    # claims to be in a release that already shipped without it.
    #
    # Skip in candidate mode — there, main IS the surveyed ref and the check
    # above already covers the same ground from the other direction.
    if (-not $isCandidate) {
        $mainRef = "origin/$($Ctx.mainBranch)"
        $vpMain = Get-VersionsPropsState -Ref $mainRef
        $nextSr = $targetSr + 1
        $expectedNextPatchPrefix = $nextSr * 10
        $mainArea = "Main bumped to SR$nextSr cycle"

        if (-not $vpMain) {
            $checks += New-ReadinessCheck -Area $mainArea -Status 'UNKNOWN' `
                -Details "Could not read eng/Versions.props from ``$mainRef``." `
                -NextAction "Inspect the file manually."
        } else {
            # If main has moved to a newer major/minor (e.g. GA happened, main is
            # on 11.0 while we ship 10.0 SR8), this check no longer applies — main
            # is past this cycle entirely.
            $mainPastMajor = ($vpMain.Major -gt $major) -or `
                             ($vpMain.Major -eq $major -and $vpMain.Minor -gt $minor)
            $mainBumpedThisCycle = ($vpMain.Major -eq $major -and $vpMain.Minor -eq $minor `
                                    -and $vpMain.Patch -ge $expectedNextPatchPrefix)

            if ($mainPastMajor) {
                $checks += New-ReadinessCheck -Area $mainArea -Status 'READY' `
                    -Details "``$mainRef`` reports ``$($vpMain.FullVersion)`` — main has moved past the $major.$minor train entirely (no bump needed for SR$targetSr stabilization)." `
                    -NextAction "No bump needed."
            } elseif ($mainBumpedThisCycle) {
                $checks += New-ReadinessCheck -Area $mainArea -Status 'READY' `
                    -Details "``$mainRef`` reports ``$($vpMain.FullVersion)`` — main is at or past ``$major.$minor.$expectedNextPatchPrefix`` so PRs merging during SR$targetSr stabilization target SR$nextSr correctly." `
                    -NextAction "No bump needed."
            } else {
                $checks += New-ReadinessCheck -Area $mainArea -Status 'BLOCKED' `
                    -Details "``$mainRef`` reports ``$($vpMain.FullVersion)`` — same cycle as the SR being shipped. Once SR$targetSr tags, every PR currently merging to main as ``$($vpMain.FullVersion)`` would falsely claim to ship in SR$targetSr." `
                    -NextAction "Bump eng/Versions.props on main: set <PatchVersion> from $($vpMain.Patch) to $expectedNextPatchPrefix (SR$nextSr cycle) before shipping SR$targetSr."
            }
        }
    }

    # === Bug template version listing ===
    # Issue templates live on the default branch (main) — they're global per repo.
    $templateRef = "origin/$($Ctx.mainBranch)"
    $templateVersions = Get-BugTemplateVersions -Ref $templateRef
    # Acceptable: any entry matching $major.$minor.<patch-in-SR-range>, with or
    # without an "SR$targetSr" or similar suffix.
    $matchPattern = "^$major\.$minor\.(\d+)"
    $matchingEntries = @($templateVersions | Where-Object {
        if ($_ -match $matchPattern) {
            $p = [int]$Matches[1]
            return ($p -ge $expectedPatchPrefix -and $p -lt ($expectedPatchPrefix + 10))
        }
        return $false
    })

    $bugArea = "Bug template lists SR$targetSr version"
    if ($templateVersions.Count -eq 0) {
        $checks += New-ReadinessCheck -Area $bugArea -Status 'UNKNOWN' `
            -Details "Could not read .github/ISSUE_TEMPLATE/bug-report.yml from ``$templateRef`` or the version-with-bug dropdown is empty." `
            -NextAction "Inspect the bug template manually."
    } elseif ($matchingEntries.Count -gt 0) {
        $first = $matchingEntries[0]
        $checks += New-ReadinessCheck -Area $bugArea -Status 'READY' `
            -Details "Bug template lists ``$first`` (and $($matchingEntries.Count - 1) other SR$targetSr entries)." `
            -NextAction "No template update needed."
    } else {
        $sample = ($templateVersions | Select-Object -First 3) -join ', '
        # CLEANUP, not BLOCKED — missing the dropdown entry doesn't prevent the
        # build from shipping; it just means the bug-report form won't list this
        # version for the first few days. Surface prominently so it gets done,
        # but don't escalate the verdict to Not Ready.
        $checks += New-ReadinessCheck -Area $bugArea -Status 'CLEANUP' `
            -Details "No entry matching ``$major.$minor.[$expectedPatchPrefix..$($expectedPatchPrefix + 9)]`` found in version-with-bug dropdown on ``$templateRef``. Top entries: $sample." `
            -NextAction "Add the SR$targetSr version (e.g. ``$major.$minor.$expectedPatchPrefix``) to .github/ISSUE_TEMPLATE/bug-report.yml — can land before or shortly after ship."
    }

    return $checks
}

# region ──────────────── 0.5 MAESTRO / BAR OPERATIONAL CHECKS ───────────────
#
# These check that the SR branch is wired into Build Asset Registry (BAR) so
# builds auto-flow to consumers. They require the `darc` CLI; in CI environments
# without darc they downgrade to UNKNOWN with verification commands, so the
# report never silently skips them — a release captain reading the issue still
# sees "BAR mapping: UNKNOWN — verify locally with: darc get-default-channels …"
#
# Real-world failure they catch: a new SR branch (e.g. release/10.0.1xx-sr8) is
# cut from main but nobody runs `darc add-default-channel`. CI builds succeed,
# but nothing flows to BAR, so at ship time there's no build to promote. The
# script would otherwise report all-green, hiding the problem.

function Test-DarcAvailable {
    <#
    .SYNOPSIS
        Cached probe for the `darc` CLI. Returns $true if `darc` is on PATH.
    .NOTES
        We deliberately use `Get-Command` instead of `darc --version`. darc itself
        sets a non-zero exit code under certain conditions (auth-not-yet, telemetry
        prompts) even when the executable is fully functional — so a `--version`
        exit-code check produces false negatives on dev boxes. The downstream
        Invoke-DarcJson wrapper handles real auth/network failures by surfacing
        them as `Success = $false`, which the check renders as UNKNOWN.
    #>
    $cached = Get-Variable -Name '_darcAvailable' -Scope Script -ValueOnly -ErrorAction SilentlyContinue
    if ($null -ne $cached) { return $cached }
    $cmd = Get-Command darc -ErrorAction SilentlyContinue
    $script:_darcAvailable = ($null -ne $cmd)
    return $script:_darcAvailable
}

function Invoke-DarcJson {
    <#
    .SYNOPSIS
        Runs `darc <args> --output-format json` and returns a result object that
        unambiguously distinguishes failure from empty-but-successful responses.
    .OUTPUTS
        [PSCustomObject] with Success (bool) and Data (array, never $null when Success).
        Returning a hashtable-style result avoids PowerShell's auto-unwrap of `@()`
        across function boundaries, which would otherwise conflate "darc auth failed"
        with "darc succeeded but returned no items".
    #>
    param([string[]]$DarcArgs)
    try {
        $jsonOutput = & darc @DarcArgs --output-format json 2>$null
        if ($LASTEXITCODE -ne 0) {
            return [PSCustomObject]@{ Success = $false; Data = @() }
        }
        $joined = ($jsonOutput | Out-String)
        if ([string]::IsNullOrWhiteSpace($joined)) {
            return [PSCustomObject]@{ Success = $true; Data = @() }
        }
        $parsed = $joined | ConvertFrom-Json -ErrorAction Stop
        if ($null -eq $parsed) {
            return [PSCustomObject]@{ Success = $true; Data = @() }
        }
        return [PSCustomObject]@{ Success = $true; Data = @($parsed) }
    } catch {
        return [PSCustomObject]@{ Success = $false; Data = @() }
    }
}

function Get-MaestroOperationalChecks {
    <#
    .SYNOPSIS
        Runs Maestro/BAR operational checks for an in-flight SR branch:
          1. SR branch is in BAR default-channel mappings (so builds auto-flow)
          2. BAR has a build for SR HEAD commit (so promotion will have something)

        SKIPPED entirely (returns @()) when:
          - $SkipChecks is set (caller opt-out)
          - $Ctx.mode is 'candidate' (SR branch doesn't exist yet — false positive)
          - SR branch name doesn't match release/X.Y.Zxx-srN (custom shapes, RC, etc.)

        When darc isn't available, emits UNKNOWN checks with verification commands
        instead of silently skipping. This is intentional: the report should always
        document what was NOT checked so the release captain can fill the gap.
    .OUTPUTS
        Array of New-ReadinessCheck records (merged into shipChecks downstream).
    #>
    param($Ctx, [switch]$SkipChecks)

    if ($SkipChecks) { return @() }
    if ($Ctx.mode -eq 'candidate') { return @() }

    # Derive expected channel from SR branch shape. dotnet/maui convention: all
    # SR branches in a major.minor cycle share ONE channel (no per-SR channel).
    # Refusing to compute channel for non-SR shapes avoids posting incorrect
    # add-default-channel commands.
    $branchMatch = [regex]::Match($Ctx.srBranch, '^release/(\d+\.\d+\.\d+xx)-sr\d+$')
    if (-not $branchMatch.Success) { return @() }
    $sdkBand = $branchMatch.Groups[1].Value
    $expectedChannel = ".NET $sdkBand SDK"
    $repoUrl = "https://github.com/$($Ctx.repo)"

    $checks = @()
    $darcReady = Test-DarcAvailable

    # === Check 1: SR branch wired into BAR default-channel mappings ===
    # The critical check. If missing, no SR builds reach BAR — release captain
    # has nothing to promote at ship time.
    $mappingArea = "BAR default-channel mapping ($($Ctx.srBranch) → $expectedChannel)"
    if (-not $darcReady) {
        $checks += New-ReadinessCheck -Area $mappingArea -Status 'UNKNOWN' `
            -Details "``darc`` CLI not available in this environment — cannot query BAR. Verify manually." `
            -NextAction "Locally: ``darc get-default-channels --source-repo $repoUrl`` and search for ``$($Ctx.srBranch)``. If missing, escalate to release engineering: ``darc add-default-channel --channel ""$expectedChannel"" --branch $($Ctx.srBranch) --repo $repoUrl``"
    } else {
        $defaultChannels = Invoke-DarcJson -DarcArgs @('get-default-channels', '--source-repo', $repoUrl)
        if (-not $defaultChannels.Success) {
            $checks += New-ReadinessCheck -Area $mappingArea -Status 'UNKNOWN' `
                -Details "``darc get-default-channels --source-repo $repoUrl`` failed (likely auth, network, or BAR outage)." `
                -NextAction "Run locally and inspect: ``darc get-default-channels --source-repo $repoUrl``"
        } else {
            $srMapping = @($defaultChannels.Data | Where-Object {
                $_.branch -eq $Ctx.srBranch -and $_.enabled
            })
            if ($srMapping.Count -gt 0) {
                $m = $srMapping[0]
                $checks += New-ReadinessCheck -Area $mappingArea -Status 'READY' `
                    -Details "``$($Ctx.srBranch)`` is wired to channel **$($m.channel.name)** (BAR mapping id $($m.id))." `
                    -NextAction "No action needed."
            } else {
                $checks += New-ReadinessCheck -Area $mappingArea -Status 'BLOCKED' `
                    -Details "``$($Ctx.srBranch)`` has NO default-channel mapping in BAR. CI builds on this branch are NOT auto-flowing to **$expectedChannel** — the release captain will have no build to promote when shipping." `
                    -NextAction "Escalate to release engineering: ``darc add-default-channel --channel ""$expectedChannel"" --branch $($Ctx.srBranch) --repo $repoUrl`` (do NOT run unprompted — requires release-eng approval)."
            }
        }
    }

    # === Check 2: BAR has a build for SR HEAD commit ===
    # Secondary signal. If mapping is OK but no build for HEAD: CI is still
    # running OR something blocked publishing. WATCH (not BLOCKED) because
    # transient — re-running the report tomorrow will resolve it.
    if (-not $Ctx.srHeadSha) { return $checks }
    $headShort = $Ctx.srHeadSha.Substring(0, 8)
    $buildArea = "BAR build for SR HEAD ($headShort)"
    if (-not $darcReady) {
        $checks += New-ReadinessCheck -Area $buildArea -Status 'UNKNOWN' `
            -Details "``darc`` CLI not available — cannot verify BAR has a build for SR HEAD." `
            -NextAction "Locally: ``darc get-build --repo $repoUrl --commit $($Ctx.srHeadSha)``"
    } else {
        $builds = Invoke-DarcJson -DarcArgs @('get-build', '--repo', $repoUrl, '--commit', $Ctx.srHeadSha)
        if (-not $builds.Success) {
            $checks += New-ReadinessCheck -Area $buildArea -Status 'UNKNOWN' `
                -Details "``darc get-build`` failed for SR HEAD ``$headShort``." `
                -NextAction "Run locally: ``darc get-build --repo $repoUrl --commit $($Ctx.srHeadSha)``"
        } elseif ($builds.Data.Count -eq 0) {
            $checks += New-ReadinessCheck -Area $buildArea -Status 'WATCH' `
                -Details "No BAR build found for SR HEAD ``$headShort``. May be normal if CI is still running, OR a symptom of the default-channel mapping being absent (see prior check)." `
                -NextAction "Wait for CI to complete on SR HEAD; re-run readiness report. If mapping is also missing (above), fix that first."
        } else {
            # Sort by BAR build id (monotonic, locale-independent) to pick the latest.
            $latest = @($builds.Data | Sort-Object id -Descending)[0]
            $chans = if ($latest.channels) { ($latest.channels -join ', ') } else { '_none_' }
            $buildLink = if ($latest.buildLink) { " ([build $($latest.id)]($($latest.buildLink)))" } else { " (build $($latest.id))" }
            $checks += New-ReadinessCheck -Area $buildArea -Status 'READY' `
                -Details "Build **$($latest.buildNumber)**$buildLink for SR HEAD ``$headShort`` is in BAR; channels: $chans." `
                -NextAction "No action needed."
        }
    }

    return $checks
}

# endregion

# region ───────────────── 0.6 MILESTONE HYGIENE CHECKS ───────────────────────
#
# Ship-readiness checks against the GitHub milestone list:
#   1. Current cycle's milestone exists (e.g. ".NET 10 SR8" must exist if
#      we're shipping SR8). Without it, fixed issues have no milestone to land on.
#   2. Next cycle's milestone exists (e.g. ".NET 10 SR9" or ".NET 11.0-preview6").
#      Without it, unfinished work has nowhere to roll forward when current ships.
#   3. Stale open milestones with past due_on are flagged. After a release ships,
#      its milestone should be closed; lingering open milestones are release hygiene
#      gaps that accumulate misfiled issues and confuse triage.
#
# These checks are evidence-backed and queried via `gh api repos/.../milestones` —
# no auth issues in normal CI; the GH MCP/CI environments always have a token.

function Get-AllMilestones {
    <#
    .SYNOPSIS
        Fetches all milestones (open + closed) for a repo via `gh api`. Returns
        a Success/Data envelope (same pattern as Invoke-DarcJson) so callers can
        distinguish "API call failed" from "no milestones exist".
    .NOTES
        Query parameters MUST be embedded in the URL — passing them via `-f`
        switches `gh api` to POST mode (treats them as form body), which the
        milestones endpoint rejects with HTTP 422.
    #>
    param([string]$Repo)
    try {
        $raw = Invoke-Gh @('api', "repos/$Repo/milestones?state=all&per_page=100", '--paginate')
        # A successful milestones query always returns at least `[]`. Empty/null
        # output means Invoke-Gh swallowed a non-zero gh exit (auth/network), so
        # surface it as a failure rather than masking it as "zero milestones"
        # (which would let milestone-hygiene checks silently pass).
        if (-not $raw) { return [PSCustomObject]@{ Success = $false; Data = @() } }
        $parsed = $raw | ConvertFrom-Json
        return [PSCustomObject]@{ Success = $true; Data = @($parsed) }
    } catch {
        return [PSCustomObject]@{ Success = $false; Data = @() }
    }
}

function Get-MilestoneHygieneChecks {
    <#
    .SYNOPSIS
        Runs three milestone-related ship-readiness checks against the repo's
        GitHub milestone list. SKIPPED entirely (returns @()) when:
          - $SkipChecks is set
          - Branch shape doesn't match an SR or preview release naming convention
            (custom shapes / RC / hotfix branches — can't reliably derive the
            expected milestone title).
        Returns BLOCKED checks when:
          - Current cycle's milestone is missing
          - Next cycle's milestone is missing
          - There are open milestones with past-due due_on dates (excluding the
            current cycle and long-running organizational milestones like Backlog
            and ".NET <major> Planning").
    .OUTPUTS
        Array of New-ReadinessCheck records (merged into shipChecks downstream).
    #>
    param($Ctx, [switch]$SkipChecks)

    if ($SkipChecks) { return @() }

    # In candidate mode we're surveying main as the next SR/preview. The cycle
    # we're prepping is the prior branch's cycle number + 1. In in-flight mode
    # we use srBranch directly.
    $branchToParse = if ($Ctx.mode -eq 'candidate') { $Ctx.priorSrBranch } else { $Ctx.srBranch }
    if (-not $branchToParse) { return @() }

    # Parse SR shape first (release/10.0.1xx-sr8), then preview (release/11.0.1xx-preview5).
    $srMatch = [regex]::Match($branchToParse, '^release/(\d+)\.0\.\d+xx-sr(\d+)$')
    $previewMatch = [regex]::Match($branchToParse, '^release/(\d+)\.0\.\d+xx-preview(\d+)$')

    $expectedTitlesCurrent = @()
    $expectedTitlesNext    = @()
    $cycleLabel = ''

    if ($srMatch.Success) {
        $major = [int]$srMatch.Groups[1].Value
        $cycleNum = [int]$srMatch.Groups[2].Value
        if ($Ctx.mode -eq 'candidate') { $cycleNum++ }
        # MAUI uses both legacy ".NET X.0 SRn" and current ".NET X SRn" forms;
        # treat either as satisfying the check so we don't trigger false BLOCKED
        # on historical milestones.
        $expectedTitlesCurrent = @(".NET $major SR$cycleNum", ".NET $major.0 SR$cycleNum")
        $expectedTitlesNext    = @(".NET $major SR$($cycleNum + 1)", ".NET $major.0 SR$($cycleNum + 1)")
        $cycleLabel = "SR$cycleNum"
    } elseif ($previewMatch.Success) {
        $major = [int]$previewMatch.Groups[1].Value
        $cycleNum = [int]$previewMatch.Groups[2].Value
        if ($Ctx.mode -eq 'candidate') { $cycleNum++ }
        $expectedTitlesCurrent = @(".NET $major.0-preview$cycleNum")
        $expectedTitlesNext    = @(".NET $major.0-preview$($cycleNum + 1)")
        $cycleLabel = "preview$cycleNum"
    } else {
        # Unknown branch shape — can't derive milestone names. Skip silently.
        return @()
    }

    $milestonesResult = Get-AllMilestones -Repo $Ctx.repo
    if (-not $milestonesResult.Success) {
        return @(New-ReadinessCheck -Area "Milestone hygiene" -Status 'UNKNOWN' `
            -Details "Failed to query milestones from GitHub API for ``$($Ctx.repo)``." `
            -NextAction "Re-run with valid 'gh' auth: ``gh auth status`` and ``gh api repos/$($Ctx.repo)/milestones``")
    }

    $allMs = $milestonesResult.Data
    $checks = @()

    # === Check 1: Current cycle's milestone exists ===
    $currentMs = @($allMs | Where-Object { $expectedTitlesCurrent -contains $_.title })
    $currentTitle = $expectedTitlesCurrent[0]
    if ($currentMs.Count -eq 0) {
        $checks += New-ReadinessCheck -Area "Milestone for current cycle ($currentTitle)" -Status 'BLOCKED' `
            -Details "No milestone matching ``$currentTitle`` exists in ``$($Ctx.repo)``. Fixed issues from this cycle have no milestone to land on, and the release notes generator will have nothing to query." `
            -NextAction "Create the milestone: ``gh api repos/$($Ctx.repo)/milestones -f title=""$currentTitle"" -f state=open``"
    }

    # === Check 2: Next cycle's milestone exists ===
    # Surfaced as CLEANUP (not BLOCKED) — a missing roll-forward milestone is a
    # follow-up concern, not a ship blocker. The current cycle can still ship
    # while the next milestone hasn't been created yet; release captain can
    # create it any time before the next cycle starts. In candidate mode this
    # is especially conservative: SR9 candidate would otherwise BLOCK on
    # missing SR10, even though we're not yet ready to cut SR9.
    $nextMs = @($allMs | Where-Object { $expectedTitlesNext -contains $_.title })
    $nextTitle = $expectedTitlesNext[0]
    if ($nextMs.Count -eq 0) {
        $checks += New-ReadinessCheck -Area "Milestone for next cycle ($nextTitle)" -Status 'CLEANUP' `
            -Details "No milestone matching ``$nextTitle`` exists. Once ``$cycleLabel`` ships, open issues will have nowhere to roll forward to — but ``$cycleLabel`` can ship first." `
            -NextAction "Create the milestone before the next cycle begins: ``gh api repos/$($Ctx.repo)/milestones -f title=""$nextTitle"" -f state=open``"
    }

    # === Check 3: Stale open milestones with past due_on ===
    # Filtered by cycle to avoid cross-train noise: when surveying an SR cycle,
    # flag only stale `.NET <same-major> SR*` milestones; when surveying a preview
    # cycle, flag only stale `.NET <same-major>.0-preview*`. A 7-day grace period
    # after due_on lets the actively-shipping release still appear open without
    # triggering BLOCKED.
    # Also excluded:
    #   - the current cycle (still being prepped)
    #   - "Backlog" (intentional long-running)
    #   - ".NET N Planning" (intentional long-running planning ms)
    #   - milestones without due_on (caller has no schedule, no signal)
    $now = (Get-Date).ToUniversalTime()
    $graceCutoff = $now.AddDays(-7)
    $cycleFilter = if ($srMatch.Success) {
        # Match ".NET <major> SR<n>" and ".NET <major>.0 SR<n>" (and SR<n>.<patch>)
        "^\.NET\s+$major(\.0)?\s+SR\d+(\.\d+)?$"
    } else {
        # Match ".NET <major>.0-preview<n>"
        "^\.NET\s+$major\.0-preview\d+$"
    }
    $staleMs = @($allMs | Where-Object {
        $_.state -eq 'open' -and
        $_.due_on -and
        ([datetime]$_.due_on).ToUniversalTime() -lt $graceCutoff -and
        ($expectedTitlesCurrent -notcontains $_.title) -and
        ($_.title -match $cycleFilter)
    } | Sort-Object { [datetime]$_.due_on })

    if ($staleMs.Count -gt 0) {
        $list = ($staleMs | ForEach-Object {
            $dueDate = ([datetime]$_.due_on).ToUniversalTime().ToString('yyyy-MM-dd')
            "[$($_.title)](https://github.com/$($Ctx.repo)/milestone/$($_.number)) (due $dueDate, $($_.open_issues) open)"
        }) -join '; '
        # CLEANUP, not BLOCKED — stale milestones from already-shipped releases are
        # a housekeeping debt (issues need to be rolled forward / closed-as-fixed),
        # but they don't prevent THIS release from shipping. Surface prominently so
        # it gets triaged, but don't escalate the verdict to Not Ready.
        $checks += New-ReadinessCheck -Area "Stale open milestones ($($staleMs.Count))" -Status 'CLEANUP' `
            -Details "$($staleMs.Count) milestone(s) in the .NET $major cycle are past due (>7 days) and still open: $list. These represent already-shipped releases that were never closed out — accumulating open issues that should have been rolled forward." `
            -NextAction "For each: triage the open issues (close-as-fixed, move to current cycle, or move to Backlog), then close the milestone: ``gh api -X PATCH repos/$($Ctx.repo)/milestones/<number> -f state=closed``"
    }

    return $checks
}

function Get-CandidatePrChecks {
    <#
    .SYNOPSIS
        Builds a ship-readiness check for the open "Candidate" PR — the PR
        that promotes a specific main commit as the basis for cutting the
        next SR. Only meaningful in candidate mode: once the SR branch is
        actually cut we switch to in-flight mode and there's no longer a
        "next SR cut" to track.
    .DESCRIPTION
        Convention: the Candidate PR has "Candidate" in the title (word
        boundary, case-insensitive — e.g. "June 8th, Candidate") AND is
        opened by a maintainer (OWNER/MEMBER/COLLABORATOR). The
        authorAssociation gate prevents an unrelated community PR titled
        "Candidate ..." from spoofing the cut PR. It's normally opened
        against ``main`` (not the SR branch), so we scan ALL open PRs on
        main, not just $openSrPrs.

        Status semantics:
          - in-flight mode  → returns @() (no check; SR is already cut)
          - candidate mode, candidate PR open → WATCH (must land before cut)
          - candidate mode, no candidate PR found → WATCH (informational)
          - candidate mode, gh query failed → WATCH (missing signal)

        Never BLOCKED: a missing candidate PR is normal early in the
        cycle. The release captain decides when to open one.
    .OUTPUTS
        Array of New-ReadinessCheck records (merged into shipChecks downstream).
    #>
    param($Ctx, [switch]$SkipChecks)

    if ($SkipChecks) { return ,@() }
    if ($Ctx.mode -ne 'candidate') { return ,@() }

    $repoUrl = "https://github.com/$($Ctx.repo)"

    # Compute next SR label from priorSrBranch (set by Resolve-Context in
    # candidate mode). priorSrBranch = e.g. 'release/10.0.1xx-sr8' → next is SR9.
    $nextSr = $null
    if ($Ctx.priorSrBranch -and $Ctx.priorSrBranch -match 'sr(\d+)$') {
        $nextSr = "SR$([int]$Matches[1] + 1)"
    }

    $area = if ($nextSr) {
        "Candidate PR for next SR cut ($nextSr)"
    } else {
        "Candidate PR for next SR cut"
    }

    # Scan open PRs targeting main (the Candidate PR is opened on main, not
    # on the SR branch, since the SR branch may not exist yet in candidate
    # mode). Cheap: one gh call returning up to 100 open PRs on main.
    # `gh pr list --json` does NOT expose authorAssociation (it's not a valid
    # list projection field). The maintainer spoof-gate below fetches
    # author_association per title-matched candidate via the REST API instead.
    # Keep this projection limited to valid `gh pr list` fields.
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Ctx.repo, '--state', 'open',
                       '--base', $Ctx.mainBranch, '--limit', '100',
                       '--json', 'number,title,author,updatedAt,url')
    if ($null -eq $raw) {
        # gh failed — distinguish from "no Candidate PR found" so the
        # verdict doesn't silently READY on tool failure.
        return ,@(New-ReadinessCheck -Area $area -Status 'WATCH' `
            -Details "Could not query open PRs on ``$($Ctx.mainBranch)`` (``gh pr list`` exited non-zero). Cut readiness cannot be evaluated until the query succeeds." `
            -NextAction "Verify ``gh auth status`` and rerun. If gh is unavailable in this environment, check the Candidate PR manually.")
    }
    $mainPrs = @()
    $parsed = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($parsed) { $mainPrs = @($parsed) }

    # Word-boundary match so "CandidateView" doesn't spoof.
    $titleMatches = @($mainPrs | Where-Object { $_.title -match '(?i)\bcandidate\b' })

    # Author gating: only PRs from a maintainer count. Outside contributors
    # never open SR-cut PRs by convention. `gh pr list` can't return the
    # association, so fetch it per title-matched candidate from the REST API
    # (cheap: titleMatches is almost always 0-1, usually 0 in candidate mode).
    # The REST field is 'author_association' (snake_case) — enum
    # OWNER|MEMBER|COLLABORATOR|CONTRIBUTOR|... Fail closed: an unreadable
    # association excludes the PR, so a missing signal can't let a
    # 'Candidate'-titled PR slip through the spoof gate. Distinguish a
    # *confirmed* non-maintainer (a real spoofer) from an *unverifiable* one
    # (transient gh/REST failure) so a legitimate maintainer Candidate PR isn't
    # mislabeled as a spoofer during an actual cut. Use -Quiet so a transient
    # lookup miss doesn't embed a raw `gh ... exited` warning in the tracker
    # body — the structured WATCH note below carries that signal instead.
    $maintainerAssociations = @('OWNER', 'MEMBER', 'COLLABORATOR')
    $candidates = @()
    $spoofers = 0
    $unverifiable = 0
    foreach ($pr in $titleMatches) {
        $assocRaw = Invoke-Gh @('api', "repos/$($Ctx.repo)/pulls/$($pr.number)",
                                '--jq', '.author_association') -Quiet
        $assoc = if ($assocRaw) { "$assocRaw".Trim() } else { $null }
        if (-not $assoc) {
            $unverifiable++
        } elseif ($maintainerAssociations -contains $assoc) {
            $candidates += $pr
        } else {
            $spoofers++
        }
    }

    if ($candidates.Count -eq 0) {
        $excludeNotes = @()
        if ($spoofers -gt 0) {
            $excludeNotes += "$spoofers non-maintainer PR(s) titled 'Candidate' were excluded as not real cut PRs"
        }
        if ($unverifiable -gt 0) {
            $excludeNotes += "$unverifiable 'Candidate'-titled PR(s) could not have their author association verified (``gh`` REST lookup failed) and were excluded fail-closed — rerun to re-check"
        }
        $rejectNote = if ($excludeNotes.Count -gt 0) { " ($($excludeNotes -join '; '))" } else { '' }
        $nextAction = if ($unverifiable -gt 0) {
            "Verify ``gh auth status`` and rerun to re-check author association. When ready to cut, open a Candidate PR against ``$($Ctx.mainBranch)`` selecting the target main commit for the next SR."
        } else {
            "When ready to cut, open a Candidate PR against ``$($Ctx.mainBranch)`` selecting the target main commit for the next SR."
        }
        return ,@(New-ReadinessCheck -Area $area -Status 'WATCH' `
            -Details "No open PR matching ``*Candidate*`` from a maintainer (OWNER/MEMBER/COLLABORATOR) found on ``$($Ctx.mainBranch)``$rejectNote. The Candidate PR is the mechanism that promotes a specific main commit as the SR cut point." `
            -NextAction $nextAction)
    }

    # Build a compact detail string listing all open candidate PRs (almost
    # always 1, but if multiple are open the release captain should pick).
    $links = ($candidates | ForEach-Object {
        $titleShort = if ($_.title.Length -gt 60) { $_.title.Substring(0, 60) + '...' } else { $_.title }
        "[#$($_.number)]($repoUrl/pull/$($_.number)) — $titleShort"
    }) -join '; '

    # Even on the accepted path, surface any title-matches that were excluded
    # (a confirmed spoofer or an unverifiable lookup) so a transient REST blip on
    # a *second* Candidate-titled PR isn't silently dropped from the captain's view.
    $excludedSuffix = ''
    if ($spoofers -gt 0 -or $unverifiable -gt 0) {
        $parts = @()
        if ($spoofers -gt 0) { $parts += "$spoofers non-maintainer" }
        if ($unverifiable -gt 0) { $parts += "$unverifiable unverifiable (``gh`` REST lookup failed — rerun to re-check)" }
        $excludedSuffix = " Also excluded $($parts -join ' and ') ``*Candidate*``-titled PR(s)."
    }
    $acceptNextAction = if ($unverifiable -gt 0) {
        "Review and merge the Candidate PR when ready; the SR cut follows from its merge commit. Also verify ``gh auth status`` and rerun to re-check the unverifiable Candidate-titled PR(s)."
    } else {
        "Review and merge the Candidate PR when ready; the SR cut follows from its merge commit."
    }
    return ,@(New-ReadinessCheck -Area $area -Status 'WATCH' `
        -Details "$($candidates.Count) open Candidate PR(s) on ``$($Ctx.mainBranch)``: $links. This PR promotes a specific main commit as the SR cut point — it must be merged (and the SR branch cut from it) before the SR cycle starts.$excludedSuffix" `
        -NextAction $acceptNextAction)
}

# endregion

# region ────────────────────── 1. CONTEXT RESOLUTION ──────────────────────

function Resolve-Context {
    param([string]$SrBranch, [string]$Repo, [string]$MainBranch,
          [string[]]$ExcludeBranches, [switch]$NoFetch, [switch]$Candidate,
          [switch]$InheritFromPriorSr)

    if ($InheritFromPriorSr -and -not $Candidate) {
        throw "-InheritFromPriorSr is only valid with -Candidate (it models the SR cut-then-merge workflow)."
    }

    # HARD VALIDATION — refuse inflight/staging refs as SR sources.
    # See $Script:ForbiddenSrPatterns at top of file for the rule rationale.
    foreach ($pat in $Script:ForbiddenSrPatterns) {
        if ($SrBranch -match $pat) {
            throw "REFUSED: '$SrBranch' is not a valid SR branch — SR branches in dotnet/maui cut from `main`, never from inflight/staging/backport refs. Use a `release/X.Y.Zxx-srN` branch, or pass -Candidate to pre-flight `main`."
        }
    }
    foreach ($eb in $ExcludeBranches) {
        $stripped = $eb -replace '^origin/', ''
        foreach ($pat in $Script:ForbiddenSrPatterns) {
            if ($stripped -match $pat) {
                Write-Warn "Exclude branch '$eb' is an inflight/staging ref — dropping. SR contents should only be compared against main or another SR branch."
                $ExcludeBranches = $ExcludeBranches | Where-Object { $_ -ne $eb }
            }
        }
    }

    if (-not $NoFetch) {
        Write-Host "Fetching latest refs..." -ForegroundColor Cyan
        & git fetch --all --quiet 2>$null | Out-Null
    }

    # Candidate mode: swap roles — main becomes the "SR-to-be", named SrBranch
    # becomes the exclude baseline (prior SR). This lets us answer "what would
    # SRn+1 contain if cut today?" without requiring the branch to exist yet.
    # Two modes encoded in the surveyed context:
    #   - 'in-flight' (default): -SrBranch points at an existing release/*-srN branch.
    #     We're surveying its current state for ship-readiness.
    #   - 'candidate': -Candidate is set, so the named SrBranch is actually the
    #     PRIOR SR (used as exclude baseline) and we're simulating "what would the
    #     NEXT SR contain if cut off main today?". Compatible legacy alias: 'shipped'.
    $mode = 'in-flight'
    $effectiveSrRef = "origin/$SrBranch"
    $effectiveExcludes = $ExcludeBranches

    if ($Candidate) {
        $mode = 'candidate'
        $priorSrRef = "origin/$SrBranch"
        $priorSrSha = Invoke-Git "rev-parse $priorSrRef"
        if (-not $priorSrSha) {
            throw "Candidate mode requires -SrBranch to be the prior SR (used as exclude baseline). '$priorSrRef' not found."
        }
        $effectiveSrRef = "origin/$MainBranch"
        # Exclude prior SR from main, so we see only "new since last SR" commits
        $effectiveExcludes = @($priorSrRef)
        Write-Host "Candidate mode: surveying $effectiveSrRef vs prior SR $priorSrRef" -ForegroundColor Cyan
    }

    if ($Candidate -and $InheritFromPriorSr) {
        Write-Host "  -InheritFromPriorSr active: SR-to-be contents will be augmented with $priorSrRef-only commits" -ForegroundColor Cyan
    }

    $srHead = Invoke-Git "rev-parse $effectiveSrRef"
    if (-not $srHead) {
        throw "Branch '$effectiveSrRef' not found. Did you push it? (try without -NoFetch)"
    }
    $srSubject = Invoke-Git "log -1 --format=%s $effectiveSrRef"

    $mainHead = Invoke-Git "rev-parse origin/$MainBranch"
    if (-not $mainHead) { Write-Warn "Main branch 'origin/$MainBranch' not found" }

    # Validate exclude branches exist; drop missing with warning
    $validExcludes = @()
    foreach ($b in $effectiveExcludes) {
        $sha = Invoke-Git "rev-parse $b"
        if ($sha) {
            $validExcludes += $b
        } else {
            Write-Warn "Exclude branch '$b' not found, dropping"
        }
    }

    @{
        repo = $Repo
        srBranch = if ($Candidate) { $MainBranch } else { $SrBranch }
        srRef = $effectiveSrRef
        srHeadSha = $srHead
        srHeadSubject = $srSubject
        mainBranch = $MainBranch
        mainHeadSha = $mainHead
        excludeBranches = $validExcludes
        mode = $mode
        priorSrBranch = if ($Candidate) { $SrBranch } else { $null }
        priorSrRef = if ($Candidate) { "origin/$SrBranch" } else { $null }
        inheritFromPriorSr = [bool]($Candidate -and $InheritFromPriorSr)
        fetchedAt = (Get-Date).ToUniversalTime().ToString('o')
    }
}

# region ────────────────────── 2. SR COMMITS + SOURCE PR EXTRACTION ───────

function Get-RevertedPrFromSubject {
    <#
    .SYNOPSIS
        Extracts the ORIGINAL (reverted) PR number from a revert commit subject.
        Returns $null when the subject carries no reverted-PR reference.
    .NOTES
        GitHub's revert button produces: Revert "Original title (#1234)" (#5678)
        The reverted PR is 1234 (inside the quoted original title). The trailing
        (#5678) is the revert PR's OWN number and must NOT be returned.

        A previous greedy pattern — Revert.*\(#(\d+)\) — captured the LAST (#N),
        i.e. 5678, into $revertsPr. Because that value was truthy, the authoritative
        SHA-lookup fallback was skipped and the real reverted PR (1234) never landed
        in the reverted set, flipping a reverted regression fix to 'in-sr-active'
        (a false-green "ready to ship" verdict for a release whose fix was backed out).
    #>
    param([string]$Subject)
    if (-not $Subject) { return $null }
    # Explicit "Revert PR #NNNN" form.
    $m = [regex]::Match($Subject, '(?i)Revert\s+PR\s+#(\d+)')
    if ($m.Success) { return [int]$m.Groups[1].Value }
    # Standard GitHub revert: the (#N) INSIDE the quoted original title, e.g.
    # Revert "Original title (#1234)" (#5678). Greedy .* anchored to the closing
    # quote captures the original PR (1234): it tolerates internal quotes in the
    # title (the old [^"]* halted at the first inner quote and returned null) and,
    # because the trailing revert PR is NOT followed by a quote, never reaches it.
    # Case-insensitive to also match hand-typed lowercase 'revert "..."' subjects.
    $m = [regex]::Match($Subject, '(?i)Revert\s+".*\(#(\d+)\)"')
    if ($m.Success) { return [int]$m.Groups[1].Value }
    return $null
}

# Internal scanner — extracts source PRs / backports / reverts from commits
# selected by an arbitrary `git log` rev-spec. Used by Get-SrCommits both for
# the primary scan and (optionally) for the inherited-from-prior-SR scan.
function Get-CommitsForRevSpec {
    param(
        [string]$RevSpec,           # e.g. "origin/main ^origin/release/10.0.1xx-sr7"
        [string]$OriginTag = 'primary'
    )

    $shaList = Invoke-Git "log --format=%H $RevSpec"
    if (-not $shaList) {
        return @{
            commits = @(); sourcePrs = @(); backportPrs = @();
            reverts = @(); fixedIssues = @()
        }
    }
    $shas = @($shaList)

    $commits = @()
    $allSourcePrs = New-Object 'System.Collections.Generic.HashSet[int]'
    $allBackportPrs = New-Object 'System.Collections.Generic.HashSet[int]'
    $reverts = @()
    $fixedIssues = New-Object 'System.Collections.Generic.HashSet[int]'

    foreach ($sha in $shas) {
        $raw = Invoke-Git "show --no-patch --format=%H%n%an%n%aI%n%s%n--BODY-START--%n%b $sha"
        if (-not $raw) { continue }
        $lines = @($raw)
        $cmtSha = $lines[0]
        $author = $lines[1]
        $authorDate = $lines[2]
        $subject = $lines[3]
        $bodyStartIdx = [Array]::IndexOf($lines, '--BODY-START--')
        $body = if ($bodyStartIdx -ge 0 -and $bodyStartIdx -lt $lines.Count - 1) {
            ($lines[($bodyStartIdx + 1)..($lines.Count - 1)] -join "`n")
        } else { '' }

        # Backport PR: last "(#NNNN)" in subject
        $backportPr = $null
        $subjMatches = [regex]::Matches($subject, '\(#(\d+)\)')
        if ($subjMatches.Count -gt 0) {
            $backportPr = [int]$subjMatches[$subjMatches.Count - 1].Groups[1].Value
            $allBackportPrs.Add($backportPr) | Out-Null
            $allSourcePrs.Add($backportPr) | Out-Null   # greedy: backport # also resolves
        }

        # Source PR strong signal: "Backport of #NNNN" / "cherry picked from PR #NNNN"
        $sourcePr = $null
        $sourceMatch = [regex]::Match($body, '(?im)(?:backport\s+of|cherry[-\s]picked\s+from(?:\s+PR)?)\s+#(\d+)')
        if ($sourceMatch.Success) {
            $sourcePr = [int]$sourceMatch.Groups[1].Value
            $allSourcePrs.Add($sourcePr) | Out-Null
        }

        # cherry-pick source SHA: "(cherry picked from commit <sha>)"
        $cherrySourceSha = $null
        $cherryShaMatch = [regex]::Match($body, '(?im)cherry\s+picked\s+from\s+commit\s+([0-9a-f]{7,40})')
        if ($cherryShaMatch.Success) { $cherrySourceSha = $cherryShaMatch.Groups[1].Value }

        # Fixed issues
        $issMatches = [regex]::Matches($body, '(?im)(?:fixes|closes|resolves)\s+(?:dotnet/maui#|#)(\d+)')
        $fixesList = @()
        foreach ($m in $issMatches) {
            $n = [int]$m.Groups[1].Value
            $fixesList += $n
            $fixedIssues.Add($n) | Out-Null
        }

        # Revert detection — matches "Revert ", "[Revert]", or "[branch-prefix] Revert ..."
        $isRevert = ($subject -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($subject -match '\[Revert\]')
        $revertsCommit = $null
        $revertsPr = $null
        if ($isRevert) {
            $revM = [regex]::Match($body, '(?im)This reverts commit\s+([0-9a-f]{7,40})')
            if ($revM.Success) { $revertsCommit = $revM.Groups[1].Value }

            # Recover the ORIGINAL (reverted) PR number from the subject. See
            # Get-RevertedPrFromSubject for why the trailing (#N) on a revert
            # subject is the revert's OWN PR and must not be used here.
            $revertsPr = Get-RevertedPrFromSubject -Subject $subject

            # Authoritative override: when we know the reverted commit SHA, read its
            # real subject — its trailing (#NNNN) IS the reverted PR's own number.
            # This is ground truth and overrides any subject-based guess above.
            if ($revertsCommit) {
                $revSubj = Invoke-Git "log -1 --format=%s $revertsCommit"
                if ($revSubj) {
                    $rsM = [regex]::Matches($revSubj, '\(#(\d+)\)')
                    if ($rsM.Count -gt 0) {
                        $revertsPr = [int]$rsM[$rsM.Count - 1].Groups[1].Value
                    }
                }
            }
            $reverts += @{
                revertCommit = $cmtSha
                revertsCommit = $revertsCommit
                revertsPr = $revertsPr
                revertBackportPr = $backportPr
                origin = $OriginTag
            }
        }

        $commits += @{
            sha = $cmtSha
            author = $author
            date = $authorDate
            subject = $subject
            isRevert = $isRevert
            backportPr = $backportPr
            sourcePr = $sourcePr
            cherrySourceSha = $cherrySourceSha
            fixedIssues = $fixesList
            origin = $OriginTag
        }
    }

    @{
        commits = $commits
        sourcePrs = @($allSourcePrs)
        backportPrs = @($allBackportPrs)
        reverts = $reverts
        fixedIssues = @($fixedIssues)
    }
}

function Get-SrCommits {
    param($Ctx)

    Write-Host "Computing SR-only commits..." -ForegroundColor Cyan
    $excludeArgs = $Ctx.excludeBranches | ForEach-Object { "^$_" }
    $primaryRevSpec = "$($Ctx.srRef) $($excludeArgs -join ' ')"
    $primary = Get-CommitsForRevSpec -RevSpec $primaryRevSpec -OriginTag 'primary'
    Write-Host "  Found $($primary.commits.Count) primary SR commits" -ForegroundColor Gray

    $inherited = $null
    if ($Ctx.inheritFromPriorSr -and $Ctx.priorSrRef) {
        # Inheritance set: commits on prior SR that are NOT yet on main.
        # When the SR-to-be (main today) has the prior SR merged in, these are
        # the additional shipping commits.
        Write-Host "Computing prior-SR-only commits ($($Ctx.priorSrRef) not in $($Ctx.srRef))..." -ForegroundColor Cyan
        $inheritRevSpec = "$($Ctx.priorSrRef) ^$($Ctx.srRef)"
        $inherited = Get-CommitsForRevSpec -RevSpec $inheritRevSpec -OriginTag 'inherited'
        Write-Host "  Found $($inherited.commits.Count) inherited-from-prior-SR commits" -ForegroundColor Gray
    }

    # Merge primary + inherited into a single SR-contents view.
    # We keep an `origin` tag on each item so the report can disambiguate.
    $mergedCommits = @($primary.commits)
    $mergedReverts = @($primary.reverts)
    $sourcePrSet = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($n in $primary.sourcePrs) { $sourcePrSet.Add([int]$n) | Out-Null }
    $backportPrSet = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($n in $primary.backportPrs) { $backportPrSet.Add([int]$n) | Out-Null }
    $fixedIssueSet = New-Object 'System.Collections.Generic.HashSet[int]'
    foreach ($n in $primary.fixedIssues) { $fixedIssueSet.Add([int]$n) | Out-Null }

    if ($inherited) {
        $mergedCommits += $inherited.commits
        $mergedReverts += $inherited.reverts
        foreach ($n in $inherited.sourcePrs) { $sourcePrSet.Add([int]$n) | Out-Null }
        foreach ($n in $inherited.backportPrs) { $backportPrSet.Add([int]$n) | Out-Null }
        foreach ($n in $inherited.fixedIssues) { $fixedIssueSet.Add([int]$n) | Out-Null }
    }

    $srcPrsSorted = @($sourcePrSet | Sort-Object)
    $result = @{
        commitCount = $mergedCommits.Count
        primaryCommitCount = $primary.commits.Count
        inheritedCommitCount = if ($inherited) { $inherited.commits.Count } else { 0 }
        commits = $mergedCommits
        sourcePrs = $srcPrsSorted
        sourcePrCount = $srcPrsSorted.Count
        primarySourcePrs = @($primary.sourcePrs | Sort-Object)
        inheritedSourcePrs = if ($inherited) { @($inherited.sourcePrs | Sort-Object) } else { @() }
        backportPrs = @($backportPrSet | Sort-Object)
        fixedIssues = @($fixedIssueSet | Sort-Object)
        reverts = $mergedReverts
    }
    return $result
}

# region ────────────────────── 3. CI STATUS ───────────────────────────────

# Safe property accessor for AzDO API responses under Set-StrictMode -Version Latest.
# AzDO build objects omit 'result' / 'finishTime' until the build is completed,
# and PSObject access throws under strict mode when a property is missing.
function Get-AzdoProp {
    param($Obj, [string]$Name)
    if ($null -eq $Obj) { return $null }
    if (-not ($Obj.PSObject -and $Obj.PSObject.Properties[$Name])) { return $null }
    return $Obj.$Name
}

function Get-PipelineLatestBuilds {
    param($Pipeline, [string]$SrBranch, [string]$SrHead)

    $org = $Pipeline.Org
    $project = $Pipeline.Project
    $defId = $Pipeline.DefinitionId
    $branchSpec = "refs/heads/$SrBranch"

    $url = "https://dev.azure.com/$org/$project/_apis/build/builds?definitions=$defId&branchName=$branchSpec&`$top=5&api-version=7.1"
    try {
        $obj = Invoke-RestMethod -Uri $url -TimeoutSec 30 -ErrorAction Stop
        $builds = Get-AzdoProp $obj 'value'
        if (-not $builds) { return $null }
        return $builds
    } catch {
        Write-Warn "Failed to query pipeline $($Pipeline.Name): $_"
        return $null
    }
}

function Get-CIStatus {
    param($Ctx)

    Write-Host "Querying CI pipelines..." -ForegroundColor Cyan
    $results = @()
    # Internal dnceng/internal pipelines require AzDO auth that the default
    # GitHub Actions runner does NOT have — querying them in public CI mode
    # always 401s and emits a permanent "unknown" tier-2 escalation. Caller
    # must explicitly opt in via -IncludeInternal (e.g. local run with az
    # login or PAT) for these to be queried at all.
    $allPipelines = if ($IncludeInternal) {
        $Script:PublicPipelines + $Script:InternalPipelines
    } else {
        $Script:PublicPipelines
    }

    foreach ($p in $allPipelines) {
        $builds = Get-PipelineLatestBuilds -Pipeline $p -SrBranch $Ctx.srBranch -SrHead $Ctx.srHeadSha
        if (-not $builds) {
            $results += @{
                name = $p.Name; definitionId = $p.DefinitionId
                verdict = 'unknown'; latestBuild = $null
                url = "https://dev.azure.com/$($p.Org)/$($p.Project)/_build?definitionId=$($p.DefinitionId)&branchFilter=$($Ctx.srBranch)"
                note = 'Could not query (auth or outage)'
            }
            continue
        }

        $latest = $builds | Select-Object -First 1
        $sourceSha = Get-AzdoProp $latest 'sourceVersion'
        $status = Get-AzdoProp $latest 'status'
        $result = Get-AzdoProp $latest 'result'
        $finishTime = Get-AzdoProp $latest 'finishTime'
        $links = Get-AzdoProp $latest '_links'
        $buildUrl = if ($links) { Get-AzdoProp (Get-AzdoProp $links 'web') 'href' } else { $null }

        $isAtOrAhead = $false
        if ($sourceSha -and $Ctx.srHeadSha) {
            # Is SR HEAD an ancestor of (or equal to) the build's source SHA?
            $isAtOrAhead = Test-CommitOnBranch -Sha $Ctx.srHeadSha -BranchRef $sourceSha
        }

        $verdict = if (-not $isAtOrAhead) {
            'stale'
        } elseif ($status -in @('inProgress','notStarted')) {
            'running'
        } elseif ($result -eq 'succeeded') {
            'green'
        } elseif ($result -eq 'partiallySucceeded') {
            'red-needs-review'
        } elseif ($result -eq 'failed') {
            'red-needs-review'  # downstream agent classifies known-flakes vs new
        } else {
            'unknown'
        }

        $results += @{
            name = $p.Name; definitionId = $p.DefinitionId
            verdict = $verdict
            latestBuild = @{
                id = Get-AzdoProp $latest 'id'
                buildNumber = Get-AzdoProp $latest 'buildNumber'
                result = $result
                status = $status
                sourceSha = $sourceSha
                isAtOrAheadOfSrHead = $isAtOrAhead
                completedAt = $finishTime
                url = $buildUrl
            }
            recentBuilds = @($builds | Select-Object -First 5 | ForEach-Object {
                @{ id = Get-AzdoProp $_ 'id'; result = Get-AzdoProp $_ 'result'; sourceSha = Get-AzdoProp $_ 'sourceVersion'; completedAt = Get-AzdoProp $_ 'finishTime' }
            })
            url = "https://dev.azure.com/$($p.Org)/$($p.Project)/_build?definitionId=$($p.DefinitionId)&branchFilter=$($Ctx.srBranch)"
        }
    }

    # Overall verdict
    $overall = 'green'
    foreach ($r in $results) {
        if ($r.verdict -eq 'stale') { $overall = 'stale'; break }
        if ($r.verdict -like 'red-*') { $overall = 'red-needs-review' }
        if ($r.verdict -eq 'running' -and $overall -eq 'green') { $overall = 'running' }
        if ($r.verdict -eq 'unknown' -and $overall -eq 'green') { $overall = 'partial-unknown' }
    }

    @{ overall = $overall; pipelines = $results }
}

# region ────────────────────── 4. REGRESSION LABEL INFERENCE ──────────────

function Get-RegressionLabelsAuto {
    param($Ctx)

    # Parse SR version from branch name: release/10.0.1xx-sr7 -> 10.0
    $branchMatch = [regex]::Match($Ctx.srBranch, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
    if (-not $branchMatch.Success) {
        return @{
            mode = 'inferred'; confidence = 'low'
            labels = @(); error = "Branch name doesn't match SR pattern; pass -RegressionLabels explicitly"
        }
    }
    $major = $branchMatch.Groups[1].Value
    $minor = $branchMatch.Groups[2].Value
    $srNum = [int]$branchMatch.Groups[3].Value

    # Query existing labels: regressed-in-{major}.{minor}.*
    $raw = Invoke-Gh @('api', "repos/$($Ctx.repo)/labels", '--paginate', '--jq',
                       ".[] | select(.name | test(`"^regressed-in-$major\\.$minor\\.\\d+$`")) | .name")
    if (-not $raw) {
        return @{ mode = 'inferred'; confidence = 'low'; labels = @();
                  error = "No regressed-in-$major.$minor.* labels found in repo" }
    }
    $allLabels = @($raw) | Sort-Object {
        # Sort by numeric patch
        [int]([regex]::Match($_, '\.(\d+)$').Groups[1].Value)
    } -Descending

    # Heuristic: take top 2 labels — covers the typical SR cycle that aggregates
    # two minor version's worth of fixes
    $picked = @($allLabels | Select-Object -First 2)

    @{
        mode = 'inferred'
        confidence = if ($picked.Count -eq 2) { 'medium' } else { 'low' }
        labels = $picked
        availableLabels = $allLabels
        note = "Inferred from SR$srNum on $major.$minor — VERIFY before treating as authoritative"
    }
}

# region ────────────────────── 5. REGRESSION CANDIDATE ANALYSIS ───────────

function Get-IssueTimelinePrs {
    param($Repo, $IssueNumber)
    $raw = Invoke-Gh @('api', "repos/$Repo/issues/$IssueNumber/timeline", '--paginate')
    if (-not $raw) { return @() }
    $events = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $events) { return @() }
    $prs = @()
    foreach ($e in $events) {
        # Use PSObject.Properties checks because strict mode forbids accessing
        # missing properties on PSCustomObject (timeline events have many shapes).
        if (-not $e.PSObject.Properties['event']) { continue }
        if ($e.event -ne 'cross-referenced') { continue }
        if (-not $e.PSObject.Properties['source']) { continue }
        $src = $e.source
        if (-not $src) { continue }
        if (-not $src.PSObject.Properties['type'] -or $src.type -ne 'issue') { continue }
        if (-not $src.PSObject.Properties['issue']) { continue }
        $iss = $src.issue
        if (-not $iss) { continue }
        # `pull_request` member only exists on issues that are actually PRs
        if (-not $iss.PSObject.Properties['pull_request']) { continue }
        if (-not $iss.pull_request) { continue }
        # Cross-referenced PRs can live in OTHER repositories (forks, or wholly
        # unrelated projects whose own PRs happened to reference this issue).
        # Only same-repo PRs are real fix candidates. A foreign PR number looked
        # up against $Repo either 404s (low numbers below the repo's PR range —
        # surfacing a `gh pr view` warning in the tracker) or, worse, silently
        # matches an unrelated $Repo PR that happens to share the number. Filter
        # to $Repo. The timeline API populates `repository.full_name` for both
        # same-repo and cross-repo references, so this is reliable.
        if (-not $iss.PSObject.Properties['repository']) { continue }
        $issRepo = $iss.repository
        if (-not $issRepo -or -not $issRepo.PSObject.Properties['full_name']) { continue }
        if ($issRepo.full_name -ne $Repo) { continue }
        if (-not $iss.PSObject.Properties['number']) { continue }
        $prs += [int]$iss.number
    }
    return @($prs | Sort-Object -Unique)
}

function Get-PrEvidenceType {
    param($PrBody, $IssueNumber)
    if (-not $PrBody) { return 'none' }
    if ($PrBody -match "(?im)(?:fixes|closes|resolves)\s+(?:dotnet/maui#|#)$IssueNumber\b") {
        return 'closing-keyword'
    }
    if ($PrBody -match '(?im)(?:backport|cherry[-\s]picked)') {
        return 'explicit-backport'
    }
    if ($PrBody -match "#$IssueNumber\b") { return 'mentions-only' }
    return 'none'
}

function Get-PrInfo {
    param($Repo, $PrNumber)
    $json = Invoke-Gh @('pr', 'view', $PrNumber, '--repo', $Repo, '--json',
        'number,title,state,baseRefName,mergedAt,closedAt,body,mergeCommit,author,labels,isDraft,files')
    if (-not $json) { return $null }
    return ($json | ConvertFrom-Json -ErrorAction SilentlyContinue)
}

function Test-PrIsToolingOnly {
    <#
    .SYNOPSIS
        Returns $true when every file changed by the PR lives under .github/
        (or related tooling roots). Such PRs are agent/skill/workflow changes
        that mention regression issues for context but are NOT product fixes.

    .DESCRIPTION
        Guards against the self-reference false-positive: when an agent or
        workflow PR's body says "Fixes #NNNNN" (as documentation context),
        the regression classifier could otherwise mistake it for a real fix.

        Returns $false when:
          - $Files is null/empty (cannot make a decision -> leave alone)
          - ANY file is outside the tooling roots (real product change)
    #>
    param($Files)
    if (-not $Files) { return $false }
    $count = 0
    foreach ($f in $Files) {
        if (-not $f.path) { continue }
        $count++
        # Tooling roots — agent infrastructure, workflows, helper scripts,
        # docs. Product code (src/, tests/, etc.) is intentionally excluded.
        if ($f.path -notmatch '^(\.github/|eng/scripts/|docs/|README|CONTRIBUTING)') {
            return $false
        }
    }
    return ($count -gt 0)
}

function Test-CommitOnBranch {
    param([string]$Sha, [string]$BranchRef)
    if (-not $Sha) { return $false }
    Invoke-Git "merge-base --is-ancestor $Sha $BranchRef" | Out-Null
    return ($LASTEXITCODE -eq 0)
}

function Get-BackportPrsForSr {
    param($Repo, $SrBranch, $SourcePrNumber)
    # Look for any PR targeting the SR branch that mentions the source PR
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Repo, '--base', $SrBranch,
                       '--state', 'all', '--search', "$SourcePrNumber in:title,body",
                       '--json', 'number,title,state,mergedAt,closedAt', '--limit', '20')
    if (-not $raw) { return @() }
    $list = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    return @($list)
}

function Classify-RegressionCandidate {
    param($Issue, $CandidatePrs, $Ctx, $SrContents)

    $sourcePrSet = @{}
    foreach ($n in $SrContents.sourcePrs) { $sourcePrSet[$n] = $true }
    $revertedPrSet = @{}
    foreach ($r in $SrContents.reverts) {
        if ($r.revertsPr) { $revertedPrSet[$r.revertsPr] = $true }
        if ($r.revertBackportPr) { $revertedPrSet[$r.revertBackportPr] = $true }
    }

    # === EARLY-EXIT: issue is already fixed by a commit IN the SR contents ===
    #
    # Bug this guards against: some fixes are opened DIRECTLY against an SR branch
    # (e.g. urgent partner regressions, SR-hotfix PRs like #35768 against
    # release/10.0.1xx-sr7). They have no main-side companion at fix time —
    # any later main PR (e.g. #35803) is just forward-flow, not the original fix.
    #
    # The downstream candidate-PR walk below would happily pick the OPEN main PR
    # and classify as 'open-on-main' ("waiting to merge then backport"), even
    # though the SR already has the fix.
    #
    # $SrContents.fixedIssues is the deterministic ground truth: it's populated
    # from `Fixes #N` / `Closes #N` closing keywords in the bodies of PRs that
    # actually merged into the SR contents (or its inherited prior-SR contents).
    # If the issue is in there, the fix has shipped — period.
    #
    # Defensive: $SrContents shape can be partial in unit-test fixtures (missing
    # .commits / .fixedIssues). Production Get-SrCommits always populates both.
    $hasCommits = if ($SrContents -is [hashtable]) { $SrContents.ContainsKey('commits') }
                  else { $SrContents.PSObject.Properties.Name -contains 'commits' }
    $fixingSrCommits = @()
    if ($hasCommits) {
        $fixingSrCommits = @($SrContents.commits | Where-Object {
            $_.fixedIssues -and ($_.fixedIssues -contains [int]$Issue.number)
        })
    }
    if ($fixingSrCommits.Count -gt 0) {
        # Determine the canonical SR fix PR (prefer the explicit backport/sourcePr;
        # the SR commit always has at least one of those if it was a real PR merge).
        $fixPrs = @()
        foreach ($c in $fixingSrCommits) {
            if ($c.backportPr) { $fixPrs += [int]$c.backportPr }
            elseif ($c.sourcePr) { $fixPrs += [int]$c.sourcePr }
        }
        $fixPrs = @($fixPrs | Sort-Object -Unique)

        # If EVERY fixing PR was reverted on SR, the fix didn't actually ship.
        $unreverted = @($fixPrs | Where-Object { -not $revertedPrSet.ContainsKey($_) })

        if ($unreverted.Count -gt 0) {
            $prList = ($unreverted | ForEach-Object { "#$_" }) -join ', '
            return @{
                classification    = 'in-sr-active'
                confidence        = 'high'
                evidence          = @("SR contents already include a fix for #$($Issue.number) via $prList (closing keyword on merged SR commit)")
                candidateFixPrs   = @($unreverted | ForEach-Object {
                    @{ number = $_; baseRef = 'release/*'; state = 'MERGED'; onMain = $false; evidenceType = 'sr-direct-fix'; backports = @(); title = '' }
                })
                recommendedAction = 'No action — fix is already shipping in this SR'
            }
        } elseif ($fixPrs.Count -gt 0) {
            # Every fix PR we found was reverted — still surface it as reverted
            # so the captain sees the regression isn't actually fixed.
            $prList = ($fixPrs | ForEach-Object { "#$_" }) -join ', '
            return @{
                classification    = 'in-sr-reverted'
                confidence        = 'high'
                evidence          = @("All SR fixes for #$($Issue.number) were reverted on SR: $prList")
                candidateFixPrs   = @()
                recommendedAction = 'Investigate: SR fix was reverted; needs a new fix or revert-of-revert'
            }
        }
        # If we found fixing commits but couldn't extract any PR number,
        # fall through to the candidate-PR walk (best-effort).
    }

    # Filter candidates to those with high evidence for this issue
    $strongPrs = @()
    $sawRevertCandidate = $false
    foreach ($prNum in $CandidatePrs) {
        $info = Get-PrInfo -Repo $Ctx.repo -PrNumber $prNum
        if (-not $info) { continue }
        $ev = Get-PrEvidenceType -PrBody $info.body -IssueNumber $Issue.number
        if ($ev -ne 'closing-keyword' -and $ev -ne 'explicit-backport') { continue }

        # Skip PRs that target SR branches (those are backport PRs themselves — examined separately)
        if ($info.baseRefName -like 'release/*') { continue }

        # False-positive guard: skip PRs whose entire change set lives in
        # tooling roots (.github/, docs/, eng/scripts/, etc). These are
        # agent/skill/workflow PRs that mention regression issue numbers in
        # their body for documentation purposes — they're not real fixes.
        if (Test-PrIsToolingOnly -Files $info.files) {
            Write-Verbose "  Skipping #$prNum — tooling-only PR (mentions #$($Issue.number) in body but changes only .github/, docs/, or eng/scripts/)"
            continue
        }

        # Detect "Revert ..." titled PRs — these are NOT fixes, they're rollbacks.
        # When the only candidate PR is a revert, the issue is likely unfixed (or
        # in a revert-of-revert chain that needs manual verification).
        $isRevertPr = ($info.title -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($info.title -match '\[Revert\]')
        if ($isRevertPr) { $sawRevertCandidate = $true; continue }

        $mergeSha = if ($info.mergeCommit) { $info.mergeCommit.oid } else { $null }
        $onMain = if ($mergeSha) { Test-CommitOnBranch -Sha $mergeSha -BranchRef "origin/$($Ctx.mainBranch)" } else { $false }

        # Look for backport PRs targeting SR
        $backports = Get-BackportPrsForSr -Repo $Ctx.repo -SrBranch $Ctx.srBranch -SourcePrNumber $prNum

        $strongPrs += @{
            number = [int]$info.number
            title = $info.title
            state = $info.state
            baseRef = $info.baseRefName
            mergeSha = $mergeSha
            mergedAt = $info.mergedAt
            evidenceType = $ev
            onMain = $onMain
            backports = @($backports | ForEach-Object {
                @{ number = $_.number; state = $_.state; mergedAt = $_.mergedAt; closedAt = $_.closedAt; title = $_.title }
            })
        }
    }

    if ($strongPrs.Count -eq 0) {
        if ($sawRevertCandidate) {
            return @{
                classification = 'needs-human-review'
                confidence = 'medium'
                evidence = @('All candidate fix PRs were Revert PRs — original fix may be missing or in a revert-of-revert chain. Manual verification required.')
                candidateFixPrs = @()
                recommendedAction = "Inspect the revert chain manually: original fix → revert → (possible) revert-of-revert. Look for the actual fix PR in `gh pr list --search 'fixes #$($Issue.number)'` excluding revert titles."
            }
        }
        return @{
            classification = 'no-fix-yet'
            confidence = 'high'
            evidence = @('no candidate PRs with closing-keyword or explicit-backport evidence')
            candidateFixPrs = @()
            recommendedAction = 'Investigate: no fix PR cross-referenced from issue'
        }
    }

    # Classify each strong PR; aggregate to issue-level verdict
    $perPrVerdicts = @()
    foreach ($pr in $strongPrs) {
        $verdict = $null
        $confidence = 'high'
        $evidence = @()

        # In-SR (with revert check)
        if ($sourcePrSet.ContainsKey($pr.number)) {
            if ($revertedPrSet.ContainsKey($pr.number)) {
                $verdict = 'in-sr-reverted'
                $evidence += "PR #$($pr.number) source-PR in SR but reverted"
            } else {
                $verdict = 'in-sr-active'
                $evidence += "PR #$($pr.number) source-PR in SR contents (active)"
            }
        }
        else {
            # Look at backport PRs targeting SR
            $openBackport = $pr.backports | Where-Object { $_.state -eq 'OPEN' } | Select-Object -First 1
            $closedUnmergedBackport = $pr.backports | Where-Object { $_.state -eq 'CLOSED' -and -not $_.mergedAt } | Select-Object -First 1
            $mergedBackport = $pr.backports | Where-Object { $_.state -eq 'MERGED' } | Select-Object -First 1

            if ($mergedBackport) {
                # backport landed but PR # is different from what we tracked → check sourcePrSet for backport #
                if ($sourcePrSet.ContainsKey([int]$mergedBackport.number)) {
                    if ($revertedPrSet.ContainsKey([int]$mergedBackport.number)) {
                        $verdict = 'in-sr-reverted'
                        $evidence += "Backport PR #$($mergedBackport.number) in SR but reverted"
                    } else {
                        $verdict = 'in-sr-active'
                        $evidence += "Backport PR #$($mergedBackport.number) in SR (active)"
                    }
                } else {
                    $verdict = 'needs-human-review'
                    $confidence = 'low'
                    $evidence += "Backport PR #$($mergedBackport.number) is MERGED in GitHub but not found in SR git contents — re-run without -NoFetch or verify the merge target manually"
                }
            }
            elseif ($openBackport) {
                $verdict = 'backport-in-progress'
                $evidence += "Backport PR #$($openBackport.number) is OPEN against $($Ctx.srBranch)"
            }
            elseif ($closedUnmergedBackport) {
                $verdict = 'rejected-from-sr'
                $evidence += "Backport PR #$($closedUnmergedBackport.number) CLOSED unmerged — needs WorkIQ for context"
            }
            elseif ($pr.state -eq 'MERGED') {
                if ($pr.onMain) {
                    $verdict = 'merged-on-main-no-backport'
                    $confidence = 'medium'
                    $evidence += "PR #$($pr.number) merged to main, no backport PR opened"
                } else {
                    $verdict = 'merged-non-main-only'
                    $confidence = 'medium'
                    $evidence += "PR #$($pr.number) merged but NOT on main (likely inflight-only)"
                }
            }
            elseif ($pr.state -eq 'OPEN') {
                $verdict = 'open-on-main'
                $evidence += "PR #$($pr.number) is OPEN, base=$($pr.baseRef)"
            }
            else {
                $verdict = 'needs-human-review'
                $confidence = 'low'
                $evidence += "PR #$($pr.number) in unexpected state: $($pr.state)"
            }
        }

        $perPrVerdicts += @{ pr = $pr; verdict = $verdict; confidence = $confidence; evidence = $evidence }
    }

    # Pick the highest-priority verdict (in-sr-active > backport-in-progress > ... > no-fix-yet)
    $priority = @{
        'in-sr-active' = 1
        'in-sr-reverted' = 2
        'backport-in-progress' = 3
        'rejected-from-sr' = 4
        'merged-on-main-no-backport' = 5
        'merged-non-main-only' = 6
        'open-on-main' = 7
        'needs-human-review' = 8
        'no-fix-yet' = 9
    }
    $best = $perPrVerdicts | Sort-Object { $priority[$_.verdict] } | Select-Object -First 1

    $recAction = switch ($best.verdict) {
        'in-sr-active' { 'No action — fix is shipping' }
        'in-sr-reverted' { 'Investigate: backport landed and was reverted on SR' }
        'rejected-from-sr' { 'Check rejection rationale (WorkIQ) — was this intentional or stale?' }
        'backport-in-progress' { 'Track backport PR to completion' }
        'merged-on-main-no-backport' { 'Open a backport PR to SR' }
        'merged-non-main-only' { 'Flow fix to main first, then backport to SR' }
        'open-on-main' { 'Wait for main merge, then open backport' }
        'no-fix-yet' { 'No fix exists — investigate priority' }
        default { 'Manual review required' }
    }

    @{
        classification = $best.verdict
        confidence = $best.confidence
        evidence = $best.evidence
        candidateFixPrs = @($strongPrs | ForEach-Object { @{
            number = $_.number; title = $_.title; state = $_.state
            baseRef = $_.baseRef; evidenceType = $_.evidenceType
            onMain = $_.onMain; backports = $_.backports
        }})
        recommendedAction = $recAction
    }
}

function Get-RegressionCandidates {
    param($Ctx, $Labels, $SrContents, [int]$MaxIssues)

    Write-Host "Scanning regression issues for labels: $($Labels -join ', ')" -ForegroundColor Cyan
    $allIssues = @()
    $seen = @{}

    foreach ($label in $Labels) {
        $raw = Invoke-Gh @('issue', 'list', '--repo', $Ctx.repo, '--label', $label,
                           '--state', 'all', '--limit', $MaxIssues.ToString(),
                           '--json', 'number,title,state,stateReason,labels,milestone,createdAt,closedAt')
        if (-not $raw) { continue }
        $list = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
        foreach ($iss in $list) {
            if (-not $seen.ContainsKey($iss.number)) {
                $seen[$iss.number] = $true
                $allIssues += $iss
            }
        }
    }
    Write-Host "  Found $($allIssues.Count) unique regression issues" -ForegroundColor Gray

    # === Future-SR scope guard ===
    # Three deterministic signals identify issues that match the regression
    # label set but actually belong to a DIFFERENT SR (typically the next one):
    #
    #   (1) Versioned label is for a future SR.
    #       e.g. SR8 readiness + `regressed-in-10.0.90` → SR9 candidate.
    #
    #   (2) Milestone explicitly names a different SR.
    #       e.g. SR8 readiness + milestone `.NET 10 SR9` → SR9 candidate.
    #       Triagers set milestones as the canonical "which cycle owns this".
    #
    #   (3) Only label is `regressed-in-inflight/current` AND main has been
    #       bumped past this SR's cycle.
    #       e.g. SR8 readiness + main's PatchVersion = 90 → "inflight/current"
    #       describes content that's now SR9-bound. Reuses the same Versions.props
    #       inspection the "Main bumped to next cycle" ship check uses.
    #
    # In-scope range for SR-N (cycleNum): patch ∈ [cycleNum*10, (cycleNum+1)*10 - 1]
    # (covers SR8 = 80..89, accommodating hotfix patches like 81/82/...)
    $srBranchMatch = [regex]::Match($Ctx.srBranch, '^release/(\d+)\.0\.\d+xx-sr(\d+)$')
    $scopeMajor = $null; $scopeMinPatch = $null; $scopeMaxPatch = $null; $scopeCycleNum = $null
    if ($srBranchMatch.Success) {
        $scopeMajor    = [int]$srBranchMatch.Groups[1].Value
        $scopeCycleNum = [int]$srBranchMatch.Groups[2].Value
        $scopeMinPatch = $scopeCycleNum * 10
        $scopeMaxPatch = ($scopeCycleNum + 1) * 10 - 1
    }

    # Signal (3): probe main's PatchVersion to know which cycle main is on.
    # If main is past this SR's cycle, `regressed-in-inflight/current` no
    # longer points at THIS SR's content.
    $mainIsPastThisSr = $false
    if ($scopeMajor -and $Ctx.mainBranch) {
        try {
            $vpMain = Get-VersionsPropsState -Ref "origin/$($Ctx.mainBranch)"
            if ($vpMain -and $vpMain.Patch -gt $scopeMaxPatch) {
                $mainIsPastThisSr = $true
            }
        } catch {
            # If we can't read main's Versions.props, fall back to label-only logic.
        }
    }

    $results = @()
    $i = 0
    foreach ($iss in $allIssues) {
        $i++
        Write-Host "  [$i/$($allIssues.Count)] Issue #$($iss.number)..." -ForegroundColor DarkGray

        # False-positive guard: issues closed as DUPLICATE are not regressions
        # against this SR — they were rolled up into a canonical issue. Skip
        # the expensive PR walk and flag them so the report can surface them
        # under an "informational" tier instead of "no fix yet".
        $isDuplicate = ($iss.state -eq 'CLOSED') -and ($iss.PSObject.Properties['stateReason']) -and ($iss.stateReason -eq 'DUPLICATE')
        if ($isDuplicate) {
            $results += @{
                issue             = [int]$iss.number
                title             = $iss.title
                state             = $iss.state
                stateReason       = $iss.stateReason
                labels            = @($iss.labels.name)
                milestone         = if ($iss.milestone) { $iss.milestone.title } else { $null }
                createdAt         = $iss.createdAt
                closedAt          = $iss.closedAt
                classification    = 'closed-as-duplicate'
                confidence        = 'high'
                evidence          = @("Issue closed with stateReason=DUPLICATE — rolled up into a canonical regression. Inspect the closing comment for the canonical issue reference.")
                candidateFixPrs   = @()
                recommendedAction = 'Confirm the canonical issue (visible in the close comment) is tracked separately. No action on this issue.'
            }
            continue
        }

        # Future-SR scope check (only when we know this SR's version range)
        if ($scopeMajor) {
            $issueLabels = @($iss.labels.name)
            $issueMilestone = if ($iss.milestone) { $iss.milestone.title } else { $null }

            # --- Signal (1): versioned regression labels (HIGHEST priority) ---
            # A `regressed-in-X.Y.Z` label states a historical fact ("the
            # regression appeared in X.Y.Z"). If the user explicitly named
            # that label in -RegressionLabels (or it's within this SR's patch
            # range), the issue is IN-SCOPE regardless of milestone — the bug
            # is still present in this SR even if triagers plan to ship the
            # fix in a later SR (which would show up as a milestone mismatch).
            $versionedRegressionLabels = @()
            foreach ($lbl in $issueLabels) {
                $vm = [regex]::Match($lbl, '^regressed-in-(\d+)\.0\.(\d+)$')
                if ($vm.Success) {
                    $versionedRegressionLabels += @{
                        label = $lbl
                        major = [int]$vm.Groups[1].Value
                        patch = [int]$vm.Groups[2].Value
                    }
                }
            }
            $anyLabelInScope = $false
            foreach ($vrl in $versionedRegressionLabels) {
                if ($vrl.major -eq $scopeMajor -and $vrl.patch -ge $scopeMinPatch -and $vrl.patch -le $scopeMaxPatch) {
                    $anyLabelInScope = $true; break
                }
                # Allow PRIOR SRs that the user explicitly named in -Labels (carry-over scope)
                if (($vrl.major -lt $scopeMajor) -or
                    ($vrl.major -eq $scopeMajor -and $vrl.patch -lt $scopeMinPatch)) {
                    if ($Labels -contains $vrl.label) { $anyLabelInScope = $true; break }
                }
            }

            # If any versioned label puts the issue in scope, do NOT exclude it.
            # Milestone-mismatch / inflight-bumped signals are subordinate.
            if (-not $anyLabelInScope) {
                $evidence = $null

                # --- Signal (1b): all versioned labels point to a different SR ---
                if ($versionedRegressionLabels.Count -gt 0) {
                    $futureList = ($versionedRegressionLabels | ForEach-Object { $_.label }) -join ', '
                    $evidence = "Versioned label(s) $futureList map to a different SR (this SR covers patches $scopeMinPatch..$scopeMaxPatch)."
                }

                # --- Signal (2): explicit milestone for a different SR ---
                if (-not $evidence -and $issueMilestone) {
                    $mm = [regex]::Match($issueMilestone, '^\.NET\s+(\d+)(?:\.0)?\s+SR(\d+)$')
                    if ($mm.Success) {
                        $milestoneMajor    = [int]$mm.Groups[1].Value
                        $milestoneCycleNum = [int]$mm.Groups[2].Value
                        if ($milestoneMajor -ne $scopeMajor -or $milestoneCycleNum -ne $scopeCycleNum) {
                            $evidence = "Milestone ``$issueMilestone`` is a different SR cycle than this readiness scope (.NET $scopeMajor SR$scopeCycleNum). The triager assigned it to a different SR — treat as out of scope here."
                        }
                    }
                }

                # --- Signal (3): only `regressed-in-inflight/current` AND main has moved past ---
                if (-not $evidence -and $mainIsPastThisSr -and $versionedRegressionLabels.Count -eq 0) {
                    if ($issueLabels -contains 'regressed-in-inflight/current') {
                        $mainPatchStr = if ($vpMain) { $vpMain.Patch } else { '(unknown)' }
                        $evidence = "Only regression label is ``regressed-in-inflight/current``, and ``origin/$($Ctx.mainBranch)`` has been bumped to PatchVersion $mainPatchStr (past this SR's cycle $scopeMinPatch..$scopeMaxPatch). 'inflight' now describes the next SR's content, not this one."
                    }
                }

                if ($evidence) {
                    $results += @{
                        issue             = [int]$iss.number
                        title             = $iss.title
                        state             = $iss.state
                        stateReason       = if ($iss.PSObject.Properties['stateReason']) { $iss.stateReason } else { $null }
                        labels            = $issueLabels
                        milestone         = $issueMilestone
                        createdAt         = $iss.createdAt
                        closedAt          = if ($iss.PSObject.Properties['closedAt']) { $iss.closedAt } else { $null }
                        classification    = 'out-of-scope-future-sr'
                        confidence        = 'high'
                        evidence          = @($evidence)
                        candidateFixPrs   = @()
                        recommendedAction = "Out of scope for this SR. Will be tracked under the relevant SR's readiness."
                    }
                    continue
                }
            }
        }

        $candidatePrs = Get-IssueTimelinePrs -Repo $Ctx.repo -IssueNumber $iss.number
        $classify = Classify-RegressionCandidate -Issue $iss -CandidatePrs $candidatePrs `
                        -Ctx $Ctx -SrContents $SrContents

        $results += @{
            issue = [int]$iss.number
            title = $iss.title
            state = $iss.state
            stateReason = if ($iss.PSObject.Properties['stateReason']) { $iss.stateReason } else { $null }
            labels = @($iss.labels.name)
            milestone = if ($iss.milestone) { $iss.milestone.title } else { $null }
            createdAt = $iss.createdAt
            closedAt = if ($iss.PSObject.Properties['closedAt']) { $iss.closedAt } else { $null }
            classification = $classify.classification
            confidence = $classify.confidence
            evidence = $classify.evidence
            candidateFixPrs = $classify.candidateFixPrs
            recommendedAction = $classify.recommendedAction
        }
    }
    return $results
}

# region ────────────────────── 6. OPEN SR-TARGETING PRs ───────────────────

function Get-OpenSrPrs {
    param($Ctx)
    Write-Host "Listing open PRs targeting $($Ctx.srBranch)..." -ForegroundColor Cyan
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Ctx.repo, '--base', $Ctx.srBranch,
                       '--state', 'open', '--limit', '100',
                       '--json', 'number,title,author,isDraft,createdAt,updatedAt,labels,reviewDecision')
    if (-not $raw) { return @() }
    return @($raw | ConvertFrom-Json -ErrorAction SilentlyContinue)
}

function Test-IsP0Pr {
    <#
    .SYNOPSIS
        True when a PR object carries the release-blocking 'p/0' label.
    .DESCRIPTION
        Ported verbatim from Get-PreviewReadiness.ps1 so the SR lane honors
        p/0-labelled PRs as blockers exactly like the Preview lane. A p/0 label
        deliberately placed on a release-targeting PR is an explicit "must ship"
        signal, so it must surface as a blocker instead of being buried in the
        generic open-PR list. StrictMode-safe: a PR with a missing or null
        `labels` property yields an empty array (-> $false) instead of throwing.
        Accepts both PSCustomObject (the production `gh ... --json` shape) and
        IDictionary/hashtable (the shape test mocks commonly use).
    #>
    param($PR)

    if (-not $PR) { return $false }
    $labels = if ($PR -is [System.Collections.IDictionary]) {
        if ($PR.Contains('labels')) { $PR['labels'] } else { $null }
    } elseif ($PR.PSObject.Properties['labels']) {
        $PR.labels
    } else {
        $null
    }
    if (-not $labels) { return $false }
    return (@($labels | ForEach-Object { $_.name }) -contains 'p/0')
}

function Get-P0PrChecks {
    <#
    .SYNOPSIS
        Builds a single readiness-check record reporting whether any open PR
        targeting the SR branch carries the release-blocking 'p/0' label.
    .DESCRIPTION
        Mirrors the Preview lane's p/0 PR check. A BLOCKED result is auto-hoisted
        into the top-of-issue "🔴 Blocking" table and escalates the verdict to
        Tier 1 (Not Ready) via the shared ship-check machinery — no verdict or
        renderer changes are needed. StrictMode-safe: guards null/empty input.
    .OUTPUTS
        Array with exactly one check record (see New-ReadinessCheck).
    #>
    param($OpenSrPrs, [string]$SrBranch)

    $prs = @($OpenSrPrs)
    $p0 = @($prs | Where-Object { Test-IsP0Pr $_ })

    if ($p0.Count -gt 0) {
        $nums = ($p0 | ForEach-Object { "#$($_.number)" }) -join ', '
        return @(New-ReadinessCheck `
            -Area 'P/0 release-branch PRs' `
            -Status 'BLOCKED' `
            -Details "$($p0.Count) open P/0-labelled PR(s) target ``$SrBranch``: $nums." `
            -NextAction 'Land or de-prioritize each P/0 PR before shipping.')
    }
    return @(New-ReadinessCheck `
        -Area 'P/0 release-branch PRs' `
        -Status 'READY' `
        -Details 'No open P/0-labelled PRs target this SR branch.' `
        -NextAction 'Continue monitoring.')
}

function Get-OpenIssuesByLabel {
    <#
    .SYNOPSIS
        Returns open issues labeled $Label with an error envelope.
    .DESCRIPTION
        Returns @{ QueryFailed=[bool]; Issues=[array] }. Wrapping the
        result distinguishes "no issues found" from "query failed" —
        without it, downstream signal checks emit a false-green READY
        when gh fails (auth expired, rate-limited, network outage) since
        `if (-not $issues)` matches both cases.
    .PARAMETER IncludeBody
        Include the issue body in the result. Kept for historical callers;
        new code doesn't need it because branch filtering now uses the
        label name (Get-CiScanLabelForBranch) instead of body markers.
    #>
    param(
        [string]$Label,
        [switch]$IncludeBody
    )

    $fields = 'number,title,url,labels,createdAt,updatedAt'
    if ($IncludeBody) { $fields += ',body' }

    $raw = Invoke-Gh @('issue', 'list', '--repo', $script:Repo, '--state', 'open',
                       '--limit', '100', '--label', $Label,
                       '--json', $fields)
    if ($null -eq $raw) {
        # Invoke-Gh returns $null only on non-zero exit (failure). A
        # successful but empty result is '[]', a non-null string. Treat
        # this case as "query failed" so callers can downgrade to WATCH.
        return @{ QueryFailed = $true; Issues = @() }
    }
    $issues = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    if (-not $issues) { return @{ QueryFailed = $false; Issues = @() } }
    return @{ QueryFailed = $false; Issues = @($issues) }
}

function Get-CiScanLabelForBranch {
    <#
    .SYNOPSIS
        Maps a branch name to the single `ci-scan*` label its scanner
        workflow writes. Returns $null when no scanner runs against that
        branch.
    .DESCRIPTION
        The CI Failure Scanner has one workflow per scanned branch
        (.github/workflows/ci-status-main.md → 'main' → 'ci-scan';
         .github/workflows/ci-status-net11.md → 'net11.0' → 'ci-scan-net11').
        The label name fully encodes the branch — no need to crack the
        issue body open to figure out where it came from.

        Mapping:
          main                                  → ci-scan
          netN.0                                → ci-scan-netN
          release/N.0.<patch>xx-previewM        → ci-scan-netN   (upstream)
          release/N.0.<patch>xx-srM             → $null          (no scanner)
          anything else                         → $null          (no scanner)

        Preview branches return the parent net<N>.0 label so an in-flight
        preview readiness check still surfaces signals from the branch the
        preview was cut from. SR branches have no continuous scanner, so
        their ci-scan set is correctly empty.

        Add a case here when a new ci-status-*.md workflow is introduced
        (e.g. for a future netN.0 — see .github/workflows/ci-status-*.md).
    #>
    param([string]$Branch)

    if ([string]::IsNullOrWhiteSpace($Branch)) { return $null }
    if ($Branch -eq 'main') { return 'ci-scan' }
    if ($Branch -match '^net(\d+)\.0$') { return "ci-scan-net$($Matches[1])" }
    if ($Branch -match '^release/(\d+)\.0\.\d+xx-preview\d+$') {
        return "ci-scan-net$($Matches[1])"
    }
    return $null
}

function Get-CiScanIssuesForSr {
    <#
    .SYNOPSIS
        Returns open ci-scan issues for the scanner attached to $Branch.
        Returns @{ Matched=[array]; FilteredOut=int; Total=int; QueryFailed=[bool]; ScannerLabel=[string]|$null }.
    .DESCRIPTION
        Uses Get-CiScanLabelForBranch to resolve the single relevant label
        and queries only that one — no more cross-branch dedup or body
        marker parsing. When the branch has no scanner (most SR branches),
        ScannerLabel is $null and Matched is empty.

        QueryFailed flips $true if the underlying `gh issue list` call
        failed (gh missing, auth expired, transient outage). Callers must
        treat that case as "no signal" rather than "no issues" to avoid
        emitting a false-green READY on tool failure.
    #>
    param([string]$Branch)

    $label = Get-CiScanLabelForBranch -Branch $Branch
    if (-not $label) {
        return @{
            Matched      = @()
            FilteredOut  = 0
            Total        = 0
            QueryFailed  = $false
            ScannerLabel = $null
        }
    }

    $result = Get-OpenIssuesByLabel -Label $label -IncludeBody
    if ($result.QueryFailed) {
        return @{
            Matched      = @()
            FilteredOut  = 0
            Total        = 0
            QueryFailed  = $true
            ScannerLabel = $label
        }
    }

    $sorted = @($result.Issues | Sort-Object {
        $u = ConvertTo-Utc -Value $_.createdAt
        if ($u) { $u } else { [DateTime]::MinValue }
    } -Descending)

    return @{
        Matched      = $sorted
        FilteredOut  = 0
        Total        = $sorted.Count
        QueryFailed  = $false
        ScannerLabel = $label
    }
}

function Test-CiScanIsFresh {
    <#
    .SYNOPSIS
        Returns $true if the ci-scan issue was filed within the last $HoursThreshold
        hours (default 24). Used to escalate the ship-check to WATCH.
    #>
    param($Issue, [int]$HoursThreshold = 24)
    if (-not $Issue.PSObject.Properties['createdAt'] -or -not $Issue.createdAt) { return $false }
    $createdUtc = ConvertTo-Utc -Value $Issue.createdAt
    if (-not $createdUtc) { return $false }
    return ((Get-Date).ToUniversalTime() - $createdUtc).TotalHours -lt $HoursThreshold
}

function Get-CiSignalChecks {
    <#
    .SYNOPSIS
        Builds two readiness-check records:
          1. CI Failure Scanner signals (ci-scan label, filtered to $Branch, escalates if any <24h)
          2. Known Build Errors (KBE label, WATCH if any open, READY otherwise)
        Returns @{ Checks = [array]; CiScanIssues = [array]; CiScanFilteredOut = [int]; KbeIssues = [array] }.
    .PARAMETER Branch
        The branch whose ci-scan signals to surface. The scanner-label
        mapping (Get-CiScanLabelForBranch) decides which `ci-scan*` label
        to query; branches without a per-branch scanner (most SR branches)
        emit a 'no scanner' READY entry instead of a confusing 'no signals'.
    #>
    param([string]$Branch)

    Write-Host "Querying ci-scan and Known Build Error issue lists..." -ForegroundColor Cyan
    $ciScanResult = Get-CiScanIssuesForSr -Branch $Branch
    $ciScan = @($ciScanResult.Matched)
    $ciScanFilteredOut = $ciScanResult.FilteredOut
    $ciScanQueryFailed = [bool]$ciScanResult.QueryFailed
    $ciScanLabel = $ciScanResult.ScannerLabel
    $kbeResult = Get-OpenIssuesByLabel -Label 'Known Build Error'
    $kbe = @($kbeResult.Issues)
    $kbeQueryFailed = [bool]$kbeResult.QueryFailed

    $checks = @()

    if ($ciScanQueryFailed) {
        # gh failed (auth/network/rate-limit). Emit WATCH so the verdict
        # acknowledges the missing signal instead of silently READY-ing.
        $checks += New-ReadinessCheck `
            -Area 'CI Failure Scanner signals' `
            -Status 'WATCH' `
            -Details "Could not query ci-scan issues (label ``$ciScanLabel`` — gh exited non-zero). Treating as unknown signal so the verdict reflects the missing data." `
            -NextAction "Verify ``gh auth status`` and rerun. If gh is unavailable in this environment, accept the WATCH and triage ci-scan manually."
    } elseif (-not $ciScanLabel) {
        # No scanner runs against this branch — that's expected for SR
        # branches, which are not continuously scanned. Distinguish this
        # from 'scanner ran and found nothing' so the report is honest.
        $checks += New-ReadinessCheck `
            -Area 'CI Failure Scanner signals' `
            -Status 'READY' `
            -Details "No per-branch CI Failure Scanner is configured for ``$Branch``. Add an entry to Get-CiScanLabelForBranch if a scanner is added later." `
            -NextAction 'No action — SR branches are not continuously scanned.'
    } else {
        $fresh = @($ciScan | Where-Object { Test-CiScanIsFresh -Issue $_ -HoursThreshold 24 })
        if ($fresh.Count -gt 0) {
            $checks += New-ReadinessCheck `
                -Area 'CI Failure Scanner signals' `
                -Status 'WATCH' `
                -Details "$($fresh.Count) ci-scan issue(s) on ``$Branch`` (label ``$ciScanLabel``) filed in the last 24h ($($ciScan.Count) total open). Likely affects this release." `
                -NextAction 'Review the freshest ci-scan issues to confirm none block ship.'
        } elseif ($ciScan.Count -gt 0) {
            $checks += New-ReadinessCheck `
                -Area 'CI Failure Scanner signals' `
                -Status 'WATCH' `
                -Details "$($ciScan.Count) open ci-scan issue(s) on ``$Branch`` (label ``$ciScanLabel``, none filed in the last 24h)." `
                -NextAction 'Skim recent ci-scan issues for impact patterns; mark accepted-known if appropriate.'
        } else {
            $checks += New-ReadinessCheck `
                -Area 'CI Failure Scanner signals' `
                -Status 'READY' `
                -Details "No open ci-scan issues on ``$Branch`` (label ``$ciScanLabel``) — scanner has not flagged recurring CI failures." `
                -NextAction 'Continue monitoring.'
        }
    }

    if ($kbeQueryFailed) {
        $checks += New-ReadinessCheck `
            -Area 'Known Build Errors' `
            -Status 'WATCH' `
            -Details 'Could not query the Known Build Error issue list (gh exited non-zero). Treating as unknown signal so the verdict reflects the missing data.' `
            -NextAction "Verify ``gh auth status`` and rerun. If gh is unavailable, triage Known Build Error issues manually."
    } elseif ($kbe.Count -gt 0) {
        $checks += New-ReadinessCheck `
            -Area 'Known Build Errors' `
            -Status 'WATCH' `
            -Details "$($kbe.Count) open Known Build Error issue(s). May explain background CI noise." `
            -NextAction 'Cross-check against any SR build failures to distinguish accepted-known vs new regressions.'
    } else {
        $checks += New-ReadinessCheck `
            -Area 'Known Build Errors' `
            -Status 'READY' `
            -Details 'No open Known Build Error issues found.' `
            -NextAction 'Continue monitoring.'
    }

    return @{
        Checks            = $checks
        CiScanIssues      = $ciScan
        CiScanFilteredOut = $ciScanFilteredOut
        KbeIssues         = $kbe
    }
}

# region ────────────────────── 7. MARKDOWN REPORT ─────────────────────────

function Get-VerdictTier {
    <#
    .SYNOPSIS
        Maps a regression-issue classification to a deterministic readiness tier.

    .DESCRIPTION
        Tier 1 (🔴 blocking): classifications that PREVENT shipping the SR.
        Tier 2 (🟡 risk):     classifications that REQUIRE human review/decision.
        Tier 3 (🟢 informational): classifications that ARE NOT actionable.

        The mapping is intentionally simple and deterministic — no scoring,
        no judgement calls. If the rules need adjustment, edit this table.
    #>
    param([string]$Classification)
    switch ($Classification) {
        'in-sr-reverted'              { 1; break }
        'no-fix-yet'                  { 1; break }
        'rejected-from-sr'            { 2; break }
        'backport-in-progress'        { 2; break }
        'merged-on-main-no-backport'  { 2; break }
        'merged-non-main-only'        { 2; break }
        'open-on-main'                { 2; break }
        'needs-human-review'          { 2; break }
        'in-sr-active'                { 3; break }
        'closed-as-duplicate'         { 3; break }
        'out-of-scope-future-sr'      { 3; break }
        default                       { 2 }   # unknown → treat as risk
    }
}

function Get-OverallVerdict {
    <#
    .SYNOPSIS
        Computes a deterministic 🔴/🟡/🟢 overall verdict from a readiness report.

    .DESCRIPTION
        Rules (evaluated in order, first match wins):

          🔴 Not Ready when ANY of:
            - One or more regression classifications in Tier 1
              (in-sr-reverted, no-fix-yet for an OPEN regression issue)
          🟡 Conditionally Ready when ANY of:
            - One or more Tier 2 classifications
            - SR CI overall verdict is 'red-needs-review', 'stale',
              'partial-unknown', or 'unknown'  (NOT candidate)

          🟢 Ready otherwise.

        For candidate / pre-flight mode, CI staleness is non-blocking and
        downgraded to advisory (the SR branch doesn't exist yet — staleness
        of main's CI is normal cycle-time noise).

    .OUTPUTS
        Hashtable with fields:
          symbol   = 🔴 / 🟡 / 🟢
          tier     = 1 / 2 / 3
          label    = 'Not Ready' / 'Conditionally Ready' / 'Ready'
          reasons  = string[] explaining each contributing factor
    #>
    param($Data)

    $isCandidate = $false
    if ($Data.metadata.ContainsKey('mode') -and $Data.metadata['mode'] -eq 'candidate') {
        $isCandidate = $true
    }

    $reasons = New-Object System.Collections.Generic.List[string]
    $tier1 = $false
    $tier2 = $false

    # Regression classifications
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        $t1Counts = @{}
        $t2Counts = @{}
        foreach ($r in $Data['regressions']) {
            $tier = Get-VerdictTier -Classification $r.classification
            # `no-fix-yet` only blocks if the issue is still OPEN
            if ($r.classification -eq 'no-fix-yet' -and $r.state -ne 'OPEN') {
                $tier = 3
            }
            if ($tier -eq 1) {
                if (-not $t1Counts.ContainsKey($r.classification)) { $t1Counts[$r.classification] = 0 }
                $t1Counts[$r.classification]++
                $tier1 = $true
            } elseif ($tier -eq 2) {
                if (-not $t2Counts.ContainsKey($r.classification)) { $t2Counts[$r.classification] = 0 }
                $t2Counts[$r.classification]++
                $tier2 = $true
            }
        }
        foreach ($k in $t1Counts.Keys | Sort-Object) {
            $reasons.Add("[Tier 1] $($t1Counts[$k]) × ``$k``") | Out-Null
        }
        foreach ($k in $t2Counts.Keys | Sort-Object) {
            $reasons.Add("[Tier 2] $($t2Counts[$k]) × ``$k``") | Out-Null
        }
    }

    # CI status (skipped for candidate mode — main CI is naturally noisy)
    if (-not $isCandidate -and $Data.ContainsKey('ci') -and $Data['ci']) {
        switch ($Data['ci'].overall) {
            'red-needs-review' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI on SR branch: ``red-needs-review`` — investigate failures before judging") | Out-Null
            }
            'stale' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI on SR branch: ``stale`` — re-run before judging") | Out-Null
            }
            'partial-unknown' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI verdict ``partial-unknown`` — one or more pipeline queries failed") | Out-Null
            }
            'unknown' {
                $tier2 = $true
                $reasons.Add("[Tier 2] CI verdict ``unknown`` — could not query pipeline") | Out-Null
            }
        }
    } elseif ($isCandidate -and $Data.ContainsKey('ci') -and $Data['ci'] -and
              $Data['ci'].overall -in @('red-needs-review', 'stale', 'partial-unknown', 'unknown')) {
        $reasons.Add("[Advisory] Candidate mode — main CI is ``$($Data['ci'].overall)``. Re-evaluate after SR cut.") | Out-Null
    }

    # Ship-readiness checks (versions.props bumped, bug template updated,
    # ci-scan/KBE signals, etc.). Mirrors the worst-wins escalation used by
    # Get-PreviewReadiness:
    #   - BLOCKED → Tier 1 (Not Ready). Must be resolved before ship.
    #   - WATCH   → Tier 2 (Conditionally Ready). Worth eyeballing; doesn't
    #              block but the verdict acknowledges the soft signal.
    # CLEANUP and UNKNOWN do NOT escalate the verdict — they're follow-ups
    # or missing data, not ship signals.
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
        $blockedShipChecks = @($Data['shipChecks'] | Where-Object { $_.Status -eq 'BLOCKED' })
        foreach ($sc in $blockedShipChecks) {
            $tier1 = $true
            $reasons.Add("[Tier 1] Ship check BLOCKED: $($sc.Area)") | Out-Null
        }
        $watchShipChecks = @($Data['shipChecks'] | Where-Object { $_.Status -eq 'WATCH' })
        foreach ($sc in $watchShipChecks) {
            $tier2 = $true
            $reasons.Add("[Tier 2] Ship check WATCH: $($sc.Area)") | Out-Null
        }
    }

    if ($tier1) {
        return @{
            symbol = '🔴'
            tier = 1
            label = 'Not Ready'
            reasons = $reasons.ToArray()
        }
    }
    if ($tier2) {
        return @{
            symbol = '🟡'
            tier = 2
            label = 'Conditionally Ready'
            reasons = $reasons.ToArray()
        }
    }
    return @{
        symbol = '🟢'
        tier = 3
        label = 'Ready'
        reasons = if ($reasons.Count -gt 0) { $reasons.ToArray() } else { @('No blocking or risk-tier signals detected.') }
    }
}

function ConvertTo-LinkedSha {
    <#
    .SYNOPSIS Linkify a commit SHA in markdown using $RepoUrl.
    #>
    param([string]$Sha, [string]$RepoUrl)
    if (-not $Sha) { return '?' }
    $short = if ($Sha.Length -ge 8) { $Sha.Substring(0, 8) } else { $Sha }
    if (-not $RepoUrl) { return "``$short``" }
    return "[``$short``]($RepoUrl/commit/$Sha)"
}

function ConvertTo-LinkedPr {
    <#
    .SYNOPSIS Linkify a PR number in markdown using $RepoUrl.
    #>
    param($PrNumber, [string]$RepoUrl)
    if (-not $PrNumber) { return '—' }
    if (-not $RepoUrl) { return "#$PrNumber" }
    return "[#$PrNumber]($RepoUrl/pull/$PrNumber)"
}

function Format-MarkdownTableCell {
    <#
    .SYNOPSIS
        Sanitize an arbitrary (often upstream-controlled) string for safe use inside a
        single Markdown table cell. Also used for the candidate-PR bulleted list, where
        the newline collapse matters and `\|` renders as `|`.
    .DESCRIPTION
        Three hazards are neutralized so a hostile/malformed issue or PR title cannot
        corrupt the rendered body:
          1. Embedded CR/LF runs are collapsed to a single space, so the value cannot
             split the row across physical lines (observed live: ci-scan issue #35957,
             whose title contained a literal newline).
          2. Each pipe is escaped to `\|`, AND any run of backslashes immediately
             preceding that pipe is doubled FIRST. This ordering is load-bearing:
             GitHub-issue/PR titles may legally contain a literal `\|` (backslash
             immediately followed by a pipe). Escaping only the pipe would turn that
             into `\\|`, which GFM renders as a literal `\` followed by an ACTIVE
             column delimiter `|` (the classic "escape-the-escaper" table breakout).
             Doubling the preceding backslash run first makes `\|` -> `\\\|`, which
             renders as a literal `\|` and cannot open a new column. The doubling is
             SCOPED to pipe-adjacent backslash runs (via the `(\\*)\|` match) rather
             than every backslash in the string, so a title's OTHER backslash escapes
             are preserved verbatim — e.g. an author-escaped `\[link\](url)` or `\*not
             emphasis\*` is NOT de-escaped into active Markdown. Titles with no pipe-
             adjacent backslash (the common case) are unaffected: `a | b` -> `a \| b`.
          3. `<` / `>` are escaped to `&lt;` / `&gt;`, so a title cannot inject raw HTML
             (e.g. an `<!-- ... -->` comment) into the body. This matches the Preview
             engine's Format-MarkdownCell, has zero visual cost (`&lt;T&gt;` renders as
             `<T>`, preserving titles like `List<T>`), and is defense-in-depth: even
             though SR is structurally hash-freeze-immune (it emits its own semantic hash
             at the TOP of the body, so an injected lower `<!-- ...hash... -->` can never
             win the workflow's `head -n1` extraction) and its human-notes markers are
             matched FULL-LINE-ANCHORED (a forged marker only fires if it lands alone on a
             physical line, which hazard 1 already prevents), escaping `<>` keeps SR and
             Preview consistent and removes any reliance on those backend invariants.
        Every SR markdown cell that embeds upstream-controlled text routes through this
        single helper: the ci-scan rows, the Open-PRs / regression / Blocking / Cleanup /
        ship-readiness-checks / Open-Fix-PRs tables, and the candidate-PR list.
    #>
    param([string]$Value)
    if ([string]::IsNullOrEmpty($Value)) { return '' }
    $v = $Value -replace '[\r\n]+', ' '
    # Escape each pipe AND double only the backslash run immediately before it, so a
    # pre-existing `\|` cannot survive as `\\|` (literal `\` + active delimiter). Non-
    # pipe backslash escapes elsewhere in the title are left intact.
    $v = [regex]::Replace($v, '(\\*)\|', { param($m) ($m.Groups[1].Value * 2) + '\|' })
    return ($v -replace '<', '&lt;' -replace '>', '&gt;').Trim()
}

function Format-CiScanIssueRows {
    <#
    .SYNOPSIS
        Builds the rows of the ci-scan section for the SR markdown report.
        Returns the table body as a single string (already terminated with newlines).
        Returns $null if there's nothing to render. Fresh issues (<24h) are
        flagged with 🆕.
    #>
    param([array]$Issues, [string]$RepoUrl, [int]$MaxRows = 15)
    if (-not $Issues -or $Issues.Count -eq 0) { return $null }

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine('| Issue | Title | Filed |')
    [void]$sb.AppendLine('|---|---|---|')
    $rows = $Issues | Select-Object -First $MaxRows
    foreach ($iss in $rows) {
        $marker = ''
        $ageDisplay = '—'
        if ($iss.PSObject.Properties['createdAt'] -and $iss.createdAt) {
            $createdUtc = ConvertTo-Utc -Value $iss.createdAt
            if ($createdUtc) {
                $hoursAgo = ((Get-Date).ToUniversalTime() - $createdUtc).TotalHours
                $ageDisplay = if ($hoursAgo -lt 24) { '{0:N0}h ago' -f $hoursAgo }
                              else                  { '{0:N0}d ago' -f ($hoursAgo / 24) }
                if ($hoursAgo -lt 24) { $marker = '🆕 ' }
            }
        }
        $issLink = "[#$($iss.number)]($RepoUrl/issues/$($iss.number))"
        # Sanitize the upstream ci-scan title for a single Markdown table cell:
        # collapse embedded CR/LF (observed: #35957) and escape pipes. See
        # Format-MarkdownTableCell for the full rationale (and why SR omits `<>`).
        $title = Format-MarkdownTableCell $iss.title
        [void]$sb.AppendLine("| $marker$issLink | $title | $ageDisplay |")
    }
    if ($Issues.Count -gt $MaxRows) {
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("_…and $($Issues.Count - $MaxRows) more. Full list: [open ci-scan issues]($RepoUrl/issues?q=is%3Aopen+is%3Aissue+label%3Aci-scan+sort%3Acreated-desc)._")
    }
    return $sb.ToString()
}

function ConvertTo-Utc {
    <#
    .SYNOPSIS
        Normalizes a value that may be a DateTime (Utc/Local/Unspecified) or a
        string into a UTC DateTime. Returns $null if conversion fails.
    .NOTES
        `ConvertFrom-Json` already parses ISO-8601 'Z' strings into DateTime
        with Kind=Utc. But `[DateTime]::Parse(...)` on a string produces
        Kind=Unspecified, which `.ToUniversalTime()` then misinterprets as
        Local — silently shifting the value by the host's UTC offset. Use
        this helper everywhere age/freshness is computed.
    #>
    param([object]$Value)

    if ($null -eq $Value) { return $null }

    if ($Value -is [DateTime]) {
        if ($Value.Kind -eq [DateTimeKind]::Utc) { return $Value }
        if ($Value.Kind -eq [DateTimeKind]::Local) { return $Value.ToUniversalTime() }
        # Unspecified — assume UTC (gh JSON normally returns 'Z' suffix)
        return [DateTime]::SpecifyKind($Value, [DateTimeKind]::Utc)
    }

    try {
        $dto = [DateTimeOffset]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture)
        return $dto.UtcDateTime
    } catch {
        return $null
    }
}

function Format-GitHubHandle {
    <#
    .SYNOPSIS Render a GitHub login as a code span so it does NOT trigger an @-mention notification.
    .DESCRIPTION
        GitHub treats `@username` in issue/PR bodies as a notification mention. To safely surface
        an author's handle in a report (without spamming them on every nightly run), wrap the
        login in backticks: `` `username` `` is rendered as a code span and is NOT interpreted as a mention.
        Handles bot/app refs (e.g. ``app/dotnet-maestro``) as well.
    .PARAMETER Login
        The raw GitHub login (with or without a leading ``@``). May be ``$null`` / empty.
    .PARAMETER Fallback
        Text to return when Login is null/empty. Defaults to ``unknown``.
    #>
    param(
        [Parameter(Mandatory = $false)][AllowNull()][AllowEmptyString()][string]$Login,
        [string]$Fallback = 'unknown'
    )
    if ([string]::IsNullOrWhiteSpace($Login)) { return $Fallback }
    $clean = $Login.TrimStart('@').Trim()
    if ([string]::IsNullOrWhiteSpace($clean)) { return $Fallback }
    return "``$clean``"
}

function Get-ReportSemanticHash {
    <#
    .SYNOPSIS
        Produces a stable SHA-256 hash of the report's semantic content.

    .DESCRIPTION
        The hash captures fields that change ONLY when the report's verdict
        or contents would meaningfully differ — used by the workflow to skip
        re-posting unchanged trackers (idempotency).

        DELIBERATELY EXCLUDED: fetchedAt timestamp, CI duration, "X minutes
        ago" relative times, and any other field that drifts on every run.
    #>
    param($Data, $Verdict)

    # MUST be [ordered]: a plain [hashtable] enumerates keys in an order derived
    # from per-process String.GetHashCode(), which .NET Core randomizes on every
    # process start. ConvertTo-Json would then emit keys in a different order each
    # run, producing a DIFFERENT hash for identical content — silently defeating
    # the workflow's idempotent no-op (which compares a hash written by an earlier
    # process against one computed now). Insertion order keeps the hash stable.
    $semantic = [ordered]@{
        verdict = $Verdict.symbol
        srHead = $Data.metadata.srHeadSha
        ciOverall = if ($Data.ContainsKey('ci') -and $Data['ci']) { $Data['ci'].overall } else { $null }
        srPrs = if ($Data.ContainsKey('srContents') -and $Data['srContents']) {
                    @($Data['srContents'].sourcePrs | Sort-Object) -join ','
                } else { '' }
        regressions = if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
                          @($Data['regressions'] | Sort-Object issue | ForEach-Object {
                              # `no-fix-yet` is the ONLY classification whose rendered tier
                              # depends on issue state (OPEN -> Tier 1, CLOSED -> Tier 3; see
                              # Format-MarkdownReport's $emitTier). Fold the state-derived tier
                              # bit into the hash for THAT class only, so a no-fix-yet issue
                              # closing (which moves its row T1 -> T3) flips the hash and
                              # refreshes the tracker — even when another blocker keeps the
                              # verdict symbol unchanged. Every other classification stays
                              # state-insensitive, so unrelated state transitions (e.g. a
                              # Tier-3 in-sr-active issue closing) do NOT churn the hash or
                              # spam issue watchers — preserving the conservative design above.
                              if ($_.classification -eq 'no-fix-yet') {
                                  $nfyTier = if ($_.state -eq 'OPEN') { 't1' } else { 't3' }
                                  "$($_.issue):$($_.classification):$nfyTier"
                              } else {
                                  "$($_.issue):$($_.classification)"
                              }
                          }) -join '|'
                      } else { '' }
        openSrPrs = if ($Data.ContainsKey('openSrPrs') -and $Data['openSrPrs']) {
                        @($Data['openSrPrs'] | Sort-Object number | ForEach-Object { $_.number }) -join ','
                    } else { '' }
        shipChecks = if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
                         @($Data['shipChecks'] | Sort-Object Area | ForEach-Object {
                             "$($_.Area):$($_.Status)"
                         }) -join '|'
                     } else { '' }
        # Nightly dogfood feed banner state. Folded in so a feed going stale (or a
        # fresh build landing) refreshes the tracker even on an otherwise-quiet branch
        # — the banner is the whole point of the feature and must not be frozen out by
        # the idempotent no-op. We hash the non-drifting tier + resolved version (NOT the
        # "N days" count) so threshold crossings and new builds flip the hash but a daily
        # day-count tick within the same tier does not (no watcher spam). Fail-open: if the
        # NightlyFeed helper isn't loaded, contributes '' (hash behaves as before).
        nightlyFeed = if ($Data.ContainsKey('nightlyFeed') -and $Data['nightlyFeed'] -and
                          (Get-Command Get-NightlyFeedTier -ErrorAction SilentlyContinue)) {
                          $nf = $Data['nightlyFeed']
                          # Reuse the SAME instant the banner was rendered with (stored by
                          # Add-SrNightlyFeedFreshness) so the hashed tier can never disagree with
                          # the displayed banner tier and freeze a stale banner via the no-op gate.
                          # Fall back to UtcNow when unset (e.g. unit tests that inject nightlyFeed directly).
                          $nfNow = if ($Data.ContainsKey('nightlyFeedNow') -and $Data['nightlyFeedNow']) {
                                       [datetime]$Data['nightlyFeedNow']
                                   } else { [datetime]::UtcNow }
                          $tier = Get-NightlyFeedTier -Freshness $nf -Now $nfNow
                          $ver = [string](Get-NightlyFeedProp $nf 'version')
                          if ($ver) { "$tier|$ver" } else { $tier }
                      } else { '' }
    }

    $json = $semantic | ConvertTo-Json -Depth 5 -Compress
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $hash = $sha.ComputeHash($bytes)
        return ([System.BitConverter]::ToString($hash) -replace '-', '').ToLowerInvariant()
    } finally {
        $sha.Dispose()
    }
}

function Format-MarkdownReport {
    param($Data, [string]$RepoUrl, [string]$TrackerKey, [int]$MaxBodyBytes = 60000)

    $ctx = $Data.metadata
    $srBranch = $ctx.srBranch
    $shortHead = if ($ctx.srHeadSha) { $ctx.srHeadSha.Substring(0, 8) } else { '?' }

    # Compute verdict + semantic hash (deterministic, used in markers)
    $verdict = Get-OverallVerdict -Data $Data
    $semanticHash = Get-ReportSemanticHash -Data $Data -Verdict $verdict

    $sb = [System.Text.StringBuilder]::new()

    # === HEADER + MARKERS ===
    # Markers go FIRST so a workflow scanning for them can short-circuit
    # without parsing the body.
    if ($TrackerKey) {
        [void]$sb.AppendLine("<!-- release-readiness-tracker: $TrackerKey -->")
    }
    [void]$sb.AppendLine("<!-- release-readiness-hash: sha=$semanticHash -->")

    $mode = if ($ctx.ContainsKey('mode')) { $ctx['mode'] } else { 'in-flight' }
    $inherits = ($ctx.ContainsKey('inheritFromPriorSr') -and $ctx['inheritFromPriorSr'])
    if ($mode -eq 'candidate') {
        if ($inherits) {
            [void]$sb.AppendLine("# Release Readiness — CANDIDATE for next SR (main + inherited from $($ctx.priorSrBranch))")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("> 🛫 **Pre-flight mode (cut-then-merge).** Surveying ``$srBranch`` (== main) PLUS commits inherited from prior SR ``$($ctx.priorSrBranch)`` (the SR will be cut from main, then have the prior SR merged into it).")
        } else {
            [void]$sb.AppendLine("# Release Readiness — CANDIDATE for next SR (vs $($ctx.priorSrBranch))")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("> 🛫 **Pre-flight mode.** Surveying ``$srBranch`` (== main) against prior SR ``$($ctx.priorSrBranch)``. Shows what WOULD ship if we cut the next SR today.")
        }
    } else {
        [void]$sb.AppendLine("# Release Readiness — $srBranch")
    }
    [void]$sb.AppendLine()

    # === VERDICT (always second, always visible) ===
    [void]$sb.AppendLine("## Verdict — $($verdict.symbol) **$($verdict.label)**")
    [void]$sb.AppendLine()
    foreach ($r in $verdict.reasons) {
        [void]$sb.AppendLine("- $r")
    }
    [void]$sb.AppendLine()

    # Tracker + provenance line (visible, complements the HTML comment marker)
    if ($TrackerKey) {
        [void]$sb.AppendLine("**Tracker:** ``$TrackerKey`` · mode=``$mode`` · branch=``$srBranch``")
    }
    $shaLinked = ConvertTo-LinkedSha -Sha $ctx.srHeadSha -RepoUrl $RepoUrl
    [void]$sb.AppendLine("**HEAD**: $shaLinked — $($ctx.srHeadSubject)")
    [void]$sb.AppendLine("**Generated**: $($ctx.fetchedAt)")
    # Nightly dogfood feed freshness — surfaces when the feed testers point at has gone
    # stale (no new build), so a captain sees at a glance whether dogfood feedback is being
    # collected against current bits. The banner string is rendered upstream in Invoke-Main
    # (where "now" is natural), keeping this renderer clock-free and deterministic. Absent in
    # phase-scoped runs / when the helper isn't loaded → nothing is appended.
    if ($Data.ContainsKey('nightlyFeedBanner') -and $Data['nightlyFeedBanner']) {
        [void]$sb.AppendLine()
        [void]$sb.AppendLine($Data['nightlyFeedBanner'])
    }
    # Expected ship date — cadence depends on PatchVersion:
    #   - x0 patches (80, 90…) + previews → 2nd Tuesday of the month
    #   - hotfix patches (81, 82…)        → ASAP, no cadence
    # Read patch from the survey ref's Versions.props. In candidate mode srRef
    # is main (so we'd see e.g. 90 for upcoming SR9, still 2nd-Tuesday cadence).
    # Defensive: $ctx may be a hashtable, PSCustomObject, or test fixture with
    # no srRef at all — fall back to 2nd-Tuesday cadence in that case.
    $patchForShipDate = $null
    $srRefForShipDate = if ($ctx -is [hashtable]) {
        if ($ctx.ContainsKey('srRef')) { $ctx['srRef'] } else { $null }
    } elseif ($ctx.PSObject.Properties.Name -contains 'srRef') {
        $ctx.srRef
    } else { $null }
    if ($srRefForShipDate) {
        $vpForShipDate = Get-VersionsPropsState -Ref $srRefForShipDate
        if ($vpForShipDate) { $patchForShipDate = [int]$vpForShipDate.Patch }
    }
    # Anchor on main-bump date for this SR's cycle, so the date doesn't slide
    # into the next SR's window once this SR's calendar month passes.
    $mainBumpDateForShip = $null
    if ($null -ne $patchForShipDate) {
        $cycleBaseForShip = [int]([Math]::Floor($patchForShipDate / 10) * 10)
        $majorForShip = $null
        if ($vpForShipDate -and $vpForShipDate.Major) { $majorForShip = [int]$vpForShipDate.Major }
        $bumpInfoForShip = if ($null -ne $majorForShip) {
            Get-MainBumpDateForCycle -CycleBase $cycleBaseForShip -MajorVersion $majorForShip
        } else {
            Get-MainBumpDateForCycle -CycleBase $cycleBaseForShip
        }
        if ($bumpInfoForShip) { $mainBumpDateForShip = $bumpInfoForShip.Date }
    }
    $shipDate = Get-ExpectedShipDate -PatchVersion $patchForShipDate -MainBumpDate $mainBumpDateForShip
    if ($shipDate.Cadence -eq 'asap-hotfix') {
        [void]$sb.AppendLine("**Expected ship date**: 🚑 $($shipDate.FormattedLong) — $($shipDate.Note)")
    } elseif ($shipDate.MissedWindow) {
        [void]$sb.AppendLine("**Expected ship date**: ⚠️ $($shipDate.FormattedLong) — **window passed** ($([Math]::Abs($shipDate.DaysFromNow)) day(s) ago). $($shipDate.Note)")
    } else {
        $whenSuffix = if ($shipDate.DaysFromNow -eq 0) {
            '🚨 **shipping today**'
        } elseif ($shipDate.DaysFromNow -eq 1) {
            '⚠️ tomorrow'
        } else {
            "in $($shipDate.DaysFromNow) days"
        }
        [void]$sb.AppendLine("**Expected ship date**: $($shipDate.FormattedLong) — $whenSuffix ($($shipDate.Note))")
    }
    [void]$sb.AppendLine("**Regression labels**: $($ctx.regressionLabels -join ', ') _(mode: $($ctx.labelInferenceMode))_")
    [void]$sb.AppendLine()

    if ($Data.ContainsKey('warnings') -and $Data['warnings'].Count -gt 0) {
        [void]$sb.AppendLine("> ⚠️ **Warnings:**")
        foreach ($w in $Data['warnings']) { [void]$sb.AppendLine("> - $w") }
        [void]$sb.AppendLine()
    }

    # === BLOCKING SUMMARY (hoisted to top, right under the verdict) ===
    # Surface every BLOCKED ship-check AND every Tier 1 regression so the
    # release captain sees what's preventing ship without scrolling past
    # CI tables, open-PR tables, and the full tier breakdown below.
    $blockingItems = New-Object System.Collections.Generic.List[hashtable]
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
        foreach ($sc in $Data['shipChecks']) {
            if ($sc.Status -eq 'BLOCKED') {
                [void]$blockingItems.Add(@{
                    area = "🛠️ $($sc.Area)"
                    details = $sc.Details
                    action = $sc.NextAction
                })
            }
        }
    }
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        foreach ($r in $Data['regressions']) {
            $tier = Get-VerdictTier -Classification $r.classification
            if ($r.classification -eq 'no-fix-yet' -and $r.state -ne 'OPEN') { $tier = 3 }
            if ($tier -eq 1) {
                $issLink = "[#$($r.issue)]($RepoUrl/issues/$($r.issue))"
                [void]$blockingItems.Add(@{
                    area = "🐞 $issLink — $($r.classification)"
                    details = $r.title
                    action = $r.recommendedAction
                })
            }
        }
    }

    if ($blockingItems.Count -gt 0) {
        [void]$sb.AppendLine("## 🔴 Blocking — $($blockingItems.Count) item(s)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Area | Details | Next action |')
        [void]$sb.AppendLine('|---|---|---|')
        foreach ($b in $blockingItems) {
            $area = Format-MarkdownTableCell $b.area
            $details = Format-MarkdownTableCell $b.details
            $action = Format-MarkdownTableCell $b.action
            [void]$sb.AppendLine("| $area | $details | $action |")
        }
        [void]$sb.AppendLine()
    } else {
        [void]$sb.AppendLine("## 🟢 No blocking items")
        [void]$sb.AppendLine()
    }

    # === CLEANUP FOLLOW-UPS (hoisted under the blocking summary) ===
    # CLEANUP-status ship checks are real follow-ups (stale milestones, missing
    # bug-template entries) that should get done but don't prevent shipping.
    # Surface them prominently so they don't get lost, but keep them separate
    # from the 🔴 Blocking table — the release captain shouldn't have to wade
    # past housekeeping to find the actual ship blockers.
    $cleanupItems = New-Object System.Collections.Generic.List[hashtable]
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
        foreach ($sc in $Data['shipChecks']) {
            if ($sc.Status -eq 'CLEANUP') {
                [void]$cleanupItems.Add(@{
                    area = "🧹 $($sc.Area)"
                    details = $sc.Details
                    action = $sc.NextAction
                })
            }
        }
    }
    if ($cleanupItems.Count -gt 0) {
        [void]$sb.AppendLine("## 🧹 Cleanup follow-ups — $($cleanupItems.Count) item(s)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("_These are housekeeping items that should be addressed but do NOT block this release._")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Area | Details | Next action |')
        [void]$sb.AppendLine('|---|---|---|')
        foreach ($c in $cleanupItems) {
            $area = Format-MarkdownTableCell $c.area
            $details = Format-MarkdownTableCell $c.details
            $action = Format-MarkdownTableCell $c.action
            [void]$sb.AppendLine("| $area | $details | $action |")
        }
        [void]$sb.AppendLine()
    }

    # === Recent CI Failure Scanner signals (hoisted near the top so signals
    #     specific to this release branch are surfaced before deeper
    #     readiness / SR contents / regression analysis) ===
    if ($Data.ContainsKey('ciScanIssues')) {
        $ciScanBranch = $ctx.srBranch
        $ciScanFilteredOut = if ($Data.ContainsKey('ciScanFilteredOut')) { [int]$Data['ciScanFilteredOut'] } else { 0 }
        $ciScanIssuesData = @($Data['ciScanIssues'])
        [void]$sb.AppendLine("## Recent CI Failure Scanner signals (``ci-scan``)")
        [void]$sb.AppendLine()
        $blurb = "_Filtered to issues whose ``**Branch**: <name>`` body marker matches ``$ciScanBranch`` (auto-filed by the CI Failure Scanner workflow every 12h). Fresh issues (<24h) are flagged 🆕._"
        if ($ciScanFilteredOut -gt 0) {
            $blurb += " _$ciScanFilteredOut other-branch issue(s) were excluded as not relevant to this SR._"
        }
        [void]$sb.AppendLine($blurb)
        [void]$sb.AppendLine()
        if ($ciScanIssuesData.Count -gt 0) {
            $rows = Format-CiScanIssueRows -Issues $ciScanIssuesData -RepoUrl $RepoUrl
            if ($rows) {
                [void]$sb.Append($rows)
            } else {
                [void]$sb.AppendLine("_No ci-scan issues target ``$ciScanBranch``._")
            }
        } else {
            [void]$sb.AppendLine("_No ci-scan issues target ``$ciScanBranch``._")
        }
        [void]$sb.AppendLine()
    }

    # === OPEN FIX PRs INBOUND (hoisted high — actionable intelligence) ===
    # Regression issues whose fix is in flight as an open PR (either against main
    # awaiting merge, or already targeting SR as a backport). These deserve more
    # visibility than buried in Tier 2 — they're the pre-backport pipeline the
    # release captain needs to watch.
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        $openFixRows = New-Object System.Collections.Generic.List[hashtable]
        foreach ($r in $Data['regressions']) {
            if ($r.classification -ne 'open-on-main' -and $r.classification -ne 'backport-in-progress') { continue }
            if (-not $r.candidateFixPrs -or $r.candidateFixPrs.Count -eq 0) { continue }

            $issLink = "[#$($r.issue)]($RepoUrl/issues/$($r.issue))"
            $titleShort = if ($r.title.Length -gt 70) { $r.title.Substring(0, 70) + '...' } else { $r.title }
            $issCell = "$issLink — $titleShort"

            if ($r.classification -eq 'backport-in-progress') {
                # The open backport PR targets the SR branch directly — pick the
                # first OPEN one from the candidate fix PR's backports array.
                foreach ($cp in $r.candidateFixPrs) {
                    # Hashtables expose ContainsKey; PSCustomObjects expose .PSObject.Properties.
                    # Test both since candidateFixPrs records can be either shape.
                    $hasBackports = $false
                    if ($cp -is [hashtable] -or $cp -is [System.Collections.IDictionary]) {
                        $hasBackports = $cp.ContainsKey('backports') -and $cp['backports']
                    } elseif ($cp.PSObject.Properties['backports']) {
                        $hasBackports = [bool]$cp.backports
                    }
                    if (-not $hasBackports) { continue }
                    $openBp = $cp.backports | Where-Object { $_.state -eq 'OPEN' } | Select-Object -First 1
                    if ($openBp) {
                        $prLink = "[#$($openBp.number)]($RepoUrl/pull/$($openBp.number))"
                        [void]$openFixRows.Add(@{
                            prCell  = $prLink
                            baseCell = "``$srBranch``"
                            issCell = $issCell
                            statusCell = "🟡 backport OPEN on SR"
                            actionCell = 'Land this PR before ship'
                        })
                        break
                    }
                }
            } else {
                # open-on-main: fix PR is OPEN against main (or another non-SR base).
                # Pick the first candidate PR whose state is OPEN.
                $openMain = $r.candidateFixPrs | Where-Object { $_.state -eq 'OPEN' } | Select-Object -First 1
                if ($openMain) {
                    $prLink = "[#$($openMain.number)]($RepoUrl/pull/$($openMain.number))"
                    $base = if ($openMain.baseRef) { "``$($openMain.baseRef)``" } else { '`main`' }
                    [void]$openFixRows.Add(@{
                        prCell  = $prLink
                        baseCell = $base
                        issCell = $issCell
                        statusCell = '🔵 OPEN — awaiting main merge'
                        actionCell = 'Watch for merge, then open backport to SR'
                    })
                }
            }
        }

        if ($openFixRows.Count -gt 0) {
            [void]$sb.AppendLine("## 📥 Open Fix PRs Inbound — $($openFixRows.Count) PR(s)")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('_Fix PRs in flight for regression issues. Land these (or their backports) before ship to close out the regression list._')
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('| Fix PR | Base | Regression issue | Status | Next action |')
            [void]$sb.AppendLine('|---|---|---|---|---|')
            foreach ($row in $openFixRows) {
                $prCell    = Format-MarkdownTableCell $row.prCell
                $baseCell  = Format-MarkdownTableCell $row.baseCell
                $issCell   = Format-MarkdownTableCell $row.issCell
                $statCell  = Format-MarkdownTableCell $row.statusCell
                $actCell   = Format-MarkdownTableCell $row.actionCell
                [void]$sb.AppendLine("| $prCell | $baseCell | $issCell | $statCell | $actCell |")
            }
            [void]$sb.AppendLine()
        }
    }

    # === SHIP-READINESS CHECKS (full table — non-blocking + blocking) ===
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks'] -and $Data['shipChecks'].Count -gt 0) {
        [void]$sb.AppendLine("## Ship-readiness checks")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Check | Status | Details | Next action |')
        [void]$sb.AppendLine('|---|---|---|---|')
        foreach ($sc in $Data['shipChecks']) {
            $statusEmoji = switch ($sc.Status) {
                'READY'   { '🟢 READY' }
                'WATCH'   { '🟡 WATCH' }
                'BLOCKED' { '🔴 BLOCKED' }
                'CLEANUP' { '🧹 CLEANUP' }
                default   { "⚪ $($sc.Status)" }
            }
            $area = Format-MarkdownTableCell $sc.Area
            $details = Format-MarkdownTableCell $sc.Details
            $action = Format-MarkdownTableCell $sc.NextAction
            [void]$sb.AppendLine("| $area | $statusEmoji | $details | $action |")
        }
        [void]$sb.AppendLine()
    }

    # === HUMAN-EDITABLE SECTION ===
    # Wrapped in begin/end markers so a workflow can preserve manual edits
    # across re-runs (idempotency). Built as a reusable block so the body-size
    # cap below can guarantee the markers survive truncation — a truncated body
    # that lost them would let the daily refresh overwrite live Release Captain
    # Notes (the markers sit mid-body, below potentially unbounded sections).
    $notesSb = [System.Text.StringBuilder]::new()
    [void]$notesSb.AppendLine("<!-- release-readiness:human-notes:begin -->")
    [void]$notesSb.AppendLine("## Release Captain Notes")
    [void]$notesSb.AppendLine()
    [void]$notesSb.AppendLine("_Add manual notes here. Anything between these begin/end markers is preserved across automated re-runs._")
    [void]$notesSb.AppendLine("<!-- release-readiness:human-notes:end -->")
    $notesBlockText = $notesSb.ToString()
    [void]$sb.Append($notesBlockText)
    [void]$sb.AppendLine()

    # === CI section ===
    if ($Data.ContainsKey('ci') -and $Data['ci']) {
        $ciData = $Data['ci']
        [void]$sb.AppendLine("## CI Status — overall: ``$($ciData.overall)``")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Pipeline | Verdict | Latest result | At/ahead of SR HEAD? | Build |')
        [void]$sb.AppendLine('|---|---|---|---|---|')
        foreach ($p in $ciData.pipelines) {
            $lb = $p.latestBuild
            $pverdict = $p.verdict
            $result = if ($lb -and $lb.result) { $lb.result } elseif ($lb -and $lb.status -in @('inProgress','notStarted')) { "_$($lb.status)_" } else { '—' }
            $fresh = if ($lb) { if ($lb.isAtOrAheadOfSrHead) { '✅' } else { '❌ stale' } } else { '—' }
            $buildLink = if ($lb -and $lb.url) { "[$($lb.id)]($($lb.url))" } else { '—' }
            [void]$sb.AppendLine("| $($p.name) | ``$pverdict`` | $result | $fresh | $buildLink |")
        }
        [void]$sb.AppendLine()
    }

    # === Recent CI Failure Scanner signals: hoisted to top, see earlier block ===

    # === SR contents section ===
    if ($Data.ContainsKey('srContents') -and $Data['srContents']) {
        $sc = $Data['srContents']
        [void]$sb.AppendLine("## What's New in SR — $($sc.commitCount) commits")
        [void]$sb.AppendLine()
        if ($inherits -and $sc.ContainsKey('inheritedCommitCount') -and $sc['inheritedCommitCount'] -gt 0) {
            [void]$sb.AppendLine("- **From main** (since prior SR): $($sc.primaryCommitCount) commits / $($sc.primarySourcePrs.Count) source PRs")
            [void]$sb.AppendLine("- **Inherited from $($ctx.priorSrBranch)** (will be merged in after cut): $($sc.inheritedCommitCount) commits / $($sc.inheritedSourcePrs.Count) source PRs")
            [void]$sb.AppendLine("- **Total source PRs** (deduplicated): **$($sc.sourcePrs.Count)** (see ``sr-source-prs.txt``)")
        } else {
            [void]$sb.AppendLine("- Source PRs included: **$($sc.sourcePrs.Count)** (see ``sr-source-prs.txt``)")
        }
        [void]$sb.AppendLine("- Reverts detected: **$($sc.reverts.Count)**")
        if ($sc.reverts.Count -gt 0) {
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('### Reverts')
            [void]$sb.AppendLine('| Revert commit | Reverts PR | Reverts commit | On |')
            [void]$sb.AppendLine('|---|---|---|---|')
            foreach ($r in $sc.reverts) {
                $rs = ConvertTo-LinkedSha -Sha $r.revertCommit -RepoUrl $RepoUrl
                $rc = ConvertTo-LinkedSha -Sha $r.revertsCommit -RepoUrl $RepoUrl
                $rp = ConvertTo-LinkedPr -PrNumber $r.revertsPr -RepoUrl $RepoUrl
                $ro = if ($r.ContainsKey('origin')) { $r.origin } else { '?' }
                [void]$sb.AppendLine("| $rs | $rp | $rc | $ro |")
            }
        }
        [void]$sb.AppendLine()
    }

    # === Open SR-targeting PRs ===
    #
    # Two modes:
    #   - Live SR (mode != 'candidate'): show the full table — these are real
    #     backport PRs targeting the SR branch, which is a small, useful set.
    #   - Candidate (mode == 'candidate'): srBranch is main, so this query
    #     returns 100+ open PRs targeting main — far too noisy for a tracker
    #     issue. Instead, surface only the dotnet/maui "candidate PR" if one
    #     exists (e.g. "June 8th, Candidate" — the PR that promotes a specific
    #     main commit as the basis for cutting the next SR).
    if ($Data.ContainsKey('openSrPrs') -and $Data['openSrPrs'] -and $Data['openSrPrs'].Count -gt 0) {
        if ($mode -eq 'candidate') {
            # Find a PR whose title looks like a candidate-promotion PR.
            # Be conservative — require a word boundary so "CandidateView" doesn't match.
            $candidatePrs = @($Data['openSrPrs'] | Where-Object {
                $_.title -match '(?i)\bcandidate\b'
            })
            [void]$sb.AppendLine("## Candidate PR for next SR cut")
            [void]$sb.AppendLine()
            if ($candidatePrs.Count -eq 0) {
                [void]$sb.AppendLine("_No open PR titled `*Candidate*` found targeting ``$srBranch``. Open one when ready to promote a main commit as the SR cut point._")
            } else {
                foreach ($cp in $candidatePrs) {
                    $cpLink = ConvertTo-LinkedPr -PrNumber $cp.number -RepoUrl $RepoUrl
                    $cpTitle = if ($cp.title.Length -gt 80) { $cp.title.Substring(0, 80) + '...' } else { $cp.title }
                    # Collapse newlines (and escape pipes) even though this is a list, not a
                    # table: an upstream title with an embedded newline could otherwise push
                    # injected content (e.g. a forged `<!-- release-readiness:human-notes -->`
                    # marker) onto its own physical line. Markdown renders `\|` as `|` in a
                    # list, so escaping is harmless here.
                    $cpTitle = Format-MarkdownTableCell $cpTitle
                    [void]$sb.AppendLine("- $cpLink — $cpTitle (by $(Format-GitHubHandle $cp.author.login), updated $($cp.updatedAt))")
                }
                [void]$sb.AppendLine()
                [void]$sb.AppendLine("_Full list of $($Data['openSrPrs'].Count) open PRs targeting ``$srBranch`` omitted to reduce noise; see [the PR list]($RepoUrl/pulls?q=is%3Apr+is%3Aopen+base%3A$srBranch)._")
            }
            [void]$sb.AppendLine()
        } else {
            [void]$sb.AppendLine("## Open PRs Targeting $srBranch — $($Data['openSrPrs'].Count)")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('| PR | Title | Author | Draft? | Review | Updated |')
            [void]$sb.AppendLine('|---|---|---|---|---|---|')
            foreach ($pr in $Data['openSrPrs']) {
                $title = if ($pr.title.Length -gt 60) { $pr.title.Substring(0, 60) + '...' } else { $pr.title }
                $title = Format-MarkdownTableCell $title
                $draft = if ($pr.isDraft) { '✏️' } else { '' }
                $rev = if ($pr.reviewDecision) { $pr.reviewDecision } else { '—' }
                $prLink = ConvertTo-LinkedPr -PrNumber $pr.number -RepoUrl $RepoUrl
                [void]$sb.AppendLine("| $prLink | $title | $(Format-GitHubHandle $pr.author.login) | $draft | $rev | $($pr.updatedAt) |")
            }
            [void]$sb.AppendLine()
        }
    }

    # === Regressions section — organized into tiers ===
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        # Force array context: regression results are hashtables, and when exactly one
        # candidate exists PowerShell unwraps the single-element array to that lone hashtable,
        # so $regs.Count would otherwise return the hashtable's key count instead of 1.
        $regs = @($Data['regressions'])
        $summary = if ($Data.ContainsKey('summary')) { $Data['summary'] } else { @{} }

        [void]$sb.AppendLine("## Regression Candidates — $($regs.Count) issues scanned")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('### Summary')
        [void]$sb.AppendLine('| Verdict | Count |')
        [void]$sb.AppendLine('|---|---|')
        foreach ($k in $summary.Keys | Sort-Object) {
            [void]$sb.AppendLine("| ``$k`` | $($summary[$k]) |")
        }
        [void]$sb.AppendLine()

        # Three deterministic tiers. Order within a tier is alphabetical
        # over the classification name for stable diffs across runs.
        $tier1Classes = @('in-sr-reverted', 'no-fix-yet') | Sort-Object
        $tier2Classes = @('rejected-from-sr', 'backport-in-progress', 'merged-on-main-no-backport',
                          'merged-non-main-only', 'open-on-main', 'needs-human-review') | Sort-Object
        $tier3Classes = @('in-sr-active', 'closed-as-duplicate', 'no-fix-yet', 'out-of-scope-future-sr') | Sort-Object

        $emitTier = {
            param([string]$Header, [string[]]$Classes, [string]$EmptyLine, [string]$NoFixYetState)
            $any = $false
            foreach ($cls in $Classes) {
                $items = @($regs | Where-Object { $_.classification -eq $cls })
                # no-fix-yet splits by issue state to mirror the verdict tiering
                # (the Get-VerdictTier downgrade): OPEN ones block (Tier 1), CLOSED-but-
                # unresolved ones are informational (Tier 3). Without this split the closed
                # entries are counted in the Summary yet rendered in no tier at all.
                if ($cls -eq 'no-fix-yet') {
                    if ($NoFixYetState -eq 'OPEN') {
                        $items = @($items | Where-Object { $_.state -eq 'OPEN' })
                    } elseif ($NoFixYetState -eq 'CLOSED') {
                        $items = @($items | Where-Object { $_.state -ne 'OPEN' })
                    }
                }
                if ($items.Count -eq 0) { continue }
                if (-not $any) {
                    [void]$sb.AppendLine("### $Header")
                    [void]$sb.AppendLine()
                    $any = $true
                }
                [void]$sb.AppendLine("#### ``$cls`` ($($items.Count))")
                [void]$sb.AppendLine()
                [void]$sb.AppendLine('| Issue | Title | Fix PRs | Action |')
                [void]$sb.AppendLine('|---|---|---|---|')
                # Stable sort: by issue number ascending
                foreach ($it in ($items | Sort-Object issue)) {
                    $title = if ($it.title.Length -gt 50) { $it.title.Substring(0, 50) + '...' } else { $it.title }
                    $title = Format-MarkdownTableCell $title
                    $prList = @($it.candidateFixPrs | ForEach-Object { ConvertTo-LinkedPr -PrNumber $_.number -RepoUrl $RepoUrl }) -join ', '
                    if (-not $prList) { $prList = '—' }
                    $issueLink = if ($RepoUrl) { "[#$($it.issue)]($RepoUrl/issues/$($it.issue))" } else { "#$($it.issue)" }
                    [void]$sb.AppendLine("| $issueLink | $title | $prList | $($it.recommendedAction) |")
                }
                [void]$sb.AppendLine()
            }
            if (-not $any -and $EmptyLine) {
                [void]$sb.AppendLine("### $Header")
                [void]$sb.AppendLine()
                [void]$sb.AppendLine($EmptyLine)
                [void]$sb.AppendLine()
            }
        }

        & $emitTier '🔴 Tier 1 — Blocking' $tier1Classes '_No blocking regressions._' 'OPEN'
        & $emitTier '🟡 Tier 2 — Risk / Review' $tier2Classes '_No risk-tier regressions._' $null
        & $emitTier '🟢 Tier 3 — Informational' $tier3Classes $null 'CLOSED'
    }

    $body = $sb.ToString()

    # === SAFETY NET: defang any bare @-mentions ===
    # Primary defense is Format-GitHubHandle at emit time, but PR/issue
    # titles or commit messages can contain raw `@user` references that
    # would notify real users every time this report is filed. Wrap any
    # `@handle` in backticks so GitHub renders it as a code span (no mention).
    $body = [regex]::Replace(
        $body,
        '(^|[^a-zA-Z0-9/`])@([a-zA-Z0-9][a-zA-Z0-9_-]*(?:/[a-zA-Z0-9][a-zA-Z0-9_-]*)?)',
        '$1`$2`'
    )

    # === BODY-SIZE CAP ===
    # GitHub issue body limit is 65,536 bytes. Cap below that and append a
    # truncation message. We measure UTF-8 bytes, not character count.
    #
    # The human-notes block must SURVIVE truncation: it sits mid-body, below
    # sections (ci-scan signals, inbound fix PRs, ship-readiness table) that can
    # grow unbounded on a busy SR. A blind byte-prefix cut could drop the
    # begin/end markers, and the daily refresh would then use a markerless body
    # to OVERWRITE the live issue — wiping any Release Captain Notes the team
    # added. So we strip the placeholder, truncate only the remaining content
    # (reserving room for the notes block + message), then re-append the block.
    # This guarantees exactly one clean begin/end pair always survives for the
    # workflow splice. The placeholder carries no human data (real notes live on
    # the issue), so removing/re-adding it is lossless. The top-of-body hash and
    # tracker markers are well within the reserved prefix, so they survive too.
    $bytes = [System.Text.Encoding]::UTF8.GetByteCount($body)
    if ($bytes -gt $MaxBodyBytes) {
        $truncateMsg = "`n`n> ⚠️ **Report truncated** ($bytes bytes exceeded cap of $MaxBodyBytes). See full data in workflow artifacts.`n"
        $tail = [System.Text.Encoding]::UTF8.GetByteCount($truncateMsg)
        $notesTail = "`n" + $notesBlockText
        $notesReserve = [System.Text.Encoding]::UTF8.GetByteCount($notesTail)
        $bodyNoNotes = $body.Replace($notesBlockText, '')
        $targetLen = $MaxBodyBytes - $tail - $notesReserve
        if ($targetLen -lt 0) { $targetLen = 0 }
        # Walk back to a safe character boundary
        $bodyBytes = [System.Text.Encoding]::UTF8.GetBytes($bodyNoNotes)
        if ($targetLen -gt $bodyBytes.Length) { $targetLen = $bodyBytes.Length }
        $truncatedBytes = New-Object byte[] $targetLen
        [Array]::Copy($bodyBytes, 0, $truncatedBytes, 0, $targetLen)
        # UTF-8 boundary repair: if the cut landed inside a multi-byte sequence,
        # drop the trailing INCOMPLETE sequence. Walk back over continuation
        # bytes (10xxxxxx) to the lead byte, infer the sequence length from the
        # lead, and cut at the lead only when the full sequence doesn't fit. A
        # naive "trim continuation bytes" loop is wrong twice over: it leaves an
        # orphan lead byte (e.g. a lone 0xF0) AND it strips a COMPLETE trailing
        # multibyte char down to its lead. Either case makes GetString() emit a
        # U+FFFD replacement char, which re-encodes to 3 bytes and can push the
        # body back over $MaxBodyBytes.
        if ($truncatedBytes.Length -gt 0) {
            $i = $truncatedBytes.Length - 1
            while ($i -ge 0 -and ($truncatedBytes[$i] -band 0xC0) -eq 0x80) { $i-- }
            if ($i -ge 0) {
                $lead = $truncatedBytes[$i]
                $seqLen = if (($lead -band 0x80) -eq 0x00) { 1 }
                          elseif (($lead -band 0xE0) -eq 0xC0) { 2 }
                          elseif (($lead -band 0xF0) -eq 0xE0) { 3 }
                          elseif (($lead -band 0xF8) -eq 0xF0) { 4 }
                          else { 1 }
                if (($i + $seqLen) -gt $truncatedBytes.Length) {
                    $newArr = New-Object byte[] $i
                    [Array]::Copy($truncatedBytes, 0, $newArr, 0, $i)
                    $truncatedBytes = $newArr
                }
            }
        }
        $body = [System.Text.Encoding]::UTF8.GetString($truncatedBytes) + $notesTail + $truncateMsg
    }

    return $body
}

# region ────────────────────── 8. ORCHESTRATOR ────────────────────────────

function Add-SrNightlyFeedFreshness {
    <#
    .SYNOPSIS
        Maps this SR lane to its nightly Azure Artifacts dogfood feed + version band,
        queries the freshest matching build, and stores both the structured result
        ($Data['nightlyFeed']) and a pre-rendered banner string ($Data['nightlyFeedBanner']).
    .DESCRIPTION
        Lane → feed/band mapping (verified against the live feeds):
          - feed    = dotnet<Major>            (e.g. dotnet10, dotnet11)
          - signal  = the inflight/current dogfood stream (ci.inflight builds) on that feed —
                      the "shipping next" bits dogfooders validate against. Resolved feed-wide
                      (not band-pinned) so it auto-follows when inflight/current advances bands
                      (e.g. 10.0.80 → 10.0.90). Ordinary main CI (ci.main) is deliberately NOT
                      tracked: it publishes daily and would paint an inflight stall green.
          - band    = <Major>.0.<Patch>        (PatchVersion from eng/Versions.props at srRef)
                      used only as a FALLBACK when the feed has no inflight builds at all
                      (e.g. a preview feed not yet in the inflight phase).
        Fail-open throughout: any gap (helper not loaded, version unreadable, network error)
        degrades to "no banner"/"unknown" rather than disturbing the verdict.
    #>
    param([hashtable]$Data)

    if (-not $Script:NightlyFeedHelperLoaded) { return }
    if (-not (Get-Command Resolve-NightlyDogfoodFreshness -ErrorAction SilentlyContinue)) { return }
    if (-not (Get-Command Format-NightlyFeedBanner -ErrorAction SilentlyContinue)) { return }

    try {
        $ctx = $Data.metadata
        $surveyRef = $ctx.srRef
        $vp = Get-VersionsPropsState -Ref $surveyRef
        if (-not $vp) { return }   # can't map a band → skip silently (no banner)

        $major = [int]$vp.Major
        $patch = [int]$vp.Patch
        $band = "$major.0.$patch"
        $feed = "dotnet$major"
        $feedUrl = "https://dev.azure.com/dnceng/public/_artifacts/feed/$feed"
        $bandPrefix = '^' + [regex]::Escape($band) + '-'

        $fresh = Resolve-NightlyDogfoodFreshness -Feed $feed -BandPrefixRegex $bandPrefix
        if ($null -eq $fresh) { $fresh = @{ unknown = $true } }

        $buildType = [string](Get-NightlyFeedProp $fresh 'buildType')
        $laneLabel = Format-NightlyFeedLaneLabel -Feed $feed -FeedUrl $feedUrl -BuildType $buildType -BandNote "``$band``"
        $fresh['laneLabel'] = $laneLabel
        $fresh['feedUrl'] = $feedUrl
        $fresh['versionPrefix'] = $bandPrefix

        # Capture ONE timestamp and reuse it for both the banner render and the semantic-hash
        # tier (Get-ReportSemanticHash reads $Data['nightlyFeedNow']) so the two can never
        # sample different sides of a tier boundary within a single run.
        $nfNow = [DateTime]::UtcNow
        $Data['nightlyFeed'] = $fresh
        $Data['nightlyFeedNow'] = $nfNow
        $banner = Format-NightlyFeedBanner -Freshness $fresh -Now $nfNow
        if ($banner) { $Data['nightlyFeedBanner'] = $banner }
    } catch {
        # -WarningAction Continue: keep this fail-open even under an ambient
        # $WarningPreference='Stop', where a bare Write-Warning would be promoted to a
        # terminating error inside the catch and escape, crashing the unattended job.
        Write-Warning "Nightly-feed freshness check failed (non-fatal): $($_.Exception.Message)" -WarningAction Continue
    }
}

function Invoke-Main {
    $excludes = $ExcludeBranches -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
    $ctx = Resolve-Context -SrBranch $SrBranch -Repo $Repo -MainBranch $MainBranch `
                           -ExcludeBranches $excludes -NoFetch:$NoFetch -Candidate:$Candidate `
                           -InheritFromPriorSr:$InheritFromPriorSr

    # Resolve regression labels
    $labelMode = 'explicit'
    $labelInfo = $null
    if ($RegressionLabels) {
        $labels = @($RegressionLabels -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    } elseif ($InferRegressionLabels) {
        $labelInfo = Get-RegressionLabelsAuto -Ctx $ctx
        $labels = @($labelInfo.labels)
        $labelMode = "inferred ($($labelInfo.confidence))"
        if ($labels.Count -eq 0) {
            Write-Warn "Label inference produced no labels: $($labelInfo.error)"
        } else {
            Write-Host "Inferred regression labels: $($labels -join ', ')" -ForegroundColor Yellow
            Write-Host "  Confidence: $($labelInfo.confidence) — agent should confirm with user" -ForegroundColor Yellow
        }
    } else {
        # No labels, no inference: regressions phase is skipped silently
        $labels = @()
    }

    $ctx['regressionLabels'] = $labels
    $ctx['labelInferenceMode'] = $labelMode

    $data = @{
        metadata = $ctx
        warnings = @()
    }

    # Nightly dogfood feed freshness (full runs only). Maps this SR lane to its Azure
    # Artifacts feed + version band and records how fresh the newest matching build is, so
    # the tracker can flag when dogfooders are testing stale bits. Fail-open inside.
    if ($Phase -eq 'all') {
        Add-SrNightlyFeedFreshness -Data $data
    }

    if ($Phase -in 'all', 'commits', 'regressions') {
        $srContents = Get-SrCommits -Ctx $ctx
        $data['srContents'] = $srContents
    }

    if ($Phase -in 'all', 'ci') {
        $data['ci'] = Get-CIStatus -Ctx $ctx
    }

    if ($Phase -in 'all', 'open-prs') {
        $data['openSrPrs'] = Get-OpenSrPrs -Ctx $ctx
    }

    # Run version + bug-template checks (cheap; included in all phases except 'ci'-only).
    # These surface the "is versions.props bumped?" and "is the bug template updated?"
    # questions as blocking items at the top of the report.
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        $data['shipChecks'] = Get-ReleaseShipChecks -Ctx $ctx
    }

    # P/0-labelled open PRs targeting the SR branch are release blockers (SR-lane
    # parity with the Preview lane). Reuse the already-fetched open-PR list — only
    # the 'all'/'open-prs' phases populate it — so we avoid an extra `gh` call. A
    # BLOCKED result is auto-hoisted into the "🔴 Blocking" summary and escalates
    # the verdict to Not Ready via the shared ship-check machinery.
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        if ($data.ContainsKey('openSrPrs')) {
            if (-not $data.ContainsKey('shipChecks') -or -not $data['shipChecks']) {
                $data['shipChecks'] = @()
            }
            $data['shipChecks'] = @($data['shipChecks']) + @(Get-P0PrChecks -OpenSrPrs $data['openSrPrs'] -SrBranch $ctx.srBranch)
        }
    }

    # CI scanner + KBE issue signals — merged into shipChecks so they appear in the
    # ship-readiness table AND can escalate the verdict (fresh ci-scan → WATCH; never
    # BLOCKED automatically because the scanner can be noisy).
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        # Scope ci-scan to the branch we're surveying so other-branch noise
        # (e.g. main CI signals on an in-flight SR report) doesn't bleed in.
        # ctx.srBranch is automatically: main/$MainBranch in candidate mode
        # (when no SR has been cut yet) or the actual SR branch in in-flight mode.
        $signalResult = Get-CiSignalChecks -Branch $ctx.srBranch
        if (-not $data.ContainsKey('shipChecks') -or -not $data['shipChecks']) {
            $data['shipChecks'] = @()
        }
        $data['shipChecks'] = @($data['shipChecks']) + @($signalResult.Checks)
        $data['ciScanIssues'] = @($signalResult.CiScanIssues)
        $data['ciScanFilteredOut'] = $signalResult.CiScanFilteredOut
        $data['kbeIssues']    = @($signalResult.KbeIssues)
    }

    # Maestro/BAR operational checks — verify the SR branch is wired into BAR's
    # default-channel mappings and the SR HEAD commit has a published build.
    # Runs via `darc` CLI; falls back to UNKNOWN with verification commands when
    # darc isn't available (CI environments without the tool installed). Append
    # to shipChecks so BLOCKED results escalate the verdict the same way.
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        $maestroChecks = Get-MaestroOperationalChecks -Ctx $ctx -SkipChecks:$SkipMaestroChecks
        if ($maestroChecks -and $maestroChecks.Count -gt 0) {
            if (-not $data.ContainsKey('shipChecks') -or -not $data['shipChecks']) {
                $data['shipChecks'] = @()
            }
            $data['shipChecks'] = @($data['shipChecks']) + @($maestroChecks)
        }
    }

    # Milestone hygiene checks — confirm the current cycle's milestone exists,
    # the next cycle's milestone has been pre-created, and no past-due milestones
    # are still open from already-shipped releases. Uses gh API (always available
    # in CI), so no UNKNOWN fallback needed beyond the per-call try/catch.
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        $milestoneChecks = Get-MilestoneHygieneChecks -Ctx $ctx -SkipChecks:$SkipMilestoneChecks
        if ($milestoneChecks -and $milestoneChecks.Count -gt 0) {
            if (-not $data.ContainsKey('shipChecks') -or -not $data['shipChecks']) {
                $data['shipChecks'] = @()
            }
            $data['shipChecks'] = @($data['shipChecks']) + @($milestoneChecks)
        }
    }

    # Candidate-PR check (candidate mode only) — surface the open PR that
    # promotes a specific main commit as the SR cut point. Most important
    # PR in the cycle: SR can't be cut until it merges. Renders as a WATCH
    # check in the ship-readiness table.
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        $candidateChecks = Get-CandidatePrChecks -Ctx $ctx
        if ($candidateChecks -and $candidateChecks.Count -gt 0) {
            if (-not $data.ContainsKey('shipChecks') -or -not $data['shipChecks']) {
                $data['shipChecks'] = @()
            }
            $data['shipChecks'] = @($data['shipChecks']) + @($candidateChecks)
        }
    }

    if ($Phase -in 'all', 'regressions') {
        if ($labels.Count -eq 0) {
            Write-Warn "No regression labels provided/inferred; skipping regressions phase. Pass -RegressionLabels or -InferRegressionLabels."
            $data['regressions'] = @()
        } else {
            $data['regressions'] = Get-RegressionCandidates -Ctx $ctx -Labels $labels `
                                       -SrContents $data['srContents'] -MaxIssues $MaxIssues

            # Summary buckets
            $summary = @{}
            foreach ($r in $data['regressions']) {
                $k = $r.classification
                if (-not $summary.ContainsKey($k)) { $summary[$k] = 0 }
                $summary[$k] += 1
            }
            $data['summary'] = $summary
        }
    }

    $data['warnings'] = @($Script:Warnings)

    # Compute deterministic verdict + semantic hash. Surfaced in JSON so
    # automation can consume it without re-parsing the markdown.
    $verdict = Get-OverallVerdict -Data $data
    $semanticHash = Get-ReportSemanticHash -Data $data -Verdict $verdict
    $data['verdict'] = @{
        symbol = $verdict.symbol
        tier = $verdict.tier
        label = $verdict.label
        reasons = $verdict.reasons
    }
    $data['semanticHash'] = $semanticHash
    # Expected ship date — surfaced in JSON so downstream automation doesn't
    # repeat the cadence math. ASAP hotfixes return null date + cadence='asap-hotfix'.
    $metaForJson = $data.metadata
    $srRefForJson = if ($metaForJson -is [hashtable]) {
        if ($metaForJson.ContainsKey('srRef')) { $metaForJson['srRef'] } else { $null }
    } elseif ($metaForJson.PSObject.Properties.Name -contains 'srRef') {
        $metaForJson.srRef
    } else { $null }
    $patchForJson = $null
    if ($srRefForJson) {
        $vpForJson = Get-VersionsPropsState -Ref $srRefForJson
        if ($vpForJson) { $patchForJson = [int]$vpForJson.Patch }
    }
    $mainBumpDateForJson = $null
    $mainBumpShaForJson = $null
    if ($null -ne $patchForJson) {
        $cycleBaseForJson = [int]([Math]::Floor($patchForJson / 10) * 10)
        $majorForJson = $null
        if ($vpForJson -and $vpForJson.Major) { $majorForJson = [int]$vpForJson.Major }
        $bumpInfoForJson = if ($null -ne $majorForJson) {
            Get-MainBumpDateForCycle -CycleBase $cycleBaseForJson -MajorVersion $majorForJson
        } else {
            Get-MainBumpDateForCycle -CycleBase $cycleBaseForJson
        }
        if ($bumpInfoForJson) {
            $mainBumpDateForJson = $bumpInfoForJson.Date
            $mainBumpShaForJson = $bumpInfoForJson.Sha
        }
    }
    $shipDateInfo = Get-ExpectedShipDate -PatchVersion $patchForJson -MainBumpDate $mainBumpDateForJson
    $data['expectedShipDate'] = @{
        cadence       = $shipDateInfo.Cadence
        date          = if ($shipDateInfo.Date) { $shipDateInfo.Date.ToString('yyyy-MM-dd') } else { $null }
        daysFromNow   = $shipDateInfo.DaysFromNow
        formattedLong = $shipDateInfo.FormattedLong
        note          = $shipDateInfo.Note
        patchVersion  = $patchForJson
        missedWindow  = $shipDateInfo.MissedWindow
        anchorSource  = $shipDateInfo.AnchorSource
        mainBumpDate  = if ($mainBumpDateForJson) { $mainBumpDateForJson.ToString('yyyy-MM-dd') } else { $null }
        mainBumpSha   = $mainBumpShaForJson
    }
    if ($TrackerKey) {
        $data['trackerKey'] = $TrackerKey
    }

    # Output
    $jsonOut = $data | ConvertTo-Json -Depth 20 -Compress:$false
    $mdOut = Format-MarkdownReport -Data $data -RepoUrl $RepoUrl -TrackerKey $TrackerKey -MaxBodyBytes $MaxBodyBytes

    if ($OutputDir) {
        if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir | Out-Null }
        if ($OutputFormat -in 'json', 'both') {
            Set-Content -Path (Join-Path $OutputDir 'release-readiness.json') -Value $jsonOut -Encoding UTF8
        }
        if ($OutputFormat -in 'markdown', 'both') {
            Set-Content -Path (Join-Path $OutputDir 'release-readiness.md') -Value $mdOut -Encoding UTF8
        }
        if ($data.ContainsKey('srContents')) {
            $srcPrs = $data['srContents'].sourcePrs -join "`n"
            Set-Content -Path (Join-Path $OutputDir 'sr-source-prs.txt') -Value $srcPrs -Encoding UTF8

            $commitsJson = $data['srContents'] | ConvertTo-Json -Depth 10
            Set-Content -Path (Join-Path $OutputDir 'sr-commits.json') -Value $commitsJson -Encoding UTF8
        }
        Write-Host "`nWrote outputs to: $OutputDir" -ForegroundColor Green
        Get-ChildItem $OutputDir | ForEach-Object { Write-Host "  $($_.Name) ($($_.Length) bytes)" }
    } else {
        if ($OutputFormat -in 'json', 'both') { Write-Output $jsonOut }
        if ($OutputFormat -in 'markdown', 'both') { Write-Output $mdOut }
    }
}

# Skip orchestration when dot-sourced for unit tests. Tests do:
#   $env:GET_RELEASE_READINESS_TEST_MODE = '1'
#   . path/to/Get-ReleaseReadiness.ps1
# which makes Invoke-Main a no-op while still loading all functions.
if (-not $env:GET_RELEASE_READINESS_TEST_MODE) {
    Invoke-Main
}
