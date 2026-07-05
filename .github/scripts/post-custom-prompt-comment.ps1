#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Appends a custom-prompt analysis into the unified AI Summary PR review on a GitHub PR.

.DESCRIPTION
    Reads the custom-prompt result markdown file and injects its content as a
    <!-- SECTION:CUSTOM-PROMPT --> block inside the AI Summary review body.

    The AI Summary is posted by post-ai-summary-comment.ps1 as a fresh PR *review*
    each run (identified by the <!-- AI Summary --> marker), so this script updates
    the review body via `PUT .../pulls/{pr}/reviews/{review_id}` rather than editing
    an issue comment.

    The custom prompt is a free-text request supplied by a maintainer (via the
    CustomPrompt pipeline parameter) — e.g. "consider ui tests when you do the review".
    A copilot-cli step analyzes the PR against that request and writes its findings to a
    markdown file; this script surfaces those findings inside the AI Summary review.

    Replace-or-append semantics: re-running refreshes the section rather than
    duplicating it. If the AI Summary review cannot be found/updated, the analysis is
    posted as a standalone AI-generated issue comment so it is never lost.

    Auto-discovers the content file from:
      CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/custom-prompt/custom-prompt-result.md

.PARAMETER PRNumber
    The PR number (required unless inferable from -ContentFile).

.PARAMETER ContentFile
    Path to the custom-prompt result markdown. Auto-discovered from PRNumber if not provided.

.PARAMETER CustomPrompt
    The maintainer's original custom-prompt text, echoed in the section header for context.

.PARAMETER ReviewId
    The AI Summary PR review id (from post-ai-summary-comment.ps1). Avoids a search race.

.PARAMETER DryRun
    Print/preview the section instead of posting.

.PARAMETER PreviewFile
    Custom path for dry-run preview output.

.EXAMPLE
    ./post-custom-prompt-comment.ps1 -PRNumber 34427 -CustomPrompt "consider ui tests" -ReviewId 123456

.EXAMPLE
    ./post-custom-prompt-comment.ps1 -PRNumber 34427 -DryRun
#>

param(
    [Parameter(Mandatory = $false)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$ContentFile,

    [Parameter(Mandatory = $false)]
    [string]$CustomPrompt,

    [Parameter(Mandatory = $false)]
    [string]$ReviewId,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [string]$PreviewFile
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Custom Prompt Analysis (append to AI Summary review)     ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$REPO = "dotnet/maui"

# ============================================================================
# AUTO-DISCOVERY
# ============================================================================

$repoRoot = git rev-parse --show-toplevel 2>$null

if ($PRNumber -gt 0 -and [string]::IsNullOrWhiteSpace($ContentFile)) {
    $candidates = @("CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/custom-prompt/custom-prompt-result.md")
    if ($repoRoot) {
        $candidates += Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/custom-prompt/custom-prompt-result.md"
    }
    foreach ($candidate in $candidates) {
        if (Test-Path $candidate) {
            $ContentFile = $candidate
            Write-Host "ℹ️  Auto-discovered content file: $ContentFile" -ForegroundColor Cyan
            break
        }
    }
}

if ($PRNumber -eq 0 -and -not [string]::IsNullOrWhiteSpace($ContentFile) -and $ContentFile -match '[/\\](\d+)[/\\]') {
    $PRNumber = [int]$Matches[1]
    Write-Host "ℹ️  Auto-detected PRNumber: $PRNumber from path" -ForegroundColor Cyan
}

if ($PRNumber -eq 0) {
    throw "PRNumber is required. Provide via -PRNumber or use -ContentFile with a path containing the PR number."
}

# ============================================================================
# LOAD CONTENT
# ============================================================================

if ([string]::IsNullOrWhiteSpace($ContentFile) -or -not (Test-Path $ContentFile)) {
    Write-Host "⚠️  No custom-prompt result file found for PR #$PRNumber — nothing to post" -ForegroundColor Yellow
    exit 0
}

$resultContent = (Get-Content $ContentFile -Raw -Encoding UTF8).Trim()
if ([string]::IsNullOrWhiteSpace($resultContent)) {
    Write-Host "⚠️  Content file is empty — nothing to post" -ForegroundColor Yellow
    exit 0
}

Write-Host "  ✅ Loaded content ($($resultContent.Length) chars) from: $ContentFile" -ForegroundColor Green

# ============================================================================
# BUILD SECTION
# ============================================================================

$MAIN_MARKER = "<!-- AI Summary -->"
$SECTION_START = "<!-- SECTION:CUSTOM-PROMPT -->"
$SECTION_END = "<!-- /SECTION:CUSTOM-PROMPT -->"

# Echo the maintainer's request (trimmed and single-lined) in the header for context.
$promptEcho = ""
if (-not [string]::IsNullOrWhiteSpace($CustomPrompt)) {
    $flat = ($CustomPrompt -replace '\s+', ' ').Trim()
    if ($flat.Length -gt 200) { $flat = $flat.Substring(0, 200) + "…" }
    $promptEcho = "> **Requested:** $flat`n`n"
}

$customSection = @"
$SECTION_START

### 🔍 Custom Prompt Analysis

> ● _AI-generated — supplementary analysis from a maintainer-supplied custom prompt (GitHub Copilot CLI)._

$promptEcho<details>
<summary><strong>Expand analysis</strong></summary>

$resultContent

</details>
$SECTION_END
"@

# Inject or replace the section into a body.
function Merge-Section {
    param([string]$Body, [string]$Section)
    if ($Body -match [regex]::Escape($SECTION_START)) {
        $pattern = [regex]::Escape($SECTION_START) + "[\s\S]*?" + [regex]::Escape($SECTION_END)
        $merged = [regex]::Replace($Body, $pattern, [System.Text.RegularExpressions.MatchEvaluator] { param($m) $Section })
    } else {
        $merged = $Body.TrimEnd() + "`n`n" + $Section
    }
    return ($merged -replace "`n{4,}", "`n`n`n")
}

# ============================================================================
# DRY RUN PREVIEW
# ============================================================================

if ($DryRun) {
    if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
        $PreviewFile = "CustomAgentLogsTmp/PRState/$PRNumber/custom-prompt-preview.md"
    }
    $previewDir = Split-Path $PreviewFile -Parent
    if ($previewDir -and -not (Test-Path $previewDir)) {
        New-Item -ItemType Directory -Path $previewDir -Force | Out-Null
    }
    Set-Content -Path $PreviewFile -Value "$($customSection.TrimEnd())`n" -Encoding UTF8 -NoNewline
    Write-Host "`n=== CUSTOM PROMPT SECTION PREVIEW ===" -ForegroundColor Yellow
    Write-Host $customSection
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "`n✅ Preview saved to: $PreviewFile" -ForegroundColor Green
    exit 0
}

# ============================================================================
# LOCATE THE AI SUMMARY REVIEW
# ============================================================================

$targetReview = $null

# Direct fetch by id (fast path — avoids GitHub API eventual-consistency race).
if (-not [string]::IsNullOrWhiteSpace($ReviewId) -and $ReviewId -match '^\d+$') {
    try {
        $rv = gh api "repos/$REPO/pulls/$PRNumber/reviews/$ReviewId" 2>$null | ConvertFrom-Json
        if ($rv.body -match [regex]::Escape($MAIN_MARKER)) {
            $targetReview = $rv
            Write-Host "✓ Found AI Summary review via passed ID (ID: $($rv.id))" -ForegroundColor Green
        } else {
            Write-Host "⚠️ Passed review ID $ReviewId has no AI Summary marker — falling back to search" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ Could not fetch review by ID $ReviewId — falling back to search" -ForegroundColor Yellow
    }
}

# Fallback: search PR reviews for the marker (last match wins — freshest review).
if (-not $targetReview) {
    try {
        $reviews = gh api "repos/$REPO/pulls/$PRNumber/reviews?per_page=100" --paginate 2>$null | ConvertFrom-Json
        foreach ($rv in $reviews) {
            if ($rv.body -match [regex]::Escape($MAIN_MARKER)) {
                $targetReview = $rv
            }
        }
        if ($targetReview) {
            Write-Host "✓ Found AI Summary review via search (ID: $($targetReview.id))" -ForegroundColor Green
        } else {
            Write-Host "⚠️ No AI Summary review found — will post a standalone comment" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "⚠️ Could not list PR reviews: $_" -ForegroundColor Yellow
    }
}

# ============================================================================
# UPDATE REVIEW BODY (or fall back to a standalone comment)
# ============================================================================

function Post-StandaloneComment {
    param([string]$Section)
    # Author ping, guarded against a 404 error body leaking into the ping.
    $prAuthor = $null
    try { $prAuthor = gh api "repos/$REPO/pulls/$PRNumber" --jq '.user.login' 2>$null } catch { $prAuthor = $null }
    if ($prAuthor -and ($prAuthor -notmatch '^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')) { $prAuthor = $null }
    $ping = if ($prAuthor) { "> ℹ @$prAuthor — a custom-prompt analysis is ready below.`n`n" } else { "" }

    $body = @"
$MAIN_MARKER
$ping$Section
"@
    $tmp = [System.IO.Path]::GetTempFileName()
    try {
        @{ body = $body } | ConvertTo-Json -Depth 10 | Set-Content -Path $tmp -Encoding UTF8
        $url = gh api --method POST "repos/$REPO/issues/$PRNumber/comments" --input $tmp --jq '.html_url'
        Write-Host "✅ Custom prompt analysis posted as standalone comment: $url" -ForegroundColor Green
    } finally {
        Remove-Item $tmp -ErrorAction SilentlyContinue
    }
}

if ($targetReview) {
    $newBody = Merge-Section -Body $targetReview.body -Section $customSection
    $tmp = [System.IO.Path]::GetTempFileName()
    try {
        @{ body = $newBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tmp -Encoding UTF8
        $putResult = gh api --method PUT "repos/$REPO/pulls/$PRNumber/reviews/$($targetReview.id)" --input $tmp --jq '.html_url' 2>&1
        if ($LASTEXITCODE -ne 0) { throw "PUT failed (exit code $LASTEXITCODE): $putResult" }
        Write-Host "✅ Custom prompt section appended to AI Summary review: $putResult" -ForegroundColor Green
    } catch {
        Write-Host "⚠️ Could not update review body ($_) — posting standalone comment instead" -ForegroundColor Yellow
        Post-StandaloneComment -Section $customSection
    } finally {
        Remove-Item $tmp -ErrorAction SilentlyContinue
    }
} else {
    Post-StandaloneComment -Section $customSection
}
