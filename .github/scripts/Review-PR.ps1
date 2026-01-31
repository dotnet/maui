<#
.SYNOPSIS
    Runs the PR Agent workflow to review a GitHub Pull Request using Copilot CLI.

.DESCRIPTION
    This script invokes GitHub Copilot CLI with the PR agent to perform a comprehensive
    5-phase review of a pull request:
    
    Phase 1: Pre-Flight - Context gathering
    Phase 2: Tests - Verify test existence
    Phase 3: Gate - Verify tests catch the bug
    Phase 4: Fix - Multi-model exploration of alternatives
    Phase 5: Report - Final recommendation
    
    The script uses the plan template at .github/agents/pr/PLAN-TEMPLATE.md.

.PARAMETER PRNumber
    The GitHub PR number to review (e.g., 33687)

.PARAMETER Platform
    The platform to use for testing. Default is 'android'.
    Valid values: android, ios, windows, maccatalyst

.PARAMETER SkipCheckout
    If specified, skips checking out the PR branch (useful if already on the branch)

.PARAMETER DryRun
    If specified, shows what would be done without actually invoking Copilot CLI

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687
    Reviews PR #33687 using the default platform (android)

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687 -Platform ios
    Reviews PR #33687 using iOS for testing

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687 -SkipCheckout
    Reviews PR #33687 without checking out (assumes already on correct branch)

.NOTES
    Prerequisites:
    - GitHub Copilot CLI installed and authenticated
    - GitHub CLI (gh) installed and authenticated
    - For testing: Appropriate platform tools (Appium, emulators, etc.)
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

# Check Copilot CLI
$copilotVersion = ghcs --version 2>$null
if (-not $copilotVersion) {
    # Try alternative command
    $copilotVersion = gh copilot --version 2>$null
    if (-not $copilotVersion) {
        Write-Error "GitHub Copilot CLI is not installed. Install with: gh extension install github/gh-copilot"
        exit 1
    }
}
Write-Host "  ✅ Copilot CLI available" -ForegroundColor Green

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

$prompt = @"
Review PR #$PRNumber using the PR agent workflow.

**Instructions:**
1. Read the plan template at `$planTemplatePath` for the 5-phase workflow
2. Read `.github/agents/pr.md` for Phases 1-3 instructions
3. Follow ALL critical rules, especially:
   - STOP on environment blockers and ask before continuing
   - Use task agent for Gate verification
   - Run multi-model try-fix in Phase 4

**Platform for testing:** $Platform

**Start with Phase 1: Pre-Flight**
- Create state file: CustomAgentLogsTmp/PRState/pr-$PRNumber.md
- Gather context from PR #$PRNumber
- Proceed through all 5 phases

Begin the review now.
"@

Write-Host ""
Write-Host "🤖 Launching Copilot CLI with PR agent..." -ForegroundColor Yellow
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
Write-Host ""

if ($DryRun) {
    Write-Host "[DRY RUN] Would invoke Copilot CLI with prompt:" -ForegroundColor Magenta
    Write-Host ""
    Write-Host $prompt -ForegroundColor Gray
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    Write-Host ""
    Write-Host "To run for real, remove the -DryRun flag" -ForegroundColor Yellow
} else {
    # Invoke Copilot CLI
    # Note: The exact command may vary based on Copilot CLI version
    # Using ghcs (GitHub Copilot Shell) or gh copilot
    
    # Write prompt to temp file to avoid escaping issues
    $promptFile = Join-Path $env:TEMP "copilot-pr-review-prompt.txt"
    $prompt | Out-File -FilePath $promptFile -Encoding utf8
    
    Write-Host "Prompt saved to: $promptFile" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Run the following command to start the review:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  ghcs `"Review PR #$PRNumber using the pr agent. Platform: $Platform`"" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Or copy this full prompt:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host $prompt -ForegroundColor White
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor DarkGray
    
    # Optionally, try to invoke directly (may not work in all environments)
    # Uncomment below to auto-launch:
    # ghcs $prompt
}

Write-Host ""
Write-Host "📝 State file will be at: CustomAgentLogsTmp/PRState/pr-$PRNumber.md" -ForegroundColor Gray
Write-Host "📋 Plan template at: $planTemplatePath" -ForegroundColor Gray
Write-Host ""
