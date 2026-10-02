#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$PrMetadataPath,

    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$PolicyPath,

    [Parameter(Mandatory = $true)]
    [string]$DecisionBaselinePath,

    [Parameter(Mandatory = $true)]
    [string]$NarrativePath,

    [Parameter(Mandatory = $false)]
    [string]$SummaryPath,

    [Parameter(Mandatory = $false)]
    [string]$TablePath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath,

    [ValidateSet("Auto", "Full", "Concise")]
    [string]$Profile = "Auto"
)

$ErrorActionPreference = "Stop"

function Get-PropertyValue($object, [string]$name, $defaultValue = $null) {
    if ($null -eq $object) {
        return $defaultValue
    }

    $property = $object.PSObject.Properties[$name]
    if ($null -eq $property -or $null -eq $property.Value) {
        return $defaultValue
    }

    return $property.Value
}

function Read-OptionalJson([string]$path) {
    if ($path -and (Test-Path $path)) {
        return Get-Content $path -Raw | ConvertFrom-Json
    }
    return $null
}

function ConvertTo-SafeNarrativeText($value) {
    $text = [string]$value
    $text = $text.Replace("<!--", "[comment-open]").Replace("-->", "[comment-close]")
    $text = $text.Replace("<", "&lt;").Replace(">", "&gt;")
    $lines = @(
        $text -split "`r?`n" | ForEach-Object {
            if ($_ -match '^\s*\*\*Verdict:\*\*') {
                $_ -replace '\*\*Verdict:\*\*', '**Narrative verdict:**'
            }
            elseif ($_ -match '^\s*#{1,6}\s+') {
                $_ -replace '^\s*#{1,6}\s+', ''
            }
            elseif ($_ -match '^\s*(?:`{3,}|~{3,})') {
                $_.Replace('`', '\`').Replace('~', '\~')
            }
            else {
                $_
            }
        }
    )
    return $lines -join [Environment]::NewLine
}

function New-StatusChip([string]$label, [string]$value, [string]$color) {
    $labelSegment = [uri]::EscapeDataString($label).Replace("-", "--").Replace("_", "__")
    $valueSegment = [uri]::EscapeDataString($value).Replace("-", "--").Replace("_", "__")
    $alt = [System.Net.WebUtility]::HtmlEncode("$label $value")
    return "  <img alt=`"$alt`" src=`"https://img.shields.io/badge/$labelSegment-$valueSegment-$color`?labelColor=30363d&amp;style=flat-square`">"
}

foreach ($path in @($PrMetadataPath, $SelectionPath, $PolicyPath, $DecisionBaselinePath, $NarrativePath)) {
    if (-not (Test-Path $path)) {
        throw "Required report input does not exist: $path"
    }
}

$pr = Get-Content $PrMetadataPath -Raw | ConvertFrom-Json
if ($pr.repository -cnotmatch '^[A-Za-z0-9_.-]+/[A-Za-z0-9_.-]+$' -or
    $pr.headRefOid -cnotmatch '^[0-9a-f]{40}$' -or
    $pr.author -cnotmatch '^[A-Za-z0-9][A-Za-z0-9-]*(?:\[bot\])?$') {
    throw "Report metadata requires a valid repository, full commit SHA, and actual PR author."
}
$shortSha = $pr.headRefOid.Substring(0, 12)
$selection = Get-Content $SelectionPath -Raw | ConvertFrom-Json
$policy = Get-Content $PolicyPath -Raw | ConvertFrom-Json
$baseline = Get-Content $DecisionBaselinePath -Raw | ConvertFrom-Json
$narrative = Get-Content $NarrativePath -Raw | ConvertFrom-Json
$summary = Read-OptionalJson $SummaryPath
$hasEmpiricalEvidence = $null -ne $summary
$fullProfile = if ($Profile -eq "Auto") { $hasEmpiricalEvidence } else { $Profile -eq "Full" }

$staticSeverity = [string](Get-PropertyValue $narrative "staticFindingSeverity" "none")
if ($staticSeverity -notin @("none", "warning", "error")) {
    $staticSeverity = "none"
}

$verdictClass = [string]$baseline.verdictClass
$confidence = [string]$baseline.confidence
$nextAction = [string]$baseline.nextAction
if ($staticSeverity -eq "error" -and [bool]$baseline.allowStaticErrorEscalation) {
    $verdictClass = "blocker"
    $confidence = "low"
    $nextAction = "optimize_before_merge"
}
elseif ($staticSeverity -eq "warning" -and [bool]$baseline.allowStaticWarningEscalation) {
    $confidence = "low"
    $nextAction = "needs_human_discussion"
}

$assessment = switch ($nextAction) {
    "no_concerns" { "not-applicable" }
    "no_perf_action_needed" { "not-applicable" }
    "optimize_before_merge" { "not-applicable" }
    default { "unclear" }
}

$recommendations = @(
    Get-PropertyValue $narrative "recommendations" @() |
        Where-Object {
            $null -ne $_ -and
            -not [string]::IsNullOrWhiteSpace([string](Get-PropertyValue $_ "text" "")) -and
            -not [string]::IsNullOrWhiteSpace([string](Get-PropertyValue $_ "evidence" "")) -and
            -not [string]::IsNullOrWhiteSpace([string](Get-PropertyValue $_ "expectedDirection" "")) -and
            -not [string]::IsNullOrWhiteSpace([string](Get-PropertyValue $_ "risk" "")) -and
            [string](Get-PropertyValue $_ "status" "") -in @($policy.recommendationEvidence)
        } |
        ForEach-Object {
            [PSCustomObject][ordered]@{
                text = ConvertTo-SafeNarrativeText $_.text
                evidence = ConvertTo-SafeNarrativeText $_.evidence
                expectedDirection = ConvertTo-SafeNarrativeText $_.expectedDirection
                risk = ConvertTo-SafeNarrativeText $_.risk
                status = [string]$_.status
                testedHere = [bool](Get-PropertyValue $_ "testedHere" $false)
            }
        } |
        Select-Object -First ([int]$policy.limits.maxRecommendations)
)
$workaround = Get-PropertyValue $narrative "workaround" $null
$workaroundStatus = [string](Get-PropertyValue $workaround "status" "none")
if ($workaroundStatus -notin @($policy.workaroundStatuses)) {
    $workaroundStatus = "none"
}
$workaroundText = ConvertTo-SafeNarrativeText (Get-PropertyValue $workaround "text" "No evidence-backed workaround identified.")

$costAttribution = [string](Get-PropertyValue $narrative "costAttribution" "unknown")
if ($costAttribution -notin @($policy.costAttributions)) {
    $costAttribution = "unknown"
}

$decision = [ordered]@{
    schemaVersion = 2
    verdictClass = $verdictClass
    assessment = $assessment
    confidence = $confidence
    costAttribution = $costAttribution
    correctnessBenefitEstablished = [bool](Get-PropertyValue $narrative "correctnessBenefitEstablished" $false)
    testedAlternativeAvailable = [bool](Get-PropertyValue $narrative "testedAlternativeAvailable" $false)
    staticFindingSeverity = $staticSeverity
    workaround = [ordered]@{
        status = $workaroundStatus
    }
    nextAction = $nextAction
    issueDisposition = "human-only"
    recommendations = $recommendations
}

$verdictPolicy = @(
    $policy.reportVerdicts | Where-Object { $_.id -eq $verdictClass }
)[0]
$verdictLabel = $verdictPolicy.label
if ([string]::IsNullOrWhiteSpace([string]$verdictLabel) -or
    [string]::IsNullOrWhiteSpace([string]$verdictPolicy.badgeLabel) -or
    $verdictPolicy.badgeColor -cnotmatch '^[0-9a-f]{6}$') {
    throw "No valid policy label and badge exist for verdict '$verdictClass'."
}

$lines = New-Object System.Collections.Generic.List[string]
$lines.Add("## Performance Review Summary")
$lines.Add("")
$lines.Add("> @$($pr.author) &#x2014; performance review for commit [``$shortSha``](https://github.com/$($pr.repository)/commit/$($pr.headRefOid)).")
$lines.Add("")
$lines.Add('<p align="left">')
$scope = if ($hasEmpiricalEvidence) { "Managed benchmarks" } else { "Static review only" }
$lines.Add((New-StatusChip "Scope" $scope "1f6feb"))
$lines.Add((New-StatusChip "Result" $verdictPolicy.badgeLabel $verdictPolicy.badgeColor))
$lines.Add((New-StatusChip "Commit" $shortSha "1f6feb"))
$lines.Add("</p>")
$lines.Add("")
$lines.Add("---")
$lines.Add("")
$lines.Add("<details>")
$lines.Add("<summary><strong>Performance Results</strong></summary>")
$lines.Add("")
$lines.Add("**Verdict:** $verdictLabel")
$lines.Add("")
$summaryText = ConvertTo-SafeNarrativeText (Get-PropertyValue $narrative "summary" "")
if ($summaryText) {
    $lines.Add($summaryText)
    $lines.Add("")
}

$coverage = $selection.coverage
$lines.Add("### Coverage")
$lines.Add("")
if ($null -ne (Get-PropertyValue $coverage "productFileCount")) {
    $lines.Add(
        "Product files: $($coverage.productFileCount); directly managed-measured: $($coverage.managedMeasuredFileCount); " +
        "device-required: $($coverage.deviceRequiredFileCount); static-reviewed: $($coverage.staticOnlyFileCount). " +
        "Sampled subsets: managed $($coverage.managedSampledFileCount), device $([int](Get-PropertyValue $coverage 'deviceSampledFileCount' 0)).")
}
else {
    $lines.Add("Coverage status: ``$([string](Get-PropertyValue $coverage "status" "unknown"))``.")
}

$deviceScenarios = @($selection.deviceScenarios | Where-Object { $null -ne $_ })
if ($deviceScenarios.Count -gt 0) {
    $lines.Add("")
    $lines.Add("> Device measurement required: the supplied evidence does not cover the changed native handler path, so the whole PR cannot receive a clean performance verdict.")
    foreach ($scenario in $deviceScenarios) {
        $coverageMode = [string](Get-PropertyValue $scenario "coverageMode" "direct")
        $lines.Add("")
        $lines.Add("- ``$([string]$scenario.id)`` on $(@($scenario.platforms) -join ", "): $coverageMode coverage; ``$([string]$scenario.automationStatus)``.")
        $lines.Add("  Operation: $(@($scenario.operations) -join " ")")
        $lines.Add("  Correctness: $([string](Get-PropertyValue $scenario "rationale" "Scenario-specific correctness validation is required."))")
    }
}

if ($TablePath -and (Test-Path $TablePath)) {
    $table = (ConvertTo-SafeNarrativeText (Get-Content $TablePath -Raw)).Trim()
    if ($table) {
        $lines.Add("")
        $lines.Add($table)
    }
}

$lines.Add("")
$lines.Add("</details>")
$lines.Add("")
$lines.Add("---")
$lines.Add("")
$lines.Add("<details>")
$lines.Add("<summary><strong>Findings &amp; Follow-up</strong></summary>")
$lines.Add("")
$staticReview = ConvertTo-SafeNarrativeText (Get-PropertyValue $narrative "staticReview" "No error-level static hot-path finding was identified.")
$lines.Add("### Static hot-path review")
$lines.Add("")
$lines.Add($staticReview)
$lines.Add("")

if ($fullProfile) {
    $lines.Add("### Tradeoff assessment")
    $lines.Add("")
    $lines.Add("Assessment: ``$assessment``. Cost attribution: ``$($decision.costAttribution)``. Confidence: ``$confidence``.")
    $tradeoffText = ConvertTo-SafeNarrativeText (Get-PropertyValue $narrative "tradeoffAssessment" "")
    if ($tradeoffText) {
        $lines.Add("")
        $lines.Add($tradeoffText)
    }
    $lines.Add("")

    $lines.Add("### Performance recommendations")
    $lines.Add("")
    if ($recommendations.Count -eq 0) {
        $lines.Add("No evidence-backed optimization identified.")
    }
    else {
        foreach ($recommendation in $recommendations) {
            $lines.Add("- $([string]$recommendation.text) Evidence source: $([string]$recommendation.evidence); status: ``$([string]$recommendation.status)``; expected direction: $([string]$recommendation.expectedDirection); risk: $([string]$recommendation.risk); tested here: $([bool]$recommendation.testedHere).")
        }
    }
    $lines.Add("")

    $lines.Add("### Possible workaround")
    $lines.Add("")
    $lines.Add("$workaroundText Status: ``$workaroundStatus``.")
    $lines.Add("")
}

$lines.Add("### Recommended next action")
$lines.Add("")
$lines.Add("``$nextAction``")
$nextActionText = ConvertTo-SafeNarrativeText (Get-PropertyValue $narrative "nextActionContext" "")
if ($nextActionText) {
    $lines.Add("")
    $lines.Add($nextActionText)
}
$lines.Add("")

$lines.Add("> Maintainers: comment ``/review performance`` to refresh this report.")
$lines.Add("")
$lines.Add("> Automated analysis by the **perf-analysis** skill.")
$lines.Add("")
$lines.Add("</details>")
$lines.Add("")
$decisionJson = ConvertTo-Json -InputObject $decision -Depth 12 -Compress
$lines.Add("<!-- perf-analysis-decision: $decisionJson -->")

$directory = Split-Path -Parent $OutputPath
if ($directory -and -not (Test-Path $directory)) {
    New-Item -ItemType Directory -Force -Path $directory | Out-Null
}
[IO.File]::WriteAllLines($OutputPath, $lines, [Text.UTF8Encoding]::new($false))
Write-Host "Rendered performance report: $verdictClass / $Profile"
