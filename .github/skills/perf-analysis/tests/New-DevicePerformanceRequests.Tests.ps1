#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DevicePerformance.Fixtures.ps1")
$script = Join-Path $PSScriptRoot "..\scripts\New-DevicePerformanceRequests.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-requests-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Path $testRoot | Out-Null
try {
    $selectionPath = Join-Path $testRoot "selection.json"
    $metadataPath = Join-Path $testRoot "pr.json"
    $outputPath = Join-Path $testRoot "requests.json"
    $resultsRoot = Join-Path $testRoot "results"
    New-Item -ItemType Directory -Path $resultsRoot | Out-Null
    $identity = New-DeviceIdentityFixture
    $metadata = [PSCustomObject]@{
        number = $identity.pullRequestNumber
        mergeBaseOid = $identity.baseCommitSha
        headRefOid = $identity.headCommitSha
        harnessSha = $identity.harnessSha
    }
    $selection = New-DeviceSelectionFixture @(
        "collectionview-scroll-ios", "carouselview-wheel-snap-windows", "collectionview-handler-device"
    )
    Write-DeviceFixtureJson $selectionPath $selection
    Write-DeviceFixtureJson $metadataPath $metadata
    $arguments = @{
        SelectionPath = $selectionPath
        PrMetadataPath = $metadataPath
        CurrentHeadSha = $identity.headCommitSha
        ResultsRoot = $resultsRoot
        OutputPath = $outputPath
    }
    & $script @arguments
    $requests = Get-Content -LiteralPath $outputPath -Raw | ConvertFrom-Json -NoEnumerate
    Assert-Equal $true (
        Test-DeviceJsonEqual $requests @(New-LocalDeviceRequests $selection $identity $resultsRoot)
    ) "Generated requests must survive a complete JSON round trip"
    Assert-Equal 3 $requests.Count "Request count excludes unsupported scenarios"
    Assert-Equal 1 @($requests | Where-Object { $_.platform -eq "windows" }).Count "Windows request"
    Assert-Equal 1 @($requests | Where-Object { $_.platform -eq "ios" }).Count "iOS request"
    Assert-Equal 1 @($requests | Where-Object { $_.platform -eq "maccatalyst" }).Count "MacCatalyst request"
    foreach ($request in $requests) {
        Assert-Equal "manual-local" $request.executionMode "Local execution mode"
        Assert-Equal 2 $request.expectedVariantRuns "ABBA variant count"
        Assert-Equal $true ($request.requestKey -cmatch '^maui-perf-[0-9a-f]{64}$') "Full SHA256 request key"
        Assert-Equal (Join-Path $resultsRoot $request.requestKey) $request.resultDirectory "Caller-owned result directory"
        Assert-Equal $false (Test-Path -LiteralPath $request.resultDirectory) "Requests must not execute or create result directories"
        Assert-Equal $false ($request.PSObject.Properties.Name -contains "pipeline") "No pipeline handoff"
        Assert-Equal $false ($request.PSObject.Properties.Name -contains "buildId") "No remote build identity"
    }
    Assert-Equal "eng/scripts/Run-WindowsDevicePerformanceComparison.ps1" $requests[0].driver "Windows local driver"
    Assert-Equal "maui-perf-e1397d4a5c3422d0cc44b92dd9e214ecbed5cc97fd04a08a4de88151e054ac5b" $requests[0].requestKey "Exact UTF-8 request key contract"
    Assert-Equal $false (Test-DeviceJsonEqual ([PSCustomObject]@{ flag = "true" }) ([PSCustomObject]@{ flag = $true })) "JSON equality must not coerce Boolean strings"
    Assert-Equal $false (Test-DeviceJsonEqual ([PSCustomObject]@{ number = "2" }) ([PSCustomObject]@{ number = 2 })) "JSON equality must not coerce numeric strings"
    $arrayValue = Get-DeviceProperty ([PSCustomObject]@{ nested = [PSCustomObject]@{ number = @(2) } }) "nested.number"
    Assert-Equal $true ($arrayValue -is [array]) "Native property access must not turn one-element arrays into valid scalars"

    $firstJson = Get-Content -LiteralPath $outputPath -Raw
    $selection.deviceScenarios = @($selection.deviceScenarios) + @($selection.deviceScenarios[0])
    Write-DeviceFixtureJson $selectionPath $selection
    & $script @arguments
    Assert-Equal $firstJson (Get-Content -LiteralPath $outputPath -Raw) "Duplicate selection preserves deterministic request bytes"

    foreach ($field in @("repository", "pullRequestNumber", "baseCommitSha", "headCommitSha", "harnessSha")) {
        $changed = New-DeviceIdentityFixture
        $changed.$field = switch ($field) {
            "repository" { "other/maui" }
            "pullRequestNumber" { 43 }
            default { "d" * 40 }
        }
        $originalKey = Get-LocalDeviceRequestKey $identity "carouselview-wheel-snap-windows" "windows"
        $changedKey = Get-LocalDeviceRequestKey $changed "carouselview-wheel-snap-windows" "windows"
        Assert-Equal $false ($originalKey -eq $changedKey) "Key must bind $field"
    }
    Assert-Equal $false (
        (Get-LocalDeviceRequestKey $identity "carouselview-swipe-disabled" "android") -eq
        (Get-LocalDeviceRequestKey $identity "carouselview-swipe-disabled" "ios")
    ) "Key must bind platform"
    Assert-Equal $false (
        (Get-LocalDeviceRequestKey $identity "handler-property-update-batch" "windows") -eq
        (Get-LocalDeviceRequestKey $identity "carouselview-wheel-snap-windows" "windows")
    ) "Key must bind scenario"

    foreach ($case in @("stale", "short-sha", "invalid-pr", "fractional-pr", "unsupported-pair", "missing-local", "promoted-sampled", "relative-root", "remote-root")) {
        $caseArguments = $arguments.Clone()
        $caseArguments.OutputPath = Join-Path $testRoot "$case.json"
        $caseSelection = New-DeviceSelectionFixture
        $caseMetadata = $metadata | ConvertTo-Json | ConvertFrom-Json
        switch ($case) {
            "stale" { $caseArguments.CurrentHeadSha = "d" * 40 }
            "short-sha" { $caseMetadata.harnessSha = "abc123" }
            "invalid-pr" { $caseMetadata.number = 0 }
            "fractional-pr" { $caseMetadata.number = 1.5 }
            "unsupported-pair" { $caseSelection.deviceScenarios[0].localRun.platforms = @("ios") }
            "missing-local" { $caseSelection.deviceScenarios[0].localRun = $null }
            "promoted-sampled" {
                $caseSelection = New-DeviceSelectionFixture @("handler-property-update-windows")
                $caseSelection.deviceScenarios[0].coverageMode = "direct"
            }
            "relative-root" { $caseArguments.ResultsRoot = "relative-results" }
            "remote-root" { $caseArguments.ResultsRoot = "\\server\share\results" }
        }
        Write-DeviceFixtureJson $selectionPath $caseSelection
        Write-DeviceFixtureJson $metadataPath $caseMetadata
        $failed = $false
        try { & $script @caseArguments } catch { $failed = $true }
        Assert-Equal $true $failed "Invalid request case '$case' must fail"
        Assert-Equal $false (Test-Path -LiteralPath $caseArguments.OutputPath) "Invalid request must not emit a handoff"
    }
    Write-DeviceFixtureJson $metadataPath $metadata
    foreach ($caseSelection in @(
        (New-DeviceSelectionFixture @("collectionview-handler-device")),
        ([PSCustomObject]@{ deviceScenarios = @() })
    )) {
        Write-DeviceFixtureJson $selectionPath $caseSelection
        & $script @arguments
        $empty = Get-Content -LiteralPath $outputPath -Raw | ConvertFrom-Json -NoEnumerate
        Assert-Equal $true ($empty -is [array]) "No handoff must remain a JSON array"
        Assert-Equal 0 $empty.Count "Unsupported or empty selection must not invent local work"
    }
    Write-Host "All local device performance request tests passed."
} finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
