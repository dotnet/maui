#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Determines which .NET MAUI Release Readiness tracker issues should
    exist right now based on shipped tags + current release branches.
    Covers both Servicing Releases (SR) AND Previews.

.DESCRIPTION
    Deterministic auto-detection used by the daily release-readiness workflow.
    Implements a four-lane algorithm documented in the release-readiness
    SKILL.md:

      Lane 1 — in-flight SR branches
        For every branch matching the strict regex
        `^release/<major>\.0\.\d+xx-sr(\d+)$`, read PatchVersion from its
        eng/Versions.props. If the stable tag `<major>.0.<PatchVersion>`
        does NOT exist on origin, the branch is in-flight — the release
        notes for that exact patch haven't been published yet, so it hasn't
        shipped. If the tag exists, the branch has already shipped that
        patch and is skipped.

      Lane 2 — next SR off main
        Identifies the highest SR (across in-flight branches AND shipped tags)
        and proposes `SR(highest + 1)` from main IF no branch for that SR
        already exists. Survey reference is the development branch for the
        major (typically `main`, or `net<major>.0` when main has rolled over).
        Skipped entirely for pre-GA majors (no `<major>.0.0` tag yet).

      Lane 3 — in-flight preview branches
        For every branch matching `^release/<major>\.0\.\d+xx-preview(\d+)$`,
        check whether ANY tag matches `<major>.0.0-preview.<N>.<date>[.<build>]`.
        Tag absent → preview is in-flight. Tag present → already shipped, skip.
        Same tag-existence rule as Lane 1, parallel semantics.

      Lane 4 — next preview off net<major>.0 (or main if no net<major>.0)
        Reads PreReleaseVersionIteration from net<major>.0's eng/Versions.props
        (or main if main carries the preview cycle for this major). If that
        iteration number has no matching tag AND no matching branch, propose
        a candidate preview tracker. Skipped for majors that are in SR phase
        (PreReleaseVersionLabel is not 'preview').

        Tag existence is the authoritative ship signal (the release-notes
        publish job creates the tag). This is more robust than comparing
        the branch's PatchVersion against the highest known patch:
          - works regardless of ship order (SR8 can ship before SR7)
          - works for hotfix branches that may reset PatchVersion below
            the highest known patch
          - never depends on inferring "shipped" from version arithmetic

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
    origin/main:eng/Versions.props. Ignored if -AllActiveMajors is set.

.PARAMETER AllActiveMajors
    Auto-detect all active major versions (main's major plus any major with
    a `net<N>.0` branch where N != main's major) and run detection for each.
    Output shape changes to { majors: [ { majorVersion, ... }, ... ] }.

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
    # Detect what trackers should exist today for main's major; print to stdout
    pwsh ./Find-ReleaseReadinessTrackers.ps1

.EXAMPLE
    # Run for ALL active majors (used by the daily workflow)
    pwsh ./Find-ReleaseReadinessTrackers.ps1 -AllActiveMajors -OutputJson CustomAgentLogsTmp/release-readiness/all-trackers.json

.EXAMPLE
    # Run for a non-current major version (cross-major support)
    pwsh ./Find-ReleaseReadinessTrackers.ps1 -MajorVersion 9 -OutputJson CustomAgentLogsTmp/release-readiness/sr-trackers.json

.OUTPUTS
    Single-major mode:
      { detectedAt, repo, majorVersion, mainBranch, highestShippedPatch,
        highestShippedTag, activityWindowDays, trackers: [ ... ] }

    Multi-major (-AllActiveMajors) mode:
      { detectedAt, repo, activityWindowDays, majors: [ { ...same shape as single-major... }, ... ] }

    Each tracker (SR):
      { branchType: 'sr', srNumber, majorVersion, mode, branchName, surveyRef,
        priorSrBranch, canonicalKey, issueTitle, expectedTag, milestoneName,
        regressionLabels, hasRecentActivity, recentCommitCount,
        priorShippedPatch, priorShippedTag }

    Each tracker (preview):
      { branchType: 'preview', previewNumber, majorVersion, mode, branchName,
        surveyRef, canonicalKey, issueTitle, expectedTagPrefix, milestoneName,
        regressionLabels, hasRecentActivity, recentCommitCount }
#>

[CmdletBinding()]
param(
    [int]$MajorVersion = 0,
    [switch]$AllActiveMajors,
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
# so abandoned, backup, or experimental branches don't masquerade as tracks.
#   SR branch: must end in `-sr<digits>` with no further qualifiers.
#     rejects: sr-next, sr10-test, sr8-backup, sr10-old
$Script:StrictSrBranchRegex = '^release/(\d+)\.0\.\d+xx-sr(\d+)$'
#   Preview branch: must end in `-preview<digits>` with no further qualifiers.
#     rejects: preview-next, preview6.1 (sub-preview), preview7-test
$Script:StrictPreviewBranchRegex = '^release/(\d+)\.0\.\d+xx-preview(\d+)$'
#   Stable tag: exactly `<major>.0.<patch>`, no prerelease suffix.
$Script:StrictStableTagRegex = '^(\d+)\.0\.(\d+)$'
#   Preview tag: `<major>.0.0-preview.<N>.<YYYYMMDD>[.<build>]`
#     e.g., 11.0.0-preview.5.26304.4
$Script:StrictPreviewTagRegex = '^(\d+)\.0\.0-preview\.(\d+)\.\d+(?:\.\d+)?$'

# Backwards-compatible exports for tests that dot-source this script.
# Tests assert against the same regex strings the algorithm uses.
$Global:FindReleaseReadinessTrackers_StrictSrBranchRegex = $Script:StrictSrBranchRegex
$Global:FindReleaseReadinessTrackers_StrictPreviewBranchRegex = $Script:StrictPreviewBranchRegex
$Global:FindReleaseReadinessTrackers_StrictStableTagRegex = $Script:StrictStableTagRegex
$Global:FindReleaseReadinessTrackers_StrictPreviewTagRegex = $Script:StrictPreviewTagRegex

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
    # Unary comma keeps an empty array from collapsing to $null at the call site.
    ,$tags
}

function Get-PreviewTagsForMajor {
    <#
    .SYNOPSIS
        Returns the preview tags for a given major version, in ascending
        previewNumber order. Throws on git failure.
    .DESCRIPTION
        Matches tags of the form `<major>.0.0-preview.<N>.<date>[.<build>]`.
        Tag sort is by previewNumber only (date suffixes for the same preview
        are kept in lexical order, which is usually chronological since the
        date prefix is YYYYMMDD).
    #>
    param([int]$Major)
    $allTags = Invoke-GitOrFail @('--no-pager', 'tag', '-l') "Could not list tags"
    $tags = @($allTags | Where-Object {
        $_ -and ($_ -match $Script:StrictPreviewTagRegex) -and ([int]$Matches[1] -eq $Major)
    } | Sort-Object {
        if ($_ -match $Script:StrictPreviewTagRegex) { [int]$Matches[2] } else { 0 }
    }, { $_ })
    ,$tags
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

function Get-ShippedPreviewSet {
    <#
    .SYNOPSIS
        Builds a HashSet[int] of shipped preview numbers from a list of
        preview tags (typically the output of Get-PreviewTagsForMajor).
    .DESCRIPTION
        Mirrors Get-ShippedPatchSet semantics but for preview tags.
        A preview is considered shipped as soon as ANY tag matching
        `<major>.0.0-preview.<N>.*` exists. Multiple tags for the same
        preview (e.g., a re-tagged final build) collapse to one entry.
    #>
    param([AllowEmptyCollection()][string[]]$PreviewTags)
    $set = [System.Collections.Generic.HashSet[int]]::new()
    if ($null -eq $PreviewTags) { return ,$set }
    foreach ($tag in $PreviewTags) {
        if ($tag -and ($tag -match $Script:StrictPreviewTagRegex)) {
            [void]$set.Add([int]$Matches[2])
        }
    }
    ,$set
}

function Test-IsBranchInFlight {
    <#
    .SYNOPSIS
        True if the SR branch is in-flight (its expected stable tag has not
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

function Test-IsStaleSrBranch {
    <#
    .SYNOPSIS
        True when a tag-absent SR branch should be treated as a stale/abandoned
        hotfix leftover rather than a live in-flight SR.
    .DESCRIPTION
        Secondary disambiguator applied ONLY after Test-IsBranchInFlight has
        already returned true (the branch's stable `<major>.0.<patch>` tag does
        not exist). Tag-existence stays the PRIMARY in-flight signal; this guard
        narrows the false-positive where an old hotfix branch sits below the
        shipped watermark with its tag never published.

        A branch is stale when BOTH:
          - its patch is strictly below the highest shipped patch
            (e.g. SR2 patch 21 / SR3 patch 33 long after SR7 patch 71 shipped), and
          - it has had no commits within the activity window (idle).

        The idle requirement preserves the out-of-order / hotfix scenario that
        tag-existence protects: a real security-hotfix branch that resets
        PatchVersion below the watermark has recent commits
        (RecentActivityCount > 0) and is therefore NOT considered stale. A
        freshly-cut live SR sits at-or-above the watermark, so it is never
        stale regardless of activity.
    #>
    param(
        [Parameter(Mandatory)][int]$BranchPatch,
        [Parameter(Mandatory)][int]$HighestShippedPatch,
        [Parameter(Mandatory)][int]$RecentActivityCount
    )
    return ($RecentActivityCount -le 0 -and $BranchPatch -lt $HighestShippedPatch)
}

function Test-IsPreviewBranchInFlight {
    <#
    .SYNOPSIS
        True if the preview branch is in-flight (no tag matching
        `<major>.0.0-preview.<N>.*` has been published). False if any
        matching tag exists (shipped).
    .DESCRIPTION
        Parallel to Test-IsBranchInFlight but uses the preview-tag shipped
        set. As soon as the release-notes pipeline publishes ANY tag for
        preview N, that preview is considered shipped.
    #>
    param(
        [Parameter(Mandatory)][int]$PreviewNumber,
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [System.Collections.Generic.HashSet[int]]$ShippedPreviews
    )
    return -not $ShippedPreviews.Contains($PreviewNumber)
}

function Get-ActiveMajorVersions {
    <#
    .SYNOPSIS
        Returns the list of active .NET major versions to detect trackers for.
    .DESCRIPTION
        Active = main's MajorVersion + any `net<N>.0` branch on origin where
        N >= main's major. This catches the common cross-major state where
        main is still on major N but net(N+1).0 has forked off to start the
        next major's preview cycle.

        Older `net<N>.0` branches (N < main's major) are frozen artifacts
        from previous major cycles — surveying them produces dead trackers
        with no shipped tags (because they predate the modern preview-tag
        scheme) and no recent activity. We exclude them.

        Returned list is sorted ascending and deduplicated.
    #>
    [CmdletBinding()]
    param()
    $majors = New-Object System.Collections.Generic.SortedSet[int]
    $mainMajor = Get-CurrentMajorVersion -Repo $Repo
    [void]$majors.Add($mainMajor)

    # Inspect any `net<N>.0` branches on origin, only N >= main's major.
    $lines = Invoke-GitOrFail @('ls-remote', '--heads', 'origin', 'net*.0') `
        "Could not list net*.0 branches on origin"
    foreach ($line in $lines) {
        if (-not $line) { continue }
        if ($line -match '^[0-9a-f]{40}\s+refs/heads/net(\d+)\.0$') {
            $candidate = [int]$Matches[1]
            if ($candidate -ge $mainMajor) {
                [void]$majors.Add($candidate)
            }
        }
    }
    # Wrap in `,` (unary comma) so an array result doesn't unroll when consumed
    # by `foreach` in callers under PS 7+ (which does the right thing) AND under
    # PS 5.1 (which is more eager to flatten).
    ,@($majors)
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
    # Unary comma preserves the array shape even when empty (otherwise PS unrolls @() to $null at the call site).
    ,$branches
}

function Get-RemotePreviewBranchesForMajor {
    <#
    .SYNOPSIS
        Returns an array of branch names matching the strict preview pattern
        for the given major version on origin. Throws on git failure.
    .OUTPUTS
        @(@{ branch = 'release/11.0.1xx-preview6'; previewNumber = 6 }, ...)
        Sorted by previewNumber ascending.
    #>
    param([int]$Major)
    $lines = Invoke-GitOrFail @('ls-remote', '--heads', 'origin', "release/$Major.0.*xx-preview*") `
        "Could not list remote preview branches for major $Major"
    $branches = @()
    foreach ($line in $lines) {
        if (-not $line) { continue }
        if ($line -match '^[0-9a-f]{40}\s+refs/heads/(.+)$') {
            $branch = $Matches[1]
            if ($branch -match $Script:StrictPreviewBranchRegex) {
                $branchMajor = [int]$Matches[1]
                $previewN    = [int]$Matches[2]
                if ($branchMajor -eq $Major) {
                    $branches += [pscustomobject]@{
                        branch        = $branch
                        previewNumber = $previewN
                    }
                }
            } else {
                Write-Verbose "Skipping non-strict preview branch '$branch' (would be ignored by lane 3)"
            }
        }
    }
    $branches = @($branches | Sort-Object previewNumber)
    if ($branches.Count -gt $MaxBranches) {
        throw "Fail-closed: matched $($branches.Count) preview branches for major $Major (> MaxBranches=$MaxBranches). Bump -MaxBranches or investigate ghost refs."
    }
    # Unary comma preserves the array shape even when empty.
    ,$branches
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

function New-PreviewRegressionLabelList {
    <#
    .SYNOPSIS
        Builds the canonical `regressed-in-X.Y.0-previewN` label list for a
        preview tracker.
    .DESCRIPTION
        Covers the immediately prior preview AND the preview itself.

        Examples:
          preview6 -> regressed-in-11.0.0-preview5, regressed-in-11.0.0-preview6
          preview1 -> regressed-in-11.0.0-preview1
                      (no prior preview to compare against — first preview)

        Note: regression labels for previews are repo-conventional. If the
        team doesn't apply `regressed-in-X.Y.0-previewN` style labels yet,
        the workflow can still list these in the issue body so triagers know
        what to add.
    #>
    param([int]$Major, [int]$PreviewNumber)
    $labels = New-Object System.Collections.Generic.List[string]
    if ($PreviewNumber -gt 1) {
        $labels.Add("regressed-in-$Major.0.0-preview$($PreviewNumber - 1)")
    }
    $labels.Add("regressed-in-$Major.0.0-preview$PreviewNumber")
    return $labels
}

function New-Tracker {
    <#
    .SYNOPSIS
        Constructs an SR tracker descriptor object for the workflow.
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
    # Always advertise a canonical proposed branch name. Even when the branch
    # doesn't exist yet (candidate mode), downstream tools want a stable
    # `release/<major>.0.1xx-sr<N>` slug; whether it exists on origin is
    # surfaced via the explicit branchExists flag.
    $branchExists = [bool]$BranchName
    $effectiveBranchName = if ($BranchName) { $BranchName } else { "release/$Major.0.1xx-sr$SrNumber" }
    $branchDisplay = if ($branchExists) { $effectiveBranchName } else { "(no branch yet — from $SurveyRef)" }
    $title = "[Release Readiness] .NET $Major SR$SrNumber — $branchDisplay"
    if ($Mode -eq 'candidate') {
        $title = "[Release Readiness] .NET $Major SR$SrNumber — candidate from $SurveyRef"
    }
    return [pscustomobject]@{
        branchType           = 'sr'
        srNumber             = $SrNumber
        majorVersion         = $Major
        mode                 = $Mode
        branchName           = $effectiveBranchName
        branchExists         = $branchExists
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

function New-PreviewTracker {
    <#
    .SYNOPSIS
        Constructs a preview tracker descriptor object for the workflow.
    .DESCRIPTION
        Preview trackers differ from SR trackers in several ways:
          - branchType = 'preview' (workflow uses this to dispatch the
            right report script: Get-PreviewReadiness.ps1 vs Get-ReleaseReadiness.ps1)
          - expectedTagPrefix instead of expectedTag (preview tags carry a
            date+build suffix that's only known at publish time, so we
            advertise the prefix `<major>.0.0-preview.<N>.`)
          - No priorSrBranch (preview cadence is sequential — surveyRef is
            the branch itself for in-flight or net<major>.0 for candidate)
          - regressionLabels use the preview-specific label format
    #>
    param(
        [int]$Major,
        [int]$PreviewNumber,
        [string]$Mode,                 # 'in-flight' or 'candidate'
        [string]$BranchName,            # nullable for candidate without branch
        [string]$SurveyRef,
        [int]$HasRecentActivityCount
    )
    $canonical = "net$Major-preview$PreviewNumber"
    $milestone = ".NET $Major.0-preview$PreviewNumber"
    # Always advertise a canonical proposed branch name even in candidate mode.
    $branchExists = [bool]$BranchName
    $effectiveBranchName = if ($BranchName) { $BranchName } else { "release/$Major.0.1xx-preview$PreviewNumber" }
    $branchDisplay = if ($branchExists) { $effectiveBranchName } else { "(no branch yet — from $SurveyRef)" }
    $title = "[Release Readiness] .NET $Major.0 preview$PreviewNumber — $branchDisplay"
    if ($Mode -eq 'candidate') {
        $title = "[Release Readiness] .NET $Major.0 preview$PreviewNumber — candidate from $SurveyRef"
    }
    $expectedTagPrefix = "$Major.0.0-preview.$PreviewNumber."
    return [pscustomobject]@{
        branchType           = 'preview'
        previewNumber        = $PreviewNumber
        majorVersion         = $Major
        mode                 = $Mode
        branchName           = $effectiveBranchName
        branchExists         = $branchExists
        surveyRef            = $SurveyRef
        canonicalKey         = $canonical
        issueTitle           = $title
        milestoneName        = $milestone
        expectedTagPrefix    = $expectedTagPrefix
        regressionLabels     = (New-PreviewRegressionLabelList -Major $Major -PreviewNumber $PreviewNumber)
        hasRecentActivity    = ($HasRecentActivityCount -gt 0)
        recentCommitCount    = $HasRecentActivityCount
    }
}

function Invoke-DetectionForMajor {
    <#
    .SYNOPSIS
        Runs the four-lane detection algorithm for a single major version.
    .DESCRIPTION
        Encapsulates Lanes 1-4 so the script body can call it once (single
        major) or in a loop (-AllActiveMajors). Returns a pscustomobject
        with the per-major envelope and a trackers array.
    #>
    param([Parameter(Mandatory)][int]$Major)

    $mainBranchForMajor = Get-MainBranchForVersion -Major $Major -Repo $Repo

    # ── Step 1: Inventory all shipped stable + preview tags for this major.
    # All helpers below use unary-comma return + plain assignment here. DON'T
    # wrap in @(...) — that combination doubles up (returns a 1-elem array
    # whose only entry is the inner array). PS unrolling is the gotcha.
    $stableTags      = Get-StableTagsForMajor   -Major $Major
    $shippedPatches  = Get-ShippedPatchSet      -StableTags  $stableTags
    $previewTags     = Get-PreviewTagsForMajor  -Major $Major
    $shippedPreviews = Get-ShippedPreviewSet    -PreviewTags $previewTags

    $highestShippedPatch = 0
    $highestShippedTag   = $null
    if ($stableTags.Count -gt 0) {
        $highestShippedTag = $stableTags[-1]
        if ($highestShippedTag -match $Script:StrictStableTagRegex) {
            $highestShippedPatch = [int]$Matches[2]
        }
    }
    $highestShippedPreview = 0
    $highestShippedPreviewTag = $null
    if ($previewTags.Count -gt 0) {
        $highestShippedPreviewTag = $previewTags[-1]
        if ($highestShippedPreviewTag -match $Script:StrictPreviewTagRegex) {
            $highestShippedPreview = [int]$Matches[2]
        }
    }
    Write-Host "[major $Major] Shipped patches:  $(if ($shippedPatches.Count -gt 0) { ($shippedPatches | Sort-Object) -join ', ' } else { '(none)' })" -ForegroundColor Cyan
    Write-Host "[major $Major] Shipped previews: $(if ($shippedPreviews.Count -gt 0) { ($shippedPreviews | Sort-Object) -join ', ' } else { '(none)' })" -ForegroundColor Cyan
    Write-Host "[major $Major] Highest stable tag:  $(if ($highestShippedTag) { $highestShippedTag } else { '(none)' })" -ForegroundColor Cyan
    Write-Host "[major $Major] Highest preview tag: $(if ($highestShippedPreviewTag) { $highestShippedPreviewTag } else { '(none)' })" -ForegroundColor Cyan

    $trackers = New-Object System.Collections.Generic.List[object]

    # ── Lane 1: in-flight SR branches.
    # Helper returns via unary-comma; assign directly (don't @() wrap).
    $srBranches = Get-RemoteSrBranchesForMajor -Major $Major
    Write-Host "[major $Major] Found $($srBranches.Count) strict release/$Major.0.*xx-sr* branches on origin" -ForegroundColor Cyan
    $highestBranchSr = 0
    $inflightBranchesBySr = @{}
    foreach ($entry in $srBranches) {
        $branch = $entry.branch
        $sr     = $entry.srNumber
        if ($sr -gt $highestBranchSr) { $highestBranchSr = $sr }

        Write-Verbose "Inspecting branch $branch (sr$sr)..."
        $versionInfo = Get-VersionFromGitRef -GitRef "origin/$branch" -Repo $Repo
        if (-not $versionInfo) {
            Write-Warning "[major $Major] Could not read Versions.props from origin/$branch — skipping (fail-soft for this branch)"
            continue
        }
        if ($versionInfo.Tag -notmatch '^(\d+)\.0\.(\d+)$') {
            Write-Warning "[major $Major] Versions.props on $branch produced unexpected tag '$($versionInfo.Tag)' — skipping"
            continue
        }
        $branchPatch = [int]$Matches[2]
        $expectedTag = $versionInfo.Tag

        if (Test-IsBranchInFlight -BranchPatch $branchPatch -ShippedPatches $shippedPatches) {
            $recent = Get-RecentCommitCount -Ref $branch -Days $ActivityWindowDays

            # Staleness guard: a tag-absent branch below the shipped watermark
            # with no recent activity is a stale/abandoned hotfix leftover
            # (e.g. SR2 patch 21 / SR3 patch 33 long after SR7 patch 71 shipped),
            # not a live in-flight SR. Dropping it here keeps it out of the
            # workflow matrix entirely (no no-op per-tracker job). Tag-existence
            # stays the primary signal; the idle requirement preserves the
            # out-of-order/hotfix case (a real reset branch has recent commits).
            if (Test-IsStaleSrBranch -BranchPatch $branchPatch -HighestShippedPatch $highestShippedPatch -RecentActivityCount $recent) {
                Write-Host "  -> skipping stale SR$sr branch '$branch' (patch=$branchPatch < highest shipped $highestShippedPatch, no tag $expectedTag, no commits in ${ActivityWindowDays}d)" -ForegroundColor DarkGray
                continue
            }

            $tracker = New-Tracker -Major $Major -SrNumber $sr -Mode 'in-flight' `
                -BranchName $branch -SurveyRef $branch -PriorSrBranch $null `
                -PriorShippedPatch $highestShippedPatch -PriorShippedTag $highestShippedTag `
                -ExpectedPatch $branchPatch -ExpectedTag $expectedTag `
                -HasRecentActivityCount $recent
            $trackers.Add($tracker)
            $inflightBranchesBySr[$sr] = $branch
            Write-Host "  -> in-flight SR tracker: SR$sr (patch=$branchPatch, no tag $expectedTag yet, recent=$recent)" -ForegroundColor Green
        } else {
            Write-Host "  -> SR$sr branch '$branch' patch=$branchPatch already shipped (tag $expectedTag exists)" -ForegroundColor DarkGray
        }
    }

    # ── Lane 2: propose next SR off main (or net<major>.0). Skip for pre-GA
    # majors: if `<major>.0.0` hasn't shipped, this major is still in preview
    # phase and there is no SR cycle yet.
    $isPreGa = -not $shippedPatches.Contains(0)
    if ($isPreGa) {
        Write-Host "[major $Major] Pre-GA (no tag $Major.0.0) — skipping Lane 2 (no SR candidate proposed)" -ForegroundColor DarkGray
    } else {
        $highestShippedSr = [int]([math]::Floor($highestShippedPatch / 10))
        $highestSr        = [int][math]::Max([int]$highestBranchSr, [int]$highestShippedSr)
        $nextSr           = $highestSr + 1
        $nextSrBranchExists = $srBranches | Where-Object { $_.srNumber -eq $nextSr }

        if (-not $nextSrBranchExists) {
            $candidateRef = $mainBranchForMajor
            $candidateVersionInfo = Get-VersionFromGitRef -GitRef "origin/$candidateRef" -Repo $Repo
            $expectedPatch = $nextSr * 10
            $expectedTag   = "$Major.0.$expectedPatch"
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
            # priorSrBranch: prefer the immediate prior SR's branch (sr<N-1>) since
            # the candidate by definition follows it. Falling back to "highest in-flight"
            # can pick stale forgotten branches (e.g. an old sr2/sr3 left around) — those
            # are NOT the prior of a current candidate.
            $priorSrNumber = $nextSr - 1
            $priorSrBranchName = "release/$Major.0.1xx-sr$priorSrNumber"
            $priorSrBranchExists = $srBranches | Where-Object { $_.branch -eq $priorSrBranchName }
            $priorSrBranch = $null
            if ($priorSrBranchExists) {
                $priorSrBranch = $priorSrBranchName
            } elseif ($inflightBranchesBySr.Count -gt 0) {
                $inflightPrior = ($inflightBranchesBySr.Keys | Where-Object { $_ -lt $nextSr } | Sort-Object | Select-Object -Last 1)
                if ($inflightPrior) { $priorSrBranch = $inflightBranchesBySr[$inflightPrior] }
            }
            if (-not $priorSrBranch -and $highestShippedSr -ge 1) {
                $priorSrBranch = "release/$Major.0.1xx-sr$highestShippedSr"
            }
            $tracker = New-Tracker -Major $Major -SrNumber $nextSr -Mode 'candidate' `
                -BranchName $null -SurveyRef $candidateRef -PriorSrBranch $priorSrBranch `
                -PriorShippedPatch $highestShippedPatch -PriorShippedTag $highestShippedTag `
                -ExpectedPatch $expectedPatch -ExpectedTag $expectedTag `
                -HasRecentActivityCount $recent
            $trackers.Add($tracker)
            Write-Host "  -> candidate SR tracker: SR$nextSr (surveyRef=$candidateRef, recent=$recent)" -ForegroundColor Green
        } else {
            Write-Host "  -> SR$nextSr already has a branch; covered by Lane 1" -ForegroundColor DarkGray
        }
    }

    # ── Lane 3: in-flight preview branches.
    $previewBranches = Get-RemotePreviewBranchesForMajor -Major $Major
    Write-Host "[major $Major] Found $($previewBranches.Count) strict release/$Major.0.*xx-preview* branches on origin" -ForegroundColor Cyan
    $highestBranchPreview = 0
    $inflightPreviewsByNum = @{}
    foreach ($entry in $previewBranches) {
        $branch = $entry.branch
        $previewN = $entry.previewNumber
        if ($previewN -gt $highestBranchPreview) { $highestBranchPreview = $previewN }

        if (Test-IsPreviewBranchInFlight -PreviewNumber $previewN -ShippedPreviews $shippedPreviews) {
            $recent = Get-RecentCommitCount -Ref $branch -Days $ActivityWindowDays
            $tracker = New-PreviewTracker -Major $Major -PreviewNumber $previewN -Mode 'in-flight' `
                -BranchName $branch -SurveyRef $branch `
                -HasRecentActivityCount $recent
            $trackers.Add($tracker)
            $inflightPreviewsByNum[$previewN] = $branch
            Write-Host "  -> in-flight preview tracker: preview$previewN (no $Major.0.0-preview.$previewN.* tag yet, recent=$recent)" -ForegroundColor Green
        } else {
            Write-Host "  -> preview$previewN branch '$branch' already shipped (tag $Major.0.0-preview.$previewN.* exists)" -ForegroundColor DarkGray
        }
    }

    # ── Lane 4: propose next preview off net<major>.0 (or main when main owns
    # the preview cycle). Reads PreReleaseVersionIteration from the survey ref;
    # if it's labeled 'preview' AND that iteration has no tag AND no matching
    # branch, emit a candidate preview tracker.
    #
    # Survey-ref selection:
    #   - Prefer net<major>.0 when it exists (it owns the preview cycle in
    #     cross-major state, e.g., net11.0 hosts the .NET 11 preview cycle
    #     while main is still on .NET 10's SR cycle).
    #   - Fall back to main only when net<major>.0 doesn't exist AND main is
    #     for this major. Main is typically the SR development line
    #     (label=ci.main, not preview), so the lookup will no-op when the
    #     major is in SR phase.
    $previewCandidateRef = $null
    $candidatePreviewVersionInfo = $null
    $netMajorBranch = "net$Major.0"
    $netMajorExists = $false
    try {
        $netMajorCheck = Invoke-GitOrFail @('ls-remote', '--heads', 'origin', $netMajorBranch) `
            "Could not check existence of $netMajorBranch"
        $netMajorExists = [bool]($netMajorCheck | Where-Object { $_ -and $_ -match '^[0-9a-f]{40}\s+refs/heads/' })
    } catch {
        Write-Warning "[major $Major] ls-remote for $netMajorBranch failed; falling back to main for Lane 4"
    }

    if ($netMajorExists) {
        $previewCandidateRef = $netMajorBranch
        $candidatePreviewVersionInfo = Get-VersionFromGitRef -GitRef "origin/$netMajorBranch" -Repo $Repo
    } elseif ($mainBranchForMajor -eq 'main') {
        $previewCandidateRef = 'main'
        $candidatePreviewVersionInfo = Get-VersionFromGitRef -GitRef "origin/main" -Repo $Repo
    }

    if ($candidatePreviewVersionInfo -and $candidatePreviewVersionInfo.PreLabel -eq 'preview' -and $candidatePreviewVersionInfo.PreIter -gt 0) {
        $candidatePreviewN = [int]$candidatePreviewVersionInfo.PreIter
        $previewBranchAlreadyExists = $previewBranches | Where-Object { $_.previewNumber -eq $candidatePreviewN }
        $previewAlreadyShipped = $shippedPreviews.Contains($candidatePreviewN)

        if ($previewAlreadyShipped) {
            Write-Host "[major $Major] preview$candidatePreviewN (from $previewCandidateRef) already shipped — skipping Lane 4" -ForegroundColor DarkGray
        } elseif ($previewBranchAlreadyExists) {
            Write-Host "[major $Major] preview$candidatePreviewN already has a branch; covered by Lane 3" -ForegroundColor DarkGray
        } else {
            $recent = Get-RecentCommitCount -Ref $previewCandidateRef -Days $ActivityWindowDays
            $tracker = New-PreviewTracker -Major $Major -PreviewNumber $candidatePreviewN -Mode 'candidate' `
                -BranchName $null -SurveyRef $previewCandidateRef `
                -HasRecentActivityCount $recent
            $trackers.Add($tracker)
            Write-Host "  -> candidate preview tracker: preview$candidatePreviewN (surveyRef=$previewCandidateRef, recent=$recent)" -ForegroundColor Green
        }
    } else {
        $labelDisplay = if ($candidatePreviewVersionInfo) { ($candidatePreviewVersionInfo.PreLabel) } else { '<n/a>' }
        $iterDisplay  = if ($candidatePreviewVersionInfo) { ($candidatePreviewVersionInfo.PreIter)  } else { '<n/a>' }
        $refDisplay   = if ($previewCandidateRef) { $previewCandidateRef } else { '<none>' }
        Write-Host "[major $Major] No active preview cycle (surveyRef=$refDisplay, label=$labelDisplay, iter=$iterDisplay)" -ForegroundColor DarkGray
    }

    return [pscustomobject]@{
        majorVersion             = $Major
        mainBranch               = $mainBranchForMajor
        highestShippedPatch      = $highestShippedPatch
        highestShippedTag        = $highestShippedTag
        highestShippedPreview    = $highestShippedPreview
        highestShippedPreviewTag = $highestShippedPreviewTag
        trackers                 = $trackers.ToArray()
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

if ($AllActiveMajors) {
    $activeMajors = Get-ActiveMajorVersions
    Write-Host "Active major versions: $($activeMajors -join ', ')" -ForegroundColor Cyan
    $perMajor = New-Object System.Collections.Generic.List[object]
    foreach ($m in $activeMajors) {
        $perMajor.Add( (Invoke-DetectionForMajor -Major $m) )
    }
    $result = [pscustomobject]@{
        detectedAt           = (Get-Date).ToUniversalTime().ToString('o')
        repo                 = (Resolve-Path $Repo).Path
        activityWindowDays   = $ActivityWindowDays
        majors               = $perMajor.ToArray()
    }
} else {
    # Single-major mode (back-compat with prior callers and the test E2E).
    if ($MajorVersion -le 0) {
        $MajorVersion = Get-CurrentMajorVersion -Repo $Repo
        Write-Host "Detected MajorVersion=$MajorVersion from origin/main:eng/Versions.props" -ForegroundColor Cyan
    }
    $single = Invoke-DetectionForMajor -Major $MajorVersion
    $result = [pscustomobject]@{
        detectedAt               = (Get-Date).ToUniversalTime().ToString('o')
        repo                     = (Resolve-Path $Repo).Path
        majorVersion             = $single.majorVersion
        mainBranch               = $single.mainBranch
        highestShippedPatch      = $single.highestShippedPatch
        highestShippedTag        = $single.highestShippedTag
        highestShippedPreview    = $single.highestShippedPreview
        highestShippedPreviewTag = $single.highestShippedPreviewTag
        activityWindowDays       = $ActivityWindowDays
        trackers                 = $single.trackers
    }
}

# ── Output ───────────────────────────────────────────────────────────────

$json = $result | ConvertTo-Json -Depth 8

if ($OutputJson) {
    $dir = Split-Path -Parent $OutputJson
    if ($dir -and -not (Test-Path $dir)) {
        New-Item -ItemType Directory -Path $dir -Force | Out-Null
    }
    Set-Content -Path $OutputJson -Value $json -Encoding utf8
    # Resilient tracker count — single-major mode has .trackers at the root,
    # AllActiveMajors mode aggregates .majors[].trackers. Total either way.
    $totalTrackers = 0
    if ($result.PSObject.Properties['trackers']) {
        $totalTrackers = @($result.trackers).Count
    } elseif ($result.PSObject.Properties['majors']) {
        $totalTrackers = ($result.majors | ForEach-Object { @($_.trackers).Count } | Measure-Object -Sum).Sum
    }
    Write-Host "Wrote $totalTrackers tracker(s) to $OutputJson" -ForegroundColor Cyan
} else {
    Write-Output $json
}
