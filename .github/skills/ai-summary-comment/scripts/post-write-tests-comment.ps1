#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a write-tests comment on a GitHub Issue or Pull Request.

.DESCRIPTION
    Creates ONE comment for all test-writing attempts with each attempt in a collapsible section.
    Uses HTML marker <!-- WRITE-TESTS-COMMENT --> for identification.
    
    **NEW: Auto-loads from CustomAgentLogsTmp/PRState/{IssueNumber}/write-tests/**
    
    If an existing write-tests comment exists, it will be EDITED with the new attempt added.
    Otherwise, a new comment will be created.
    
    Format:
    ## 🧪 Test Writing for Issue #XXXXX
    <!-- WRITE-TESTS-COMMENT -->
    
    <details>
    <summary><b>Attempt 1: Test description ✅ Verified</b></summary>

    ... test details ...
    </details>
    
    <details>
    <summary><b>Attempt 2: Different approach ❌ Failed</b></summary>

    ... test details ...
    </details>

.PARAMETER IssueNumber
    The issue number to post comment on (required unless TestDir provided)

.PARAMETER AttemptNumber
    The attempt number (1, 2, 3, etc.) - auto-detected from TestDir if not specified

.PARAMETER TestDir
    Path to test attempt directory (e.g., CustomAgentLogsTmp/PRState/27246/write-tests/attempt-1)
    If provided, all parameters are auto-loaded from files in this directory

.PARAMETER TestDescription
    Brief description of what the test verifies (required unless loading from TestDir)

.PARAMETER HostAppFile
    Path to the HostApp test page file (required unless loading from TestDir)

.PARAMETER TestFile
    Path to the NUnit test file (required unless loading from TestDir)

.PARAMETER TestMethod
    Name of the test method (required unless loading from TestDir)

.PARAMETER Category
    UITestCategories category used (required unless loading from TestDir)

.PARAMETER VerificationStatus
    Status: "Verified" (tests fail without fix), "Failed" (tests don't catch bug), "Unverified" (not yet run) (required unless loading from TestDir)

.PARAMETER Platforms
    Platforms the test runs on (e.g., "All", "iOS, Android") (optional)

.PARAMETER Notes
    Additional notes about the test (optional)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    # Simplest: Just provide test directory (all info auto-loaded)
    ./post-write-tests-comment.ps1 -TestDir CustomAgentLogsTmp/PRState/27246/write-tests/attempt-1

.EXAMPLE
    # Or just provide issue number (auto-discovers latest attempt)
    ./post-write-tests-comment.ps1 -IssueNumber 27246

.EXAMPLE
    # Manual parameters (legacy)
    ./post-write-tests-comment.ps1 -IssueNumber 33331 -AttemptNumber 1 `
        -TestDescription "Verifies Picker.IsOpen property changes correctly" `
        -HostAppFile "src/Controls/tests/TestCases.HostApp/Issues/Issue33331.cs" `
        -TestFile "src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue33331.cs" `
        -TestMethod "PickerIsOpenPropertyChanges" `
        -Category "Picker" `
        -VerificationStatus "Verified"
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$IssueNumber,

    [Parameter(Mandatory=$false)]
    [int]$AttemptNumber,

    [Parameter(Mandatory=$false)]
    [string]$TestDir,

    [Parameter(Mandatory=$false)]
    [string]$TestDescription,

    [Parameter(Mandatory=$false)]
    [string]$HostAppFile,

    [Parameter(Mandatory=$false)]
    [string]$TestFile,

    [Parameter(Mandatory=$false)]
    [string]$TestMethod,

    [Parameter(Mandatory=$false)]
    [string]$Category,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Verified", "Failed", "Unverified", "")]
    [string]$VerificationStatus,

    [Parameter(Mandatory=$false)]
    [string]$Platforms = "All",

    [Parameter(Mandatory=$false)]
    [string]$Notes,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [string]$PreviewFile
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Write-Tests Comment (Post/Update)                        ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# ============================================================================
# AUTO-DISCOVERY FROM DIRECTORIES
# ============================================================================

# If TestDir provided, load everything from there
if (-not [string]::IsNullOrWhiteSpace($TestDir)) {
    if (-not (Test-Path $TestDir)) {
        throw "Test directory not found: $TestDir"
    }
    
    # Extract IssueNumber from path (e.g., CustomAgentLogsTmp/PRState/27246/write-tests/attempt-1)
    if ($TestDir -match '[/\\](\d+)[/\\]write-tests') {
        if ($IssueNumber -eq 0) {
            $IssueNumber = [int]$Matches[1]
            Write-Host "ℹ️  Auto-detected IssueNumber: $IssueNumber from path" -ForegroundColor Cyan
        }
    }
    
    # Extract AttemptNumber from path (e.g., attempt-1)
    if ($TestDir -match 'attempt-(\d+)$') {
        if ($AttemptNumber -eq 0) {
            $AttemptNumber = [int]$Matches[1]
            Write-Host "ℹ️  Auto-detected AttemptNumber: $AttemptNumber from path" -ForegroundColor Cyan
        }
    }
    
    # Load test description from description.md or description.txt
    if ([string]::IsNullOrWhiteSpace($TestDescription)) {
        $descFile = Join-Path $TestDir "description.md"
        if (-not (Test-Path $descFile)) {
            $descFile = Join-Path $TestDir "description.txt"
        }
        if (Test-Path $descFile) {
            $TestDescription = (Get-Content $descFile -Raw -Encoding UTF8).Trim()
            Write-Host "ℹ️  Loaded description from: $descFile" -ForegroundColor Cyan
        }
    }
    
    # Load test info from test-info.json or test-info.txt
    $infoFile = Join-Path $TestDir "test-info.json"
    if (Test-Path $infoFile) {
        $info = Get-Content $infoFile -Raw | ConvertFrom-Json
        if ([string]::IsNullOrWhiteSpace($HostAppFile) -and $info.HostAppFile) { $HostAppFile = $info.HostAppFile }
        if ([string]::IsNullOrWhiteSpace($TestFile) -and $info.TestFile) { $TestFile = $info.TestFile }
        if ([string]::IsNullOrWhiteSpace($TestMethod) -and $info.TestMethod) { $TestMethod = $info.TestMethod }
        if ([string]::IsNullOrWhiteSpace($Category) -and $info.Category) { $Category = $info.Category }
        Write-Host "ℹ️  Loaded test info from: $infoFile" -ForegroundColor Cyan
    }
    
    # Load verification status from result.txt
    if ([string]::IsNullOrWhiteSpace($VerificationStatus)) {
        $resultFile = Join-Path $TestDir "result.txt"
        if (Test-Path $resultFile) {
            $resultContent = (Get-Content $resultFile -Raw -Encoding UTF8).Trim().ToUpper()
            $VerificationStatus = switch -Regex ($resultContent) {
                'VERIFIED' { "Verified" }
                'PASS' { "Verified" }
                'FAILED' { "Failed" }
                'FAIL' { "Failed" }
                default { "Unverified" }
            }
            Write-Host "ℹ️  Loaded status: $VerificationStatus from result.txt" -ForegroundColor Cyan
        }
    }
    
    # Load notes from notes.md
    if ([string]::IsNullOrWhiteSpace($Notes)) {
        $notesFile = Join-Path $TestDir "notes.md"
        if (Test-Path $notesFile) {
            $Notes = Get-Content $notesFile -Raw -Encoding UTF8
            Write-Host "ℹ️  Loaded notes from: $notesFile" -ForegroundColor Cyan
        }
    }
}

# If IssueNumber provided but no TestDir, try to find all attempts
if ($IssueNumber -gt 0 -and [string]::IsNullOrWhiteSpace($TestDir) -and [string]::IsNullOrWhiteSpace($TestDescription)) {
    $testBase = "CustomAgentLogsTmp/PRState/$IssueNumber/write-tests"
    if (-not (Test-Path $testBase)) {
        $repoRoot = git rev-parse --show-toplevel 2>$null
        if ($repoRoot) {
            $testBase = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$IssueNumber/write-tests"
        }
    }
    
    if (Test-Path $testBase) {
        $attemptDirs = Get-ChildItem -Path $testBase -Directory | Where-Object { $_.Name -match '^attempt-\d+$' } | Sort-Object Name
        if ($attemptDirs.Count -gt 0) {
            Write-Host "ℹ️  Found $($attemptDirs.Count) attempt(s) in $testBase" -ForegroundColor Cyan
            Write-Host "    Use -TestDir to post a specific attempt, or posting latest..." -ForegroundColor Cyan
            
            # Post the latest attempt
            $latestAttempt = $attemptDirs | Sort-Object { [int]($_.Name -replace 'attempt-', '') } | Select-Object -Last 1
            $TestDir = $latestAttempt.FullName
            
            # Recurse with the discovered directory
            & $PSCommandPath -TestDir $TestDir -DryRun:$DryRun
            exit 0
        }
    }
}

# Validate required parameters
if ($IssueNumber -eq 0) {
    throw "IssueNumber is required. Provide via -IssueNumber or use -TestDir with path containing issue number"
}

if ($AttemptNumber -eq 0) {
    throw "AttemptNumber is required. Provide via -AttemptNumber or use -TestDir with path like attempt-N"
}

if ([string]::IsNullOrWhiteSpace($TestDescription)) {
    throw "TestDescription is required. Provide via -TestDescription or create description.md in TestDir"
}

if ([string]::IsNullOrWhiteSpace($HostAppFile)) {
    throw "HostAppFile is required. Provide via -HostAppFile or add to test-info.json in TestDir"
}

if ([string]::IsNullOrWhiteSpace($TestFile)) {
    throw "TestFile is required. Provide via -TestFile or add to test-info.json in TestDir"
}

if ([string]::IsNullOrWhiteSpace($TestMethod)) {
    throw "TestMethod is required. Provide via -TestMethod or add to test-info.json in TestDir"
}

if ([string]::IsNullOrWhiteSpace($Category)) {
    throw "Category is required. Provide via -Category or add to test-info.json in TestDir"
}

if ([string]::IsNullOrWhiteSpace($VerificationStatus)) {
    $VerificationStatus = "Unverified"
}

# Status emoji mapping
$statusEmoji = switch ($VerificationStatus) {
    "Verified" { "✅ Verified" }
    "Failed" { "❌ Failed" }
    "Unverified" { "⏳ Unverified" }
    default { $VerificationStatus }
}

# Build the new attempt section (collapsible)
$attemptSection = @"
<details>
<summary><b>Attempt $AttemptNumber`: $TestDescription $statusEmoji</b></summary>


### Test Details

| Property | Value |
|----------|-------|
| **Test Method** | ``$TestMethod`` |
| **Category** | ``UITestCategories.$Category`` |
| **Platforms** | $Platforms |
| **Status** | $statusEmoji |

### Files Created

<details>
<summary>📄 HostApp Test Page - Click to expand code</summary>

**File:** ``$HostAppFile``

``````csharp
$(if (Test-Path $HostAppFile) { Get-Content $HostAppFile -Raw } else { "File not found: $HostAppFile" })
``````

</details>

<details>
<summary>🧪 NUnit Test - Click to expand code</summary>

**File:** ``$TestFile``

``````csharp
$(if (Test-Path $TestFile) { Get-Content $TestFile -Raw } else { "File not found: $TestFile" })
``````

</details>

"@

if (-not [string]::IsNullOrWhiteSpace($Notes)) {
    $attemptSection += @"

### Notes

$Notes

"@
}

$attemptSection += @"
</details>
"@

# ============================================================================
# UNIFIED COMMENT HANDLING
# Uses single <!-- AI Summary --> comment with section markers
# ============================================================================

$MAIN_MARKER = "<!-- AI Summary -->"
$SECTION_START = "<!-- SECTION:WRITE-TESTS -->"
$SECTION_END = "<!-- /SECTION:WRITE-TESTS -->"

Write-Host "`nChecking for existing AI Summary comment on #$IssueNumber..." -ForegroundColor Yellow
$existingComment = $null
$existingBody = ""

try {
    $commentsJson = gh api "repos/dotnet/maui/issues/$IssueNumber/comments" 2>$null
    $comments = $commentsJson | ConvertFrom-Json
    
    foreach ($comment in $comments) {
        if ($comment.body -match [regex]::Escape($MAIN_MARKER)) {
            $existingComment = $comment
            $existingBody = $comment.body
            Write-Host "✓ Found existing AI Summary comment (ID: $($comment.id))" -ForegroundColor Green
            break
        }
    }
    
    if (-not $existingComment) {
        Write-Host "✓ No existing AI Summary comment found - will create new" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✓ No existing AI Summary comment found - will create new" -ForegroundColor Yellow
}

# Build the write-tests section content
$writeTestsHeader = "### 🧪 Test Writing`n`n"

# Extract existing write-tests section to preserve previous attempts
$existingWriteTestsContent = ""
$startPattern = [regex]::Escape($SECTION_START)
$endPattern = [regex]::Escape($SECTION_END)
if ($existingBody -match "(?s)$startPattern(.*?)$endPattern") {
    $existingWriteTestsContent = $Matches[1].Trim()
}

# Check if this attempt number already exists and replace it, or add new
$attemptPattern = "(?s)<details>\s*<summary><b>Attempt $AttemptNumber`:.*?</details>"
if ($existingWriteTestsContent -match $attemptPattern) {
    Write-Host "Replacing existing Attempt $AttemptNumber..." -ForegroundColor Yellow
    $tryFixContent = $existingWriteTestsContent -replace $attemptPattern, $attemptSection
} elseif (-not [string]::IsNullOrWhiteSpace($existingWriteTestsContent)) {
    Write-Host "Adding new Attempt $AttemptNumber..." -ForegroundColor Yellow
    # Remove header if present to avoid duplication
    $existingWriteTestsContent = $existingWriteTestsContent -replace "^### 🧪 Test Writing\s*`n*", ""
    $writeTestsContent = $writeTestsHeader + $existingWriteTestsContent.TrimEnd() + "`n`n" + $attemptSection
} else {
    Write-Host "Creating first attempt..." -ForegroundColor Yellow
    $writeTestsContent = $writeTestsHeader + $attemptSection
}

# Build the section with markers
$writeTestsSection = @"
$SECTION_START
$writeTestsContent
$SECTION_END
"@

if ($existingComment) {
    # Update existing comment - replace or add write-tests section
    if ($existingBody -match "(?s)$startPattern.*?$endPattern") {
        # Replace existing write-tests section
        $commentBody = $existingBody -replace "(?s)$startPattern.*?$endPattern", $writeTestsSection
    } else {
        # Add write-tests section before footer
        $footerPattern = "(?s)(---\s*\n+<sub>.*?</sub>\s*)$"
        if ($existingBody -match $footerPattern) {
            $commentBody = $existingBody -replace $footerPattern, "`n$writeTestsSection`n`n`$1"
        } else {
            $commentBody = $existingBody.TrimEnd() + "`n`n$writeTestsSection"
        }
    }
} else {
    # Create new unified comment
    $commentBody = @"
$MAIN_MARKER

## 🤖 AI Summary

$writeTestsSection
"@
}

if ($DryRun) {
    # File-based DryRun: mirrors GitHub comment behavior using a local file
    if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
        $PreviewFile = "CustomAgentLogsTmp/PRState/$IssueNumber/ai-summary-comment-preview.md"
    }
    
    # Ensure directory exists
    $previewDir = Split-Path $PreviewFile -Parent
    if (-not (Test-Path $previewDir)) {
        New-Item -ItemType Directory -Path $previewDir -Force | Out-Null
    }
    
    # Read existing preview file
    $existingPreview = ""
    if (Test-Path $PreviewFile) {
        $existingPreview = Get-Content $PreviewFile -Raw -Encoding UTF8
        Write-Host "ℹ️  Updating existing preview file: $PreviewFile" -ForegroundColor Cyan
    } else {
        Write-Host "ℹ️  Creating new preview file: $PreviewFile" -ForegroundColor Cyan
    }
    
    # Update or insert the WRITE-TESTS section
    $sectionMarker = "<!-- SECTION:WRITE-TESTS -->"
    $sectionEndMarker = "<!-- /SECTION:WRITE-TESTS -->"
    $wrappedSection = "$sectionMarker`n$writeTestsSection`n$sectionEndMarker"
    
    if ($existingPreview -match [regex]::Escape($sectionMarker)) {
        # Replace existing WRITE-TESTS section
        $pattern = [regex]::Escape($sectionMarker) + "[\s\S]*?" + [regex]::Escape($sectionEndMarker)
        $finalComment = $existingPreview -replace $pattern, $wrappedSection
    } elseif (-not [string]::IsNullOrWhiteSpace($existingPreview)) {
        # Append WRITE-TESTS section to existing content
        $finalComment = $existingPreview.TrimEnd() + "`n`n" + $wrappedSection
    } else {
        # New file - use full comment body with section markers
        $finalComment = $commentBody.Replace($writeTestsSection, $wrappedSection)
    }
    
    # Write to preview file
    Set-Content -Path $PreviewFile -Value "$($finalComment.TrimEnd())`n" -Encoding UTF8 -NoNewline
    
    Write-Host "`n=== COMMENT PREVIEW ===" -ForegroundColor Yellow
    Write-Host $finalComment
    Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
    Write-Host "`n✅ Preview saved to: $PreviewFile" -ForegroundColor Green
    Write-Host "   Run 'open $PreviewFile' to view in editor" -ForegroundColor Gray
    exit 0
}

# Write to temp file to avoid shell escaping issues
$tempFile = [System.IO.Path]::GetTempFileName()
@{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

if ($existingComment) {
    Write-Host "Updating comment ID $($existingComment.id)..." -ForegroundColor Yellow
    $result = gh api --method PATCH "repos/dotnet/maui/issues/comments/$($existingComment.id)" --input $tempFile --jq '.html_url'
    Write-Host "✅ Comment updated: $result" -ForegroundColor Green
} else {
    Write-Host "Posting new comment to issue #$IssueNumber..." -ForegroundColor Yellow
    $result = gh api --method POST "repos/dotnet/maui/issues/$IssueNumber/comments" --input $tempFile --jq '.html_url'
    Write-Host "✅ Comment posted: $result" -ForegroundColor Green
}

Remove-Item $tempFile
