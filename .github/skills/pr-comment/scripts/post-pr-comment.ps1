#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a phase completion comment on a GitHub Pull Request.

.DESCRIPTION
    Each PR agent phase gets its own comment that mirrors the state file structure.
    Comments use HTML markers (<!-- PR-AGENT-PHASE: phase-name -->) for identification.
    Prevents duplicate comments by checking existence before posting.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER Phase
    The phase: pre-flight, tests, gate, fix, or report (required)

.PARAMETER StateFile
    Path to the PR session state file (required)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    ./post-pr-comment-v2.ps1 -PRNumber 12345 -Phase pre-flight -StateFile .github/agent-pr-session/pr-12345.md
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$PRNumber,

    [Parameter(Mandatory=$true)]
    [ValidateSet("pre-flight", "tests", "gate", "fix", "report")]
    [string]$Phase,

    [Parameter(Mandatory=$true)]
    [string]$StateFile,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if (-not (Test-Path $StateFile)) {
    Write-Error "State file not found: $StateFile"
    exit 1
}

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  PR Comment - Phase: $($Phase.ToUpper().PadRight(35))║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Read state file
$stateContent = Get-Content -Path $StateFile -Raw

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
foreach ($comment in $existingComments) {
    if ($comment.body -match [regex]::Escape($phaseMarker)) {
        $existingCommentId = $comment.id
        Write-Host "✓ Found existing $Phase comment (ID: $existingCommentId)" -ForegroundColor Green
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
        $sectionContent = $match.Groups[1].Value.Trim()
        return "<details>`n<summary><strong>$SectionName</strong></summary>`n`n$sectionContent`n`n</details>"
    }
    return $null
}

# Build phase-specific content from state file
$commentBody = ""

switch ($Phase) {
    "pre-flight" {
        # Extract sections from state file
        $issueSummary = Extract-Section $stateContent "📋 Issue Summary"
        $filesChanged = Extract-Section $stateContent "📁 Files Changed"
        $prDiscussion = Extract-Section $stateContent "💬 PR Discussion Summary"
        
        $commentBody = @"
$phaseMarker
## 🔍 Pre-Flight: Context Gathering

**Status:** ✅ COMPLETE

$issueSummary

$filesChanged

$prDiscussion

---
*Phase completed - Tests phase next*
"@
    }
    
    "tests" {
        # Extract test section from state file
        $testsSection = Extract-Section $stateContent "🧪 Tests"
        
        $commentBody = @"
$phaseMarker
## 🧪 Tests: Verification

**Status:** ✅ COMPLETE

$testsSection

---
*Phase completed - Gate phase next*
"@
    }
    
    "gate" {
        # Extract gate section from state file
        $gateSection = Extract-Section $stateContent "🚦 Gate - Test Verification"
        
        $gateResult = "PENDING"
        if ($stateContent -match 'Result:\*\*\s*PASSED ✅') {
            $gateResult = "✅ PASSED"
        } elseif ($stateContent -match 'Result:\*\*\s*FAILED ❌') {
            $gateResult = "❌ FAILED"
        }
        
        $commentBody = @"
$phaseMarker
## 🚦 Gate: Test Validation

**Status:** $gateResult

$gateSection

---
*Phase completed - Fix phase next*
"@
    }
    
    "fix" {
        # Extract fix candidates section from state file
        $fixSection = Extract-Section $stateContent "🔧 Fix Candidates"
        
        $commentBody = @"
$phaseMarker
## 🔧 Fix: Analysis

**Status:** ✅ COMPLETE

$fixSection

---
*Phase completed - Report phase next*
"@
    }
    
    "report" {
        # Extract recommendation
        $recommendation = "PENDING"
        if ($stateContent -match 'Final Recommendation:\s*(APPROVE|REQUEST CHANGES)') {
            $recommendation = $matches[1]
        }
        
        $recommendationIcon = switch ($recommendation) {
            "APPROVE" { "✅" }
            "REQUEST CHANGES" { "⚠️" }
            default { "💬" }
        }
        
        # Extract comparison analysis if exists
        $comparisonAnalysis = ""
        if ($stateContent -match '\*\*Comparison Analysis:\*\*(.*?)(?=\*\*(?:Exhausted|Selected Fix):|\Z)') {
            $comparisonAnalysis = $matches[1].Trim()
        }
        
        $commentBody = @"
$phaseMarker
## 📋 Report: Final Recommendation

**Status:** $recommendationIcon $recommendation

### Review Summary

All phases completed:
- ✅ Pre-Flight: Context gathered
- ✅ Tests: Verified
- ✅ Gate: Tests validated
- ✅ Fix: Alternatives explored
- ✅ Report: Recommendation generated

### Comparison Analysis

$comparisonAnalysis

### Recommendation

**$recommendation** - See state file for full details: `.github/agent-pr-session/pr-$PRNumber.md`

---
*Review complete!*
"@
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
        Write-Host "⚠️  Comment already exists for $Phase phase - skipping duplicate" -ForegroundColor Yellow
        Write-Host "   To update, delete comment ID $existingCommentId first" -ForegroundColor Gray
        exit 0
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
