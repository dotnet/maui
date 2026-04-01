#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates a batch backlog grooming report by assessing multiple issues.

.DESCRIPTION
    This script runs assess-issue.ps1 against a batch of issues (from query-backlog.ps1
    or specified manually) and produces a comprehensive markdown report grouped by
    grooming category.

    The report is saved to CustomAgentLogsTmp/BacklogGroom/ and can be used for
    team review or as input to the GitHub Action for automated grooming.

.PARAMETER IssueNumbers
    Comma-separated list of issue numbers to assess. If not provided, runs query-backlog.ps1.

.PARAMETER GroomType
    Passed to query-backlog.ps1 when auto-querying: "stale", "needs-repro", "possibly-fixed", "missing-labels", "all"

.PARAMETER Platform
    Passed to query-backlog.ps1 when auto-querying

.PARAMETER Limit
    Maximum issues to assess (default: 25)

.PARAMETER OutputDir
    Directory for the report (default: CustomAgentLogsTmp/BacklogGroom)

.PARAMETER ReportName
    Name for the report file (default: timestamp-based)

.EXAMPLE
    ./generate-report.ps1
    # Auto-query and assess 25 issues, produce report

.EXAMPLE
    ./generate-report.ps1 -GroomType stale -Limit 50
    # Report on 50 stale issues

.EXAMPLE
    ./generate-report.ps1 -IssueNumbers "12345,67890,11111"
    # Assess specific issues
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$IssueNumbers = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("stale", "needs-repro", "possibly-fixed", "missing-labels", "all")]
    [string]$GroomType = "all",

    [Parameter(Mandatory = $false)]
    [ValidateSet("android", "ios", "windows", "maccatalyst", "all")]
    [string]$Platform = "all",

    [Parameter(Mandatory = $false)]
    [int]$Limit = 25,

    [Parameter(Mandatory = $false)]
    [string]$OutputDir = "CustomAgentLogsTmp/BacklogGroom",

    [Parameter(Mandatory = $false)]
    [string]$ReportName = ""
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot

# ── Prerequisites ──────────────────────────────────────────────────────────────
try {
    $null = Get-Command gh -ErrorAction Stop
} catch {
    Write-Host "❌ GitHub CLI (gh) is not installed" -ForegroundColor Red
    exit 1
}

$authStatus = gh auth status 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ GitHub CLI (gh) is not authenticated. Run: gh auth login" -ForegroundColor Red
    exit 1
}

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           Backlog Grooming Report Generator               ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Create output directory
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

if ($ReportName -eq "") {
    $ReportName = "groom-report-$(Get-Date -Format 'yyyy-MM-dd-HHmm')"
}

$reportFile = Join-Path $OutputDir "$ReportName.md"
$jsonFile = Join-Path $OutputDir "$ReportName.json"

# ── Gather issue numbers ──────────────────────────────────────────────────────
$issuesToAssess = @()

if ($IssueNumbers -ne "") {
    # Manual list
    $issuesToAssess = $IssueNumbers -split "," | ForEach-Object { [int]$_.Trim() }
    Write-Host ""
    Write-Host "Assessing $($issuesToAssess.Count) specified issues..." -ForegroundColor Cyan
}
else {
    # Auto-query using query-backlog.ps1 — capture returned objects directly
    Write-Host ""
    Write-Host "Querying backlog for issues to groom..." -ForegroundColor Cyan

    $queryScript = Join-Path $scriptDir "query-backlog.ps1"
    $queryIssues = & $queryScript -GroomType $GroomType -Platform $Platform -Limit $Limit -OutputFormat "groom" 2>$null

    if ($null -eq $queryIssues -or @($queryIssues).Count -eq 0) {
        Write-Host "No issues found to assess." -ForegroundColor Yellow
        exit 0
    }

    $issuesToAssess = @($queryIssues) | ForEach-Object { $_.Number }
}

if ($issuesToAssess.Count -eq 0) {
    Write-Host "No issues to assess." -ForegroundColor Yellow
    exit 0
}

Write-Host "  Found $($issuesToAssess.Count) issues to assess" -ForegroundColor Green

# ── Assess each issue ────────────────────────────────────────────────────────
$assessScript = Join-Path $scriptDir "assess-issue.ps1"
$assessments = @()
$issueIndex = 0
$failedIssues = @()

foreach ($issueNum in $issuesToAssess) {
    $issueIndex++
    Write-Host "`r  [$issueIndex/$($issuesToAssess.Count)] Assessing #$issueNum..." -NoNewline -ForegroundColor DarkGray

    try {
        $result = & $assessScript -IssueNumber $issueNum -OutputFormat "json" 2>$null
        if ($result) {
            $assessment = $result | ConvertFrom-Json
            $assessments += $assessment
        }
    }
    catch {
        $failedIssues += $issueNum
        Write-Warning "Failed to assess #$issueNum`: $_"
    }

    # Rate limiting: small delay between API calls
    Start-Sleep -Milliseconds 500
}

Write-Host "" # New line after progress
Write-Host "  Assessed $($assessments.Count) issues ($($failedIssues.Count) failed)" -ForegroundColor Green

if ($assessments.Count -eq 0) {
    Write-Host "No assessments produced." -ForegroundColor Yellow
    exit 0
}

# ── Generate markdown report ─────────────────────────────────────────────────
Write-Host ""
Write-Host "Generating report..." -ForegroundColor Cyan

$report = @()
$report += "# 📋 Backlog Grooming Report"
$report += ""
$report += "**Generated:** $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$report += "**Scope:** GroomType=$GroomType, Platform=$Platform, Assessed=$($assessments.Count) issues"
$report += ""

# Summary statistics
$gradeDistribution = $assessments | Group-Object HealthGrade | Sort-Object Name
$avgHealth = [Math]::Round(($assessments | Measure-Object HealthScore -Average).Average)

$report += "## 📊 Summary"
$report += ""
$report += "| Metric | Value |"
$report += "|--------|-------|"
$report += "| Issues assessed | $($assessments.Count) |"
$report += "| Average health score | $avgHealth/100 |"
$report += "| Failed assessments | $($failedIssues.Count) |"
$report += ""

$report += "### Health Distribution"
$report += ""
$report += "| Grade | Count | Description |"
$report += "|-------|-------|-------------|"
foreach ($grade in @("A", "B", "C", "D", "F")) {
    $count = ($gradeDistribution | Where-Object { $_.Name -eq $grade })?.Count ?? 0
    $desc = switch ($grade) {
        "A" { "Healthy - no action needed" }
        "B" { "Minor issues" }
        "C" { "Needs attention" }
        "D" { "Significant issues" }
        "F" { "Critical - immediate action needed" }
    }
    $report += "| $grade | $count | $desc |"
}
$report += ""

# Group by flag category
$flagGroups = @{}
foreach ($assessment in $assessments) {
    foreach ($flag in $assessment.Flags) {
        if (-not $flagGroups.ContainsKey($flag)) { $flagGroups[$flag] = @() }
        $flagGroups[$flag] += $assessment
    }
}

$report += "### Flag Summary"
$report += ""
$report += "| Flag | Count | Description |"
$report += "|------|-------|-------------|"

$flagDescriptions = @{
    "possibly-fixed" = "Has merged PR(s) — may already be fixed"
    "very-stale"     = "No activity for 1+ year"
    "stale"          = "No activity for 6+ months"
    "needs-repro"    = "Missing reproduction steps"
    "weak-repro"     = "Has description but no code/steps"
    "no-platform"    = "Missing platform label"
    "no-area"        = "Missing area label"
    "no-milestone"   = "No milestone assigned"
}

foreach ($flag in @("possibly-fixed", "very-stale", "stale", "needs-repro", "weak-repro", "no-platform", "no-area", "no-milestone")) {
    if (-not $flagGroups.ContainsKey($flag)) { continue }
    $count = $flagGroups[$flag].Count
    $desc = $flagDescriptions[$flag]
    $emoji = switch ($flag) {
        "possibly-fixed" { "✅" }
        "very-stale"     { "💀" }
        "stale"          { "😴" }
        "needs-repro"    { "🔍" }
        "weak-repro"     { "📝" }
        "no-platform"    { "🏷️" }
        "no-area"        { "🏷️" }
        "no-milestone"   { "📌" }
    }
    $report += "| $emoji $flag | $count | $desc |"
}
$report += ""

# Detailed sections per flag
foreach ($flag in @("possibly-fixed", "very-stale", "stale", "needs-repro", "weak-repro", "no-platform", "no-area")) {
    if (-not $flagGroups.ContainsKey($flag)) { continue }
    $flagIssues = $flagGroups[$flag]

    $emoji = switch ($flag) {
        "possibly-fixed" { "✅" }
        "very-stale"     { "💀" }
        "stale"          { "😴" }
        "needs-repro"    { "🔍" }
        "weak-repro"     { "📝" }
        "no-platform"    { "🏷️" }
        "no-area"        { "🏷️" }
    }

    $report += "## $emoji $($flag.ToUpper()) ($($flagIssues.Count) issues)"
    $report += ""
    $report += "| Issue | Title | Health | Age | Last Update | Milestone | Suggested Action |"
    $report += "|-------|-------|--------|-----|-------------|-----------|------------------|"

    foreach ($issue in $flagIssues | Sort-Object HealthScore) {
        $issueLink = "[#$($issue.Number)]($($issue.URL))"
        $ms = if ($issue.Milestone) { $issue.Milestone } else { "_none_" }
        $grade = "$($issue.HealthGrade) ($($issue.HealthScore))"

        # Pick most relevant suggestion
        $suggestion = ""
        if ($issue.Actions -and $issue.Actions.Count -gt 0) {
            $firstAction = $issue.Actions[0]
            if ($firstAction.Type -eq "label") { $suggestion = "Add ``$($firstAction.Add)``" }
            elseif ($firstAction.Type -eq "comment") {
                $preview = $firstAction.Body
                if ($preview.Length -gt 60) { $preview = $preview.Substring(0, 57) + "..." }
                $suggestion = $preview
            }
        }

        $report += "| $issueLink | $($issue.Title -replace '\|', '\|') | $grade | $($issue.AgeDays)d | $($issue.DaysSinceUpdate)d ago | $ms | $suggestion |"
    }
    $report += ""
}

# Action items summary
$actionableIssues = $assessments | Where-Object { $_.Actions -and $_.Actions.Count -gt 0 }
if ($actionableIssues.Count -gt 0) {
    $report += "## 🎯 Recommended Actions"
    $report += ""
    $report += "These issues have specific recommended actions:"
    $report += ""

    foreach ($issue in $actionableIssues | Sort-Object HealthScore) {
        $report += "### [#$($issue.Number)]($($issue.URL)) — $($issue.Title)"
        $report += ""
        $report += "- **Health:** $($issue.HealthGrade) ($($issue.HealthScore)/100)"
        $report += "- **Flags:** $($issue.Flags -join ', ')"
        $report += "- **Actions:**"
        foreach ($action in $issue.Actions) {
            if ($action.Type -eq "label") {
                $report += "  - 🏷️ Add label: ``$($action.Add)``"
            }
            elseif ($action.Type -eq "comment") {
                $report += "  - 💬 Post comment asking for update"
            }
        }
        $report += ""
    }
}

$report += "---"
$report += ""
$report += "_Report generated by backlog-groom skill. Review before applying actions._"

# ── Save report ───────────────────────────────────────────────────────────────
$reportContent = $report -join "`n"
$reportContent | Out-File -FilePath $reportFile -Encoding UTF8

# Save JSON for programmatic use
$assessments | ConvertTo-Json -Depth 10 | Out-File -FilePath $jsonFile -Encoding UTF8

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║           Report Generated Successfully                   ║" -ForegroundColor Green
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "  📄 Markdown: $reportFile" -ForegroundColor White
Write-Host "  📊 JSON:     $jsonFile" -ForegroundColor White
Write-Host ""
Write-Host "  Issues assessed:    $($assessments.Count)" -ForegroundColor White
Write-Host "  Actionable issues:  $($actionableIssues.Count)" -ForegroundColor White
Write-Host "  Average health:     $avgHealth/100" -ForegroundColor White
Write-Host ""

# Return assessments for pipeline usage
return $assessments
