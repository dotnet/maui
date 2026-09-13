#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds bounded context for PRs queued with s/agent-ready-for-rerun.
#>

param(
    [int]$MaxPRs = 5,
    [string]$Owner = 'dotnet',
    [string]$Repo = 'maui',
    [string]$OutputPath = "CustomAgentLogsTmp/RerunScanner/candidates.json"
)

$ErrorActionPreference = 'Stop'
$ReadyForRerunLabel = 's/agent-ready-for-rerun'
$ReviewInProgressLabel = 's/agent-review-in-progress'

. "$PSScriptRoot/Resolve-RerunEligibility.ps1" -Owner $Owner -Repo $Repo
. "$PSScriptRoot/shared/Update-AgentLabels.ps1"

function ConvertTo-ActivityItemFromJson {
    param(
        [Parameter(Mandatory = $true)]$JsonItem,
        [Parameter(Mandatory = $true)][string]$Kind
    )

    return ConvertTo-RerunActivityItem -Item $JsonItem -Kind $Kind
}

function Get-IssueLabels {
    param([int]$Number)

    return @(gh api "repos/$Owner/$Repo/issues/$Number/labels" --jq '.[].name' 2>$null)
}

function Get-ActivityForPR {
    param([int]$Number)

    $issueComments = @(gh api "repos/$Owner/$Repo/issues/$Number/comments?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-ActivityItemFromJson -JsonItem ($_ | ConvertFrom-Json) -Kind 'issue-comment' })
    $reviews = @(gh api "repos/$Owner/$Repo/pulls/$Number/reviews?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-ActivityItemFromJson -JsonItem ($_ | ConvertFrom-Json) -Kind 'review' })
    $reviewComments = @(gh api "repos/$Owner/$Repo/pulls/$Number/comments?per_page=100" --paginate --jq '.[]' | ForEach-Object { ConvertTo-ActivityItemFromJson -JsonItem ($_ | ConvertFrom-Json) -Kind 'review-comment' })
    return @($issueComments + $reviews + $reviewComments)
}

function Get-CommitsForPR {
    param([int]$Number)

    return @(gh api "repos/$Owner/$Repo/pulls/$Number/commits?per_page=100" --paginate --jq '.[]' | ForEach-Object { $_ | ConvertFrom-Json })
}

function Get-PlatformFromLabels {
    param([string[]]$Labels)

    $lower = @($Labels | ForEach-Object { $_.ToLowerInvariant() })
    if ($lower -contains 'platform/ios') { return 'ios' }
    if ($lower -contains 'platform/macos' -or $lower -contains 'platform/maccatalyst') { return 'catalyst' }
    if ($lower -contains 'platform/android') { return 'android' }
    if ($lower -contains 'platform/windows') { return 'windows' }
    return 'android'
}

$searchJson = gh pr list `
    --repo "$Owner/$Repo" `
    --state open `
    --label $ReadyForRerunLabel `
    --limit $MaxPRs `
    --json number,title,url,headRefOid,isDraft,labels,author
if ($LASTEXITCODE -ne 0) {
    throw "Failed to list open PRs labeled '$ReadyForRerunLabel' (gh pr list exited with code $LASTEXITCODE)."
}
$searchResult = $searchJson | ConvertFrom-Json

$candidates = @()
foreach ($pr in @($searchResult)) {
    $number = [int]$pr.number
    $labels = @(Get-IssueLabels -Number $number)
    if ($labels -notcontains $ReadyForRerunLabel) {
        continue
    }
    if ($labels -contains $ReviewInProgressLabel -and -not (Test-AgentReviewInProgressIsStale -PRNumber $number -Owner $Owner -Repo $Repo)) {
        continue
    }

    $activity = @(Get-ActivityForPR -Number $number)
    $commits = @(Get-CommitsForPR -Number $number)
    $latestRerun = Get-LatestRerunComment -Comments $activity
    $reviewOptions = Get-LatestReviewCommandOptions -Comments $activity -Owner $Owner -Repo $Repo
    $rawAuthorLogin = if ($pr.author -and $pr.author.login) { [string]$pr.author.login } else { '' }
    $authorLogin = Normalize-GitHubActorLogin $rawAuthorLogin
    $contextMarkdown = New-RerunContextMarkdown -Comments $activity -Commits $commits -CurrentHeadSha $pr.headRefOid -PRAuthorLogin $authorLogin -CurrentLabels $labels
    $platform = if ($reviewOptions.Platform) { $reviewOptions.Platform } else { Get-PlatformFromLabels -Labels $labels }
    $pipelineRef = if ($reviewOptions.PipelineRef) { $reviewOptions.PipelineRef } else { 'main' }

    $candidates += [pscustomobject]@{
        prNumber        = $number
        title           = [string]$pr.title
        url             = [string]$pr.url
        authorLogin     = $authorLogin
        isDraft         = [bool]$pr.isDraft
        headSha         = [string]$pr.headRefOid
        platform        = $platform
        pipelineRef     = $pipelineRef
        reviewCommandId = $reviewOptions.CommentId
        reviewCommand   = $reviewOptions.Body
        labels          = $labels
        rerunCommentId  = if ($latestRerun) { [Int64]$latestRerun.id } else { $null }
        contextMarkdown = $contextMarkdown
    }
}

$outputDir = Split-Path -Parent $OutputPath
if ($outputDir) {
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
}

$json = @{ generatedAt = (Get-Date).ToUniversalTime().ToString('o'); candidates = @($candidates) } | ConvertTo-Json -Depth 20
$json | Set-Content -LiteralPath $OutputPath -Encoding UTF8

Write-Host "Wrote $($candidates.Count) rerun-ready candidate(s) to $OutputPath"
Write-Output $json
