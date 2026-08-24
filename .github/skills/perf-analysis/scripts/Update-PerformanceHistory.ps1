#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $false)]
    [string]$SummaryPath,

    [Parameter(Mandatory = $false)]
    [string]$ManifestPath,

    [Parameter(Mandatory = $false)]
    [string]$PointPath,

    [Parameter(Mandatory = $false)]
    [string]$HistoryPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [ValidateRange(1, 3650)]
    [int]$RetentionDays = 365,

    [ValidateRange(1, 10000)]
    [int]$MaxPoints = 2000
)

$ErrorActionPreference = "Stop"

$history = if ($HistoryPath -and (Test-Path $HistoryPath)) {
    Get-Content $HistoryPath -Raw | ConvertFrom-Json
}
else {
    [PSCustomObject]@{
        schemaVersion = 1
        points = @()
    }
}
if ([int]$history.schemaVersion -ne 1) {
    throw "Unsupported performance history schema."
}

$point = if ($PointPath) {
    if (-not (Test-Path $PointPath)) {
        throw "Performance history point does not exist: $PointPath"
    }
    Get-Content $PointPath -Raw | ConvertFrom-Json
}
else {
    foreach ($path in @($SummaryPath, $ManifestPath)) {
        if ([string]::IsNullOrWhiteSpace($path) -or -not (Test-Path $path)) {
            throw "SummaryPath and ManifestPath are required when PointPath is not provided."
        }
    }

    $summary = Get-Content $SummaryPath -Raw | ConvertFrom-Json
    $manifest = Get-Content $ManifestPath -Raw | ConvertFrom-Json
    if ([int]$summary.schemaVersion -ne 2 -or [int]$manifest.schemaVersion -ne 2) {
        throw "Unsupported performance evidence schema."
    }
    if ([string]$manifest.status -ne "complete") {
        throw "Only complete benchmark runs can be added to performance history."
    }
    if (-not $summary.benchmarks -or @($summary.benchmarks).Count -gt 500) {
        throw "Performance summary must contain between 1 and 500 benchmark points."
    }

    $sdkVersion = try {
        (& dotnet --version 2>$null | Select-Object -First 1).Trim()
    }
    catch {
        "unknown"
    }
    $environment = [ordered]@{
        machine = [Environment]::MachineName
        os = [System.Runtime.InteropServices.RuntimeInformation]::OSDescription
        architecture = [System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString()
        framework = [System.Runtime.InteropServices.RuntimeInformation]::FrameworkDescription
        processor = if ($env:PROCESSOR_IDENTIFIER) { $env:PROCESSOR_IDENTIFIER } else { "unknown" }
        sdk = $sdkVersion
    }
    $fingerprintEnvironment = [ordered]@{
        os = $environment.os
        architecture = $environment.architecture
        framework = $environment.framework
        processor = $environment.processor
        sdk = $environment.sdk
    }
    $environmentJson = $fingerprintEnvironment | ConvertTo-Json -Compress
    $environmentFingerprint = [Convert]::ToHexString(
        [Security.Cryptography.SHA256]::HashData(
            [Text.Encoding]::UTF8.GetBytes($environmentJson)
        )
    ).Substring(0, 16).ToLowerInvariant()

    [PSCustomObject]@{
        timestampUtc = [DateTimeOffset]::UtcNow.ToString("O")
        prNumber = [int]$manifest.prNumber
        baseRef = [string]$manifest.baseRef
        baseSha = [string]$manifest.baseSha
        headSha = [string]$manifest.headSha
        status = [string]$manifest.status
        verdict = [string]$summary.verdict
        environmentFingerprint = $environmentFingerprint
        environment = [PSCustomObject]$environment
        benchmarks = @($summary.benchmarks)
    }
}

if ([string]::IsNullOrWhiteSpace([string]$point.headSha) -or
    [string]::IsNullOrWhiteSpace([string]$point.environmentFingerprint) -or
    [string]$point.status -ne "complete" -or
    @($point.benchmarks).Count -eq 0 -or
    @($point.benchmarks).Count -gt 500) {
    throw "Performance history point is missing required provenance or benchmark data."
}
foreach ($benchmark in @($point.benchmarks)) {
    $benchmarkName = [string]$benchmark.name
    if ([string]::IsNullOrWhiteSpace($benchmarkName) -or $benchmarkName.Length -gt 500) {
        throw "Performance history contains an invalid benchmark name."
    }
}

$cutoff = [DateTimeOffset]::UtcNow.AddDays(-$RetentionDays)
$points = @(
    @($history.points) |
        Where-Object {
            try {
                [DateTimeOffset]$_.timestampUtc -ge $cutoff
            }
            catch {
                $true
            }
        } |
        Where-Object {
            -not (
                [string]$_.headSha -eq $point.headSha -and
                [string]$_.environmentFingerprint -eq $point.environmentFingerprint
            )
        }
) + @($point)

$points = @($points | Sort-Object timestampUtc)
if ($points.Count -gt $MaxPoints) {
    $points = @($points | Select-Object -Last $MaxPoints)
}

$result = [PSCustomObject]@{
    schemaVersion = 1
    updatedUtc = [DateTimeOffset]::UtcNow.ToString("O")
    retentionDays = $RetentionDays
    maxPoints = $MaxPoints
    points = $points
}
$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}
$result | ConvertTo-Json -Depth 14 | Set-Content $OutputPath -Encoding UTF8
Write-Host "Updated performance history with $(@($point.benchmarks).Count) benchmark point(s)."
