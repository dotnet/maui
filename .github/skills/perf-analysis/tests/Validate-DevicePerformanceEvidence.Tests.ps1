#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "DevicePerformance.Fixtures.ps1")
$script = Join-Path $PSScriptRoot "..\scripts\Validate-DevicePerformanceEvidence.ps1"
$comparator = [IO.Path]::GetFullPath([IO.Path]::Combine(
    $PSScriptRoot, "..", "..", "..", "..", "eng", "scripts", "Compare-DevicePerformanceResults.ps1"))
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-device-evidence-" + [Guid]::NewGuid().ToString("N"))
$failures = [Collections.Generic.List[string]]::new()
$passed = 0

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Test-Case([string]$name, [scriptblock]$action) {
    try {
        & $action
        $script:passed++
        Write-Host "PASS: $name"
    } catch {
        $failures.Add("${name}: $($_.Exception.Message)")
        Write-Host "FAIL: $name - $($_.Exception.Message)"
    }
}

function New-EvidenceCase([string]$name, [string[]]$ids = @("carouselview-wheel-snap-windows")) {
    $root = Join-Path $testRoot $name
    $resultsRoot = Join-Path $root "results"
    New-Item -ItemType Directory -Force -Path $resultsRoot | Out-Null
    $identity = New-DeviceIdentityFixture
    $selection = New-DeviceSelectionFixture $ids
    $requests = @(New-LocalDeviceRequests $selection $identity $resultsRoot)
    $selectionPath = Join-Path $root "selection.json"
    $requestPath = Join-Path $root "requests.json"
    Write-DeviceFixtureJson $selectionPath $selection
    Write-DeviceFixtureJson $requestPath $requests
    foreach ($request in $requests) { New-DeviceBundleFixture $request }
    [PSCustomObject]@{
        root = $root
        requests = $requests
        summaryPaths = @($requests | ForEach-Object { Join-Path $_.resultDirectory "comparison-summary.json" })
        arguments = @{
            SelectionPath = $selectionPath
            RequestPath = $requestPath
            ResultsRoot = $resultsRoot
            Repository = $identity.repository
            PullRequestNumber = $identity.pullRequestNumber
            BaseCommitSha = $identity.baseCommitSha
            HeadCommitSha = $identity.headCommitSha
            CurrentHeadSha = $identity.headCommitSha
            HarnessSha = $identity.harnessSha
            JsonOut = Join-Path $root "validation.json"
        }
    }
}

function Edit-FixtureJson([string]$path, [scriptblock]$edit) {
    $value = Get-Content -LiteralPath $path -Raw | ConvertFrom-Json -NoEnumerate
    & $edit $value
    Write-DeviceFixtureJson $path $value
}

function Invoke-EvidenceCase($case, [switch]$noSummaries) {
    $arguments = $case.arguments.Clone()
    $arguments.SummaryPath = if ($noSummaries) { @() } else { $case.summaryPaths }
    $before = @(Get-ChildItem -LiteralPath $arguments.ResultsRoot -File -Recurse |
        Sort-Object FullName | Get-FileHash -Algorithm SHA256 | ForEach-Object { "$($_.Path)|$($_.Hash)" })
    & $script @arguments
    $exitCode = $LASTEXITCODE
    $after = @(Get-ChildItem -LiteralPath $arguments.ResultsRoot -File -Recurse |
        Sort-Object FullName | Get-FileHash -Algorithm SHA256 | ForEach-Object { "$($_.Path)|$($_.Hash)" })
    Assert-Equal ($before -join "`n") ($after -join "`n") "Validation must not modify acquisition artifacts"
    [PSCustomObject]@{
        exitCode = $exitCode
        result = Get-Content -LiteralPath $arguments.JsonOut -Raw | ConvertFrom-Json
    }
}

function Assert-Rejected($actual, [string]$reason) {
    Assert-Equal 2 $actual.exitCode "Invalid evidence must fail"
    Assert-Equal $false $actual.result.sealed "Invalid evidence must not seal"
    Assert-Equal $false $actual.result.deviceEvidenceComplete "Invalid evidence must not be complete"
    Assert-Equal 0 @($actual.result.acceptedMeasurements).Count "Invalid measurements must not be admitted"
    if (($actual.result.errors -join " ") -notmatch $reason) {
        throw "Expected reason '$reason', got: $($actual.result.errors -join ' ')"
    }
}

function Assert-Accepted($actual) {
    Assert-Equal 0 $actual.exitCode "Complete schema-3 native evidence must validate: $($actual.result.errors -join ' ')"
    Assert-Equal $true $actual.result.sealed "Complete evidence must seal"
    Assert-Equal $true $actual.result.deviceEvidenceComplete "Complete native coverage"
    Assert-Equal $true $actual.result.correctnessPassed "HEAD correctness"
}

function Update-CoreSummary($case) {
    foreach ($request in $case.requests) {
        & $comparator -ResultsPath (Join-Path $request.resultDirectory "results.json") `
            -JsonOut (Join-Path $request.resultDirectory "comparison-summary.json") `
            -MarkdownOut (Join-Path $request.resultDirectory "comparison-summary.md") `
            -ExpectedRepository $request.repository -ExpectedPullRequestNumber $request.pullRequestNumber `
            -ExpectedBaseCommitSha $request.baseCommitSha -ExpectedHeadCommitSha $request.headCommitSha `
            -ExpectedHarnessSha $request.harnessSha -ExpectedPlatform $request.platform `
            -ExpectedScenario $request.expectedScenario -ExpectedVariantRuns 2 -TimePctTolerance 15
        Assert-Equal 0 $LASTEXITCODE "Synthetic comparison generation"
    }
}

New-Item -ItemType Directory -Path $testRoot | Out-Null
try {
    Test-Case "no implicit discovery or execution" {
        $case = New-EvidenceCase "pending"
        $actual = Invoke-EvidenceCase $case -noSummaries
        Assert-Equal 0 $actual.exitCode "Pending evidence is a coverage gap, not corrupt input"
        Assert-Equal $true $actual.result.sealed "Pending handoff metadata is valid"
        Assert-Equal $false $actual.result.deviceEvidenceComplete "Omitted summaries must not be discovered implicitly"
        Assert-Equal 1 @($actual.result.missingMeasurements).Count "Pending request"
        Assert-Equal $false $actual.result.correctnessPassed "No HEAD outcome from absent evidence"
    }
    Test-Case "unsupported pairs have no handoff" {
        $case = New-EvidenceCase "unsupported" @("collectionview-handler-device")
        $actual = Invoke-EvidenceCase $case
        Assert-Equal 0 $actual.exitCode "Unsupported path is a coverage gap"
        Assert-Equal 0 $case.requests.Count "No fabricated supported request"
        Assert-Equal $false $actual.result.deviceEvidenceComplete "Unsupported coverage stays incomplete"
        Assert-Equal 1 @($actual.result.missingMeasurements).Count "Unsupported measurement is explicit"
    }

    foreach ($field in @("requestKey", "repository", "pullRequestNumber", "baseCommitSha", "headCommitSha", "harnessSha", "expectedScenario", "platform", "resultDirectory")) {
        Test-Case "handoff mismatch: $field" {
            $case = New-EvidenceCase "handoff-$field"
            Edit-FixtureJson $case.arguments.RequestPath {
                param($value)
                $value[0].$field = if ($field -eq "pullRequestNumber") { 43 } else { "mismatched" }
            }
            Assert-Rejected (Invoke-EvidenceCase $case) "Local handoff"
        }
    }
    foreach ($caseName in @("missing-handoff", "missing-request", "duplicate-request", "extra-request", "stale-head", "wrong-root")) {
        Test-Case $caseName {
            $case = New-EvidenceCase $caseName
            switch ($caseName) {
                "missing-handoff" { Remove-Item -LiteralPath $case.arguments.RequestPath }
                "missing-request" { Write-DeviceFixtureJson $case.arguments.RequestPath @() }
                "duplicate-request" { Write-DeviceFixtureJson $case.arguments.RequestPath @($case.requests[0], $case.requests[0]) }
                "extra-request" {
                    $extra = $case.requests[0] | ConvertTo-Json | ConvertFrom-Json
                    $extra.platform = "unsupported"
                    Write-DeviceFixtureJson $case.arguments.RequestPath @($case.requests[0], $extra)
                }
                "stale-head" { $case.arguments.CurrentHeadSha = "d" * 40 }
                "wrong-root" {
                    $case.arguments.ResultsRoot = Join-Path $case.root "other-results"
                    New-Item -ItemType Directory -Path $case.arguments.ResultsRoot | Out-Null
                }
            }
            Assert-Rejected (Invoke-EvidenceCase $case) "exist|request array|stale|Local handoff"
        }
    }
    foreach ($file in @("run-plan.json", "results.json", "comparison-summary.json", "comparison-summary.md")) {
        Test-Case "missing local file: $file" {
            $case = New-EvidenceCase "missing-$file"
            Remove-Item -LiteralPath (Join-Path $case.requests[0].resultDirectory $file)
            Assert-Rejected (Invoke-EvidenceCase $case) "does not exist"
        }
    }
    Test-Case "malformed local JSON" {
        $case = New-EvidenceCase "invalid-json"
        "not JSON" | Set-Content -LiteralPath (Join-Path $case.requests[0].resultDirectory "results.json")
        Assert-Rejected (Invoke-EvidenceCase $case) "JSON"
    }
    Test-Case "empty local Markdown companion" {
        $case = New-EvidenceCase "empty-markdown"
        "" | Set-Content -LiteralPath (Join-Path $case.requests[0].resultDirectory "comparison-summary.md") -NoNewline
        Assert-Rejected (Invoke-EvidenceCase $case) "comparison-summary.md is empty"
    }
    Test-Case "summary outside reviewed directory" {
        $case = New-EvidenceCase "outside-summary"
        $outside = Join-Path $case.root "outside.json"
        Copy-Item -LiteralPath $case.summaryPaths[0] -Destination $outside
        $case.summaryPaths = @($outside)
        Assert-Rejected (Invoke-EvidenceCase $case) "Summary path"
    }
    Test-Case "input cannot be used as validation output" {
        $case = New-EvidenceCase "output-collision"
        $case.arguments.JsonOut = $case.summaryPaths[0]
        $failed = $false
        try { Invoke-EvidenceCase $case | Out-Null } catch {
            $failed = $_.Exception.Message -match "outside the read-only results root"
        }
        Assert-Equal $true $failed "Validation output must not overwrite input"
    }
    foreach ($caseName in @("wrong-abba", "wrong-plan-commit", "missing-run-directory", "wrong-run-directory")) {
        Test-Case $caseName {
            $case = New-EvidenceCase $caseName @("collectionview-items-update-android")
            $planPath = Join-Path $case.requests[0].resultDirectory "run-plan.json"
            switch ($caseName) {
                "wrong-abba" { Edit-FixtureJson $planPath { param($p) $p[0].Variant = "head" } }
                "wrong-plan-commit" { Edit-FixtureJson $planPath { param($p) $p[1].CommitSha = "d" * 40 } }
                "missing-run-directory" {
                    Remove-Item -LiteralPath (Join-Path $case.requests[0].resultDirectory "head-run2")
                }
                "wrong-run-directory" {
                    Edit-FixtureJson $planPath { param($p) $p[1].RunDirectory = $case.root }
                }
            }
            Assert-Rejected (Invoke-EvidenceCase $case) "ABBA|does not exist|RunDirectory mismatch"
        }
    }
    Test-Case "linked result directory" {
        $case = New-EvidenceCase "linked-result"
        $link = $case.requests[0].resultDirectory
        $target = Join-Path $case.root "link-target"
        Move-Item -LiteralPath $link -Destination $target
        $linkKind = if ($IsWindows) { "Junction" } else { "SymbolicLink" }
        New-Item -ItemType $linkKind -Path $link -Target $target | Out-Null
        try {
            $rejected = $false
            try { Invoke-EvidenceCase $case | Out-Null } catch {
                $rejected = $_.Exception.Message -match "symbolic links or reparse points"
            }
            Assert-Equal $true $rejected "Linked inputs must fail before opening an output artifact"
            Assert-Equal $false (Test-Path -LiteralPath $case.arguments.JsonOut) "Unsafe path must not produce a validation artifact"
        } finally {
            Remove-Item -LiteralPath $link -Force
        }
    }

    foreach ($field in @("repository", "pullRequestNumber", "scenario", "platform", "harnessSha", "baseCommit", "headCommit")) {
        Test-Case "raw identity mismatch: $field" {
            $case = New-EvidenceCase "raw-$field"
            Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
                param($records)
                switch ($field) {
                    "pullRequestNumber" { $records[0].pullRequestNumber = 43 }
                    "baseCommit" { $records[0].commitSha = "d" * 40 }
                    "headCommit" { $records[1].commitSha = "d" * 40 }
                    default { $records[0].$field = "mismatched" }
                }
            }
            Assert-Rejected (Invoke-EvidenceCase $case) "does not match|commit|identity"
        }
    }
    foreach ($field in @("executionKind", "deviceModel", "osVersion", "runtimeFramework", "processArchitecture", "runtimeVariant", "sdkVersion")) {
        Test-Case "missing one environment field: $field" {
            $case = New-EvidenceCase "environment-$field"
            Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
                param($records)
                $records[3].environment.PSObject.Properties.Remove($field)
            }
            Assert-Rejected (Invoke-EvidenceCase $case) "environment.$field"
        }
    }
    $recordCases = [ordered]@{
        "old-native-schema" = @{ edit = { param($r) $r[0].schemaVersion = 2 }; reason = "schemaVersion=3" }
        "missing-schema" = @{ edit = { param($r) $r[0].PSObject.Properties.Remove("schemaVersion") }; reason = "schemaVersion=3" }
        "negative-warmup" = @{ edit = { param($r) $r[0].warmupCount = -1 }; reason = "nonnegative integer" }
        "fractional-warmup" = @{ edit = { param($r) $r[0].warmupCount = 1.5 }; reason = "nonnegative integer" }
        "string-warmup" = @{ edit = { param($r) $r[0].warmupCount = "2" }; reason = "nonnegative integer" }
        "missing-warmup" = @{ edit = { param($r) $r[0].PSObject.Properties.Remove("warmupCount") }; reason = "nonnegative integer" }
        "head-correctness-failed" = @{ edit = { param($r) $r[1].correctness.passed = $false }; reason = "HEAD operation-level correctness" }
        "missing-base-correctness" = @{ edit = { param($r) $r[0].correctness.PSObject.Properties.Remove("passed") }; reason = "explicit boolean" }
        "string-correctness" = @{ edit = { param($r) $r[1].correctness.passed = "true" }; reason = "explicit boolean" }
        "accessibility-failed" = @{ edit = { param($r) $r[1].correctness.accessibilityStatus = "failed" }; reason = "accessibility failure" }
        "missing-accessibility" = @{ edit = { param($r) $r[1].correctness.PSObject.Properties.Remove("accessibilityStatus") }; reason = "accessibilityStatus" }
        "missing-timestamp" = @{ edit = { param($r) $r[0].PSObject.Properties.Remove("timestampUtc") }; reason = "timestampUtc" }
        "duplicate-run" = @{ edit = { param($r) $r[3].runOrdinal = 1 }; reason = "duplicate variant/run" }
        "fractional-ordinal" = @{ edit = { param($r) $r[0].runOrdinal = 1.5 }; reason = "run identity" }
        "missing-run-count" = @{ edit = { param($r) $r[0].PSObject.Properties.Remove("expectedVariantRuns") }; reason = "run identity" }
        "array-scenario" = @{ edit = { param($r) $r[0].scenario = @($r[0].scenario) }; reason = "run identity" }
        "array-commit" = @{ edit = { param($r) $r[0].commitSha = @($r[0].commitSha) }; reason = "mismatched commit" }
        "array-environment-value" = @{ edit = { param($r) $r[0].environment.deviceModel = @($r[0].environment.deviceModel) }; reason = "environment.deviceModel" }
        "array-statistic" = @{ edit = { param($r) $r[0].statistics.medianMilliseconds = @(100) }; reason = "statistics.medianMilliseconds" }
        "missing-measurements" = @{ edit = { param($r) $r[0].measurementsMilliseconds = @() }; reason = "nonempty array" }
        "negative-measurement" = @{ edit = { param($r) $r[0].measurementsMilliseconds[0] = -1 }; reason = "nonnegative" }
        "string-measurement" = @{ edit = { param($r) $r[0].measurementsMilliseconds[0] = "100" }; reason = "number" }
        "missing-statistic" = @{ edit = { param($r) $r[0].statistics.PSObject.Properties.Remove("medianMilliseconds") }; reason = "statistics.medianMilliseconds" }
        "incorrect-statistic" = @{ edit = { param($r) $r[0].statistics.meanMilliseconds = 99 }; reason = "statistics.meanMilliseconds does not match" }
        "missing-base-counter" = @{ edit = { param($r) $r[3].counters.PSObject.Properties.Remove("maximumCenterError") }; reason = "counters.*complete" }
        "string-counter" = @{ edit = { param($r) $r[0].counters.maximumCenterError = "0" }; reason = "counters.maximumCenterError" }
        "mixed-environment" = @{ edit = { param($r) $r[1].environment.sdkVersion = "other" }; reason = "environment.sdkVersion" }
    }
    foreach ($caseName in $recordCases.Keys) {
        Test-Case $caseName {
            $case = New-EvidenceCase $caseName
            Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") $recordCases[$caseName].edit
            Assert-Rejected (Invoke-EvidenceCase $case) $recordCases[$caseName].reason
        }
    }
    Test-Case "missing raw run" {
        $case = New-EvidenceCase "missing-raw-run"
        $path = Join-Path $case.requests[0].resultDirectory "results.json"
        $records = @(Get-Content -LiteralPath $path -Raw | ConvertFrom-Json)
        Write-DeviceFixtureJson $path @($records[0..2])
        Assert-Rejected (Invoke-EvidenceCase $case) "all four native ABBA"
    }
    foreach ($field in @("repository", "pullRequestNumber", "baseCommitSha", "headCommitSha", "harnessSha", "scenario", "platform")) {
        Test-Case "summary identity mismatch: $field" {
            $case = New-EvidenceCase "summary-$field"
            Edit-FixtureJson $case.summaryPaths[0] {
                param($summary)
                $summary.expected.$field = if ($field -eq "pullRequestNumber") { 43 } else { "mismatched" }
            }
            Assert-Rejected (Invoke-EvidenceCase $case) "summary expected identities"
        }
    }
    foreach ($field in @("Scenario", "Platform", "BaseCommit", "HeadCommit")) {
        Test-Case "comparison identity mismatch: $field" {
            $case = New-EvidenceCase "comparison-$field"
            Edit-FixtureJson $case.summaryPaths[0] { param($summary) $summary.comparisons[0].$field = "mismatched" }
            Assert-Rejected (Invoke-EvidenceCase $case) "comparison identities"
        }
    }
    $summaryCases = [ordered]@{
        "old-summary-schema" = @{ edit = { param($s) $s.schemaVersion = 2 }; reason = "schemaVersion=3" }
        "incomplete-summary" = @{ edit = { param($s) $s.comparisons[0].Complete = $false }; reason = "incomplete" }
        "string-complete" = @{ edit = { param($s) $s.comparisons[0].Complete = "true" }; reason = "incomplete" }
        "missing-base-outcome" = @{ edit = { param($s) $s.comparisons[0].PSObject.Properties.Remove("BaseCorrectnessPassed") }; reason = "incomplete" }
        "missing-base-failure-count" = @{ edit = { param($s) $s.PSObject.Properties.Remove("baseCorrectnessFailureCount") }; reason = "incomplete" }
        "invalid-native-verdict" = @{ edit = { param($s) $s.verdict = "improvement" }; reason = "verdict" }
        "altered-threshold" = @{ edit = { param($s) $s.timePctTolerance = 100 }; reason = "15 percent" }
    }
    foreach ($caseName in $summaryCases.Keys) {
        Test-Case $caseName {
            $case = New-EvidenceCase $caseName
            Edit-FixtureJson $case.summaryPaths[0] $summaryCases[$caseName].edit
            Assert-Rejected (Invoke-EvidenceCase $case) $summaryCases[$caseName].reason
        }
    }

    # These assertions exercise the real trusted core comparator, never a schema shim.
    Test-Case "complete schema-3 native consumption" {
        $case = New-EvidenceCase "complete" @(
            "collectionview-items-update-android", "collectionview-scroll-ios",
            "carouselview-swipe-disabled", "carouselview-wheel-snap-windows",
            "handler-property-update-windows"
        )
        $actual = Invoke-EvidenceCase $case
        Assert-Accepted $actual
        Assert-Equal $case.requests.Count @($actual.result.acceptedMeasurements).Count "Every selected local pair"
        Assert-Equal "not-assessed" $actual.result.accessibilityStatus "No inferred accessibility coverage"
        foreach ($measurement in $actual.result.acceptedMeasurements) {
            Assert-Equal $true ($measurement.resultsSha256 -cmatch '^[0-9a-f]{64}$') "Local result checksum"
            Assert-Equal $false ($measurement.PSObject.Properties.Name -contains "azdoBuildId") "No remote identity"
        }
    }
    Test-Case "partial platform coverage" {
        $case = New-EvidenceCase "partial" @("collectionview-scroll-ios")
        $case.summaryPaths = @($case.summaryPaths[0])
        $actual = Invoke-EvidenceCase $case
        Assert-Equal 0 $actual.exitCode "Valid partial native evidence must validate: $($actual.result.errors -join ' ')"
        Assert-Equal $true $actual.result.sealed "Partial local evidence can be validated"
        Assert-Equal $false $actual.result.deviceEvidenceComplete "Partial platforms cannot clear whole-PR coverage"
        Assert-Equal 1 @($actual.result.missingMeasurements).Count "Missing second Apple platform"
    }
    Test-Case "buggy baseline with valid HEAD" {
        $case = New-EvidenceCase "baseline-failure"
        Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
            param($records)
            $records[0].correctness.passed = $false
            $records[3].correctness.passed = $false
        }
        Update-CoreSummary $case
        $actual = Invoke-EvidenceCase $case
        Assert-Accepted $actual
        Assert-Equal $false $actual.result.acceptedMeasurements[0].baseCorrectnessPassed "Baseline outcome preserved"
        Assert-Equal 2 $actual.result.baseCorrectnessFailureCount "Both failed baseline runs remain context"
    }
    Test-Case "zero integer warmups are valid" {
        $case = New-EvidenceCase "zero-warmup"
        Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
            param($records)
            foreach ($record in $records) { $record.warmupCount = 0 }
        }
        Assert-Accepted (Invoke-EvidenceCase $case)
    }
    Test-Case "duplicate summaries are not extra coverage" {
        $case = New-EvidenceCase "duplicate-summary"
        $case.summaryPaths = @($case.summaryPaths[0], $case.summaryPaths[0])
        $actual = Invoke-EvidenceCase $case
        Assert-Equal 2 $actual.exitCode "Duplicate summary must fail"
        Assert-Equal $false $actual.result.deviceEvidenceComplete "Duplicate summary cannot clear coverage"
        Assert-Equal 1 @($actual.result.acceptedMeasurements).Count "Only one independently validated request"
        Assert-Equal $true (($actual.result.errors -join " ") -match "without duplicates") "Duplicate error is explicit"
    }
    Test-Case "forged neutral summary cannot override measurements" {
        $case = New-EvidenceCase "forged-neutral"
        Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
            param($records)
            foreach ($record in @($records | Where-Object { $_.variant -eq "head" })) {
                $record.measurementsMilliseconds = @(130, 130)
                foreach ($property in $record.statistics.PSObject.Properties) { $property.Value = 130 }
            }
        }
        Assert-Rejected (Invoke-EvidenceCase $case) "summary 'verdict'.*recomputation"
    }
    Test-Case "required scenario counters cannot be omitted on both sides" {
        $case = New-EvidenceCase "missing-required-counter"
        Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
            param($records)
            foreach ($record in $records) { $record.counters.PSObject.Properties.Remove("maximumCenterError") }
        }
        Assert-Rejected (Invoke-EvidenceCase $case) "missing centering counters"
    }
    Test-Case "HEAD counters override a contradictory passed flag" {
        $case = New-EvidenceCase "contradictory-head-counter"
        Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
            param($records)
            $records[1].counters.maximumCenterError = 2
        }
        Assert-Rejected (Invoke-EvidenceCase $case) "center wheel-scroll"
    }
    Test-Case "supported evidence cannot clear an unsupported native path" {
        $case = New-EvidenceCase "mixed-supported" @("carouselview-wheel-snap-windows", "collectionview-handler-device")
        $actual = Invoke-EvidenceCase $case
        Assert-Equal 0 $actual.exitCode "Supported native evidence must validate: $($actual.result.errors -join ' ')"
        Assert-Equal 1 @($actual.result.acceptedMeasurements).Count "Supported pair remains useful evidence"
        Assert-Equal $false $actual.result.deviceEvidenceComplete "Unsupported native path still prevents clearance"
        Assert-Equal 1 @($actual.result.missingMeasurements).Count "Unsupported path remains explicit"
    }
    foreach ($headValue in @(114, 115, 85, 86)) {
        Test-Case "native threshold at $headValue percent of baseline" {
            $case = New-EvidenceCase "threshold-$headValue"
            Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
                param($records)
                foreach ($record in @($records | Where-Object { $_.variant -eq "head" })) {
                    $record.measurementsMilliseconds = @($headValue, $headValue)
                    foreach ($property in $record.statistics.PSObject.Properties) { $property.Value = $headValue }
                }
            }
            Update-CoreSummary $case
            $actual = Invoke-EvidenceCase $case
            Assert-Accepted $actual
            $expected = switch ($headValue) {
                115 { "time-regression-advisory" }
                85 { "time-improvement-advisory" }
                default { "neutral" }
            }
            Assert-Equal $expected $actual.result.acceptedMeasurements[0].verdict "Exact deterministic 15 percent boundary"
        }
    }
    Test-Case "overlapping native ranges stay neutral" {
        $case = New-EvidenceCase "overlapping-ranges"
        Edit-FixtureJson (Join-Path $case.requests[0].resultDirectory "results.json") {
            param($records)
            foreach ($record in @($records | Where-Object { $_.variant -eq "head" })) {
                $record.measurementsMilliseconds = @(100, 140)
                $record.statistics = [PSCustomObject]@{
                    minimumMilliseconds = 100
                    maximumMilliseconds = 140
                    medianMilliseconds = 120
                    p95Milliseconds = 140
                    meanMilliseconds = 120
                }
            }
        }
        Update-CoreSummary $case
        $actual = Invoke-EvidenceCase $case
        Assert-Accepted $actual
        Assert-Equal "neutral" $actual.result.acceptedMeasurements[0].verdict "Overlap blocks a timing advisory despite a 20 percent median increase"
        Assert-Equal $false $actual.result.acceptedMeasurements[0].rangesDoNotOverlap "Exact overlap condition"
    }

    Write-Host "Local device evidence cases: $passed passed; $($failures.Count) failed."
    if ($failures.Count) {
        throw ($failures -join [Environment]::NewLine)
    }
    Write-Host "All local device evidence validation tests passed."
} finally {
    Remove-Item -LiteralPath $testRoot -Recurse -Force
}
