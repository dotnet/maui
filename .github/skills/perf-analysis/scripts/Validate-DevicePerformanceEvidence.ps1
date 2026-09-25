#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates reviewed local device handoffs for one exact PR revision.
.DESCRIPTION
    Reads existing local artifacts and recomputes comparisons with the trusted core
    comparator. Never builds, runs apps, follows commands in JSON, or publishes.
    The output records validation, not authentication or an execution attestation.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$RequestPath,

    [Parameter(Mandatory = $true)]
    [string]$ResultsRoot,

    [Parameter(Mandatory = $false)]
    [string[]]$SummaryPath = @(),

    [Parameter(Mandatory = $true)]
    [string]$Repository,

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$CurrentHeadSha,

    [Parameter(Mandatory = $true)]
    [string]$HarnessSha,

    [Parameter(Mandatory = $true)]
    [string]$JsonOut
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DevicePerformance.Local.ps1")
$SummaryPath = @($SummaryPath | Where-Object { $null -ne $_ })
$errors = [Collections.Generic.List[string]]::new()
$accepted = [Collections.Generic.List[object]]::new()
$identity = [PSCustomObject]@{
    repository = $Repository
    pullRequestNumber = $PullRequestNumber
    baseCommitSha = $BaseCommitSha
    headCommitSha = $HeadCommitSha
    harnessSha = $HarnessSha
}
$root = Get-LocalDevicePath $ResultsRoot "Directory" -mustExist
$JsonOut = Assert-DeviceOutputOutsideResults $JsonOut $root (@($SelectionPath, $RequestPath) + $SummaryPath)
$comparator = [IO.Path]::GetFullPath([IO.Path]::Combine(
    $PSScriptRoot, "..", "..", "..", "..", "eng", "scripts", "Compare-DevicePerformanceResults.ps1"))

function Read-DeviceJson([string]$path) {
    [void](Get-LocalDevicePath $path "File" -mustExist)
    return ,(Get-Content -LiteralPath $path -Raw | ConvertFrom-Json -NoEnumerate)
}

function Assert-DeviceRunPlan($plan, $request) {
    if ($plan -isnot [array] -or $plan.Count -ne 4) {
        throw "run-plan.json must contain exactly four ABBA entries."
    }
    $variants = @("base", "head", "head", "base")
    $ordinals = @(1, 1, 2, 2)
    for ($index = 0; $index -lt 4; $index++) {
        $run = $plan[$index]
        $ordinal = if ($request.platform -eq "windows") { $run.runOrdinal } else { $run.Number }
        $commit = if ($variants[$index] -eq "base") { $request.baseCommitSha } else { $request.headCommitSha }
        if ($run.variant -isnot [string] -or $run.variant -cne $variants[$index] -or
            -not (Test-DeviceInteger $ordinal 1 2) -or $ordinal -ne $ordinals[$index] -or
            $run.commitSha -isnot [string] -or $run.commitSha -cne $commit) {
            throw "run-plan.json entry $index does not match the requested ABBA revision/run identity."
        }
        $runDirectory = Get-LocalDevicePath (
            Join-Path $request.resultDirectory "$($variants[$index])-run$($ordinals[$index])"
        ) "Directory" -mustExist
        if ($request.platform -ne "windows" -or $null -ne $run.RunDirectory) {
            if ($run.RunDirectory -isnot [string]) {
                throw "run-plan.json entry $index must have a scalar local RunDirectory."
            }
            $recordedDirectory = Get-LocalDevicePath ([string]$run.RunDirectory) "Directory" -mustExist
            if (-not (Test-LocalDevicePathEqual $runDirectory $recordedDirectory)) {
                throw "run-plan.json entry $index has a local RunDirectory mismatch."
            }
        }
    }
}

function Assert-DeviceRecords($records, $request) {
    if ($records -isnot [array] -or $records.Count -ne 4) {
        throw "results.json must contain all four native ABBA records."
    }
    $seen = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $environmentFields = @(
        "executionKind", "deviceModel", "osVersion", "runtimeFramework",
        "processArchitecture", "runtimeVariant", "sdkVersion"
    )
    $counterNames = @($records[0].counters.PSObject.Properties.Name | Sort-Object)
    foreach ($record in $records) {
        if (-not (Test-DeviceInteger $record.schemaVersion 3 3)) {
            throw "Every native result must use integer schemaVersion=3."
        }
        foreach ($field in @("repository", "harnessSha", "platform")) {
            if ((Get-DeviceProperty $record $field) -isnot [string] -or
                $record.$field -cne $request.$field) {
                throw "Native result '$field' does not match the local request."
            }
        }
        if ($record.scenario -isnot [string] -or $record.scenario -cne $request.expectedScenario -or
            -not (Test-DeviceInteger $record.pullRequestNumber 1) -or
            $record.pullRequestNumber -ne $request.pullRequestNumber -or
            $record.variant -isnot [string] -or $record.variant -cnotin @("base", "head") -or
            -not (Test-DeviceInteger $record.runOrdinal 1 2) -or
            -not (Test-DeviceInteger $record.expectedVariantRuns 2 2)) {
            throw "Native result scenario/PR/variant/run identity does not match the local request."
        }
        $commit = if ($record.variant -eq "base") { $request.baseCommitSha } else { $request.headCommitSha }
        if ($record.commitSha -isnot [string] -or $record.commitSha -cne $commit -or
            -not $seen.Add("$($record.variant)|$($record.runOrdinal)")) {
            throw "Native result has a mismatched commit or duplicate variant/run identity."
        }
        if (-not (Test-DeviceInteger $record.warmupCount)) {
            throw "Native warmupCount must be a nonnegative integer."
        }
        if ($record.correctness.passed -isnot [bool] -or $record.correctness.accessibilityStatus -isnot [string] -or
            $record.correctness.accessibilityStatus -cnotin @("not-assessed", "passed", "failed")) {
            throw "Native correctness requires an explicit boolean passed and a known accessibilityStatus."
        }
        if ($record.variant -eq "head" -and -not $record.correctness.passed) {
            throw "Native HEAD operation-level correctness failed."
        }
        if ($record.correctness.accessibilityStatus -eq "failed") {
            throw "Native result reports an accessibility failure."
        }
        $timestamp = [DateTimeOffset]::MinValue
        if (($record.timestampUtc -isnot [string] -and $record.timestampUtc -isnot [DateTime] -and
            $record.timestampUtc -isnot [DateTimeOffset]) -or
            -not [DateTimeOffset]::TryParse([string]$record.timestampUtc,
            [Globalization.CultureInfo]::InvariantCulture,
            [Globalization.DateTimeStyles]::RoundtripKind, [ref]$timestamp)) {
            throw "Native result timestampUtc is missing or invalid."
        }
        foreach ($field in $environmentFields) {
            $value = Get-DeviceProperty $record.environment $field
            if ($value -isnot [string] -or [string]::IsNullOrWhiteSpace($value) -or
                $value -eq "unknown" -or $value -cne $records[0].environment.$field) {
                throw "Native environment.$field must be present and identical in every base/head record."
            }
        }
        if ($record.measurementsMilliseconds -isnot [array] -or
            $record.measurementsMilliseconds.Count -eq 0) {
            throw "Native measurementsMilliseconds must be a nonempty array."
        }
        foreach ($value in $record.measurementsMilliseconds) {
            Assert-DeviceNumber $value "measurementsMilliseconds"
        }
        if ($record.warmupCount -ne $records[0].warmupCount -or
            $record.measurementsMilliseconds.Count -ne $records[0].measurementsMilliseconds.Count) {
            throw "Native warmup and measurement counts must describe the same workload in all four runs."
        }

        # Check per-run statistics; the trusted core comparator owns cross-variant verdicts.
        $sorted = @($record.measurementsMilliseconds | Sort-Object)
        $middle = [int][Math]::Floor($sorted.Count / 2)
        $statistics = [ordered]@{
            minimumMilliseconds = $sorted[0]
            maximumMilliseconds = $sorted[-1]
            medianMilliseconds = if ($sorted.Count % 2) {
                $sorted[$middle]
            } else {
                ([double]$sorted[$middle - 1] + [double]$sorted[$middle]) / 2
            }
            p95Milliseconds = $sorted[[int][Math]::Ceiling($sorted.Count * 0.95) - 1]
            meanMilliseconds = ($sorted | Measure-Object -Average).Average
        }
        foreach ($field in $statistics.Keys) {
            $value = Get-DeviceProperty $record.statistics $field
            Assert-DeviceNumber $value "statistics.$field"
            if ([Math]::Abs([double]$value - [double]$statistics[$field]) -gt
                [Math]::Max(1e-9, [Math]::Abs([double]$statistics[$field]) * 1e-9)) {
                throw "Native statistics.$field does not match the recorded measurements."
            }
        }
        if ($record.counters -isnot [PSCustomObject] -or $counterNames.Count -eq 0 -or
            (@($record.counters.PSObject.Properties.Name | Sort-Object) -join "|") -cne ($counterNames -join "|")) {
            throw "Native counters must be nonempty and complete on both sides in every run."
        }
        foreach ($field in $counterNames) {
            Assert-DeviceNumber $record.counters.$field "counters.$field" -allowNegative
        }
    }
}

$selection = [PSCustomObject]@{ deviceScenarios = @() }
$required = @()
$requests = @()
try {
    Assert-LocalDeviceIdentity $identity
    if ($HeadCommitSha -cne $CurrentHeadSha) {
        throw "Requested head '$HeadCommitSha' is stale; current head is '$CurrentHeadSha'."
    }
    $selection = Read-DeviceJson $SelectionPath
    $required = @(Get-LocalDeviceRequirements $selection)
    $requests = @(New-LocalDeviceRequests $selection $identity $root)
    $handoff = Read-DeviceJson $RequestPath
    if ($handoff -isnot [array] -or $handoff.Count -ne $requests.Count) {
        throw "RequestPath must contain exactly the selected local request array, including pending pairs."
    }
    foreach ($request in $requests) {
        $entries = @($handoff | Where-Object { $_.requestKey -ceq $request.requestKey })
        if ($entries.Count -ne 1 -or -not (Test-DeviceJsonEqual $entries[0] $request)) {
            throw "Local handoff '$($request.requestKey)' has mismatched identities, fields, or resultDirectory."
        }
        Assert-LocalDeviceIdentity $entries[0]
        if (-not (Test-DeviceInteger $entries[0].schemaVersion 1 1) -or
            -not (Test-DeviceInteger $entries[0].expectedVariantRuns 2 2)) {
            throw "Local handoff schemaVersion and expectedVariantRuns must be integers."
        }
    }
} catch {
    $errors.Add($_.Exception.Message)
}

$seenPaths = [Collections.Generic.HashSet[string]]::new(
    $(if ($IsWindows) { [StringComparer]::OrdinalIgnoreCase } else { [StringComparer]::Ordinal }))
if ($errors.Count -eq 0) {
    foreach ($path in $SummaryPath) {
        try {
            $summaryFile = Get-LocalDevicePath $path "File" -mustExist
            $matches = @($requests | Where-Object {
                Test-LocalDevicePathEqual $summaryFile (Join-Path $_.resultDirectory "comparison-summary.json")
            })
            if ($matches.Count -ne 1 -or -not $seenPaths.Add($summaryFile)) {
                throw "Summary path must match exactly one requested local result directory, without duplicates."
            }
            $request = $matches[0]
            [void](Get-LocalDevicePath $request.resultDirectory "Directory" -mustExist)
            $planPath = Join-Path $request.resultDirectory "run-plan.json"
            $resultsPath = Join-Path $request.resultDirectory "results.json"
            $markdownPath = Get-LocalDevicePath (
                Join-Path $request.resultDirectory "comparison-summary.md"
            ) "File" -mustExist
            if ((Get-Item -LiteralPath $markdownPath).Length -eq 0) {
                throw "Local comparison-summary.md is empty."
            }
            $plan = Read-DeviceJson $planPath
            $records = Read-DeviceJson $resultsPath
            $summary = Read-DeviceJson $summaryFile
            Assert-DeviceRunPlan $plan $request
            Assert-DeviceRecords $records $request
            if (-not (Test-DeviceInteger $summary.schemaVersion 3 3)) {
                throw "The local comparison summary must use schemaVersion=3."
            }
            $expected = [PSCustomObject]@{
                repository = $Repository
                pullRequestNumber = $PullRequestNumber
                baseCommitSha = $BaseCommitSha
                headCommitSha = $HeadCommitSha
                harnessSha = $HarnessSha
                platform = $request.platform
                scenario = $request.expectedScenario
                variantRuns = 2
            }
            if (-not (Test-DeviceJsonEqual $summary.expected $expected)) {
                throw "Local summary expected identities do not match the requested PR/revisions/scenario/platform."
            }
            $suppliedComparisons = @($summary.comparisons)
            if ($suppliedComparisons.Count -ne 1 -or
                $suppliedComparisons[0].Scenario -cne $request.expectedScenario -or
                $suppliedComparisons[0].Platform -cne $request.platform -or
                $suppliedComparisons[0].BaseCommit -cne $BaseCommitSha -or
                $suppliedComparisons[0].HeadCommit -cne $HeadCommitSha) {
                throw "Local summary comparison identities do not match the local request."
            }
            if ($summary.provenanceValidated -isnot [bool] -or -not $summary.provenanceValidated -or
                $summary.correctnessPassed -isnot [bool] -or -not $summary.correctnessPassed -or
                $suppliedComparisons[0].Complete -isnot [bool] -or -not $suppliedComparisons[0].Complete -or
                $suppliedComparisons[0].ProvenanceValidated -isnot [bool] -or -not $suppliedComparisons[0].ProvenanceValidated -or
                $suppliedComparisons[0].CorrectnessPassed -isnot [bool] -or -not $suppliedComparisons[0].CorrectnessPassed -or
                $suppliedComparisons[0].BaseCorrectnessPassed -isnot [bool] -or
                -not (Test-DeviceInteger $suppliedComparisons[0].BaseResultCount 2 2) -or
                -not (Test-DeviceInteger $suppliedComparisons[0].HeadResultCount 2 2) -or
                -not (Test-DeviceInteger $summary.baseCorrectnessFailureCount 0 2)) {
                throw "Local summary is incomplete or failed provenance/HEAD correctness."
            }
            if ($summary.verdict -cnotin @("neutral", "time-regression-advisory", "time-improvement-advisory")) {
                throw "Local summary verdict must be neutral or a native timing advisory."
            }
            if ($null -ne $summary.timePctTolerance) {
                Assert-DeviceNumber $summary.timePctTolerance "timePctTolerance"
                if ($summary.timePctTolerance -ne 15) {
                    throw "Local timing tolerance must be the deterministic 15 percent threshold."
                }
            }

            $temporary = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-validation-" + [Guid]::NewGuid().ToString("N"))
            New-Item -ItemType Directory -Path $temporary | Out-Null
            try {
                $recomputedPath = Join-Path $temporary "comparison.json"
                & $comparator -ResultsPath $resultsPath -JsonOut $recomputedPath `
                    -MarkdownOut (Join-Path $temporary "comparison.md") `
                    -ExpectedRepository $Repository -ExpectedPullRequestNumber $PullRequestNumber `
                    -ExpectedBaseCommitSha $BaseCommitSha -ExpectedHeadCommitSha $HeadCommitSha `
                    -ExpectedHarnessSha $HarnessSha -ExpectedPlatform $request.platform `
                    -ExpectedScenario $request.expectedScenario -ExpectedVariantRuns 2 -TimePctTolerance 15
                if ($LASTEXITCODE -ne 0) {
                    throw "Trusted core comparator failed with exit code $LASTEXITCODE."
                }
                $recomputed = Read-DeviceJson $recomputedPath
            } finally {
                Remove-Item -LiteralPath $temporary -Recurse -Force
            }
            $comparisons = @($recomputed.comparisons)
            if ($recomputed.schemaVersion -ne 3 -or
                -not (Test-DeviceInteger $recomputed.baseCorrectnessFailureCount)) {
                throw "Trusted core comparator must produce schema 3 with BaseCorrectnessPassed and baseCorrectnessFailureCount; integrate the updated device prerequisite."
            }
            if ($comparisons.Count -ne 1) {
                throw "Trusted core comparison must contain exactly one requested scenario/platform pair."
            }
            $comparison = $comparisons[0]
            if ($recomputed.provenanceValidated -isnot [bool] -or -not $recomputed.provenanceValidated -or
                $recomputed.correctnessPassed -isnot [bool] -or -not $recomputed.correctnessPassed -or
                $comparison.Complete -isnot [bool] -or -not $comparison.Complete -or
                $recomputed.verdict -cnotin @("neutral", "time-regression-advisory", "time-improvement-advisory")) {
                throw "Local native evidence is incomplete or failed provenance/HEAD correctness: $($comparison.Reason)"
            }
            if ($comparison.BaseCorrectnessPassed -isnot [bool]) {
                throw "Trusted core comparator must report BaseCorrectnessPassed for complete evidence; integrate the updated device prerequisite."
            }
            foreach ($field in @(
                "schemaVersion", "expected", "verdict", "provenanceValidated", "correctnessPassed",
                "baseCorrectnessFailureCount", "accessibilityStatuses", "comparisons"
            )) {
                if (-not (Test-DeviceJsonEqual $summary.$field $recomputed.$field)) {
                    throw "Local summary '$field' does not match trusted recomputation of results.json."
                }
            }
            $accepted.Add([PSCustomObject]@{
                requestKey = $request.requestKey
                scenarioIds = @($request.scenarioIds)
                resultScenario = $request.expectedScenario
                platform = $request.platform
                resultDirectory = $request.resultDirectory
                summaryPath = $summaryFile
                resultsPath = $resultsPath
                runPlanSha256 = (Get-FileHash -LiteralPath $planPath -Algorithm SHA256).Hash.ToLowerInvariant()
                resultsSha256 = (Get-FileHash -LiteralPath $resultsPath -Algorithm SHA256).Hash.ToLowerInvariant()
                summarySha256 = (Get-FileHash -LiteralPath $summaryFile -Algorithm SHA256).Hash.ToLowerInvariant()
                verdict = $recomputed.verdict
                correctnessPassed = $true
                baseCorrectnessPassed = $comparison.BaseCorrectnessPassed
                baseCorrectnessFailureCount = $recomputed.baseCorrectnessFailureCount
                accessibilityStatuses = @($recomputed.accessibilityStatuses)
                environment = $comparison.Environment
                base = $comparison.Base
                head = $comparison.Head
                medianDeltaPct = $comparison.MedianDeltaPct
                rangesDoNotOverlap = $comparison.RangesDoNotOverlap
                counters = @($comparison.Counters)
            })
        } catch {
            $errors.Add("Summary '$path': $($_.Exception.Message)")
        }
    }
}

$missing = @(
    foreach ($request in $requests) {
        if ($request.requestKey -cnotin @($accepted.requestKey)) {
            [PSCustomObject]@{
                requestKey = $request.requestKey
                scenarioIds = @($request.scenarioIds)
                resultScenario = $request.expectedScenario
                platform = $request.platform
                resultDirectory = $request.resultDirectory
                reason = "No complete validated local handoff was supplied."
            }
        }
    }
    foreach ($scenario in @($selection.deviceScenarios | Where-Object {
        $null -ne $_ -and $_.automationStatus -ne "manual-local-ready"
    })) {
        [PSCustomObject]@{
            scenarioIds = @($scenario.id)
            resultScenario = $null
            platform = @($scenario.platforms) -join ","
            reason = "No supported local device scenario is available."
        }
    }
)
$sealed = $errors.Count -eq 0
$complete = $sealed -and $required.Count -gt 0 -and $missing.Count -eq 0
$accessibilityStatuses = @($accepted.accessibilityStatuses | ForEach-Object { $_ } | Sort-Object -Unique)
$result = [PSCustomObject]@{
    schemaVersion = 2
    evidenceKind = "manual-local-device"
    nativeSchemaVersion = 3
    sealed = $sealed
    deviceEvidenceComplete = $complete
    repository = $Repository
    pullRequestNumber = $PullRequestNumber
    baseCommitSha = $BaseCommitSha
    headCommitSha = $HeadCommitSha
    harnessSha = $HarnessSha
    resultsRoot = $root
    correctnessPassed = $sealed -and $accepted.Count -gt 0
    baseCorrectnessFailureCount = [int](($accepted | Measure-Object -Property baseCorrectnessFailureCount -Sum).Sum)
    accessibilityStatus = if ($sealed -and $accessibilityStatuses.Count -eq 1 -and
        $accessibilityStatuses[0] -eq "passed") { "passed" } else { "not-assessed" }
    allAffectedPlatformsCovered = $complete
    requiredMeasurements = $required
    acceptedMeasurements = @($accepted | ForEach-Object { $_ })
    missingMeasurements = $missing
    errors = @($errors | ForEach-Object { $_ })
}
$directory = Split-Path -Parent $JsonOut
if (-not (Test-Path -LiteralPath $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}
$result | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $JsonOut -Encoding UTF8
if (-not $sealed) {
    foreach ($validationError in $errors) {
        [Console]::Error.WriteLine("ERROR: $validationError")
    }
    exit 2
}
Write-Host "Local device evidence validated (complete=$complete); no execution or publication performed."
exit 0
