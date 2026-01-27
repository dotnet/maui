#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates a PR finalize comment on a GitHub Pull Request.

.DESCRIPTION
    Creates ONE comment for PR finalization reviews with each review in a collapsible section.
    Uses HTML marker <!-- PR-FINALIZE-COMMENT --> for identification.
    
    **NEW: Auto-loads from CustomAgentLogsTmp/PRState/{PRNumber}/pr-finalize/**
    
    If an existing finalize comment exists, it will be EDITED with the new review added.
    Otherwise, a new comment will be created.
    
    Format:
    ## 📋 PR Finalization Review
    <!-- PR-FINALIZE-COMMENT -->
    
    <details>
    <summary><b>Review 1: Title and description check ✅ Ready</b></summary>

    ... review details ...
    </details>
    
    <details>
    <summary><b>Review 2: Updated after implementation change ⚠️ Needs Update</b></summary>

    ... review details ...
    </details>

.PARAMETER PRNumber
    The PR number to post comment on (required unless SummaryFile provided)

.PARAMETER SummaryFile
    Path to pr-finalize-summary.md file. If provided, auto-loads review data from this file.

.PARAMETER ReviewNumber
    The review number (1, 2, 3, etc.) - auto-detected from SummaryFile if not specified

.PARAMETER ReviewDescription
    Brief description of what was reviewed (required unless loading from SummaryFile)

.PARAMETER TitleStatus
    Title assessment: "Good", "NeedsUpdate" (required unless loading from SummaryFile)

.PARAMETER CurrentTitle
    Current PR title (required unless loading from SummaryFile)

.PARAMETER RecommendedTitle
    Recommended PR title (optional - only if TitleStatus is NeedsUpdate)

.PARAMETER DescriptionStatus
    Description assessment: "Excellent", "Good", "NeedsUpdate", "NeedsRewrite" (required unless loading from SummaryFile)

.PARAMETER DescriptionAssessment
    Quality assessment of the description (required unless loading from SummaryFile)

.PARAMETER MissingElements
    List of missing elements (optional)

.PARAMETER RecommendedDescription
    Full recommended description (optional - only if needs rewrite)

.PARAMETER DryRun
    Print comment instead of posting

.EXAMPLE
    # Simplest: Just provide PR number (auto-loads from CustomAgentLogsTmp)
    ./post-pr-finalize-comment.ps1 -PRNumber 27246

.EXAMPLE
    # Or provide summary file path
    ./post-pr-finalize-comment.ps1 -SummaryFile CustomAgentLogsTmp/PRState/27246/pr-finalize/pr-finalize-summary.md

.EXAMPLE
    # Manual parameters (legacy)
    ./post-pr-finalize-comment.ps1 -PRNumber 25748 -ReviewNumber 1 `
        -ReviewDescription "Initial finalization check" `
        -TitleStatus "Good" `
        -CurrentTitle "[iOS] Fix SafeArea padding calculation" `
        -DescriptionStatus "Good" `
        -DescriptionAssessment "Clear structure, accurate technical details, matches implementation"
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$PRNumber,

    [Parameter(Mandatory=$false)]
    [string]$SummaryFile,

    [Parameter(Mandatory=$false)]
    [int]$ReviewNumber,

    [Parameter(Mandatory=$false)]
    [string]$ReviewDescription,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Good", "NeedsUpdate", "")]
    [string]$TitleStatus,

    [Parameter(Mandatory=$false)]
    [string]$CurrentTitle,

    [Parameter(Mandatory=$false)]
    [string]$RecommendedTitle,

    [Parameter(Mandatory=$false)]
    [ValidateSet("Excellent", "Good", "NeedsUpdate", "NeedsRewrite", "")]
    [string]$DescriptionStatus,

    [Parameter(Mandatory=$false)]
    [string]$DescriptionAssessment,

    [Parameter(Mandatory=$false)]
    [string]$MissingElements,

    [Parameter(Mandatory=$false)]
    [string]$RecommendedDescription,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

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
if ($PRNumber -gt 0 -and [string]::IsNullOrWhiteSpace($SummaryFile) -and [string]::IsNullOrWhiteSpace($ReviewDescription)) {
    $summaryPath = "CustomAgentLogsTmp/PRState/$PRNumber/pr-finalize/pr-finalize-summary.md"
    if (-not (Test-Path $summaryPath)) {
        $repoRoot = git rev-parse --show-toplevel 2>$null
        if ($repoRoot) {
            $summaryPath = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/pr-finalize/pr-finalize-summary.md"
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
    
    # Extract ReviewNumber (default to 1)
    if ($ReviewNumber -eq 0) {
        # Check if there's a review number in the content
        if ($content -match 'Review (\d+)') {
            $ReviewNumber = [int]$Matches[1]
        } else {
            $ReviewNumber = 1
        }
        Write-Host "ℹ️  Using ReviewNumber: $ReviewNumber" -ForegroundColor Cyan
    }
    
    # Extract verdict for ReviewDescription
    if ([string]::IsNullOrWhiteSpace($ReviewDescription)) {
        if ($content -match '✅\s*No Changes Needed') {
            $ReviewDescription = "Finalization check - Ready"
        } elseif ($content -match '⚠️\s*Needs Updates') {
            $ReviewDescription = "Finalization check - Needs Updates"
        } else {
            $ReviewDescription = "Finalization review"
        }
    }
    
    # Extract Title assessment
    if ([string]::IsNullOrWhiteSpace($TitleStatus)) {
        # Check if there are title issues or a recommended title
        $hasRecommendedTitle = $content -match '\*\*Recommended.*?Title'
        $hasTitleIssues = $content -match '(?s)### 📋 Title Assessment.+?\*\*Issues:\*\*'
        
        if ($hasRecommendedTitle -or $hasTitleIssues) {
            $TitleStatus = "NeedsUpdate"
        } elseif ($content -match '### Title.*?✅') {
            $TitleStatus = "Good"
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
    
    # Extract Recommended Title
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
    
    # Extract Description assessment
    if ([string]::IsNullOrWhiteSpace($DescriptionStatus)) {
        if ($content -match 'Description.*?Excellent|Excellent.*?description') {
            $DescriptionStatus = "Excellent"
        } elseif ($content -match 'Description.*?Good|Good.*?description') {
            $DescriptionStatus = "Good"
        } elseif ($content -match 'Needs\s*Rewrite|NeedsRewrite') {
            $DescriptionStatus = "NeedsRewrite"
        } elseif ($content -match 'Needs\s*Update|NeedsUpdate') {
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
        # Try to extract detailed issues from the summary
        $issuesSection = ""
        if ($content -match '(?s)### 📝 Description Quality Assessment(.+?)(?=###|---|\z)') {
            $issuesSection = $Matches[1].Trim()
        } elseif ($content -match '(?s)\*\*Issues:\*\*(.+?)(?=\*\*Recommended|\*\*Action|###|---|\z)') {
            $issuesSection = $Matches[1].Trim()
        }
        
        # Try to extract what works
        $worksSection = ""
        if ($content -match '(?s)### ❌ Issues Found(.+?)(?=###|---|\z)') {
            $worksSection = $Matches[1].Trim()
        } elseif ($content -match '(?s)\| Issue \| Severity \| Details \|(.+?)(?=###|---|\z)') {
            # Extract table content
            $worksSection = "**Issues:**`n" + $Matches[1].Trim()
        }
        
        # Combine into assessment
        if (-not [string]::IsNullOrWhiteSpace($issuesSection) -or -not [string]::IsNullOrWhiteSpace($worksSection)) {
            $DescriptionAssessment = ""
            if ($worksSection) { $DescriptionAssessment += $worksSection + "`n`n" }
            if ($issuesSection) { $DescriptionAssessment += $issuesSection }
        } else {
            # Fallback - try to get the verdict section
            if ($content -match '(?s)## ⚠️ Verdict:(.+?)(?=###|$)') {
                $DescriptionAssessment = $Matches[1].Trim()
            } else {
                $DescriptionAssessment = "Description needs updates. See details below."
            }
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
}

# Validate required parameters
if ($PRNumber -eq 0) {
    throw "PRNumber is required. Provide via -PRNumber or use -SummaryFile with path containing PR number"
}

if ($ReviewNumber -eq 0) {
    $ReviewNumber = 1
}

if ([string]::IsNullOrWhiteSpace($ReviewDescription)) {
    throw "ReviewDescription is required. Provide via -ReviewDescription or use -SummaryFile"
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
    throw "DescriptionAssessment is required. Provide via -DescriptionAssessment or use -SummaryFile"
}

# Status emoji mapping
$titleEmoji = switch ($TitleStatus) {
    "Good" { "✅" }
    "NeedsUpdate" { "⚠️" }
    default { "" }
}

$descEmoji = switch ($DescriptionStatus) {
    "Excellent" { "✅" }
    "Good" { "✅" }
    "NeedsUpdate" { "⚠️" }
    "NeedsRewrite" { "❌" }
    default { "" }
}

# Overall status
$overallStatus = if ($TitleStatus -eq "Good" -and ($DescriptionStatus -eq "Excellent" -or $DescriptionStatus -eq "Good")) {
    "✅ Ready"
} elseif ($DescriptionStatus -eq "NeedsRewrite") {
    "❌ Needs Rewrite"
} else {
    "⚠️ Needs Update"
}

# Build the new review section (collapsible)
$reviewSection = @"
<details>
<summary><b>Review $ReviewNumber`: $ReviewDescription $overallStatus</b></summary>

### Title $titleEmoji $TitleStatus

**Current:** ``$CurrentTitle``
"@

if (-not [string]::IsNullOrWhiteSpace($titleIssues) -and $TitleStatus -eq "NeedsUpdate") {
    $reviewSection += "`n`n**Issues:**`n$titleIssues"
}

if (-not [string]::IsNullOrWhiteSpace($RecommendedTitle)) {
    $reviewSection += "`n`n**Recommended:** ``$RecommendedTitle``"
}

$reviewSection += @"


### Description $descEmoji $DescriptionStatus

$DescriptionAssessment

"@

if (-not [string]::IsNullOrWhiteSpace($MissingElements)) {
    $reviewSection += @"
### Missing Elements

$MissingElements

"@
}

if (-not [string]::IsNullOrWhiteSpace($RecommendedDescription)) {
    # Always format recommended description in an expandable section
    $reviewSection += @"

<details>
<summary>Click to see proposed description</summary>

``````markdown
$RecommendedDescription
``````

</details>

"@
}

$reviewSection += @"
</details>
"@

# ============================================================================
# STANDALONE COMMENT HANDLING
# Posts as separate PR Finalization comment with marker
# ============================================================================

$FINALIZE_MARKER = "<!-- PR-FINALIZE-COMMENT -->"

Write-Host "`nChecking for existing PR Finalization comment on #$PRNumber..." -ForegroundColor Yellow
$existingComment = $null

try {
    $commentsJson = gh api "repos/dotnet/maui/issues/$PRNumber/comments" 2>$null
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

# Build the full comment body
$commentBody = @"
## 📋 PR Finalization Review
$FINALIZE_MARKER

"@

if ($existingComment) {
    # Parse existing reviews
    $existingReviews = @()
    $pattern = '(?s)<details>\s*<summary><b>Review (\d+):.+?</details>'
    $matches = [regex]::Matches($existingComment.body, $pattern)
    
    foreach ($match in $matches) {
        $reviewNum = [int]$match.Groups[1].Value
        if ($reviewNum -ne $ReviewNumber) {
            $existingReviews += $match.Value
        }
    }
    
    # Add existing reviews first, then new review
    if ($existingReviews.Count -gt 0) {
        $commentBody += ($existingReviews -join "`n`n")
        $commentBody += "`n`n"
    }
}

$commentBody += $reviewSection

if ($DryRun) {
    # File-based DryRun: uses separate preview file for finalize (separate comment from unified)
    if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
        $PreviewFile = "CustomAgentLogsTmp/PRState/$PRNumber/pr-finalize-preview.md"
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
