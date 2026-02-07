<#
.SYNOPSIS
    Analyzes collected agent data and generates instruction improvement recommendations.

.DESCRIPTION
    Reads the collected data from Collect-AgentData.ps1 and identifies patterns
    across agent interactions. Produces a markdown report with prioritized
    recommendations for instruction file updates.

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
