#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a phase completion comment on a GitHub Pull Request.

.DESCRIPTION
    Each PR agent phase gets its own comment with collapsible Review Session details.
    Comments use HTML markers (<!-- PR-AGENT-PHASE: phase-name -->) for identification.
    Prevents duplicate comments and supports appending multiple review sessions.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER Phase
    The phase: pre-flight, tests, gate, fix, or report (required)

.PARAMETER Content
    The content to post in the review session (required)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    # Post pre-flight phase content
    ./post-pr-comment.ps1 -PRNumber 12345 -Phase pre-flight -Content "Issue summary content..."
    
.EXAMPLE
    # Dry run to preview
    ./post-pr-comment.ps1 -PRNumber 12345 -Phase gate -Content "Test results..." -DryRun
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$PRNumber,

    [Parameter(Mandatory=$true)]
    [ValidateSet("pre-flight", "tests", "gate", "fix", "report")]
    [string]$Phase,

    [Parameter(Mandatory=$true)]
    [string]$Content,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  PR Comment - Phase: $($Phase.ToUpper().PadRight(35))║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Phase marker
$phaseMarker = "<!-- PR-AGENT-PHASE: $Phase -->"

# Fetch existing comments ONCE
Write-Host "Fetching existing comments..." -ForegroundColor Yellow
$existingComments = @()
try {
    $commentsJson = gh api "/repos/dotnet/maui/issues/$PRNumber/comments" 2>&1
    if ($LASTEXITCODE -eq 0 -and $commentsJson) {
        $existingComments = $commentsJson | ConvertFrom-Json
    }
} catch {
    Write-Host "Could not fetch comments: $_" -ForegroundColor Yellow
}

# Check if this phase already has a comment
$existingCommentId = $null
$existingCommentBody = $null
$reviewNumber = 1

foreach ($comment in $existingComments) {
    if ($comment.body -match [regex]::Escape($phaseMarker)) {
        $existingCommentId = $comment.id
        $existingCommentBody = $comment.body
        
        # Count existing sessions to determine next session number
        $sessionMatches = [regex]::Matches($existingCommentBody, '📝 Review Session (\d+)')
        if ($sessionMatches.Count -gt 0) {
            $reviewNumber = ([int]$sessionMatches[$sessionMatches.Count - 1].Groups[1].Value) + 1
        }
        
        Write-Host "✓ Found existing $Phase comment (ID: $existingCommentId) - will append session #$reviewNumber" -ForegroundColor Green
        break
    }
}

# Check if previous phases have comments (enforce ordering)
$phaseOrder = @("pre-flight", "tests", "gate", "fix", "report")
$currentIndex = $phaseOrder.IndexOf($Phase)

$phasesWithComments = @{}
foreach ($comment in $existingComments) {
    foreach ($p in $phaseOrder) {
        $marker = "<!-- PR-AGENT-PHASE: $p -->"
        if ($comment.body -match [regex]::Escape($marker)) {
            $phasesWithComments[$p] = $comment.id
        }
    }
}

# Check all previous phases
$missingPhases = @()
for ($i = 0; $i -lt $currentIndex; $i++) {
    $requiredPhase = $phaseOrder[$i]
    if (-not $phasesWithComments.ContainsKey($requiredPhase)) {
        $missingPhases += $requiredPhase
    }
}

if ($missingPhases.Count -gt 0) {
    Write-Host "⚠️  Missing comments for previous phases: $($missingPhases -join ', ')" -ForegroundColor Yellow
    Write-Host "📝 Posting missing phase comments first..." -ForegroundColor Cyan
    
    foreach ($missingPhase in $missingPhases) {
        Write-Host "   → Posting $missingPhase comment..." -ForegroundColor Gray
        & $PSCommandPath -PRNumber $PRNumber -Phase $missingPhase -StateFile $StateFile
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to post $missingPhase comment"
            exit 1
        }
    }
    
    Write-Host "✅ All previous phase comments posted" -ForegroundColor Green
    Write-Host ""
    Write-Host "Now posting $Phase comment..." -ForegroundColor Cyan
}

# Helper function to extract section from state file
function Extract-Section {
    param([string]$Content, [string]$SectionName)
    
    $pattern = "<details>\s*<summary><strong>$([regex]::Escape($SectionName))</strong></summary>(.*?)</details>"
    $match = [regex]::Match($Content, $pattern, [System.Text.RegularExpressions.RegexOptions]::Singleline)
    
    if ($match.Success) {
        return $match.Groups[1].Value.Trim()
    }
    return $null
}

# Fetch latest commit title and SHA from PR
$lastCommitTitle = "Unknown commit"
$lastCommitLink = ""
try {
    $lastCommitJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' 2>&1
    if ($LASTEXITCODE -eq 0 -and $lastCommitJson) {
        $commitData = $lastCommitJson | ConvertFrom-Json
        # Get first line of commit message only
        $lastCommitTitle = ($commitData.message -split "`n")[0].Trim()
        $lastCommitSha = $commitData.sha.Substring(0, 7)  # Short SHA
        # Use HTML for summary tag (markdown doesn't work inside <summary>)
        $lastCommitLink = "<strong>$lastCommitTitle</strong> · <a href=`"https://github.com/dotnet/maui/commit/$($commitData.sha)`"><code>$lastCommitSha</code></a>"
    }
} catch {
    Write-Host "Could not fetch commit info, using default" -ForegroundColor Yellow
    $lastCommitLink = "<strong>$lastCommitTitle</strong>"
}

# Build phase-specific content - extract from state file and store in comment
$newSessionContent = ""

switch ($Phase) {
    "pre-flight" {
        # Extract sections from state file
        $issueSummary = Extract-Section $Content "📋 Issue Summary"
        $filesChanged = Extract-Section $Content "📁 Files Changed"
        $prDiscussion = Extract-Section $Content "💬 PR Discussion Summary"
        
        $newSessionContent = @"
<details>
<summary>📝 <strong>Review Session $reviewNumber</strong> — $lastCommitLink</summary>

---

<details>
<summary><strong>📋 Issue Summary</strong></summary>

$issueSummary

</details>

<details>
<summary><strong>📁 Files Changed</strong></summary>

$filesChanged

</details>

<details>
<summary><strong>💬 PR Discussion Summary</strong></summary>

$prDiscussion

</details>

</details>
"@

        if ($existingCommentBody) {
            # Append new session to existing comment
            $commentBody = $existingCommentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $commentBody = @"
$phaseMarker
## 🔍 Pre-Flight — Context & Validation

**Last Pre-Flight Status:** ✅ **SUCCESS**

---

$newSessionContent
"@
        }
    }
    
    "tests" {
        # Extract test section from state file
        $testsContent = Extract-Section $Content "🧪 Tests"
        
        $newSessionContent = @"
<details>
<summary>📝 <strong>Review Session $reviewNumber</strong> — $lastCommitLink</summary>

---

$testsContent

</details>
"@

        if ($existingCommentBody) {
            $commentBody = $existingCommentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $commentBody = @"
$phaseMarker
## 🧪 Tests — Verification

**Last Tests Status:** ✅ **SUCCESS**

---

$newSessionContent
"@
        }
    }
    
    "gate" {
        # Extract gate section from state file
        $gateContent = Extract-Section $Content "🚦 Gate - Test Verification"
        
        $gateResult = "PENDING"
        if ($Content -match 'Result:\*\*\s*PASSED ✅') {
            $gateResult = "✅ **PASSED**"
        } elseif ($Content -match 'Result:\*\*\s*FAILED ❌') {
            $gateResult = "❌ **FAILED**"
        }
        
        $newSessionContent = @"
<details>
<summary>📝 <strong>Review Session $reviewNumber</strong> — $lastCommitLink</summary>

---

$gateContent

**Result:** $gateResult

</details>
"@

        if ($existingCommentBody) {
            $commentBody = $existingCommentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $lastStatus = if ($gateResult -eq '✅ **PASSED**') { '✅ **SUCCESS**' } else { '❌ **FAILED**' }
            $commentBody = @"
$phaseMarker
## 🚦 Gate — Test Validation

**Last Gate Status:** $lastStatus

---

$newSessionContent
"@
        }
    }
    
    "fix" {
        # Extract fix candidates section from state file
        $fixContent = Extract-Section $Content "🔧 Fix Candidates"
        
        $newSessionContent = @"
<details>
<summary>📝 <strong>Review Session $reviewNumber</strong> — $lastCommitLink</summary>

---

$fixContent

</details>
"@

        if ($existingCommentBody) {
            $commentBody = $existingCommentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $commentBody = @"
$phaseMarker
## 🔧 Fix — Analysis

**Last Fix Status:** ✅ **SUCCESS**

---

$newSessionContent
"@
        }
    }
    
    "report" {
        # Extract recommendation
        $recommendation = "PENDING"
        if ($Content -match 'Final Recommendation:\s*(APPROVE|REQUEST CHANGES)') {
            $recommendation = $matches[1]
        }
        
        $recommendationIcon = switch ($recommendation) {
            "APPROVE" { "✅" }
            "REQUEST CHANGES" { "⚠️" }
            default { "💬" }
        }
        
        # Extract comparison analysis if exists
        $comparisonAnalysis = ""
        if ($Content -match '\*\*Comparison Analysis:\*\*(.*?)(?=\*\*(?:Exhausted|Selected Fix):|\Z)') {
            $comparisonAnalysis = $matches[1].Trim()
        }
        
        # Extract selected fix
        $selectedFix = ""
        if ($Content -match '\*\*Selected Fix:\*\*\s*(.+?)(?=\n\n|\</details>|\Z)') {
            $selectedFix = $matches[1].Trim()
        }
        
        $newSessionContent = @"
<details>
<summary>📝 <strong>Review Session $reviewNumber</strong> — $lastCommitLink</summary>

---

### Final Recommendation: $recommendationIcon **$recommendation**

**Comparison Analysis:**

$comparisonAnalysis

**Selected Fix:**

$selectedFix

</details>
"@

        if ($existingCommentBody) {
            $commentBody = $existingCommentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $lastStatus = switch ($recommendation) {
                "APPROVE" { "✅ **APPROVED**" }
                "REQUEST CHANGES" { "❌ **CHANGES REQUESTED**" }
                default { "💬 **COMMENT**" }
            }
            $commentBody = @"
$phaseMarker
## 📋 Report — Complete

**Last Report Status:** $lastStatus

---

$newSessionContent
"@
        }
    }
}

# Post or update comment
if ($DryRun) {
    Write-Host "`n=== DRY RUN - Comment content ===" -ForegroundColor Magenta
    Write-Host $commentBody
    Write-Host "`n=== END ===" -ForegroundColor Magenta
    exit 0
}

try {
    if ($existingCommentId) {
        Write-Host "Updating existing comment ID: $existingCommentId with session #$reviewNumber" -ForegroundColor Green
        # Create JSON payload
        $payload = @{ body = $commentBody } | ConvertTo-Json
        $tempFile = [System.IO.Path]::GetTempFileName()
        Set-Content -Path $tempFile -Value $payload -NoNewline
        $result = gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input "$tempFile" 2>&1
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
        if ($LASTEXITCODE -ne 0) {
            throw "gh api failed with exit code $LASTEXITCODE : $result"
        }
        Write-Host "✅ Comment updated with new session" -ForegroundColor Green
    } else {
        Write-Host "Creating new comment for PR #$PRNumber" -ForegroundColor Green
        $tempFile = [System.IO.Path]::GetTempFileName()
        Set-Content -Path $tempFile -Value $commentBody -NoNewline
        $result = gh pr comment $PRNumber --body-file $tempFile --repo dotnet/maui 2>&1
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
        if ($LASTEXITCODE -ne 0) {
            throw "gh pr comment failed with exit code $LASTEXITCODE : $result"
        }
        Write-Host "✅ Comment posted successfully" -ForegroundColor Green
    }
} catch {
    Write-Error "Failed to post/update comment: $_"
    exit 1
}
