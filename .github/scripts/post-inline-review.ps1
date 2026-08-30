#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts inline review comments on a GitHub Pull Request from a JSON findings file.

.DESCRIPTION
    Reads inline-findings.json (produced by the maui-expert-reviewer agent) and posts
    them as a GitHub PR review with inline file:line comments.

    Also posts review-summary.md as the review body.

    Uses the GitHub Pulls Review API to create a single review with all inline comments
    attached at their exact file:line locations.

.PARAMETER PRNumber
    The pull request number (required)

.PARAMETER FindingsFile
    Path to inline-findings.json. Default: CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/inline-findings.json

.PARAMETER SummaryFile
    Path to review-summary.md. Default: CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/review-summary.md

.PARAMETER DryRun
    Print the review payload instead of posting

.EXAMPLE
    ./post-inline-review.ps1 -PRNumber 12345

.EXAMPLE
    ./post-inline-review.ps1 -PRNumber 12345 -DryRun

.EXAMPLE
    ./post-inline-review.ps1 -PRNumber 12345 -FindingsFile /tmp/findings.json
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [string]$FindingsFile,

    [Parameter(Mandatory = $false)]
    [string]$SummaryFile,

    [Parameter(Mandatory = $false)]
    [ValidatePattern('^$|^[0-9a-fA-F]{40}$')]
    [string]$ReviewedCommit = '',

    # Repository holding the pull request, in `owner/name` form. Defaults to the
    # upstream project so every existing caller keeps its behaviour. The bot's own
    # fix pull requests live on the testing fork, and a review that always posted
    # to dotnet/maui would either 404 or, far worse, attach a review to whichever
    # unrelated upstream pull request happens to carry the same number.
    [Parameter(Mandatory = $false)]
    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})/[A-Za-z0-9._-]+$')]
    [string]$Repository = 'dotnet/maui',

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

if (-not (Get-Command ConvertTo-AzdoSafeConsole -CommandType Function -ErrorAction SilentlyContinue)) {
    function ConvertTo-AzdoSafeConsole {
        param([string]$Text)
        return ($Text -replace '[\r\n\f\v]+', ' ') -replace '##(?=\[|vso\[)', '## '
    }
}

function New-InlineReviewMarker {
    param(
        [Parameter(Mandatory = $true)]
        [string]$ReviewedHead,

        [Parameter(Mandatory = $true)]
        [object[]]$Comments
    )

    $normalized = @($Comments | ForEach-Object {
        [pscustomobject][ordered]@{
            path = [string]$_.path
            line = [int]$_.line
            body = [string]$_.body
        }
    } | Sort-Object path, line, body)
    $canonicalJson = $normalized | ConvertTo-Json -Depth 5 -Compress
    $sha256 = [Security.Cryptography.SHA256]::Create()
    try {
        $hashBytes = $sha256.ComputeHash([Text.Encoding]::UTF8.GetBytes($canonicalJson))
    } finally {
        $sha256.Dispose()
    }
    $findingsHash = ($hashBytes | ForEach-Object { $_.ToString('x2') }) -join ''
    return "<!-- maui-copilot-inline-review:$($ReviewedHead.ToLowerInvariant()):$findingsHash -->"
}

function Test-InlineReviewMarkerExists {
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$ReviewBodies,

        [Parameter(Mandatory = $true)]
        [string]$Marker
    )

    foreach ($body in $ReviewBodies) {
        if (-not [string]::IsNullOrEmpty($body) -and
            $body.IndexOf($Marker, [StringComparison]::Ordinal) -ge 0) {
            return $true
        }
    }
    return $false
}

# ============================================================================
# RESOLVE FILE PATHS
# ============================================================================

$PRAgentDir = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
if (-not (Test-Path $PRAgentDir)) {
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($repoRoot) {
        $PRAgentDir = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    }
}

if (-not $FindingsFile) {
    $FindingsFile = Join-Path $PRAgentDir "inline-findings.json"
}
if (-not $SummaryFile) {
    $SummaryFile = Join-Path $PRAgentDir "review-summary.md"
}

if (-not (Test-Path $FindingsFile)) {
    Write-Host "No findings file found at: $FindingsFile" -ForegroundColor Yellow
    Write-Host "Nothing to post." -ForegroundColor Yellow
    exit 0
}

# ============================================================================
# LOAD FINDINGS
# ============================================================================

Write-Host "Loading findings from: $FindingsFile" -ForegroundColor Cyan
$rawJson = Get-Content -Path $FindingsFile -Raw -Encoding UTF8

# Guard: an empty or whitespace-only findings file means the expert review
# produced ZERO inline findings. Check this before ConvertFrom-Json so malformed
# non-empty JSON still surfaces as a parse error instead of being treated as no
# findings.
if ([string]::IsNullOrWhiteSpace($rawJson)) {
    Write-Host "Findings file is empty — no inline findings to post." -ForegroundColor Green
    exit 0
}

try {
    $parsed = $rawJson | ConvertFrom-Json -ErrorAction Stop
} catch {
    throw "Findings file '$FindingsFile' contains malformed JSON: $($_.Exception.Message)"
}

# Guard: a literal-"null" findings file parses to $null. The expert review writes
# the inline-findings.post.ok sentinel whenever the PR fix won, even when it
# produced ZERO inline findings — so this block can legitimately run against a
# null findings file. Calling .GetType() or .PSObject on $null throws "You cannot
# call a method on a null-valued expression" (surfaced as a scary non-fatal error
# in the deferred-post catch). Treat it as "no findings" and exit cleanly.
if ($null -eq $parsed) {
    Write-Host "Findings file parsed to null ('null') — no inline findings to post." -ForegroundColor Green
    exit 0
}

# Diagnostic: log what the parser sees
Write-Host "  Parsed type: $($parsed.GetType().FullName)" -ForegroundColor Gray
if ($parsed -is [System.Management.Automation.PSCustomObject]) {
    $propertyNames = ($parsed.PSObject.Properties | ForEach-Object { $_.Name }) -join ', '
    Write-Host "  Object properties: $(ConvertTo-AzdoSafeConsole $propertyNames)" -ForegroundColor Gray
}

# The agent may produce:
#   1. A bare array [...] of findings
#   2. An object wrapper {"findings": [...]} or {"schemaVersion":1, "findings":[...]}
#   3. An object wrapper {"items": [...]}
#   4. A single finding object {...}
# Detect and unwrap all forms robustly.
$findings = @()
if ($parsed -is [System.Collections.IEnumerable] -and $parsed -isnot [string]) {
    # Already an array
    $findings = @($parsed)
} elseif ($parsed.PSObject.Properties.Match('findings').Count -gt 0 -and $null -ne $parsed.findings) {
    # Object wrapper with explicit 'findings' property
    $findings = @($parsed.findings)
} elseif ($parsed.PSObject.Properties.Match('items').Count -gt 0 -and $null -ne $parsed.items) {
    # Alternative wrapper with 'items' property
    $findings = @($parsed.items)
} elseif ($parsed.PSObject.Properties.Match('file').Count -gt 0 -or $parsed.PSObject.Properties.Match('path').Count -gt 0) {
    # Single finding object — wrap in array
    $findings = @($parsed)
} else {
    Write-Host "  ⚠️ Unrecognized findings format — dumping first 200 chars:" -ForegroundColor Yellow
    $rawPreview = $rawJson.Substring(0, [Math]::Min(200, $rawJson.Length))
    Write-Host "  $(ConvertTo-AzdoSafeConsole $rawPreview)" -ForegroundColor Gray
}

if (-not $findings -or $findings.Count -eq 0) {
    Write-Host "No findings to post." -ForegroundColor Green
    exit 0
}

Write-Host "  Found $($findings.Count) inline findings" -ForegroundColor Gray
$findingKeys = ($findings[0].PSObject.Properties | ForEach-Object { $_.Name }) -join ', '
Write-Host "  First finding keys: $(ConvertTo-AzdoSafeConsole $findingKeys)" -ForegroundColor Gray

# Load summary if available
$summaryBody = ""
if (Test-Path $SummaryFile) {
    $summaryBody = Get-Content -Path $SummaryFile -Raw -Encoding UTF8
    Write-Host "  Loaded summary ($($summaryBody.Length) chars)" -ForegroundColor Gray
} else {
    $summaryBody = "## Expert Review — $($findings.Count) findings`n`nSee inline comments for details."
}

# ============================================================================
# GET PR HEAD COMMIT (required by GitHub API)
# ============================================================================

Write-Host "Fetching PR #$PRNumber head commit..." -ForegroundColor Cyan
$prJson = gh api "repos/$Repository/pulls/$PRNumber" --jq '{sha: .head.sha}' 2>&1
if ($LASTEXITCODE -ne 0) {
    if (-not [string]::IsNullOrWhiteSpace($ReviewedCommit)) {
        Write-Host "Could not verify the current PR head; skipping snapshot-bound inline findings." -ForegroundColor Yellow
        exit 0
    }
    throw "Failed to fetch PR #${PRNumber}: $prJson"
}
$prData = $prJson | ConvertFrom-Json
$currentHeadSha = [string]$prData.sha
$commitSha = if ([string]::IsNullOrWhiteSpace($ReviewedCommit)) { $currentHeadSha } else { $ReviewedCommit }
if (-not [string]::IsNullOrWhiteSpace($currentHeadSha) -and
    -not $currentHeadSha.Equals($commitSha, [StringComparison]::OrdinalIgnoreCase)) {
    Write-Host "PR advanced after the review snapshot; skipping stale inline findings." -ForegroundColor Yellow
    exit 0
}
Write-Host "  Reviewed commit: $commitSha" -ForegroundColor Gray

# ============================================================================
# BUILD REVIEW PAYLOAD
# ============================================================================

$comments = @()
foreach ($f in $findings) {
    # Defense-in-depth: reject suspicious paths so a malformed/hostile finding
    # cannot poison the whole review post (especially in the fallback branch
    # below where the GitHub diff fetch failed and we can't cross-validate).
    $p = if ($f.path) { [string]$f.path } elseif ($f.file) { [string]$f.file } else { '' }
    if ([string]::IsNullOrWhiteSpace($p) -or
        $p.Contains('..') -or
        $p.StartsWith('/') -or
        $p.StartsWith('\') -or
        $p.Contains('\') -or
        $p -match '[\x00-\x1F]' -or
        $p -match '^[A-Za-z]:') {
        Write-Host "  ⚠️ Skipping finding with suspicious path: '$(ConvertTo-AzdoSafeConsole $p)'" -ForegroundColor Yellow
        continue
    }

    $rawBody = if ($f.body) { [string]$f.body } elseif ($f.message) { [string]$f.message } elseif ($f.content) { [string]$f.content } else { "(no description)" }
    $aiMarker = "> 🔍 **AI-Generated Review** (multi-model)`n`n"
    
    $comment = @{
        path = $p
        line = [int]$f.line
        body = $aiMarker + $rawBody
    }
    # GitHub API requires 'side' for pull request review comments
    $comment['side'] = 'RIGHT'
    $comments += $comment
}

# ============================================================================
# FILTER COMMENTS TO LINES PRESENT IN THE PR DIFF
# GitHub returns 422 "Line could not be resolved" if ANY comment targets a
# line outside the diff. Pre-validate to avoid losing the entire review.
# ============================================================================

Write-Host "Fetching PR diff for line validation..." -ForegroundColor Cyan
$filesJson = gh api --paginate "repos/$Repository/pulls/$PRNumber/files" 2>&1
if ($LASTEXITCODE -ne 0) {
    $filesError = ($filesJson | Out-String).Trim()
    Write-Host "  ⚠️ Could not fetch PR files for validation: $(ConvertTo-AzdoSafeConsole $filesError)" -ForegroundColor Yellow
    Write-Host "  Posting all findings without pre-validation." -ForegroundColor Yellow
} else {
    $files = $filesJson | ConvertFrom-Json
    # Build map: path -> set of new-file line numbers commentable on RIGHT side
    $diffLines = @{}
    foreach ($file in $files) {
        $path = $file.filename
        $patch = $file.patch
        if (-not $patch) { continue }
        $lineSet = New-Object System.Collections.Generic.HashSet[int]
        $newLine = 0
        foreach ($pl in ($patch -split "`n")) {
            if ($pl -match '^@@ -\d+(?:,\d+)? \+(\d+)(?:,\d+)? @@') {
                $newLine = [int]$Matches[1]
                continue
            }
            if ($pl.StartsWith('+') -and -not $pl.StartsWith('+++')) {
                [void]$lineSet.Add($newLine)
                $newLine++
            } elseif ($pl.StartsWith('-') -and -not $pl.StartsWith('---')) {
                # deletion — does not advance new-file line
            } elseif ($pl.StartsWith(' ')) {
                # context — also commentable (RIGHT side)
                [void]$lineSet.Add($newLine)
                $newLine++
            }
        }
        $diffLines[$path] = $lineSet
    }

    $kept = @()
    $dropped = @()
    foreach ($c in $comments) {
        if ($diffLines.ContainsKey($c.path) -and $diffLines[$c.path].Contains([int]$c.line)) {
            $kept += $c
        } else {
            $dropped += $c
        }
    }
    if ($dropped.Count -gt 0) {
        Write-Host "  ⚠️ Dropping $($dropped.Count) finding(s) whose lines aren't in the PR diff:" -ForegroundColor Yellow
        foreach ($d in $dropped) {
            Write-Host "      $(ConvertTo-AzdoSafeConsole ([string]$d.path)):$($d.line)" -ForegroundColor Gray
        }
    }
    Write-Host "  ✅ $($kept.Count) of $($comments.Count) findings target lines in the diff" -ForegroundColor Gray
    $comments = $kept
}

if ($comments.Count -eq 0) {
    Write-Host "No inline-commentable findings remain after diff validation. Skipping review post." -ForegroundColor Yellow
    exit 0
}

$reviewMarker = New-InlineReviewMarker -ReviewedHead $commitSha -Comments $comments
$reviewPayload = @{
    commit_id = $commitSha
    body      = "$reviewMarker`n$summaryBody"
    event     = "COMMENT"  # Never APPROVE or REQUEST_CHANGES — that's a human decision
    comments  = $comments
}

$payloadJson = $reviewPayload | ConvertTo-Json -Depth 10

# ============================================================================
# DRY RUN
# ============================================================================

if ($DryRun) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  DRY RUN — Review preview (not posted)" -ForegroundColor Yellow
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Summary:" -ForegroundColor Cyan
    Write-Host $summaryBody
    Write-Host ""
    Write-Host "Inline comments ($($comments.Count)):" -ForegroundColor Cyan
    foreach ($c in $comments) {
        Write-Host "  $($c.path):$($c.line)" -ForegroundColor White -NoNewline
        Write-Host " — $($c.body.Substring(0, [Math]::Min($c.body.Length, 120)))..." -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    Write-Host "  Payload size: $($payloadJson.Length) chars" -ForegroundColor Gray
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Yellow
    return
}

# ============================================================================
# POST REVIEW
# ============================================================================

Write-Host "Posting review with $($comments.Count) inline comments..." -ForegroundColor Cyan

$viewerLogin = [string](gh api user --jq '.login' 2>$null)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($viewerLogin)) {
    throw "Could not resolve the authenticated GitHub identity before checking inline-review idempotency."
}

$existingReviewsJson = gh api --paginate --slurp "repos/$Repository/pulls/$PRNumber/reviews?per_page=100" 2>&1
if ($LASTEXITCODE -ne 0) {
    $reviewsError = ($existingReviewsJson | Out-String).Trim()
    throw "Could not query existing inline reviews before posting: $(ConvertTo-AzdoSafeConsole $reviewsError)"
}

try {
    $reviewPages = $existingReviewsJson | ConvertFrom-Json -ErrorAction Stop
} catch {
    throw "Could not parse existing inline reviews before posting: $($_.Exception.Message)"
}

$reviewBodies = @()
foreach ($page in @($reviewPages)) {
    foreach ($review in @($page)) {
        $reviewAuthor = if ($review.user -and $review.user.login) { [string]$review.user.login } else { '' }
        if ($reviewAuthor.Equals($viewerLogin, [StringComparison]::OrdinalIgnoreCase) -and
            $review.PSObject.Properties.Match('body').Count -gt 0) {
            $reviewBodies += [string]$review.body
        }
    }
}
if (Test-InlineReviewMarkerExists -ReviewBodies $reviewBodies -Marker $reviewMarker) {
    Write-Host "Inline review already posted for reviewed head and findings hash; skipping duplicate." -ForegroundColor Green
    return
}

$tempFile = [System.IO.Path]::GetTempFileName()
try {
    $payloadJson | Set-Content -Path $tempFile -Encoding UTF8

    $result = gh api --method POST "repos/$Repository/pulls/$PRNumber/reviews" --input $tempFile 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to post review: $result"
    }

    $reviewData = $result | ConvertFrom-Json
    Write-Host "Review posted (ID: $($reviewData.id))" -ForegroundColor Green
    Write-Host "  $($comments.Count) inline comments at file:line" -ForegroundColor Gray
    Write-Host "  URL: $($reviewData.html_url)" -ForegroundColor Gray
} finally {
    Remove-Item -Path $tempFile -Force -ErrorAction SilentlyContinue
}
