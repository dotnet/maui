#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Posts or updates the PR agent review comment on a GitHub Pull Request with validation.

.DESCRIPTION
    Creates ONE comment for the entire PR review with all phases wrapped in an expandable section.
    Uses HTML marker <!-- AI Summary --> for identification.
    
    **NEW: Validates that phases marked as COMPLETE actually have content.**
    **NEW: Auto-loads state file from CustomAgentLogsTmp/PRState/pr-XXXXX.md**
    
    Format:
    ## 🤖 AI Summary — ✅ APPROVE
    <details><summary>📊 Expand Full Review</summary>
      Status table + all 4 phases as nested details
    </details>

.PARAMETER PRNumber
    The pull request number (required unless -StateFile is provided with pr-XXXXX.md naming)

.PARAMETER StateFile
    Path to state file (defaults to CustomAgentLogsTmp/PRState/pr-{PRNumber}.md)
    If provided with pr-XXXXX.md naming, PRNumber is auto-extracted

.PARAMETER Content
    The full state file content (alternative to -StateFile)

.PARAMETER DryRun
    Print comment instead of posting

.PARAMETER SkipValidation
    Skip validation checks (not recommended)

.EXAMPLE
    # Simplest: just provide PR number, state file auto-loaded
    ./post-ai-summary-comment.ps1 -PRNumber 12345

.EXAMPLE
    # Provide state file directly (PR number auto-extracted from filename)
    ./post-ai-summary-comment.ps1 -StateFile CustomAgentLogsTmp/PRState/pr-27246.md

.EXAMPLE
    # Legacy: provide content directly
    ./post-ai-summary-comment.ps1 -PRNumber 12345 -Content "$(cat CustomAgentLogsTmp/PRState/pr-12345.md)"
#>

param(
    [Parameter(Mandatory=$false)]
    [int]$PRNumber,

    [Parameter(Mandatory=$false)]
    [string]$StateFile,

    [Parameter(Mandatory=$false)]
    [string]$Content,

    [Parameter(Mandatory=$false)]
    [switch]$DryRun,

    [Parameter(Mandatory=$false)]
    [switch]$SkipValidation,

    [Parameter(Mandatory=$false)]
    [string]$PreviewFile
)

$ErrorActionPreference = "Stop"

# ============================================================================
# STATE FILE RESOLUTION
# ============================================================================

# Priority: 1) -Content, 2) -StateFile, 3) Auto-detect from PRNumber

# If StateFile provided, extract PRNumber from filename if not already set
if (-not [string]::IsNullOrWhiteSpace($StateFile)) {
    if ($StateFile -match 'pr-(\d+)\.md$') {
        $extractedPR = [int]$Matches[1]
        if ($PRNumber -eq 0) {
            $PRNumber = $extractedPR
            Write-Host "ℹ️  Auto-detected PRNumber: $PRNumber from state file name" -ForegroundColor Cyan
        } elseif ($PRNumber -ne $extractedPR) {
            Write-Host "⚠️  Warning: PRNumber ($PRNumber) differs from state file name (pr-$extractedPR.md)" -ForegroundColor Yellow
        }
    }
    
    if (Test-Path $StateFile) {
        $Content = Get-Content $StateFile -Raw -Encoding UTF8
        Write-Host "ℹ️  Loaded state file: $StateFile" -ForegroundColor Cyan
    } else {
        throw "State file not found: $StateFile"
    }
}

# If no Content and no StateFile, try auto-detect from PRNumber
if ([string]::IsNullOrWhiteSpace($Content) -and $PRNumber -gt 0) {
    $autoStateFile = "CustomAgentLogsTmp/PRState/pr-$PRNumber.md"
    if (Test-Path $autoStateFile) {
        $Content = Get-Content $autoStateFile -Raw -Encoding UTF8
        Write-Host "ℹ️  Auto-loaded state file: $autoStateFile" -ForegroundColor Cyan
    } else {
        # Try relative to repo root
        $repoRoot = git rev-parse --show-toplevel 2>$null
        if ($repoRoot) {
            $autoStateFile = Join-Path $repoRoot "CustomAgentLogsTmp/PRState/pr-$PRNumber.md"
            if (Test-Path $autoStateFile) {
                $Content = Get-Content $autoStateFile -Raw -Encoding UTF8
                Write-Host "ℹ️  Auto-loaded state file: $autoStateFile" -ForegroundColor Cyan
            }
        }
    }
}

# If Content still not provided, try stdin (legacy support)
if ([string]::IsNullOrWhiteSpace($Content)) {
    $Content = $input | Out-String
}

# Final validation
if ([string]::IsNullOrWhiteSpace($Content)) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  ⛔ No state file content found                           ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "Usage options:" -ForegroundColor Yellow
    Write-Host "  1. ./post-ai-summary-comment.ps1 -PRNumber 12345" -ForegroundColor Gray
    Write-Host "     (auto-loads CustomAgentLogsTmp/PRState/pr-12345.md)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  2. ./post-ai-summary-comment.ps1 -StateFile path/to/pr-12345.md" -ForegroundColor Gray
    Write-Host "     (loads specified file, extracts PRNumber from name)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  3. ./post-ai-summary-comment.ps1 -PRNumber 12345 -Content `"...`"" -ForegroundColor Gray
    Write-Host "     (legacy: provide content directly)" -ForegroundColor Gray
    Write-Host ""
    throw "Content is required. See usage options above."
}

if ($PRNumber -eq 0) {
    throw "PRNumber is required. Provide via -PRNumber or use a state file named pr-XXXXX.md"
}

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  AI Summary Comment (with Validation)                    ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# ============================================================================
# VALIDATION FUNCTIONS
# ============================================================================

function Test-PhaseContentComplete {
    param(
        [string]$PhaseContent,
        [string]$PhaseName,
        [string]$PhaseStatus,
        [switch]$Debug
    )
    
    # Skip validation if phase is not marked COMPLETE or PASSED
    if ($PhaseStatus -notmatch '✅\s*(COMPLETE|PASSED)') {
        return @{ IsValid = $true; Errors = @(); Warnings = @() }
    }
    
    $validationErrors = @()
    $validationWarnings = @()
    
    # Check if content exists
    if ([string]::IsNullOrWhiteSpace($PhaseContent)) {
        $validationErrors += "Phase $PhaseName is marked as '$PhaseStatus' but has NO content in state file"
        if ($Debug) {
            Write-Host "  [DEBUG] Content is null or whitespace for phase: $PhaseName" -ForegroundColor DarkGray
        }
        return @{ IsValid = $false; Errors = $validationErrors; Warnings = @() }
    }
    
    if ($Debug) {
        Write-Host "  [DEBUG] $PhaseName content length: $($PhaseContent.Length) chars" -ForegroundColor DarkGray
        Write-Host "  [DEBUG] First 100 chars: $($PhaseContent.Substring(0, [Math]::Min(100, $PhaseContent.Length)))" -ForegroundColor DarkGray
    }
    
    # Check for PENDING markers
    $pendingMatches = [regex]::Matches($PhaseContent, '\[PENDING\]|⏳\s*PENDING')
    if ($pendingMatches.Count -gt 0) {
        $validationErrors += "Phase $PhaseName is marked as '$PhaseStatus' but contains $($pendingMatches.Count) PENDING markers"
    }
    
    # Phase-specific validation (relaxed for better UX)
    switch ($PhaseName) {
        "Pre-Flight" {
            if ($PhaseContent -notmatch 'Platforms Affected:') {
                $validationWarnings += "Pre-Flight missing 'Platforms Affected' section (non-critical)"
            }
        }
        "Gate" {
            if ($PhaseContent -notmatch 'Result:') {
                $validationWarnings += "Gate phase missing 'Result' field (non-critical)"
            }
        }
        "Fix" {
            if ($PhaseContent -notmatch 'Selected Fix:') {
                $validationErrors += "Fix phase missing 'Selected Fix' field"
            }
            if ($PhaseContent -notmatch 'Exhausted:') {
                $validationWarnings += "Fix phase missing 'Exhausted' field (non-critical)"
            }
        }
        "Report" {
            # Relaxed validation - only check for substantive content
            $hasRecommendation = $PhaseContent -match '(Final Recommendation|Verdict|Recommendation:|APPROVE|REQUEST CHANGES)'
            $hasAnalysis = $PhaseContent -match '(Summary|Fix Quality|Test Quality|Why|Analysis)'
            
            if (-not $hasRecommendation) {
                $validationWarnings += "Report phase missing clear recommendation (non-critical)"
            }
            if (-not $hasAnalysis) {
                $validationWarnings += "Report phase missing analysis sections (non-critical)"
            }
            
            # Only error if content is extremely short
            if ($PhaseContent.Length -lt 200) {
                $validationErrors += "Report phase content is too short ($($PhaseContent.Length) chars) - expected comprehensive final report"
            }
        }
    }
    
    return @{
        IsValid = ($validationErrors.Count -eq 0)
        Errors = $validationErrors
        Warnings = $validationWarnings
    }
}

# ============================================================================
# EXTRACTION FUNCTIONS
# ============================================================================

# Extract recommendation from state file
$recommendation = "IN PROGRESS"
if ($Content -match '##\s+✅\s+Final Recommendation:\s+APPROVE') {
    $recommendation = "✅ APPROVE"
} elseif ($Content -match '##\s+⚠️\s+Final Recommendation:\s+REQUEST CHANGES') {
    $recommendation = "⚠️ REQUEST CHANGES"
} elseif ($Content -match 'Final Recommendation:\s+APPROVE') {
    $recommendation = "✅ APPROVE"
} elseif ($Content -match 'Final Recommendation:\s+REQUEST CHANGES') {
    $recommendation = "⚠️ REQUEST CHANGES"
}

# Extract phase statuses from state file
$phaseStatuses = @{
    "Pre-Flight" = "⏳ PENDING"
    "Gate" = "⏳ PENDING"
    "Fix" = "⏳ PENDING"
    "Report" = "⏳ PENDING"
}

# Parse phase status table - match any status format
if ($Content -match '(?s)\|\s*Phase\s*\|\s*Status\s*\|.*?\n\|[\s-]+\|[\s-]+\|(.*?)(?=\n\n|---|\z)') {
    $tableContent = $Matches[1]
    $tableContent -split '\n' | ForEach-Object {
        if ($_ -match '\|\s*(.+?)\s*\|\s*(.+?)\s*\|') {
            $phaseName = $Matches[1].Trim() -replace '^🔍\s*', '' -replace '^🧪\s*', '' -replace '^🚦\s*', '' -replace '^🔧\s*', '' -replace '^📋\s*', ''
            $status = $Matches[2].Trim()
            if ($phaseStatuses.ContainsKey($phaseName)) {
                $phaseStatuses[$phaseName] = $status
            }
        }
    }
}

# ============================================================================
# DYNAMIC SECTION EXTRACTION
# ============================================================================

# Extract ALL sections from state file dynamically
function Extract-AllSections {
    param(
        [string]$StateContent,
        [switch]$Debug
    )
    
    $sections = @{}
    
    # Pattern to find all <details><summary><strong>TITLE</strong></summary>...content...</details> blocks
    # Note: [^>]* handles optional attributes like "open" in <details open>
    $pattern = '(?s)<details[^>]*>\s*<summary><strong>([^<]+)</strong></summary>(.*?)</details>'
    $matches = [regex]::Matches($StateContent, $pattern)
    
    if ($Debug) {
        Write-Host "  [DEBUG] Found $($matches.Count) section(s) in state file" -ForegroundColor Cyan
    }
    
    foreach ($match in $matches) {
        $title = $match.Groups[1].Value.Trim()
        $content = $match.Groups[2].Value.Trim()
        
        $sections[$title] = $content
        
        if ($Debug) {
            Write-Host "  [DEBUG] Section: '$title' (${content.Length} chars)" -ForegroundColor DarkGray
        }
    }
    
    return $sections
}

# Extract all sections dynamically
$debugMode = $false  # Set to $true for debugging
if ($DebugPreference -eq 'Continue') { $debugMode = $true }

$allSections = Extract-AllSections -StateContent $Content -Debug:$debugMode

# Map sections to phase content using flexible matching
function Get-SectionByPattern {
    param(
        [hashtable]$Sections,
        [string[]]$Patterns,
        [switch]$Debug
    )
    
    foreach ($pattern in $Patterns) {
        foreach ($key in $Sections.Keys) {
            if ($key -match $pattern) {
                if ($Debug) {
                    Write-Host "  [DEBUG] Matched '$key' with pattern '$pattern'" -ForegroundColor Green
                }
                return $Sections[$key]
            }
        }
    }
    
    if ($Debug) {
        Write-Host "  [DEBUG] No match for patterns: $($Patterns -join ', ')" -ForegroundColor Yellow
        Write-Host "  [DEBUG] Available sections: $($Sections.Keys -join ', ')" -ForegroundColor Yellow
    }
    
    return $null
}

# Map to phase content with flexible patterns (regex)
$preFlightContent = Get-SectionByPattern -Sections $allSections -Patterns @(
    '📋.*Issue Summary',
    '📋.*Pre-Flight',
    '🔍.*Pre-Flight'
) -Debug:$debugMode

$gateContent = Get-SectionByPattern -Sections $allSections -Patterns @(
    '🚦.*Gate',
    '📋.*Gate'
) -Debug:$debugMode

$fixContent = Get-SectionByPattern -Sections $allSections -Patterns @(
    '🔧.*Fix',
    '📋.*Fix'
) -Debug:$debugMode

$reportContent = Get-SectionByPattern -Sections $allSections -Patterns @(
    '📋.*Report',
    'Phase 4.*Report',
    'Final Report'
) -Debug:$debugMode

# Fallback: If Report content not found in <details> blocks, look for
# "## Final Recommendation" section directly in the markdown (agent sometimes
# writes Report as a top-level heading instead of a <details> block)
if ([string]::IsNullOrWhiteSpace($reportContent)) {
    if ($Content -match '(?s)##\s+[✅⚠️❌]*\s*Final Recommendation[:\s].+') {
        $reportContent = $Matches[0].Trim()
        if ($debugMode) {
            Write-Host "  [DEBUG] Report extracted from '## Final Recommendation' heading ($($reportContent.Length) chars)" -ForegroundColor Green
        }
    }
}

# ============================================================================
# VALIDATION
# ============================================================================

if (-not $SkipValidation) {
    Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
    Write-Host "║  Phase Content Validation                                 ║" -ForegroundColor Yellow
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
    
    $allValidationErrors = @()
    $allValidationWarnings = @()
    
    # Validate each phase
    $phases = @(
        @{ Name = "Pre-Flight"; Content = $preFlightContent; Status = $phaseStatuses["Pre-Flight"] },
        @{ Name = "Tests"; Content = $testsContent; Status = $phaseStatuses["Tests"] },
        @{ Name = "Gate"; Content = $gateContent; Status = $phaseStatuses["Gate"] },
        @{ Name = "Fix"; Content = $fixContent; Status = $phaseStatuses["Fix"] },
        @{ Name = "Report"; Content = $reportContent; Status = $phaseStatuses["Report"] }
    )
    
    foreach ($phase in $phases) {
        $result = Test-PhaseContentComplete -PhaseContent $phase.Content -PhaseName $phase.Name -PhaseStatus $phase.Status -Debug:$debugMode
        
        if ($result.IsValid) {
            Write-Host "  ✅ $($phase.Name): Valid" -ForegroundColor Green
        } else {
            Write-Host "  ❌ $($phase.Name): INVALID" -ForegroundColor Red
            foreach ($err in $result.Errors) {
                Write-Host "     - $err" -ForegroundColor Red
                $allValidationErrors += "$($phase.Name): $err"
            }
        }
        
        # Show warnings
        if ($result.Warnings -and $result.Warnings.Count -gt 0) {
            foreach ($warning in $result.Warnings) {
                Write-Host "     ⚠️  $warning" -ForegroundColor Yellow
                $allValidationWarnings += "$($phase.Name): $warning"
            }
        }
    }
    
    # Show warnings summary if any
    if ($allValidationWarnings.Count -gt 0) {
        Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
        Write-Host "║  ⚠️  VALIDATION WARNINGS                                  ║" -ForegroundColor Yellow
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "Found $($allValidationWarnings.Count) warning(s) (non-critical):" -ForegroundColor Yellow
        foreach ($warning in $allValidationWarnings) {
            Write-Host "  - $warning" -ForegroundColor Yellow
        }
        Write-Host ""
        Write-Host "💡 These are suggestions for improvement but won't block posting." -ForegroundColor Cyan
    }
    
    # Only fail on errors
    if ($allValidationErrors.Count -gt 0) {
        Write-Host "`n╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
        Write-Host "║  ⛔ VALIDATION FAILED                                     ║" -ForegroundColor Red
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
        Write-Host ""
        Write-Host "Found $($allValidationErrors.Count) validation error(s):" -ForegroundColor Red
        foreach ($err in $allValidationErrors) {
            Write-Host "  - $err" -ForegroundColor Red
        }
        Write-Host ""
        Write-Host "💡 Fix these issues in the state file before posting the review comment." -ForegroundColor Cyan
        Write-Host "   Or use -SkipValidation to bypass these checks (not recommended)." -ForegroundColor Cyan
        Write-Host ""
        Write-Host "🐛 Debug tip: Run with `$DebugPreference = 'Continue' for detailed extraction info" -ForegroundColor DarkGray
        exit 1
    }
    
    if ($allValidationWarnings.Count -eq 0) {
        Write-Host ""
        Write-Host "✅ All validation checks passed!" -ForegroundColor Green
    }
}

# ============================================================================
# BUILD COMMENT (Rest of the original script logic)
# ============================================================================

# Get latest commit for NEW Review Session header
Write-Host "`nFetching latest commit info..." -ForegroundColor Yellow
$commitJson = gh api "repos/dotnet/maui/pulls/$PRNumber/commits" --jq '.[-1] | {message: .commit.message, sha: .sha}' | ConvertFrom-Json
$latestCommitTitle = ($commitJson.message -split "`n")[0]
$latestCommitSha = $commitJson.sha.Substring(0, 7)
$latestCommitUrl = "https://github.com/dotnet/maui/commit/$($commitJson.sha)"

# Helper function to create a NEW review session
function New-ReviewSession {
    param([string]$PhaseContent, [string]$CommitTitle, [string]$CommitSha, [string]$CommitUrl)
    
    if ([string]::IsNullOrWhiteSpace($PhaseContent)) {
        return ""
    }
    
    return @"
<details>
<summary>📝 <strong>Review Session</strong> — <strong>$CommitTitle</strong> · <a href="$CommitUrl"><code>$CommitSha</code></a></summary>

---

$PhaseContent

</details>
"@
}

# Helper function to extract existing review sessions from a phase
function Get-ExistingReviewSessions {
    param([string]$PhaseContent)
    
    if ([string]::IsNullOrWhiteSpace($PhaseContent)) {
        return @()
    }
    
    $sessions = @()
    $pattern = '(?s)<details>\s*<summary>📝.*?</summary>.*?</details>'
    $matches = [regex]::Matches($PhaseContent, $pattern)
    
    foreach ($match in $matches) {
        $sessions += $match.Value
    }
    
    return $sessions
}

# Helper function to combine existing sessions with new session
function Merge-ReviewSessions {
    param(
        [string[]]$ExistingSessions,
        [string]$NewSession,
        [string]$NewCommitSha
    )
    
    if ([string]::IsNullOrWhiteSpace($NewSession)) {
        return ""
    }
    
    # Check if any existing session is for the same commit
    $allSessions = @()
    $replaced = $false
    
    foreach ($existingSession in $ExistingSessions) {
        # Check if this session contains the new commit SHA
        if ($existingSession -match "<code>$NewCommitSha</code>") {
            # Replace this session with the new one (only once)
            if (-not $replaced) {
                $allSessions += $NewSession
                $replaced = $true
            }
            # Skip the old session with same commit SHA
        } else {
            # Keep the existing session
            $allSessions += $existingSession
        }
    }
    
    # If we didn't replace any session, add the new one
    if (-not $replaced) {
        $allSessions += $NewSession
    }
    
    return ($allSessions -join "`n`n---`n`n")
}

# Fetch existing comment to preserve old review sessions
Write-Host "Checking for existing review comment..." -ForegroundColor Yellow
$existingComment = gh api "repos/dotnet/maui/issues/$PRNumber/comments" --jq '.[] | select(.body | contains("<!-- AI Summary -->")) | {id: .id, body: .body}' | ConvertFrom-Json

$existingPreFlightSessions = @()
$existingTestsSessions = @()
$existingGateSessions = @()
$existingFixSessions = @()
$existingReportSessions = @()

if ($existingComment) {
    Write-Host "✓ Found existing review comment (ID: $($existingComment.id)) - extracting review sessions..." -ForegroundColor Green
    
    # Helper function to extract phase content with fallback patterns
    function Extract-PhaseFromComment {
        param(
            [string]$CommentBody,
            [string]$Emoji,
            [string]$PhaseName
        )
        
        # Try patterns in order of specificity (most specific first)
        $patterns = @(
            # Pattern 1: Phase name anywhere in the header
            "(?s)<summary><strong>.*?$PhaseName.*?</strong></summary>(.*?)</details>"
            # Pattern 2: Just emoji (most lenient fallback)
            "(?s)<summary><strong>$Emoji[^<]*</strong></summary>(.*?)</details>"
        )
        
        foreach ($pattern in $patterns) {
            if ($CommentBody -match $pattern) {
                return $Matches[1]
            }
        }
        
        return $null
    }
    
    # Extract existing sessions from each phase with fallback
    $preFlightMatch = Extract-PhaseFromComment -CommentBody $existingComment.body -Emoji "🔍" -PhaseName "Pre-Flight"
    if ($preFlightMatch) { $existingPreFlightSessions = Get-ExistingReviewSessions -PhaseContent $preFlightMatch }
    
    $gateMatch = Extract-PhaseFromComment -CommentBody $existingComment.body -Emoji "🚦" -PhaseName "Gate"
    if ($gateMatch) { $existingGateSessions = Get-ExistingReviewSessions -PhaseContent $gateMatch }
    
    $fixMatch = Extract-PhaseFromComment -CommentBody $existingComment.body -Emoji "🔧" -PhaseName "Fix"
    if ($fixMatch) { $existingFixSessions = Get-ExistingReviewSessions -PhaseContent $fixMatch }
    
    $reportMatch = Extract-PhaseFromComment -CommentBody $existingComment.body -Emoji "📋" -PhaseName "Report"
    if ($reportMatch) { $existingReportSessions = Get-ExistingReviewSessions -PhaseContent $reportMatch }
} else {
    Write-Host "✓ No existing comment found - creating new..." -ForegroundColor Yellow
}

# Create NEW review sessions from current state file
$newPreFlightSession = New-ReviewSession -PhaseContent $preFlightContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl
$newGateSession = New-ReviewSession -PhaseContent $gateContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl
$newFixSession = New-ReviewSession -PhaseContent $fixContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl
$newReportSession = New-ReviewSession -PhaseContent $reportContent -CommitTitle $latestCommitTitle -CommitSha $latestCommitSha -CommitUrl $latestCommitUrl

# Merge existing sessions with new session (if new content exists)
$allPreFlightSessions = if ($newPreFlightSession) { Merge-ReviewSessions -ExistingSessions $existingPreFlightSessions -NewSession $newPreFlightSession -NewCommitSha $latestCommitSha } else { $existingPreFlightSessions -join "`n`n---`n`n" }
$allGateSessions = if ($newGateSession) { Merge-ReviewSessions -ExistingSessions $existingGateSessions -NewSession $newGateSession -NewCommitSha $latestCommitSha } else { $existingGateSessions -join "`n`n---`n`n" }
$allFixSessions = if ($newFixSession) { Merge-ReviewSessions -ExistingSessions $existingFixSessions -NewSession $newFixSession -NewCommitSha $latestCommitSha } else { $existingFixSessions -join "`n`n---`n`n" }
$allReportSessions = if ($newReportSession) { Merge-ReviewSessions -ExistingSessions $existingReportSessions -NewSession $newReportSession -NewCommitSha $latestCommitSha } else { $existingReportSessions -join "`n`n---`n`n" }

# Build phase sections dynamically - only include phases with content
$phaseSections = @()

# Helper to create phase section
function New-PhaseSection {
    param(
        [string]$Icon,
        [string]$PhaseName,
        [string]$Subtitle,
        [string]$Content,
        [string]$Status
    )
    
    # Skip phases with no content
    if ([string]::IsNullOrWhiteSpace($Content) -or $Content -eq "_No review sessions yet_") {
        return $null
    }
    
    return @"
<details>
<summary><strong>$Icon $PhaseName — $Subtitle</strong></summary>

---

$Content

</details>
"@
}

# Build phase sections (only non-empty ones)
$preFlightSection = New-PhaseSection -Icon "🔍" -PhaseName "Pre-Flight" -Subtitle "Context & Validation" -Content $allPreFlightSessions -Status $phaseStatuses['Pre-Flight']
$gateSection = New-PhaseSection -Icon "🚦" -PhaseName "Gate" -Subtitle "Test Verification" -Content $allGateSessions -Status $phaseStatuses['Gate']
$fixSection = New-PhaseSection -Icon "🔧" -PhaseName "Fix" -Subtitle "Analysis & Comparison" -Content $allFixSessions -Status $phaseStatuses['Fix']
$reportSection = New-PhaseSection -Icon "📋" -PhaseName "Report" -Subtitle "Final Recommendation" -Content $allReportSessions -Status $phaseStatuses['Report']

# Collect non-null sections
if ($preFlightSection) { $phaseSections += $preFlightSection }
if ($gateSection) { $phaseSections += $gateSection }
if ($fixSection) { $phaseSections += $fixSection }
if ($reportSection) { $phaseSections += $reportSection }

# Join sections with separators
$phaseContent = if ($phaseSections.Count -gt 0) {
    $phaseSections -join "`n`n---`n`n"
} else {
    "_No phases completed yet_"
}

# ============================================================================
# UNIFIED COMMENT HANDLING
# Uses single <!-- AI Summary --> comment with section markers
# ============================================================================

$MAIN_MARKER = "<!-- AI Summary -->"
$SECTION_START = "<!-- SECTION:PR-REVIEW -->"
$SECTION_END = "<!-- /SECTION:PR-REVIEW -->"

# Build the PR review section with markers
$prReviewSection = @"
$SECTION_START
<details>
<summary>📊 <strong>Expand Full Review</strong></summary>

---

$phaseContent

---

</details>
$SECTION_END
"@

# Check if there are other sections in the existing comment that we need to preserve
$existingOtherSections = ""
if ($existingComment) {
    $body = $existingComment.body
    
    # Extract all non-PR-REVIEW sections
    $sectionTypes = @("TRY-FIX", "WRITE-TESTS", "PR-FINALIZE")
    foreach ($sectionType in $sectionTypes) {
        $sStart = [regex]::Escape("<!-- SECTION:$sectionType -->")
        $sEnd = [regex]::Escape("<!-- /SECTION:$sectionType -->")
        if ($body -match "(?s)($sStart.*?$sEnd)") {
            $existingOtherSections += "`n`n" + $Matches[1]
        }
    }
}

# Build aggregated comment body
$commentBody = @"
$MAIN_MARKER

## 🤖 AI Summary

$prReviewSection
$existingOtherSections
"@

# Clean up any double newlines
$commentBody = $commentBody -replace "`n{4,}", "`n`n`n"

if ($DryRun) {
    # File-based DryRun: mirrors GitHub comment behavior using a local file
    if ([string]::IsNullOrWhiteSpace($PreviewFile)) {
        $PreviewFile = "CustomAgentLogsTmp/PRState/$PRNumber/ai-summary-comment-preview.md"
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
    
    # Update or insert the PR-REVIEW section
    $PR_REVIEW_MARKER = "<!-- SECTION:PR-REVIEW -->"
    $PR_REVIEW_END_MARKER = "<!-- /SECTION:PR-REVIEW -->"
    
    if ($existingPreview -match [regex]::Escape($PR_REVIEW_MARKER)) {
        # Replace existing PR-REVIEW section
        $pattern = [regex]::Escape($PR_REVIEW_MARKER) + "[\s\S]*?" + [regex]::Escape($PR_REVIEW_END_MARKER)
        $finalComment = $existingPreview -replace $pattern, $prReviewSection
    } elseif (-not [string]::IsNullOrWhiteSpace($existingPreview)) {
        # Append PR-REVIEW section to existing content
        $finalComment = $existingPreview.TrimEnd() + "`n`n" + $prReviewSection
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

# Post or update comment (reuse $existingComment from earlier check)
if ($existingComment) {
    Write-Host "✓ Updating existing review comment (ID: $($existingComment.id))..." -ForegroundColor Green
    
    # Create temp file for update
    $tempFile = [System.IO.Path]::GetTempFileName()
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8

    gh api --method PATCH "repos/dotnet/maui/issues/comments/$($existingComment.id)" --input $tempFile | Out-Null
    Remove-Item $tempFile

    Write-Host "✅ Review comment updated successfully" -ForegroundColor Green
} else {
    Write-Host "Creating new review comment..." -ForegroundColor Yellow
    
    # Create temp file for new comment
    $tempFile = [System.IO.Path]::GetTempFileName()
    @{ body = $commentBody } | ConvertTo-Json -Depth 10 | Set-Content -Path $tempFile -Encoding UTF8
    
    gh api --method POST "repos/dotnet/maui/issues/$PRNumber/comments" --input $tempFile | Out-Null
    Remove-Item $tempFile
    
    Write-Host "✅ Review comment posted successfully" -ForegroundColor Green
}
