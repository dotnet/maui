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
    [Parameter(Mandatory = $true)]
    [ValidateSet('main', 'net11.0')]
    [string]$ExpectedBaseBranch,
    [string]$OutputDirectory = '/tmp/gh-aw/agent',
    [string]$ExpectationDirectory = '/tmp/gh-aw/agent/ci-fix-output-expectations'
)

$ErrorActionPreference = 'Stop'

function Test-JsonInt64Value {
    param(
        [AllowNull()]
        [object]$Value,
        [long]$ExpectedValue
    )

    return $null -ne $Value -and
        $Value.GetType() -eq [long] -and
        $Value -eq $ExpectedValue
}

function Test-JsonBooleanValue {
    param(
        [AllowNull()]
        [object]$Value,
        [bool]$ExpectedValue
    )

    return $null -ne $Value -and
        $Value.GetType() -eq [bool] -and
        $Value -eq $ExpectedValue
}

function Test-JsonArray {
    param(
        [AllowNull()]
        [object]$Value
    )

    return $null -ne $Value -and $Value.GetType() -eq [object[]]
}

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
    'baseBranch' -in $rootProperties -and
    'issueEvidence' -in $rootProperties -and
    'candidates' -in $rootProperties -and
    (Test-JsonInt64Value -Value $snapshot.schemaVersion -ExpectedValue 2) -and
    $snapshot.repository -is [string] -and
    $snapshot.repository -ceq 'dotnet/maui' -and
    $snapshot.baseBranch -is [string] -and
    $snapshot.baseBranch -ceq $ExpectedBaseBranch -and
    $issueEvidence -is [pscustomobject] -and
    'authoritative' -in $issueProperties -and
    'exactLabel' -in $issueProperties -and
    'scopedIssueNumber' -in $issueProperties -and
    'truncated' -in $issueProperties -and
    'count' -in $issueProperties -and
    'totalMatched' -in $issueProperties -and
    'issues' -in $issueProperties -and
    (Test-JsonBooleanValue -Value $issueEvidence.authoritative -ExpectedValue $true) -and
    $issueEvidence.exactLabel -is [string] -and
    $issueEvidence.exactLabel -ceq $ExpectedIssueLabel -and
    $null -eq $issueEvidence.scopedIssueNumber -and
    (Test-JsonBooleanValue -Value $issueEvidence.truncated -ExpectedValue $false) -and
    (Test-JsonInt64Value -Value $issueEvidence.count -ExpectedValue 0) -and
    (Test-JsonInt64Value -Value $issueEvidence.totalMatched -ExpectedValue 0) -and
    (Test-JsonArray -Value $issueEvidence.issues) -and
    $issueEvidence.issues.Count -eq 0 -and
    (Test-JsonArray -Value $snapshot.candidates) -and
    $snapshot.candidates.Count -eq 0

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
