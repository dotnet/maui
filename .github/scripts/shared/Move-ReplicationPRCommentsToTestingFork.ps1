#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Copies discussion and review feedback from migrated upstream reproduction PRs.

.DESCRIPTION
    GitHub cannot transfer comments while preserving their original authorship.
    This script posts attributed copies to matching testing-fork PRs and uses
    hidden source markers to make repeated runs idempotent.
#>

[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$SourceOwner = 'dotnet',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$SourceRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$TargetOwner = 'kubaflo',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$TargetRepository = 'maui',

    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

$replicationMarkerPattern = '<!--\s*MAUI_COPILOT_REPLICATION\s+issue=(\d+)\s+platform=([a-z0-9-]+)\s*-->'

function Invoke-ReplicationGhJson {
    param(
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description
    )

    $json = & gh @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
    if ([string]::IsNullOrWhiteSpace([string]$json)) {
        return $null
    }
    return $json | ConvertFrom-Json -Depth 40
}

function Get-ReplicationCommentSourceMarker {
    param(
        [Parameter(Mandatory = $true)][ValidateSet('issue', 'review', 'inline')][string]$Kind,
        [Parameter(Mandatory = $true)][long]$Id
    )

    return "<!-- MAUI_COPILOT_MIGRATED_COMMENT kind=$Kind id=$Id -->"
}

function Get-ReplicationPullRequestKey {
    param([Parameter(Mandatory = $true)]$PullRequest)

    $match = [regex]::Match([string]$PullRequest.body, $replicationMarkerPattern)
    if (-not $match.Success) {
        throw "Pull request #$($PullRequest.number) has no valid replication marker."
    }
    return "$($match.Groups[1].Value)/$($match.Groups[2].Value)"
}

function New-ReplicationMigratedCommentBody {
    param(
        [Parameter(Mandatory = $true)][ValidateSet('issue', 'review', 'inline')][string]$Kind,
        [Parameter(Mandatory = $true)]$Comment
    )

    $marker = Get-ReplicationCommentSourceMarker -Kind $Kind -Id ([long]$Comment.id)
    $author = if ([string]::IsNullOrWhiteSpace([string]$Comment.user.login)) {
        'unknown'
    } else {
        [string]$Comment.user.login
    }
    $createdAt = if ($Comment.PSObject.Properties['created_at']) {
        [string]$Comment.created_at
    } elseif ($Comment.PSObject.Properties['submitted_at']) {
        [string]$Comment.submitted_at
    } else {
        'unknown time'
    }
    $sourceUrl = [string]$Comment.html_url
    $body = [string]$Comment.body
    if ($body.Length -gt 60000) {
        $body = $body.Substring(0, 60000) + "`n`n_[Original comment truncated during migration.]_"
    }

    $kindLabel = switch ($Kind) {
        'issue' { 'discussion comment' }
        'review' { 'pull-request review' }
        'inline' { 'inline review comment' }
    }
    $location = ''
    if ($Kind -eq 'inline') {
        $path = [string]$Comment.path
        $line = if ($Comment.line) { [string]$Comment.line } else { [string]$Comment.original_line }
        if ($path) {
            $location = " Path: ``$path``"
            if ($line) {
                $location += ", line $line."
            } else {
                $location += '.'
            }
        }
    }

    return @"
$marker

> Migrated $kindLabel from **@$author** on ``$createdAt``. [View the original comment]($sourceUrl).$location

$body
"@
}

function Publish-ReplicationMigratedComment {
    param(
        [Parameter(Mandatory = $true)][string]$TargetRepositoryName,
        [Parameter(Mandatory = $true)][int]$TargetPullRequestNumber,
        [Parameter(Mandatory = $true)][string]$Body
    )

    $payload = @{ body = $Body } | ConvertTo-Json -Depth 5
    $result = $payload |
        & gh api -X POST "repos/$TargetRepositoryName/issues/$TargetPullRequestNumber/comments" --input -
    if ($LASTEXITCODE -ne 0) {
        throw "Posting a migrated comment to $TargetRepositoryName#$TargetPullRequestNumber failed."
    }
    return $result | ConvertFrom-Json -Depth 20
}

if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
    throw 'GH_TOKEN is required to migrate reproduction pull-request comments.'
}

$authenticatedLogin = (& gh api user --jq '.login').Trim()
if ($LASTEXITCODE -ne 0 -or
    -not $authenticatedLogin.Equals('MauiBot', [StringComparison]::OrdinalIgnoreCase)) {
    throw "GH_TOKEN must authenticate as 'MauiBot'."
}

$sourceRepositoryName = "$SourceOwner/$SourceRepository"
$targetRepositoryName = "$TargetOwner/$TargetRepository"
$sourcePulls = @(
    Invoke-ReplicationGhJson `
        -Arguments @('api', "repos/$sourceRepositoryName/pulls?state=closed&sort=updated&direction=desc&per_page=100") `
        -Description "Listing recently closed pull requests in $sourceRepositoryName"
) | Where-Object {
    [string]$_.user.login -eq 'MauiBot' -and
    [regex]::IsMatch([string]$_.body, $replicationMarkerPattern)
}
$targetPulls = @(
    Invoke-ReplicationGhJson `
        -Arguments @('api', "repos/$targetRepositoryName/pulls?state=open&per_page=100") `
        -Description "Listing open pull requests in $targetRepositoryName"
) | Where-Object {
    [regex]::IsMatch([string]$_.body, $replicationMarkerPattern)
}

$targetByKey = @{}
foreach ($pull in $targetPulls) {
    $targetByKey[(Get-ReplicationPullRequestKey -PullRequest $pull)] = $pull
}

$copied = 0
$skipped = 0
$failures = [Collections.Generic.List[string]]::new()
foreach ($sourcePull in $sourcePulls) {
    try {
        $key = Get-ReplicationPullRequestKey -PullRequest $sourcePull
        $targetPull = $targetByKey[$key]
        if (-not $targetPull) {
            throw "No open testing-fork PR matches upstream PR #$($sourcePull.number)."
        }

        $existingComments = @(
            Invoke-ReplicationGhJson `
                -Arguments @('api', "repos/$targetRepositoryName/issues/$($targetPull.number)/comments?per_page=100") `
                -Description "Listing existing comments for $targetRepositoryName#$($targetPull.number)"
        )
        $existingMarkers = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($existing in $existingComments) {
            foreach ($markerMatch in [regex]::Matches(
                [string]$existing.body,
                '<!--\s*MAUI_COPILOT_MIGRATED_COMMENT\s+kind=(?:issue|review|inline)\s+id=\d+\s*-->')) {
                [void]$existingMarkers.Add($markerMatch.Value)
            }
        }

        $sources = @(
            [pscustomobject]@{
                Kind = 'issue'
                Items = @(
                    Invoke-ReplicationGhJson `
                        -Arguments @('api', "repos/$sourceRepositoryName/issues/$($sourcePull.number)/comments?per_page=100") `
                        -Description "Listing discussion comments for upstream PR #$($sourcePull.number)"
                )
            },
            [pscustomobject]@{
                Kind = 'review'
                Items = @(
                    Invoke-ReplicationGhJson `
                        -Arguments @('api', "repos/$sourceRepositoryName/pulls/$($sourcePull.number)/reviews?per_page=100") `
                        -Description "Listing reviews for upstream PR #$($sourcePull.number)"
                ) | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_.body) }
            },
            [pscustomobject]@{
                Kind = 'inline'
                Items = @(
                    Invoke-ReplicationGhJson `
                        -Arguments @('api', "repos/$sourceRepositoryName/pulls/$($sourcePull.number)/comments?per_page=100") `
                        -Description "Listing inline comments for upstream PR #$($sourcePull.number)"
                )
            }
        )

        foreach ($source in $sources) {
            foreach ($comment in $source.Items) {
                $marker = Get-ReplicationCommentSourceMarker -Kind $source.Kind -Id ([long]$comment.id)
                if ($existingMarkers.Contains($marker)) {
                    $skipped++
                    continue
                }
                $body = New-ReplicationMigratedCommentBody -Kind $source.Kind -Comment $comment
                $published = Publish-ReplicationMigratedComment `
                    -TargetRepositoryName $targetRepositoryName `
                    -TargetPullRequestNumber ([int]$targetPull.number) `
                    -Body $body
                if ([string]::IsNullOrWhiteSpace([string]$published.html_url)) {
                    throw "GitHub did not return a URL for migrated $($source.Kind) comment $($comment.id)."
                }
                [void]$existingMarkers.Add($marker)
                $copied++
            }
        }
    }
    catch {
        $failures.Add("PR #$($sourcePull.number): $($_.Exception.Message)")
    }
}

$manifest = [ordered]@{
    sourceRepository = $sourceRepositoryName
    targetRepository = $targetRepositoryName
    sourcePullRequestCount = $sourcePulls.Count
    copiedCommentCount = $copied
    skippedCommentCount = $skipped
    failureCount = $failures.Count
    failures = @($failures)
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path ([IO.Path]::GetTempPath()) 'maui-replication-comment-migration.json'
}
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$manifest | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
Write-Host "Replication comment migration manifest: $OutputPath"
Write-Host "Copied $copied comment(s); skipped $skipped existing migrated comment(s)."

if ($failures.Count -gt 0) {
    throw "Failed to migrate comments for $($failures.Count) reproduction PR(s): $($failures -join '; ')"
}
