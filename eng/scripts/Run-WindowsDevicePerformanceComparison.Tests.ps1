#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Run-WindowsDevicePerformanceComparison.ps1"
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "..", ".."))
$helixProject = Join-Path $repositoryRoot "eng\helix_device_performance.proj"
$pipeline = Join-Path $repositoryRoot "eng\pipelines\ci-device-performance.yml"
$buildJob = Join-Path $repositoryRoot "eng\pipelines\common\device-performance-build-job.yml"
$provisionTemplate = Join-Path $repositoryRoot "eng\pipelines\common\provision.yml"
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
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
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
        -AzdoBuildId 100 `
        -AzdoBuildUrl https://build/100 `
        -BaseRuntimeVariant coreclr `
        -HeadRuntimeVariant coreclr `
        -BaseSdkVersion 10.0.100 `
        -HeadSdkVersion 10.0.101 `
        -OutputDirectory $handlerOutput `
        -DryRun
    $handlerPlan = Get-Content (Join-Path $handlerOutput "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal "PerformanceHandlerPropertyUpdate" $handlerPlan[0].category "Windows handler family category"

    $helixSource = Get-Content $helixProject -Raw
    Assert-Equal $true $helixSource.Contains("windows.11.amd64.client.open") "Windows Helix queue"
    Assert-Equal $true $helixSource.Contains("Run-WindowsDevicePerformanceComparison.ps1") "Windows Helix runner"
    $pipelineSource = Get-Content $pipeline -Raw
    Assert-Equal $true $pipelineSource.Contains("- windows") "Windows pipeline parameter"
    Assert-Equal $true $pipelineSource.Contains("DEVICE_PERFORMANCE_SCENARIO") "Pipeline scenario environment binding"
    Assert-Equal $true $pipelineSource.Contains('DEVICE_PERFORMANCE_PR_AUTHOR: ${{ parameters.pullRequestAuthor }}') "Pipeline report author environment binding"
    Assert-Equal $true $pipelineSource.Contains("Compare-DevicePerformanceResults.Tests.ps1") "Pipeline guards the generated comment format"
    $buildJobSource = Get-Content $buildJob -Raw
    $prValidation = [regex]::Match(
        $buildJobSource,
        '(?m)^  - pwsh: \|\r?\n(?<script>(?:      [^\r\n]*\r?\n)+)    displayName: Validate device performance PR number\r?\n    env:\r?\n      DEVICE_PERFORMANCE_PR_NUMBER: \$\{\{ parameters\.prNumber \}\}')
    Assert-Equal $true $prValidation.Success "Build job must validate PR number before fetch"
    Assert-Equal $true ($prValidation.Index -lt $buildJobSource.IndexOf("git cat-file")) "PR number validation must precede both platform fetch paths"
    $validatePrNumber = [scriptblock]::Create($prValidation.Groups["script"].Value)
    $savedPrNumber = $env:DEVICE_PERFORMANCE_PR_NUMBER
    try {
        foreach ($invalidPrNumber in @("", "0", "-1", "1.5", "not-a-number", "2147483648")) {
            $env:DEVICE_PERFORMANCE_PR_NUMBER = $invalidPrNumber
            $invalidRejected = $false
            try {
                & $validatePrNumber
            }
            catch {
                $invalidRejected = $_.Exception.Message -like "*prNumber must be a positive integer*"
            }
            Assert-Equal $true $invalidRejected "Invalid PR number '$invalidPrNumber' must fail clearly"
        }
        $env:DEVICE_PERFORMANCE_PR_NUMBER = "38274"
        & $validatePrNumber
    }
    finally {
        $env:DEVICE_PERFORMANCE_PR_NUMBER = $savedPrNumber
    }
    $provisionSource = Get-Content $provisionTemplate -Raw
    $declaredProvisionParameters = @(
        [regex]::Matches(($provisionSource -split '(?m)^steps:', 2)[0], '(?m)^  (?<name>[A-Za-z][A-Za-z0-9]*):') |
            ForEach-Object { $_.Groups['name'].Value }
    )
    $provisionCall = [regex]::Match(
        $buildJobSource,
        '(?m)^  - template: /eng/pipelines/common/provision\.yml@self\r?\n    parameters:\r?\n(?<parameters>(?:      [^\r\n]*(?:\r?\n|$))+)')
    Assert-Equal $true $provisionCall.Success "Build job provisioning template call"
    foreach ($parameter in [regex]::Matches($provisionCall.Groups['parameters'].Value, '(?m)^      (?<name>[A-Za-z][A-Za-z0-9]*):')) {
        $name = $parameter.Groups['name'].Value
        Assert-Equal $true ($declaredProvisionParameters -contains $name) "Provisioning template must declare '$name'"
    }
    Assert-Equal $true $buildJobSource.Contains("DEVICE_PERFORMANCE_COMMIT_SHA") "Build commit environment binding"
    Assert-Equal $false $buildJobSource.Contains("SHA='`${{ parameters.commitSha }}'") "Build SHA must not be interpolated into Bash"
    Assert-Equal $true $buildJobSource.Contains("/p:UseMonoRuntime=false") "Windows CoreCLR build"
    Assert-Equal $true $buildJobSource.Contains("dotnet publish") "Windows device-test publish"
    Assert-Equal $true $buildJobSource.Contains("/p:_MauiDeviceTestUnpackaged=true") "Windows unpackaged build"
    Assert-Equal $true $buildJobSource.Contains("/p:SelfContained=true") "Windows self-contained build"

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
                -AzdoBuildId 100 `
                -AzdoBuildUrl https://build/100 `
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
