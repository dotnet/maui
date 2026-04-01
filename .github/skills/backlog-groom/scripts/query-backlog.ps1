#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Queries open issues from dotnet/maui that need backlog grooming.

.DESCRIPTION
    This script queries GitHub for open issues and evaluates them for grooming needs:
    - Stale issues (no activity for extended periods)
    - Issues missing reproduction steps
    - Issues with linked PRs that have been merged (potentially fixed)
    - Issues missing platform/area labels
    - Issues without milestones

    Unlike issue-triage (which focuses on NEW issues without milestones),
    this script targets the EXISTING backlog — issues that may have milestones
    but need attention for other reasons.

.PARAMETER GroomType
    Type of grooming to perform:
    - "stale" - Issues with no activity for MinAge days (default: 180)
    - "needs-repro" - Issues that appear to lack reproduction steps
    - "possibly-fixed" - Issues with linked merged PRs but still open
    - "missing-labels" - Issues missing platform or area labels
    - "all" - Run all assessments (default)

.PARAMETER Platform
    Filter by platform label: "android", "ios", "windows", "maccatalyst", or "all" (default)

.PARAMETER Area
    Filter by area label pattern (e.g., "collectionview", "shell", "navigation")

.PARAMETER Limit
    Maximum number of issues to return (default: 50)

.PARAMETER MinAge
    Minimum age in days for staleness check (default: 180 for stale, 0 otherwise)

.PARAMETER OutputFormat
    Output format: "table", "json", "markdown", "groom" (default: "table")

.PARAMETER OutputFile
    Optional file path to save results

.PARAMETER Skip
    Skip first N issues (for pagination)

.PARAMETER IncludeMilestoned
    Include issues that already have milestones (default: true, unlike issue-triage)

.EXAMPLE
    ./query-backlog.ps1
    # Returns up to 50 issues needing grooming across all categories

.EXAMPLE
    ./query-backlog.ps1 -GroomType stale -MinAge 365
    # Find issues with no activity for over a year

.EXAMPLE
    ./query-backlog.ps1 -GroomType possibly-fixed -OutputFormat markdown
    # Find issues that may have been fixed by merged PRs

.EXAMPLE
    ./query-backlog.ps1 -GroomType needs-repro -Platform android -Limit 20
    # Find Android issues lacking reproduction steps
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("stale", "needs-repro", "possibly-fixed", "missing-labels", "all")]
    [string]$GroomType = "all",

    [Parameter(Mandatory = $false)]
    [ValidateSet("android", "ios", "windows", "maccatalyst", "all")]
    [string]$Platform = "all",

    [Parameter(Mandatory = $false)]
    [string]$Area = "",

    [Parameter(Mandatory = $false)]
    [int]$Limit = 50,

    [Parameter(Mandatory = $false)]
    [int]$MinAge = 0,

    [Parameter(Mandatory = $false)]
    [ValidateSet("table", "json", "markdown", "groom")]
    [string]$OutputFormat = "table",

    [Parameter(Mandatory = $false)]
    [string]$OutputFile = "",

    [Parameter(Mandatory = $false)]
    [int]$Skip = 0,

    [Parameter(Mandatory = $false)]
    [switch]$IncludeMilestoned = $true
)

$ErrorActionPreference = "Stop"

# ── Prerequisites ──────────────────────────────────────────────────────────────
try {
    $null = Get-Command gh -ErrorAction Stop
} catch {
    Write-Host ""
    Write-Host "❌ GitHub CLI (gh) is not installed" -ForegroundColor Red
    Write-Host ""
    Write-Host "The backlog-groom skill requires GitHub CLI for querying issues." -ForegroundColor Yellow
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

$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌ GitHub CLI (gh) is not authenticated" -ForegroundColor Red
    Write-Host "Run: gh auth login" -ForegroundColor Cyan
    Write-Host ""
    exit 1
}

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Backlog Grooming Query                          ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""
Write-Host "  Groom type: $GroomType" -ForegroundColor White
Write-Host "  Platform:   $Platform" -ForegroundColor White
Write-Host "  Limit:      $Limit" -ForegroundColor White

# ── Platform label mapping ─────────────────────────────────────────────────────
$platformLabels = @{
    "android"      = "platform/android 🤖"
    "ios"          = "platform/iOS 🍎"
    "windows"      = "platform/windows 🪟"
    "maccatalyst"  = "platform/macOS 🍏"
}

# ── Retry helper ───────────────────────────────────────────────────────────────
function Invoke-GitHubWithRetry {
    param(
        [scriptblock]$ScriptBlock,
        [string]$Description = "GitHub API call",
        [int]$MaxRetries = 3
    )

    $retryCount = 0
    $baseDelay = 2

    while ($retryCount -lt $MaxRetries) {
        try {
            $result = & $ScriptBlock
            if ($LASTEXITCODE -eq 0 -or $null -eq $LASTEXITCODE) {
                return $result
            }

            $errorText = "$result"
            if ($errorText -match "502|503|504|timeout|stream error|CANCEL|Bad Gateway") {
                $retryCount++
                if ($retryCount -lt $MaxRetries) {
                    $delay = $baseDelay * [Math]::Pow(2, $retryCount - 1)
                    Write-Warning "  Transient error for $Description. Retrying in ${delay}s... (attempt $($retryCount + 1)/$MaxRetries)"
                    Start-Sleep -Seconds $delay
                    continue
                }
            }
            throw "Failed: $Description - $errorText"
        }
        catch {
            $retryCount++
            if ($retryCount -ge $MaxRetries) { throw }
            $delay = $baseDelay * [Math]::Pow(2, $retryCount - 1)
            Write-Warning "  Error in $Description. Retrying in ${delay}s... (attempt $($retryCount + 1)/$MaxRetries)"
            Start-Sleep -Seconds $delay
        }
    }
}

# ── Query issues ───────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "Querying GitHub issues..." -ForegroundColor Cyan

# Build search qualifiers based on groom type
$searchQualifiers = @()

switch ($GroomType) {
    "stale" {
        # Issues not updated recently
        $staleAge = if ($MinAge -gt 0) { $MinAge } else { 180 }
        $staleBefore = (Get-Date).AddDays(-$staleAge).ToString("yyyy-MM-dd")
        $searchQualifiers += "updated:<$staleBefore"
        Write-Host "  Staleness threshold: $staleAge days (before $staleBefore)" -ForegroundColor DarkGray
    }
    "needs-repro" {
        # We'll post-filter for missing repro — no special search qualifier needed
    }
    "possibly-fixed" {
        # We'll post-filter for linked merged PRs — sort by updated to find recently-touched
        $searchQualifiers += "sort:updated-desc"
    }
    "missing-labels" {
        # Issues without platform or area labels — post-filter
    }
    "all" {
        # Broader query, post-filter for all categories
    }
}

# Labels to exclude (issues already flagged for action)
$excludeLabels = @(
    "s/needs-info",
    "s/needs-repro",
    "s/try-latest-version",
    "s/move-to-vs-feedback"
)

# Build gh command
$ghArgs = @(
    "issue", "list",
    "--repo", "dotnet/maui",
    "--state", "open",
    "--limit", [Math]::Min($Limit * 2, 500),  # Over-fetch to account for post-filtering
    "--json", "number,title,author,createdAt,updatedAt,labels,comments,url,milestone,body"
)

# Add platform filter
if ($Platform -ne "all" -and $platformLabels.ContainsKey($Platform)) {
    $ghArgs += @("--label", $platformLabels[$Platform])
}

# Add area filter
if ($Area -ne "") {
    $ghArgs += @("--label", "area-$Area")
}

# Add search qualifiers
if ($searchQualifiers.Count -gt 0) {
    $ghArgs += @("--search", ($searchQualifiers -join " "))
}

Write-Host "  Running: gh $($ghArgs -join ' ')" -ForegroundColor DarkGray

$allIssues = Invoke-GitHubWithRetry -Description "issue query" -ScriptBlock {
    $result = & gh @ghArgs 2>&1
    if ($LASTEXITCODE -ne 0) { throw "gh error: $result" }
    $result | ConvertFrom-Json
}

# ── Post-filter ────────────────────────────────────────────────────────────────
$issues = $allIssues | Where-Object {
    # Exclude issues with blocking labels
    $issueLabels = $_.labels | ForEach-Object { $_.name }
    $hasBlockingLabel = $false
    foreach ($excludeLabel in $excludeLabels) {
        if ($issueLabels -contains $excludeLabel) {
            $hasBlockingLabel = $true
            break
        }
    }
    if ($hasBlockingLabel) { return $false }

    # Optionally exclude milestoned issues
    if (-not $IncludeMilestoned) {
        if ($null -ne $_.milestone -and $_.milestone.title -ne "") {
            return $false
        }
    }

    $true
}

# Apply date filters for non-stale queries
if ($GroomType -ne "stale" -and $MinAge -gt 0) {
    $minDate = (Get-Date).AddDays(-$MinAge)
    $issues = $issues | Where-Object { [DateTime]$_.createdAt -lt $minDate }
}

# Apply skip for pagination
if ($Skip -gt 0) {
    $issues = $issues | Select-Object -Skip $Skip
}

# NOTE: We do NOT limit here — we limit AFTER groom-type filtering below
# so that $Limit reflects the number of matching issues, not pre-filter candidates.

if (@($issues).Count -eq 0) {
    Write-Host "No issues found matching the criteria." -ForegroundColor Yellow
    exit 0
}

Write-Host "Found $(@($issues).Count) candidate issues to assess" -ForegroundColor Green

# ── Assess each issue ──────────────────────────────────────────────────────────
Write-Host "Assessing issue health..." -ForegroundColor Cyan

$processedIssues = @()
$issueIndex = 0
$totalIssues = @($issues).Count

foreach ($issue in $issues) {
    $issueIndex++
    Write-Host "`r  Assessing issue $issueIndex of $totalIssues (#$($issue.number))..." -NoNewline -ForegroundColor DarkGray

    $labelNames = ($issue.labels | ForEach-Object { $_.name }) -join ", "
    $platformLabel = ($issue.labels | Where-Object { $_.name -like "platform/*" } | Select-Object -First 1)?.name ?? ""
    $areaLabels = ($issue.labels | Where-Object { $_.name -like "area-*" } | ForEach-Object { $_.name -replace "^area-", "" }) -join ", "
    $priorityLabel = ($issue.labels | Where-Object { $_.name -like "p/*" } | Select-Object -First 1)?.name ?? ""

    $createdDate = [DateTime]$issue.createdAt
    $updatedDate = [DateTime]$issue.updatedAt
    $ageInDays = [Math]::Round(((Get-Date) - $createdDate).TotalDays)
    $daysSinceUpdate = [Math]::Round(((Get-Date) - $updatedDate).TotalDays)
    $commentCount = if ($issue.comments) { @($issue.comments).Count } else { 0 }
    $milestoneTitle = if ($issue.milestone) { $issue.milestone.title } else { "" }

    # ── Health checks ──────────────────────────────────────────────────────────
    $flags = @()
    $suggestions = @()

    # 1. Staleness check
    $isStale = $daysSinceUpdate -ge 180
    $isVeryStale = $daysSinceUpdate -ge 365
    if ($isVeryStale) {
        $flags += "very-stale"
        $suggestions += "No activity for $daysSinceUpdate days. Consider closing or requesting updated repro."
    }
    elseif ($isStale) {
        $flags += "stale"
        $suggestions += "No activity for $daysSinceUpdate days. May need re-evaluation."
    }

    # 2. Reproduction check (heuristic: look for code blocks, numbered steps, XAML, repro link)
    $body = if ($issue.body) { $issue.body } else { "" }
    $hasCodeBlock = $body -match '```'
    $hasNumberedSteps = $body -match '^\s*\d+[\.\)]\s' -or $body -match 'steps?\s*(to\s*)?reproduce'
    $hasXaml = $body -match '<\w+.*xmlns'
    $hasScreenshot = $body -match '!\[.*\]\(.*\)'
    $hasReproLink = $body -match '(?i)(github\.com|gitlab\.com|bitbucket\.org)/\S+'
    $hasExpectedActual = $body -match '(?i)(expected|actual)\s*(behavior|result|output|outcome)?'
    $hasPlatformVersionDetails = $body -match '(?i)(iOS\s*\d+|Android\s*\d+|API\s*(level\s*)?\d+|Windows\s*(10|11)|macOS\s*\d+)'
    $hasDotNetVersion = $body -match '(?i)(\.NET\s*(SDK\s*)?\d+\.\d+|MAUI\s*(workload\s*)?\d+\.\d+|dotnet\s+workload)'
    $hasRegressionInfo = $body -match '(?i)(regression|regressed|used\s*to\s*work|worked\s*(in|before|on)|broke\s*(in|since))'
    $hasStackTrace = $body -match '(?i)(exception|stacktrace|stack trace|at\s+\w+\.\w+)'
    $hasFrequencyInfo = $body -match '(?i)(always|every\s*time|consistently|100\s*%|sometimes|intermittent|random(ly)?|occasionally|sporadic|reproducib)'
    $hasWorkaroundInfo = $body -match '(?i)(workaround|work[\s-]*around|temporary\s*(fix|solution))'
    $bodyLength = $body.Length

    $hasRepro = $hasCodeBlock -or $hasNumberedSteps -or $hasXaml -or $hasReproLink
    $hasMinimalInfo = $bodyLength -gt 200

    if (-not $hasRepro -and -not $hasMinimalInfo) {
        $flags += "needs-repro"
        $suggestions += "Issue body is short ($bodyLength chars) with no code blocks, numbered steps, or repro link."
    }
    elseif (-not $hasRepro -and $hasMinimalInfo) {
        $flags += "weak-repro"
        $suggestions += "Issue has description but no code blocks, XAML, numbered steps, or repro link."
    }

    # 3. Missing labels check
    $missingPlatform = $platformLabel -eq ""
    $missingArea = $areaLabels -eq ""
    $missingPriority = $priorityLabel -eq ""

    if ($missingPlatform) {
        $flags += "no-platform"
        $suggestions += "Missing platform label."
    }
    if ($missingArea) {
        $flags += "no-area"
        $suggestions += "Missing area label."
    }

    # 4. Linked PR check (GraphQL for merged PRs)
    $linkedPRs = @()
    $hasMergedPR = $false
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
        $prResult = Invoke-GitHubWithRetry -Description "linked PRs for #$($issue.number)" -ScriptBlock {
            gh api graphql -f query=$graphqlQuery 2>$null | ConvertFrom-Json
        }
        $nodes = $prResult.data.repository.issue.timelineItems.nodes
        foreach ($node in $nodes) {
            $pr = $null
            if ($node.subject) { $pr = $node.subject }
            elseif ($node.source) { $pr = $node.source }
            if ($pr -and $pr.number) {
                $linkedPRs += [PSCustomObject]@{
                    Number    = $pr.number
                    Title     = $pr.title
                    State     = $pr.state
                    URL       = $pr.url
                    Milestone = $pr.milestone?.title ?? ""
                }
                if ($pr.state -eq "MERGED") { $hasMergedPR = $true }
            }
        }
    }
    catch {
        # Silently continue if GraphQL fails
    }

    if ($hasMergedPR) {
        $mergedPRs = $linkedPRs | Where-Object { $_.State -eq "MERGED" }
        $prNumbers = ($mergedPRs | ForEach-Object { "#$($_.Number)" }) -join ", "
        $flags += "possibly-fixed"
        $suggestions += "Has merged PR(s): $prNumbers. May already be fixed — verify and close."
    }

    # 5. No milestone check
    if ($milestoneTitle -eq "") {
        $flags += "no-milestone"
    }

    # ── Filter by groom type ──────────────────────────────────────────────────
    $includeIssue = switch ($GroomType) {
        "stale"          { $flags -contains "stale" -or $flags -contains "very-stale" }
        "needs-repro"    { $flags -contains "needs-repro" -or $flags -contains "weak-repro" }
        "possibly-fixed" { $flags -contains "possibly-fixed" }
        "missing-labels" { $flags -contains "no-platform" -or $flags -contains "no-area" }
        "all"            { $flags.Count -gt 0 }
    }

    if (-not $includeIssue) { continue }

    $processedIssues += [PSCustomObject]@{
        Number          = $issue.number
        Title           = if ($issue.title.Length -gt 60) { $issue.title.Substring(0, 57) + "..." } else { $issue.title }
        FullTitle       = $issue.title
        Author          = $issue.author.login
        Created         = $createdDate.ToString("yyyy-MM-dd")
        Updated         = $updatedDate.ToString("yyyy-MM-dd")
        Age             = "$ageInDays days"
        AgeDays         = $ageInDays
        DaysSinceUpdate = $daysSinceUpdate
        CommentCount    = $commentCount
        Platform        = $platformLabel -replace "^platform/", "" -replace " [🤖🍎🪟🍏]$", ""
        Areas           = $areaLabels
        Priority        = $priorityLabel
        Labels          = $labelNames
        Milestone       = $milestoneTitle
        LinkedPRs       = $linkedPRs
        HasMergedPR     = $hasMergedPR
        HasRepro        = $hasRepro
        HasReproLink    = $hasReproLink
        HasExpectedActual = $hasExpectedActual
        HasPlatformVersionDetails = $hasPlatformVersionDetails
        HasDotNetVersion  = $hasDotNetVersion
        HasRegressionInfo = $hasRegressionInfo
        HasStackTrace     = $hasStackTrace
        HasFrequencyInfo  = $hasFrequencyInfo
        HasWorkaroundInfo = $hasWorkaroundInfo
        BodyLength      = $bodyLength
        Flags           = $flags
        Suggestions     = $suggestions
        URL             = $issue.url
    }

    # Stop once we have enough matching issues
    if ($processedIssues.Count -ge $Limit) { break }
}
Write-Host "" # New line after progress

if ($processedIssues.Count -eq 0) {
    Write-Host "No issues matched the grooming criteria." -ForegroundColor Yellow
    exit 0
}

Write-Host "  $($processedIssues.Count) issues need attention" -ForegroundColor Green

# ── Output formatters ──────────────────────────────────────────────────────────

function Format-Table-Output {
    param($issues)

    Write-Host ""
    Write-Host "Backlog Issues Needing Grooming" -ForegroundColor Cyan
    Write-Host ("=" * 110)

    $issues | Format-Table -Property @(
        @{Label = "Issue";   Expression = { $_.Number }; Width = 7 },
        @{Label = "Title";   Expression = { $_.Title }; Width = 45 },
        @{Label = "Age";     Expression = { $_.Age }; Width = 10 },
        @{Label = "Updated"; Expression = { "$($_.DaysSinceUpdate)d ago" }; Width = 10 },
        @{Label = "Flags";   Expression = { ($_.Flags -join ", ") }; Width = 35 }
    ) -AutoSize
}

function Format-Markdown-Output {
    param($issues)

    $output = @()
    $output += "# Backlog Grooming Report"
    $output += ""
    $output += "Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
    $output += "Query: GroomType=$GroomType, Platform=$Platform, Area=$Area"
    $output += ""

    # Group by flag for easier reading
    $grouped = @{}
    foreach ($issue in $issues) {
        foreach ($flag in $issue.Flags) {
            if (-not $grouped.ContainsKey($flag)) { $grouped[$flag] = @() }
            $grouped[$flag] += $issue
        }
    }

    foreach ($flag in @("possibly-fixed", "very-stale", "stale", "needs-repro", "weak-repro", "no-platform", "no-area", "no-milestone")) {
        if (-not $grouped.ContainsKey($flag)) { continue }
        $flagIssues = $grouped[$flag]

        $flagEmoji = switch ($flag) {
            "possibly-fixed" { "✅" }
            "very-stale"     { "💀" }
            "stale"          { "😴" }
            "needs-repro"    { "🔍" }
            "weak-repro"     { "📝" }
            "no-platform"    { "🏷️" }
            "no-area"        { "🏷️" }
            "no-milestone"   { "📌" }
            default          { "❓" }
        }

        $output += "## $flagEmoji $flag ($($flagIssues.Count) issues)"
        $output += ""
        $output += "| Issue | Title | Age | Last Updated | Milestone |"
        $output += "|-------|-------|-----|--------------|-----------|"

        foreach ($issue in $flagIssues) {
            $issueLink = "[#$($issue.Number)]($($issue.URL))"
            $ms = if ($issue.Milestone) { $issue.Milestone } else { "_none_" }
            $output += "| $issueLink | $($issue.FullTitle -replace '\|', '\|') | $($issue.Age) | $($issue.DaysSinceUpdate)d ago | $ms |"
        }
        $output += ""
    }

    $output += "---"
    $output += "Total: $($issues.Count) issues across $($grouped.Keys.Count) categories"

    return $output -join "`n"
}

function Format-Groom-Output {
    param($issues)

    foreach ($issue in $issues) {
        Write-Output "==="
        Write-Output "Number:$($issue.Number)"
        Write-Output "Title:$($issue.FullTitle)"
        Write-Output "URL:$($issue.URL)"
        Write-Output "Author:$($issue.Author)"
        Write-Output "Platform:$($issue.Platform)"
        Write-Output "Areas:$($issue.Areas)"
        Write-Output "Age:$($issue.Age)"
        Write-Output "DaysSinceUpdate:$($issue.DaysSinceUpdate)"
        Write-Output "Milestone:$($issue.Milestone)"
        Write-Output "Labels:$($issue.Labels)"
        Write-Output "HasRepro:$($issue.HasRepro)"
        Write-Output "HasReproLink:$($issue.HasReproLink)"
        Write-Output "HasExpectedActual:$($issue.HasExpectedActual)"
        Write-Output "HasPlatformVersionDetails:$($issue.HasPlatformVersionDetails)"
        Write-Output "HasDotNetVersion:$($issue.HasDotNetVersion)"
        Write-Output "HasRegressionInfo:$($issue.HasRegressionInfo)"
        Write-Output "HasStackTrace:$($issue.HasStackTrace)"
        Write-Output "HasFrequencyInfo:$($issue.HasFrequencyInfo)"
        Write-Output "HasWorkaroundInfo:$($issue.HasWorkaroundInfo)"
        Write-Output "BodyLength:$($issue.BodyLength)"

        if ($issue.LinkedPRs -and $issue.LinkedPRs.Count -gt 0) {
            $prList = ($issue.LinkedPRs | ForEach-Object {
                $prInfo = "#$($_.Number)[$($_.State)]"
                if ($_.Milestone) { $prInfo += "(MS:$($_.Milestone))" }
                $prInfo
            }) -join "; "
            Write-Output "LinkedPRs:$prList"
        }

        Write-Output "CommentCount:$($issue.CommentCount)"
        Write-Output "Flags:$(($issue.Flags -join ', '))"
        Write-Output "Suggestions:"
        foreach ($s in $issue.Suggestions) {
            Write-Output "  - $s"
        }
    }
}

function Format-Json-Output {
    param($issues)
    return $issues | ConvertTo-Json -Depth 10
}

# ── Generate output ────────────────────────────────────────────────────────────
switch ($OutputFormat) {
    "table" {
        Format-Table-Output -issues $processedIssues
        $outputContent = $null
    }
    "markdown" {
        $outputContent = Format-Markdown-Output -issues $processedIssues
        Write-Host $outputContent
    }
    "groom" {
        Format-Groom-Output -issues $processedIssues
        $outputContent = $null
    }
    "json" {
        # Emit JSON on the success stream (Write-Output) so callers can capture it
        $processedIssues | ConvertTo-Json -Depth 10 | Write-Output
        $outputContent = $null
    }
}

# Save to file if requested
if ($OutputFile -ne "" -and $outputContent) {
    $outputContent | Out-File -FilePath $OutputFile -Encoding UTF8
    Write-Host ""
    Write-Host "Results saved to: $OutputFile" -ForegroundColor Green
}

# ── Summary statistics (suppress for json/groom — callers parse output) ────────
if ($OutputFormat -notin @("json", "groom")) {
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor Cyan

    $flagStats = @{}
    foreach ($issue in $processedIssues) {
        foreach ($flag in $issue.Flags) {
            if (-not $flagStats.ContainsKey($flag)) { $flagStats[$flag] = 0 }
            $flagStats[$flag]++
        }
    }

    foreach ($flag in $flagStats.Keys | Sort-Object) {
        $emoji = switch ($flag) {
            "possibly-fixed" { "✅" }
            "very-stale"     { "💀" }
            "stale"          { "😴" }
            "needs-repro"    { "🔍" }
            "weak-repro"     { "📝" }
            "no-platform"    { "🏷️" }
            "no-area"        { "🏷️" }
            "no-milestone"   { "📌" }
            default          { "❓" }
        }
        Write-Host "  $emoji $flag`: $($flagStats[$flag]) issues"
    }

    if ($processedIssues.Count -gt 0) {
        $avgAge = [Math]::Round(($processedIssues | Measure-Object AgeDays -Average).Average)
        $avgUpdate = [Math]::Round(($processedIssues | Measure-Object DaysSinceUpdate -Average).Average)
        Write-Host "  Average age: $avgAge days"
        Write-Host "  Average since last update: $avgUpdate days"
    }
}

# Return for pipeline usage
return $processedIssues
