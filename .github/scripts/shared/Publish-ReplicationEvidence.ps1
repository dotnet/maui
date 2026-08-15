#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publishes validated reproduction media to public Azure Blob Storage.

.DESCRIPTION
    This script is intended to run in the trusted replication publisher job.
    It uploads only the fixed evidence allowlist and never executes generated
    repository content.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ValidatedCandidatePath,

    [Parameter(Mandatory = $true)]
    [string]$EvidenceDirectory,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-z0-9]{3,24}$')]
    [string]$StorageAccount,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])?$')]
    [string]$Container,

    [Parameter(Mandatory = $true)]
    [string]$BlobPrefix,

    [string]$PublicBaseUrl = '',

    [string]$OutputPath = '',

    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

function Test-ReplicationBlobPrefix {
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

function Get-ReplicationEvidenceContentType {
    param([Parameter(Mandatory = $true)][string]$FileName)

    switch ([IO.Path]::GetExtension($FileName).ToLowerInvariant()) {
        '.mp4' { return 'video/mp4' }
        '.gif' { return 'image/gif' }
        '.png' { return 'image/png' }
        '.json' { return 'application/json; charset=utf-8' }
        default { throw "Unsupported public evidence type: $FileName" }
    }
}

function Test-ReplicationPublicBaseUrl {
    param(
        [Parameter(Mandatory = $true)][string]$Value,
        [Parameter(Mandatory = $true)][string]$StorageAccount,
        [Parameter(Mandatory = $true)][string]$Container
    )

    $uri = $null
    if (-not [Uri]::TryCreate($Value, [UriKind]::Absolute, [ref]$uri)) {
        return $false
    }
    return (
        $uri.Scheme -ceq 'https' -and
        $uri.Host -ceq "$StorageAccount.blob.core.windows.net" -and
        [string]::IsNullOrEmpty($uri.UserInfo) -and
        [string]::IsNullOrEmpty($uri.Query) -and
        [string]::IsNullOrEmpty($uri.Fragment) -and
        $uri.AbsolutePath.TrimEnd('/') -ceq "/$Container"
    )
}

function ConvertTo-ReplicationUrlPath {
    param([Parameter(Mandatory = $true)][string]$Value)

    return (($Value -split '/') | ForEach-Object { [Uri]::EscapeDataString($_) }) -join '/'
}

function Get-ReplicationPublicBlobUrl {
    param(
        [Parameter(Mandatory = $true)][string]$BaseUrl,
        [Parameter(Mandatory = $true)][string]$Prefix,
        [Parameter(Mandatory = $true)][string]$FileName
    )

    $safePrefix = ConvertTo-ReplicationUrlPath -Value $Prefix
    $safeFile = [Uri]::EscapeDataString($FileName)
    return "$($BaseUrl.TrimEnd('/'))/$safePrefix/$safeFile"
}

function Get-ReplicationCandidateValue {
    param(
        [Parameter(Mandatory = $true)]$Candidate,
        [Parameter(Mandatory = $true)][string[]]$Names,
        [Parameter(Mandatory = $true)][string]$Description
    )

    foreach ($name in $Names) {
        $property = $Candidate.PSObject.Properties[$name]
        if ($property -and $null -ne $property.Value -and -not [string]::IsNullOrWhiteSpace([string]$property.Value)) {
            return $property.Value
        }
    }

    throw "Validated candidate is missing $Description."
}

if (-not (Test-ReplicationBlobPrefix -Value $BlobPrefix)) {
    throw "BlobPrefix contains an invalid or traversal-like path: '$BlobPrefix'."
}

$candidate = Get-Content -LiteralPath $ValidatedCandidatePath -Raw | ConvertFrom-Json -Depth 50
$validationProperty = $candidate.PSObject.Properties['validationPassed']
if (-not $validationProperty -or $validationProperty.Value -ne $true) {
    throw 'Candidate validation did not pass; public evidence will not be uploaded.'
}

$issueNumber = [int](Get-ReplicationCandidateValue -Candidate $candidate -Names @('issueNumber') -Description 'issueNumber')
$platform = [string](Get-ReplicationCandidateValue -Candidate $candidate -Names @('platform') -Description 'platform')
$baseSha = [string](Get-ReplicationCandidateValue -Candidate $candidate -Names @('baseSha', 'baseCommit') -Description 'base SHA')
$testType = [string](Get-ReplicationCandidateValue -Candidate $candidate -Names @('testType') -Description 'test type')
$testFilter = [string](Get-ReplicationCandidateValue -Candidate $candidate -Names @('testFilter') -Description 'test filter')
$failureSignature = [string](Get-ReplicationCandidateValue -Candidate $candidate -Names @('expectedFailureSignature', 'failureSignature') -Description 'failure signature')
$actualFailureMessage = [string](Get-ReplicationCandidateValue -Candidate $candidate -Names @('actualFailureMessage') -Description 'targeted failure message')
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
}

foreach ($fileName in $fileNames.Values) {
    $filePath = Join-Path $evidenceRoot $fileName
    if (-not (Test-Path -LiteralPath $filePath -PathType Leaf)) {
        throw "Required reproduction evidence is missing: $fileName"
    }
}

if ([string]::IsNullOrWhiteSpace($PublicBaseUrl)) {
    $PublicBaseUrl = "https://$StorageAccount.blob.core.windows.net/$Container"
}
if (-not (Test-ReplicationPublicBaseUrl `
    -Value $PublicBaseUrl `
    -StorageAccount $StorageAccount `
    -Container $Container)) {
    throw 'PublicBaseUrl must be the HTTPS Azure Blob endpoint for the configured account and container.'
}

$publication = [ordered]@{
    schemaVersion = 1
    issueNumber = $issueNumber
    platform = $platform
    baseSha = $baseSha
    test = [ordered]@{
        type = $testType
        filter = $testFilter
        expectedFailureSignature = $failureSignature
        actualFailureMessage = $actualFailureMessage
    }
    capture = Get-Content -LiteralPath (Join-Path $evidenceRoot 'evidence.json') -Raw | ConvertFrom-Json -Depth 20
    blobs = [ordered]@{}
}

foreach ($entry in $fileNames.GetEnumerator()) {
    $publication.blobs[$entry.Key] = Get-ReplicationPublicBlobUrl `
        -BaseUrl $PublicBaseUrl `
        -Prefix $BlobPrefix `
        -FileName $entry.Value
}

$evidenceJsonPath = Join-Path $evidenceRoot 'evidence.json'
$publication.blobs['manifest'] = Get-ReplicationPublicBlobUrl `
    -BaseUrl $PublicBaseUrl `
    -Prefix $BlobPrefix `
    -FileName 'evidence.json'
$publication | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $evidenceJsonPath -Encoding utf8NoBOM
$fileNames['manifest'] = 'evidence.json'

if (-not $DryRun) {
    foreach ($entry in $fileNames.GetEnumerator()) {
        $filePath = Join-Path $evidenceRoot $entry.Value
        $blobName = "$BlobPrefix/$($entry.Value)"
        $contentType = Get-ReplicationEvidenceContentType -FileName $entry.Value

        & az storage blob upload `
            --account-name $StorageAccount `
            --container-name $Container `
            --file $filePath `
            --name $blobName `
            --auth-mode login `
            --overwrite false `
            --content-type $contentType `
            --only-show-errors `
            --output none

        if ($LASTEXITCODE -ne 0) {
            throw "Azure Blob upload failed for '$($entry.Value)'."
        }
    }
}

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $evidenceRoot 'published-evidence.json'
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$publication | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $OutputPath -Encoding utf8NoBOM
Write-Host "Published reproduction evidence manifest: $OutputPath"
