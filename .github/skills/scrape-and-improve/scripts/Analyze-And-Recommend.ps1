<#
.SYNOPSIS
    Analyzes collected agent data and generates instruction improvement recommendations.

.DESCRIPTION
    Reads the collected data from Collect-AgentData.ps1 and identifies patterns
    across agent interactions, memories, and PR review responses. Produces a
    markdown report with prioritized recommendations for instruction file updates.

.PARAMETER InputFile
    Path to the collected data JSON file. Default: CustomAgentLogsTmp/scrape-and-improve/collected-data.json

.PARAMETER OutputDir
    Output directory for the analysis report. Default: CustomAgentLogsTmp/scrape-and-improve

.PARAMETER RepoRoot
    Repository root directory. Default: current directory.

.EXAMPLE
    pwsh .github/skills/scrape-and-improve/scripts/Analyze-And-Recommend.ps1
    pwsh .github/skills/scrape-and-improve/scripts/Analyze-And-Recommend.ps1 -InputFile path/to/data.json
#>

param(
    [string]$InputFile = "CustomAgentLogsTmp/scrape-and-improve/collected-data.json",
    [string]$OutputDir = "CustomAgentLogsTmp/scrape-and-improve",
    [string]$RepoRoot = "."
)

$ErrorActionPreference = "Continue"

# Analysis thresholds
$HIGH_REJECTION_THRESHOLD = 30    # Percentage - flag when Copilot suggestion rejection rate exceeds this
$HOTSPOT_COMMENT_THRESHOLD = 5    # Count - flag areas with this many or more review comments
$MEMORY_FREQUENCY_THRESHOLD = 2   # Count - flag memory subjects appearing this many or more times

# Ensure output directory exists
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Scrape and Improve: Analysis"  -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# ─────────────────────────────────────────────────────────────
# Load collected data
# ─────────────────────────────────────────────────────────────
if (-not (Test-Path $InputFile)) {
    Write-Host "❌ Input file not found: $InputFile" -ForegroundColor Red
    Write-Host "   Run Collect-AgentData.ps1 first." -ForegroundColor Red
    exit 1
}

$data = Get-Content -Path $InputFile -Raw | ConvertFrom-Json

Write-Host "`n📊 Loaded data from $InputFile"
Write-Host "   PRs analyzed: $($data.summary.totalPRsAnalyzed)"
Write-Host "   Fix attempts: $($data.summary.totalFixAttempts)"

# ─────────────────────────────────────────────────────────────
# Identify existing instruction files
# ─────────────────────────────────────────────────────────────
Write-Host "`n📄 Scanning existing instruction files..." -ForegroundColor Yellow

$instructionDir = Join-Path $RepoRoot ".github/instructions"
$existingInstructions = @()
if (Test-Path $instructionDir) {
    $existingInstructions = Get-ChildItem -Path $instructionDir -Filter "*.instructions.md" -File |
        ForEach-Object { $_.Name }
    Write-Host "   Found $($existingInstructions.Count) instruction file(s)"
}

$skillDir = Join-Path $RepoRoot ".github/skills"
$existingSkills = @()
if (Test-Path $skillDir) {
    $existingSkills = Get-ChildItem -Path $skillDir -Directory |
        ForEach-Object { $_.Name }
    Write-Host "   Found $($existingSkills.Count) skill(s)"
}

# ─────────────────────────────────────────────────────────────
# Pattern Analysis
# ─────────────────────────────────────────────────────────────
Write-Host "`n🔍 Analyzing patterns..." -ForegroundColor Yellow

$patterns = @{
    multipleAttempts   = @()   # PRs with >2 fix attempts
    singleAttempt      = @()   # PRs with exactly 1 attempt (quick success)
    failedAttempts     = @()   # Individual failed fix attempts
    successfulAttempts = @()   # Individual successful fix attempts
    rootCauses         = @()   # Extracted root causes
    platformPatterns   = @{}   # Platform-specific patterns
}

foreach ($session in $data.sources.agentSessions) {
    $attemptCount = $session.fixCandidates.Count

    if ($attemptCount -gt 2) {
        $patterns.multipleAttempts += @{
            pr       = $session.prNumber
            attempts = $attemptCount
            file     = $session.file
        }
    } elseif ($attemptCount -eq 1) {
        $patterns.singleAttempt += @{
            pr   = $session.prNumber
            file = $session.file
        }
    }

    foreach ($fix in $session.fixCandidates) {
        if ($fix.failed) {
            $patterns.failedAttempts += @{
                pr       = $session.prNumber
                approach = $fix.approach
                source   = $fix.source
            }
        }
        if ($fix.passed) {
            $patterns.successfulAttempts += @{
                pr       = $session.prNumber
                approach = $fix.approach
                source   = $fix.source
            }
        }
    }

    if ($session.rootCause) {
        $patterns.rootCauses += @{
            pr        = $session.prNumber
            rootCause = $session.rootCause
        }
    }
}

# Analyze Copilot comment patterns
$prsWithAISummary = ($data.sources.copilotComments | Where-Object { $_.aiSummaryFound }).Count
$prsWithTryFix = ($data.sources.copilotComments | Where-Object { $_.tryFixAttempts -gt 0 }).Count
$prsWithTestVerification = ($data.sources.copilotComments | Where-Object { $_.testVerification }).Count

# ─────────────────────────────────────────────────────────────
# Analyze Memories
# ─────────────────────────────────────────────────────────────
Write-Host "`n🧠 Analyzing memories..." -ForegroundColor Yellow

$memoryAnalysis = @{
    totalMemories     = 0
    bySubject         = @{}
    bySource          = @{}
    conventions       = @()
    buildCommands     = @()
    storeEvents       = @()
}

if ($data.sources.memories) {
    $memoryAnalysis.totalMemories = $data.sources.memories.Count

    foreach ($mem in $data.sources.memories) {
        $subject = if ($mem.subject) { $mem.subject } else { "unknown" }
        $source = if ($mem.source) { $mem.source } else { "unknown" }

        if (-not $memoryAnalysis.bySubject.ContainsKey($subject)) {
            $memoryAnalysis.bySubject[$subject] = 0
        }
        $memoryAnalysis.bySubject[$subject]++

        if (-not $memoryAnalysis.bySource.ContainsKey($source)) {
            $memoryAnalysis.bySource[$source] = 0
        }
        $memoryAnalysis.bySource[$source]++

        # Categorize
        switch ($subject) {
            "convention"    { $memoryAnalysis.conventions += $mem }
            "build-command" { $memoryAnalysis.buildCommands += $mem }
            "store-event"   { $memoryAnalysis.storeEvents += $mem }
        }
    }
    Write-Host "   Memories analyzed: $($memoryAnalysis.totalMemories)"
    Write-Host "   Subjects found: $($memoryAnalysis.bySubject.Count)"
}

# ─────────────────────────────────────────────────────────────
# Analyze Recent PR Suggestion Responses
# ─────────────────────────────────────────────────────────────
Write-Host "`n📊 Analyzing recent PR suggestion responses..." -ForegroundColor Yellow

$suggestionAnalysis = @{
    totalPRs                = 0
    copilotPRs              = 0
    totalReviewComments     = 0
    totalCopilotSuggestions = 0
    acceptanceRate          = 0
    rejectionRate           = 0
    commonChangeRequests    = @()
    filePathPatterns        = @{}
    copilotPRList           = @()
}

if ($data.sources.recentPRSuggestions) {
    $suggestionAnalysis.totalPRs = $data.sources.recentPRSuggestions.Count

    foreach ($prData in $data.sources.recentPRSuggestions) {
        if ($prData.isCopilotPR) {
            $suggestionAnalysis.copilotPRs++
            $suggestionAnalysis.copilotPRList += @{
                number = $prData.prNumber
                title  = $prData.title
                state  = $prData.state
            }
        }

        $stats = $prData.suggestionStats
        $suggestionAnalysis.totalReviewComments += $stats.totalReviewComments
        $suggestionAnalysis.totalCopilotSuggestions += $stats.copilotSuggestions

        # Track file path patterns in review comments
        foreach ($rc in $prData.reviewComments) {
            if ($rc.path) {
                # Extract component area from path
                $area = ""
                if ($rc.path -match "src/Controls/") { $area = "Controls" }
                elseif ($rc.path -match "src/Core/") { $area = "Core" }
                elseif ($rc.path -match "src/Essentials/") { $area = "Essentials" }
                elseif ($rc.path -match "src/BlazorWebView/") { $area = "BlazorWebView" }
                elseif ($rc.path -match "\.github/") { $area = "GitHub/CI" }
                elseif ($rc.path -match "eng/") { $area = "Engineering" }
                else { $area = "Other" }

                if (-not $suggestionAnalysis.filePathPatterns.ContainsKey($area)) {
                    $suggestionAnalysis.filePathPatterns[$area] = 0
                }
                $suggestionAnalysis.filePathPatterns[$area]++
            }
        }
    }

    # Calculate rates
    $totalSuggestions = $suggestionAnalysis.totalCopilotSuggestions
    if ($totalSuggestions -gt 0) {
        $totalAccepted = ($data.sources.recentPRSuggestions | ForEach-Object { $_.suggestionStats.suggestionsAccepted } | Measure-Object -Sum).Sum
        $totalRejected = ($data.sources.recentPRSuggestions | ForEach-Object { $_.suggestionStats.suggestionsRejected } | Measure-Object -Sum).Sum
        $suggestionAnalysis.acceptanceRate = [Math]::Round(($totalAccepted / $totalSuggestions) * 100, 1)
        $suggestionAnalysis.rejectionRate = [Math]::Round(($totalRejected / $totalSuggestions) * 100, 1)
    }

    Write-Host "   Recent PRs analyzed: $($suggestionAnalysis.totalPRs)"
    Write-Host "   Copilot PRs: $($suggestionAnalysis.copilotPRs)"
    Write-Host "   Review comments: $($suggestionAnalysis.totalReviewComments)"
}

# ─────────────────────────────────────────────────────────────
# Generate recommendations
# ─────────────────────────────────────────────────────────────
Write-Host "`n💡 Generating recommendations..." -ForegroundColor Yellow

$recommendations = @()

# Recommendation: High failure rate patterns
if ($patterns.failedAttempts.Count -gt 0) {
    $failedApproaches = $patterns.failedAttempts | Group-Object { $_.approach.Substring(0, [Math]::Min(50, $_.approach.Length)) }
    foreach ($group in ($failedApproaches | Sort-Object Count -Descending | Select-Object -First 3)) {
        $recommendations += @{
            category = "Instruction File"
            priority = "Medium"
            location = ".github/instructions/"
            change   = "Document that '$($group.Name)' approach has failed in $($group.Count) PR(s): $($group.Group.pr -join ', ')"
            evidence = "PRs: $($group.Group.pr -join ', ')"
            impact   = "Prevents agents from repeating known-failing approaches"
        }
    }
}

# Recommendation: Quick success patterns to reinforce
if ($patterns.singleAttempt.Count -gt 0) {
    $recommendations += @{
        category = "Skill Enhancement"
        priority = "Medium"
        location = ".github/skills/try-fix/SKILL.md"
        change   = "Document quick-success patterns from PRs: $($patterns.singleAttempt.pr -join ', '). These succeeded on first attempt."
        evidence = "PRs: $($patterns.singleAttempt.pr -join ', ')"
        impact   = "Reinforces successful search/fix strategies"
    }
}

# Recommendation: Multiple attempts indicate discovery issues
if ($patterns.multipleAttempts.Count -gt 0) {
    $avgAttempts = ($patterns.multipleAttempts | Measure-Object -Property attempts -Average).Average
    $recommendations += @{
        category = "Instruction File"
        priority = "High"
        location = ".github/copilot-instructions.md"
        change   = "PRs with slow discovery (avg $([Math]::Round($avgAttempts, 1)) attempts): $($patterns.multipleAttempts.pr -join ', '). Consider adding guidance for these component areas."
        evidence = "PRs: $($patterns.multipleAttempts.pr -join ', ')"
        impact   = "Reduces average fix attempts from $([Math]::Round($avgAttempts, 1)) toward 1-2"
    }
}

# Recommendation: Root cause patterns
if ($patterns.rootCauses.Count -gt 0) {
    $recommendations += @{
        category = "Architecture Doc"
        priority = "Low"
        location = ".github/instructions/"
        change   = "Document common root causes found across $($patterns.rootCauses.Count) PR(s) to help agents identify root causes faster."
        evidence = "PRs: $($patterns.rootCauses.pr -join ', ')"
        impact   = "Helps agents identify root causes faster by learning from past findings"
    }
}

# Recommendation: If PRs lack test verification
if ($prsWithTryFix -gt 0 -and $prsWithTestVerification -eq 0) {
    $recommendations += @{
        category = "Skill Enhancement"
        priority = "High"
        location = ".github/skills/try-fix/SKILL.md"
        change   = "Ensure try-fix attempts include test verification step. $prsWithTryFix PR(s) had try-fix but no test verification."
        evidence = "Analysis of Copilot comments"
        impact   = "Increases confidence in fix correctness before reporting"
    }
}

# Memory-based recommendations
if ($memoryAnalysis.totalMemories -gt 0) {
    # Check for frequently recurring memory subjects
    $topSubjects = $memoryAnalysis.bySubject.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 3
    foreach ($subject in $topSubjects) {
        if ($subject.Value -ge $MEMORY_FREQUENCY_THRESHOLD) {
            $recommendations += @{
                category = "Instruction File"
                priority = "Medium"
                location = ".github/instructions/"
                change   = "Memory pattern '$($subject.Key)' appears $($subject.Value) times across sessions. Consider formalizing this as an instruction to prevent agents from having to re-learn it."
                evidence = "Memory analysis: $($subject.Value) occurrences of '$($subject.Key)'"
                impact   = "Reduces re-learning overhead for agents by codifying frequently stored knowledge"
            }
        }
    }

    # If many conventions are being stored, they should be in instruction files
    if ($memoryAnalysis.conventions.Count -gt 0) {
        $recommendations += @{
            category = "Instruction File"
            priority = "High"
            location = ".github/copilot-instructions.md"
            change   = "Found $($memoryAnalysis.conventions.Count) convention-related memory entries. These should be documented in instruction files so agents don't need to rely on memory recall."
            evidence = "Memory scan: $($memoryAnalysis.conventions.Count) convention patterns found"
            impact   = "Ensures conventions are always available, not dependent on memory retention"
        }
    }
}

# Recent PR suggestion-based recommendations
if ($suggestionAnalysis.totalPRs -gt 0) {
    # High rejection rate indicates suggestion quality issues
    if ($suggestionAnalysis.rejectionRate -gt $HIGH_REJECTION_THRESHOLD) {
        $recommendations += @{
            category = "Instruction File"
            priority = "High"
            location = ".github/copilot-instructions.md"
            change   = "Copilot suggestions have a $($suggestionAnalysis.rejectionRate)% rejection rate across $($suggestionAnalysis.totalPRs) recent PRs. Review rejected suggestions to identify common issues and add guidance."
            evidence = "Review comment analysis: $($suggestionAnalysis.totalCopilotSuggestions) suggestions, $($suggestionAnalysis.rejectionRate)% rejected"
            impact   = "Improves suggestion acceptance rate by addressing common rejection reasons"
        }
    }

    # High review comment volume in specific areas suggests missing guidance
    $hotAreas = $suggestionAnalysis.filePathPatterns.GetEnumerator() | Sort-Object Value -Descending | Select-Object -First 3
    foreach ($area in $hotAreas) {
        if ($area.Value -ge $HOTSPOT_COMMENT_THRESHOLD) {
            $recommendations += @{
                category = "Instruction File"
                priority = "Medium"
                location = ".github/instructions/"
                change   = "Area '$($area.Key)' has $($area.Value) review comments across recent PRs. Consider adding or enhancing instruction files for this area."
                evidence = "Review comment analysis: $($area.Value) comments in $($area.Key) files"
                impact   = "Reduces review friction in high-comment areas"
            }
        }
    }

    # Copilot PRs success tracking
    if ($suggestionAnalysis.copilotPRs -gt 0) {
        $recommendations += @{
            category = "General"
            priority = "Low"
            location = ".github/copilot-instructions.md"
            change   = "Found $($suggestionAnalysis.copilotPRs) Copilot-labeled PR(s) in the last $($suggestionAnalysis.totalPRs) PRs. Track these for ongoing quality improvement."
            evidence = "PRs: $(($suggestionAnalysis.copilotPRList | Select-Object -First 10 | ForEach-Object { "#$($_.number)" }) -join ', ')"
            impact   = "Provides baseline metrics for agent improvement tracking"
        }
    }
}

# ─────────────────────────────────────────────────────────────
# Generate Report
# ─────────────────────────────────────────────────────────────
Write-Host "`n📝 Generating report..." -ForegroundColor Yellow

$report = @"
# Scrape and Improve: Analysis Report

**Generated:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss UTC")

## Data Summary

| Metric | Value |
|--------|-------|
| Total PRs Analyzed | $($data.summary.totalPRsAnalyzed) |
| Agent Session Files | $($data.summary.totalSessionFiles) |
| Copilot Comments | $($data.summary.totalCopilotComments) |
| CCA Sessions | $($data.summary.totalCCASessions) |
| Memories Collected | $($data.summary.totalMemories) |
| Recent PRs Scraped | $($data.summary.totalRecentPRsScraped) |
| Suggestion Responses | $($data.summary.totalSuggestionResponses) |
| Total Fix Attempts | $($data.summary.totalFixAttempts) |
| Successful Fixes | $($data.summary.totalSuccessfulFixes) |
| Failed Fixes | $($data.summary.totalFailedFixes) |
| Success Rate | $(if ($data.summary.totalFixAttempts -gt 0) { [Math]::Round(($data.summary.totalSuccessfulFixes / $data.summary.totalFixAttempts) * 100, 1) } else { "N/A" })% |

## Pattern Analysis

### PRs Requiring Multiple Attempts (Slow Discovery)

$(if ($patterns.multipleAttempts.Count -gt 0) {
    "| PR | Attempts | Session File |`n|-----|----------|--------------|"
    $patterns.multipleAttempts | ForEach-Object { "| #$($_.pr) | $($_.attempts) | $($_.file) |" }
} else {
    "_No PRs with multiple attempts found._"
})

### Quick Successes (First Attempt)

$(if ($patterns.singleAttempt.Count -gt 0) {
    "| PR | Session File |`n|-----|--------------|"
    $patterns.singleAttempt | ForEach-Object { "| #$($_.pr) | $($_.file) |" }
} else {
    "_No single-attempt successes found._"
})

### Copilot Integration Status

| Metric | Count |
|--------|-------|
| PRs with AI Summary | $prsWithAISummary |
| PRs with Try-Fix | $prsWithTryFix |
| PRs with Test Verification | $prsWithTestVerification |

### Root Causes Identified

$(if ($patterns.rootCauses.Count -gt 0) {
    $patterns.rootCauses | ForEach-Object {
        "- **PR #$($_.pr)**: $($_.rootCause.Substring(0, [Math]::Min(200, $_.rootCause.Length)))..."
    }
} else {
    "_No root causes extracted._"
})

## Memory Analysis

| Metric | Value |
|--------|-------|
| Total Memories | $($memoryAnalysis.totalMemories) |
| Unique Subjects | $($memoryAnalysis.bySubject.Count) |
| Convention Patterns | $($memoryAnalysis.conventions.Count) |
| Build Command Patterns | $($memoryAnalysis.buildCommands.Count) |

$(if ($memoryAnalysis.bySubject.Count -gt 0) {
    "### Memory Subject Frequency`n`n| Subject | Count |`n|---------|-------|"
    $memoryAnalysis.bySubject.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { "| $($_.Key) | $($_.Value) |" }
} else {
    "_No memory data available. Pass -MemoryContext parameter with stored memories to analyze._"
})

## Recent PR Suggestion Analysis

| Metric | Value |
|--------|-------|
| Recent PRs Scraped | $($suggestionAnalysis.totalPRs) |
| Copilot PRs Found | $($suggestionAnalysis.copilotPRs) |
| Total Review Comments | $($suggestionAnalysis.totalReviewComments) |
| Copilot Suggestions | $($suggestionAnalysis.totalCopilotSuggestions) |
| Suggestion Acceptance Rate | $($suggestionAnalysis.acceptanceRate)% |
| Suggestion Rejection Rate | $($suggestionAnalysis.rejectionRate)% |

$(if ($suggestionAnalysis.copilotPRList.Count -gt 0) {
    "### Copilot PRs`n`n| PR | Title | State |`n|----|-------|-------|"
    $suggestionAnalysis.copilotPRList | ForEach-Object { "| #$($_.number) | $($_.title) | $($_.state) |" }
})

$(if ($suggestionAnalysis.filePathPatterns.Count -gt 0) {
    "### Review Comment Hotspots (by area)`n`n| Area | Review Comments |`n|------|----------------|"
    $suggestionAnalysis.filePathPatterns.GetEnumerator() | Sort-Object Value -Descending | ForEach-Object { "| $($_.Key) | $($_.Value) |" }
})

## Recommendations

$(if ($recommendations.Count -gt 0) {
    $i = 0
    $recommendations | ForEach-Object {
        $i++
        @"

### $i. [$($_.priority)] $($_.category)

- **Location:** ``$($_.location)``
- **Change:** $($_.change)
- **Evidence:** $($_.evidence)
- **Impact:** $($_.impact)

"@
    }
} else {
    "_No recommendations generated. Consider collecting more data._"
})

## Existing Instruction Files

$(if ($existingInstructions.Count -gt 0) {
    $existingInstructions | ForEach-Object { "- ``$_``" }
} else {
    "_No instruction files found._"
})

## Next Steps

1. Review recommendations above and decide which to apply
2. For automated application, use the ``learn-from-pr`` agent
3. For manual application, edit the target files directly
4. Re-run this analysis periodically to track improvement
"@

$reportFile = Join-Path $OutputDir "analysis-report.md"
$report | Set-Content -Path $reportFile -Encoding UTF8

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Analysis Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Recommendations: $($recommendations.Count)"
Write-Host "  Report: $reportFile" -ForegroundColor Cyan

# Also output recommendations as JSON for programmatic use
$recommendationsFile = Join-Path $OutputDir "recommendations.json"
$recommendations | ConvertTo-Json -Depth 5 | Set-Content -Path $recommendationsFile -Encoding UTF8
Write-Host "  Recommendations JSON: $recommendationsFile" -ForegroundColor Cyan
