<#
.SYNOPSIS
    Runs a PR review using Copilot CLI with skill-based prompts.

.DESCRIPTION
    Orchestrates a 5-step PR review by invoking Copilot CLI with skill prompts:
    
    Step 0: Branch setup        - Create review branch from main, cherry-pick PR squashed
    Step 1: pr-review skill     - 4-phase review (Pre-Flight, Gate, Try-Fix, Report)
    Step 2: pr-finalize skill   - Verify PR title/description match implementation
    Step 3: ai-summary-comment  - Post unified AI Summary comment to the PR
    Step 4: Apply labels        - Apply agent labels based on review results

    By default, the script checks out main and creates a review branch from it.
    Use -UseCurrentBranch to create the review branch from the current branch instead.

.PARAMETER PRNumber
    The GitHub PR number to review

.PARAMETER Platform
    Platform for testing. Valid: android, ios, windows, maccatalyst

.PARAMETER UseCurrentBranch
    Create the review branch from the current branch instead of main.
    By default, the script checks out main before creating the review branch.

.PARAMETER DryRun
    Show what would be done without making changes

.PARAMETER LogFile
    Capture all output via Start-Transcript

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687
    .\Review-PR.ps1 -PRNumber 33687 -Platform ios
    .\Review-PR.ps1 -PRNumber 33687 -UseCurrentBranch
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [ValidateSet('android', 'ios', 'windows', 'maccatalyst')]
    [string]$Platform,

    [Parameter(Mandatory = $false)]
    [switch]$UseCurrentBranch,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [string]$LogFile
)

$ErrorActionPreference = 'Stop'

if ($LogFile) {
    $logDir = Split-Path $LogFile -Parent
    if ($logDir -and -not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    Start-Transcript -Path $LogFile -Force | Out-Null
}

$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) { Write-Error "Not in a git repository"; exit 1 }

# ─── Banner ───────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           PR Review with Copilot CLI                      ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  PR:        #$PRNumber                                          ║" -ForegroundColor Cyan
if ($Platform) {
    Write-Host "║  Platform:  $Platform                                        ║" -ForegroundColor Cyan
} else {
    Write-Host "║  Platform:  (agent will determine)                        ║" -ForegroundColor Cyan
}
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# ─── Prerequisites ────────────────────────────────────────────────────────────
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

$ghVersion = gh --version 2>$null | Select-Object -First 1
if (-not $ghVersion) { Write-Error "GitHub CLI (gh) not installed"; exit 1 }
Write-Host "  ✅ GitHub CLI: $ghVersion" -ForegroundColor Green

$copilotCmd = Get-Command copilot -ErrorAction SilentlyContinue
if (-not $copilotCmd) { Write-Error "Copilot CLI not installed"; exit 1 }
$copilotVersion = (& copilot --version 2>&1 | Out-String).Trim()
if (-not $copilotVersion) { $copilotVersion = $copilotCmd.Source }
Write-Host "  ✅ Copilot CLI: $copilotVersion" -ForegroundColor Green

$prInfo = gh pr view $PRNumber --json title,state 2>$null | ConvertFrom-Json
if (-not $prInfo) { Write-Error "PR #$PRNumber not found"; exit 1 }
Write-Host "  ✅ PR: $($prInfo.title)" -ForegroundColor Green

# ─── Shared prompt rules ─────────────────────────────────────────────────────
$platformInstruction = if ($Platform) {
    "**Platform for testing:** $Platform"
} else {
    "**Platform for testing:** Determine from PR's affected code paths and current host OS."
}

$autonomousRules = @"

🚨 **AUTONOMOUS EXECUTION:**
- There is NO human operator - NEVER stop and ask for input
- On environment blockers: skip the blocked phase and continue
- Always prefer CONTINUING with partial results over STOPPING
"@

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 0: Branch Setup (Create Review Branch & Cherry-Pick PR)
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
Write-Host "║  STEP 0: BRANCH SETUP                                     ║" -ForegroundColor Yellow
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow

$reviewBranch = "pr-review-$PRNumber"

if ($DryRun) {
    if ($UseCurrentBranch) {
        Write-Host "[DRY RUN] Would create review branch '$reviewBranch' from current branch" -ForegroundColor Magenta
    } else {
        Write-Host "[DRY RUN] Would checkout main, then create review branch '$reviewBranch'" -ForegroundColor Magenta
    }
    Write-Host "[DRY RUN] Would fetch and cherry-pick PR #$PRNumber (squashed)" -ForegroundColor Magenta
} else {
    # Check for dirty working tree
    $dirtyFiles = git status --porcelain 2>$null
    if ($dirtyFiles) {
        Write-Error "Working tree is dirty. Please commit or stash changes before running review.`n$dirtyFiles"
        exit 1
    }

    # Delete leftover review branch from a previous run (if it exists)
    $existingBranch = git branch --list $reviewBranch 2>$null
    if ($existingBranch) {
        Write-Host "  ⚠️ Removing leftover branch '$reviewBranch' from previous run" -ForegroundColor Yellow
        git branch -D $reviewBranch 2>$null
    }

    if (-not $UseCurrentBranch) {
        # Default: checkout main first
        Write-Host "  📌 Checking out main branch..." -ForegroundColor Cyan
        git checkout main 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Error "Failed to checkout main"; exit 1 }
        git pull origin main --ff-only 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠️ git pull failed (non-fatal, continuing with local main)" -ForegroundColor Yellow
        }
    } else {
        $currentBranch = git branch --show-current 2>$null
        Write-Host "  📌 Using current branch: $currentBranch" -ForegroundColor Cyan
    }

    # Create review branch
    Write-Host "  🔀 Creating review branch: $reviewBranch" -ForegroundColor Cyan
    git checkout -b $reviewBranch 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create branch '$reviewBranch'"; exit 1 }

    # Fetch PR commits
    Write-Host "  📥 Fetching PR #$PRNumber..." -ForegroundColor Cyan
    $tempBranch = "temp-pr-$PRNumber"

    # Clean up any leftover temp branch
    git branch -D $tempBranch 2>$null | Out-Null

    # Try fetching from origin (same-repo PRs)
    git fetch origin "pull/$PRNumber/head:$tempBranch" 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        # Fork PR — get fork info
        Write-Host "  📥 Fetching from fork..." -ForegroundColor Cyan
        $forkInfo = gh pr view $PRNumber --json headRepositoryOwner,headRefName,headRepository 2>$null | ConvertFrom-Json
        if (-not $forkInfo -or -not $forkInfo.headRepositoryOwner) {
            Write-Error "Failed to fetch PR #$PRNumber (not found on origin or fork)"
            git checkout - 2>$null
            exit 1
        }
        $forkUrl = "https://github.com/$($forkInfo.headRepositoryOwner.login)/$($forkInfo.headRepository.name).git"
        git fetch $forkUrl "$($forkInfo.headRefName):$tempBranch" 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to fetch from fork: $forkUrl"
            git checkout - 2>$null
            exit 1
        }
    }

    # Identify PR-only commits (oldest first)
    $prCommits = git log --oneline --reverse "$tempBranch" --not HEAD 2>$null
    if (-not $prCommits) {
        Write-Host "  ⚠️ No new commits found in PR (already up to date?)" -ForegroundColor Yellow
        git branch -D $tempBranch 2>$null | Out-Null
    } else {
        $commitHashes = git log --format='%H' --reverse "$tempBranch" --not HEAD 2>$null
        $commitCount = ($commitHashes -split "`n" | Where-Object { $_.Trim() }).Count
        Write-Host "  🔀 Cherry-picking $commitCount commit(s) (squashed)..." -ForegroundColor Cyan

        # Cherry-pick all commits squashed into one
        $hashList = ($commitHashes -split "`n" | Where-Object { $_.Trim() }) -join ' '
        Invoke-Expression "git cherry-pick --no-commit $hashList" 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ❌ Cherry-pick conflict detected. Aborting." -ForegroundColor Red
            git cherry-pick --abort 2>$null
            git checkout - 2>$null
            git branch -D $reviewBranch 2>$null
            git branch -D $tempBranch 2>$null
            Write-Error "Cherry-pick conflicts for PR #$PRNumber. Manual resolution required."
            exit 1
        }

        git commit -m "PR #$PRNumber squashed for review" 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) { Write-Error "Failed to create squashed commit"; exit 1 }

        # Clean up temp branch
        git branch -D $tempBranch 2>$null | Out-Null
    }

    # Verify
    $headCommit = git log --oneline -1 2>$null
    Write-Host "  ✅ Review branch ready: $reviewBranch" -ForegroundColor Green
    Write-Host "  📝 HEAD: $headCommit" -ForegroundColor Gray
}

# ─── Helper: Invoke Copilot ──────────────────────────────────────────────────
function Invoke-CopilotStep {
    param([string]$StepName, [string]$Prompt)

    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
    Write-Host "║  $($StepName.PadRight(55))║" -ForegroundColor Magenta
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

    if ($DryRun) {
        Write-Host "[DRY RUN] Prompt:" -ForegroundColor Magenta
        Write-Host $Prompt -ForegroundColor Gray
        return 0
    }

    # Use JSON output format to stream live progress of agent activity.
    # Without this, -p mode shows nothing until completion.
    & copilot -p $Prompt --allow-all --output-format json 2>&1 | ForEach-Object {
        $line = $_.ToString()
        try {
            $event = $line | ConvertFrom-Json -ErrorAction Stop
            switch ($event.type) {
                'assistant.turn_start' {
                    $turnId = $event.data.turnId
                    Write-Host "  ─── Turn $turnId ───" -ForegroundColor DarkGray
                }
                'tool.execution_start' {
                    $toolName = $event.data.toolName
                    $args_ = $event.data.arguments
                    $detail = $args_.description ?? $args_.command ?? $args_.pattern ?? $args_.query ?? $args_.intent ?? ''
                    if ($detail) { $detail = $detail.Substring(0, [Math]::Min($detail.Length, 80)) }
                    if ($toolName -ne 'report_intent') {
                        Write-Host "  🔧 $toolName" -ForegroundColor Cyan -NoNewline
                        if ($detail) { Write-Host ": $detail" -ForegroundColor Gray } else { Write-Host "" }
                    }
                }
                'tool.execution_complete' {
                    $ok = if ($event.data.success) { "✅" } else { "❌" }
                    # Intentionally quiet on success to reduce noise
                    if (-not $event.data.success) {
                        Write-Host "    $ok tool failed" -ForegroundColor Red
                    }
                }
                'assistant.message' {
                    $content = $event.data.content
                    if ($content -and $content.Trim()) {
                        # Show first 300 chars of agent response
                        $preview = $content.Trim().Substring(0, [Math]::Min($content.Trim().Length, 300))
                        Write-Host "  💬 $preview" -ForegroundColor White
                    }
                }
            }
        } catch {
            # Non-JSON line (e.g. stats) — pass through as-is
            if ($line.Trim()) {
                Write-Host $line
            }
        }
    }
    $exitCode = $LASTEXITCODE

    if ($exitCode -eq 0) {
        Write-Host "  ✅ $StepName completed" -ForegroundColor Green
    } else {
        Write-Host "  ⚠️ $StepName exited with code: $exitCode" -ForegroundColor Yellow
    }
    return $exitCode
}

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 1: PR Review (4-phase skill)
# ═════════════════════════════════════════════════════════════════════════════

$step1Prompt = @"
Use a skill to review PR #$PRNumber.

$platformInstruction
$autonomousRules

📁 Write phase output to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/{phase}/content.md``
"@

Invoke-CopilotStep -StepName "STEP 1: PR REVIEW" -Prompt $step1Prompt | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 2: PR Finalize
# ═════════════════════════════════════════════════════════════════════════════

$step2Prompt = @"
Use a skill to finalize PR #$PRNumber. Write findings to ``CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pr-finalize/pr-finalize-summary.md``.
$autonomousRules
"@

Invoke-CopilotStep -StepName "STEP 2: PR FINALIZE" -Prompt $step2Prompt | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 3: Post AI Summary Comment
# ═════════════════════════════════════════════════════════════════════════════

$step3Prompt = @"
Use a skill to post an AI summary comment for PR #$PRNumber.
$autonomousRules
"@

Invoke-CopilotStep -StepName "STEP 3: POST AI SUMMARY" -Prompt $step3Prompt | Out-Null

# ═════════════════════════════════════════════════════════════════════════════
#  STEP 4: Apply Labels
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Blue
Write-Host "║  STEP 4: APPLY LABELS                                     ║" -ForegroundColor Blue
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Blue

$labelHelperPath = Join-Path $RepoRoot ".github/scripts/shared/Update-AgentLabels.ps1"
if (Test-Path $labelHelperPath) {
    try {
        . $labelHelperPath
        Apply-AgentLabels -PRNumber $PRNumber -RepoRoot $RepoRoot
        Write-Host "  ✅ Labels applied" -ForegroundColor Green
    } catch {
        Write-Host "  ⚠️ Label application failed (non-fatal): $_" -ForegroundColor Yellow
    }
} else {
    Write-Host "  ⚠️ Label helper not found — skipping" -ForegroundColor Yellow
}

# ═════════════════════════════════════════════════════════════════════════════
#  Cleanup
# ═════════════════════════════════════════════════════════════════════════════

Write-Host ""
Write-Host "🧹 Cleaning up child processes..." -ForegroundColor Gray
try {
    $orphans = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
        ($_.Path -and $_.Path -match "copilot") -or
        ($_.CommandLine -and $_.CommandLine -match "copilot")
    }
    $copilotProcs = Get-Process -Name "copilot" -ErrorAction SilentlyContinue
    $allOrphans = @($orphans) + @($copilotProcs) | Where-Object { $_ -ne $null } | Sort-Object Id -Unique
    if ($allOrphans.Count -gt 0) {
        Write-Host "  Stopping $($allOrphans.Count) orphaned process(es)" -ForegroundColor Gray
        $allOrphans | Stop-Process -Force -ErrorAction SilentlyContinue
    }
} catch {
    Write-Host "  ⚠️ Cleanup warning: $_" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "✅ Review complete for PR #$PRNumber" -ForegroundColor Green
Write-Host "📁 Output: CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/" -ForegroundColor Gray
Write-Host ""

if ($LogFile) { Stop-Transcript | Out-Null }
