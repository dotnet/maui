#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts the UI / device test category detection results into the unified
    AI Summary comment on a GitHub PR.

.DESCRIPTION
    Runs Detect-UITestCategories.ps1 against the PR diff and injects a
    <!-- SECTION:TEST-CATEGORIES --> block into the AI Summary comment.

    The section lists which UI test categories and device test categories
    are likely affected by the PR's changes, derived from:
      1. [Category(...)] attributes in added test code, OR
      2. Path-based inference from changed source files.

    It also surfaces the corresponding pipeline parameter values
    (`uiTestCategories` / `deviceTestCategories`) so reviewers can trigger
    targeted runs of `maui-pr-uitests` / `maui-pr-devicetests`.

.PARAMETER PRNumber
    The PR number to analyze and post to (required).

.PARAMETER Repository
    GitHub repository in 'owner/repo' form. Defaults to 'dotnet/maui'.

.PARAMETER ExistingCommentId
    Optional comment ID to update directly (avoids GitHub API search race).

.PARAMETER DryRun
    Print to a preview file instead of posting.

.PARAMETER PreviewFile
    Custom path for the dry-run preview output.

.EXAMPLE
    ./post-test-categories-comment.ps1 -PRNumber 34845

.EXAMPLE
    ./post-test-categories-comment.ps1 -PRNumber 34845 -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$Repository = 'dotnet/maui',

    [Parameter(Mandatory = $false)]
    [string]$ExistingCommentId,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [string]$PreviewFile
)

$ErrorActionPreference = 'Stop'

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Test Categories Comment (Post/Update)                    ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$repoRoot = git rev-parse --show-toplevel 2>$null
if (-not $repoRoot) { Write-Error "Not in a git repository"; exit 1 }

$detectScript = Join-Path $repoRoot ".github/scripts/Detect-UITestCategories.ps1"
if (-not (Test-Path $detectScript)) {
    Write-Error "Detect-UITestCategories.ps1 not found at $detectScript"
    exit 1
}

# ── 1. Run the detection script ─────────────────────────────────────────────
Write-Host "🔍 Running category detection on PR #$PRNumber..." -ForegroundColor Yellow
$jsonOut = & pwsh -NoProfile -File $detectScript -PrNumber $PRNumber -Repository $Repository -Json 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Detection failed (exit $LASTEXITCODE):" -ForegroundColor Yellow
    Write-Host ($jsonOut -join "`n") -ForegroundColor DarkGray
    exit 0  # non-fatal
}

try {
    $detected = ($jsonOut -join "`n") | ConvertFrom-Json
} catch {
    Write-Host "⚠️  Could not parse detection JSON output — skipping" -ForegroundColor Yellow
    exit 0
}

$uiCats     = @($detected.uiCategories     | Where-Object { $_ })
$deviceCats = @($detected.deviceCategories | Where-Object { $_ })
$uiSrc      = $detected.uiCategoriesSource
$deviceSrc  = $detected.deviceCategoriesSource
$uiFilter   = $detected.uiCategoriesFilter
$deviceFilter = $detected.deviceCategoriesFilter
$totalFiles = $detected.totalChangedFiles

Write-Host "  UI categories ($uiSrc):     $(if ($uiCats.Count) { $uiCats -join ', ' } else { '(none)' })" -ForegroundColor Cyan
Write-Host "  Device categories ($deviceSrc): $(if ($deviceCats.Count) { $deviceCats -join ', ' } else { '(none)' })" -ForegroundColor Cyan

# ── 2. Build the section body ───────────────────────────────────────────────
$MAIN_MARKER   = "<!-- AI Summary -->"
$SECTION_START = "<!-- SECTION:TEST-CATEGORIES -->"
$SECTION_END   = "<!-- /SECTION:TEST-CATEGORIES -->"

function Format-CategoryTable {
    param([string[]]$Categories, [string]$Source)
    if (-not $Categories -or $Categories.Count -eq 0) { return "_(none detected)_" }
    $sourceLabel = switch ($Source) {
        'attribute'      { 'Detected from `[Category(...)]` attribute' }
        'path-inference' { 'Inferred from changed source path' }
        default          { $Source }
    }
    $rows = ($Categories | ForEach-Object { "| ``$_`` | $sourceLabel |" }) -join "`n"
    @"
| Category | Source |
|----------|--------|
$rows
"@
}

$uiTable     = Format-CategoryTable -Categories $uiCats     -Source $uiSrc
$deviceTable = Format-CategoryTable -Categories $deviceCats -Source $deviceSrc

if ($uiCats.Count -eq 0 -and $deviceCats.Count -eq 0) {
    $sectionBody = @"
$SECTION_START
<details>
<summary>🏷️ <strong>Test Categories for Regression Detection</strong></summary>

No UI or device test categories were detected for this PR. The changes don't appear to affect any control or platform code with associated test categories.

_$totalFiles file(s) changed. Detection: ``Detect-UITestCategories.ps1 -PrNumber $PRNumber``_

</details>
$SECTION_END
"@
} else {
    $uiBlock = if ($uiCats.Count) {
        @"
**UI Test Categories**

$uiTable

Trigger ``maui-pr-uitests`` with: ``uiTestCategories = $uiFilter``
"@
    } else { "" }

    $deviceBlock = if ($deviceCats.Count) {
        @"
**Device Test Categories**

$deviceTable

Trigger ``maui-pr-devicetests`` with: ``deviceTestCategories = $deviceFilter``
"@
    } else { "" }

    $sectionBody = @"
$SECTION_START
<details>
<summary>🏷️ <strong>Test Categories for Regression Detection</strong></summary>

$uiBlock

$deviceBlock

> ℹ️ Categories are detected from ``[Category(...)]`` attributes on added test lines, or inferred from changed source file paths. Source: ``Detect-UITestCategories.ps1 -PrNumber $PRNumber``

</details>
$SECTION_END
"@
}

# ── 3. Find existing AI Summary comment ─────────────────────────────────────
Write-Host "`nInjecting into AI Summary comment on #$PRNumber..." -ForegroundColor Yellow

$existingUnifiedComment = $null

if (-not [string]::IsNullOrWhiteSpace($ExistingCommentId)) {
    try {
        $commentJson = gh api "repos/$Repository/issues/comments/$ExistingCommentId" 2>$null
        $directComment = $commentJson | ConvertFrom-Json
        if ($directComment.body -match [regex]::Escape($MAIN_MARKER)) {
            $existingUnifiedComment = $directComment
            Write-Host "✓ Found AI Summary comment via passed ID (ID: $($directComment.id))" -ForegroundColor Green
        }
    } catch {
        Write-Host "⚠️ Could not fetch comment by ID $ExistingCommentId — falling back to search" -ForegroundColor Yellow
    }
}

if (-not $existingUnifiedComment) {
    try {
        $commentsJson = gh api "repos/$Repository/issues/$PRNumber/comments?per_page=100" 2>$null
        $comments = $commentsJson | ConvertFrom-Json
        foreach ($comment in $comments) {
            if ($comment.body -match [regex]::Escape($MAIN_MARKER)) {
                $existingUnifiedComment = $comment
                Write-Host "✓ Found AI Summary comment (ID: $($comment.id))" -ForegroundColor Green
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

# ── 4. Build the final comment body ─────────────────────────────────────────
if ($existingUnifiedComment) {
    $body = $existingUnifiedComment.body
    if ($body -match [regex]::Escape($SECTION_START)) {
        $pattern = [regex]::Escape($SECTION_START) + "[\s\S]*?" + [regex]::Escape($SECTION_END)
        $finalComment = $body -replace $pattern, $sectionBody
    } else {
        $finalComment = $body.TrimEnd() + "`n`n" + $sectionBody
    }
    $finalComment = $finalComment -replace "`n{4,}", "`n`n`n"
} else {
    $finalComment = @"
$MAIN_MARKER

## 🤖 AI Summary

$sectionBody
"@
}

# ── 5. DryRun: write preview ────────────────────────────────────────────────
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

# ── 6. Post / update ────────────────────────────────────────────────────────
$tempFile = [System.IO.Path]::GetTempFileName()
@{ body = $finalComment } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

try {
    if ($existingUnifiedComment) {
        Write-Host "Updating AI Summary comment ID $($existingUnifiedComment.id)..." -ForegroundColor Yellow
        try {
            $patchResult = gh api --method PATCH "repos/$Repository/issues/comments/$($existingUnifiedComment.id)" --input $tempFile --jq '.html_url' 2>&1
            if ($LASTEXITCODE -ne 0) { throw "PATCH failed (exit $LASTEXITCODE): $patchResult" }
            Write-Host "✅ Test categories section updated: $patchResult" -ForegroundColor Green
        } catch {
            Write-Host "⚠️ Could not update — posting standalone comment: $_" -ForegroundColor Yellow
            $standaloneBody = @"
$MAIN_MARKER

## 🤖 AI Summary

$sectionBody
"@
            $standaloneTempFile = [System.IO.Path]::GetTempFileName()
            try {
                @{ body = $standaloneBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $standaloneTempFile -Encoding UTF8
                $result = gh api --method POST "repos/$Repository/issues/$PRNumber/comments" --input $standaloneTempFile --jq '.html_url'
                Write-Host "✅ Standalone comment posted: $result" -ForegroundColor Green
            } finally {
                Remove-Item $standaloneTempFile -ErrorAction SilentlyContinue
            }
        }
    } else {
        Write-Host "Creating new AI Summary comment on PR #$PRNumber..." -ForegroundColor Yellow
        $result = gh api --method POST "repos/$Repository/issues/$PRNumber/comments" --input $tempFile --jq '.html_url'
        Write-Host "✅ Comment posted: $result" -ForegroundColor Green
    }
} finally {
    Remove-Item $tempFile -ErrorAction SilentlyContinue
}
