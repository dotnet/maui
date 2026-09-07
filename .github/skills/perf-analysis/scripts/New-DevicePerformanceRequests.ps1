#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates trusted AzDO queue requests from sealed performance selection evidence.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$PrMetadataPath,

    [Parameter(Mandatory = $true)]
    [string]$CurrentHeadSha,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

foreach ($path in @($SelectionPath, $PrMetadataPath)) {
    if (-not (Test-Path $path)) {
        throw "Required request input does not exist: $path"
    }
}

$selection = Get-Content $SelectionPath -Raw | ConvertFrom-Json
$pr = Get-Content $PrMetadataPath -Raw | ConvertFrom-Json

if ([int]$pr.number -le 0 -or
    [string]::IsNullOrWhiteSpace([string]$pr.mergeBaseOid) -or
    [string]::IsNullOrWhiteSpace([string]$pr.headRefOid) -or
    [string]::IsNullOrWhiteSpace([string]$pr.harnessSha))
{
    throw "PR metadata is incomplete."
}

if ($pr.headRefOid -ne $CurrentHeadSha) {
    throw "PR head changed from '$($pr.headRefOid)' to '$CurrentHeadSha'."
}

$requests = New-Object System.Collections.Generic.List[object]
$keys = New-Object System.Collections.Generic.HashSet[string]
foreach ($scenario in @($selection.deviceScenarios)) {
    if ($scenario.automationStatus -ne "manual-device-ci-ready") {
        continue
    }

    if ([string]::IsNullOrWhiteSpace([string]$scenario.resultScenario) -or
        [string]$scenario.pipeline.path -ne "eng/pipelines/ci-device-performance.yml")
    {
        throw "Supported scenario '$($scenario.id)' has incomplete pipeline metadata."
    }

    foreach ($platformValue in @($scenario.pipeline.platforms)) {
        $platform = ([string]$platformValue).ToLowerInvariant()
        if ($platform -notin @("android", "ios", "maccatalyst", "windows")) {
            throw "Supported scenario '$($scenario.id)' has unsupported platform '$platform'."
        }

        $rawKey = @(
            $pr.number,
            $pr.mergeBaseOid,
            $pr.headRefOid,
            $pr.harnessSha,
            $scenario.resultScenario,
            $platform
        ) -join "|"
        $hash = [Convert]::ToHexString(
            [Security.Cryptography.SHA256]::HashData(
                [Text.Encoding]::UTF8.GetBytes($rawKey))).ToLowerInvariant()
        $requestKey = "maui-perf-$($hash.Substring(0, 24))"

        if (-not $keys.Add($requestKey)) {
            continue
        }

        $requests.Add([PSCustomObject]@{
            requestKey = $requestKey
            repository = "dotnet/maui"
            pullRequestNumber = [int]$pr.number
            baseCommitSha = [string]$pr.mergeBaseOid
            headCommitSha = [string]$pr.headRefOid
            harnessSha = [string]$pr.harnessSha
            scenarioId = [string]$scenario.id
            expectedScenario = [string]$scenario.resultScenario
            platform = $platform
        })
    }
}

$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force $directory | Out-Null
}
$output = @($requests | ForEach-Object { $_ })
ConvertTo-Json -InputObject $output -Depth 8 |
    Set-Content $OutputPath -Encoding UTF8

Write-Host "Created $($requests.Count) device performance request(s)."
