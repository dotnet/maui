<#
.SYNOPSIS
    Shared helper module for applying agent workflow labels to PRs.

.DESCRIPTION
    Provides functions to apply outcome labels (mutually exclusive) and signal labels
    (additive) to PRs based on agent workflow results. All labels use the s/agent-* prefix.

    Label Categories:
    - Outcome (mutually exclusive): s/agent-approved, s/agent-changes-requested, s/agent-review-incomplete
    - Signal (additive): s/agent-gate-passed, s/agent-gate-failed, s/agent-fix-optimal
    - Base: s/agent-reviewed (always applied on completed runs)
    - Manual: s/agent-fix-implemented (applied by maintainers, never auto-applied)

.NOTES
    All functions use gh api REST calls. Label failures are warnings, never fatal errors.
#>

# ============================================================
# Label Definitions
# ============================================================

$script:OutcomeLabels = @{
    Approved        = "s/agent-approved"
    ChangesRequested = "s/agent-changes-requested"
    ReviewIncomplete = "s/agent-review-incomplete"
}

$script:SignalLabels = @{
    GatePassed  = "s/agent-gate-passed"
    GateFailed  = "s/agent-gate-failed"
    FixOptimal  = "s/agent-fix-optimal"
}

$script:BaseLabel = "s/agent-reviewed"

# Label definitions for auto-creation (Ensure-LabelExists)
$script:LabelDefinitions = @{
    "s/agent-reviewed"           = @{ Color = "2E7D32"; Description = "PR was reviewed by AI agent workflow" }
    "s/agent-approved"           = @{ Color = "2E7D32"; Description = "AI agent recommends approval" }
    "s/agent-changes-requested"  = @{ Color = "E65100"; Description = "AI agent recommends changes" }
    "s/agent-review-incomplete"  = @{ Color = "B71C1C"; Description = "AI agent could not complete all phases" }
    "s/agent-gate-passed"        = @{ Color = "4CAF50"; Description = "AI verified tests catch the bug" }
    "s/agent-gate-failed"        = @{ Color = "FF9800"; Description = "AI could not verify tests catch the bug" }
    "s/agent-fix-optimal"        = @{ Color = "66BB6A"; Description = "AI confirms PR fix is the best among candidates" }
    "s/agent-fix-implemented"    = @{ Color = "7B1FA2"; Description = "PR author implemented the agent suggested fix" }
}

# ============================================================
# Helper Functions
# ============================================================

function Get-PRExistingLabels {
    param([string]$PRNumber)
    
    $labels = gh pr view $PRNumber --repo dotnet/maui --json labels --jq '.labels[].name' 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ⚠️  Failed to fetch existing labels for PR #$PRNumber" -ForegroundColor Yellow
        return @()
    }
    return @($labels | Where-Object { $_ })
}

function Add-Label {
    param([string]$PRNumber, [string]$Label)
    
    $result = gh api "repos/dotnet/maui/issues/$PRNumber/labels" --method POST -f "labels[]=$Label" 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ⚠️  Failed to add label: $Label ($result)" -ForegroundColor Yellow
        return $false
    }
    return $true
}

function Remove-Label {
    param([string]$PRNumber, [string]$Label)
    
    # URL-encode the label name (handles / in s/agent-*)
    $encodedLabel = [System.Uri]::EscapeDataString($Label)
    gh api "repos/dotnet/maui/issues/$PRNumber/labels/$encodedLabel" --method DELETE 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ⚠️  Failed to remove label: $Label" -ForegroundColor Yellow
        return $false
    }
    return $true
}

# ============================================================
# Public Functions
# ============================================================

function Ensure-LabelExists {
    <#
    .SYNOPSIS
        Creates a label in the repo if it doesn't already exist.
    #>
    param([string]$Label)
    
    if (-not $script:LabelDefinitions.ContainsKey($Label)) {
        Write-Host "  ⚠️  Unknown label: $Label" -ForegroundColor Yellow
        return
    }
    
    $def = $script:LabelDefinitions[$Label]
    
    # Check if label exists
    $existing = gh label list --repo dotnet/maui --search $Label --limit 1 --json name --jq '.[].name' 2>$null
    if ($existing -eq $Label) {
        return  # Already exists
    }
    
    Write-Host "  📌 Creating label: $Label" -ForegroundColor Cyan
    gh label create $Label --repo dotnet/maui --color $def.Color --description $def.Description --force 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ⚠️  Failed to create label: $Label" -ForegroundColor Yellow
    }
}

function Update-AgentOutcomeLabel {
    <#
    .SYNOPSIS
        Applies exactly one outcome label and removes conflicting ones.
    .PARAMETER Outcome
        One of: 'Approved', 'ChangesRequested', 'ReviewIncomplete'
    #>
    param(
        [Parameter(Mandatory)]
        [ValidateSet('Approved', 'ChangesRequested', 'ReviewIncomplete')]
        [string]$Outcome,
        
        [Parameter(Mandatory)]
        [string]$PRNumber
    )
    
    $labelToAdd = $script:OutcomeLabels[$Outcome]
    $labelsToRemove = $script:OutcomeLabels.Values | Where-Object { $_ -ne $labelToAdd }
    
    $existingLabels = Get-PRExistingLabels -PRNumber $PRNumber
    
    # Remove conflicting outcome labels
    foreach ($label in $labelsToRemove) {
        if ($existingLabels -contains $label) {
            Write-Host "  🔄 Removing: $label" -ForegroundColor Yellow
            Remove-Label -PRNumber $PRNumber -Label $label | Out-Null
        }
    }
    
    # Add the outcome label
    if ($existingLabels -notcontains $labelToAdd) {
        Write-Host "  ✅ Adding: $labelToAdd" -ForegroundColor Green
        Add-Label -PRNumber $PRNumber -Label $labelToAdd | Out-Null
    } else {
        Write-Host "  ℹ️  Already has: $labelToAdd" -ForegroundColor Gray
    }
}

function Update-AgentSignalLabel {
    <#
    .SYNOPSIS
        Adds or removes a signal label. For mutually exclusive pairs (gate-passed/gate-failed),
        adding one removes the other.
    .PARAMETER Signal
        One of: 'GatePassed', 'GateFailed', 'FixOptimal'
    #>
    param(
        [Parameter(Mandatory)]
        [ValidateSet('GatePassed', 'GateFailed', 'FixOptimal')]
        [string]$Signal,
        
        [Parameter(Mandatory)]
        [string]$PRNumber,
        
        [switch]$Remove
    )
    
    $label = $script:SignalLabels[$Signal]
    $existingLabels = Get-PRExistingLabels -PRNumber $PRNumber
    
    if ($Remove) {
        if ($existingLabels -contains $label) {
            Write-Host "  🔄 Removing: $label" -ForegroundColor Yellow
            Remove-Label -PRNumber $PRNumber -Label $label | Out-Null
        }
        return
    }
    
    # For gate labels, they are mutually exclusive with each other
    if ($Signal -eq 'GatePassed' -and $existingLabels -contains $script:SignalLabels['GateFailed']) {
        Write-Host "  🔄 Removing: $($script:SignalLabels['GateFailed'])" -ForegroundColor Yellow
        Remove-Label -PRNumber $PRNumber -Label $script:SignalLabels['GateFailed'] | Out-Null
    }
    elseif ($Signal -eq 'GateFailed' -and $existingLabels -contains $script:SignalLabels['GatePassed']) {
        Write-Host "  🔄 Removing: $($script:SignalLabels['GatePassed'])" -ForegroundColor Yellow
        Remove-Label -PRNumber $PRNumber -Label $script:SignalLabels['GatePassed'] | Out-Null
    }
    
    if ($existingLabels -notcontains $label) {
        Write-Host "  ✅ Adding: $label" -ForegroundColor Green
        Add-Label -PRNumber $PRNumber -Label $label | Out-Null
    } else {
        Write-Host "  ℹ️  Already has: $label" -ForegroundColor Gray
    }
}

function Update-AgentReviewedLabel {
    <#
    .SYNOPSIS
        Applies the base s/agent-reviewed label to mark the PR as agent-reviewed.
    #>
    param(
        [Parameter(Mandatory)]
        [string]$PRNumber
    )
    
    $existingLabels = Get-PRExistingLabels -PRNumber $PRNumber
    
    if ($existingLabels -notcontains $script:BaseLabel) {
        Write-Host "  ✅ Adding: $($script:BaseLabel)" -ForegroundColor Green
        Add-Label -PRNumber $PRNumber -Label $script:BaseLabel | Out-Null
    } else {
        Write-Host "  ℹ️  Already has: $($script:BaseLabel)" -ForegroundColor Gray
    }
}

function Invoke-AgentLabels {
    <#
    .SYNOPSIS
        Main entry point: parses a state file and applies all appropriate labels.
    .PARAMETER StateFile
        Path to the PR state markdown file (e.g., CustomAgentLogsTmp/PRState/pr-33528.md)
    .PARAMETER PRNumber
        The PR number to apply labels to.
    #>
    param(
        [Parameter(Mandatory)]
        [string]$StateFile,
        
        [Parameter(Mandatory)]
        [string]$PRNumber
    )
    
    if (-not (Test-Path $StateFile)) {
        Write-Host "  ⚠️  State file not found: $StateFile" -ForegroundColor Yellow
        return
    }
    
    $content = Get-Content $StateFile -Raw
    
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Blue
    Write-Host "║  PHASE 4: APPLY LABELS                                    ║" -ForegroundColor Blue
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Blue
    Write-Host ""
    Write-Host "🏷️  Applying agent workflow labels to PR #$PRNumber..." -ForegroundColor Cyan
    
    # 1. Always apply s/agent-reviewed
    Update-AgentReviewedLabel -PRNumber $PRNumber
    
    # 2. Determine and apply outcome label
    $outcome = Get-OutcomeFromState -Content $content
    Write-Host "  📊 Outcome: $outcome" -ForegroundColor Cyan
    Update-AgentOutcomeLabel -Outcome $outcome -PRNumber $PRNumber
    
    # 3. Determine and apply signal labels
    $gateResult = Get-GateResultFromState -Content $content
    if ($gateResult -eq 'Passed') {
        Update-AgentSignalLabel -Signal GatePassed -PRNumber $PRNumber
    } elseif ($gateResult -eq 'Failed') {
        Update-AgentSignalLabel -Signal GateFailed -PRNumber $PRNumber
    }
    
    $fixOptimal = Get-FixOptimalFromState -Content $content
    if ($fixOptimal) {
        Update-AgentSignalLabel -Signal FixOptimal -PRNumber $PRNumber
    }
    
    Write-Host ""
    Write-Host "✅ Labels applied to PR #$PRNumber" -ForegroundColor Green
}

# ============================================================
# State File Parsing
# ============================================================

function Get-OutcomeFromState {
    param([string]$Content)
    
    # Check for Final Recommendation
    if ($Content -match '(?i)Final Recommendation[:\s]*APPROVE') {
        return 'Approved'
    }
    if ($Content -match '(?i)Final Recommendation[:\s]*(REQUEST[_ ]CHANGES|CHANGES[_ ]REQUESTED)') {
        return 'ChangesRequested'
    }
    # Check for Verdict line (alternative format)
    if ($Content -match '(?i)Verdict[:\s]*✅\s*APPROVE') {
        return 'Approved'
    }
    if ($Content -match '(?i)Verdict[:\s]*(⚠️|❌)') {
        return 'ChangesRequested'
    }
    
    # Check if all phases completed — if not, it's incomplete
    $phases = @('Pre-Flight', 'Tests', 'Gate', 'Fix', 'Report')
    $allComplete = $true
    foreach ($phase in $phases) {
        if ($Content -notmatch "(?i)$phase\s*\|\s*✅") {
            $allComplete = $false
            break
        }
    }
    
    if (-not $allComplete) {
        return 'ReviewIncomplete'
    }
    
    # Phases complete but no clear recommendation — default to incomplete
    return 'ReviewIncomplete'
}

function Get-GateResultFromState {
    param([string]$Content)
    
    # Check the phase status table
    if ($Content -match '(?i)Gate\s*\|\s*✅\s*PASSED') {
        return 'Passed'
    }
    if ($Content -match '(?i)Gate\s*\|\s*❌\s*FAILED') {
        return 'Failed'
    }
    if ($Content -match '(?i)Gate\s*\|\s*⚠️') {
        return 'Failed'
    }
    
    return $null  # Gate not run or status unclear
}

function Get-FixOptimalFromState {
    param([string]$Content)
    
    # Look for indicators that the PR's fix was selected as best
    if ($Content -match '(?i)Selected Fix[:\s]*PR') {
        return $true
    }
    if ($Content -match '(?i)PR.s fix is.*best') {
        return $true
    }
    if ($Content -match '(?i)PR fix.*optimal') {
        return $true
    }
    
    return $false
}
