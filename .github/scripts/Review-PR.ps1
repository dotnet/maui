<#
.SYNOPSIS
    Runs a PR review using Copilot CLI and the PR Agent workflow.

.DESCRIPTION
    This script invokes Copilot CLI to perform a comprehensive 4-phase PR review:
    
    Phase 1: Pre-Flight - Context gathering
    Phase 2: Gate - Verify tests catch the bug
    Phase 3: Fix - Multi-model exploration of alternatives
    Phase 4: Report - Final recommendation
    
    The script:
    - Validates prerequisites (gh CLI, PR exists)
    - Validates current branch is not protected (main, release/*, net*.0)
    - Merges the PR into the current branch (for isolated testing)
    - Creates the state directory
    - Invokes Copilot CLI with the pr agent

.PARAMETER PRNumber
    The GitHub PR number to review (e.g., 33687)

.PARAMETER Platform
    The platform to use for testing. Default is 'android'.
    Valid values: android, ios, windows, maccatalyst

.PARAMETER SkipMerge
    If specified, skips merging the PR into the current branch (useful if already merged)

.PARAMETER Interactive
    If specified, starts Copilot in interactive mode with the prompt.
    Default is non-interactive mode (exits after completion).

.PARAMETER DryRun
    If specified, shows what would be done without making changes

.PARAMETER RunFinalize
    If specified, runs the pr-finalize skill after the PR agent completes
    to verify PR title/description match the implementation.

.PARAMETER PostSummaryComment
    If specified, runs the ai-summary-comment skill after all other phases complete
    to post a combined summary comment on the PR from all phases.

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687
    Reviews PR #33687 in non-interactive mode (default) using auto-detected platform

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687 -Platform ios -SkipMerge
    Reviews PR #33687 on iOS without merging (assumes already merged)

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687 -Interactive
    Reviews PR #33687 in interactive mode (stays open for follow-up questions)

.NOTES
    Prerequisites:
    - GitHub CLI (gh) installed and authenticated
    - Copilot CLI (copilot) installed
    - For testing: Appropriate platform tools (Appium, emulators, etc.)
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [ValidateSet('android', 'ios', 'windows', 'maccatalyst')]
    [string]$Platform,  # Optional - agent will determine appropriate platform if not specified

    [Parameter(Mandatory = $false)]
    [switch]$SkipMerge,

    [Parameter(Mandatory = $false)]
    [switch]$Interactive,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun,

    [Parameter(Mandatory = $false)]
    [switch]$PostSummaryComment,

    [Parameter(Mandatory = $false)]
    [switch]$RunFinalize,

    [Parameter(Mandatory = $false)]
    [string]$LogFile  # If provided, captures all output via Start-Transcript
)

$ErrorActionPreference = 'Stop'

# Start transcript logging if LogFile specified (replaces external tee pipe)
if ($LogFile) {
    $logDir = Split-Path $LogFile -Parent
    if ($logDir -and -not (Test-Path $logDir)) {
        New-Item -ItemType Directory -Path $logDir -Force | Out-Null
    }
    Start-Transcript -Path $LogFile -Force | Out-Null
}

# Get repository root
$RepoRoot = git rev-parse --show-toplevel 2>$null
if (-not $RepoRoot) {
    Write-Error "Not in a git repository"
    exit 1
}

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

# Step 1: Verify prerequisites
Write-Host "📋 Checking prerequisites..." -ForegroundColor Yellow

# Check GitHub CLI
$ghVersion = gh --version 2>$null | Select-Object -First 1
if (-not $ghVersion) {
    Write-Error "GitHub CLI (gh) is not installed. Install from: https://cli.github.com/"
    exit 1
}
Write-Host "  ✅ GitHub CLI: $ghVersion" -ForegroundColor Green

# Check Copilot CLI
$copilotVersion = copilot --version 2>$null
if (-not $copilotVersion) {
    Write-Error "Copilot CLI is not installed. Install with: npm install -g @github/copilot"
    exit 1
}
Write-Host "  ✅ Copilot CLI: $copilotVersion" -ForegroundColor Green

# Check PR exists
Write-Host "  🔍 Verifying PR #$PRNumber exists..." -ForegroundColor Gray
$prInfo = gh pr view $PRNumber --json title,state,url 2>$null | ConvertFrom-Json
if (-not $prInfo) {
    Write-Error "PR #$PRNumber not found or not accessible"
    exit 1
}
Write-Host "  ✅ PR: $($prInfo.title)" -ForegroundColor Green
Write-Host "  ✅ State: $($prInfo.state)" -ForegroundColor Green

# Step 2: Validate current branch and merge PR
Write-Host ""
$currentBranch = git branch --show-current
Write-Host "📍 Current branch: $currentBranch" -ForegroundColor Yellow

# Check if on a protected branch (main, release/*, net*.0)
$protectedBranches = @('main', 'master')
$isProtected = $protectedBranches -contains $currentBranch -or 
               $currentBranch -match '^release/' -or 
               $currentBranch -match '^net\d+\.0$'

if ($isProtected) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  ERROR: Cannot run on protected branch!                   ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  Current branch: $currentBranch" -ForegroundColor Red
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  This script merges the PR into the current branch.      ║" -ForegroundColor Red
    Write-Host "║  Protected branches: main, release/*, net*.0             ║" -ForegroundColor Red
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Please checkout a working branch first:                  ║" -ForegroundColor Red
    Write-Host "║    git checkout -b pr-review-$PRNumber                        ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    exit 1
}

Write-Host "  ✅ Branch '$currentBranch' is not protected" -ForegroundColor Green

# Merge the PR into the current branch (unless skipped)
if (-not $SkipMerge) {
    Write-Host ""
    Write-Host "🔀 Merging PR #$PRNumber into current branch..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "  [DRY RUN] Would fetch and merge PR #$PRNumber" -ForegroundColor Magenta
    } else {
        # Fetch the PR ref and merge it
        Write-Host "  📥 Fetching PR #$PRNumber..." -ForegroundColor Gray
        git fetch origin pull/$PRNumber/head:temp-pr-$PRNumber 2>$null
        if ($LASTEXITCODE -ne 0) {
            # Try fetching from the PR's head repository (for fork PRs)
            $prDetails = gh pr view $PRNumber --json headRepositoryOwner,headRefName 2>$null | ConvertFrom-Json
            if ($prDetails) {
                $forkOwner = $prDetails.headRepositoryOwner.login
                $headRef = $prDetails.headRefName
                Write-Host "  📥 PR is from fork: $forkOwner, fetching..." -ForegroundColor Gray
                git fetch "https://github.com/$forkOwner/maui.git" "${headRef}:temp-pr-$PRNumber"
                if ($LASTEXITCODE -ne 0) {
                    Write-Error "Failed to fetch PR #$PRNumber from fork"
                    exit 1
                }
            } else {
                Write-Error "Failed to fetch PR #$PRNumber"
                exit 1
            }
        }
        
        Write-Host "  🔀 Merging into '$currentBranch'..." -ForegroundColor Gray
        git merge "temp-pr-$PRNumber" --no-edit
        if ($LASTEXITCODE -ne 0) {
            Write-Host ""
            Write-Host "⚠️  Merge conflict detected!" -ForegroundColor Red
            Write-Host "  Please resolve conflicts manually and re-run the script with -SkipMerge" -ForegroundColor Yellow
            git merge --abort 2>$null
            git branch -D "temp-pr-$PRNumber" 2>$null
            exit 1
        }
        
        # Clean up temp branch
        git branch -D "temp-pr-$PRNumber" 2>$null
        
        Write-Host "  ✅ PR #$PRNumber merged into '$currentBranch'" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "⏭️  Skipping merge (assuming PR is already merged)" -ForegroundColor Yellow
}

# Step 3: Ensure state directory exists
$stateDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState"
if (-not (Test-Path $stateDir)) {
    New-Item -ItemType Directory -Path $stateDir -Force | Out-Null
    Write-Host "  📁 Created state directory: $stateDir" -ForegroundColor Gray
}

# Step 4: Build the prompt for Copilot CLI
$planTemplatePath = ".github/agents/pr/PLAN-TEMPLATE.md"

# Build platform instruction
$platformInstruction = if ($Platform) {
    "**Platform for testing:** $Platform"
} else {
    "**Platform for testing:** Determine the appropriate platform(s) based on the PR's affected code paths and the current host OS."
}

$prompt = @"
Review PR #$PRNumber using the pr agent workflow.

$platformInstruction

🚨 **CRITICAL - NEVER MODIFY GIT STATE:**
- NEVER run ``git checkout``, ``git switch``, ``git fetch``, ``git stash``, or ``git reset``
- NEVER run ``git push`` - you do NOT have permission to push anything
- You are ALWAYS on the correct branch already - the script handles this
- If the state file says "wrong branch", that's stale state - delete it and start fresh
- If you think you need to switch branches or push changes, you are WRONG - ask the user instead

**Instructions:**
1. Read the plan template at ``$planTemplatePath`` for the 4-phase workflow
2. Read ``.github/agents/pr.md`` for Phases 1-2 instructions
3. Follow ALL critical rules, especially:
   - STOP on environment blockers and ask before continuing
   - Use task agent for Gate verification
   - Run multi-model try-fix in Phase 3

**Start with Phase 1: Pre-Flight**
- Create state file: CustomAgentLogsTmp/PRState/pr-$PRNumber.md
- Gather context from PR #$PRNumber
- Proceed through all 4 phases

Begin the review now.
"@

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

if ($DryRun) {
    Write-Host "[DRY RUN] Would invoke Copilot CLI with:" -ForegroundColor Magenta
    Write-Host ""
    Write-Host "  Agent: pr" -ForegroundColor Gray
    Write-Host "  Mode: $(if ($Interactive) { 'Interactive (-i)' } else { 'Non-interactive (-p)' })" -ForegroundColor Gray
    Write-Host "  PR: #$PRNumber" -ForegroundColor Gray
    Write-Host "  Platform: $(if ($Platform) { $Platform } else { '(agent will determine)' })" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Prompt:" -ForegroundColor Gray
    Write-Host $prompt -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "To run for real, remove the -DryRun flag" -ForegroundColor Yellow
} else {
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║  PHASE 1: PR AGENT REVIEW                                 ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "PR Review Context:" -ForegroundColor Cyan
    Write-Host "  PR_NUMBER:      $PRNumber" -ForegroundColor White
    Write-Host "  PLATFORM:       $(if ($Platform) { $Platform } else { '(agent will determine)' })" -ForegroundColor White
    Write-Host "  STATE_FILE:     CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor White
    Write-Host "  PLAN_TEMPLATE:  $planTemplatePath" -ForegroundColor White
    Write-Host "  CURRENT_BRANCH: $(git branch --show-current)" -ForegroundColor White
    Write-Host "  PR_TITLE:       $($prInfo.title)" -ForegroundColor White
    Write-Host "  MODE:           $(if ($Interactive) { 'Interactive' } else { 'Non-interactive' })" -ForegroundColor White
    Write-Host ""
    Write-Host "Workflow:" -ForegroundColor Cyan
    Write-Host "  1. PR Agent Review (this phase)" -ForegroundColor White
    if ($RunFinalize) {
        Write-Host "  2. pr-finalize skill (queued)" -ForegroundColor White
    }
    if ($PostSummaryComment) {
        $phase3Label = "3. Post comments: agent summary"
        if ($RunFinalize) {
            $phase3Label += " + finalize"
        }
        Write-Host "  $phase3Label (queued)" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""
    
    # Build the copilot command arguments
    $copilotArgs = @(
        "--agent", "pr",
        "--stream", "on"  # Enable streaming for real-time output
    )
    
    # NOTE: --deny-tool does NOT work with --allow-all (allow-all takes precedence)
    # Branch switching prevention relies on agent instructions in pr.md only
    
    # Create log directory for this PR
    $prLogDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/copilot-logs"
    if (-not (Test-Path $prLogDir)) {
        New-Item -ItemType Directory -Path $prLogDir -Force | Out-Null
    }
    
    # Add logging options
    $copilotArgs += @("--log-dir", $prLogDir, "--log-level", "info")
    
    if ($Interactive) {
        # Interactive mode: -i to start with prompt
        $copilotArgs += @("-i", $prompt)
    } else {
        # Non-interactive mode (default): -p with --allow-all
        # Also save session to markdown for review
        $sessionFile = Join-Path $prLogDir "session-$(Get-Date -Format 'yyyyMMdd-HHmmss').md"
        $copilotArgs += @("-p", $prompt, "--allow-all", "--share", $sessionFile)
    }
    
    Write-Host "🚀 Starting Copilot CLI..." -ForegroundColor Yellow
    Write-Host ""
    
    # Invoke Copilot CLI
    & copilot @copilotArgs
    
    $exitCode = $LASTEXITCODE
    
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    if ($exitCode -eq 0) {
        Write-Host "✅ Copilot CLI completed successfully" -ForegroundColor Green
    } else {
        Write-Host "⚠️ Copilot CLI exited with code: $exitCode" -ForegroundColor Yellow
    }
    
    # Post-completion skills (only run if main agent completed successfully)
    if ($exitCode -eq 0) {
        
        # Restore tracked files to clean state before running post-completion skills.
        # Phase 1 (PR Agent) may have left the working tree dirty from try-fix attempts,
        # which can cause skill files to be missing or modified in subsequent phases.
        # NOTE: State files in CustomAgentLogsTmp/ are .gitignore'd and untracked,
        # so this won't touch them. Using HEAD to also restore deleted files.
        Write-Host ""
        Write-Host "🧹 Restoring working tree to clean state between phases..." -ForegroundColor Yellow
        git status --porcelain 2>$null | Set-Content "CustomAgentLogsTmp/PRState/phase1-exit-git-status.log" -ErrorAction SilentlyContinue
        git checkout HEAD -- . 2>&1 | Out-Null
        Write-Host "  ✅ Working tree restored" -ForegroundColor Green
        
        # Phase 2: Run pr-finalize skill if requested
        if ($RunFinalize) {
            Write-Host ""
            Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
            Write-Host "║  PHASE 2: PR-FINALIZE SKILL                               ║" -ForegroundColor Magenta
            Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
            Write-Host ""
            
            # Ensure output directory exists for finalize results
            $finalizeDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/pr-finalize"
            if (-not (Test-Path $finalizeDir)) {
                New-Item -ItemType Directory -Path $finalizeDir -Force | Out-Null
            }
            
            $finalizePrompt = "Run the pr-finalize skill for PR #$PRNumber. Verify the PR title and description match the actual implementation. Do NOT post a comment. Write your findings to CustomAgentLogsTmp/PRState/$PRNumber/pr-finalize/pr-finalize-summary.md (NOT the main state file pr-$PRNumber.md which contains phase data that must not be overwritten). If you recommend a new description, also write it to CustomAgentLogsTmp/PRState/$PRNumber/pr-finalize/recommended-description.md. If you have code review findings, also write them to CustomAgentLogsTmp/PRState/$PRNumber/pr-finalize/code-review.md."
            
            $finalizeArgs = @(
                "-p", $finalizePrompt,
                "--allow-all",
                "--stream", "on"
            )
            
            Write-Host "🔍 Running pr-finalize..." -ForegroundColor Yellow
            & copilot @finalizeArgs
            
            $finalizeExit = $LASTEXITCODE
            if ($finalizeExit -eq 0) {
                Write-Host "✅ pr-finalize completed" -ForegroundColor Green
            } else {
                Write-Host "⚠️ pr-finalize exited with code: $finalizeExit" -ForegroundColor Yellow
            }
        }
        
        # Phase 3: Post comments if requested
        # Runs scripts directly instead of via Copilot CLI to avoid:
        # - LLM creating its own broken version if skill files are missing
        # - Dirty tree from Phase 2 corrupting script files
        if ($PostSummaryComment) {
            Write-Host ""
            Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
            Write-Host "║  PHASE 3: POST COMMENTS                                   ║" -ForegroundColor Magenta
            Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
            Write-Host ""
            
            # Restore tracked files (including deleted ones) to clean state.
            Write-Host "🧹 Restoring working tree to clean state..." -ForegroundColor Yellow
            git status --porcelain 2>$null | Set-Content "CustomAgentLogsTmp/PRState/phase2-exit-git-status.log" -ErrorAction SilentlyContinue
            git checkout HEAD -- . 2>&1 | Out-Null
            Write-Host "  ✅ Working tree restored" -ForegroundColor Green
            
            # 3a: Post PR agent summary comment (from Phase 1 state file)
            $scriptPath = ".github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1"
            if (-not (Test-Path $scriptPath)) {
                Write-Host "⚠️ Script missing after checkout, attempting targeted recovery..." -ForegroundColor Yellow
                git checkout HEAD -- $scriptPath 2>&1 | Out-Null
            }
            if (Test-Path $scriptPath) {
                Write-Host "💬 Running post-ai-summary-comment.ps1 directly..." -ForegroundColor Yellow
                & $scriptPath -PRNumber $PRNumber
                
                $commentExit = $LASTEXITCODE
                if ($commentExit -eq 0) {
                    Write-Host "✅ Agent summary comment posted" -ForegroundColor Green
                } else {
                    Write-Host "⚠️ post-ai-summary-comment.ps1 exited with code: $commentExit" -ForegroundColor Yellow
                }
            } else {
                Write-Host "⚠️ Script not found at: $scriptPath" -ForegroundColor Yellow
                Write-Host "   Current directory: $(Get-Location)" -ForegroundColor Gray
                Write-Host "   Skipping agent summary comment." -ForegroundColor Gray
            }
            
            # 3b: Post PR finalize comment (from Phase 2 finalize results)
            if ($RunFinalize) {
                $finalizeScriptPath = ".github/skills/ai-summary-comment/scripts/post-pr-finalize-comment.ps1"
                if (-not (Test-Path $finalizeScriptPath)) {
                    Write-Host "⚠️ Finalize script missing, attempting targeted recovery..." -ForegroundColor Yellow
                    git checkout HEAD -- $finalizeScriptPath 2>&1 | Out-Null
                }
                if (Test-Path $finalizeScriptPath) {
                    Write-Host "💬 Running post-pr-finalize-comment.ps1 directly..." -ForegroundColor Yellow
                    & $finalizeScriptPath -PRNumber $PRNumber
                    
                    $finalizeCommentExit = $LASTEXITCODE
                    if ($finalizeCommentExit -eq 0) {
                        Write-Host "✅ Finalize comment posted" -ForegroundColor Green
                    } else {
                        Write-Host "⚠️ post-pr-finalize-comment.ps1 exited with code: $finalizeCommentExit" -ForegroundColor Yellow
                    }
                } else {
                    Write-Host "⚠️ Script not found at: $finalizeScriptPath" -ForegroundColor Yellow
                    Write-Host "   Skipping finalize comment." -ForegroundColor Gray
                }
            }
        }
        
        # Phase 4: Apply agent workflow labels
        $labelHelperPath = ".github/scripts/helpers/Update-AgentLabels.ps1"
        if (-not (Test-Path $labelHelperPath)) {
            Write-Host "⚠️ Label helper missing, attempting recovery..." -ForegroundColor Yellow
            git checkout HEAD -- $labelHelperPath 2>&1 | Out-Null
        }
        if (Test-Path $labelHelperPath) {
            . $labelHelperPath
            $stateFilePath = "CustomAgentLogsTmp/PRState/pr-$PRNumber.md"
            Invoke-AgentLabels -StateFile $stateFilePath -PRNumber $PRNumber
        } else {
            Write-Host "⚠️ Label helper not found at: $labelHelperPath — skipping labels" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "📝 State file: CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor Gray
Write-Host "📋 Plan template: $planTemplatePath" -ForegroundColor Gray
if (-not $DryRun) {
    Write-Host "📁 Copilot logs: CustomAgentLogsTmp/PRState/$PRNumber/copilot-logs/" -ForegroundColor Gray
    if (-not $Interactive) {
        Write-Host "📄 Session markdown: $sessionFile" -ForegroundColor Gray
    }
}
Write-Host ""

# NOTE: This cleanup targets CI/ADO agent environments where only this script's
# Copilot CLI processes should exist. On developer machines, this could potentially
# affect other Copilot processes (e.g., VS Code extension). The risk is low since
# this runs at script end, but be aware if running locally.
# Clean up orphaned copilot CLI processes that may hold stdout fd open
# IMPORTANT: Only target processes whose command line contains "copilot" to avoid
# accidentally terminating the ADO agent's own node process
Write-Host "🧹 Cleaning up child processes..." -ForegroundColor Gray
try {
    $myPid = $PID
    # Find node processes running copilot CLI (not the ADO agent's node process)
    $orphans = Get-Process -Name "node" -ErrorAction SilentlyContinue | Where-Object {
        $_.Id -ne $myPid -and
        (($_.Path -and $_.Path -match "copilot") -or
         ($_.CommandLine -and $_.CommandLine -match "copilot"))
    }
    # Also get any process literally named "copilot"
    $copilotProcs = Get-Process -Name "copilot" -ErrorAction SilentlyContinue
    $allOrphans = @($orphans) + @($copilotProcs) | Where-Object { $_ -ne $null } | Sort-Object Id -Unique
    if ($allOrphans.Count -gt 0) {
        Write-Host "  Stopping $($allOrphans.Count) orphaned process(es): $($allOrphans | ForEach-Object { "$($_.ProcessName)($($_.Id))" } | Join-String -Separator ', ')" -ForegroundColor Gray
        $allOrphans | Stop-Process -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "  No orphaned copilot processes found" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ⚠️ Cleanup warning: $_" -ForegroundColor Yellow
}

if ($LogFile) {
    Stop-Transcript | Out-Null
}
