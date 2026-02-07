<#
.SYNOPSIS
    Collects agent interaction data from multiple sources for analysis.

.DESCRIPTION
    Gathers data from:
    - Agent PR session files (.github/agent-pr-session/*.md)
    - Copilot comments on PRs (via gh CLI)
    - CCA session logs (CustomAgentLogsTmp/PRState/)
    Outputs structured JSON for analysis by Analyze-And-Recommend.ps1.

.PARAMETER PRNumbers
    Comma-separated list of PR numbers to analyze. If not specified, discovers PRs automatically.

.PARAMETER Label
    GitHub label to filter PRs by (e.g., "copilot"). Default: "copilot"

.PARAMETER Since
    ISO 8601 date string to filter PRs from. Default: 30 days ago.

.PARAMETER OutputDir
    Output directory for collected data. Default: CustomAgentLogsTmp/scrape-and-improve

.PARAMETER RepoRoot
    Repository root directory. Default: current directory.

.EXAMPLE
    pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1
    pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1 -PRNumbers "33380,33134"
    pwsh .github/skills/scrape-and-improve/scripts/Collect-AgentData.ps1 -Label "copilot" -Since "2026-01-01"
#>

param(
    [string]$PRNumbers = "",
    [string]$Label = "copilot",
    [string]$Since = "",
    [string]$OutputDir = "CustomAgentLogsTmp/scrape-and-improve",
    [string]$RepoRoot = ".",
    [string]$Repository = "dotnet/maui"
)

$ErrorActionPreference = "Continue"

# Ensure output directory exists
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Scrape and Improve: Data Collection"  -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Default Since to 30 days ago
if (-not $Since) {
    $Since = (Get-Date).AddDays(-30).ToString("yyyy-MM-dd")
}

$collectedData = @{
    collectedAt = (Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
    sources     = @{
        agentSessions  = @()
        copilotComments = @()
        ccaSessions    = @()
    }
    summary     = @{
        totalPRsAnalyzed       = 0
        totalSessionFiles      = 0
        totalCopilotComments   = 0
        totalCCASessions       = 0
        totalFixAttempts       = 0
        totalSuccessfulFixes   = 0
        totalFailedFixes       = 0
    }
}

# ─────────────────────────────────────────────────────────────
# Source 1: Agent PR Session files
# ─────────────────────────────────────────────────────────────
Write-Host "`n📁 Collecting Agent PR Session files..." -ForegroundColor Yellow

$sessionDir = Join-Path $RepoRoot ".github/agent-pr-session"
if (Test-Path $sessionDir) {
    $sessionFiles = Get-ChildItem -Path $sessionDir -Filter "*.md" -File
    Write-Host "   Found $($sessionFiles.Count) session file(s)"

    foreach ($file in $sessionFiles) {
        $content = Get-Content -Path $file.FullName -Raw
        $prNumber = ""

        # Extract PR number from filename (pr-XXXXX.md)
        if ($file.Name -match "pr-(\d+)\.md") {
            $prNumber = $Matches[1]
        }

        # Extract key sections
        $session = @{
            file      = $file.Name
            prNumber  = $prNumber
            phases    = @()
            fixCandidates = @()
            recommendation = ""
            rootCause = ""
        }

        # Extract phase statuses
        if ($content -match "(?s)\| Phase \| Status \|.*?\n((?:\|.*?\n)+)") {
            $phaseLines = $Matches[1] -split "`n" | Where-Object { $_ -match "^\|" }
            foreach ($line in $phaseLines) {
                if ($line -match "\|\s*(.+?)\s*\|\s*(.+?)\s*\|") {
                    $session.phases += @{
                        phase  = $Matches[1].Trim()
                        status = $Matches[2].Trim()
                    }
                }
            }
        }

        # Extract recommendation
        if ($content -match "Recommendation:\s*(.*?)$" ) {
            $session.recommendation = $Matches[1].Trim()
        }

        # Extract fix candidates table
        if ($content -match "(?s)\| # \| Source \| Approach \| Test Result \|.*?\n((?:\|.*?\n)+)") {
            $fixLines = $Matches[1] -split "`n" | Where-Object { $_ -match "^\|" }
            foreach ($line in $fixLines) {
                $parts = ($line -split "\|" | Where-Object { $_.Trim() } | ForEach-Object { $_.Trim() })
                if ($parts.Count -ge 4) {
                    $result = $parts[3]
                    $isPass = $result -match "PASS"
                    $isFail = $result -match "FAIL"
                    $session.fixCandidates += @{
                        number   = $parts[0]
                        source   = $parts[1]
                        approach = $parts[2]
                        result   = $result
                        passed   = $isPass
                        failed   = $isFail
                    }
                    $collectedData.summary.totalFixAttempts++
                    if ($isPass) { $collectedData.summary.totalSuccessfulFixes++ }
                    if ($isFail) { $collectedData.summary.totalFailedFixes++ }
                }
            }
        }

        # Extract root cause
        if ($content -match "(?s)Root Cause.*?\n(.*?)(?=\n#{2,}|\n\*\*|</details>)") {
            $session.rootCause = $Matches[1].Trim().Substring(0, [Math]::Min(500, $Matches[1].Trim().Length))
        }

        $collectedData.sources.agentSessions += $session
        $collectedData.summary.totalSessionFiles++
    }
} else {
    Write-Host "   No agent-pr-session directory found"
}

# ─────────────────────────────────────────────────────────────
# Source 2: Copilot Comments on PRs (via gh CLI)
# ─────────────────────────────────────────────────────────────
Write-Host "`n💬 Collecting Copilot comments from PRs..." -ForegroundColor Yellow

# Check if gh CLI is available
$ghAvailable = $null -ne (Get-Command "gh" -ErrorAction SilentlyContinue)

if ($ghAvailable) {
    $prList = @()

    if ($PRNumbers) {
        # Use specified PR numbers
        $prList = $PRNumbers -split "," | ForEach-Object { $_.Trim() }
        Write-Host "   Using specified PRs: $($prList -join ', ')"
    } else {
        # Discover PRs with copilot label
        Write-Host "   Searching for PRs with label '$Label' since $Since..."
        try {
            $searchResult = gh pr list --repo $Repository --label $Label --state all --limit 50 --json number,title,state,createdAt 2>&1
            if ($LASTEXITCODE -eq 0 -and $searchResult) {
                $prs = $searchResult | ConvertFrom-Json
                $prList = $prs | Where-Object {
                    [datetime]$_.createdAt -ge [datetime]$Since
                } | ForEach-Object { $_.number.ToString() }
                Write-Host "   Found $($prList.Count) PR(s) with label '$Label'"
            }
        } catch {
            Write-Host "   ⚠️ Could not search PRs: $_" -ForegroundColor Red
        }
    }

    foreach ($pr in $prList) {
        Write-Host "   Processing PR #$pr..."
        try {
            # Get PR comments
            $comments = gh pr view $pr --repo $Repository --json comments --jq '.comments[] | {body: .body, author: .author.login, createdAt: .createdAt}' 2>&1
            if ($LASTEXITCODE -ne 0) { continue }

            $prCommentData = @{
                prNumber        = $pr
                aiSummaryFound  = $false
                tryFixAttempts  = 0
                testVerification = $false
                codeReview      = $false
                commentCount    = 0
                patterns        = @()
            }

            # Parse comments as JSON lines
            $commentObjects = @()
            try {
                $commentObjects = $comments | ConvertFrom-Json -ErrorAction SilentlyContinue
            } catch {
                # Try line-by-line parsing
            }

            foreach ($comment in $commentObjects) {
                $body = if ($comment.body) { $comment.body } else { "$comment" }
                $prCommentData.commentCount++

                # Check for AI Summary marker
                if ($body -match "<!-- AI Summary -->") {
                    $prCommentData.aiSummaryFound = $true
                }

                # Count try-fix attempts
                if ($body -match "Try-Fix Attempt") {
                    $prCommentData.tryFixAttempts++
                }

                # Check for test verification
                if ($body -match "SECTION:VERIFY-TESTS|Test Verification") {
                    $prCommentData.testVerification = $true
                }

                # Check for code review
                if ($body -match "SECTION:PR-REVIEW|Code Review") {
                    $prCommentData.codeReview = $true
                }

                # Look for failure patterns in comments
                if ($body -match "(?i)wrong file|tunnel vision|misread error|over-engineer") {
                    $prCommentData.patterns += "failure-pattern-mentioned"
                }
            }

            $collectedData.sources.copilotComments += $prCommentData
            $collectedData.summary.totalCopilotComments += $prCommentData.commentCount

        } catch {
            Write-Host "   ⚠️ Could not process PR #${pr}: $_" -ForegroundColor Red
        }
    }
} else {
    Write-Host "   ⚠️ gh CLI not available, skipping Copilot comment collection" -ForegroundColor Red
}

# ─────────────────────────────────────────────────────────────
# Source 3: CCA Session Data
# ─────────────────────────────────────────────────────────────
Write-Host "`n📂 Collecting CCA session data..." -ForegroundColor Yellow

$ccaDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState"
if (Test-Path $ccaDir) {
    $ccaFiles = Get-ChildItem -Path $ccaDir -Filter "*.md" -File -ErrorAction SilentlyContinue
    Write-Host "   Found $($ccaFiles.Count) CCA session file(s)"

    foreach ($file in $ccaFiles) {
        $content = Get-Content -Path $file.FullName -Raw
        $sessionData = @{
            file       = $file.Name
            issueOrPR  = ""
            hasFixCandidates = $false
            attemptCount     = 0
            phases           = @()
        }

        # Extract issue/PR number
        if ($file.Name -match "(issue|pr)-(\d+)\.md") {
            $sessionData.issueOrPR = "$($Matches[1])-$($Matches[2])"
        }

        # Check for fix candidates
        if ($content -match "Fix Candidates") {
            $sessionData.hasFixCandidates = $true
        }

        # Count attempts
        $attemptMatches = [regex]::Matches($content, "(?i)attempt|try-fix")
        $sessionData.attemptCount = $attemptMatches.Count

        # Extract phases
        if ($content -match "(?s)\| Phase \| Status \|.*?\n((?:\|.*?\n)+)") {
            $phaseLines = $Matches[1] -split "`n" | Where-Object { $_ -match "^\|" }
            foreach ($line in $phaseLines) {
                if ($line -match "\|\s*(.+?)\s*\|\s*(.+?)\s*\|") {
                    $sessionData.phases += @{
                        phase  = $Matches[1].Trim()
                        status = $Matches[2].Trim()
                    }
                }
            }
        }

        $collectedData.sources.ccaSessions += $sessionData
        $collectedData.summary.totalCCASessions++
    }

    # Also check for try-fix attempt directories
    $tryFixDirs = Get-ChildItem -Path $ccaDir -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $tryFixPath = Join-Path $_.FullName "try-fix"
        if (Test-Path $tryFixPath) {
            Get-ChildItem -Path $tryFixPath -Directory -Filter "attempt-*" -ErrorAction SilentlyContinue
        }
    }

    if ($tryFixDirs) {
        Write-Host "   Found $($tryFixDirs.Count) try-fix attempt directories"
    }
} else {
    Write-Host "   No CCA session data directory found"
}

# ─────────────────────────────────────────────────────────────
# Compute totals
# ─────────────────────────────────────────────────────────────
$allPRs = @()
$allPRs += $collectedData.sources.agentSessions | ForEach-Object { $_.prNumber } | Where-Object { $_ }
$allPRs += $collectedData.sources.copilotComments | ForEach-Object { $_.prNumber } | Where-Object { $_ }
$collectedData.summary.totalPRsAnalyzed = ($allPRs | Sort-Object -Unique).Count

# ─────────────────────────────────────────────────────────────
# Save output
# ─────────────────────────────────────────────────────────────
$outputFile = Join-Path $OutputDir "collected-data.json"
$collectedData | ConvertTo-Json -Depth 10 | Set-Content -Path $outputFile -Encoding UTF8

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  Collection Complete!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Session files:      $($collectedData.summary.totalSessionFiles)"
Write-Host "  Copilot comments:   $($collectedData.summary.totalCopilotComments)"
Write-Host "  CCA sessions:       $($collectedData.summary.totalCCASessions)"
Write-Host "  Total PRs analyzed: $($collectedData.summary.totalPRsAnalyzed)"
Write-Host "  Fix attempts:       $($collectedData.summary.totalFixAttempts)"
Write-Host "    Successful:       $($collectedData.summary.totalSuccessfulFixes)"
Write-Host "    Failed:           $($collectedData.summary.totalFailedFixes)"
Write-Host ""
Write-Host "  Output: $outputFile" -ForegroundColor Cyan
