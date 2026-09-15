#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Shared functions for managing agent workflow labels on GitHub PRs.

.DESCRIPTION
    Provides idempotent label management for the pr-review skill review workflow.
    Labels use the 's/agent-*' prefix convention for easy querying.

    Label categories:
    - Outcome labels (mutually exclusive): agent-approved, agent-changes-requested, agent-review-incomplete
    - Signal labels (additive): agent-gate-passed, agent-gate-failed, agent-fix-win, agent-fix-pr-picked
    - Manual / queue labels: agent-fix-implemented, agent-ready-for-rerun, agent-review-in-progress
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
    's/agent-fix-implemented'   = @{ Description = 'PR author implemented the agent suggested fix'; Color = '7B1FA2' }
    's/agent-ready-for-rerun'   = @{ Description = 'AI review has new PR activity and is ready for rerun'; Color = '5319E7' }
    's/agent-review-in-progress' = @{ Description = 'AI review is currently running for this PR'; Color = 'FBCA04' }
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

    $tmp = $null
    try {
        $tmp = New-TemporaryFile
        @{ labels = @($LabelName) } | ConvertTo-Json -Compress | Set-Content -LiteralPath $tmp -Encoding utf8 -NoNewline
        $output = & gh api "repos/$Owner/$Repo/issues/$PRNumber/labels" `
            --method POST `
            --input $tmp 2>&1
        $exitCode = $LASTEXITCODE
        if ($exitCode -eq 0) {
            return $true
        }

        $message = ($output | Out-String).Trim()
        if ([string]::IsNullOrWhiteSpace($message)) {
            $message = "gh api exited with code $exitCode."
        } elseif ($message.Length -gt 1000) {
            $message = $message.Substring(0, 1000) + '...'
        }

        Write-Host "  ⚠️  Failed to add label '$LabelName' to PR #$PRNumber (gh api exit code $exitCode): $message" -ForegroundColor Yellow
        return $false
    } finally {
        if ($tmp) {
            Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
        }
    }
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

    & gh api "repos/$Owner/$Repo/issues/$PRNumber/labels/$([uri]::EscapeDataString($LabelName))" `
        --method DELETE 1>$null 2>$null
    return $LASTEXITCODE -eq 0
}

# ============================================================
# Set-AgentReviewInProgress
# ============================================================
function Set-AgentReviewInProgress {
    <#
    .SYNOPSIS
        Applies the persistent in-progress lock label before triggering review.
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $label = 's/agent-review-in-progress'
    $def = $script:ManualLabels[$label]
    Ensure-LabelExists -LabelName $label -Description $def.Description -Color $def.Color -Owner $Owner -Repo $Repo

    $currentLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo
    if ($currentLabels -contains $label) {
        Write-Host "  ✅ Already present: $label" -ForegroundColor Green
        return $true
    }

    $ok = Add-Label -PRNumber $PRNumber -LabelName $label -Owner $Owner -Repo $Repo
    $updatedLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo
    if ($ok -or $updatedLabels -contains $label) {
        Write-Host "  ✅ Applied: $label" -ForegroundColor Green
        return $true
    }

    Write-Host "  ⚠️  Failed to apply: $label" -ForegroundColor Yellow
    return $false
}

# ============================================================
# Clear-AgentReviewInProgress
# ============================================================
function Clear-AgentReviewInProgress {
    <#
    .SYNOPSIS
        Removes the persistent in-progress lock label after review finishes.
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $label = 's/agent-review-in-progress'
    $currentLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo
    if ($currentLabels -notcontains $label) {
        Write-Host "  ✅ Not present: $label" -ForegroundColor Green
        return $true
    }

    $ok = Remove-Label -PRNumber $PRNumber -LabelName $label -Owner $Owner -Repo $Repo
    $updatedLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo
    if ($ok -or $updatedLabels -notcontains $label) {
        Write-Host "  ✅ Removed: $label" -ForegroundColor Green
        return $true
    }

    Write-Host "  ⚠️  Failed to remove: $label" -ForegroundColor Yellow
    return $false
}

# ============================================================
# Test-AgentReviewInProgressIsStale
# ============================================================
function Test-AgentReviewInProgressIsStale {
    <#
    .SYNOPSIS
        Returns true when the in-progress lock label is older than the stale threshold.

    .DESCRIPTION
        This is a cancellation safety net. Normal AzDO runs clear the lock in a
        final cleanup stage; if a run is cancelled before cleanup can start, the
        scanner/manual trigger can recover after the conservative stale window.
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui',
        [int]$StaleAfterHours = 18
    )

    $label = 's/agent-review-in-progress'
    $createdAtValues = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/events?per_page=100" --paginate --jq ".[] | select(.event == `"labeled`" and .label.name == `"$label`") | .created_at" 2>$null)
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  ⚠️  Could not inspect label history for PR #$PRNumber; treating $label as fresh" -ForegroundColor Yellow
        return $false
    }

    if ($createdAtValues.Count -eq 0) {
        Write-Host "  ⚠️  No label history found for $label on PR #$PRNumber; treating it as fresh" -ForegroundColor Yellow
        return $false
    }

    $latestAppliedAt = $createdAtValues | ForEach-Object {
        [datetimeoffset]::Parse([string]$_, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::AssumeUniversal)
    } | Sort-Object -Descending | Select-Object -First 1

    $age = [datetimeoffset]::UtcNow - $latestAppliedAt
    if ($age -gt [timespan]::FromHours($StaleAfterHours)) {
        Write-Host "  ⚠️  $label on PR #$PRNumber is stale (applied $($latestAppliedAt.ToString('u')))" -ForegroundColor Yellow
        return $true
    }

    Write-Host "  ✅ $label on PR #$PRNumber is fresh (applied $($latestAppliedAt.ToString('u')))" -ForegroundColor Green
    return $false
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
        Determines outcome + signal labels from a review run's artifacts.

    .DESCRIPTION
        Prefers the authoritative machine-readable artifacts over prose:
          - Gate  -> gate/gate-result.txt (PASSED|SKIPPED|FAILED); SKIPPED => no gate label.
          - Fix   -> winner.json (isPRFix); false => 'win' (alternative beat PR),
                     true => 'lose' (PR fix best); missing => no fix label.
          - Outcome -> report/content.md Final Recommendation.

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

    # --- Gate result (authoritative: gate/gate-result.txt) ---
    # The Gate phase writes the canonical verdict (PASSED|SKIPPED|FAILED) to gate-result.txt.
    # SKIPPED means "no runnable tests were detected" — it is NOT a failure, so it maps to
    # $null (no gate signal label). Fall back to the gate report header only if the file is
    # missing (using the real "### Gate Result:" format, not the old broken "^Result:").
    $gateVerdict = $null
    $gateResultFile = Join-Path $baseDir "gate/gate-result.txt"
    if (Test-Path $gateResultFile) {
        $gateVerdict = (Get-Content $gateResultFile -Raw -ErrorAction SilentlyContinue)
    }
    if (-not $gateVerdict) {
        $gateFile = Join-Path $baseDir "gate/content.md"
        if (Test-Path $gateFile) {
            $gateContent = Get-Content $gateFile -Raw -ErrorAction SilentlyContinue
            if ($gateContent -and $gateContent -match '(?im)Gate Result:\s*(?:\S+\s*)?(PASSED|FAILED|SKIPPED)') {
                $gateVerdict = $matches[1]
            }
        }
    }
    switch -Regex (($gateVerdict ?? '').Trim()) {
        '(?i)^PASSED' { $result.GateResult = 'passed' }
        '(?i)^FAILED' { $result.GateResult = 'failed' }
        # SKIPPED / empty / anything else => $null (no gate signal label)
    }

    # --- Fix result (authoritative: winner.json) ---
    # winner.json is the machine-readable comparison verdict written by the Report phase.
    #   isPRFix = $false (winner is a try-fix-* candidate) => an alternative beat the PR => 'win'
    #   isPRFix = $true  (winner is pr / pr-plus-reviewer)  => the PR fix was best        => 'lose'
    # A missing/invalid winner.json (e.g. review-incomplete) => $null (no fix signal label),
    # so we never guess a fix outcome the comparison did not actually produce.
    $winnerFile = Join-Path $baseDir "winner.json"
    if (Test-Path $winnerFile) {
        $winner = $null
        try { $winner = Get-Content $winnerFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop } catch { $winner = $null }
        if ($winner) {
            $winnerName = [string]$winner.winner
            if ($null -ne $winner.isPRFix) {
                $result.FixResult = if ($winner.isPRFix) { 'lose' } else { 'win' }
            }
            elseif ($winnerName -match '(?i)^try-fix') {
                $result.FixResult = 'win'
            }
            elseif ($winnerName -match '(?i)^(pr|pr-plus-reviewer)$') {
                $result.FixResult = 'lose'
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
