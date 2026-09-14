#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Parse-DevicePerformanceResults.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-perf-parser-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual)
    {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try
{
    $input = Join-Path $testRoot "xharness.log"
    $output = Join-Path $testRoot "results.json"
    $baseRecord = 'MAUI_PERF_RESULT:{"schemaVersion":3,"repository":"dotnet/maui","pullRequestNumber":42,"scenario":"collectionview-scroll","platform":"ios","variant":"base","commitSha":"abc123","harnessSha":"harness123","runOrdinal":1,"expectedVariantRuns":1,"environment":{"executionKind":"simulator","deviceModel":"iPhone","osVersion":"18.5","runtimeFramework":".NET 10","processArchitecture":"Arm64","runtimeVariant":"mono","sdkVersion":"10.0"},"correctness":{"passed":true,"accessibilityStatus":"not-assessed"},"timestampUtc":"2026-07-13T09:00:00Z","measurementsMilliseconds":[10.1,11.2],"statistics":{"minimumMilliseconds":10.1,"maximumMilliseconds":11.2,"medianMilliseconds":10.65,"p95Milliseconds":11.2,"meanMilliseconds":10.65},"counters":{"layoutPasses":4}}'
    $headRecord = 'MAUI_PERF_RESULT:{"schemaVersion":3,"repository":"dotnet/maui","pullRequestNumber":42,"scenario":"collectionview-scroll","platform":"ios","variant":"head","commitSha":"def456","harnessSha":"harness123","runOrdinal":1,"expectedVariantRuns":1,"environment":{"executionKind":"simulator","deviceModel":"iPhone","osVersion":"18.5","runtimeFramework":".NET 10","processArchitecture":"Arm64","runtimeVariant":"mono","sdkVersion":"10.0"},"correctness":{"passed":true,"accessibilityStatus":"not-assessed"},"timestampUtc":"2026-07-13T09:00:01Z","measurementsMilliseconds":[8.1,8.2],"statistics":{"minimumMilliseconds":8.1,"maximumMilliseconds":8.2,"medianMilliseconds":8.15,"p95Milliseconds":8.2,"meanMilliseconds":8.15},"counters":{"layoutPasses":2}}'
    $chunkedRecord = $baseRecord.
        Replace('"platform":"ios"', '"platform":"maccatalyst"').
        Replace('"commitSha":"abc123"', '"commitSha":"chunk123"')
    $chunkSplit = [int][Math]::Floor($chunkedRecord.Length / 2)
    $chunk1 = $chunkedRecord.Substring(0, $chunkSplit)
    $chunk2 = $chunkedRecord.Substring($chunkSplit)

    @(
        "Unrelated log line",
        "2026-07-13 09:00:00 $baseRecord",
        $headRecord,
        "MAUI_PERF_CHUNK:chunk-result:2/2:$chunk2",
        "MAUI_PERF_CHUNK:chunk-result:1/2:$chunk1",
        "MAUI_PERF_CHUNK:chunk-result:1/2:$chunk1"
    ) | Set-Content $input -Encoding UTF8

    & $script -InputPath $input -OutputPath $output
    Assert-Equal 0 $LASTEXITCODE "Parser should succeed"

    $parsed = Get-Content $output -Raw | ConvertFrom-Json
    Assert-Equal 3 ($parsed.Count) "Parser should return full and chunked records"
    Assert-Equal "base" $parsed[0].variant "First record variant"
    Assert-Equal "head" $parsed[1].variant "Second record variant"
    Assert-Equal 2 (@($parsed[1].measurementsMilliseconds).Count) "Head measurement count"
    Assert-Equal "maccatalyst" $parsed[2].platform "Chunked record platform"
    Assert-Equal "chunk123" $parsed[2].commitSha "Chunked record commit"
    Assert-Equal 3 $parsed[0].schemaVersion "Local result schema version"
    Assert-Equal $null $parsed[0].PSObject.Properties["build"] "Local records need no remote build identity"
    $resultSource = Get-Content (Join-Path $PSScriptRoot "../../src/Core/tests/DeviceTests.Shared/DevicePerformanceResult.cs") -Raw
    Assert-Equal $true $resultSource.Contains("CurrentSchemaVersion = 3;") "Native emitter and parser must use the same schema"

    $windowsInput = Join-Path $testRoot "windows.log"
    $windowsOutput = Join-Path $testRoot "windows.json"
    $baseRecord.Replace('"platform":"ios"', '"platform":"windows"') |
        Set-Content $windowsInput -Encoding UTF8
    & $script -InputPath $windowsInput -OutputPath $windowsOutput
    Assert-Equal 0 $LASTEXITCODE "Windows result parsing should succeed"
    Assert-Equal "windows" (Get-Content $windowsOutput -Raw | ConvertFrom-Json).platform "Windows platform"

    $emptyInput = Join-Path $testRoot "empty.log"
    $emptyOutput = Join-Path $testRoot "empty.json"
    "No performance records" | Set-Content $emptyInput -Encoding UTF8

    & $script -InputPath $emptyInput -OutputPath $emptyOutput -AllowEmpty
    Assert-Equal 0 $LASTEXITCODE "AllowEmpty should succeed"
    $emptyResults = Get-Content $emptyOutput -Raw | ConvertFrom-Json
    Assert-Equal 0 ($emptyResults.Count) "AllowEmpty result count"

    $incompleteInput = Join-Path $testRoot "incomplete-chunk.log"
    $incompleteOutput = Join-Path $testRoot "incomplete-chunk.json"
    "MAUI_PERF_CHUNK:incomplete:1/2:$chunk1" |
        Set-Content $incompleteInput -Encoding UTF8
    $incompleteFailed = $false
    try {
        & $script -InputPath $incompleteInput -OutputPath $incompleteOutput
    } catch {
        $incompleteFailed = $true
    }
    Assert-Equal $true $incompleteFailed "Incomplete chunks should fail"

    $invalidInput = Join-Path $testRoot "invalid.log"
    $invalidOutput = Join-Path $testRoot "invalid.json"
    $baseRecord.Replace('"runOrdinal":1', '"runOrdinal":2') |
        Set-Content $invalidInput -Encoding UTF8
    $invalidFailed = $false
    try {
        & $script -InputPath $invalidInput -OutputPath $invalidOutput
    } catch {
        $invalidFailed = $_.Exception.Message -like "*invalid PR/run provenance*"
    }
    Assert-Equal $true $invalidFailed "Invalid run provenance should fail"

    foreach ($invalidCase in @(
        @{ Record = $baseRecord.Replace('"schemaVersion":3', '"schemaVersion":2'); Error = "*Unsupported performance result schema*"; Name = "Old schema" },
        @{ Record = $baseRecord.Replace('"harnessSha":"harness123",', ''); Error = "*missing 'harnessSha'*"; Name = "Missing harness identity" },
        @{ Record = $baseRecord.Replace('"sdkVersion":"10.0"', '"sdkVersion":""'); Error = "*missing 'sdkVersion'*"; Name = "Missing SDK identity" }
    )) {
        $invalidCase.Record | Set-Content $invalidInput -Encoding UTF8
        $rejected = $false
        try {
            & $script -InputPath $invalidInput -OutputPath $invalidOutput
        } catch {
            $rejected = $_.Exception.Message -like $invalidCase.Error
        }
        Assert-Equal $true $rejected "$($invalidCase.Name) should fail"
    }

    Write-Host "All device performance parser tests passed."
}
finally
{
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
