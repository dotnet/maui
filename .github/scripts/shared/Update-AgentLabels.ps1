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

function Get-AgentReviewInProgressAppliedAt {
    <#
    .SYNOPSIS
        Returns the DateTimeOffset the in-progress lock label was most recently
        applied, or $null when it isn't applied / history is unavailable.

    .DESCRIPTION
        Used to dedupe the "a review is already running" skip notice so at most
        one notice is posted per in-progress cycle (see review-trigger.yml): a
        skip comment newer than this timestamp means the current lock already
        has a notice and a repeat /review must stay silent.
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $label = 's/agent-review-in-progress'
    $createdAtValues = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/events?per_page=100" --paginate --jq ".[] | select(.event == `"labeled`" and .label.name == `"$label`") | .created_at" 2>$null)
    if ($LASTEXITCODE -ne 0 -or $createdAtValues.Count -eq 0) {
        return $null
    }

    return ($createdAtValues | ForEach-Object {
        [datetimeoffset]::Parse([string]$_, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::AssumeUniversal)
    } | Sort-Object -Descending | Select-Object -First 1)
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
# Clear-AgentOutcomeLabels
# ============================================================
function Clear-AgentOutcomeLabels {
    <#
    .SYNOPSIS
        Removes all outcome labels when a completed report has no trustworthy
        canonical recommendation.
    #>
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    $currentLabels = Get-AgentLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo
    foreach ($olName in $script:OutcomeLabels.Keys) {
        if ($currentLabels -contains $olName) {
            Write-Host "  🗑️  Removing stale outcome: $olName" -ForegroundColor Yellow
            Remove-Label -PRNumber $PRNumber -LabelName $olName -Owner $Owner -Repo $Repo
        }
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
    else {
        # SKIPPED / INCONCLUSIVE / TIMEDOUT produce no current gate signal. Remove
        # either label from an older run so the PR does not keep advertising a stale
        # pass or failure after the latest Gate was unable or not required to verify.
        foreach ($staleLabel in @('s/agent-gate-passed', 's/agent-gate-failed')) {
            if ($currentLabels -contains $staleLabel) {
                Remove-Label -PRNumber $PRNumber -LabelName $staleLabel -Owner $Owner -Repo $Repo | Out-Null
                Write-Host "  🗑️  Removed stale: $staleLabel" -ForegroundColor Yellow
            }
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
    else {
        # A missing/invalid winner means this run did not complete a trustworthy fix
        # comparison. Clear both alternatives rather than retaining a previous run's winner.
        foreach ($staleLabel in @('s/agent-fix-win', 's/agent-fix-pr-picked')) {
            if ($currentLabels -contains $staleLabel) {
                Remove-Label -PRNumber $PRNumber -LabelName $staleLabel -Owner $Owner -Repo $Repo | Out-Null
                Write-Host "  🗑️  Removed stale: $staleLabel" -ForegroundColor Yellow
            }
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
# Get-OutcomeFromCodeReviewVerdict — fallback outcome source
# ============================================================
function Get-OutcomeFromCodeReviewVerdict {
    <#
    .SYNOPSIS
        Derive an outcome label from the code-review Verdict when the Report phase
        completed but omitted its canonical "Final Recommendation:" line.

    .DESCRIPTION
        The current reviewer writes its code-review verdict to
        expert-pr-eval/content.md. Older runs may instead have a verdict in
        pre-flight/code-review.md. Map any usable verdict to an outcome label so a
        completed review whose Report omitted the recommendation line is not
        mislabeled review-incomplete. Returns null when no usable verdict is present
        so callers can clear stale outcome labels without inventing a recommendation.
        Matches both "**Verdict:** LGTM" and "### Verdict: NEEDS_CHANGES".
    #>
    param([Parameter(Mandatory)] [string]$BaseDir)

    foreach ($rel in @('expert-pr-eval/content.md', 'pre-flight/code-review.md')) {
        $f = Join-Path $BaseDir $rel
        if (-not (Test-Path $f)) { continue }
        $c = Get-Content $f -Raw -ErrorAction SilentlyContinue
        if (-not $c) { continue }
        $verdict = $null
        if ($c -match '(?im)Verdict:\s*\**\s*(LGTM|APPROVE|NEEDS[ _]?CHANGES|NEEDS[ _]?DISCUSSION|REQUEST[ _]?CHANGES)') {
            $verdict = $matches[1]
        }
        elseif ($c -match '(?im)^[ \t]*#{1,6}[ \t]+(?:Initial[ \t]+)?Verdict[^\r\n]*(?:\r?\n[ \t]*)+\**[ \t]*(LGTM|APPROVE|NEEDS[ _]?CHANGES|NEEDS[ _]?DISCUSSION|REQUEST[ _]?CHANGES)\b') {
            $verdict = $matches[1]
        }
        if ($verdict) {
            switch -Regex ($verdict) {
                '(?i)^(LGTM|APPROVE)' { return 'approved' }
                default               { return 'changes-requested' }
            }
        }
    }
    return $null
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
        [string]$RepoRoot = (git rev-parse --show-toplevel 2>$null),
        [ValidateSet('PASSED', 'SKIPPED', 'INCONCLUSIVE', 'FAILED', 'TIMEDOUT', '')]
        [string]$TrustedGateResult = ''
    )

    $baseDir = Join-Path $RepoRoot "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent"
    $result = @{
        Outcome    = $null  # 'approved', 'changes-requested', 'review-incomplete', or null
        GateResult = $null  # 'passed', 'failed'
        FixResult  = $null  # 'win', 'lose'
    }

    # --- Gate result ---
    # Stage 3 supplies the pipeline-frozen verdict whenever available. It must override
    # gate-result.txt/content.md because those files cross the agent-writable artifact
    # boundary. In particular, a timed-out Gate can leave partial FAILED-looking content
    # even though no trusted verdict was produced (build 14878396 / PR #36698).
    $gateVerdict = $TrustedGateResult
    if ([string]::IsNullOrWhiteSpace($gateVerdict)) {
        # Local/Task-3 fallback: use the Gate phase artifacts when no frozen value was passed.
        $gateResultFile = Join-Path $baseDir "gate/gate-result.txt"
        if (Test-Path $gateResultFile) {
            $gateVerdict = (Get-Content $gateResultFile -Raw -ErrorAction SilentlyContinue)
        }
        if (-not $gateVerdict) {
            $gateFile = Join-Path $baseDir "gate/content.md"
            if (Test-Path $gateFile) {
                $gateContent = Get-Content $gateFile -Raw -ErrorAction SilentlyContinue
                if ($gateContent -and $gateContent -match '(?im)Gate Result:\s*(?:\S+\s*)?(PASSED|FAILED|SKIPPED|INCONCLUSIVE|TIMEDOUT)') {
                    $gateVerdict = $matches[1]
                }
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
    #   winner = try-fix-* (isPRFix = $false) => an alternative beat the PR      => 'win'
    #   winner = pr-plus-reviewer             => the agent improved the PR fix   => 'win'
    #   winner = pr (isPRFix = $true)         => the submitted PR fix was best   => 'lose'
    # pr-plus-reviewer must NOT map to 'lose': that label ("AI could not beat the
    # PR fix") would contradict the report contract, which treats a
    # pr-plus-reviewer win as "the submitted PR still needs the winning changes".
    # A missing/invalid winner.json (e.g. review-incomplete) => $null (no fix signal label),
    # so we never guess a fix outcome the comparison did not actually produce.
    $winnerName = $null
    $winnerRequiresPRChanges = $false
    $winnerFile = Join-Path $baseDir "winner.json"
    if (Test-Path $winnerFile) {
        $winner = $null
        try { $winner = Get-Content $winnerFile -Raw -ErrorAction Stop | ConvertFrom-Json -ErrorAction Stop } catch { $winner = $null }
        if ($winner) {
            $winnerName = [string]$winner.winner
            $winnerRequiresPRChanges =
                ($winner.isPRFix -eq $false) -or
                ($winnerName -match '(?i)^(pr-plus-reviewer|try-fix(?:-|$))')
            if ($winnerName -match '(?i)^(try-fix(?:-|$)|pr-plus-reviewer$)') {
                $result.FixResult = 'win'
            }
            elseif ($winnerName -match '(?i)^pr$') {
                $result.FixResult = 'lose'
            }
            elseif ($null -ne $winner.isPRFix) {
                $result.FixResult = if ($winner.isPRFix) { 'lose' } else { 'win' }
            }
        }
    }

    # --- Parse report content.md for outcome ---
    $reportCompleted = $false
    $reportFile = Join-Path $baseDir "report/content.md"
    if (Test-Path $reportFile) {
        $reportContent = Get-Content $reportFile -Raw -ErrorAction SilentlyContinue
        if (-not [string]::IsNullOrWhiteSpace($reportContent)) {
            $reportCompleted = $true
            if ($reportContent -match '(?im)^\s*(?:##\s*)?(?:✅\s*)?Final\s+Recommendation:\s*APPROVE\s*$') {
                $result.Outcome = 'approved'
            }
            elseif ($reportContent -match '(?im)^\s*(?:##\s*)?(?:⚠️\s*)?Final\s+Recommendation:\s*REQUEST\s+CHANGES\s*$') {
                $result.Outcome = 'changes-requested'
            }
            else {
                # The Report phase ran to completion (report/content.md exists) but the
                # LLM omitted the canonical "Final Recommendation: {APPROVE|REQUEST CHANGES}"
                # line — it sometimes emits only a "Winning candidate" comparative section
                # (observed on PR #36541, build 14698057, which mislabeled a NEEDS_CHANGES
                # review as review-incomplete). A completed report is NOT review-incomplete:
                # A winning alternative or pr-plus-reviewer candidate means the submitted
                # PR still needs changes, regardless of whether a prose verdict was emitted.
                # Otherwise fall back to the expert/legacy code-review Verdict. If neither
                # exists, leave Outcome null so stale outcome labels are removed instead of
                # misclassifying a completed review.
                $result.Outcome = if ($winnerRequiresPRChanges) {
                    'changes-requested'
                } elseif ($winnerName -match '(?i)^pr$') {
                    Get-OutcomeFromCodeReviewVerdict -BaseDir $baseDir
                } else {
                    $null
                }
            }
        } else {
            $result.Outcome = 'review-incomplete'
        }
    } else {
        # No report means the agent didn't finish
        $result.Outcome = 'review-incomplete'
    }

    # The submitted PR still needs changes whenever a modified PR candidate or
    # independent try-fix wins. This is authoritative even if the prose report
    # accidentally emits APPROVE.
    if ($reportCompleted -and $winnerRequiresPRChanges) {
        $result.Outcome = 'changes-requested'
    }

    # Keep labels aligned with the trusted validation verdict. The summary posting path
    # already vetoes APPROVE over a failed/timed-out Gate; labels must not contradict it.
    if ($result.Outcome -eq 'approved' -and (($gateVerdict ?? '').Trim() -match '(?i)^(FAILED|TIMEDOUT)$')) {
        $result.Outcome = 'changes-requested'
    }

    # Same alignment for the expert code-review verdict: the summary path vetoes an APPROVE
    # over a blocking expert verdict (Test-ExpertReviewIsBlocking in post-ai-summary-comment.ps1),
    # so an 'approved' label over the same artifact would contradict the posted review event.
    if ($result.Outcome -eq 'approved' -and (Get-OutcomeFromCodeReviewVerdict -BaseDir $baseDir) -eq 'changes-requested') {
        $result.Outcome = 'changes-requested'
    }

    return $result
}

# ============================================================
# Apply-AgentLabels — main entry point
# ============================================================
function Test-AgentLabelHeadMatches {
    param(
        [Parameter(Mandatory)] [string]$PRNumber,
        [string]$ExpectedHeadSha = '',
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    if ([string]::IsNullOrWhiteSpace($ExpectedHeadSha)) {
        return $true
    }

    if ($ExpectedHeadSha -notmatch '^[0-9a-fA-F]{40}$') {
        Write-Host "  ⚠️  Refusing to apply labels because the reviewed commit is invalid." -ForegroundColor Yellow
        return $false
    }

    $currentHeadSha = gh api "repos/$Owner/$Repo/pulls/$PRNumber" --jq '.head.sha' 2>$null
    if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($currentHeadSha)) {
        Write-Host "  ⚠️  Could not verify the current PR head; leaving review labels unchanged." -ForegroundColor Yellow
        return $false
    }

    if (-not ([string]$currentHeadSha).Trim().Equals($ExpectedHeadSha, [StringComparison]::OrdinalIgnoreCase)) {
        Write-Host "  ⏭️  PR head advanced after this review snapshot; leaving review labels unchanged." -ForegroundColor Yellow
        return $false
    }

    return $true
}

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
        [ValidateSet('PASSED', 'SKIPPED', 'INCONCLUSIVE', 'FAILED', 'TIMEDOUT', '')]
        [string]$TrustedGateResult = '',
        [string]$ExpectedHeadSha = '',
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui'
    )

    Write-Host ""
    Write-Host "🏷️  Applying agent labels to PR #$PRNumber..." -ForegroundColor Cyan

    if (-not (Test-AgentLabelHeadMatches `
        -PRNumber $PRNumber `
        -ExpectedHeadSha $ExpectedHeadSha `
        -Owner $Owner `
        -Repo $Repo)) {
        return
    }

    # Parse phase outcomes from content.md files
    $outcomes = Parse-PhaseOutcomes `
        -PRNumber $PRNumber `
        -RepoRoot $RepoRoot `
        -TrustedGateResult $TrustedGateResult
    Write-Host "  📊 Parsed outcomes:" -ForegroundColor Gray
    Write-Host "     Outcome:    $($outcomes.Outcome ?? '(none)')" -ForegroundColor Gray
    Write-Host "     Gate:       $($outcomes.GateResult ?? '(skipped)')" -ForegroundColor Gray
    Write-Host "     Fix:        $($outcomes.FixResult ?? '(skipped)')" -ForegroundColor Gray

    try {
        # 1. Apply outcome label (exactly one)
        if ($outcomes.Outcome) {
            Update-AgentOutcomeLabel -PRNumber $PRNumber -Outcome $outcomes.Outcome -Owner $Owner -Repo $Repo
        } else {
            # A non-empty report without a canonical recommendation completed all phases,
            # but does not provide enough evidence for an outcome label. Remove stale labels
            # from earlier runs rather than falsely applying review-incomplete.
            Clear-AgentOutcomeLabels -PRNumber $PRNumber -Owner $Owner -Repo $Repo
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
