#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates the gate verification comment on a GitHub Pull Request.

.DESCRIPTION
    Maintains ONE comment per PR, identified by <!-- AI Gate --> marker.
    Each gate run adds an expandable session keyed by HEAD commit SHA.
    - Same commit SHA → replaces that session in-place.
    - New commit SHA  → prepends a new session (latest first).
    Older sessions stay collapsed; the newest is expanded by default.

    After posting, the PR author is @-mentioned so they know to review.

    Reads content from CustomAgentLogsTmp/PRState/<PRNumber>/PRAgent/gate/content.md.

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

$sessionMarkerStart = "<!-- SESSION:$commitSha7 START -->"
$sessionMarkerEnd = "<!-- SESSION:$commitSha7 END -->"

$newSessionBlock = @"
$sessionMarkerStart
<details open>
<summary>🚦 <strong>Gate Session</strong> — <a href="$commitUrl"><code>$commitSha7</code></a> · <strong>$commitTitle</strong> · <em>$timestamp</em></summary>

---

$gateContent

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

Write-Host "Checking for existing gate comment..." -ForegroundColor Yellow
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
    $authorPing = "> 👋 @$prAuthor — new gate results are available. Please review the latest session below."
}

if ($existingBody) {
    $mergedSessions = Merge-Sessions -ExistingBody $existingBody -NewSession $newSessionBlock -CommitSha7 $commitSha7

    $commentBody = @"
$MARKER

## 🚦 Gate — Test Before and After Fix

$authorPing

$mergedSessions
"@
} else {
    $commentBody = @"
$MARKER

## 🚦 Gate — Test Before and After Fix

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
    Write-Host "=== GATE COMMENT PREVIEW ===" -ForegroundColor Cyan
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
        Write-Host "Updating gate comment (ID: $existingCommentId)..." -ForegroundColor Yellow
        try {
            gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input $tempFile 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) { throw "PATCH failed" }
            Write-Host "✅ Gate comment updated" -ForegroundColor Green
            Write-Output "COMMENT_ID=$existingCommentId"
        } catch {
            Write-Host "⚠️ Could not update comment $existingCommentId : $_" -ForegroundColor Yellow
            $botLogin = gh api user --jq .login 2>$null
            if ($botLogin) {
                $ownRaw = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --paginate 2>$null
                $ownCommentId = $null
                if ($ownRaw) {
                    try {
                        $ownAll = $ownRaw | ConvertFrom-Json
                        $ownMatch = @($ownAll | Where-Object { $_.body -and $_.body.Contains($MARKER) -and $_.user.login -eq $botLogin }) | Select-Object -Last 1
                        if ($ownMatch) { $ownCommentId = $ownMatch.id }
                    } catch { }
                }
                if ($ownCommentId -and $ownCommentId -ne "null") {
                    Write-Host "  Retrying with own comment (ID: $ownCommentId)..." -ForegroundColor Yellow
                    gh api --method PATCH "repos/dotnet/maui/issues/comments/$ownCommentId" --input $tempFile 2>&1 | Out-Null
                    if ($LASTEXITCODE -eq 0) {
                        Write-Host "✅ Gate comment updated (own comment)" -ForegroundColor Green
                        Write-Output "COMMENT_ID=$ownCommentId"
                        return
                    }
                }
            }
            Write-Host "  Creating new comment as fallback..." -ForegroundColor Yellow
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
