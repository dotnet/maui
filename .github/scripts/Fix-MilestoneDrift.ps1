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
    3. All SRs:     -AllSRs -MajorVersion 10

.PARAMETER PrNumber
    Analyze and fix a single PR (and its linked issues).

.PARAMETER Tag
    Release tag to analyze (e.g., "10.0.50"). For single-PR mode, auto-detected if omitted.

.PARAMETER PreviousTag
    Previous release tag. Auto-detected if omitted.

.PARAMETER AllSRs
    Analyze all SR releases for the given major version.

.PARAMETER MajorVersion
    Major .NET version (default: 10).

.PARAMETER RepoPath
    Path to a dotnet/maui git checkout with full tag history.

.PARAMETER Output
    Output JSON file path.

.PARAMETER Apply
    Actually apply milestone fixes. Without this flag, only a dry-run report is produced.

.EXAMPLE
    ./Fix-MilestoneDrift.ps1 -PrNumber 33818 -RepoPath ~/Projects/maui
    ./Fix-MilestoneDrift.ps1 -PrNumber 33818 -Apply
    ./Fix-MilestoneDrift.ps1 -Tag 10.0.50 -RepoPath ~/Projects/maui
    ./Fix-MilestoneDrift.ps1 -AllSRs -MajorVersion 10 -RepoPath ~/Projects/maui
#>

[CmdletBinding()]
param(
    [int]$PrNumber,
    [string]$Tag,
    [string]$PreviousTag,
    [switch]$AllSRs,
    [int]$MajorVersion = 10,
    [string]$RepoPath = ".",
    [string]$Output,
    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

#region ── Milestone mapping helpers ──────────────────────────────────────

function ConvertTo-Milestone([string]$ReleaseTag) {
    <# Converts "10.0.50" → ".NET 10 SR5", "10.0.0" → ".NET 10.0 GA" #>
    if ($ReleaseTag -notmatch '^(\d+)\.0\.(\d+)$') { return $null }
    $major = [int]$Matches[1]; $patch = [int]$Matches[2]
    if ($patch -eq 0)  { return ".NET $major.0 GA" }
    if ($patch -lt 10) { return ".NET $major.0 SR1" }
    return ".NET $major SR$([math]::Floor($patch / 10))"
}

function Get-PatchVersion([string]$ReleaseTag) {
    if ($ReleaseTag -match '^(\d+)\.0\.(\d+)$') { return [int]$Matches[2] }
    return 0
}

function Test-IsSrTag([string]$ReleaseTag, [int]$Major) {
    return ($ReleaseTag -match "^$Major\.0\.\d+$")
}

function Test-MilestoneMatch([string]$Actual, [string]$Expected) {
    <# Handles ".NET 10.0 SR4" vs ".NET 10 SR4" and sub-patches like ".NET 10 SR5.1" #>
    if ([string]::IsNullOrEmpty($Actual)) { return $false }
    if ($Actual -eq $Expected) { return $true }

    # Normalize: ".NET 10.0 SRx" → ".NET 10 SRx"
    $normActual   = $Actual   -replace '\.NET (\d+)\.0 SR', '.NET $1 SR'
    $normExpected = $Expected -replace '\.NET (\d+)\.0 SR', '.NET $1 SR'
    if ($normActual -eq $normExpected) { return $true }

    # Sub-patch: ".NET 10 SR5.1" matches ".NET 10 SR5"
    if ($normActual -match "^$([regex]::Escape($normExpected))\.\d+$") { return $true }

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

function Find-TagContainingPr([int]$PrNum, [string]$Repo, [int]$Major) {
    <# Searches tag ranges to find which release contains this PR #>
    $allTags = Get-AllTags $Repo
    $srTags = $allTags | Where-Object { Test-IsSrTag $_ $Major } |
              Sort-Object { Get-PatchVersion $_ }

    for ($i = 0; $i -lt $srTags.Count; $i++) {
        $prev = if ($i -eq 0) { $null } else { $srTags[$i - 1] }
        $current = $srTags[$i]
        if (-not $prev) { continue }

        $prsInRange = Get-PrNumbersBetweenTags $prev $current $Repo
        if ($PrNum -in $prsInRange) {
            return @{ Tag = $current; PreviousTag = $prev }
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
        return @{
            Number    = $PrNum
            Title     = $pr.title
            Milestone = if ($pr.milestone) { $pr.milestone.title } else { $null }
            MsNumber  = if ($pr.milestone) { $pr.milestone.number } else { $null }
            Url       = $pr.html_url
            Body      = if ($pr.body) { $pr.body } else { "" }
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
    foreach ($m in [regex]::Matches($text, 'github\.com/dotnet/maui/issues/(\d+)')) {
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

    # Auto-detect tag if not provided
    if (-not $ReleaseTag) {
        Write-Host "Auto-detecting release tag for PR #$PrNum..."
        $found = Find-TagContainingPr $PrNum $Repo $Major
        if (-not $found) { throw "PR #$PrNum not found in any release tag range for .NET $Major" }
        $ReleaseTag = $found.Tag
        Write-Host "  Found in: $($found.PreviousTag)..$ReleaseTag`n"
    }

    $expectedMs = ConvertTo-Milestone $ReleaseTag
    if (-not $expectedMs) { throw "Cannot determine milestone for tag $ReleaseTag" }

    Write-Host "Expected milestone: $expectedMs"
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
    $expectedMs = ConvertTo-Milestone $ReleaseTag
    if (-not $expectedMs) { throw "Cannot determine milestone for tag $ReleaseTag" }

    $allTags = Get-AllTags $Repo
    if ($ReleaseTag -notin $allTags) { throw "Tag $ReleaseTag not found in repo" }

    if (-not $PrevTag) {
        $PrevTag = Find-PreviousTag $ReleaseTag $allTags
        if (-not $PrevTag) { throw "Cannot determine previous tag for $ReleaseTag" }
    }

    Write-Host "`n$('═' * 70)"
    Write-Host "  Release: $ReleaseTag"
    Write-Host "  Previous: $PrevTag"
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
        $via = if ($c.RelatedPr) { " (via PR #$($c.RelatedPr))" } else { "" }
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
}
elseif ($AllSRs) {
    $allTags = Get-AllTags $RepoPath
    $srTags = $allTags | Where-Object { Test-IsSrTag $_ $MajorVersion } |
              Sort-Object { Get-PatchVersion $_ }

    Write-Host "Found $($srTags.Count) release tags for .NET ${MajorVersion}: $($srTags -join ', ')`n"
    $totalCorrections = 0

    foreach ($srTag in $srTags) {
        try {
            $report = Invoke-AnalyzeRelease $srTag $null $RepoPath
            Write-Report $report
            if ($Output) { Save-ReportJson $report $Output }
            Invoke-ApplyCorrections $report $Apply.IsPresent
            $totalCorrections += $report.Corrections.Count
        } catch {
            Write-Warning "Error analyzing ${srTag}: $_"
        }
    }
    Write-Host "`n$('═' * 70)"
    Write-Host "  TOTAL across all releases: $totalCorrections corrections needed"
    Write-Host "$('═' * 70)`n"
}
elseif ($Tag) {
    $report = Invoke-AnalyzeRelease $Tag $PreviousTag $RepoPath
    Write-Report $report
    $outPath = if ($Output) { $Output } else { "milestone-drift-$($Tag -replace '\.','_').json" }
    Save-ReportJson $report $outPath
    Invoke-ApplyCorrections $report $Apply.IsPresent
}
else {
    Write-Host "Error: -PrNumber, -Tag, or -AllSRs is required." -ForegroundColor Red
    Get-Help $MyInvocation.MyCommand.Path -Detailed
    exit 1
}

#endregion
