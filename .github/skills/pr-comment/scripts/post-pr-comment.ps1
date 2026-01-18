#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates the PR agent review comment on a GitHub Pull Request.

.DESCRIPTION
    Creates ONE comment for the entire PR review with all phases wrapped in an expandable section.
    Uses HTML marker <!-- PR-AGENT-REVIEW --> for identification.
    
    Format:
    ## 🤖 PR Agent Review — ✅ APPROVE
    <details><summary>📊 Expand Full Review</summary>
      Status table + all 5 phases as nested details
    </details>

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER Content
    The full state file content (required) - script extracts all phase content from this

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    # Post/update review comment
    ./post-pr-comment.ps1 -PRNumber 12345 -Content "$(cat .github/agent-pr-session/pr-12345.md)"
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$PRNumber,

    [Parameter(Mandatory=$false)]
    [string]$Content,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

# If Content is not provided as parameter, read from stdin
if ([string]::IsNullOrWhiteSpace($Content)) {
    $Content = $input | Out-String
}

if ([string]::IsNullOrWhiteSpace($Content)) {
    throw "Content parameter is required (provide via -Content or stdin)"
}

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  PR Agent Review Comment                                  ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Extract recommendation from state file
$recommendation = "IN PROGRESS"
if ($Content -match '##\s+✅\s+Final Recommendation:\s+APPROVE') {
    $recommendation = "✅ APPROVE"
} elseif ($Content -match '##\s+⚠️\s+Final Recommendation:\s+REQUEST CHANGES') {
    $recommendation = "⚠️ REQUEST CHANGES"
} elseif ($Content -match 'Final Recommendation:\s+APPROVE') {
    $recommendation = "✅ APPROVE"
} elseif ($Content -match 'Final Recommendation:\s+REQUEST CHANGES') {
    $recommendation = "⚠️ REQUEST CHANGES"
}

# Extract phase statuses from state file
$phaseStatuses = @{
    "Pre-Flight" = "⏳ PENDING"
    "Tests" = "⏳ PENDING"
    "Gate" = "⏳ PENDING"
    "Fix" = "⏳ PENDING"
    "Report" = "⏳ PENDING"
}

# Parse phase status table - match any status format
if ($Content -match '(?s)\|\s*Phase\s*\|\s*Status\s*\|.*?\n\|[\s-]+\|[\s-]+\|(.*?)(?=\n\n|---|\z)') {
    $tableContent = $Matches[1]
    $tableContent -split '\n' | ForEach-Object {
        if ($_ -match '\|\s*(.+?)\s*\|\s*(.+?)\s*\|') {
            $phaseName = $Matches[1].Trim() -replace '^🔍\s*', '' -replace '^🧪\s*', '' -replace '^🚦\s*', '' -replace '^🔧\s*', '' -replace '^📋\s*', ''
            $status = $Matches[2].Trim()
            if ($phaseStatuses.ContainsKey($phaseName)) {
                $phaseStatuses[$phaseName] = $status
            }
        }
    }
}

# Get latest commit for NEW Review Session header
Write-Host "Fetching latest commit info..." -ForegroundColor Yellow
$commitJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' | ConvertFrom-Json
$latestCommitTitle = ($commitJson.message -split "`n")[0]
$latestCommitSha = $commitJson.sha.Substring(0, 7)
$latestCommitUrl = "https://github.com/dotnet/maui/commit/$($commitJson.sha)"

# Extract phase content from state file
function Extract-PhaseContent {
    param([string]$StateContent, [string]$PhaseTitle)
    
    $pattern = "(?s)<details>\s*<summary><strong>$PhaseTitle</strong></summary>(.*?)</details>\s*(?=<details>|---|\Z)"
    if ($StateContent -match $pattern) {
        return $Matches[1].Trim()
    }
    return $null
}

# Helper function to create a NEW review session
function New-ReviewSession {
    param([string]$PhaseContent, [string]$CommitTitle, [string]$CommitSha, [string]$CommitUrl)
    
    if ([string]::IsNullOrWhiteSpace($PhaseContent)) {
        return ""
    }
    
    return @"
<details>
<summary>📝 <strong>Review Session</strong> — <strong>$CommitTitle</strong> · <a href="$CommitUrl"><code>$CommitSha</code></a></summary>

---

$PhaseContent

</details>
"@
}

# Helper function to extract existing review sessions from a phase
function Get-ExistingReviewSessions {
    param([string]$PhaseContent)
    
    if ([string]::IsNullOrWhiteSpace($PhaseContent)) {
        return @()
    }
    
    $sessions = @()
    $pattern = '(?s)<details>\s*<summary>📝.*?</summary>.*?</details>'
    $matches = [regex]::Matches($PhaseContent, $pattern)
    
    foreach ($match in $matches) {
        $sessions += $match.Value
    }
    
    return $sessions
}

# Helper function to combine existing sessions with new session
function Merge-ReviewSessions {
    param(
        [string[]]$ExistingSessions,
        [string]$NewSession
    )
    
    if ([string]::IsNullOrWhiteSpace($NewSession)) {
        return ""
    }
    
    $allSessions = @()
    $allSessions += $ExistingSessions
    $allSessions += $NewSession
    
    return ($allSessions -join "`n`n---`n`n")
}

# Extract content from state file (for NEW review session)
$preFlightContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "📋 Issue Summary"
$testsContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "🧪 Tests"
$gateContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "🚦 Gate - Test Verification"
$fixContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "🔧 Fix Candidates"
$reportContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "📋 Phase 5: Report — Final Recommendation"

# If Phase 5 not found with full title, try short title
if ([string]::IsNullOrWhiteSpace($reportContent)) {
    $reportContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "📋 Report"
}

# Fetch existing comment to preserve old review sessions
Write-Host "Checking for existing review comment..." -ForegroundColor Yellow
$existingComment = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --jq '.[] | select(.body | contains("<!-- PR-AGENT-REVIEW -->")) | {id: .id, body: .body}' | ConvertFrom-Json

$existingPreFlightSessions = @()
$existingTestsSessions = @()
$existingGateSessions = @()
$existingFixSessions = @()
$existingReportSessions = @()

if ($existingComment) {
    Write-Host "✓ Found existing review comment (ID: $($existingComment.id)) - extracting review sessions..." -ForegroundColor Green
    
    # Extract existing sessions from each phase
    if ($existingComment.body -match '(?s)<summary><strong>🔍 Phase 1: Pre-Flight.*?</strong></summary>(.*?)</details>\s*---\s*<details>') {
        $existingPreFlightSessions = Get-ExistingReviewSessions -PhaseContent $Matches[1]
    }
    if ($existingComment.body -match '(?s)<summary><strong>🧪 Phase 2: Tests.*?</strong></summary>(.*?)</details>\s*---\s*<details>') {
        $existingTestsSessions = Get-ExistingReviewSessions -PhaseContent $Matches[1]
    }
    if ($existingComment.body -match '(?s)<summary><strong>🚦 Phase 3: Gate.*?</strong></summary>(.*?)</details>\s*---\s*<details>') {
        $existingGateSessions = Get-ExistingReviewSessions -PhaseContent $Matches[1]
    }
    if ($existingComment.body -match '(?s)<summary><strong>🔧 Phase 4: Fix.*?</strong></summary>(.*?)</details>\s*---\s*<details>') {
        $existingFixSessions = Get-ExistingReviewSessions -PhaseContent $Matches[1]
    }
    if ($existingComment.body -match '(?s)<summary><strong>📋 Phase 5: Report.*?</strong></summary>(.*?)</details>\s*---\s*') {
        $existingReportSessions = Get-ExistingReviewSessions -PhaseContent $Matches[1]
    }
} else {
    Write-Host "✓ No existing comment found - creating new..." -ForegroundColor Yellow
}

# Create NEW review sessions from current state file
$newPreFlightSession = New-ReviewSession -PhaseContent $preFlightContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl
$newTestsSession = New-ReviewSession -PhaseContent $testsContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl
$newGateSession = New-ReviewSession -PhaseContent $gateContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl
$newFixSession = New-ReviewSession -PhaseContent $fixContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl
$newReportSession = New-ReviewSession -PhaseContent $reportContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl

# Merge existing sessions with new session (if new content exists)
$allPreFlightSessions = if ($newPreFlightSession) { Merge-ReviewSessions -ExistingSessions $existingPreFlightSessions -NewSession $newPreFlightSession } else { $existingPreFlightSessions -join "`n`n---`n`n" }
$allTestsSessions = if ($newTestsSession) { Merge-ReviewSessions -ExistingSessions $existingTestsSessions -NewSession $newTestsSession } else { $existingTestsSessions -join "`n`n---`n`n" }
$allGateSessions = if ($newGateSession) { Merge-ReviewSessions -ExistingSessions $existingGateSessions -NewSession $newGateSession } else { $existingGateSessions -join "`n`n---`n`n" }
$allFixSessions = if ($newFixSession) { Merge-ReviewSessions -ExistingSessions $existingFixSessions -NewSession $newFixSession } else { $existingFixSessions -join "`n`n---`n`n" }
$allReportSessions = if ($newReportSession) { Merge-ReviewSessions -ExistingSessions $existingReportSessions -NewSession $newReportSession } else { $existingReportSessions -join "`n`n---`n`n" }

# Add placeholder for phases with no sessions
if ([string]::IsNullOrWhiteSpace($allPreFlightSessions)) { $allPreFlightSessions = "_No review sessions yet_" }
if ([string]::IsNullOrWhiteSpace($allTestsSessions)) { $allTestsSessions = "_No review sessions yet_" }
if ([string]::IsNullOrWhiteSpace($allGateSessions)) { $allGateSessions = "_No review sessions yet_" }
if ([string]::IsNullOrWhiteSpace($allFixSessions)) { $allFixSessions = "_No review sessions yet_" }
if ([string]::IsNullOrWhiteSpace($allReportSessions)) { $allReportSessions = "_No review sessions yet_" }

# Build aggregated comment body
$commentBody = @"
<!-- PR-AGENT-REVIEW -->

## 🤖 PR Agent Review

<details>
<summary>📊 <strong>Expand Full Review</strong></summary>

---

**Status:** $recommendation

| Phase | Status |
|-------|--------|
| 🔍 Pre-Flight | $($phaseStatuses['Pre-Flight']) |
| 🧪 Tests | $($phaseStatuses['Tests']) |
| 🚦 Gate | $($phaseStatuses['Gate']) |
| 🔧 Fix | $($phaseStatuses['Fix']) |
| 📋 Report | $($phaseStatuses['Report']) |

---

<details>
<summary><strong>🔍 Phase 1: Pre-Flight — Context & Validation</strong></summary>

---

$allPreFlightSessions

</details>

---

<details>
<summary><strong>🧪 Phase 2: Tests — Verification</strong></summary>

---

$allTestsSessions

</details>

---

<details>
<summary><strong>🚦 Phase 3: Gate — Test Verification</strong></summary>

---

$allGateSessions

</details>

---

<details>
<summary><strong>🔧 Phase 4: Fix — Analysis & Comparison</strong></summary>

---

$allFixSessions

</details>

---

<details>
<summary><strong>📋 Phase 5: Report — Final Recommendation</strong></summary>

---

$allReportSessions

</details>

---

**Review Complete** — All phases passed. PR is ready for merge pending CI validation.

</details>
"@

if ($DryRun) {
    Write-Host "`n=== COMMENT PREVIEW ===" -ForegroundColor Yellow
    Write-Host $commentBody
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    exit 0
}

# Check if aggregated comment already exists
Write-Host "Checking for existing review comment..." -ForegroundColor Yellow
$existingComments = gh api "/repos/dotnet/maui/issues/$PRNumber/comments" --jq '.[] | select(.body | contains("<!-- PR-AGENT-REVIEW -->")) | {id: .id, body: .body}' | ConvertFrom-Json

if ($existingComments) {
    if ($existingComments -is [System.Array]) {
        $commentToUpdate = $existingComments[0]
    } else {
        $commentToUpdate = $existingComments
    }
    
    Write-Host "✓ Found existing review comment (ID: $($commentToUpdate.id)) - updating..." -ForegroundColor Green
    
    # Create temp file for update
    $tempFile = [System.IO.Path]::GetTempFileName()
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8
    
    gh api --method PATCH "repos/dotnet/maui/issues/comments/$($commentToUpdate.id)" --input $tempFile | Out-Null
    Remove-Item $tempFile
    
    Write-Host "✅ Review comment updated successfully" -ForegroundColor Green
} else {
    Write-Host "Creating new review comment..." -ForegroundColor Yellow
    
    # Create temp file for new comment
    $tempFile = [System.IO.Path]::GetTempFileName()
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8
    
    gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile | Out-Null
    Remove-Item $tempFile
    
    Write-Host "✅ Review comment posted successfully" -ForegroundColor Green
}
