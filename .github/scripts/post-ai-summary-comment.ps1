#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates the AI review summary comment on a GitHub Pull Request.

.DESCRIPTION
    Maintains ONE comment per PR, identified by <!-- AI Summary --> marker.
    Each review run adds an expandable session keyed by HEAD commit SHA.
    - Same commit SHA → replaces that session in-place.
    - New commit SHA  → prepends a new session (latest first).
    Older sessions stay collapsed; the newest is expanded by default.

    After posting, the PR author is @-mentioned so they know to review.

    Content is auto-loaded from PRAgent phase files:
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/gate/content.md          (always shown first, open)
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/{pre-flight,try-fix,report}/content.md
    CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/pre-flight/code-review.md

    Gate is included as a section inside this unified comment — the script may
    be called by Review-PR.ps1 twice per run: once after the gate completes
    (gate-only update) and once after the review phases finish (full update).

    Any standalone legacy "<!-- AI Gate -->" comment from older versions of
    the script is deleted after a successful post to avoid duplicates.

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
    "pre-flight"  = @{ File = "pre-flight/content.md";     Icon = "🔍"; Title = "Pre-Flight — Context & Validation" }
    "code-review" = @{ File = "pre-flight/code-review.md"; Icon = "🔬"; Title = "Code Review — Deep Analysis" }
    "try-fix"     = @{ File = "try-fix/content.md";        Icon = "🔧"; Title = "Fix — Analysis & Comparison" }
    "report"      = @{ File = "report/content.md";         Icon = "📋"; Title = "Report — Final Recommendation" }
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

---

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

foreach ($key in $phases.Keys) {
    $phase = $phases[$key]
    $filePath = Join-Path $PRAgentDir $phase.File

    if (Test-Path $filePath) {
        $content = Get-Content $filePath -Raw -Encoding UTF8
        if (-not [string]::IsNullOrWhiteSpace($content)) {
            Write-Host "  ✅ $key ($((Get-Item $filePath).Length) bytes)" -ForegroundColor Green
            $phaseSections += @"
<details>
<summary><strong>$($phase.Icon) $($phase.Title)</strong></summary>

---

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

---

$phaseContent

---

</details>
$sessionMarkerEnd
"@

# ============================================================================
# MERGE WITH EXISTING SESSIONS
# ============================================================================

function Merge-Sessions {
    param(
        [string]$ExistingBody,
        [string]$NewSession,
        [string]$CommitSha7
    )

    # Extract all session blocks from existing body
    $sessionPattern = '(?s)<!-- SESSION:([a-f0-9]+) START -->.*?<!-- SESSION:\1 END -->'
    $existingSessions = [regex]::Matches($ExistingBody, $sessionPattern)

    $sessions = [ordered]@{}
    foreach ($match in $existingSessions) {
        $sha = $match.Groups[1].Value
        $sessions[$sha] = $match.Value
    }

    # Replace or prepend new session
    $sessions[$CommitSha7] = $NewSession

    # Rebuild: newest session first (the one we just added/replaced)
    $orderedKeys = @($CommitSha7) + @($sessions.Keys | Where-Object { $_ -ne $CommitSha7 })

    $allSessions = @()
    $isFirst = $true
    foreach ($sha in $orderedKeys) {
        $block = $sessions[$sha]
        if ($isFirst) {
            # Ensure ONLY the outer (session-wrapping) details tag is open. Inner
            # phase tags must keep their original open/collapsed state — we used
            # to re-open all of them via a global regex replace, which forced
            # every phase to expand on each new session.
            $rx = [regex]::new('<details(?:\s+open)?>')
            $block = $rx.Replace($block, '<details open>', 1)
            $isFirst = $false
        } else {
            # Collapse the outer details of older sessions; leave inner phases alone.
            $rx = [regex]::new('<details\s+open>')
            $block = $rx.Replace($block, '<details>', 1)
        }
        $allSessions += $block
    }

    return ($allSessions -join "`n`n---`n`n")
}

# ============================================================================
# FIND EXISTING COMMENT & BUILD FINAL BODY
# ============================================================================

Write-Host "Checking for existing review comment..." -ForegroundColor Yellow
$existingCommentId = $null
$existingBody = $null

$existingRaw = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --paginate 2>$null
$existingObj = $null
if ($existingRaw) {
    try {
        $allComments = $existingRaw | ConvertFrom-Json
        $existingObj = @($allComments | Where-Object { $_.body -and $_.body.Contains($MARKER) }) | Select-Object -Last 1
    } catch {
        Write-Host "⚠️ Could not parse comments: $_" -ForegroundColor Yellow
    }
}

if ($existingObj -and $existingObj.id) {
    $existingCommentId = $existingObj.id
    $existingBody = $existingObj.body
    Write-Host "✓ Found existing comment (ID: $existingCommentId)" -ForegroundColor Green
}

$authorPing = ""
if ($prAuthor) {
    $authorPing = "> 👋 @$prAuthor — new AI review results are available. Please review the latest session below."
}

if ($existingBody) {
    # Merge new session into existing body
    $mergedSessions = Merge-Sessions -ExistingBody $existingBody -NewSession $newSessionBlock -CommitSha7 $commitSha7

    # Preserve any PR-FINALIZE section that may already exist
    $finalizeSection = ""
    $finalizePattern = '(?s)(<!-- SECTION:PR-FINALIZE -->.*?<!-- /SECTION:PR-FINALIZE -->)'
    if ($existingBody -match $finalizePattern) {
        $finalizeSection = "`n`n" + $Matches[1]
    }

    $commentBody = @"
$MARKER

## 🤖 AI Summary

$authorPing

$mergedSessions$finalizeSection
"@
} else {
    $commentBody = @"
$MARKER

## 🤖 AI Summary

$authorPing

$newSessionBlock
"@
}

# Clean up excessive blank lines
$commentBody = $commentBody -replace "`n{4,}", "`n`n`n"

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

$tempFile = [System.IO.Path]::GetTempFileName()
try {
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

    if ($existingCommentId) {
        Write-Host "Updating comment (ID: $existingCommentId)..." -ForegroundColor Yellow
        try {
            gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input $tempFile 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "PATCH failed" }
            Write-Host "✅ Review comment updated" -ForegroundColor Green
            Write-Output "COMMENT_ID=$existingCommentId"
        } catch {
            Write-Host "⚠️ Could not update comment $existingCommentId : $_" -ForegroundColor Yellow
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

# ============================================================================
# CLEAN UP LEGACY STANDALONE GATE COMMENTS
# ============================================================================
# Earlier versions of this workflow posted gate results in a separate comment
# marked with <!-- AI Gate -->. Now that the gate is included as a section in
# this unified comment, those legacy comments are duplicates and should go.

try {
    $legacyMarker = "<!-- AI Gate -->"
    $allRaw = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --paginate 2>$null
    if ($allRaw) {
        $allComments = $allRaw | ConvertFrom-Json
        $legacy = @($allComments | Where-Object { $_.body -and $_.body.Contains($legacyMarker) })
        foreach ($lc in $legacy) {
            Write-Host "🧹 Deleting legacy gate comment (ID: $($lc.id))..." -ForegroundColor Gray
            gh api --method DELETE "repos/dotnet/maui/issues/comments/$($lc.id)" 2>&1 | Out-Null
        }
    }
} catch {
    Write-Host "⚠️ Legacy gate-comment cleanup failed (non-fatal): $_" -ForegroundColor Yellow
}
