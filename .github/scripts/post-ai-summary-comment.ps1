#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates the AI review summary comment on a GitHub Pull Request.

.DESCRIPTION
    Creates ONE comment for the PR review with phases wrapped in expandable sections.
    Uses HTML marker <!-- AI Summary --> for identification.
    Always overwrites the existing comment (no session history).

    Content is auto-loaded from PRAgent phase files:
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/{pre-flight,try-fix,report}/content.md

    Gate is posted separately by post-gate-comment.ps1.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    ./post-ai-summary-comment.ps1 -PRNumber 12345

.EXAMPLE
    ./post-ai-summary-comment.ps1 -PRNumber 12345 -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$MARKER = "<!-- AI Summary -->"

# ============================================================================
# LOAD PHASE CONTENT
# ============================================================================

Write-Host "ℹ️  Loading phase content for PR #$PRNumber..." -ForegroundColor Cyan

$PRAgentDir = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
if (-not (Test-Path $PRAgentDir)) {
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($repoRoot) {
        $PRAgentDir = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    }
}

if (-not (Test-Path $PRAgentDir)) {
    throw "No PRAgent directory found at: $PRAgentDir"
}

$phases = [ordered]@{
    "pre-flight" = @{ File = "pre-flight/content.md"; Icon = "🔍"; Title = "Pre-Flight — Context & Validation" }
    "try-fix"    = @{ File = "try-fix/content.md";    Icon = "🔧"; Title = "Fix — Analysis & Comparison" }
    "report"     = @{ File = "report/content.md";     Icon = "📋"; Title = "Report — Final Recommendation" }
}

$phaseSections = @()
$statusRows = @()

foreach ($key in $phases.Keys) {
    $phase = $phases[$key]
    $filePath = Join-Path $PRAgentDir $phase.File

    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw -Encoding UTF8
        if (-not [string]::IsNullOrWhiteSpace($content)) {
            Write-Host "  ✅ $key ($((Get-Item $filePath).Length) bytes)" -ForegroundColor Green
            $statusRows += "| $($phase.Title -replace ' —.*','') | ✅ COMPLETE |"
            $phaseSections += @"
<details>
<summary><strong>$($phase.Icon) $($phase.Title)</strong></summary>

---

$content

</details>
"@
        } else {
            Write-Host "  ⏭️  $key (empty)" -ForegroundColor Gray
            $statusRows += "| $($phase.Title -replace ' —.*','') | ⏳ PENDING |"
        }
    } else {
        Write-Host "  ⏭️  $key (not found)" -ForegroundColor Gray
        $statusRows += "| $($phase.Title -replace ' —.*','') | ⏳ PENDING |"
    }
}

if ($phaseSections.Count -eq 0) {
    throw "No phase content found. Ensure at least one content.md exists in $PRAgentDir."
}

# ============================================================================
# BUILD COMMENT BODY
# ============================================================================

# Get latest commit info for the summary header
$commitJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' 2>$null | ConvertFrom-Json
$commitTitle = if ($commitJson) { ($commitJson.message -split "`n")[0] } else { "Unknown" }
$commitSha = if ($commitJson) { $commitJson.sha.Substring(0, 7) } else { "unknown" }
$commitUrl = if ($commitJson) { "https://github.com/dotnet/maui/commit/$($commitJson.sha)" } else { "#" }

$statusTable = "| Phase | Status |`n|-------|--------|`n$($statusRows -join "`n")"
$phaseContent = $phaseSections -join "`n`n---`n`n"

$commentBody = @"
$MARKER

## 🤖 AI Summary

<!-- SECTION:PR-REVIEW -->
<details>
<summary>📊 <strong>Expand Full Review</strong> — <a href="$commitUrl"><code>$commitSha</code></a> · <strong>$commitTitle</strong></summary>

---

$statusTable

---

$phaseContent

---

</details>
<!-- /SECTION:PR-REVIEW -->
"@

Write-Host "  ✅ Built comment ($($commentBody.Length) chars)" -ForegroundColor Green

# ============================================================================
# DRY RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "=== COMMENT PREVIEW ===" -ForegroundColor Cyan
    Write-Host $commentBody
    Write-Host "=== END PREVIEW ===" -ForegroundColor Cyan
    exit 0
}

# ============================================================================
# POST OR UPDATE COMMENT
# ============================================================================

Write-Host "Checking for existing review comment..." -ForegroundColor Yellow
$existingCommentId = gh api "repos/dotnet/maui/issues/$PRNumber/comments" `
    --jq ".[] | select(.body | contains(`"$MARKER`")) | .id" 2>$null | Select-Object -First 1

$tempFile = [System.IO.Path]::GetTempFileName()
try {
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

    if ($existingCommentId) {
        Write-Host "✓ Found existing comment (ID: $existingCommentId) — updating..." -ForegroundColor Green
        try {
            gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input $tempFile 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "PATCH failed" }
            Write-Host "✅ Review comment updated" -ForegroundColor Green
            Write-Output "COMMENT_ID=$existingCommentId"
        } catch {
            Write-Host "⚠️ Could not update — creating new: $_" -ForegroundColor Yellow
            $newJson = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile
            $newId = ($newJson | ConvertFrom-Json).id
            Write-Host "✅ Review comment posted (ID: $newId)" -ForegroundColor Green
            Write-Output "COMMENT_ID=$newId"
        }
    } else {
        Write-Host "Creating new review comment..." -ForegroundColor Yellow
        $newJson = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile
        $newId = ($newJson | ConvertFrom-Json).id
        Write-Host "✅ Review comment posted (ID: $newId)" -ForegroundColor Green
        Write-Output "COMMENT_ID=$newId"
    }
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
