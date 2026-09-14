#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Run-WindowsDevicePerformanceComparison.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-windows-device-perf-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
try {
    $baseApp = Join-Path $testRoot "base/Microsoft.Maui.Controls.DeviceTests.exe"
    $headApp = Join-Path $testRoot "head/Microsoft.Maui.Controls.DeviceTests.exe"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $baseApp), (Split-Path -Parent $headApp) | Out-Null
    New-Item -ItemType File -Force -Path $baseApp, $headApp | Out-Null
    $output = Join-Path $testRoot "output"

    & $script `
        -BaseApp $baseApp `
        -HeadApp $headApp `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario carouselview-wheel-snap-windows `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -PullRequestAuthor perf-author `
        -HarnessSha harness123 `
        -BaseRuntimeVariant coreclr `
        -HeadRuntimeVariant coreclr `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $output `
        -DryRun

    Assert-Equal 0 $LASTEXITCODE "Windows driver dry-run"
    $plan = Get-Content (Join-Path $output "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal 4 @($plan).Count "Windows ABBA run count"
    Assert-Equal "base" $plan[0].variant "Windows first run"
    Assert-Equal "head" $plan[1].variant "Windows second run"
    Assert-Equal "head" $plan[2].variant "Windows third run"
    Assert-Equal "base" $plan[3].variant "Windows fourth run"
    Assert-Equal "PerformanceCarouselViewWheelSnap" $plan[0].category "Windows category"
    $driverSource = Get-Content $script -Raw
    Assert-Equal $true $driverSource.Contains('-PullRequestAuthor $PullRequestAuthor') "Windows driver forwards author to the shared renderer"

    $handlerOutput = Join-Path $testRoot "handler-output"
    & $script `
        -BaseApp $baseApp `
        -HeadApp $headApp `
        -BaseCommitSha abc123 `
        -HeadCommitSha def456 `
        -ExpectedScenario handler-property-update-batch `
        -Repository dotnet/maui `
        -PullRequestNumber 42 `
        -HarnessSha harness123 `
        -BaseRuntimeVariant coreclr `
        -HeadRuntimeVariant coreclr `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $handlerOutput `
        -DryRun
    $handlerPlan = Get-Content (Join-Path $handlerOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal "PerformanceHandlerPropertyUpdate" $handlerPlan[0].category "Windows handler family category"

    if ($IsWindows) {
        $failingBaseApp = Join-Path $testRoot "failing-base/fail.cmd"
        $failingHeadApp = Join-Path $testRoot "failing-head/fail.cmd"
        New-Item -ItemType Directory -Force -Path (Split-Path -Parent $failingBaseApp), (Split-Path -Parent $failingHeadApp) | Out-Null
        "@echo off`r`nexit /b 7`r`n" | Set-Content $failingBaseApp -Encoding ASCII
        Copy-Item $failingBaseApp $failingHeadApp

        $failingOutput = Join-Path $testRoot "failing-output"
        New-Item -ItemType Directory -Force -Path (Join-Path $failingOutput "base-discovery") | Out-Null
        "PerformanceCarouselViewWheelSnap" |
            Set-Content (Join-Path $failingOutput "base-discovery/devicetestcategories.txt")

        $failedRunRejected = $false
        try {
            & $script `
                -BaseApp $failingBaseApp `
                -HeadApp $failingHeadApp `
                -BaseCommitSha abc123 `
                -HeadCommitSha def456 `
                -ExpectedScenario carouselview-wheel-snap-windows `
                -Repository dotnet/maui `
                -PullRequestNumber 42 `
                -HarnessSha harness123 `
                -BaseRuntimeVariant coreclr `
                -HeadRuntimeVariant coreclr `
                -BaseSdkVersion 10.0.100 `
                -HeadSdkVersion 10.0.101 `
                -OutputDirectory $failingOutput `
                -DiscoveryTimeoutSeconds 5 `
                -RunTimeoutSeconds 5
        }
        catch {
            $failedRunRejected = $_.Exception.Message -like "*exited with code 7*"
        }
        Assert-Equal $true $failedRunRejected "A stale discovery file must not hide a failed app process"
    }

    Write-Host "Windows device performance driver tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
