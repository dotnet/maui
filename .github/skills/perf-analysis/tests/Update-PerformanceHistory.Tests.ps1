#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$skillRoot = Split-Path -Parent $PSScriptRoot
$script = [IO.Path]::Combine($skillRoot, "scripts", "Update-PerformanceHistory.ps1")
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-perf-history-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Force $testRoot | Out-Null
try {
    $summaryPath = Join-Path $testRoot "summary.json"
    $manifestPath = Join-Path $testRoot "manifest.json"
    $historyPath = Join-Path $testRoot "history.json"
    @{
        schemaVersion = 2
        verdict = "neutral"
        benchmarks = @(@{
            name = "Sample"
            flag = "neutral"
            allocation = @{ baseMedian = 10; headMedian = 10 }
            time = @{ baseMedianNs = 100; headMedianNs = 101 }
        })
    } | ConvertTo-Json -Depth 8 | Set-Content $summaryPath -Encoding UTF8
    @{
        schemaVersion = 2
        prNumber = 42
        baseRef = "main"
        baseSha = "base"
        headSha = "head"
        status = "complete"
    } | ConvertTo-Json | Set-Content $manifestPath -Encoding UTF8

    & $script -SummaryPath $summaryPath -ManifestPath $manifestPath -OutputPath $historyPath
    $history = Get-Content $historyPath -Raw | ConvertFrom-Json
    Assert-Equal 1 @($history.points).Count "Initial history point count"
    Assert-Equal "Sample" $history.points[0].benchmarks[0].name "Benchmark identity"

    & $script -SummaryPath $summaryPath -ManifestPath $manifestPath -HistoryPath $historyPath -OutputPath $historyPath
    $history = Get-Content $historyPath -Raw | ConvertFrom-Json
    Assert-Equal 1 @($history.points).Count "Same head/environment should replace its point"
    Assert-Equal 42 $history.points[0].prNumber "PR provenance"

    $pointPath = Join-Path $testRoot "point.json"
    $history.points[0] | ConvertTo-Json -Depth 12 | Set-Content $pointPath -Encoding UTF8
    Remove-Item $historyPath
    & $script -PointPath $pointPath -OutputPath $historyPath
    $history = Get-Content $historyPath -Raw | ConvertFrom-Json
    Assert-Equal "head" $history.points[0].headSha "Sealed point merge"

    $incompletePoint = $history.points[0]
    $incompletePoint.status = "incomplete"
    $incompletePoint | ConvertTo-Json -Depth 12 | Set-Content $pointPath -Encoding UTF8
    $incompleteRejected = $false
    try {
        & $script -PointPath $pointPath -OutputPath $historyPath
    }
    catch {
        $incompleteRejected = $_.Exception.Message -like "*missing required provenance or benchmark data*"
    }
    Assert-Equal $true $incompleteRejected "Incomplete benchmark point rejection"

    Write-Host "Performance history tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
