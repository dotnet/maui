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

function Get-CurrentMajorVersion([string]$Repo) {
    <# Reads MajorVersion from eng/Versions.props on origin/main. #>
    $versionXml = git -C $Repo --no-pager show origin/main:eng/Versions.props 2>&1
    if ($LASTEXITCODE -eq 0) {
        $joined = ($versionXml -join "`n")
        if ($joined -match '<MajorVersion>(\d+)</MajorVersion>') {
            return [int]$Matches[1]
        }
    }
    throw "Could not read MajorVersion from origin/main:eng/Versions.props"
}

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
            Write-Verbose "origin/main has MajorVersion=$mainMajor, not $Major — version lives on net$Major.0"
            return "net$Major.0"
        }
    }
    Write-Warning "Could not read MajorVersion from origin/main:eng/Versions.props — falling back to net$Major.0"
    return "net$Major.0"
}

function Get-VersionFromGitRef([string]$GitRef, [string]$Repo) {
    <# Reads version info from eng/Versions.props at a specific git ref.
       Returns a hashtable with Tag (synthetic release tag like "10.0.60"),
       PreLabel (e.g. "preview", "rc", or $null for stable),
       and PreIter (e.g. 3).
       Fetches the commit if not available locally (e.g. PRs merged to inflight). #>
    $versionXml = git -C $Repo --no-pager show "${GitRef}:eng/Versions.props" 2>&1
    if ($LASTEXITCODE -ne 0) {
        # Ref not in local history — fetch it.
        # Strip "origin/" prefix for the fetch refspec (git fetch origin <branch>, not origin/origin/<branch>)
        $fetchRef = $GitRef -replace '^origin/', ''
        Write-Verbose "  Fetching ref $fetchRef..."
        $null = git -C $Repo fetch origin $fetchRef --quiet 2>&1
        $versionXml = git -C $Repo --no-pager show "${GitRef}:eng/Versions.props" 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Could not read Versions.props at $GitRef (even after fetch)"
            return $null
        }
    }
    $joined = ($versionXml -join "`n")
    if ($joined -match '<MajorVersion>(\d+)</MajorVersion>') {
        $major = $Matches[1]
    } else {
        Write-Warning "Could not parse MajorVersion from Versions.props at $GitRef"
        return $null
    }
    if ($joined -match '<PatchVersion>(\d+)</PatchVersion>') {
        $patch = $Matches[1]
    } else {
        Write-Warning "Could not parse PatchVersion from Versions.props at $GitRef"
        return $null
    }

    # Detect pre-release label (preview, rc) and iteration
    $preLabel = $null
    $preIter = $null
    if ($joined -match '<PreReleaseVersionLabel[^>]*>([^<]+)</PreReleaseVersionLabel>') {
        $rawLabel = $Matches[1]
        # Only treat "preview" and "rc" as pre-release; "ci.main", "ci.inflight", "servicing" are stable builds
        if ($rawLabel -match '^(preview|rc)$') {
            $preLabel = $rawLabel
            if ($joined -match '<PreReleaseVersionIteration>(\d+)</PreReleaseVersionIteration>') {
                $preIter = [int]$Matches[1]
            }
        }
    }

    return @{
        Tag       = "$major.0.$patch"
        PreLabel  = $preLabel
        PreIter   = $preIter
    }
}

function ConvertTo-Milestone([string]$ReleaseTag, [string]$PreLabel, [int]$PreIter) {
    <# Converts version info to a milestone name:
       "10.0.50"                    → ".NET 10 SR5"
       "10.0.41"                    → ".NET 10 SR4.1"
       "10.0.0"                     → ".NET 10.0 GA"
       "11.0.0" + preview + 3       → ".NET 11.0-preview3"
       "11.0.0" + rc + 1            → ".NET 11.0-rc1" #>
    if ($ReleaseTag -notmatch '^(\d+)\.0\.(\d+)$') { return $null }
    $major = [int]$Matches[1]; $patch = [int]$Matches[2]

    # Pre-release: preview/rc milestones
    if ($PreLabel -and $PreIter -gt 0) {
        return ".NET $major.0-$PreLabel$PreIter"
    }
    if ($PreLabel -and $PreIter -le 0) {
        Write-Warning "PreReleaseVersionLabel is '$PreLabel' but PreReleaseVersionIteration is missing or 0 — falling back to GA/SR mapping"
    }

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

function Test-IsReleaseTag([string]$ReleaseTag, [int]$Major) {
    # Matches stable tags (10.0.50) and preview/RC tags (11.0.0-preview.3.26203.7)
    return ($ReleaseTag -match "^$Major\.0\.")
}

function Get-TagSortKey([string]$ReleaseTag) {
    <# Returns a numeric sort key for ordering tags chronologically.
       preview1 (100) < preview7 (107) < rc1 (200) < rc2 (201) < GA/stable (500+patch) #>
    if ($ReleaseTag -match '-preview\.(\d+)') { return 100 + [int]$Matches[1] }
    if ($ReleaseTag -match '-rc\.(\d+)')      { return 200 + [int]$Matches[1] }
    if ($ReleaseTag -match '^(\d+)\.0\.(\d+)$') { return 500 + [int]$Matches[2] }
    return 0
}

function Test-MilestoneMatch([string]$Actual, [string]$Expected) {
    <# Handles ".NET 10.0 SR4" vs ".NET 10 SR4" and ".NET 10.0 GA" vs ".NET 10 GA" normalization.
       Sub-patches like ".NET 10 SR4.1" are distinct milestones and do NOT match ".NET 10 SR4". #>
    if ([string]::IsNullOrEmpty($Actual)) { return $false }
    if ($Actual -eq $Expected) { return $true }

    # Normalize: ".NET 10.0 SRx" → ".NET 10 SRx" and ".NET 10.0 GA" → ".NET 10 GA"
    $normActual   = $Actual   -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
    $normExpected = $Expected -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
    if ($normActual -eq $normExpected) { return $true }

    return $false
}

function Find-MatchingMilestone([string]$Expected, [hashtable]$AllMilestones) {
    <# Returns @{Title; Number} or $null #>
    if ($AllMilestones.ContainsKey($Expected)) {
        return @{ Title = $Expected; Number = $AllMilestones[$Expected] }
    }
    # Normalized search (handles ".NET 10.0 SRx" ↔ ".NET 10 SRx" and ".NET 10.0 GA" ↔ ".NET 10 GA")
    $normExpected = $Expected -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
    foreach ($key in $AllMilestones.Keys) {
        $normKey = $key -replace '\.NET (\d+)\.0 (SR|GA)', '.NET $1 $2'
        if ($normKey -eq $normExpected) {
            return @{ Title = $key; Number = $AllMilestones[$key] }
        }
    }
    return $null
}

function Find-PreviousTag([string]$ReleaseTag, [string[]]$AllTags) {
    <# Finds the immediately preceding tag for the same major version.
       Works for both stable tags (10.0.50 → 10.0.41) and preview/RC tags
       (11.0.0-preview.3.x → 11.0.0-preview.2.x). #>
    if ($ReleaseTag -notmatch '^(\d+)\.') { return $null }
    $major = [int]$Matches[1]
    $thisKey = Get-TagSortKey $ReleaseTag

    # Find all tags for this major version with a lower sort key
    $candidates = $AllTags | Where-Object {
        ($_ -match "^$major\.0\.") -and (Get-TagSortKey $_) -lt $thisKey
    } | Sort-Object { Get-TagSortKey $_ }
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
    $srTags = $allTags | Where-Object { Test-IsReleaseTag $_ $Major } |
              Sort-Object { Get-TagSortKey $_ }

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

function ConvertBranchToMilestone([string]$BranchName) {
    <# Converts a release branch name to a milestone name:
       release/10.0.1xx        → ".NET 10.0 GA"
       release/10.0.1xx-sr5    → ".NET 10 SR5"
       release/11.0.1xx-preview3 → ".NET 11.0-preview3"
       release/11.0.1xx-rc1    → ".NET 11.0-rc1" #>
    if ($BranchName -match '^release/(\d+)\.0\.\d+xx$') {
        return ".NET $([int]$Matches[1]).0 GA"
    }
    if ($BranchName -match '^release/(\d+)\.0\.\d+xx-sr(\d+)$') {
        return ".NET $([int]$Matches[1]) SR$([int]$Matches[2])"
    }
    if ($BranchName -match '^release/(\d+)\.0\.\d+xx-(preview|rc)(\d+)$') {
        return ".NET $([int]$Matches[1]).0-$($Matches[2])$([int]$Matches[3])"
    }
    return $null
}

function Find-ReleaseBranchForCommit([string]$CommitSha, [string]$Repo, [int]$Major, [int]$PrNum = 0) {
    <# Finds the earliest release branch containing a PR.
       First checks git ancestry (commit SHA). If that fails (rebase/cherry-pick
       changed the SHA), falls back to searching commit messages for the PR number.
       Checks in chronological order: previews → RCs → GA → SRs.
       Returns @{ Branch; Milestone } or $null. #>

    # Fetch all release branches for this major version
    $remoteBranches = git -C $Repo --no-pager ls-remote --heads origin "refs/heads/release/$Major.0.*" 2>&1
    if ($LASTEXITCODE -ne 0 -or -not $remoteBranches) { return $null }

    $branches = ($remoteBranches -split "`n") | ForEach-Object {
        if ($_ -match 'refs/heads/(.+)$') { $Matches[1] }
    } | Where-Object { $_ }

    # Sort by chronological release order within a major version:
    # preview1, preview2, ..., rc1, rc2, GA, sr1, sr2, ...
    # A commit in both preview7 and GA should be milestoned preview7
    # (that's where it first shipped).
    $sorted = $branches | Sort-Object {
        if ($_ -match '-preview(\d+)$') { return 100  + [int]$Matches[1] }
        if ($_ -match '-rc(\d+)$')      { return 200  + [int]$Matches[1] }
        if ($_ -match '-sr(\d+)$')      { return 1000 + [int]$Matches[1] }
        return 500  # GA (no suffix) — after previews/RCs, before SRs
    }

    # Check each branch: first by ancestry, then by PR number in commit messages.
    # The commit message approach handles rebases and cherry-picks where the SHA changes
    # but the PR number "(#NNNNN)" is preserved in the squash-merge message.
    foreach ($branch in $sorted) {
        $null = git -C $Repo fetch origin $branch --quiet 2>&1

        # Try ancestry first (fastest, exact match)
        $null = git -C $Repo merge-base --is-ancestor $CommitSha "origin/$branch" 2>&1
        if ($LASTEXITCODE -eq 0) {
            $milestone = ConvertBranchToMilestone $branch
            if ($milestone) {
                return @{ Branch = $branch; Milestone = $milestone }
            }
        }

        # Fall back to commit message search (handles rebase/cherry-pick)
        if ($PrNum -gt 0) {
            $grepResult = git -C $Repo --no-pager log "origin/$branch" --oneline --grep="(#$PrNum)" -1 2>&1
            if ($LASTEXITCODE -eq 0 -and $grepResult) {
                $milestone = ConvertBranchToMilestone $branch
                if ($milestone) {
                    Write-Verbose "  PR #$PrNum found via commit message on $branch (rebased/cherry-picked)"
                    return @{ Branch = $branch; Milestone = $milestone }
                }
            }
        }
    }
    return $null
}

#endregion

function Invoke-GhApi([string]$Endpoint) {
    $result = gh api $Endpoint 2>&1
    if ($LASTEXITCODE -ne 0) { throw "gh api $Endpoint failed: $result" }
    return ($result | ConvertFrom-Json)
}

function Get-AllMilestones {
    $milestones = @{}; $page = 1
    while ($true) {
        $data = Invoke-GhApi "repos/dotnet/maui/milestones?state=all&per_page=100&page=$page"
        foreach ($ms in $data) { $milestones[$ms.title] = $ms.number }
        if ($data.Count -lt 100) { break }
        $page++
    }
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
    foreach ($m in [regex]::Matches($text, '(?:fix(?:es|ed)?|close[sd]?|resolve[sd]?)\s+#(\d+)', 'IgnoreCase')) {
        [void]$issues.Add([int]$m.Groups[1].Value)
    }
    # Match URLs only when preceded by a fixing keyword (mirrors GitHub auto-close behavior).
    # Bare URLs like "See https://github.com/.../issues/123" are informational, not fixing references.
    foreach ($m in [regex]::Matches($text, '(?:fix(?:es|ed)?|close[sd]?|resolve[sd]?)\s+https?://github\.com/dotnet/maui/issues/(\d+)', 'IgnoreCase')) {
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
        # Dedup: don't count the same item as correct twice (e.g. issue linked from multiple PRs)
        $key = "$ItemType`:$ItemNumber"
        if (-not $Report.ContainsKey('_checkedItems')) { $Report._checkedItems = [System.Collections.Generic.HashSet[string]]::new() }
        if ($Report._checkedItems.Add($key)) {
            $Report.AlreadyCorrect++
        }
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

function Invoke-AnalyzeSinglePr([int]$PrNum, [string]$ReleaseTag, [string]$Repo) {
    Write-Host "`n$('═' * 70)"
    Write-Host "  Single-PR mode: #$PrNum"
    Write-Host "$('═' * 70)`n"

    $expectedMs = $null
    $preLabel = $null
    $preIter = 0
    $versionInfo = $null

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
        # For preview/RC tags, read Versions.props at the tag to get pre-release info
        $versionInfo = Get-VersionFromGitRef $ReleaseTag $Repo
        if ($versionInfo) {
            $preLabel = $versionInfo.PreLabel
            $preIter = if ($versionInfo.PreIter) { $versionInfo.PreIter } else { 0 }
        }
    } elseif ($pr.MergeCommitSha) {
        # Step 1: Check release branches first — find the earliest release containing this commit.
        # This is the most accurate signal for which release the PR actually ships in.
        $versionInfo = Get-VersionFromGitRef $pr.MergeCommitSha $Repo
        $detectedMajor = if ($versionInfo -and $versionInfo.Tag -match '^(\d+)\.') { [int]$Matches[1] } else { Get-CurrentMajorVersion $Repo }

        $releaseBranch = Find-ReleaseBranchForCommit $pr.MergeCommitSha $Repo $detectedMajor $PrNum
        if ($releaseBranch) {
            $expectedMs = $releaseBranch.Milestone
            if ($versionInfo) { $ReleaseTag = $versionInfo.Tag }
            Write-Host "  Found in release branch: $($releaseBranch.Branch) → $expectedMs"
        } else {
            # Step 2: Fall back to Versions.props on the development branch HEAD.
            # All branches ultimately feed into the main development branch for their
            # .NET version (main for .NET 10, net11.0 for .NET 11). Read from there
            # to get the current target version, regardless of which staging branch
            # the PR was merged to (inflight/current, inflight/candidate, darc/*, etc.).
            $mainBranch = Get-MainBranchForVersion $detectedMajor $Repo
            $devBranch = "origin/$mainBranch"
            $branchVersionInfo = Get-VersionFromGitRef $devBranch $Repo
            if ($branchVersionInfo) {
                $versionInfo = $branchVersionInfo
                $ReleaseTag = $branchVersionInfo.Tag
                $preLabel = $branchVersionInfo.PreLabel
                $preIter = if ($branchVersionInfo.PreIter) { $branchVersionInfo.PreIter } else { 0 }
                $preDisplay = if ($branchVersionInfo.PreLabel) { " ($($branchVersionInfo.PreLabel)$($branchVersionInfo.PreIter))" } else { "" }
                Write-Host "  Version from Versions.props on $devBranch`: $ReleaseTag$preDisplay"
            } elseif ($versionInfo) {
                # Branch read failed; fall back to merge commit's version info
                Write-Verbose "  Could not read from $devBranch — using merge commit Versions.props"
                $ReleaseTag = $versionInfo.Tag
                $preLabel = $versionInfo.PreLabel
                $preIter = if ($versionInfo.PreIter) { $versionInfo.PreIter } else { 0 }
            }
        }
    }

    # Determine expected milestone if not already set by release branch detection
    if (-not $expectedMs) {
        # Fallback: try to find in existing tag ranges
        if (-not $ReleaseTag) {
            Write-Host "Auto-detecting release tag for PR #$PrNum..."
            $fallbackMajor = Get-CurrentMajorVersion $Repo
            $found = Find-TagContainingPr $PrNum $Repo $fallbackMajor
            if (-not $found) { throw "PR #$PrNum not found in any release tag range for .NET $fallbackMajor" }
            $ReleaseTag = $found.Tag
            $prevDisplay = if ($found.PreviousTag) { $found.PreviousTag } else { "(root)" }
            Write-Host "  Found in: $prevDisplay..$ReleaseTag"
        }

        # Use the clean version tag (e.g. "11.0.0") for milestone mapping, not the full tag
        $cleanTag = if ($versionInfo) { $versionInfo.Tag } else { $ReleaseTag }
        $expectedMs = ConvertTo-Milestone $cleanTag $preLabel $preIter

        # If ConvertTo-Milestone failed (e.g. $cleanTag is a preview tag string),
        # read Versions.props at the tag to get clean version + pre-release info
        if (-not $expectedMs -and $ReleaseTag) {
            $tagVersionInfo = Get-VersionFromGitRef $ReleaseTag $Repo
            if ($tagVersionInfo) {
                $preLabel = $tagVersionInfo.PreLabel
                $preIter = if ($tagVersionInfo.PreIter) { $tagVersionInfo.PreIter } else { 0 }
                $expectedMs = ConvertTo-Milestone $tagVersionInfo.Tag $preLabel $preIter
            }
        }
    }
    if (-not $expectedMs) { throw "Cannot determine milestone for PR #$PrNum" }

    # Derive MajorVersion from the milestone name, tag, or Versions.props
    $Major = if ($expectedMs -match '\.NET (\d+)') {
        [int]$Matches[1]
    } elseif ($ReleaseTag -and $ReleaseTag -match '^(\d+)\.') {
        [int]$Matches[1]
    } else {
        Get-CurrentMajorVersion $Repo
    }

    # Detect which branch owns this version's tags
    $Branch = Get-MainBranchForVersion $Major $Repo
    Write-Host "  Main branch for .NET $Major`: $Branch"
    Write-Host "  Expected milestone: $expectedMs"
    Write-Host "Fetching GitHub milestones..."
    $allMilestones = Get-AllMilestones
    $match = Find-MatchingMilestone $expectedMs $allMilestones
    if (-not $match) {
        # Milestone may not have been created yet (common during normal development).
        # Warn and return empty report — prevents red CI on every auto-triggered merge.
        Write-Warning "No GitHub milestone found matching `"$expectedMs`". The milestone may not have been created yet. Skipping."
        return @{
            Tag               = $ReleaseTag
            ExpectedMilestone = $expectedMs
            TotalPrs          = 1
            PrsChecked        = 0
            IssuesChecked     = 0
            AlreadyCorrect    = 0
            Corrections       = [System.Collections.ArrayList]::new()
            Errors            = [System.Collections.ArrayList]::new()
        }
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

function Invoke-AnalyzeRelease([string]$ReleaseTag, [string]$PrevTag, [string]$Repo) {
    # Try direct tag-to-milestone mapping first (works for stable tags like 10.0.50)
    $expectedMs = ConvertTo-Milestone $ReleaseTag

    # If that fails, read Versions.props at the tag (works for preview/RC tags like 11.0.0-preview.3.26203.7)
    if (-not $expectedMs) {
        $versionInfo = Get-VersionFromGitRef $ReleaseTag $Repo
        if ($versionInfo) {
            $preLabel = $versionInfo.PreLabel
            $preIter = if ($versionInfo.PreIter) { $versionInfo.PreIter } else { 0 }
            $expectedMs = ConvertTo-Milestone $versionInfo.Tag $preLabel $preIter
        }
    }
    if (-not $expectedMs) { throw "Cannot determine milestone for tag $ReleaseTag" }

    # Derive MajorVersion from the tag
    $Major = if ($ReleaseTag -match '^(\d+)\.') { [int]$Matches[1] } else { Get-CurrentMajorVersion $Repo }

    $allTags = Get-AllTags $Repo
    if ($ReleaseTag -notin $allTags) { throw "Tag $ReleaseTag not found in repo" }

    if (-not $PrevTag) {
        $PrevTag = Find-PreviousTag $ReleaseTag $allTags
        # No previous tag means this is the first release (e.g. GA).
        # We'll use Get-PrNumbersReachableFromTag instead of a range.
    }

    # Detect which branch owns this version's tags
    $Branch = Get-MainBranchForVersion $Major $Repo

    $prevDisplay = if ($PrevTag) { $PrevTag } else { "(root)" }

    Write-Host "`n$('═' * 70)"
    Write-Host "  Release: $ReleaseTag"
    Write-Host "  Previous: $prevDisplay"
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

    if ($PrevTag) {
        Write-Host "Finding PRs between $PrevTag..$ReleaseTag..."
        $prNumbers = Get-PrNumbersBetweenTags $PrevTag $ReleaseTag $Repo
    } else {
        Write-Host "Finding all PRs reachable from $ReleaseTag (first tag)..."
        $prNumbers = Get-PrNumbersReachableFromTag $ReleaseTag $Repo
    }
    Write-Host "  Found $($prNumbers.Count) PRs`n"

    $report = @{
        Tag               = $ReleaseTag
        PreviousTag       = $PrevTag
        ExpectedMilestone = $expectedMs
        ResolvedMilestone = $match.Title
        ResolvedMsNumber  = $match.Number
        TotalPrs          = $prNumbers.Count
        PrsChecked        = 0
        PrsSkippedWrongBranch = 0
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
            $report.PrsSkippedWrongBranch++
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
        if ($Report.Errors.Count -gt 0 -and $Report.PrsChecked -eq 0) {
            Write-Host "  ❌ No PRs were successfully checked — all $($Report.Errors.Count) failed.`n"
        } else {
            Write-Host "  ✅ All milestones are correct!`n"
        }
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
        previous_tag       = if ($Report.ContainsKey('PreviousTag')) { $Report.PreviousTag } else { $null }
        expected_milestone = $Report.ExpectedMilestone
        resolved_milestone = $Report.ResolvedMilestone
        summary            = @{
            total_prs_in_range = $Report.TotalPrs
            prs_checked        = $Report.PrsChecked
            prs_skipped_wrong_branch = if ($Report.ContainsKey('PrsSkippedWrongBranch')) { $Report.PrsSkippedWrongBranch } else { 0 }
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
    $failCount = 0
    foreach ($c in $Report.Corrections) {
        $current = if ($c.Current) { $c.Current } else { "(none)" }
        if ($DoApply) {
            try {
                Set-ItemMilestone $c.Number $c.ResolvedNo
                Write-Host "  ✅ Updated $($c.ItemType) #$($c.Number): $current → $($c.Resolved)"
            } catch {
                $failCount++
                Write-Host "  ❌ Failed $($c.ItemType) #$($c.Number): $_"
            }
        } else {
            Write-Host "  [DRY-RUN] Would set $($c.ItemType) #$($c.Number) milestone: $current → $($c.Resolved)"
        }
    }
    if ($failCount -gt 0) {
        throw "$failCount milestone update(s) failed. Check output above for details."
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
    $report = Invoke-AnalyzeSinglePr $PrNumber $Tag $RepoPath
    Write-Report $report
    if ($Output) { Save-ReportJson $report $Output }
    Invoke-ApplyCorrections $report $Apply.IsPresent
    if ($CreateIssue -and $report.Corrections.Count -gt 0) { New-GitHubIssue $report $Apply.IsPresent }
}
elseif ($Tag) {
    $report = Invoke-AnalyzeRelease $Tag $PreviousTag $RepoPath
    Write-Report $report
    $outPath = if ($Output) { $Output } else { "milestone-drift-$($Tag -replace '\.','_').json" }
    Save-ReportJson $report $outPath
    Invoke-ApplyCorrections $report $Apply.IsPresent
    if ($CreateIssue -and $report.Corrections.Count -gt 0) { New-GitHubIssue $report $Apply.IsPresent }
    if ($report.Errors.Count -gt 0 -and $report.PrsChecked -eq 0) {
        throw "Release analysis failed: $($report.Errors.Count) errors, 0 PRs checked."
    }
}
else {
    Write-Host "Error: -PrNumber or -Tag is required." -ForegroundColor Red
    Get-Help $MyInvocation.MyCommand.Path -Detailed
    exit 1
}

#endregion
