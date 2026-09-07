#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$skillRoot = Split-Path -Parent $PSScriptRoot
$validator = [IO.Path]::Combine($skillRoot, "scripts", "Validate-PerformanceReport.ps1")
$policy = [IO.Path]::Combine($skillRoot, "references", "recommendation-policy.json")
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-perf-report-validator-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Write-Json([string]$path, $value) {
    ConvertTo-Json -InputObject $value -Depth 12 | Set-Content $path -Encoding UTF8
}

function New-Selection(
    [bool]$wholePrClean,
    [string]$deviceStatus = "",
    [int]$staticCount = 0,
    [int]$sampledCount = 0
) {
    $deviceScenarios = @()
    if ($deviceStatus) {
        $deviceScenarios = @([PSCustomObject]@{
            id = "scenario"
            automationStatus = $deviceStatus
        })
    }

    return [PSCustomObject]@{
        coverage = [PSCustomObject]@{
            canClaimWholePrClean = $wholePrClean
        }
        deviceScenarios = $deviceScenarios
        sampledProductFiles = if ($sampledCount -gt 0) { @(1..$sampledCount) } else { @() }
        staticOnlyProductFiles = if ($staticCount -gt 0) { @(1..$staticCount) } else { @() }
    }
}

function New-Summary(
    [string]$verdict = "neutral",
    [bool]$complete = $true,
    [bool]$clean = $true,
    [bool]$confirmedRegression = $false
) {
    return [PSCustomObject]@{
        verdict = $verdict
        coverageComplete = $complete
        executionComplete = $complete
        benchmarkDataComplete = $complete
        canClaimClean = $clean
        allocRegressions = if ($confirmedRegression) {
            @([PSCustomObject]@{ name = "Benchmark"; confirmed = $true })
        } else {
            @()
        }
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
    }
}

function New-Decision(
    [string]$assessment,
    [string]$nextAction,
    [string]$verdictClass = "inconclusive",
    [string]$costAttribution = "unknown",
    [string]$workaroundStatus = "none",
    [string]$staticSeverity = "none",
    [bool]$correctnessBenefitEstablished = $false,
    [bool]$testedAlternativeAvailable = $false
) {
    return [ordered]@{
        schemaVersion = 2
        verdictClass = $verdictClass
        assessment = $assessment
        confidence = "low"
        costAttribution = $costAttribution
        correctnessBenefitEstablished = $correctnessBenefitEstablished
        testedAlternativeAvailable = $testedAlternativeAvailable
        staticFindingSeverity = $staticSeverity
        workaround = [ordered]@{
            status = $workaroundStatus
        }
        nextAction = $nextAction
        issueDisposition = "human-only"
        recommendations = @()
    }
}

function Write-Report([string]$path, $decision, [bool]$concise = $false) {
    $decisionJson = ConvertTo-Json -InputObject $decision -Depth 10 -Compress
    $verdict = (Get-Content $policy -Raw | ConvertFrom-Json).reportVerdicts |
        Where-Object { $_.id -eq $decision.verdictClass } |
        Select-Object -ExpandProperty label -First 1
    $optionalSections = if ($concise) {
        ""
    } else {
@"
### Tradeoff assessment

test

### Performance recommendations

No evidence-backed optimization identified.

### Possible workaround

test

"@
    }
    @"
## Performance analysis

**Verdict:** $verdict

$optionalSections
### Recommended next action

test

### Coverage

test

> Automated analysis by the **perf-analysis** skill.

<!-- perf-analysis-decision: $decisionJson -->
"@ | Set-Content $path -Encoding UTF8
}

function Invoke-Validation(
    [string]$name,
    $decision,
    $selection,
    $summary,
    $deviceValidation = $null,
    [bool]$concise = $false,
    $decisionBaseline = $null
) {
    $caseRoot = Join-Path $testRoot $name
    New-Item -ItemType Directory -Force -Path $caseRoot | Out-Null
    $reportPath = Join-Path $caseRoot "report.md"
    $selectionPath = Join-Path $caseRoot "selection.json"
    $summaryPath = Join-Path $caseRoot "summary.json"
    $validationPath = Join-Path $caseRoot "validation.json"
    Write-Report $reportPath $decision $concise
    Write-Json $selectionPath $selection
    if ($null -ne $summary) {
        Write-Json $summaryPath $summary
    }

    $arguments = @{
        ReportPath = $reportPath
        PolicyPath = $policy
        SelectionPath = $selectionPath
        JsonOut = $validationPath
    }
    if ($null -ne $summary) {
        $arguments.SummaryPath = $summaryPath
    }

    if ($null -ne $deviceValidation) {
        $devicePath = Join-Path $caseRoot "device-validation.json"
        Write-Json $devicePath $deviceValidation
        $arguments.DeviceValidationPath = $devicePath
    }
    if ($null -ne $decisionBaseline) {
        $baselinePath = Join-Path $caseRoot "decision-baseline.json"
        Write-Json $baselinePath $decisionBaseline
        $arguments.DecisionBaselinePath = $baselinePath
    }

    & $validator @arguments
    $exitCode = $LASTEXITCODE
    $validation = Get-Content $validationPath -Raw | ConvertFrom-Json
    return [PSCustomObject]@{
        ExitCode = $exitCode
        Validation = $validation
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try {
    $clean = Invoke-Validation `
        "clean" `
        (New-Decision "not-applicable" "no_concerns" "clean") `
        (New-Selection $true) `
        (New-Summary)
    Assert-Equal 0 $clean.ExitCode "Complete clean report should pass"
    Assert-Equal $true $clean.Validation.valid "Complete clean validity"

    $directTimingSelection = New-Selection $true
    $directTimingSelection | Add-Member -NotePropertyName suites -NotePropertyValue @(
        [PSCustomObject]@{
            directlyCoveredFiles = @("direct.cs")
            filters = @("*Sample*")
        }
    )
    $directTimingImprovement = Invoke-Validation `
        "direct-timing-improvement" `
        (New-Decision "unclear" "needs_human_discussion" "advisory") `
        $directTimingSelection `
        (New-Summary "time-improvement-advisory" $true $true)
    Assert-Equal 0 $directTimingImprovement.ExitCode "Direct timing improvement must remain advisory"

    $advisoryAccept = Invoke-Validation `
        "advisory-accept" `
        (New-Decision "likely-worth-it" "accept_tradeoff" "advisory" "deliberate") `
        (New-Selection $true) `
        (New-Summary "time-regression-advisory" $true $false)
    Assert-Equal 2 $advisoryAccept.ExitCode "Advisory evidence must not accept tradeoff"

    foreach ($acceptanceAction in @("accept_tradeoff", "accept_with_followup")) {
        $unmeasuredAcceptance = Invoke-Validation `
            "unmeasured-$acceptanceAction" `
            (New-Decision "likely-worth-it" $acceptanceAction "blocker" "deliberate" "none" "none" $true) `
            (New-Selection $true) `
            (New-Summary)
        Assert-Equal 2 $unmeasuredAcceptance.ExitCode "$acceptanceAction requires a measured cost"

        $measuredAcceptance = Invoke-Validation `
            "measured-$acceptanceAction" `
            (New-Decision "likely-worth-it" $acceptanceAction "blocker" "deliberate" "none" "none" $true) `
            (New-Selection $true) `
            (New-Summary "alloc-regression" $true $false $true)
        Assert-Equal 0 $measuredAcceptance.ExitCode "$acceptanceAction should accept sealed measured cost"
    }

    $partialNoConcerns = Invoke-Validation `
        "partial-no-concerns" `
        (New-Decision "not-applicable" "no_concerns" "no-blocker-incomplete") `
        (New-Selection $false "manual-device-ci-ready") `
        (New-Summary "inconclusive" $false $false)
    Assert-Equal 2 $partialNoConcerns.ExitCode "Partial coverage must not report no concerns"

    $confirmedRegression = Invoke-Validation `
        "confirmed-regression" `
        (New-Decision "not-applicable" "optimize_before_merge" "blocker" "accidental") `
        (New-Selection $false "manual-device-ci-ready") `
        (New-Summary "alloc-regression" $true $false $true)
    Assert-Equal 0 $confirmedRegression.ExitCode "Confirmed regression should override coverage gap"

    $confirmedRegressionDeferred = Invoke-Validation `
        "confirmed-regression-deferred" `
        (New-Decision "unclear" "run_more_measurements" "blocker" "accidental") `
        (New-Selection $false "manual-device-ci-ready") `
        (New-Summary "alloc-regression" $true $false $true)
    Assert-Equal 2 $confirmedRegressionDeferred.ExitCode "Confirmed accidental regression must not be deferred"

    $supportedMissing = Invoke-Validation `
        "supported-missing" `
        (New-Decision "unclear" "run_more_measurements" "device-required") `
        (New-Selection $false "manual-device-ci-ready") `
        (New-Summary "inconclusive" $false $false)
    Assert-Equal 0 $supportedMissing.ExitCode "Supported missing evidence should request measurement"

    $unsupportedMissing = Invoke-Validation `
        "unsupported-missing" `
        (New-Decision "unclear" "needs_human_discussion" "device-required") `
        (New-Selection $false "required-not-yet-automated") `
        (New-Summary "inconclusive" $false $false)
    Assert-Equal 0 $unsupportedMissing.ExitCode "Unsupported evidence should request discussion"

    $unsupportedValidatedWorkaround = Invoke-Validation `
        "unsupported-validated-workaround" `
        (New-Decision "likely-not-worth-it" "prefer_validated_workaround" "blocker" "deliberate" "validated") `
        (New-Selection $true) `
        (New-Summary "alloc-regression" $true $false $true)
    Assert-Equal 2 $unsupportedValidatedWorkaround.ExitCode "Unsupported validated workaround decisions must fail"

    $deviceOnlySelection = [PSCustomObject]@{
        coverage = [PSCustomObject]@{
            productFileCount = 1
            managedMeasuredFileCount = 0
            managedSampledFileCount = 0
            deviceRequiredFileCount = 1
            staticOnlyFileCount = 0
            canClaimWholePrClean = $false
        }
        deviceScenarios = @([PSCustomObject]@{
            id = "scenario"
            resultScenario = "scenario-result"
            automationStatus = "manual-device-ci-ready"
        })
        sampledProductFiles = @()
        staticOnlyProductFiles = @()
    }
    $completeDeviceEvidence = [PSCustomObject]@{
        sealed = $true
        deviceEvidenceComplete = $true
        correctnessPassed = $true
        allAffectedPlatformsCovered = $true
        acceptedMeasurements = @([PSCustomObject]@{
            resultScenario = "scenario-result"
            verdict = "time-regression-advisory"
        })
    }
    $deviceAdvisoryDiscussion = Invoke-Validation `
        "device-advisory-discussion" `
        (New-Decision "unclear" "needs_human_discussion" "advisory" "deliberate") `
        $deviceOnlySelection `
        (New-Summary "inconclusive" $false $false) `
        $completeDeviceEvidence
    Assert-Equal 0 $deviceAdvisoryDiscussion.ExitCode "Complete advisory device evidence should require discussion"

    $deviceAdvisoryAccept = Invoke-Validation `
        "device-advisory-accept" `
        (New-Decision "likely-worth-it" "accept_tradeoff" "advisory" "deliberate" "none" "none" $true) `
        $deviceOnlySelection `
        (New-Summary "inconclusive" $false $false) `
        $completeDeviceEvidence
    Assert-Equal 2 $deviceAdvisoryAccept.ExitCode "Advisory device evidence must not accept a tradeoff"

    $mixedSelection = [PSCustomObject]@{
        coverage = [PSCustomObject]@{
            productFileCount = 2
            managedMeasuredFileCount = 1
            managedSampledFileCount = 0
            deviceRequiredFileCount = 1
            staticOnlyFileCount = 0
            canClaimWholePrClean = $false
        }
        deviceScenarios = $deviceOnlySelection.deviceScenarios
        sampledProductFiles = @()
        staticOnlyProductFiles = @()
    }
    $deviceAdvisoryNoConcerns = Invoke-Validation `
        "device-advisory-no-concerns" `
        (New-Decision "not-applicable" "no_concerns" "advisory") `
        $mixedSelection `
        (New-Summary) `
        $completeDeviceEvidence
    Assert-Equal 2 $deviceAdvisoryNoConcerns.ExitCode "Device advisory evidence must block no_concerns"

    $cleanDeviceEvidence = [PSCustomObject]@{
        sealed = $true
        deviceEvidenceComplete = $true
        correctnessPassed = $true
        allAffectedPlatformsCovered = $true
        acceptedMeasurements = @([PSCustomObject]@{
            resultScenario = "scenario-result"
            verdict = "neutral"
        })
    }
    $deviceOnlyNoConcerns = Invoke-Validation `
        "device-only-no-concerns" `
        (New-Decision "not-applicable" "no_concerns" "clean") `
        $deviceOnlySelection `
        (New-Summary "inconclusive" $false $false) `
        $cleanDeviceEvidence
    Assert-Equal 0 $deviceOnlyNoConcerns.ExitCode "Clean device-only evidence should allow no_concerns"

    $staticOnlyConcise = Invoke-Validation `
        "static-only-concise" `
        (New-Decision "unclear" "needs_human_discussion" "no-blocker-incomplete") `
        (New-Selection $false "" 1) `
        $null `
        $null `
        $true
    Assert-Equal 0 $staticOnlyConcise.ExitCode "Static-only evidence should allow the concise report profile"

    $empiricalConcise = Invoke-Validation `
        "empirical-concise" `
        (New-Decision "not-applicable" "no_concerns" "clean") `
        (New-Selection $true) `
        (New-Summary) `
        $null `
        $true
    Assert-Equal 2 $empiricalConcise.ExitCode "Empirical evidence must require the full report profile"

    $falseBlocker = Invoke-Validation `
        "false-blocker" `
        (New-Decision "not-applicable" "optimize_before_merge" "blocker" "accidental") `
        (New-Selection $true) `
        (New-Summary)
    Assert-Equal 2 $falseBlocker.ExitCode "Blocker verdict requires confirmed blocking evidence"

    $falseIncompleteDecision = New-Decision "not-applicable" "no_concerns" "no-blocker-incomplete"
    Assert-Equal "no-blocker-incomplete" $falseIncompleteDecision.verdictClass "Incomplete test verdict setup"
    $falseIncompleteSelection = New-Selection $true
    Assert-Equal $true $falseIncompleteSelection.coverage.canClaimWholePrClean "Incomplete test coverage setup"
    $falseIncomplete = Invoke-Validation `
        "false-incomplete" `
        $falseIncompleteDecision `
        $falseIncompleteSelection `
        (New-Summary)
    Assert-Equal 2 $falseIncomplete.ExitCode "Incomplete verdict must not describe complete clean evidence"

    $baseline = [PSCustomObject]@{
        schemaVersion = 1
        verdictClass = "no-blocker-incomplete"
        confidence = "low"
        nextAction = "needs_human_discussion"
        issueDisposition = "human-only"
        allowStaticErrorEscalation = $true
    }
    $matchingBaseline = Invoke-Validation `
        "matching-baseline" `
        (New-Decision "unclear" "needs_human_discussion" "no-blocker-incomplete") `
        (New-Selection $false "" 1) `
        $null `
        $null `
        $true `
        $baseline
    Assert-Equal 0 $matchingBaseline.ExitCode "Report should match the sealed decision baseline"

    $mismatchedBaseline = Invoke-Validation `
        "mismatched-baseline" `
        (New-Decision "unclear" "run_more_measurements" "no-blocker-incomplete") `
        (New-Selection $false "" 1) `
        $null `
        $null `
        $true `
        $baseline
    Assert-Equal 2 $mismatchedBaseline.ExitCode "Decision metadata must match the sealed baseline"

    $staticEscalationDecision = New-Decision `
        "not-applicable" `
        "optimize_before_merge" `
        "blocker" `
        "accidental" `
        "none" `
        "error"
    $staticEscalation = Invoke-Validation `
        "static-escalation" `
        $staticEscalationDecision `
        (New-Selection $false "" 1) `
        $null `
        $null `
        $true `
        $baseline
    Assert-Equal 0 $staticEscalation.ExitCode "Error-level static review may escalate the sealed baseline"

    Write-Host "All performance report validator tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
