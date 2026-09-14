#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$skillRoot = Split-Path -Parent $PSScriptRoot
$summaryScript = Join-Path $skillRoot "scripts\Build-UiEvidenceAgentSummary.ps1"
$validator = Join-Path $skillRoot "scripts\Validate-UiEvidenceReport.ps1"
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-ui-evidence-skill-tests-" + [Guid]::NewGuid().ToString("N"))

function Assert-Equal($Expected, $Actual, [string]$Message) {
    if ($Expected -ne $Actual) {
        throw "$Message. Expected '$Expected', actual '$Actual'."
    }
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
try {
    $selection = @{
        schemaVersion = 1
        repository = "dotnet/maui"
        pullRequestNumber = 42
        baseCommitSha = ("1" * 40)
        headCommitSha = ("2" * 40)
        harnessSha = ("3" * 40)
        coverage = @{
            status = "complete"
            uiRelevantFileCount = 1
            directFileCount = 1
            sampledFileCount = 0
            unmappedFileCount = 0
        }
        requests = @(@{
            requestKey = "maui-ui-0123456789abcdef01234567"
            scenarioId = "layout-controls-smoke"
            platform = "windows"
            coverage = "direct"
        })
    }
    $selectionPath = Join-Path $testRoot "selection.json"
    $selection | ConvertTo-Json -Depth 12 | Set-Content $selectionPath
    $bundle = Join-Path $testRoot "bundles\maui-ui-0123456789abcdef01234567"
    New-Item -ItemType Directory -Force -Path $bundle | Out-Null
    @{
        request = @{
            requestKey = "maui-ui-0123456789abcdef01234567"
            headCommitSha = ("2" * 40)
        }
        verdict = "visual-change-advisory"
        trustLevel = "co-resident-advisory"
        evidenceComplete = $true
        environmentComparable = $true
        environment = @{
            targetPlatform = "windows"
            platformVersion = "10.0"
            deviceModel = "machine"
            displaySize = "1920x1080"
            displayDensity = "1"
            orientation = "landscape"
        }
        checkpointComparisons = @(@{
            checkpointId = "initial"
            status = "change-detected"
            baseIntraDifference = 0
            headIntraDifference = 0
            minimumCrossDifference = 0.2
            maximumCrossDifference = 0.2
        })
        newLayoutFindingKeys = @()
        errors = @()
    } | ConvertTo-Json -Depth 12 | Set-Content (Join-Path $bundle "comparison-summary.json")

    $summaryPath = Join-Path $testRoot "agent-summary.json"
    & $summaryScript `
        -SelectionPath $selectionPath `
        -BundlesRoot (Join-Path $testRoot "bundles") `
        -OutputPath $summaryPath
    if (-not (Test-Path $summaryPath)) {
        throw "Agent summary was not created."
    }
    $summary = Get-Content $summaryPath -Raw | ConvertFrom-Json
    Assert-Equal "visual-change-advisory" $summary.overallVerdict "Verdict precedence"

    $selection.coverage.status = "partial"
    $selection | ConvertTo-Json -Depth 12 | Set-Content $selectionPath
    $comparisonPath = Join-Path $bundle "comparison-summary.json"
    $comparison = Get-Content $comparisonPath -Raw | ConvertFrom-Json
    $comparison.verdict = "no-difference-observed"
    $comparison | ConvertTo-Json -Depth 12 | Set-Content $comparisonPath
    $partialSummaryPath = Join-Path $testRoot "partial-agent-summary.json"
    & $summaryScript `
        -SelectionPath $selectionPath `
        -BundlesRoot (Join-Path $testRoot "bundles") `
        -OutputPath $partialSummaryPath
    $partialSummary = Get-Content $partialSummaryPath -Raw | ConvertFrom-Json
    Assert-Equal "inconclusive" $partialSummary.overallVerdict "Partial coverage must suppress no-difference"

    $selection.coverage.status = "complete"
    $selection | ConvertTo-Json -Depth 12 | Set-Content $selectionPath
    $comparison.verdict = "visual-change-advisory"
    $comparison | ConvertTo-Json -Depth 12 | Set-Content $comparisonPath

    $reportPath = Join-Path $testRoot "report.md"
    @"
## UI evidence analysis

**Empirical verdict:** ``visual-change-advisory``

Measured head: $($selection.headCommitSha)

### Coverage

Complete.

### Evidence

Visual change observed.

### Limitations

This result is advisory and is not a merge gate.

> Automated analysis by the **ui-evidence** agentic workflow.
"@ | Set-Content $reportPath
    & $validator -ReportPath $reportPath -AgentSummaryPath $summaryPath

    Add-Content $reportPath "This PR is safe to merge."
    & pwsh -NoProfile -File $validator -ReportPath $reportPath -AgentSummaryPath $summaryPath *> $null
    Assert-Equal 1 $LASTEXITCODE "Unsafe report claim should fail"

    Write-Host "All UI evidence skill tests passed."
}
finally {
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
