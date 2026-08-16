#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Exports current feedback from open testing-fork reproduction PRs.
#>

[CmdletBinding()]
param(
    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$RepositoryOwner = 'kubaflo',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$RepositoryName = 'maui',

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

function Invoke-FeedbackGhJson {
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
    return $json | ConvertFrom-Json -Depth 50
}

if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
    throw 'GH_TOKEN is required to export reproduction PR feedback.'
}

$authenticatedLogin = (& gh api user --jq '.login').Trim()
if ($LASTEXITCODE -ne 0 -or
    -not $authenticatedLogin.Equals('MauiBot', [StringComparison]::OrdinalIgnoreCase)) {
    throw "GH_TOKEN must authenticate as 'MauiBot'."
}

$repository = "$RepositoryOwner/$RepositoryName"
$markerPattern = '<!--\s*MAUI_COPILOT_REPLICATION\s+issue=(\d+)\s+platform=([a-z0-9-]+)\s*-->'
$pulls = @(
    Invoke-FeedbackGhJson `
        -Arguments @('api', "repos/$repository/pulls?state=open&per_page=100") `
        -Description "Listing open pull requests in $repository"
) | Where-Object {
    [regex]::IsMatch([string]$_.body, $markerPattern)
}

$exportedPulls = [Collections.Generic.List[object]]::new()
foreach ($pull in $pulls) {
    $marker = [regex]::Match([string]$pull.body, $markerPattern)
    $number = [int]$pull.number
    $exportedPulls.Add([ordered]@{
        number = $number
        url = [string]$pull.html_url
        title = [string]$pull.title
        issueNumber = [int]$marker.Groups[1].Value
        platform = $marker.Groups[2].Value
        draft = [bool]$pull.draft
        updatedAt = [string]$pull.updated_at
        headSha = [string]$pull.head.sha
        headRepository = [string]$pull.head.repo.full_name
        discussionComments = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/issues/$number/comments?per_page=100") `
                -Description "Listing discussion comments for $repository#$number"
        )
        reviews = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/pulls/$number/reviews?per_page=100") `
                -Description "Listing reviews for $repository#$number"
        )
        inlineComments = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/pulls/$number/comments?per_page=100") `
                -Description "Listing inline review comments for $repository#$number"
        )
        commits = @(
            Invoke-FeedbackGhJson `
                -Arguments @('api', "repos/$repository/pulls/$number/commits?per_page=100") `
                -Description "Listing commits for $repository#$number"
        ) | ForEach-Object {
            [ordered]@{
                sha = [string]$_.sha
                author = [string]$_.commit.author.name
                authoredAt = [string]$_.commit.author.date
                message = [string]$_.commit.message
            }
        }
    })
}

$output = [ordered]@{
    repository = $repository
    generatedAt = [DateTimeOffset]::UtcNow.ToString('O')
    pullRequestCount = $exportedPulls.Count
    pullRequests = @($exportedPulls)
}
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$output | ConvertTo-Json -Depth 50 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
Write-Host "Exported feedback for $($exportedPulls.Count) reproduction PR(s) to $OutputPath."
