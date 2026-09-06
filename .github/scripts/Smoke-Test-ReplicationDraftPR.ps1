#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates, verifies, and closes a MauiBot draft PR to prove publication works.

.DESCRIPTION
    This is a manual transport smoke test. It runs only on a clean trusted
    checkout, creates a temporary branch in MauiBot's fork, opens a draft PR
    against the testing fork, verifies its identity and routing, then closes the
    PR and deletes the branch. GitHub retains the closed PR record as evidence.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$ParentOwner = 'dotnet',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$ParentRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$SourceOwner = 'MauiBot',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$SourceRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9](?:[A-Za-z0-9-]{0,38})$')]
    [string]$TargetOwner = 'kubaflo',

    [ValidatePattern('^[A-Za-z0-9._-]+$')]
    [string]$TargetRepository = 'maui',

    [ValidatePattern('^[A-Za-z0-9._/-]+$')]
    [string]$BaseBranch = 'main'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

. (Join-Path $PSScriptRoot 'Get-ReplicationGitHubLogin.ps1')
. (Join-Path $PSScriptRoot 'Open-ReplicationDraftPullRequest.ps1')

function Invoke-ReplicationSmokeCommand {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [Parameter(Mandatory = $true)][string[]]$Arguments,
        [Parameter(Mandatory = $true)][string]$Description
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

$buildId = if ($env:BUILD_BUILDID -match '^[1-9]\d*$') {
    $env:BUILD_BUILDID
} else {
    [DateTimeOffset]::UtcNow.ToUnixTimeSeconds().ToString()
}
$attempt = if ($env:SYSTEM_JOBATTEMPT -match '^[1-9]\d*$') {
    $env:SYSTEM_JOBATTEMPT
} else {
    '1'
}
$branchName = "copilot/publication-smoke-$buildId-$attempt"
$marker =
    "<!-- MAUI_COPILOT_PUBLICATION_SMOKE build=$buildId attempt=$attempt -->"
$title = "[maui-bot-smoke] Draft PR publication $buildId-$attempt"
$bodyPath = Join-Path (
    [IO.Path]::GetTempPath()) "maui-publication-smoke-$buildId-$attempt.md"
$smokeRelativePath = '.github/replication-publication-smoke.md'
$result = [ordered]@{
    schemaVersion = 1
    buildId = $buildId
    attempt = $attempt
    authenticatedLogin = $null
    sourceRepository = "$SourceOwner/$SourceRepository"
    targetRepository = "$TargetOwner/$TargetRepository"
    baseBranch = $BaseBranch
    branch = $branchName
    url = $null
    number = $null
    created = $false
    verified = $false
    closed = $false
    branchDeleted = $false
    error = $null
}

$primaryError = $null
$cleanupErrors = [Collections.Generic.List[string]]::new()
$branchPushed = $false
$pullRequestNumber = 0
$locationPushed = $false
$sourceRemote = 'replication-smoke-source'

try {
    if ([string]::IsNullOrWhiteSpace($env:GH_TOKEN)) {
        throw 'GH_TOKEN is required for the replication publication smoke test.'
    }

    $authenticatedLogin = Get-ReplicationGitHubLogin
    $result.authenticatedLogin = $authenticatedLogin
    if (-not $authenticatedLogin.Equals(
            'MauiBot',
            [StringComparison]::OrdinalIgnoreCase)) {
        throw "GH_TOKEN must authenticate as 'MauiBot'."
    }

    $sourceJson = & gh api "repos/$SourceOwner/$SourceRepository"
    if ($LASTEXITCODE -ne 0) {
        throw "Unable to inspect $SourceOwner/$SourceRepository."
    }
    $source = $sourceJson | ConvertFrom-Json -Depth 10
    if ($source.fork -ne $true -or
        [string]$source.parent.full_name -cne
            "$ParentOwner/$ParentRepository" -or
        $source.permissions.push -ne $true) {
        throw (
            "$SourceOwner/$SourceRepository must be MauiBot's writable fork of " +
            "$ParentOwner/$ParentRepository.")
    }

    Push-Location $RepositoryRoot
    $locationPushed = $true
    $status = @(& git status --porcelain)
    if ($LASTEXITCODE -ne 0 -or $status.Count -ne 0) {
        throw 'The publication smoke checkout must be clean.'
    }

    $targetRemote = 'replication-smoke-target'
    & git remote remove $targetRemote 2>$null
    & git remote remove $sourceRemote 2>$null
    $global:LASTEXITCODE = 0

    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @(
            'remote',
            'add',
            $targetRemote,
            "https://github.com/$TargetOwner/$TargetRepository.git") `
        -Description 'Configuring the publication smoke target'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @('fetch', '--no-tags', $targetRemote, $BaseBranch) `
        -Description 'Fetching the publication smoke base'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @('checkout', '--detach', 'FETCH_HEAD') `
        -Description 'Checking out the publication smoke base'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @('switch', '-c', $branchName) `
        -Description 'Creating the publication smoke branch'

    $smokePath = Join-Path $RepositoryRoot $smokeRelativePath
    @(
        '# MauiBot draft PR publication smoke test'
        ''
        "Build: $buildId"
        "Attempt: $attempt"
        ''
        'This file exists only on the temporary smoke-test branch.'
    ) | Set-Content -LiteralPath $smokePath -Encoding utf8NoBOM

    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @('add', '--', $smokeRelativePath) `
        -Description 'Staging the publication smoke marker'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @('config', 'user.name', 'maui-copilot-replication') `
        -Description 'Configuring the publication smoke author'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @(
            'config',
            'user.email',
            '223556219+Copilot@users.noreply.github.com') `
        -Description 'Configuring the publication smoke email'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'gh' `
        -Arguments @('auth', 'setup-git') `
        -Description 'Configuring MauiBot Git authentication'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @(
            'commit',
            '-m',
            "Verify MauiBot draft PR publication`n`n" +
                "Co-authored-by: Copilot App " +
                "<223556219+Copilot@users.noreply.github.com>") `
        -Description 'Committing the publication smoke marker'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @(
            'remote',
            'add',
            $sourceRemote,
            "https://github.com/$SourceOwner/$SourceRepository.git") `
        -Description 'Configuring the MauiBot fork'
    Invoke-ReplicationSmokeCommand `
        -FilePath 'git' `
        -Arguments @(
            'push',
            $sourceRemote,
            "HEAD:refs/heads/$branchName") `
        -Description 'Pushing the publication smoke branch'
    $branchPushed = $true

    @(
        $marker
        '> [!WARNING]'
        '> This draft PR is an automated publication transport smoke test.'
        '> It contains no product fix and will be closed automatically.'
        ''
        "Pipeline build: $buildId"
    ) | Set-Content -LiteralPath $bodyPath -Encoding utf8NoBOM

    $prUrl = Open-ReplicationDraftPullRequest `
        -TargetOwner $TargetOwner `
        -TargetRepository $TargetRepository `
        -SourceOwner $SourceOwner `
        -BranchName $branchName `
        -BaseBranch $BaseBranch `
        -Title $title `
        -BodyPath $bodyPath
    $pullRequestNumber =
        [int]([regex]::Match($prUrl, '/pull/(?<number>\d+)$').
            Groups['number'].Value)
    $result.url = $prUrl
    $result.number = $pullRequestNumber
    $result.created = $true
    $verifiedPullRequest = Assert-ReplicationDraftPullRequest `
        -Url $prUrl `
        -TargetOwner $TargetOwner `
        -TargetRepository $TargetRepository `
        -SourceOwner $SourceOwner `
        -BranchName $branchName `
        -BaseBranch $BaseBranch
    $pullRequestNumber = $verifiedPullRequest.Number
    $result.number = $pullRequestNumber
    $result.verified = $true
}
catch {
    $primaryError = $_
    $result.error = $_.Exception.Message
}
finally {
    if ($branchPushed) {
        try {
            $cleanup = Remove-ReplicationDraftPullRequest `
                -TargetOwner $TargetOwner `
                -TargetRepository $TargetRepository `
                -SourceOwner $SourceOwner `
                -SourceRepository $SourceRepository `
                -BranchName $branchName `
                -BaseBranch $BaseBranch `
                -CloseComment (
                    'MauiBot draft publication smoke test completed. ' +
                    'Closing this temporary PR automatically.')
            if ($cleanup.Found) {
                $result.created = $true
                $result.number = $cleanup.Number
                $result.url = $cleanup.Url
                $result.closed = $cleanup.Closed
            }
            $result.branchDeleted = $cleanup.BranchDeleted
        }
        catch {
            $cleanupErrors.Add(
                "Cleaning up the smoke publication failed: $($_.Exception.Message)")
        }
    }

    Remove-Item -LiteralPath $bodyPath -Force -ErrorAction SilentlyContinue
    if ($cleanupErrors.Count -gt 0) {
        $result.error = @(
            $result.error
            $cleanupErrors
        ) | Where-Object { -not [string]::IsNullOrWhiteSpace([string]$_) } |
            Join-String -Separator ' '
    }
    $outputDirectory = Split-Path -Parent $OutputPath
    if ($outputDirectory) {
        New-Item -ItemType Directory -Path $outputDirectory -Force |
            Out-Null
    }
    $result | ConvertTo-Json -Depth 10 |
        Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
    if ($locationPushed) {
        Pop-Location
    }
}

if ($primaryError) {
    throw $primaryError
}
if ($cleanupErrors.Count -gt 0) {
    throw ($cleanupErrors -join ' ')
}
if (-not $result.created -or
    -not $result.verified -or
    -not $result.closed -or
    -not $result.branchDeleted) {
    throw 'The MauiBot draft PR publication smoke test was incomplete.'
}

Write-Host (
    "MauiBot draft PR publication smoke test passed and cleaned up: " +
    "$($result.url)")
