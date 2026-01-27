#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates the verification test results in the unified AI Summary comment.

.DESCRIPTION
    Reads verification report from CustomAgentLogsTmp and posts/updates
    the VERIFY-TESTS section in the unified AI Summary comment.
    
    Uses the same <!-- AI Summary --> marker and <!-- SECTION:VERIFY-TESTS --> 
    section markers as other comment scripts.

.PARAMETER PRNumber
    The PR number to post comment on (required unless ReportFile provided)

.PARAMETER ReportFile
    Path to verification-report.md file. If not provided, auto-discovers from
    CustomAgentLogsTmp/PRState/{PRNumber}/verify-tests-fail/verification-report.md

.PARAMETER Status
    Overall verification status: "Passed", "Failed" (auto-detected from report if not provided)

.PARAMETER Platform
    Platform tested (auto-detected from report if not provided)

.PARAMETER Mode
    Verification mode: "FailureOnly", "FullVerification" (auto-detected from report if not provided)

.PARAMETER Summary
    Brief summary of results (auto-generated if not provided)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    # Simplest: Just provide PR number (auto-loads from CustomAgentLogsTmp)
    ./post-verify-tests-comment.ps1 -PRNumber 27246

.EXAMPLE
    # Or provide report file path
    ./post-verify-tests-comment.ps1 -ReportFile CustomAgentLogsTmp/PRState/27246/verify-tests-fail/verification-report.md

.EXAMPLE
    # Manual parameters
    ./post-verify-tests-comment.ps1 -PRNumber 27246 -Status "Passed" -Platform "android" -Mode "FullVerification"
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$PRNumber,

    [Parameter(Mandatory=$false)]
    [string]$ReportFile,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Passed", "Failed", "")]
    [string]$Status,

    [Parameter(Mandatory=$false)]
    [string]$Platform,

    [Parameter(Mandatory=$false)]
    [ValidateSet("FailureOnly", "FullVerification", "")]
    [string]$Mode,

    [Parameter(Mandatory=$false)]
    [string]$Summary,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [string]$PreviewFile
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Verify-Tests Comment (Post/Update)                       ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# ============================================================================
# AUTO-DISCOVERY FROM REPORT FILE
# ============================================================================

# If PRNumber provided but no ReportFile, try to find it
if ($PRNumber -gt 0 -and [string]::IsNullOrWhiteSpace($ReportFile)) {
    $reportPath = "CustomAgentLogsTmp/PRState/$PRNumber/verify-tests-fail/verification-report.md"
    if (-not (Test-Path $reportPath)) {
        $repoRoot = git rev-parse --show-toplevel 2>$null
        if ($repoRoot) {
            $reportPath = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/verify-tests-fail/verification-report.md"
        }
    }
    
    if (Test-Path $reportPath) {
        $ReportFile = $reportPath
        Write-Host "ℹ️  Auto-discovered report file: $ReportFile" -ForegroundColor Cyan
    }
}

# If ReportFile provided, parse it
if (-not [string]::IsNullOrWhiteSpace($ReportFile)) {
    if (-not (Test-Path $ReportFile)) {
        throw "Report file not found: $ReportFile"
    }
    
    $reportContent = Get-Content $ReportFile -Raw -Encoding UTF8
    Write-Host "ℹ️  Loading from report file: $ReportFile" -ForegroundColor Cyan
    
    # Extract PRNumber from path if not provided
    if ($PRNumber -eq 0 -and $ReportFile -match '[/\\](\d+)[/\\]verify-tests-fail') {
        $PRNumber = [int]$Matches[1]
        Write-Host "ℹ️  Auto-detected PRNumber: $PRNumber from path" -ForegroundColor Cyan
    }
    
    # Extract Status from report
    if ([string]::IsNullOrWhiteSpace($Status)) {
        if ($reportContent -match 'VERIFICATION PASSED|✅\s*PASSED') {
            $Status = "Passed"
        } elseif ($reportContent -match 'VERIFICATION FAILED|❌\s*FAILED') {
            $Status = "Failed"
        } else {
            $Status = "Unknown"
        }
        Write-Host "ℹ️  Detected Status: $Status" -ForegroundColor Cyan
    }
    
    # Extract Platform from report
    if ([string]::IsNullOrWhiteSpace($Platform)) {
        if ($reportContent -match 'Platform[:\s]+(\w+)') {
            $Platform = $Matches[1]
        } elseif ($reportContent -match '(android|ios|catalyst|windows)' ) {
            $Platform = $Matches[1]
        }
        if ($Platform) {
            Write-Host "ℹ️  Detected Platform: $Platform" -ForegroundColor Cyan
        }
    }
    
    # Extract Mode from report
    if ([string]::IsNullOrWhiteSpace($Mode)) {
        if ($reportContent -match 'Full Verification|FAIL without fix.*PASS with fix') {
            $Mode = "FullVerification"
        } elseif ($reportContent -match 'Verify Failure Only|tests.*FAILED as expected') {
            $Mode = "FailureOnly"
        }
        if ($Mode) {
            Write-Host "ℹ️  Detected Mode: $Mode" -ForegroundColor Cyan
        }
    }
    
    # Use report content as summary if not provided
    if ([string]::IsNullOrWhiteSpace($Summary)) {
        # Extract key results from report
        $Summary = $reportContent
    }
}

# Validate required parameters
if ($PRNumber -eq 0) {
    throw "PRNumber is required. Provide via -PRNumber or use -ReportFile with path containing PR number"
}

if ([string]::IsNullOrWhiteSpace($Status)) {
    throw "Status is required. Provide via -Status or use -ReportFile with verification results"
}

# Generate summary if not provided
if ([string]::IsNullOrWhiteSpace($Summary)) {
    $statusEmoji = if ($Status -eq "Passed") { "✅" } else { "❌" }
    $modeDesc = if ($Mode -eq "FullVerification") { "Full verification (FAIL without fix, PASS with fix)" } else { "Failure only (tests FAIL as expected)" }
    $Summary = @"
**Status**: $statusEmoji $Status

**Mode**: $modeDesc
**Platform**: $Platform

_Run `verify-tests-fail.ps1` for full details._
"@
}

# Status emoji
$statusEmoji = if ($Status -eq "Passed") { "✅ PASSED" } else { "❌ FAILED" }
$modeDesc = if ($Mode -eq "FullVerification") { "Full Verification" } else { "Failure Only" }

# Build verification section content
$verifyContent = @"
### 🚦 Test Verification

**Result**: $statusEmoji
**Mode**: $modeDesc
**Platform**: $Platform

<details>
<summary>Expand Details</summary>

$Summary

</details>
"@

# ============================================================================
# UNIFIED COMMENT HANDLING
# Uses single <!-- AI Summary --> comment with section markers
# ============================================================================

$MAIN_MARKER = "<!-- AI Summary -->"
$SECTION_START = "<!-- SECTION:VERIFY-TESTS -->"
$SECTION_END = "<!-- /SECTION:VERIFY-TESTS -->"

Write-Host "`nChecking for existing AI Summary comment on #$PRNumber..." -ForegroundColor Yellow
$existingComment = $null
$existingBody = ""

try {
    $commentsJson = gh api "repos/dotnet/maui/issues/$PRNumber/comments" 2>$null
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

# Build the section with markers
$verifySection = @"
$SECTION_START
$verifyContent
$SECTION_END
"@

$startPattern = [regex]::Escape($SECTION_START)
$endPattern = [regex]::Escape($SECTION_END)

if ($existingComment) {
    # Update existing comment - replace or add verify-tests section
    if ($existingBody -match "(?s)$startPattern.*?$endPattern") {
        # Replace existing verify-tests section
        $commentBody = $existingBody -replace "(?s)$startPattern.*?$endPattern", $verifySection
    } else {
        # Add verify-tests section before footer
        $footerPattern = "(?s)(---\s*\n+<sub>.*?</sub>\s*)$"
        if ($existingBody -match $footerPattern) {
            $commentBody = $existingBody -replace $footerPattern, "`n$verifySection`n`n`$1"
        } else {
            $commentBody = $existingBody.TrimEnd() + "`n`n$verifySection"
        }
    }
} else {
    # Create new unified comment
    $commentBody = @"
$MAIN_MARKER

## 🤖 AI Summary

$verifySection
"@
}

if ($DryRun) {
    # File-based DryRun: mirrors GitHub comment behavior using a local file
    if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
        $PreviewFile = "CustomAgentLogsTmp/PRState/$PRNumber/pr-comment-preview.md"
    }
    
    # Ensure directory exists
    $previewDir = Split-Path $PreviewFile -Parent
    if (-not (Test-Path $previewDir)) {
        New-Item -ItemType Directory -Path $previewDir -Force | Out-Null
    }
    
    # Read existing preview file (mimics reading existing GitHub comment)
    $existingPreview = ""
    if (Test-Path $PreviewFile) {
        $existingPreview = Get-Content $PreviewFile -Raw -Encoding UTF8
        Write-Host "ℹ️  Updating existing preview file: $PreviewFile" -ForegroundColor Cyan
    } else {
        Write-Host "ℹ️  Creating new preview file: $PreviewFile" -ForegroundColor Cyan
    }
    
    # Update or insert the VERIFY-TESTS section
    $VERIFY_MARKER = "<!-- SECTION:VERIFY-TESTS -->"
    $VERIFY_END_MARKER = "<!-- /SECTION:VERIFY-TESTS -->"
    
    if ($existingPreview -match [regex]::Escape($VERIFY_MARKER)) {
        # Replace existing VERIFY-TESTS section
        $pattern = [regex]::Escape($VERIFY_MARKER) + "[\s\S]*?" + [regex]::Escape($VERIFY_END_MARKER)
        $finalComment = $existingPreview -replace $pattern, $verifySection
    } elseif (-not [string]::IsNullOrWhiteSpace($existingPreview)) {
        # Append VERIFY-TESTS section to existing content
        $finalComment = $existingPreview.TrimEnd() + "`n`n" + $verifySection
    } else {
        # New file - use full comment body
        $finalComment = $commentBody
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
    Write-Host "Posting new comment to PR #$PRNumber..." -ForegroundColor Yellow
    $result = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile --jq '.html_url'
    Write-Host "✅ Comment posted: $result" -ForegroundColor Green
}

Remove-Item $tempFile
