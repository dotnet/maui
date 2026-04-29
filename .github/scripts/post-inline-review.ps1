#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts inline review comments on a GitHub Pull Request from a JSON findings file.

.DESCRIPTION
    Reads inline-findings.json (produced by the maui-expert-reviewer agent) and posts
    them as a GitHub PR review with inline file:line comments.

    Also posts review-summary.md as the review body.

    Uses the GitHub Pulls Review API to create a single review with all inline comments
    attached at their exact file:line locations.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER FindingsFile
    Path to inline-findings.json. Default: CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/inline-findings.json

.PARAMETER SummaryFile
    Path to review-summary.md. Default: CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/review-summary.md

.PARAMETER DryRun
    Print the review payload instead of posting

.EXAMPLE
    ./post-inline-review.ps1 -PRNumber 12345

.EXAMPLE
    ./post-inline-review.ps1 -PRNumber 12345 -DryRun

.EXAMPLE
    ./post-inline-review.ps1 -PRNumber 12345 -FindingsFile /tmp/findings.json
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$FindingsFile,

    [Parameter(Mandatory = $false)]
    [string]$SummaryFile,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

# ============================================================================
# RESOLVE FILE PATHS
# ============================================================================

$PRAgentDir = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
if (-not (Test-Path $PRAgentDir)) {
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($repoRoot) {
        $PRAgentDir = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    }
}

if (-not $FindingsFile) {
    $FindingsFile = Join-Path $PRAgentDir "inline-findings.json"
}
if (-not $SummaryFile) {
    $SummaryFile = Join-Path $PRAgentDir "review-summary.md"
}

if (-not (Test-Path $FindingsFile)) {
    Write-Host "No findings file found at: $FindingsFile" -ForegroundColor Yellow
    Write-Host "Nothing to post." -ForegroundColor Yellow
    exit 0
}

# ============================================================================
# LOAD FINDINGS
# ============================================================================

Write-Host "Loading findings from: $FindingsFile" -ForegroundColor Cyan
$findings = Get-Content -Path $FindingsFile -Raw -Encoding UTF8 | ConvertFrom-Json

if (-not $findings -or $findings.Count -eq 0) {
    Write-Host "No findings to post." -ForegroundColor Green
    exit 0
}

Write-Host "  Found $($findings.Count) inline findings" -ForegroundColor Gray

# Load summary if available
$summaryBody = ""
if (Test-Path $SummaryFile) {
    $summaryBody = Get-Content -Path $SummaryFile -Raw -Encoding UTF8
    Write-Host "  Loaded summary ($($summaryBody.Length) chars)" -ForegroundColor Gray
} else {
    $summaryBody = "## Expert Review — $($findings.Count) findings`n`nSee inline comments for details."
}

# ============================================================================
# GET PR HEAD COMMIT (required by GitHub API)
# ============================================================================

Write-Host "Fetching PR #$PRNumber head commit..." -ForegroundColor Cyan
$prJson = gh api "repos/dotnet/maui/pulls/$PRNumber" --jq '{sha: .head.sha}' 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Failed to fetch PR #$PRNumber: $prJson"
}
$prData = $prJson | ConvertFrom-Json
$commitSha = $prData.sha
Write-Host "  HEAD: $commitSha" -ForegroundColor Gray

# ============================================================================
# BUILD REVIEW PAYLOAD
# ============================================================================

$comments = @()
foreach ($f in $findings) {
    $comment = @{
        path = $f.path
        line = [int]$f.line
        body = $f.body
    }
    # GitHub API requires 'side' for pull request review comments
    $comment['side'] = 'RIGHT'
    $comments += $comment
}

$reviewPayload = @{
    commit_id = $commitSha
    body      = $summaryBody
    event     = "COMMENT"  # Never APPROVE or REQUEST_CHANGES — that's a human decision
    comments  = $comments
}

$payloadJson = $reviewPayload | ConvertTo-Json -Depth 10

# ============================================================================
# DRY RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  DRY RUN — Review preview (not posted)" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor Cyan
    Write-Host $summaryBody
    Write-Host ""
    Write-Host "Inline comments ($($comments.Count)):" -ForegroundColor Cyan
    foreach ($c in $comments) {
        Write-Host "  $($c.path):$($c.line)" -ForegroundColor White -NoNewline
        Write-Host " — $($c.body.Substring(0, [Math]::Min($c.body.Length, 120)))..." -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  Payload size: $($payloadJson.Length) chars" -ForegroundColor Gray
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    return
}

# ============================================================================
# POST REVIEW
# ============================================================================

Write-Host "Posting review with $($comments.Count) inline comments..." -ForegroundColor Cyan

$tempFile = [System.IO.Path]::GetTempFileName()
try {
    $payloadJson | Set-Content -Path $tempFile -Encoding UTF8

    $result = gh api --method POST "repos/dotnet/maui/pulls/$PRNumber/reviews" --input $tempFile 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to post review: $result"
    }

    $reviewData = $result | ConvertFrom-Json
    Write-Host "Review posted (ID: $($reviewData.id))" -ForegroundColor Green
    Write-Host "  $($comments.Count) inline comments at file:line" -ForegroundColor Gray
    Write-Host "  URL: $($reviewData.html_url)" -ForegroundColor Gray
} finally {
    Remove-Item -Path $tempFile -Force -ErrorAction SilentlyContinue
}
