#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Finds open PRs in dotnet/maui and dotnet/docs-maui that are good candidates for review.

.DESCRIPTION
    This script queries GitHub for open PRs and prioritizes them by:
    1. P/0 labeled PRs (critical priority - always on top)
    2. Approved (not merged) PRs
    3. Ready To Review (from project board)
    4. Milestoned PRs (dynamically sorted by SR number - lower numbers first, e.g., SR5 before SR6)
    5. Agent Reviewed PRs (detected via labels)
    6. Partner PRs (Syncfusion, etc.)
    7. Community PRs (external contributions)
    8. Recent PRs waiting for review (last 2 weeks)
    9. docs-maui PRs (priority + waiting for review)

.PARAMETER Category
    Filter by category: "default" (P/0 + milestoned only), "milestoned", "priority", "recent", "partner", "community", "docs-maui", "approved", "ready-to-review", "agent-reviewed", "all"

.PARAMETER Platform
    Filter by platform: "android", "ios", "windows", "maccatalyst", "all"

.PARAMETER Limit
    Maximum number of PRs to return per category (default: 100)

.PARAMETER RecentLimit
    Maximum number of recent PRs to return from maui (default: 5)

.PARAMETER DocsLimit
    Maximum number of docs-maui PRs to return per sub-category (priority/recent) (default: 5)

.PARAMETER OutputFormat
    Output format: "table", "json", "review" (default: "review")

.EXAMPLE
    ./query-reviewable-prs.ps1
    # Returns only P/0 and Milestoned PRs (default behavior)

.EXAMPLE
    ./query-reviewable-prs.ps1 -Category all
    # Returns PRs across all categories

.EXAMPLE
    ./query-reviewable-prs.ps1 -Category milestoned -Platform android
    # Returns milestoned Android PRs only

.EXAMPLE
    ./query-reviewable-prs.ps1 -Category docs-maui
    # Returns only docs-maui PRs
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("default", "milestoned", "priority", "recent", "partner", "community", "docs-maui", "approved", "ready-to-review", "agent-reviewed", "all")]
    [string]$Category = "default",

    [Parameter(Mandatory = $false)]
    [ValidateSet("android", "ios", "windows", "maccatalyst", "all")]
    [string]$Platform = "all",

    [Parameter(Mandatory = $false)]
    [int]$Limit = 100,

    [Parameter(Mandatory = $false)]
    [int]$RecentLimit = 5,

    [Parameter(Mandatory = $false)]
    [int]$DocsLimit = 5,

    [Parameter(Mandatory = $false)]
    [ValidateSet("table", "json", "review")]
    [string]$OutputFormat = "review",

    [Parameter(Mandatory = $false)]
    [string[]]$ExcludeAuthors = @(),

    [Parameter(Mandatory = $false)]
    [string[]]$IncludeAuthors = @()
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

# Helper function to invoke GitHub CLI with retry logic
function Invoke-GitHubWithRetry {
    param(
        [string]$Command,
        [string]$Description,
        [int]$MaxRetries = 3
    )
    
    $retryCount = 0
    $baseDelay = 2  # Start with 2 seconds
    
    while ($retryCount -lt $MaxRetries) {
        try {
            # Reset LASTEXITCODE before running command
            $global:LASTEXITCODE = 0
            $result = Invoke-Expression $Command 2>&1
            if ($LASTEXITCODE -eq 0) {
                return $result
            }
            
            # Check if it's a transient error (502, 503, timeout, stream error)
            $errorText = $result -join " "
            if ($errorText -match "502|503|504|timeout|stream error|CANCEL|Bad Gateway") {
                $retryCount++
                if ($retryCount -lt $MaxRetries) {
                    $delay = $baseDelay * [Math]::Pow(2, $retryCount - 1)  # Exponential backoff: 2, 4, 8 seconds
                    Write-Warning "  Transient error detected. Retrying in $delay seconds... (attempt $($retryCount + 1)/$MaxRetries)"
                    Start-Sleep -Seconds $delay
                    continue
                }
            }
            
            # Non-transient error or max retries reached
            throw "Failed to $Description`: $errorText"
        }
        catch {
            $retryCount++
            if ($retryCount -ge $MaxRetries) {
                throw $_
            }
            $delay = $baseDelay * [Math]::Pow(2, $retryCount - 1)
            Write-Warning "  Error occurred. Retrying in $delay seconds... (attempt $($retryCount + 1)/$MaxRetries)"
            Start-Sleep -Seconds $delay
        }
    }
    
    throw "Failed to $Description after $MaxRetries attempts"
}

# MAUI SDK Ongoing project board constants
$MAUI_PROJECT_ID = "PVT_kwDOAIt-yc4AH1zp"
$READY_TO_REVIEW_OPTION_ID = "11d42e2a"

# Helper function to fetch PR numbers in "Ready To Review" from the project board
function Get-ReadyToReviewPRNumbers {
    param([bool]$HasProjectScope)
    
    if (-not $HasProjectScope) {
        Write-Host "  ⚠ Skipping project board query (missing read:project scope)" -ForegroundColor Yellow
        return @()
    }
    
    Write-Host "  Fetching 'Ready To Review' items from MAUI SDK Ongoing board..." -ForegroundColor Gray
    
    try {
        $readyPRs = @()
        $hasNextPage = $true
        $endCursor = $null

        while ($hasNextPage) {
            $afterClause = if ($endCursor) { ", after: `"$endCursor`"" } else { "" }
            $graphqlQuery = @"
{
  node(id: "$MAUI_PROJECT_ID") {
    ... on ProjectV2 {
      items(first: 100$afterClause) {
        pageInfo {
          hasNextPage
          endCursor
        }
        nodes {
          fieldValueByName(name: "Status") {
            ... on ProjectV2ItemFieldSingleSelectValue {
              optionId
            }
          }
          content {
            ... on PullRequest {
              number
              state
            }
          }
        }
      }
    }
  }
}
"@
            $result = Invoke-GitHubWithRetry -Command "gh api graphql -f query='$($graphqlQuery -replace "`r?`n", " " -replace "'", "'\''")'" -Description "fetch project board items"
            $parsed = $result | ConvertFrom-Json

            foreach ($item in $parsed.data.node.items.nodes) {
                if ($item.fieldValueByName -and 
                    $item.fieldValueByName.optionId -eq $READY_TO_REVIEW_OPTION_ID -and
                    $item.content -and 
                    $item.content.number -and
                    $item.content.state -eq "OPEN") {
                    $readyPRs += $item.content.number
                }
            }

            $hasNextPage = $parsed.data.node.items.pageInfo.hasNextPage
            $endCursor = $parsed.data.node.items.pageInfo.endCursor
        }
        
        Write-Host "    Ready To Review: $($readyPRs.Count) PRs" -ForegroundColor Gray
        return $readyPRs
    }
    catch {
        Write-Warning "  Could not query project board: $_"
        return @()
    }
}

# Check if we have read:project scope by testing a simple query with projectItems
$hasProjectScope = $true
Write-Host ""
Write-Host "Checking GitHub token scopes..." -ForegroundColor Cyan
try {
    $testResult = gh pr list --repo dotnet/maui --state open --limit 1 --json projectItems 2>&1
    if ($LASTEXITCODE -ne 0) {
        if ($testResult -match "read:project") {
            $hasProjectScope = $false
            Write-Host ""
            Write-Host "╔═══════════════════════════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
            Write-Host "║  NOTE: Your GitHub token is missing the 'read:project' scope.                 ║" -ForegroundColor Yellow
            Write-Host "║  Project board data will not be available for PR prioritization.              ║" -ForegroundColor Yellow
            Write-Host "║                                                                               ║" -ForegroundColor Yellow
            Write-Host "║  To enable project data, run:  gh auth refresh -s read:project                ║" -ForegroundColor Yellow
            Write-Host "╚═══════════════════════════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
            Write-Host ""
        }
    }
}
catch {
    $hasProjectScope = $false
}

# Build the JSON fields list based on available scopes
$baseFields = "number,title,labels,createdAt,updatedAt,isDraft,author,assignees,additions,deletions,changedFiles,milestone,url,reviewDecision,reviews"
$jsonFields = if ($hasProjectScope) { "$baseFields,projectItems" } else { $baseFields }

# Fetch "Ready To Review" PR numbers from the project board
$readyToReviewPRNumbers = Get-ReadyToReviewPRNumbers -HasProjectScope $hasProjectScope

# Fetch PRs from dotnet/maui using multiple targeted queries to ensure comprehensive coverage
Write-Host ""
Write-Host "Fetching open PRs from dotnet/maui..." -ForegroundColor Cyan

$allPRs = @()
$seenPRNumbers = @{}

# Helper function to add PRs while avoiding duplicates
function Add-UniquePRs {
    param($prs, [ref]$allPRs, [ref]$seenNumbers)
    
    $added = 0
    foreach ($pr in $prs) {
        if (-not $seenNumbers.Value.ContainsKey($pr.number)) {
            $seenNumbers.Value[$pr.number] = $true
            $allPRs.Value += $pr
            $added++
        }
    }
    return $added
}

try {
    # Query 1: Milestoned PRs - dynamically fetch active SR and Preview milestones
    Write-Host "  Fetching active milestones..." -ForegroundColor Gray
    $milestonesResult = Invoke-GitHubWithRetry -Command "gh api repos/dotnet/maui/milestones --jq '.[].title'" -Description "fetch milestones"
    $allMilestones = $milestonesResult -split "`n" | Where-Object { $_ -ne "" }
    
    # Filter for SR and Preview milestones only (ignore Servicing, Backlog, Planning)
    $targetMilestones = $allMilestones | Where-Object { 
        $_ -match "SR\d" -or $_ -match "preview"
    }
    
    if ($targetMilestones.Count -gt 0) {
        Write-Host "  Fetching milestoned PRs (SR/Preview)..." -ForegroundColor Gray
        foreach ($milestone in $targetMilestones) {
            $searchQuery = "milestone:`"$milestone`""
            $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search '$searchQuery' --limit 25 --json $jsonFields" -Description "fetch $milestone PRs"
            $prs = $prResult | ConvertFrom-Json
            $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
            if ($added -gt 0) { Write-Host "    $milestone`: $added PRs" -ForegroundColor Gray }
        }
    } else {
        Write-Host "    No active SR/Preview milestones found" -ForegroundColor Gray
    }
    
    # Query 2: P/0 priority PRs
    Write-Host "  Fetching P/0 priority PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'label:p/0' --limit 25 --json $jsonFields" -Description "fetch P/0 PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    P/0: $added PRs" -ForegroundColor Gray
    
    # Query 3: Partner PRs
    Write-Host "  Fetching partner PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'label:partner/syncfusion' --limit 25 --json $jsonFields" -Description "fetch partner PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    Partner: $added PRs" -ForegroundColor Gray
    
    # Query 4: Community PRs
    Write-Host "  Fetching community PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'label:`"community ✨`"' --limit 25 --json $jsonFields" -Description "fetch community PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    Community: $added PRs" -ForegroundColor Gray
    
    # Query 5: Recently updated PRs (sorted by updated date)
    Write-Host "  Fetching recently updated PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'sort:updated-desc' --limit 25 --json $jsonFields" -Description "fetch recently updated PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    Recently updated: $added PRs" -ForegroundColor Gray
    
    # Query 6: Recently created PRs (sorted by created date)
    Write-Host "  Fetching recently created PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'sort:created-desc' --limit 25 --json $jsonFields" -Description "fetch recently created PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    Recently created: $added PRs" -ForegroundColor Gray
    
    # Query 7: Approved but not merged PRs
    Write-Host "  Fetching approved (not merged) PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'review:approved' --limit 25 --json $jsonFields" -Description "fetch approved PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    Approved (not merged): $added PRs" -ForegroundColor Gray
    
    # Query 8: Agent-reviewed PRs
    Write-Host "  Fetching agent-reviewed PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'label:s/agent-reviewed' --limit 25 --json $jsonFields" -Description "fetch agent-reviewed PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    Agent-reviewed: $added PRs" -ForegroundColor Gray
    
    # Query 9: Agent-approved PRs
    Write-Host "  Fetching agent-approved PRs..." -ForegroundColor Gray
    $prResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/maui --state open --search 'label:s/agent-approved' --limit 25 --json $jsonFields" -Description "fetch agent-approved PRs"
    $prs = $prResult | ConvertFrom-Json
    $added = Add-UniquePRs -prs $prs -allPRs ([ref]$allPRs) -seenNumbers ([ref]$seenPRNumbers)
    Write-Host "    Agent-approved: $added PRs" -ForegroundColor Gray
    
    # Filter out drafts
    $allPRs = $allPRs | Where-Object { -not $_.isDraft }
    
    Write-Host "  Found $($allPRs.Count) unique non-draft open PRs in dotnet/maui" -ForegroundColor Green
}
catch {
    Write-Error "Failed to query GitHub: $_"
    exit 1
}

# Fetch all open non-draft PRs from dotnet/docs-maui (sorted by updated date)
Write-Host ""
Write-Host "Fetching open PRs from dotnet/docs-maui..." -ForegroundColor Cyan

$docsMauiPRs = @()
try {
    $docsResult = Invoke-GitHubWithRetry -Command "gh pr list --repo dotnet/docs-maui --state open --search 'sort:updated-desc' --limit 50 --json $jsonFields" -Description "fetch PRs from dotnet/docs-maui"
    $docsMauiPRs = $docsResult | ConvertFrom-Json
    
    # Filter out drafts
    $docsMauiPRs = $docsMauiPRs | Where-Object { -not $_.isDraft }
    
    Write-Host "  Found $($docsMauiPRs.Count) non-draft open PRs in dotnet/docs-maui" -ForegroundColor Green
}
catch {
    Write-Error "Failed to query docs-maui: $_"
    exit 1
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
    $createdDate = [DateTime]::Parse($pr.createdAt, [System.Globalization.CultureInfo]::InvariantCulture)
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

# Helper function to check if author matches filter
function Test-AuthorFilter {
    param($author, $excludeAuthors, $includeAuthors)
    
    # If include list is specified, author must be in it
    if ($includeAuthors.Count -gt 0) {
        return $author -in $includeAuthors
    }
    
    # If exclude list is specified, author must NOT be in it
    if ($excludeAuthors.Count -gt 0) {
        return $author -notin $excludeAuthors
    }
    
    # No filter specified, include all
    return $true
}

# Process and categorize all PRs
$processedPRs = @()

foreach ($pr in $allPRs) {
    # Apply platform filter
    if (-not (Test-PRPlatform -pr $pr -platformFilter $Platform)) {
        continue
    }
    
    # Apply author filter
    if (-not (Test-AuthorFilter -author $pr.author.login -excludeAuthors $ExcludeAuthors -includeAuthors $IncludeAuthors)) {
        continue
    }
    
    $categories = Get-PRCategory -pr $pr
    $labelNames = ($pr.labels | ForEach-Object { $_.name }) -join ", "
    
    # Detect agent labels
    $labelList = $pr.labels | ForEach-Object { $_.name }
    $isAgentApproved = $labelList -contains "s/agent-approved"
    $isAgentReviewed = $labelList -contains "s/agent-reviewed"
    $isAgentChangesRequested = $labelList -contains "s/agent-changes-requested"
    $hasAgentReview = $isAgentApproved -or $isAgentReviewed -or $isAgentChangesRequested
    $agentStatus = if ($isAgentApproved) { "✅ Agent Approved" } 
                   elseif ($isAgentChangesRequested) { "⚠️ Agent Changes Requested" }
                   elseif ($isAgentReviewed) { "🤖 Agent Reviewed" }
                   else { "" }
    
    # Check if PR is in "Ready To Review" on the project board
    $isReadyToReview = $readyToReviewPRNumbers -contains $pr.number
    
    # Check if PR is approved but not merged
    $isApproved = $pr.reviewDecision -eq "APPROVED"
    
    $processedPRs += [PSCustomObject]@{
        Number = $pr.number
        Title = $pr.title
        Author = $pr.author.login
        Assignees = if ($pr.assignees) { ($pr.assignees | ForEach-Object { $_.login }) -join ", " } else { "" }
        Platform = Get-PRPlatform -pr $pr
        Complexity = Get-PRComplexity -pr $pr
        Milestone = if ($pr.milestone) { $pr.milestone.title } else { "" }
        Categories = $categories
        Labels = $labelNames
        CreatedAt = [DateTime]::Parse($pr.createdAt, [System.Globalization.CultureInfo]::InvariantCulture)
        UpdatedAt = [DateTime]::Parse($pr.updatedAt, [System.Globalization.CultureInfo]::InvariantCulture)
        Age = [Math]::Round(((Get-Date) - [DateTime]::Parse($pr.createdAt, [System.Globalization.CultureInfo]::InvariantCulture)).TotalDays)
        Updated = [Math]::Round(((Get-Date) - [DateTime]::Parse($pr.updatedAt, [System.Globalization.CultureInfo]::InvariantCulture)).TotalDays)
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
        IsApproved = $isApproved
        IsReadyToReview = $isReadyToReview
        HasAgentReview = $hasAgentReview
        AgentStatus = $agentStatus
    }
}

Write-Host "  Processed $($processedPRs.Count) PRs matching filters" -ForegroundColor Green

# Process docs-maui PRs
$processedDocsMauiPRs = @()

foreach ($pr in $docsMauiPRs) {
    # Apply author filter
    if (-not (Test-AuthorFilter -author $pr.author.login -excludeAuthors $ExcludeAuthors -includeAuthors $IncludeAuthors)) {
        continue
    }
    
    $categories = Get-PRCategory -pr $pr
    $labelNames = ($pr.labels | ForEach-Object { $_.name }) -join ", "
    
    $processedDocsMauiPRs += [PSCustomObject]@{
        Number = $pr.number
        Title = $pr.title
        Author = $pr.author.login
        Assignees = if ($pr.assignees) { ($pr.assignees | ForEach-Object { $_.login }) -join ", " } else { "" }
        Platform = "Docs"
        Complexity = Get-PRComplexity -pr $pr
        Milestone = if ($pr.milestone) { $pr.milestone.title } else { "" }
        Categories = $categories
        Labels = $labelNames
        CreatedAt = [DateTime]::Parse($pr.createdAt, [System.Globalization.CultureInfo]::InvariantCulture)
        UpdatedAt = [DateTime]::Parse($pr.updatedAt, [System.Globalization.CultureInfo]::InvariantCulture)
        Age = [Math]::Round(((Get-Date) - [DateTime]::Parse($pr.createdAt, [System.Globalization.CultureInfo]::InvariantCulture)).TotalDays)
        Updated = [Math]::Round(((Get-Date) - [DateTime]::Parse($pr.updatedAt, [System.Globalization.CultureInfo]::InvariantCulture)).TotalDays)
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
$approvedPRs = $processedPRs | Where-Object { $_.IsApproved } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, { if ($_.IsPriority) { 0 } else { 1 } }, CreatedAt
$readyToReviewPRs = $processedPRs | Where-Object { $_.IsReadyToReview } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, { if ($_.IsPriority) { 0 } else { 1 } }, CreatedAt
$agentReviewedPRList = $processedPRs | Where-Object { $_.HasAgentReview } | Sort-Object { 
    if ($_.AgentStatus -match "Approved") { 0 } elseif ($_.AgentStatus -match "Reviewed") { 1 } else { 2 }
}, { Get-MilestonePriority $_.Milestone }, CreatedAt
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
# Recent PRs that are waiting for review (REVIEW_REQUIRED or no reviews yet)
$recentPRs = $processedPRs | Where-Object { 
    $_.Age -le 14 -and ($_.ReviewDecision -eq "REVIEW_REQUIRED" -or $_.ReviewDecision -eq $null -or $_.ReviewDecision -eq "")
} | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, { if ($_.IsPriority) { 0 } else { 1 } }, CreatedAt -Descending

# Organize docs-maui PRs by priority and recency (filter recent to waiting for review)
$docsMauiPriorityPRs = $processedDocsMauiPRs | Where-Object { $_.IsPriority } | Sort-Object { 
    Get-MilestonePriority $_.Milestone
}, UpdatedAt -Descending
$docsMauiRecentPRs = $processedDocsMauiPRs | Where-Object {
    $_.ReviewDecision -eq "REVIEW_REQUIRED" -or $_.ReviewDecision -eq $null -or $_.ReviewDecision -eq ""
} | Sort-Object UpdatedAt -Descending

# Filter by category if specified
if ($Category -ne "all") {
    switch ($Category) {
        "milestoned" { $processedPRs = $milestonedPRs }
        "priority" { $processedPRs = $priorityPRs }
        "partner" { $processedPRs = $partnerPRs }
        "community" { $processedPRs = $communityPRs }
        "recent" { $processedPRs = $recentPRs }
        "approved" { $processedPRs = $approvedPRs }
        "ready-to-review" { $processedPRs = $readyToReviewPRs }
        "agent-reviewed" { $processedPRs = $agentReviewedPRList }
        "docs-maui" { $processedPRs = @() } # Will be handled separately
        "default" {
            $defaultPRs = @()
            if ($priorityPRs) { $defaultPRs += @($priorityPRs) }
            if ($milestonedPRs) { $defaultPRs += @($milestonedPRs) }
            $processedPRs = $defaultPRs |
                Where-Object { $_.ReviewDecision -ne "CHANGES_REQUESTED" } |
                Sort-Object Number -Unique
        }
    }
}

# Output functions

# Helper to print a single PR entry (avoids repetition)
function Write-PREntry {
    param($pr, [string]$CategoryName, [string]$RepoOverride)
    
    Write-Host "==="
    Write-Host "Number:$($pr.Number)"
    Write-Host "Title:$($pr.Title)"
    Write-Host "URL:$($pr.URL)"
    Write-Host "Author:$($pr.Author)"
    if ($pr.Assignees) { Write-Host "Assignees:$($pr.Assignees)" } else { Write-Host "Assignees:⚠️ Unassigned" }
    if ($RepoOverride) { Write-Host "Repo:$RepoOverride" } else { Write-Host "Platform:$($pr.Platform)" }
    Write-Host "Complexity:$($pr.Complexity)"
    Write-Host "Milestone:$($pr.Milestone)"
    Write-Host "ReviewStatus:$($pr.ReviewStatus)"
    if ($pr.ProjectStatus) { Write-Host "ProjectStatus:$($pr.ProjectStatus)" }
    if ($pr.AgentStatus) { Write-Host "AgentReview:$($pr.AgentStatus)" } else { Write-Host "AgentReview:❌ Not Reviewed" }
    if ($pr.IsReadyToReview) { Write-Host "BoardStatus:Ready To Review" }
    Write-Host "Age:$($pr.Age) days"
    Write-Host "Updated:$($pr.Updated) days ago"
    Write-Host "Files:$($pr.Files) (+$($pr.Additions)/-$($pr.Deletions))"
    Write-Host "Labels:$($pr.Labels)"
    Write-Host "Category:$CategoryName"
}

function Format-Review-Output {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    # Helper: check if a category should be displayed
    # "default" shows only priority + milestoned; "all" shows everything
    $showCategory = {
        param([string]$cat)
        if ($Category -eq $cat) { return $true }
        if ($Category -eq "all") { return $true }
        if ($Category -eq "default" -and ($cat -eq "priority" -or $cat -eq "milestoned")) { return $true }
        return $false
    }

    # In default mode, only show approved or needs-review PRs (skip changes-requested)
    $defaultFilter = {
        param($prList)
        if ($Category -eq "default") {
            $prList | Where-Object { $_.ReviewDecision -ne "CHANGES_REQUESTED" }
        } else {
            $prList
        }
    }

    # 1. Priority PRs (P/0) - ALWAYS on top
    if ($priorityPRs.Count -gt 0 -and (& $showCategory "priority")) {
        $priorityToDisplay = & $defaultFilter $priorityPRs
        if ($priorityToDisplay.Count -gt 0) {
            Write-Host ""
            Write-Host "🔴 PRIORITY (P/0) PRs - $($priorityToDisplay.Count) found" -ForegroundColor Red
            Write-Host "-----------------------------------------------------------"
            foreach ($pr in ($priorityToDisplay | Select-Object -First $Limit)) {
                Write-PREntry -pr $pr -CategoryName "priority"
            }
        }
    }
    
    # 2. Approved but not merged PRs
    if ($approvedPRs.Count -gt 0 -and (& $showCategory "approved")) {
        $approvedToDisplay = if ($Category -eq "approved") {
            $approvedPRs
        } else {
            $approvedPRs | Where-Object { -not $_.IsPriority }
        }
        if ($approvedToDisplay.Count -gt 0) {
            Write-Host ""
            Write-Host "🟢 APPROVED (NOT MERGED) PRs - $($approvedToDisplay.Count) found" -ForegroundColor Green
            Write-Host "-----------------------------------------------------------"
            foreach ($pr in ($approvedToDisplay | Select-Object -First $Limit)) {
                Write-PREntry -pr $pr -CategoryName "approved"
            }
        }
    }
    
    # 3. Ready To Review (from project board)
    if ($readyToReviewPRs.Count -gt 0 -and (& $showCategory "ready-to-review")) {
        $readyToDisplay = if ($Category -eq "ready-to-review") {
            $readyToReviewPRs
        } else {
            $readyToReviewPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved }
        }
        if ($readyToDisplay.Count -gt 0) {
            Write-Host ""
            Write-Host "📋 READY TO REVIEW (Project Board) PRs - $($readyToDisplay.Count) found" -ForegroundColor Yellow
            Write-Host "-----------------------------------------------------------"
            foreach ($pr in ($readyToDisplay | Select-Object -First $Limit)) {
                Write-PREntry -pr $pr -CategoryName "ready-to-review"
            }
        }
    }
    
    # 4. Milestoned PRs (review these BEFORE non-milestoned agent-reviewed PRs)
    if ($milestonedPRs.Count -gt 0 -and (& $showCategory "milestoned")) {
        $milestonedToDisplay = if ($Category -eq "milestoned") {
            $milestonedPRs
        } else {
            $milestonedPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview }
        }
        $milestonedToDisplay = & $defaultFilter $milestonedToDisplay
        if ($milestonedToDisplay.Count -gt 0) {
            Write-Host ""
            Write-Host "📅 MILESTONED PRs - $($milestonedToDisplay.Count) found" -ForegroundColor Yellow
            Write-Host "-----------------------------------------------------------"
            foreach ($pr in ($milestonedToDisplay | Select-Object -First $Limit)) {
                Write-PREntry -pr $pr -CategoryName "milestoned"
            }
        }
    }
    
    # 5. Agent Reviewed PRs
    if ($agentReviewedPRList.Count -gt 0 -and (& $showCategory "agent-reviewed")) {
        $agentToDisplay = if ($Category -eq "agent-reviewed") {
            $agentReviewedPRList
        } else {
            $agentReviewedPRList | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview -and $_.Milestone -eq "" }
        }
        if ($agentToDisplay.Count -gt 0) {
            Write-Host ""
            Write-Host "🤖 AGENT REVIEWED PRs - $($agentToDisplay.Count) found" -ForegroundColor DarkCyan
            Write-Host "-----------------------------------------------------------"
            foreach ($pr in ($agentToDisplay | Select-Object -First $Limit)) {
                Write-PREntry -pr $pr -CategoryName "agent-reviewed"
            }
        }
    }
    
    # 6. Partner PRs
    if ($partnerPRs.Count -gt 0 -and (& $showCategory "partner")) {
        $partnerToDisplay = if ($Category -eq "partner") {
            $partnerPRs
        } else {
            $partnerPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview -and $_.Milestone -eq "" }
        }
        if ($partnerToDisplay.Count -gt 0) {
            Write-Host ""
            Write-Host "🤝 PARTNER PRs - $($partnerToDisplay.Count) found" -ForegroundColor Magenta
            Write-Host "-----------------------------------------------------------"
            foreach ($pr in ($partnerToDisplay | Select-Object -First $Limit)) {
                Write-PREntry -pr $pr -CategoryName "partner"
            }
        }
    }
    
    # 7. Community PRs
    if ($communityPRs.Count -gt 0 -and (& $showCategory "community")) {
        $communityToDisplay = if ($Category -eq "community") {
            $communityPRs
        } else {
            $communityPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview -and $_.Milestone -eq "" }
        }
        if ($communityToDisplay.Count -gt 0) {
            Write-Host ""
            Write-Host "✨ COMMUNITY PRs - $($communityToDisplay.Count) found" -ForegroundColor Green
            Write-Host "-----------------------------------------------------------"
            foreach ($pr in ($communityToDisplay | Select-Object -First $Limit)) {
                Write-PREntry -pr $pr -CategoryName "community"
            }
        }
    }
    
    # 8. Recent PRs waiting for review
    $recentToDisplay = if ($Category -eq "recent") {
        $recentPRs
    } else {
        $recentPRs | Where-Object { 
            -not $_.IsPriority -and -not $_.IsPartner -and -not $_.IsCommunity -and -not $_.IsApproved -and -not $_.IsReadyToReview -and $_.Milestone -eq ""
        }
    }
    if ($recentToDisplay.Count -gt 0 -and (& $showCategory "recent")) {
        Write-Host ""
        Write-Host "🕐 RECENT PRs WAITING FOR REVIEW (last 2 weeks) - $($recentToDisplay.Count) found" -ForegroundColor Cyan
        Write-Host "-----------------------------------------------------------"
        $recentToShow = [Math]::Max($RecentLimit, 5)
        foreach ($pr in ($recentToDisplay | Select-Object -First $recentToShow)) {
            Write-PREntry -pr $pr -CategoryName "recent"
        }
    }
    
    # 9. docs-maui PRs - Priority
    if ($docsMauiPriorityPRs.Count -gt 0 -and (& $showCategory "docs-maui")) {
        Write-Host ""
        Write-Host "📚 DOCS-MAUI PRIORITY PRs - $($docsMauiPriorityPRs.Count) found" -ForegroundColor Blue
        Write-Host "-----------------------------------------------------------"
        foreach ($pr in ($docsMauiPriorityPRs | Select-Object -First $DocsLimit)) {
            Write-PREntry -pr $pr -CategoryName "docs-maui-priority" -RepoOverride "docs-maui"
        }
    }
    
    # 10. docs-maui PRs - Waiting for Review
    if ($docsMauiRecentPRs.Count -gt 0 -and (& $showCategory "docs-maui")) {
        Write-Host ""
        Write-Host "📖 DOCS-MAUI PRs WAITING FOR REVIEW - $($docsMauiRecentPRs.Count) found" -ForegroundColor Blue
        Write-Host "-----------------------------------------------------------"
        $docsToShow = [Math]::Max($DocsLimit, 5)
        foreach ($pr in ($docsMauiRecentPRs | Select-Object -First $docsToShow)) {
            Write-PREntry -pr $pr -CategoryName "docs-maui-waiting-for-review" -RepoOverride "docs-maui"
        }
    }
    
    # Summary - show only categories that were displayed, with actual filtered/deduped counts
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "SUMMARY (showing: $Category)" -ForegroundColor Cyan
    if (& $showCategory "priority") {
        $c = @(& $defaultFilter $priorityPRs).Count
        Write-Host "  Priority (P/0): $c"
    }
    if (& $showCategory "approved") {
        $l = if ($Category -eq "approved") { $approvedPRs } else { @($approvedPRs | Where-Object { -not $_.IsPriority }) }
        Write-Host "  Approved (not merged): $($l.Count)"
    }
    if (& $showCategory "ready-to-review") {
        $l = if ($Category -eq "ready-to-review") { $readyToReviewPRs } else { @($readyToReviewPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved }) }
        Write-Host "  Ready To Review (board): $($l.Count)"
    }
    if (& $showCategory "milestoned") {
        $l = if ($Category -eq "milestoned") { $milestonedPRs } else { @($milestonedPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview }) }
        $l = @(& $defaultFilter $l)
        Write-Host "  Milestoned: $($l.Count)"
    }
    if (& $showCategory "agent-reviewed") {
        $l = if ($Category -eq "agent-reviewed") { $agentReviewedPRList } else { @($agentReviewedPRList | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview -and $_.Milestone -eq "" }) }
        Write-Host "  Agent Reviewed: $($l.Count)"
    }
    if (& $showCategory "partner") {
        $l = if ($Category -eq "partner") { $partnerPRs } else { @($partnerPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview -and $_.Milestone -eq "" }) }
        Write-Host "  Partner: $($l.Count)"
    }
    if (& $showCategory "community") {
        $l = if ($Category -eq "community") { $communityPRs } else { @($communityPRs | Where-Object { -not $_.IsPriority -and -not $_.IsApproved -and -not $_.IsReadyToReview -and $_.Milestone -eq "" }) }
        Write-Host "  Community: $($l.Count)"
    }
    if (& $showCategory "recent") { Write-Host "  Recent Waiting for Review (2 weeks): $($recentPRs.Count)" }
    if (& $showCategory "docs-maui") { Write-Host "  docs-maui Priority: $($docsMauiPriorityPRs.Count)" }
    if (& $showCategory "docs-maui") { Write-Host "  docs-maui Waiting for Review: $($docsMauiRecentPRs.Count)" }
}

function Format-Json-Output {
    $output = @{
        Priority = $priorityPRs | Select-Object -First $Limit
        Approved = $approvedPRs | Select-Object -First $Limit
        ReadyToReview = $readyToReviewPRs | Select-Object -First $Limit
        AgentReviewed = $agentReviewedPRList | Select-Object -First $Limit
        Milestoned = $milestonedPRs | Select-Object -First $Limit
        Partner = $partnerPRs | Select-Object -First $Limit
        Community = $communityPRs | Select-Object -First $Limit
        RecentWaitingForReview = $recentPRs | Select-Object -First ([Math]::Max($RecentLimit, 5))
        DocsMauiPriority = $docsMauiPriorityPRs | Select-Object -First $DocsLimit
        DocsMauiWaitingForReview = $docsMauiRecentPRs | Select-Object -First ([Math]::Max($DocsLimit, 5))
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
