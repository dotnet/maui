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

$RepoRoot = git rev-parse --show-toplevel 2>$null
$PRAgentDir = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
if (-not (Test-Path $PRAgentDir)) {
    if ($RepoRoot) {
        $PRAgentDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    }
}

if (-not (Test-Path $PRAgentDir)) {
    throw "No PRAgent directory found at: $PRAgentDir"
}

$phases = [ordered]@{
    "uitests"          = @{ File = "uitests/content.md";            Title = "UI Tests" }
    "regression-check" = @{ File = "regression-check/content.md";   Title = "Regression Cross-Reference" }
    "pre-flight"       = @{ File = "pre-flight/content.md";         Title = "Pre-Flight — Context & Validation" }
    "code-review"      = @{ File = "pre-flight/code-review.md";     Title = "Code Review — Deep Analysis" }
    "try-fix"          = @{ File = "try-fix/content.md";            Title = "Fix — Analysis & Comparison" }
    "report"           = @{ File = "report/content.md";             Title = "Report — Final Recommendation" }
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

function ConvertTo-TitleCase {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $Value
    }

    $trimmed = $Value.Trim()
    switch -Regex ($trimmed) {
        '(?i)^android$' { return 'Android' }
        '(?i)^ios$' { return 'iOS' }
        '(?i)^maccatalyst$' { return 'MacCatalyst' }
        '(?i)^windows$' { return 'Windows' }
        '(?i)^all$' { return 'All' }
    }

    return (Get-Culture).TextInfo.ToTitleCase($trimmed.ToLowerInvariant())
}

function ConvertTo-ShieldsSegment {
    param([string]$Value)

    $encoded = [uri]::EscapeDataString($Value)
    return ($encoded -replace '-', '--' -replace '_', '__')
}

function New-StatusChip {
    param(
        [Parameter(Mandatory = $true)][string]$Label,
        [Parameter(Mandatory = $true)][string]$Value,
        [Parameter(Mandatory = $true)][string]$Color
    )

    $labelSegment = ConvertTo-ShieldsSegment $Label
    $valueSegment = ConvertTo-ShieldsSegment $Value
    $alt = "$Label $Value" -replace '"', '&quot;'
    return "  <img alt=`"$alt`" src=`"https://img.shields.io/badge/$labelSegment-$valueSegment-$Color`?labelColor=30363d&style=flat-square`">"
}

function Get-GateStatus {
    param([string]$GateContent)

    if ([string]::IsNullOrWhiteSpace($GateContent)) {
        return 'Unknown'
    }

    if ($GateContent -match '(?im)Gate Result:\s*(?:\S+\s*)?(FAILED|PASSED|SKIPPED)') {
        return ConvertTo-TitleCase $Matches[1]
    }

    if ($GateContent -match '(?i)\bfailed\b') { return 'Failed' }
    if ($GateContent -match '(?i)\bpassed\b') { return 'Passed' }
    if ($GateContent -match '(?i)\bskipped\b') { return 'Skipped' }
    return 'Unknown'
}

function Get-ConfidenceStatus {
    param([string[]]$Contents)

    foreach ($content in $Contents) {
        if ([string]::IsNullOrWhiteSpace($content)) {
            continue
        }

        if ($content -match '(?im)\*\*Confidence:\*\*\s*(high|medium|low|unknown)') {
            return ConvertTo-TitleCase $Matches[1]
        }
        if ($content -match '(?im)^Confidence:\s*(high|medium|low|unknown)') {
            return ConvertTo-TitleCase $Matches[1]
        }
    }

    return 'Unknown'
}

function Get-PlatformStatus {
    param([string[]]$Contents)

    foreach ($content in $Contents) {
        if ([string]::IsNullOrWhiteSpace($content)) {
            continue
        }

        if ($content -match '(?im)\*\*Platform:\*\*\s*([A-Za-z, /]+)') {
            return ConvertTo-TitleCase (($Matches[1] -split '[,/]')[0])
        }
        if ($content -match '(?im)\*\*Platforms Affected:\*\*\s*([A-Za-z, /]+)') {
            return ConvertTo-TitleCase (($Matches[1] -split '[,/]')[0])
        }
    }

    return 'Unknown'
}

function New-StatusChipRow {
    param(
        [string]$GateStatus,
        [string]$ReviewStatus,
        [string]$Confidence,
        [string]$Platform
    )

    $gateColor = switch ($GateStatus) {
        'Passed' { '1a7f37' }
        'Skipped' { 'bf8700' }
        default { 'd1242f' }
    }
    $reviewColor = switch ($ReviewStatus) {
        'LGTM' { '1a7f37' }
        'Approved' { '1a7f37' }
        'Needs Changes' { 'd1242f' }
        default { '0969da' }
    }
    $confidenceColor = switch ($Confidence) {
        'High' { '0969da' }
        'Medium' { 'bf8700' }
        'Low' { 'd1242f' }
        default { '57606a' }
    }
    $platformColor = if ($Platform -eq 'Unknown') { '57606a' } else { '8250df' }

    $chips = @(
        (New-StatusChip -Label 'Gate' -Value $GateStatus -Color $gateColor),
        (New-StatusChip -Label 'Code Review' -Value $ReviewStatus -Color $reviewColor),
        (New-StatusChip -Label 'Confidence' -Value $Confidence -Color $confidenceColor),
        (New-StatusChip -Label 'Platform' -Value $Platform -Color $platformColor)
    )

    return @"
<p align="left">
$($chips -join "`n")
</p>
"@
}

function New-FutureActionSection {
    param(
        [Parameter(Mandatory = $true)][string]$PRAgentDir
    )

    $winnerFile = Join-Path $PRAgentDir "winner.json"
    if (-not (Test-Path $winnerFile)) {
        return @"
---

<details>
<summary><strong>Future Action</strong> — review latest findings</summary>
<br/>

No alternative fix was selected for this run. Review the session findings and CI results before merging.

</details>
"@
    }

    try {
        $winner = Get-Content -Raw -LiteralPath $winnerFile -Encoding UTF8 | ConvertFrom-Json
    } catch {
        return @"
---

<details>
<summary><strong>Future Action</strong> — review latest findings</summary>
<br/>

The workflow could not parse the fix-selection result. Review the session findings and CI results before merging.

</details>
"@
    }

    if ($winner.isPRFix -eq $true -or [string]::IsNullOrWhiteSpace([string]$winner.winner)) {
        return @"
---

<details>
<summary><strong>Future Action</strong> — review latest findings</summary>
<br/>

No alternative fix was selected for this run. Review the session findings and CI results before merging.

</details>
"@
    }

    $selected = [string]$winner.winner
    $rationale = if ($winner.summary) { [string]$winner.summary } else { "Automated review identified a stronger candidate fix." }
    $diff = [string]$winner.candidateDiff
    $truncated = $false

    if ([string]::IsNullOrWhiteSpace($diff)) {
        $diff = "Candidate diff was not available in winner.json."
    } else {
        $maxDiffBytes = 55KB
        $marker = "`n... [truncated]"
        $markerBytes = [System.Text.Encoding]::UTF8.GetByteCount($marker)
        $budget = $maxDiffBytes - $markerBytes
        if ([System.Text.Encoding]::UTF8.GetByteCount($diff) -gt $maxDiffBytes) {
            $lo = 0
            $hi = $diff.Length
            while ($lo -lt $hi) {
                $mid = [int](($lo + $hi + 1) / 2)
                $bytes = [System.Text.Encoding]::UTF8.GetByteCount($diff.Substring(0, $mid))
                if ($bytes -le $budget) { $lo = $mid } else { $hi = $mid - 1 }
            }
            $diff = $diff.Substring(0, $lo) + $marker
            $truncated = $true
        }
    }

    $maxBacktickRun = 0
    foreach ($m in [regex]::Matches($diff, '`+')) {
        if ($m.Length -gt $maxBacktickRun) { $maxBacktickRun = $m.Length }
    }
    $fenceLen = [Math]::Max(4, $maxBacktickRun + 1)
    $fence = '`' * $fenceLen
    $truncatedNote = if ($truncated) { "`n_The diff was truncated to fit GitHub's review body limit._" } else { "" }

    return @"
---

<details>
<summary><strong>Future Action</strong> — alternative fix proposed (<code>$selected</code>)</summary>
<br/>

**Automated review — alternative fix proposed**

The expert-reviewer evaluation compared the PR fix against automatically generated candidates and selected <code>$selected</code> as the strongest fix.

**Why:** $rationale

Please consider applying the candidate diff below (or use it as guidance). Once you push an update, this workflow will re-trigger and re-evaluate.

<details><summary>Candidate diff (<code>$selected</code>)</summary>

${fence}diff
$diff
$fence

</details>
$truncatedNote

</details>
"@
}

function Test-HasNonPRWinner {
    param(
        [Parameter(Mandatory = $true)][string]$PRAgentDir
    )

    $winnerFile = Join-Path $PRAgentDir "winner.json"
    if (-not (Test-Path $winnerFile)) {
        return $false
    }

    try {
        $winner = Get-Content -Raw -LiteralPath $winnerFile -Encoding UTF8 | ConvertFrom-Json
        return ($winner.isPRFix -eq $false -and -not [string]::IsNullOrWhiteSpace([string]$winner.winner))
    } catch {
        return $false
    }
}

function Get-AIReviewEventForRun {
    param(
        [string]$ReportContent,

        [Parameter(Mandatory = $true)]
        [string]$PRAgentDir
    )

    $reviewEvent = Get-AIReviewEvent -ReportContent $ReportContent
    if ((Test-HasNonPRWinner -PRAgentDir $PRAgentDir) -and $reviewEvent -eq 'COMMENT') {
        return 'REQUEST_CHANGES'
    }

    return $reviewEvent
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

# ─── Gate content (rendered first, collapsed) ───
$gateSection = $null
$gateContent = $null
$gateFilePath = Join-Path $PRAgentDir "gate/content.md"
if (Test-Path $gateFilePath) {
    $gateContent = Get-Content $gateFilePath -Raw -Encoding UTF8
    if (-not [string]::IsNullOrWhiteSpace($gateContent)) {
        Write-Host "  ✅ gate ($((Get-Item $gateFilePath).Length) bytes)" -ForegroundColor Green
        $gateSection = @"
<details>
<summary><strong>Gate — Test Before & After Fix</strong></summary>
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
            $phaseTitle = $phase.Title
            if ($key -eq "uitests") {
                $catMatch = [regex]::Match($content, 'Detected UI test categories:\*\*\s*`{1,2}([^`]+)`{1,2}')
                if ($catMatch.Success) {
                    $phaseTitle = "$($phase.Title) — $($catMatch.Groups[1].Value)"
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

$reviewEvent = Get-AIReviewEventForRun -ReportContent $phaseContentByKey['report'] -PRAgentDir $PRAgentDir
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

# Combine gate (always first) with phases (collapsed). When only one
# kind of content is available, the session still renders cleanly.
$sessionParts = @()
if ($gateSection)            { $sessionParts += $gateSection }
if ($phaseSections.Count -gt 0) { $sessionParts += ($phaseSections -join "`n`n---`n`n") }
$phaseContent = $sessionParts -join "`n`n---`n`n"

$sessionMarkerStart = "<!-- SESSION:$commitSha7 START -->"
$sessionMarkerEnd = "<!-- SESSION:$commitSha7 END -->"

$newSessionBlock = @"
$sessionMarkerStart
<details>
<summary><strong>Review Sessions</strong> — click to expand</summary>
<br/>

$phaseContent

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
    $authorPing = "> @$prAuthor — new AI review results are available based on this last commit: <a href=`"$commitUrl`"><code>$commitSha7</code></a>.`n> **$commitTitle**"
    $authorPing += ' To request a fresh review after new comments or commits, comment `/review rerun`.'
}

$reviewStatus = switch ($reviewEvent) {
    'APPROVE' { 'LGTM' }
    'REQUEST_CHANGES' { 'Needs Changes' }
    default { 'In Review' }
}
$summaryContent = @($gateContent) + @($phaseContentByKey.Values)
$statusChipRow = New-StatusChipRow `
    -GateStatus (Get-GateStatus -GateContent $gateContent) `
    -ReviewStatus $reviewStatus `
    -Confidence (Get-ConfidenceStatus -Contents $summaryContent) `
    -Platform (Get-PlatformStatus -Contents $summaryContent)
$futureActionSection = New-FutureActionSection -PRAgentDir $PRAgentDir

$commentBody = @"
$MARKER

## AI Review Summary

$authorPing

$statusChipRow

$newSessionBlock

$futureActionSection
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

if (Get-Command Hide-StaleMauiBotIssueComments -ErrorAction SilentlyContinue) {
    Hide-StaleMauiBotIssueComments `
        -PRNumber $PRNumber `
        -IncludeAISummary `
        -IncludeLegacyGate `
        -IncludeMergeConflict `
        -IncludeTryFix `
        -Reason "stale generated PR review artifact"
}

if (Get-Command Hide-StaleMauiBotPullRequestReviews -ErrorAction SilentlyContinue) {
    Hide-StaleMauiBotPullRequestReviews `
        -PRNumber $PRNumber `
        -IncludeAISummary `
        -IncludeTryFix `
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
