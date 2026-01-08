#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Queries open issues from dotnet/maui that need triage.

.DESCRIPTION
    This script queries GitHub for open issues that:
    - Have no milestone assigned
    - Don't have blocking labels (needs-info, needs-repro, try-latest-version, etc.)
    - Aren't Blazor issues (separate triage process)
    
    The results can be filtered by platform, area, age, and other criteria.

.PARAMETER Platform
    Filter by platform label: "android", "ios", "windows", "maccatalyst", or "all" (default)

.PARAMETER Area
    Filter by area label pattern (e.g., "collectionview", "shell", "navigation")

.PARAMETER Limit
    Maximum number of issues to return (default: 50)

.PARAMETER SortBy
    Sort by: "created", "updated", "comments", "reactions" (default: "created")

.PARAMETER SortOrder
    Sort order: "asc" or "desc" (default: "desc")

.PARAMETER MinAge
    Minimum age in days (e.g., 7 for issues older than a week)

.PARAMETER MaxAge
    Maximum age in days (e.g., 30 for issues newer than a month)

.PARAMETER OutputFormat
    Output format: "table", "json", or "markdown" (default: "table")

.PARAMETER OutputFile
    Optional file path to save results

.EXAMPLE
    ./query-issues.ps1
    # Returns up to 50 issues needing triage, sorted by creation date

.EXAMPLE
    ./query-issues.ps1 -Platform android -Limit 20
    # Returns up to 20 Android issues needing triage

.EXAMPLE
    ./query-issues.ps1 -Area "collectionview" -SortBy reactions -OutputFormat markdown
    # Returns CollectionView issues sorted by reactions, formatted as markdown

.EXAMPLE
    ./query-issues.ps1 -MinAge 30 -OutputFile "old-issues.md" -OutputFormat markdown
    # Saves issues older than 30 days to a markdown file
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("android", "ios", "windows", "maccatalyst", "all")]
    [string]$Platform = "all",

    [Parameter(Mandatory = $false)]
    [string]$Area = "",

    [Parameter(Mandatory = $false)]
    [int]$Limit = 50,

    [Parameter(Mandatory = $false)]
    [ValidateSet("created", "updated", "comments", "reactions")]
    [string]$SortBy = "created",

    [Parameter(Mandatory = $false)]
    [ValidateSet("asc", "desc")]
    [string]$SortOrder = "desc",

    [Parameter(Mandatory = $false)]
    [int]$MinAge = 0,

    [Parameter(Mandatory = $false)]
    [int]$MaxAge = 0,

    [Parameter(Mandatory = $false)]
    [ValidateSet("table", "json", "markdown", "triage")]
    [string]$OutputFormat = "table",

    [Parameter(Mandatory = $false)]
    [string]$OutputFile = "",

    [Parameter(Mandatory = $false)]
    [switch]$SkipDetails,

    [Parameter(Mandatory = $false)]
    [int]$Skip = 0,

    [Parameter(Mandatory = $false)]
    [switch]$RequireAreaLabel
)

$ErrorActionPreference = "Stop"

Write-Host "Querying GitHub issues..." -ForegroundColor Cyan

# Labels to exclude from triage results
$excludeLabels = @(
    "s/needs-info",
    "s/needs-repro", 
    "area-blazor",
    "s/try-latest-version",
    "s/move-to-vs-feedback"
)

# Platform label mapping
$platformLabels = @{
    "android" = "platform/android 🤖"
    "ios" = "platform/iOS 🍎"
    "windows" = "platform/windows 🪟"
    "maccatalyst" = "platform/macOS 🍏"
}

# Query current milestones dynamically to avoid hardcoding
Write-Host "Fetching current milestones..." -ForegroundColor DarkGray
$currentMilestones = @{
    CurrentSR = ""           # The soonest SR milestone (for regressions)
    NextSR = ""              # The next SR milestone (for other important bugs)
    Servicing = ""           # The Servicing milestone (for PRs)
    Backlog = "Backlog"      # Always "Backlog"
}

try {
    $msResult = gh api repos/dotnet/maui/milestones --jq '.[] | {title, due_on}' 2>$null
    $msLines = $msResult -split "`n" | Where-Object { $_.Trim() -ne "" }
    
    $srMilestones = @()
    $servicingMilestone = ""
    
    foreach ($line in $msLines) {
        try {
            $ms = $line | ConvertFrom-Json -ErrorAction Stop
            # Match .NET SR milestones (e.g., ".NET 10.0 SR3", ".NET 9.0 SR5")
            if ($ms.title -match "\.NET.*SR\d+") {
                $srMilestones += [PSCustomObject]@{
                    Title = $ms.title
                    DueOn = $ms.due_on
                }
            }
            # Match Servicing milestones (e.g., ".NET 10 Servicing")
            elseif ($ms.title -match "\.NET.*Servicing" -and $ms.title -notmatch "SR") {
                $servicingMilestone = $ms.title
            }
        }
        catch {
            # Skip lines that aren't valid JSON
            continue
        }
    }
    
    # Sort SR milestones by due date (soonest first) or by SR number
    if ($srMilestones.Count -gt 0) {
        $sortedSR = $srMilestones | Sort-Object { 
            $parsedDate = [DateTime]::MinValue
            if ($_.DueOn -and [DateTime]::TryParse($_.DueOn, [ref]$parsedDate)) {
                $parsedDate
            } else {
                [DateTime]::MaxValue
            }
        }
        $currentMilestones.CurrentSR = $sortedSR[0].Title
        if ($sortedSR.Count -gt 1) {
            $currentMilestones.NextSR = $sortedSR[1].Title
        } else {
            $currentMilestones.NextSR = $sortedSR[0].Title
        }
    }
    
    if ($servicingMilestone) {
        $currentMilestones.Servicing = $servicingMilestone
    }
    
    Write-Host "  Current SR: $($currentMilestones.CurrentSR)" -ForegroundColor DarkGray
    Write-Host "  Next SR: $($currentMilestones.NextSR)" -ForegroundColor DarkGray
    Write-Host "  Servicing: $($currentMilestones.Servicing)" -ForegroundColor DarkGray
}
catch {
    Write-Host "  Warning: Could not fetch milestones, using defaults" -ForegroundColor Yellow
}

# Build gh issue list command arguments
$ghArgs = @(
    "issue", "list",
    "--repo", "dotnet/maui",
    "--state", "open",
    "--limit", $Limit,
    "--json", "number,title,author,createdAt,updatedAt,labels,comments,url,milestone"
)

# Add platform filter
if ($Platform -ne "all" -and $platformLabels.ContainsKey($Platform)) {
    $ghArgs += @("--label", $platformLabels[$Platform])
}

# Add area filter
if ($Area -ne "") {
    $ghArgs += @("--label", "area-$Area")
}

Write-Host "Running: gh $($ghArgs -join ' ')" -ForegroundColor DarkGray

try {
    $result = & gh @ghArgs 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "GitHub CLI error: $result"
        exit 1
    }
    
    $allIssues = $result | ConvertFrom-Json
    
    # Post-filter: exclude issues with blocking labels, require no milestone
    $issues = $allIssues | Where-Object {
        # Skip issues that have a milestone
        if ($null -ne $_.milestone -and $_.milestone -ne "") {
            return $false
        }
        
        $issueLabels = $_.labels | ForEach-Object { $_.name }
        $hasBlockingLabel = $false
        foreach ($excludeLabel in $excludeLabels) {
            if ($issueLabels -contains $excludeLabel) {
                $hasBlockingLabel = $true
                break
            }
        }
        -not $hasBlockingLabel
    }
    
    # Apply date filters
    if ($MinAge -gt 0) {
        $minDate = (Get-Date).AddDays(-$MinAge)
        $issues = $issues | Where-Object { [DateTime]::Parse($_.createdAt) -lt $minDate }
    }
    
    if ($MaxAge -gt 0) {
        $maxDate = (Get-Date).AddDays(-$MaxAge)
        $issues = $issues | Where-Object { [DateTime]::Parse($_.createdAt) -gt $maxDate }
    }
    
    # Filter to only issues with area labels if requested
    if ($RequireAreaLabel) {
        $issues = $issues | Where-Object {
            $hasAreaLabel = $false
            foreach ($label in $_.labels) {
                if ($label.name -like "area-*") {
                    $hasAreaLabel = $true
                    break
                }
            }
            $hasAreaLabel
        }
    }
    
    # Apply skip for pagination
    if ($Skip -gt 0) {
        $issues = $issues | Select-Object -Skip $Skip
    }
    
}
catch {
    Write-Error "Failed to query GitHub: $_"
    exit 1
}

if ($issues.Count -eq 0) {
    Write-Host "No issues found matching the criteria." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($issues.Count) issues" -ForegroundColor Green

# Process and format the results with additional details
if (-not $SkipDetails) {
    Write-Host "Fetching additional details for each issue..." -ForegroundColor Cyan
}
$processedIssues = @()
$issueIndex = 0
$totalIssues = @($issues).Count

foreach ($issue in $issues) {
    $issueIndex++
    if (-not $SkipDetails) {
        Write-Host "`r  Processing issue $issueIndex of $totalIssues (#$($issue.number))..." -NoNewline -ForegroundColor DarkGray
    }
    
    $labelNames = ($issue.labels | ForEach-Object { $_.name }) -join ", "
    $platformLabel = ($issue.labels | Where-Object { $_.name -like "platform/*" } | Select-Object -First 1)?.name ?? "unknown"
    $areaLabels = ($issue.labels | Where-Object { $_.name -like "area-*" } | ForEach-Object { $_.name -replace "^area-", "" }) -join ", "
    
    $createdDate = [DateTime]::Parse($issue.createdAt)
    $ageInDays = [Math]::Round(((Get-Date) - $createdDate).TotalDays)
    
    # gh issue list returns comments as an array, count them
    $commentCount = if ($issue.comments) { $issue.comments.Count } else { 0 }
    
    # Check if author is likely Syncfusion (has partner/syncfusion label)
    $isSyncfusion = $labelNames -match "partner/syncfusion"
    
    # Check for regression labels
    $isRegression = $labelNames -match "i/regression|regressed-in"
    $regressedIn = ($issue.labels | Where-Object { $_.name -like "regressed-in-*" } | Select-Object -First 1)?.name ?? ""
    
    # Fetch linked PRs via GraphQL (skip if -SkipDetails)
    $linkedPRs = @()
    if (-not $SkipDetails) {
        try {
            $graphqlQuery = @"
{
  repository(owner: "dotnet", name: "maui") {
    issue(number: $($issue.number)) {
      timelineItems(itemTypes: [CONNECTED_EVENT, CROSS_REFERENCED_EVENT], first: 10) {
        nodes {
          ... on ConnectedEvent { subject { ... on PullRequest { number title state url milestone { title } } } }
          ... on CrossReferencedEvent { source { ... on PullRequest { number title state url milestone { title } } } }
        }
      }
    }
  }
}
"@
            $prResult = gh api graphql -f query=$graphqlQuery 2>$null | ConvertFrom-Json
            $nodes = $prResult.data.repository.issue.timelineItems.nodes
            foreach ($node in $nodes) {
                $pr = $null
                if ($node.subject) { $pr = $node.subject }
                elseif ($node.source) { $pr = $node.source }
                if ($pr -and $pr.number) {
                    $linkedPRs += [PSCustomObject]@{
                        Number = $pr.number
                        Title = $pr.title
                        State = $pr.state
                        URL = $pr.url
                        Milestone = $pr.milestone?.title ?? ""
                    }
                }
            }
        }
        catch {
            # Silently continue if GraphQL fails
        }
    }
    
    # Fetch project items (skip if -SkipDetails)
    $projects = @()
    if (-not $SkipDetails) {
        try {
            $projectResult = gh issue view $issue.number --repo dotnet/maui --json projectItems 2>$null | ConvertFrom-Json
            if ($projectResult.projectItems) {
                foreach ($proj in $projectResult.projectItems) {
                    $projects += $proj.title
                }
            }
        }
        catch {
            # Silently continue if project fetch fails
        }
    }
    
    # Fetch and summarize comments (skip if -SkipDetails)
    $commentSummary = @()
    if (-not $SkipDetails) {
        try {
            $commentsResult = gh issue view $issue.number --repo dotnet/maui --json comments 2>$null | ConvertFrom-Json
            if ($commentsResult.comments -and $commentsResult.comments.Count -gt 0) {
                foreach ($comment in $commentsResult.comments) {
                    # Skip bot comments
                    if ($comment.author.login -eq "MihuBot") { continue }
                    $bodyPreview = $comment.body -replace '\r?\n', ' ' -replace '\s+', ' '
                    if ($bodyPreview.Length -gt 300) { $bodyPreview = $bodyPreview.Substring(0, 297) + "..." }
                    $commentSummary += [PSCustomObject]@{
                        Author = $comment.author.login
                        Association = $comment.authorAssociation
                        CreatedAt = $comment.createdAt
                        BodyPreview = $bodyPreview
                    }
                }
            }
        }
        catch {
            # Silently continue if comment fetch fails
        }
    }
    
    # Generate milestone suggestion based on issue characteristics
    $suggestedMilestone = $currentMilestones.Backlog
    $suggestionReason = "No PR, not a regression"
    
    # Check if any linked PR has a milestone
    $prMilestone = ""
    foreach ($pr in $linkedPRs) {
        if ($pr.Milestone -and $pr.Milestone -ne "") {
            $prMilestone = $pr.Milestone
            break
        }
    }
    
    if ($prMilestone -ne "") {
        $suggestedMilestone = $prMilestone
        $suggestionReason = "PR already has milestone"
    }
    elseif ($isRegression) {
        # Use the current (soonest) SR milestone for regressions
        if ($currentMilestones.CurrentSR) {
            $suggestedMilestone = $currentMilestones.CurrentSR
            $suggestionReason = "Regression - current SR milestone"
        } else {
            $suggestedMilestone = $currentMilestones.Backlog
            $suggestionReason = "Regression (no SR milestone found)"
        }
    }
    elseif ($linkedPRs.Count -gt 0) {
        $openPRs = $linkedPRs | Where-Object { $_.State -eq "OPEN" }
        if ($openPRs.Count -gt 0) {
            # Use Servicing milestone for PRs, fallback to next SR
            if ($currentMilestones.Servicing) {
                $suggestedMilestone = $currentMilestones.Servicing
                $suggestionReason = "Has open PR"
            } elseif ($currentMilestones.NextSR) {
                $suggestedMilestone = $currentMilestones.NextSR
                $suggestionReason = "Has open PR"
            }
        }
    }
    
    $processedIssues += [PSCustomObject]@{
        Number = $issue.number
        Title = if ($issue.title.Length -gt 60) { $issue.title.Substring(0, 57) + "..." } else { $issue.title }
        FullTitle = $issue.title
        Author = $issue.author.login
        IsSyncfusion = $isSyncfusion
        Created = $createdDate.ToString("yyyy-MM-dd")
        Age = "$ageInDays days"
        AgeDays = $ageInDays
        CommentCount = $commentCount
        Comments = $commentSummary
        Platform = $platformLabel -replace "^platform/", "" -replace " [🤖🍎🪟🍏]$", ""
        Areas = $areaLabels
        Labels = $labelNames
        IsRegression = $isRegression
        RegressedIn = $regressedIn
        LinkedPRs = $linkedPRs
        Projects = $projects -join ", "
        URL = $issue.url
        SuggestedMilestone = $suggestedMilestone
        SuggestionReason = $suggestionReason
    }
}
Write-Host "" # New line after progress

# Output based on format
function Format-Table-Output {
    param($issues)
    
    Write-Host ""
    Write-Host "Issues Needing Triage" -ForegroundColor Cyan
    Write-Host ("=" * 100)
    
    $issues | Format-Table -Property @(
        @{Label="Issue"; Expression={$_.Number}; Width=7},
        @{Label="Title"; Expression={$_.Title}; Width=50},
        @{Label="Age"; Expression={$_.Age}; Width=10},
        @{Label="Platform"; Expression={$_.Platform}; Width=12},
        @{Label="Comments"; Expression={$_.CommentCount}; Width=8}
    ) -AutoSize
}

function Format-Markdown-Output {
    param($issues)
    
    $output = @()
    $output += "# Issues Needing Triage"
    $output += ""
    $output += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $output += "Query filters: Platform=$Platform, Area=$Area, MinAge=$MinAge, MaxAge=$MaxAge"
    $output += ""
    $output += "| Issue | Title | Age | Platform | Areas | Comments |"
    $output += "|-------|-------|-----|----------|-------|----------|"
    
    foreach ($issue in $issues) {
        $issueLink = "[#$($issue.Number)]($($issue.URL))"
        $output += "| $issueLink | $($issue.FullTitle -replace '\|', '\|') | $($issue.Age) | $($issue.Platform) | $($issue.Areas) | $($issue.CommentCount) |"
    }
    
    $output += ""
    $output += "---"
    $output += "Total: $($issues.Count) issues"
    
    return $output -join "`n"
}

function Format-Triage-Output {
    param($issues)
    
    foreach ($issue in $issues) {
        Write-Output "==="
        Write-Output "Number:$($issue.Number)"
        Write-Output "Title:$($issue.FullTitle)"
        Write-Output "URL:$($issue.URL)"
        Write-Output "Author:$($issue.Author)"
        if ($issue.IsSyncfusion) { Write-Output "IsSyncfusion:True" }
        Write-Output "Platform:$($issue.Platform)"
        Write-Output "Areas:$($issue.Areas)"
        Write-Output "Age:$($issue.Age)"
        Write-Output "Labels:$($issue.Labels)"
        if ($issue.IsRegression) { 
            Write-Output "IsRegression:True"
            if ($issue.RegressedIn) { Write-Output "RegressedIn:$($issue.RegressedIn)" }
        }
        if ($issue.Projects) { Write-Output "Projects:$($issue.Projects)" }
        
        # Linked PRs with milestones
        if ($issue.LinkedPRs -and $issue.LinkedPRs.Count -gt 0) {
            $prList = ($issue.LinkedPRs | ForEach-Object { 
                $prInfo = "#$($_.Number)[$($_.State)]"
                if ($_.Milestone) { $prInfo += "(MS:$($_.Milestone))" }
                $prInfo
            }) -join "; "
            Write-Output "LinkedPRs:$prList"
        }
        
        # Comments summary
        Write-Output "CommentCount:$($issue.CommentCount)"
        if ($issue.Comments -and $issue.Comments.Count -gt 0) {
            Write-Output "Comments:"
            foreach ($c in $issue.Comments) {
                Write-Output "  - [$($c.Author)] $($c.BodyPreview)"
            }
        }
        
        # Milestone suggestion
        Write-Output "SuggestedMilestone:$($issue.SuggestedMilestone)"
        Write-Output "SuggestionReason:$($issue.SuggestionReason)"
    }
}

function Format-Json-Output {
    param($issues)
    return $issues | ConvertTo-Json -Depth 10
}

# Generate output
switch ($OutputFormat) {
    "table" {
        Format-Table-Output -issues $processedIssues
        $outputContent = $null
    }
    "markdown" {
        $outputContent = Format-Markdown-Output -issues $processedIssues
        Write-Host $outputContent
    }
    "triage" {
        Format-Triage-Output -issues $processedIssues
        $outputContent = $null
    }
    "json" {
        $outputContent = Format-Json-Output -issues $processedIssues
        Write-Host $outputContent
    }
}

# Save to file if requested
if ($OutputFile -ne "" -and $outputContent) {
    $outputContent | Out-File -FilePath $OutputFile -Encoding UTF8
    Write-Host ""
    Write-Host "Results saved to: $OutputFile" -ForegroundColor Green
}

# Summary statistics
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
$platformStats = $processedIssues | Group-Object Platform | Sort-Object Count -Descending
foreach ($stat in $platformStats) {
    Write-Host "  $($stat.Name): $($stat.Count) issues"
}

$avgAge = [Math]::Round(($processedIssues | Measure-Object AgeDays -Average).Average)
Write-Host "  Average age: $avgAge days"

# Return the issues for pipeline usage
return $processedIssues
