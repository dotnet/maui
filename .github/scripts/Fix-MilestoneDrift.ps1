#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detects and fixes milestone drift for dotnet/maui releases.

.DESCRIPTION
    When PRs merge to inflight/current, they may get milestoned prematurely.
    The actual release they ship in depends on which Candidate PR carries them
    to main and which SR branch cut includes that commit. This script detects
    and reports (or optionally fixes) milestone mismatches.

    Modes:
    1. Single PR:   -PrNumber 33818 [-Tag 10.0.50]
    2. Single tag:  -Tag 10.0.50 [-PreviousTag 10.0.41]

    Safety: PRs merged before 2026-01-01 are always skipped.

.PARAMETER PrNumber
    Analyze and fix a single PR (and its linked issues).

.PARAMETER Tag
    Release tag to analyze (e.g., "10.0.50"). For single-PR mode, auto-detected if omitted.

.PARAMETER PreviousTag
    Previous release tag. Auto-detected if omitted.

.PARAMETER MajorVersion
    Major .NET version (default: 10).

.PARAMETER RepoPath
    Path to a dotnet/maui git checkout with full tag history.

.PARAMETER Output
    Output JSON file path.

.PARAMETER Apply
    Actually apply milestone fixes. Without this flag, only a dry-run report is produced.

.PARAMETER CreateIssue
    Create a GitHub issue in dotnet/maui with the milestone drift report.

.EXAMPLE
    ./Fix-MilestoneDrift.ps1 -PrNumber 33818 -RepoPath ~/Projects/maui -Verbose
    ./Fix-MilestoneDrift.ps1 -PrNumber 33818 -Apply
    ./Fix-MilestoneDrift.ps1 -Tag 10.0.50 -RepoPath ~/Projects/maui
#>

[CmdletBinding()]
param(
    [int]$PrNumber,
    [string]$Tag,
    [string]$PreviousTag,
    [int]$MajorVersion = 10,
    [string]$RepoPath = ".",
    [string]$Output,
    [switch]$Apply,
    [switch]$CreateIssue
)

# Safety: never process PRs merged before 2026
$script:MergedAfterCutoff = [datetime]::new(2026, 1, 1, 0, 0, 0, [System.DateTimeKind]::Utc)

# Only enable StrictMode during normal execution — not when dot-sourced for testing,
# since StrictMode leaks into the caller scope and can break Pester or other scripts.
if ($MyInvocation.InvocationName -ne '.') {
    Set-StrictMode -Version Latest
    $ErrorActionPreference = "Stop"
}

#region ── Milestone mapping helpers ──────────────────────────────────────

function Get-MainBranchForVersion([int]$Major, [string]$Repo) {
    <# Determines which development branch owns a .NET version by reading
       MajorVersion from eng/Versions.props on main. If main's MajorVersion
       matches, the version lives on main. Otherwise it's on net{Major}.0.
       This works correctly across version transitions — when main moves
       from .NET 10 to .NET 11, MajorVersion in Versions.props changes too. #>
    $versionXml = git -C $Repo --no-pager show origin/main:eng/Versions.props 2>&1
    if ($LASTEXITCODE -eq 0) {
        $joined = ($versionXml -join "`n")
        if ($joined -match '<MajorVersion>(\d+)</MajorVersion>') {
            $mainMajor = [int]$Matches[1]
            if ($mainMajor -eq $Major) { return "main" }
        }
    }
    return "net$Major.0"
}

function Get-VersionFromGitRef([string]$GitRef, [string]$Repo) {
    <# Reads MajorVersion and PatchVersion from eng/Versions.props at a specific
       git ref (commit SHA, branch, tag). Returns a synthetic release tag string
       like "10.0.60" that can be passed to ConvertTo-Milestone.
       Fetches the commit if not available locally (e.g. PRs merged to inflight). #>
    $versionXml = git -C $Repo --no-pager show "${GitRef}:eng/Versions.props" 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Commit not in local history — fetch it
        Write-Verbose "  Fetching commit $GitRef..."
        $null = git -C $Repo fetch origin $GitRef --quiet 2>&1
        $versionXml = git -C $Repo --no-pager show "${GitRef}:eng/Versions.props" 2>&1
        if ($LASTEXITCODE -ne 0) { return $null }
    }
    $joined = ($versionXml -join "`n")
    if ($joined -match '<MajorVersion>(\d+)</MajorVersion>') {
        $major = $Matches[1]
    } else { return $null }
    if ($joined -match '<PatchVersion>(\d+)</PatchVersion>') {
        $patch = $Matches[1]
    } else { return $null }
    return "$major.0.$patch"
}

function ConvertTo-Milestone([string]$ReleaseTag) {
    <# Converts "10.0.50" → ".NET 10 SR5", "10.0.41" → ".NET 10 SR4.1", "10.0.0" → ".NET 10.0 GA" #>
    if ($ReleaseTag -notmatch '^(\d+)\.0\.(\d+)$') { return $null }
    $major = [int]$Matches[1]; $patch = [int]$Matches[2]
    if ($patch -eq 0)  { return ".NET $major.0 GA" }
    if ($patch -lt 10) { return ".NET $major.0 SR1" }
    $sr = [math]::Floor($patch / 10)
    $sub = $patch % 10
    if ($sub -eq 0) { return ".NET $major SR$sr" }
    return ".NET $major SR$sr.$sub"
}

function Get-PatchVersion([string]$ReleaseTag) {
    if ($ReleaseTag -match '^(\d+)\.0\.(\d+)$') { return [int]$Matches[2] }
    return 0
}

function Test-IsSrTag([string]$ReleaseTag, [int]$Major) {
    return ($ReleaseTag -match "^$Major\.0\.\d+$")
}

function Test-MilestoneMatch([string]$Actual, [string]$Expected) {
    <# Handles ".NET 10.0 SR4" vs ".NET 10 SR4" normalization only.
       Sub-patches like ".NET 10 SR4.1" are distinct milestones and do NOT match ".NET 10 SR4". #>
    if ([string]::IsNullOrEmpty($Actual)) { return $false }
    if ($Actual -eq $Expected) { return $true }

    # Normalize: ".NET 10.0 SRx" → ".NET 10 SRx"
    $normActual   = $Actual   -replace '\.NET (\d+)\.0 SR', '.NET $1 SR'
    $normExpected = $Expected -replace '\.NET (\d+)\.0 SR', '.NET $1 SR'
    if ($normActual -eq $normExpected) { return $true }

    return $false
}

function Find-MatchingMilestone([string]$Expected, [hashtable]$AllMilestones) {
    <# Returns @{Title; Number} or $null #>
    if ($AllMilestones.ContainsKey($Expected)) {
        return @{ Title = $Expected; Number = $AllMilestones[$Expected] }
    }
    # Normalized search
    $normExpected = $Expected -replace '\.NET (\d+)\.0 SR', '.NET $1 SR'
    foreach ($key in $AllMilestones.Keys) {
        $normKey = $key -replace '\.NET (\d+)\.0 SR', '.NET $1 SR'
        if ($normKey -eq $normExpected) {
            return @{ Title = $key; Number = $AllMilestones[$key] }
        }
    }
    # Try ".NET 10 SR5" → ".NET 10.0 SR5"
    $alt = $Expected -replace '\.NET (\d+) SR', '.NET $1.0 SR'
    if ($AllMilestones.ContainsKey($alt)) {
        return @{ Title = $alt; Number = $AllMilestones[$alt] }
    }
    return $null
}

function Find-PreviousTag([string]$ReleaseTag, [string[]]$AllTags) {
    if ($ReleaseTag -notmatch '^(\d+)\.0\.(\d+)$') { return $null }
    $major = [int]$Matches[1]; $patch = [int]$Matches[2]
    $candidates = $AllTags | Where-Object {
        $_ -match "^$major\.0\.(\d+)$" -and [int]$Matches[1] -lt $patch
    } | Sort-Object { Get-PatchVersion $_ }
    return ($candidates | Select-Object -Last 1)
}

function Test-PrBelongsToVersion([string]$BaseRef, [string]$MainBranch, [int]$Major) {
    <# Checks if a PR's base branch is compatible with the version being analyzed.
       Prevents merge-up commits from causing incorrect milestoning.
       E.g. when main (.NET 10) merges into net11.0, the .NET 10 PRs should not
       be milestoned as .NET 11. #>
    if ([string]::IsNullOrEmpty($BaseRef)) { return $true }

    # PR targets the main branch for this version — always allowed
    if ($BaseRef -eq $MainBranch) { return $true }

    # Allow inflight/darc branches only if they feed into this version's main branch
    if ($BaseRef -match '^(inflight|darc)/') {
        # inflight/* and darc/* branches feed into 'main', so only allow when MainBranch is 'main'
        return ($MainBranch -eq 'main')
    }

    # Release branches: only allow if they match this major version
    if ($BaseRef -match '^release/(\d+)\.') {
        return ([int]$Matches[1] -eq $Major)
    }

    # Reject PRs explicitly targeting a different .NET version branch (net11.0, net12.0, etc.)
    if ($BaseRef -match '^net(\d+)\.\d+$') {
        return ([int]$Matches[1] -eq $Major)
    }

    # Reject 'main' when we're analyzing a non-main version (e.g. .NET 11 on net11.0)
    # PRs targeting 'main' belong to the .NET version that lives on main, not this one.
    if ($BaseRef -eq 'main' -and $MainBranch -ne 'main') {
        return $false
    }

    # Unknown branches (feature/*, etc.) — allow
    return $true
}

#endregion

#region ── Git helpers ────────────────────────────────────────────────────

function Get-AllTags([string]$Repo) {
    $output = git -C $Repo --no-pager tag -l 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git tag failed: $output" }
    return ($output -split "`n" | Where-Object { $_ })
}

function Get-PrNumbersBetweenTags([string]$TagFrom, [string]$TagTo, [string]$Repo) {
    $output = git -C $Repo --no-pager log --oneline "$TagFrom..$TagTo" 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git log failed: $output" }
    $prs = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($line in ($output -split "`n")) {
        foreach ($m in [regex]::Matches($line, '\(#(\d+)\)')) {
            [void]$prs.Add([int]$m.Groups[1].Value)
        }
    }
    return ($prs | Sort-Object)
}

function Get-PrNumbersReachableFromTag([string]$TagName, [string]$Repo) {
    <# Returns PR numbers reachable from a tag (all commits up to and including the tag). #>
    $output = git -C $Repo --no-pager log --oneline $TagName 2>&1
    if ($LASTEXITCODE -ne 0) { throw "git log failed: $output" }
    $prs = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($line in ($output -split "`n")) {
        foreach ($m in [regex]::Matches($line, '\(#(\d+)\)')) {
            [void]$prs.Add([int]$m.Groups[1].Value)
        }
    }
    return ($prs | Sort-Object)
}

function Find-TagContainingPr([int]$PrNum, [string]$Repo, [int]$Major) {
    <# Searches tag ranges to find which release contains this PR.
       Handles GA (first tag) by searching all reachable commits. #>
    $allTags = Get-AllTags $Repo
    $srTags = $allTags | Where-Object { Test-IsSrTag $_ $Major } |
              Sort-Object { Get-PatchVersion $_ }

    for ($i = 0; $i -lt $srTags.Count; $i++) {
        $current = $srTags[$i]

        if ($i -eq 0) {
            # First tag (e.g. GA) — search all commits reachable from it
            $prsInRange = Get-PrNumbersReachableFromTag $current $Repo
        } else {
            $prev = $srTags[$i - 1]
            $prsInRange = Get-PrNumbersBetweenTags $prev $current $Repo
        }

        if ($PrNum -in $prsInRange) {
            $previousTag = if ($i -gt 0) { $srTags[$i - 1] } else { $null }
            return @{ Tag = $current; PreviousTag = $previousTag }
        }
    }
    return $null
}

#endregion

#region ── GitHub helpers ─────────────────────────────────────────────────

function Invoke-GhApi([string]$Endpoint) {
    $result = gh api $Endpoint 2>&1
    if ($LASTEXITCODE -ne 0) { throw "gh api $Endpoint failed: $result" }
    return ($result | ConvertFrom-Json)
}

function Get-AllMilestones {
    $milestones = @{}; $page = 1
    do {
        $data = Invoke-GhApi "repos/dotnet/maui/milestones?state=all&per_page=100&page=$page"
        foreach ($ms in $data) { $milestones[$ms.title] = $ms.number }
        $page++
    } while ($data.Count -eq 100)
    return $milestones
}

function Get-PrInfo([int]$PrNum) {
    try {
        $pr = Invoke-GhApi "repos/dotnet/maui/pulls/$PrNum"
        # Safety: skip PRs merged before cutoff date
        if ($pr.merged_at) {
            # ConvertFrom-Json may return a DateTime or a string depending on PS version
            $mergedAt = if ($pr.merged_at -is [datetime]) {
                $pr.merged_at.ToUniversalTime()
            } else {
                [datetime]::Parse($pr.merged_at, [System.Globalization.CultureInfo]::InvariantCulture,
                    [System.Globalization.DateTimeStyles]::AdjustToUniversal)
            }
            if ($mergedAt -lt $script:MergedAfterCutoff) {
                Write-Warning "PR #$PrNum merged $($pr.merged_at) — before cutoff ($($script:MergedAfterCutoff.ToString('yyyy-MM-dd'))). Skipping."
                return $null
            }
        }
        return @{
            Number         = $PrNum
            Title          = $pr.title
            Milestone      = if ($pr.milestone) { $pr.milestone.title } else { $null }
            MsNumber       = if ($pr.milestone) { $pr.milestone.number } else { $null }
            Url            = $pr.html_url
            Body           = if ($pr.body) { $pr.body } else { "" }
            BaseRef        = if ($pr.base -and $pr.base.ref) { $pr.base.ref } else { $null }
            MergeCommitSha = if ($pr.merge_commit_sha) { $pr.merge_commit_sha } else { $null }
        }
    } catch {
        Write-Warning "Failed to fetch PR #$PrNum`: $_"
        return $null
    }
}

function Get-IssueInfo([int]$IssueNumber) {
    try {
        $issue = Invoke-GhApi "repos/dotnet/maui/issues/$IssueNumber"
        if ($issue.PSObject.Properties['pull_request'] -and $issue.pull_request) { return $null }
        return @{
            Number    = $IssueNumber
            Title     = $issue.title
            State     = $issue.state
            Milestone = if ($issue.milestone) { $issue.milestone.title } else { $null }
            MsNumber  = if ($issue.milestone) { $issue.milestone.number } else { $null }
            Url       = $issue.html_url
        }
    } catch {
        Write-Warning "Failed to fetch issue #$IssueNumber`: $_"
        return $null
    }
}

function Get-LinkedIssues([string]$Body, [string]$Title) {
    $text = "$Title`n$Body"
    $issues = [System.Collections.Generic.HashSet[int]]::new()
    foreach ($m in [regex]::Matches($text, '(?:fix(?:es|ed)?|clos(?:es|ed)?|resolv(?:es|ed)?)\s+#(\d+)', 'IgnoreCase')) {
        [void]$issues.Add([int]$m.Groups[1].Value)
    }
    # Match URLs only when preceded by a fixing keyword (mirrors GitHub auto-close behavior).
    # Bare URLs like "See https://github.com/.../issues/123" are informational, not fixing references.
    foreach ($m in [regex]::Matches($text, '(?:fix(?:es|ed)?|clos(?:es|ed)?|resolv(?:es|ed)?)\s+https?://github\.com/dotnet/maui/issues/(\d+)', 'IgnoreCase')) {
        [void]$issues.Add([int]$m.Groups[1].Value)
    }
    return ($issues | Sort-Object)
}

function Set-ItemMilestone([int]$ItemNumber, [int]$MilestoneNumber) {
    $body = @{ milestone = $MilestoneNumber } | ConvertTo-Json
    $result = $body | gh api "repos/dotnet/maui/issues/$ItemNumber" -X PATCH --input - 2>&1
    if ($LASTEXITCODE -ne 0) { throw "Failed to set milestone on #$ItemNumber`: $result" }
}

#endregion

#region ── Correction helpers ─────────────────────────────────────────────

function Test-AndRecordCorrection(
    [string]$ItemType,
    [int]$ItemNumber,
    [string]$ItemTitle,
    [string]$ItemUrl,
    [string]$CurrentMilestone,
    [string]$ExpectedMs,
    [hashtable]$ResolvedMs,
    [int]$RelatedPr,
    [hashtable]$Report
) {
    if (Test-MilestoneMatch $CurrentMilestone $ExpectedMs) {
        $Report.AlreadyCorrect++
        Write-Verbose "  ✅ $ItemType #$ItemNumber`: $CurrentMilestone (correct)"
        return
    }

    # Skip if a correction for this item was already recorded (e.g. same issue linked from multiple PRs)
    $existing = $Report.Corrections | Where-Object { $_.ItemType -eq $ItemType -and $_.Number -eq $ItemNumber }
    if ($existing) {
        Write-Verbose "  ⏭️  $ItemType #$ItemNumber`: already queued for correction (via PR #$($existing.RelatedPr))"
        return
    }

    $correction = @{
        ItemType   = $ItemType
        Number     = $ItemNumber
        Title      = $ItemTitle
        Url        = $ItemUrl
        Current    = $CurrentMilestone
        Expected   = $ExpectedMs
        Resolved   = $ResolvedMs.Title
        ResolvedNo = $ResolvedMs.Number
    }
    if ($RelatedPr -gt 0) { $correction.RelatedPr = $RelatedPr }
    [void]$Report.Corrections.Add($correction)

    $current = if ($CurrentMilestone) { $CurrentMilestone } else { "(none)" }
    Write-Verbose "  ⚠️  $ItemType #$ItemNumber`: $current → $($ResolvedMs.Title)"
}

#endregion

#region ── Analysis ───────────────────────────────────────────────────────

function Invoke-AnalyzeSinglePr([int]$PrNum, [string]$ReleaseTag, [string]$Repo, [int]$Major) {
    Write-Host "`n$('═' * 70)"
    Write-Host "  Single-PR mode: #$PrNum"
    Write-Host "$('═' * 70)`n"

    # Fetch PR info first — we need merge_commit_sha for version detection
    $pr = Get-PrInfo $PrNum
    if (-not $pr) { throw "Could not fetch PR #$PrNum" }

    if ($ReleaseTag) {
        # Explicit tag: validate it exists and optionally check PR is in range
        $allTags = Get-AllTags $Repo
        if ($ReleaseTag -notin $allTags) {
            throw "Tag '$ReleaseTag' not found in repo. Available SR tags: $(($allTags | Where-Object { $_ -match '^\d+\.0\.\d+$' }) -join ', ')"
        }
        $prev = Find-PreviousTag $ReleaseTag $allTags
        if ($prev) {
            $prsInRange = Get-PrNumbersBetweenTags $prev $ReleaseTag $Repo
        } else {
            $prsInRange = Get-PrNumbersReachableFromTag $ReleaseTag $Repo
        }
        if ($PrNum -notin $prsInRange) {
            Write-Warning "PR #$PrNum is not in the commit range for tag $ReleaseTag. The milestone may be set incorrectly."
        }
        Write-Host "  Using explicit tag: $ReleaseTag"
    } elseif ($pr.MergeCommitSha) {
        # No tag — read Versions.props at the merge commit to determine
        # the version the branch was building when this PR merged.
        $versionAtMerge = Get-VersionFromGitRef $pr.MergeCommitSha $Repo
        if ($versionAtMerge) {
            $ReleaseTag = $versionAtMerge
            Write-Host "  Version from Versions.props at merge commit: $ReleaseTag"
        }
    }

    # Fallback: try to find in existing tag ranges
    if (-not $ReleaseTag) {
        Write-Host "Auto-detecting release tag for PR #$PrNum..."
        $found = Find-TagContainingPr $PrNum $Repo $Major
        if (-not $found) { throw "PR #$PrNum not found in any release tag range for .NET $Major" }
        $ReleaseTag = $found.Tag
        $prevDisplay = if ($found.PreviousTag) { $found.PreviousTag } else { "(root)" }
        Write-Host "  Found in: $prevDisplay..$ReleaseTag"
    }

    $expectedMs = ConvertTo-Milestone $ReleaseTag
    if (-not $expectedMs) { throw "Cannot determine milestone for tag $ReleaseTag" }

    # Auto-detect MajorVersion from the tag if not explicitly set
    if ($ReleaseTag -match '^(\d+)\.') { $Major = [int]$Matches[1] }

    # Detect which branch owns this version's tags
    $Branch = Get-MainBranchForVersion $Major $Repo
    Write-Host "  Main branch for .NET $Major`: $Branch"
    Write-Host "  Expected milestone: $expectedMs"
    Write-Host "Fetching GitHub milestones..."
    $allMilestones = Get-AllMilestones
    $match = Find-MatchingMilestone $expectedMs $allMilestones
    if (-not $match) {
        throw "No GitHub milestone found matching `"$expectedMs`". Available: $($allMilestones.Keys -join ', ')"
    }
    Write-Host "  Resolved to: `"$($match.Title)`" (#$($match.Number))`n"

    $report = @{
        Tag               = $ReleaseTag
        ExpectedMilestone = $expectedMs
        ResolvedMilestone = $match.Title
        ResolvedMsNumber  = $match.Number
        TotalPrs          = 1
        PrsChecked        = 0
        IssuesChecked     = 0
        AlreadyCorrect    = 0
        Corrections       = [System.Collections.ArrayList]::new()
        Errors            = [System.Collections.ArrayList]::new()
    }

    $pr = Get-PrInfo $PrNum
    if (-not $pr) { throw "Could not fetch PR #$PrNum" }

    # Safety: skip PRs targeting a different .NET version branch
    if (-not (Test-PrBelongsToVersion $pr.BaseRef $Branch $Major)) {
        Write-Warning "PR #$PrNum targets '$($pr.BaseRef)' which is not for .NET $Major (MainBranch: $Branch). Skipping."
        return $report
    }
    $report.PrsChecked++

    Test-AndRecordCorrection "pr" $PrNum $pr.Title $pr.Url $pr.Milestone $expectedMs $match 0 $report

    # Check linked issues
    $linked = Get-LinkedIssues $pr.Body $pr.Title
    foreach ($issueNum in $linked) {
        $issue = Get-IssueInfo $issueNum
        if (-not $issue) { continue }
        $report.IssuesChecked++
        Test-AndRecordCorrection "issue" $issueNum $issue.Title $issue.Url $issue.Milestone $expectedMs $match $PrNum $report
    }

    return $report
}

function Invoke-AnalyzeRelease([string]$ReleaseTag, [string]$PrevTag, [string]$Repo, [int]$Major) {
    $expectedMs = ConvertTo-Milestone $ReleaseTag
    if (-not $expectedMs) { throw "Cannot determine milestone for tag $ReleaseTag" }

    $allTags = Get-AllTags $Repo
    if ($ReleaseTag -notin $allTags) { throw "Tag $ReleaseTag not found in repo" }

    if (-not $PrevTag) {
        $PrevTag = Find-PreviousTag $ReleaseTag $allTags
        if (-not $PrevTag) { throw "Cannot determine previous tag for $ReleaseTag" }
    }

    # Detect which branch owns this version's tags
    $Branch = Get-MainBranchForVersion $Major $Repo

    Write-Host "`n$('═' * 70)"
    Write-Host "  Release: $ReleaseTag"
    Write-Host "  Previous: $PrevTag"
    Write-Host "  Main branch: $Branch"
    Write-Host "  Expected milestone: $expectedMs"
    Write-Host "$('═' * 70)`n"

    Write-Host "Fetching GitHub milestones..."
    $allMilestones = Get-AllMilestones
    $match = Find-MatchingMilestone $expectedMs $allMilestones
    if (-not $match) {
        throw "No GitHub milestone found matching `"$expectedMs`". Available: $($allMilestones.Keys -join ', ')"
    }
    Write-Host "  Resolved to: `"$($match.Title)`" (#$($match.Number))`n"

    Write-Host "Finding PRs between $PrevTag..$ReleaseTag..."
    $prNumbers = Get-PrNumbersBetweenTags $PrevTag $ReleaseTag $Repo
    Write-Host "  Found $($prNumbers.Count) PRs`n"

    $report = @{
        Tag               = $ReleaseTag
        PreviousTag       = $PrevTag
        ExpectedMilestone = $expectedMs
        ResolvedMilestone = $match.Title
        ResolvedMsNumber  = $match.Number
        TotalPrs          = $prNumbers.Count
        PrsChecked        = 0
        IssuesChecked     = 0
        AlreadyCorrect    = 0
        Corrections       = [System.Collections.ArrayList]::new()
        Errors            = [System.Collections.ArrayList]::new()
    }

    for ($i = 0; $i -lt $prNumbers.Count; $i++) {
        $prNum = $prNumbers[$i]
        Write-Verbose "  [$($i+1)/$($prNumbers.Count)] PR #$prNum..."

        $pr = Get-PrInfo $prNum
        if (-not $pr) {
            [void]$report.Errors.Add("Failed to fetch PR #$prNum")
            continue
        }

        # Safety: skip PRs targeting a different .NET version branch
        if (-not (Test-PrBelongsToVersion $pr.BaseRef $Branch $Major)) {
            Write-Verbose "  ⏭️  PR #$prNum targets '$($pr.BaseRef)' — not for .NET $Major, skipping"
            $report.PrsSkippedWrongBranch = ($report.PrsSkippedWrongBranch ?? 0) + 1
            continue
        }
        $report.PrsChecked++

        Test-AndRecordCorrection "pr" $prNum $pr.Title $pr.Url $pr.Milestone $expectedMs $match 0 $report

        $linked = Get-LinkedIssues $pr.Body $pr.Title
        foreach ($issueNum in $linked) {
            $issue = Get-IssueInfo $issueNum
            if (-not $issue) { continue }
            $report.IssuesChecked++
            Test-AndRecordCorrection "issue" $issueNum $issue.Title $issue.Url $issue.Milestone $expectedMs $match $prNum $report
        }
    }

    return $report
}

#endregion

#region ── Output ─────────────────────────────────────────────────────────

function Write-Report([hashtable]$Report) {
    Write-Host "`n$('═' * 70)"
    Write-Host "  MILESTONE DRIFT REPORT: $($Report.Tag)"
    Write-Host "$('═' * 70)"
    if ($Report.ContainsKey('PreviousTag') -and $Report.PreviousTag) { Write-Host "  Range: $($Report.PreviousTag)..$($Report.Tag)" }
    Write-Host "  Expected milestone: $($Report.ExpectedMilestone)"
    Write-Host "  Resolved milestone: $($Report.ResolvedMilestone)"
    Write-Host "  PRs in range: $($Report.TotalPrs)"
    Write-Host "  PRs checked: $($Report.PrsChecked)"
    if ($Report.ContainsKey('PrsSkippedWrongBranch') -and $Report.PrsSkippedWrongBranch -gt 0) {
        Write-Host "  PRs skipped (wrong branch): $($Report.PrsSkippedWrongBranch)"
    }
    Write-Host "  Issues checked: $($Report.IssuesChecked)"
    Write-Host "  Already correct: $($Report.AlreadyCorrect)"
    Write-Host "  Corrections needed: $($Report.Corrections.Count)"
    if ($Report.Errors.Count -gt 0) { Write-Host "  Errors: $($Report.Errors.Count)" }
    Write-Host ""

    if ($Report.Corrections.Count -eq 0) {
        Write-Host "  ✅ All milestones are correct!`n"
        return
    }

    foreach ($c in $Report.Corrections) {
        $current = if ($c.Current) { $c.Current } else { "(none)" }
        $action = if ($c.Current) { "CHANGE" } else { "SET" }
        $via = if ($c.ContainsKey('RelatedPr') -and $c.RelatedPr) { " (via PR #$($c.RelatedPr))" } else { "" }
        Write-Host "  [$action] $($c.ItemType) #$($c.Number)$via`: $current → $($c.Resolved)"
    }
    Write-Host ""
}

function Save-ReportJson([hashtable]$Report, [string]$Path) {
    $data = @{
        tag                = $Report.Tag
        previous_tag       = $Report.PreviousTag
        expected_milestone = $Report.ExpectedMilestone
        resolved_milestone = $Report.ResolvedMilestone
        summary            = @{
            total_prs_in_range = $Report.TotalPrs
            prs_checked        = $Report.PrsChecked
            issues_checked     = $Report.IssuesChecked
            already_correct    = $Report.AlreadyCorrect
            corrections_needed = $Report.Corrections.Count
            errors             = $Report.Errors.Count
        }
        corrections        = @($Report.Corrections)
        errors             = @($Report.Errors)
    }
    $data | ConvertTo-Json -Depth 5 | Set-Content -Path $Path -Encoding utf8
    Write-Host "  📄 Report saved to: $Path"
}

function Invoke-ApplyCorrections([hashtable]$Report, [bool]$DoApply) {
    foreach ($c in $Report.Corrections) {
        $current = if ($c.Current) { $c.Current } else { "(none)" }
        if ($DoApply) {
            try {
                Set-ItemMilestone $c.Number $c.ResolvedNo
                Write-Host "  ✅ Updated $($c.ItemType) #$($c.Number): $current → $($c.Resolved)"
            } catch {
                Write-Host "  ❌ Failed $($c.ItemType) #$($c.Number): $_"
            }
        } else {
            Write-Host "  [DRY-RUN] Would set $($c.ItemType) #$($c.Number) milestone: $current → $($c.Resolved)"
        }
    }
}

function New-GitHubIssue([hashtable]$Report, [bool]$WasApplied) {
    $tag = $Report.Tag
    $status = if ($WasApplied) { "Applied" } else { "Dry-Run" }
    $title = "Milestone drift report: $tag ($status)"

    $sb = [System.Text.StringBuilder]::new()
    [void]$sb.AppendLine("## Milestone Drift Report: ``$tag``")
    [void]$sb.AppendLine()
    if ($Report.ContainsKey('PreviousTag') -and $Report.PreviousTag) {
        [void]$sb.AppendLine("**Range:** ``$($Report.PreviousTag)..$tag``")
    }
    [void]$sb.AppendLine("**Expected milestone:** $($Report.ExpectedMilestone)")
    [void]$sb.AppendLine("**Resolved milestone:** $($Report.ResolvedMilestone)")
    [void]$sb.AppendLine("**Status:** $status")
    [void]$sb.AppendLine()

    # Summary table
    [void]$sb.AppendLine("### Summary")
    [void]$sb.AppendLine()
    [void]$sb.AppendLine("| Metric | Count |")
    [void]$sb.AppendLine("|--------|-------|")
    [void]$sb.AppendLine("| PRs in range | $($Report.TotalPrs) |")
    [void]$sb.AppendLine("| PRs checked | $($Report.PrsChecked) |")
    [void]$sb.AppendLine("| Issues checked | $($Report.IssuesChecked) |")
    [void]$sb.AppendLine("| Already correct | $($Report.AlreadyCorrect) |")
    [void]$sb.AppendLine("| Corrections needed | $($Report.Corrections.Count) |")
    if ($Report.Errors.Count -gt 0) {
        [void]$sb.AppendLine("| Errors | $($Report.Errors.Count) |")
    }
    [void]$sb.AppendLine()

    if ($Report.Corrections.Count -eq 0) {
        [void]$sb.AppendLine("✅ All milestones are correct!")
    } else {
        # Corrections table
        [void]$sb.AppendLine("### Corrections")
        [void]$sb.AppendLine()
        [void]$sb.AppendLine("| Action | Type | Item | Via PR | Current | Expected |")
        [void]$sb.AppendLine("|--------|------|------|--------|---------|----------|")
        foreach ($c in $Report.Corrections) {
            $current = if ($c.Current) { $c.Current } else { "_(none)_" }
            $action = if ($c.Current) { "CHANGE" } else { "SET" }
            $via = if ($c.ContainsKey('RelatedPr') -and $c.RelatedPr) { "#$($c.RelatedPr)" } else { "—" }
            [void]$sb.AppendLine("| $action | $($c.ItemType) | #$($c.Number) | $via | $current | $($c.Resolved) |")
        }
    }

    $body = $sb.ToString()

    # Write body to a temp file to avoid shell quoting issues
    $tempFile = [System.IO.Path]::GetTempFileName()
    try {
        $body | Set-Content -Path $tempFile -Encoding utf8 -NoNewline
        $result = gh issue create --repo dotnet/maui --title $title --body-file $tempFile --label "area/infrastructure 🏗️" 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Failed to create issue: $result" }
        Write-Host "  📋 Issue created: $result"
    } finally {
        Remove-Item $tempFile -ErrorAction SilentlyContinue
    }
}

#endregion

#region ── Main ───────────────────────────────────────────────────────────

# Guard: skip main execution when dot-sourced for testing
if ($MyInvocation.InvocationName -eq '.' -or $MyInvocation.Line -match '^\.\s') { return }

if ($Apply) {
    Write-Host "⚠️  --Apply mode: Will modify GitHub milestones!"
}

if ($PrNumber -gt 0) {
    $report = Invoke-AnalyzeSinglePr $PrNumber $Tag $RepoPath $MajorVersion
    Write-Report $report
    if ($Output) { Save-ReportJson $report $Output }
    Invoke-ApplyCorrections $report $Apply.IsPresent
    if ($CreateIssue -and $report.Corrections.Count -gt 0) { New-GitHubIssue $report $Apply.IsPresent }
}
elseif ($Tag) {
    # Auto-detect MajorVersion from the tag if not explicitly overridden
    if ($Tag -match '^(\d+)\.' -and -not $PSBoundParameters.ContainsKey('MajorVersion')) {
        $MajorVersion = [int]$Matches[1]
    }
    $report = Invoke-AnalyzeRelease $Tag $PreviousTag $RepoPath $MajorVersion
    Write-Report $report
    $outPath = if ($Output) { $Output } else { "milestone-drift-$($Tag -replace '\.','_').json" }
    Save-ReportJson $report $outPath
    Invoke-ApplyCorrections $report $Apply.IsPresent
    if ($CreateIssue -and $report.Corrections.Count -gt 0) { New-GitHubIssue $report $Apply.IsPresent }
}
else {
    Write-Host "Error: -PrNumber or -Tag is required." -ForegroundColor Red
    Get-Help $MyInvocation.MyCommand.Path -Detailed
    exit 1
}

#endregion
