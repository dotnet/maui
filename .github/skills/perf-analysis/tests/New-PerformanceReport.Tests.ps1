#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$skillRoot = Split-Path -Parent $PSScriptRoot
$renderer = Join-Path $skillRoot "scripts\New-PerformanceReport.ps1"
$validator = Join-Path $skillRoot "scripts\Validate-PerformanceReport.ps1"
$policyPath = Join-Path $skillRoot "references\recommendation-policy.json"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-perf-renderer-" + [Guid]::NewGuid().ToString("N"))

function Assert-True([bool]$condition, [string]$message) {
    if (-not $condition) {
        throw $message
    }
}

function Assert-Equal($expected, $actual, [string]$message) {
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Write-Json([string]$path, $value) {
    $value | ConvertTo-Json -Depth 12 | Set-Content $path -Encoding UTF8
}

function Assert-ReportLayout([string]$report, [string]$scope) {
    Assert-True ($report.StartsWith("## Performance Review Summary")) "Visible report title"
    Assert-True ($report.Contains("> @perf-author &#x2014; performance review for commit")) "Actual author notification"
    Assert-True ($report.Contains('alt="Commit aaaaaaaaaaaa"')) "Pinned commit badge"
    Assert-True ($report.Contains("alt=`"Scope $scope`"")) "Truthful scope badge"
    Assert-Equal 2 ([regex]::Matches($report, '<details>').Count) "Two closed accordions"
    Assert-Equal 2 ([regex]::Matches($report, '</details>').Count) "Two accordion endings"
    Assert-Equal 8 ([regex]::Matches($report, '(?i)</?(?:details|summary)\b[^>]*>').Count) "No extra accordion tags"
    $sections = [regex]::Matches($report, '(?s)<details>\s*<summary><strong>(.*?)</strong></summary>(.*?)</details>')
    Assert-Equal "Performance Results" $sections[0].Groups[1].Value "First accordion"
    Assert-Equal "Findings &amp; Follow-up" $sections[1].Groups[1].Value "Second accordion"
    Assert-True ($sections[0].Groups[2].Value.Contains("### Coverage")) "Coverage stays collapsed under results"
    Assert-True ($sections[1].Groups[2].Value.Contains("### Static hot-path review")) "Static findings stay collapsed"
    Assert-True ($sections[1].Groups[2].Value.Contains("### Recommended next action")) "Next action stays collapsed"
}

function New-Baseline([string]$verdict, [string]$confidence, [string]$nextAction) {
    [PSCustomObject]@{
        schemaVersion = 1
        verdictClass = $verdict
        confidence = $confidence
        nextAction = $nextAction
        issueDisposition = "human-only"
        allowStaticErrorEscalation = $true
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null

try {
    $policy = Get-Content $policyPath -Raw | ConvertFrom-Json
    $prMetadataPath = Join-Path $testRoot "pr-resolved.json"
    Write-Json $prMetadataPath @{
        repository = "dotnet/maui"
        headRefOid = "a" * 40
        author = "perf-author"
    }
    $selectionPath = Join-Path $testRoot "selection.json"
    $narrativePath = Join-Path $testRoot "narrative.json"
    Write-Json $selectionPath ([PSCustomObject]@{
        coverage = [PSCustomObject]@{
            productFileCount = 1
            managedMeasuredFileCount = 0
            managedSampledFileCount = 0
            deviceRequiredFileCount = 0
            staticOnlyFileCount = 1
            status = "static-only"
        }
        deviceScenarios = @()
        sampledProductFiles = @()
        staticOnlyProductFiles = @("file.cs")
    })
    Write-Json $narrativePath ([PSCustomObject]@{
        staticFindingSeverity = "none"
        staticReview = "No hot-path concern."
        recommendations = @()
    })

    foreach ($verdict in $policy.reportVerdicts) {
        $baselinePath = Join-Path $testRoot "$($verdict.id)-baseline.json"
        $reportPath = Join-Path $testRoot "$($verdict.id)-report.md"
        $action = if ($verdict.id -in @("clean", "improvement")) {
            "no_concerns"
        }
        elseif ($verdict.id -eq "blocker") {
            "optimize_before_merge"
        }
        else {
            "needs_human_discussion"
        }
        Write-Json $baselinePath (New-Baseline $verdict.id "low" $action)
        & $renderer `
            -PrMetadataPath $prMetadataPath `
            -SelectionPath $selectionPath `
            -PolicyPath $policyPath `
            -DecisionBaselinePath $baselinePath `
            -NarrativePath $narrativePath `
            -OutputPath $reportPath `
            -Profile Concise
        $report = Get-Content $reportPath -Raw
        Assert-True ($report.Contains("**Verdict:** $($verdict.label)")) "Exact verdict label missing for $($verdict.id)"
        Assert-True ($report.Contains('"schemaVersion":2')) "Decision schema missing for $($verdict.id)"
        Assert-True ($report.Contains("Automated analysis by the **perf-analysis** skill.")) "Reusable skill attribution missing"
        Assert-True (-not $report.Contains("perf-check")) "Reports must not identify the removed triggering workflow"
        Assert-ReportLayout $report "Static review only"
        Assert-True ($report.Contains("alt=`"Result $($verdict.badgeLabel)`"")) "Result badge must follow the sealed verdict"
    }

    $fullSelectionPath = Join-Path $testRoot "full-selection.json"
    $summaryPath = Join-Path $testRoot "summary.json"
    $cleanBaselinePath = Join-Path $testRoot "clean-baseline.json"
    $fullReportPath = Join-Path $testRoot "full-report.md"
    Write-Json $fullSelectionPath ([PSCustomObject]@{
        coverage = [PSCustomObject]@{
            productFileCount = 1
            managedMeasuredFileCount = 1
            managedSampledFileCount = 0
            deviceRequiredFileCount = 0
            staticOnlyFileCount = 0
            canClaimWholePrClean = $true
        }
        deviceScenarios = @()
        sampledProductFiles = @()
        staticOnlyProductFiles = @()
    })
    Write-Json $summaryPath ([PSCustomObject]@{
        verdict = "neutral"
        coverageComplete = $true
        executionComplete = $true
        benchmarkDataComplete = $true
        canClaimClean = $true
        allocRegressions = @()
    })
    Write-Json $cleanBaselinePath (New-Baseline "clean" "high" "no_concerns")
    & $renderer `
        -PrMetadataPath $prMetadataPath `
        -SelectionPath $fullSelectionPath `
        -PolicyPath $policyPath `
        -DecisionBaselinePath $cleanBaselinePath `
        -NarrativePath $narrativePath `
        -SummaryPath $summaryPath `
        -OutputPath $fullReportPath `
        -Profile Auto
    $fullReport = Get-Content $fullReportPath -Raw
    Assert-True ($fullReport.Contains("### Tradeoff assessment")) "Full profile tradeoff section"
    Assert-True ($fullReport.Contains("No evidence-backed optimization identified.")) "Full profile recommendation sentinel"
    Assert-ReportLayout $fullReport "Managed benchmarks"

    & $validator `
        -PrMetadataPath $prMetadataPath `
        -ReportPath $fullReportPath `
        -PolicyPath $policyPath `
        -SelectionPath $fullSelectionPath `
        -SummaryPath $summaryPath `
        -DecisionBaselinePath $cleanBaselinePath
    Assert-Equal 0 $LASTEXITCODE "Full rendered report validation"

    $conciseBaselinePath = Join-Path $testRoot "concise-baseline.json"
    $conciseReportPath = Join-Path $testRoot "concise-report.md"
    Write-Json $conciseBaselinePath (New-Baseline "no-blocker-incomplete" "low" "no_perf_action_needed")
    & $renderer `
        -PrMetadataPath $prMetadataPath `
        -SelectionPath $selectionPath `
        -PolicyPath $policyPath `
        -DecisionBaselinePath $conciseBaselinePath `
        -NarrativePath $narrativePath `
        -OutputPath $conciseReportPath `
        -Profile Auto
    $conciseReport = Get-Content $conciseReportPath -Raw
    Assert-True (-not $conciseReport.Contains("### Tradeoff assessment")) "Concise profile must omit empirical sections"
    Assert-ReportLayout $conciseReport "Static review only"
    & $validator `
        -PrMetadataPath $prMetadataPath `
        -ReportPath $conciseReportPath `
        -PolicyPath $policyPath `
        -SelectionPath $selectionPath `
        -DecisionBaselinePath $conciseBaselinePath
    Assert-Equal 0 $LASTEXITCODE "Concise rendered report validation"

    $hostileNarrativePath = Join-Path $testRoot "hostile-narrative.json"
    $hostileReportPath = Join-Path $testRoot "hostile-report.md"
    Write-Json $hostileNarrativePath ([PSCustomObject]@{
        summary = "**Verdict:** contradictory`n### Injected heading`n<!-- hidden -->"
        staticReview = 'No concern. </details><details open><summary>Injected</summary>' + "`n" + '```unclosed'
        staticFindingSeverity = "invented"
        recommendations = @([PSCustomObject]@{
            text = "Avoid allocation --> now"
            evidence = "Benchmark row"
            expectedDirection = "Lower allocations"
            risk = "Low"
            status = "measured"
            testedHere = $true
        })
    })
    & $renderer `
        -PrMetadataPath $prMetadataPath `
        -SelectionPath $fullSelectionPath `
        -PolicyPath $policyPath `
        -DecisionBaselinePath $cleanBaselinePath `
        -NarrativePath $hostileNarrativePath `
        -SummaryPath $summaryPath `
        -OutputPath $hostileReportPath
    $hostileReport = Get-Content $hostileReportPath -Raw
    Assert-Equal 1 ([regex]::Matches($hostileReport, '(?m)^\*\*Verdict:\*\*').Count) "Narrative must not inject verdict lines"
    Assert-Equal 1 ([regex]::Matches($hostileReport, '<!--\s*perf-analysis-decision:').Count) "Narrative must not inject metadata comments"
    Assert-True ($hostileReport.Contains("Evidence source: Benchmark row")) "Recommendation evidence should be rendered"
    Assert-True (-not $hostileReport.Contains("Avoid allocation --> now")) "HTML comment terminators must be neutralized"
    Assert-True ($hostileReport.Contains('"staticFindingSeverity":"none"')) "Invalid severity should fail safely to none"
    Assert-ReportLayout $hostileReport "Managed benchmarks"
    Assert-True ($hostileReport.Contains("&lt;details open&gt;")) "Narrative HTML must not control the accordions"
    Assert-True ($hostileReport.Contains('\`\`\`unclosed')) "Narrative code fences must not swallow the accordion endings"
    & $validator `
        -PrMetadataPath $prMetadataPath `
        -ReportPath $hostileReportPath `
        -PolicyPath $policyPath `
        -SelectionPath $fullSelectionPath `
        -SummaryPath $summaryPath `
        -DecisionBaselinePath $cleanBaselinePath
    Assert-Equal 0 $LASTEXITCODE "Escaped narrative retains a valid report layout"

    Write-Host "All performance report renderer tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
