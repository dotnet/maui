#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Determines whether a /review rerun request should mark a PR ready for rerun.

.DESCRIPTION
    This script is intentionally deterministic: it never uses AI and never
    inspects untrusted text semantically. A rerun is eligible only when there is
    new PR activity after the previous AI Summary or previous /review rerun:
    a new non-command comment, or a new commit.
#>

param(
    [Parameter(Mandatory = $true)]
    [int]$PRNumber,

    [Parameter(Mandatory = $true)]
    [Int64]$CurrentCommentId,

    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',

    [switch]$ApplyLabel
)

$ErrorActionPreference = 'Stop'
$AISummaryMarker = '<!-- AI Summary -->'
$ReadyForRerunLabel = 's/agent-ready-for-rerun'
$ReadyForRerunLabelDescription = 'AI review has new PR activity and is ready for rerun'
$ReadyForRerunLabelColor = '5319E7'

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

function Test-RerunCommand {
    param([string]$Body)

    return ([string]$Body).Trim() -match '(?i)^/review\s+rerun\s*$'
}

function Get-ObjectDate {
    param(
        [Parameter(Mandatory = $true)]$Object,
        [Parameter(Mandatory = $true)][string]$PropertyName
    )

    $value = $Object.$PropertyName
    if ($null -eq $value) {
        return $null
    }

    return ConvertTo-DateTimeOffset $value
}

function Get-LatestAISummaryComment {
    param([object[]]$Comments)

    return @($Comments |
        Where-Object { $_.body -and ([string]$_.body).Contains($AISummaryMarker) } |
        Sort-Object @{ Expression = { Get-ObjectDate $_ 'updated_at' }; Descending = $true }, @{ Expression = { [Int64]$_.id }; Descending = $true } |
        Select-Object -First 1)
}

function Get-LatestRerunCommentBefore {
    param(
        [object[]]$Comments,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId
    )

    $current = @($Comments | Where-Object { [Int64]$_.id -eq $CurrentCommentId } | Select-Object -First 1)
    if (-not $current) {
        return $null
    }

    $currentCreatedAt = Get-ObjectDate $current 'created_at'
    return @($Comments |
        Where-Object {
            [Int64]$_.id -ne $CurrentCommentId -and
            (Test-RerunCommand $_.body) -and
            (Get-ObjectDate $_ 'created_at') -lt $currentCreatedAt
        } |
        Sort-Object @{ Expression = { Get-ObjectDate $_ 'created_at' }; Descending = $true }, @{ Expression = { [Int64]$_.id }; Descending = $true } |
        Select-Object -First 1)
}

function Get-LatestReviewedSha {
    param([string]$AISummaryBody)

    if ([string]::IsNullOrWhiteSpace($AISummaryBody)) {
        return $null
    }

    $matches = [regex]::Matches($AISummaryBody, '<!--\s*SESSION:([0-9a-fA-F]{7,40})\s+START\s*-->')
    if ($matches.Count -eq 0) {
        return $null
    }

    return $matches[$matches.Count - 1].Groups[1].Value.ToLowerInvariant()
}

function Test-CommentIsEvidence {
    param(
        [Parameter(Mandatory = $true)]$Comment,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId
    )

    if ([Int64]$Comment.id -eq $CurrentCommentId) {
        return $false
    }
    if (Test-RerunCommand $Comment.body) {
        return $false
    }
    if ($Comment.user -and $Comment.user.type -eq 'Bot') {
        return $false
    }
    if ($Comment.user -and $Comment.user.login -match '(?i)^(maui-bot|github-actions)(\[bot\])?$') {
        return $false
    }

    return $true
}

function Test-HasEvidenceCommentAfter {
    param(
        [object[]]$Comments,
        [Parameter(Mandatory = $true)][datetimeoffset]$Checkpoint,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId
    )

    return [bool]@($Comments | Where-Object {
        (Test-CommentIsEvidence -Comment $_ -CurrentCommentId $CurrentCommentId) -and
        (Get-ObjectDate $_ 'created_at') -gt $Checkpoint
    } | Select-Object -First 1)
}

function Test-HasCommitAfter {
    param(
        [object[]]$Commits,
        [Parameter(Mandatory = $true)][datetimeoffset]$Checkpoint
    )

    return [bool]@($Commits | Where-Object {
        $date = $null
        if ($_.commit -and $_.commit.committer -and $_.commit.committer.date) {
            $date = ConvertTo-DateTimeOffset $_.commit.committer.date
        } elseif ($_.commit -and $_.commit.author -and $_.commit.author.date) {
            $date = ConvertTo-DateTimeOffset $_.commit.author.date
        }

        $date -and $date -gt $Checkpoint
    } | Select-Object -First 1)
}

function Test-HeadDiffersFromReviewedSha {
    param(
        [string]$CurrentHeadSha,
        [string]$LatestReviewedSha
    )

    if ([string]::IsNullOrWhiteSpace($CurrentHeadSha) -or [string]::IsNullOrWhiteSpace($LatestReviewedSha)) {
        return $false
    }

    return -not $CurrentHeadSha.ToLowerInvariant().StartsWith($LatestReviewedSha.ToLowerInvariant())
}

function ConvertTo-RerunActivityItem {
    param(
        [Parameter(Mandatory = $true)]$Item,
        [Parameter(Mandatory = $true)][string]$Kind
    )

    $createdAt = $Item.created_at
    if ($Kind -eq 'review') {
        $createdAt = $Item.submitted_at
    }

    $updatedAt = $Item.updated_at
    if ($null -eq $updatedAt) {
        $updatedAt = $createdAt
    }

    return [pscustomobject]@{
        id         = [Int64]$Item.id
        kind       = $Kind
        body       = [string]$Item.body
        created_at = $createdAt
        updated_at = $updatedAt
        user       = $Item.user
    }
}

function Resolve-RerunEligibility {
    param(
        [object[]]$Comments,
        [object[]]$Commits,
        [Parameter(Mandatory = $true)][Int64]$CurrentCommentId,
        [string]$CurrentHeadSha,
        [object[]]$CurrentLabels = @()
    )

    $current = @($Comments | Where-Object { [Int64]$_.id -eq $CurrentCommentId } | Select-Object -First 1)
    if (-not $current) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'current-comment-not-found'; Label = $ReadyForRerunLabel }
    }

    if (-not (Test-RerunCommand $current.body)) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'not-rerun-command'; Label = $ReadyForRerunLabel }
    }

    if ($current.user -and ($current.user.type -eq 'Bot' -or $current.user.login -match '(?i)^(maui-bot|github-actions)(\[bot\])?$')) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'bot-comment'; Label = $ReadyForRerunLabel }
    }

    $latestSummary = Get-LatestAISummaryComment -Comments $Comments
    if (-not $latestSummary) {
        return [pscustomobject]@{ Eligible = $false; Reason = 'no-ai-summary'; Label = $ReadyForRerunLabel }
    }

    if (@($CurrentLabels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0) {
        return [pscustomobject]@{ Eligible = $true; Reason = 'label-already-present'; Label = $ReadyForRerunLabel }
    }

    $summaryUpdatedAt = Get-ObjectDate $latestSummary 'updated_at'
    $latestReviewedSha = Get-LatestReviewedSha -AISummaryBody $latestSummary.body
    $previousRerun = Get-LatestRerunCommentBefore -Comments $Comments -CurrentCommentId $CurrentCommentId
    $checkpoint = $summaryUpdatedAt
    $checkpointReason = 'ai-summary'
    if ($previousRerun) {
        $previousRerunCreatedAt = Get-ObjectDate $previousRerun 'created_at'
        if ($previousRerunCreatedAt -gt $checkpoint) {
            $checkpoint = $previousRerunCreatedAt
            $checkpointReason = 'previous-rerun'
        }
    }

    if ($checkpointReason -eq 'ai-summary' -and (Test-HeadDiffersFromReviewedSha -CurrentHeadSha $CurrentHeadSha -LatestReviewedSha $latestReviewedSha)) {
        return [pscustomobject]@{ Eligible = $true; Reason = 'new-head-commit'; Label = $ReadyForRerunLabel }
    }

    if (Test-HasEvidenceCommentAfter -Comments $Comments -Checkpoint $checkpoint -CurrentCommentId $CurrentCommentId) {
        $reason = if ($checkpointReason -eq 'previous-rerun') { 'new-comment-after-previous-rerun' } else { 'new-comment-after-ai-summary' }
        return [pscustomobject]@{ Eligible = $true; Reason = $reason; Label = $ReadyForRerunLabel }
    }

    if (Test-HasCommitAfter -Commits $Commits -Checkpoint $checkpoint) {
        $reason = if ($checkpointReason -eq 'previous-rerun') { 'new-commit-after-previous-rerun' } else { 'new-commit-after-ai-summary' }
        return [pscustomobject]@{ Eligible = $true; Reason = $reason; Label = $ReadyForRerunLabel }
    }

    return [pscustomobject]@{ Eligible = $false; Reason = 'no-new-comments-or-commits'; Label = $ReadyForRerunLabel }
}

if ($MyInvocation.InvocationName -eq '.') {
    return
}

$issueComments = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/comments?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'issue-comment' })
$reviews = @(gh api "repos/$Owner/$Repo/pulls/$PRNumber/reviews?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review' })
$reviewComments = @(gh api "repos/$Owner/$Repo/pulls/$PRNumber/comments?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-RerunActivityItem -Item ($_ | ConvertFrom-Json) -Kind 'review-comment' })
$comments = @($issueComments + $reviews + $reviewComments)
$pr = gh api "repos/$Owner/$Repo/pulls/$PRNumber" | ConvertFrom-Json
$commits = @(gh api "repos/$Owner/$Repo/pulls/$PRNumber/commits?per_page=100" --paginate --jq '.[]' | ForEach-Object { $_ | ConvertFrom-Json })
$labels = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/labels" --jq '.[].name' 2>$null)

if ($pr.state -ne 'open') {
    throw "PR #$PRNumber is not open (state: $($pr.state))"
}

$result = Resolve-RerunEligibility `
    -Comments $comments `
    -Commits $commits `
    -CurrentCommentId $CurrentCommentId `
    -CurrentHeadSha $pr.head.sha `
    -CurrentLabels $labels

Write-Host "Rerun eligibility: $($result.Eligible) ($($result.Reason))"

if ($env:GITHUB_OUTPUT) {
    "eligible=$($result.Eligible.ToString().ToLowerInvariant())" >> $env:GITHUB_OUTPUT
    "reason=$($result.Reason)" >> $env:GITHUB_OUTPUT
    "label=$($result.Label)" >> $env:GITHUB_OUTPUT
}

if ($ApplyLabel -and $result.Eligible) {
    . "$PSScriptRoot/shared/Update-AgentLabels.ps1"
    Ensure-LabelExists `
        -LabelName $ReadyForRerunLabel `
        -Description $ReadyForRerunLabelDescription `
        -Color $ReadyForRerunLabelColor `
        -Owner $Owner `
        -Repo $Repo

    $alreadyPresent = @($labels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
    if ($alreadyPresent) {
        Write-Host "  ✅ Already present: $ReadyForRerunLabel" -ForegroundColor Green
    } else {
        $addSucceeded = Add-Label -PRNumber $PRNumber -LabelName $ReadyForRerunLabel -Owner $Owner -Repo $Repo
        $updatedLabels = @(gh api "repos/$Owner/$Repo/issues/$PRNumber/labels" --jq '.[].name' 2>$null)
        $labelIsPresent = @($updatedLabels | Where-Object { $_ -eq $ReadyForRerunLabel }).Count -gt 0
        if ($addSucceeded -or $labelIsPresent) {
            Write-Host "  ✅ Applied: $ReadyForRerunLabel" -ForegroundColor Green
        } else {
            throw "Failed to apply label: $ReadyForRerunLabel"
        }
    }
}
