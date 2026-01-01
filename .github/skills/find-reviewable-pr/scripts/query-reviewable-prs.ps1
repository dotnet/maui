#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Finds open PRs in dotnet/maui and dotnet/docs-maui that are good candidates for review.

.DESCRIPTION
    This script queries GitHub for open PRs and prioritizes them by:
    1. Milestoned PRs (SR3, SR4, Servicing)
    2. P/0 labeled PRs (critical priority)
    3. Recent PRs (5 from maui + 5 from docs-maui by default)
    4. Partner PRs (Syncfusion, etc.)
    5. Community PRs (external contributions)
    6. docs-maui PRs (5 priority + 5 recent by default)

.PARAMETER Category
    Filter by category: "milestoned", "priority", "recent", "partner", "community", "docs-maui", "all"

.PARAMETER Platform
    Filter by platform: "android", "ios", "windows", "maccatalyst", "all"

.PARAMETER Limit
    Maximum number of PRs to return per category (default: 10)

.PARAMETER RecentLimit
    Maximum number of recent PRs to return from maui (default: 5)

.PARAMETER DocsLimit
    Maximum number of docs-maui PRs to return per sub-category (priority/recent) (default: 5)

.PARAMETER OutputFormat
    Output format: "table", "json", "review" (default: "review")

.EXAMPLE
    ./query-reviewable-prs.ps1
    # Returns prioritized PRs across all categories including 5 recent from maui and 5 from docs-maui

.EXAMPLE
    ./query-reviewable-prs.ps1 -Category milestoned -Platform android
    # Returns milestoned Android PRs only

.EXAMPLE
    ./query-reviewable-prs.ps1 -Category docs-maui
    # Returns only docs-maui PRs
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("milestoned", "priority", "recent", "partner", "community", "docs-maui", "all")]
    [string]$Category = "all",

    [Parameter(Mandatory = $false)]
    [ValidateSet("android", "ios", "windows", "maccatalyst", "all")]
    [string]$Platform = "all",

    [Parameter(Mandatory = $false)]
    [int]$Limit = 10,

    [Parameter(Mandatory = $false)]
    [int]$RecentLimit = 5,

    [Parameter(Mandatory = $false)]
    [int]$DocsLimit = 5,

    [Parameter(Mandatory = $false)]
    [ValidateSet("table", "json", "review")]
    [string]$OutputFormat = "review"
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Finding Reviewable PRs                          ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Platform label mapping
$platformLabels = @{
    "android" = "platform/android"
    "ios" = "platform/iOS"
    "windows" = "platform/windows"
    "maccatalyst" = "platform/macOS"
}

# Calculate date for "recent" filter (2 weeks ago)
$twoWeeksAgo = (Get-Date).AddDays(-14).ToString("yyyy-MM-dd")

# Fetch all open non-draft PRs from dotnet/maui
Write-Host ""
Write-Host "Fetching open PRs from dotnet/maui..." -ForegroundColor Cyan

$allPRs = @()
try {
    $prResult = gh pr list --repo dotnet/maui --state open --limit 200 --json number,title,labels,createdAt,isDraft,author,additions,deletions,changedFiles,milestone,url,reviewDecision,reviews,projectItems 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to fetch PRs: $prResult"
        exit 1
    }
    $allPRs = $prResult | ConvertFrom-Json
    
    # Filter out drafts
    $allPRs = $allPRs | Where-Object { -not $_.isDraft }
    
    Write-Host "  Found $($allPRs.Count) non-draft open PRs in dotnet/maui" -ForegroundColor Green
}
catch {
    Write-Error "Failed to query GitHub: $_"
    exit 1
}

# Fetch all open non-draft PRs from dotnet/docs-maui
Write-Host ""
Write-Host "Fetching open PRs from dotnet/docs-maui..." -ForegroundColor Cyan

$docsMauiPRs = @()
try {
    $docsResult = gh pr list --repo dotnet/docs-maui --state open --limit 100 --json number,title,labels,createdAt,isDraft,author,additions,deletions,changedFiles,milestone,url,reviewDecision,reviews,projectItems 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Failed to fetch docs-maui PRs: $docsResult"
    } else {
        $docsMauiPRs = $docsResult | ConvertFrom-Json
        
        # Filter out drafts
        $docsMauiPRs = $docsMauiPRs | Where-Object { -not $_.isDraft }
        
        Write-Host "  Found $($docsMauiPRs.Count) non-draft open PRs in dotnet/docs-maui" -ForegroundColor Green
    }
}
catch {
    Write-Warning "Failed to query docs-maui: $_"
}

# Helper function to get review status summary
function Get-ReviewStatus {
    param($pr)
    
    $decision = $pr.reviewDecision
    $reviews = $pr.reviews
    
    # Count review states
    $approved = 0
    $changesRequested = 0
    $commented = 0
    
    if ($reviews) {
        foreach ($review in $reviews) {
            switch ($review.state) {
                "APPROVED" { $approved++ }
                "CHANGES_REQUESTED" { $changesRequested++ }
                "COMMENTED" { $commented++ }
            }
        }
    }
    
    # Build status string
    $status = switch ($decision) {
        "APPROVED" { "✅ Approved" }
        "CHANGES_REQUESTED" { "⚠️ Changes Requested" }
        "REVIEW_REQUIRED" { "🔍 Review Required" }
        default { "🔍 Needs Review" }
    }
    
    # Add counts if there are reviews
    if ($approved -gt 0 -or $changesRequested -gt 0) {
        $counts = @()
        if ($approved -gt 0) { $counts += "$approved✅" }
        if ($changesRequested -gt 0) { $counts += "$changesRequested⚠️" }
        $status += " ($($counts -join ', '))"
    }
    
    return $status
}

# Helper function to get project status
function Get-ProjectStatus {
    param($pr)
    
    if (-not $pr.projectItems -or $pr.projectItems.Count -eq 0) {
        return ""
    }
    
    $projectStatuses = @()
    foreach ($item in $pr.projectItems) {
        $projectName = $item.title
        $status = ""
        
        # Look for status field in project item
        if ($item.status) {
            $status = $item.status.name
        }
        
        if ($status) {
            $projectStatuses += "$projectName`: $status"
        } else {
            $projectStatuses += $projectName
        }
    }
    
    return $projectStatuses -join "; "
}

# Helper function to categorize a PR
function Get-PRCategory {
    param($pr)
    
    
    $categories = @()
    $labelNames = $pr.labels | ForEach-Object { $_.name }
    
    # Check for milestone
    if ($pr.milestone -and $pr.milestone.title) {
        $categories += "milestoned"
    }
    
    # Check for P/0 label
    if ($labelNames -contains "p/0") {
        $categories += "priority"
    }
    
    # Check for partner label
    if ($labelNames | Where-Object { $_ -like "partner/*" }) {
        $categories += "partner"
    }
    
    # Check for community label
    if ($labelNames -contains "community ✨") {
        $categories += "community"
    }
    
    # Check if recent (within 2 weeks)
    $createdDate = [DateTime]::Parse($pr.createdAt)
    if ($createdDate -gt (Get-Date).AddDays(-14)) {
        $categories += "recent"
    }
    
    return $categories
}

# Helper function to get complexity
function Get-PRComplexity {
    param($pr)
    
    $files = $pr.changedFiles
    $additions = $pr.additions
    
    if ($files -le 5 -and $additions -le 200) {
        return "Easy"
    }
    elseif ($files -le 15 -and $additions -le 500) {
        return "Medium"
    }
    else {
        return "Complex"
    }
}

# Helper function to get platform from labels
function Get-PRPlatform {
    param($pr)
    
    $labelNames = $pr.labels | ForEach-Object { $_.name }
    $platforms = @()
    
    if ($labelNames -contains "platform/android" -or $labelNames -like "*android*") { $platforms += "Android" }
    if ($labelNames -contains "platform/iOS" -or $labelNames -like "*iOS*") { $platforms += "iOS" }
    if ($labelNames -contains "platform/windows" -or $labelNames -like "*windows*") { $platforms += "Windows" }
    if ($labelNames -contains "platform/macOS" -or $labelNames -like "*macOS*" -or $labelNames -like "*catalyst*") { $platforms += "Mac" }
    
    if ($platforms.Count -eq 0) { return "Unknown" }
    return $platforms -join ", "
}

# Helper function to check if PR matches platform filter
function Test-PRPlatform {
    param($pr, $platformFilter)
    
    if ($platformFilter -eq "all") { return $true }
    
    $labelNames = $pr.labels | ForEach-Object { $_.name }
    $targetLabel = $platformLabels[$platformFilter]
    
    # Check if any label contains the platform
    foreach ($label in $labelNames) {
        if ($label -like "*$platformFilter*" -or $label -eq $targetLabel) {
            return $true
        }
    }
    
    return $false
}

# Process and categorize all PRs
$processedPRs = @()

foreach ($pr in $allPRs) {
    # Apply platform filter
    if (-not (Test-PRPlatform -pr $pr -platformFilter $Platform)) {
        continue
    }
    
    $categories = Get-PRCategory -pr $pr
    $labelNames = ($pr.labels | ForEach-Object { $_.name }) -join ", "
    
    $processedPRs += [PSCustomObject]@{
        Number = $pr.number
        Title = $pr.title
        Author = $pr.author.login
        Platform = Get-PRPlatform -pr $pr
        Complexity = Get-PRComplexity -pr $pr
        Milestone = if ($pr.milestone) { $pr.milestone.title } else { "" }
        Categories = $categories
        Labels = $labelNames
        CreatedAt = [DateTime]::Parse($pr.createdAt)
        Age = [Math]::Round(((Get-Date) - [DateTime]::Parse($pr.createdAt)).TotalDays)
        Files = $pr.changedFiles
        Additions = $pr.additions
        Deletions = $pr.deletions
        URL = $pr.url
        ReviewDecision = $pr.reviewDecision
        ReviewStatus = Get-ReviewStatus -pr $pr
        ProjectStatus = Get-ProjectStatus -pr $pr
        IsPartner = ($labelNames -match "partner/")
        IsCommunity = ($labelNames -match "community")
        IsPriority = ($labelNames -match "p/0")
    }
}

Write-Host "  Processed $($processedPRs.Count) PRs matching filters" -ForegroundColor Green

# Process docs-maui PRs
$processedDocsMauiPRs = @()

foreach ($pr in $docsMauiPRs) {
    $categories = Get-PRCategory -pr $pr
    $labelNames = ($pr.labels | ForEach-Object { $_.name }) -join ", "
    
    $processedDocsMauiPRs += [PSCustomObject]@{
        Number = $pr.number
        Title = $pr.title
        Author = $pr.author.login
        Platform = "Docs"
        Complexity = Get-PRComplexity -pr $pr
        Milestone = if ($pr.milestone) { $pr.milestone.title } else { "" }
        Categories = $categories
        Labels = $labelNames
        CreatedAt = [DateTime]::Parse($pr.createdAt)
        Age = [Math]::Round(((Get-Date) - [DateTime]::Parse($pr.createdAt)).TotalDays)
        Files = $pr.changedFiles
        Additions = $pr.additions
        Deletions = $pr.deletions
        URL = $pr.url
        ReviewDecision = $pr.reviewDecision
        ReviewStatus = Get-ReviewStatus -pr $pr
        ProjectStatus = Get-ProjectStatus -pr $pr
        IsPartner = ($labelNames -match "partner/")
        IsCommunity = ($labelNames -match "community")
        IsPriority = ($labelNames -match "p/0" -or $labelNames -match "pri0" -or $labelNames -match "priority")
        Repo = "docs-maui"
    }
}

Write-Host "  Processed $($processedDocsMauiPRs.Count) docs-maui PRs" -ForegroundColor Green

# Helper function to get milestone sort priority (lower = higher priority)
function Get-MilestonePriority {
    param($milestone)
    
    if (-not $milestone) { return 9999 }
    
    # Extract SR number if present (e.g., "SR2.1" -> 2.1, "SR3" -> 3, "SR4" -> 4)
    if ($milestone -match "SR(\d+\.?\d*)") {
        return [double]$Matches[1]
    }
    # Servicing comes after all SR milestones
    if ($milestone -match "Servicing") {
        return 100
    }
    # Backlog and others come last
    if ($milestone -match "Backlog") {
        return 1000
    }
    # Unknown milestones
    return 500
}

# Organize PRs by category
$milestonedPRs = $processedPRs | Where-Object { $_.Milestone -ne "" } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, { if ($_.IsPriority) { 0 } else { 1 } }, CreatedAt
$priorityPRs = $processedPRs | Where-Object { $_.IsPriority } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, CreatedAt
$partnerPRs = $processedPRs | Where-Object { $_.IsPartner } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, { if ($_.IsPriority) { 0 } else { 1 } }, CreatedAt -Descending
$communityPRs = $processedPRs | Where-Object { $_.IsCommunity } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, { if ($_.IsPriority) { 0 } else { 1 } }, CreatedAt -Descending
$recentPRs = $processedPRs | Where-Object { $_.Age -le 14 } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, { if ($_.IsPriority) { 0 } else { 1 } }, CreatedAt -Descending

# Organize docs-maui PRs by priority and recency
$docsMauiPriorityPRs = $processedDocsMauiPRs | Where-Object { $_.IsPriority } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, CreatedAt
$docsMauiRecentPRs = $processedDocsMauiPRs | Sort-Object CreatedAt -Descending

# Filter by category if specified
if ($Category -ne "all") {
    switch ($Category) {
        "milestoned" { $processedPRs = $milestonedPRs }
        "priority" { $processedPRs = $priorityPRs }
        "partner" { $processedPRs = $partnerPRs }
        "community" { $processedPRs = $communityPRs }
        "recent" { $processedPRs = $recentPRs }
        "docs-maui" { $processedPRs = @() } # Will be handled separately
    }
}

# Output functions
function Format-Review-Output {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    # Priority PRs (P/0)
    if ($priorityPRs.Count -gt 0 -and ($Category -eq "all" -or $Category -eq "priority")) {
        Write-Host ""
        Write-Host "🔴 PRIORITY (P/0) PRs - $($priorityPRs.Count) found" -ForegroundColor Red
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($priorityPRs | Select-Object -First $Limit)) {
            Write-Host "==="
            Write-Host "Number:$($pr.Number)"
            Write-Host "Title:$($pr.Title)"
            Write-Host "URL:$($pr.URL)"
            Write-Host "Author:$($pr.Author)"
            Write-Host "Platform:$($pr.Platform)"
            Write-Host "Complexity:$($pr.Complexity)"
            Write-Host "Milestone:$($pr.Milestone)"
            Write-Host "ReviewStatus:$($pr.ReviewStatus)"
            if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
            Write-Host "Age:$($pr.Age) days"
            Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
            Write-Host "Labels:$($pr.Labels)"
            Write-Host "Category:priority"
        }
    }
    
    # Milestoned PRs
    if ($milestonedPRs.Count -gt 0 -and ($Category -eq "all" -or $Category -eq "milestoned")) {
        Write-Host ""
        Write-Host "📅 MILESTONED PRs - $($milestonedPRs.Count) found" -ForegroundColor Yellow
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($milestonedPRs | Select-Object -First $Limit)) {
            Write-Host "==="
            Write-Host "Number:$($pr.Number)"
            Write-Host "Title:$($pr.Title)"
            Write-Host "URL:$($pr.URL)"
            Write-Host "Author:$($pr.Author)"
            Write-Host "Platform:$($pr.Platform)"
            Write-Host "Complexity:$($pr.Complexity)"
            Write-Host "Milestone:$($pr.Milestone)"
            Write-Host "ReviewStatus:$($pr.ReviewStatus)"
            if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
            Write-Host "Age:$($pr.Age) days"
            Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
            Write-Host "Labels:$($pr.Labels)"
            Write-Host "Category:milestoned"
        }
    }
    
    # Partner PRs
    if ($partnerPRs.Count -gt 0 -and ($Category -eq "all" -or $Category -eq "partner")) {
        Write-Host ""
        Write-Host "🤝 PARTNER PRs - $($partnerPRs.Count) found" -ForegroundColor Magenta
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($partnerPRs | Select-Object -First $Limit)) {
            Write-Host "==="
            Write-Host "Number:$($pr.Number)"
            Write-Host "Title:$($pr.Title)"
            Write-Host "URL:$($pr.URL)"
            Write-Host "Author:$($pr.Author)"
            Write-Host "Platform:$($pr.Platform)"
            Write-Host "Complexity:$($pr.Complexity)"
            Write-Host "Milestone:$($pr.Milestone)"
            Write-Host "ReviewStatus:$($pr.ReviewStatus)"
            if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
            Write-Host "Age:$($pr.Age) days"
            Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
            Write-Host "Labels:$($pr.Labels)"
            Write-Host "Category:partner"
        }
    }
    
    # Community PRs
    if ($communityPRs.Count -gt 0 -and ($Category -eq "all" -or $Category -eq "community")) {
        Write-Host ""
        Write-Host "✨ COMMUNITY PRs - $($communityPRs.Count) found" -ForegroundColor Green
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($communityPRs | Select-Object -First $Limit)) {
            Write-Host "==="
            Write-Host "Number:$($pr.Number)"
            Write-Host "Title:$($pr.Title)"
            Write-Host "URL:$($pr.URL)"
            Write-Host "Author:$($pr.Author)"
            Write-Host "Platform:$($pr.Platform)"
            Write-Host "Complexity:$($pr.Complexity)"
            Write-Host "Milestone:$($pr.Milestone)"
            Write-Host "ReviewStatus:$($pr.ReviewStatus)"
            if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
            Write-Host "Age:$($pr.Age) days"
            Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
            Write-Host "Labels:$($pr.Labels)"
            Write-Host "Category:community"
        }
    }
    
    # Recent PRs (only show those not in other categories)
    $recentOnly = $recentPRs | Where-Object { 
        -not $_.IsPriority -and -not $_.IsPartner -and -not $_.IsCommunity -and $_.Milestone -eq ""
    }
    if ($recentOnly.Count -gt 0 -and ($Category -eq "all" -or $Category -eq "recent")) {
        Write-Host ""
        Write-Host "🕐 RECENT PRs (last 2 weeks) - $($recentOnly.Count) found" -ForegroundColor Cyan
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($recentOnly | Select-Object -First $RecentLimit)) {
            Write-Host "==="
            Write-Host "Number:$($pr.Number)"
            Write-Host "Title:$($pr.Title)"
            Write-Host "URL:$($pr.URL)"
            Write-Host "Author:$($pr.Author)"
            Write-Host "Platform:$($pr.Platform)"
            Write-Host "Complexity:$($pr.Complexity)"
            Write-Host "Milestone:$($pr.Milestone)"
            Write-Host "ReviewStatus:$($pr.ReviewStatus)"
            if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
            Write-Host "Age:$($pr.Age) days"
            Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
            Write-Host "Labels:$($pr.Labels)"
            Write-Host "Category:recent"
        }
    }
    
    # docs-maui PRs - Priority
    if ($docsMauiPriorityPRs.Count -gt 0 -and ($Category -eq "all" -or $Category -eq "docs-maui")) {
        Write-Host ""
        Write-Host "📚 DOCS-MAUI PRIORITY PRs - $($docsMauiPriorityPRs.Count) found" -ForegroundColor Blue
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($docsMauiPriorityPRs | Select-Object -First $DocsLimit)) {
            Write-Host "==="
            Write-Host "Number:$($pr.Number)"
            Write-Host "Title:$($pr.Title)"
            Write-Host "URL:$($pr.URL)"
            Write-Host "Author:$($pr.Author)"
            Write-Host "Repo:docs-maui"
            Write-Host "Complexity:$($pr.Complexity)"
            Write-Host "Milestone:$($pr.Milestone)"
            Write-Host "ReviewStatus:$($pr.ReviewStatus)"
            if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
            Write-Host "Age:$($pr.Age) days"
            Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
            Write-Host "Labels:$($pr.Labels)"
            Write-Host "Category:docs-maui-priority"
        }
    }
    
    # docs-maui PRs - Recent
    if ($docsMauiRecentPRs.Count -gt 0 -and ($Category -eq "all" -or $Category -eq "docs-maui")) {
        Write-Host ""
        Write-Host "📖 DOCS-MAUI RECENT PRs - $($docsMauiRecentPRs.Count) found" -ForegroundColor Blue
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($docsMauiRecentPRs | Select-Object -First $DocsLimit)) {
            Write-Host "==="
            Write-Host "Number:$($pr.Number)"
            Write-Host "Title:$($pr.Title)"
            Write-Host "URL:$($pr.URL)"
            Write-Host "Author:$($pr.Author)"
            Write-Host "Repo:docs-maui"
            Write-Host "Complexity:$($pr.Complexity)"
            Write-Host "Milestone:$($pr.Milestone)"
            Write-Host "ReviewStatus:$($pr.ReviewStatus)"
            if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
            Write-Host "Age:$($pr.Age) days"
            Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
            Write-Host "Labels:$($pr.Labels)"
            Write-Host "Category:docs-maui-recent"
        }
    }
    
    # Summary
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "SUMMARY" -ForegroundColor Cyan
    Write-Host "  Priority (P/0): $($priorityPRs.Count)"
    Write-Host "  Milestoned: $($milestonedPRs.Count)"
    Write-Host "  Partner: $($partnerPRs.Count)"
    Write-Host "  Community: $($communityPRs.Count)"
    Write-Host "  Recent (2 weeks): $($recentPRs.Count)"
    Write-Host "  docs-maui Priority: $($docsMauiPriorityPRs.Count)"
    Write-Host "  docs-maui Recent: $($docsMauiRecentPRs.Count)"
}

function Format-Json-Output {
    $output = @{
        Priority = $priorityPRs | Select-Object -First $Limit
        Milestoned = $milestonedPRs | Select-Object -First $Limit
        Partner = $partnerPRs | Select-Object -First $Limit
        Community = $communityPRs | Select-Object -First $Limit
        Recent = $recentPRs | Select-Object -First $RecentLimit
        DocsMauiPriority = $docsMauiPriorityPRs | Select-Object -First $DocsLimit
        DocsMauiRecent = $docsMauiRecentPRs | Select-Object -First $DocsLimit
    }
    return $output | ConvertTo-Json -Depth 10
}

function Format-Table-Output {
    Write-Host ""
    Write-Host "Reviewable PRs" -ForegroundColor Cyan
    Write-Host ("=" * 100)
    
    $processedPRs | Select-Object -First ($Limit * 2) | Format-Table -Property @(
        @{Label="PR"; Expression={$_.Number}; Width=6},
        @{Label="Title"; Expression={ if ($_.Title.Length -gt 40) { $_.Title.Substring(0,37) + "..." } else { $_.Title } }; Width=40},
        @{Label="Platform"; Expression={$_.Platform}; Width=10},
        @{Label="Milestone"; Expression={$_.Milestone}; Width=15},
        @{Label="Age"; Expression={"$($_.Age)d"}; Width=5},
        @{Label="Files"; Expression={$_.Files}; Width=5}
    ) -AutoSize
}

# Generate output
switch ($OutputFormat) {
    "review" { Format-Review-Output }
    "json" { Format-Json-Output }
    "table" { Format-Table-Output }
}

# Return processed PRs for pipeline usage (only when not in review mode)
if ($OutputFormat -ne "review") {
    return $processedPRs
}
