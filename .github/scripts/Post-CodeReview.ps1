#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts a formatted code review comment on a GitHub Pull Request, matching the AI Summary style.

.DESCRIPTION
    Maintains ONE comment per PR, identified by <!-- Code Review --> marker.
    Each review run adds an expandable session keyed by HEAD commit SHA.
    - Same commit SHA → replaces that session in-place.
    - New commit SHA  → prepends a new session (latest first).
    Older sessions stay collapsed; the newest is expanded by default.

    After posting, the PR author is @-mentioned so they know to review.

    Auto-detects verdict from content by scanning for "Verdict: LGTM", "Verdict: NEEDS_CHANGES",
    or "Verdict: NEEDS_DISCUSSION" and sets the appropriate colored status dot.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER ReviewFile
    Path to the review content markdown file (required unless -Content is used)

.PARAMETER Content
    Review content as a string (alternative to -ReviewFile)

.PARAMETER DryRun
    Print the formatted comment to stdout instead of posting

.PARAMETER Update
    Update an existing code review comment instead of creating a new one.
    This is now the default behavior — the flag is kept for backward compatibility.

.EXAMPLE
    ./Post-CodeReview.ps1 -PRNumber 12345 -ReviewFile /tmp/review.md

.EXAMPLE
    ./Post-CodeReview.ps1 -PRNumber 12345 -ReviewFile /tmp/review.md -DryRun
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$ReviewFile,

    [Parameter(Mandatory = $false)]
    [string]$Content,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Update
)

$ErrorActionPreference = "Stop"
$COMMENT_MARKER = "<!-- Code Review -->"

# ============================================================================
# INPUT VALIDATION
# ============================================================================

if (-not $ReviewFile -and -not $Content) {
    throw "Either -ReviewFile or -Content must be provided."
}

if ($ReviewFile -and -not (Test-Path $ReviewFile)) {
    throw "Review file not found: $ReviewFile"
}

if ($ReviewFile) {
    $Content = Get-Content -Path $ReviewFile -Raw -Encoding UTF8
}

if ([string]::IsNullOrWhiteSpace($Content)) {
    throw "Review content is empty."
}

# ============================================================================
# FETCH PR METADATA
# ============================================================================

Write-Host "📋 Fetching PR #$PRNumber metadata..." -ForegroundColor Cyan

$prJson = gh pr view $PRNumber --json title,headRefOid,author,number,url 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Failed to fetch PR #$PRNumber metadata: $prJson"
}

$pr = $prJson | ConvertFrom-Json
$prTitle = $pr.title
$prTitle = $prTitle -replace '&','&amp;' -replace '<','&lt;' -replace '>','&gt;'
$commitSha7 = $pr.headRefOid.Substring(0, 7)
$commitFull = $pr.headRefOid
$prAuthor = $pr.author.login
$prUrl = $pr.url

Write-Host "  PR: #$PRNumber - $prTitle" -ForegroundColor Gray
Write-Host "  Author: $prAuthor | HEAD: $commitSha7" -ForegroundColor Gray

$timestamp = (Get-Date).ToUniversalTime().ToString("yyyy-MM-dd HH:mm UTC")

# ============================================================================
# DETERMINE VERDICT
# ============================================================================

$verdictDot = "🔵"
$verdictLabel = "In Review"
if ($Content -match 'Verdict:\s*\*?\*?LGTM') {
    $verdictDot = "🟢"
    $verdictLabel = "Approved"
}
elseif ($Content -match 'Verdict:\s*\*?\*?NEEDS_CHANGES') {
    $verdictDot = "🟡"
    $verdictLabel = "Changes Suggested"
}
elseif ($Content -match 'Verdict:\s*\*?\*?NEEDS_DISCUSSION') {
    $verdictDot = "🟠"
    $verdictLabel = "Discussion Needed"
}

# ============================================================================
# BUILD NEW SESSION BLOCK
# ============================================================================

$sessionMarkerStart = "<!-- SESSION:$commitSha7 START -->"
$sessionMarkerEnd = "<!-- SESSION:$commitSha7 END -->"

$newSessionBlock = @"
$sessionMarkerStart
<details open>
<summary>$verdictDot <strong>Review Session — $verdictLabel</strong> — <a href="https://github.com/dotnet/maui/commit/$commitFull"><code>$commitSha7</code></a> · <strong>$prTitle</strong> · <em>$timestamp</em></summary>

---

$Content

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

    $sessionPattern = '(?s)<!-- SESSION:([a-f0-9]+) START -->.*?<!-- SESSION:\1 END -->'
    $existingSessions = [regex]::Matches($ExistingBody, $sessionPattern)

    $sessions = [ordered]@{}
    foreach ($match in $existingSessions) {
        $sha = $match.Groups[1].Value
        $sessions[$sha] = $match.Value
    }

    $sessions[$CommitSha7] = $NewSession

    $orderedKeys = @($CommitSha7) + @($sessions.Keys | Where-Object { $_ -ne $CommitSha7 })

    $allSessions = @()
    $isFirst = $true
    foreach ($sha in $orderedKeys) {
        $block = $sessions[$sha]
        if ($isFirst) {
            $block = $block -replace '<details(?:\s+open)?>', '<details open>'
            $isFirst = $false
        } else {
            $block = $block -replace '<details\s+open>', '<details>'
        }
        $allSessions += $block
    }

    return ($allSessions -join "`n`n---`n`n")
}

# ============================================================================
# FIND EXISTING COMMENT & BUILD FINAL BODY
# ============================================================================

Write-Host "🔍 Checking for existing code review comment..." -ForegroundColor Cyan
$existingCommentId = $null
$existingBody = $null

$existingRaw = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --paginate 2>$null
$existingObj = $null
if ($existingRaw) {
    try {
        $allComments = $existingRaw | ConvertFrom-Json
        $existingObj = @($allComments | Where-Object { $_.body -and $_.body.Contains($COMMENT_MARKER) }) | Select-Object -Last 1
    } catch {
        Write-Host "  ⚠️ Could not parse comments: $_" -ForegroundColor Yellow
    }
}

if ($existingObj -and $existingObj.id) {
    $existingCommentId = $existingObj.id
    $existingBody = $existingObj.body
    Write-Host "  ✓ Found existing comment (ID: $existingCommentId)" -ForegroundColor Green
}

$authorPing = ""
if ($prAuthor) {
    $authorPing = "> 👋 @$prAuthor — new code review results are available. Please review the latest session below."
}

if ($existingBody) {
    $mergedSessions = Merge-Sessions -ExistingBody $existingBody -NewSession $newSessionBlock -CommitSha7 $commitSha7

    $commentBody = @"
$COMMENT_MARKER

## $verdictDot .NET MAUI Code Review — $verdictLabel

$authorPing

$mergedSessions
"@
} else {
    $commentBody = @"
$COMMENT_MARKER

## $verdictDot .NET MAUI Code Review — $verdictLabel

$authorPing

$newSessionBlock
"@
}

$commentBody = $commentBody -replace "`n{4,}", "`n`n`n"

# ============================================================================
# DRY RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  DRY RUN — Comment preview (not posted)" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    Write-Host $commentBody
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  Comment length: $($commentBody.Length) chars" -ForegroundColor Gray
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    return
}

# ============================================================================
# POST OR UPDATE COMMENT
# ============================================================================

$tempFile = [System.IO.Path]::GetTempFileName()
$tempBodyFile = [System.IO.Path]::GetTempFileName()
try {
    # JSON format for gh api --input (PATCH)
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8
    # Plain text for gh pr comment --body-file (fallback/new)
    $commentBody | Set-Content -Path $tempBodyFile -Encoding UTF8

    if ($existingCommentId) {
        Write-Host "  Updating comment (ID: $existingCommentId)..." -ForegroundColor Gray
        try {
            gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input $tempFile 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "PATCH failed" }
            Write-Host ""
            Write-Host "✅ Code review comment updated on PR #$PRNumber" -ForegroundColor Green
        } catch {
            Write-Host "⚠️  Update failed, creating new comment instead..." -ForegroundColor Yellow
            $result = gh pr comment $PRNumber --body-file $tempBodyFile 2>&1
            if ($LASTEXITCODE -ne 0) { throw "Failed to post comment: $result" }
            Write-Host ""
            Write-Host "✅ Code review comment posted on PR #$PRNumber" -ForegroundColor Green
            Write-Host "   $result" -ForegroundColor Gray
        }
    } else {
        $result = gh pr comment $PRNumber --body-file $tempBodyFile 2>&1
        if ($LASTEXITCODE -ne 0) { throw "Failed to post comment: $result" }
        Write-Host ""
        Write-Host "✅ Code review comment posted on PR #$PRNumber" -ForegroundColor Green
        Write-Host "   $result" -ForegroundColor Gray
    }
} finally {
    Remove-Item -Path $tempFile -ErrorAction SilentlyContinue
    Remove-Item -Path $tempBodyFile -ErrorAction SilentlyContinue
}
