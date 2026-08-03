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
    # Mark the survey as a SHIPPED SR (its stable tag already exists). Surveys
    # the SR branch directly, but applies post-ship verdict, carry-forward, and
    # hotfix-vs-next-SR guidance semantics. Set by the workflow for the most-
    # recently-shipped SR, whose tracker refreshes until a human closes it.
    # Mutually exclusive with -Candidate.
    [switch]$Shipped,
    # Optional immutable published tag override for -Shipped mode.
    # Prevents a live branch hotfix bump from moving the shipped content anchor
    # before the newer hotfix tag is actually published.
    [string]$ShippedTag,
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
    [switch]$IncludeInternal,
    # Redact private/internal coordinates from Markdown and JSON outputs. Keep
    # enabled for any report that may be posted to a public GitHub issue.
    [bool]$PublicSafe = $true
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

$publicSanitizerHelperPath = Join-Path $PSScriptRoot 'PublicReportSanitizer.ps1'
if (-not (Test-Path $publicSanitizerHelperPath)) {
    throw "Required public-report sanitizer not found at $publicSanitizerHelperPath."
}
. $publicSanitizerHelperPath

function Select-OutputSrContents {
    param(
        [Parameter(Mandatory)]$Data,
        [bool]$PublicSafe = $true
    )

    $srContents = Get-MetadataValue -Container $Data -Name 'srContents'
    if ($null -eq $srContents) { return $null }
    if ($PublicSafe) { return ConvertTo-PublicSafeValue -Value $srContents }
    return $srContents
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
$Script:RegressionEvidenceFailures = [System.Collections.Generic.List[string]]::new()

function Write-Warn([string]$msg) {
    $Script:Warnings.Add($msg) | Out-Null
    Write-Host "warn: $msg" -ForegroundColor Yellow
}

function Add-RegressionEvidenceFailure([string]$Context) {
    if (-not [string]::IsNullOrWhiteSpace($Context)) {
        [void]$Script:RegressionEvidenceFailures.Add($Context)
    }
}

function ConvertFrom-GhJsonArrayResult {
    param($Raw, [string]$Context, [switch]$SuppressRegressionFailure)
    if ($null -eq $Raw) {
        if (-not $SuppressRegressionFailure) { Add-RegressionEvidenceFailure $Context }
        return [PSCustomObject]@{ Success = $false; Items = @() }
    }
    try {
        $rawJson = ($Raw | Out-String).Trim()
        if ([string]::IsNullOrWhiteSpace($rawJson)) { throw 'empty response' }
        $items = ConvertFrom-Json -InputObject $rawJson -NoEnumerate -ErrorAction Stop
        if ($null -eq $items -or $items -isnot [System.Array]) { throw 'expected a JSON array' }
        $flattened = if ($items.Count -gt 0 -and @($items | Where-Object { $_ -isnot [System.Array] }).Count -eq 0) {
            @($items | ForEach-Object { @($_) })
        } else {
            @($items)
        }
        return [PSCustomObject]@{ Success = $true; Items = $flattened }
    } catch {
        if (-not $SuppressRegressionFailure) { Add-RegressionEvidenceFailure "$Context ($($_.Exception.Message))" }
        return [PSCustomObject]@{ Success = $false; Items = @() }
    }
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

function Get-StableTagInfo {
    <#
    .SYNOPSIS
        Resolves the stable git tag for a shipped SR version and returns its date.
    .DESCRIPTION
        Shipped SRs publish a BARE stable tag `Major.Minor.Patch` (e.g. `10.0.60`)
        — no prerelease suffix. Given that version string, look up the tag ref and
        prefer the corresponding GitHub Release's published_at timestamp. MAUI's
        stable tags are lightweight, so git alone exposes only the tagged commit's
        committer date, not when the release became public. When release metadata is
        unavailable, use that commit date as a conservative content-freeze anchor.

        Returns [PSCustomObject]@{ Tag; Date (UTC); DateSource } or $null when the
        tag is absent or its date is unreadable.
    #>
    param(
        [Parameter(Mandatory)][string]$Version,
        [string]$Repo = 'dotnet/maui'
    )
    if ([string]::IsNullOrWhiteSpace($Version)) { return $null }
    # Guard: only accept a clean Major.Minor.Patch — never a prerelease/malformed
    # string that could resolve a non-stable tag.
    if ($Version -notmatch '^\d+\.\d+\.\d+$') { return $null }

    # The public GitHub Release timestamp is the best available evidence for when
    # customers could consume the release. Query it quietly because the tracker can
    # run between tag creation and Release publication, or during a GitHub outage.
    $publishedAt = Invoke-Gh @('api', "repos/$Repo/releases/tags/$Version", '--jq', '.published_at') -Quiet
    if ($publishedAt) {
        if ($publishedAt -is [array]) { $publishedAt = $publishedAt[0] }
        $publishedUtc = ConvertTo-Utc -Value ([string]$publishedAt).Trim()
        if ($publishedUtc) {
            return [PSCustomObject]@{
                Tag        = $Version
                Date       = $publishedUtc
                DateSource = 'github-release'
            }
        }
    }

    $ref = "refs/tags/$Version"
    # `creatordate` is the tagger date for annotated tags and the target commit's
    # committer date for lightweight tags. No spaces in the format token keeps it
    # safe under Invoke-Git's argument splitting.
    $dateStr = Invoke-Git "for-each-ref --format=%(creatordate:iso-strict) $ref"
    if (-not $dateStr) { $dateStr = Invoke-Git "log -1 --format=%cI $Version" }
    if (-not $dateStr) { return $null }
    if ($dateStr -is [array]) { $dateStr = $dateStr[0] }
    $dateUtc = ConvertTo-Utc -Value ([string]$dateStr).Trim()
    if (-not $dateUtc) { return $null }
    $tagType = Invoke-Git "cat-file -t $ref"
    if ($tagType -is [array]) { $tagType = $tagType[0] }
    return [PSCustomObject]@{
        Tag        = $Version
        Date       = $dateUtc
        DateSource = if (([string]$tagType).Trim() -eq 'tag') { 'annotated-tag' } else { 'tagged-commit' }
    }
}

function Test-GitRefResolves {
    param([string]$Ref)
    if ([string]::IsNullOrWhiteSpace($Ref)) { return $false }
    return [bool](Invoke-Git "rev-parse --verify --quiet $Ref`^{commit}")
}

function Test-BranchAdvancedBeyondTag {
    param(
        [Parameter(Mandatory)][string]$Tag,
        [Parameter(Mandatory)][string]$HeadSha
    )

    if ([string]::IsNullOrWhiteSpace($Tag) -or [string]::IsNullOrWhiteSpace($HeadSha)) {
        return $false
    }

    $tagCommit = Invoke-Git "rev-parse --verify --quiet refs/tags/$Tag`^{commit}"
    if ($tagCommit -is [array]) { $tagCommit = $tagCommit[0] }
    $tagCommit = ([string]$tagCommit).Trim()
    if (-not $tagCommit -or $tagCommit -eq $HeadSha) { return $false }

    return Test-CommitOnBranch -Sha $tagCommit -BranchRef $HeadSha
}

function Get-PublishedStableTags {
    param([string]$Repo = 'dotnet/maui')

    $raw = Invoke-Gh @(
        'api', "repos/$Repo/releases", '--paginate',
        '--jq', '.[] | select(.draft == false and .published_at != null) | .tag_name'
    ) -Quiet
    if ($null -eq $raw) { return $null }
    $values = @()
    foreach ($item in @($raw)) {
        foreach ($line in ([string]$item -split '\r?\n')) {
            $tag = $line.Trim()
            if ($tag -match '^\d+\.\d+\.\d+$') { $values += $tag }
        }
    }
    return @($values | Sort-Object -Unique)
}

function Get-LocalStableTags {
    $raw = Invoke-Git "tag --list"
    if ($null -eq $raw) { return $null }

    $values = @()
    foreach ($item in @($raw)) {
        $tag = ([string]$item).Trim()
        if ($tag -notmatch '^\d+\.\d+\.\d+$') { continue }
        [version]$parsed = $null
        if (-not [version]::TryParse($tag, [ref]$parsed)) { continue }
        $values += [PSCustomObject]@{ Name = $tag; Version = $parsed }
    }
    return @($values | Sort-Object Version | Select-Object -ExpandProperty Name -Unique)
}

function Write-ShippedPublicationPendingWarning {
    param([Parameter(Mandatory)][string]$Tag)

    Write-Warn "Stable tag '$Tag' exists, but its GitHub Release is not published yet. Shipped contents are anchored to the immutable tag and the displayed date uses tagged-commit evidence until publication metadata is available."
}

function Write-ShippedPublicationStatusUnknownWarning {
    param([Parameter(Mandatory)][string]$Tag)

    Write-Warn "Could not query GitHub Release publication metadata for stable tag '$Tag'. Shipped contents remain anchored to the immutable local tag, publication status is unknown, and the displayed date uses tagged-commit evidence."
}

function Resolve-ShippedPublicationState {
    param(
        [bool]$ListQueryFailed,
        [bool]$AnchorInPublishedList,
        [string]$TagDateSource
    )

    # A successful per-tag published_at lookup is definitive even when the
    # paginated list request failed independently.
    if ($TagDateSource -eq 'github-release' -or $AnchorInPublishedList) {
        return 'published'
    }
    if ($ListQueryFailed) { return 'unknown' }
    return 'pending'
}

function Select-LatestStableTagForSr {
    param(
        [string]$SrBranch,
        [string[]]$StableTags
    )

    $match = [regex]::Match($SrBranch, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
    if (-not $match.Success) { return $null }
    $major = [int]$match.Groups[1].Value
    $minor = [int]$match.Groups[2].Value
    $patchFloor = [int]$match.Groups[3].Value * 10
    $matches = @()
    foreach ($tag in @($StableTags)) {
        [version]$parsed = $null
        if (-not [version]::TryParse([string]$tag, [ref]$parsed)) { continue }
        if ($parsed.Major -eq $major -and $parsed.Minor -eq $minor -and
            $parsed.Build -ge $patchFloor -and $parsed.Build -lt ($patchFloor + 10)) {
            $matches += [PSCustomObject]@{ Name = [string]$tag; Version = $parsed }
        }
    }
    $latest = @($matches | Sort-Object Version -Descending | Select-Object -First 1)
    if ($latest.Count -gt 0) { return [string]$latest[0].Name }
    return $null
}

function Select-LatestPublishedTagForSr {
    param(
        [string]$SrBranch,
        [string[]]$PublishedTags
    )
    return Select-LatestStableTagForSr -SrBranch $SrBranch -StableTags $PublishedTags
}

function Test-StableTagMatchesSr {
    param(
        [string]$Tag,
        [string]$SrBranch
    )

    [version]$tagVersion = $null
    if (-not [version]::TryParse($Tag, [ref]$tagVersion)) { return $false }
    $match = [regex]::Match($SrBranch, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
    if (-not $match.Success) { return $false }
    $patchFloor = [int]$match.Groups[3].Value * 10
    return $tagVersion.Major -eq [int]$match.Groups[1].Value -and
        $tagVersion.Minor -eq [int]$match.Groups[2].Value -and
        $tagVersion.Build -ge $patchFloor -and $tagVersion.Build -lt ($patchFloor + 10)
}

function Get-PreviousSrBaselineTag {
    param(
        [string]$Version,
        [string[]]$PublishedTags
    )

    [version]$current = $null
    if (-not [version]::TryParse($Version, [ref]$current)) { return $null }
    $srPatchFloor = [int][math]::Floor($current.Build / 10) * 10
    $parsedTags = @()
    foreach ($tag in @($PublishedTags)) {
        if ($tag -notmatch '^\d+\.\d+\.\d+$') { continue }
        [version]$parsedTag = $null
        if (-not [version]::TryParse([string]$tag, [ref]$parsedTag)) { continue }
        if ($parsedTag -ge $current) { continue }
        # A hotfix tag must retain the full SR inventory, not only the delta
        # since the previous tag in the same patch decade.
        if ($parsedTag.Major -eq $current.Major -and
            $parsedTag.Minor -eq $current.Minor -and
            $parsedTag.Build -ge $srPatchFloor) { continue }
        $parsedTags += [PSCustomObject]@{ Name = [string]$tag; Version = $parsedTag }
    }
    $prior = @($parsedTags | Sort-Object Version -Descending | Select-Object -First 1)
    if ($prior.Count -gt 0) { return [string]$prior[0].Name }
    return $null
}

function Resolve-ShippedContentsRefs {
    param(
        [string]$Version,
        [string]$Repo = 'dotnet/maui',
        [string[]]$PublishedTags
    )

    if (-not (Test-GitRefResolves -Ref $Version)) {
        throw "Stable tag '$Version' does not resolve locally. Rerun without -NoFetch (or fetch refs/tags/$Version) before generating a shipped tracker."
    }
    if ($null -eq $PublishedTags) {
        $PublishedTags = Get-PublishedStableTags -Repo $Repo
    }
    if ($null -eq $PublishedTags -or @($PublishedTags).Count -eq 0) {
        throw "Cannot query stable-tag evidence to bound shipped contents for '$Version'."
    }
    if (@($PublishedTags) -notcontains $Version) {
        throw "Stable tag '$Version' is not present in the supplied stable-tag evidence set."
    }
    $previousTag = Get-PreviousSrBaselineTag -Version $Version -PublishedTags $PublishedTags
    if (-not $previousTag -or -not (Test-GitRefResolves -Ref $previousTag)) {
        throw "Cannot resolve a prior SR baseline tag to bound shipped contents for '$Version'. Fetch stable tags before generating a shipped tracker."
    }
    return [PSCustomObject]@{
        ContentsRef = $Version
        ExcludeRefs = @($previousTag)
        PreviousTag = $previousTag
    }
}

function Set-ShippedContentsRefs {
    param(
        [Parameter(Mandatory)][System.Collections.IDictionary]$Context,
        [Parameter(Mandatory)]$ShippedRefs
    )

    $Context['contentsRef'] = $ShippedRefs.ContentsRef
    $Context['excludeBranches'] = @($ShippedRefs.ExcludeRefs)
    $Context['previousStableTag'] = $ShippedRefs.PreviousTag
    $Context['mainRevertBaselineRef'] = $ShippedRefs.PreviousTag
}

function Get-ShippedStableTagsForBounds {
    param(
        [string]$AnchorTag,
        [string[]]$PublishedTags,
        [string[]]$LocalStableTags,
        [bool]$PublicationQueryFailed
    )

    if ($PublicationQueryFailed) {
        return @(Get-NonEmptyStringValues -Value $LocalStableTags | Sort-Object -Unique)
    }
    return @(Get-NonEmptyStringValues -Value @($PublishedTags + $AnchorTag) | Sort-Object -Unique)
}

function Test-IsCarryForwardRegression {
    <#
    .SYNOPSIS
        Decides whether a regression record is a POST-SHIP carry-forward for an
        already-shipped SR (and therefore non-gating in -Shipped mode).
    .DESCRIPTION
        Deterministic, evidence-only. A regression is carry-forward only when its
        milestone explicitly names a LATER SR (same major, higher SR number) or a
        later major — structured evidence that a triager assigned it to a future
        cycle. Issue creation time is not sufficient: a defect in shipped binaries
        can be reported after publication and may still require a hotfix decision.

        Does NOT parse free-text human notes, comments, or labels beyond the
        structured milestone. Returns $false when it cannot make a positive
        determination (unknown shipped cycle or no later-SR milestone) so the
        caller keeps the conservative "still a follow-up to review" stance.

        StrictMode/shape-safe: reads `milestone` through Get-MetadataValue so a
        hashtable (live survey) and a pscustomobject (JSON round-trip) both work.
    #>
    param(
        $Regression,
        [int]$ShippedSrNumber = 0,
        [int]$ShippedMajor = 0,
        [int]$ShippedSubPatch = 0
    )
    if ($null -eq $Regression) { return $false }

    # A structured milestone names a later SR / later major.
    $milestone = Get-MetadataValue -Container $Regression -Name 'milestone'
    $milestoneMajor = Get-MauiReleaseMilestoneMajor -Milestone $milestone
    if ($ShippedMajor -gt 0 -and $milestoneMajor -gt $ShippedMajor) { return $true }

    $milestoneParts = Get-SrMilestoneParts -Milestone $milestone
    if ($milestoneParts) {
        if ($milestoneParts.Major -eq $ShippedMajor -and $ShippedSrNumber -gt 0 -and $milestoneParts.SrNumber -gt $ShippedSrNumber) { return $true }
        if ($milestoneParts.Major -eq $ShippedMajor -and $milestoneParts.SrNumber -eq $ShippedSrNumber -and
            $milestoneParts.SubPatch -gt $ShippedSubPatch) { return $true }
    }

    return $false
}

function Get-SrMilestoneParts {
    param([string]$Milestone)
    if ([string]::IsNullOrWhiteSpace($Milestone)) { return $null }

    $match = [regex]::Match($Milestone, '^\.NET\s+(\d+)(?:\.0)?\s+SR(\d+)(?:\.(\d+))?$')
    if (-not $match.Success) { return $null }

    $major = ConvertTo-PrNumber -Value $match.Groups[1].Value
    $srNumber = ConvertTo-PrNumber -Value $match.Groups[2].Value
    [long]$parsedSubPatch = 0
    $subPatch = if ($match.Groups[3].Success) {
        if ([long]::TryParse($match.Groups[3].Value, [ref]$parsedSubPatch) -and
            $parsedSubPatch -ge 0 -and $parsedSubPatch -le [int]::MaxValue) {
            [int]$parsedSubPatch
        } else { $null }
    } else { 0 }
    if ($null -eq $major -or $null -eq $srNumber -or $null -eq $subPatch) { return $null }
    return [PSCustomObject]@{ Major = $major; SrNumber = $srNumber; SubPatch = $subPatch }
}

function Get-SrSubPatchFromVersion {
    param([string]$Version)
    $match = [regex]::Match([string]$Version, '^\d+\.\d+\.(\d+)$')
    if (-not $match.Success) { return 0 }

    $patch = [int]$match.Groups[1].Value
    if ($patch -lt 10) { return 0 }
    return ($patch % 10)
}

function Get-MauiReleaseMilestoneMajor {
    param([string]$Milestone)
    if ([string]::IsNullOrWhiteSpace($Milestone)) { return 0 }

    $patterns = @(
        '(?i)^\.NET\s+(\d+)(?:\.0)?\s+SR\d+(?:\.\d+)?$',
        '(?i)^\.NET\s+(\d+)(?:\.0)?-(?:preview|rc)\d+$',
        '(?i)^\.NET\s+(\d+)(?:\.0)?\s+(?:GA|Servicing)$'
    )
    foreach ($pattern in $patterns) {
        $match = [regex]::Match($Milestone, $pattern)
        if ($match.Success) {
            $major = ConvertTo-PrNumber -Value $match.Groups[1].Value
            if ($null -ne $major) { return $major }
            return 0
        }
    }
    return 0
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
    $isShipped = ($Ctx.mode -eq 'shipped')

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
    # Shipped mode: the immutable stable-tag contents.
    # Candidate mode: main (which would carry the bump once SR-prior cuts).
    $versionsRef = if ($isCandidate) {
        "origin/$($Ctx.mainBranch)"
    } elseif ($isShipped) {
        Get-MetadataValue -Container $Ctx -Name 'contentsRef' -Default $Ctx.srRef
    } else {
        $Ctx.srRef
    }
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
            $prevSrRef    = if ($isShipped) {
                Get-MetadataValue -Container $Ctx -Name 'previousStableTag'
            } else {
                "origin/$prevSrBranch"
            }
            $mainRef      = if ($Ctx -is [hashtable]) {
                if ($Ctx.ContainsKey('mainBranch')) { "origin/$($Ctx['mainBranch'])" } else { 'origin/main' }
            } elseif ($Ctx.PSObject.Properties.Name -contains 'mainBranch') {
                "origin/$($Ctx.mainBranch)"
            } else { 'origin/main' }
            $flipDirectSha = $null
            try {
                $releaseContentRef = if ($isShipped) { $versionsRef } else { "origin/$($Ctx.srBranch)" }
                $logArgs = @('log', '--no-merges', '--pretty=%H', $releaseContentRef)
                if ($prevSrRef) { $logArgs += "^$prevSrRef" }
                if (-not $isShipped) { $logArgs += "^$mainRef" }
                $logArgs += @('--', 'eng/Versions.props')
                $shas = & git @logArgs 2>$null
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
                    $mergeSha = git log --merges --pretty='%H' --first-parent $versionsRef -- eng/Versions.props 2>$null | Select-Object -First 1
                    if ($mergeSha) { $mergeShaShort = $mergeSha.Trim().Substring(0, 10) }
                } catch { }
                $provenanceLabel = if ($isShipped -and $prevSrRef) { $prevSrRef } else { $prevSrBranch }
                $provenance = if ($mergeShaShort) {
                    "inherited from ``$provenanceLabel`` via catch-up merge ``$mergeShaShort``"
                } else {
                    "inherited from ``$provenanceLabel``"
                }
                $details = "``$versionsRef`` has ``PreReleaseVersionLabel=servicing`` and ``StabilizePackageVersion=true`` — branch IS configured to produce stable release packages. The values were $provenance, rather than from an SR-direct flip PR; this is the valid cut-then-merge pattern used by .NET 10 SR8."
            }

            $checks += New-ReadinessCheck -Area $flipArea -Status 'READY' `
                -Details $details `
                -NextAction "No change needed."
        } else {
            $missing = @()
            if (-not $labelOk) { $missing += "``PreReleaseVersionLabel=$actualLabel`` (expected ``servicing``)" }
            if (-not $stabilizeOk) { $missing += "``StabilizePackageVersion=$actualStabilize`` (expected ``true``)" }
            $flipDetails = if ($isShipped) {
                "The published stable tag ``$versionsRef`` records an invalid servicing configuration: $($missing -join '; '). This cannot be repaired retroactively in the shipped tag; verify the published assets and decide whether a hotfix/rebuild or documented follow-up is required."
            } else {
                "``$versionsRef`` is NOT flipped to servicing-release mode: $($missing -join '; '). Without these flips the branch builds prerelease packages and will not ship as a stable .NET release — CI stays green so nothing else catches it."
            }
            $flipNextAction = if ($isShipped) {
                "Inspect the published ``$versionsRef`` packages and release metadata. If stable assets are wrong, coordinate the release-owner hotfix/rebuild path; otherwise document why the tag is safe despite the configuration."
            } else {
                "After the last required backport, open a focused PR targeting ``$($Ctx.srBranch)``. Preserve ``PatchVersion``; replace the base ``ci.main`` label and remove its ``inflight/current`` conditional with ``<PreReleaseVersionLabel>servicing</PreReleaseVersionLabel>``, then set ``<StabilizePackageVersion Condition=`"'`$(StabilizePackageVersion)' == ''`">true</StabilizePackageVersion>``. Keep ``main`` on its next-cycle version and rerun final CI after the SR PR merges."
            }
            $checks += New-ReadinessCheck -Area $flipArea -Status 'BLOCKED' `
                -Details $flipDetails `
                -NextAction $flipNextAction
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

            # A bumped PatchVersion alone is not enough: main must also still be on
            # the dev-main config (PreReleaseVersionLabel=ci.main,
            # StabilizePackageVersion=false). If main is misconfigured as a
            # servicing/stable build while its patch is bumped, PRs merging to main
            # would emit packages that misrepresent their ship vehicle — so that is
            # BLOCKED, not READY. An empty/missing PreReleaseVersionLabel is
            # release-only/stable in Arcade, so only ci.main is acceptable here.
            # StabilizePackageVersion defaults false when omitted; only explicit
            # true/non-false is bad.
            $mainLabelOk     = ($vpMain.PreReleaseVersionLabel -eq 'ci.main')
            $mainStabilizeOk = [string]::IsNullOrEmpty($vpMain.StabilizePackageVersion) -or ($vpMain.StabilizePackageVersion -eq 'false')
            $mainMainlineOk  = $mainLabelOk -and $mainStabilizeOk

            if (($mainPastMajor -or $mainBumpedThisCycle) -and (-not $mainMainlineOk)) {
                # The version state (past-major OR patch bumped to the next cycle)
                # says "no bump needed", but main is misconfigured as a servicing/
                # stable build. Gate BOTH READY states on the mainline settings: a
                # dev branch emitting servicing/stable packages misrepresents its
                # ship vehicle regardless of its version number.
                $mainOffenders = @()
                if (-not $mainLabelOk)     { $mainOffenders += "``PreReleaseVersionLabel=$($vpMain.PreReleaseVersionLabel)`` (expected ``ci.main``)" }
                if (-not $mainStabilizeOk) { $mainOffenders += "``StabilizePackageVersion=$($vpMain.StabilizePackageVersion)`` (expected ``false``)" }
                $checks += New-ReadinessCheck -Area $mainArea -Status 'BLOCKED' `
                    -Details "``$mainRef`` reports ``$($vpMain.FullVersion)`` with $($mainOffenders -join ' and ') — main's version is already clear of the SR$targetSr cycle (no PatchVersion bump needed), but its mainline settings are configured for a stable/servicing build, not dev main. PRs merging to ``$($Ctx.mainBranch)`` would emit packages that misrepresent their ship vehicle." `
                    -NextAction "On ``$($Ctx.mainBranch)`` restore the dev-main settings in ``eng/Versions.props``: set ``PreReleaseVersionLabel=ci.main`` and ``StabilizePackageVersion=false``. Only the SR branch flips to ``servicing``/``true``; main must stay on ci.main/false throughout SR$targetSr stabilization."
            } elseif ($mainPastMajor) {
                $checks += New-ReadinessCheck -Area $mainArea -Status 'READY' `
                    -Details "``$mainRef`` reports ``$($vpMain.FullVersion)`` — main has moved past the $major.$minor train entirely (no bump needed for SR$targetSr stabilization)." `
                    -NextAction "No bump needed."
            } elseif ($mainBumpedThisCycle) {
                $checks += New-ReadinessCheck -Area $mainArea -Status 'READY' `
                    -Details "``$mainRef`` reports ``$($vpMain.FullVersion)`` — main is at or past ``$major.$minor.$expectedNextPatchPrefix`` so PRs merging during SR$targetSr stabilization target SR$nextSr correctly." `
                    -NextAction "No bump needed."
            } else {
                $mainBumpTitle = "Update PatchVersion from $($vpMain.Patch) to $expectedNextPatchPrefix"
                # Same cycle → a PatchVersion bump is required. But if main is
                # ALSO misconfigured for a servicing/stable build (rare: same
                # cycle AND mainline settings flipped), the bump PR must ADDITIONALLY
                # restore the dev-main settings — telling the captain to keep them
                # "unchanged" would leave main emitting servicing/stable packages.
                if ($mainMainlineOk) {
                    $mainlineKeepClause = "Keep ``SdkBandVersion``, ``PreReleaseVersionLabel=ci.main``, and ``StabilizePackageVersion=false`` unchanged"
                } else {
                    $mainFixNeeded = @()
                    if (-not $mainLabelOk)     { $mainFixNeeded += "``PreReleaseVersionLabel`` (currently ``$($vpMain.PreReleaseVersionLabel)``)" }
                    if (-not $mainStabilizeOk) { $mainFixNeeded += "``StabilizePackageVersion`` (currently ``$($vpMain.StabilizePackageVersion)``)" }
                    $mainlineKeepClause = "Keep ``SdkBandVersion`` unchanged, and in the SAME PR restore the dev-main mainline settings that are currently misconfigured for a stable/servicing build ($($mainFixNeeded -join ' and ')): set ``PreReleaseVersionLabel=ci.main`` and ``StabilizePackageVersion=false`` — leaving them as-is would keep ``$($Ctx.mainBranch)`` emitting servicing/stable packages"
                }
                $mainBumpDetails = if ($isShipped) {
                    "``$mainRef`` still reports ``$($vpMain.FullVersion)`` even though SR$targetSr already shipped. New PR builds can continue claiming the shipped version until main advances to SR$nextSr."
                } else {
                    "``$mainRef`` reports ``$($vpMain.FullVersion)`` — same cycle as the SR being shipped. Once SR$targetSr tags, every PR currently merging to main as ``$($vpMain.FullVersion)`` would falsely claim to ship in SR$targetSr."
                }
                $mainBumpNextAction = if ($isShipped) {
                    "Open the focused ``$mainBumpTitle`` PR against ``$($Ctx.mainBranch)`` immediately. Change only ``<PatchVersion>$($vpMain.Patch)</PatchVersion>`` to ``<PatchVersion>$expectedNextPatchPrefix</PatchVersion>``; $mainlineKeepClause. This is post-ship containment, not a pre-ship gate."
                } else {
                    "Open a focused PR targeting ``$($Ctx.mainBranch)`` titled ``$mainBumpTitle``. In ``eng/Versions.props``, change only ``<PatchVersion>$($vpMain.Patch)</PatchVersion>`` to ``<PatchVersion>$expectedNextPatchPrefix</PatchVersion>``. $mainlineKeepClause; do not combine this main bump with the SR servicing-flip PR. This is the one-line pattern used by #35433 and #35879. Merge it before shipping SR$targetSr."
                }
                $checks += New-ReadinessCheck -Area $mainArea -Status 'BLOCKED' `
                    -Details $mainBumpDetails `
                    -NextAction $mainBumpNextAction
            }
        }
    }

    # === Bug template version listing ===
    # Issue templates live on the default branch (main) — they're global per repo.
    $templateRef = "origin/$($Ctx.mainBranch)"
    $templateVersions = @(Get-BugTemplateVersions -Ref $templateRef)
    # Before ship, any entry in the target SR decade proves the dropdown is
    # prepared. After ship, users must be able to select the exact immutable
    # published version; a sibling patch in the same decade is not sufficient.
    $shippedTemplateVersion = if ($isShipped) {
        [string](Get-MetadataValue -Container $Ctx -Name 'shippedTagVersion')
    } else { $null }
    $matchPattern = "^$major\.$minor\.(\d+)"
    $matchingEntries = @(
        if ($isShipped -and $shippedTemplateVersion) {
            $exactPattern = "^\s*$([regex]::Escape($shippedTemplateVersion))(?:\s|$)"
            $templateVersions | Where-Object { $_ -match $exactPattern }
        } else {
            $templateVersions | Where-Object {
            if ($_ -match $matchPattern) {
                $p = [int]$Matches[1]
                return ($p -ge $expectedPatchPrefix -and $p -lt ($expectedPatchPrefix + 10))
            }
            return $false
            }
        }
    )

    $bugArea = "Bug template lists SR$targetSr version"
    if ($templateVersions.Count -eq 0) {
        $checks += New-ReadinessCheck -Area $bugArea -Status 'UNKNOWN' `
            -Details "Could not read .github/ISSUE_TEMPLATE/bug-report.yml from ``$templateRef`` or the version-with-bug dropdown is empty." `
            -NextAction "Inspect the bug template manually."
    } elseif ($isShipped -and -not $shippedTemplateVersion) {
        $checks += New-ReadinessCheck -Area $bugArea -Status 'UNKNOWN' `
            -Details "The shipped tag version is unavailable, so the exact version-with-bug entry cannot be verified on ``$templateRef``." `
            -NextAction "Resolve the immutable shipped tag and verify its exact version is listed in .github/ISSUE_TEMPLATE/bug-report.yml."
    } elseif ($matchingEntries.Count -gt 0) {
        $first = $matchingEntries[0]
        $checks += New-ReadinessCheck -Area $bugArea -Status 'READY' `
            -Details $(if ($isShipped) {
                "Bug template lists the exact shipped version ``$first``."
            } else {
                "Bug template lists ``$first`` (and $($matchingEntries.Count - 1) other SR$targetSr entries)."
            }) `
            -NextAction "No template update needed."
    } else {
        $sample = ($templateVersions | Select-Object -First 3) -join ', '
        # CLEANUP, not BLOCKED — missing the dropdown entry doesn't prevent the
        # build from shipping; it just means the bug-report form won't list this
        # version for the first few days. Surface prominently so it gets done,
        # but don't escalate the verdict to Not Ready.
        $checks += New-ReadinessCheck -Area $bugArea -Status 'CLEANUP' `
            -Details $(if ($isShipped) {
                "Exact shipped version ``$shippedTemplateVersion`` is missing from the version-with-bug dropdown on ``$templateRef``. Same-decade entries do not let users select this shipped patch. Top entries: $sample."
            } else {
                "No entry matching ``$major.$minor.[$expectedPatchPrefix..$($expectedPatchPrefix + 9)]`` found in version-with-bug dropdown on ``$templateRef``. Top entries: $sample."
            }) `
            -NextAction $(if ($isShipped) {
                "Add ``$shippedTemplateVersion`` to .github/ISSUE_TEMPLATE/bug-report.yml so users can file against the exact shipped patch."
            } else {
                "Add the SR$targetSr version (e.g. ``$major.$minor.$expectedPatchPrefix``) to .github/ISSUE_TEMPLATE/bug-report.yml — can land before or shortly after ship."
            })
    }

    if ($isShipped) {
        $liveVersion = [string](Get-MetadataValue -Container $Ctx -Name 'liveBranchVersion')
        $publishedVersion = [string](Get-MetadataValue -Container $Ctx -Name 'shippedTagVersion')
        $hasPostTagCommits = [bool](Get-MetadataValue -Container $Ctx -Name 'hotfixHasPostTagCommits' -Default $false)
        $hotfixInProgress = [bool](Get-MetadataValue -Container $Ctx -Name 'hotfixInProgress' `
            -Default ($liveVersion -and $publishedVersion -and $liveVersion -ne $publishedVersion))
        if ($hotfixInProgress) {
            $hotfixEvidence = if ($hasPostTagCommits -and
                ([string]::IsNullOrEmpty($liveVersion) -or $liveVersion -eq $publishedVersion)) {
                if ($liveVersion) {
                    "The live SR branch has commits after the published ``$publishedVersion`` tag even though ``eng/Versions.props`` still reports that version."
                } else {
                    "The live SR branch has commits after the published ``$publishedVersion`` tag; the live ``eng/Versions.props`` version could not be determined."
                }
            } else {
                "The live SR branch reports ``$liveVersion``, while the latest published tag for this SR cycle is ``$publishedVersion``."
            }
            $checks += New-ReadinessCheck -Area "Unpublished hotfix branch state" -Status 'WATCH' `
                -Details "$hotfixEvidence The shipped tracker remains anchored to the published tag until a newer stable tag exists." `
                -NextAction "Treat the post-tag branch state as an in-progress hotfix candidate. Bump the target version if needed, complete build/sign/validation, and publish its stable tag before advancing the shipped-content anchor."
        }
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
        $exitCode = $LASTEXITCODE
        if ($exitCode -ne 0) {
            # darc's non-zero exit is Constants.ErrorCode (42) in almost every case —
            # GetBuildOperation/GetAssetOperation return it for no matches, invalid
            # arguments, auth failures, and unhandled exceptions alike. It is therefore
            # NOT a reliable "no match" signal, so we deliberately do not derive a
            # NoMatch flag from it; the caller surfaces any failure as UNKNOWN rather
            # than a reassuring "no build yet". A genuine empty-but-successful response
            # is exit 0 with empty output (handled below).
            return [PSCustomObject]@{ Success = $false; Data = @(); ExitCode = $exitCode }
        }
        $joined = ($jsonOutput | Out-String)
        if ([string]::IsNullOrWhiteSpace($joined)) {
            return [PSCustomObject]@{ Success = $true; Data = @(); ExitCode = 0 }
        }
        $parsed = $joined | ConvertFrom-Json -ErrorAction Stop
        if ($null -eq $parsed) {
            return [PSCustomObject]@{ Success = $true; Data = @(); ExitCode = 0 }
        }
        return [PSCustomObject]@{ Success = $true; Data = @($parsed); ExitCode = 0 }
    } catch {
        return [PSCustomObject]@{ Success = $false; Data = @(); ExitCode = $null }
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
        # darc is also required to tell whether the build is promoted (and thus
        # whether its per-build validation feed exists for the ship Assessment).
        # Emit the feed row here too so the scheduled/CI run — where darc is not
        # installed — still surfaces the Assessment-feed guidance instead of
        # silently dropping it.
        $feedSha8Unknown = $Ctx.srHeadSha.Substring(0, [Math]::Min(8, $Ctx.srHeadSha.Length))
        $feedUrlUnknown = "https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-maui-$feedSha8Unknown/nuget/v3/index.json"
        $checks += New-ReadinessCheck -Area 'Ship Assessment validation feed' -Status 'UNKNOWN' `
            -Details "``darc`` CLI not available — cannot confirm whether SR HEAD's build is promoted or whether its per-build validation feed exists to link in the ship Assessment. If promoted, the feed will be ``$feedUrlUnknown``." `
            -NextAction "Locally, once the build is confirmed: ``darc get-asset --name Microsoft.Maui.Controls --build <id>`` to get the NugetFeed URL, then link it in the ship Assessment."
    } else {
        $builds = Invoke-DarcJson -DarcArgs @('get-build', '--repo', $repoUrl, '--commit', $Ctx.srHeadSha)
        if (-not $builds.Success) {
            # darc failed. Its exit code is the generic Constants.ErrorCode (42) for
            # no-match, auth, network, and BAR outages alike, so we cannot safely
            # downgrade this to a reassuring "no build yet" WATCH — report UNKNOWN and
            # tell the reader it may be transient. A genuine empty-but-successful darc
            # response (exit 0, no builds) is the WATCH case handled further below.
            $buildExit = Get-AzdoProp $builds 'ExitCode'
            $exitInfo = if ($null -ne $buildExit) { " (darc exit $buildExit)" } else { "" }
            $checks += New-ReadinessCheck -Area $buildArea -Status 'UNKNOWN' `
                -Details "``darc get-build`` did not return a usable result for SR HEAD ``$headShort``$exitInfo. darc exit 42 is a generic error code (no build yet, auth failure, or a network/BAR outage), so this is UNKNOWN rather than a definitive no-build; it may be transient while CI/BAR publishing is still running." `
                -NextAction "Run locally: ``darc get-build --repo $repoUrl --commit $($Ctx.srHeadSha)``. If CI is still in-flight, re-run the readiness report shortly; if it persists after CI is green, check darc auth and BAR publishing for the SR build."
            $feedSha8Fail = $Ctx.srHeadSha.Substring(0, [Math]::Min(8, $Ctx.srHeadSha.Length))
            $feedUrlFail = "https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-maui-$feedSha8Fail/nuget/v3/index.json"
            $checks += New-ReadinessCheck -Area 'Ship Assessment validation feed' -Status 'UNKNOWN' `
                -Details "``darc get-build`` failed for SR HEAD ``$headShort`` — cannot confirm promotion or whether the per-build validation feed exists for the ship Assessment. If promoted, the feed will be ``$feedUrlFail``." `
                -NextAction "Re-run ``darc get-build`` locally; once the build is confirmed, ``darc get-asset --name Microsoft.Maui.Controls --build <id>`` gives the NugetFeed URL to link in the ship Assessment."
        } elseif ($builds.Data.Count -eq 0) {
            $checks += New-ReadinessCheck -Area $buildArea -Status 'WATCH' `
                -Details "No BAR build found for SR HEAD ``$headShort``. May be normal if CI is still running, OR a symptom of the default-channel mapping being absent (see prior check)." `
                -NextAction "Wait for CI to complete on SR HEAD; re-run readiness report. If mapping is also missing (above), fix that first."
        } else {
            # darc get-build --commit is branch-agnostic: right after the SR is
            # cut from main both branches share the SR HEAD SHA, so a promoted
            # *main* build for the same commit can otherwise be picked and make
            # the SR look ready. Keep only builds actually produced on the SR
            # branch (matched across the darc/BAR branch field names). A build
            # carrying no branch metadata at all is left in rather than dropped.
            $srBranchBuilds = @($builds.Data | Where-Object {
                $branchNames = @()
                foreach ($prop in 'branch', 'gitHubBranch', 'githubBranch', 'azureDevOpsBranch') {
                    $bv = Get-AzdoProp $_ $prop
                    if ($bv) { $branchNames += (([string]$bv) -replace '^refs/heads/', '') }
                }
                ($branchNames.Count -eq 0) -or ($branchNames -contains $Ctx.srBranch)
            })
            if ($srBranchBuilds.Count -eq 0) {
                $checks += New-ReadinessCheck -Area $buildArea -Status 'WATCH' `
                    -Details "BAR has build(s) for SR HEAD ``$headShort`` but none produced on ``$($Ctx.srBranch)`` — a same-commit build on another branch (e.g. ``$($Ctx.mainBranch)`` right after the branch cut) does not count as the SR's own build." `
                    -NextAction "Wait for CI to complete on ``$($Ctx.srBranch)`` at SR HEAD, then re-run the readiness report."
                return $checks
            }
            # Sort by BAR build id (monotonic, locale-independent) to pick the latest.
            $latest = @($srBranchBuilds | Sort-Object id -Descending)[0]
            # Filter null/empty channel entries: @($null).Count is 1, which would
            # otherwise false-mark a build with a missing/null `channels` property as
            # promoted and emit a bogus READY Assessment-feed row. Reuse the filtered
            # list for the join so display and promotion state stay consistent.
            $realChans = @((Get-AzdoProp $latest 'channels') | Where-Object { $_ })
            $hasChans = $realChans.Count -gt 0
            $chans = if ($hasChans) { ($realChans -join ', ') } else { '_none_' }
            $buildLink = if ($latest.buildLink) { " ([build $($latest.id)]($($latest.buildLink)))" } else { " (build $($latest.id))" }
            $checks += New-ReadinessCheck -Area $buildArea -Status 'READY' `
                -Details "Build **$($latest.buildNumber)**$buildLink for SR HEAD ``$headShort`` is in BAR; channels: $chans." `
                -NextAction "No action needed."

            # === Check 3: per-build validation feed for the ship Assessment ===
            # The DevDiv ship "Assessment" work item MUST link the per-build
            # darc-pub NuGet feed so CSI/customers can validate the exact
            # candidate packages. That feed is generated ONLY when the build is
            # promoted to a channel (which requires the default-channel mapping
            # in Check 1). No promotion => no feed => the Assessment gets created
            # without a validation feed (the exact gap that shipped an incomplete
            # SR9 assessment). Feed name is darc-pub-dotnet-maui-<sha8>, sha8 =
            # the build's commit short SHA (falls back to SR HEAD).
            $feedArea = "Ship Assessment validation feed"
            $commitProp = $latest.PSObject.Properties['commit']
            $buildCommit = if ($commitProp -and $commitProp.Value) { [string]$commitProp.Value } else { [string]$Ctx.srHeadSha }
            $buildSha8 = $buildCommit.Substring(0, [Math]::Min(8, $buildCommit.Length))
            $feedUrl = "https://pkgs.dev.azure.com/dnceng/public/_packaging/darc-pub-dotnet-maui-$buildSha8/nuget/v3/index.json"
            if ($hasChans) {
                $asset = Invoke-DarcJson -DarcArgs @('get-asset', '--name', 'Microsoft.Maui.Controls', '--build', "$($latest.id)")
                $nugetFeed = $null
                # Initialize BEFORE the success check so the WATCH branch below can
                # ALWAYS read them. A failed `darc get-asset` (auth/network/no asset)
                # leaves $asset.Success false and skips the success block; under
                # Set-StrictMode -Version Latest an unset $feedCandidates or
                # $expectedFeedToken in that branch would THROW and abort the ENTIRE
                # readiness report instead of degrading this one feed check.
                $feedCandidates = @()
                $expectedFeedToken = "darc-pub-dotnet-maui-$buildSha8"
                $assetLookupOk = [bool]$asset.Success
                if ($assetLookupOk) {
                    # `darc get-asset --output-format json` projects each asset's
                    # locations to a flat array of URL STRINGS
                    # (GetAssetOperation: `locations = ...Select(l => l.Location)`).
                    # It does NOT emit `{ type, location }` objects or a top-level
                    # `NugetFeed` property, so collect the NuGet v3 feed URLs directly.
                    foreach ($a in @($asset.Data)) {
                        foreach ($loc in @((Get-AzdoProp $a 'locations'))) {
                            $locStr = [string]$loc
                            if ($locStr -match '/nuget/v\d+/index\.json') {
                                $feedCandidates += $locStr
                            }
                        }
                    }
                    # ONLY the exact per-build darc-pub validation feed for THIS
                    # build's SHA proves CSI/customers can validate the precise
                    # candidate packages. BAR asset locations routinely ALSO carry
                    # shared/durable/internal feeds (transport, dotnet-eng,
                    # darc-int-*, etc.) and darc-pub feeds for OTHER builds' SHAs —
                    # none of those prove per-build validation (they may mix builds
                    # or require auth). A loose "any NuGet v3 feed" or substring
                    # "darc-pub" match would mark READY and tell the captain to link
                    # a feed that can't validate the exact packages. Gate strictly on
                    # the already-computed expected feed name
                    # `darc-pub-dotnet-maui-<buildSha8>` (SHA-exact; -match is
                    # case-insensitive). Guard indexing for Set-StrictMode -Version Latest.
                    $confirmedFeeds = @($feedCandidates | Where-Object { $_ -match [regex]::Escape($expectedFeedToken) })
                    if ($confirmedFeeds.Count -gt 0) {
                        $nugetFeed = $confirmedFeeds[0]
                    }
                }
                if ($nugetFeed) {
                    $checks += New-ReadinessCheck -Area $feedArea -Status 'READY' `
                        -Details "Build **$($latest.buildNumber)** is promoted ($chans) and ``darc get-asset`` confirms the per-build validation feed ``$nugetFeed`` (matches the expected ``$expectedFeedToken``). Link this feed in the ship Assessment (DevDiv 'Assessment' work item) so CSI/customers can validate the exact candidate packages." `
                        -NextAction "Add the confirmed per-build NugetFeed URL to the Assessment."
                } else {
                    if (-not $assetLookupOk) {
                        $otherFeedNote = " ``darc get-asset --name Microsoft.Maui.Controls --build $($latest.id)`` did not return a usable result (darc auth/network failure, or the asset is not published yet), so the per-build feed could not be confirmed."
                    } elseif ($feedCandidates.Count -gt 0) {
                        $otherFeedNote = " ``darc get-asset`` returned $($feedCandidates.Count) other NuGet feed location(s) (e.g. ``$($feedCandidates[0])``) — shared/durable or wrong-SHA feeds that may mix builds or require auth, so they do NOT prove validation of this exact candidate and are not linked."
                    } else {
                        $otherFeedNote = " ``darc get-asset`` returned no NuGet feed location for this build."
                    }
                    $checks += New-ReadinessCheck -Area $feedArea -Status 'WATCH' `
                        -Details "Build **$($latest.buildNumber)** is promoted ($chans), but the expected per-build validation feed ``$expectedFeedToken`` was not confirmed among ``darc get-asset --name Microsoft.Maui.Controls --build $($latest.id)`` locations.$otherFeedNote Do not link a substitute endpoint until BAR shows the exact per-build feed. Expected feed, once published, is ``$feedUrl``." `
                        -NextAction "Re-run ``darc get-asset --name Microsoft.Maui.Controls --build $($latest.id)`` and add the returned per-build ``$expectedFeedToken`` NugetFeed URL to the Assessment once it appears."
                }
            } else {
                $checks += New-ReadinessCheck -Area $feedArea -Status 'WATCH' `
                    -Details "Build **$($latest.buildNumber)** for SR HEAD is NOT promoted to any channel → its per-build darc-pub feed is not generated, so the ship Assessment has no validation feed to link (this is what left the SR9 assessment incomplete). Once promoted, the feed will be ``$feedUrl``." `
                    -NextAction "Ensure the default-channel mapping (Check 1) exists, then promote the build to ``$expectedChannel`` (release-eng). Verify with ``darc get-asset --name Microsoft.Maui.Controls --build $($latest.id)`` and add the resulting NugetFeed URL to the Assessment."
            }
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

# .NET ships preview1..preview7, then rc1, rc2, then GA. There is NO preview8 —
# preview7 is the FINAL preview of a major, and the milestone that follows it is
# `.NET <major>.0-rc1`. Verified against dotnet/maui's own history:
#   .NET 9  → 9.0.0-preview.7.24407.4  → 9.0.0-rc.1.24453.9  → 9.0.0-rc.2.24503.2
#   .NET 10 → 10.0.0-preview.7.25406.3 → 10.0.0-rc.1.25424.2 → 10.0.0-rc.2.25504.7
# and the matching milestones `.NET 10.0-preview7` → `.NET 10.0-rc1` → `.NET 10.0-rc2`.
# (.NET 5 shipped 8 previews; the 7-preview cadence has held for every major since
# .NET 6. If that ever changes, this constant is the single place to update.)
$script:FinalPreviewNumber = 7
$script:RcCountPerMajor    = 2

function Get-PreviewTrainMilestoneTitle {
    <#
    .SYNOPSIS
        PURE. Maps a 1-based preview-train ordinal to its GitHub milestone title,
        honouring the preview→rc transition.
    .DESCRIPTION
        The pre-release train for a major is a single ordered sequence, so
        "the cycle after preview7" is rc1 — not the non-existent preview8.
        Naively incrementing the preview number is the bug this replaces: it
        told release captains to create a `.NET <major>.0-preview8` milestone
        that .NET never ships.

            ordinal 1..7  → ".NET <major>.0-preview<ordinal>"
            ordinal 8     → ".NET <major>.0-rc1"
            ordinal 9     → ".NET <major>.0-rc2"
            ordinal 10+   → $null   (GA — no further pre-release milestone)

        Returning $null for post-rc2 ordinals lets callers skip the roll-forward
        check rather than inventing an "rc3" that will never exist.
    .OUTPUTS
        [string] milestone title, or $null when the ordinal runs past rc2.
    #>
    param(
        [Parameter(Mandatory)][int]$Major,
        [Parameter(Mandatory)][int]$Ordinal
    )

    if ($Ordinal -lt 1) { return $null }
    if ($Ordinal -le $script:FinalPreviewNumber) {
        return ".NET $Major.0-preview$Ordinal"
    }

    $rcNumber = $Ordinal - $script:FinalPreviewNumber
    if ($rcNumber -le $script:RcCountPerMajor) {
        return ".NET $Major.0-rc$rcNumber"
    }
    return $null
}

function Get-PastDueOpenMilestones {
    <#
    .SYNOPSIS
        Open milestones whose `due_on` lapsed before $Cutoff, oldest first.
    .DESCRIPTION
        The single definition of "past due" shared by Check 3 (already-shipped
        debt) and Check 3b (slipped next-cycle target). Those two checks partition
        the same milestone set on the next-cycle title — one excludes it, the other
        selects it — so they MUST agree on what "past due" means. If the state /
        due_on / cutoff test drifted between them, a milestone could either
        double-report or fall through both checks unreported, which is exactly the
        bug class Check 3b was added to fix. Keeping the test here makes that
        agreement structural rather than a copy-paste invariant nothing enforces.

        `-Stable` is required, not cosmetic. PowerShell's default `Sort-Object` is
        NOT stable — per the cmdlet docs, equal-key inputs are only delivered in
        received order when `-Top`, `-Bottom`, or `-Stable` is used. In practice the
        default stays ordered below .NET's ~16-element insertion-sort threshold and
        starts permuting ties above it, so a bug here would stay invisible in small
        fixtures and only appear on a repo with many past-due milestones. Because
        sorting now happens BEFORE each caller's discriminator instead of after,
        a stable sort is what makes the two orderings equivalent: filtering a stably
        sorted list preserves relative order, so each caller sees the same sequence
        it would have produced by filtering first. It also makes tie order
        deterministic, which the previous filter-then-sort code did not guarantee
        either.
    #>
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [array] $Milestones,

        [Parameter(Mandatory)]
        [datetime] $Cutoff
    )

    return @($Milestones | Where-Object {
        $_.state -eq 'open' -and
        $_.due_on -and
        ([datetime]$_.due_on).ToUniversalTime() -lt $Cutoff
    } | Sort-Object -Stable { [datetime]$_.due_on })
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
        # Walk the preview train (preview1..preview7 → rc1 → rc2), NOT a naive
        # preview number increment — .NET has no preview8, so the cycle after
        # preview7 is rc1.
        $currentTrainTitle = Get-PreviewTrainMilestoneTitle -Major $major -Ordinal $cycleNum
        $nextTrainTitle    = Get-PreviewTrainMilestoneTitle -Major $major -Ordinal ($cycleNum + 1)
        if (-not $currentTrainTitle) {
            if ($cycleNum -lt 1) {
                # Nonsensical ordinal (e.g. a `release/<major>.0.1xx-preview0`
                # branch — the `\d+` capture syntactically accepts 0). The
                # pre-release train has no member below 1. This is a MATCHED
                # branch shape with an out-of-range ordinal — a misconfiguration,
                # not the legitimate end of the train — so surface UNKNOWN rather
                # than silently dropping the current-cycle signal, which would let
                # a bad branch name masquerade as "all clear". Distinct from the
                # past-rc2 case just below, where no milestone exists by design and
                # an empty result is correct.
                return @(New-ReadinessCheck -Area "Milestone hygiene (branch shape)" -Status 'UNKNOWN' `
                    -Details "Unrecognized pre-release ordinal ``$cycleNum`` derived from branch ``$branchToParse`` — cannot map it to a preview/rc milestone title." `
                    -NextAction "Verify the branch follows the ``release/<major>.0.<feature>xx-preview<n>`` convention with ``n >= 1``.")
            }
            # Past rc2 — the pre-release train is over and GA milestones don't
            # follow this naming convention. Skip silently rather than guess.
            return @()
        }
        $expectedTitlesCurrent = @($currentTrainTitle)
        # NOTE: assign via a statement, not `= if (...) { @($x) } else { @() }` —
        # an if-expression unrolls a single-element array back to a scalar, which
        # then blows up on `.Count` under `Set-StrictMode -Version Latest`.
        $expectedTitlesNext = @()
        if ($nextTrainTitle) { $expectedTitlesNext = @($nextTrainTitle) }
        $cycleLabel = $currentTrainTitle -replace "^\.NET $major\.0-", ''
    } else {
        # Unknown branch shape — can't derive milestone names. Skip silently.
        return @()
    }

    $milestonesResult = Get-AllMilestones -Repo $Ctx.repo
    if (-not $milestonesResult.Success) {
        return @(New-ReadinessCheck -Area "Milestone hygiene (API failure)" -Status 'UNKNOWN' `
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
    #
    # $expectedTitlesNext is empty when the pre-release train has no successor
    # (i.e. we're on rc2, after which comes GA) — skip rather than invent one.
    if ($expectedTitlesNext.Count -gt 0) {
        $nextMs = @($allMs | Where-Object { $expectedTitlesNext -contains $_.title })
        $nextTitle = $expectedTitlesNext[0]
        if ($nextMs.Count -eq 0) {
            $checks += New-ReadinessCheck -Area "Milestone for next cycle ($nextTitle)" -Status 'CLEANUP' `
                -Details "No milestone matching ``$nextTitle`` exists. Once ``$cycleLabel`` ships, open issues will have nowhere to roll forward to — but ``$cycleLabel`` can ship first." `
                -NextAction "Create the milestone before the next cycle begins: ``gh api repos/$($Ctx.repo)/milestones -f title=""$nextTitle"" -f state=open``"
        }
    }

    # === Check 3: Stale open milestones with past due_on ===
    # Filtered by cycle to avoid cross-train noise: when surveying an SR cycle,
    # flag only stale `.NET <same-major> SR*` milestones; when surveying a preview
    # cycle, flag only stale `.NET <same-major>.0-preview*`. A 7-day grace period
    # after due_on lets the actively-shipping release still appear open without
    # triggering BLOCKED.
    # Also excluded:
    #   - the current cycle (still being prepped)
    #   - the next cycle (the roll-forward target Check 2 may have just told the
    #     captain to create; a slipped next-cycle milestone whose due_on passed is
    #     NOT "already-shipped debt" — flagging it here would contradict Check 2's
    #     "create it" advice. It is instead re-classified by Check 3b below, so the
    #     signal is preserved rather than dropped. Lane-agnostic: this holds for an
    #     SR next-cycle milestone as much as a preview/rc one.)
    #   - "Backlog" (intentional long-running)
    #   - ".NET N Planning" (intentional long-running planning ms)
    #   - milestones without due_on (caller has no schedule, no signal)
    $now = (Get-Date).ToUniversalTime()
    $graceCutoff = $now.AddDays(-7)
    $cycleFilter = if ($srMatch.Success) {
        # Match ".NET <major> SR<n>" and ".NET <major>.0 SR<n>" (and SR<n>.<patch>)
        "^\.NET\s+$major(\.0)?\s+SR\d+(\.\d+)?$"
    } else {
        # Match ".NET <major>.0-preview<n>" AND ".NET <major>.0-rc<n>" — preview
        # and rc are one continuous pre-release train, so a stale rc1 milestone
        # is the same class of housekeeping debt as a stale preview6 one.
        "^\.NET\s+$major\.0-(preview|rc)\d+$"
    }
    # Shared "past due" set — Check 3 and Check 3b select disjoint halves of it.
    $pastDueOpen = Get-PastDueOpenMilestones -Milestones $allMs -Cutoff $graceCutoff

    $staleMs = @($pastDueOpen | Where-Object {
        ($expectedTitlesCurrent -notcontains $_.title) -and
        ($expectedTitlesNext -notcontains $_.title) -and
        ($_.title -match $cycleFilter)
    })

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

    # === Check 3b: Next-cycle milestone exists but has slipped past its due date ===
    # Check 3 deliberately excludes the next-cycle (roll-forward target) milestone
    # from the "already-shipped debt" bucket — calling it that would contradict
    # Check 2's advice to create it. But an EXISTING next-cycle milestone that is
    # well past due must not become invisible: an unbounded slip usually means the
    # schedule moved, or the cycle was skipped and the milestone abandoned. Re-classify
    # it into its own row with accurate wording so the signal is preserved without the
    # misleading "already shipped" framing. Lane-agnostic — fires for an SR next-cycle
    # milestone (e.g. a long-overdue SR9 while surveying SR8) exactly as for a slipped
    # preview/rc one, which is the SR-lane signal the bare Check-3 exclusion had dropped.
    $slippedNext = @($pastDueOpen | Where-Object {
        $expectedTitlesNext -contains $_.title
    })

    if ($slippedNext.Count -gt 0) {
        $slippedList = ($slippedNext | ForEach-Object {
            $dueDate = ([datetime]$_.due_on).ToUniversalTime().ToString('yyyy-MM-dd')
            "[$($_.title)](https://github.com/$($Ctx.repo)/milestone/$($_.number)) (due $dueDate, $($_.open_issues) open)"
        }) -join '; '
        # CLEANUP, not BLOCKED — like stale debt, a slipped roll-forward target is
        # housekeeping, not a blocker on THIS cycle shipping.
        $checks += New-ReadinessCheck -Area "Next-cycle milestone past due ($($slippedNext.Count))" -Status 'CLEANUP' `
            -Details "The next-cycle roll-forward milestone(s) exist but are past due (>7 days) and still open: $slippedList. This is NOT already-shipped debt — it's the target open issues roll forward to after ``$cycleLabel`` ships. A slip usually means the schedule moved, or the cycle was skipped and the milestone was abandoned." `
            -NextAction "If the schedule slipped, update the milestone's ``due_on``; if the cycle was skipped, triage its open issues and close it: ``gh api -X PATCH repos/$($Ctx.repo)/milestones/<number> -f state=closed``"
    }

    return $checks
}

function Get-CandidatePrResolution {
    <#
    .SYNOPSIS
        Discovers the open "Candidate" PR(s) on main — the PR(s) that promote a
        specific main commit as the basis for cutting the next SR — and returns a
        structured resolution consumed by BOTH the ship-readiness check
        (Get-CandidatePrChecks) AND the prominent "Candidate PR" report section.
        Runs the gh query + maintainer spoof-gate exactly ONCE per report so the
        two consumers don't each pay for the network round-trips.
    .DESCRIPTION
        Convention: the Candidate PR has "Candidate" in the title (word boundary,
        case-insensitive) AND is opened by a maintainer (OWNER/MEMBER/COLLABORATOR).
        `gh pr list --json` does NOT expose authorAssociation, so the maintainer
        spoof-gate fetches author_association per title-matched candidate via the
        REST API. Fail closed: an unreadable association excludes the PR (tracked
        separately as `unverifiable`, distinct from a confirmed non-maintainer
        `spoofer`, so a transient blip during a real cut isn't mislabeled).

        The `gh pr list` projection requests the richer fields (createdAt, isDraft,
        mergeable, reviewDecision, state) the prominent section renders — all are
        valid `gh pr list --json` fields. Consumers must treat every field as
        possibly-absent (older stubs / partial data) and degrade gracefully.
    .OUTPUTS
        Hashtable:
          mode         — 'skip' (not candidate mode) | 'query-failed' | 'resolved'
          candidates   — @() of accepted (maintainer-gated) enriched PR objects
          spoofers     — [int] confirmed non-maintainer 'Candidate'-titled PRs excluded
          unverifiable — [int] 'Candidate'-titled PRs whose author-assoc lookup failed
          nextSr       — 'SR9' | $null
          versionBase  — '10.0.90' | $null  (Major.Minor.(targetSr*10))
    #>
    param($Ctx)

    $res = [ordered]@{
        mode = 'skip'; candidates = @(); spoofers = 0; unverifiable = 0
        nextSr = $null; versionBase = $null
    }
    if ($Ctx.mode -ne 'candidate') { return $res }

    # Next-SR label + version base from priorSrBranch (release/MAJOR.MINOR.1xx-srN).
    # Full parse gives the version base (e.g. SR9 → 10.0.90) for the section header;
    # fall back to the loose 'srN$' match (nextSr only) if the branch is oddly shaped.
    if ($Ctx.priorSrBranch -and $Ctx.priorSrBranch -match '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$') {
        $mj = [int]$Matches[1]; $mn = [int]$Matches[2]; $target = [int]$Matches[3] + 1
        $res.nextSr = "SR$target"
        $res.versionBase = "$mj.$mn.$($target * 10)"
    } elseif ($Ctx.priorSrBranch -and $Ctx.priorSrBranch -match 'sr(\d+)$') {
        $res.nextSr = "SR$([int]$Matches[1] + 1)"
    }

    # One gh call: up to 100 open PRs on main. The Candidate PR is opened on main
    # (not the SR branch, which may not exist yet in candidate mode).
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Ctx.repo, '--state', 'open',
                       '--base', $Ctx.mainBranch, '--limit', '100',
                       '--json', 'number,title,author,createdAt,updatedAt,isDraft,mergeable,reviewDecision,state,url')
    if ($null -eq $raw) { $res.mode = 'query-failed'; return $res }

    $mainPrs = @()
    $parsed = $raw | ConvertFrom-Json -ErrorAction SilentlyContinue
    if ($parsed) { $mainPrs = @($parsed) }

    # Word-boundary match so "CandidateView" doesn't spoof.
    $titleMatches = @($mainPrs | Where-Object { $_.title -match '(?i)\bcandidate\b' })

    # Author gating (see .DESCRIPTION). -Quiet so a transient lookup miss doesn't
    # embed a raw `gh ... exited` warning in the tracker body.
    $maintainerAssociations = @('OWNER', 'MEMBER', 'COLLABORATOR')
    foreach ($pr in $titleMatches) {
        $assocRaw = Invoke-Gh @('api', "repos/$($Ctx.repo)/pulls/$($pr.number)",
                                '--jq', '.author_association') -Quiet
        $assoc = if ($assocRaw) { "$assocRaw".Trim() } else { $null }
        if (-not $assoc) {
            $res.unverifiable++
        } elseif ($maintainerAssociations -contains $assoc) {
            $res.candidates += $pr
        } else {
            $res.spoofers++
        }
    }
    $res.mode = 'resolved'
    return $res
}

function Get-CandidatePrAge {
    <#
    .SYNOPSIS
        Returns the stable age decision used by both Candidate rendering and hashing.
    #>
    param($Candidate, $Now)

    $created = ConvertTo-Utc -Value (Get-MetadataValue -Container $Candidate -Name 'createdAt')
    $nowUtc = ConvertTo-Utc -Value $Now
    if (-not $created -or -not $nowUtc) {
        return [PSCustomObject]@{ AgeDays = $null; Bucket = 'unknown' }
    }

    $ageDays = [int][Math]::Floor(($nowUtc - $created).TotalDays)
    return [PSCustomObject]@{
        AgeDays = $ageDays
        Bucket  = if ($ageDays -ge 14) { 'stale' } else { 'fresh' }
    }
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
    param($Ctx, $Resolution, [switch]$SkipChecks)

    if ($SkipChecks) { return ,@() }
    if ($Ctx.mode -ne 'candidate') { return ,@() }

    # Discover the candidate PR(s) once. Callers that also render the prominent
    # "Candidate PR" report section pass the shared -Resolution so the gh query
    # + maintainer spoof-gate runs a SINGLE time per report; direct callers
    # (e.g. unit tests) omit it and this recomputes on demand.
    if (-not $Resolution) { $Resolution = Get-CandidatePrResolution -Ctx $Ctx }

    $repoUrl = "https://github.com/$($Ctx.repo)"

    $nextSr = $Resolution.nextSr
    $area = if ($nextSr) {
        "Candidate PR for next SR cut ($nextSr)"
    } else {
        "Candidate PR for next SR cut"
    }

    # gh query failed → surface as WATCH (a missing signal, not a silent READY).
    if ($Resolution.mode -eq 'query-failed') {
        return ,@(New-ReadinessCheck -Area $area -Status 'WATCH' `
            -Details "Could not query open PRs on ``$($Ctx.mainBranch)`` (``gh pr list`` exited non-zero). Cut readiness cannot be evaluated until the query succeeds." `
            -NextAction "Verify ``gh auth status`` and rerun. If gh is unavailable in this environment, check the Candidate PR manually.")
    }

    # Accepted (maintainer-gated) candidates + exclusion counts, from the shared
    # resolution computed by Get-CandidatePrResolution (query + spoof-gate).
    $candidates = @($Resolution.candidates)
    $spoofers = $Resolution.spoofers
    $unverifiable = $Resolution.unverifiable

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
          [switch]$InheritFromPriorSr, [switch]$Shipped)

    if ($InheritFromPriorSr -and -not $Candidate) {
        throw "-InheritFromPriorSr is only valid with -Candidate (it models the SR cut-then-merge workflow)."
    }

    if ($Shipped -and $Candidate) {
        throw "-Shipped and -Candidate are mutually exclusive: -Shipped surveys an existing (already-tagged) SR branch; -Candidate pre-flights main as the next SR."
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
    $priorSrRef = $null

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
    elseif ($Shipped) {
        # Survey the same SR branch as in-flight mode, but apply post-ship verdict,
        # carry-forward, and hotfix-vs-next-SR guidance semantics.
        $mode = 'shipped'
        Write-Host "Shipped mode: surveying already-tagged SR branch $effectiveSrRef with post-ship semantics" -ForegroundColor Cyan
    }

    # Main-side revert detection needs a bounded release-window baseline, but the
    # CURRENT SR tip is not a safe bound: candidate/inflight catch-up history can
    # make a fix and its later main revert common ancestors of both refs, hiding
    # the revert from `main ^srRef`. Use the prior SR (or GA for SR1) so reverts
    # introduced during this release cycle remain visible even after the current
    # SR inherits them. Candidate mode already names the prior SR explicitly.
    $mainRevertBaselineRef = $null
    if ($Candidate) {
        $mainRevertBaselineRef = $priorSrRef
    } elseif ($SrBranch -match '^release/(\d+)\.(\d+)\.(\d+)xx-sr(\d+)$') {
        $baselineMajor = [int]$Matches[1]
        $baselineMinor = [int]$Matches[2]
        $baselineBand = [string]$Matches[3]
        $currentSrNumber = [int]$Matches[4]
        $baselineCandidate = if ($currentSrNumber -gt 1) {
            "origin/release/$baselineMajor.$baselineMinor.${baselineBand}xx-sr$($currentSrNumber - 1)"
        } else {
            "$baselineMajor.$baselineMinor.0"
        }
        if (Invoke-Git "rev-parse $baselineCandidate") {
            $mainRevertBaselineRef = $baselineCandidate
        } else {
            Write-Warn "Main-revert release baseline '$baselineCandidate' was not found; falling back to an unbounded correctness-first revert scan."
        }
    } else {
        Write-Warn "Could not derive a main-revert release baseline from '$SrBranch'; falling back to an unbounded correctness-first revert scan."
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
        mainRevertBaselineRef = $mainRevertBaselineRef
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
    $Subject = $Subject -replace '[\u201C\u201D]', '"'
    $Subject = [regex]::Replace($Subject, '^(?i)(?:\[(?!Revert\])[^]]+\]\s*)+', '')
    # Common hand-authored forms without GitHub's quoted-title convention.
    $m = [regex]::Match($Subject, '(?i)^(?:This\s+)?Revert(?:s|ing)?\s+(?:PR\s+)?#(\d+)(?![\p{L}\p{N}_])')
    if ($m.Success) { return (ConvertTo-PrNumber -Value $m.Groups[1].Value) }
    $m = [regex]::Match($Subject, '(?i)^Backing\s+out\s+(?:the\s+)?fix\s+(?:for\s+)?#(\d+)(?![\p{L}\p{N}_])')
    if ($m.Success) { return (ConvertTo-PrNumber -Value $m.Groups[1].Value) }
    $m = [regex]::Match($Subject, '(?i)^(?:This\s+)?Revert(?:s|ed|ing)?\b[\s:–—-]*(?:(?:of|for)\s+)?(?:the\s+)?(?:(?:broken\s+)?(?:change|fix)\s+(?:for\s+)?)?(?:PR\s+)?#(\d+)(?![\p{L}\p{N}_])')
    if ($m.Success) { return (ConvertTo-PrNumber -Value $m.Groups[1].Value) }
    if ($Subject -match '(?i)^\[Revert\]') {
        $bareRefs = [regex]::Matches($Subject, '(?<!\()#(\d+)(?![\p{L}\p{N}_]|\))')
        if ($bareRefs.Count -eq 1) {
            return (ConvertTo-PrNumber -Value $bareRefs[0].Groups[1].Value)
        }
    }
    if ($Subject -match '(?i)^Backing\s+out\b') {
        $bareRefs = [regex]::Matches($Subject, '(?<!\()#(\d+)(?![\p{L}\p{N}_]|\))')
        if ($bareRefs.Count -eq 1) {
            return (ConvertTo-PrNumber -Value $bareRefs[0].Groups[1].Value)
        }
    }
    # Explicit "Revert PR #NNNN" form.
    $m = [regex]::Match($Subject, '(?i)Revert\s+PR\s+#(\d+)(?![\p{L}\p{N}_])')
    if ($m.Success) { return (ConvertTo-PrNumber -Value $m.Groups[1].Value) }
    # Standard GitHub revert: the (#N) INSIDE the quoted original title, e.g.
    # Revert "Original title (#1234)" (#5678). Greedy .* anchored to the closing
    # quote captures the original PR (1234): it tolerates internal quotes in the
    # title (the old [^"]* halted at the first inner quote and returned null) and,
    # because the trailing revert PR is NOT followed by a quote, never reaches it.
    # Case-insensitive to also match hand-typed lowercase 'revert "..."' subjects.
    $m = [regex]::Match($Subject, '(?i)Revert\s+".*\(#(\d+)(?![\p{L}\p{N}_])\)"')
    if ($m.Success) { return (ConvertTo-PrNumber -Value $m.Groups[1].Value) }
    # Manual/hand-authored revert form (no GitHub quotes, and the body carries no
    # "This reverts commit <sha>" line, so the SHA-override in the caller can't
    # recover it either): "Revert - <original title> #<reverted> (#<revertPR>)".
    # Real example: `Revert - Fix Android stale ContainerView root leak #35372 (#36152)`
    # → 35372 is the reverted fix, (#36152) is the revert's OWN squash PR. Without
    # this, revertedPrSet records only 36152, the reverted fix's original commit
    # still satisfies Test-PrNumberOnBranch, and a "fixed by #35372" comment on a
    # CLOSED issue is falsely de-noised to closed-fix-unlinked ("No ship risk").
    # Manual subjects are only deterministic when they name one bare reference,
    # or explicitly identify exactly one of several references as "PR #N".
    $m = [regex]::Match($Subject, '(?i)^(?:\[[^\]]+\]\s+)?Revert\b(?<title>.*?)\s+\(#\d+\)\s*$')
    if ($m.Success) {
        $title = $m.Groups['title'].Value
        $prRefs = @([regex]::Matches($title, '(?i)\bPR\s*#(\d+)(?![\p{L}\p{N}_])'))
        if ($prRefs.Count -eq 1) {
            return (ConvertTo-PrNumber -Value $prRefs[0].Groups[1].Value)
        }
        $bareRefs = @([regex]::Matches($title, '#(\d+)(?![\p{L}\p{N}_])'))
        if ($bareRefs.Count -eq 1) {
            return (ConvertTo-PrNumber -Value $bareRefs[0].Groups[1].Value)
        }
    }
    return $null
}

function Test-IsRevertPrTitle {
    param([AllowNull()][AllowEmptyString()][string]$Title)

    if ([string]::IsNullOrWhiteSpace($Title)) { return $false }
    return ($Title -match '(?i)^(?:\[[^\]]+\]\s+)?(?:Revert\b|Backing\s+out\b)') -or
        ($Title -match '(?i)\[Revert\]')
}

function ConvertTo-NegationNormalizedText {
    param([AllowNull()][AllowEmptyString()][string]$Text)

    if ([string]::IsNullOrEmpty($Text)) { return $Text }
    $Text = $Text -replace '[\u2018\u2019\u201B\u02BC\uA78C\uFF07]', "'"
    # Struck text is explicitly withdrawn; remove it rather than turning
    # `~~Fixes #N~~` into affirmative evidence.
    $normalized = [regex]::Replace($Text, '(?s)~~.*?~~', '')
    $normalized = $normalized -replace '\*\*|__', ''
    $normalized = $normalized -replace '(?<!\w)[*_](?=\w)|(?<=\w)[*_](?!\w)', ''
    $asidePattern = '(?i)\b(not|never)\s*(?:,\s*[^,\r\n]{0,80},|\([^)\r\n]{0,80}\)|[—–-]\s*[^—–\r\n]{0,80}[—–-]|(?:;\s*[^;\r\n]{0,80})+;)\s*'
    do {
        $previous = $normalized
        $normalized = [regex]::Replace($normalized, $asidePattern, '$1 ')
    } while ($normalized -ne $previous)
    $normalized = [regex]::Replace(
        $normalized,
        '(?i)\b(?<neg>not|never)\s*;\s*(?=(?:fix(?:e[sd])?|close[sd]?|resolve[sd]?|include(?:d)?|apply|applied|land(?:ed)?|ship(?:ped)?|(?:re-?)?backport(?:ed)?|cherry[-\s]pick(?:ed)?|require(?:d)?|need(?:ed)?|relevant|applicable)\b)',
        '${neg} ')
    $normalized = [regex]::Replace(
        $normalized,
        "(?i)\b(?<neg>don't|doesn't|didn't|won't|wouldn't|shouldn't|can't|couldn't|mustn't)\s*;\s*(?=(?:fix(?:e[sd])?|close[sd]?|resolve[sd]?|include(?:d)?|apply|applied|land(?:ed)?|ship(?:ped)?|(?:re-?)?backport(?:ed)?|cherry[-\s]pick(?:ed)?)\b)",
        '${neg} ')
    $normalized = [regex]::Replace(
        $normalized,
        "(?i)\b(?<neg>not|never)\b(?<gap>[^.!?;`r`n]{0,100}?)(?=\b(?:fix(?:e[sd])?|close[sd]?|resolve[sd]?|include(?:d)?|apply|applied|land(?:ed)?|ship(?:ped)?|backport(?:ed)?|cherry[-\s]pick(?:ed)?|require(?:d)?|need(?:ed)?|relevant|applicable)\b)",
        {
            param($match)
            $gap = [regex]::Replace($match.Groups['gap'].Value, '[^\p{L}\p{N}_'']+', ' ')
            "$($match.Groups['neg'].Value) $($gap.Trim()) "
        })
    return $normalized
}

function Get-ClosingIssueNumbers {
    <#
    .SYNOPSIS
        Extracts dotnet/maui issue numbers named by GitHub closing keywords.
    #>
    param([string]$Text)
    if ([string]::IsNullOrWhiteSpace($Text)) { return @() }
    $Text = ConvertTo-NegationNormalizedText -Text $Text

    $matches = [regex]::Matches(
        $Text,
        '(?im)\b(?:fix(?:e[sd])?|close[sd]?|resolve[sd]?)\s*:?\s+(?:dotnet/maui#|#|https?://github\.com/dotnet/maui/issues/)(\d+)(?![\p{L}\p{N}_])')
    return @($matches | ForEach-Object {
        $prefixStart = [Math]::Max(0, $_.Index - 512)
        $prefix = $Text.Substring($prefixStart, $_.Index - $prefixStart)
        $prefixLines = @([regex]::Split($prefix, '\r\n|\r|\n'))
        $prefix = $prefixLines[-1]
        if ($prefixLines.Count -ge 2 -and -not [string]::IsNullOrWhiteSpace($prefixLines[-2])) {
            $prefix = "$($prefixLines[-2]) $prefix"
        }
        $negated = $prefix -match "(?i)(?:\b(?:do(?:es)?|did|will|would|should|can|could|must)\s+not(?:\s+\w+){0,8}\s+$|\bcannot(?:\s+\w+){0,8}\s+$|\b(?:isn't|wasn't|aren't|weren't|doesn't|don't|didn't|won't|wouldn't|shouldn't|can't|couldn't|mustn't)\s+(?:\w+\s+){0,8}$|\bnever(?:\s+\w+){0,8}\s+$|\bno\s+longer\s+$|\bnot\s+(?!only\b)(?:\w+\s+){0,8}$|\bfail(?:s|ed)?\s+to(?:\s+\w+ly){0,8}\s+$|\bunable\s+to(?:\s+\w+ly){0,8}\s+$|\bonly\s+(?:partial(?:ly)?|partly)\s+$)"
        if (-not $negated) {
            ConvertTo-PrNumber -Value $_.Groups[1].Value
        }
    } | Where-Object { $null -ne $_ } | Sort-Object -Unique)
}

function ConvertTo-PrNumber {
    param([string]$Value)

    [long]$parsed = 0
    if (-not [long]::TryParse($Value, [ref]$parsed)) { return $null }
    if ($parsed -lt 1 -or $parsed -gt [int]::MaxValue) { return $null }
    return [int]$parsed
}

function Test-IsLineageVerbNegated {
    param(
        [string]$Prefix,
        [string]$Between
    )

    $prefix = ConvertTo-NegationNormalizedText -Text ($Prefix -replace '[\u2018\u2019]', "'")
    $between = ConvertTo-NegationNormalizedText -Text ($Between -replace '[\u2018\u2019]', "'")
    $preVerbNegation = "(?i)(?:\b(?:no|without|never|cannot|against|avoid(?:ing)?)\s+$|\bnever\s+(?:\w+\s+){0,3}to\s+$|\b(?:do(?:es)?|did|should|must|will|would|can|could)\s+not\s+$|\b(?:do(?:es)?|did)\s+not(?:\s+\w+){0,4}\s+to\s+$|\b(?:should|must|will|would|can|could)\s+not(?:\s+\w+){0,4}\s+$|\b(?:can't|couldn't|shouldn't|mustn't|won't|wouldn't)\s+(?:\w+\s+){0,3}$|\b(?:don't|doesn't|didn't)\s+(?:\w+\s+){0,4}to\s+$|\b(?:isn't|wasn't|aren't|weren't)\s+(?:\w+\s+){0,4}intended\s+to\s+(?:be\s+)?$|\b(?:don't|doesn't|didn't|shouldn't|mustn't|won't|wouldn't|can't|couldn't|isn't|wasn't|aren't|weren't)\s+$|\bdon't\s+think\s+(?:we\s+)?should\s+$|\bnot\s+to\s+$|\bnot\s+(?:(?:going|planning|intending|expected|allowed|authorized|permitted|supposed|ready|able)\s+|(?:think|believe)(?:\s+\w+){0,5}\s+)to\s+$|\bnot\s*,\s*(?:after|following|pending)\b[^,]*,\s*to\s+$|\bno\s+(?:need|reason|plan|intention)\b[\s\S]*?\bto\s+$|\bnot\s+(?:a\s+)?$|\brevert(?:s|ed|ing)?\s+(?:the\s+)?$)"
    $postVerbNegation = "(?i)(?:\bnot\b|\bno\s+longer\b|\bnever\s+(?:include|apply|land|ship|use)\w*\b|\bcannot\s+(?:be\s+)?(?:included?|applied?|landed?|shipped?|used?)\b|\bno\s+(?:need|reason|plan|intention)\b|\b(?:don't|doesn't|didn't|shouldn't|mustn't|won't|wouldn't|can't|couldn't|isn't|wasn't|aren't|weren't)\s+(?:be\s+)?(?:included?|applied?|landed?|shipped?|used?)\b|\brevert(?:s|ed|ing)?\s+(?:the\s+)?)"
    $clauseNegation = $prefix -match "(?i)\b(?:not(?!\s+(?:only|unusual|unlikely|impossible)\b)|never|cannot|isn't|wasn't|aren't|weren't|can't|couldn't|shouldn't|mustn't|won't|wouldn't|don't|doesn't|didn't)\b(?:(?!\b(?:but|however|nevertheless|nonetheless|so|agreed|decided|chose|want(?:ed)?|went\s+ahead)\b|\b(?:and|yet|still|plus|then|therefore|thus|hence|instead|meanwhile|regardless)\s+(?:we|i|it|this|that|they|maintainers?|the\s+(?:team|fix|change|pr))\b)[^.!?;]){0,2048}$"
    return ($prefix -match $preVerbNegation) -or ($between -match $postVerbNegation) -or $clauseNegation
}

function Test-IsLineageReferenceNegated {
    param(
        [string]$Suffix,
        [switch]$PresenceOnly,
        [switch]$DirectPrSubject
    )

    $suffix = ConvertTo-NegationNormalizedText -Text ($Suffix -replace '[\u2018\u2019]', "'")
    $lead = '(?:was|is|were|are|did|does|should|must|will|would|can|could)'
    $contraction = "(?:wasn't|isn't|weren't|aren't|didn't|shouldn't|mustn't|won't|wouldn't|can't|couldn't)"
    $presenceEffect = '(?:include(?:d)?|omit(?:ted)?|exclude(?:d)?|appl(?:y|ied|ies)|land(?:ed)?|ship(?:ped)?|backport(?:ed)?|cherry[-\s]pick(?:ed)?)'
    $fullEffect = '(?:include(?:d)?|omit(?:ted)?|exclude(?:d)?|appl(?:y|ied|ies|icable)|relevant|need(?:ed)?|require(?:d)?|necessary|pertain(?:s|ed|ing)?|land(?:ed)?|ship(?:ped)?|use(?:d)?|backport(?:ed)?|cherry[-\s]pick(?:ed)?)'
    $effect = if ($PresenceOnly -and -not $DirectPrSubject) { $presenceEffect } else { $fullEffect }
    $prefix = '\s*(?:[,\-–—.!?;]\s*)*[\(\[]?\s*(?:(?:which|that|this|it|though|but|although|yet|however)\s*[,;:]?\s+){0,3}'
    $negatedEffect = "(?i)^$prefix(?:(?:$lead\s+(?:\w+\s+){0,3}?(?:not|never))|(?:$contraction))(?:\s+\w+){0,8}?\s+(?:be\s+)?$effect\b"
    $directNegatedEffect = "(?i)^$prefix(?:not|never)\s+(?:\w+\s+){0,8}?$effect\b"
    $passiveRemovalLead = '(?:(?:this|it)\s+)?(?:(?:(?:was|is|were|are|has\s+been|had\s+been|got|gets)\s+(?:\w+\s+){0,3}?)|(?:(?:since|later|subsequently|ultimately)\s+))?'
    $rollback = "(?i)^$prefix$passiveRemovalLead" + 'revert(?:s|ed|ing)?\b'
    $omitted = "(?i)^$prefix$passiveRemovalLead" + '(?:omit(?:ted)?|exclude(?:d)?)\b'
    $noLongerEffect = "(?i)^$prefix(?:\w+\s+){0,3}?no\s+longer\s+$effect\b"
    $rolledBack = "(?i)^$prefix(?:(?:this|it)\s+)?(?:(?:(?:was|is|were|are|has\s+been|had\s+been|got|gets)\s+(?:\w+\s+){0,3}?)|(?:later\s+(?:(?:\w+\s+){0,8}?and\s+)?)|(?:(?:since|subsequently|ultimately)\s+))?(?:rolled\s+back|back(?:ed|ing)\s+out)\b"
    $laterPresenceRemoval = "(?i)\b(?:but|however|although|yet|nevertheless|nonetheless)\b\s*(?:it|this\s+(?:change|fix|PR))\s+(?:(?:was|is|were|are)\s+(?:not|never)\s+(?:\w+\s+){0,3}?(?:be\s+)?$presenceEffect\b|(?:was|is)\s+(?:revert(?:ed)?|omit(?:ted)?|exclude(?:d)?|rolled\s+back)\b)"
    $negatedHardRemoval = "(?i)^$prefix(?:\w+\s+){0,6}?(?:not|never)\s+(?:\w+\s+){0,2}?(?:revert(?:s|ed|ing)?|omit(?:ted)?|exclude(?:d)?|rolled\s+back)\b"
    if ($suffix -match $negatedHardRemoval) { return $false }
    if ($suffix -match $laterPresenceRemoval) { return $true }
    $restoredPresence = '(?i)\b(?:but|however|nevertheless|nonetheless)\b\s*(?:(?:(?:it|this\s+(?:change|fix|PR)|the\s+(?:change|fix))\s+)?(?:(?:was|is|has\s+been)\s+)?(?:(?:now|later|then|subsequently|ultimately|eventually|afterwards?)\s+)?)?(?:re-?)?(?:included|applied|landed|shipped|backported|retained|restored)\b'
    $decisionRetraction = '(?is)^[\s\S]{0,240}?\b(?:(?:we|(?:the\s+)?team|maintainers?)\s+)?(?:(?:decided|opted|chose|elected)\s+(?:against\s+(?:it|this\s+(?:backport|change|fix)|the\s+backport|(?:including|applying|landing|shipping|backporting|cherry-picking)\s+it)|not\s+to\s+(?:proceed|include|apply|land|ship|backport|cherry-pick))|(?:rejected|declined|abandoned)\s+(?:it|this\s+(?:backport|change|fix)|the\s+backport))\b'
    if ($suffix -match $decisionRetraction) { return -not ($suffix -match $restoredPresence) }
    $isHardRemoval = ($suffix -match $rollback) -or ($suffix -match $omitted) -or
        ($suffix -match $noLongerEffect) -or ($suffix -match $rolledBack)
    if ($isHardRemoval) { return -not ($suffix -match $restoredPresence) }

    $isNegated = ($suffix -match $negatedEffect) -or ($suffix -match $directNegatedEffect)
    if (-not $isNegated) { return $false }
    if ($PresenceOnly -and -not $DirectPrSubject) { return $true }

    # Only an explicit correction of a prior expectation can reverse a soft
    # "not needed/required" phrase. Generic contrastive prose ("not backported,
    # but it is required reading") and hard removal states never restore lineage.
    # "was not expected to be needed, but it is required for this backport."
    $firstSentence = ($suffix -split '(?<=[.;!?])\s|\r?\n', 2)[0]
    $expectationNegation = "(?i)^$prefix(?:(?:$lead\s+not)|$contraction)\s+(?:\w+\s+){0,2}(?:expected|planned|intended|supposed|anticipated)\s+to\s+be\s+(?:needed|required|relevant|applicable)\b"
    $contrastiveLineageAffirmation = "(?i)\b(?:but|however|nevertheless|nonetheless)\b\s*(?:it|this\s+(?:change|fix|PR))\s+(?:is|remains?|will\s+be|must\s+be|should\s+be|is\s+still)\s+(?:needed|required|included|applied|relevant)\s+(?:for|in|to)\s+(?:this|the)\s+(?:backport|fix|branch|SR)\b"
    if (($firstSentence -match $expectationNegation) -and
        ($firstSentence -match $contrastiveLineageAffirmation)) {
        return $false
    }

    return $true
}

function Test-IsDirectLineageReferenceSubject {
    param([string]$Prefix)

    return $Prefix -match '(?i)(?:^|[.!?;,:]\s*|\bPR\s*)(?:(?:however|but|although|though|yet|nevertheless|nonetheless)\b[\s,;:]*)?$'
}

function Get-ExplicitBackportSourceNumbers {
    <#
    .SYNOPSIS
        Extracts non-negated source PR lineage from backport/cherry-pick prose.
    .DESCRIPTION
        Shared by commit scanning and PR lookup so both evidence paths apply
        identical negation, multi-source-list, repository, and overflow rules.
    #>
    param([string]$Text)

    if ([string]::IsNullOrWhiteSpace($Text)) { return @() }
    # Remove withdrawn Markdown spans before calculating reference offsets; doing
    # this only on split prefix/suffix fragments leaves unmatched `~~` sentinels.
    $Text = [regex]::Replace($Text, '(?s)~~.*?~~', '')
    $Text = $Text -replace '[\u2018\u2019]', "'"
    $Text = $Text -replace "`r`n", "`n"
    $Text = $Text -replace "`r", "`n"
    $Text = $Text -replace '\u2028', "`n"
    $Text = $Text -replace '\u2029', "`n`n"
    # Bridge only semicolon-delimited asides that sit between a governing
    # negator and the lineage verb; full-text normalization would alter verb
    # boundaries such as `re-backport`.
    $Text = [regex]::Replace(
        $Text,
        '(?i)\b(?<neg>not|never)(?:;\s*[^;\r\n]{0,80})*;\s*(?=(?:a\s+)?(?:re-?)?backport|cherry[-\s]pick)',
        '${neg} ')
    $Text = [regex]::Replace(
        $Text,
        "(?i)\b(?<neg>don't|doesn't|didn't|won't|wouldn't|shouldn't|can't|couldn't|mustn't)(?:;\s*[^;\r\n]{0,80})*;\s*(?=(?:a\s+)?(?:re-?)?backport|cherry[-\s]pick)",
        '${neg} ')
    # Drop a contextual middle item without orphaning later explicit list items.
    $Text = [regex]::Replace(
        $Text,
        '(?i)(?:;\s*(?:(?:PRs?\s*)?#)\d+(?![\p{L}\p{N}_])\s+(?:is|was)\s+(?:(?:only\s+)?(?:context|background|reference)(?:\s+only)?)\s*)+;\s*(?=(?:and|plus)\s+(?:(?:PRs?\s*)?#)\d+(?![\p{L}\p{N}_]))',
        ', ')
    # A semicolon can be list punctuation rather than a clause boundary:
    # `#A, #B; and #C`. Normalize only this structured continuation.
    $Text = [regex]::Replace(
        $Text,
        '(?i)(?<ref>(?:https://github\.com/dotnet/maui/pull/|dotnet/maui/pull/|(?<!/)pull/|dotnet/maui#|(?:PRs?\s*)?#)\d+(?![\p{L}\p{N}_]))\s*;\s*(?=(?:(?:and|plus)\s+)?(?:https://github\.com/dotnet/maui/pull/|dotnet/maui/pull/|(?<!/)pull/|dotnet/maui#|(?:PRs?\s*)?#)\d+(?![\p{L}\p{N}_])(?!(?:\s+)(?:is|was)\s+(?:only\s+)?(?:context|background|reference)\b))',
        '${ref}, ')
    # Preserve paragraphs and Markdown block/list boundaries, but fold genuine
    # wrapped continuation lines so `Backport of` + newline + `#N` remains one
    # explicit lineage clause. A new bullet is independent evidence and must
    # never bind to the prior bullet's lineage verb.
    $newMarkdownBlock = '(?![ \t]*(?:[-*+]\s+|\d+[.)]\s+|>\s+|#{1,6}\s+))'
    $Text = [regex]::Replace($Text, "(?<!\n)\n(?!\n)$newMarkdownBlock", ' ')
    $result = [System.Collections.Generic.HashSet[int]]::new()
    # A period terminates only when followed by whitespace/end, preserving dots
    # inside github.com URLs while preventing a later sentence's PR mention from
    # binding to an earlier lineage verb.
    $clauses = @([regex]::Split($Text, '(?:[!?;](?:\s+|$)|\.(?=\s|$)(?:\s+|$)|\r?\n+)'))
    $processedVerbs = 0
    $searchOffset = 0
    foreach ($clause in $clauses) {
        if ([string]::IsNullOrWhiteSpace($clause)) { continue }
        $clauseOffset = $Text.IndexOf($clause, $searchOffset, [System.StringComparison]::Ordinal)
        if ($clauseOffset -lt 0) { $clauseOffset = $searchOffset }
        $searchOffset = $clauseOffset + $clause.Length
        $verbs = [regex]::Matches(
            $clause,
            '(?im)\b(?:(?:re-?)?backport(?:s|ed|ing)?|cherry[-\s]pick(?:ed|ing)?(?:\s+from)?)\b')
        foreach ($verb in $verbs) {
            $processedVerbs++
            if ($processedVerbs -gt 200) { break }
            # When a single prose clause has an extreme prefix, fail closed
            # rather than silently ignoring a distant negator outside the
            # bounded analysis window.
            if ($verb.Index -gt 2048) { continue }
            $prefixStart = [Math]::Max(0, $verb.Index - 2048)
            $prefix = $clause.Substring($prefixStart, $verb.Index - $prefixStart)
            $clauseStart = $verb.Index + $verb.Length
            $windowLength = [Math]::Min(2048, $clause.Length - $clauseStart)
            $window = $clause.Substring($clauseStart, $windowLength)
            $references = @([regex]::Matches($window,
                '(?i)(?:https://github\.com/dotnet/maui/pull/|dotnet/maui/pull/|(?<!/)pull/|dotnet/maui#|(?:PRs?\s*)?#)(?<number>\d+)(?![\p{L}\p{N}_])'))
            if ($references.Count -eq 0) { continue }
            $acceptedReferenceIndexes = [System.Collections.Generic.HashSet[int]]::new()

            for ($i = 0; $i -lt $references.Count; $i++) {
                $reference = $references[$i]
                $number = ConvertTo-PrNumber -Value $reference.Groups['number'].Value
                $between = $window.Substring(0, $reference.Index)
                if ($null -eq $number) {
                    if ($i -eq 0) {
                        $firstShapeOk = $between -match '(?i)^\s*[:\-]?\s*(?:\([^)]{0,200}\)\s*)?(?:(?:(?:of|from|for(?:\s+issue)?|targeting|resolving|addresses)|(?:the\s+)?(?:fix|changes?)\s+from|that\s+fixes|of\s+(?:the\s+change\s+in|the\s+following|this)|(?:the\s+following|this))\s*:?\s+)?(?:PRs?\s*)?:?\s*$'
                        if ($firstShapeOk) { [void]$acceptedReferenceIndexes.Add(0) }
                    }
                    continue
                }
                if (Test-IsLineageVerbNegated -Prefix $prefix -Between $between) {
                    continue
                }
                $absoluteReferenceEnd = $clauseOffset + $clauseStart + $reference.Index + $reference.Length
                $suffixLength = [Math]::Min(2048, $Text.Length - $absoluteReferenceEnd)
                $suffix = $Text.Substring($absoluteReferenceEnd, $suffixLength)
                $sawRepeatedReference = $false
                $repeatedReferenceIsDirectSubject = $false
                # Negation after a later PR reference belongs to that later
                # list item, not the current one.
                while ($true) {
                    $nextReference = [regex]::Match($suffix, '(?i)(?:dotnet/maui/pull/|(?<!/)pull/|dotnet/maui#|#)(?<number>\d+)(?![\p{L}\p{N}_])')
                    if (-not $nextReference.Success) { break }
                    $nextNumber = ConvertTo-PrNumber -Value $nextReference.Groups['number'].Value
                    if ($nextNumber -eq $number) {
                        $beforeRepeatedReference = $suffix.Substring(0, $nextReference.Index)
                        $contextualObject = $beforeRepeatedReference -match '(?i)\b(?:in|from|by|of|about|within|via)\s+(?:PR\s*)?$'
                        $lineageObject = $beforeRepeatedReference -match '(?i)\b(?:change|fix|work|code|commit|backport|patch|implementation|workaround)\b[^#\r\n]{0,80}\b(?:in|from|by|of|about|within|via)\s+(?:PR\s*)?$'
                        if ($contextualObject -and -not $lineageObject) {
                            # An unrelated object ("the guide mentioned in #N")
                            # cannot retract the PR's lineage.
                            $suffix = $beforeRepeatedReference
                            break
                        }
                        $repeatedReferenceIsDirectSubject =
                            (-not $contextualObject) -and
                            (Test-IsDirectLineageReferenceSubject -Prefix $beforeRepeatedReference)
                        # A repeated mention of the same PR often introduces its
                        # non-inclusion reason ("#N, but #N was not included").
                        $sawRepeatedReference = $true
                        $suffix = $suffix.Substring($nextReference.Index + $nextReference.Length)
                        continue
                    }
                    $suffix = $suffix.Substring(0, $nextReference.Index)
                    break
                }
                if ($suffix -match '(?i)^\s*(?:is|was)\s+(?:(?:only\s+)?(?:context|background|reference)(?:\s+only)?)\b') {
                    [void]$result.Remove($number)
                    continue
                }
                if (Test-IsLineageReferenceNegated -Suffix $suffix `
                    -PresenceOnly:$sawRepeatedReference `
                    -DirectPrSubject:$repeatedReferenceIsDirectSubject) {
                    # A later repeated mention can retract an earlier positive
                    # occurrence ("#A and #B, but #A was not included"). Remove
                    # any already-accepted occurrence before continuing.
                    [void]$result.Remove($number)
                    continue
                }

                if ($i -eq 0) {
                    # The first reference must be syntactically governed by the
                    # lineage verb. Arbitrary prose such as "updates tests, see
                    # PR #N for context" is not lineage.
                    $firstShapeOk = $between -match '(?i)^\s*[:\-]?\s*(?:\([^)]{0,200}\)\s*)?(?:(?:(?:of|from|for(?:\s+issue)?|targeting|resolving|addresses)|(?:the\s+)?(?:fix|changes?)\s+from|that\s+fixes|of\s+(?:the\s+change\s+in|the\s+following|this)|(?:the\s+following|this))\s*:?\s+)?(?:PRs?\s*)?:?\s*$'
                    if (-not $firstShapeOk) { continue }
                    [void]$result.Add($number)
                    [void]$acceptedReferenceIndexes.Add(0)
                    continue
                }

                if (-not $acceptedReferenceIndexes.Contains($i - 1)) { continue }
                $isExplicitList = $true
                for ($j = 1; $j -le $i; $j++) {
                    $previous = $references[$j - 1]
                    $current = $references[$j]
                    $betweenStart = $previous.Index + $previous.Length
                    $listSeparator = $window.Substring($betweenStart, $current.Index - $betweenStart)
                    # Multi-source prose is deliberately structured-only:
                    # direct separators, conjunctions, or a bounded parenthetical
                    # qualifier followed by a conjunction. Ambiguous free text is
                    # rejected rather than risking false shipped-lineage evidence.
                    $listShapeOk = $listSeparator -match '^\s*(?:(?:,|/|&)\s*(?:PRs?\s*)?|(?:,\s*)?(?:and|plus)\s+(?:PRs?\s*)?|(?:,\s*)?\([^#\r\n]{0,80}\)\s*,?\s*(?:and|plus)\s+(?:PRs?\s*)?)$'
                    $listNegated = Test-IsLineageVerbNegated -Prefix '' -Between $listSeparator
                    $listIsContextual = $listSeparator -match '(?i)\b(?:see|consult|details?|context|background|related|discussion|reference|compare|mention|describ(?:e|ed)|above|below|introduced\s+by|caused\s+by|regressed\s+by|depends?\s+on|conflicts?\s+with|supersed(?:e|es|ed)|blocks?|unblocks?|affected\s+by|reverted\s+by)\b'
                    if (-not $listShapeOk -or $listNegated -or $listIsContextual -or [string]::IsNullOrWhiteSpace($listSeparator)) {
                        $isExplicitList = $false
                        break
                    }
                }
                if ($isExplicitList) {
                    [void]$result.Add($number)
                    [void]$acceptedReferenceIndexes.Add($i)
                }
            }
        }
        if ($processedVerbs -gt 200) { break }
    }

    # A later clause can explicitly retract an earlier source after intervening
    # PR numbers ("#A and #B. #A was not included"). Re-scan accepted numbers
    # for any repeated negated occurrence across the bounded full text so list
    # order and sentence boundaries cannot preserve false-positive lineage.
    foreach ($acceptedNumber in @($result)) {
        $acceptedPattern = "(?i)(?:dotnet/maui/pull/|(?<!/)pull/|dotnet/maui#|#)$acceptedNumber(?![\p{L}\p{N}_])"
        foreach ($occurrence in [regex]::Matches($Text, $acceptedPattern)) {
            $prefixStart = [Math]::Max(0, $occurrence.Index - 160)
            $occurrencePrefix = $Text.Substring($prefixStart, $occurrence.Index - $prefixStart)
            $contextualObject = $occurrencePrefix -match '(?i)\b(?:in|from|by|of|about|within|via)\s+(?:PR\s*)?$'
            $lineageObject = $occurrencePrefix -match '(?i)\b(?:change|fix|work|code|commit|backport|patch|implementation|workaround)\b[^#\r\n]{0,80}\b(?:in|from|by|of|about|within|via)\s+(?:PR\s*)?$'
            if ($contextualObject -and -not $lineageObject) { continue }
            $occurrenceIsDirectSubject =
                (-not $contextualObject) -and
                (Test-IsDirectLineageReferenceSubject -Prefix $occurrencePrefix)
            $occurrenceEnd = $occurrence.Index + $occurrence.Length
            $remainingLength = [Math]::Min(2048, $Text.Length - $occurrenceEnd)
            $occurrenceSuffix = $Text.Substring($occurrenceEnd, $remainingLength)
            # This occurrence gets its own retraction decision. Stop before any
            # later PR reference so another PR's removal cannot retract it; a
            # repeated occurrence of the same number is evaluated separately.
            $nextReference = [regex]::Match(
                $occurrenceSuffix,
                '(?i)(?:dotnet/maui/pull/|(?<!/)pull/|dotnet/maui#|#)(?<number>\d+)(?![\p{L}\p{N}_])')
            if ($nextReference.Success) {
                $occurrenceSuffix = $occurrenceSuffix.Substring(0, $nextReference.Index)
            }
            if (Test-IsLineageReferenceNegated -Suffix $occurrenceSuffix `
                -PresenceOnly -DirectPrSubject:$occurrenceIsDirectSubject) {
                [void]$result.Remove($acceptedNumber)
                break
            }
        }
    }
    return @($result | Sort-Object)
}

function Get-CopilotBackportSourceNumbers {
    param([string]$HeadRefName)

    $match = [regex]::Match(
        $HeadRefName,
        '(?i)^copilot/backport-(?:(?:prs?|fix-from-pr|dotnet-maui)-)?(?<numbers>\d{4,7}(?:-\d{4,7})*)$')
    if (-not $match.Success) { return @() }
    $result = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($token in $match.Groups['numbers'].Value -split '-') {
        $number = ConvertTo-PrNumber -Value $token
        if ($null -ne $number) { [void]$result.Add($number) }
    }
    return @($result | Sort-Object)
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
            $backportPr = ConvertTo-PrNumber -Value $subjMatches[$subjMatches.Count - 1].Groups[1].Value
            if ($backportPr) {
                $allBackportPrs.Add($backportPr) | Out-Null
                $allSourcePrs.Add($backportPr) | Out-Null   # greedy: backport # also resolves
            }
        }

        # Source PR strong signals share the same bounded, negation-aware parser
        # used by PR lookup.
        $sourcePrs = @(Get-ExplicitBackportSourceNumbers -Text $body)
        $sourcePr = if ($sourcePrs.Count -gt 0) { $sourcePrs[0] } else { $null }
        foreach ($sourcePrNumber in $sourcePrs) {
            $allSourcePrs.Add($sourcePrNumber) | Out-Null
        }

        # cherry-pick source SHA: "(cherry picked from commit <sha>)"
        $cherrySourceSha = $null
        $cherryShaMatch = [regex]::Match($body, '(?im)cherry\s+picked\s+from\s+commit\s+([0-9a-f]{7,40})')
        if ($cherryShaMatch.Success) { $cherrySourceSha = $cherryShaMatch.Groups[1].Value }

        # Fixed issues
        $fixesList = @(Get-ClosingIssueNumbers -Text $body)
        foreach ($n in $fixesList) {
            $fixedIssues.Add($n) | Out-Null
        }

        # Revert detection uses the same parser as the extracted PR identity so
        # hand-authored "This reverts", "Reverting", and "Backing out" subjects
        # cannot be parsed successfully but skipped by a narrower outer gate.
        $revertsPr = Get-RevertedPrFromSubject -Subject $subject
        $revM = [regex]::Match($body, '(?im)This reverts commit\s+([0-9a-f]{7,40})')
        $revertsCommit = if ($revM.Success) { $revM.Groups[1].Value } else { $null }
        $isRevert = ($null -ne $revertsPr) -or ($null -ne $revertsCommit) -or
            ($subject -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or
            ($subject -match '\[Revert\]')
        if ($isRevert) {
            # Authoritative override: when we know the reverted commit SHA, read its
            # real subject — its trailing (#NNNN) IS the reverted PR's own number.
            # This is ground truth and overrides any subject-based guess above.
            if ($revertsCommit) {
                $revSubj = Invoke-Git "log -1 --format=%s $revertsCommit"
                if ($revSubj) {
                    $rsM = [regex]::Matches($revSubj, '\(#(\d+)\)')
                    if ($rsM.Count -gt 0) {
                        $revertsPr = ConvertTo-PrNumber -Value $rsM[$rsM.Count - 1].Groups[1].Value
                    }
                }
            }
            $reverts += @{
                revertCommit = $cmtSha
                revertsCommit = $revertsCommit
                revertsPr = $revertsPr
                revertBackportPr = $backportPr
                date = $authorDate
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
            sourcePrs = $sourcePrs
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
    $contentsRef = Get-MetadataValue -Container $Ctx -Name 'contentsRef' -Default $Ctx.srRef
    $primaryRevSpec = "$contentsRef $($excludeArgs -join ' ')"
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

    # Main-side reverts matter for backport guidance: a source PR can remain an
    # ancestor of main after a later revert, so ancestry alone is not enough to
    # safely recommend `/backport`. Keep this separate from SR-side reverts so
    # existing SR-content classifications stay unchanged.
    $mainReverts = @()
    if ($Ctx.mainBranch) {
        # Bound the scan by the PRIOR release baseline, not the current SR tip.
        # Current SR catch-up history can contain both a source fix and its later
        # main revert; `main ^srRef` would then exclude the common-ancestor revert
        # and could recommend re-backporting code main deliberately backed out.
        # If context resolution could not find a baseline, prefer the slower
        # unbounded scan over a false-safe backport recommendation.
        $mainRevertBaselineRef = Get-MetadataValue -Container $Ctx -Name 'mainRevertBaselineRef'
        $mainRevertBounds = if ($mainRevertBaselineRef) { @("^$mainRevertBaselineRef") } else { @() }
        $mainRevertRevSpec = "origin/$($Ctx.mainBranch) $(($mainRevertBounds -join ' ')) --regexp-ignore-case --grep=Revert --grep=Backing"
        $mainRevertScan = Get-CommitsForRevSpec -RevSpec $mainRevertRevSpec -OriginTag 'main'
        $mainReverts = @($mainRevertScan.reverts)
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
        mainReverts = $mainReverts
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
    $raw = Invoke-Gh @('api', "repos/$Repo/issues/$IssueNumber/timeline", '--paginate', '--slurp')
    $parsed = ConvertFrom-GhJsonArrayResult -Raw $raw -Context "issue #$IssueNumber timeline lookup failed"
    if (-not $parsed.Success) { return @() }
    $events = @($parsed.Items)
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

function Get-IssueCommentPrs {
    <#
    .SYNOPSIS
        Extract PR numbers referenced in an issue's COMMENT BODIES (as opposed
        to the structured timeline). Recovers fix linkage that lives ONLY in
        human prose.

    .DESCRIPTION
        Common QA close pattern in this repo: a maintainer closes a regression
        with a comment like "This issue was fixed by PR #35028" but the fix PR
        never used a closing keyword (`Fixes #NNNNN`) and GitHub therefore
        recorded NO `cross-referenced` event on the ISSUE timeline. `Get-IssueTimelinePrs`
        sees nothing, the classifier finds zero candidates, and the issue is
        mislabelled `no-fix-yet` even though the fix shipped. This helper reads
        the comment bodies so that linkage can be recovered.

        Returns @( @{ number = <int>; evidence = 'fix-phrase' | 'mention' } ).
        `evidence` is 'fix-phrase' when the comment pairs the PR reference with
        fix/resolve/close language (higher confidence), else 'mention'.

        IMPORTANT: this only surfaces CANDIDATES. Callers MUST still verify each
        PR actually MERGED and that its commit is on the target branch
        (`Test-CommitOnBranch`) before trusting it as a real fix — a bare prose
        mention ("duplicate of #X", "see #Y") is not proof of anything.
    #>
    param($Repo, $IssueNumber)
    $raw = Invoke-Gh @('api', "repos/$Repo/issues/$IssueNumber/comments", '--paginate', '--slurp')
    $parsed = ConvertFrom-GhJsonArrayResult -Raw $raw -Context "issue #$IssueNumber comments lookup failed"
    if (-not $parsed.Success) { return @() }
    $comments = @($parsed.Items)

    # strongest evidence seen per PR number ('fix-phrase' beats 'mention')
    $byNum = @{}
    foreach ($c in $comments) {
        $body = Get-AzdoProp $c 'body'
        if (-not $body) { continue }
        # Extract PR references, rejecting CROSS-REPO ones. A maui regression is
        # only de-noised by a fix that lives in THIS repo, so a cross-repo
        # shorthand (`dotnet/runtime#123`), a github.com/<other>/<repo>/pull/123
        # URL, or a scheme-less <other>/<repo>/pull/123 path must NOT be mistaken
        # for maui#123. Same-repo shorthand (`dotnet/maui#123`), same-repo pull
        # URLs/paths, bare `#123` and `PR#123` are all accepted. The
        # `qual`/`urlrepo`/`pathrepo` groups capture any owner/repo qualifier so a
        # foreign one can be skipped.
        $refs = [regex]::Matches($body, '(?:(?<qual>[A-Za-z0-9._-]+/[A-Za-z0-9._-]+)#|github\.com/(?<urlrepo>[A-Za-z0-9._-]+/[A-Za-z0-9._-]+)/pull/|(?<pathrepo>[A-Za-z0-9._-]+/[A-Za-z0-9._-]+)/pull/|pull/|#)(?<number>\d+)(?![\p{L}\p{N}_])')
        foreach ($m in $refs) {
            $qual     = $m.Groups['qual'].Value
            $urlRepo  = $m.Groups['urlrepo'].Value
            $pathRepo = $m.Groups['pathrepo'].Value
            if ($qual     -and $qual     -ne $Repo) { continue }   # cross-repo owner/repo#N shorthand
            if ($urlRepo  -and $urlRepo  -ne $Repo) { continue }   # cross-repo github.com/.../pull/N URL
            if ($pathRepo -and $pathRepo -ne $Repo) { continue }   # cross-repo scheme-less owner/repo/pull/N
            $num = ConvertTo-PrNumber -Value $m.Groups['number'].Value
            if ($null -eq $num) { continue }
            # Does THIS comment pair the reference with fix/resolve/close language
            # within a short window (tolerates the long ".../pull/" URL prefix)?
            # The negative lookbehind drops ADJACENTLY-negated fix phrases ("not
            # fixed by #X", "won't fix #Y", "isn't resolved by #Z") so they score as
            # a bare 'mention', not high-confidence 'fix-phrase'. Because -match
            # backtracks, a separate non-negated fix phrase for the same PR still
            # matches; only a SOLELY-(adjacently-)negated reference is demoted. A
            # non-adjacent negation ("won't be fixed by #X") is not caught here, but
            # the caller's merged-AND-on-branch gates still bound the blast radius.
            $partialQualifier = '(?:partial(?:ly)?|partly|temporar(?:y|ily)|workaround|in\s+part)'
            $fixVerb = '(?:fix(?:e[ds])?|resolv(?:e[ds]|ing)?|close[ds]?)'
            $partialFix = ($body -match "(?i)\b$partialQualifier\s+$fixVerb\b[\s\S]{0,60}?(?:pull/|#)$num\b") -or
                ($body -match "(?i)\b$fixVerb\b[\s\S]{0,60}?(?:pull/|#)$num\b[^.!?`r`n]{0,100}?\b$partialQualifier\b") -or
                ($body -match "(?i)\b$fixVerb\b[\s\S]{0,60}?(?:pull/|#)$num\b\s*[.!?]\s*(?:Note:\s*)?(?:it|this|that|the\s+(?:change|fix|resolution))\s+(?:is|was)\s+(?:only\s+)?(?:a\s+)?$partialQualifier(?:\s+fix)?\b")
            $isFix = (-not $partialFix) -and
                ($body -match "(?i)(?<!\b(?:not|never|no|cannot|can't|cant|isn't|isnt|wasn't|wasnt|aren't|arent|weren't|werent|won't|wont|don't|dont|doesn't|doesnt|didn't|didnt)\s{0,3})(?:fix(?:e[ds])?|resolv(?:e[ds]|ing)?|close[ds]?)\b[\s\S]{0,60}?(?:pull/|#)$num\b")
            $ev = if ($isFix) { 'fix-phrase' } else { 'mention' }
            if (-not $byNum.ContainsKey($num) -or $ev -eq 'fix-phrase') { $byNum[$num] = $ev }
        }
    }
    return @($byNum.GetEnumerator() | ForEach-Object { @{ number = [int]$_.Key; evidence = $_.Value } })
}

function Get-PrEvidenceType {
    param($PrBody, $IssueNumber)
    if (-not $PrBody) { return 'none' }
    if (@(Get-ClosingIssueNumbers -Text $PrBody) -contains [int]$IssueNumber) {
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
    if ($null -eq $json) {
        Add-RegressionEvidenceFailure "PR #$PrNumber details lookup failed"
        return $null
    }
    try {
        $rawJson = ($json | Out-String).Trim()
        if ([string]::IsNullOrWhiteSpace($rawJson)) { throw 'empty response' }
        $info = ConvertFrom-Json -InputObject $rawJson -ErrorAction Stop
        if ($null -eq $info -or $info -is [System.Array] -or
            $info -is [string] -or $info -is [ValueType]) {
            throw 'expected a JSON object'
        }
        $requiredProperties = @('number', 'title', 'state', 'baseRefName', 'body', 'mergeCommit', 'files')
        $missingProperties = @($requiredProperties | Where-Object { -not $info.PSObject.Properties[$_] })
        if ($missingProperties.Count -gt 0) {
            throw "PR JSON object missing required properties: $($missingProperties -join ', ')"
        }
        return $info
    } catch {
        Add-RegressionEvidenceFailure "PR #$PrNumber details lookup failed ($($_.Exception.Message))"
        return $null
    }
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

function Test-PrNumberOnBranch {
    <#
    .SYNOPSIS
        Returns $true when $BranchRef has a commit whose message contains the
        literal `(#<PrNumber>)` token GitHub stamps onto squash/merge subjects.

    .DESCRIPTION
        Robust companion to Test-CommitOnBranch for the cross-branch flow.
        A PR's own `mergeCommit.oid` is the SHA on the branch it merged INTO
        (e.g. inflight/candidate). When that change later reaches the SR branch
        via a branch-merge or cherry-pick it gets a DIFFERENT SHA, so a bare
        `merge-base --is-ancestor <prMergeSha> <srBranch>` returns false even
        though the fix IS present. The `(#<num>)` subject token survives all
        three flows (squash, branch-merge, cherry-pick -x), so matching on it
        recovers presence that SHA-ancestry misses. `--fixed-strings` keeps the
        closing paren literal, preventing `(#3502)` from matching `(#35028)`.
    #>
    param([int]$PrNumber, [string]$BranchRef)
    if (-not $PrNumber) { return $false }
    $hit = Invoke-Git "log $BranchRef --fixed-strings --grep=(#$PrNumber) --format=%H -1"
    return [bool]$hit
}

function Get-BackportPrsForSr {
    param($Repo, $SrBranch, $SourcePrNumber)
    # Search broadly, then require explicit source→backport lineage before a
    # result can prove fix presence. A bare contextual mention is not lineage.
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Repo, '--base', $SrBranch,
                       '--state', 'all', '--search', "$SourcePrNumber in:title,body",
                       '--json', 'number,title,body,headRefName,state,mergedAt,closedAt', '--limit', '21')
    $parsed = ConvertFrom-GhJsonArrayResult -Raw $raw -Context "backport lookup for source PR #$SourcePrNumber failed"
    if (-not $parsed.Success) { return @() }
    $items = @($parsed.Items)
    if ($items.Count -gt 20) {
        Add-RegressionEvidenceFailure "backport lookup for source PR #$SourcePrNumber exceeded the 20-result evidence cap"
        $items = @($items | Select-Object -First 20)
    }
    return @($items | Where-Object { Test-IsExplicitBackportForSource -Pr $_ -SourcePrNumber $SourcePrNumber })
}

function Test-IsExplicitBackportForSource {
    param($Pr, [int]$SourcePrNumber)
    if (-not $Pr -or -not $SourcePrNumber) { return $false }

    $body = [string](Get-MetadataValue -Container $Pr -Name 'body' -Default '')
    $title = [string](Get-MetadataValue -Container $Pr -Name 'title' -Default '')
    $head = [string](Get-MetadataValue -Container $Pr -Name 'headRefName' -Default '')

    if (@(Get-CopilotBackportSourceNumbers -HeadRefName $head) -contains $SourcePrNumber) { return $true }
    if (@(Get-ExplicitBackportSourceNumbers -Text $title) -contains $SourcePrNumber) { return $true }
    if (@(Get-ExplicitBackportSourceNumbers -Text $body) -contains $SourcePrNumber) { return $true }
    if ($head -match "(?i)^backport/pr-$SourcePrNumber-to-") { return $true }
    return $false
}

function Resolve-ClosedFixUnlinked {
    <#
    .SYNOPSIS
        Recover 'closed-fix-unlinked' for a CLOSED regression issue whose fix
        lives ONLY in comment prose (no closing keyword, no timeline link).

    .DESCRIPTION
        Maintainers routinely close a regression with a plain-text comment
        ("fixed by PR #35028") that GitHub never turns into a structured link.
        Recover the cited PR and — ONLY when it actually MERGED and its commit
        is verifiably on THIS SR branch — classify 'closed-fix-unlinked': the
        fix is present (no ship risk), but the issue<->PR link is missing and
        should be added for traceability.

        Two gates make prose evidence safe, and BOTH are required:
          1. fix-phrase ONLY — the comment must pair the PR with fix/resolve/
             close language ("was fixed by PR #X"). A BARE mention is rejected
             because regression issues routinely name the CAUSE PR for context
             ("Before PR #32080 ... After PR #32080 the behavior changed"), and
             the cause naturally lives on the branch — it is not a fix.
          2. merged AND on the SR branch — a cited PR that never merged, or
             merged elsewhere, is not proof the fix shipped here.

        Reverted fixes are dropped (a rolled-back fix is not a fix), mirroring
        the $revertedPrSet / Revert-title handling on the SR-contents and
        candidate paths.

        Returns the 'closed-fix-unlinked' result hashtable, or $null when the
        issue is not CLOSED or no cited PR survives both gates (caller falls
        back to its own no-fix-yet handling).
    #>
    param($Ctx, $Issue, $RevertedPrSet)

    $issueState = Get-AzdoProp $Issue 'state'
    if ($issueState -ne 'CLOSED') { return $null }

    $commentPrs = Get-IssueCommentPrs -Repo $Ctx.repo -IssueNumber $Issue.number
    $verifiedFixes = @()
    foreach ($cp in $commentPrs) {
        # Gate 1: require explicit fix language. Bare mentions (the cause-PR
        # blame pattern) are NOT fixes and must not reclassify the issue.
        if ($cp.evidence -ne 'fix-phrase') { continue }
        if ($cp.number -eq $Issue.number) { continue }   # self-reference
        $info = Get-PrInfo -Repo $Ctx.repo -PrNumber $cp.number
        if (-not $info) { continue }
        if ($info.state -ne 'MERGED') { continue }
        # A reverted fix is NOT a fix. Mirror the main SR-contents/candidate
        # paths: drop PRs the SR later reverted, and drop PRs that are themselves
        # rollbacks ("Revert ..." titles). Without this, the `(#<num>)` on-branch
        # token checked below matches the reverted fix's number inside the revert
        # commit's own subject `Revert "... (#num)" (#N)`, so a rolled-back fix
        # would pass the on-branch gate and be reported as "No ship risk".
        if ($RevertedPrSet.ContainsKey([int]$info.number)) { continue }
        if (($info.title -match '(?i)^(?:\[[^\]]+\]\s+)?Revert\b') -or ($info.title -match '\[Revert\]')) { continue }
        # Skip agent/skill/workflow PRs that only mention the issue for context.
        if (Test-PrIsToolingOnly -Files $info.files) { continue }
        $mergeSha = if ($info.mergeCommit) { $info.mergeCommit.oid } else { $null }
        # Gate 2: presence in the release contents via EITHER signal — direct SHA
        # ancestry (fix merged straight to SR) OR the `(#<num>)` subject token
        # (fix flowed in from inflight/main under a different SHA — the common
        # case). Shipped mode supplies an immutable stable tag as contentsRef;
        # other modes fall back to the live SR ref.
        $fixTargetRef = Get-MetadataValue -Container $Ctx -Name 'contentsRef' `
            -Default (Get-MetadataValue -Container $Ctx -Name 'srRef' -Default "origin/$($Ctx.srBranch)")
        $onSr = (Test-CommitOnBranch -Sha $mergeSha -BranchRef $fixTargetRef) `
                -or (Test-PrNumberOnBranch -PrNumber ([int]$info.number) -BranchRef $fixTargetRef)
        if (-not $onSr) { continue }
        $verifiedFixes += @{
            number = [int]$info.number
            title = $info.title
            state = $info.state
            mergeSha = $mergeSha
            evidenceType = "comment-$($cp.evidence)"
        }
    }
    if ($verifiedFixes.Count -gt 0) {
        $prList = (@($verifiedFixes | ForEach-Object { "#$($_.number)" }) | Sort-Object -Unique) -join ', '
        return @{
            classification = 'closed-fix-unlinked'
            confidence = 'high'
            verifiedFromSrContents = $false
            evidence = @("Issue is CLOSED and fix PR $prList is MERGED and present on $($Ctx.srBranch), but was never linked to the issue (no closing keyword, no timeline cross-reference). Linkage recovered from a closing comment that explicitly names the fix.")
            candidateFixPrs = @($verifiedFixes | ForEach-Object {
                @{ number = $_.number; title = $_.title; state = $_.state; evidenceType = $_.evidenceType }
            })
            recommendedAction = "No ship risk — fix is already in the SR. Add a closing reference for traceability (e.g. ``Fixes #$($Issue.number)`` in $prList, or link via the issue's Development panel) so future runs classify it automatically."
        }
    }
    return $null
}

function Get-NetRevertedPrSet {
    param($Reverts)

    $rows = @($Reverts)
    $net = @{}
    if ($rows.Count -eq 0) { return $net }

    $targetToReverters = @{}
    $syntheticReverter = -1
    foreach ($row in $rows) {
        $revertPr = [int](Get-MetadataValue -Container $row -Name 'revertBackportPr' -Default 0)
        $targetPr = [int](Get-MetadataValue -Container $row -Name 'revertsPr' -Default 0)
        if (-not $targetPr) { continue }
        if (-not $revertPr) {
            $revertPr = $syntheticReverter
            $syntheticReverter--
        }
        if (-not $targetToReverters.ContainsKey($targetPr)) {
            $targetToReverters[$targetPr] = [System.Collections.Generic.List[int]]::new()
        }
        if (-not $targetToReverters[$targetPr].Contains($revertPr)) {
            [void]$targetToReverters[$targetPr].Add($revertPr)
        }
    }

    # A PR is net-reverted when at least one currently-active revert PR targets
    # it. This graph model preserves independent revert contributions while a
    # revert-of-a-revert disables only the specific reverter it targets.
    $memo = @{}
    $visiting = @{}
    $resolve = $null
    $resolve = {
        param([int]$PrNumber)
        if ($memo.ContainsKey($PrNumber)) { return [bool]$memo[$PrNumber] }
        if ($visiting.ContainsKey($PrNumber)) {
            # Cyclic metadata is ambiguous; fail safe rather than claiming active.
            foreach ($cyclePr in @($visiting.Keys)) { $memo[[int]$cyclePr] = $true }
            return $true
        }
        $visiting[$PrNumber] = $true
        $isReverted = $false
        if ($targetToReverters.ContainsKey($PrNumber)) {
            foreach ($reverter in $targetToReverters[$PrNumber]) {
                $reverterIsReverted = & $resolve ([int]$reverter)
                if ($memo.ContainsKey($PrNumber) -and [bool]$memo[$PrNumber]) {
                    [void]$visiting.Remove($PrNumber)
                    return $true
                }
                if (-not $reverterIsReverted) {
                    $isReverted = $true
                    break
                }
            }
        }
        [void]$visiting.Remove($PrNumber)
        $memo[$PrNumber] = $isReverted
        return $isReverted
    }

    foreach ($targetPr in $targetToReverters.Keys) {
        if (& $resolve ([int]$targetPr)) { $net[[int]$targetPr] = $true }
    }
    return $net
}

function Classify-RegressionCandidate {
    param($Issue, $CandidatePrs, $Ctx, $SrContents)

    $sourcePrSet = @{}
    foreach ($n in $SrContents.sourcePrs) { $sourcePrSet[$n] = $true }
    $revertedPrSet = Get-NetRevertedPrSet -Reverts $SrContents.reverts
    # sourcePrSet intentionally contains both a backport PR number and the source
    # PR named by "Backport of #N". Preserve that mapping so a later revert of the
    # backport cannot leave the source number looking active (e.g. source #36495,
    # backport #36498, then a revert of #36498).
    $sourceToBackportPrs = @{}
    $srCommitsForMapping = Get-MetadataValue -Container $SrContents -Name 'commits'
    foreach ($commit in @($srCommitsForMapping)) {
        $sourcePrs = @((Get-MetadataValue -Container $commit -Name 'sourcePrs') |
            Where-Object { $null -ne $_ -and [int]$_ -gt 0 })
        if ($sourcePrs.Count -eq 0) {
            $legacySourcePr = [int](Get-MetadataValue -Container $commit -Name 'sourcePr' -Default 0)
            if ($legacySourcePr) { $sourcePrs = @($legacySourcePr) }
        }
        $backportPr = [int](Get-MetadataValue -Container $commit -Name 'backportPr' -Default 0)
        if (-not $backportPr) { continue }
        foreach ($sourcePr in $sourcePrs) {
            $sourcePr = [int]$sourcePr
            if (-not $sourcePr) { continue }
            if (-not $sourceToBackportPrs.ContainsKey($sourcePr)) {
                $sourceToBackportPrs[$sourcePr] = [System.Collections.Generic.List[int]]::new()
            }
            if (-not $sourceToBackportPrs[$sourcePr].Contains($backportPr)) {
                [void]$sourceToBackportPrs[$sourcePr].Add($backportPr)
            }
        }
    }
    $mainRevertedPrSet = @{}
    # Shape-safe read: $SrContents is a [hashtable] during a live survey but can be
    # an arbitrary IDictionary (e.g. [ordered]@{}) or a [pscustomobject] after a
    # JSON round-trip. The old `-is [hashtable]` probe fell through to a PSObject
    # property read that an ordered dictionary does not satisfy, so `mainReverts`
    # was silently ignored and the guard no-op'd. Route through Get-MetadataValue
    # (IDictionary.Contains) so it fires for every dictionary shape (#36497 review).
    $mainReverts = Get-MetadataValue -Container $SrContents -Name 'mainReverts'
    if ($mainReverts) { $mainRevertedPrSet = Get-NetRevertedPrSet -Reverts $mainReverts }

    # Target-branch on-ancestry context. A fix merged BEFORE the SR was cut (or a
    # catch-up merge) is common ancestry of BOTH main and the SR, so the differential
    # `srRef ^main` source-PR set omits it — yet the fix IS present on the SR. When a
    # merged fix's commit is verifiably an ancestor of the target branch, that beats a
    # `merged-on-main-no-backport` conclusion (real-world: #35615 under SR9). Only apply
    # this override when the target is a GENUINE SR branch distinct from main — in
    # candidate mode the target IS main, so common ancestry proves nothing.
    $ctxModeEarly = Get-MetadataValue -Container $Ctx -Name 'mode'
    $targetSrRef  = Get-MetadataValue -Container $Ctx -Name 'contentsRef' `
        -Default (Get-MetadataValue -Container $Ctx -Name 'srRef')
    $targetIsDistinct = ($ctxModeEarly -ne 'candidate') -and [bool]$targetSrRef
    $candidateCutLagGuidance = 'Fix is already merged on main, but the selected Candidate cut can lag current main. Rerun readiness after the release/...-srN branch exists to verify inclusion and get the exact backport command if needed.'

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
            else {
                $commitSourcePrs = @((Get-MetadataValue -Container $c -Name 'sourcePrs') |
                    Where-Object { $null -ne $_ -and [int]$_ -gt 0 })
                if ($commitSourcePrs.Count -gt 0) {
                    $fixPrs += @($commitSourcePrs | ForEach-Object { [int]$_ })
                } else {
                    $legacyCommitSourcePr = [int](Get-MetadataValue -Container $c -Name 'sourcePr' -Default 0)
                    if ($legacyCommitSourcePr) { $fixPrs += $legacyCommitSourcePr }
                }
            }
        }
        $fixPrs = @($fixPrs | Sort-Object -Unique)

        # If EVERY fixing PR was reverted on SR, the fix didn't actually ship.
        $unreverted = @($fixPrs | Where-Object { -not $revertedPrSet.ContainsKey($_) })

        if ($unreverted.Count -gt 0) {
            $prList = ($unreverted | ForEach-Object { "#$_" }) -join ', '
            if ($ctxModeEarly -eq 'candidate') {
                return @{
                    classification    = 'merged-on-main-no-backport'
                    confidence        = 'medium'
                    verifiedFromSrContents = $false
                    evidence          = @("Candidate survey of current main includes a fix for #$($Issue.number) via $prList, but the eventual SR cut ancestry is not yet available")
                    candidateFixPrs   = @($unreverted | ForEach-Object {
                        @{ number = $_; baseRef = 'main'; state = 'MERGED'; onMain = $true; evidenceType = 'candidate-main-fix'; backports = @(); title = '' }
                    })
                    recommendedAction = $candidateCutLagGuidance
                }
            }
            return @{
                classification    = 'in-sr-active'
                confidence        = 'high'
                verifiedFromSrContents = $true
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
                verifiedFromSrContents = $true
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

        $mergeSha = if ($info.mergeCommit) { $info.mergeCommit.oid } else { $null }
        $onMain = if ($mergeSha) { Test-CommitOnBranch -Sha $mergeSha -BranchRef "origin/$($Ctx.mainBranch)" } else { $false }
        # Verified presence on the (real) target SR branch. `$onTargetRef` is the raw
        # ancestry check against srRef (valid even in candidate mode, where srRef==main).
        # `$onTarget` gates the merged-on-main → in-sr-active override to a DISTINCT SR
        # target only.
        $onTargetRef = if ($targetSrRef -and $mergeSha) { Test-CommitOnBranch -Sha $mergeSha -BranchRef $targetSrRef } else { $false }
        $onTarget = [bool]($targetIsDistinct -and $onTargetRef)

        # Detect "Revert ..." titled PRs. Normally a revert is a ROLLBACK, not a fix,
        # so skip it and remember we saw one. BUT a revert of the change that INTRODUCED
        # the regression can itself be the fix. Accept a revert as a fix ONLY under
        # strict, conservative evidence: it MERGED, it EXPLICITLY closes THIS issue
        # (closing-keyword — not a bare mention or generic "Reverts #X" body), and the
        # source PR is verified in the target contents OR its merge commit is an
        # ancestor of the target. The source-PR set covers normal backports, whose
        # target merge SHA differs from the source merge SHA (real-world: #36495 →
        # SR9 backport #36498). Anything short of all three stays a rollback.
        $isRevertPr = Test-IsRevertPrTitle -Title $info.title
        if ($isRevertPr) {
            $sourcePrInTargetContents = $sourcePrSet.ContainsKey([int]$prNum)
            $revertCountsAsFix = ($ev -eq 'closing-keyword') -and ($info.state -eq 'MERGED') -and
                ($onTargetRef -or $sourcePrInTargetContents)
            if (-not $revertCountsAsFix) { $sawRevertCandidate = $true; continue }
        }

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
            onTarget = $onTarget
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

        # ── FALLBACK: closed issue whose fix lives ONLY in comment prose ──
        # No timeline-cross-referenced candidate survived the evidence filter,
        # but the issue is CLOSED. Recover a fix cited only in a closing comment
        # (see Resolve-ClosedFixUnlinked for the fix-phrase + merged-on-SR gates).
        $rec = Resolve-ClosedFixUnlinked -Ctx $Ctx -Issue $Issue -RevertedPrSet $revertedPrSet
        if ($rec) { return $rec }

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
        $subreason = $null
        $subreasonPr = $null
        $confidence = 'high'
        $evidence = @()
        $verifiedFromSrContents = $false

        # In-SR (with revert check)
        if ($targetIsDistinct -and $sourcePrSet.ContainsKey($pr.number)) {
            $verifiedFromSrContents = $true
            $mappedBackports = @()
            if ($sourceToBackportPrs.ContainsKey([int]$pr.number)) {
                $mappedBackports = @($sourceToBackportPrs[[int]$pr.number])
            }
            $activeMappedBackports = @($mappedBackports | Where-Object { -not $revertedPrSet.ContainsKey([int]$_) })
            $directSourceActive = $pr.onTarget -and -not $revertedPrSet.ContainsKey([int]$pr.number)
            if ($directSourceActive -or $activeMappedBackports.Count -gt 0) {
                $verdict = 'in-sr-active'
                if ($directSourceActive) {
                    $evidence += "PR #$($pr.number) merge commit is directly present and active on $($Ctx.srBranch)"
                } else {
                    $mappedList = ($activeMappedBackports | ForEach-Object { "#$_" }) -join ', '
                    $evidence += "PR #$($pr.number) source-PR is active in the SR through mapped backport(s) $mappedList"
                }
            } elseif ($mappedBackports.Count -gt 0) {
                $verdict = 'in-sr-reverted'
                $mappedList = ($mappedBackports | ForEach-Object { "#$_" }) -join ', '
                $evidence += "PR #$($pr.number) reached the SR through mapped backport(s) $mappedList, but every mapped backport was reverted"
            } elseif ($revertedPrSet.ContainsKey($pr.number)) {
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

            # A source PR reverted on main must never be presented as safe to track
            # or land, regardless of backport state. Check this AHEAD of the
            # backport-state branches: otherwise an OPEN backport (or any other
            # backport state) masks the revert and the report emits
            # 'backport-in-progress' ("Track backport PR to completion") for code
            # that main has already backed out (PR #36497 review).
            if ($mainRevertedPrSet.ContainsKey([int]$pr.number)) {
                $verdict = 'needs-human-review'
                $subreason = 'reverted-on-main'
                $subreasonPr = [int]$pr.number
                $confidence = 'medium'
                $evidence += "PR #$($pr.number) merged to main but was later reverted on main — do not backport until a human verifies the current fix/revert chain"
            }
            elseif ($mergedBackport) {
                # backport landed but PR # is different from what we tracked → check sourcePrSet for backport #
                if ($sourcePrSet.ContainsKey([int]$mergedBackport.number)) {
                    $verifiedFromSrContents = $true
                    if ($revertedPrSet.ContainsKey([int]$mergedBackport.number)) {
                        $verdict = 'in-sr-reverted'
                        $evidence += "Backport PR #$($mergedBackport.number) in SR but reverted"
                    } else {
                        $verdict = 'in-sr-active'
                        $evidence += "Backport PR #$($mergedBackport.number) in SR (active)"
                    }
                } else {
                    $verdict = 'needs-human-review'
                    $subreason = 'merged-backport-missing'
                    $subreasonPr = [int]$mergedBackport.number
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
                # reverted-on-main is handled by the hoisted guard above, so a PR
                # reaching here is known NOT to have been reverted on main.
                if ($pr.onTarget) {
                    $verifiedFromSrContents = $true
                    # Merge commit is verifiably an ancestor of the SR branch, even
                    # though it fell out of the differential source-PR set (common
                    # ancestry with main — merged before the cut or via catch-up
                    # merge). Presence on the branch beats a "no backport" verdict.
                    if ($revertedPrSet.ContainsKey([int]$pr.number)) {
                        $verdict = 'in-sr-reverted'
                        $evidence += "PR #$($pr.number) merge commit is present on $($Ctx.srBranch) but was reverted on the SR"
                    } else {
                        $verdict = 'in-sr-active'
                        $evidence += "PR #$($pr.number) merge commit verified on $($Ctx.srBranch) via common ancestry (absent from the differential source-PR set, but present on the branch)"
                    }
                }
                elseif ($pr.onMain) {
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
                if ($pr.baseRef -eq $Ctx.mainBranch) {
                    $verdict = 'open-on-main'
                    $evidence += "PR #$($pr.number) is OPEN against main"
                } else {
                    $verdict = 'needs-human-review'
                    $confidence = 'medium'
                    if ($pr.baseRef -eq 'inflight/current') {
                        $subreason = 'open-non-main-inflight'
                        $subreasonPr = [int]$pr.number
                        # inflight/current PRs reach main via normal Candidate promotion — do
                        # NOT instruct a captain to retarget. Wait for the merge + promotion flow.
                        $evidence += "PR #$($pr.number) is OPEN against $($pr.baseRef) — wait for it to merge and flow to main via Candidate promotion, then rerun readiness. (Retargeting to main directly is an optional expedited path, not required.)"
                    } else {
                        $subreason = 'open-non-main-other'
                        $subreasonPr = [int]$pr.number
                        $evidence += "PR #$($pr.number) is OPEN against $($pr.baseRef), not main — wait for its content to reach main (via merge + forward-flow), then rerun readiness"
                    }
                }
            }
            else {
                $verdict = 'needs-human-review'
                $confidence = 'low'
                $evidence += "PR #$($pr.number) in unexpected state: $($pr.state)"
            }
        }

        $perPrVerdicts += @{
            pr = $pr
            verdict = $verdict
            subreason = $subreason
            subreasonPr = $subreasonPr
            confidence = $confidence
            evidence = $evidence
            verifiedFromSrContents = $verifiedFromSrContents
        }
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

    # ── CLOSED-issue guard against a contradictory 'open-on-main' ──
    # 'open-on-main' means "the fix PR is still OPEN on main; wait for it to
    # merge, then backport" — an ACTIVE (Tier-2) regression. That is impossible
    # for a CLOSED issue: an unmerged PR cannot have closed it. This happens
    # when a giant still-open 'Candidate' changelog PR `Fixes`-lists dozens of
    # issues, so its OPEN state gets attributed to an already-completed issue
    # (real-world: #35615 shown as open-on-main under SR9 while candidate #35716
    # was still open). Never emit open-on-main for a CLOSED issue:
    #   a. First try the same comment-prose recovery path 1 uses — a merged fix
    #      verifiably on the SR wins → 'closed-fix-unlinked' (Tier 3).
    #   b. Otherwise fall to the honest 'no-fix-yet' (Tier 3 for a CLOSED issue):
    #      the automation can't pin a verified fix on this SR and the open
    #      candidate hasn't merged. NOT an active SR regression.
    # Scope: the contradiction is "issue CLOSED but the SELECTED fix PR is still
    # OPEN/unmerged" — an unmerged PR cannot have closed the issue. Gate on the
    # selected PR being OPEN, not just the verdict string: the OPEN-candidate
    # split routes an OPEN PR targeting a non-`main` branch (e.g. inflight/current)
    # to 'needs-human-review' rather than 'open-on-main', and that path is equally
    # contradictory for a CLOSED issue. Every other verdict (merged-*,
    # backport-in-progress, rejected-from-sr, in-sr-*) stays as-is even for CLOSED
    # issues — those are still actionable (the SR may still need the backport).
    # The merged-backport 'needs-human-review' (~L2399) is excluded because its
    # selected PR is not OPEN; and on any rare overlap, the Resolve-ClosedFixUnlinked
    # recovery below reclassifies to closed-fix-unlinked before we fall to no-fix-yet.
    $selectedPrOpenUnmerged = $best.pr -and $best.pr.state -eq 'OPEN'
    $closedWithOpenCandidate =
        $best.verdict -eq 'open-on-main' -or
        ($best.verdict -eq 'needs-human-review' -and $selectedPrOpenUnmerged -and $best.pr.baseRef -ne $Ctx.mainBranch)
    if ($closedWithOpenCandidate -and (Get-AzdoProp $Issue 'state') -eq 'CLOSED') {
        $rec = Resolve-ClosedFixUnlinked -Ctx $Ctx -Issue $Issue -RevertedPrSet $revertedPrSet
        if ($rec) { return $rec }
        return @{
            classification = 'no-fix-yet'
            confidence = 'medium'
            evidence = @("Issue is CLOSED but the candidate fix PR (#$($best.pr.number)) is OPEN/unmerged on $($best.pr.baseRef) — an unmerged PR cannot have closed this issue; the real fix likely shipped elsewhere or the candidate is stale. Not an active SR regression.")
            candidateFixPrs = @($strongPrs | ForEach-Object { @{
                number = $_.number; title = $_.title; state = $_.state
                baseRef = $_.baseRef; evidenceType = $_.evidenceType
                onMain = $_.onMain; backports = $_.backports
            } })
            recommendedAction = 'Verify the fix is present on this SR (or add a closing reference); the open candidate PR has not merged.'
        }
    }

    # Shape-safe read: IDictionary does not guarantee ContainsKey — an [ordered]
    # dictionary (OrderedDictionary) exposes only .Contains, so `$Ctx.ContainsKey`
    # throws MethodNotFound under StrictMode. Get-MetadataValue uses
    # IDictionary.Contains and also handles the [pscustomobject] round-trip shape.
    $ctxMode = Get-MetadataValue -Container $Ctx -Name 'mode'
    $isCandidateMode = $ctxMode -eq 'candidate' -or $Ctx.srBranch -eq $Ctx.mainBranch
    $isShippedMode = $ctxMode -eq 'shipped'
    $backportCommand = "/backport to $($Ctx.srBranch)"
    # Candidate mode surveys current main, but an already-selected Candidate cut
    # commit can lag it. Keep the row as a risk until the SR branch exists and
    # ancestry can prove the fix was included.
    $candidateMergedGuidance = $candidateCutLagGuidance
    $candidateOpenGuidance = 'Wait for the main merge before the SR cut; rerun readiness after the release/...-srN branch exists to get the exact backport command.'
    # Shipped mode: the SR already tagged, so the current-SR `/backport` command no
    # longer applies. Reframe as a human hotfix/next-SR decision (never an automatic
    # backport to an already-shipped SR).
    $shippedMergedGuidance = "SR ``$($Ctx.srBranch)`` has already shipped — the automatic current-SR backport workflow no longer applies. A human decides whether to hotfix this shipped SR or carry the fix forward to the next SR."
    $shippedOpenGuidance = "SR ``$($Ctx.srBranch)`` has already shipped and this fix has not merged yet — a human decides whether it warrants a hotfix to the shipped SR or should ride the next SR; the automatic current-SR backport workflow no longer applies."
    $recAction = switch ($best.verdict) {
        'in-sr-active' { 'No action — fix is shipping' }
        'in-sr-reverted' { 'Investigate: backport landed and was reverted on SR' }
        'rejected-from-sr' { 'Check rejection rationale (WorkIQ) — was this intentional or stale?' }
        'backport-in-progress' {
            if ($isShippedMode) {
                "SR ``$($Ctx.srBranch)`` has already shipped — decide whether the open backport should land as a hotfix or close in favor of the next SR; it is not a retroactive ship blocker."
            } else {
                'Track backport PR to completion'
            }
        }
        'merged-on-main-no-backport' {
            if ($isShippedMode) { $shippedMergedGuidance }
            elseif ($isCandidateMode) { $candidateMergedGuidance }
            else { "On the merged source PR, post ``$backportCommand``" }
        }
        'merged-non-main-only' { 'Flow fix to main first, then rerun readiness to verify the merged source PR is on main before requesting a backport' }
        'open-on-main' {
            if ($isShippedMode) { $shippedOpenGuidance }
            elseif ($isCandidateMode) { $candidateOpenGuidance }
            else { "Wait for main merge; then post ``$backportCommand`` on the merged source PR" }
        }
        'needs-human-review' {
            switch ($best.subreason) {
                'merged-backport-missing' { "Re-run readiness without ``-NoFetch`` (or verify the backport's merge target manually) — backport #$($best.subreasonPr) is MERGED on GitHub but absent from SR git contents" }
                'open-non-main-inflight'  { "Wait for #$($best.subreasonPr) to merge and reach main via Candidate promotion, then rerun readiness (retargeting to main directly is an optional expedited path, not required)" }
                'open-non-main-other'     { "Wait for #$($best.subreasonPr)'s content to reach main (merge + forward-flow), then rerun readiness" }
                'reverted-on-main'        { "Manual review required: source PR #$($best.subreasonPr) was reverted on main; verify the revert chain or find a replacement fix before requesting any SR backport" }
                default { 'Manual review required' }
            }
        }
        'no-fix-yet' { 'No fix exists — investigate priority' }
        default { 'Manual review required' }
    }

    @{
        classification = $best.verdict
        confidence = $best.confidence
        verifiedFromSrContents = [bool]$best.verifiedFromSrContents
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
    $failedLabels = [System.Collections.Generic.List[string]]::new()
    $truncatedLabels = [System.Collections.Generic.List[string]]::new()
    $failedIssues = [System.Collections.Generic.List[int]]::new()
    $Script:RegressionEvidenceFailures.Clear()
    $probeLimit = $MaxIssues + 1

    foreach ($label in $Labels) {
        $raw = Invoke-Gh @('issue', 'list', '--repo', $Ctx.repo, '--label', $label,
                           '--state', 'all', '--limit', $probeLimit.ToString(),
                           '--json', 'number,title,state,stateReason,labels,milestone,createdAt,closedAt')
        if ($null -eq $raw) {
            [void]$failedLabels.Add($label)
            continue
        }
        try {
            $rawJson = ($raw | Out-String).Trim()
            if ([string]::IsNullOrWhiteSpace($rawJson)) {
                throw "gh returned an empty response"
            }
            $list = ConvertFrom-Json -InputObject $rawJson -NoEnumerate -ErrorAction Stop
            if ($null -eq $list) {
                throw "expected a JSON array but received null"
            }
            if ($list -isnot [System.Array]) {
                throw "expected a JSON array but received $($list.GetType().Name)"
            }
        } catch {
            [void]$failedLabels.Add($label)
            Write-Warn "Failed to parse regression issue query for label '$label': $($_.Exception.Message)"
            continue
        }
        if ($list.Count -gt $MaxIssues) {
            [void]$truncatedLabels.Add($label)
            Write-Warn "Regression issue query for label '$label' exceeded -MaxIssues $MaxIssues; results are truncated and incomplete."
            $list = @($list | Select-Object -First $MaxIssues)
        }
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
                    $milestoneParts = Get-SrMilestoneParts -Milestone $issueMilestone
                    if ($milestoneParts) {
                        $milestoneMajor    = $milestoneParts.Major
                        $milestoneCycleNum = $milestoneParts.SrNumber
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

        $evidenceFailureStart = $Script:RegressionEvidenceFailures.Count
        $candidatePrs = Get-IssueTimelinePrs -Repo $Ctx.repo -IssueNumber $iss.number
        $classify = Classify-RegressionCandidate -Issue $iss -CandidatePrs $candidatePrs `
                        -Ctx $Ctx -SrContents $SrContents
        $issueEvidenceFailures = @($Script:RegressionEvidenceFailures | Select-Object -Skip $evidenceFailureStart)
        if ($issueEvidenceFailures.Count -gt 0) {
            [void]$failedIssues.Add([int]$iss.number)
            # Deterministic target-content evidence is sufficient even if an
            # unrelated secondary lookup failed. Active and reverted verdicts
            # receive the same protection when their git evidence is complete.
            $verifiedFromSrContents = [bool](Get-MetadataValue -Container $classify -Name 'verifiedFromSrContents' -Default $false)
            if (-not $verifiedFromSrContents) {
                $classify = @{
                    classification    = 'needs-human-review'
                    confidence        = 'low'
                    evidence          = @("Evidence lookup incomplete for issue #$($iss.number): $($issueEvidenceFailures -join '; ')")
                    candidateFixPrs   = @()
                    recommendedAction = 'Rerun readiness after GitHub API access recovers; do not treat missing PR/backport evidence as authoritative.'
                }
            }
        }

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
    return [PSCustomObject]@{
        Items           = @($results)
        IsComplete      = ($failedLabels.Count -eq 0 -and $truncatedLabels.Count -eq 0 -and $failedIssues.Count -eq 0)
        FailedLabels    = @($failedLabels)
        TruncatedLabels = @($truncatedLabels)
        FailedIssues    = @($failedIssues)
    }
}

# region ────────────────────── 6. OPEN SR-TARGETING PRs ───────────────────

function Get-OpenSrPrs {
    param($Ctx)
    Write-Host "Listing open PRs targeting $($Ctx.srBranch)..." -ForegroundColor Cyan
    $raw = Invoke-Gh @('pr', 'list', '--repo', $Ctx.repo, '--base', $Ctx.srBranch,
                       '--state', 'open', '--limit', '101',
                       '--json', 'number,title,author,isDraft,createdAt,updatedAt,labels,reviewDecision')
    $parsed = ConvertFrom-GhJsonArrayResult -Raw $raw -Context "open PR lookup for $($Ctx.srBranch) failed" -SuppressRegressionFailure
    if (-not $parsed.Success) {
        return [PSCustomObject]@{ Items = @(); IsComplete = $false; Reason = "Open PR query failed for $($Ctx.srBranch)." }
    }
    $items = @($parsed.Items)
    $truncated = ($items.Count -gt 100)
    if ($truncated) { $items = @($items | Select-Object -First 100) }
    return [PSCustomObject]@{
        Items      = @($items)
        IsComplete = -not $truncated
        Reason     = if ($truncated) { "Open PR query for $($Ctx.srBranch) exceeded the 100-result cap." } else { $null }
    }
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
    param($OpenSrPrs, [string]$SrBranch, [switch]$Shipped, [switch]$Incomplete, [string]$IncompleteReason)

    $prs = @($OpenSrPrs)
    $p0 = @($prs | Where-Object { Test-IsP0Pr $_ })
    if ($p0.Count -gt 0) {
        $nums = ($p0 | ForEach-Object { "#$($_.number)" }) -join ', '
        $nextAction = if ($Shipped) {
            "``$SrBranch`` already shipped — decide whether to land each P/0 PR as a hotfix, carry it to the next SR, or explicitly de-prioritize it."
        } else {
            'Land or de-prioritize each P/0 PR before shipping.'
        }
        $incompleteSuffix = if ($Incomplete) {
            $reason = if ($IncompleteReason) { $IncompleteReason } else { 'Open PR scan incomplete.' }
            " $reason Additional P/0 PRs may be omitted."
        } else { '' }
        if ($Incomplete) {
            $nextAction += ' Inspect the full release-branch PR queue because the retained results may omit additional P/0 PRs.'
        }
        return @(New-ReadinessCheck `
            -Area 'P/0 release-branch PRs' `
            -Status 'BLOCKED' `
            -Details "$($p0.Count) open P/0-labelled PR(s) target ``$SrBranch``: $nums.$incompleteSuffix" `
            -NextAction $nextAction)
    }
    if ($Incomplete) {
        return @(New-ReadinessCheck `
            -Area 'P/0 release-branch PRs' `
            -Status 'WATCH' `
            -Details $(if ($IncompleteReason) { $IncompleteReason } else { 'Open PR scan incomplete; P/0 status could not be confirmed.' }) `
            -NextAction 'Rerun readiness with working GitHub access and a sufficient PR limit before treating P/0 status as clear.')
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
        'closed-fix-unlinked'         { 3; break }
        'out-of-scope-future-sr'      { 3; break }
        default                       { 2 }   # unknown → treat as risk
    }
}

function Get-EffectiveVerdictTier {
    <#
    .SYNOPSIS
        Applies state-sensitive tier adjustments to a regression classification.
        The Mode parameter is retained so verdict, hash, and renderer callers share
        one stable policy surface as lifecycle-specific rules evolve.
    #>
    param(
        [string]$Classification,
        [string]$Mode = 'in-flight',
        [string]$State = 'OPEN'
    )

    $tier = Get-VerdictTier -Classification $Classification
    if ($Classification -eq 'no-fix-yet' -and $State -ne 'OPEN') {
        return 3
    }
    return $tier
}

# Shape-safe accessor for report metadata (and any maybe-hashtable /
# maybe-pscustomobject payload). Report data is a live [hashtable] during a
# survey but a [pscustomobject] once round-tripped through JSON (renderer /
# idempotency callers). Under Set-StrictMode -Version Latest, `.ContainsKey()`
# throws MethodNotFound on a pscustomobject and a bare property read throws on a
# hashtable missing the key — so probe the shape before reading and return
# $Default when the key/property is absent. Reused by Get-OverallVerdict,
# Get-ReportSemanticHash and Format-MarkdownReport so every metadata read stays
# shape-safe from one place.
function Get-MetadataValue {
    param($Container, [string]$Name, $Default = $null)
    if ($null -eq $Container) { return $Default }
    if ($Container -is [System.Collections.IDictionary]) {
        if ($Container.Contains($Name)) { return $Container[$Name] }
        return $Default
    }
    if ($Container.PSObject -and $Container.PSObject.Properties[$Name]) {
        return $Container.$Name
    }
    return $Default
}

function ConvertTo-TopLevelDictionary {
    param($Container)
    if ($Container -is [System.Collections.IDictionary]) { return $Container }

    $result = @{}
    if ($null -ne $Container -and $Container.PSObject) {
        foreach ($property in $Container.PSObject.Properties) {
            $result[$property.Name] = $property.Value
        }
    }
    return $result
}

function Get-NonEmptyStringValues {
    param($Value)
    @($Value) |
        Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
        ForEach-Object { ([string]$_).Trim() }
}

function Get-ShippedVerdict {
    <#
    .SYNOPSIS
        Computes the post-ship follow-up verdict for an already-tagged SR (-Shipped).
    .DESCRIPTION
        An SR that has tagged cannot be un-shipped, so this NEVER returns 🔴 Not
        Ready. It surfaces the same regression/CI/ship-check signals the in-flight
        verdict uses, but reframes them:
          - Any actionable Tier-1/Tier-2 regression — carry-forward or not — and
            any BLOCKED ship check drive a 🟡 "Shipped — follow-up required".
          - Regressions milestoned to a later SR are carry-forward: non-gating for
            the shipped release (never 🔴), but still yellow until forward-tracking
            is acknowledged.
          - CI red/stale/unknown is advisory only and does not change the symbol.
          - Otherwise the verdict is 🟢 "Shipped — clean".

        Reads the shipped SR number/major from $Data.shippedInfo (populated by
        Invoke-Main) to classify carry-forward. Absent shippedInfo → carry-forward
        detection finds nothing and every actionable signal stays a follow-up.
    #>
    param($Data)

    $reasons = New-Object System.Collections.Generic.List[string]
    $followUp = $false
    if ([bool](Get-MetadataValue -Container $Data -Name 'surveyIncomplete' -Default $false)) {
        $followUp = $true
        $partialReason = Get-MetadataValue -Container $Data -Name 'surveyIncompleteReason' -Default 'Partial survey did not query every readiness axis.'
        $reasons.Add("[Follow-up] $partialReason Rerun with ``-Phase all`` before treating the tracker as clean.") | Out-Null
    }
    if ([bool](Get-MetadataValue -Container $Data -Name 'regressionScanIncomplete' -Default $false)) {
        $followUp = $true
        $failedLabels = @(Get-NonEmptyStringValues -Value (Get-MetadataValue -Container $Data -Name 'regressionFailedLabels'))
        $failedLabelText = if ($failedLabels.Count -gt 0) { " ($($failedLabels -join ', '))" } else { '' }
        $reasons.Add("[Follow-up] Regression scan incomplete$failedLabelText — results may be understated; rerun before treating the tracker as clean.") | Out-Null
    }

    # Shipped cycle anchor for carry-forward classification.
    $shippedSr = 0; $shippedMajor = 0; $shippedSubPatch = 0
    $si = Get-MetadataValue -Container $Data -Name 'shippedInfo'
    if ($si) {
        $shippedSr    = [int](Get-MetadataValue -Container $si -Name 'srNumber' -Default 0)
        $shippedMajor = [int](Get-MetadataValue -Container $si -Name 'major' -Default 0)
        $shippedSubPatch = Get-SrSubPatchFromVersion -Version (Get-MetadataValue -Container $si -Name 'version')
        if ([bool](Get-MetadataValue -Container $si -Name 'hotfixInProgress' -Default $false)) {
            $followUp = $true
            $liveHotfixVersion = [string](Get-MetadataValue -Container $si -Name 'liveVersion')
            $hotfixSuffix = if ($liveHotfixVersion) { " (``$liveHotfixVersion``)" } else { '' }
            $reasons.Add("[Follow-up] An unpublished hotfix$hotfixSuffix is in progress on the shipped SR branch.") | Out-Null
        }
    }

    $shippedRegressions = Get-MetadataValue -Container $Data -Name 'regressions'
    if ($shippedRegressions) {
        $followCounts = @{}
        $carryCount = 0
        foreach ($r in $shippedRegressions) {
            $regressionState = [string](Get-MetadataValue -Container $r -Name 'state' -Default 'OPEN')
            $tier = Get-EffectiveVerdictTier -Classification $r.classification -Mode 'shipped' -State $regressionState
            if ($tier -ge 3) { continue }
            if (Test-IsCarryForwardRegression -Regression $r `
                    -ShippedSrNumber $shippedSr -ShippedMajor $shippedMajor -ShippedSubPatch $shippedSubPatch) {
                $carryCount++
                continue
            }
            if (-not $followCounts.ContainsKey($r.classification)) { $followCounts[$r.classification] = 0 }
            $followCounts[$r.classification]++
            $followUp = $true
        }
        foreach ($k in $followCounts.Keys | Sort-Object) {
            $reasons.Add("[Follow-up] $($followCounts[$k]) × ``$k`` — post-ship carry-forward / hotfix decision, not a ship blocker") | Out-Null
        }
        if ($carryCount -gt 0) {
            $followUp = $true
            $reasons.Add("[Advisory] $carryCount regression(s) milestoned to a later SR — carry-forward, non-gating") | Out-Null
        }
    }

    # CI is advisory in shipped mode — the tag already published.
    $shippedCi = Get-MetadataValue -Container $Data -Name 'ci'
    $shippedCiOverall = Get-MetadataValue -Container $shippedCi -Name 'overall'
    if ($shippedCiOverall -in @('red-needs-review', 'stale', 'partial-unknown', 'unknown')) {
        $reasons.Add("[Advisory] Post-ship CI on the SR branch is ``$shippedCiOverall`` — informational; it does not affect the already-shipped release.") | Out-Null
    }

    # BLOCKED/WATCH/UNKNOWN ship checks post-ship are follow-ups, never
    # retroactive blockers. Missing required evidence must not render "clean".
    $shippedChecks = Get-MetadataValue -Container $Data -Name 'shipChecks'
    if ($shippedChecks) {
        $blockedShipChecks = @($shippedChecks | Where-Object { $_.Status -eq 'BLOCKED' })
        foreach ($sc in $blockedShipChecks) {
            $followUp = $true
            $reasons.Add("[Follow-up] Ship check needs post-ship attention: $($sc.Area)") | Out-Null
        }
        $uncertainShipChecks = @($shippedChecks | Where-Object { $_.Status -in @('WATCH', 'UNKNOWN') })
        foreach ($sc in $uncertainShipChecks) {
            $followUp = $true
            $reasons.Add("[Follow-up] Ship check $($sc.Status): $($sc.Area)") | Out-Null
        }
    }

    if ($followUp) {
        return @{
            symbol = '🟡'
            tier = 2
            label = 'Shipped — follow-up required'
            reasons = $reasons.ToArray()
        }
    }
    return @{
        symbol = '🟢'
        tier = 3
        label = 'Shipped — clean'
        reasons = if ($reasons.Count -gt 0) { $reasons.ToArray() } else { @('Shipped — no urgent or gating post-ship follow-ups detected.') }
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
    $Data = ConvertTo-TopLevelDictionary -Container $Data

    $mode = Get-MetadataValue -Container $Data.metadata -Name 'mode'
    $isCandidate = ($mode -eq 'candidate')

    # ── SHIPPED MODE: post-ship follow-up framing (never retroactively blocks) ──
    # The SR already tagged. Nothing surfaced here can un-ship it, so we NEVER
    # return 🔴 Not Ready. Newly discovered regressions and signals become
    # post-ship FOLLOW-UPS (carry-forward / hotfix decisions). CI red is advisory.
    # Regressions milestoned to a later SR are carry-forward and non-gating.
    # Carry-forward still requires tracking, so it produces a yellow follow-up
    # verdict; only a report with no actionable or carry-forward work is green.
    if ($mode -eq 'shipped') {
        return (Get-ShippedVerdict -Data $Data)
    }

    $reasons = New-Object System.Collections.Generic.List[string]
    $tier1 = $false
    $tier2 = $false
    if ([bool](Get-MetadataValue -Container $Data -Name 'surveyIncomplete' -Default $false)) {
        $tier2 = $true
        $partialReason = Get-MetadataValue -Container $Data -Name 'surveyIncompleteReason' -Default 'Partial survey did not query every readiness axis.'
        $reasons.Add("[Tier 2] $partialReason Rerun with ``-Phase all`` for a global verdict.") | Out-Null
    }
    if ([bool](Get-MetadataValue -Container $Data -Name 'regressionScanIncomplete' -Default $false)) {
        $tier2 = $true
        $failedLabels = @(Get-NonEmptyStringValues -Value (Get-MetadataValue -Container $Data -Name 'regressionFailedLabels'))
        $failedLabelText = if ($failedLabels.Count -gt 0) { " ($($failedLabels -join ', '))" } else { '' }
        $reasons.Add("[Tier 2] Regression scan incomplete$failedLabelText — results may be understated") | Out-Null
    }

    # Regression classifications
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        $t1Counts = @{}
        $t2Counts = @{}
        foreach ($r in $Data['regressions']) {
            $regressionState = [string](Get-MetadataValue -Container $r -Name 'state' -Default 'OPEN')
            $tier = Get-EffectiveVerdictTier -Classification $r.classification -Mode $mode -State $regressionState
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
    # CLEANUP does not escalate. UNKNOWN means required evidence is incomplete,
    # so align with Preview and surface a conditional verdict.
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
        $unknownShipChecks = @($Data['shipChecks'] | Where-Object { $_.Status -eq 'UNKNOWN' })
        foreach ($sc in $unknownShipChecks) {
            $tier2 = $true
            $reasons.Add("[Tier 2] Ship check UNKNOWN: $($sc.Area)") | Out-Null
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
    $Data = ConvertTo-TopLevelDictionary -Container $Data

    # MUST be [ordered]: a plain [hashtable] enumerates keys in an order derived
    # from per-process String.GetHashCode(), which .NET Core randomizes on every
    # process start. ConvertTo-Json would then emit keys in a different order each
    # run, producing a DIFFERENT hash for identical content — silently defeating
    # the workflow's idempotent no-op (which compares a hash written by an earlier
    # process against one computed now). Insertion order keeps the hash stable.
    $mainBranchName = Get-MetadataValue -Container $Data.metadata -Name 'mainBranch'
    $modeForHash = Get-MetadataValue -Container $Data.metadata -Name 'mode' -Default 'in-flight'
    if (-not $modeForHash) { $modeForHash = 'in-flight' }
    $shippedInfoForHash = if ($modeForHash -eq 'shipped' -and $Data.ContainsKey('shippedInfo') -and $Data['shippedInfo']) {
        $Data['shippedInfo']
    } else { $null }
    $shipDateForHash = if ($shippedInfoForHash) {
        ConvertTo-Utc -Value (Get-MetadataValue -Container $shippedInfoForHash -Name 'tagDate')
    } else { $null }
    $shipVersionForHash = if ($shippedInfoForHash) {
        [string](Get-MetadataValue -Container $shippedInfoForHash -Name 'version')
    } else { '' }
    $shipDateSourceForHash = if ($shippedInfoForHash) {
        [string](Get-MetadataValue -Container $shippedInfoForHash -Name 'dateSource')
    } else { '' }
    $shipPublicationStateForHash = if ($shippedInfoForHash) {
        [string](Get-MetadataValue -Container $shippedInfoForHash -Name 'publicationState' -Default 'unknown')
    } else { '' }
    $shipSrForHash = if ($shippedInfoForHash) {
        [int](Get-MetadataValue -Container $shippedInfoForHash -Name 'srNumber' -Default 0)
    } else { 0 }
    $shipMajorForHash = if ($shippedInfoForHash) {
        [int](Get-MetadataValue -Container $shippedInfoForHash -Name 'major' -Default 0)
    } else { 0 }
    $shipSubPatchForHash = if ($shippedInfoForHash) {
        Get-SrSubPatchFromVersion -Version $shipVersionForHash
    } else { 0 }

    $semantic = [ordered]@{
        verdict = $Verdict.symbol
        # Tracker lifecycle mode (candidate / in-flight / shipped). Folded in so a
        # lifecycle TRANSITION always flips the hash and refreshes the tracker —
        # even when every other hashed field is byte-for-byte identical across the
        # flip. This matters most at in-flight -> shipped: `-Shipped` surveys the
        # SAME SR branch as in-flight, so srHead, ci, srPrs, regressions, shipChecks
        # and nightlyFeed can all be unchanged at the moment the stable tag
        # publishes. Without `mode` here, hash(shipped)
        # == hash(in-flight), the workflow's idempotent no-op skips `gh issue edit`,
        # and the tracker never visually flips to "shipped" (the whole point of the
        # shipped lifecycle). `mode` is constant within a mode, so it adds NO daily
        # churn — only the one-time transition refreshes. Default 'in-flight' when
        # absent, matching Format-MarkdownReport's $mode default.
        mode = $modeForHash
        shippedAnchor = if ($shippedInfoForHash) {
            $shipDateToken = if ($shipDateForHash) { $shipDateForHash.ToString('o') } else { '' }
            $hotfixVersion = [string](Get-MetadataValue -Container $shippedInfoForHash -Name 'liveVersion')
            $hotfixInProgress = [bool](Get-MetadataValue -Container $shippedInfoForHash -Name 'hotfixInProgress' -Default $false)
            "$shipVersionForHash|$shipDateToken|$shipDateSourceForHash|$shipPublicationStateForHash|$hotfixInProgress|$hotfixVersion"
        } else { '' }
        regressionScan = if ([bool](Get-MetadataValue -Container $Data -Name 'regressionScanIncomplete' -Default $false)) {
            $failedLabels = @(@(Get-NonEmptyStringValues -Value (Get-MetadataValue -Container $Data -Name 'regressionFailedLabels')) | Sort-Object)
            "incomplete|$($failedLabels -join ',')"
        } else { 'complete' }
        survey = if ([bool](Get-MetadataValue -Container $Data -Name 'surveyIncomplete' -Default $false)) {
            "partial|$(Get-MetadataValue -Container $Data -Name 'surveyIncompleteReason')"
        } else { 'complete' }
        srHead = Get-MetadataValue -Container $Data.metadata -Name 'srHeadSha'
        ciOverall = if ($Data.ContainsKey('ci') -and $Data['ci']) { $Data['ci'].overall } else { $null }
        srPrs = if ($Data.ContainsKey('srContents') -and $Data['srContents']) {
                    @($Data['srContents'].sourcePrs | Sort-Object) -join ','
                } else { '' }
        regressions = if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
                          @($Data['regressions'] | Sort-Object issue | ForEach-Object {
                              $cfp = $null
                              if ($_ -is [System.Collections.IDictionary]) {
                                  if ($_.Contains('candidateFixPrs')) { $cfp = $_['candidateFixPrs'] }
                              } elseif ($_.PSObject.Properties['candidateFixPrs']) {
                                  $cfp = $_.candidateFixPrs
                              }
                              $selPrNum = ''
                              if ($cfp) {
                                  $sel = Select-OpenMainFixPr -CandidateFixPrs $cfp -MainBranch $mainBranchName
                                  if ($sel) {
                                      if ($sel -is [System.Collections.IDictionary]) {
                                          if ($sel.Contains('number')) { $selPrNum = $sel['number'] }
                                      } elseif ($sel.PSObject.Properties['number']) {
                                          $selPrNum = $sel.number
                                      }
                                  }
                              }
                              $recAct = ''
                              if ($_ -is [System.Collections.IDictionary]) {
                                  if ($_.Contains('recommendedAction')) { $recAct = [string]$_['recommendedAction'] }
                              } elseif ($_.PSObject.Properties['recommendedAction']) {
                                  $recAct = [string]$_.recommendedAction
                              }
                              $lifecycleBucket = ''
                              $regressionState = [string](Get-MetadataValue -Container $_ -Name 'state' -Default 'OPEN')
                              $regressionTitle = [string](Get-MetadataValue -Container $_ -Name 'title' -Default '')
                              if ($modeForHash -eq 'shipped') {
                                  $effectiveTier = Get-EffectiveVerdictTier -Classification $_.classification -Mode $modeForHash -State $regressionState
                                  if ($effectiveTier -lt 3) {
                                      $isCarryForward = Test-IsCarryForwardRegression -Regression $_ `
                                          -ShippedSrNumber $shipSrForHash -ShippedMajor $shipMajorForHash -ShippedSubPatch $shipSubPatchForHash
                                      $lifecycleBucket = if ($isCarryForward) { 'carry-forward' } else { 'follow-up' }
                                  }
                              }
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
                              $modeTierSuffix = if ($modeForHash -eq 'candidate' -and
                                  $_.classification -eq 'merged-on-main-no-backport') {
                                  $effectiveTier = Get-EffectiveVerdictTier -Classification $_.classification `
                                      -Mode $modeForHash -State $regressionState
                                  ":tier$effectiveTier"
                              } else { '' }
                              if ($_.classification -eq 'no-fix-yet') {
                                  $nfyTier = if ($regressionState -eq 'OPEN') { 't1' } else { 't3' }
                                  "$($_.issue):${regressionTitle}:$($_.classification):$($nfyTier):$($lifecycleBucket):$($selPrNum):$recAct$modeTierSuffix"
                              } else {
                                  "$($_.issue):${regressionTitle}:$($_.classification):$($lifecycleBucket):$($selPrNum):$recAct$modeTierSuffix"
                              }
                          }) -join '|'
                      } else { '' }
        openSrPrs = if ($Data.ContainsKey('openSrPrs') -and $Data['openSrPrs']) {
                        @($Data['openSrPrs'] | Sort-Object number | ForEach-Object {
                            $author = Get-MetadataValue -Container $_ -Name 'author'
                            @(
                                Get-MetadataValue -Container $_ -Name 'number'
                                Get-MetadataValue -Container $_ -Name 'title'
                                Get-MetadataValue -Container $author -Name 'login'
                                [bool](Get-MetadataValue -Container $_ -Name 'isDraft' -Default $false)
                                Get-MetadataValue -Container $_ -Name 'reviewDecision'
                                Get-MetadataValue -Container $_ -Name 'updatedAt'
                            ) -join '~'
                        }) -join '|'
                    } else { '' }
        candidatePr = if ($Data.ContainsKey('candidatePr') -and $Data['candidatePr']) {
                          $candidateResolution = $Data['candidatePr']
                          $candidateItems = @(Get-MetadataValue -Container $candidateResolution -Name 'candidates')
                          $primaryCandidate = $candidateItems | Select-Object -First 1
                          $candidateNow = ConvertTo-Utc -Value (Get-MetadataValue -Container $Data.metadata -Name 'fetchedAt')
                          $primaryAge = Get-CandidatePrAge -Candidate $primaryCandidate -Now $candidateNow
                          [ordered]@{
                              mode = Get-MetadataValue -Container $candidateResolution -Name 'mode'
                              nextSr = Get-MetadataValue -Container $candidateResolution -Name 'nextSr'
                              versionBase = Get-MetadataValue -Container $candidateResolution -Name 'versionBase'
                              spoofers = Get-MetadataValue -Container $candidateResolution -Name 'spoofers' -Default 0
                              unverifiable = Get-MetadataValue -Container $candidateResolution -Name 'unverifiable' -Default 0
                              primaryNumber = Get-MetadataValue -Container $primaryCandidate -Name 'number'
                              primaryAgeBucket = $primaryAge.Bucket
                              candidates = @(
                                  $candidateItems |
                                      Sort-Object { Get-MetadataValue -Container $_ -Name 'number' } |
                                      ForEach-Object {
                                          $candidate = $_
                                          $candidateAuthor = Get-MetadataValue -Container $candidate -Name 'author'
                                          [ordered]@{
                                              number = Get-MetadataValue -Container $candidate -Name 'number'
                                              title = Get-MetadataValue -Container $candidate -Name 'title'
                                              author = Get-MetadataValue -Container $candidateAuthor -Name 'login'
                                              state = Get-MetadataValue -Container $candidate -Name 'state'
                                              isDraft = [bool](Get-MetadataValue -Container $candidate -Name 'isDraft' -Default $false)
                                              mergeable = Get-MetadataValue -Container $candidate -Name 'mergeable'
                                              reviewDecision = Get-MetadataValue -Container $candidate -Name 'reviewDecision'
                                          }
                                      }
                              )
                          }
                      } else { $null }
        shipChecks = if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
                         @($Data['shipChecks'] | Sort-Object Area | ForEach-Object {
                             $details = [string](Get-MetadataValue -Container $_ -Name 'Details' -Default '')
                             $na = ''
                             if ($_ -is [System.Collections.IDictionary]) {
                                 if ($_.Contains('NextAction')) { $na = [string]$_['NextAction'] }
                             } elseif ($_.PSObject.Properties['NextAction']) {
                                 $na = [string]$_.NextAction
                             }
                             "$($_.Area):$($_.Status):${details}:$na"
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

function Select-OpenMainFixPr {
    <#
    .SYNOPSIS
        From a regression record's candidate fix PRs, pick the OPEN PR that
        actually drove the 'open-on-main' verdict: the one targeting main.

    .DESCRIPTION
        The classifier reports 'open-on-main' when ANY candidate is an OPEN PR
        against main, but candidateFixPrs can hold several OPEN PRs in arbitrary
        order (e.g. an inflight/current PR first, the main PR second). The
        Open-Fix-PRs-Inbound renderer must surface the main-targeting PR — the
        one that owns the '🔵 awaiting main merge' status and the /backport
        action — not merely the first OPEN candidate, which would attach that
        row to the wrong PR and hide the PR that drove the verdict. Falls back
        to the first OPEN candidate when none targets main (defensive; preserves
        the prior single-candidate behavior).
    #>
    param($CandidateFixPrs, [string]$MainBranch)
    $open = @($CandidateFixPrs | Where-Object {
        $state = $null
        if ($_ -is [System.Collections.IDictionary]) {
            if ($_.Contains('state')) { $state = $_['state'] }
        } elseif ($_.PSObject.Properties['state']) {
            $state = $_.state
        }
        $state -eq 'OPEN'
    })
    if ($open.Count -eq 0) { return $null }
    # Only attempt the main-targeting match when we actually know the main branch.
    # $MainBranch is typed [string], so a $null caller argument arrives as ''.
    # Without this guard 'baseRef -eq ""' would match a candidate whose own
    # baseRef is empty/missing, wrongly selecting it and defeating the intended
    # first-OPEN fallback below.
    if (-not [string]::IsNullOrEmpty($MainBranch)) {
        $onMain = $open | Where-Object {
            $baseRef = $null
            if ($_ -is [System.Collections.IDictionary]) {
                if ($_.Contains('baseRef')) { $baseRef = $_['baseRef'] }
            } elseif ($_.PSObject.Properties['baseRef']) {
                $baseRef = $_.baseRef
            }
            $baseRef -eq $MainBranch
        } | Select-Object -First 1
        if ($onMain) { return $onMain }
    }
    return $open | Select-Object -First 1
}

function Format-MarkdownReport {
    param(
        $Data,
        [string]$RepoUrl,
        [string]$TrackerKey,
        [int]$MaxBodyBytes = 60000,
        [bool]$PublicSafe = $true
    )
    $Data = ConvertTo-TopLevelDictionary -Container $Data

    $ctx = $Data.metadata
    $srBranch = $ctx.srBranch
    # Main branch, read shape-safe via the shared accessor: real reports carry it
    # in metadata, but some renderer fixtures/callers omit it, and metadata may be
    # a hashtable (live survey) OR a pscustomobject (JSON round-trip). A null value
    # makes Select-OpenMainFixPr fall back to the first OPEN candidate.
    $mainBranchName = Get-MetadataValue -Container $ctx -Name 'mainBranch'
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
    $hotfixMarkerInfo = Get-MetadataValue -Container $Data -Name 'shippedInfo'
    $shippedMarkerVersion = [string](Get-MetadataValue -Container $hotfixMarkerInfo -Name 'version')
    if ($shippedMarkerVersion) {
        [void]$sb.AppendLine("<!-- release-readiness-shipped: $shippedMarkerVersion -->")
    }
    if ([bool](Get-MetadataValue -Container $hotfixMarkerInfo -Name 'hotfixInProgress' -Default $false)) {
        $hotfixMarkerVersion = [string](Get-MetadataValue -Container $hotfixMarkerInfo -Name 'liveVersion')
        $hotfixMarkerCommit = [string](Get-MetadataValue -Container $Data.metadata -Name 'srHeadSha')
        if (-not $hotfixMarkerVersion) { $hotfixMarkerVersion = 'version-pending' }
        if ($hotfixMarkerCommit) {
            [void]$sb.AppendLine("<!-- release-readiness-hotfix: $hotfixMarkerVersion@$hotfixMarkerCommit -->")
        }
    }

    # $mode / $inherits, read shape-safe via the shared accessor for the SAME
    # reason as $mainBranchName above (metadata may be a hashtable during a survey
    # or a pscustomobject after a JSON round-trip; key/property absent -> the
    # documented 'in-flight' / $false defaults).
    $mode = Get-MetadataValue -Container $ctx -Name 'mode' -Default 'in-flight'
    if (-not $mode) { $mode = 'in-flight' }
    $inherits = [bool](Get-MetadataValue -Container $ctx -Name 'inheritFromPriorSr' -Default $false)

    # Shipped-render context — resolved once and reused by the header, the ship-date
    # line, and the post-ship follow-up summary. Reads the actual stable-tag ship
    # date + shipped SR number/major from $Data.shippedInfo (Invoke-Main populates
    # it). Absent → carry-forward detection finds nothing and every follow-up stays
    # actionable (conservative).
    $isShippedRender = ($mode -eq 'shipped')
    $shippedInfoRender = if ($isShippedRender -and $Data.ContainsKey('shippedInfo') -and $Data['shippedInfo']) { $Data['shippedInfo'] } else { $null }
    $shipTagDateRender = if ($shippedInfoRender) { ConvertTo-Utc -Value (Get-MetadataValue -Container $shippedInfoRender -Name 'tagDate') } else { $null }
    $shipTagVersion    = if ($shippedInfoRender) { Get-MetadataValue -Container $shippedInfoRender -Name 'version' } else { $null }
    $shipTagDateSource = if ($shippedInfoRender) { Get-MetadataValue -Container $shippedInfoRender -Name 'dateSource' } else { $null }
    $shipTagSrNum      = if ($shippedInfoRender) { [int](Get-MetadataValue -Container $shippedInfoRender -Name 'srNumber' -Default 0) } else { 0 }
    $shipTagMajor      = if ($shippedInfoRender) { [int](Get-MetadataValue -Container $shippedInfoRender -Name 'major' -Default 0) } else { 0 }
    $shipTagSubPatch   = if ($shippedInfoRender) { Get-SrSubPatchFromVersion -Version $shipTagVersion } else { 0 }

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
        if ($isShippedRender) {
            [void]$sb.AppendLine()
            if ($shipTagDateRender -and $shipTagDateSource -eq 'github-release') {
                $shipVerLabel = if ($shipTagVersion) { "$shipTagVersion " } else { '' }
                [void]$sb.AppendLine("> 📦 **Shipped ${shipVerLabel}on $($shipTagDateRender.ToString('yyyy-MM-dd')) (post-ship tracker).** ``$srBranch`` has already tagged. Findings below are post-ship **follow-ups / carry-forward** (hotfix or next-SR decisions), not ship blockers.")
            } elseif ($shipTagDateRender) {
                $shipVerLabel = if ($shipTagVersion) { "$shipTagVersion " } else { '' }
                [void]$sb.AppendLine("> 📦 **Shipped ${shipVerLabel}(post-ship tracker; tagged-content anchor $($shipTagDateRender.ToString('yyyy-MM-dd'))).** ``$srBranch`` has already tagged. Findings below are post-ship **follow-ups / carry-forward** (hotfix or next-SR decisions), not ship blockers.")
            } else {
                [void]$sb.AppendLine("> 📦 **Shipped SR (post-ship tracker).** ``$srBranch`` has already tagged. Findings below are post-ship **follow-ups / carry-forward** (hotfix or next-SR decisions), not ship blockers.")
            }
        }
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
    # Report freshness banner — a DERIVED-AT-RENDER note of how long ago this report was
    # generated, with a ⏳ "may be stale" flag past the threshold. Computed here from the
    # generation timestamp against a render-time clock; it is a pure presentation concern and
    # is DELIBERATELY NOT folded into Get-ReportSemanticHash (hashing a render-time age would
    # differ every run and break the idempotent no-op). Fail-open: skip if the helper isn't
    # loaded or the timestamp is unreadable.
    if ($ctx.fetchedAt -and (Get-Command Format-ReportFreshnessBanner -ErrorAction SilentlyContinue)) {
        $freshnessBanner = Format-ReportFreshnessBanner -GeneratedAt $ctx.fetchedAt -Now ([DateTime]::UtcNow)
        if ($freshnessBanner) {
            [void]$sb.AppendLine()
            [void]$sb.AppendLine($freshnessBanner)
        }
    }
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
    if ($isShippedRender) {
        # The SR already tagged — show the public release date when available,
        # otherwise label the tagged-commit fallback precisely. Suppress the overdue
        # "window passed" warning either way (a shipped SR is not "late").
        if ($shipTagDateRender) {
            $shipVerLabel2 = if ($shipTagVersion) { "$shipTagVersion " } else { '' }
            if ($shipTagDateSource -eq 'github-release') {
                [void]$sb.AppendLine("**Shipped**: 📦 ${shipVerLabel2}on $($shipTagDateRender.ToString('dddd MMMM d, yyyy')) (GitHub Release published) — post-ship tracker; ship-window checks no longer apply.")
            } else {
                [void]$sb.AppendLine("**Tag content anchor**: 📦 ${shipVerLabel2}$($shipTagDateRender.ToString('dddd MMMM d, yyyy')) (tagged commit date; public release timestamp unavailable) — post-ship tracker; ship-window checks no longer apply.")
            }
        } else {
            [void]$sb.AppendLine("**Shipped**: 📦 This SR has already tagged — post-ship tracker; ship-window checks no longer apply.")
        }
    } elseif ($shipDate.Cadence -eq 'asap-hotfix') {
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
    $surveyIncompleteRender = [bool](Get-MetadataValue -Container $Data -Name 'surveyIncomplete' -Default $false)
    if ($surveyIncompleteRender) {
        $partialReason = Get-MetadataValue -Container $Data -Name 'surveyIncompleteReason' -Default 'Partial survey did not query every readiness axis.'
        [void]$sb.AppendLine("> ⚠️ **Partial survey — not a global ship verdict.** $partialReason Rerun with ``-Phase all`` for the complete assessment.")
        [void]$sb.AppendLine()
    }

    # === BLOCKING / POST-SHIP FOLLOW-UP SUMMARY (hoisted to top, under verdict) ===
    # In-flight/candidate: every BLOCKED ship-check + Tier 1 regression is a ship
    # blocker. Shipped: the SR already tagged, so the same items become post-ship
    # FOLLOW-UPS (hotfix / next-SR decisions), and Tier-1 regressions explicitly
    # milestoned to a later SR are split into a separate, non-gating list so they
    # stay VISIBLE without implying the shipped release is broken.
    $blockingItems = New-Object System.Collections.Generic.List[hashtable]
    $blockedShipCheckItems = New-Object System.Collections.Generic.List[hashtable]
    $releaseContentFollowUpItems = New-Object System.Collections.Generic.List[hashtable]
    $carryForwardItems = New-Object System.Collections.Generic.List[hashtable]
    $regressionScanIncompleteRender = [bool](Get-MetadataValue -Container $Data -Name 'regressionScanIncomplete' -Default $false)
    if ($Data.ContainsKey('shipChecks') -and $Data['shipChecks']) {
        foreach ($sc in $Data['shipChecks']) {
            $isHotfixWatch = $isShippedRender -and
                $sc.Status -eq 'WATCH' -and
                $sc.Area -eq 'Unpublished hotfix branch state'
            $isShippedFollowUp = $isShippedRender -and $sc.Status -in @('BLOCKED', 'WATCH', 'UNKNOWN')
            if ($sc.Status -eq 'BLOCKED' -or $isHotfixWatch -or $isShippedFollowUp) {
                $shipCheckItem = @{
                    area = "🛠️ $($sc.Area)"
                    details = $sc.Details
                    action = $sc.NextAction
                }
                if ($isShippedRender -and $sc.Area -eq 'P/0 release-branch PRs') {
                    [void]$releaseContentFollowUpItems.Add($shipCheckItem)
                } else {
                    [void]$blockedShipCheckItems.Add($shipCheckItem)
                }
            }
        }
    }
    if ($Data.ContainsKey('regressions') -and $Data['regressions']) {
        foreach ($r in $Data['regressions']) {
            $regressionState = [string](Get-MetadataValue -Container $r -Name 'state' -Default 'OPEN')
            $tier = Get-EffectiveVerdictTier -Classification $r.classification -Mode $mode -State $regressionState
            # In normal release modes only Tier 1 belongs in the hoisted blocking
            # summary. After ship, Tier 1 and Tier 2 are follow-ups/hotfix decisions,
            # so hoist both; otherwise the report can claim "No post-ship follow-ups"
            # while listing a needs-human-review row later in the document.
            if ($tier -eq 1 -or ($isShippedRender -and $tier -eq 2)) {
                $issLink = "[#$($r.issue)]($RepoUrl/issues/$($r.issue))"
                $item = @{
                    area = "🐞 $issLink — $($r.classification)"
                    details = $r.title
                    action = $r.recommendedAction
                }
                if ($isShippedRender -and (Test-IsCarryForwardRegression -Regression $r `
                        -ShippedSrNumber $shipTagSrNum -ShippedMajor $shipTagMajor -ShippedSubPatch $shipTagSubPatch)) {
                    [void]$carryForwardItems.Add($item)
                } else {
                    [void]$blockingItems.Add($item)
                }
            }
        }
    }

    if ($isShippedRender) {
        if ($regressionScanIncompleteRender) {
            $incompleteReasons = @(Get-NonEmptyStringValues -Value (Get-MetadataValue -Container $Data -Name 'regressionFailedLabels'))
            $incompleteReasonText = if ($incompleteReasons.Count -gt 0) { " ($($incompleteReasons -join ', '))" } else { '' }
            [void]$sb.AppendLine("## ⚠️ Urgent follow-ups unknown — regression scan incomplete")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("_Regression scan did not complete$incompleteReasonText. See the Verdict and Warnings above, then rerun with the appropriate labels, phase, or ``-MaxIssues`` before treating this shipped tracker as clean._")
            [void]$sb.AppendLine()
        }
        if ($blockingItems.Count -gt 0) {
            [void]$sb.AppendLine("## 📌 Post-ship regression follow-ups — $($blockingItems.Count) item(s)")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("_``$srBranch`` already shipped — these did NOT block the release. Each needs a human hotfix-vs-next-SR decision._")
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
        }
        if ($blockedShipCheckItems.Count -gt 0) {
            [void]$sb.AppendLine("## 🛠️ Post-ship operational follow-ups — $($blockedShipCheckItems.Count) item(s)")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("_Lifecycle or configuration checks still need direct remediation after ship. Follow each row's specific next action; these are not regression hotfix-vs-next-SR decisions._")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('| Area | Details | Next action |')
            [void]$sb.AppendLine('|---|---|---|')
            foreach ($b in $blockedShipCheckItems) {
                $area = Format-MarkdownTableCell $b.area
                $details = Format-MarkdownTableCell $b.details
                $action = Format-MarkdownTableCell $b.action
                [void]$sb.AppendLine("| $area | $details | $action |")
            }
            [void]$sb.AppendLine()
        }
        if ($releaseContentFollowUpItems.Count -gt 0) {
            [void]$sb.AppendLine("## 🚩 Post-ship release-content decisions — $($releaseContentFollowUpItems.Count) item(s)")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("_Release-critical content is still open after ship. Decide whether to land it as a hotfix, carry it to the next SR, or explicitly de-prioritize it._")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('| Area | Details | Next action |')
            [void]$sb.AppendLine('|---|---|---|')
            foreach ($b in $releaseContentFollowUpItems) {
                $area = Format-MarkdownTableCell $b.area
                $details = Format-MarkdownTableCell $b.details
                $action = Format-MarkdownTableCell $b.action
                [void]$sb.AppendLine("| $area | $details | $action |")
            }
            [void]$sb.AppendLine()
        }
        if (-not $surveyIncompleteRender -and -not $regressionScanIncompleteRender -and $blockingItems.Count -eq 0 -and
            $blockedShipCheckItems.Count -eq 0 -and $releaseContentFollowUpItems.Count -eq 0 -and
            $carryForwardItems.Count -eq 0) {
            [void]$sb.AppendLine("## 🟢 No urgent post-ship follow-ups")
            [void]$sb.AppendLine()
        }
        if ($carryForwardItems.Count -gt 0) {
            [void]$sb.AppendLine("## 🔁 Carry-forward — $($carryForwardItems.Count) item(s)")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("_Regressions explicitly milestoned to a later SR. Non-gating for this shipped SR — tracked forward to the next cycle._")
            [void]$sb.AppendLine()
            [void]$sb.AppendLine('| Area | Details | Next action |')
            [void]$sb.AppendLine('|---|---|---|')
            foreach ($b in $carryForwardItems) {
                $area = Format-MarkdownTableCell $b.area
                $details = Format-MarkdownTableCell $b.details
                $action = Format-MarkdownTableCell $b.action
                [void]$sb.AppendLine("| $area | $details | $action |")
            }
            [void]$sb.AppendLine()
        }
    }
    elseif (($blockingItems.Count + $blockedShipCheckItems.Count + $releaseContentFollowUpItems.Count) -gt 0) {
        $blockingCount = $blockingItems.Count + $blockedShipCheckItems.Count + $releaseContentFollowUpItems.Count
        [void]$sb.AppendLine("## 🔴 Blocking — $blockingCount item(s)")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine('| Area | Details | Next action |')
        [void]$sb.AppendLine('|---|---|---|')
        foreach ($b in @($blockedShipCheckItems) + @($releaseContentFollowUpItems) + @($blockingItems)) {
            $area = Format-MarkdownTableCell $b.area
            $details = Format-MarkdownTableCell $b.details
            $action = Format-MarkdownTableCell $b.action
            [void]$sb.AppendLine("| $area | $details | $action |")
        }
        [void]$sb.AppendLine()
    } elseif ($surveyIncompleteRender -or $regressionScanIncompleteRender) {
        $incompleteLabel = if ($surveyIncompleteRender) { 'partial survey' } else { 'regression scan incomplete' }
        [void]$sb.AppendLine("## ⚠️ Blocking status incomplete — $incompleteLabel")
        [void]$sb.AppendLine()
    } else {
        [void]$sb.AppendLine("## 🟢 No blocking items")
        [void]$sb.AppendLine()
    }

    # === CANDIDATE PR — SR cut point (hoisted directly under the Blocking summary) ===
    # In candidate mode the single most important PR in the cycle is the one that
    # promotes a specific `main` commit as the SR cut point: the SR branch cannot
    # be cut until it merges. Surface it prominently — with live status and age —
    # right under the blocking summary instead of leaving it buried as a lone WATCH
    # row in the ship-readiness table at the bottom.
    if ($mode -eq 'candidate' -and $Data.ContainsKey('candidatePr') -and $Data['candidatePr']) {
        $cpr = $Data['candidatePr']
        $srLabel = if ($cpr.nextSr) { $cpr.nextSr } else { 'next SR' }
        $verLabel = if ($cpr.versionBase) { " ($($cpr.versionBase))" } else { '' }
        # Report generation instant — used as the "now" reference for PR age so the
        # rendered "N days" is deterministic (driven by the report, not wall-clock).
        $nowRef = ConvertTo-Utc -Value $ctx.fetchedAt

        [void]$sb.AppendLine("## 🚩 Candidate PR — $srLabel cut point$verLabel")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("_The Candidate PR promotes a specific ``$($ctx.mainBranch)`` commit as the base for cutting **$srLabel$verLabel**. The SR branch can't be cut until it merges, so its status gates the whole cycle._")
        [void]$sb.AppendLine()

        if ($cpr.mode -eq 'query-failed') {
            [void]$sb.AppendLine("> ⚠️ Could not query open PRs on ``$($ctx.mainBranch)`` (``gh pr list`` failed). Candidate-PR status is unavailable — verify ``gh auth status`` and rerun.")
            [void]$sb.AppendLine()
        }
        elseif (@($cpr.candidates).Count -eq 0) {
            $exNotes = @()
            if ($cpr.spoofers -gt 0)     { $exNotes += "$($cpr.spoofers) non-maintainer ``*Candidate*``-titled PR(s) were excluded as not real cut PRs" }
            if ($cpr.unverifiable -gt 0) { $exNotes += "$($cpr.unverifiable) ``*Candidate*``-titled PR(s) could not be author-verified and were excluded fail-closed — rerun to re-check" }
            $exSuffix = if ($exNotes.Count -gt 0) { ' ' + ($exNotes -join '; ') + '.' } else { '' }
            [void]$sb.AppendLine("**No open Candidate PR yet.** When ready to cut $srLabel, open a PR titled ``*Candidate*`` against ``$($ctx.mainBranch)`` selecting the target commit — the SR is cut from its merge commit.$exSuffix")
            [void]$sb.AppendLine()
        }
        else {
            [void]$sb.AppendLine('| PR | Status | Opened | Last update | Review |')
            [void]$sb.AppendLine('|---|---|---|---|---|')
            foreach ($cp in @($cpr.candidates)) {
                $cpLink = ConvertTo-LinkedPr -PrNumber $cp.number -RepoUrl $RepoUrl
                $cpTitleRaw = if ($cp.title.Length -gt 60) { $cp.title.Substring(0, 60) + '...' } else { $cp.title }
                $cpTitle = Format-MarkdownTableCell $cpTitleRaw
                $author = if ($cp.author -and $cp.author.login) { Format-GitHubHandle $cp.author.login } else { 'unknown' }

                # Status: lead with a non-contradictory summary that prioritizes
                # BLOCKING facts (draft, merge conflicts, review required / changes
                # requested) ahead of non-blocking facts (mergeable, approved, ready
                # for review). All component fields are optional; degrade gracefully.
                # The old cell rendered "✅ Ready · ⚠️ conflicts" — a review-required,
                # conflicting PR is NOT "Ready", so blocking facts now come first and
                # the not-draft state is labelled "Ready for review" (not "Ready").
                $isDraft = [bool](Get-MetadataValue -Container $cp -Name 'isDraft' -Default $false)
                $mergeState = "$((Get-MetadataValue -Container $cp -Name 'mergeable' -Default ''))".ToUpperInvariant()
                $reviewState = "$((Get-MetadataValue -Container $cp -Name 'reviewDecision' -Default ''))".ToUpperInvariant()

                $blockingFacts = @()
                if ($isDraft) { $blockingFacts += '📝 Draft' }
                if ($mergeState -eq 'CONFLICTING') { $blockingFacts += '⚠️ conflicts' }
                if ($reviewState -eq 'CHANGES_REQUESTED') { $blockingFacts += '⚠️ changes requested' }
                elseif ($reviewState -eq 'REVIEW_REQUIRED') { $blockingFacts += 'review required' }

                $okFacts = @()
                if ($mergeState -eq 'MERGEABLE') { $okFacts += 'mergeable' }
                if ($reviewState -eq 'APPROVED') { $okFacts += 'approved' }
                if (-not $isDraft) { $okFacts += 'Ready for review' }

                $facts = @($blockingFacts) + @($okFacts)
                $leadSymbol = if ($blockingFacts.Count -gt 0) { '🟠' } else { '🟢' }
                $statusCell = if ($facts.Count -gt 0) {
                    "$leadSymbol Open · " + ($facts -join ' · ')
                } else {
                    "$leadSymbol Open"
                }

                # Age of the PR (created) and last activity (updated), relative to $nowRef.
                $openedCell = '—'
                $createdUtc = ConvertTo-Utc -Value $cp.createdAt
                if ($createdUtc) {
                    if ($nowRef) {
                        $a = [int][Math]::Floor(($nowRef - $createdUtc).TotalDays)
                        $openedCell = "$($createdUtc.ToString('yyyy-MM-dd')) ($a day$(if ($a -eq 1){''}else{'s'}) ago)"
                    } else {
                        $openedCell = $createdUtc.ToString('yyyy-MM-dd')
                    }
                }
                $updatedCell = '—'
                $updatedUtc = ConvertTo-Utc -Value $cp.updatedAt
                if ($updatedUtc) {
                    if ($nowRef) {
                        $u = [int][Math]::Floor(($nowRef - $updatedUtc).TotalDays)
                        $updatedCell = "$u day$(if ($u -eq 1){''}else{'s'}) ago"
                    } else {
                        $updatedCell = $updatedUtc.ToString('yyyy-MM-dd')
                    }
                }
                $reviewCell = if ($cp.reviewDecision) { $cp.reviewDecision } else { '—' }

                [void]$sb.AppendLine("| $cpLink — $cpTitle (by $author) | $statusCell | $openedCell | $updatedCell | $reviewCell |")
            }
            [void]$sb.AppendLine()

            # Relevance / staleness callout for the primary candidate, tied to the SR
            # version base and the ship window. A cut PR that has sat open a long time
            # likely points at a now-stale `main` commit and should be re-confirmed.
            $primary = @($cpr.candidates)[0]
            $primaryAge = Get-CandidatePrAge -Candidate $primary -Now $nowRef
            $pAge = $primaryAge.AgeDays
            $shipBit = if ($shipDate -and $shipDate.FormattedLong) {
                if ($shipDate.DaysFromNow -ge 0) { " Ship target: **$($shipDate.FormattedLong)** (in $($shipDate.DaysFromNow) day(s))." }
                else { " Ship target **$($shipDate.FormattedLong)** has passed." }
            } else { '' }
            if ($null -ne $pAge -and $pAge -ge 14) {
                [void]$sb.AppendLine("> ⚠️ **Stale ($pAge days old).** ``$($ctx.mainBranch)`` has advanced since this PR was opened — confirm the target cut commit is still the intended $srLabel base, or refresh/re-point the PR before cutting.$shipBit")
            } else {
                [void]$sb.AppendLine("> Merge this PR to lock the $srLabel$verLabel cut point, then cut the SR branch from its merge commit.$shipBit")
            }
            [void]$sb.AppendLine()
            [void]$sb.AppendLine("_Other open PRs on ``$($ctx.mainBranch)`` are omitted here to reduce noise; see [the full PR list]($RepoUrl/pulls?q=is%3Apr+is%3Aopen+base%3A$($ctx.mainBranch))._")
            [void]$sb.AppendLine()
        }
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
                        $postShipBackport = ($mode -eq 'shipped')
                        [void]$openFixRows.Add(@{
                            prCell  = $prLink
                            baseCell = "``$srBranch``"
                            issCell = $issCell
                            statusCell = if ($postShipBackport) { '🟡 backport OPEN — post-ship decision' } else { '🟡 backport OPEN on SR' }
                            actionCell = if ($postShipBackport) {
                                'Decide whether this backport should land as a hotfix or close in favor of the next SR; it is not a retroactive ship blocker.'
                            } else {
                                'Land this PR before ship'
                            }
                        })
                        break
                    }
                }
            } else {
                # open-on-main: fix PR is OPEN against main (or another non-SR base).
                # Surface the OPEN PR that actually drove the verdict — the one
                # targeting main — not merely the first OPEN candidate. A mixed
                # candidate list (inflight PR first, main PR second) would
                # otherwise render the inflight PR with the wrong '🔵 awaiting
                # main merge' row + /backport action, hiding the main PR.
                $openMain = Select-OpenMainFixPr -CandidateFixPrs $r.candidateFixPrs -MainBranch $mainBranchName
                if ($openMain) {
                    $prLink = "[#$($openMain.number)]($RepoUrl/pull/$($openMain.number))"
                    $base = if ($openMain.baseRef) { "``$($openMain.baseRef)``" } else { '`main`' }
                    $postShipOpenFix = ($mode -eq 'shipped')
                    [void]$openFixRows.Add(@{
                        prCell  = $prLink
                        baseCell = $base
                        issCell = $issCell
                        statusCell = if ($postShipOpenFix) { '🔵 OPEN — post-ship fix decision' } else { '🔵 OPEN — awaiting main merge' }
                        actionCell = if ($mode -eq 'candidate') {
                            'Watch for merge to main before the SR cut; rerun readiness after the release/...-srN branch is cut to get the exact backport command'
                        } elseif ($postShipOpenFix) {
                            "After merge, decide whether this warrants a hotfix to the shipped SR or should ride the next SR; no automatic current-SR backport applies."
                        } else {
                            "Watch for merge, then post ``/backport to $srBranch`` on the merged source PR"
                        }
                    })
                }
            }
        }

        if ($openFixRows.Count -gt 0) {
            $openFixHeading = if ($mode -eq 'shipped') { 'Open Fix PRs Post-ship' } else { 'Open Fix PRs Inbound' }
            [void]$sb.AppendLine("## 📥 $openFixHeading — $($openFixRows.Count) PR(s)")
            [void]$sb.AppendLine()
            if ($mode -eq 'shipped') {
                [void]$sb.AppendLine('_Fix PRs discovered after the SR shipped. Track them for a human hotfix-vs-next-SR decision; they are not retroactive ship blockers._')
            } else {
                [void]$sb.AppendLine('_Fix PRs in flight for regression issues. Land these (or their backports) before ship to close out the regression list._')
            }
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
        $inheritedCommitCount = [int](Get-MetadataValue -Container $sc -Name 'inheritedCommitCount' -Default 0)
        if ($inherits -and $inheritedCommitCount -gt 0) {
            [void]$sb.AppendLine("- **From main** (since prior SR): $($sc.primaryCommitCount) commits / $($sc.primarySourcePrs.Count) source PRs")
            [void]$sb.AppendLine("- **Inherited from $($ctx.priorSrBranch)** (will be merged in after cut): $inheritedCommitCount commits / $($sc.inheritedSourcePrs.Count) source PRs")
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
                $ro = Get-MetadataValue -Container $r -Name 'origin' -Default '?'
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
        # Candidate mode (srBranch == main) would dump 100+ open main PRs here —
        # far too noisy for a tracker issue. The single relevant PR (the SR cut
        # "Candidate" PR) is surfaced, maintainer-gated and with live status/age, in
        # the hoisted "🚩 Candidate PR" section near the top. So this full table is
        # emitted only in live-SR mode, where openSrPrs is the small, useful set of
        # backport PRs targeting the SR branch.
        if ($mode -ne 'candidate') {
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
        if ($isShippedRender) {
            [void]$sb.AppendLine("_Post-ship tracker: the tiers below are severity groupings, not ship gates. ``$srBranch`` already shipped — treat Tier 1/2 rows as post-ship **follow-ups / carry-forward** (see the summary above)._")
            [void]$sb.AppendLine()
        }
        [void]$sb.AppendLine('### Summary')
        [void]$sb.AppendLine('| Verdict | Count |')
        [void]$sb.AppendLine('|---|---|')
        $summaryEntries = if ($summary -is [System.Collections.IDictionary]) {
            @($summary.Keys | ForEach-Object { [PSCustomObject]@{ Name = [string]$_; Value = $summary[$_] } })
        } else {
            @($summary.PSObject.Properties | ForEach-Object { [PSCustomObject]@{ Name = $_.Name; Value = $_.Value } })
        }
        foreach ($entry in $summaryEntries | Sort-Object Name) {
            [void]$sb.AppendLine("| ``$($entry.Name)`` | $($entry.Value) |")
        }
        [void]$sb.AppendLine()

        # Derive detail-section membership from the same lifecycle-aware tier
        # helper used by verdicts and the hoisted summary. `no-fix-yet` appears
        # in Tier 1 for OPEN issues and Tier 3 for CLOSED issues.
        $knownClasses = @(
            'in-sr-reverted', 'no-fix-yet', 'rejected-from-sr',
            'backport-in-progress', 'merged-on-main-no-backport',
            'merged-non-main-only', 'open-on-main', 'needs-human-review',
            'in-sr-active', 'closed-as-duplicate', 'closed-fix-unlinked',
            'out-of-scope-future-sr'
        )
        $tier1Classes = @($knownClasses | Where-Object {
            (Get-EffectiveVerdictTier -Classification $_ -Mode $mode -State 'OPEN') -eq 1
        } | Sort-Object)
        $tier2Classes = @($knownClasses | Where-Object {
            (Get-EffectiveVerdictTier -Classification $_ -Mode $mode -State 'OPEN') -eq 2
        } | Sort-Object)
        $tier3Classes = @($knownClasses | Where-Object {
            (Get-EffectiveVerdictTier -Classification $_ -Mode $mode -State 'CLOSED') -eq 3
        } | Sort-Object)

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
                        $items = @($items | Where-Object {
                            [string](Get-MetadataValue -Container $_ -Name 'state' -Default 'OPEN') -eq 'OPEN'
                        })
                    } elseif ($NoFixYetState -eq 'CLOSED') {
                        $items = @($items | Where-Object {
                            [string](Get-MetadataValue -Container $_ -Name 'state' -Default 'OPEN') -ne 'OPEN'
                        })
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

        $tier1Title = if ($isShippedRender) { '🔴 Tier 1 — Urgent follow-up' } else { '🔴 Tier 1 — Blocking' }
        $tier2Title = if ($isShippedRender) { '🟡 Tier 2 — Follow-up / Review' } else { '🟡 Tier 2 — Risk / Review' }
        $tier1Empty = if ($isShippedRender) { '_No urgent follow-up regressions._' } else { '_No blocking regressions._' }
        $tier2Empty = if ($isShippedRender) { '_No follow-up/review regressions._' } else { '_No risk-tier regressions._' }
        & $emitTier $tier1Title $tier1Classes $tier1Empty 'OPEN'
        & $emitTier $tier2Title $tier2Classes $tier2Empty $null
        & $emitTier '🟢 Tier 3 — Informational' $tier3Classes $null 'CLOSED'
    }

    $body = $sb.ToString()
    if ($PublicSafe) {
        $body = ConvertTo-PublicSafeMarkdown -Text $body
    }

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
                           -InheritFromPriorSr:$InheritFromPriorSr -Shipped:$Shipped

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
    $ctx['phase'] = $Phase
    $ctx['contentsRef'] = $ctx.srRef

    $data = @{
        metadata = $ctx
        warnings = @()
        surveyIncomplete = ($Phase -ne 'all')
        surveyIncompleteReason = if ($Phase -ne 'all') { "Partial survey (-Phase $Phase) did not query every readiness axis." } else { $null }
    }

    # Shipped mode: prefer the GitHub Release publication time for the report's
    # "shipped on" date. If release metadata is unavailable, use the stable tag's
    # content date as an explicitly labeled conservative display anchor.
    # Use an explicit immutable tag override when supplied; otherwise select
    # the latest published stable tag in this SR's patch range.
    if ($ctx.mode -eq 'shipped') {
        $shippedInfo = @{ version = $null; liveVersion = $null; srNumber = 0; major = 0; tagDate = $null; dateSource = $null; tagFound = $false }
        $srMatchShip = [regex]::Match($ctx.srBranch, '^release/(\d+)\.(\d+)\.\d+xx-sr(\d+)$')
        if ($srMatchShip.Success) {
            $shippedInfo.major    = [int]$srMatchShip.Groups[1].Value
            $shippedInfo.srNumber = [int]$srMatchShip.Groups[3].Value
        }
        $vpShipped = Get-VersionsPropsState -Ref $ctx.srRef
        if ($vpShipped) {
            $shippedInfo.liveVersion = $vpShipped.FullVersion
            $ctx['liveBranchVersion'] = $vpShipped.FullVersion
            if (-not $shippedInfo.major) { $shippedInfo.major = [int]$vpShipped.Major }
        }
        $localStableTags = Get-LocalStableTags
        if ($null -eq $localStableTags -or @($localStableTags).Count -eq 0) {
            throw "Cannot query local stable tags for shipped branch '$($ctx.srBranch)'. Fetch tags before generating the tracker."
        }
        $publishedTags = Get-PublishedStableTags -Repo $ctx.repo
        $publicationQueryFailed = $null -eq $publishedTags
        if ($publicationQueryFailed) {
            $publishedTags = @()
        }
        $anchorTag = if ($ShippedTag) {
            if (-not (Test-StableTagMatchesSr -Tag $ShippedTag -SrBranch $ctx.srBranch)) {
                throw "Explicit shipped tag '$ShippedTag' does not belong to SR branch '$($ctx.srBranch)'."
            }
            $ShippedTag
        } else {
            Select-LatestStableTagForSr -SrBranch $ctx.srBranch -StableTags $localStableTags
        }
        if (-not $anchorTag) {
            throw "No stable tag was found for shipped branch '$($ctx.srBranch)'."
        }
        $stableTagsForBounds = @(Get-ShippedStableTagsForBounds `
            -AnchorTag $anchorTag `
            -PublishedTags @($publishedTags) `
            -LocalStableTags @($localStableTags) `
            -PublicationQueryFailed $publicationQueryFailed)
        $anchorIsPublished = @($publishedTags) -contains $anchorTag
        $shippedInfo.version = $anchorTag
        $ctx['shippedTagVersion'] = $anchorTag
        $tagInfo = Get-StableTagInfo -Version $anchorTag -Repo $ctx.repo
        if (-not $tagInfo) {
            throw "Cannot resolve release metadata for immutable shipped tag '$anchorTag'."
        }
        $shippedInfo.tagDate = $tagInfo.Date.ToString('o')
        $shippedInfo.dateSource = $tagInfo.DateSource
        $shippedInfo['publicationState'] = Resolve-ShippedPublicationState `
            -ListQueryFailed $publicationQueryFailed `
            -AnchorInPublishedList $anchorIsPublished `
            -TagDateSource $tagInfo.DateSource
        if ($shippedInfo.publicationState -eq 'unknown') {
            Write-ShippedPublicationStatusUnknownWarning -Tag $anchorTag
        } elseif ($shippedInfo.publicationState -eq 'pending') {
            Write-ShippedPublicationPendingWarning -Tag $anchorTag
        }
        $shippedInfo.tagFound = $true
        $shippedRefs = Resolve-ShippedContentsRefs -Version $anchorTag -Repo $ctx.repo -PublishedTags $stableTagsForBounds
        Set-ShippedContentsRefs -Context $ctx -ShippedRefs $shippedRefs
        $shippedInfo.previousTag = $shippedRefs.PreviousTag
        $hasPostTagCommits = Test-BranchAdvancedBeyondTag -Tag $anchorTag -HeadSha $ctx.srHeadSha
        $shippedInfo.hotfixHasPostTagCommits = $hasPostTagCommits
        $shippedInfo.hotfixInProgress = [bool](
            ($shippedInfo.liveVersion -and $shippedInfo.liveVersion -ne $anchorTag) -or
            $hasPostTagCommits
        )
        $ctx['hotfixHasPostTagCommits'] = $hasPostTagCommits
        $ctx['hotfixInProgress'] = $shippedInfo.hotfixInProgress
        $data['shippedInfo'] = $shippedInfo
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
        $openPrScan = Get-OpenSrPrs -Ctx $ctx
        $data['openSrPrs'] = @($openPrScan.Items)
        $data['openPrScanIncomplete'] = -not $openPrScan.IsComplete
        $data['openPrScanIncompleteReason'] = $openPrScan.Reason
        if (-not $openPrScan.IsComplete) {
            $data['surveyIncomplete'] = $true
            $existingReason = Get-MetadataValue -Container $data -Name 'surveyIncompleteReason'
            $data['surveyIncompleteReason'] = @($existingReason, $openPrScan.Reason |
                Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) }) -join ' '
        }
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
            $data['shipChecks'] = @($data['shipChecks']) + @(Get-P0PrChecks -OpenSrPrs $data['openSrPrs'] `
                                                                                 -SrBranch $ctx.srBranch `
                                                                                 -Shipped:($ctx.mode -eq 'shipped') `
                                                                                 -Incomplete:$data['openPrScanIncomplete'] `
                                                                                 -IncompleteReason $data['openPrScanIncompleteReason'])
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
    # PR in the cycle: SR can't be cut until it merges. Resolved ONCE here so
    # both the WATCH ship-check AND the prominent "🚩 Candidate PR" section
    # (hoisted under the Blocking summary) share a single gh query + spoof-gate.
    if ($Phase -in 'all', 'commits', 'regressions', 'open-prs') {
        $candidateResolution = Get-CandidatePrResolution -Ctx $ctx
        $data['candidatePr'] = $candidateResolution
        $candidateChecks = Get-CandidatePrChecks -Ctx $ctx -Resolution $candidateResolution
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
            $data['regressionScanIncomplete'] = $true
            $data['regressionFailedLabels'] = @('(no labels provided)')
        } else {
            $regressionScan = Get-RegressionCandidates -Ctx $ctx -Labels $labels `
                                                    -SrContents $data['srContents'] -MaxIssues $MaxIssues
            $data['regressions'] = @($regressionScan.Items)
            $data['regressionScanIncomplete'] = -not $regressionScan.IsComplete
            $data['regressionFailedLabels'] = @($regressionScan.FailedLabels) +
                @($regressionScan.TruncatedLabels | ForEach-Object { "$_ (truncated at -MaxIssues $MaxIssues)" }) +
                @($regressionScan.FailedIssues | ForEach-Object { "issue #$_ evidence lookup incomplete" })

            # Summary buckets
            $summary = @{}
            foreach ($r in $data['regressions']) {
                $k = $r.classification
                if (-not $summary.ContainsKey($k)) { $summary[$k] = 0 }
                $summary[$k] += 1
            }
            $data['summary'] = $summary
        }
    } else {
        # Partial diagnostic phases intentionally skip regression discovery. Mark
        # that absence explicitly so a focused `-Phase ci|commits|open-prs` run
        # cannot emit a global Ready/Shipped-clean verdict for data never queried.
        $data['regressions'] = @()
        $data['regressionScanIncomplete'] = $true
        $data['regressionFailedLabels'] = @("(regressions phase not run: -Phase $Phase)")
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
    $jsonData = if ($PublicSafe) { ConvertTo-PublicSafeValue -Value $data } else { $data }
    $jsonOut = $jsonData | ConvertTo-Json -Depth 20 -Compress:$false
    $mdOut = Format-MarkdownReport -Data $data -RepoUrl $RepoUrl -TrackerKey $TrackerKey `
        -MaxBodyBytes $MaxBodyBytes -PublicSafe:$PublicSafe

    if ($OutputDir) {
        if (-not (Test-Path $OutputDir)) { New-Item -ItemType Directory -Path $OutputDir | Out-Null }
        if ($OutputFormat -in 'json', 'both') {
            Set-Content -Path (Join-Path $OutputDir 'release-readiness.json') -Value $jsonOut -Encoding UTF8
        }
        if ($OutputFormat -in 'markdown', 'both') {
            Set-Content -Path (Join-Path $OutputDir 'release-readiness.md') -Value $mdOut -Encoding UTF8
        }
        if ($data.ContainsKey('srContents')) {
            $outputSrContents = Select-OutputSrContents -Data $data -PublicSafe:$PublicSafe
            $srcPrs = (Get-MetadataValue -Container $outputSrContents -Name 'sourcePrs' -Default @()) -join "`n"
            Set-Content -Path (Join-Path $OutputDir 'sr-source-prs.txt') -Value $srcPrs -Encoding UTF8

            $commitsJson = $outputSrContents | ConvertTo-Json -Depth 10
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
