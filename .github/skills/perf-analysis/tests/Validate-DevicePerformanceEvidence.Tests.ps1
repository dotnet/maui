#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "..\scripts\Validate-DevicePerformanceEvidence.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-evidence-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Write-Json([string]$path, $value) {
    $value | ConvertTo-Json -Depth 15 | Set-Content $path -Encoding UTF8
}

function New-Summary([string]$platform, [string]$scenario, [string]$headSha = "head123") {
    return [PSCustomObject]@{
        schemaVersion = 2
        verdict = "neutral"
        expected = [PSCustomObject]@{
            repository = "dotnet/maui"
            pullRequestNumber = 42
            baseCommitSha = "base123"
            headCommitSha = $headSha
            harnessSha = "harness123"
            platform = $platform
            scenario = $scenario
            variantRuns = 2
        }
        provenanceValidated = $true
        correctnessPassed = $true
        accessibilityStatuses = @("not-assessed")
        comparisons = @(
            [PSCustomObject]@{
                Complete = $true
                Build = [PSCustomObject]@{
                    azdoBuildId = "100"
                    azdoBuildUrl = "https://build/100"
                }
            }
        )
    }
}

New-Item -ItemType Directory -Force $testRoot | Out-Null
try {
    $selectionPath = Join-Path $testRoot "selection.json"
    Write-Json $selectionPath ([PSCustomObject]@{
        deviceScenarios = @(
            [PSCustomObject]@{
                id = "collectionview-scroll-ios"
                automationStatus = "manual-device-ci-ready"
                resultScenario = "collectionview-grouped-scrollto-makevisible"
                pipeline = [PSCustomObject]@{ platforms = @("ios", "maccatalyst") }
            }
        )
    })

    $iosPath = Join-Path $testRoot "ios.json"
    $catalystPath = Join-Path $testRoot "maccatalyst.json"
    Write-Json $iosPath (New-Summary "ios" "collectionview-grouped-scrollto-makevisible")
    Write-Json $catalystPath (New-Summary "maccatalyst" "collectionview-grouped-scrollto-makevisible")

    $common = @{
        SelectionPath = $selectionPath
        Repository = "dotnet/maui"
        PullRequestNumber = 42
        BaseCommitSha = "base123"
        HeadCommitSha = "head123"
        CurrentHeadSha = "head123"
        HarnessSha = "harness123"
    }

    $partialOut = Join-Path $testRoot "partial.json"
    & $script @common -SummaryPath $iosPath -JsonOut $partialOut
    Assert-Equal 0 $LASTEXITCODE "Partial valid evidence should seal"
    $partial = Get-Content $partialOut -Raw | ConvertFrom-Json
    Assert-Equal $true $partial.sealed "Partial provenance should be sealed"
    Assert-Equal $false $partial.allAffectedPlatformsCovered "Partial platform coverage"
    Assert-Equal 1 @($partial.missingMeasurements).Count "Missing platform count"

    $completeOut = Join-Path $testRoot "complete.json"
    & $script @common -SummaryPath @($iosPath, $catalystPath) -JsonOut $completeOut
    Assert-Equal 0 $LASTEXITCODE "Complete evidence should seal"
    $complete = Get-Content $completeOut -Raw | ConvertFrom-Json
    Assert-Equal $true $complete.deviceEvidenceComplete "Complete device evidence"
    Assert-Equal "not-assessed" $complete.accessibilityStatus "Accessibility remains explicit"

    $selectionWithUnsupported = Get-Content $selectionPath -Raw | ConvertFrom-Json
    $selectionWithUnsupported.deviceScenarios = @($selectionWithUnsupported.deviceScenarios) + @(
        [PSCustomObject]@{
            id = "collectionview-handler-device"
            automationStatus = "required-not-yet-automated"
            platforms = @("iOS")
        }
    )
    Write-Json $selectionPath $selectionWithUnsupported
    $unsupportedOut = Join-Path $testRoot "unsupported.json"
    & $script @common -SummaryPath @($iosPath, $catalystPath) -JsonOut $unsupportedOut
    Assert-Equal 0 $LASTEXITCODE "Unsupported paths are a coverage gap, not corrupt evidence"
    $unsupported = Get-Content $unsupportedOut -Raw | ConvertFrom-Json
    Assert-Equal $false $unsupported.deviceEvidenceComplete "Unsupported path blocks complete evidence"
    Assert-Equal 1 @($unsupported.missingMeasurements).Count "Unsupported measurement count"
    Write-Json $selectionPath ([PSCustomObject]@{
        deviceScenarios = @(
            [PSCustomObject]@{
                id = "collectionview-scroll-ios"
                automationStatus = "manual-device-ci-ready"
                resultScenario = "collectionview-grouped-scrollto-makevisible"
                pipeline = [PSCustomObject]@{ platforms = @("ios", "maccatalyst") }
            }
        )
    })

    $staleOut = Join-Path $testRoot "stale.json"
    & $script @common -CurrentHeadSha newerHead -SummaryPath @($iosPath, $catalystPath) -JsonOut $staleOut 2>$null
    Assert-Equal 2 $LASTEXITCODE "Stale head should fail"
    $stale = Get-Content $staleOut -Raw | ConvertFrom-Json
    Assert-Equal $false $stale.sealed "Stale evidence must not seal"

    $wrongScenarioPath = Join-Path $testRoot "wrong-scenario.json"
    Write-Json $wrongScenarioPath (New-Summary "ios" "unrelated-scenario")
    $wrongOut = Join-Path $testRoot "wrong.json"
    & $script @common -SummaryPath $wrongScenarioPath -JsonOut $wrongOut 2>$null
    Assert-Equal 2 $LASTEXITCODE "Unexpected scenario should fail"

    Write-Host "All device evidence validation tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
