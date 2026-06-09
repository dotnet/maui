#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Determines which .NET MAUI Servicing Release (SR) tracker issues should
    exist right now based on shipped tags + current release branches.

.DESCRIPTION
    Deterministic auto-detection used by the daily release-readiness workflow.
    Implements the two-lane algorithm documented in the release-readiness
    SKILL.md:

      Lane 1 — in-flight branches
        For every branch matching the strict regex
        `^release/<major>\.0\.\d+xx-sr(\d+)$`, read PatchVersion from its
        eng/Versions.props. If the stable tag `<major>.0.<PatchVersion>`
        does NOT exist on origin, the branch is in-flight — the release
        notes for that exact patch haven't been published yet, so it hasn't
        shipped. If the tag exists, the branch has already shipped that
        patch and is skipped.

        Tag existence is the authoritative ship signal (the release-notes
        publish job creates the tag). This is more robust than comparing
        the branch's PatchVersion against the highest known patch:
          - works regardless of ship order (SR8 can ship before SR7)
          - works for hotfix branches that may reset PatchVersion below
            the highest known patch
          - never depends on inferring "shipped" from version arithmetic

      Lane 2 — next SR off main
        Identifies the highest SR (across in-flight branches AND shipped tags)
        and proposes `SR(highest + 1)` from main IF no branch for that SR
        already exists. Survey reference is the development branch for the
        major (typically `main`, or `net<major>.0` when main has rolled over).

    All git failures fail-closed: the script exits non-zero and emits no
    detections, never an empty success.

    For each detected tracker, computes:
      - canonical key (stable issue-search marker)
      - regression labels (one per shipped SR in the band, inferred)
      - prior shipped patch / tag (for candidate mode -SrBranch + exclude)
      - recent-activity flag (true if surveyRef had commits in the last
        ActivityWindowDays days) — used by the workflow to decide whether
        to create a NEW issue when no open one exists

    The workflow caller is responsible for honoring `hasRecentActivity`:
      - if an open tracker issue exists -> always update
      - if no open tracker exists -> only create if hasRecentActivity = true
    This naturally honors human-closed inactive trackers and surfaces newly-
    active abandoned SRs.

.PARAMETER MajorVersion
    Override the .NET major version. Default: auto-detected from
    origin/main:eng/Versions.props.

.PARAMETER Repo
    Path to a git checkout of dotnet/maui with origin remote. Default: current
    directory.

.PARAMETER ActivityWindowDays
    Days to look back for commit activity on the surveyRef. Default: 7.

.PARAMETER NoFetch
    Skip `git fetch origin --tags`. Use cached refs.

.PARAMETER OutputJson
    Path to write the JSON result. If unset, writes to stdout.

.PARAMETER MaxBranches
    Safety cap on number of release branches inspected. Default: 50.

.EXAMPLE
    # Detect what trackers should exist today; print to stdout
    pwsh ./Find-ReleaseReadinessTrackers.ps1

.EXAMPLE
    # Run for a non-current major version (cross-major support)
    pwsh ./Find-ReleaseReadinessTrackers.ps1 -MajorVersion 9 -OutputJson CustomAgentLogsTmp/release-readiness/sr-trackers.json

.OUTPUTS
    JSON: { detectedAt, repo, majorVersion, mainBranch, highestShippedPatch,
            highestShippedTag, trackers: [ ... ] }

    Each tracker:
      { srNumber, majorVersion, mode, branchName, surveyRef, priorSrBranch,
        canonicalKey, issueTitle, expectedTag, milestoneName, regressionLabels,
        hasRecentActivity, recentCommitCount, priorShippedPatch,
        priorShippedTag }
#>

[CmdletBinding()]
param(
    [int]$MajorVersion = 0,
    [string]$Repo = (Get-Location).Path,
    [int]$ActivityWindowDays = 7,
    [switch]$NoFetch,
    [string]$OutputJson,
    [int]$MaxBranches = 50
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

Import-Module (Join-Path $PSScriptRoot '..' '..' '..' 'scripts' 'shared' 'MauiReleaseVersioning.psm1') -Force

# Strict regex contracts. These deliberately reject malformed/temporary refs
# so abandoned, backup, or experimental branches don't masquerade as SR tracks.
#   Branch: must end in `-sr<digits>` with no further qualifiers.
#     rejects: sr08 (leading zero), sr-next, sr10-test, sr8-backup, sr10-old
$Script:StrictSrBranchRegex = '^release/(\d+)\.0\.\d+xx-sr(\d+)$'
#   Tag (stable only): exactly `<major>.0.<patch>`, no prerelease suffix.
$Script:StrictStableTagRegex = '^(\d+)\.0\.(\d+)$'

# Backwards-compatible exports for tests that dot-source this script.
# Tests assert against the same regex strings the algorithm uses.
$Global:FindReleaseReadinessTrackers_StrictSrBranchRegex = $Script:StrictSrBranchRegex
$Global:FindReleaseReadinessTrackers_StrictStableTagRegex = $Script:StrictStableTagRegex

function Invoke-GitOrFail {
    <#
    .SYNOPSIS
        Runs git with the given arguments. Captures stdout and exits non-zero
        on failure (fail-closed). Empty output is allowed; only a non-zero
        exit code triggers a fail-close.
    #>
    param([string[]]$ArgList, [string]$FailureMessage)
    $out = & git -C $Repo @ArgList 2>&1
    if ($LASTEXITCODE -ne 0) {
        $joined = ($out -join "`n")
        throw "Fail-closed: $FailureMessage (git exit $LASTEXITCODE)`n$joined"
    }
    return $out
}

function Get-StableTagsForMajor {
    <#
    .SYNOPSIS
        Returns the stable (non-prerelease) tags for a given major version,
        in ascending patch order. Throws on git failure.
    #>
    param([int]$Major)
    $allTags = Invoke-GitOrFail @('--no-pager', 'tag', '-l') "Could not list tags"
    $tags = @($allTags | Where-Object {
        $_ -and ($_ -match $Script:StrictStableTagRegex) -and ([int]$Matches[1] -eq $Major)
    } | Sort-Object {
        if ($_ -match $Script:StrictStableTagRegex) { [int]$Matches[2] } else { 0 }
    })
    return $tags
}

function Get-ShippedPatchSet {
    <#
    .SYNOPSIS
        Builds a HashSet[int] of shipped patch numbers from a list of stable
        tags (typically the output of Get-StableTagsForMajor).
    .DESCRIPTION
        O(1) lookup is essential for the in-flight loop: each branch needs
        to ask "does my PatchVersion already have a published tag?".

        Malformed/prerelease/non-matching tags are silently dropped — this
        function is for the in-flight check only, where only exact stable
        tag matches count as "shipped".
    #>
    param([AllowEmptyCollection()][string[]]$StableTags)
    $set = [System.Collections.Generic.HashSet[int]]::new()
    if ($null -eq $StableTags) { return ,$set }
    foreach ($tag in $StableTags) {
        if ($tag -and ($tag -match $Script:StrictStableTagRegex)) {
            [void]$set.Add([int]$Matches[2])
        }
    }
    # Unary comma prevents PS from unrolling the single-object return value.
    ,$set
}

function Test-IsBranchInFlight {
    <#
    .SYNOPSIS
        True if the branch is in-flight (its expected stable tag has not
        been published). False if its tag already exists (shipped).
    .DESCRIPTION
        The release-notes pipeline creates the tag `<major>.0.<patch>` when
        a release publishes. Tag absent → branch hasn't shipped that patch
        → in-flight. Tag present → already shipped → skip.

        This replaces the older "PatchVersion > HighestShippedPatch" check,
        which was fragile to out-of-order ships and hotfix branches that
        reset PatchVersion.
    #>
    param(
        [Parameter(Mandatory)][int]$BranchPatch,
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.HashSet[int]]$ShippedPatches
    )
    return -not $ShippedPatches.Contains($BranchPatch)
}

function Get-RemoteSrBranchesForMajor {
    <#
    .SYNOPSIS
        Returns an array of branch names (without `refs/heads/` prefix) on
        origin matching the strict SR pattern for the given major version.
        Throws on git failure.
    .OUTPUTS
        @(@{ branch = 'release/10.0.1xx-sr7'; srNumber = 7 }, ...)
        Sorted by srNumber ascending.
    #>
    param([int]$Major)
    # Use a wide globbed ls-remote so we can validate strictly in PS. The
    # globs `release/<major>.0.*xx-sr*` still need post-filtering because
    # git globs are not regex.
    $lines = Invoke-GitOrFail @('ls-remote', '--heads', 'origin', "release/$Major.0.*xx-sr*") `
        "Could not list remote SR branches for major $Major"
    $branches = @()
    foreach ($line in $lines) {
        if (-not $line) { continue }
        # Format: "<sha>\trefs/heads/<branchName>"
        if ($line -match '^[0-9a-f]{40}\s+refs/heads/(.+)$') {
            $branch = $Matches[1]
            if ($branch -match $Script:StrictSrBranchRegex) {
                $branchMajor = [int]$Matches[1]
                $sr          = [int]$Matches[2]
                if ($branchMajor -eq $Major) {
                    $branches += [pscustomobject]@{
                        branch   = $branch
                        srNumber = $sr
                    }
                }
            } else {
                Write-Verbose "Skipping non-strict SR branch '$branch' (would be ignored by lane 1)"
            }
        }
    }
    # Stable, deterministic order: srNumber ascending.
    $branches = @($branches | Sort-Object srNumber)
    if ($branches.Count -gt $MaxBranches) {
        throw "Fail-closed: matched $($branches.Count) SR branches for major $Major (> MaxBranches=$MaxBranches). Bump -MaxBranches or investigate ghost refs."
    }
    return $branches
}

function Get-RecentCommitCount {
    <#
    .SYNOPSIS
        Counts commits on the given ref in the last $Days days. Used to gate
        "create a NEW tracker issue" decisions.
    #>
    param([string]$Ref, [int]$Days)
    $remoteRef = if ($Ref -match '^origin/') { $Ref } else { "origin/$Ref" }
    # Use --pretty=format:%H to count lines without a trailing newline.
    $lines = Invoke-GitOrFail @('--no-pager', 'log', $remoteRef, "--since=${Days}.days", '--pretty=format:%H') `
        "Could not count recent commits on $remoteRef"
    if (-not $lines) { return 0 }
    return @($lines | Where-Object { $_ }).Count
}

function New-RegressionLabelList {
    <#
    .SYNOPSIS
        Builds the canonical `regressed-in-X.Y.NN` label list for an SR.
    .DESCRIPTION
        The label set covers the prior shipped SR (`<major>.0.<priorSr*10>`)
        AND the SR's own patch band (`<major>.0.<srNumber*10>`).

        Examples:
          SR7 (patch=71) -> regressed-in-10.0.60, regressed-in-10.0.70
          SR8 candidate (priorSr=7, patch=80) -> regressed-in-10.0.70, regressed-in-10.0.80
          SR1 (patch=11) -> regressed-in-10.0.0,  regressed-in-10.0.10

        Includes the GA label (`<major>.0.0`) when the prior SR is 0.
    #>
    param([int]$Major, [int]$SrNumber)
    $labels = New-Object System.Collections.Generic.List[string]
    $priorSr = $SrNumber - 1
    if ($priorSr -lt 0) { $priorSr = 0 }
    if ($priorSr -eq 0) {
        $labels.Add("regressed-in-$Major.0.0")
    } else {
        $labels.Add("regressed-in-$Major.0.$($priorSr * 10)")
    }
    $labels.Add("regressed-in-$Major.0.$($SrNumber * 10)")
    return $labels
}

function New-Tracker {
    <#
    .SYNOPSIS
        Constructs a tracker descriptor object for the workflow.
    #>
    param(
        [int]$Major,
        [int]$SrNumber,
        [string]$Mode,                 # 'in-flight' or 'candidate'
        [string]$BranchName,            # nullable for candidate without branch
        [string]$SurveyRef,             # branch or development ref to survey
        [string]$PriorSrBranch,         # nullable; used as -SrBranch for -Candidate mode
        [int]$PriorShippedPatch,
        [string]$PriorShippedTag,
        [int]$ExpectedPatch,
        [string]$ExpectedTag,
        [int]$HasRecentActivityCount
    )
    $canonical = "net$Major-sr$SrNumber"
    $milestone = ".NET $Major SR$SrNumber"
    $branchDisplay = if ($BranchName) { $BranchName } else { "(no branch yet — from $SurveyRef)" }
    $title = "[Release Readiness] .NET $Major SR$SrNumber — $branchDisplay"
    if ($Mode -eq 'candidate') {
        $title = "[Release Readiness] .NET $Major SR$SrNumber — candidate from $SurveyRef"
    }
    return [pscustomobject]@{
        srNumber             = $SrNumber
        majorVersion         = $Major
        mode                 = $Mode
        branchName           = $BranchName
        surveyRef            = $SurveyRef
        priorSrBranch        = $PriorSrBranch
        canonicalKey         = $canonical
        issueTitle           = $title
        milestoneName        = $milestone
        expectedPatch        = $ExpectedPatch
        expectedTag          = $ExpectedTag
        regressionLabels     = (New-RegressionLabelList -Major $Major -SrNumber $SrNumber)
        hasRecentActivity    = ($HasRecentActivityCount -gt 0)
        recentCommitCount    = $HasRecentActivityCount
        priorShippedPatch    = $PriorShippedPatch
        priorShippedTag      = $PriorShippedTag
    }
}

# ── Main ─────────────────────────────────────────────────────────────────

# Guard: skip the driver when dot-sourced (tests dot-source to access helpers
# like New-RegressionLabelList and the strict regex constants).
if ($MyInvocation.InvocationName -eq '.' -or $MyInvocation.Line -match '^\.\s') { return }

if (-not (Test-Path (Join-Path $Repo '.git'))) {
    throw "Fail-closed: $Repo is not a git repository. Pass -Repo <checkout>."
}

if (-not $NoFetch) {
    Write-Host "Fetching origin (branches + tags)..." -ForegroundColor Cyan
    Invoke-GitOrFail @('fetch', 'origin', '--tags', '--prune', '--quiet') `
        "git fetch failed (fail-closed; cannot guess in-flight SRs from stale refs)" | Out-Null
}

# Resolve major version (auto from origin/main if not supplied).
if ($MajorVersion -le 0) {
    $MajorVersion = Get-CurrentMajorVersion -Repo $Repo
    Write-Host "Detected MajorVersion=$MajorVersion from origin/main:eng/Versions.props" -ForegroundColor Cyan
}
$mainBranchForMajor = Get-MainBranchForVersion -Major $MajorVersion -Repo $Repo

# Step 1: Inventory all shipped stable tags for this major.
# We capture both the full set (authoritative ship signal for Lane 1's
# tag-existence check) AND the highest patch (informational + used by
# Lane 2 to derive the "next SR off main" number).
$stableTags = Get-StableTagsForMajor -Major $MajorVersion
$shippedPatches = Get-ShippedPatchSet -StableTags $stableTags
$highestShippedPatch = 0
$highestShippedTag   = $null
if ($stableTags.Count -gt 0) {
    $highestShippedTag = $stableTags[-1]
    if ($highestShippedTag -match $Script:StrictStableTagRegex) {
        $highestShippedPatch = [int]$Matches[2]
    }
}
Write-Host "Shipped patches for major ${MajorVersion}: $(if ($shippedPatches.Count -gt 0) { ($shippedPatches | Sort-Object) -join ', ' } else { '(none)' })" -ForegroundColor Cyan
Write-Host "Highest shipped stable tag for major ${MajorVersion}: $(if ($highestShippedTag) { $highestShippedTag } else { '(none)' })" -ForegroundColor Cyan

# Step 2: Inspect remote SR branches and classify.
$branches = Get-RemoteSrBranchesForMajor -Major $MajorVersion
Write-Host "Found $($branches.Count) strict release/$MajorVersion.0.*xx-sr* branches on origin" -ForegroundColor Cyan

$trackers = New-Object System.Collections.Generic.List[object]
$highestBranchSr = 0
$inflightBranchesBySr = @{}
foreach ($entry in $branches) {
    $branch = $entry.branch
    $sr     = $entry.srNumber
    if ($sr -gt $highestBranchSr) { $highestBranchSr = $sr }

    Write-Verbose "Inspecting branch $branch (sr$sr)..."
    $versionInfo = Get-VersionFromGitRef -GitRef "origin/$branch" -Repo $Repo
    if (-not $versionInfo) {
        Write-Warning "Could not read Versions.props from origin/$branch — skipping (fail-soft for this branch)"
        continue
    }
    if ($versionInfo.Tag -notmatch '^(\d+)\.0\.(\d+)$') {
        Write-Warning "Versions.props on $branch produced unexpected tag '$($versionInfo.Tag)' — skipping"
        continue
    }
    $branchPatch = [int]$Matches[2]
    $expectedTag = $versionInfo.Tag

    if (Test-IsBranchInFlight -BranchPatch $branchPatch -ShippedPatches $shippedPatches) {
        # In-flight: stable tag `<major>.0.<branchPatch>` not yet published →
        # release notes haven't gone out → SR is still cooking.
        $recent = Get-RecentCommitCount -Ref $branch -Days $ActivityWindowDays
        $tracker = New-Tracker -Major $MajorVersion -SrNumber $sr -Mode 'in-flight' `
            -BranchName $branch -SurveyRef $branch -PriorSrBranch $null `
            -PriorShippedPatch $highestShippedPatch -PriorShippedTag $highestShippedTag `
            -ExpectedPatch $branchPatch -ExpectedTag $expectedTag `
            -HasRecentActivityCount $recent
        $trackers.Add($tracker)
        $inflightBranchesBySr[$sr] = $branch
        Write-Host "  -> in-flight tracker for SR$sr (patch=$branchPatch, no tag $expectedTag yet, recent=$recent)" -ForegroundColor Green
    } else {
        Write-Host "  -> SR$sr branch '$branch' patch=$branchPatch already shipped (tag $expectedTag exists)" -ForegroundColor DarkGray
    }
}

# Step 3: Lane 2 — propose next SR off main IF no branch exists for it.
$highestShippedSr = [int]([math]::Floor($highestShippedPatch / 10))
$highestSr        = [math]::Max($highestBranchSr, $highestShippedSr)
$nextSr           = $highestSr + 1
$nextSrBranchName = "release/$MajorVersion.0.1xx-sr$nextSr"
$nextSrBranchExists = $branches | Where-Object { $_.srNumber -eq $nextSr }

if (-not $nextSrBranchExists) {
    # Candidate from main (or net<major>.0).
    $candidateRef = $mainBranchForMajor
    $candidateVersionInfo = Get-VersionFromGitRef -GitRef "origin/$candidateRef" -Repo $Repo
    $expectedPatch = $nextSr * 10
    $expectedTag   = "$MajorVersion.0.$expectedPatch"
    if ($candidateVersionInfo -and $candidateVersionInfo.Tag -match '^(\d+)\.0\.(\d+)$') {
        # Only adopt main's PatchVersion if it has actually advanced to or past the
        # next SR's expected band. Immediately after an SR cut, main still carries
        # the cut SR's patch (e.g., main=80 right after SR8 is cut), so a naive
        # adoption would mis-label the candidate as the same SR.
        $mainPatch = [int]$Matches[2]
        if ($mainPatch -ge $expectedPatch) {
            $expectedPatch = $mainPatch
            $expectedTag   = $candidateVersionInfo.Tag
        }
    }
    $recent = Get-RecentCommitCount -Ref $candidateRef -Days $ActivityWindowDays
    # Prior SR branch (for -SrBranch baseline in candidate mode): the highest
    # in-flight branch, falling back to a synthesized name for the last shipped SR.
    $priorSrBranch = $null
    if ($inflightBranchesBySr.Count -gt 0) {
        $priorSr = ($inflightBranchesBySr.Keys | Sort-Object | Select-Object -Last 1)
        $priorSrBranch = $inflightBranchesBySr[$priorSr]
    } elseif ($highestShippedSr -ge 1) {
        $priorSrBranch = "release/$MajorVersion.0.1xx-sr$highestShippedSr"
    }
    $tracker = New-Tracker -Major $MajorVersion -SrNumber $nextSr -Mode 'candidate' `
        -BranchName $null -SurveyRef $candidateRef -PriorSrBranch $priorSrBranch `
        -PriorShippedPatch $highestShippedPatch -PriorShippedTag $highestShippedTag `
        -ExpectedPatch $expectedPatch -ExpectedTag $expectedTag `
        -HasRecentActivityCount $recent
    $trackers.Add($tracker)
    Write-Host "  -> candidate tracker for SR$nextSr (surveyRef=$candidateRef, recent=$recent)" -ForegroundColor Green
} else {
    Write-Host "  -> SR$nextSr already has a branch (sr$nextSr); covered by lane 1" -ForegroundColor DarkGray
}

# ── Output ───────────────────────────────────────────────────────────────

$result = [pscustomobject]@{
    detectedAt           = (Get-Date).ToUniversalTime().ToString('o')
    repo                 = (Resolve-Path $Repo).Path
    majorVersion         = $MajorVersion
    mainBranch           = $mainBranchForMajor
    highestShippedPatch  = $highestShippedPatch
    highestShippedTag    = $highestShippedTag
    activityWindowDays   = $ActivityWindowDays
    trackers             = $trackers.ToArray()
}

$json = $result | ConvertTo-Json -Depth 8

if ($OutputJson) {
    $dir = Split-Path -Parent $OutputJson
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    Set-Content -Path $OutputJson -Value $json -Encoding utf8
    Write-Host "Wrote $($trackers.Count) tracker(s) to $OutputJson" -ForegroundColor Cyan
} else {
    Write-Output $json
}
