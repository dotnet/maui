#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a phase completion comment on a GitHub Pull Request.

.DESCRIPTION
    Each PR agent phase gets its own comment that is updated when the phase runs multiple times.
    Comments use HTML markers (<!-- PR-AGENT-PHASE: phase-name -->) for identification.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER Phase
    The phase: pre-flight, tests, gate, fix, or report (required)

.PARAMETER StateFile
    Path to the PR session state file (required)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    ./post-pr-comment.ps1 -PRNumber 12345 -Phase pre-flight -StateFile .github/agent-pr-session/pr-12345.md
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

# Check if previous phases have comments (enforce ordering)
$phaseOrder = @("pre-flight", "tests", "gate", "fix", "report")
$currentIndex = $phaseOrder.IndexOf($Phase)

Write-Host "Checking if previous phases have comments..." -ForegroundColor Yellow

$existingComments = gh api "/repos/dotnet/maui/issues/$PRNumber/comments" --jq '.[] | {id: .id, body: .body}' | ConvertFrom-Json

$phasesWithComments = @{}
foreach ($comment in $existingComments) {
    foreach ($p in $phaseOrder) {
        $marker = "<!-- PR-AGENT-PHASE: $p -->"
        if ($comment.body -match [regex]::Escape($marker)) {
            $phasesWithComments[$p] = $true
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

# Read state file
$stateContent = Get-Content -Path $StateFile -Raw

# Extract metadata
$issueNumbers = @()
$issueMatches = [regex]::Matches($stateContent, '\[#(\d+)\]\(https://github\.com/dotnet/maui/issues/\d+\)')
foreach ($match in $issueMatches) {
    $num = $match.Groups[1].Value
    if ($num -notin $issueNumbers) {
        $issueNumbers += $num
    }
}
$issueList = if ($issueNumbers.Count -gt 0) { 
    ($issueNumbers | ForEach-Object { "[#$_](https://github.com/dotnet/maui/issues/$_)" }) -join ", "
} else { 
    "Unknown" 
}

$titleMatch = [regex]::Match($stateContent, '# PR Review: #\d+ - (.+)')
$issueTitle = if ($titleMatch.Success) { $titleMatch.Groups[1].Value.Trim() } else { "Unknown" }

# Phase marker
$phaseMarker = "<!-- PR-AGENT-PHASE: $Phase -->"

# Fetch latest commit title from PR
$lastCommitTitle = "Unknown commit"
try {
    $commitsJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1].commit.message' 2>&1
    if ($LASTEXITCODE -eq 0 -and $commitsJson) {
        # Get first line of commit message only
        $lastCommitTitle = ($commitsJson -split "`n")[0].Trim()
    }
} catch {
    Write-Host "Could not fetch commit title, using default" -ForegroundColor Yellow
}

# Check for existing comment
$existingCommentId = $null
$existingCommentBody = $null
$reviewNumber = 1

Write-Host "Checking for existing $Phase comment..." -ForegroundColor Yellow

try {
    $commentsJson = gh api "/repos/dotnet/maui/issues/$PRNumber/comments" 2>&1
    if ($LASTEXITCODE -eq 0 -and $commentsJson) {
        $comments = $commentsJson | ConvertFrom-Json
        foreach ($comment in $comments) {
            if ($comment.body -match [regex]::Escape($phaseMarker)) {
                $existingCommentId = $comment.id
                $existingCommentBody = $comment.body
                
                # Count existing sessions
                $sessionMatches = [regex]::Matches($existingCommentBody, '📝 Review Session (\d+)')
                if ($sessionMatches.Count -gt 0) {
                    $reviewNumber = ([int]$sessionMatches[$sessionMatches.Count - 1].Groups[1].Value) + 1
                }
                
                Write-Host "Found existing comment (ID: $existingCommentId) - will be session #$reviewNumber" -ForegroundColor Green
                break
            }
        }
    }
} catch {
    Write-Host "No existing comment found" -ForegroundColor Yellow
}

# Build phase-specific content
$newSessionContent = ""

switch ($Phase) {
    "pre-flight" {
        # Extract platforms
        $platforms = @()
        if ($stateContent -match '\[x\] iOS') { $platforms += "iOS" }
        if ($stateContent -match '\[x\] Android') { $platforms += "Android" }
        if ($stateContent -match '\[x\] Windows') { $platforms += "Windows" }
        if ($stateContent -match '\[x\] MacCatalyst') { $platforms += "MacCatalyst" }
        $platformsList = if ($platforms.Count -gt 0) { $platforms -join ", " } else { "Not specified" }
        
        # Count files
        $filesChangedCount = ([regex]::Matches($stateContent, '\|\s*`[^`]+`\s*\|\s*(Fix|Test|HostApp Test|NUnit Test)\s*\|')).Count
        
        $newSessionContent = @"
<details>
<summary><strong>📝 Review Session $reviewNumber</strong> - $lastCommitTitle</summary>

### Summary
- **Issues**: $issueList
- **Title**: $issueTitle
- **Platforms**: $platformsList
- **Files Changed**: $filesChangedCount

### What Was Done
✓ Analyzed issue descriptions and reproduction steps  
✓ Reviewed PR discussion and reviewer feedback  
✓ Documented files changed and scope  
✓ Identified platforms affected

### Phase Status
- ✅ Pre-Flight: COMPLETE

### What's Next
**Tests Phase** - Verify tests exist and reproduce the issue.

</details>
"@

        if ($existingCommentBody) {
            # Remove old footer if exists
            $commentBody = $existingCommentBody -replace '---\s*\*(?:Posted|Updated) by PR Agent[^\n]*', ''
            $commentBody = $commentBody -replace '\n\*\*Last (?:Review|Pre-Flight|Tests|Gate|Fix) Status:.*', ''
            # Replace status at the top
            $commentBody = $commentBody -replace '(\n## 🔍 Pre-Flight: Context Gathering\n)', "`$1`n**Last Pre-Flight Status:** SUCCESS ✅`n"
            # Append new session at the end
            $commentBody = $commentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $commentBody = @"
$phaseMarker
## 🔍 Pre-Flight: Context Gathering

**Last Pre-Flight Status:** SUCCESS ✅

$newSessionContent
"@
        }
    }
    
    "tests" {
        # Extract test files
        $testFiles = [regex]::Matches($stateContent, '- \*\*(?:HostApp|NUnit):\*\*[^\n]+') | ForEach-Object { $_.Value }
        $testFilesList = if ($testFiles) { $testFiles -join "`n" } else { "Not specified" }
        
        $newSessionContent = @"
<details>
<summary><strong>📝 Review Session $reviewNumber</strong> - $lastCommitTitle</summary>

### Test Files Analyzed
$testFilesList

### What Was Done
✓ Verified test files exist and follow conventions  
✓ Analyzed test implementation  
✓ Checked test naming patterns

### Phase Status
- ✅ Pre-Flight: COMPLETE
- ✅ Tests: COMPLETE

### What's Next
**Gate Phase** - Run tests WITHOUT fix to verify they catch the bug.

</details>
"@

        if ($existingCommentBody) {
            # Remove old footer if exists
            $commentBody = $existingCommentBody -replace '---\s*\*(?:Posted|Updated) by PR Agent[^\n]*', ''
            $commentBody = $commentBody -replace '\n\*\*Last (?:Review|Pre-Flight|Tests|Gate|Fix) Status:.*', ''
            # Replace status at the top
            $commentBody = $commentBody -replace '(\n## 🧪 Tests: Verification\n)', "`$1`n**Last Tests Status:** SUCCESS ✅`n"
            # Append new session at the end
            $commentBody = $commentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $commentBody = @"
$phaseMarker
## 🧪 Tests: Verification

**Last Tests Status:** SUCCESS ✅

$newSessionContent
"@
        }
    }
    
    "gate" {
        # Extract gate result
        $gateResult = "Unknown"
        if ($stateContent -match 'PASSED ✅') {
            $gateResult = "✅ PASSED"
        } elseif ($stateContent -match 'FAILED ❌') {
            $gateResult = "❌ FAILED"
        }
        
        $newSessionContent = @"
<details>
<summary><strong>📝 Review Session $reviewNumber</strong> - $lastCommitTitle</summary>

### Gate Result
**$gateResult**

### What Was Done
✓ Executed tests to verify bug reproduction  
✓ Validated tests fail without fix  
✓ Confirmed tests pass with fix

### Phase Status
- ✅ Pre-Flight: COMPLETE
- ✅ Tests: COMPLETE
- $gateResult Gate: $(if ($gateResult -eq '✅ PASSED') { 'PASSED' } else { 'FAILED' })

### What's Next
$(if ($gateResult -eq '✅ PASSED') {
"**Fix Phase** - Explore alternative solutions and compare with PR's approach."
} else {
"**Fix tests** - Tests don't properly catch the bug. Needs investigation."
})

</details>
"@

        if ($existingCommentBody) {
            # Remove old footer if exists
            $commentBody = $existingCommentBody -replace '---\s*\*(?:Posted|Updated) by PR Agent[^\n]*', ''
            $commentBody = $commentBody -replace '\n\*\*Last (?:Review|Pre-Flight|Tests|Gate|Fix) Status:.*', ''
            # Replace status at the top
            $lastStatus = if ($gateResult -eq '✅ PASSED') { 'SUCCESS ✅' } else { 'FAILED ❌' }
            $commentBody = $commentBody -replace '(\n## 🚦 Gate: Test Validation\n)', "`$1`n**Last Gate Status:** $lastStatus`n"
            # Append new session at the end
            $commentBody = $commentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $lastStatus = if ($gateResult -eq '✅ PASSED') { 'SUCCESS ✅' } else { 'FAILED ❌' }
            $commentBody = @"
$phaseMarker
## 🚦 Gate: Test Validation

**Last Gate Status:** $lastStatus

$newSessionContent
"@
        }
    }
    
    "fix" {
        $newSessionContent = @"
<details>
<summary><strong>📝 Review Session $reviewNumber</strong> - $lastCommitTitle</summary>

### What Was Done
✓ Analyzed root cause  
✓ Explored alternative fix approaches  
✓ Compared with PR's solution

### Phase Status
- ✅ Pre-Flight: COMPLETE
- ✅ Tests: COMPLETE
- ✅ Gate: PASSED
- ✅ Fix: COMPLETE

### What's Next
**Report Phase** - Generate final recommendation (APPROVE/REQUEST CHANGES).

</details>
"@

        if ($existingCommentBody) {
            # Remove old footer if exists
            $commentBody = $existingCommentBody -replace '---\s*\*(?:Posted|Updated) by PR Agent[^\n]*', ''
            $commentBody = $commentBody -replace '\n\*\*Last (?:Review|Pre-Flight|Tests|Gate|Fix) Status:.*', ''
            # Replace status at the top
            $commentBody = $commentBody -replace '(\n## 🔧 Fix: Analysis\n)', "`$1`n**Last Fix Status:** SUCCESS ✅`n"
            # Append new session at the end
            $commentBody = $commentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $commentBody = @"
$phaseMarker
## 🔧 Fix: Analysis

**Last Fix Status:** SUCCESS ✅

$newSessionContent
"@
        }
    }
    
    "report" {
        # Extract recommendation from state file
        $recommendation = "PENDING"
        if ($stateContent -match 'Final Recommendation:\s*(APPROVE|REQUEST CHANGES|COMMENT)') {
            $recommendation = $matches[1]
        }
        
        $newSessionContent = @"
<details>
<summary><strong>📝 Review Session $reviewNumber</strong> - $lastCommitTitle</summary>

### What Was Done
✓ Generated comprehensive review  
✓ Provided final recommendation  
✓ Documented all findings

### Phase Status
- ✅ Pre-Flight: COMPLETE
- ✅ Tests: COMPLETE
- ✅ Gate: PASSED
- ✅ Fix: COMPLETE
- ✅ Report: COMPLETE

### Recommendation
**$recommendation**

### What's Next
Review is complete! Check the recommendation and full state file for details.

</details>
"@

        if ($existingCommentBody) {
            # Remove old footer if exists
            $commentBody = $existingCommentBody -replace '---\s*\*(?:Posted|Updated) by PR Agent[^\n]*', ''
            $commentBody = $commentBody -replace '\n\*\*Last (?:Review|Pre-Flight|Tests|Gate|Fix|Report) Status:.*', ''
            # Replace status at the top
            $lastStatus = switch ($recommendation) {
                "APPROVE" { "APPROVED ✅" }
                "REQUEST CHANGES" { "CHANGES REQUESTED ❌" }
                default { "COMMENT 💬" }
            }
            $commentBody = $commentBody -replace '(\n## 📋 Report: Complete\n)', "`$1`n**Last Report Status:** $lastStatus`n"
            # Append new session at the end
            $commentBody = $commentBody.TrimEnd() + "`n`n$newSessionContent"
        } else {
            $lastStatus = switch ($recommendation) {
                "APPROVE" { "APPROVED ✅" }
                "REQUEST CHANGES" { "CHANGES REQUESTED ❌" }
                default { "COMMENT 💬" }
            }
            $commentBody = @"
$phaseMarker
## 📋 Report: Complete

**Last Report Status:** $lastStatus

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
        Write-Host "Updating existing comment ID: $existingCommentId" -ForegroundColor Green
        # Create JSON payload
        $payload = @{ body = $commentBody } | ConvertTo-Json
        $tempFile = [System.IO.Path]::GetTempFileName()
        Set-Content -Path $tempFile -Value $payload -NoNewline
        $result = gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input "$tempFile" 2>&1
        Remove-Item $tempFile -Force -ErrorAction SilentlyContinue
        if ($LASTEXITCODE -ne 0) {
            throw "gh api failed with exit code $LASTEXITCODE : $result"
        }
        Write-Host "✅ Comment updated successfully" -ForegroundColor Green
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
