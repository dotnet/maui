#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$BaseArtifacts,

    [Parameter(Mandatory = $true)]
    [string]$HeadArtifacts,

    [Parameter(Mandatory = $true)]
    [string]$RequestPath,

    [Parameter(Mandatory = $true)]
    [string]$RegistryPath,

    [Parameter(Mandatory = $true)]
    [string]$DevFlowFeed,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

$request = Read-UiEvidenceJson $RequestPath
if ((Get-UiEvidenceSha256 $RegistryPath) -cne [string]$request.registrySha256) {
    throw "UI evidence registry hash does not match the request."
}
$baseMetadataPath = @(Get-UiEvidenceFiles $BaseArtifacts | Where-Object Name -eq "ui-evidence-build-metadata.json")
$headMetadataPath = @(Get-UiEvidenceFiles $HeadArtifacts | Where-Object Name -eq "ui-evidence-build-metadata.json")
Get-UiEvidenceFiles $DevFlowFeed | Out-Null
if ($baseMetadataPath.Count -ne 1 -or $headMetadataPath.Count -ne 1) {
    throw "Base and head artifacts must each contain one UI evidence build metadata file."
}
$baseMetadata = Read-UiEvidenceJson $baseMetadataPath[0].FullName
$headMetadata = Read-UiEvidenceJson $headMetadataPath[0].FullName

foreach ($item in @(
    @{ Name = "base"; Metadata = $baseMetadata; Commit = [string]$request.baseCommitSha },
    @{ Name = "head"; Metadata = $headMetadata; Commit = [string]$request.headCommitSha }
)) {
    if ([string]$item.Metadata.requestKey -ne [string]$request.requestKey -or
        [string]$item.Metadata.variant -ne [string]$item.Name -or
        [string]$item.Metadata.commitSha -ne [string]$item.Commit -or
        [string]$item.Metadata.harnessSha -ne [string]$request.harnessSha -or
        [string]$item.Metadata.scenarioId -ne [string]$request.scenarioId -or
        [string]$item.Metadata.platform -ne [string]$request.platform) {
        throw "$($item.Name) build metadata does not match the request."
    }
}

$output = New-UiEvidenceOutputDirectory $OutputDirectory @($BaseArtifacts, $HeadArtifacts, $DevFlowFeed)

function Copy-Variant {
    param(
        [string]$Name,
        [string]$Artifacts,
        [object]$Metadata
    )

    $sourceRoot = Resolve-UiEvidenceChildPath $Artifacts ([string]$Metadata.appRootRelativePath)
    $sourceApp = Resolve-UiEvidenceChildPath $Artifacts ([string]$Metadata.appRelativePath)
    $relativeWithinRoot = Get-UiEvidenceRelativePath $sourceRoot $sourceApp
    if (-not (Test-Path -LiteralPath $sourceRoot -PathType Container) -or
        -not (Test-Path -LiteralPath $sourceApp -PathType Leaf)) {
        throw "$Name app artifact is missing."
    }
    if ([string]$Metadata.appSha256 -ne (Get-UiEvidenceSha256 $sourceApp)) {
        throw "$Name app artifact hash does not match build metadata."
    }

    $sealedPaths = @{}
    $inventory = @()
    foreach ($entry in @($Metadata.appFiles)) {
        $relativePath = Normalize-UiEvidenceRepositoryPath ([string]$entry.relativePath)
        $key = $relativePath.ToLowerInvariant()
        if ($sealedPaths.ContainsKey($key)) {
            throw "$Name app metadata contains duplicate path '$relativePath'."
        }
        $sealedPaths[$key] = $true
        $path = Resolve-UiEvidenceChildPath $sourceRoot $relativePath
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "$Name app file is missing: $relativePath"
        }
        $file = Get-Item -LiteralPath $path -Force
        $actualHash = Get-UiEvidenceSha256 $path
        if ([long]$entry.sizeBytes -ne $file.Length -or [string]$entry.sha256 -ne $actualHash) {
            throw "$Name app file changed: $relativePath"
        }
        $inventory += "${relativePath}:$($file.Length):$actualHash"
    }
    foreach ($file in @(Get-UiEvidenceFiles $sourceRoot)) {
        $relativePath = Get-UiEvidenceRelativePath $sourceRoot $file.FullName
        if (-not $sealedPaths.ContainsKey($relativePath.ToLowerInvariant())) {
            throw "$Name app root contains an unsealed file: $relativePath"
        }
    }
    if ((Get-UiEvidenceStringSha256 ($inventory -join "`n")) -ne [string]$Metadata.appDirectorySha256) {
        throw "$Name app directory digest does not match build metadata."
    }

    $destinationRoot = Join-Path $output "$Name\app"
    New-Item -ItemType Directory -Force -Path $destinationRoot | Out-Null
    Get-ChildItem -LiteralPath $sourceRoot -Force | Copy-Item -Destination $destinationRoot -Recurse -Force
    return "$Name/app/$relativeWithinRoot"
}

$baseAppPath = Copy-Variant "base" $BaseArtifacts $baseMetadata
$headAppPath = Copy-Variant "head" $HeadArtifacts $headMetadata
Copy-Item -LiteralPath $RequestPath -Destination (Join-Path $output "request.json") -Force
Copy-Item -LiteralPath $RegistryPath -Destination (Join-Path $output "scenarios.json") -Force
Copy-Item -LiteralPath $DevFlowFeed -Destination (Join-Path $output "devflow-feed") -Recurse -Force

$manifest = [PSCustomObject][ordered]@{
    schemaVersion = 1
    requestKey = [string]$request.requestKey
    baseAppRelativePath = $baseAppPath
    headAppRelativePath = $headAppPath
    baseAppSha256 = [string]$baseMetadata.appSha256
    headAppSha256 = [string]$headMetadata.appSha256
    baseAppDirectorySha256 = [string]$baseMetadata.appDirectorySha256
    headAppDirectorySha256 = [string]$headMetadata.appDirectorySha256
    baseFiles = @($baseMetadata.appFiles)
    headFiles = @($headMetadata.appFiles)
    devFlowCommit = [string]$baseMetadata.devFlowCommit
    devFlowPackageVersion = [string]$baseMetadata.devFlowPackageVersion
}
Write-UiEvidenceJson $manifest (Join-Path $output "payload-manifest.json")
