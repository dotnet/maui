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

. "$PSScriptRoot/shared/Update-AgentLabels.ps1"

function Get-AgentItems {
    if (-not $env:GH_AW_AGENT_OUTPUT -or -not (Test-Path $env:GH_AW_AGENT_OUTPUT)) {
        throw "GH_AW_AGENT_OUTPUT is missing or does not exist."
    }

    $payload = Get-Content -Raw -LiteralPath $env:GH_AW_AGENT_OUTPUT | ConvertFrom-Json
    return @($payload.items | Where-Object { $_.type -eq 'trigger_rerun_review' })
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

    if ($Fallback) {
        return $Fallback
    }

    $lower = @($Labels | ForEach-Object { $_.ToLowerInvariant() })
    if ($lower -contains 'platform/ios') { return 'ios' }
    if ($lower -contains 'platform/macos' -or $lower -contains 'platform/maccatalyst') { return 'catalyst' }
    if ($lower -contains 'platform/android') { return 'android' }
    if ($lower -contains 'platform/windows') { return 'windows' }
    return 'android'
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

foreach ($item in $items) {
    $prNumber = [int]$item.pr_number
    $decision = ([string]$item.decision).Trim().ToLowerInvariant()
    $rerunCommentId = [Int64]$item.rerun_comment_id
    $reason = [string]$item.reason
    $expectedHeadSha = [string]$item.expected_head_sha

    if ($decision -notin @('trigger', 'skip')) {
        throw "Invalid decision '$decision' for PR #$prNumber."
    }
    if ($prNumber -le 0) {
        throw "Invalid PR number '$($item.pr_number)'."
    }
    if ($rerunCommentId -le 0) {
        throw "Invalid rerun comment id '$($item.rerun_comment_id)' for PR #$prNumber."
    }

    Write-Host "Processing PR #$prNumber decision=$decision reason=$reason"
    $pr = gh api "repos/$Owner/$Repo/pulls/$prNumber" | ConvertFrom-Json
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

    if ($decision -eq 'trigger') {
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

            Add-CommentReaction -CommentId $rerunCommentId -Content '+1'
            $platform = Get-PlatformFromLabels -Labels $labels -Fallback ([string]$item.platform)
            Invoke-AzDOReviewPipeline -PRNumber $prNumber -Platform $platform -PipelineRef $DefaultPipelineRef
        } catch {
            if ($lockApplied) {
                Clear-AgentReviewInProgress -PRNumber $prNumber -Owner $Owner -Repo $Repo | Out-Null
            }
            throw
        }
    } else {
        Add-CommentReaction -CommentId $rerunCommentId -Content '-1'
        Write-Host "  ⏭️ AI scanner decided not to trigger PR #$prNumber"
    }

    if ($DryRun) {
        Write-Host "[dry-run] Would remove $ReadyForRerunLabel from PR #$prNumber"
    } else {
        Remove-Label -PRNumber $prNumber -LabelName $ReadyForRerunLabel -Owner $Owner -Repo $Repo | Out-Null
        Write-Host "  ✅ Removed $ReadyForRerunLabel from PR #$prNumber"
    }
}
