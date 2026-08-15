#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs the trusted failure-only verifier for an issue replication candidate.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateRange(1, [int]::MaxValue)]
    [int]$IssueNumber,

    [Parameter(Mandatory = $true)]
    [ValidateSet('android', 'ios', 'catalyst', 'windows')]
    [string]$Platform,

    [Parameter(Mandatory = $true)]
    [ValidateSet('UITest', 'UnitTest', 'XamlUnitTest', 'DeviceTest')]
    [string]$TestType,

    [Parameter(Mandatory = $true)]
    [ValidateLength(1, 500)]
    [string]$TestFilter,

    [Parameter(Mandatory = $true)]
    [ValidateLength(3, 1000)]
    [string]$ExpectedFailureSignature,

    [Parameter(Mandatory = $true)]
    [string]$VerifierPath,

    [Parameter(Mandatory = $true)]
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3.0

function Test-ReplicationExpectedFailureSignature {
    param(
        [AllowEmptyString()][string]$Content,
        [Parameter(Mandatory = $true)][string]$Signature
    )

    if ([string]::IsNullOrWhiteSpace($Content) -or [string]::IsNullOrWhiteSpace($Signature)) {
        return $false
    }

    return $Content.Contains($Signature, [StringComparison]::Ordinal)
}

function Test-ReplicationInfrastructureFailure {
    param([AllowEmptyString()][string]$Content)

    if ([string]::IsNullOrWhiteSpace($Content)) {
        return $true
    }

    $patterns = @(
        '(?im)^\s*Build FAILED\.?\s*$',
        '(?im)\berror (?:CS|MSB|NETSDK|XLS|APT|XA)\d{3,}\b',
        '(?im)\bNo test matches\b',
        '(?im)\b(?:test run|operation|command|task) timed out\b',
        '(?im)\b(?:snapshot|baseline).{0,80}\b(?:missing|not found|does not exist)\b',
        '(?im)\b(?:device|simulator|emulator).{0,80}\b(?:offline|unavailable|failed to boot)\b'
    )

    foreach ($pattern in $patterns) {
        if ($Content -match $pattern) {
            return $true
        }
    }
    return $false
}

function ConvertTo-AzdoSafeReplicationOutput {
    param([AllowEmptyString()][string]$Value)

    if ($null -eq $Value) {
        return ''
    }

    return $Value -replace '##vso\[[^\]]*\]', '' -replace '##\[[^\]]*\]', ''
}

if (-not (Test-Path -LiteralPath $VerifierPath -PathType Leaf)) {
    throw "Trusted failure-only verifier was not found: $VerifierPath"
}

New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
$consoleLog = Join-Path $OutputDirectory 'verification-console.log'
$resultPath = Join-Path $OutputDirectory 'verification-result.json'

$secretNames = @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN', 'GH_REPLICATION_TOKEN')
$savedSecrets = @{}
$previousIssue = [Environment]::GetEnvironmentVariable('MAUI_REPRODUCTION_ISSUE')

try {
    foreach ($name in $secretNames) {
        $savedSecrets[$name] = [Environment]::GetEnvironmentVariable($name)
        [Environment]::SetEnvironmentVariable($name, $null)
    }
    [Environment]::SetEnvironmentVariable('MAUI_REPRODUCTION_ISSUE', [string]$IssueNumber)

    $arguments = @(
        '-NoProfile',
        '-File', $VerifierPath,
        '-Platform', $Platform,
        '-TestType', $TestType,
        '-TestFilter', $TestFilter,
        '-PRNumber', [string]$IssueNumber
    )

    $output = @(& pwsh @arguments 2>&1)
    $exitCode = $LASTEXITCODE
    $outputText = ($output | ForEach-Object { [string]$_ }) -join [Environment]::NewLine
    $outputText | Set-Content -LiteralPath $consoleLog -Encoding utf8NoBOM
    Write-Host (ConvertTo-AzdoSafeReplicationOutput -Value $outputText)
}
finally {
    [Environment]::SetEnvironmentVariable('MAUI_REPRODUCTION_ISSUE', $previousIssue)
    foreach ($name in $secretNames) {
        [Environment]::SetEnvironmentVariable($name, $savedSecrets[$name])
    }
}

$candidateLogs = @($consoleLog)
$verificationRoot = Join-Path (git rev-parse --show-toplevel) "CustomAgentLogsTmp/PRState/$IssueNumber/PRAgent/gate/verify-tests-fail"
if (Test-Path -LiteralPath $verificationRoot) {
    $candidateLogs += @(Get-ChildItem -LiteralPath $verificationRoot -Recurse -File -ErrorAction SilentlyContinue |
        Where-Object { $_.Name -in @('verification-log.txt', 'test-without-fix.log', 'verification-report.md') } |
        ForEach-Object FullName)
}

$combined = ($candidateLogs | Sort-Object -Unique | ForEach-Object {
    if (Test-Path -LiteralPath $_ -PathType Leaf) {
        Get-Content -LiteralPath $_ -Raw -ErrorAction SilentlyContinue
    }
}) -join [Environment]::NewLine

$verifierPassed = $exitCode -eq 0 -and $combined -match 'VERIFICATION PASSED'
$signatureMatched = Test-ReplicationExpectedFailureSignature -Content $combined -Signature $ExpectedFailureSignature
$infrastructureFailure = Test-ReplicationInfrastructureFailure -Content $combined
$verificationPassed = $verifierPassed -and $signatureMatched -and -not $infrastructureFailure

$result = [ordered]@{
    schemaVersion = 1
    issueNumber = $IssueNumber
    platform = $Platform
    testType = $TestType
    testFilter = $TestFilter
    expectedFailureSignature = $ExpectedFailureSignature
    verifierExitCode = $exitCode
    verifierPassed = $verifierPassed
    signatureMatched = $signatureMatched
    infrastructureFailure = $infrastructureFailure
    verificationPassed = $verificationPassed
    logFiles = @($candidateLogs | Sort-Object -Unique)
}
$result | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $resultPath -Encoding utf8NoBOM

if (-not $verificationPassed) {
    throw "Replication test verification failed (verifierPassed=$verifierPassed, signatureMatched=$signatureMatched, infrastructureFailure=$infrastructureFailure)."
}

Write-Host 'REPLICATION TEST VERIFICATION PASSED'
Write-Host "Verification result: $resultPath"
