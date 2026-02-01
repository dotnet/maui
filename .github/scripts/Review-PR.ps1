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
    - Optionally checks out the PR branch
    - Creates the state directory
    - Invokes Copilot CLI with the pr agent

.PARAMETER PRNumber
    The GitHub PR number to review (e.g., 33687)

.PARAMETER Platform
    The platform to use for testing. Default is 'android'.
    Valid values: android, ios, windows, maccatalyst

.PARAMETER SkipCheckout
    If specified, skips checking out the PR branch (useful if already on the branch)

.PARAMETER Interactive
    If specified, starts Copilot in interactive mode with the prompt (default).
    Use -NoInteractive for non-interactive mode that exits after completion.

.PARAMETER NoInteractive
    If specified, runs in non-interactive mode (exits after completion).
    Requires --allow-all for tool permissions.

.PARAMETER DryRun
    If specified, shows what would be done without making changes

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687
    Reviews PR #33687 interactively using the default platform (android)

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687 -Platform ios -SkipCheckout
    Reviews PR #33687 on iOS without checking out, in interactive mode

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687 -NoInteractive
    Reviews PR #33687 in non-interactive mode (exits after completion)

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
    [switch]$SkipCheckout,

    [Parameter(Mandatory = $false)]
    [switch]$NoInteractive,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
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

# Step 2: Checkout PR (unless skipped)
if (-not $SkipCheckout) {
    Write-Host ""
    Write-Host "📥 Checking out PR #$PRNumber..." -ForegroundColor Yellow
    
    if ($DryRun) {
        Write-Host "  [DRY RUN] Would run: gh pr checkout $PRNumber" -ForegroundColor Magenta
    } else {
        gh pr checkout $PRNumber
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Failed to checkout PR #$PRNumber"
            exit 1
        }
        Write-Host "  ✅ Checked out PR branch" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "⏭️  Skipping checkout (using current branch)" -ForegroundColor Yellow
    $currentBranch = git branch --show-current
    Write-Host "  Current branch: $currentBranch" -ForegroundColor Gray
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
1. Read the plan template at `$planTemplatePath` for the 5-phase workflow
2. Read `.github/agents/pr.md` for Phases 1-3 instructions
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
    Write-Host "  Mode: $(if ($NoInteractive) { 'Non-interactive (-p)' } else { 'Interactive (-i)' })" -ForegroundColor Gray
    Write-Host "  PR: #$PRNumber" -ForegroundColor Gray
    Write-Host "  Platform: $(if ($Platform) { $Platform } else { '(agent will determine)' })" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Prompt:" -ForegroundColor Gray
    Write-Host $prompt -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "To run for real, remove the -DryRun flag" -ForegroundColor Yellow
} else {
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║  LAUNCHING COPILOT CLI                                    ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "PR Review Context:" -ForegroundColor Cyan
    Write-Host "  PR_NUMBER:      $PRNumber" -ForegroundColor White
    Write-Host "  PLATFORM:       $(if ($Platform) { $Platform } else { '(agent will determine)' })" -ForegroundColor White
    Write-Host "  STATE_FILE:     CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor White
    Write-Host "  PLAN_TEMPLATE:  $planTemplatePath" -ForegroundColor White
    Write-Host "  CURRENT_BRANCH: $(git branch --show-current)" -ForegroundColor White
    Write-Host "  PR_TITLE:       $($prInfo.title)" -ForegroundColor White
    Write-Host "  MODE:           $(if ($NoInteractive) { 'Non-interactive' } else { 'Interactive' })" -ForegroundColor White
    Write-Host ""
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""
    
    # Build the copilot command arguments
    $copilotArgs = @(
        "--agent", "pr",
        "--stream", "on"  # Enable streaming for real-time output
    )
    
    # Deny branch-switching commands - agent should work on current branch
    $copilotArgs += @(
        "--deny-tool", "shell(git checkout*)",
        "--deny-tool", "shell(git switch*)",
        "--deny-tool", "shell(git stash*)",
        "--deny-tool", "shell(gh pr checkout*)"
    )
    
    # Create log directory for this PR
    $prLogDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/copilot-logs"
    if (-not (Test-Path $prLogDir)) {
        New-Item -ItemType Directory -Path $prLogDir -Force | Out-Null
    }
    
    # Add logging options
    $copilotArgs += @("--log-dir", $prLogDir, "--log-level", "info")
    
    if ($NoInteractive) {
        # Non-interactive mode: -p with --allow-all
        # Also save session to markdown for review
        $sessionFile = Join-Path $prLogDir "session-$(Get-Date -Format 'yyyyMMdd-HHmmss').md"
        $copilotArgs += @("-p", $prompt, "--allow-all", "--share", $sessionFile)
    } else {
        # Interactive mode: -i to start with prompt
        $copilotArgs += @("-i", $prompt)
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
}

Write-Host ""
Write-Host "📝 State file: CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor Gray
Write-Host "📋 Plan template: $planTemplatePath" -ForegroundColor Gray
if (-not $DryRun) {
    Write-Host "📁 Copilot logs: CustomAgentLogsTmp/PRState/$PRNumber/copilot-logs/" -ForegroundColor Gray
    if ($NoInteractive) {
        Write-Host "📄 Session markdown: $sessionFile" -ForegroundColor Gray
    }
}
Write-Host ""
