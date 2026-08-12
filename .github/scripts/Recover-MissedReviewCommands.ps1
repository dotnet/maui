#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Recovers authorized /review comments missed by GitHub Actions webhooks.

.DESCRIPTION
    Polls recent repository issue comments, finds unprocessed /review commands,
    verifies the commenter still has write access, dispatches review-trigger.yml,
    and marks the source comment so delayed webhook delivery cannot double-trigger.

    The script only reads trusted main when invoked by review-trigger-recovery.yml.
    It never checks out or executes pull request code.
#>

param(
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [int]$LookbackHours = 24,
    [int]$MinimumAgeMinutes = 25,
    [int]$MaxRecoveries = 5,
    [datetimeoffset]$NotBefore = [datetimeoffset]::MinValue,
    [switch]$DryRun
)

$notBeforeWasSpecified = $PSBoundParameters.ContainsKey('NotBefore')
$ErrorActionPreference = 'Stop'

. "$PSScriptRoot/Resolve-RerunEligibility.ps1" -Owner $Owner -Repo $Repo

$script:RecoveryMarker = 'rocket'
$script:RecoveryMarkerActor = 'github-actions[bot]'

function ConvertTo-RecoveryErrorText {
    param([AllowNull()][object]$Value)

    $text = (([string]$Value) -replace '[\r\n]+', ' ').Trim()
    if ($text.Length -gt 500) {
        return $text.Substring(0, 497) + '...'
    }

    return $text
}

function Invoke-ReviewRecoveryGhApi {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [switch]$AllowNotFound
    )

    $stderrFile = New-TemporaryFile
    try {
        $raw = (& gh api @Arguments 2>$stderrFile | Out-String)
        $exitCode = $LASTEXITCODE
        $stderr = Get-Content -Raw -LiteralPath $stderrFile -ErrorAction SilentlyContinue
    } finally {
        Remove-Item -LiteralPath $stderrFile -Force -ErrorAction SilentlyContinue
    }

    if ($exitCode -ne 0) {
        $detail = ConvertTo-RecoveryErrorText $stderr
        if ($AllowNotFound -and $detail -match '(?i)\bHTTP\s+(404|410)\b') {
            return $null
        }
        if ([string]::IsNullOrWhiteSpace($detail)) {
            $detail = "gh api exited with code $exitCode."
        }

        throw "GitHub API request failed: $detail"
    }

    if ([string]::IsNullOrWhiteSpace($raw)) {
        return $null
    }

    try {
        return $raw | ConvertFrom-Json
    } catch {
        throw "GitHub API returned invalid JSON: $(ConvertTo-RecoveryErrorText $_)"
    }
}

function Get-RecentIssueComments {
    param(
        [string]$Owner,
        [string]$Repo,
        [int]$LookbackHours,
        [datetimeoffset]$Now = [datetimeoffset]::UtcNow,
        [int]$MaxPages = 50
    )

    $since = $Now.AddHours(-$LookbackHours).UtcDateTime.ToString('yyyy-MM-ddTHH:mm:ssZ')
    $encodedSince = [uri]::EscapeDataString($since)
    $comments = [System.Collections.Generic.List[object]]::new()

    for ($page = 1; $page -le $MaxPages; $page++) {
        $endpoint = "repos/$Owner/$Repo/issues/comments?sort=created&direction=desc&since=$encodedSince&per_page=100&page=$page"
        $pageItems = @(Invoke-ReviewRecoveryGhApi -Arguments @($endpoint))
        foreach ($comment in $pageItems) {
            if ($null -ne $comment) {
                $comments.Add($comment)
            }
        }

        if ($pageItems.Count -lt 100) {
            return $comments.ToArray()
        }
    }

    throw "Recent issue comment scan exceeded $MaxPages pages; refusing to recover from a truncated result."
}

function Get-ReviewRecoveryDeploymentEpoch {
    param(
        [string]$Owner,
        [string]$Repo,
        [int]$LookbackHours,
        [datetimeoffset]$Now = [datetimeoffset]::UtcNow
    )

    $firstPage = Invoke-ReviewRecoveryGhApi -Arguments @(
        "repos/$Owner/$Repo/actions/workflows/review-trigger-recovery.yml/runs?event=schedule&per_page=100&page=1"
    )
    $totalCount = [int]$firstPage.total_count
    if ($totalCount -eq 0) {
        return $Now
    }

    # GitHub Actions schedules cannot run more frequently than every five
    # minutes. Once more than this many runs exist, deployment is necessarily
    # older than the comment lookback and no additional lower bound is needed.
    $maximumRunsInLookback = $LookbackHours * 12
    if ($totalCount -gt $maximumRunsInLookback) {
        return [datetimeoffset]::MinValue
    }

    $oldestPage = [int][math]::Ceiling($totalCount / 100.0)
    $page = if ($oldestPage -eq 1) {
        $firstPage
    } else {
        Invoke-ReviewRecoveryGhApi -Arguments @(
            "repos/$Owner/$Repo/actions/workflows/review-trigger-recovery.yml/runs?event=schedule&per_page=100&page=$oldestPage"
        )
    }

    $oldestRun = @($page.workflow_runs | Sort-Object {
        ConvertTo-DateTimeOffset $_.created_at
    })[0]
    if (-not $oldestRun -or [string]::IsNullOrWhiteSpace([string]$oldestRun.head_sha)) {
        throw "Could not determine the review-trigger recovery deployment epoch from $totalCount workflow runs."
    }

    $deploymentCommit = Invoke-ReviewRecoveryGhApi -Arguments @(
        "repos/$Owner/$Repo/commits/$($oldestRun.head_sha)"
    )
    $deploymentEpoch = ConvertTo-DateTimeOffset $deploymentCommit.commit.committer.date
    if ($deploymentEpoch -gt $Now) {
        throw "Review-trigger recovery deployment epoch '$deploymentEpoch' is in the future."
    }

    return $deploymentEpoch
}

function Select-ReviewCommandCandidates {
    param(
        [object[]]$Comments,
        [int]$LookbackHours,
        [int]$MinimumAgeMinutes,
        [datetimeoffset]$NotBefore = [datetimeoffset]::MinValue,
        [datetimeoffset]$Now = [datetimeoffset]::UtcNow
    )

    $oldestAllowed = $Now.AddHours(-$LookbackHours)
    if ($NotBefore -gt $oldestAllowed) {
        $oldestAllowed = $NotBefore
    }
    $newestAllowed = $Now.AddMinutes(-$MinimumAgeMinutes)
    $candidates = [System.Collections.Generic.List[object]]::new()

    foreach ($comment in @($Comments)) {
        if ($null -eq $comment) {
            continue
        }

        $createdAt = ConvertTo-DateTimeOffset $comment.created_at
        if ($createdAt -lt $oldestAllowed -or $createdAt -gt $newestAllowed) {
            continue
        }

        $parsed = ConvertFrom-ReviewCommand ([string]$comment.body)
        if (-not $parsed) {
            continue
        }

        $issueUrl = [string]$comment.issue_url
        if ($issueUrl -notmatch '/issues/([1-9][0-9]*)$') {
            continue
        }
        $prNumber = [int]$Matches[1]

        $authorLogin = if ($comment.user) { [string]$comment.user.login } else { '' }
        $nodeId = [string]$comment.node_id
        if ([string]::IsNullOrWhiteSpace($authorLogin) -or [string]::IsNullOrWhiteSpace($nodeId)) {
            continue
        }

        $candidates.Add([pscustomobject]@{
            CommentId     = [Int64]$comment.id
            CommentNodeId = $nodeId
            PRNumber      = $prNumber
            AuthorLogin   = $authorLogin
            CreatedAt     = $createdAt
            Platform      = [string]$parsed.Platform
            PipelineRef   = [string]$parsed.PipelineRef
        })
    }

    return @($candidates | Sort-Object CreatedAt, CommentId)
}

function Test-ReviewCommentIsMinimized {
    param([Parameter(Mandatory = $true)][string]$NodeId)

    $query = 'query($id:ID!){node(id:$id){... on IssueComment{isMinimized}}}'
    $response = Invoke-ReviewRecoveryGhApi -Arguments @(
        'graphql',
        '-f', "query=$query",
        '-f', "id=$NodeId"
    )

    if ($null -eq $response.data.node) {
        throw "Could not resolve issue comment node '$NodeId'."
    }

    return [bool]$response.data.node.isMinimized
}

function Test-ReviewCommentHasRecoveryMarker {
    param(
        [string]$Owner,
        [string]$Repo,
        [Parameter(Mandatory = $true)][Int64]$CommentId
    )

    $reactions = [System.Collections.Generic.List[object]]::new()
    for ($page = 1; ; $page++) {
        $pageReactions = @(Invoke-ReviewRecoveryGhApi -Arguments @(
            "repos/$Owner/$Repo/issues/comments/$CommentId/reactions?per_page=100&page=$page",
            '-H', 'Accept: application/vnd.github+json'
        ))

        foreach ($reaction in $pageReactions) {
            if ($null -ne $reaction) {
                $reactions.Add($reaction)
            }
        }

        if ($pageReactions.Count -lt 100) {
            break
        }
    }

    return @($reactions | Where-Object {
        $_.content -eq $script:RecoveryMarker -and
        $_.user -and
        $_.user.login -eq $script:RecoveryMarkerActor
    }).Count -gt 0
}

function Get-ReviewRecoveryPullRequest {
    param(
        [string]$Owner,
        [string]$Repo,
        [Parameter(Mandatory = $true)][int]$PRNumber
    )

    return Invoke-ReviewRecoveryGhApi `
        -Arguments @("repos/$Owner/$Repo/pulls/$PRNumber") `
        -AllowNotFound
}

function Invoke-ReviewWorkflowDispatch {
    param(
        [string]$Owner,
        [string]$Repo,
        [Parameter(Mandatory = $true)][int]$PRNumber,
        [string]$Platform,
        [string]$PipelineRef,
        [Parameter(Mandatory = $true)][Int64]$CommentId,
        [Parameter(Mandatory = $true)][string]$CommentNodeId
    )

    $payloadPath = New-TemporaryFile
    try {
        [ordered]@{
            ref = 'main'
            inputs = [ordered]@{
                pr_number = [string]$PRNumber
                platform = [string]$Platform
                pipeline_ref = [string]$PipelineRef
                source_comment_id = [string]$CommentId
                source_comment_node_id = $CommentNodeId
            }
        } |
            ConvertTo-Json -Depth 5 |
            Set-Content -LiteralPath $payloadPath -Encoding utf8 -NoNewline

        Invoke-ReviewRecoveryGhApi -Arguments @(
            '--method', 'POST',
            "repos/$Owner/$Repo/actions/workflows/review-trigger.yml/dispatches",
            '--input', $payloadPath
        ) | Out-Null
    } finally {
        Remove-Item -LiteralPath $payloadPath -Force -ErrorAction SilentlyContinue
    }
}

function Invoke-MissedReviewCommandRecovery {
    param(
        [string]$Owner = 'dotnet',
        [string]$Repo = 'maui',
        [int]$LookbackHours = 24,
        [int]$MinimumAgeMinutes = 25,
        [int]$MaxRecoveries = 5,
        [datetimeoffset]$NotBefore = [datetimeoffset]::MinValue,
        [switch]$DryRun,
        [datetimeoffset]$Now = [datetimeoffset]::UtcNow
    )

    if ($LookbackHours -lt 1 -or $LookbackHours -gt 168) {
        throw 'LookbackHours must be between 1 and 168.'
    }
    if ($MinimumAgeMinutes -lt 1 -or $MinimumAgeMinutes -gt 120) {
        throw 'MinimumAgeMinutes must be between 1 and 120.'
    }
    if ($MaxRecoveries -lt 1 -or $MaxRecoveries -gt 20) {
        throw 'MaxRecoveries must be between 1 and 20.'
    }
    if ($NotBefore -gt $Now) {
        throw 'NotBefore cannot be in the future.'
    }

    Clear-ReviewOptionPermissionCache
    $comments = @(Get-RecentIssueComments -Owner $Owner -Repo $Repo -LookbackHours $LookbackHours -Now $Now)
    $candidates = @(Select-ReviewCommandCandidates `
        -Comments $comments `
        -LookbackHours $LookbackHours `
        -MinimumAgeMinutes $MinimumAgeMinutes `
        -NotBefore $NotBefore `
        -Now $Now)
    $recovered = [System.Collections.Generic.List[object]]::new()

    foreach ($candidate in $candidates) {
        if ($recovered.Count -ge $MaxRecoveries) {
            break
        }

        if (Test-ReviewCommentIsMinimized -NodeId $candidate.CommentNodeId) {
            continue
        }
        if (Test-ReviewCommentHasRecoveryMarker -Owner $Owner -Repo $Repo -CommentId $candidate.CommentId) {
            continue
        }

        $pullRequest = Get-ReviewRecoveryPullRequest -Owner $Owner -Repo $Repo -PRNumber $candidate.PRNumber
        if (-not $pullRequest -or [string]$pullRequest.state -ne 'open') {
            continue
        }

        if (-not (Test-ReviewOptionLoginTrusted `
            -Login $candidate.AuthorLogin `
            -Owner $Owner `
            -Repo $Repo)) {
            continue
        }

        $acknowledgementPending = -not $DryRun
        if ($DryRun) {
            Write-Host "[dry-run] Would recover comment $($candidate.CommentId) for PR #$($candidate.PRNumber)."
        } else {
            Invoke-ReviewWorkflowDispatch `
                -Owner $Owner `
                -Repo $Repo `
                -PRNumber $candidate.PRNumber `
                -Platform $candidate.Platform `
                -PipelineRef $candidate.PipelineRef `
                -CommentId $candidate.CommentId `
                -CommentNodeId $candidate.CommentNodeId

            Write-Host "Dispatched comment $($candidate.CommentId) for PR #$($candidate.PRNumber); the serialized review-trigger workflow will acknowledge it after dedupe."
        }

        $recovered.Add([pscustomobject]@{
            CommentId = $candidate.CommentId
            PRNumber = $candidate.PRNumber
            Platform = $candidate.Platform
            PipelineRef = $candidate.PipelineRef
            AcknowledgementPending = $acknowledgementPending
            DryRun = [bool]$DryRun
        })
    }

    return [pscustomobject]@{
        CommentsScanned = $comments.Count
        Candidates = $candidates.Count
        Recovered = $recovered.ToArray()
        DryRun = [bool]$DryRun
    }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

if (-not $notBeforeWasSpecified) {
    $NotBefore = Get-ReviewRecoveryDeploymentEpoch `
        -Owner $Owner `
        -Repo $Repo `
        -LookbackHours $LookbackHours
}

$result = Invoke-MissedReviewCommandRecovery `
    -Owner $Owner `
    -Repo $Repo `
    -LookbackHours $LookbackHours `
    -MinimumAgeMinutes $MinimumAgeMinutes `
    -MaxRecoveries $MaxRecoveries `
    -NotBefore $NotBefore `
    -DryRun:$DryRun

$mode = if ($DryRun) { 'dry run' } else { 'apply' }
Write-Host "Review trigger recovery ($mode): scanned=$($result.CommentsScanned) candidates=$($result.Candidates) recovered=$($result.Recovered.Count)"

if ($env:GITHUB_STEP_SUMMARY) {
    @"
## Review trigger recovery

- Mode: $mode
- Comments scanned: $($result.CommentsScanned)
- Review command candidates: $($result.Candidates)
- Commands recovered: $($result.Recovered.Count)
"@ | Add-Content -LiteralPath $env:GITHUB_STEP_SUMMARY -Encoding utf8
}
