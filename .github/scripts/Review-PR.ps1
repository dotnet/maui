<#
.SYNOPSIS
    Runs a PR review using Copilot CLI with skill-based prompts.

.DESCRIPTION
    Orchestrates a 4-step PR review by invoking Copilot CLI with skill prompts:
    
    Step 1: pr-review skill     - 4-phase review (Pre-Flight, Gate, Try-Fix, Report)
    Step 2: pr-finalize skill   - Verify PR title/description match implementation
    Step 3: ai-summary-comment  - Post unified AI Summary comment to the PR
    Step 4: Apply labels        - Apply agent labels based on review results

    The pr-review skill handles all git operations (cherry-pick, branch setup).
    This script only invokes Copilot CLI and applies labels.

.PARAMETER PRNumber
    The GitHub PR number to review

.PARAMETER Platform
    Platform for testing. Valid: android, ios, windows, maccatalyst

.PARAMETER DryRun
    Show what would be done without making changes

.PARAMETER LogFile
    Capture all output via Start-Transcript

.EXAMPLE
    .\Review-PR.ps1 -PRNumber 33687
    .\Review-PR.ps1 -PRNumber 33687 -Platform ios
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [int]$PRNumber,

    [Parameter(Mandatory = $false)]
    [ValidateSet('android', 'ios', 'windows', 'maccatalyst')]
    [string]$Platform,

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

    & copilot -p $Prompt --allow-all --stream on
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
