#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a PR finalize comment on a GitHub Pull Request.

.DESCRIPTION
    By default, injects a PR Finalization section into the unified AI Summary comment
    (<!-- AI Summary -->). Use -Standalone to post as a separate comment instead.
    
    **Auto-loads from CustomAgentLogsTmp/PRState/{PRNumber}/pr-finalize/**
    
    Provides three collapsible sections: Title, Description, and Code Review.
    
    Format:
    ## 📋 PR Finalization Review
    <!-- PR-FINALIZE-COMMENT -->
    
    <details>
    <summary><b>Title: ✅ Good</b></summary>
    ... title details ...
    </details>
    
    <details>
    <summary><b>Description: ⚠️ Needs Update</b></summary>
    ... description details ...
    </details>
    
    <details>
    <summary><b>Code Review: ✅ Passed</b></summary>
    ... code review findings ...
    </details>

.PARAMETER PRNumber
    The PR number to post comment on (required unless SummaryFile provided)

.PARAMETER SummaryFile
    Path to pr-finalize-summary.md file. If provided, auto-loads review data from this file.

.PARAMETER TitleStatus
    Title assessment: "Good", "NeedsUpdate" (required unless loading from SummaryFile)

.PARAMETER CurrentTitle
    Current PR title (required unless loading from SummaryFile)

.PARAMETER RecommendedTitle
    Recommended PR title (optional - only if TitleStatus is NeedsUpdate)

.PARAMETER TitleIssues
    List of issues with the current title (optional - only if TitleStatus is NeedsUpdate)

.PARAMETER DescriptionStatus
    Description assessment: "Excellent", "Good", "NeedsUpdate", "NeedsRewrite" (required unless loading from SummaryFile)

.PARAMETER DescriptionAssessment
    Quality assessment of the description (required unless loading from SummaryFile)

.PARAMETER MissingElements
    List of missing elements (optional)

.PARAMETER RecommendedDescription
    Full recommended description (optional - only if needs rewrite)

.PARAMETER CodeReviewStatus
    Code review assessment: "Passed", "IssuesFound", "Skipped" (optional - defaults to "Skipped" if not provided)

.PARAMETER CodeReviewFindings
    Code review findings content (optional - markdown with critical issues, suggestions, and positive observations)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    # Simplest: Just provide PR number (auto-loads from CustomAgentLogsTmp)
    ./post-pr-finalize-comment.ps1 -PRNumber 27246

.EXAMPLE
    # Or provide summary file path
    ./post-pr-finalize-comment.ps1 -SummaryFile CustomAgentLogsTmp/PRState/27246/pr-finalize/pr-finalize-summary.md

.EXAMPLE
    # Manual parameters
    ./post-pr-finalize-comment.ps1 -PRNumber 25748 `
        -TitleStatus "Good" `
        -CurrentTitle "[iOS] Fix SafeArea padding calculation" `
        -DescriptionStatus "Good" `
        -DescriptionAssessment "Clear structure, accurate technical details, matches implementation" `
        -CodeReviewStatus "Passed" `
        -CodeReviewFindings "No critical issues found. Code follows best practices."
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$PRNumber,

    [Parameter(Mandatory=$false)]
    [string]$SummaryFile,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Good", "NeedsUpdate", "")]
    [string]$TitleStatus,

    [Parameter(Mandatory=$false)]
    [string]$CurrentTitle,

    [Parameter(Mandatory=$false)]
    [string]$RecommendedTitle,

    [Parameter(Mandatory=$false)]
    [string]$TitleIssues,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Good", "NeedsUpdate", "")]
    [string]$DescriptionStatus,

    [Parameter(Mandatory=$false)]
    [string]$DescriptionAssessment,

    [Parameter(Mandatory=$false)]
    [string]$MissingElements,

    [Parameter(Mandatory=$false)]
    [string]$RecommendedDescription,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Passed", "IssuesFound", "Skipped", "")]
    [string]$CodeReviewStatus,

    [Parameter(Mandatory=$false)]
    [string]$CodeReviewFindings,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [switch]$Standalone,

    [Parameter(Mandatory=$false)]
    [string]$PreviewFile
)

$ErrorActionPreference = "Stop"

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  PR Finalize Comment (Post/Update)                        ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# ============================================================================
# AUTO-DISCOVERY FROM SUMMARY FILE
# ============================================================================

# If PRNumber provided but no SummaryFile, try to find it
if ($PRNumber -gt 0 -and [string]::IsNullOrWhiteSpace($SummaryFile)) {
    $summaryPath = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize/pr-finalize-summary.md"
    if (-not (Test-Path $summaryPath)) {
        $repoRoot = git rev-parse --show-toplevel 2>$null
        if ($repoRoot) {
            $summaryPath = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize/pr-finalize-summary.md"
        }
    }
    
    if (Test-Path $summaryPath) {
        $SummaryFile = $summaryPath
        Write-Host "ℹ️  Auto-discovered summary file: $SummaryFile" -ForegroundColor Cyan
    }
}

# If SummaryFile provided, parse it
if (-not [string]::IsNullOrWhiteSpace($SummaryFile)) {
    if (-not (Test-Path $SummaryFile)) {
        throw "Summary file not found: $SummaryFile"
    }
    
    $content = Get-Content $SummaryFile -Raw -Encoding UTF8
    Write-Host "ℹ️  Loading from summary file: $SummaryFile" -ForegroundColor Cyan
    
    # Extract PRNumber from path if not provided
    if ($PRNumber -eq 0 -and $SummaryFile -match '[/\\](\d+)[/\\]pr-finalize') {
        $PRNumber = [int]$Matches[1]
        Write-Host "ℹ️  Auto-detected PRNumber: $PRNumber from path" -ForegroundColor Cyan
    }
    
    # Extract Recommended Title FIRST (needed for TitleStatus detection)
    if ([string]::IsNullOrWhiteSpace($RecommendedTitle)) {
        # Try different patterns
        if ($content -match '\*\*Recommended.*?Title.*?\*\*:?\s*[`"]?([^`"\n]+)[`"]?') {
            $RecommendedTitle = $Matches[1].Trim()
        } elseif ($content -match 'Recommended:\s*`([^`]+)`') {
            $RecommendedTitle = $Matches[1].Trim()
        } elseif ($content -match '(?s)\*\*Recommended:\*\*\s*```\s*([^\n]+)') {
            # Code fence format
            $RecommendedTitle = $Matches[1].Trim()
        } elseif ($content -match '(?s)### 📋 Title Assessment.+?\*\*Recommended:\*\*\s*```\s*([^\n]+)') {
            $RecommendedTitle = $Matches[1].Trim()
        }
        if ($RecommendedTitle) {
            Write-Host "ℹ️  Extracted RecommendedTitle: $RecommendedTitle" -ForegroundColor Cyan
        }
    }
    
    # Extract Title assessment - if RecommendedTitle exists, title needs update
    if ([string]::IsNullOrWhiteSpace($TitleStatus)) {
        # If we have a recommended title, the title needs update
        if (-not [string]::IsNullOrWhiteSpace($RecommendedTitle)) {
            $TitleStatus = "NeedsUpdate"
        }
        # Look for explicit status in Title Assessment section
        elseif ($content -match '(?s)### 📋 Title Assessment.+?\*\*Status:\*\*\s*(✅|❌|⚠️)?\s*(Good|NeedsUpdate|Needs Update)') {
            $statusMatch = $Matches[2] -replace '\s+', ''
            if ($statusMatch -eq "Good") {
                $TitleStatus = "Good"
            } else {
                $TitleStatus = "NeedsUpdate"
            }
        }
        # Fallback: check for recommended title specifically in title section
        elseif ($content -match '(?s)### 📋 Title Assessment.+?\*\*Recommended.*?Title') {
            $TitleStatus = "NeedsUpdate"
        } else {
            $TitleStatus = "Good"
        }
        Write-Host "ℹ️  Detected TitleStatus: $TitleStatus" -ForegroundColor Cyan
    }
    
    # Extract Current Title
    if ([string]::IsNullOrWhiteSpace($CurrentTitle)) {
        if ($content -match '\*\*Current.*?Title.*?\*\*:?\s*[`"]?([^`"\n]+)[`"]?') {
            $CurrentTitle = $Matches[1].Trim()
        } elseif ($content -match 'Current:\s*`([^`]+)`') {
            $CurrentTitle = $Matches[1].Trim()
        }
        if ($CurrentTitle) {
            Write-Host "ℹ️  Extracted CurrentTitle: $CurrentTitle" -ForegroundColor Cyan
        }
    }
    
    # Extract Description assessment
    if ([string]::IsNullOrWhiteSpace($DescriptionStatus)) {
        if ($content -match 'Needs\s*Rewrite|NeedsRewrite|Needs\s*Update|NeedsUpdate|Missing') {
            $DescriptionStatus = "NeedsUpdate"
        } else {
            $DescriptionStatus = "Good"
        }
        Write-Host "ℹ️  Detected DescriptionStatus: $DescriptionStatus" -ForegroundColor Cyan
    }
    
    # Extract Title Issues/Reasoning
    $titleIssues = ""
    if ([string]::IsNullOrWhiteSpace($RecommendedTitle)) {
        # No recommended title means title is good
        $titleIssues = "Title is clear and follows conventions."
    } else {
        # Extract the issues list
        if ($content -match '(?s)\*\*Issues:\*\*(.+?)(?=\*\*Recommended|\*\*Reasoning|###|$)') {
            $titleIssues = $Matches[1].Trim()
        } elseif ($content -match '(?s)### 📋 Title Assessment.+?\*\*Issues:\*\*(.+?)(?=\*\*Recommended|###|$)') {
            $titleIssues = $Matches[1].Trim()
        }
    }
    
    # Extract Description Assessment text
    if ([string]::IsNullOrWhiteSpace($DescriptionAssessment)) {
        # Try to extract from our standard summary format (### ⚠️ Description: ... or ### ✅ Description: ...)
        if ($content -match '(?s)###\s*[⚠️✅❌]*\s*Description[:\s].*?\n(.+?)(?=###\s|## |---|\z)') {
            $DescriptionAssessment = $Matches[1].Trim()
        }
        # Try to extract detailed issues from the summary
        elseif ($content -match '(?s)### 📝 Description Quality Assessment(.+?)(?=###|---|\z)') {
            $issuesSection = $Matches[1].Trim()
            $issuesSection = $issuesSection -replace '(?m)^\*\*Status:\*\*.*$\n?', ''
            $DescriptionAssessment = $issuesSection.Trim()
        }
        # Fallback
        if ([string]::IsNullOrWhiteSpace($DescriptionAssessment)) {
            $DescriptionAssessment = "No issues found."
        }
    }
    
    # Extract Missing Elements
    if ([string]::IsNullOrWhiteSpace($MissingElements)) {
        if ($content -match '(?s)Missing.*?elements?:(.+?)(?=###|$)') {
            $MissingElements = $Matches[1].Trim()
        } elseif ($content -match '(?s)Only addition needed:(.+?)(?=###|\*\*Action|$)') {
            $MissingElements = $Matches[1].Trim()
        }
    }
    
    # Extract Recommended Description
    if ([string]::IsNullOrWhiteSpace($RecommendedDescription)) {
        # First, try to find a separate recommended-description.md file in same directory
        $summaryDir = Split-Path $SummaryFile -Parent
        $recommendedDescFile = Join-Path $summaryDir "recommended-description.md"
        
        if (Test-Path $recommendedDescFile) {
            Write-Host "ℹ️  Found recommended-description.md file, loading full content..." -ForegroundColor Cyan
            $RecommendedDescription = Get-Content $recommendedDescFile -Raw -Encoding UTF8
            $RecommendedDescription = $RecommendedDescription.Trim()
        }
        # Try to extract from <details> section in summary file
        elseif ($content -match '(?s)<details>\s*<summary>Click to see proposed description</summary>\s*```markdown\s*(.+?)```\s*</details>') {
            Write-Host "ℹ️  Extracted recommended description from expandable section..." -ForegroundColor Cyan
            $RecommendedDescription = $Matches[1].Trim()
        }
        # Otherwise, try to extract from header in summary file
        elseif ($content -match '(?s)### Recommended Description(.+?)(?=### [A-Z]|---|\z)') {
            $RecommendedDescription = $Matches[1].Trim()
        }
        # If still empty, check for recommended description block in summary
        elseif ($content -match '(?s)```markdown\s*<!-- Please let the below(.+?)```') {
            $RecommendedDescription = $Matches[1].Trim()
        }
    }
    
    # Extract Code Review Status and Findings
    if ([string]::IsNullOrWhiteSpace($CodeReviewStatus)) {
        # First, try to find a separate code-review.md file in same directory
        $summaryDir = Split-Path $SummaryFile -Parent
        $codeReviewFile = Join-Path $summaryDir "code-review.md"
        
        if (Test-Path $codeReviewFile) {
            Write-Host "ℹ️  Found code-review.md file, loading content..." -ForegroundColor Cyan
            $codeReviewContent = Get-Content $codeReviewFile -Raw -Encoding UTF8
            $CodeReviewFindings = $codeReviewContent.Trim()
            
            # Determine status from content
            if ($codeReviewContent -match '🔴 Critical Issues') {
                $CodeReviewStatus = "IssuesFound"
            } elseif ($codeReviewContent -match '(✅ Looks Good|No.*?issues found|Code review passed)') {
                $CodeReviewStatus = "Passed"
            } else {
                $CodeReviewStatus = "Passed"
            }
        }
        # Try to extract from summary file - look for Code Review section
        elseif ($content -match '(?s)## Code Review Findings(.+?)(?=## [A-Z]|---|\z)') {
            Write-Host "ℹ️  Extracted code review from summary file..." -ForegroundColor Cyan
            $CodeReviewFindings = $Matches[1].Trim()
            
            # Determine status from content
            if ($CodeReviewFindings -match '🔴 Critical Issues') {
                $CodeReviewStatus = "IssuesFound"
            } else {
                $CodeReviewStatus = "Passed"
            }
        }
        # Also check for ### Code Review format
        elseif ($content -match '(?s)### 🔍 Code Review(.+?)(?=### [A-Z]|---|\z)') {
            $CodeReviewFindings = $Matches[1].Trim()
            if ($CodeReviewFindings -match '🔴 Critical Issues') {
                $CodeReviewStatus = "IssuesFound"
            } else {
                $CodeReviewStatus = "Passed"
            }
        }
        else {
            # Default to Skipped if no code review found
            $CodeReviewStatus = "Skipped"
        }
        
        if ($CodeReviewStatus -ne "Skipped") {
            Write-Host "ℹ️  Detected CodeReviewStatus: $CodeReviewStatus" -ForegroundColor Cyan
        }
    }
}

# Validate required parameters
if ($PRNumber -eq 0) {
    throw "PRNumber is required. Provide via -PRNumber or use -SummaryFile with path containing PR number"
}

if ([string]::IsNullOrWhiteSpace($TitleStatus)) {
    $TitleStatus = "Good"
}

if ([string]::IsNullOrWhiteSpace($CurrentTitle)) {
    # Try to fetch from GitHub
    $prInfo = gh pr view $PRNumber --json title --jq '.title' 2>$null
    if ($prInfo) {
        $CurrentTitle = $prInfo
        Write-Host "ℹ️  Fetched CurrentTitle from GitHub: $CurrentTitle" -ForegroundColor Cyan
    } else {
        throw "CurrentTitle is required. Provide via -CurrentTitle or use -SummaryFile"
    }
}

if ([string]::IsNullOrWhiteSpace($DescriptionStatus)) {
    $DescriptionStatus = "Good"
}

if ([string]::IsNullOrWhiteSpace($DescriptionAssessment)) {
    if (-not [string]::IsNullOrWhiteSpace($SummaryFile)) {
        # We have a summary file but couldn't extract a specific assessment section.
        # Use the full summary content as the assessment (the whole file IS the assessment).
        $DescriptionAssessment = $content
        Write-Host "ℹ️  Using full summary file content as DescriptionAssessment" -ForegroundColor Cyan
    } else {
        throw "DescriptionAssessment is required. Provide via -DescriptionAssessment or use -SummaryFile"
    }
}

# Warn if description needs work but no recommended description is provided
if (($DescriptionStatus -eq "NeedsUpdate" -or $DescriptionStatus -eq "NeedsRewrite") -and [string]::IsNullOrWhiteSpace($RecommendedDescription)) {
    Write-Host "⚠️  Warning: DescriptionStatus is '$DescriptionStatus' but no RecommendedDescription provided." -ForegroundColor Yellow
    Write-Host "   Consider providing -RecommendedDescription with a suggested PR description." -ForegroundColor Yellow
}

# Warn if title needs update but no recommended title is provided
if ($TitleStatus -eq "NeedsUpdate" -and [string]::IsNullOrWhiteSpace($RecommendedTitle)) {
    Write-Host "⚠️  Warning: TitleStatus is 'NeedsUpdate' but no RecommendedTitle provided." -ForegroundColor Yellow
    Write-Host "   Consider providing -RecommendedTitle with a suggested title." -ForegroundColor Yellow
}

# Initialize $titleIssues from parameter if provided, otherwise set default based on status
if (-not (Get-Variable -Name 'titleIssues' -ErrorAction SilentlyContinue) -or [string]::IsNullOrWhiteSpace($titleIssues)) {
    if (-not [string]::IsNullOrWhiteSpace($TitleIssues)) {
        $titleIssues = $TitleIssues
    } elseif ($TitleStatus -eq "Good") {
        $titleIssues = "Title is clear and follows conventions."
    } else {
        $titleIssues = ""
    }
}

# Status emoji mapping
$titleEmoji = switch ($TitleStatus) {
    "Good" { "✅" }
    "NeedsUpdate" { "⚠️" }
    default { "" }
}

$descEmoji = switch ($DescriptionStatus) {
    "Good" { "✅" }
    "NeedsUpdate" { "⚠️" }
    default { "" }
}

# Format status with spaces for display
$titleStatusDisplay = switch ($TitleStatus) {
    "NeedsUpdate" { "Needs Update" }
    default { $TitleStatus }
}

$descStatusDisplay = switch ($DescriptionStatus) {
    "NeedsUpdate" { "Needs Update" }
    default { $DescriptionStatus }
}

# Build Title section (collapsible)
$titleSection = @"
<details>

<summary><b>Title: $titleEmoji $titleStatusDisplay</b></summary>

<br>

**Current:** ``$CurrentTitle``
"@

if (-not [string]::IsNullOrWhiteSpace($titleIssues) -and $TitleStatus -eq "NeedsUpdate") {
    $titleSection += "`n`n**Issues:**`n$titleIssues"
}

if (-not [string]::IsNullOrWhiteSpace($RecommendedTitle) -and $TitleStatus -eq "NeedsUpdate") {
    $titleSection += @"


**Recommended:** ``$RecommendedTitle``
"@
}

$titleSection += @"

</details>
"@

# Build Description section (collapsible)
$descSection = @"
<details>

<summary><b>Description: $descEmoji $descStatusDisplay</b></summary>

<br>

$DescriptionAssessment

"@

if (-not [string]::IsNullOrWhiteSpace($MissingElements)) {
    $descSection += @"
**Missing Elements:**

$MissingElements

"@
}

if (-not [string]::IsNullOrWhiteSpace($RecommendedDescription)) {
    $descSection += @"
### ✨ Suggested PR Description

$RecommendedDescription

"@
}

$descSection += @"
</details>
"@

# Code Review status emoji and display
$codeReviewEmoji = switch ($CodeReviewStatus) {
    "Passed" { "✅" }
    "IssuesFound" { "⚠️" }
    "Skipped" { "⏭️" }
    default { "⏭️" }
}

$codeReviewStatusDisplay = switch ($CodeReviewStatus) {
    "IssuesFound" { "Issues Found" }
    default { $CodeReviewStatus }
}

# Build Code Review section (collapsible) - only if not skipped or if we have findings
$codeReviewSection = ""
if ($CodeReviewStatus -ne "Skipped" -or -not [string]::IsNullOrWhiteSpace($CodeReviewFindings)) {
    # Default status to "Passed" if we have findings but no explicit status
    if ([string]::IsNullOrWhiteSpace($CodeReviewStatus)) {
        $CodeReviewStatus = "Passed"
        $codeReviewEmoji = "✅"
        $codeReviewStatusDisplay = "Passed"
    }
    
    $codeReviewSection = @"

<details>

<summary><b>Code Review: $codeReviewEmoji $codeReviewStatusDisplay</b></summary>

<br>

"@

    if (-not [string]::IsNullOrWhiteSpace($CodeReviewFindings)) {
        # Trim the findings and ensure proper newline spacing
        $trimmedFindings = $CodeReviewFindings.Trim()
        $codeReviewSection += "`n$trimmedFindings"
    } else {
        $codeReviewSection += "`nNo significant issues found. Code follows best practices."
    }

    $codeReviewSection += @"

</details>
"@
}

# ============================================================================
# COMMENT POSTING
# Default: unified (inject into AI Summary comment). Use -Standalone for separate comment.
# ============================================================================

if (-not $Standalone) {
    # ========================================================================
    # UNIFIED MODE: Inject into the <!-- AI Summary --> comment
    # ========================================================================

    $MAIN_MARKER = "<!-- AI Summary -->"
    $SECTION_START = "<!-- SECTION:PR-FINALIZE -->"
    $SECTION_END = "<!-- /SECTION:PR-FINALIZE -->"
    $LEGACY_MARKER = "<!-- PR-FINALIZE-COMMENT -->"

    # Build the finalize section wrapped in expandable details
    $finalizeSection = @"
$SECTION_START
<details>
<summary>📋 <strong>Expand PR Finalization Review</strong></summary>

---

$titleSection

$descSection
$codeReviewSection

---

</details>
$SECTION_END
"@

    Write-Host "`nUnified mode: injecting into AI Summary comment on #$PRNumber..." -ForegroundColor Yellow

    $existingUnifiedComment = $null
    $existingLegacyComment = $null

    try {
        $commentsJson = gh api "repos/dotnet/maui/issues/$PRNumber/comments?per_page=100" 2>$null
        $comments = $commentsJson | ConvertFrom-Json

        foreach ($comment in $comments) {
            if ($comment.body -match [regex]::Escape($MAIN_MARKER)) {
                $existingUnifiedComment = $comment
                Write-Host "✓ Found unified AI Summary comment (ID: $($comment.id))" -ForegroundColor Green
            }
            if ($comment.body -match [regex]::Escape($LEGACY_MARKER)) {
                $existingLegacyComment = $comment
            }
        }
    } catch {
        Write-Host "⚠️ Could not fetch comments: $_" -ForegroundColor Yellow
    }

    if ($DryRun) {
        if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
            $PreviewFile = "CustomAgentLogsTmp/PRState/$PRNumber/ai-summary-comment-preview.md"
        }

        $previewDir = Split-Path $PreviewFile -Parent
        if (-not (Test-Path $previewDir)) {
            New-Item -ItemType Directory -Path $previewDir -Force | Out-Null
        }

        $existingPreview = ""
        if (Test-Path $PreviewFile) {
            $existingPreview = Get-Content $PreviewFile -Raw -Encoding UTF8
        }

        if ($existingPreview -match [regex]::Escape($SECTION_START)) {
            $pattern = [regex]::Escape($SECTION_START) + "[\s\S]*?" + [regex]::Escape($SECTION_END)
            $finalComment = $existingPreview -replace $pattern, $finalizeSection
        } elseif (-not [string]::IsNullOrWhiteSpace($existingPreview)) {
            $finalComment = $existingPreview.TrimEnd() + "`n`n" + $finalizeSection
        } else {
            $finalComment = @"
$MAIN_MARKER

## 🤖 AI Summary

$finalizeSection
"@
        }

        Set-Content -Path $PreviewFile -Value "$($finalComment.TrimEnd())`n" -Encoding UTF8 -NoNewline

        Write-Host "`n=== COMMENT PREVIEW ===" -ForegroundColor Yellow
        Write-Host $finalComment
        Write-Host "`n=== END PREVIEW ===" -ForegroundColor Yellow
        Write-Host "`n✅ Preview saved to: $PreviewFile" -ForegroundColor Green
        exit 0
    }

    if ($existingUnifiedComment) {
        $body = $existingUnifiedComment.body

        if ($body -match [regex]::Escape($SECTION_START)) {
            $pattern = [regex]::Escape($SECTION_START) + "[\s\S]*?" + [regex]::Escape($SECTION_END)
            $newBody = $body -replace $pattern, $finalizeSection
        } else {
            $newBody = $body.TrimEnd() + "`n`n" + $finalizeSection
        }

        $newBody = $newBody -replace "`n{4,}", "`n`n`n"

        Write-Host "Updating unified comment ID $($existingUnifiedComment.id) with PR finalize section..." -ForegroundColor Yellow
        $tempFile = [System.IO.Path]::GetTempFileName()
        @{ body = $newBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

        $result = gh api --method PATCH "repos/dotnet/maui/issues/comments/$($existingUnifiedComment.id)" --input $tempFile --jq '.html_url'
        Remove-Item $tempFile
        Write-Host "✅ PR finalize section added to unified comment: $result" -ForegroundColor Green
    } else {
        $commentBody = @"
$MAIN_MARKER

## 🤖 AI Summary

$finalizeSection
"@

        Write-Host "Creating new unified comment with PR finalize section on PR #$PRNumber..." -ForegroundColor Yellow
        $tempFile = [System.IO.Path]::GetTempFileName()
        @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

        $result = gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile --jq '.html_url'
        Remove-Item $tempFile
        Write-Host "✅ Unified comment posted: $result" -ForegroundColor Green
    }

    # Clean up legacy standalone finalize comment if it exists
    if ($existingLegacyComment) {
        Write-Host "🧹 Removing legacy standalone PR Finalization comment (ID: $($existingLegacyComment.id))..." -ForegroundColor Yellow
        gh api --method DELETE "repos/dotnet/maui/issues/comments/$($existingLegacyComment.id)" 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✅ Legacy comment removed" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️ Could not remove legacy comment (non-fatal)" -ForegroundColor Yellow
        }
    }

} else {
    # ========================================================================
    # STANDALONE MODE (default): Post as separate PR Finalization comment
    # ========================================================================

$FINALIZE_MARKER = "<!-- PR-FINALIZE-COMMENT -->"

Write-Host "`nChecking for existing PR Finalization comment on #$PRNumber..." -ForegroundColor Yellow
$existingComment = $null

try {
    $commentsJson = gh api "repos/dotnet/maui/issues/$PRNumber/comments?per_page=100" 2>$null
    $comments = $commentsJson | ConvertFrom-Json
    
    foreach ($comment in $comments) {
        if ($comment.body -match [regex]::Escape($FINALIZE_MARKER)) {
            $existingComment = $comment
            Write-Host "✓ Found existing PR Finalization comment (ID: $($comment.id))" -ForegroundColor Green
            break
        }
    }
    
    if (-not $existingComment) {
        Write-Host "✓ No existing PR Finalization comment found - will create new" -ForegroundColor Yellow
    }
} catch {
    Write-Host "✓ No existing PR Finalization comment found - will create new" -ForegroundColor Yellow
}

# Build the full comment body (always replaces existing comment entirely)
$commentBody = @"
## 📋 PR Finalization Review
$FINALIZE_MARKER

$titleSection

$descSection
$codeReviewSection
"@

if ($DryRun) {
    # File-based DryRun: uses separate preview file for finalize (separate comment from unified)
    if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
        $PreviewFile = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize-preview.md"
    }
    
    # Ensure directory exists
    $previewDir = Split-Path $PreviewFile -Parent
    if (-not (Test-Path $previewDir)) {
        New-Item -ItemType Directory -Path $previewDir -Force | Out-Null
    }
    
    # For finalize, we replace the entire file (it's a separate comment)
    Write-Host "ℹ️  Saving finalize preview to: $PreviewFile" -ForegroundColor Cyan
    Set-Content -Path $PreviewFile -Value "$($commentBody.TrimEnd())`n" -Encoding UTF8 -NoNewline
    
    Write-Host "`n=== COMMENT PREVIEW ===" -ForegroundColor Yellow
    Write-Host $commentBody
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

} # end standalone mode
