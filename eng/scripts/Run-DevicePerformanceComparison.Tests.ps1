#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Run-DevicePerformanceComparison.ps1"
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "..", ".."))
$helixProject = [IO.Path]::Combine($repositoryRoot, "eng", "helix_device_performance.proj")
$workflowSource = [IO.Path]::Combine($repositoryRoot, ".github", "workflows", "perf-check.md")
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
    $baseApp = Join-Path $testRoot "base.app"
    $headApp = Join-Path $testRoot "head.app"
    $output = Join-Path $testRoot "output"
    New-Item -ItemType Directory -Force -Path $baseApp, $headApp | Out-Null

    & $script `
        -Platform ios `
        -BaseApp $baseApp `
        -HeadApp $headApp `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario collectionview-grouped-scrollto-makevisible `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $output `
        -DeviceId simulator-id `
        -DryRun

    Assert-Equal 0 $LASTEXITCODE "Dry-run should succeed"

    $plan = Get-Content (Join-Path $output "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal 4 ($plan.Count) "ABBA run count"
    Assert-Equal "base" $plan[0].variant "Run 1 variant"
    Assert-Equal "head" $plan[1].variant "Run 2 variant"
    Assert-Equal "head" $plan[2].variant "Run 3 variant"
    Assert-Equal "base" $plan[3].variant "Run 4 variant"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=TestFilter=Category=PerformanceCollectionViewScroll") "iOS performance filter"
    Assert-Equal $true ($plan[1].arguments -contains "--set-env=MAUI_PERF_VARIANT=head") "Head variant metadata"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_PR_NUMBER=42") "iOS PR provenance"
    Assert-Equal $true ($plan[2].arguments -contains "--set-env=MAUI_PERF_RUN_ORDINAL=2") "iOS ABBA run ordinal"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_HARNESS_SHA=harness123") "iOS harness provenance"
    Assert-Equal $true ($plan[0].arguments -contains "--set-env=MAUI_PERF_SDK_VERSION=10.0.100") "iOS base SDK provenance"
    Assert-Equal $true ($plan[1].arguments -contains "--set-env=MAUI_PERF_SDK_VERSION=10.0.101") "iOS head SDK provenance"

    $driverSource = Get-Content $script -Raw
    $helixProjectSource = Get-Content $helixProject -Raw
    $workflowSourceText = Get-Content $workflowSource -Raw
    foreach ($artifactName in @("results.json", "comparison-summary.json", "comparison-summary.md")) {
        Assert-Equal $true $driverSource.Contains($artifactName) "Driver artifact contract for $artifactName"
        Assert-Equal $true $helixProjectSource.Contains($artifactName) "Helix artifact contract for $artifactName"
    }
    Assert-Equal $true $workflowSourceText.Contains("comparison-summary.json") "Workflow summary artifact contract"

    $devicesSharedSource = Get-Content $devicesShared -Raw
    Assert-Equal $true (
        $devicesSharedSource.Contains('category.StartsWith("Performance", StringComparison.Ordinal)')
    ) "Normal Apple device runs must exclude all performance categories"

    $macCatalystOutput = Join-Path $testRoot "maccatalyst-output"
    $macCatalystResultRoot = Join-Path $testRoot "maccatalyst-results"
    & $script `
        -Platform maccatalyst `
        -BaseApp $baseApp `
        -HeadApp $headApp `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario collectionview-grouped-scrollto-makevisible `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $macCatalystOutput `
        -MacCatalystResultFileRoot $macCatalystResultRoot `
        -DryRun

    $macCatalystPlan = Get-Content (Join-Path $macCatalystOutput "run-plan.json") -Raw | ConvertFrom-Json
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
        & $script `
            -Platform ios `
            -BaseApp $baseApp `
            -HeadApp $headApp `
            -BaseCommitSha abc123 `
            -HeadCommitSha def456 `
            -ExpectedScenario collectionview-grouped-scrollto-makevisible `
            -Repository dotnet/maui `
            -PullRequestNumber 42 `
            -HarnessSha harness123 `
            -AzdoBuildId 100 `
            -AzdoBuildUrl https://build/100 `
            -BaseRuntimeVariant mono `
            -HeadRuntimeVariant mono `
            -BaseSdkVersion 10.0.100 `
            -HeadSdkVersion 10.0.101 `
            -OutputDirectory (Join-Path $testRoot "invalid-result-root") `
            -MacCatalystResultFileRoot $macCatalystResultRoot `
            -DryRun
    }
    catch
    {
        $invalidResultRootFailed = $true
    }
    Assert-Equal $true $invalidResultRootFailed "iOS should reject MacCatalystResultFileRoot"

    $androidOutput = Join-Path $testRoot "android-output"
    $baseApk = Join-Path $testRoot "base.apk"
    $headApk = Join-Path $testRoot "head.apk"
    New-Item -ItemType File -Force -Path $baseApk, $headApk | Out-Null

    & $script `
        -Platform android `
        -BaseApp $baseApk `
        -HeadApp $headApk `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario collectionview-keepitemsinview-update `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $androidOutput `
        -DeviceId emulator-5554 `
        -DryRun

    $androidPlan = Get-Content (Join-Path $androidOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal $true ($androidPlan[0].arguments -contains "TestFilter=Category=PerformanceCollectionViewItemsUpdate") "Android performance filter"
    Assert-Equal $true ($androidPlan[0].arguments -contains "com.microsoft.maui.controls.devicetests.TestInstrumentation") "Android instrumentation"
    Assert-Equal $true ($androidPlan[1].arguments -contains "MAUI_PERF_VARIANT=head") "Android head metadata"
    Assert-Equal $true ($androidPlan[0].arguments -contains "MAUI_PERF_REPOSITORY=dotnet/maui") "Android repository provenance"
    Assert-Equal $true ($androidPlan[3].arguments -contains "MAUI_PERF_RUN_ORDINAL=2") "Android ABBA run ordinal"
    Assert-Equal $true ($androidPlan[0].arguments -contains "MAUI_PERF_AZDO_BUILD_ID=100") "Android build provenance"

    $carouselOutput = Join-Path $testRoot "carousel-output"
    & $script `
        -Platform android `
        -BaseApp $baseApk `
        -HeadApp $headApk `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario carouselview-swipe-disabled `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $carouselOutput `
        -DeviceId emulator-5554 `
        -DryRun

    $carouselPlan = Get-Content (Join-Path $carouselOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal $true ($carouselPlan[0].arguments -contains "TestFilter=Category=PerformanceCarouselViewSwipe") "CarouselView performance filter"

    $handlerOutput = Join-Path $testRoot "handler-output"
    & $script `
        -Platform android `
        -BaseApp $baseApk `
        -HeadApp $headApk `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario handler-property-update-batch `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant mono `
        -HeadRuntimeVariant mono `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $handlerOutput `
        -DeviceId emulator-5554 `
        -DryRun
    $handlerPlan = Get-Content (Join-Path $handlerOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal $true ($handlerPlan[0].arguments -contains "TestFilter=Category=PerformanceHandlerPropertyUpdate") "Handler family performance filter"

    $helixOutput = Join-Path $testRoot "helix-output"
    $previousXHarnessCliPath = $env:XHARNESS_CLI_PATH
    $previousHelixCorrelationId = $env:HELIX_CORRELATION_ID
    $previousHelixWorkItem = $env:HELIX_WORKITEM_FRIENDLYNAME
    try
    {
        $env:XHARNESS_CLI_PATH = "/payload/Microsoft.DotNet.XHarness.CLI.dll"
        $env:HELIX_CORRELATION_ID = "helix-job"
        $env:HELIX_WORKITEM_FRIENDLYNAME = "DevicePerformance-ios"
        & $script `
            -Platform ios `
            -BaseApp $baseApp `
            -HeadApp $headApp `
            -BaseCommitSha abc123 `
            -HeadCommitSha def456 `
            -ExpectedScenario collectionview-grouped-scrollto-makevisible `
            -Repository dotnet/maui `
            -PullRequestNumber 42 `
            -HarnessSha harness123 `
            -AzdoBuildId 100 `
            -AzdoBuildUrl https://build/100 `
            -BaseRuntimeVariant mono `
            -HeadRuntimeVariant mono `
            -BaseSdkVersion 10.0.100 `
            -HeadSdkVersion 10.0.101 `
            -OutputDirectory $helixOutput `
            -XHarnessMode helix `
            -DryRun

        $helixPlan = Get-Content (Join-Path $helixOutput "run-plan.json") -Raw | ConvertFrom-Json
        Assert-Equal "dotnet" $helixPlan[0].executable "Helix executable"
        Assert-Equal "exec" $helixPlan[0].arguments[0] "Helix dotnet exec argument"
        Assert-Equal $env:XHARNESS_CLI_PATH $helixPlan[0].arguments[1] "Helix CLI path"
    }
    finally
    {
        $env:XHARNESS_CLI_PATH = $previousXHarnessCliPath
        $env:HELIX_CORRELATION_ID = $previousHelixCorrelationId
        $env:HELIX_WORKITEM_FRIENDLYNAME = $previousHelixWorkItem
    }

    Write-Host "All device performance driver tests passed."
}
finally
{
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
