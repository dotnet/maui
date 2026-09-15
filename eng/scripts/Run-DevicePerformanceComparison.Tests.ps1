#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Run-DevicePerformanceComparison.ps1"
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "..", ".."))
$devicesShared = [IO.Path]::Combine($repositoryRoot, "eng", "devices", "devices-shared.cake")
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-perf-driver-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual)
    {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try
{
    $baseApp = Join-Path $testRoot "base app.app"
    $headApp = Join-Path $testRoot "head app.app"
    $output = Join-Path $testRoot "output"
    New-Item -ItemType Directory -Force -Path $baseApp, $headApp | Out-Null
    $runArguments = @{
        Platform = "ios"
        BaseApp = $baseApp
        HeadApp = $headApp
        BaseCommitSha = "abc123"
        HeadCommitSha = "def456"
        ExpectedScenario = "collectionview-grouped-scrollto-makevisible"
        Repository = "dotnet/maui"
        PullRequestNumber = 42
        PullRequestAuthor = "perf-author"
        HarnessSha = "harness123"
        BaseRuntimeVariant = "mono"
        HeadRuntimeVariant = "mono"
        BaseSdkVersion = "10.0.100"
        HeadSdkVersion = "10.0.101"
        OutputDirectory = $output
        DeviceId = "simulator-id"
        DryRun = $true
    }

    & $script @runArguments 6>$null
    Assert-Equal 0 $LASTEXITCODE "Dry-run should succeed"
    $plan = Get-Content (Join-Path $output "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal 4 $plan.Count "ABBA run count"
    Assert-Equal "base,head,head,base" ($plan.variant -join ",") "ABBA run order"
    Assert-Equal "dotnet" $plan[0].executable "Default local executable"
    Assert-Equal "xharness" $plan[0].arguments[0] "Local tool invocation"
    Assert-Equal $true ($plan[0].arguments -contains $baseApp) "App paths with spaces remain a single argument"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=TestFilter=Category=PerformanceCollectionViewScroll") "iOS performance filter"
    Assert-Equal $true ($plan[1].arguments -contains "--set-env=MAUI_PERF_VARIANT=head") "Head variant metadata"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_PR_NUMBER=42") "iOS PR provenance"
    Assert-Equal $true ($plan[2].arguments -contains "--set-env=MAUI_PERF_RUN_ORDINAL=2") "iOS ABBA run ordinal"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_HARNESS_SHA=harness123") "iOS harness provenance"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_SDK_VERSION=10.0.100") "iOS base SDK provenance"
    Assert-Equal $true ($plan[1].arguments -contains "--set-env=MAUI_PERF_SDK_VERSION=10.0.101") "iOS head SDK provenance"

    $driverSource = Get-Content $script -Raw
    Assert-Equal $true $driverSource.Contains('-PullRequestAuthor $PullRequestAuthor') "Driver forwards author to the shared renderer"
    foreach ($artifactName in @("results.json", "comparison-summary.json", "comparison-summary.md")) {
        Assert-Equal $true $driverSource.Contains($artifactName) "Driver artifact contract for $artifactName"
    }

    $devicesSharedSource = Get-Content $devicesShared -Raw
    Assert-Equal $true (
        $devicesSharedSource.Contains('category.StartsWith("Performance", StringComparison.Ordinal)')
    ) "Normal Apple device runs must exclude all performance categories"

    $globalArguments = $runArguments.Clone()
    $globalArguments.XHarnessMode = "global"
    $globalArguments.OutputDirectory = Join-Path $testRoot "global-output"
    & $script @globalArguments 6>$null
    $globalPlan = Get-Content (Join-Path $globalArguments.OutputDirectory "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal "xharness" $globalPlan[0].executable "Global local executable"
    Assert-Equal "apple" $globalPlan[0].arguments[0] "Global tool invocation"

    $driverCommand = Get-Command $script
    $modeValidation = $driverCommand.Parameters["XHarnessMode"].Attributes |
        Where-Object { $_ -is [Management.Automation.ValidateSetAttribute] }
    Assert-Equal "dotnet,global" ($modeValidation.ValidValues -join ",") "Only installed local XHarness modes are supported"

    $macCatalystArguments = $runArguments.Clone()
    $macCatalystArguments.Platform = "maccatalyst"
    $macCatalystArguments.OutputDirectory = Join-Path $testRoot "maccatalyst-output"
    $macCatalystResultRoot = Join-Path $testRoot "maccatalyst-results"
    $macCatalystArguments.MacCatalystResultFileRoot = $macCatalystResultRoot
    & $script @macCatalystArguments 6>$null
    $macCatalystPlan = Get-Content (Join-Path $macCatalystArguments.OutputDirectory "run-plan.json") -Raw | ConvertFrom-Json
    $resolvedResultRoot = (Resolve-Path $macCatalystResultRoot).Path
    Assert-Equal 4 (@($macCatalystPlan.resultFile | Sort-Object -Unique).Count) "MacCatalyst result file count"
    Assert-Equal $true ([string]$macCatalystPlan[0].resultFile).StartsWith(
        $resolvedResultRoot,
        [StringComparison]::Ordinal) "MacCatalyst result file root"
    Assert-Equal $true ($macCatalystPlan[0].arguments -contains
        "--set-env=MAUI_PERF_RESULT_FILE=$($macCatalystPlan[0].resultFile)") "MacCatalyst result file environment"

    $invalidResultRootFailed = $false
    try
    {
        $invalidArguments = $macCatalystArguments.Clone()
        $invalidArguments.Platform = "ios"
        $invalidArguments.OutputDirectory = Join-Path $testRoot "invalid-result-root"
        & $script @invalidArguments 6>$null
    }
    catch
    {
        $invalidResultRootFailed = $_.Exception.Message -like "*MacCatalystResultFileRoot can only be used*"
    }
    Assert-Equal $true $invalidResultRootFailed "iOS should reject MacCatalystResultFileRoot"

    $baseApk = Join-Path $testRoot "base.apk"
    $headApk = Join-Path $testRoot "head.apk"
    New-Item -ItemType File -Force -Path $baseApk, $headApk | Out-Null
    $androidArguments = $runArguments.Clone()
    $androidArguments.Platform = "android"
    $androidArguments.BaseApp = $baseApk
    $androidArguments.HeadApp = $headApk
    $androidArguments.DeviceId = "emulator-5554"
    foreach ($scenario in @(
        @{ Name = "collectionview-keepitemsinview-update"; Category = "PerformanceCollectionViewItemsUpdate" },
        @{ Name = "carouselview-swipe-disabled"; Category = "PerformanceCarouselViewSwipe" },
        @{ Name = "handler-property-update-batch"; Category = "PerformanceHandlerPropertyUpdate" }
    )) {
        $androidArguments.ExpectedScenario = $scenario.Name
        $androidArguments.OutputDirectory = Join-Path $testRoot $scenario.Name
        & $script @androidArguments 6>$null
        $androidPlan = Get-Content (Join-Path $androidArguments.OutputDirectory "run-plan.json") -Raw | ConvertFrom-Json
        Assert-Equal 4 $androidPlan.Count "Android ABBA run count"
        Assert-Equal $true ($androidPlan[0].arguments -contains "TestFilter=Category=$($scenario.Category)") "Android performance filter"
        Assert-Equal $true ($androidPlan[0].arguments -contains "com.microsoft.maui.controls.devicetests.TestInstrumentation") "Android instrumentation"
        Assert-Equal $true ($androidPlan[1].arguments -contains "MAUI_PERF_VARIANT=head") "Android head metadata"
        Assert-Equal $true ($androidPlan[0].arguments -contains "MAUI_PERF_REPOSITORY=dotnet/maui") "Android repository provenance"
        Assert-Equal $true ($androidPlan[3].arguments -contains "MAUI_PERF_RUN_ORDINAL=2") "Android ABBA run ordinal"
        Assert-Equal $true ($androidPlan[0].arguments -contains "MAUI_PERF_HARNESS_SHA=harness123") "Android harness provenance"
        Assert-Equal $true ($androidPlan[0].arguments -contains "MAUI_PERF_RUNTIME_VARIANT=mono") "Android runtime provenance"
    }

    foreach ($platform in @("ios", "maccatalyst")) {
        foreach ($scenario in @("carouselview-swipe-disabled", "collectionview-keepitemsinview-update")) {
            $supported = $runArguments.Clone()
            $supported.Platform = $platform
            $supported.ExpectedScenario = $scenario
            $supported.OutputDirectory = Join-Path $testRoot "$platform-$scenario"
            & $script @supported 6>$null
            Assert-Equal 0 $LASTEXITCODE "Supported $platform/$scenario pair"
            Assert-Equal $true (Test-Path (Join-Path $supported.OutputDirectory "run-plan.json")) "Supported pair produces a plan"
        }
    }
    foreach ($pair in @(
        @{ Platform = "android"; Scenario = "collectionview-grouped-scrollto-makevisible" },
        @{ Platform = "ios"; Scenario = "handler-property-update-batch" },
        @{ Platform = "maccatalyst"; Scenario = "handler-property-update-batch" }
    )) {
        $unsupported = $runArguments.Clone()
        $unsupported.Platform = $pair.Platform
        $unsupported.ExpectedScenario = $pair.Scenario
        $unsupported.BaseApp = Join-Path $testRoot "missing-base"
        $unsupported.HeadApp = Join-Path $testRoot "missing-head"
        $unsupported.OutputDirectory = Join-Path $testRoot "unsupported-$($pair.Platform)"
        $rejected = $false
        try {
            & $script @unsupported 6>$null
        } catch {
            $rejected = $_.Exception.Message -like "*not supported on platform*"
        }
        Assert-Equal $true $rejected "Unsupported pair must fail before app discovery"
        Assert-Equal $false (Test-Path $unsupported.OutputDirectory) "Unsupported pair must not create an execution plan"
    }

    Write-Host "All device performance driver tests passed."
}
finally
{
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
