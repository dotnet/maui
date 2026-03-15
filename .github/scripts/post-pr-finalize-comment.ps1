#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts the PR finalize summary into the unified AI Summary comment on a GitHub PR.

.DESCRIPTION
    Reads the pr-finalize-summary.md file and injects its content directly into the
    <!-- SECTION:PR-FINALIZE --> block of the unified AI Summary comment.

    No parsing or re-formatting — the summary file content is posted as-is inside
    a collapsible details block.

    Auto-discovers the summary file from:
      CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/pr-finalize/pr-finalize-summary.md

.PARAMETER PRNumber
    The PR number to post comment on (required)

.PARAMETER SummaryFile
    Path to pr-finalize-summary.md file. Auto-discovered from PRNumber if not provided.

.PARAMETER DryRun
    Print comment instead of posting

.PARAMETER PreviewFile
    Custom path for dry-run preview output

.EXAMPLE
    ./post-pr-finalize-comment.ps1 -PRNumber 34427

.EXAMPLE
    ./post-pr-finalize-comment.ps1 -PRNumber 34427 -DryRun
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$PRNumber,

    [Parameter(Mandatory=$false)]
    [string]$SummaryFile,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [string]$PreviewFile,

    # Legacy parameters — accepted but ignored for backward compatibility
    [Parameter(Mandatory=$false)][string]$TitleStatus,
    [Parameter(Mandatory=$false)][string]$CurrentTitle,
    [Parameter(Mandatory=$false)][string]$RecommendedTitle,
    [Parameter(Mandatory=$false)][string]$TitleIssues,
    [Parameter(Mandatory=$false)][string]$DescriptionStatus,
    [Parameter(Mandatory=$false)][string]$DescriptionAssessment,
    [Parameter(Mandatory=$false)][string]$MissingElements,
    [Parameter(Mandatory=$false)][string]$RecommendedDescription,
    [Parameter(Mandatory=$false)][string]$CodeReviewStatus,
    [Parameter(Mandatory=$false)][string]$CodeReviewFindings,
    [Parameter(Mandatory=$false)][switch]$Standalone
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  PR Finalize Comment (Post/Update)                        ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# ============================================================================
# AUTO-DISCOVERY
# ============================================================================

$repoRoot = git rev-parse --show-toplevel 2>$null

if ($PRNumber -gt 0 -and [string]::IsNullOrWhiteSpace($SummaryFile)) {
    $candidates = @(
        "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize/pr-finalize-summary.md"
    )
    if ($repoRoot) {
        $candidates += Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize/pr-finalize-summary.md"
    }
    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            $SummaryFile = $candidate
            Write-Host "ℹ️  Auto-discovered summary file: $SummaryFile" -ForegroundColor Cyan
            break
        }
    }
}

# Extract PRNumber from path if not provided
if ($PRNumber -eq 0 -and -not [string]::IsNullOrWhiteSpace($SummaryFile) -and $SummaryFile -match '[/\\](\d+)[/\\]') {
    $PRNumber = [int]$Matches[1]
    Write-Host "ℹ️  Auto-detected PRNumber: $PRNumber from path" -ForegroundColor Cyan
}

if ($PRNumber -eq 0) {
    throw "PRNumber is required. Provide via -PRNumber or use -SummaryFile with path containing PR number."
}

# ============================================================================
# LOAD SUMMARY CONTENT
# ============================================================================

if ([string]::IsNullOrWhiteSpace($SummaryFile) -or -not (Test-Path $SummaryFile)) {
    Write-Host "⚠️  No pr-finalize summary file found for PR #$PRNumber — nothing to post" -ForegroundColor Yellow
    exit 0
}

$summaryContent = (Get-Content $SummaryFile -Raw -Encoding UTF8).Trim()
if ([string]::IsNullOrWhiteSpace($summaryContent)) {
    Write-Host "⚠️  Summary file is empty — nothing to post" -ForegroundColor Yellow
    exit 0
}

Write-Host "  ✅ Loaded summary ($($summaryContent.Length) chars) from: $SummaryFile" -ForegroundColor Green

# ============================================================================
# BUILD SECTION
# ============================================================================

$MAIN_MARKER = "<!-- AI Summary -->"
$SECTION_START = "<!-- SECTION:PR-FINALIZE -->"
$SECTION_END = "<!-- /SECTION:PR-FINALIZE -->"

$finalizeSection = @"
$SECTION_START
<details>
<summary>📋 <strong>Expand PR Finalization Review</strong></summary>

$summaryContent

</details>
$SECTION_END
"@

# ============================================================================
# POST / UPDATE
# ============================================================================

Write-Host "`nInjecting into AI Summary comment on #$PRNumber..." -ForegroundColor Yellow

# Find existing unified comment
$existingUnifiedComment = $null
try {
    $commentsJson = gh api "repos/dotnet/maui/issues/$PRNumber/comments?per_page=100" 2>$null
    $comments = $commentsJson | ConvertFrom-Json
    foreach ($comment in $comments) {
        if ($comment.body -match [regex]::Escape($MAIN_MARKER)) {
            $existingUnifiedComment = $comment
            Write-Host "✓ Found unified AI Summary comment (ID: $($comment.id))" -ForegroundColor Green
            break
        }
    }
    if (-not $existingUnifiedComment) {
        Write-Host "✓ No existing AI Summary comment found — will create new" -ForegroundColor Yellow
    }
} catch {
    Write-Host "⚠️ Could not fetch comments: $_" -ForegroundColor Yellow
}

# Build the final comment body
if ($existingUnifiedComment) {
    $body = $existingUnifiedComment.body
    if ($body -match [regex]::Escape($SECTION_START)) {
        $pattern = [regex]::Escape($SECTION_START) + "[\s\S]*?" + [regex]::Escape($SECTION_END)
        $finalComment = $body -replace $pattern, $finalizeSection
    } else {
        $finalComment = $body.TrimEnd() + "`n`n" + $finalizeSection
    }
    $finalComment = $finalComment -replace "`n{4,}", "`n`n`n"
} else {
    $finalComment = @"
$MAIN_MARKER

## 🤖 AI Summary

$finalizeSection
"@
}

# DryRun: preview to file
if ($DryRun) {
    if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
        $PreviewFile = "CustomAgentLogsTmp/PRState/$PRNumber/ai-summary-comment-preview.md"
    }
    $previewDir = Split-Path $PreviewFile -Parent
    if ($previewDir -and -not (Test-Path $previewDir)) {
        New-Item -ItemType Directory -Path $previewDir -Force | Out-Null
    }
    Set-Content -Path $PreviewFile -Value "$($finalComment.TrimEnd())`n" -Encoding UTF8 -NoNewline
    Write-Host "`n=== COMMENT PREVIEW ===" -ForegroundColor Yellow
    Write-Host $finalComment
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "`n✅ Preview saved to: $PreviewFile" -ForegroundColor Green
    exit 0
}

# Post to GitHub
$tempFile = [System.IO.Path]::GetTempFileName()
@{ body = $finalComment } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

if ($existingUnifiedComment) {
    Write-Host "Updating unified comment ID $($existingUnifiedComment.id)..." -ForegroundColor Yellow
    $result = gh api --method PATCH "repos/dotnet/maui/issues/comments/$($existingUnifiedComment.id)" --input $tempFile --jq '.html_url'
    Write-Host "✅ PR finalize section updated: $result" -ForegroundColor Green
} else {
    Write-Host "Creating new unified comment on PR #$PRNumber..." -ForegroundColor Yellow
    $result = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile --jq '.html_url'
    Write-Host "✅ Unified comment posted: $result" -ForegroundColor Green
}

Remove-Item $tempFile
