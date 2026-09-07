#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Validates an AI-generated performance report against trusted caller-supplied evidence.

.DESCRIPTION
    The report must contain a `perf-analysis-decision` JSON metadata comment. This
    script validates its schema, recommendation limits, evidence labels, workaround
    claims, and next-action gates against selection/benchmark evidence.
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$ReportPath,

    [Parameter(Mandatory = $true)]
    [string]$PolicyPath,

    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $false)]
    [string]$SummaryPath,

    [Parameter(Mandatory = $false)]
    [string]$DeviceValidationPath,

    [Parameter(Mandatory = $false)]
    [string]$DecisionBaselinePath,

    [Parameter(Mandatory = $false)]
    [string]$JsonOut
)

$ErrorActionPreference = "Stop"
$errors = New-Object System.Collections.Generic.List[string]

function Add-ValidationError([string]$message) {
    $errors.Add($message)
}

function Test-AllowedValue($value, $allowedValues, [string]$fieldName) {
    if ([string]::IsNullOrWhiteSpace([string]$value) -or $value -notin @($allowedValues)) {
        Add-ValidationError "'$fieldName' must be one of: $(@($allowedValues) -join ', ')."
        return $false
    }

    return $true
}

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

foreach ($path in @($ReportPath, $PolicyPath, $SelectionPath)) {
    if (-not (Test-Path $path)) {
        throw "Required validation input does not exist: $path"
    }
    if ($DecisionBaselinePath -and -not (Test-Path $DecisionBaselinePath)) {
        throw "Required decision baseline does not exist: $DecisionBaselinePath"
    }
}

$report = Get-Content $ReportPath -Raw
$policy = Get-Content $PolicyPath -Raw | ConvertFrom-Json
$selection = Get-Content $SelectionPath -Raw | ConvertFrom-Json
$summary = if ($SummaryPath -and (Test-Path $SummaryPath)) {
    Get-Content $SummaryPath -Raw | ConvertFrom-Json
} else {
    $null
}
$deviceValidation = if ($DeviceValidationPath -and (Test-Path $DeviceValidationPath)) {
    Get-Content $DeviceValidationPath -Raw | ConvertFrom-Json
} else {
    $null
}
$decisionBaseline = if ($DecisionBaselinePath -and (Test-Path $DecisionBaselinePath)) {
    Get-Content $DecisionBaselinePath -Raw | ConvertFrom-Json
} else {
    $null
}

$hasEmpiricalEvidence = $null -ne $summary -or (
    $null -ne $deviceValidation -and
    [bool](Get-PropertyValue $deviceValidation "sealed")
)

$requiredHeadings = @(
    "## Performance analysis",
    "### Recommended next action",
    "### Coverage"
)

if ($hasEmpiricalEvidence) {
    $requiredHeadings += @(
        "### Tradeoff assessment",
        "### Performance recommendations",
        "### Possible workaround"
    )
}

foreach ($heading in $requiredHeadings) {
    if ($report -notmatch "(?m)^$([regex]::Escape($heading))\s*$") {
        Add-ValidationError "Required heading is missing: $heading"
    }
}

if ($report -notmatch '(?i)automated analysis by the \*\*perf-analysis\*\* skill') {
    Add-ValidationError "AI/skill attribution is missing."
}

$decisionMatches = [regex]::Matches(
    $report,
    '<!--\s*perf-analysis-decision:\s*(\{.*\})\s*-->',
    [System.Text.RegularExpressions.RegexOptions]::Singleline)

if ($decisionMatches.Count -ne 1) {
    Add-ValidationError "Report must contain exactly one perf-analysis-decision metadata comment."
    $decision = $null
} else {
    try {
        $decision = $decisionMatches[0].Groups[1].Value | ConvertFrom-Json
    } catch {
        Add-ValidationError "perf-analysis-decision metadata is invalid JSON: $($_.Exception.Message)"
        $decision = $null
    }
}

if ($null -ne $decision) {
    if ([int](Get-PropertyValue $decision "schemaVersion") -ne 2) {
        Add-ValidationError "Decision schemaVersion must be 2."
    }

    $verdictClass = Get-PropertyValue $decision "verdictClass"
    [void](Test-AllowedValue $verdictClass @($policy.reportVerdicts.id) "verdictClass")
    [void](Test-AllowedValue (Get-PropertyValue $decision "assessment") $policy.tradeoffAssessments "assessment")
    [void](Test-AllowedValue (Get-PropertyValue $decision "confidence") $policy.confidenceLevels "confidence")
    [void](Test-AllowedValue (Get-PropertyValue $decision "costAttribution") $policy.costAttributions "costAttribution")
    [void](Test-AllowedValue (Get-PropertyValue $decision "nextAction") @($policy.nextActions.id) "nextAction")
    [void](Test-AllowedValue (Get-PropertyValue $decision.workaround "status") $policy.workaroundStatuses "workaround.status")

    if ((Get-PropertyValue $decision "issueDisposition") -ne "human-only") {
        Add-ValidationError "issueDisposition must be 'human-only'."
    }

    $correctnessBenefitEstablished = Get-PropertyValue $decision "correctnessBenefitEstablished"
    if ($correctnessBenefitEstablished -isnot [bool]) {
        Add-ValidationError "correctnessBenefitEstablished must be a boolean."
    }
    $testedAlternativeAvailable = Get-PropertyValue $decision "testedAlternativeAvailable"
    if ($testedAlternativeAvailable -isnot [bool]) {
        Add-ValidationError "testedAlternativeAvailable must be a boolean."
    }

    $staticFindingSeverity = Get-PropertyValue $decision "staticFindingSeverity"
    if ($staticFindingSeverity -notin @("none", "warning", "error")) {
        Add-ValidationError "staticFindingSeverity must be none, warning, or error."
    }

    if ($null -ne $decisionBaseline) {
        if ([int](Get-PropertyValue $decisionBaseline "schemaVersion") -ne 1) {
            Add-ValidationError "Decision baseline schemaVersion must be 1."
        }

        $staticEscalation = $staticFindingSeverity -eq "error" `
            -and [bool](Get-PropertyValue $decisionBaseline "allowStaticErrorEscalation")
        $staticWarningEscalation = $staticFindingSeverity -eq "warning" `
            -and [bool](Get-PropertyValue $decisionBaseline "allowStaticWarningEscalation")
        if ($staticEscalation) {
            if ($verdictClass -ne "blocker" -or [string]$decision.nextAction -ne "optimize_before_merge") {
                Add-ValidationError "Error-level static findings must escalate to blocker / optimize_before_merge."
            }
            if ([string]$decision.confidence -ne "low") {
                Add-ValidationError "Static-only blocker escalation must use low confidence."
            }
        }
        elseif ($staticWarningEscalation) {
            if ($verdictClass -ne [string]$decisionBaseline.verdictClass -or
                [string]$decision.nextAction -ne "needs_human_discussion") {
                Add-ValidationError "Warning-level static findings must retain the baseline verdict and escalate to needs_human_discussion."
            }
            if ([string]$decision.confidence -ne "low") {
                Add-ValidationError "Static warning escalation must use low confidence."
            }
        }
        else {
            foreach ($field in @("verdictClass", "confidence", "nextAction", "issueDisposition")) {
                if ([string](Get-PropertyValue $decision $field) -ne [string](Get-PropertyValue $decisionBaseline $field)) {
                    Add-ValidationError "Decision field '$field' must match the sealed baseline."
                }
            }
        }
    }

    $recommendations = @($decision.recommendations | Where-Object { $null -ne $_ })
    if ($recommendations.Count -gt [int]$policy.limits.maxRecommendations) {
        Add-ValidationError "At most $($policy.limits.maxRecommendations) recommendations are allowed."
    }

    for ($index = 0; $index -lt $recommendations.Count; $index++) {
        $recommendation = $recommendations[$index]
        $prefix = "recommendations[$index]"

        foreach ($field in @("text", "evidence", "expectedDirection", "risk")) {
            if ([string]::IsNullOrWhiteSpace([string](Get-PropertyValue $recommendation $field))) {
                Add-ValidationError "$prefix.$field is required."
            }
        }

        [void](Test-AllowedValue (Get-PropertyValue $recommendation "status") $policy.recommendationEvidence "$prefix.status")

        if ((Get-PropertyValue $recommendation "testedHere") -isnot [bool]) {
            Add-ValidationError "$prefix.testedHere must be a boolean."
        }
    }

    if ($hasEmpiricalEvidence -and $recommendations.Count -eq 0 -and $report -notmatch '(?i)No evidence-backed optimization identified\.') {
        Add-ValidationError "An empty recommendation list requires the explicit no-optimization statement."
    }

    $verdictPolicy = @($policy.reportVerdicts | Where-Object { $_.id -eq $verdictClass }) | Select-Object -First 1
    if ($null -ne $verdictPolicy) {
        $expectedVerdict = [string]$verdictPolicy.label
        if ($report -notmatch "(?m)^\*\*Verdict:\*\*\s+$([regex]::Escape($expectedVerdict))\s*$") {
            Add-ValidationError "Verdict text must match verdictClass '$verdictClass': $expectedVerdict"
        }
    }

    $assessment = [string](Get-PropertyValue $decision "assessment")
    $nextAction = [string](Get-PropertyValue $decision "nextAction")
    $actionPolicy = @($policy.nextActions | Where-Object { $_.id -eq $nextAction }) | Select-Object -First 1
    if ($null -ne $actionPolicy -and $assessment -notin @($actionPolicy.allowedAssessments)) {
        Add-ValidationError "Action '$nextAction' is incompatible with assessment '$assessment'."
    }

    $coverage = $selection.coverage
    $deviceScenarios = @($selection.deviceScenarios | Where-Object { $null -ne $_ })
    $sampledProductFiles = @($selection.sampledProductFiles | Where-Object { $null -ne $_ })
    $staticOnlyProductFiles = @($selection.staticOnlyProductFiles | Where-Object { $null -ne $_ })
    $unsupportedDevicePath = @(
        $deviceScenarios | Where-Object { $_.automationStatus -eq "required-not-yet-automated" }
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
    $managedCount = $null
    $deviceCount = $null
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
    } else {
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
    } else {
        $summaryComplete -and [bool](Get-PropertyValue $summary "canClaimClean")
    }
    $deviceEvidenceClean = if ($null -ne $deviceCount) {
        $deviceCount -eq 0 -or ($deviceEvidenceComplete -and -not $deviceAdvisory)
    } else {
        $deviceScenarios.Count -eq 0 -or ($deviceEvidenceComplete -and -not $deviceAdvisory)
    }
    $confirmedAllocationRegression = $null -ne $summary -and @(
        $summary.allocRegressions | Where-Object { $_.confirmed -eq $true }
    ).Count -gt 0
    $confirmedBlockingRegression = $confirmedAllocationRegression -or $staticFindingSeverity -eq "error"
    $confirmedMeasuredCost = $confirmedAllocationRegression
    $hasMeasuredImprovement = $wholePrEvidenceComplete -and (
        ($null -ne $summary -and [string]$summary.verdict -eq "improvement") -or
        ($deviceEvidenceComplete -and @(
            $deviceValidation.acceptedMeasurements |
                Where-Object { $_.verdict -eq "improvement" }
        ).Count -gt 0)
    )

    $supportedMeasurementPath = @(
        $deviceScenarios | Where-Object { $_.automationStatus -eq "manual-device-ci-ready" }
    ).Count -gt 0
    $hasCoverageGap = -not $wholePrEvidenceComplete `
        -or [bool](Get-PropertyValue $coverage "benchmarkInputsChanged") `
        -or $sampledProductFiles.Count -gt 0 `
        -or $staticOnlyProductFiles.Count -gt 0 `
        -or ($deviceScenarios.Count -gt 0 -and -not $deviceEvidenceComplete)

    if ($verdictClass -eq "blocker" -and -not $confirmedBlockingRegression) {
        Add-ValidationError "verdictClass 'blocker' requires a confirmed regression or error-level static finding."
    }
    elseif ($verdictClass -eq "advisory" -and (-not $advisoryOnly -or $confirmedBlockingRegression)) {
        Add-ValidationError "verdictClass 'advisory' requires advisory-only evidence and no confirmed blocking regression."
    }
    elseif ($verdictClass -eq "improvement" -and -not $hasMeasuredImprovement) {
        Add-ValidationError "verdictClass 'improvement' requires complete whole-PR measured improvement evidence."
    }
    elseif ($verdictClass -eq "clean" -and (
        -not $wholePrEvidenceComplete `
        -or -not $managedEvidenceClean `
        -or -not $deviceEvidenceClean `
        -or $staticFindingSeverity -eq "error")) {
        Add-ValidationError "verdictClass 'clean' requires complete clean whole-PR evidence."
    }
    elseif ($verdictClass -eq "no-blocker-incomplete" -and (
        $confirmedBlockingRegression -or -not $hasCoverageGap)) {
        Add-ValidationError "verdictClass 'no-blocker-incomplete' requires incomplete coverage and no confirmed blocker."
    }
    elseif ($verdictClass -eq "device-required" -and (
        $deviceScenarios.Count -eq 0 -or $deviceEvidenceComplete -or $confirmedBlockingRegression)) {
        Add-ValidationError "verdictClass 'device-required' requires missing device evidence and no confirmed blocker."
    }
    elseif ($verdictClass -eq "inconclusive" -and (
        -not $hasCoverageGap -and $summaryComplete -and -not $advisoryOnly)) {
        Add-ValidationError "verdictClass 'inconclusive' requires incomplete or conflicting evidence."
    }

    if ($assessment -in @("likely-worth-it", "likely-not-worth-it") `
        -and (-not $wholePrEvidenceComplete -or $advisoryOnly)) {
        Add-ValidationError "A worth-it assessment requires complete non-advisory whole-PR evidence."
    }

    if ($assessment -eq "likely-worth-it" `
        -and (-not $correctnessBenefitEstablished -or $testedAlternativeAvailable)) {
        Add-ValidationError "likely-worth-it requires an established correctness benefit and no better tested alternative."
    }

    if ($assessment -eq "likely-not-worth-it" `
        -and (-not $confirmedBlockingRegression -or -not $testedAlternativeAvailable)) {
        Add-ValidationError "likely-not-worth-it requires a confirmed regression plus a tested lower-cost alternative."
    }

    if ($nextAction -in @("accept_tradeoff", "accept_with_followup") `
        -and (-not $wholePrEvidenceComplete -or $advisoryOnly)) {
        Add-ValidationError "Acceptance actions require complete non-advisory whole-PR evidence."
    }

    if ($nextAction -in @("accept_tradeoff", "accept_with_followup") -and -not $confirmedMeasuredCost) {
        Add-ValidationError "Acceptance actions require a confirmed measured cost in sealed benchmark evidence."
    }

    if ($nextAction -in @("accept_tradeoff", "accept_with_followup") `
        -and (-not $correctnessBenefitEstablished `
            -or [string]$decision.costAttribution -ne "deliberate" `
            -or $testedAlternativeAvailable)) {
        Add-ValidationError "Acceptance actions require a deliberate cost, established correctness benefit, and no better tested alternative."
    }

    if ($nextAction -eq "no_concerns" `
        -and (-not $wholePrEvidenceComplete `
            -or -not $managedEvidenceClean `
            -or -not $deviceEvidenceClean `
            -or $staticFindingSeverity -eq "error")) {
        Add-ValidationError "no_concerns requires complete clean whole-PR evidence."
    }

    if ($nextAction -eq "no_perf_action_needed" -and (
        $staticFindingSeverity -ne "none" `
        -or -not $hasCoverageGap `
        -or $confirmedBlockingRegression `
        -or $advisoryOnly `
        -or (@($selection.suites | Where-Object { $null -ne $_ }).Count -gt 0 -and
            (-not $summaryComplete -or @($summary.allocRegressions | Where-Object { $null -ne $_ }).Count -gt 0)) `
        -or ($deviceScenarios.Count -gt 0 -and -not $deviceEvidenceComplete))) {
        Add-ValidationError "no_perf_action_needed requires incomplete direct coverage, clean completed sampled evidence, and no static concern."
    }

    if ($nextAction -eq "optimize_before_merge" -and -not $confirmedBlockingRegression) {
        Add-ValidationError "optimize_before_merge requires a confirmed regression or error-level static finding."
    }

    if ($nextAction -eq "run_more_measurements") {
        if (-not $supportedMeasurementPath) {
            Add-ValidationError "run_more_measurements requires a concrete supported measurement path."
        }
        if ($confirmedBlockingRegression -and [string]$decision.costAttribution -eq "accidental") {
            Add-ValidationError "A confirmed accidental blocking regression cannot be deferred to more measurements."
        }
    }

    if ($nextAction -eq "needs_human_discussion" -and -not ($unsupportedDevicePath -or $hasCoverageGap -or $advisoryOnly -or $staticFindingSeverity -ne "none")) {
        Add-ValidationError "needs_human_discussion requires an unsupported path, coverage gap, or static concern."
    }
}

$validationResult = [PSCustomObject]@{
    schemaVersion = 1
    valid = $errors.Count -eq 0
    errors = @($errors | ForEach-Object { $_ })
}

if ($JsonOut) {
    $directory = Split-Path -Parent $JsonOut
    if ($directory -and -not (Test-Path $directory)) {
        New-Item -ItemType Directory -Force -Path $directory | Out-Null
    }

    ConvertTo-Json -InputObject $validationResult -Depth 8 |
        Set-Content -Path $JsonOut -Encoding UTF8
}

if ($errors.Count -gt 0) {
    foreach ($validationError in $errors) {
        [Console]::Error.WriteLine("ERROR: $validationError")
    }
    exit 2
}

Write-Host "Performance report validation passed."
exit 0
