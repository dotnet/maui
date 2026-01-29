#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a try-fix attempts comment on a GitHub Issue or Pull Request.

.DESCRIPTION
    Creates ONE comment for all try-fix attempts with each attempt in a collapsible section.
    Uses HTML marker <!-- TRY-FIX-COMMENT --> for identification.
    
    If an existing try-fix comment exists, it will be EDITED with the new attempt added.
    Otherwise, a new comment will be created.
    
    **NEW: Auto-loads from CustomAgentLogsTmp/PRState/{PRNumber}/try-fix/**
    
    Format:
    ## 🔧 Try-Fix Analysis for Issue #XXXXX
    <!-- TRY-FIX-COMMENT -->
    
    <details>
    <summary><b>Attempt 1: Approach Name ✅ PASS</b></summary>

    ... attempt details ...
    </details>
    
    <details>
    <summary><b>Attempt 2: Different Approach ❌ FAIL</b></summary>

    ... attempt details ...
    </details>

.PARAMETER IssueNumber
    The issue number to post comment on (required unless -TryFixDir provided)

.PARAMETER AttemptNumber
    The attempt number (1, 2, 3, etc.) - auto-detected from TryFixDir if not specified

.PARAMETER TryFixDir
    Path to try-fix attempt directory (e.g., CustomAgentLogsTmp/PRState/27246/try-fix/attempt-1)
    If provided, all parameters are auto-loaded from files in this directory

.PARAMETER Approach
    Brief description of the fix approach (required unless loading from TryFixDir)

.PARAMETER RootCause
    Description of the root cause identified (optional for failed attempts)

.PARAMETER FilesChanged
    Markdown table or list of files changed (required unless loading from TryFixDir)

.PARAMETER Status
    Status of the attempt: "Compiles", "Pass", "Fail" (required unless loading from TryFixDir)

.PARAMETER Analysis
    Analysis of why it worked or failed (optional)

.PARAMETER CodeSnippet
    Code snippet showing the fix (optional)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    # Simplest: Just provide attempt directory (all info auto-loaded)
    ./post-try-fix-comment.ps1 -TryFixDir CustomAgentLogsTmp/PRState/27246/try-fix/attempt-1

.EXAMPLE
    # Post all attempts for an issue
    ./post-try-fix-comment.ps1 -IssueNumber 27246

.EXAMPLE
    # Manual parameters (legacy)
    ./post-try-fix-comment.ps1 -IssueNumber 19560 -AttemptNumber 1 `
        -Approach "Change Shadow base class to StyleableElement" `
        -RootCause "Shadow inherits from Element which lacks styling support" `
        -FilesChanged "| File | Changes |`n|------|---------|`n| Shadow.cs | +1/-1 |" `
        -Status "Pass"
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$IssueNumber,

    [Parameter(Mandatory=$false)]
    [int]$AttemptNumber,

    [Parameter(Mandatory=$false)]
    [string]$TryFixDir,

    [Parameter(Mandatory=$false)]
    [string]$Approach,

    [Parameter(Mandatory=$false)]
    [string]$RootCause,

    [Parameter(Mandatory=$false)]
    [string]$FilesChanged,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Compiles", "Pass", "Fail", "")]
    [string]$Status,

    [Parameter(Mandatory=$false)]
    [string]$Analysis,

    [Parameter(Mandatory=$false)]
    [string]$CodeSnippet,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [string]$PreviewFile
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  Try-Fix Comment (Post/Update)                            ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# ============================================================================
# AUTO-DISCOVERY FROM DIRECTORIES
# ============================================================================

# If TryFixDir provided, load everything from there
if (-not [string]::IsNullOrWhiteSpace($TryFixDir)) {
    if (-not (Test-Path $TryFixDir)) {
        throw "Try-fix directory not found: $TryFixDir"
    }
    
    # Extract IssueNumber from path (e.g., CustomAgentLogsTmp/PRState/27246/try-fix/attempt-1)
    if ($TryFixDir -match '[/\\](\d+)[/\\]try-fix') {
        if ($IssueNumber -eq 0) {
            $IssueNumber = [int]$Matches[1]
            Write-Host "ℹ️  Auto-detected IssueNumber: $IssueNumber from path" -ForegroundColor Cyan
        }
    }
    
    # Extract AttemptNumber from path (e.g., attempt-1)
    if ($TryFixDir -match 'attempt-(\d+)$') {
        if ($AttemptNumber -eq 0) {
            $AttemptNumber = [int]$Matches[1]
            Write-Host "ℹ️  Auto-detected AttemptNumber: $AttemptNumber from path" -ForegroundColor Cyan
        }
    }
    
    # Load approach from approach.md or approach.txt
    if ([string]::IsNullOrWhiteSpace($Approach)) {
        $approachFile = Join-Path $TryFixDir "approach.md"
        if (-not (Test-Path $approachFile)) {
            $approachFile = Join-Path $TryFixDir "approach.txt"
        }
        if (Test-Path $approachFile) {
            $Approach = Get-Content $approachFile -Raw -Encoding UTF8
            Write-Host "ℹ️  Loaded approach from: $approachFile" -ForegroundColor Cyan
        }
    }
    
    # Load result from result.txt
    if ([string]::IsNullOrWhiteSpace($Status)) {
        $resultFile = Join-Path $TryFixDir "result.txt"
        if (Test-Path $resultFile) {
            $resultContent = (Get-Content $resultFile -Raw -Encoding UTF8).Trim().ToUpper()
            $Status = switch -Regex ($resultContent) {
                'PASS' { "Pass" }
                'FAIL' { "Fail" }
                'COMPILES' { "Compiles" }
                default { "Fail" }
            }
            Write-Host "ℹ️  Loaded status: $Status from result.txt" -ForegroundColor Cyan
        }
    }
    
    # Load analysis from analysis.md
    if ([string]::IsNullOrWhiteSpace($Analysis)) {
        $analysisFile = Join-Path $TryFixDir "analysis.md"
        if (Test-Path $analysisFile) {
            $Analysis = Get-Content $analysisFile -Raw -Encoding UTF8
            Write-Host "ℹ️  Loaded analysis from: $analysisFile" -ForegroundColor Cyan
        }
    }
    
    # Load diff from fix.diff
    if ([string]::IsNullOrWhiteSpace($CodeSnippet)) {
        $diffFile = Join-Path $TryFixDir "fix.diff"
        if (Test-Path $diffFile) {
            $CodeSnippet = Get-Content $diffFile -Raw -Encoding UTF8
            Write-Host "ℹ️  Loaded code diff from: $diffFile" -ForegroundColor Cyan
        }
    }
    
    # Generate FilesChanged from diff if not provided
    if ([string]::IsNullOrWhiteSpace($FilesChanged) -and -not [string]::IsNullOrWhiteSpace($CodeSnippet)) {
        $files = $CodeSnippet | Select-String -Pattern "^\+\+\+ b/(.+)$" -AllMatches | 
                 ForEach-Object { $_.Matches.Groups[1].Value } | 
                 Sort-Object -Unique
        if ($files) {
            $FilesChanged = "| File | Type |`n|------|------|`n"
            foreach ($file in $files) {
                $FilesChanged += "| ``$file`` | Modified |`n"
            }
        } else {
            $FilesChanged = "_See diff above_"
        }
    }
}

# If IssueNumber provided but no TryFixDir, try to find all attempts
if ($IssueNumber -gt 0 -and [string]::IsNullOrWhiteSpace($TryFixDir) -and [string]::IsNullOrWhiteSpace($Approach)) {
    $tryFixBase = "CustomAgentLogsTmp/PRState/$IssueNumber/try-fix"
    if (-not (Test-Path $tryFixBase)) {
        $repoRoot = git rev-parse --show-toplevel 2>$null
        if ($repoRoot) {
            $tryFixBase = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$IssueNumber/try-fix"
        }
    }
    
    if (Test-Path $tryFixBase) {
        $attemptDirs = Get-ChildItem -Path $tryFixBase -Directory | Where-Object { $_.Name -match '^attempt-\d+$' } | Sort-Object { [int]($_.Name -replace 'attempt-', '') }
        if ($attemptDirs.Count -gt 0) {
            Write-Host "ℹ️  Found $($attemptDirs.Count) attempt(s) in $tryFixBase" -ForegroundColor Cyan
            Write-Host "    Posting ALL attempts..." -ForegroundColor Cyan
            
            # Loop through ALL attempts and recurse for each
            foreach ($attemptDir in $attemptDirs) {
                Write-Host "    Processing: $($attemptDir.Name)" -ForegroundColor Gray
                & $PSCommandPath -TryFixDir $attemptDir.FullName -DryRun:$DryRun -PreviewFile:$PreviewFile
            }
            exit 0
        }
    }
}

# Validate required parameters
if ($IssueNumber -eq 0) {
    throw "IssueNumber is required. Provide via -IssueNumber or use -TryFixDir with path containing issue number"
}

if ($AttemptNumber -eq 0) {
    throw "AttemptNumber is required. Provide via -AttemptNumber or use -TryFixDir with path like attempt-N"
}

if ([string]::IsNullOrWhiteSpace($Approach)) {
    throw "Approach is required. Provide via -Approach or create approach.md in TryFixDir"
}

if ([string]::IsNullOrWhiteSpace($Status)) {
    throw "Status is required. Provide via -Status or create result.txt in TryFixDir"
}

if ([string]::IsNullOrWhiteSpace($FilesChanged)) {
    $FilesChanged = "_No files changed information available_"
}

# Status emoji mapping
$statusEmoji = switch ($Status) {
    "Pass" { "✅" }
    "Fail" { "❌" }
    "Compiles" { "🔨" }
    default { "⚪" }
}

# Build the new attempt section - compact format
# Note: blank line after </summary> is required for proper markdown rendering
$attemptSection = @"
<details>
<summary>$statusEmoji Fix $AttemptNumber</summary>

"@

# Show brief approach description
if (-not [string]::IsNullOrWhiteSpace($Approach)) {
    $attemptSection += "$Approach`n`n"
}

# Only show diff if available
if (-not [string]::IsNullOrWhiteSpace($CodeSnippet)) {
    $attemptSection += @"
``````diff
$CodeSnippet
``````

"@
}

# Show analysis if available (explains why it passed/failed)
if (-not [string]::IsNullOrWhiteSpace($Analysis)) {
    $attemptSection += "$Analysis`n"
}

$attemptSection += @"
</details>
"@

# ============================================================================
# UNIFIED COMMENT HANDLING
# Uses single <!-- AI Summary --> comment with section markers
# ============================================================================

$MAIN_MARKER = "<!-- AI Summary -->"
$SECTION_START = "<!-- SECTION:TRY-FIX -->"
$SECTION_END = "<!-- /SECTION:TRY-FIX -->"

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

# Build the try-fix section content
$tryFixHeader = "### Try-Fix Analysis`n`n"

# Extract existing try-fix section to preserve previous attempts
$existingTryFixContent = ""
$startPattern = [regex]::Escape($SECTION_START)
$endPattern = [regex]::Escape($SECTION_END)
if ($existingBody -match "(?s)$startPattern(.*?)$endPattern") {
    $existingTryFixContent = $Matches[1].Trim()
}

# Check if this attempt number already exists and replace it, or add new
# Match both old format (Attempt N:) and new format (Fix N)
$attemptPattern = "(?s)<details>\s*<summary>.*?(Attempt $AttemptNumber`:|Fix $AttemptNumber).*?</details>"
if ($existingTryFixContent -match $attemptPattern) {
    Write-Host "Replacing existing Fix $AttemptNumber..." -ForegroundColor Yellow
    $tryFixContent = $existingTryFixContent -replace $attemptPattern, $attemptSection
} elseif (-not [string]::IsNullOrWhiteSpace($existingTryFixContent)) {
    Write-Host "Adding new Fix $AttemptNumber..." -ForegroundColor Yellow
    # Remove header if present to avoid duplication (match any emoji or no emoji)
    $existingTryFixContent = $existingTryFixContent -replace "^###\s*[^\n]*Try-Fix Analysis[^\n]*`n*", ""
    $tryFixContent = $tryFixHeader + $existingTryFixContent.TrimEnd() + "`n`n" + $attemptSection
} else {
    Write-Host "Creating first fix..." -ForegroundColor Yellow
    $tryFixContent = $tryFixHeader + $attemptSection
}

# Build the section with markers
$tryFixSection = @"
$SECTION_START
$tryFixContent
$SECTION_END
"@

if ($existingComment) {
    # Update existing comment - replace or add try-fix section
    if ($existingBody -match "(?s)$startPattern.*?$endPattern") {
        # Replace existing try-fix section
        $commentBody = $existingBody -replace "(?s)$startPattern.*?$endPattern", $tryFixSection
    } else {
        # Add try-fix section before footer
        $footerPattern = "(?s)(---\s*\n+<sub>.*?</sub>\s*)$"
        if ($existingBody -match $footerPattern) {
            $commentBody = $existingBody -replace $footerPattern, "`n$tryFixSection`n`n`$1"
        } else {
            $commentBody = $existingBody.TrimEnd() + "`n`n$tryFixSection"
        }
    }
} else {
    # Create new unified comment
    $commentBody = @"
$MAIN_MARKER

## AI Summary

$tryFixSection
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
    
    # Read existing preview file (mimics reading existing GitHub comment)
    $existingPreview = ""
    if (Test-Path $PreviewFile) {
        $existingPreview = Get-Content $PreviewFile -Raw -Encoding UTF8
        Write-Host "ℹ️  Updating existing preview file: $PreviewFile" -ForegroundColor Cyan
    } else {
        Write-Host "ℹ️  Creating new preview file: $PreviewFile" -ForegroundColor Cyan
    }
    
    # Update or insert the TRY-FIX section
    $TRY_FIX_MARKER = "<!-- SECTION:TRY-FIX -->"
    $TRY_FIX_END_MARKER = "<!-- /SECTION:TRY-FIX -->"
    
    if ($existingPreview -match [regex]::Escape($TRY_FIX_MARKER)) {
        # Extract existing TRY-FIX content to preserve previous attempts (same logic as GitHub comment path)
        $startPattern = [regex]::Escape($TRY_FIX_MARKER)
        $endPattern = [regex]::Escape($TRY_FIX_END_MARKER)
        $existingTryFixPreview = ""
        if ($existingPreview -match "(?s)$startPattern(.*?)$endPattern") {
            $existingTryFixPreview = $Matches[1].Trim()
        }
        
        # Check if this attempt already exists - replace it, otherwise append
        $attemptPatternPreview = "(?s)<details>\s*<summary>.*?(Attempt $AttemptNumber`:|Fix $AttemptNumber).*?</details>"
        if ($existingTryFixPreview -match $attemptPatternPreview) {
            Write-Host "Replacing existing Fix $AttemptNumber in preview..." -ForegroundColor Yellow
            $updatedTryFixContent = $existingTryFixPreview -replace $attemptPatternPreview, $attemptSection
            $tryFixSectionUpdated = "$SECTION_START`n$tryFixHeader$updatedTryFixContent`n$SECTION_END"
        } else {
            Write-Host "Adding Fix $AttemptNumber to preview..." -ForegroundColor Yellow
            # Remove header if present to avoid duplication (match any emoji or no emoji)
            $existingTryFixPreview = $existingTryFixPreview -replace "^###\s*[^\n]*Try-Fix Analysis[^\n]*`n*", ""
            $updatedTryFixContent = $tryFixHeader + $existingTryFixPreview.TrimEnd() + "`n`n" + $attemptSection
            $tryFixSectionUpdated = "$SECTION_START`n$updatedTryFixContent`n$SECTION_END"
        }
        
        # Replace the section in the preview
        $pattern = [regex]::Escape($TRY_FIX_MARKER) + "[\s\S]*?" + [regex]::Escape($TRY_FIX_END_MARKER)
        $finalComment = $existingPreview -replace $pattern, $tryFixSectionUpdated
    } elseif (-not [string]::IsNullOrWhiteSpace($existingPreview)) {
        # Append TRY-FIX section to existing content
        $finalComment = $existingPreview.TrimEnd() + "`n`n" + $tryFixSection
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
    Write-Host "Posting new comment to issue #$IssueNumber..." -ForegroundColor Yellow
    $result = gh api --method POST "repos/dotnet/maui/issues/$IssueNumber/comments" --input $tempFile --jq '.html_url'
    Write-Host "✅ Comment posted: $result" -ForegroundColor Green
}

Remove-Item $tempFile
