#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a try-fix attempts comment on a GitHub Issue or Pull Request.

.DESCRIPTION
    Creates ONE comment for all try-fix attempts with each attempt in a collapsible section.
    Uses HTML marker <!-- TRY-FIX-COMMENT --> for identification.
    
    If an existing try-fix comment exists, it will be EDITED with the new attempt added.
    Otherwise, a new comment will be created.
    
    Format:
    ## 🔧 Try-Fix Analysis for Issue #XXXXX
    <!-- TRY-FIX-COMMENT -->
    
    <details>
    <summary><b>Attempt #1: Approach Name ✅ PASS</b></summary>
    ... attempt details ...
    </details>
    
    <details>
    <summary><b>Attempt #2: Different Approach ❌ FAIL</b></summary>
    ... attempt details ...
    </details>

.PARAMETER IssueNumber
    The issue number to post comment on (required)

.PARAMETER AttemptNumber
    The attempt number (1, 2, 3, etc.) (required)

.PARAMETER Approach
    Brief description of the fix approach (required)

.PARAMETER RootCause
    Description of the root cause identified (optional for failed attempts)

.PARAMETER FilesChanged
    Markdown table or list of files changed (required)

.PARAMETER Status
    Status of the attempt: "Compiles", "Pass", "Fail" (required)

.PARAMETER Analysis
    Analysis of why it worked or failed (optional)

.PARAMETER CodeSnippet
    Code snippet showing the fix (optional)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    ./post-try-fix-comment.ps1 -IssueNumber 19560 -AttemptNumber 1 `
        -Approach "Change Shadow base class to StyleableElement" `
        -RootCause "Shadow inherits from Element which lacks styling support" `
        -FilesChanged "| File | Changes |`n|------|---------|`n| Shadow.cs | +1/-1 |" `
        -Status "Pass"
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$IssueNumber,

    [Parameter(Mandatory=$true)]
    [int]$AttemptNumber,

    [Parameter(Mandatory=$true)]
    [string]$Approach,

    [Parameter(Mandatory=$false)]
    [string]$RootCause,

    [Parameter(Mandatory=$true)]
    [string]$FilesChanged,

    [Parameter(Mandatory=$true)]
    [ValidateSet("Compiles", "Pass", "Fail")]
    [string]$Status,

    [Parameter(Mandatory=$false)]
    [string]$Analysis,

    [Parameter(Mandatory=$false)]
    [string]$CodeSnippet,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Try-Fix Comment (Post/Update)                            ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Status emoji mapping
$statusEmoji = switch ($Status) {
    "Pass" { "✅ PASS" }
    "Fail" { "❌ FAIL" }
    "Compiles" { "✅ Compiles" }
    default { $Status }
}

# Build the new attempt section (single-level collapsible)
$attemptSection = @"
<details>
<summary><b>Attempt #$AttemptNumber`: $Approach $statusEmoji</b></summary>

"@

if (-not [string]::IsNullOrWhiteSpace($RootCause)) {
    $attemptSection += @"
### Root Cause
$RootCause

"@
}

if (-not [string]::IsNullOrWhiteSpace($CodeSnippet)) {
    $attemptSection += @"
### Fix

``````diff
$CodeSnippet
``````

"@
}

$attemptSection += @"
### Files Changed

$FilesChanged

### Result
$statusEmoji
"@

if (-not [string]::IsNullOrWhiteSpace($Analysis)) {
    $attemptSection += "`n`n$Analysis"
}

$attemptSection += @"

</details>
"@

# Check for existing try-fix comment on the issue
Write-Host "`nChecking for existing try-fix comment on issue #$IssueNumber..." -ForegroundColor Yellow
$existingCommentId = $null
$existingBody = ""

try {
    # Get all comments and find one with our marker
    $commentsJson = gh api "repos/dotnet/maui/issues/$IssueNumber/comments" 2>$null
    $comments = $commentsJson | ConvertFrom-Json
    
    foreach ($comment in $comments) {
        if ($comment.body -match "<!-- TRY-FIX-COMMENT -->") {
            $existingCommentId = $comment.id
            $existingBody = $comment.body
            Write-Host "✓ Found existing try-fix comment (ID: $existingCommentId)" -ForegroundColor Green
            break
        }
    }
    
    if (-not $existingCommentId) {
        Write-Host "✓ No existing try-fix comment found - will create new" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✓ No existing try-fix comment found - will create new" -ForegroundColor Yellow
}

if ($existingCommentId) {
    # Update existing comment - add new attempt or replace existing one with same number
    $attemptPattern = "(?s)<details>\s*<summary><b>Attempt #$AttemptNumber`:.*?</details>"
    
    if ($existingBody -match $attemptPattern) {
        # Replace existing attempt with same number
        Write-Host "Replacing existing Attempt #$AttemptNumber..." -ForegroundColor Yellow
        $newBody = $existingBody -replace $attemptPattern, $attemptSection
    } else {
        # Add new attempt before the footer section
        Write-Host "Adding new Attempt #$AttemptNumber..." -ForegroundColor Yellow
        
        # Remove old recommendation section if present (legacy cleanup)
        $recPattern = "(?s)---\s*\n+### Recommendation\s*\n+.*?(?=---|\z)"
        $newBody = $existingBody -replace $recPattern, ""
        
        # Remove footer
        $footerPattern = "(?s)---\s*\n+<sub>.*?</sub>\s*$"
        $newBody = $newBody -replace $footerPattern, ""
        
        # Add the new attempt and footer
        $newBody = $newBody.TrimEnd() + "`n`n" + $attemptSection
    }
    
    # Ensure footer is present
    if ($newBody -notmatch "<sub>.*Generated by Copilot CLI") {
        $newBody = $newBody.TrimEnd() + @"

---
<sub>🤖 Generated by Copilot CLI try-fix skill</sub>
"@
    }
    
    $commentBody = $newBody
} else {
    # Create new comment
    Write-Host "Creating new try-fix comment..." -ForegroundColor Yellow
    $commentBody = @"
## 🔧 Try-Fix Analysis for Issue #$IssueNumber

<!-- TRY-FIX-COMMENT -->

$attemptSection

---
<sub>🤖 Generated by Copilot CLI try-fix skill</sub>
"@
}

if ($DryRun) {
    Write-Host "`n=== COMMENT PREVIEW ===" -ForegroundColor Yellow
    Write-Host $commentBody
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    exit 0
}

# Write to temp file to avoid shell escaping issues
$tempFile = [System.IO.Path]::GetTempFileName()
@{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

if ($existingCommentId) {
    Write-Host "Updating comment ID $existingCommentId..." -ForegroundColor Yellow
    $result = gh api --method PATCH "repos/dotnet/maui/issues/comments/$existingCommentId" --input $tempFile --jq '.html_url'
    Write-Host "✅ Comment updated: $result" -ForegroundColor Green
} else {
    Write-Host "Posting new comment to issue #$IssueNumber..." -ForegroundColor Yellow
    $result = gh api --method POST "repos/dotnet/maui/issues/$IssueNumber/comments" --input $tempFile --jq '.html_url'
    Write-Host "✅ Comment posted: $result" -ForegroundColor Green
}

Remove-Item $tempFile
