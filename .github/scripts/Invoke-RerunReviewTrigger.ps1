#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Applies AI rerun scanner decisions: react, remove queue label, and trigger AzDO.
#>

param(
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [string]$DefaultPipelineRef = 'main',
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$ReadyForRerunLabel = 's/agent-ready-for-rerun'
$ReviewInProgressLabel = 's/agent-review-in-progress'
$ReviewTriggerCooldownMinutes = 60
$ReviewTriggerWindowHours = 24
$MaxReviewTriggersPerWindow = 3

. "$PSScriptRoot/shared/Update-AgentLabels.ps1"

function Expand-RerunDecisionItems {
    param([object[]]$Items)

    # A custom safe-output job is capped at one invocation per run, so the agent
    # now batches every candidate's decision into a single item's `decisions`
    # field (a JSON array, or array of objects). Expand that into one object per
    # decision. Items that already carry scalar decision fields (legacy shape or
    # a single decision) are passed through unchanged for back-compatibility.
    $expanded = [System.Collections.Generic.List[object]]::new()
    foreach ($item in $Items) {
        $rawDecisions = $item.PSObject.Properties['decisions']
        if (-not $rawDecisions -or $null -eq $rawDecisions.Value) {
            if ($item.PSObject.Properties['pr_number']) {
                $expanded.Add($item)
            }
            continue
        }

        $value = $rawDecisions.Value
        $parsed = $null
        if ($value -is [string]) {
            if ([string]::IsNullOrWhiteSpace($value)) { continue }
            try {
                $parsed = $value | ConvertFrom-Json
            } catch {
                Write-Host "::warning::Skipping unparseable decisions payload: $($_.Exception.Message)"
                continue
            }
        } else {
            $parsed = $value
        }

        foreach ($decision in @($parsed)) {
            if ($null -ne $decision) {
                $expanded.Add($decision)
            }
        }
    }

    return $expanded.ToArray()
}

function Get-AgentItems {
    if (-not $env:GH_AW_AGENT_OUTPUT -or -not (Test-Path $env:GH_AW_AGENT_OUTPUT)) {
        throw "GH_AW_AGENT_OUTPUT is missing or does not exist."
    }

    $payload = Get-Content -Raw -LiteralPath $env:GH_AW_AGENT_OUTPUT | ConvertFrom-Json
    $triggerItems = @($payload.items | Where-Object { $_.type -eq 'trigger_rerun_review' })
    return Expand-RerunDecisionItems -Items $triggerItems
}

function Get-CandidateItems {
    param([string]$Path)

    if ([string]::IsNullOrWhiteSpace($Path)) {
        throw "RERUN_CANDIDATES_PATH is required."
    }
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Rerun candidates file '$Path' does not exist."
    }

    $payload = Get-Content -Raw -LiteralPath $Path | ConvertFrom-Json
    return @($payload.candidates)
}

function Get-MatchingCandidate {
    param(
        [object[]]$Candidates,
        [Parameter(Mandatory = $true)][int]$PRNumber
    )

    return @($Candidates | Where-Object { [int]$_.prNumber -eq $PRNumber } | Select-Object -First 1)
}

function ConvertTo-SafeLogValue {
    param(
        [string]$Value,
        [int]$MaxLength = 180
    )

    $safe = ([string]$Value) -replace '[\r\n]+', ' '
    $safe = $safe -replace '::', ': :'
    $safe = $safe.Trim()
    if ($safe.Length -gt $MaxLength) {
        $safe = $safe.Substring(0, $MaxLength - 3) + '...'
    }

    return $safe
}

function Test-GhApiPrNotFound {
    param([string]$Output)

    if ([string]::IsNullOrWhiteSpace($Output)) {
        return $false
    }

    # Only treat an explicit HTTP 404/410 status as "deleted". gh emits a
    # structured error such as "gh: Not Found (HTTP 404)" for a genuinely
    # missing resource. The bare words "Not Found"/"Gone" are intentionally NOT
    # matched on their own: auth failures, proxy/firewall errors, and rate-limit
    # bodies can contain that text and previously caused open PRs to be falsely
    # classified as deleted, silently skipping every rerun dispatch.
    return $Output -match '(?i)\bHTTP\s+(404|410)\b'
}

function ConvertTo-TrimmedString {
    param([AllowNull()][object]$Value)

    if ($null -eq $Value) {
        return ''
    }

    return ([string]$Value).Trim()
}

function Get-PullRequestApiResult {
    param(
        [Parameter(Mandatory = $true)][string]$Owner,
        [Parameter(Mandatory = $true)][string]$Repo,
        [Parameter(Mandatory = $true)][int]$PRNumber
    )

    $stdErrFile = New-TemporaryFile
    try {
        $output = @(& gh api "repos/$Owner/$Repo/pulls/$PRNumber" 2> $stdErrFile)
        $exitCode = $LASTEXITCODE
        $json = ConvertTo-TrimmedString ($output | Out-String)
        $stdErr = ConvertTo-TrimmedString (Get-Content -Raw -LiteralPath $stdErrFile -ErrorAction SilentlyContinue)
    } finally {
        Remove-Item -LiteralPath $stdErrFile -Force -ErrorAction SilentlyContinue
    }

    return [pscustomobject]@{
        ExitCode = $exitCode
        Json     = $json
        StdErr   = $stdErr
    }
}

function Add-CommentReaction {
    param(
        [Parameter(Mandatory = $true)][Int64]$CommentId,
        [Parameter(Mandatory = $true)][ValidateSet('+1', '-1')][string]$Content
    )

    if ($DryRun) {
        Write-Host "[dry-run] Would react '$Content' to comment $CommentId"
        return
    }

    $tmp = New-TemporaryFile
    try {
        @{ content = $Content } | ConvertTo-Json -Compress | Set-Content -LiteralPath $tmp -Encoding utf8 -NoNewline
        & gh api "repos/$Owner/$Repo/issues/comments/$CommentId/reactions" `
            --method POST `
            -H "Accept: application/vnd.github+json" `
            --input $tmp 1>$null 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  ⚠️ Reaction '$Content' may already exist on comment $CommentId" -ForegroundColor Yellow
        } else {
            Write-Host "  ✅ Reacted '$Content' to comment $CommentId" -ForegroundColor Green
        }
    } finally {
        Remove-Item -LiteralPath $tmp -Force -ErrorAction SilentlyContinue
    }
}

function Get-PlatformFromLabels {
    param([string[]]$Labels, [string]$Fallback)

    $validPlatforms = @('android', 'ios', 'catalyst', 'windows')
    if ($Fallback -and $validPlatforms -contains $Fallback.ToLowerInvariant()) {
        return $Fallback.ToLowerInvariant()
    }

    $lower = @($Labels | ForEach-Object { $_.ToLowerInvariant() })
    if ($lower -contains 'platform/ios') { return 'ios' }
    if ($lower -contains 'platform/macos' -or $lower -contains 'platform/maccatalyst') { return 'catalyst' }
    if ($lower -contains 'platform/android') { return 'android' }
    if ($lower -contains 'platform/windows') { return 'windows' }
    return 'android'
}

function Normalize-PipelineRef {
    param([string]$Value, [string]$Fallback = 'main')

    $pipelineRef = if ([string]::IsNullOrWhiteSpace($Value)) { $Fallback } else { ([string]$Value).Trim() }
    $pipelineRef = $pipelineRef -replace '^refs/heads/', ''
    $pipelineRef = $pipelineRef -replace '[^a-zA-Z0-9/_.\-]', ''
    if ([string]::IsNullOrWhiteSpace($pipelineRef)) {
        return $Fallback
    }
    if ($pipelineRef -match '\.\.' -or $pipelineRef -match '//' -or $pipelineRef.EndsWith('/') -or $pipelineRef.StartsWith('/')) {
        return $Fallback
    }
    return $pipelineRef
}

function ConvertTo-DateTimeOffset {
    param([Parameter(Mandatory = $true)]$Value)

    if ($Value -is [datetimeoffset]) {
        return $Value
    }
    if ($Value -is [datetime]) {
        return [datetimeoffset]$Value
    }

    return [datetimeoffset]::Parse([string]$Value, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::AssumeUniversal)
}

function Get-ReviewTriggerRateLimitStatus {
    param(
        [datetimeoffset[]]$TriggeredAt = @(),
        [datetimeoffset]$Now = [datetimeoffset]::UtcNow,
        [int]$CooldownMinutes = $script:ReviewTriggerCooldownMinutes,
        [int]$WindowHours = $script:ReviewTriggerWindowHours,
        [int]$MaxTriggersPerWindow = $script:MaxReviewTriggersPerWindow
    )

    $sorted = @($TriggeredAt | Sort-Object -Descending)
    $latest = @($sorted | Select-Object -First 1)
    if ($latest.Count -gt 0) {
        $cooldownAge = $Now - $latest[0]
        if ($cooldownAge -lt [timespan]::FromMinutes($CooldownMinutes)) {
            return [pscustomobject]@{
                Allowed         = $false
                Reason          = "cooldown-active"
                LatestTriggered = $latest[0]
                RecentCount     = @($sorted | Where-Object { ($Now - $_) -lt [timespan]::FromHours($WindowHours) }).Count
            }
        }
    }

    $recent = @($sorted | Where-Object { ($Now - $_) -lt [timespan]::FromHours($WindowHours) })
    if ($recent.Count -ge $MaxTriggersPerWindow) {
        return [pscustomobject]@{
            Allowed         = $false
            Reason          = "rerun-quota-exhausted"
            LatestTriggered = if ($latest.Count -gt 0) { $latest[0] } else { $null }
            RecentCount     = $recent.Count
        }
    }

    return [pscustomobject]@{
        Allowed         = $true
        Reason          = "allowed"
        LatestTriggered = if ($latest.Count -gt 0) { $latest[0] } else { $null }
        RecentCount     = $recent.Count
    }
}

function Get-ReviewTriggerLabelTimes {
    param([Parameter(Mandatory = $true)][int]$PRNumber)

    $createdAtValues = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/events?per_page=100" --paginate --jq ".[] | select(.event == `"labeled`" and .label.name == `"$ReviewInProgressLabel`") | .created_at" 2>$null)
    if ($LASTEXITCODE -ne 0) {
        throw "Could not inspect $ReviewInProgressLabel history for PR #$PRNumber."
    }

    return @($createdAtValues | ForEach-Object { ConvertTo-DateTimeOffset $_ })
}

function Test-ReviewTriggerRateLimit {
    param([Parameter(Mandatory = $true)][int]$PRNumber)

    $triggeredAt = @(Get-ReviewTriggerLabelTimes -PRNumber $PRNumber)
    return Get-ReviewTriggerRateLimitStatus -TriggeredAt $triggeredAt
}

function Invoke-AzDOReviewPipeline {
    param(
        [Parameter(Mandatory = $true)][int]$PRNumber,
        [Parameter(Mandatory = $true)][string]$Platform,
        [string]$PipelineRef = 'main'
    )

    if ($DryRun) {
        Write-Host "[dry-run] Would trigger maui-copilot for PR #$PRNumber (platform: $Platform, ref: $PipelineRef)"
        return
    }

    if (-not $env:AZDO_TRIGGER_TENANT_ID -or -not $env:AZDO_TRIGGER_CLIENT_ID) {
        throw "AZDO_TRIGGER_TENANT_ID and AZDO_TRIGGER_CLIENT_ID secrets are required to trigger AzDO."
    }

    $oidcResponse = Invoke-RestMethod `
        -Headers @{ Authorization = "bearer $env:ACTIONS_ID_TOKEN_REQUEST_TOKEN" } `
        -Uri "$($env:ACTIONS_ID_TOKEN_REQUEST_URL)&audience=api://AzureADTokenExchange"
    $oidcToken = $oidcResponse.value
    if (-not $oidcToken) {
        throw "Failed to get GitHub OIDC token."
    }
    "::add-mask::$oidcToken" | Write-Host

    $aadResponse = Invoke-RestMethod `
        -Method Post `
        -Uri "https://login.microsoftonline.com/$($env:AZDO_TRIGGER_TENANT_ID)/oauth2/v2.0/token" `
        -Body @{
            grant_type             = 'client_credentials'
            client_id              = $env:AZDO_TRIGGER_CLIENT_ID
            client_assertion_type  = 'urn:ietf:params:oauth:client-assertion-type:jwt-bearer'
            client_assertion       = $oidcToken
            scope                  = '499b84ac-1321-427f-aa17-267ca6975798/.default'
        }

    $azdoToken = $aadResponse.access_token
    if (-not $azdoToken) {
        throw "Failed to get Azure DevOps token."
    }
    "::add-mask::$azdoToken" | Write-Host

    $payload = @{
        templateParameters = @{
            PRNumber = [string]$PRNumber
            Platform = $Platform
        }
        resources = @{
            repositories = @{
                self = @{
                    refName = "refs/heads/$PipelineRef"
                }
            }
        }
    } | ConvertTo-Json -Depth 10

    $response = Invoke-RestMethod `
        -Method Post `
        -Uri "https://dev.azure.com/DevDiv/DevDiv/_apis/pipelines/27723/runs?api-version=7.1" `
        -Headers @{ Authorization = "Bearer $azdoToken"; 'Content-Type' = 'application/json' } `
        -Body $payload

    Write-Host "  ✅ Triggered maui-copilot run $($response.id) for PR #$PRNumber"
}

$items = Get-AgentItems
if ($items.Count -eq 0) {
    Write-Host "No trigger_rerun_review decisions found."
    exit 0
}
$candidates = @(Get-CandidateItems -Path $env:RERUN_CANDIDATES_PATH)
$hadProcessingFailure = $false

foreach ($item in $items) {
    $prNumber = 0
    try {
        $prNumberRaw = ([string]$item.pr_number).Trim()
        if ($prNumberRaw -notmatch '^[1-9]\d*$') {
            throw "Invalid pr_number; expected positive integer string."
        }
        $prNumber = [int]$prNumberRaw
        $decision = ([string]$item.decision).Trim().ToLowerInvariant()
        $reason = [string]$item.reason
        $expectedHeadSha = ([string]$item.expected_head_sha).Trim()

        if ($decision -notin @('trigger', 'skip')) {
            throw "Invalid decision; expected 'trigger' or 'skip'."
        }
        if ([string]::IsNullOrWhiteSpace($expectedHeadSha)) {
            throw "Missing expected head SHA for $decision decision on PR #$prNumber."
        }
        $candidate = Get-MatchingCandidate -Candidates $candidates -PRNumber $prNumber
        if (-not $candidate) {
            throw "PR #$prNumber was not in the deterministic rerun candidate set."
        }
        if ([string]::IsNullOrWhiteSpace([string]$candidate.headSha)) {
            throw "Candidate for PR #$prNumber has no recorded head SHA."
        }
        if ([string]$candidate.headSha -ne $expectedHeadSha) {
            throw "PR #$prNumber decision head SHA does not match candidate head SHA."
        }

        # Source operational values from the deterministic candidate set, not from the agent's emission.
        # The agent's pipeline_ref / platform / rerun_comment_id fields are advisory only.
        $rerunCommentId = if ($candidate.rerunCommentId) { [Int64]$candidate.rerunCommentId } else { [Int64]0 }
        $candidatePlatformFallback = [string]$candidate.platform
        $candidatePipelineRef = Normalize-PipelineRef -Value ([string]$candidate.pipelineRef) -Fallback $DefaultPipelineRef

        if ($decision -eq 'trigger' -and $rerunCommentId -le 0) {
            throw "Candidate for PR #$prNumber has no rerun comment id; cannot trigger."
        }

        Write-Host "Processing PR #$prNumber decision=$decision reason=$(ConvertTo-SafeLogValue $reason)"
        $prFetch = Get-PullRequestApiResult -Owner $Owner -Repo $Repo -PRNumber $prNumber
        if ($prFetch.ExitCode -ne 0) {
            $prError = if ([string]::IsNullOrWhiteSpace($prFetch.StdErr)) { $prFetch.Json } else { $prFetch.StdErr }
            # Always surface the raw gh error so misclassified failures are
            # debuggable instead of being swallowed by the not-found guard.
            Write-Host "  ⚠️  gh api for PR #$prNumber failed (exit $($prFetch.ExitCode)): $(ConvertTo-SafeLogValue $prError)"
            if (Test-GhApiPrNotFound -Output $prError) {
                # Confirm with a second authenticated probe before treating the
                # PR as deleted. A single transient/auth/proxy 404 must not
                # silently cancel a real rerun dispatch.
                $confirm = Get-PullRequestApiResult -Owner $Owner -Repo $Repo -PRNumber $prNumber
                $confirmError = if ([string]::IsNullOrWhiteSpace($confirm.StdErr)) { $confirm.Json } else { $confirm.StdErr }
                if ($confirm.ExitCode -ne 0 -and (Test-GhApiPrNotFound -Output $confirmError)) {
                    $global:LASTEXITCODE = 0
                    Write-Host "  ⏭️ PR #$prNumber no longer exists (confirmed HTTP 404/410); skipping stale decision"
                    continue
                }
                if ($confirm.ExitCode -eq 0) {
                    # First 404 was transient; recheck recovered. Reuse the successful result.
                    $global:LASTEXITCODE = 0
                    $prFetch = $confirm
                    Write-Host "  ✓ PR #$prNumber recheck succeeded (transient 404 recovered)"
                } else {
                    throw "PR #$prNumber returned a not-found error that did not reproduce on re-check; refusing to silently skip. First: $(ConvertTo-SafeLogValue $prError); Recheck: $(ConvertTo-SafeLogValue $confirmError)"
                }
            } else {
                throw "Failed to load PR #$prNumber via gh api: $(ConvertTo-SafeLogValue $prError)"
            }
        }
        $prJson = $prFetch.Json
        if ([string]::IsNullOrWhiteSpace($prJson)) {
            throw "Failed to load PR #$prNumber via gh api: empty response."
        }
        try {
            $pr = $prJson | ConvertFrom-Json
        } catch {
            throw "Failed to parse PR #$prNumber response from gh api: $(ConvertTo-SafeLogValue ([string]$_))"
        }
        if ($pr.state -ne 'open') {
            Write-Host "  ⏭️ PR #$prNumber is not open ($($pr.state)); skipping"
            continue
        }
        if ($expectedHeadSha -and $pr.head.sha -ne $expectedHeadSha) {
            Write-Host "  ⏭️ PR #$prNumber head changed from $expectedHeadSha to $($pr.head.sha); skipping stale decision"
            continue
        }

        $labels = @(gh api "repos/$Owner/$Repo/issues/$prNumber/labels" --jq '.[].name' 2>$null)
        if ($labels -notcontains $ReadyForRerunLabel) {
            Write-Host "  ⏭️ PR #$prNumber no longer has $ReadyForRerunLabel; skipping"
            continue
        }
        if ($labels -contains $ReviewInProgressLabel) {
            if (Test-AgentReviewInProgressIsStale -PRNumber $prNumber -Owner $Owner -Repo $Repo) {
                if ($DryRun) {
                    Write-Host "[dry-run] Would remove stale $ReviewInProgressLabel from PR #$prNumber"
                } else {
                    Clear-AgentReviewInProgress -PRNumber $prNumber -Owner $Owner -Repo $Repo | Out-Null
                }
                $labels = @($labels | Where-Object { $_ -ne $ReviewInProgressLabel })
            } else {
                Write-Host "  ⏭️ PR #$prNumber already has $ReviewInProgressLabel; skipping duplicate review trigger"
                continue
            }
        }

        $preserveReadyLabel = $false
        if ($decision -eq 'trigger') {
            $rateLimit = Test-ReviewTriggerRateLimit -PRNumber $prNumber
            if (-not $rateLimit.Allowed) {
                $latestText = if ($rateLimit.LatestTriggered) { $rateLimit.LatestTriggered.ToString('u') } else { 'never' }
                Write-Host "  ⏭️ PR #$prNumber rerun trigger blocked by deterministic rate limit ($($rateLimit.Reason); recent=$($rateLimit.RecentCount); latest=$latestText); leaving $ReadyForRerunLabel in place for a future scan"
                $preserveReadyLabel = $true
            } else {
                $lockApplied = $false
                try {
                    if ($DryRun) {
                        Write-Host "[dry-run] Would apply $ReviewInProgressLabel to PR #$prNumber"
                    } else {
                        $lockApplied = Set-AgentReviewInProgress -PRNumber $prNumber -Owner $Owner -Repo $Repo
                        if (-not $lockApplied) {
                            throw "Failed to apply $ReviewInProgressLabel to PR #$prNumber; refusing to trigger duplicate-prone review."
                        }
                    }

                    $platform = Get-PlatformFromLabels -Labels $labels -Fallback $candidatePlatformFallback
                    Invoke-AzDOReviewPipeline -PRNumber $prNumber -Platform $platform -PipelineRef $candidatePipelineRef
                    if ($rerunCommentId -gt 0) {
                        try {
                            Add-CommentReaction -CommentId $rerunCommentId -Content '+1'
                        } catch {
                            Write-Host "::warning::Triggered PR #$prNumber but failed to react '+1' to comment $rerunCommentId`: $(ConvertTo-SafeLogValue ([string]$_))"
                        }
                    }
                } catch {
                    if ($lockApplied) {
                        Clear-AgentReviewInProgress -PRNumber $prNumber -Owner $Owner -Repo $Repo | Out-Null
                    }
                    throw
                }
            }
        } else {
            if ($rerunCommentId -gt 0) {
                try {
                    Add-CommentReaction -CommentId $rerunCommentId -Content '-1'
                } catch {
                    Write-Host "::warning::Skipped PR #$prNumber but failed to react '-1' to comment $rerunCommentId`: $(ConvertTo-SafeLogValue ([string]$_))"
                }
            } else {
                Write-Host "  ⏭️ No rerun comment id was provided; skipping '-1' reaction"
            }
            Write-Host "  ⏭️ AI scanner decided not to trigger PR #$prNumber"
        }

        if ($preserveReadyLabel) {
            Write-Host "  ℹ️ Leaving $ReadyForRerunLabel on PR #$prNumber for a future scan"
        } elseif ($DryRun) {
            Write-Host "[dry-run] Would remove $ReadyForRerunLabel from PR #$prNumber"
        } else {
            Remove-Label -PRNumber $prNumber -LabelName $ReadyForRerunLabel -Owner $Owner -Repo $Repo | Out-Null
            Write-Host "  ✅ Removed $ReadyForRerunLabel from PR #$prNumber"
        }
    } catch {
        $target = if ($prNumber -gt 0) { "PR #$prNumber" } else { "agent decision" }
        Write-Host "::error::Failed to process $target`: $(ConvertTo-SafeLogValue ([string]$_))"
        $hadProcessingFailure = $true
        continue
    }
}

if ($hadProcessingFailure) {
    exit 1
}

exit 0
