#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "..\scripts\New-DevicePerformanceRequests.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-requests-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Force $testRoot | Out-Null
try {
    $selectionPath = Join-Path $testRoot "selection.json"
    $metadataPath = Join-Path $testRoot "pr.json"
    $outputPath = Join-Path $testRoot "requests.json"
    @{
        deviceScenarios = @(
            @{
                id = "collectionview-scroll-ios"
                automationStatus = "manual-device-ci-ready"
                resultScenario = "collectionview-grouped-scrollto-makevisible"
                pipeline = @{
                    path = "eng/pipelines/ci-device-performance.yml"
                    platforms = @("ios", "maccatalyst")
                }
            },
            @{
                id = "carouselview-wheel-snap-windows"
                automationStatus = "manual-device-ci-ready"
                resultScenario = "carouselview-wheel-snap-windows"
                pipeline = @{
                    path = "eng/pipelines/ci-device-performance.yml"
                    platforms = @("windows")
                }
            },
            @{
                id = "platform-hot-path"
                automationStatus = "required-not-yet-automated"
                pipeline = $null
            }
        )
    } | ConvertTo-Json -Depth 10 | Set-Content $selectionPath
    @{
        number = 42
        mergeBaseOid = "base123"
        headRefOid = "head123"
        harnessSha = "harness123"
    } | ConvertTo-Json | Set-Content $metadataPath

    & $script `
        -SelectionPath $selectionPath `
        -PrMetadataPath $metadataPath `
        -CurrentHeadSha head123 `
        -OutputPath $outputPath

    $requests = @(Get-Content $outputPath -Raw | ConvertFrom-Json)
    Assert-Equal 3 $requests.Count "Request count"
    Assert-Equal "ios" $requests[0].platform "First platform"
    Assert-Equal "maccatalyst" $requests[1].platform "Second platform"
    Assert-Equal "windows" $requests[2].platform "Third platform"
    Assert-Equal "collectionview-grouped-scrollto-makevisible" $requests[0].expectedScenario "Result scenario"
    Assert-Equal "carouselview-wheel-snap-windows" $requests[2].expectedScenario "Windows result scenario"
    Assert-Equal $true ($requests[0].requestKey -match '^maui-perf-[0-9a-f]{24}$') "Stable request key"

    $staleFailed = $false
    try {
        & $script `
            -SelectionPath $selectionPath `
            -PrMetadataPath $metadataPath `
            -CurrentHeadSha newerHead `
            -OutputPath $outputPath 2>$null
    } catch {
        $staleFailed = $true
    }
    Assert-Equal $true $staleFailed "Stale head must fail"

    @{ deviceScenarios = @() } | ConvertTo-Json | Set-Content $selectionPath
    & $script `
        -SelectionPath $selectionPath `
        -PrMetadataPath $metadataPath `
        -CurrentHeadSha head123 `
        -OutputPath $outputPath
    $emptyRequests = @(Get-Content $outputPath -Raw | ConvertFrom-Json)
    Assert-Equal 0 $emptyRequests.Count "Empty selection should write an empty request array"

    Write-Host "All device performance request tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
