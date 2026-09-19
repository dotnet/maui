#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$SelectionPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

$selection = Read-UiEvidenceJson $SelectionPath
if ($selection.schemaVersion -ne 1) {
    throw "Unsupported UI evidence selection version '$($selection.schemaVersion)'."
}
if ($selection.selectionStatus -ne "ready") {
    Write-UiEvidenceJson @() $OutputPath
    exit 3
}

[object[]]$requests = foreach ($request in @($selection.requests | Sort-Object scenarioId, platform)) {
    Assert-UiEvidenceId ([string]$request.scenarioId) "Scenario id"
    if ($request.platform -notin @("android", "windows")) {
        throw "Unsupported UI evidence platform '$($request.platform)'."
    }

    [PSCustomObject][ordered]@{
        schemaVersion = 1
        requestKey = Get-UiEvidenceRequestKey `
            -Repository ([string]$selection.repository) `
            -PullRequestNumber ([int]$selection.pullRequestNumber) `
            -BaseCommitSha ([string]$selection.baseCommitSha) `
            -HeadCommitSha ([string]$selection.headCommitSha) `
            -HarnessSha ([string]$selection.harnessSha) `
            -RegistrySha256 ([string]$selection.registrySha256) `
            -ScenarioId ([string]$request.scenarioId) `
            -Platform ([string]$request.platform)
        repository = [string]$selection.repository
        pullRequestNumber = [int]$selection.pullRequestNumber
        baseCommitSha = [string]$selection.baseCommitSha
        headCommitSha = [string]$selection.headCommitSha
        harnessSha = [string]$selection.harnessSha
        registrySha256 = [string]$selection.registrySha256
        scenarioId = [string]$request.scenarioId
        platform = [string]$request.platform
        coverage = [string]$request.coverage
        expectedRunOrder = @("base-1", "head-1", "head-2", "base-2")
        expectedVariantRuns = 2
    }
}

Write-UiEvidenceJson $requests $OutputPath
exit 0
