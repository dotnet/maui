<#
.SYNOPSIS
    Runs a PR review using Copilot CLI and the PR Agent workflow.

.DESCRIPTION
    This script invokes Copilot CLI to perform a comprehensive 5-phase PR review:
    
    Phase 1: Pre-Flight - Context gathering
    Phase 2: Tests - Verify test existence
    Phase 3: Gate - Verify tests catch the bug
    Phase 4: Fix - Multi-model exploration of alternatives
    Phase 5: Report - Final recommendation
    
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
    [switch]$RunFinalize
)

$ErrorActionPreference = 'Stop'

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

**Instructions:**
1. Read the plan template at ``$planTemplatePath`` for the 5-phase workflow
2. Read ``.github/agents/pr.md`` for Phases 1-3 instructions
3. Follow ALL critical rules, especially:
   - STOP on environment blockers and ask before continuing
   - Use task agent for Gate verification
   - Run multi-model try-fix in Phase 4

**Start with Phase 1: Pre-Flight**
- Create state file: CustomAgentLogsTmp/PRState/pr-$PRNumber.md
- Gather context from PR #$PRNumber
- Proceed through all 5 phases

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
        Write-Host "  3. ai-summary-comment skill (queued)" -ForegroundColor White
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
        
        # Phase 2: Run pr-finalize skill if requested
        if ($RunFinalize) {
            Write-Host ""
            Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
            Write-Host "║  PHASE 2: PR-FINALIZE SKILL                               ║" -ForegroundColor Magenta
            Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
            Write-Host ""
            
            $finalizePrompt = "Run the pr-finalize skill for PR #$PRNumber. Verify the PR title and description match the actual implementation. Do NOT post a comment - just update the state file at CustomAgentLogsTmp/PRState/pr-$PRNumber.md with your findings."
            
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
        
        # Phase 3: Run ai-summary-comment skill if requested (posts combined results)
        if ($PostSummaryComment) {
            Write-Host ""
            Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
            Write-Host "║  PHASE 3: POST SUMMARY COMMENT                            ║" -ForegroundColor Magenta
            Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
            Write-Host ""
            
            $commentPrompt = "Use the ai-summary-comment skill to post a review comment on PR #$PRNumber. Read the state file at CustomAgentLogsTmp/PRState/pr-$PRNumber.md which contains results from both the PR agent review and pr-finalize phases. Post a single combined summary comment to the PR."
            
            $commentArgs = @(
                "-p", $commentPrompt,
                "--allow-all",
                "--stream", "on"
            )
            
            Write-Host "💬 Posting summary comment..." -ForegroundColor Yellow
            & copilot @commentArgs
            
            $commentExit = $LASTEXITCODE
            if ($commentExit -eq 0) {
                Write-Host "✅ Summary comment posted" -ForegroundColor Green
            } else {
                Write-Host "⚠️ ai-summary-comment exited with code: $commentExit" -ForegroundColor Yellow
            }
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
