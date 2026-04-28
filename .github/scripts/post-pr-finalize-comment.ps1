#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts the PR finalize summary into the unified AI Summary comment on a GitHub PR.

.DESCRIPTION
    Reads the pr-finalize-summary.md file and injects its content into the
    <!-- SECTION:PR-FINALIZE --> block of the unified AI Summary comment.

    Compatible with session-based AI Summary comments: the finalize section is placed
    after all session blocks and the PR author ping is updated.

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
    [string]$ExistingCommentId,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [string]$PreviewFile
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
# FETCH PR AUTHOR FOR PING
# ============================================================================

try {
    $prAuthor = gh api "repos/dotnet/maui/pulls/$PRNumber" --jq '.user.login' 2>$null
} catch { $prAuthor = $null }

# ============================================================================
# POST / UPDATE
# ============================================================================

Write-Host "`nInjecting into AI Summary comment on #$PRNumber..." -ForegroundColor Yellow

# Find existing unified comment
$existingUnifiedComment = $null

# If caller passed the comment ID (avoids GitHub API eventual-consistency race), fetch it directly
if (-not [string]::IsNullOrWhiteSpace($ExistingCommentId)) {
    try {
        $commentJson = gh api "repos/dotnet/maui/issues/comments/$ExistingCommentId" 2>$null
        $directComment = $commentJson | ConvertFrom-Json
        if ($directComment.body -match [regex]::Escape($MAIN_MARKER)) {
            $existingUnifiedComment = $directComment
            Write-Host "✓ Found unified AI Summary comment via passed ID (ID: $($directComment.id))" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Passed comment ID $ExistingCommentId does not contain AI Summary marker — falling back to search" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ Could not fetch comment by ID $ExistingCommentId — falling back to search" -ForegroundColor Yellow
    }
}

# Fallback: search through all PR comments
if (-not $existingUnifiedComment) {
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

    # Update the author ping to reflect the finalize update
    if ($prAuthor) {
        $pingPattern = '> 👋 @\S+ — .*?(?=\n)'
        $newPing = "> 👋 @$prAuthor — PR finalization review is ready. Please review below."
        if ($finalComment -match $pingPattern) {
            $finalComment = $finalComment -replace $pingPattern, $newPing
        }
    }

    $finalComment = $finalComment -replace "`n{4,}", "`n`n`n"
} else {
    $authorPing = ""
    if ($prAuthor) {
        $authorPing = "> 👋 @$prAuthor — PR finalization review is ready. Please review below.`n"
    }
    $finalComment = @"
$MAIN_MARKER

## 🤖 AI Summary

$authorPing
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

try {
    if ($existingUnifiedComment) {
        Write-Host "Updating unified comment ID $($existingUnifiedComment.id)..." -ForegroundColor Yellow
        $patchResult = $null
        try {
            $patchResult = gh api --method PATCH "repos/dotnet/maui/issues/comments/$($existingUnifiedComment.id)" --input $tempFile --jq '.html_url' 2>&1
            if ($LASTEXITCODE -ne 0) { throw "PATCH failed (exit code $LASTEXITCODE): $patchResult" }
            Write-Host "✅ PR finalize section updated: $patchResult" -ForegroundColor Green
        } catch {
            Write-Host "⚠️ Could not update comment (no edit permission?) — creating new comment instead: $_" -ForegroundColor Yellow
            $authorPing = ""
            if ($prAuthor) {
                $authorPing = "> 👋 @$prAuthor — PR finalization review is ready. Please review below.`n"
            }
            $standaloneBody = @"
$MAIN_MARKER

## 🤖 AI Summary

$authorPing
$finalizeSection
"@
            $standaloneTempFile = [System.IO.Path]::GetTempFileName()
            try {
                @{ body = $standaloneBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $standaloneTempFile -Encoding UTF8
                $result = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $standaloneTempFile --jq '.html_url'
                Write-Host "✅ Unified comment posted (new): $result" -ForegroundColor Green
            } finally {
                Remove-Item $standaloneTempFile -ErrorAction SilentlyContinue
            }
        }
    } else {
        Write-Host "Creating new unified comment on PR #$PRNumber..." -ForegroundColor Yellow
        $result = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile --jq '.html_url'
        Write-Host "✅ Unified comment posted: $result" -ForegroundColor Green
    }
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
