#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a try-fix attempts comment on a GitHub Pull Request.

.DESCRIPTION
    Creates ONE comment for all try-fix attempts wrapped in a collapsible "Expand Full Details" section.
    Uses HTML marker <!-- TRY-FIX-COMMENT --> for identification.
    
    If an existing try-fix comment exists, it will be updated with the new attempt.
    Otherwise, a new comment will be created.
    
    Format:
    ## 🔧 Try-Fix Attempts for Issue #XXXXX
    <!-- TRY-FIX-COMMENT -->
    <details>
    <summary>📊 Expand Full Details</summary>
      Issue link + all attempts as nested <details> sections
    </details>

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER IssueNumber
    The related issue number (required)

.PARAMETER AttemptNumber
    The attempt number (1, 2, 3, etc.) (required)

.PARAMETER Approach
    Brief description of the fix approach (required)

.PARAMETER RootCause
    Description of the root cause identified (required)

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
    ./post-try-fix-comment.ps1 -PRNumber 20133 -IssueNumber 19806 -AttemptNumber 1 `
        -Approach "LayoutExtensions Width Constraint" `
        -RootCause "ComputeFrame only constrains width for Fill alignment" `
        -FilesChanged "| File | Changes |`n|------|---------|`n| LayoutExtensions.cs | +17/-3 |" `
        -Status "Compiles"
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$PRNumber,

    [Parameter(Mandatory=$true)]
    [int]$IssueNumber,

    [Parameter(Mandatory=$true)]
    [int]$AttemptNumber,

    [Parameter(Mandatory=$true)]
    [string]$Approach,

    [Parameter(Mandatory=$true)]
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
    "Pass" { "✅ Pass" }
    "Fail" { "❌ Fail" }
    "Compiles" { "✅ Compiles" }
    default { $Status }
}

# Build the new attempt section
$attemptSection = @"
<details>
<summary><strong>🔧 Attempt #$AttemptNumber`: $Approach</strong> $statusEmoji</summary>

### Root Cause (Independently Identified)

$RootCause

"@

if (-not [string]::IsNullOrWhiteSpace($CodeSnippet)) {
    $attemptSection += @"

### Fix Approach

``````csharp
$CodeSnippet
``````

"@
}

$attemptSection += @"

### Files Changed
$FilesChanged

### Status
- $statusEmoji
"@

if (-not [string]::IsNullOrWhiteSpace($Analysis)) {
    $attemptSection += "`n- $Analysis"
}

$attemptSection += @"

</details>
"@

# Check for existing try-fix comment
Write-Host "`nChecking for existing try-fix comment..." -ForegroundColor Yellow
$existingComment = $null
$existingCommentId = $null
$existingBody = ""

try {
    $comments = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --jq '.[] | select(.body | contains("<!-- TRY-FIX-COMMENT -->")) | {id: .id, body: .body}' 2>$null
    if ($comments) {
        $existingComment = $comments | ConvertFrom-Json
        if ($existingComment) {
            $existingCommentId = $existingComment.id
            $existingBody = $existingComment.body
            Write-Host "✓ Found existing try-fix comment (ID: $existingCommentId)" -ForegroundColor Green
        }
    }
} catch {
    Write-Host "✓ No existing try-fix comment found" -ForegroundColor Yellow
}

if ($existingCommentId) {
    # Update existing comment - replace or add attempt section
    $attemptPattern = "(?s)<details>\s*<summary><strong>🔧 Attempt #$AttemptNumber`:.*?</details>"
    
    if ($existingBody -match $attemptPattern) {
        # Replace existing attempt with same number
        Write-Host "Replacing existing Attempt #$AttemptNumber..." -ForegroundColor Yellow
        $newBody = $existingBody -replace $attemptPattern, $attemptSection
    } else {
        # Add new attempt before the closing note (inside the outer wrapper)
        Write-Host "Adding new Attempt #$AttemptNumber..." -ForegroundColor Yellow
        # Match the closing pattern: "---\n\n*This fix was developed independently.*\n\n</details>"
        $closingPattern = "(?s)---\s*\n\s*\*This fix was developed independently\.\*\s*\n\s*</details>\s*$"
        if ($existingBody -match $closingPattern) {
            $replacement = @"
---

$attemptSection

---

*This fix was developed independently.*

</details>
"@
            $newBody = $existingBody -replace $closingPattern, $replacement
        } else {
            # Fallback: insert before final </details>
            $newBody = $existingBody -replace "</details>\s*$", "$attemptSection`n`n---`n`n*This fix was developed independently.*`n`n</details>"
        }
    }
    
    $commentBody = $newBody
} else {
    # Create new comment
    Write-Host "Creating new try-fix comment..." -ForegroundColor Yellow
    $commentBody = @"
## 🔧 Try-Fix Attempts for Issue #$IssueNumber

<!-- TRY-FIX-COMMENT -->

<details>
<summary>📊 <strong>Expand Full Details</strong></summary>

---

**Issue:** [#$IssueNumber](https://github.com/dotnet/maui/issues/$IssueNumber)

---

$attemptSection

---

*This fix was developed independently.*

</details>
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
    Write-Host "Posting new comment..." -ForegroundColor Yellow
    $result = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile --jq '.html_url'
    Write-Host "✅ Comment posted: $result" -ForegroundColor Green
}

Remove-Item $tempFile
