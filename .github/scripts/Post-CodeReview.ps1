#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts a formatted code review comment on a GitHub Pull Request, matching the AI Summary style.

.DESCRIPTION
    Takes review content (from a file or string) and wraps it in a collapsible HTML
    <details>/<summary> section with PR metadata. Supports create-or-update via HTML marker.
    
    Auto-detects verdict from content by scanning for "Verdict: LGTM", "Verdict: NEEDS_CHANGES",
    or "Verdict: NEEDS_DISCUSSION" and sets the appropriate colored status dot.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER ReviewFile
    Path to the review content markdown file (required unless -Content is used)

.PARAMETER Content
    Review content as a string (alternative to -ReviewFile)

.PARAMETER DryRun
    Print the formatted comment to stdout instead of posting

.PARAMETER Update
    Update an existing code review comment instead of creating a new one

.EXAMPLE
    ./Post-CodeReview.ps1 -PRNumber 12345 -ReviewFile /tmp/review.md

.EXAMPLE
    ./Post-CodeReview.ps1 -PRNumber 12345 -ReviewFile /tmp/review.md -DryRun

.EXAMPLE
    ./Post-CodeReview.ps1 -PRNumber 12345 -ReviewFile /tmp/review.md -Update
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$ReviewFile,

    [Parameter(Mandatory = $false)]
    [string]$Content,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$Update
)

$ErrorActionPreference = "Stop"
$COMMENT_MARKER = "<!-- Code Review -->"

# ============================================================================
# INPUT VALIDATION
# ============================================================================

if (-not $ReviewFile -and -not $Content) {
    throw "Either -ReviewFile or -Content must be provided."
}

if ($ReviewFile -and -not (Test-Path $ReviewFile)) {
    throw "Review file not found: $ReviewFile"
}

if ($ReviewFile) {
    $Content = Get-Content -Path $ReviewFile -Raw -Encoding UTF8
}

if ([string]::IsNullOrWhiteSpace($Content)) {
    throw "Review content is empty."
}

# ============================================================================
# FETCH PR METADATA
# ============================================================================

Write-Host "📋 Fetching PR #$PRNumber metadata..." -ForegroundColor Cyan

$prJson = gh pr view $PRNumber --json title,headRefOid,author,number,url 2>&1
if ($LASTEXITCODE -ne 0) {
    throw "Failed to fetch PR #$PRNumber metadata: $prJson"
}

$pr = $prJson | ConvertFrom-Json
$prTitle = $pr.title
$commitSha = $pr.headRefOid.Substring(0, 7)
$commitFull = $pr.headRefOid
$prAuthor = $pr.author.login
$prUrl = $pr.url

Write-Host "  PR: #$PRNumber - $prTitle" -ForegroundColor Gray
Write-Host "  Author: $prAuthor | HEAD: $commitSha" -ForegroundColor Gray

# ============================================================================
# DETERMINE VERDICT
# ============================================================================

$verdictDot = "🔵"
$verdictLabel = "In Review"
if ($Content -match 'Verdict:\s*\*?\*?LGTM') {
    $verdictDot = "🟢"
    $verdictLabel = "Approved"
}
elseif ($Content -match 'Verdict:\s*\*?\*?NEEDS_CHANGES') {
    $verdictDot = "🟡"
    $verdictLabel = "Changes Suggested"
}
elseif ($Content -match 'Verdict:\s*\*?\*?NEEDS_DISCUSSION') {
    $verdictDot = "🟠"
    $verdictLabel = "Discussion Needed"
}

# ============================================================================
# BUILD FORMATTED COMMENT
# ============================================================================

$sb = [System.Text.StringBuilder]::new()

# Marker for find/update
[void]$sb.AppendLine($COMMENT_MARKER)
[void]$sb.AppendLine("")
[void]$sb.AppendLine("## $verdictDot .NET MAUI Review - $verdictLabel")
[void]$sb.AppendLine("")

# Single collapsible section with all content inside
[void]$sb.AppendLine("<details>")
[void]$sb.AppendLine("<summary><strong>Expand Full Review</strong> - <a href=`"https://github.com/dotnet/maui/commit/$commitFull`"><code>$commitSha</code></a> - <strong>$prTitle</strong></summary>")
[void]$sb.AppendLine("")
[void]$sb.AppendLine("---")
[void]$sb.AppendLine("")
[void]$sb.AppendLine($Content)
[void]$sb.AppendLine("")
[void]$sb.AppendLine("</details>")

$formattedComment = $sb.ToString()

# ============================================================================
# POST OR DRY-RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  DRY RUN — Comment preview (not posted)" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    Write-Host $formattedComment
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  Comment length: $($formattedComment.Length) chars" -ForegroundColor Gray
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    return
}

# Write to temp file for posting (avoids shell escaping issues)
$tempFile = [System.IO.Path]::GetTempFileName()
try {
    Set-Content -Path $tempFile -Value $formattedComment -Encoding UTF8 -NoNewline

    if ($Update) {
        # Find existing comment with marker
        Write-Host "🔍 Searching for existing code review comment..." -ForegroundColor Cyan
        $existingComments = gh pr view $PRNumber --json comments --jq ".comments[] | select(.body | startswith(`"$COMMENT_MARKER`")) | .url" 2>&1

        if ($LASTEXITCODE -eq 0 -and $existingComments) {
            $commentUrl = ($existingComments -split "`n" | Select-Object -Last 1).Trim()
            if ($commentUrl -match '#issuecomment-(\d+)$') {
                $commentId = $Matches[1]
                Write-Host "  Found existing comment: $commentId — updating..." -ForegroundColor Gray
                $result = gh api "repos/dotnet/maui/issues/comments/$commentId" -X PATCH -F "body=@$tempFile" 2>&1
                if ($LASTEXITCODE -ne 0) {
                    Write-Host "⚠️  Update failed, creating new comment instead..." -ForegroundColor Yellow
                    $result = gh pr comment $PRNumber --body-file $tempFile 2>&1
                }
            }
            else {
                $result = gh pr comment $PRNumber --body-file $tempFile 2>&1
            }
        }
        else {
            Write-Host "  No existing comment found — creating new..." -ForegroundColor Gray
            $result = gh pr comment $PRNumber --body-file $tempFile 2>&1
        }
    }
    else {
        $result = gh pr comment $PRNumber --body-file $tempFile 2>&1
    }

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to post comment: $result"
    }

    Write-Host ""
    Write-Host "✅ Code review comment posted on PR #$PRNumber" -ForegroundColor Green
    Write-Host "   $result" -ForegroundColor Gray
}
finally {
    Remove-Item -Path $tempFile -ErrorAction SilentlyContinue
}
