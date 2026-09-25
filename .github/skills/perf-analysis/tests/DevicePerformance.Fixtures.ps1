. (Join-Path $PSScriptRoot "..\scripts\DevicePerformance.Local.ps1")

function Write-DeviceFixtureJson([string]$path, $value) {
    ConvertTo-Json -InputObject $value -Depth 25 | Set-Content -LiteralPath $path -Encoding UTF8
}

function New-DeviceIdentityFixture {
    [PSCustomObject]@{
        repository = "dotnet/maui"
        pullRequestNumber = 42
        baseCommitSha = "a" * 40
        headCommitSha = "b" * 40
        harnessSha = "c" * 40
    }
}

function New-DeviceSelectionFixture([string[]]$ids = @("carouselview-wheel-snap-windows")) {
    $registry = Get-Content -LiteralPath (
        Join-Path $PSScriptRoot "..\references\platform-scenarios.json"
    ) -Raw | ConvertFrom-Json
    $scenarios = @($registry.scenarios | Where-Object { $_.id -in $ids })
    if ($scenarios.Count -ne @($ids | Sort-Object -Unique).Count) {
        throw "Unknown device fixture scenario."
    }
    $sampledCount = @($scenarios | Where-Object { $_.coverageMode -eq "sampled" }).Count
    [PSCustomObject]@{
        coverage = [PSCustomObject]@{
            productFileCount = $scenarios.Count
            managedMeasuredFileCount = 0
            managedSampledFileCount = 0
            deviceRequiredFileCount = $scenarios.Count - $sampledCount
            deviceSampledFileCount = $sampledCount
            staticOnlyFileCount = $sampledCount
            canClaimWholePrClean = $false
            benchmarkInputsChanged = $false
        }
        suites = @()
        deviceScenarios = $scenarios
        sampledProductFiles = @()
        staticOnlyProductFiles = if ($sampledCount) { @("sampled-handler.cs") } else { @() }
    }
}

function New-DeviceRecordFixture($request, [string]$variant, [int]$ordinal) {
    $counters = switch ($request.expectedScenario) {
        "collectionview-keepitemsinview-update" {
            @{ lastFirstVisiblePosition = 8; lastExpectedFirstVisiblePosition = 8; updatesPreservingFirstVisibleItem = 4 }
        }
        "collectionview-grouped-scrollto-makevisible" {
            @{ targetPositionSpread = 0; positionsOutsideTolerance = 0 }
        }
        "carouselview-swipe-disabled" {
            if ($request.platform -eq "android") {
                @{ interceptedTouchEventCount = 0; finalPosition = 0 }
            } else {
                @{ embeddedScrollViewCount = 2; stateReapplicationFailures = 0 }
            }
        }
        "carouselview-wheel-snap-windows" {
            @{ maximumCenterError = 0; positionsOutsideTolerance = 0; positionMismatchCount = 0 }
        }
        "handler-property-update-batch" {
            @{ completedUpdateBatches = 4; nativeValueMismatchCount = 0 }
        }
        default { throw "No synthetic counters for '$($request.expectedScenario)'." }
    }
    [PSCustomObject]@{
        schemaVersion = 3
        repository = $request.repository
        pullRequestNumber = $request.pullRequestNumber
        scenario = $request.expectedScenario
        platform = $request.platform
        variant = $variant
        commitSha = if ($variant -eq "base") { $request.baseCommitSha } else { $request.headCommitSha }
        harnessSha = $request.harnessSha
        runOrdinal = $ordinal
        expectedVariantRuns = 2
        timestampUtc = "2026-09-15T00:00:00Z"
        warmupCount = 2
        environment = [PSCustomObject]@{
            executionKind = "native-host"
            deviceModel = "Synthetic fixture"
            osVersion = "fixture-os"
            runtimeFramework = ".NET fixture"
            processArchitecture = "X64"
            runtimeVariant = "fixture-runtime"
            sdkVersion = "fixture-sdk"
        }
        correctness = [PSCustomObject]@{ passed = $true; accessibilityStatus = "not-assessed" }
        measurementsMilliseconds = @(100, 100)
        statistics = [PSCustomObject]@{
            minimumMilliseconds = 100
            maximumMilliseconds = 100
            medianMilliseconds = 100
            p95Milliseconds = 100
            meanMilliseconds = 100
        }
        counters = [PSCustomObject]$counters
    }
}

function New-DeviceBundleFixture($request) {
    New-Item -ItemType Directory -Force -Path $request.resultDirectory | Out-Null
    $records = @(
        (New-DeviceRecordFixture $request "base" 1),
        (New-DeviceRecordFixture $request "head" 1),
        (New-DeviceRecordFixture $request "head" 2),
        (New-DeviceRecordFixture $request "base" 2)
    )
    $plan = @(
        foreach ($record in $records) {
            $runDirectory = Join-Path $request.resultDirectory "$($record.variant)-run$($record.runOrdinal)"
            New-Item -ItemType Directory -Force -Path $runDirectory | Out-Null
            if ($request.platform -eq "windows") {
                [PSCustomObject]@{
                    variant = $record.variant
                    runOrdinal = $record.runOrdinal
                    commitSha = $record.commitSha
                    app = "synthetic-not-an-executable"
                    category = "Synthetic"
                    categoryIndex = 0
                }
            } else {
                [PSCustomObject]@{
                    Variant = $record.variant
                    Number = $record.runOrdinal
                    CommitSha = $record.commitSha
                    RunDirectory = $runDirectory
                    Executable = "synthetic-must-not-execute"
                    Arguments = @("must-not-execute")
                    ResultFile = $null
                }
            }
        }
    )
    $summary = [PSCustomObject]@{
        schemaVersion = 3
        verdict = "neutral"
        timePctTolerance = 15
        expected = [PSCustomObject]@{
            repository = $request.repository
            pullRequestNumber = $request.pullRequestNumber
            baseCommitSha = $request.baseCommitSha
            headCommitSha = $request.headCommitSha
            harnessSha = $request.harnessSha
            platform = $request.platform
            scenario = $request.expectedScenario
            variantRuns = 2
        }
        provenanceValidated = $true
        correctnessPassed = $true
        baseCorrectnessFailureCount = 0
        accessibilityStatuses = @("not-assessed")
        comparisons = @([PSCustomObject]@{
            Scenario = $request.expectedScenario
            Platform = $request.platform
            Complete = $true
            ProvenanceValidated = $true
            CorrectnessPassed = $true
            BaseCorrectnessPassed = $true
            BaseCommit = $request.baseCommitSha
            HeadCommit = $request.headCommitSha
            BaseResultCount = 2
            HeadResultCount = 2
            Environment = $records[0].environment
            Base = [PSCustomObject]@{ Minimum = 100; Maximum = 100; Median = 100; Count = 4 }
            Head = [PSCustomObject]@{ Minimum = 100; Maximum = 100; Median = 100; Count = 4 }
            MedianDeltaPct = 0
            RangesDoNotOverlap = $false
            Counters = @($records[0].counters.PSObject.Properties.Name | Sort-Object | ForEach-Object {
                [PSCustomObject]@{ Name = $_; Base = $records[0].counters.$_; Head = $records[0].counters.$_; Delta = 0 }
            })
            Flag = "neutral"
            Reason = $null
        })
    }
    Write-DeviceFixtureJson (Join-Path $request.resultDirectory "run-plan.json") $plan
    Write-DeviceFixtureJson (Join-Path $request.resultDirectory "results.json") $records
    Write-DeviceFixtureJson (Join-Path $request.resultDirectory "comparison-summary.json") $summary
    "Synthetic local comparison; no apps were executed." |
        Set-Content -LiteralPath (Join-Path $request.resultDirectory "comparison-summary.md")
}

function New-DeviceValidationFixture($selection, [string]$verdict = "neutral", [bool]$basePassed = $true) {
    $identity = New-DeviceIdentityFixture
    $required = @(Get-LocalDeviceRequirements $selection)
    $root = Join-Path ([IO.Path]::GetTempPath()) "synthetic-policy-evidence"
    $accepted = @(
        foreach ($requirement in $required) {
            $key = Get-LocalDeviceRequestKey $identity $requirement.resultScenario $requirement.platform
            [PSCustomObject]@{
                requestKey = $key
                scenarioIds = @($requirement.scenarioIds)
                resultScenario = $requirement.resultScenario
                platform = $requirement.platform
                resultDirectory = Join-Path $root $key
                summaryPath = Join-Path (Join-Path $root $key) "comparison-summary.json"
                verdict = $verdict
                correctnessPassed = $true
                baseCorrectnessPassed = $basePassed
                baseCorrectnessFailureCount = if ($basePassed) { 0 } else { 2 }
                base = [PSCustomObject]@{ Minimum = 100; Maximum = 100; Median = 100; Count = 4 }
                head = [PSCustomObject]@{ Minimum = 100; Maximum = 100; Median = 100; Count = 4 }
            }
        }
    )
    [PSCustomObject]@{
        schemaVersion = 2
        evidenceKind = "manual-local-device"
        nativeSchemaVersion = 3
        sealed = $true
        deviceEvidenceComplete = $true
        repository = $identity.repository
        pullRequestNumber = $identity.pullRequestNumber
        baseCommitSha = $identity.baseCommitSha
        headCommitSha = $identity.headCommitSha
        harnessSha = $identity.harnessSha
        resultsRoot = $root
        correctnessPassed = $true
        baseCorrectnessFailureCount = if ($basePassed) { 0 } else { 2 * $accepted.Count }
        allAffectedPlatformsCovered = $true
        accessibilityStatus = "not-assessed"
        requiredMeasurements = $required
        acceptedMeasurements = $accepted
        missingMeasurements = @()
        errors = @()
    }
}
