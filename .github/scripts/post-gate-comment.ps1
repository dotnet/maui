#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates the gate verification comment on a GitHub Pull Request.

.DESCRIPTION
    Creates a separate comment for the gate result, identified by <!-- AI Gate --> marker.
    Reads content from CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/gate/content.md.
    Updates existing gate comment if found, creates new one otherwise.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    ./post-gate-comment.ps1 -PRNumber 12345

.EXAMPLE
    ./post-gate-comment.ps1 -PRNumber 12345 -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$MARKER = "<!-- AI Gate -->"

# ============================================================================
# LOAD GATE CONTENT
# ============================================================================

$gateContentPath = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate/content.md"
if (-not (Test-Path $gateContentPath)) {
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($repoRoot) {
        $gateContentPath = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate/content.md"
    }
}

if (-not (Test-Path $gateContentPath)) {
    Write-Host "⚠️ No gate content found at: $gateContentPath" -ForegroundColor Yellow
    exit 0
}

$gateContent = Get-Content $gateContentPath -Raw -Encoding UTF8
if ([string]::IsNullOrWhiteSpace($gateContent)) {
    Write-Host "⚠️ Gate content is empty" -ForegroundColor Yellow
    exit 0
}

Write-Host "✅ Loaded gate content ($($gateContent.Length) chars)" -ForegroundColor Green

# ============================================================================
# BUILD COMMENT BODY
# ============================================================================

# Get latest commit info
try {
    $commitJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' 2>$null | ConvertFrom-Json
} catch {
    Write-Host "⚠️ Failed to fetch commit info: $_" -ForegroundColor Yellow
    $commitJson = $null
}
$commitTitle = if ($commitJson) { ($commitJson.message -split "`n")[0] } else { "Unknown" }
$commitSha = if ($commitJson) { $commitJson.sha.Substring(0, 7) } else { "unknown" }
$commitUrl = if ($commitJson) { "https://github.com/dotnet/maui/commit/$($commitJson.sha)" } else { "#" }

$commentBody = @"
$MARKER

## 🚦 Gate - Test Before and After Fix

<details>
<summary>📊 <strong>Expand Full Gate</strong> — <a href="$commitUrl"><code>$commitSha</code></a> · <strong>$commitTitle</strong></summary>

---

$gateContent

---

</details>
"@

# ============================================================================
# DRY RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "=== GATE COMMENT PREVIEW ===" -ForegroundColor Cyan
    Write-Host $commentBody
    Write-Host "=== END PREVIEW ===" -ForegroundColor Cyan
    exit 0
}

# ============================================================================
# POST OR UPDATE COMMENT
# ============================================================================

Write-Host "Checking for existing gate comment..." -ForegroundColor Yellow
$existingCommentId = gh api "repos/dotnet/maui/issues/$PRNumber/comments" `
    --jq ".[] | select(.body | contains(`"$MARKER`")) | .id" 2>$null | Select-Object -First 1

$tempFile = [System.IO.Path]::GetTempFileName()
try {
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

    if ($existingCommentId) {
        Write-Host "✓ Found existing gate comment (ID: $existingCommentId) — updating..." -ForegroundColor Green
        try {
            gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input $tempFile 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "PATCH failed" }
            Write-Host "✅ Gate comment updated" -ForegroundColor Green
            Write-Output "COMMENT_ID=$existingCommentId"
        } catch {
            Write-Host "⚠️ Could not update — creating new: $_" -ForegroundColor Yellow
            $newJson = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile
            $newId = ($newJson | ConvertFrom-Json).id
            Write-Host "✅ Gate comment posted (ID: $newId)" -ForegroundColor Green
            Write-Output "COMMENT_ID=$newId"
        }
    } else {
        Write-Host "Creating new gate comment..." -ForegroundColor Yellow
        $newJson = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile
        $newId = ($newJson | ConvertFrom-Json).id
        Write-Host "✅ Gate comment posted (ID: $newId)" -ForegroundColor Green
        Write-Output "COMMENT_ID=$newId"
    }
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
