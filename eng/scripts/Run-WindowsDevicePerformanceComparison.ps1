#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$BaseApp,

    [Parameter(Mandatory = $true)]
    [string]$HeadApp,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [ValidateSet("carouselview-wheel-snap-windows", "handler-property-update-batch")]
    [string]$ExpectedScenario,

    [Parameter(Mandatory = $true)]
    [string]$Repository,

    [Parameter(Mandatory = $true)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $false)]
    [ValidatePattern('\A(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?(?:\[bot\])?)?\z')]
    [string]$PullRequestAuthor = "",

    [Parameter(Mandatory = $true)]
    [string]$HarnessSha,

    [Parameter(Mandatory = $true)]
    [string]$AzdoBuildId,

    [Parameter(Mandatory = $true)]
    [string]$AzdoBuildUrl,

    [Parameter(Mandatory = $true)]
    [string]$BaseRuntimeVariant,

    [Parameter(Mandatory = $true)]
    [string]$HeadRuntimeVariant,

    [Parameter(Mandatory = $true)]
    [string]$BaseSdkVersion,

    [Parameter(Mandatory = $true)]
    [string]$HeadSdkVersion,

    [Parameter(Mandatory = $false)]
    [ValidateSet(2)]
    [int]$ExpectedVariantRuns = 2,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 600)]
    [int]$DiscoveryTimeoutSeconds = 120,

    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 10800)]
    [int]$RunTimeoutSeconds = 4500,

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$category = switch ($ExpectedScenario) {
    "carouselview-wheel-snap-windows" { "PerformanceCarouselViewWheelSnap" }
    "handler-property-update-batch" { "PerformanceHandlerPropertyUpdate" }
}
$parser = Join-Path $PSScriptRoot "Parse-DevicePerformanceResults.ps1"
$comparator = Join-Path $PSScriptRoot "Compare-DevicePerformanceResults.ps1"

function Get-EnvironmentValue([string]$name, [string]$defaultValue) {
    $value = [Environment]::GetEnvironmentVariable($name)
    return $(if ([string]::IsNullOrWhiteSpace($value)) { $defaultValue } else { $value })
}

function Wait-ForSuccessfulProcess(
    [Diagnostics.Process]$process,
    [int]$timeoutSeconds,
    [string]$description
) {
    if (-not $process.WaitForExit($timeoutSeconds * 1000)) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        [void]$process.WaitForExit(30000)
        throw "$description timed out after $timeoutSeconds seconds."
    }

    if ($process.ExitCode -eq -1073741189) {
        throw "Windows App SDK bootstrap failed during $description."
    }
    if ($process.ExitCode -ne 0) {
        throw "$description exited with code $($process.ExitCode)."
    }
}

function Get-CategoryIndex([string]$app, [string]$variant) {
    $discoveryDirectory = Join-Path $OutputDirectory "$variant-discovery"
    New-Item -ItemType Directory -Force -Path $discoveryDirectory | Out-Null
    $resultsFile = Join-Path $discoveryDirectory "TestResults.xml"
    $categoryFile = Join-Path $discoveryDirectory "devicetestcategories.txt"
    Remove-Item -LiteralPath $resultsFile, $categoryFile -Force -ErrorAction SilentlyContinue

    $savedInclude = $env:MAUI_INCLUDE_PERFORMANCE_TESTS
    $env:MAUI_INCLUDE_PERFORMANCE_TESTS = "1"
    try {
        $process = Start-Process `
            -FilePath $app `
            -ArgumentList "`"$resultsFile`"", "-1" `
            -WorkingDirectory (Split-Path -Parent $app) `
            -PassThru
        Wait-ForSuccessfulProcess $process $DiscoveryTimeoutSeconds "$variant category discovery"
        if (-not (Test-Path $categoryFile)) {
            throw "Windows performance category discovery failed for $variant."
        }
    }
    finally {
        $env:MAUI_INCLUDE_PERFORMANCE_TESTS = $savedInclude
    }

    $categories = @(Get-Content $categoryFile)
    $index = [Array]::IndexOf($categories, $category)
    if ($index -lt 0) {
        throw "Windows performance category '$category' was not discovered for $variant."
    }

    return $index
}

foreach ($app in @($BaseApp, $HeadApp)) {
    if (-not (Test-Path $app)) {
        throw "Windows performance app does not exist: $app"
    }
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$baseCategory = if ($DryRun) {
    [PSCustomObject]@{ Index = 0; File = $null }
} else {
    [PSCustomObject]@{
        Index = Get-CategoryIndex $BaseApp "base"
        File = Join-Path $OutputDirectory "base-discovery\devicetestcategories.txt"
    }
}
$headCategory = if ($DryRun) {
    [PSCustomObject]@{ Index = 0; File = $null }
} else {
    [PSCustomObject]@{
        Index = Get-CategoryIndex $HeadApp "head"
        File = Join-Path $OutputDirectory "head-discovery\devicetestcategories.txt"
    }
}
$runs = @(
    [PSCustomObject]@{ Variant = "base"; App = $BaseApp; CommitSha = $BaseCommitSha; Number = 1; CategoryIndex = $baseCategory.Index; CategoryFile = $baseCategory.File },
    [PSCustomObject]@{ Variant = "head"; App = $HeadApp; CommitSha = $HeadCommitSha; Number = 1; CategoryIndex = $headCategory.Index; CategoryFile = $headCategory.File },
    [PSCustomObject]@{ Variant = "head"; App = $HeadApp; CommitSha = $HeadCommitSha; Number = 2; CategoryIndex = $headCategory.Index; CategoryFile = $headCategory.File },
    [PSCustomObject]@{ Variant = "base"; App = $BaseApp; CommitSha = $BaseCommitSha; Number = 2; CategoryIndex = $baseCategory.Index; CategoryFile = $baseCategory.File }
)

$plan = @(
    foreach ($run in $runs) {
        [PSCustomObject]@{
            variant = $run.Variant
            runOrdinal = $run.Number
            commitSha = $run.CommitSha
            app = $run.App
            category = $category
            categoryIndex = $run.CategoryIndex
        }
    }
)
$plan | ConvertTo-Json -Depth 6 |
    Set-Content (Join-Path $OutputDirectory "run-plan.json") -Encoding UTF8

if ($DryRun) {
    $plan | Format-Table
    exit 0
}

foreach ($run in $runs) {
    $runDirectory = Join-Path $OutputDirectory "$($run.Variant)-run$($run.Number)"
    New-Item -ItemType Directory -Force -Path $runDirectory | Out-Null
    $testResults = Join-Path $runDirectory "TestResults.xml"
    $resultFile = Join-Path $runDirectory "maui-perf-result.log"
    $consoleLog = Join-Path $runDirectory "windows-console.log"
    $errorLog = Join-Path $runDirectory "windows-error.log"
    Remove-Item -LiteralPath $testResults, $resultFile, $consoleLog, $errorLog -Force -ErrorAction SilentlyContinue
    Copy-Item $run.CategoryFile (Join-Path $runDirectory "devicetestcategories.txt") -Force
    $runtimeVariant = if ($run.Variant -eq "base") { $BaseRuntimeVariant } else { $HeadRuntimeVariant }
    $sdkVersion = if ($run.Variant -eq "base") { $BaseSdkVersion } else { $HeadSdkVersion }
    $environment = [ordered]@{
        MAUI_INCLUDE_PERFORMANCE_TESTS = "1"
        MAUI_PERF_RESULT_FILE = $resultFile
        MAUI_PERF_VARIANT = $run.Variant
        MAUI_PERF_COMMIT_SHA = $run.CommitSha
        MAUI_PERF_REPOSITORY = $Repository
        MAUI_PERF_PR_NUMBER = $PullRequestNumber
        MAUI_PERF_HARNESS_SHA = $HarnessSha
        MAUI_PERF_RUN_ORDINAL = $run.Number
        MAUI_PERF_EXPECTED_VARIANT_RUNS = $ExpectedVariantRuns
        MAUI_PERF_AZDO_BUILD_ID = $AzdoBuildId
        MAUI_PERF_AZDO_BUILD_URL = $AzdoBuildUrl
        MAUI_PERF_HELIX_JOB_ID = Get-EnvironmentValue "HELIX_CORRELATION_ID" "local"
        MAUI_PERF_HELIX_WORK_ITEM = Get-EnvironmentValue "HELIX_WORKITEM_FRIENDLYNAME" "local"
        MAUI_PERF_RUNTIME_VARIANT = $runtimeVariant
        MAUI_PERF_SDK_VERSION = $sdkVersion
    }
    $savedEnvironment = @{}
    foreach ($entry in $environment.GetEnumerator()) {
        $savedEnvironment[$entry.Key] = [Environment]::GetEnvironmentVariable($entry.Key)
        [Environment]::SetEnvironmentVariable($entry.Key, "$($entry.Value)")
    }

    try {
        $process = Start-Process `
            -FilePath $run.App `
            -ArgumentList "`"$testResults`"", "$($run.CategoryIndex)" `
            -WorkingDirectory (Split-Path -Parent $run.App) `
            -RedirectStandardOutput $consoleLog `
            -RedirectStandardError $errorLog `
            -PassThru
        Wait-ForSuccessfulProcess $process $RunTimeoutSeconds "$($run.Variant) run $($run.Number)"
        if (-not (Test-Path $resultFile)) {
            throw "Windows performance result was not created: $resultFile"
        }
    }
    finally {
        foreach ($entry in $savedEnvironment.GetEnumerator()) {
            [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
        }
    }
}

$resultsPath = Join-Path $OutputDirectory "results.json"
$summaryJson = Join-Path $OutputDirectory "comparison-summary.json"
$summaryMarkdown = Join-Path $OutputDirectory "comparison-summary.md"
& $parser -InputPath $OutputDirectory -OutputPath $resultsPath
if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

& $comparator `
    -ResultsPath $resultsPath `
    -JsonOut $summaryJson `
    -MarkdownOut $summaryMarkdown `
    -ExpectedRepository $Repository `
    -ExpectedPullRequestNumber $PullRequestNumber `
    -PullRequestAuthor $PullRequestAuthor `
    -ExpectedBaseCommitSha $BaseCommitSha `
    -ExpectedHeadCommitSha $HeadCommitSha `
    -ExpectedHarnessSha $HarnessSha `
    -ExpectedPlatform windows `
    -ExpectedScenario $ExpectedScenario `
    -ExpectedVariantRuns $ExpectedVariantRuns
exit $LASTEXITCODE
