#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$RequestKey,

    [Parameter(Mandatory = $false)]
    [string]$Repository = "dotnet/maui",

    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$PullRequestNumber,

    [Parameter(Mandatory = $true)]
    [string]$BaseCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HeadCommitSha,

    [Parameter(Mandatory = $true)]
    [string]$HarnessSha,

    [Parameter(Mandatory = $true)]
    [string]$RegistrySha256,

    [Parameter(Mandatory = $true)]
    [string]$ScenarioId,

    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "windows")]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [ValidateSet("direct", "sampled")]
    [string]$Coverage,

    [Parameter(Mandatory = $true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

Assert-UiEvidenceSha $BaseCommitSha "BaseCommitSha"
Assert-UiEvidenceSha $HeadCommitSha "HeadCommitSha"
Assert-UiEvidenceSha $HarnessSha "HarnessSha"
Assert-UiEvidenceId $ScenarioId "ScenarioId"
if ($RegistrySha256 -notmatch '^[0-9a-fA-F]{64}$') {
    throw "RegistrySha256 must be a 64-character SHA-256 digest."
}

$expectedKey = Get-UiEvidenceRequestKey `
    -Repository $Repository `
    -PullRequestNumber $PullRequestNumber `
    -BaseCommitSha $BaseCommitSha `
    -HeadCommitSha $HeadCommitSha `
    -HarnessSha $HarnessSha `
    -RegistrySha256 $RegistrySha256 `
    -ScenarioId $ScenarioId `
    -Platform $Platform
if ($RequestKey -ne $expectedKey) {
    throw "RequestKey '$RequestKey' does not match the request identity '$expectedKey'."
}

$request = [PSCustomObject][ordered]@{
    schemaVersion = 1
    requestKey = $RequestKey
    repository = $Repository
    pullRequestNumber = $PullRequestNumber
    baseCommitSha = $BaseCommitSha.ToLowerInvariant()
    headCommitSha = $HeadCommitSha.ToLowerInvariant()
    harnessSha = $HarnessSha.ToLowerInvariant()
    registrySha256 = $RegistrySha256.ToLowerInvariant()
    scenarioId = $ScenarioId
    platform = $Platform
    coverage = $Coverage
    expectedRunOrder = @("base-1", "head-1", "head-2", "base-2")
    expectedVariantRuns = 2
}
Write-UiEvidenceJson $request $OutputPath
