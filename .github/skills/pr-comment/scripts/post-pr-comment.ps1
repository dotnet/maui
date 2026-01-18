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

    [Parameter(Mandatory=$true)]
    [string]$Content,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

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

# Get latest commit for Review Session header
Write-Host "Fetching latest commit info..." -ForegroundColor Yellow
$commitJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' | ConvertFrom-Json
$commitTitle = ($commitJson.message -split "`n")[0]
$commitSha = $commitJson.sha.Substring(0, 7)
$commitUrl = "https://github.com/dotnet/maui/commit/$($commitJson.sha)"

# Extract phase content from state file
function Extract-PhaseContent {
    param([string]$StateContent, [string]$PhaseTitle)
    
    $pattern = "(?s)<details>\s*<summary><strong>$PhaseTitle</strong></summary>(.*?)</details>\s*(?=<details>|---|\Z)"
    if ($StateContent -match $pattern) {
        return $Matches[1].Trim()
    }
    return $null
}

$preFlightContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "📋 Issue Summary"
$testsContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "🧪 Tests"
$gateContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "🚦 Gate - Test Verification"
$fixContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "🔧 Fix Candidates"
$reportContent = Extract-PhaseContent -StateContent $Content -PhaseTitle "📋 Report"

# Helper function to wrap phase content in Review Session
function New-ReviewSession {
    param([string]$PhaseContent, [string]$CommitTitle, [string]$CommitSha, [string]$CommitUrl)
    
    if ([string]::IsNullOrWhiteSpace($PhaseContent)) {
        return ""
    }
    
    return @"

---

<details>
<summary>📝 <strong>Review Session</strong> — <strong>$CommitTitle</strong> · <a href="$CommitUrl"><code>$CommitSha</code></a></summary>

---

$PhaseContent

</details>
"@
}

# Build aggregated comment body
$commentBody = @"
<!-- PR-AGENT-REVIEW -->

## 🤖 PR Agent Review — $recommendation

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
$(New-ReviewSession -PhaseContent $preFlightContent -CommitTitle $commitTitle -CommitSha $commitSha -CommitUrl $commitUrl)
</details>

---

<details>
<summary><strong>🧪 Phase 2: Tests — Verification</strong></summary>
$(New-ReviewSession -PhaseContent $testsContent -CommitTitle $commitTitle -CommitSha $commitSha -CommitUrl $commitUrl)
</details>

---

<details>
<summary><strong>🚦 Phase 3: Gate — Test Verification</strong></summary>
$(New-ReviewSession -PhaseContent $gateContent -CommitTitle $commitTitle -CommitSha $commitSha -CommitUrl $commitUrl)
</details>

---

<details>
<summary><strong>🔧 Phase 4: Fix — Analysis & Comparison</strong></summary>
$(New-ReviewSession -PhaseContent $fixContent -CommitTitle $commitTitle -CommitSha $commitSha -CommitUrl $commitUrl)
</details>

---

<details>
<summary><strong>📋 Phase 5: Report — Final Recommendation</strong></summary>
$(New-ReviewSession -PhaseContent $reportContent -CommitTitle $commitTitle -CommitSha $commitSha -CommitUrl $commitUrl)
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
