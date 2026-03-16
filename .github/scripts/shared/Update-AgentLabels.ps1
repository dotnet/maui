#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Shared functions for managing agent workflow labels on GitHub PRs.

.DESCRIPTION
    Provides idempotent label management for the PR agent review workflow.
    Labels use the 's/agent-*' prefix convention for easy querying.

    Label categories:
    - Outcome labels (mutually exclusive): agent-approved, agent-changes-requested, agent-review-incomplete
    - Signal labels (additive): agent-gate-passed, agent-gate-failed, agent-fix-win, agent-fix-pr-picked
    - Manual labels (applied by maintainers): agent-fix-implemented
    - Tracking label: agent-reviewed (always applied on completed run)

.NOTES
    All functions are designed to be non-fatal: label failures emit warnings
    but do not throw or exit with error codes.
#>

# ============================================================
# Label definitions
# ============================================================

$script:OutcomeLabels = @{
    's/agent-approved'          = @{ Description = 'AI agent recommends approval - PR fix is correct and optimal'; Color = '2E7D32' }
    's/agent-changes-requested' = @{ Description = 'AI agent recommends changes - found a better alternative or issues'; Color = 'E65100' }
    's/agent-review-incomplete' = @{ Description = 'AI agent could not complete all phases (blocker, timeout, error)'; Color = 'B71C1C' }
}

$script:SignalLabels = @{
    's/agent-gate-passed' = @{ Description = 'AI verified tests catch the bug (fail without fix, pass with fix)'; Color = '4CAF50' }
    's/agent-gate-failed' = @{ Description = 'AI could not verify tests catch the bug'; Color = 'FF9800' }
    's/agent-fix-win'     = @{ Description = 'AI found a better alternative fix than the PR'; Color = '66BB6A' }
    's/agent-fix-pr-picked'    = @{ Description = 'AI could not beat the PR fix - PR is the best among all candidates'; Color = 'FF7043' }
}

$script:ManualLabels = @{
    's/agent-fix-implemented' = @{ Description = 'PR author implemented the agent suggested fix'; Color = '7B1FA2' }
}

$script:TrackingLabel = @{
    's/agent-reviewed' = @{ Description = 'PR was reviewed by AI agent workflow (full 4-phase review)'; Color = '1565C0' }
}

# All label definitions combined
$script:AllLabelDefs = @{}
foreach ($group in @($script:OutcomeLabels, $script:SignalLabels, $script:ManualLabels, $script:TrackingLabel)) {
    foreach ($key in $group.Keys) {
        $script:AllLabelDefs[$key] = $group[$key]
    }
}

# ============================================================
# Helper: Ensure a label exists in the repository
# ============================================================
function Ensure-LabelExists {
    <#
    .SYNOPSIS
        Creates a label in the repository if it doesn't already exist.
        Updates description/color if the label exists but has stale metadata.
    #>
    param(
        [Parameter(Mandatory)] [string]$LabelName,
        [Parameter(Mandatory)] [string]$Description,
        [Parameter(Mandatory)] [string]$Color,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    try {
        # Check if label exists
        $existing = gh api "repos/$Owner/$Repo/labels/$([uri]::EscapeDataString($LabelName))" 2>$null | ConvertFrom-Json
        if ($LASTEXITCODE -eq 0 -and $existing) {
            # Label exists — update if description or color changed
            $needsUpdate = ($existing.description -ne $Description) -or ($existing.color -ne $Color)
            if ($needsUpdate) {
                gh api "repos/$Owner/$Repo/labels/$([uri]::EscapeDataString($LabelName))" `
                    --method PATCH `
                    -f description="$Description" `
                    -f color="$Color" 2>$null | Out-Null
                Write-Host "  🏷️  Updated label: $LabelName" -ForegroundColor Gray
            }
        } else {
            # Label doesn't exist — create it
            gh api "repos/$Owner/$Repo/labels" `
                --method POST `
                -f name="$LabelName" `
                -f description="$Description" `
                -f color="$Color" 2>$null | Out-Null
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  🏷️  Created label: $LabelName" -ForegroundColor Green
            } else {
                Write-Host "  ⚠️  Failed to create label: $LabelName" -ForegroundColor Yellow
            }
        }
    }
    catch {
        Write-Host "  ⚠️  Label operation failed for '$LabelName': $_" -ForegroundColor Yellow
    }
}

# ============================================================
# Helper: Get current agent labels on a PR
# ============================================================
function Get-AgentLabels {
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $labels = gh api "repos/$Owner/$Repo/issues/$PRNumber/labels" --jq '.[].name' 2>$null
    if ($LASTEXITCODE -ne 0) { return @() }
    return @($labels | Where-Object { $_ -like 's/agent-*' })
}

# ============================================================
# Helper: Add a label to a PR
# ============================================================
function Add-Label {
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [Parameter(Mandatory)] [string]$LabelName,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    gh api "repos/$Owner/$Repo/issues/$PRNumber/labels" `
        --method POST `
        -f "labels[]=$LabelName" 2>$null | Out-Null
    return $LASTEXITCODE -eq 0
}

# ============================================================
# Helper: Remove a label from a PR
# ============================================================
function Remove-Label {
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [Parameter(Mandatory)] [string]$LabelName,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    gh api "repos/$Owner/$Repo/issues/$PRNumber/labels/$([uri]::EscapeDataString($LabelName))" `
        --method DELETE 2>$null | Out-Null
    return $LASTEXITCODE -eq 0
}

# ============================================================
# Update-AgentOutcomeLabel
# ============================================================
function Update-AgentOutcomeLabel {
    <#
    .SYNOPSIS
        Applies exactly one outcome label, removing any conflicting outcome labels.

    .PARAMETER Outcome
        One of: 'approved', 'changes-requested', 'review-incomplete'
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [Parameter(Mandatory)]
        [ValidateSet('approved', 'changes-requested', 'review-incomplete')]
        [string]$Outcome,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $targetLabel = "s/agent-$Outcome"
    Write-Host "  📌 Outcome: $targetLabel" -ForegroundColor Cyan

    # Ensure the target label exists in the repo
    $def = $script:OutcomeLabels[$targetLabel]
    Ensure-LabelExists -LabelName $targetLabel -Description $def.Description -Color $def.Color -Owner $Owner -Repo $Repo

    # Get current labels on the PR
    $currentLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo

    # Remove conflicting outcome labels
    foreach ($olName in $script:OutcomeLabels.Keys) {
        if ($olName -ne $targetLabel -and $currentLabels -contains $olName) {
            Write-Host "  🗑️  Removing stale: $olName" -ForegroundColor Yellow
            Remove-Label -PRNumber $PRNumber -LabelName $olName -Owner $Owner -Repo $Repo
        }
    }

    # Add the target label (idempotent — GitHub ignores duplicates)
    if ($currentLabels -notcontains $targetLabel) {
        $ok = Add-Label -PRNumber $PRNumber -LabelName $targetLabel -Owner $Owner -Repo $Repo
        if ($ok) {
            Write-Host "  ✅ Applied: $targetLabel" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Failed to apply: $targetLabel" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ✅ Already present: $targetLabel" -ForegroundColor Green
    }
}

# ============================================================
# Update-AgentSignalLabels
# ============================================================
function Update-AgentSignalLabels {
    <#
    .SYNOPSIS
        Adds or removes signal labels based on phase results.

    .PARAMETER GateResult
        Gate phase result: 'passed', 'failed', or $null (skipped)

    .PARAMETER FixResult
        Fix phase result: 'win' (PR best), 'lose' (alternative better), or $null (skipped)
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$GateResult,    # 'passed', 'failed', or $null
        [string]$FixResult,     # 'win' (agent found better alternative), 'lose' (PR is best), or $null
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $currentLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo

    # --- Gate labels ---
    if ($GateResult -eq 'passed') {
        $label = 's/agent-gate-passed'
        $def = $script:SignalLabels[$label]
        Ensure-LabelExists -LabelName $label -Description $def.Description -Color $def.Color -Owner $Owner -Repo $Repo

        # Add gate-passed, remove gate-failed
        if ($currentLabels -notcontains $label) {
            Add-Label -PRNumber $PRNumber -LabelName $label -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  ✅ Signal: $label" -ForegroundColor Green
        }
        if ($currentLabels -contains 's/agent-gate-failed') {
            Remove-Label -PRNumber $PRNumber -LabelName 's/agent-gate-failed' -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  🗑️  Removed stale: s/agent-gate-failed" -ForegroundColor Yellow
        }
    }
    elseif ($GateResult -eq 'failed') {
        $label = 's/agent-gate-failed'
        $def = $script:SignalLabels[$label]
        Ensure-LabelExists -LabelName $label -Description $def.Description -Color $def.Color -Owner $Owner -Repo $Repo

        # Add gate-failed, remove gate-passed
        if ($currentLabels -notcontains $label) {
            Add-Label -PRNumber $PRNumber -LabelName $label -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  ✅ Signal: $label" -ForegroundColor Green
        }
        if ($currentLabels -contains 's/agent-gate-passed') {
            Remove-Label -PRNumber $PRNumber -LabelName 's/agent-gate-passed' -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  🗑️  Removed stale: s/agent-gate-passed" -ForegroundColor Yellow
        }
    }

    # --- Fix labels ---
    if ($FixResult -eq 'win') {
        $label = 's/agent-fix-win'
        $def = $script:SignalLabels[$label]
        Ensure-LabelExists -LabelName $label -Description $def.Description -Color $def.Color -Owner $Owner -Repo $Repo

        if ($currentLabels -notcontains $label) {
            Add-Label -PRNumber $PRNumber -LabelName $label -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  ✅ Signal: $label" -ForegroundColor Green
        }
        if ($currentLabels -contains 's/agent-fix-pr-picked') {
            Remove-Label -PRNumber $PRNumber -LabelName 's/agent-fix-pr-picked' -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  🗑️  Removed stale: s/agent-fix-pr-picked" -ForegroundColor Yellow
        }
    }
    elseif ($FixResult -eq 'lose') {
        $label = 's/agent-fix-pr-picked'
        $def = $script:SignalLabels[$label]
        Ensure-LabelExists -LabelName $label -Description $def.Description -Color $def.Color -Owner $Owner -Repo $Repo

        if ($currentLabels -notcontains $label) {
            Add-Label -PRNumber $PRNumber -LabelName $label -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  ✅ Signal: $label" -ForegroundColor Green
        }
        if ($currentLabels -contains 's/agent-fix-win') {
            Remove-Label -PRNumber $PRNumber -LabelName 's/agent-fix-win' -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  🗑️  Removed stale: s/agent-fix-win" -ForegroundColor Yellow
        }
    }
}

# ============================================================
# Update-AgentReviewedLabel
# ============================================================
function Update-AgentReviewedLabel {
    <#
    .SYNOPSIS
        Ensures the s/agent-reviewed tracking label is on the PR.
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $label = 's/agent-reviewed'
    $def = $script:TrackingLabel[$label]
    Ensure-LabelExists -LabelName $label -Description $def.Description -Color $def.Color -Owner $Owner -Repo $Repo

    $currentLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo
    if ($currentLabels -notcontains $label) {
        $ok = Add-Label -PRNumber $PRNumber -LabelName $label -Owner $Owner -Repo $Repo
        if ($ok) {
            Write-Host "  ✅ Tracking: $label" -ForegroundColor Green
        } else {
            Write-Host "  ⚠️  Failed to apply: $label" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  ✅ Already present: $label" -ForegroundColor Green
    }
}

# ============================================================
# Parse-PhaseOutcomes — read content.md files to determine labels
# ============================================================
function Parse-PhaseOutcomes {
    <#
    .SYNOPSIS
        Reads phase output content.md files and determines outcome + signal labels.

    .OUTPUTS
        Hashtable with keys: Outcome, GateResult, FixResult
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$RepoRoot = (git rev-parse --show-toplevel 2>$null)
    )

    $baseDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    $result = @{
        Outcome    = $null  # 'approved', 'changes-requested', 'review-incomplete'
        GateResult = $null  # 'passed', 'failed'
        FixResult  = $null  # 'win', 'lose'
    }

    # --- Parse Gate content.md ---
    $gateFile = Join-Path $baseDir "gate/content.md"
    if (Test-Path $gateFile) {
        $gateContent = Get-Content $gateFile -Raw -ErrorAction SilentlyContinue
        if ($gateContent) {
            # Match the Result line specifically to avoid false matches from other text
            if ($gateContent -match '(?im)^\*?\*?Result\*?\*?:.*(?:✅|PASSED)') {
                $result.GateResult = 'passed'
            }
            elseif ($gateContent -match '(?im)^\*?\*?Result\*?\*?:.*(?:❌|FAILED|SKIPPED)') {
                $result.GateResult = 'failed'
            }
        }
    }

    # --- Parse try-fix content.md for fix result ---
    $fixFile = Join-Path $baseDir "try-fix/content.md"
    if (Test-Path $fixFile) {
        $fixContent = Get-Content $fixFile -Raw -ErrorAction SilentlyContinue
        if ($fixContent) {
            # Extract just the fix name (before any reason separator like " — ")
            # to avoid false matches from reason text containing keywords like "try-fix" or "alternative"
            if ($fixContent -match '(?i)Selected Fix:\s*\*?\*?\s*(.+?)(?:\s*—|\s*$)') {
                $fixName = $matches[1].Trim()
                # Agent wins: fix name starts with Candidate/Alternative/try-fix
                if ($fixName -match '(?i)^(?:Candidate|Alternative|try-fix)') {
                    $result.FixResult = 'win'
                }
                # Agent loses: fix name starts with PR
                elseif ($fixName -match '(?i)^(?:\*?\*?\s*)?PR\b') {
                    $result.FixResult = 'lose'
                }
            }
        }
    }

    # --- Parse report content.md for outcome ---
    $reportFile = Join-Path $baseDir "report/content.md"
    if (Test-Path $reportFile) {
        $reportContent = Get-Content $reportFile -Raw -ErrorAction SilentlyContinue
        if ($reportContent) {
            if ($reportContent -match '(?i)Final\s+Recommendation:\s*APPROVE|✅\s*Final\s+Recommendation:\s*APPROVE') {
                $result.Outcome = 'approved'
            }
            elseif ($reportContent -match '(?i)Final\s+Recommendation:\s*REQUEST.CHANGES|⚠️\s*Final\s+Recommendation:\s*REQUEST.CHANGES') {
                $result.Outcome = 'changes-requested'
            }
            else {
                $result.Outcome = 'review-incomplete'
            }
        } else {
            $result.Outcome = 'review-incomplete'
        }
    } else {
        # No report means the agent didn't finish
        $result.Outcome = 'review-incomplete'
    }

    return $result
}

# ============================================================
# Apply-AgentLabels — main entry point
# ============================================================
function Apply-AgentLabels {
    <#
    .SYNOPSIS
        Main entry point: parses phase outputs and applies all appropriate labels.

    .DESCRIPTION
        1. Parses content.md files from each phase
        2. Applies exactly one outcome label
        3. Applies signal labels based on phase results
        4. Always applies s/agent-reviewed

    .PARAMETER PRNumber
        The GitHub PR number.

    .PARAMETER RepoRoot
        Repository root path. Defaults to git rev-parse --show-toplevel.
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$RepoRoot = (git rev-parse --show-toplevel 2>$null),
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    Write-Host ""
    Write-Host "🏷️  Applying agent labels to PR #$PRNumber..." -ForegroundColor Cyan

    # Parse phase outcomes from content.md files
    $outcomes = Parse-PhaseOutcomes -PRNumber $PRNumber -RepoRoot $RepoRoot
    Write-Host "  📊 Parsed outcomes:" -ForegroundColor Gray
    Write-Host "     Outcome:    $($outcomes.Outcome ?? '(none)')" -ForegroundColor Gray
    Write-Host "     Gate:       $($outcomes.GateResult ?? '(skipped)')" -ForegroundColor Gray
    Write-Host "     Fix:        $($outcomes.FixResult ?? '(skipped)')" -ForegroundColor Gray

    try {
        # 1. Apply outcome label (exactly one)
        if ($outcomes.Outcome) {
            Update-AgentOutcomeLabel -PRNumber $PRNumber -Outcome $outcomes.Outcome -Owner $Owner -Repo $Repo
        }

        # 2. Apply signal labels
        Update-AgentSignalLabels -PRNumber $PRNumber -GateResult $outcomes.GateResult -FixResult $outcomes.FixResult -Owner $Owner -Repo $Repo

        # 3. Always apply tracking label
        Update-AgentReviewedLabel -PRNumber $PRNumber -Owner $Owner -Repo $Repo

        Write-Host ""
        Write-Host "  ✅ Labels applied successfully" -ForegroundColor Green
    }
    catch {
        Write-Host ""
        Write-Host "  ⚠️  Label application error (non-fatal): $_" -ForegroundColor Yellow
    }
}
