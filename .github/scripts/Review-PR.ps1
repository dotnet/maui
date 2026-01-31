<#
.SYNOPSIS
    Prepares the environment for a PR review using the PR Agent workflow.

.DESCRIPTION
    This script prepares the environment for a comprehensive 5-phase PR review:
    
    Phase 1: Pre-Flight - Context gathering
    Phase 2: Tests - Verify test existence
    Phase 3: Gate - Verify tests catch the bug
    Phase 4: Fix - Multi-model exploration of alternatives
    Phase 5: Report - Final recommendation
    
    Since Copilot CLI is interactive, this script:
    - Validates prerequisites (gh CLI, PR exists)
    - Optionally checks out the PR branch
    - Creates the state directory
    - Outputs structured context for the agent to use
    
    Run this script, then ask Copilot CLI to "review PR #XXXXX using the pr agent"

.PARAMETER PRNumber
    The GitHub PR number to review (e.g., 33687)

.PARAMETER Platform
    The platform to use for testing. Default is 'android'.
    Valid values: android, ios, windows, maccatalyst

.PARAMETER SkipCheckout
    If specified, skips checking out the PR branch (useful if already on the branch)

.PARAMETER DryRun
    If specified, shows what would be done without making changes

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687
    Prepares to review PR #33687 using the default platform (android)

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687 -Platform ios -SkipCheckout
    Prepares to review PR #33687 on iOS without checking out

.NOTES
    Prerequisites:
    - GitHub CLI (gh) installed and authenticated
    - For testing: Appropriate platform tools (Appium, emulators, etc.)
    
    After running this script, tell Copilot CLI:
    "Review PR #XXXXX using the pr agent. Platform: <platform>"
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [ValidateSet('android', 'ios', 'windows', 'maccatalyst')]
    [string]$Platform = 'android',

    [Parameter(Mandatory = $false)]
    [switch]$SkipCheckout,

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
Write-Host "║  Platform:  $Platform                                        ║" -ForegroundColor Cyan
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

# Check Copilot CLI - informational only
Write-Host "  ℹ️  Note: After this script, tell Copilot CLI to review the PR" -ForegroundColor Gray

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

Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

if ($DryRun) {
    Write-Host "[DRY RUN] Would prepare environment with:" -ForegroundColor Magenta
    Write-Host "  PR: #$PRNumber" -ForegroundColor Gray
    Write-Host "  Platform: $Platform" -ForegroundColor Gray
    Write-Host "  State file: CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor Gray
    Write-Host ""
    Write-Host "To run for real, remove the -DryRun flag" -ForegroundColor Yellow
} else {
    # Output structured context that can be used by the agent
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║  ENVIRONMENT READY                                        ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    Write-Host ""
    Write-Host "PR Review Context:" -ForegroundColor Cyan
    Write-Host "  PR_NUMBER:      $PRNumber" -ForegroundColor White
    Write-Host "  PLATFORM:       $Platform" -ForegroundColor White
    Write-Host "  STATE_FILE:     CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor White
    Write-Host "  PLAN_TEMPLATE:  $planTemplatePath" -ForegroundColor White
    Write-Host "  CURRENT_BRANCH: $(git branch --show-current)" -ForegroundColor White
    Write-Host "  PR_TITLE:       $($prInfo.title)" -ForegroundColor White
    Write-Host ""
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "Next step - Tell Copilot CLI:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  Review PR #$PRNumber using the pr agent. Platform: $Platform" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Or for the full workflow:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  @pr Review PR #$PRNumber following .github/agents/pr/PLAN-TEMPLATE.md" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host ""
Write-Host "📝 State file will be at: CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor Gray
Write-Host "📋 Plan template at: $planTemplatePath" -ForegroundColor Gray
Write-Host ""
