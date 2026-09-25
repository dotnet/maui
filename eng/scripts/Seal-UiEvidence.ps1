#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$Root,

    [Parameter(Mandatory = $true)]
    [string]$RequestManifestPath,

    [Parameter(Mandatory = $false)]
    [string]$OutputPath = (Join-Path $Root "evidence-seal.json"),

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 2147483647)]
    [long]$MaximumFileBytes = 67108864,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 2147483647)]
    [long]$MaximumTotalBytes = 536870912
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

$resolvedRoot = Assert-UiEvidenceLocalPath $Root
$request = Read-UiEvidenceJson $RequestManifestPath
if ($request.schemaVersion -ne 1) {
    throw "Unsupported UI evidence request version '$($request.schemaVersion)'."
}

$outputFullPath = Assert-UiEvidenceLocalPath $OutputPath
$files = @()
$totalBytes = [long]0

foreach ($file in @(Get-UiEvidenceFiles $resolvedRoot | Sort-Object FullName)) {
    if ([IO.Path]::GetFullPath($file.FullName) -eq $outputFullPath) {
        continue
    }
    if ($file.Length -gt $MaximumFileBytes) {
        throw "Evidence file exceeds the per-file limit: $($file.FullName)"
    }

    $totalBytes += $file.Length
    if ($totalBytes -gt $MaximumTotalBytes) {
        throw "Evidence bundle exceeds the total size limit."
    }

    $files += [PSCustomObject][ordered]@{
        relativePath = Get-UiEvidenceRelativePath $resolvedRoot $file.FullName
        sizeBytes = [long]$file.Length
        sha256 = Get-UiEvidenceSha256 $file.FullName
    }
}

if ($files.Count -eq 0) {
    throw "Evidence root contains no files to seal."
}

$inventory = @($files | ForEach-Object {
    "$($_.relativePath):$($_.sizeBytes):$($_.sha256)"
}) -join "`n"

$seal = [PSCustomObject][ordered]@{
    schemaVersion = 1
    sealed = $true
    algorithm = "sha256"
    requestKey = [string]$request.requestKey
    repository = [string]$request.repository
    pullRequestNumber = [int]$request.pullRequestNumber
    baseCommitSha = [string]$request.baseCommitSha
    headCommitSha = [string]$request.headCommitSha
    harnessSha = [string]$request.harnessSha
    scenarioId = [string]$request.scenarioId
    platform = [string]$request.platform
    createdAtUtc = [DateTimeOffset]::UtcNow.ToString("O")
    totalBytes = $totalBytes
    files = @($files)
    sealDigest = Get-UiEvidenceStringSha256 $inventory
}

Write-UiEvidenceJson $seal $OutputPath
Write-Host "Sealed $($files.Count) UI evidence files at $OutputPath."
