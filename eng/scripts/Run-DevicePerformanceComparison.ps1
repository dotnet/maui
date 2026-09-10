#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs base/head MAUI device-performance apps in ABBA order on one device.
#>

param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("ios", "maccatalyst", "android")]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [string]$BaseApp,

    [Parameter(Mandatory = $true)]
    [string]$HeadApp,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$ExpectedScenario,

    [Parameter(Mandatory = $false)]
    [ValidateNotNullOrEmpty()]
    [string]$Repository = "dotnet/maui",

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $false)]
    [ValidatePattern('\A(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,37}[A-Za-z0-9])?(?:\[bot\])?)?\z')]
    [string]$PullRequestAuthor = "",

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$HarnessSha,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$AzdoBuildId,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$AzdoBuildUrl,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$BaseRuntimeVariant,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$HeadRuntimeVariant,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$BaseSdkVersion,

    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$HeadSdkVersion,

    [Parameter(Mandatory = $false)]
    [ValidateSet(2)]
    [int]$ExpectedVariantRuns = 2,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory,

    [Parameter(Mandatory = $false)]
    [string]$MacCatalystResultFileRoot,

    [Parameter(Mandatory = $false)]
    [string]$DeviceId,

    [Parameter(Mandatory = $false)]
    [string]$AndroidPackageName = "com.microsoft.maui.controls.devicetests",

    [Parameter(Mandatory = $false)]
    [string]$AndroidInstrumentation = "com.microsoft.maui.controls.devicetests.TestInstrumentation",

    [Parameter(Mandatory = $false)]
    [string]$Timeout = "01:15:00",

    [Parameter(Mandatory = $false)]
    [ValidateSet("dotnet", "global", "helix")]
    [string]$XHarnessMode = "dotnet",

    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$scriptDirectory = $PSScriptRoot
$parser = Join-Path $scriptDirectory "Parse-DevicePerformanceResults.ps1"
$comparator = Join-Path $scriptDirectory "Compare-DevicePerformanceResults.ps1"
$resultFileRunId = "maui-perf-$PID-$([Guid]::NewGuid().ToString("N"))"
$effectiveTestFilter = switch ($ExpectedScenario) {
    "carouselview-swipe-disabled" { "Category=PerformanceCarouselViewSwipe" }
    "collectionview-keepitemsinview-update" { "Category=PerformanceCollectionViewItemsUpdate" }
    "collectionview-grouped-scrollto-makevisible" { "Category=PerformanceCollectionViewScroll" }
    "handler-property-update-batch" { "Category=PerformanceHandlerPropertyUpdate" }
    default { throw "No trusted test filter is registered for scenario '$ExpectedScenario'." }
}

if ($XHarnessMode -eq "helix") {
    foreach ($name in @("XHARNESS_CLI_PATH", "HELIX_CORRELATION_ID", "HELIX_WORKITEM_FRIENDLYNAME")) {
        if ([string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($name))) {
            throw "$name is required for Helix execution."
        }
    }
}

function Get-EnvironmentValue([string]$name, [string]$defaultValue) {
    $value = [Environment]::GetEnvironmentVariable($name)
    return $(if ([string]::IsNullOrWhiteSpace($value)) { $defaultValue } else { $value })
}

function Assert-AppExists([string]$path, [string]$name) {
    if (-not (Test-Path $path))
    {
        throw "$name app does not exist: $path"
    }
}

function Get-XHarnessCommand([string]$variant, [string]$app, [string]$commitSha, [string]$runDirectory) {
    $arguments = New-Object System.Collections.Generic.List[string]
    if ($XHarnessMode -eq "dotnet")
    {
        $executable = "dotnet"
        $arguments.Add("xharness")
    }
    elseif ($XHarnessMode -eq "helix")
    {
        if ([string]::IsNullOrWhiteSpace($env:XHARNESS_CLI_PATH))
        {
            throw "XHARNESS_CLI_PATH is required for Helix execution."
        }

        $executable = "dotnet"
        $arguments.Add("exec")
        $arguments.Add($env:XHARNESS_CLI_PATH)
    }
    else
    {
        $executable = "xharness"
    }

    if ($Platform -eq "android")
    {
        $arguments.Add("android")
        $arguments.Add("test")
        $arguments.Add("--app")
        $arguments.Add($app)
        $arguments.Add("--package-name")
        $arguments.Add($AndroidPackageName)
        $arguments.Add("--instrumentation")
        $arguments.Add($AndroidInstrumentation)
        if ($DeviceId)
        {
            $arguments.Add("--device-id")
            $arguments.Add($DeviceId)
        }
        $arguments.Add("--output-directory")
        $arguments.Add($runDirectory)
        $arguments.Add("--timeout")
        $arguments.Add($Timeout)
        $arguments.Add("-v")
        $arguments.Add("--arg")
        $arguments.Add("TestFilter=$effectiveTestFilter")
        $arguments.Add("--arg")
        $arguments.Add("MAUI_PERF_VARIANT=$variant")
        $arguments.Add("--arg")
        $arguments.Add("MAUI_PERF_COMMIT_SHA=$commitSha")
    }
    else
    {
        $arguments.Add("apple")
        $arguments.Add("test")
        $arguments.Add("--app")
        $arguments.Add($app)
        $arguments.Add("--target")
        $arguments.Add($(if ($Platform -eq "ios") { "ios-simulator-64" } else { "maccatalyst" }))
        if ($DeviceId)
        {
            $arguments.Add("--device")
            $arguments.Add($DeviceId)
        }
        $arguments.Add("--output-directory")
        $arguments.Add($runDirectory)
        $arguments.Add("--timeout")
        $arguments.Add($Timeout)
        $arguments.Add("--launch-timeout")
        $arguments.Add("00:10:00")
        $arguments.Add("-v")
        $arguments.Add("--set-env=TestFilter=$effectiveTestFilter")
        $arguments.Add("--set-env=MAUI_PERF_VARIANT=$variant")
        $arguments.Add("--set-env=MAUI_PERF_COMMIT_SHA=$commitSha")
    }

    $runtimeVariant = if ($variant -eq "base") { $BaseRuntimeVariant } else { $HeadRuntimeVariant }
    $sdkVersion = if ($variant -eq "base") { $BaseSdkVersion } else { $HeadSdkVersion }
    $provenance = [ordered]@{
        MAUI_PERF_REPOSITORY = $Repository
        MAUI_PERF_PR_NUMBER = $PullRequestNumber
        MAUI_PERF_HARNESS_SHA = $HarnessSha
        MAUI_PERF_RUN_ORDINAL = $script:currentRunOrdinal
        MAUI_PERF_EXPECTED_VARIANT_RUNS = $ExpectedVariantRuns
        MAUI_PERF_AZDO_BUILD_ID = $AzdoBuildId
        MAUI_PERF_AZDO_BUILD_URL = $AzdoBuildUrl
        MAUI_PERF_HELIX_JOB_ID = Get-EnvironmentValue "HELIX_CORRELATION_ID" "local"
        MAUI_PERF_HELIX_WORK_ITEM = Get-EnvironmentValue "HELIX_WORKITEM_FRIENDLYNAME" "local"
        MAUI_PERF_RUNTIME_VARIANT = $runtimeVariant
        MAUI_PERF_SDK_VERSION = $sdkVersion
    }
    $resultFile = $null
    if ($Platform -eq "maccatalyst") {
        $resultFile = Join-Path `
            $MacCatalystResultFileRoot `
            "$resultFileRunId-$variant-run$($script:currentRunOrdinal).log"
        $provenance.MAUI_PERF_RESULT_FILE = $resultFile
    }

    foreach ($entry in $provenance.GetEnumerator()) {
        if ($Platform -eq "android") {
            $arguments.Add("--arg")
            $arguments.Add("$($entry.Key)=$($entry.Value)")
        } else {
            $arguments.Add("--set-env=$($entry.Key)=$($entry.Value)")
        }
    }

    return [PSCustomObject]@{
        Executable = $executable
        Arguments = @($arguments)
        ResultFile = $resultFile
    }
}

Assert-AppExists $BaseApp "Base"
Assert-AppExists $HeadApp "Head"

if (-not (Test-Path $OutputDirectory))
{
    New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
}

if ($Platform -eq "maccatalyst")
{
    if ([string]::IsNullOrWhiteSpace($MacCatalystResultFileRoot))
    {
        $MacCatalystResultFileRoot = if ($IsMacOS) { "/private/tmp" } else { [IO.Path]::GetTempPath() }
    }

    New-Item -ItemType Directory -Force -Path $MacCatalystResultFileRoot | Out-Null
    $MacCatalystResultFileRoot = (Resolve-Path $MacCatalystResultFileRoot).Path
}
elseif (-not [string]::IsNullOrWhiteSpace($MacCatalystResultFileRoot))
{
    throw "MacCatalystResultFileRoot can only be used with Platform=maccatalyst."
}

$runs = @(
    [PSCustomObject]@{ Variant = "base"; App = $BaseApp; CommitSha = $BaseCommitSha; Number = 1 },
    [PSCustomObject]@{ Variant = "head"; App = $HeadApp; CommitSha = $HeadCommitSha; Number = 1 },
    [PSCustomObject]@{ Variant = "head"; App = $HeadApp; CommitSha = $HeadCommitSha; Number = 2 },
    [PSCustomObject]@{ Variant = "base"; App = $BaseApp; CommitSha = $BaseCommitSha; Number = 2 }
)

$plan = New-Object System.Collections.Generic.List[object]
foreach ($run in $runs)
{
    $runDirectory = Join-Path $OutputDirectory "$($run.Variant)-run$($run.Number)"
    $script:currentRunOrdinal = $run.Number
    $command = Get-XHarnessCommand $run.Variant $run.App $run.CommitSha $runDirectory
    $plan.Add([PSCustomObject]@{
        Variant = $run.Variant
        Number = $run.Number
        CommitSha = $run.CommitSha
        RunDirectory = $runDirectory
        Executable = $command.Executable
        Arguments = @($command.Arguments)
        ResultFile = $command.ResultFile
    })
}

ConvertTo-Json -InputObject @($plan | ForEach-Object { $_ }) -Depth 8 |
    Set-Content -Path (Join-Path $OutputDirectory "run-plan.json") -Encoding UTF8

if ($DryRun)
{
    foreach ($run in $plan)
    {
        Write-Host "$($run.Executable) $($run.Arguments -join ' ')"
    }
    exit 0
}

foreach ($run in $plan)
{
    New-Item -ItemType Directory -Force -Path $run.RunDirectory | Out-Null
    $consoleLog = Join-Path $run.RunDirectory "xharness-console.log"
    if (-not [string]::IsNullOrWhiteSpace([string]$run.ResultFile))
    {
        Remove-Item $run.ResultFile -Force -ErrorAction SilentlyContinue
    }

    Write-Host "Running $($run.Variant) iteration $($run.Number) on $Platform..."
    $arguments = @($run.Arguments)
    & $run.Executable @arguments *> $consoleLog
    if ($LASTEXITCODE -ne 0)
    {
        throw "XHarness failed for $($run.Variant) iteration $($run.Number) with exit code $LASTEXITCODE."
    }

    if (-not [string]::IsNullOrWhiteSpace([string]$run.ResultFile))
    {
        if (-not (Test-Path $run.ResultFile))
        {
            throw "Performance result file was not created: $($run.ResultFile)"
        }

        Move-Item `
            $run.ResultFile `
            (Join-Path $run.RunDirectory "maui-perf-result.log") `
            -Force
    }
}

$resultsPath = Join-Path $OutputDirectory "results.json"
$summaryJson = Join-Path $OutputDirectory "comparison-summary.json"
$summaryMarkdown = Join-Path $OutputDirectory "comparison-summary.md"

& $parser -InputPath $OutputDirectory -OutputPath $resultsPath
if ($LASTEXITCODE -ne 0)
{
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
    -ExpectedPlatform $Platform `
    -ExpectedScenario $ExpectedScenario `
    -ExpectedVariantRuns $ExpectedVariantRuns
exit $LASTEXITCODE
