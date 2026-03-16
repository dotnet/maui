#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Queries open issues from dotnet/maui labeled with "Known Build Error".

.DESCRIPTION
    This script queries GitHub for open issues that have the "Known Build Error" label
    and checks each for associated pull requests. Issues without associated PRs are
    prime candidates for automated fixing.

    The results can be filtered and formatted for different consumers (agents, humans, CI).

.PARAMETER Limit
    Maximum number of issues to return (default: 50)

.PARAMETER IncludeWithPRs
    If set, includes issues that already have linked PRs. By default, only issues
    without open PRs are returned.

.PARAMETER OutputFormat
    Output format: "table", "json", "markdown", or "triage" (default: "table")

.PARAMETER SortBy
    Sort by: "created" or "updated" (default: "created")

.PARAMETER SortOrder
    Sort order: "asc" (oldest first) or "desc" (newest first) (default: "asc")

.EXAMPLE
    ./query-build-errors.ps1
    # Returns known build errors without associated PRs, oldest first

.EXAMPLE
    ./query-build-errors.ps1 -IncludeWithPRs -OutputFormat markdown
    # Returns all known build errors in markdown format

.EXAMPLE
    ./query-build-errors.ps1 -OutputFormat triage -Limit 10
    # Returns up to 10 issues in triage format for agent consumption
#>

param(
    [Parameter(Mandatory = $false)]
    [int]$Limit = 50,

    [Parameter(Mandatory = $false)]
    [switch]$IncludeWithPRs,

    [Parameter(Mandatory = $false)]
    [ValidateSet("table", "json", "markdown", "triage")]
    [string]$OutputFormat = "table",

    [Parameter(Mandatory = $false)]
    [ValidateSet("created", "updated")]
    [string]$SortBy = "created",

    [Parameter(Mandatory = $false)]
    [ValidateSet("asc", "desc")]
    [string]$SortOrder = "asc"
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Known Build Error Discovery                     ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Check for GitHub CLI prerequisite
try {
    $null = Get-Command gh -ErrorAction Stop
} catch {
    Write-Host ""
    Write-Host "❌ GitHub CLI (gh) is not installed" -ForegroundColor Red
    Write-Host ""
    Write-Host "Installation:" -ForegroundColor Cyan
    Write-Host "  Windows:  winget install --id GitHub.cli" -ForegroundColor White
    Write-Host "  macOS:    brew install gh" -ForegroundColor White
    Write-Host "  Linux:    See https://cli.github.com/manual/installation" -ForegroundColor White
    Write-Host ""
    Write-Host "After installation, authenticate with: gh auth login" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

# Verify GitHub CLI authentication
$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ GitHub CLI (gh) is not authenticated" -ForegroundColor Red
    Write-Host "Run: gh auth login" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}
Write-Host "✅ GitHub CLI authenticated" -ForegroundColor Green

Write-Host ""
Write-Host "Querying issues with 'Known Build Error' label..." -ForegroundColor Cyan

# Build gh issue list command arguments
$ghArgs = @(
    "issue", "list",
    "--repo", "dotnet/maui",
    "--state", "open",
    "--label", "Known Build Error",
    "--limit", $Limit,
    "--json", "number,title,author,createdAt,updatedAt,labels,url,milestone,body"
)

Write-Host "Running: gh $($ghArgs -join ' ')" -ForegroundColor DarkGray

try {
    $result = & gh @ghArgs 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Error "GitHub CLI error: $result"
        exit 1
    }

    $issues = $result | ConvertFrom-Json
}
catch {
    Write-Error "Failed to query GitHub: $_"
    exit 1
}

if ($issues.Count -eq 0) {
    Write-Host "No issues found with 'Known Build Error' label." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $($issues.Count) issues with 'Known Build Error' label" -ForegroundColor Green

# Process each issue and check for linked PRs
Write-Host "Checking for linked PRs..." -ForegroundColor Cyan
$processedIssues = @()
$issueIndex = 0
$totalIssues = @($issues).Count

foreach ($issue in $issues) {
    $issueIndex++
    Write-Host "`r  Processing issue $issueIndex of $totalIssues (#$($issue.number))..." -NoNewline -ForegroundColor DarkGray

    $createdDate = [DateTime]::Parse($issue.createdAt)
    $ageInDays = [Math]::Round(((Get-Date) - $createdDate).TotalDays)
    $labelNames = ($issue.labels | ForEach-Object { $_.name }) -join ", "

    # Extract error summary from body (first 200 chars, cleaned)
    $errorSummary = ""
    if ($issue.body) {
        $errorSummary = $issue.body -replace '\r?\n', ' ' -replace '\s+', ' '
        if ($errorSummary.Length -gt 200) {
            $errorSummary = $errorSummary.Substring(0, 197) + "..."
        }
    }

    # Milestone info
    $milestoneName = ""
    if ($issue.milestone) {
        $milestoneName = $issue.milestone.title
    }

    # Fetch linked PRs via GraphQL
    $linkedPRs = @()
    $hasOpenPR = $false
    try {
        $graphqlQuery = @"
{
  repository(owner: "dotnet", name: "maui") {
    issue(number: $($issue.number)) {
      timelineItems(itemTypes: [CONNECTED_EVENT, CROSS_REFERENCED_EVENT], first: 10) {
        nodes {
          ... on ConnectedEvent { subject { ... on PullRequest { number title state url } } }
          ... on CrossReferencedEvent { source { ... on PullRequest { number title state url } } }
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
                    Title  = $pr.title
                    State  = $pr.state
                    URL    = $pr.url
                }
                if ($pr.state -eq "OPEN") {
                    $hasOpenPR = $true
                }
            }
        }
    }
    catch {
        # Silently continue if GraphQL fails
    }

    $processedIssues += [PSCustomObject]@{
        Number       = $issue.number
        Title        = if ($issue.title.Length -gt 80) { $issue.title.Substring(0, 77) + "..." } else { $issue.title }
        FullTitle    = $issue.title
        Author       = $issue.author.login
        Created      = $createdDate.ToString("yyyy-MM-dd")
        Age          = "$ageInDays days"
        AgeDays      = $ageInDays
        Labels       = $labelNames
        Milestone    = $milestoneName
        LinkedPRs    = $linkedPRs
        HasOpenPR    = $hasOpenPR
        ErrorSummary = $errorSummary
        URL          = $issue.url
    }
}
Write-Host "" # New line after progress

# Filter based on PR status (unless -IncludeWithPRs is set)
if (-not $IncludeWithPRs) {
    $beforeCount = $processedIssues.Count
    $processedIssues = $processedIssues | Where-Object { -not $_.HasOpenPR }
    $filteredCount = $beforeCount - @($processedIssues).Count
    if ($filteredCount -gt 0) {
        Write-Host "Filtered out $filteredCount issues with open PRs" -ForegroundColor DarkGray
    }
}

# Sort results
$processedIssues = switch ($SortBy) {
    "created" {
        if ($SortOrder -eq "asc") { $processedIssues | Sort-Object AgeDays -Descending }
        else { $processedIssues | Sort-Object AgeDays }
    }
    "updated" {
        $processedIssues  # Already in default order from GitHub
    }
}

$processedIssues = @($processedIssues)

if ($processedIssues.Count -eq 0) {
    Write-Host ""
    Write-Host "All Known Build Error issues already have associated PRs." -ForegroundColor Green
    Write-Host "Nothing to fix! 🎉" -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "Found $($processedIssues.Count) issues ready for fixing" -ForegroundColor Green

# Output based on format
function Format-Table-Output {
    param($issues)

    Write-Host ""
    Write-Host "Known Build Errors (No Open PR)" -ForegroundColor Cyan
    Write-Host ("=" * 100)

    $issues | Format-Table -Property @(
        @{Label = "Issue"; Expression = { $_.Number }; Width = 7 },
        @{Label = "Title"; Expression = { $_.Title }; Width = 55 },
        @{Label = "Age"; Expression = { $_.Age }; Width = 10 },
        @{Label = "Milestone"; Expression = { $_.Milestone }; Width = 20 },
        @{Label = "PRs"; Expression = { $_.LinkedPRs.Count }; Width = 4 }
    ) -AutoSize
}

function Format-Markdown-Output {
    param($issues)

    $output = @()
    $output += "# Known Build Errors Without Open PRs"
    $output += ""
    $output += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $output += ""
    $output += "| Issue | Title | Age | Milestone | Linked PRs |"
    $output += "|-------|-------|-----|-----------|------------|"

    foreach ($issue in $issues) {
        $issueLink = "[#$($issue.Number)]($($issue.URL))"
        $prInfo = if ($issue.LinkedPRs.Count -eq 0) { "None" } else {
            ($issue.LinkedPRs | ForEach-Object { "#$($_.Number)[$($_.State)]" }) -join ", "
        }
        $output += "| $issueLink | $($issue.FullTitle -replace '\|', '\|') | $($issue.Age) | $($issue.Milestone) | $prInfo |"
    }

    $output += ""
    $output += "---"
    $output += "Total: $($issues.Count) issues without open PRs"

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
        Write-Output "Age:$($issue.Age)"
        Write-Output "Milestone:$($issue.Milestone)"
        Write-Output "Labels:$($issue.Labels)"
        Write-Output "HasOpenPR:$($issue.HasOpenPR)"

        # Linked PRs
        if ($issue.LinkedPRs -and $issue.LinkedPRs.Count -gt 0) {
            $prList = ($issue.LinkedPRs | ForEach-Object {
                "#$($_.Number)[$($_.State)]"
            }) -join "; "
            Write-Output "LinkedPRs:$prList"
        }
        else {
            Write-Output "LinkedPRs:None"
        }

        Write-Output "ErrorSummary:$($issue.ErrorSummary)"
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

# Summary statistics
Write-Host ""
Write-Host "Summary:" -ForegroundColor Cyan
Write-Host "  Total Known Build Errors: $totalIssues"
Write-Host "  Without Open PR: $($processedIssues.Count)"
Write-Host "  With Open PR: $($totalIssues - $processedIssues.Count)"

if ($processedIssues.Count -gt 0) {
    $avgAge = [Math]::Round(($processedIssues | Measure-Object AgeDays -Average).Average)
    Write-Host "  Average age (no PR): $avgAge days"
}

# Return the issues for pipeline usage
return $processedIssues
