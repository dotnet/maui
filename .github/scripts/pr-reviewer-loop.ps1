<#
.SYNOPSIS
    Orchestrates multi-agent PR review by invoking phase agents sequentially.

.DESCRIPTION
    This script manages the PR review workflow by:
    1. Initializing a state file to track progress
    2. Invoking each phase agent with the state file
    3. Checking phase status before proceeding
    4. Stopping if any gate fails

.PARAMETER PRNumber
    The GitHub PR number to review.

.PARAMETER Platform
    The platform to test on (android, ios). Default: android

.PARAMETER StateFile
    Optional custom path for state file. Default: pr-{PRNumber}-review.md in repo root

.PARAMETER StartPhase
    Optional phase to start from (gate, analyze, compare, regression, report). Default: gate

.EXAMPLE
    pwsh .github/scripts/pr-reviewer-loop.ps1 -PRNumber 12345 -Platform android

.EXAMPLE
    pwsh .github/scripts/pr-reviewer-loop.ps1 -PRNumber 12345 -StartPhase compare
#>

param(
    [Parameter(Mandatory=$true)]
    [int]$PRNumber,

    [ValidateSet("android", "ios")]
    [string]$Platform = "android",

    [string]$StateFile,

    [ValidateSet("gate", "analyze", "compare", "regression", "report")]
    [string]$StartPhase = "gate"
)

$ErrorActionPreference = "Stop"

# Determine state file location
if (-not $StateFile) {
    $StateFile = "pr-$PRNumber-review.md"
}

$RepoRoot = git rev-parse --show-toplevel
$StateFilePath = Join-Path $RepoRoot $StateFile

Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║           PR Review Orchestrator                          ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  PR:        #$PRNumber                                        " -ForegroundColor Cyan
Write-Host "║  Platform:  $Platform                                         " -ForegroundColor Cyan
Write-Host "║  State:     $StateFile                                    " -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Phase definitions with their required tools
$Phases = @(
    @{ 
        Name = "gate"
        Agent = "pr-reviewer-gate"
        Description = "Verify tests catch the bug"
        Tools = @("shell", "read", "write")
    },
    @{ 
        Name = "analyze"
        Agent = "pr-reviewer-analyze"
        Description = "Independent analysis"
        Tools = @("shell", "read", "grep", "glob")
    },
    @{ 
        Name = "compare"
        Agent = "pr-reviewer-compare"
        Description = "Compare approaches"
        Tools = @("shell", "read", "write", "grep", "glob")
    },
    @{ 
        Name = "regression"
        Agent = "pr-reviewer-regression"
        Description = "Regression testing"
        Tools = @("shell", "read", "grep", "glob")
    },
    @{ 
        Name = "report"
        Agent = "pr-reviewer-report"
        Description = "Generate final report"
        Tools = @("read", "write")
    }
)

# Initialize state file if it doesn't exist or starting from gate
function Initialize-StateFile {
    $prInfo = gh pr view $PRNumber --json title,body,url | ConvertFrom-Json
    
    $stateContent = @"
# PR Review State: #$PRNumber

**Title**: $($prInfo.title)
**URL**: $($prInfo.url)
**Platform**: $Platform
**Started**: $(Get-Date -Format "yyyy-MM-ddTHH:mm:ssZ")
**Current Phase**: gate

---

## Gate
**Status**: PENDING ⏳

---

## Analysis
**Status**: PENDING ⏳

---

## Comparison
**Status**: PENDING ⏳

---

## Regression
**Status**: PENDING ⏳

---

## Report
**Status**: PENDING ⏳

"@

    Set-Content -Path $StateFilePath -Value $stateContent -Encoding UTF8
    Write-Host "✅ Initialized state file: $StateFilePath" -ForegroundColor Green
}

# Check if a phase passed by reading state file
function Test-PhaseStatus {
    param([string]$PhaseName)
    
    if (-not (Test-Path $StateFilePath)) {
        return $false
    }
    
    $content = Get-Content $StateFilePath -Raw
    
    # Look for Status: PASSED or COMPLETED in the phase section
    $pattern = "## $PhaseName[\s\S]*?\*\*Status\*\*:\s*(PASSED|COMPLETED)"
    if ($content -match $pattern) {
        return $true
    }
    
    # Check for FAILED
    $failPattern = "## $PhaseName[\s\S]*?\*\*Status\*\*:\s*FAILED"
    if ($content -match $failPattern) {
        Write-Host "❌ Phase '$PhaseName' FAILED" -ForegroundColor Red
        return $false
    }
    
    return $false
}

# Invoke an agent with a prompt
function Invoke-Agent {
    param(
        [string]$AgentName,
        [string]$Prompt,
        [string[]]$AllowTools = @()
    )
    
    Write-Host "`n" -NoNewline
    Write-Host "┌─────────────────────────────────────────────────────────────" -ForegroundColor Magenta
    Write-Host "│ 📤 INVOKING AGENT: $AgentName" -ForegroundColor Magenta
    Write-Host "├─────────────────────────────────────────────────────────────" -ForegroundColor Magenta
    Write-Host "│ Prompt:" -ForegroundColor Magenta
    $Prompt -split "`n" | ForEach-Object { Write-Host "│   $_" -ForegroundColor Gray }
    Write-Host "│ Tools: $($AllowTools -join ', ')" -ForegroundColor Magenta
    Write-Host "└─────────────────────────────────────────────────────────────" -ForegroundColor Magenta
    
    $startTime = Get-Date
    Write-Host "`n⏳ Agent started at: $($startTime.ToString('HH:mm:ss'))" -ForegroundColor Yellow
    
    # Build tool allow arguments
    $toolArgs = @()
    foreach ($tool in $AllowTools) {
        $toolArgs += "--allow-tool"
        $toolArgs += $tool
    }
    
    $toolArgsDisplay = ($AllowTools | ForEach-Object { "--allow-tool $_" }) -join " "
    Write-Host "📋 Command: copilot --agent=$AgentName $toolArgsDisplay -p `"...`"" -ForegroundColor Cyan
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    
    # Run copilot with specified tools
    $result = & copilot --agent=$AgentName @toolArgs -p $Prompt 2>&1
    
    $endTime = Get-Date
    $duration = $endTime - $startTime
    
    Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "⏱️ Agent finished at: $($endTime.ToString('HH:mm:ss')) (Duration: $($duration.ToString('mm\:ss')))" -ForegroundColor Yellow
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "⚠️ Agent returned non-zero exit code: $LASTEXITCODE" -ForegroundColor Yellow
    } else {
        Write-Host "✅ Agent exited successfully" -ForegroundColor Green
    }
    
    return $result
}

# Update current phase in state file
function Update-CurrentPhase {
    param([string]$PhaseName)
    
    $content = Get-Content $StateFilePath -Raw
    $content = $content -replace '\*\*Current Phase\*\*: \w+', "**Current Phase**: $PhaseName"
    Set-Content -Path $StateFilePath -Value $content -Encoding UTF8
}

# Main orchestration loop
function Start-PRReview {
    # Find starting index
    $startIndex = 0
    for ($i = 0; $i -lt $Phases.Count; $i++) {
        if ($Phases[$i].Name -eq $StartPhase) {
            $startIndex = $i
            break
        }
    }
    
    # Initialize state file if starting from gate
    if ($StartPhase -eq "gate") {
        Initialize-StateFile
    } elseif (-not (Test-Path $StateFilePath)) {
        Write-Host "❌ State file not found: $StateFilePath" -ForegroundColor Red
        Write-Host "   Run with -StartPhase gate to initialize" -ForegroundColor Yellow
        exit 1
    }
    
    # Run each phase
    for ($i = $startIndex; $i -lt $Phases.Count; $i++) {
        $phase = $Phases[$i]
        
        Write-Host "`n" -NoNewline
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host "  Phase: $($phase.Name.ToUpper()) - $($phase.Description)" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
        
        Update-CurrentPhase -PhaseName $phase.Name
        
        # Build prompt for this phase
        $prompt = @"
Read the current PR review state from: $StateFilePath

PR Number: $PRNumber
Platform: $Platform

Complete the $($phase.Name) phase and update the state file with your results.
"@
        
        # Invoke the agent with its allowed tools
        $output = Invoke-Agent -AgentName $phase.Agent -Prompt $prompt -AllowTools $phase.Tools
        
        # Display output
        Write-Host $output
        
        # Show state file section after agent completes
        Write-Host "`n┌─────────────────────────────────────────────────────────────" -ForegroundColor Blue
        Write-Host "│ 📄 STATE FILE - $($phase.Name.ToUpper()) SECTION" -ForegroundColor Blue
        Write-Host "└─────────────────────────────────────────────────────────────" -ForegroundColor Blue
        
        if (Test-Path $StateFilePath) {
            $stateContent = Get-Content $StateFilePath -Raw
            # Extract just this phase's section
            $phasePattern = "## $($phase.Name)[\s\S]*?(?=---|\z)"
            if ($stateContent -match $phasePattern) {
                Write-Host $Matches[0] -ForegroundColor DarkCyan
            } else {
                Write-Host "(Could not find $($phase.Name) section)" -ForegroundColor DarkGray
            }
        } else {
            Write-Host "(State file not found)" -ForegroundColor DarkGray
        }
        Write-Host "─────────────────────────────────────────────────────────────" -ForegroundColor Blue
        
        # Check if phase passed (except for report which is always last)
        if ($phase.Name -ne "report") {
            # Give user a moment to review
            Write-Host "`n⏳ Checking phase status..." -ForegroundColor Yellow
            Start-Sleep -Seconds 2
            
            if (-not (Test-PhaseStatus -PhaseName $phase.Name)) {
                # Check if it's a hard failure (gate) or just not completed
                $content = Get-Content $StateFilePath -Raw
                if ($content -match "## $($phase.Name)[\s\S]*?\*\*Status\*\*:\s*FAILED") {
                    Write-Host "`n❌ Phase '$($phase.Name)' FAILED - stopping review" -ForegroundColor Red
                    Write-Host "   See state file for details: $StateFilePath" -ForegroundColor Yellow
                    exit 1
                } else {
                    Write-Host "`n⚠️ Phase '$($phase.Name)' status unclear - check state file" -ForegroundColor Yellow
                    Write-Host "   State file: $StateFilePath" -ForegroundColor Gray
                    Write-Host "   Continue? (y/n): " -NoNewline
                    $continue = Read-Host
                    if ($continue -ne "y") {
                        exit 1
                    }
                }
            } else {
                Write-Host "✅ Phase '$($phase.Name)' completed successfully" -ForegroundColor Green
            }
        }
    }
    
    Write-Host "`n" -NoNewline
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║           PR Review Complete! ✅                          ║" -ForegroundColor Green
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
    Write-Host "║  State file: $StateFile                                   " -ForegroundColor Green
    Write-Host "║  Review the Report section for final recommendation       ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
}

# Run the review
Start-PRReview
