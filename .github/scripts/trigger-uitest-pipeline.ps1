#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Detects UI test categories, queues AzDO pipeline, monitors, and posts results.

.DESCRIPTION
    Orchestrates the full UI test flow for a PR:
    1. Read AI-suggested categories from uitests/ai-categories.md (if available)
    2. Detect categories via source-path mapping + test file analysis
    3. Queue maui-pr-uitests pipeline (def 313) on AzDO with detected categories
    4. Poll until build completes (timeout 3h)
    5. Post results comment via post-uitest-categories-comment.ps1
    6. Write uitests/content.md for AI summary integration

.PARAMETER PRNumber
    The GitHub PR number (required).

.PARAMETER SourceBranch
    AzDO pipeline source branch. Defaults to the PR merge ref (pull/N/merge).

.PARAMETER DryRun
    Show what would be done without queuing or posting.

.PARAMETER SkipMonitor
    Queue the build but don't wait for it to complete.

.EXAMPLE
    ./trigger-uitest-pipeline.ps1 -PRNumber 35015
    ./trigger-uitest-pipeline.ps1 -PRNumber 35015 -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$SourceBranch,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$SkipMonitor
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) { $RepoRoot = $PSScriptRoot | Split-Path | Split-Path }

$AzdoOrg = "dnceng-public"
$AzdoProject = "public"
$PipelineDefId = 313
$PollIntervalSec = 120
$TimeoutMinutes = 180
$OutputDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/uitests"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  UI TEST PIPELINE — PR #$PRNumber                              ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Default SourceBranch to the PR's merge ref so tests run the PR's actual code
if ([string]::IsNullOrWhiteSpace($SourceBranch)) {
    $SourceBranch = "pull/$PRNumber/merge"
    Write-Host "  📌 Source branch: refs/$SourceBranch (PR merge ref)" -ForegroundColor Cyan
}

# ============================================================================
# STEP 1: Read AI categories (Tier 3) from pre-flight output
# ============================================================================

$aiCategories = $null
$aiCategoriesFile = Join-Path $OutputDir "ai-categories.md"
if (Test-Path $aiCategoriesFile) {
    $aiContent = Get-Content $aiCategoriesFile -Raw -Encoding UTF8
    if (-not [string]::IsNullOrWhiteSpace($aiContent) -and $aiContent.Trim() -ne 'NONE') {
        # Extract category names (lines like "Button — justification")
        $aiCatLines = @($aiContent -split "`n" | ForEach-Object {
            if ($_ -match '^([A-Za-z]+)') { $Matches[1] }
        } | Where-Object { $_ })
        if ($aiCatLines.Count -gt 0) {
            $aiCategories = $aiCatLines -join ','
            Write-Host "  🤖 AI categories (from pre-flight): $aiCategories" -ForegroundColor Green
        }
    }
} else {
    Write-Host "  ℹ️ No AI categories file found (pre-flight may not have run yet)" -ForegroundColor DarkGray
}

# ============================================================================
# STEP 2: Detect categories using the detect script (Tier 1 + 2 + 3)
# ============================================================================

Write-Host "  🔍 Running category detection for PR #$PRNumber..." -ForegroundColor Cyan

$detectScript = Join-Path $RepoRoot "eng/scripts/detect-ui-test-categories.ps1"
$detectArgs = @("-PrNumber", "$PRNumber")
if ($aiCategories) { $detectArgs += @("-AiCategories", $aiCategories) }

# Capture the output to extract detected categories
$detectOutput = & pwsh -NoProfile -File $detectScript @detectArgs 2>&1
$detectOutput | ForEach-Object { Write-Host "    $_" }

# Parse detected categories from the script output
$detectedCategories = $null
foreach ($line in $detectOutput) {
    $lineStr = $line.ToString()
    if ($lineStr -match 'UITestCategoryList;isOutput=true\](.+)$') {
        $detectedCategories = $Matches[1]
    }
}

if ([string]::IsNullOrWhiteSpace($detectedCategories)) {
    Write-Host "  📦 No specific categories detected — full matrix would run" -ForegroundColor Yellow
    $detectedCategories = $null
} elseif ($detectedCategories -eq 'NONE') {
    Write-Host "  ⏭️ No UI-relevant changes detected — skipping UI test pipeline" -ForegroundColor Yellow

    # Write skip result
    @"
### UI Tests: ⏭️ Skipped

No UI-relevant changes detected for this PR. UI test pipeline was not triggered.
"@ | Set-Content (Join-Path $OutputDir "content.md") -Encoding UTF8

    Write-Output "UITEST_RESULT=SKIPPED"
    Write-Output "UITEST_CATEGORIES=NONE"
    return
} else {
    Write-Host "  🎯 Detected categories: $detectedCategories" -ForegroundColor Green
}

# ============================================================================
# STEP 3: Queue AzDO build
# ============================================================================

Write-Host "  🚀 Queueing maui-pr-uitests pipeline..." -ForegroundColor Cyan

$templateParams = @{ prNumber = "$PRNumber" }
if ($detectedCategories) {
    $templateParams['categories'] = $detectedCategories
}

$sourceBranchRef = if ($SourceBranch -like "pull/*") { "refs/$SourceBranch" } else { "refs/heads/$SourceBranch" }

$buildBody = @{
    definition = @{ id = $PipelineDefId }
    sourceBranch = $sourceBranchRef
    templateParameters = $templateParams
} | ConvertTo-Json -Depth 5

if ($DryRun) {
    Write-Host "  [DRY RUN] Would queue build with:" -ForegroundColor Magenta
    Write-Host "    Source: $SourceBranch" -ForegroundColor Gray
    Write-Host "    Categories: $($detectedCategories ?? 'all')" -ForegroundColor Gray
    Write-Host "    PR: $PRNumber" -ForegroundColor Gray

    @"
### UI Tests: 🏗️ Dry Run

**Detected categories:** ``$($detectedCategories ?? 'all')``
**Source branch:** ``$SourceBranch``

Build was not queued (dry run mode).
"@ | Set-Content (Join-Path $OutputDir "content.md") -Encoding UTF8

    Write-Output "UITEST_RESULT=DRYRUN"
    Write-Output "UITEST_CATEGORIES=$($detectedCategories ?? 'all')"
    return
}

# Queue via AzDO REST API
$build = $null
$queueMethod = "none"

$azdoPat = $env:AZURE_DEVOPS_EXT_PAT
if ($azdoPat -and $azdoPat -notmatch '^\$\(' -and $azdoPat.Length -ge 40) {
    $patLen = $azdoPat.Length
    $maskedPat = $azdoPat.Substring(0, [Math]::Min(4, $patLen)) + "****"
    Write-Host "  📡 Queueing via AzDO REST API (Basic auth, token: $maskedPat, length: $patLen)..." -ForegroundColor Gray
    $ErrorActionPreference = "Continue"
    try {
        $base64 = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes(":$azdoPat"))
        $headers = @{
            Authorization  = "Basic $base64"
            'Content-Type' = 'application/json'
        }
        $queueUrl = "https://dev.azure.com/$AzdoOrg/$AzdoProject/_apis/build/builds?api-version=7.1"
        Write-Host "    URL: $queueUrl" -ForegroundColor DarkGray
        $response = Invoke-RestMethod -Uri $queueUrl -Method POST -Headers $headers -Body $buildBody -ErrorAction Stop
        if ($response.id) {
            $build = $response
            $queueMethod = "rest-api"
            Write-Host "  ✅ Build queued via REST API (build #$($response.id))" -ForegroundColor Green
        }
    } catch {
        $errMsg = $_.Exception.Message
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  ❌ REST API failed (HTTP $statusCode): $errMsg" -ForegroundColor Red
        if ($_.ErrorDetails.Message) {
            Write-Host "    Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
        }
    }
    $ErrorActionPreference = "Stop"
} else {
    Write-Host "  ⚠️ AZURE_DEVOPS_EXT_PAT not set, unresolved, or too short (need PAT ≥40 chars)" -ForegroundColor Yellow
    if ($azdoPat) { Write-Host "    (got length $($azdoPat.Length) — expected ≥40 for a PAT. Check DNCENG_PUBLIC_PAT pipeline variable.)" -ForegroundColor Yellow }
}

if (-not $build -or -not $build.id) {
    Write-Host "  ❌ Could not queue build on $AzdoOrg/$AzdoProject" -ForegroundColor Red
    Write-Host "  💡 Detected categories: $($detectedCategories ?? 'all')" -ForegroundColor Yellow

    @"
### UI Tests: ❌ Failed to Queue

**Detected categories:** ``$($detectedCategories ?? 'all')``

Could not queue build on $AzdoOrg/$AzdoProject. Check DNCENG_PUBLIC_PAT pipeline variable.
"@ | Set-Content (Join-Path $OutputDir "content.md") -Encoding UTF8

    Write-Output "UITEST_RESULT=QUEUE_FAILED"
    Write-Output "UITEST_CATEGORIES=$($detectedCategories ?? 'all')"
    return
}

Write-Host "  📡 Queued via: $queueMethod" -ForegroundColor Gray

$buildId = $build.id
$buildUrl = "https://dev.azure.com/$AzdoOrg/$AzdoProject/_build/results?buildId=$buildId"
Write-Host "  ✅ Build queued: #$buildId" -ForegroundColor Green
Write-Host "  🔗 $buildUrl" -ForegroundColor Cyan

if ($SkipMonitor) {
    Write-Host "  ⏭️ Skipping monitoring (SkipMonitor flag)" -ForegroundColor Yellow

    @"
### UI Tests: 🔄 In Progress

**Detected categories:** ``$($detectedCategories ?? 'all')``
**Build:** [#$buildId]($buildUrl) — queued, not yet complete

Monitoring was skipped. Check the build link for results.
"@ | Set-Content (Join-Path $OutputDir "content.md") -Encoding UTF8

    Write-Output "UITEST_RESULT=QUEUED"
    Write-Output "UITEST_BUILD_ID=$buildId"
    Write-Output "UITEST_CATEGORIES=$($detectedCategories ?? 'all')"
    return
}

# ============================================================================
# STEP 4: Monitor build until completion
# ============================================================================

Write-Host "  ⏳ Monitoring build #$buildId (poll every ${PollIntervalSec}s, timeout ${TimeoutMinutes}m)..." -ForegroundColor Cyan

$startTime = Get-Date
$buildResult = $null

while ($true) {
    $elapsed = (Get-Date) - $startTime
    if ($elapsed.TotalMinutes -gt $TimeoutMinutes) {
        Write-Host "  ⚠️ Timeout after $TimeoutMinutes minutes" -ForegroundColor Yellow
        $buildResult = "TIMEOUT"
        break
    }

    try {
        $status = Invoke-RestMethod -Uri "https://dev.azure.com/$AzdoOrg/$AzdoProject/_apis/build/builds/${buildId}?api-version=7.1"
        if ($status.status -eq 'completed') {
            $buildResult = $status.result
            Write-Host "  ✅ Build completed: $buildResult ($([math]::Round($elapsed.TotalMinutes, 1))m)" -ForegroundColor $(if ($buildResult -eq 'succeeded') { 'Green' } else { 'Yellow' })
            break
        }
        Write-Host "    [$($elapsed.ToString('hh\:mm\:ss'))] $($status.status)..." -ForegroundColor DarkGray
    } catch {
        Write-Host "    [$($elapsed.ToString('hh\:mm\:ss'))] Poll error: $($_.Exception.Message)" -ForegroundColor DarkGray
    }

    Start-Sleep -Seconds $PollIntervalSec
}

# ============================================================================
# STEP 5: Post results comment
# ============================================================================

Write-Host "  📝 Posting results comment..." -ForegroundColor Cyan

$commentScript = Join-Path $PSScriptRoot "post-uitest-categories-comment.ps1"
if (Test-Path $commentScript) {
    try {
        $commentOutput = & $commentScript -PRNumber $PRNumber -BuildId $buildId
        $commentIdLine = $commentOutput | Where-Object { $_ -match '^COMMENT_ID=' } | Select-Object -Last 1
        if ($commentIdLine -match '^COMMENT_ID=(\d+)$') {
            Write-Host "  ✅ Comment posted (ID: $($Matches[1]))" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ⚠️ Failed to post comment (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ post-uitest-categories-comment.ps1 not found" -ForegroundColor Yellow
}

# ============================================================================
# STEP 6: Write content.md for AI summary
# ============================================================================

$resultIcon = switch ($buildResult) {
    "succeeded" { "✅" }
    "failed"    { "❌" }
    "TIMEOUT"   { "⏱️" }
    default     { "⚠️" }
}

@"
### UI Tests: $resultIcon $buildResult

**Detected categories:** ``$($detectedCategories ?? 'all')``
**Build:** [#$buildId]($buildUrl)
**Duration:** $([math]::Round(((Get-Date) - $startTime).TotalMinutes, 1)) minutes
"@ | Set-Content (Join-Path $OutputDir "content.md") -Encoding UTF8

Write-Output "UITEST_RESULT=$buildResult"
Write-Output "UITEST_BUILD_ID=$buildId"
Write-Output "UITEST_CATEGORIES=$($detectedCategories ?? 'all')"
