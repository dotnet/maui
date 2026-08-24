#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$skillRoot = Split-Path -Parent $PSScriptRoot
$resolver = Join-Path $skillRoot "scripts\Resolve-PerfDecision.ps1"
$policy = Join-Path $skillRoot "references\recommendation-policy.json"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-perf-decision-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Write-Json([string]$path, $value) {
    $value | ConvertTo-Json -Depth 10 | Set-Content $path -Encoding UTF8
}

function Resolve-Case([string]$name, $selection, $summary = $null, $device = $null) {
    $root = Join-Path $testRoot $name
    New-Item -ItemType Directory -Force -Path $root | Out-Null
    $selectionPath = Join-Path $root "selection.json"
    $outputPath = Join-Path $root "decision.json"
    Write-Json $selectionPath $selection
    $arguments = @{
        SelectionPath = $selectionPath
        PolicyPath = $policy
        OutputPath = $outputPath
    }
    if ($null -ne $summary) {
        $summaryPath = Join-Path $root "summary.json"
        Write-Json $summaryPath $summary
        $arguments.SummaryPath = $summaryPath
    }
    if ($null -ne $device) {
        $devicePath = Join-Path $root "device.json"
        Write-Json $devicePath $device
        $arguments.DeviceValidationPath = $devicePath
    }

    & $resolver @arguments
    $exitCode = if ($null -eq $LASTEXITCODE) { 0 } else { $LASTEXITCODE }
    Assert-Equal 0 $exitCode "Resolver case '$name'"
    return Get-Content $outputPath -Raw | ConvertFrom-Json
}

function New-Selection(
    [int]$managed,
    [int]$device,
    [int]$static,
    [string]$automationStatus = ""
) {
    return [PSCustomObject]@{
        coverage = [PSCustomObject]@{
            productFileCount = $managed + $device + $static
            managedMeasuredFileCount = $managed
            managedSampledFileCount = 0
            deviceRequiredFileCount = $device
            staticOnlyFileCount = $static
            benchmarkInputsChanged = $false
        }
        deviceScenarios = if ($device -gt 0) {
            @([PSCustomObject]@{ automationStatus = $automationStatus })
        } else {
            @()
        }
        sampledProductFiles = @()
        staticOnlyProductFiles = if ($static -gt 0) { @("file.cs") } else { @() }
    }
}

function New-Summary([string]$verdict, [bool]$clean, [bool]$regression = $false) {
    return [PSCustomObject]@{
        verdict = $verdict
        coverageComplete = $true
        executionComplete = $true
        benchmarkDataComplete = $true
        canClaimClean = $clean
        timeRegressions = if ($verdict -eq "time-regression-advisory") {
            @([PSCustomObject]@{ name = "Microsoft.Maui.Benchmarks.Sample.Run" })
        } else {
            @()
        }
        improvements = if ($verdict -eq "time-improvement-advisory") {
            @([PSCustomObject]@{
                name = "Microsoft.Maui.Benchmarks.Sample.Run"
                flag = "time-improvement"
            })
        } else {
            @()
        }
        allocRegressions = if ($regression) {
            @([PSCustomObject]@{ confirmed = $true })
        } else {
            @()
        }
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
try {
    $clean = Resolve-Case "clean" (New-Selection 1 0 0) (New-Summary "neutral" $true)
    Assert-Equal "clean" $clean.verdictClass "Clean verdict"
    Assert-Equal "high" $clean.confidence "Clean confidence"
    Assert-Equal "no_concerns" $clean.nextAction "Clean action"

    $regression = Resolve-Case "regression" (New-Selection 1 0 0) (New-Summary "alloc-regression" $false $true)
    Assert-Equal "blocker" $regression.verdictClass "Regression verdict"
    Assert-Equal "optimize_before_merge" $regression.nextAction "Regression action"

    $staticOnly = Resolve-Case "static" (New-Selection 0 0 1)
    Assert-Equal "no-blocker-incomplete" $staticOnly.verdictClass "Static-only verdict"
    Assert-Equal "no_perf_action_needed" $staticOnly.nextAction "Static-only action"
    Assert-Equal $true $staticOnly.allowStaticWarningEscalation "Static warning escalation capability"

    $partialSummary = New-Summary "neutral" $true
    $partialSelection = New-Selection 1 0 0
    $partialSelection.coverage.benchmarkInputsChanged = $true
    $partial = Resolve-Case "partial" $partialSelection $partialSummary
    Assert-Equal "no-blocker-incomplete" $partial.verdictClass "Planned partial coverage verdict"

    $sampledSelection = New-Selection 0 0 1
    $sampledSelection | Add-Member -NotePropertyName suites -NotePropertyValue @([PSCustomObject]@{ project = "Core" })
    $sampled = Resolve-Case "sampled-clean" $sampledSelection (New-Summary "neutral" $true)
    Assert-Equal "no_perf_action_needed" $sampled.nextAction "Clean sampled family action"

    $sampledTiming = Resolve-Case "sampled-timing" $sampledSelection (New-Summary "time-regression-advisory" $false)
    Assert-Equal "no_perf_action_needed" $sampledTiming.nextAction "Sampled timing remains informational"

    $mixedDirectSelection = New-Selection 1 0 1
    $mixedDirectSelection | Add-Member -NotePropertyName suites -NotePropertyValue @(
        [PSCustomObject]@{
            project = "Core"
            directlyCoveredFiles = @("direct.cs")
            filters = @("*Sample*")
        }
    )
    $mixedDirectTiming = Resolve-Case "mixed-direct-timing" $mixedDirectSelection (New-Summary "time-regression-advisory" $false)
    Assert-Equal "advisory" $mixedDirectTiming.verdictClass "Direct timing regression survives unrelated coverage gaps"

    $directTimingImprovement = Resolve-Case "direct-timing-improvement" $mixedDirectSelection (New-Summary "time-improvement-advisory" $true)
    Assert-Equal "advisory" $directTimingImprovement.verdictClass "Direct timing improvement remains advisory"

    $unconfirmedAllocation = New-Summary "inconclusive" $false
    $unconfirmedAllocation.allocRegressions = @([PSCustomObject]@{ confirmed = $false })
    $sampledAllocation = Resolve-Case "sampled-unconfirmed-allocation" $sampledSelection $unconfirmedAllocation
    Assert-Equal "needs_human_discussion" $sampledAllocation.nextAction "Unconfirmed allocation evidence must not be dismissed"

    $deviceReady = Resolve-Case "device-ready" (New-Selection 0 1 0 "manual-device-ci-ready")
    Assert-Equal "device-required" $deviceReady.verdictClass "Device verdict"
    Assert-Equal "run_more_measurements" $deviceReady.nextAction "Device action"

    $deviceUnsupported = Resolve-Case "device-unsupported" (New-Selection 0 1 0 "required-not-yet-automated")
    Assert-Equal "needs_human_discussion" $deviceUnsupported.nextAction "Unsupported device action"

    Write-Host "Performance decision resolver tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
