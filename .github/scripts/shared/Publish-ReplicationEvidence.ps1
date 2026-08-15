#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes validated reproduction media to the existing public asset branch.

.DESCRIPTION
    Run only from the trusted replication publisher job after candidate
    validation. The script copies the fixed evidence allowlist into an
    immutable build-specific path on review-tests-assets-v2 and pushes with the
    trusted checkout credential already persisted in the clean publisher job.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ValidatedCandidatePath,

    [Parameter(Mandatory = $true)]
    [string]$EvidenceDirectory,

    [Parameter(Mandatory = $true)]
    [string]$RepositoryRoot,

    [ValidatePattern('^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$')]
    [string]$Repository = 'dotnet/maui',

    [ValidatePattern('^[A-Za-z0-9._/-]+$')]
    [string]$AssetBranch = 'review-tests-assets-v2',

    [Parameter(Mandatory = $true)]
    [string]$AssetPrefix,

    [string]$OutputPath = '',

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

function Test-ReplicationAssetPrefix {
    param([Parameter(Mandatory = $true)][string]$Value)

    if ($Value.Length -gt 220 -or $Value -notmatch '^[A-Za-z0-9][A-Za-z0-9._/-]*$') {
        return $false
    }
    if ($Value.Contains('//')) {
        return $false
    }
    foreach ($segment in ($Value -split '/')) {
        if ($segment -in @('', '.', '..')) {
            return $false
        }
    }
    return $true
}

function ConvertTo-ReplicationUrlPath {
    param([Parameter(Mandatory = $true)][string]$Value)

    return (($Value -split '/') | ForEach-Object {
        [Uri]::EscapeDataString($_)
    }) -join '/'
}

function Get-ReplicationPublicAssetUrl {
    param(
        [Parameter(Mandatory = $true)][string]$Repository,
        [Parameter(Mandatory = $true)][string]$Commit,
        [Parameter(Mandatory = $true)][string]$Prefix,
        [Parameter(Mandatory = $true)][string]$FileName
    )

    $repositoryPath = ConvertTo-ReplicationUrlPath -Value $Repository
    $prefixPath = ConvertTo-ReplicationUrlPath -Value $Prefix
    $filePath = [Uri]::EscapeDataString($FileName)
    return "https://raw.githubusercontent.com/$repositoryPath/$Commit/$prefixPath/$filePath"
}

function Get-ReplicationCandidateValue {
    param(
        [Parameter(Mandatory = $true)]$Candidate,
        [Parameter(Mandatory = $true)][string[]]$Names,
        [Parameter(Mandatory = $true)][string]$Description
    )

    foreach ($name in $Names) {
        $property = $Candidate.PSObject.Properties[$name]
        if (
            $property -and
            $null -ne $property.Value -and
            -not [string]::IsNullOrWhiteSpace([string]$property.Value)
        ) {
            return $property.Value
        }
    }
    throw "Validated candidate is missing $Description."
}

function Invoke-ReplicationExternalCommand {
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

function Publish-ReplicationAssetCommit {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$AssetBranch,
        [Parameter(Mandatory = $true)][string]$AssetPrefix,
        [Parameter(Mandatory = $true)][string]$EvidenceRoot,
        [Parameter(Mandatory = $true)][string[]]$FileNames,
        [Parameter(Mandatory = $true)][int]$IssueNumber,
        [Parameter(Mandatory = $true)][string]$Platform
    )

    $worktreePath = Join-Path `
        ([IO.Path]::GetTempPath()) `
        "maui-replication-assets-$([guid]::NewGuid().ToString('N'))"

    Push-Location $RepositoryRoot
    try {
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @(
                'fetch',
                '--no-tags',
                'origin',
                "refs/heads/$AssetBranch`:refs/remotes/origin/$AssetBranch"
            ) `
            -Description 'Fetching public replication asset branch'
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @(
                'worktree',
                'add',
                '--detach',
                $worktreePath,
                "refs/remotes/origin/$AssetBranch"
            ) `
            -Description 'Creating isolated replication asset worktree'

        $targetRoot = Join-Path $worktreePath $AssetPrefix
        New-Item -ItemType Directory -Path $targetRoot -Force | Out-Null
        $relativePaths = @()
        foreach ($fileName in $FileNames) {
            $sourcePath = Join-Path $EvidenceRoot $fileName
            $targetPath = Join-Path $targetRoot $fileName
            if (Test-Path -LiteralPath $targetPath) {
                throw "Replication asset path already exists and will not be overwritten: $AssetPrefix/$fileName"
            }
            Copy-Item -LiteralPath $sourcePath -Destination $targetPath
            $relativePaths += "$AssetPrefix/$fileName"
        }

        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('-C', $worktreePath, 'config', 'user.name', 'maui-copilot-replication') `
            -Description 'Configuring replication asset author'
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @(
                '-C',
                $worktreePath,
                'config',
                'user.email',
                '223556219+Copilot@users.noreply.github.com'
            ) `
            -Description 'Configuring replication asset email'
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments (@('-C', $worktreePath, 'add', '--') + $relativePaths) `
            -Description 'Staging replication evidence'

        $message = "Publish reproduction evidence for #$IssueNumber on $Platform"
        Invoke-ReplicationExternalCommand `
            -FilePath 'git' `
            -Arguments @('-C', $worktreePath, 'commit', '-m', $message) `
            -Description 'Committing replication evidence'

        $pushed = $false
        for ($attempt = 1; $attempt -le 3 -and -not $pushed; $attempt++) {
            & git -C $worktreePath push origin "HEAD:refs/heads/$AssetBranch"
            if ($LASTEXITCODE -eq 0) {
                $pushed = $true
                break
            }
            if ($attempt -lt 3) {
                Invoke-ReplicationExternalCommand `
                    -FilePath 'git' `
                    -Arguments @(
                        '-C',
                        $worktreePath,
                        'fetch',
                        '--no-tags',
                        'origin',
                        $AssetBranch
                    ) `
                    -Description 'Refreshing replication asset branch'
                Invoke-ReplicationExternalCommand `
                    -FilePath 'git' `
                    -Arguments @(
                        '-C',
                        $worktreePath,
                        'rebase',
                        "origin/$AssetBranch"
                    ) `
                    -Description 'Rebasing replication evidence'
            }
        }
        if (-not $pushed) {
            throw 'Publishing replication evidence to the public asset branch failed after three attempts.'
        }

        $commit = (& git -C $worktreePath rev-parse HEAD).Trim()
        if ($LASTEXITCODE -ne 0 -or $commit -cnotmatch '^[0-9a-f]{40}$') {
            throw 'Could not resolve the committed replication evidence SHA.'
        }
        return $commit
    }
    finally {
        if (Test-Path -LiteralPath $worktreePath) {
            & git worktree remove --force $worktreePath 2>$null
        }
        & git worktree prune 2>$null
        Pop-Location
    }
}

if (-not (Test-ReplicationAssetPrefix -Value $AssetPrefix)) {
    throw "AssetPrefix contains an invalid or traversal-like path: '$AssetPrefix'."
}
if ($AssetBranch.Contains('..') -or $AssetBranch.StartsWith('/') -or $AssetBranch.EndsWith('/')) {
    throw "AssetBranch is invalid: '$AssetBranch'."
}

$candidate = Get-Content -LiteralPath $ValidatedCandidatePath -Raw |
    ConvertFrom-Json -Depth 50
$validationProperty = $candidate.PSObject.Properties['validationPassed']
if (-not $validationProperty -or $validationProperty.Value -ne $true) {
    throw 'Candidate validation did not pass; public evidence will not be uploaded.'
}

$issueNumber = [int](Get-ReplicationCandidateValue `
    -Candidate $candidate `
    -Names @('issueNumber') `
    -Description 'issueNumber')
$platform = [string](Get-ReplicationCandidateValue `
    -Candidate $candidate `
    -Names @('platform') `
    -Description 'platform')
$baseSha = [string](Get-ReplicationCandidateValue `
    -Candidate $candidate `
    -Names @('baseSha', 'baseCommit') `
    -Description 'base SHA')
$testType = [string](Get-ReplicationCandidateValue `
    -Candidate $candidate `
    -Names @('testType') `
    -Description 'test type')
$testFilter = [string](Get-ReplicationCandidateValue `
    -Candidate $candidate `
    -Names @('testFilter') `
    -Description 'test filter')
$failureSignature = [string](Get-ReplicationCandidateValue `
    -Candidate $candidate `
    -Names @('expectedFailureSignature', 'failureSignature') `
    -Description 'failure signature')
$actualFailureMessage = [string](Get-ReplicationCandidateValue `
    -Candidate $candidate `
    -Names @('actualFailureMessage') `
    -Description 'targeted failure message')
if (-not $actualFailureMessage.Contains(
    $failureSignature,
    [StringComparison]::Ordinal)) {
    throw 'Validated candidate targeted failure message does not contain the expected failure signature.'
}

$evidenceRoot = (Resolve-Path -LiteralPath $EvidenceDirectory).Path
$fileNames = [ordered]@{
    video = 'repro.mp4'
    preview = 'preview.gif'
    thumbnail = 'thumbnail.png'
    manifest = 'evidence.json'
}
foreach ($fileName in $fileNames.Values) {
    $filePath = Join-Path $evidenceRoot $fileName
    if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
        throw "Required reproduction evidence is missing: $fileName"
    }
}

$assetCommit = if ($DryRun) {
    '0000000000000000000000000000000000000000'
} else {
    Publish-ReplicationAssetCommit `
        -RepositoryRoot $RepositoryRoot `
        -AssetBranch $AssetBranch `
        -AssetPrefix $AssetPrefix `
        -EvidenceRoot $evidenceRoot `
        -FileNames @($fileNames.Values) `
        -IssueNumber $issueNumber `
        -Platform $platform
}

$publication = [ordered]@{
    schemaVersion = 1
    issueNumber = $issueNumber
    platform = $platform
    baseSha = $baseSha
    assetRepository = $Repository
    assetBranch = $AssetBranch
    assetCommit = $assetCommit
    assetPrefix = $AssetPrefix
    test = [ordered]@{
        type = $testType
        filter = $testFilter
        expectedFailureSignature = $failureSignature
        actualFailureMessage = $actualFailureMessage
    }
    capture = Get-Content -LiteralPath (Join-Path $evidenceRoot 'evidence.json') -Raw |
        ConvertFrom-Json -Depth 20
    blobs = [ordered]@{}
}
foreach ($entry in $fileNames.GetEnumerator()) {
    $publication.blobs[$entry.Key] = Get-ReplicationPublicAssetUrl `
        -Repository $Repository `
        -Commit $assetCommit `
        -Prefix $AssetPrefix `
        -FileName $entry.Value
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $evidenceRoot 'published-evidence.json'
}
$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$publication | ConvertTo-Json -Depth 10 |
    Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
Write-Host "Published reproduction evidence manifest: $OutputPath"
