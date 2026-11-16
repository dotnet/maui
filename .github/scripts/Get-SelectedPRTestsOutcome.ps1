$script:SelectedPRTestsPipelineIds = @{ 'maui-pr' = 302; 'maui-pr-uitests' = 313; 'maui-pr-devicetests' = 314 }

function Get-SelectedPRTestsOutcome {
    <#
    .SYNOPSIS
    Collects anonymous Azure DevOps status and test totals for selected-test runs.
    .DESCRIPTION
    Uses public dnceng-public endpoints only. Failed test names are confirmed through
    per-run result details because the build-wide result list can mislabel titles.
    #>
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string]$Pipeline,
        [Parameter(Mandatory)][long]$BuildId,
        [int]$MaxFailedTests = 25,
        [scriptblock]$Get = { param($uri) Invoke-RestMethod -Uri $uri -TimeoutSec 60 }
    )

    if (-not $script:SelectedPRTestsPipelineIds.ContainsKey($Pipeline)) {
        throw "Unsupported selected-test pipeline '$Pipeline'."
    }
    $base = 'https://dev.azure.com/dnceng-public/public/_apis'
    $build = & $Get "$base/build/builds/$BuildId`?api-version=7.1"
    if ([int]$build.definition.id -ne $script:SelectedPRTestsPipelineIds[$Pipeline]) {
        throw "Build $BuildId does not belong to $Pipeline."
    }

    $summary = & $Get "https://vstmr.dev.azure.com/dnceng-public/public/_apis/testresults/resultsummarybybuild?buildId=$BuildId&api-version=7.1-preview.1"
    $byOutcome = $summary.aggregatedResultsAnalysis.resultsByOutcome
    $count = { param($name) if ($byOutcome -and $byOutcome.$name) { [int]$byOutcome.$name.count } else { 0 } }
    $failedCount = & $count 'Failed'

    $failedTests = @()
    if ($failedCount -gt 0 -and $MaxFailedTests -gt 0) {
        $failed = & $Get "https://vstmr.dev.azure.com/dnceng-public/public/_apis/testresults/resultsbybuild?buildId=$BuildId&outcomes=Failed&`$top=$MaxFailedTests&api-version=7.1-preview.1"
        foreach ($item in @($failed.value | Select-Object -First $MaxFailedTests)) {
            $detail = & $Get "$base/test/Runs/$([long]$item.runId)/Results/$([long]$item.id)?api-version=7.1"
            if ($detail.outcome -ne 'Failed') { continue }
            $failedTests += [pscustomobject]@{
                name = [string]$detail.automatedTestName
                run = [string]$detail.testRun.name
            }
        }
    }

    [pscustomobject]@{
        pipeline = $Pipeline
        buildId = $BuildId
        url = "https://dev.azure.com/dnceng-public/public/_build/results?buildId=$BuildId"
        status = [string]$build.status
        result = [string]$build.result
        total = [int]$summary.aggregatedResultsAnalysis.totalTests
        passed = & $count 'Passed'
        failed = $failedCount
        notExecuted = & $count 'NotExecuted'
        failedTests = $failedTests
    }
}
