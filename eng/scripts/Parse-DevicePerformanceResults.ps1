#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Extracts MAUI device-performance JSON records from XHarness, Helix, or test logs.
#>

param(
    [Parameter(Mandatory = $true)]
    [string[]]$InputPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [Parameter(Mandatory = $false)]
    [switch]$AllowEmpty
)

$ErrorActionPreference = "Stop"
$prefix = "MAUI_PERF_RESULT:"
$chunkPrefix = "MAUI_PERF_CHUNK:"
$results = New-Object System.Collections.Generic.List[object]
$seenJson = New-Object System.Collections.Generic.HashSet[string]
$chunkSets = @{}

function Get-InputFiles([string]$path) {
    if (-not (Test-Path $path))
    {
        throw "Input path does not exist: $path"
    }

    $item = Get-Item $path
    if ($item.PSIsContainer)
    {
        return @(Get-ChildItem $item.FullName -File -Recurse)
    }

    return @($item)
}

function Assert-RequiredProperty($result, [string]$name, [string]$source) {
    $property = $result.PSObject.Properties[$name]
    if ($null -eq $property -or $null -eq $property.Value -or "$($property.Value)" -eq "")
    {
        throw "Performance result in '$source' is missing '$name'."
    }
}

function Add-PerformanceResult([string]$json, [string]$source) {
    if (-not $json.StartsWith("{", [StringComparison]::Ordinal))
    {
        throw "Invalid performance result marker in '$source'."
    }

    if (-not $seenJson.Add($json))
    {
        return
    }

    try
    {
        $result = $json | ConvertFrom-Json
    }
    catch
    {
        throw "Invalid performance result JSON in '$source': $($_.Exception.Message)"
    }

    Assert-RequiredProperty $result "schemaVersion" $source
    Assert-RequiredProperty $result "repository" $source
    Assert-RequiredProperty $result "pullRequestNumber" $source
    Assert-RequiredProperty $result "scenario" $source
    Assert-RequiredProperty $result "platform" $source
    Assert-RequiredProperty $result "variant" $source
    Assert-RequiredProperty $result "commitSha" $source
    Assert-RequiredProperty $result "harnessSha" $source
    Assert-RequiredProperty $result "runOrdinal" $source
    Assert-RequiredProperty $result "expectedVariantRuns" $source
    Assert-RequiredProperty $result "build" $source
    Assert-RequiredProperty $result "environment" $source
    Assert-RequiredProperty $result "correctness" $source
    Assert-RequiredProperty $result "timestampUtc" $source
    Assert-RequiredProperty $result "measurementsMilliseconds" $source
    Assert-RequiredProperty $result "statistics" $source
    Assert-RequiredProperty $result "counters" $source

    if ([int]$result.schemaVersion -ne 2)
    {
        throw "Unsupported performance result schema '$($result.schemaVersion)' in '$source'."
    }

    if ([int]$result.pullRequestNumber -le 0 -or
        [int]$result.runOrdinal -le 0 -or
        [int]$result.expectedVariantRuns -le 0 -or
        [int]$result.runOrdinal -gt [int]$result.expectedVariantRuns)
    {
        throw "Performance result in '$source' has invalid PR/run provenance."
    }

    if ($result.variant -notin @("base", "head") -or
        $result.platform -notin @("android", "ios", "maccatalyst", "windows"))
    {
        throw "Performance result in '$source' has an unsupported variant or platform."
    }

    foreach ($field in @("azdoBuildId", "azdoBuildUrl", "helixJobId", "helixWorkItem"))
    {
        Assert-RequiredProperty $result.build $field $source
    }

    foreach ($field in @("executionKind", "deviceModel", "osVersion", "runtimeFramework", "processArchitecture", "runtimeVariant", "sdkVersion"))
    {
        Assert-RequiredProperty $result.environment $field $source
    }

    Assert-RequiredProperty $result.correctness "passed" $source
    Assert-RequiredProperty $result.correctness "accessibilityStatus" $source
    if ($result.correctness.passed -isnot [bool] -or
        $result.correctness.accessibilityStatus -notin @("not-assessed", "passed", "failed"))
    {
        throw "Performance result in '$source' has invalid correctness metadata."
    }

    $timestamp = [DateTimeOffset]::MinValue
    if (-not [DateTimeOffset]::TryParse(
        [string]$result.timestampUtc,
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::RoundtripKind,
        [ref]$timestamp))
    {
        throw "Performance result in '$source' has an invalid timestamp."
    }

    if (@($result.measurementsMilliseconds).Count -eq 0)
    {
        throw "Performance result in '$source' contains no measurements."
    }

    $results.Add($result)
}

foreach ($path in $InputPath)
{
    foreach ($file in Get-InputFiles $path)
    {
        foreach ($line in Get-Content $file.FullName)
        {
            $chunkPrefixIndex = $line.IndexOf($chunkPrefix, [StringComparison]::Ordinal)
            if ($chunkPrefixIndex -ge 0)
            {
                $chunkRecord = $line.Substring($chunkPrefixIndex + $chunkPrefix.Length)
                $match = [regex]::Match($chunkRecord, '^([^:]+):(\d+)/(\d+):(.*)$')
                if (-not $match.Success)
                {
                    throw "Invalid performance result chunk in '$($file.FullName)'."
                }

                $chunkId = $match.Groups[1].Value
                $chunkIndex = [int]$match.Groups[2].Value
                $chunkCount = [int]$match.Groups[3].Value
                $chunk = $match.Groups[4].Value

                if ($chunkIndex -lt 1 -or $chunkIndex -gt $chunkCount)
                {
                    throw "Invalid chunk index for '$chunkId' in '$($file.FullName)'."
                }

                if (-not $chunkSets.ContainsKey($chunkId))
                {
                    $chunkSets[$chunkId] = @{
                        Count = $chunkCount
                        Parts = @{}
                    }
                }

                $chunkSet = $chunkSets[$chunkId]
                if ($chunkSet.Count -ne $chunkCount)
                {
                    throw "Conflicting chunk counts for '$chunkId'."
                }
                if ($chunkSet.Parts.ContainsKey($chunkIndex) -and
                    $chunkSet.Parts[$chunkIndex] -ne $chunk)
                {
                    throw "Conflicting chunk content for '$chunkId' part $chunkIndex."
                }

                $chunkSet.Parts[$chunkIndex] = $chunk
                continue
            }

            $prefixIndex = $line.IndexOf($prefix, [StringComparison]::Ordinal)
            if ($prefixIndex -lt 0)
            {
                continue
            }

            $json = $line.Substring($prefixIndex + $prefix.Length).Trim()
            Add-PerformanceResult $json $file.FullName
        }
    }
}

foreach ($chunkId in @($chunkSets.Keys | Sort-Object))
{
    $chunkSet = $chunkSets[$chunkId]
    if ($chunkSet.Parts.Count -ne $chunkSet.Count)
    {
        throw "Incomplete performance result chunks for '$chunkId'."
    }

    $record = (1..$chunkSet.Count | ForEach-Object { $chunkSet.Parts[$_] }) -join ""
    if (-not $record.StartsWith($prefix, [StringComparison]::Ordinal))
    {
        throw "Reassembled performance result '$chunkId' has an invalid prefix."
    }

    Add-PerformanceResult $record.Substring($prefix.Length) "chunk set '$chunkId'"
}

if ($results.Count -eq 0 -and -not $AllowEmpty)
{
    Write-Error "No '$prefix' records were found."
    exit 3
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory -and -not (Test-Path $outputDirectory))
{
    New-Item -ItemType Directory -Force -Path $outputDirectory | Out-Null
}

$output = @($results | ForEach-Object { $_ })
ConvertTo-Json -InputObject $output -Depth 12 |
    Set-Content -Path $OutputPath -Encoding UTF8

Write-Host "Wrote $($results.Count) performance result(s) to $OutputPath"
exit 0
