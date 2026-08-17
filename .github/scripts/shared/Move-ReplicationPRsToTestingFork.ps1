#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Recreates open MauiBot reproduction PRs in the testing fork, then closes upstream PRs.

.DESCRIPTION
    The operation is idempotent and fail-closed. An upstream PR is closed only
    after a matching PR exists in the testing repository.
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

    [ValidatePattern('^[A-Za-z0-9._/-]+$')]
    [string]$TargetBaseBranch = 'main',

    [string]$OutputPath = ''
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

. (Join-Path $PSScriptRoot 'Get-ReplicationGitHubLogin.ps1')

function Test-ReplicationPullRequestBody {
    param([AllowEmptyString()][string]$Body)

    return $Body -match '<!--\s*MAUI_COPILOT_REPLICATION\s+issue=\d+\s+platform=[a-z0-9-]+\s*-->'
}

function Get-ReplicationMigrationKey {
    param([Parameter(Mandatory = $true)]$PullRequest)

    $match = [regex]::Match(
        [string]$PullRequest.body,
        '<!--\s*MAUI_COPILOT_REPLICATION\s+issue=(\d+)\s+platform=([a-z0-9-]+)\s*-->')
    if (-not $match.Success) {
        throw "Pull request #$($PullRequest.number) has no valid replication marker."
    }
    return "$($match.Groups[1].Value)/$($match.Groups[2].Value)"
}

function Invoke-GitHubJson {
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
    return $json | ConvertFrom-Json -Depth 30
}

if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
    throw 'GH_TOKEN is required to migrate reproduction pull requests.'
}

$authenticatedLogin = Get-ReplicationGitHubLogin
if (-not $authenticatedLogin.Equals('MauiBot', [StringComparison]::OrdinalIgnoreCase)) {
    throw "GH_TOKEN must authenticate as 'MauiBot'."
}

$sourceRepositoryName = "$SourceOwner/$SourceRepository"
$targetRepositoryName = "$TargetOwner/$TargetRepository"
$sourcePulls = @(
    Invoke-GitHubJson `
        -Arguments @('api', "repos/$sourceRepositoryName/pulls?state=open&per_page=100") `
        -Description "Listing open pull requests in $sourceRepositoryName"
) | Where-Object {
    [string]$_.user.login -eq 'MauiBot' -and
    (Test-ReplicationPullRequestBody -Body ([string]$_.body))
}
$targetPulls = @(
    Invoke-GitHubJson `
        -Arguments @('api', "repos/$targetRepositoryName/pulls?state=open&per_page=100") `
        -Description "Listing open pull requests in $targetRepositoryName"
) | Where-Object {
    Test-ReplicationPullRequestBody -Body ([string]$_.body)
}

$targetByKey = @{}
foreach ($pull in $targetPulls) {
    $targetByKey[(Get-ReplicationMigrationKey -PullRequest $pull)] = $pull
}

$results = [Collections.Generic.List[object]]::new()
$failures = [Collections.Generic.List[string]]::new()
foreach ($pull in $sourcePulls) {
    try {
        $key = Get-ReplicationMigrationKey -PullRequest $pull
        $headRepository = [string]$pull.head.repo.full_name
        $headOwner = [string]$pull.head.repo.owner.login
        $headRef = [string]$pull.head.ref
        if ($headRepository -ne 'MauiBot/maui' -or
            $headOwner -ne 'MauiBot' -or
            [string]::IsNullOrWhiteSpace($headRef)) {
            throw "Upstream PR #$($pull.number) does not use a MauiBot/maui head branch."
        }

        $targetPull = $targetByKey[$key]
        if (-not $targetPull) {
            $payloadPath = Join-Path ([IO.Path]::GetTempPath()) "maui-replication-move-$($pull.number).json"
            try {
                [ordered]@{
                    title = [string]$pull.title
                    head = "$headOwner`:$headRef"
                    base = $TargetBaseBranch
                    body = [string]$pull.body
                    draft = $true
                } |
                    ConvertTo-Json -Depth 10 |
                    Set-Content -LiteralPath $payloadPath -Encoding utf8NoBOM
                $targetPull = Invoke-GitHubJson `
                    -Arguments @('api', '-X', 'POST', "repos/$targetRepositoryName/pulls", '--input', $payloadPath) `
                    -Description "Creating testing PR for upstream PR #$($pull.number)"
            }
            finally {
                Remove-Item -LiteralPath $payloadPath -Force -ErrorAction SilentlyContinue
            }
            $targetByKey[$key] = $targetPull
        }

        $targetUrl = [string]$targetPull.html_url
        if ([string]::IsNullOrWhiteSpace($targetUrl)) {
            throw "Testing PR for upstream PR #$($pull.number) has no URL."
        }

        $closePath = Join-Path ([IO.Path]::GetTempPath()) "maui-replication-close-$($pull.number).json"
        try {
            @{ state = 'closed' } |
                ConvertTo-Json |
                Set-Content -LiteralPath $closePath -Encoding utf8NoBOM
            $closedPull = Invoke-GitHubJson `
                -Arguments @('api', '-X', 'PATCH', "repos/$sourceRepositoryName/pulls/$($pull.number)", '--input', $closePath) `
                -Description "Closing upstream PR #$($pull.number)"
            if ([string]$closedPull.state -ne 'closed') {
                throw "GitHub did not report upstream PR #$($pull.number) as closed."
            }
        }
        finally {
            Remove-Item -LiteralPath $closePath -Force -ErrorAction SilentlyContinue
        }

        $results.Add([pscustomobject]@{
            key = $key
            sourceNumber = [int]$pull.number
            sourceUrl = [string]$pull.html_url
            targetUrl = $targetUrl
            state = 'migrated'
        })
    }
    catch {
        $failures.Add("PR #$($pull.number): $($_.Exception.Message)")
    }
}

$manifest = [ordered]@{
    sourceRepository = $sourceRepositoryName
    targetRepository = $targetRepositoryName
    migratedCount = $results.Count
    failureCount = $failures.Count
    migrated = @($results)
    failures = @($failures)
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path ([IO.Path]::GetTempPath()) 'maui-replication-pr-migration.json'
}
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$manifest | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
Write-Host "Replication PR migration manifest: $OutputPath"

if ($failures.Count -gt 0) {
    throw "Failed to migrate $($failures.Count) reproduction pull request(s): $($failures -join '; ')"
}
