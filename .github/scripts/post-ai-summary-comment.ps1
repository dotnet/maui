#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts the AI review summary as a GitHub Pull Request review.

.DESCRIPTION
    Creates a new PR review per run, identified by <!-- AI Summary --> marker.
    Before posting a fresh review, older generated AI Summary artifacts are
    hidden as outdated. The replacement review contains only the latest review
    session, keyed by the current HEAD commit SHA.

    After posting, the PR author is @-mentioned so they know to review.

    Content is auto-loaded from PRAgent phase files:
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/gate/content.md          (always shown first, open)
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/{pre-flight,try-fix,report}/content.md
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/pre-flight/code-review.md

    Gate is included as a section inside this unified review body — the script may
    be called by Review-PR.ps1 twice per run: once after the gate completes
    (gate-only update) and once after the review phases finish (full update).

    Any standalone legacy "<!-- AI Gate -->" comment from older versions of
    the script is hidden before the fresh review is posted to avoid duplicates.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER DryRun
    Print review body instead of posting

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

$commentCleanupScript = Join-Path $PSScriptRoot "shared/Remove-StaleMauiBotComments.ps1"
if (Test-Path $commentCleanupScript) {
    . $commentCleanupScript
}

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
    "uitests"          = @{ File = "uitests/content.md";            Icon = "🧪"; Title = "UI Tests" }
    "regression-check" = @{ File = "regression-check/content.md";   Icon = "🔍"; Title = "Regression Cross-Reference" }
    "pre-flight"       = @{ File = "pre-flight/content.md";         Icon = "🔍"; Title = "Pre-Flight — Context & Validation" }
    "code-review"      = @{ File = "pre-flight/code-review.md";     Icon = "🔬"; Title = "Code Review — Deep Analysis" }
    "try-fix"          = @{ File = "try-fix/content.md";            Icon = "🔧"; Title = "Fix — Analysis & Comparison" }
    "report"           = @{ File = "report/content.md";             Icon = "📋"; Title = "Report — Final Recommendation" }
}

function Test-PhaseContentIsNoOp {
    param(
        [Parameter(Mandatory = $true)]
        [string]$PhaseKey,

        [Parameter(Mandatory = $true)]
        [string]$Content
    )

    $normalized = ($Content -replace "`r`n", "`n").Trim()

    switch ($PhaseKey) {
        "uitests" {
            return $normalized -match '^No UI test categories needed for this PR \(no UI-relevant changes\)\.?$'
        }
        "regression-check" {
            $withoutHeading = ($normalized -replace '(?m)^##\s+.*Regression Cross-Reference\s*\n+', '').Trim()
            return (
                $withoutHeading -match '^🟢\s+No implementation files modified\s+[—-]\s+skipping regression cross-reference\.\s*$' -or
                $withoutHeading -match '^🟢\s+No regression risks detected\.\s+No labeled bug-fix PRs in the last \d+ months touched the modified files\.\s*$'
            )
        }
        default {
            return $false
        }
    }
}

function Get-AIReviewEvent {
    param([string]$ReportContent)

    if ([string]::IsNullOrWhiteSpace($ReportContent)) {
        return 'COMMENT'
    }

    $normalized = $ReportContent -replace "`r`n", "`n"
    if ($normalized -match '(?im)^\s*(?:##\s*)?(?:✅\s*)?Final\s+Recommendation:\s*APPROVE\s*$') {
        return 'APPROVE'
    }

    if ($normalized -match '(?im)^\s*(?:##\s*)?(?:⚠️\s*)?Final\s+Recommendation:\s*REQUEST\s+CHANGES\s*$') {
        return 'REQUEST_CHANGES'
    }

    return 'COMMENT'
}

function Get-PreservedMauiBotNodeIds {
    param([Parameter(Mandatory = $true)][string]$PRAgentDir)

    $files = @(
        'try-fix-review-node-id.txt',
        'ai-summary-review-node-id.txt',
        'current-review-node-ids.txt'
    )

    $nodeIds = @()
    foreach ($file in $files) {
        $path = Join-Path $PRAgentDir $file
        if (-not (Test-Path $path)) {
            continue
        }

        $nodeIds += Get-Content $path -Encoding UTF8 | ForEach-Object {
            $value = [string]$_
            if (-not [string]::IsNullOrWhiteSpace($value)) {
                $value.Trim()
            }
        }
    }

    return @($nodeIds | Where-Object { -not [string]::IsNullOrWhiteSpace($_) } | Select-Object -Unique)
}

function Invoke-PostPullRequestReview {
    param(
        [Parameter(Mandatory = $true)]
        [int]$PRNumber,

        [Parameter(Mandatory = $true)]
        [string]$Body,

        [Parameter(Mandatory = $true)]
        [ValidateSet('APPROVE', 'REQUEST_CHANGES', 'COMMENT')]
        [string]$Event
    )

    $tempFile = [System.IO.Path]::GetTempFileName()
    try {
        @{ body = $Body; event = $Event } |
            ConvertTo-Json -Depth 10 |
            Set-Content -Path $tempFile -Encoding UTF8

        $response = gh api --method POST "repos/dotnet/maui/pulls/$PRNumber/reviews" --input $tempFile 2>&1
        if ($LASTEXITCODE -ne 0) {
            throw "POST review failed (exit code $LASTEXITCODE): $response"
        }

        return (($response -join [Environment]::NewLine) | ConvertFrom-Json)
    } finally {
        Remove-Item $tempFile -ErrorAction SilentlyContinue
    }
}

# ─── Gate content (rendered first, always open) ───
$gateSection = $null
$gateFilePath = Join-Path $PRAgentDir "gate/content.md"
if (Test-Path $gateFilePath) {
    $gateContent = Get-Content $gateFilePath -Raw -Encoding UTF8
    if (-not [string]::IsNullOrWhiteSpace($gateContent)) {
        Write-Host "  ✅ gate ($((Get-Item $gateFilePath).Length) bytes)" -ForegroundColor Green
        $gateSection = @"
<details open>
<summary>🚦 <strong>Gate — Test Before & After Fix</strong></summary>
<br/>

$gateContent

</details>
"@
    } else {
        Write-Host "  ⏭️  gate (empty)" -ForegroundColor Gray
    }
} else {
    Write-Host "  ⏭️  gate (not found)" -ForegroundColor Gray
}

$phaseSections = @()
$phaseContentByKey = @{}

foreach ($key in $phases.Keys) {
    $phase = $phases[$key]
    $filePath = Join-Path $PRAgentDir $phase.File

    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw -Encoding UTF8
        if (-not [string]::IsNullOrWhiteSpace($content)) {
            if (Test-PhaseContentIsNoOp -PhaseKey $key -Content $content) {
                Write-Host "  ⏭️  $key (no actionable content)" -ForegroundColor Gray
                continue
            }

            $phaseContentByKey[$key] = $content
            Write-Host "  ✅ $key ($((Get-Item $filePath).Length) bytes)" -ForegroundColor Green
            # For uitests, make title dynamic: "UI Tests — Cat1, Cat2"
            $phaseTitle = "$($phase.Icon) $($phase.Title)"
            if ($key -eq "uitests") {
                $catMatch = [regex]::Match($content, 'Detected UI test categories:\*\*\s*`{1,2}([^`]+)`{1,2}')
                if ($catMatch.Success) {
                    $phaseTitle = "$($phase.Icon) $($phase.Title) — $($catMatch.Groups[1].Value)"
                }
            }
            $phaseSections += @"
<details>
<summary><strong>$phaseTitle</strong></summary>
<br/>

$content

</details>
"@
        } else {
            Write-Host "  ⏭️  $key (empty)" -ForegroundColor Gray
        }
    } else {
        Write-Host "  ⏭️  $key (not found)" -ForegroundColor Gray
    }
}

if (-not $gateSection -and $phaseSections.Count -eq 0) {
    throw "No gate or phase content found. Ensure at least one of gate/content.md or {phase}/content.md exists in $PRAgentDir."
}

$reviewEvent = Get-AIReviewEvent -ReportContent $phaseContentByKey['report']
Write-Host "  🧾 PR review event: $reviewEvent" -ForegroundColor Cyan

# ============================================================================
# FETCH PR METADATA (commit + author)
# ============================================================================

try {
    $commitJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' 2>$null | ConvertFrom-Json
} catch {
    Write-Host "⚠️ Failed to fetch commit info: $_" -ForegroundColor Yellow
    $commitJson = $null
}
$commitTitle = if ($commitJson) { ($commitJson.message -split "`n")[0] } else { "Unknown" }
$commitTitle = $commitTitle -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
$commitSha7 = if ($commitJson) { $commitJson.sha.Substring(0, 7) } else { "unknown" }
$commitFull = if ($commitJson) { $commitJson.sha } else { "" }
$commitUrl = if ($commitJson) { "https://github.com/dotnet/maui/commit/$commitFull" } else { "#" }

try {
    $prAuthor = gh api "repos/dotnet/maui/pulls/$PRNumber" --jq '.user.login' 2>$null
} catch { $prAuthor = $null }

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm UTC")

# ============================================================================
# BUILD NEW SESSION BLOCK
# ============================================================================

# Combine gate (always first, open) with phases (collapsed). When only one
# kind of content is available, the session still renders cleanly.
$sessionParts = @()
if ($gateSection)            { $sessionParts += $gateSection }
if ($phaseSections.Count -gt 0) { $sessionParts += ($phaseSections -join "`n`n---`n`n") }
$phaseContent = $sessionParts -join "`n`n---`n`n"

$sessionMarkerStart = "<!-- SESSION:$commitSha7 START -->"
$sessionMarkerEnd = "<!-- SESSION:$commitSha7 END -->"

# The latest session is built with <details open>; when merged into existing
# sessions the script re-tags only the newest as "open".
$newSessionBlock = @"
$sessionMarkerStart
<details open>
<summary>📊 <strong>Review Session</strong> — <a href="$commitUrl"><code>$commitSha7</code></a> · <strong>$commitTitle</strong> · <em>$timestamp</em></summary>
<br/>

$phaseContent

---

</details>
$sessionMarkerEnd
"@

# ============================================================================
# FIND EXISTING AI SUMMARY ARTIFACTS & BUILD FINAL BODY
# ============================================================================

Write-Host "Checking for existing AI Summary artifacts..." -ForegroundColor Yellow
$existingCommentIds = @()
$existingReviewIds = @()
$existingBodies = @()

$existingRaw = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --paginate 2>$null
if ($existingRaw) {
    try {
        $allComments = $existingRaw | ConvertFrom-Json
        $existingObjs = @($allComments | Where-Object { $_.body -and $_.body.Contains($MARKER) })
        if ($existingObjs.Count -gt 0) {
            $existingCommentIds = @($existingObjs | ForEach-Object { $_.id })
            $existingBodies = @($existingObjs | ForEach-Object { [string]$_.body })
            Write-Host "✓ Found existing AI Summary issue comment(s): $($existingCommentIds -join ', ')" -ForegroundColor Green
        }

        if (Get-Command Get-GitHubPullRequestReviews -ErrorAction SilentlyContinue) {
            $existingReviewObjs = @(Get-GitHubPullRequestReviews -PRNumber $PRNumber | Where-Object { $_.body -and $_.body.Contains($MARKER) })
            if ($existingReviewObjs.Count -gt 0) {
                $existingReviewIds = @($existingReviewObjs | ForEach-Object { $_.id })
                $existingBodies += @($existingReviewObjs | ForEach-Object { [string]$_.body })
                Write-Host "✓ Found existing AI Summary review(s): $($existingReviewIds -join ', ')" -ForegroundColor Green
            }
        }
    } catch {
        Write-Host "⚠️ Could not parse comments: $_" -ForegroundColor Yellow
    }
}

$authorPing = ""
if ($prAuthor) {
    $authorPing = "> 👋 @$prAuthor — new AI review results are available. Please review the latest session below."
}

$finalizeSection = ""
$finalizePattern = '(?s)(<!-- SECTION:PR-FINALIZE -->.*?<!-- /SECTION:PR-FINALIZE -->)'
if ($existingBodies -and $existingBodies.Count -gt 0) {
    for ($i = $existingBodies.Count - 1; $i -ge 0; $i--) {
        if ($existingBodies[$i] -match $finalizePattern) {
            $finalizeSection = "`n`n" + $Matches[1]
            break
        }
    }
}

$commentBody = @"
$MARKER

## 🤖 AI Summary

$authorPing

$newSessionBlock$finalizeSection
"@

# Clean up excessive blank lines
$commentBody = $commentBody -replace "`n{4,}", "`n`n`n"

Write-Host "  ✅ Built review body ($($commentBody.Length) chars)" -ForegroundColor Green

# ============================================================================
# DRY RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "Review event: $reviewEvent" -ForegroundColor Cyan
    Write-Host "=== COMMENT PREVIEW ===" -ForegroundColor Cyan
    Write-Host $commentBody
    Write-Host "=== END PREVIEW ===" -ForegroundColor Cyan
    exit 0
}

# ============================================================================
# HIDE STALE GENERATED ARTIFACTS, THEN POST REVIEW
# ============================================================================

$preserveNodeIds = Get-PreservedMauiBotNodeIds -PRAgentDir $PRAgentDir

if (Get-Command Hide-StaleMauiBotIssueComments -ErrorAction SilentlyContinue) {
    Hide-StaleMauiBotIssueComments `
        -PRNumber $PRNumber `
        -IncludeAISummary `
        -IncludeLegacyGate `
        -IncludeMergeConflict `
        -IncludeTryFix `
        -PreserveNodeIds $preserveNodeIds `
        -Reason "stale generated PR review artifact"
}

if (Get-Command Hide-StaleMauiBotPullRequestReviews -ErrorAction SilentlyContinue) {
    Hide-StaleMauiBotPullRequestReviews `
        -PRNumber $PRNumber `
        -IncludeAISummary `
        -IncludeTryFix `
        -PreserveNodeIds $preserveNodeIds `
        -Reason "stale generated PR review" `
        -DismissFormalReviews
}

Write-Host "Creating new AI Summary PR review ($reviewEvent)..." -ForegroundColor Yellow
$postedEvent = $reviewEvent
try {
    $review = Invoke-PostPullRequestReview -PRNumber $PRNumber -Body $commentBody -Event $postedEvent
} catch {
    if ($postedEvent -eq 'COMMENT') {
        throw
    }

    Write-Host "⚠️ Formal $postedEvent review was rejected; retrying as COMMENT: $_" -ForegroundColor Yellow
    $postedEvent = 'COMMENT'
    $review = Invoke-PostPullRequestReview -PRNumber $PRNumber -Body $commentBody -Event $postedEvent
}

$reviewId = [string]$review.id
$reviewNodeId = [string]$review.node_id

if (-not [string]::IsNullOrWhiteSpace($reviewId)) {
    Set-Content -Path (Join-Path $PRAgentDir "ai-summary-review-id.txt") -Value $reviewId -Encoding UTF8
}
if (-not [string]::IsNullOrWhiteSpace($reviewNodeId)) {
    Set-Content -Path (Join-Path $PRAgentDir "ai-summary-review-node-id.txt") -Value $reviewNodeId -Encoding UTF8
}

Write-Host "✅ AI Summary PR review posted (ID: $reviewId, event: $postedEvent)" -ForegroundColor Green
Write-Output "AI_SUMMARY_REVIEW_ID=$reviewId"
Write-Output "AI_SUMMARY_REVIEW_NODE_ID=$reviewNodeId"
Write-Output "AI_SUMMARY_REVIEW_EVENT=$postedEvent"
