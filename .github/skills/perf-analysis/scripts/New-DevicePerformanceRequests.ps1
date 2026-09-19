#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates inert, reviewable local device requests. Does not build, run, or publish.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$PrMetadataPath,

    [Parameter(Mandatory = $true)]
    [string]$CurrentHeadSha,

    [Parameter(Mandatory = $true)]
    [string]$ResultsRoot,

    [Parameter(Mandatory = $false)]
    [string]$Repository = "dotnet/maui",

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DevicePerformance.Local.ps1")

foreach ($path in @($SelectionPath, $PrMetadataPath)) {
    [void](Get-LocalDevicePath $path "File" -mustExist)
}

$selection = Get-Content -LiteralPath $SelectionPath -Raw | ConvertFrom-Json
$pr = Get-Content -LiteralPath $PrMetadataPath -Raw | ConvertFrom-Json
$identity = [PSCustomObject]@{
    repository = $Repository
    pullRequestNumber = $pr.number
    baseCommitSha = $pr.mergeBaseOid
    headCommitSha = $pr.headRefOid
    harnessSha = $pr.harnessSha
}
Assert-LocalDeviceIdentity $identity
if ($pr.headRefOid -cne $CurrentHeadSha) {
    throw "PR head changed from '$($pr.headRefOid)' to '$CurrentHeadSha'."
}

$root = Get-LocalDevicePath $ResultsRoot "Directory" -mustExist
$requests = @(New-LocalDeviceRequests $selection $identity $root)
$OutputPath = Assert-DeviceOutputOutsideResults $OutputPath $root @($SelectionPath, $PrMetadataPath)
$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}
ConvertTo-Json -InputObject $requests -Depth 8 |
    Set-Content -LiteralPath $OutputPath -Encoding UTF8

Write-Host "Created $($requests.Count) manual local device request(s); nothing was executed."
