#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$PolicyPath,

    [Parameter(Mandatory = $false)]
    [string]$SummaryPath,

    [Parameter(Mandatory = $false)]
    [string]$DeviceValidationPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

function Get-PropertyValue($object, [string]$name) {
    if ($null -eq $object) {
        return $null
    }

    $property = $object.PSObject.Properties[$name]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

foreach ($path in @($SelectionPath, $PolicyPath)) {
    if (-not (Test-Path $path)) {
        throw "Required decision input does not exist: $path"
    }
}

$selection = Get-Content $SelectionPath -Raw | ConvertFrom-Json
$policy = Get-Content $PolicyPath -Raw | ConvertFrom-Json
$summary = if ($SummaryPath -and (Test-Path $SummaryPath)) {
    Get-Content $SummaryPath -Raw | ConvertFrom-Json
}
else {
    $null
}
$deviceValidation = if ($DeviceValidationPath -and (Test-Path $DeviceValidationPath)) {
    Get-Content $DeviceValidationPath -Raw | ConvertFrom-Json
}
else {
    $null
}

$coverage = $selection.coverage
$deviceScenarios = @($selection.deviceScenarios | Where-Object { $null -ne $_ })
$unsupportedDevicePath = @(
    $deviceScenarios | Where-Object { $_.automationStatus -eq "required-not-yet-automated" }
).Count -gt 0
$supportedMeasurementPath = @(
    $deviceScenarios | Where-Object { $_.automationStatus -eq "manual-device-ci-ready" }
).Count -gt 0

$summaryComplete = $null -ne $summary `
    -and [bool](Get-PropertyValue $summary "coverageComplete") `
    -and [bool](Get-PropertyValue $summary "executionComplete") `
    -and [bool](Get-PropertyValue $summary "benchmarkDataComplete")
$deviceEvidenceComplete = $null -ne $deviceValidation `
    -and [bool](Get-PropertyValue $deviceValidation "sealed") `
    -and [bool](Get-PropertyValue $deviceValidation "deviceEvidenceComplete") `
    -and [bool](Get-PropertyValue $deviceValidation "correctnessPassed") `
    -and [bool](Get-PropertyValue $deviceValidation "allAffectedPlatformsCovered") `
    -and -not $unsupportedDevicePath

$productFileCount = Get-PropertyValue $coverage "productFileCount"
if ($null -ne $productFileCount) {
    $managedCount = [int](Get-PropertyValue $coverage "managedMeasuredFileCount")
    $deviceCount = [int](Get-PropertyValue $coverage "deviceRequiredFileCount")
    $sampledCount = [int](Get-PropertyValue $coverage "managedSampledFileCount")
    $staticCount = [int](Get-PropertyValue $coverage "staticOnlyFileCount")
    $classificationComplete = [int]$productFileCount -gt 0 `
        -and ($managedCount + $deviceCount) -eq [int]$productFileCount `
        -and $sampledCount -eq 0 `
        -and $staticCount -eq 0 `
        -and -not [bool](Get-PropertyValue $coverage "benchmarkInputsChanged")
    $wholePrEvidenceComplete = $classificationComplete `
        -and ($managedCount -eq 0 -or $summaryComplete) `
        -and ($deviceCount -eq 0 -or $deviceEvidenceComplete)
}
else {
    $managedCount = $null
    $deviceCount = $null
    $wholePrEvidenceComplete =
        [bool](Get-PropertyValue $coverage "canClaimWholePrClean") -and $summaryComplete
}

$directDeviceScenarioIds = @(
    $deviceScenarios |
        Where-Object { [string]$_.coverageMode -ne "sampled" } |
        ForEach-Object { [string]$_.resultScenario } |
        Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
)
$deviceAdvisory = $deviceEvidenceComplete -and @(
    $deviceValidation.acceptedMeasurements |
        Where-Object {
            $_.verdict -in @("time-regression-advisory", "time-improvement-advisory") -and
            $_.resultScenario -in $directDeviceScenarioIds
        }
).Count -gt 0
$directManagedFilters = @(
    $selection.suites | Where-Object {
        @($_.directlyCoveredFiles | Where-Object { $null -ne $_ }).Count -gt 0
    } | ForEach-Object { @($_.filters) }
)
$directManagedTimingNames = @(
    @($summary.timeRegressions | ForEach-Object { [string]$_.name }) +
    @($summary.improvements | Where-Object { $_.flag -eq "time-improvement" } | ForEach-Object { [string]$_.name })
)
$hasDirectManagedTimingSignal = $null -ne $summary -and @(
    $directManagedTimingNames | Where-Object {
        $benchmarkName = [string]$_
        @($directManagedFilters | Where-Object { [string]$_ -and $benchmarkName -like $_ }).Count -gt 0
    }
).Count -gt 0
$managedTimingAdvisory = $null -ne $summary `
    -and [string]$summary.verdict -in @("time-regression-advisory", "time-improvement-advisory") `
    -and $hasDirectManagedTimingSignal
$advisoryOnly = $managedTimingAdvisory `
    -or $deviceAdvisory
$managedEvidenceClean = if ($null -ne $managedCount) {
    $managedCount -eq 0 -or ($summaryComplete -and [bool](Get-PropertyValue $summary "canClaimClean"))
}
else {
    $summaryComplete -and [bool](Get-PropertyValue $summary "canClaimClean")
}
$deviceEvidenceClean = if ($null -ne $deviceCount) {
    $deviceCount -eq 0 -or ($deviceEvidenceComplete -and -not $deviceAdvisory)
}
else {
    $deviceScenarios.Count -eq 0 -or ($deviceEvidenceComplete -and -not $deviceAdvisory)
}
$confirmedAllocationRegression = $null -ne $summary -and @(
    $summary.allocRegressions | Where-Object { $_.confirmed -eq $true }
).Count -gt 0
$hasMeasuredImprovement = $wholePrEvidenceComplete -and (
    ($null -ne $summary -and [string]$summary.verdict -eq "improvement") -or
    ($deviceEvidenceComplete -and @(
        $deviceValidation.acceptedMeasurements |
            Where-Object { $_.verdict -eq "improvement" }
    ).Count -gt 0)
)
$sampledFiles = @($selection.sampledProductFiles | Where-Object { $null -ne $_ })
$staticFiles = @($selection.staticOnlyProductFiles | Where-Object { $null -ne $_ })
$hasCoverageGap = -not $wholePrEvidenceComplete `
    -or [bool](Get-PropertyValue $coverage "benchmarkInputsChanged") `
    -or $sampledFiles.Count -gt 0 `
    -or $staticFiles.Count -gt 0 `
    -or ($deviceScenarios.Count -gt 0 -and -not $deviceEvidenceComplete)
$executionIncomplete = $null -ne $summary -and (
    -not [bool](Get-PropertyValue $summary "executionComplete") -or
    -not [bool](Get-PropertyValue $summary "benchmarkDataComplete")
)
$pureStaticOnly = $null -eq $summary `
    -and $deviceScenarios.Count -eq 0 `
    -and @($selection.suites | Where-Object { $null -ne $_ }).Count -eq 0 `
    -and $staticFiles.Count -gt 0 `
    -and $sampledFiles.Count -eq 0
$selectedSuiteCount = @($selection.suites | Where-Object { $null -ne $_ }).Count
$sampledEvidenceComplete = $hasCoverageGap `
    -and ($selectedSuiteCount -eq 0 -or (
        $summaryComplete -and
        @($summary.allocRegressions | Where-Object { $null -ne $_ }).Count -eq 0
    )) `
    -and ($deviceScenarios.Count -eq 0 -or ($deviceEvidenceComplete -and -not $deviceAdvisory))

if ($confirmedAllocationRegression) {
    $verdictClass = "blocker"
    $confidence = "high"
    $nextAction = "optimize_before_merge"
}
elseif ($advisoryOnly) {
    $verdictClass = "advisory"
    $confidence = "medium"
    $nextAction = "needs_human_discussion"
}
elseif ($hasMeasuredImprovement) {
    $verdictClass = "improvement"
    $confidence = "high"
    $nextAction = "no_concerns"
}
elseif ($wholePrEvidenceComplete -and $managedEvidenceClean -and $deviceEvidenceClean) {
    $verdictClass = "clean"
    $confidence = "high"
    $nextAction = "no_concerns"
}
elseif ($executionIncomplete) {
    $verdictClass = "inconclusive"
    $confidence = "low"
    $nextAction = if ($supportedMeasurementPath) { "run_more_measurements" } else { "needs_human_discussion" }
}
elseif ($deviceScenarios.Count -gt 0 -and -not $deviceEvidenceComplete) {
    $verdictClass = "device-required"
    $confidence = "low"
    $nextAction = if ($supportedMeasurementPath) { "run_more_measurements" } else { "needs_human_discussion" }
}
elseif ($pureStaticOnly) {
    $verdictClass = "no-blocker-incomplete"
    $confidence = "low"
    $nextAction = "no_perf_action_needed"
}
elseif ($sampledEvidenceComplete) {
    $verdictClass = "no-blocker-incomplete"
    $confidence = "low"
    $nextAction = "no_perf_action_needed"
}
elseif ($hasCoverageGap) {
    $verdictClass = "no-blocker-incomplete"
    $confidence = "low"
    $nextAction = if ($supportedMeasurementPath) { "run_more_measurements" } else { "needs_human_discussion" }
}
else {
    $verdictClass = "inconclusive"
    $confidence = "low"
    $nextAction = [string]$policy.decisionRules.unsupportedMissingEvidenceAction
}

$decision = [PSCustomObject]@{
    schemaVersion = 1
    verdictClass = $verdictClass
    confidence = $confidence
    nextAction = $nextAction
    issueDisposition = "human-only"
    allowStaticErrorEscalation = $true
    allowStaticWarningEscalation = $true
}

$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}

$decision | ConvertTo-Json -Depth 6 |
    Set-Content -Path $OutputPath -Encoding UTF8
Write-Host "Resolved performance decision: $verdictClass / $confidence / $nextAction"
