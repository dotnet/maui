#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Persists and registers the authoritative empty-queue CI-fixer outcome.
#>

param(
    [string]$CandidatesPath = '/tmp/gh-aw/agent/prefetch.json',
    [Parameter(Mandatory = $true)]
    [ValidateSet('ci-scan', 'ci-scan-net11')]
    [string]$ExpectedIssueLabel,
    [string]$OutputDirectory = '/tmp/gh-aw/agent',
    [string]$ExpectationDirectory = '/tmp/gh-aw/agent/ci-fix-output-expectations'
)

$ErrorActionPreference = 'Stop'

$snapshot = Get-Content -Raw -LiteralPath $CandidatesPath | ConvertFrom-Json -Depth 100
$issueEvidence = $snapshot.issueEvidence
$rootProperties = @($snapshot.PSObject.Properties.Name)
$issueProperties = if ($null -ne $issueEvidence) {
    @($issueEvidence.PSObject.Properties.Name)
} else {
    @()
}
$isAuthoritativeEmptyQueue =
    'schemaVersion' -in $rootProperties -and
    'repository' -in $rootProperties -and
    'issueEvidence' -in $rootProperties -and
    'candidates' -in $rootProperties -and
    $snapshot.schemaVersion -eq 2 -and
    [string]$snapshot.repository -ceq 'dotnet/maui' -and
    $null -ne $issueEvidence -and
    'authoritative' -in $issueProperties -and
    'exactLabel' -in $issueProperties -and
    'scopedIssueNumber' -in $issueProperties -and
    'truncated' -in $issueProperties -and
    'count' -in $issueProperties -and
    'totalMatched' -in $issueProperties -and
    'issues' -in $issueProperties -and
    $issueEvidence.authoritative -eq $true -and
    [string]$issueEvidence.exactLabel -ceq $ExpectedIssueLabel -and
    $null -eq $issueEvidence.scopedIssueNumber -and
    $issueEvidence.truncated -eq $false -and
    [int]$issueEvidence.count -eq 0 -and
    [int]$issueEvidence.totalMatched -eq 0 -and
    $null -ne $issueEvidence.issues -and
    @($issueEvidence.issues).Count -eq 0 -and
    $null -ne $snapshot.candidates -and
    @($snapshot.candidates).Count -eq 0

if (-not $isAuthoritativeEmptyQueue) {
    throw 'The CI-fixer snapshot is not an authoritative unscoped empty queue.'
}

New-Item -ItemType Directory -Force -Path $OutputDirectory | Out-Null
$utf8NoBom = [Text.UTF8Encoding]::new($false)
[IO.File]::WriteAllText(
    (Join-Path $OutputDirectory 'coverage.txt'),
    '',
    $utf8NoBom)
[IO.File]::WriteAllText(
    (Join-Path $OutputDirectory 'summary.md'),
    "| issue | branch | attempt | outcome | reason |`n|---|---|---|---|---|`n",
    $utf8NoBom)

$registrationScript = Join-Path $PSScriptRoot 'Register-CiFixSafeOutputExpectation.ps1'
$expectationPath = & $registrationScript `
    -Type noop `
    -OutputDirectory $ExpectationDirectory

[pscustomobject]@{
    emptyQueue = $true
    coveragePath = Join-Path $OutputDirectory 'coverage.txt'
    summaryPath = Join-Path $OutputDirectory 'summary.md'
    expectationPath = $expectationPath
}
