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

    [string]$TestProject = '',

    [string]$TestProjectPath = '',

    [Parameter(Mandatory = $true)]
    [ValidateLength(1, 500)]
    [string]$TestClass,

    [Parameter(Mandatory = $true)]
    [ValidateLength(1, 500)]
    [string]$TestMethod,

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
$machineResultPath = Join-Path $OutputDirectory 'verifier-machine-result.json'

$secretNames = @('GH_TOKEN', 'GITHUB_TOKEN', 'COPILOT_GITHUB_TOKEN')
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
        '-TestClass', $TestClass,
        '-TestMethod', $TestMethod,
        '-MachineResultPath', $machineResultPath,
        '-PRNumber', [string]$IssueNumber
    )
    if (-not [string]::IsNullOrWhiteSpace($TestProject)) {
        $arguments += @('-TestProject', $TestProject)
    }
    if (-not [string]::IsNullOrWhiteSpace($TestProjectPath)) {
        $arguments += @('-TestProjectPath', $TestProjectPath)
    }
    if ($TestType -eq 'DeviceTest') {
        $trustedSkillsRoot = Split-Path -Parent (
            Split-Path -Parent (
                Split-Path -Parent $VerifierPath))
        $deviceTestScriptPath = Join-Path `
            $trustedSkillsRoot `
            'run-device-tests/scripts/Run-DeviceTests.ps1'
        if (-not (Test-Path -LiteralPath $deviceTestScriptPath -PathType Leaf)) {
            throw "Trusted device-test runner was not found: $deviceTestScriptPath"
        }
        $arguments += @('-DeviceTestScriptPath', $deviceTestScriptPath)
    }

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
$actualFailureMessage = ''
$expectedMachineFilter = if ($TestType -in @('UnitTest', 'XamlUnitTest')) {
    "FullyQualifiedName=$TestClass.$TestMethod"
} else {
    $TestFilter
}
if (Test-Path -LiteralPath $machineResultPath -PathType Leaf) {
    try {
        $machineResultFile = Get-Item -LiteralPath $machineResultPath
        if ($machineResultFile.Length -gt 64KB) {
            throw 'Verifier machine result exceeds the trusted size limit.'
        }
        $machineResult = Get-Content -LiteralPath $machineResultPath -Raw |
            ConvertFrom-Json -ErrorAction Stop
        if (
            [int]$machineResult.schemaVersion -eq 1 -and
            $machineResult.failed -eq $true -and
            ([string]$machineResult.testType) -ceq $TestType -and
            ([string]$machineResult.testFilter) -ceq $expectedMachineFilter -and
            ([string]$machineResult.testProject) -ceq $TestProject -and
            ([string]$machineResult.testProjectPath) -ceq $TestProjectPath -and
            ([string]$machineResult.testClass) -ceq $TestClass -and
            ([string]$machineResult.testMethod) -ceq $TestMethod -and
            ([string]$machineResult.actualFailureMessage).Length -le 10000
        ) {
            $actualFailureMessage = [string]$machineResult.actualFailureMessage
        }
    } catch {
        $actualFailureMessage = ''
    }
    Remove-Item -LiteralPath $machineResultPath -Force
}
$signatureMatched = Test-ReplicationExpectedFailureSignature `
    -Content $actualFailureMessage `
    -Signature $ExpectedFailureSignature
$infrastructureFailure = Test-ReplicationInfrastructureFailure -Content $combined
$verificationPassed = $verifierPassed -and $signatureMatched -and -not $infrastructureFailure

$result = [ordered]@{
    schemaVersion = 1
    issueNumber = $IssueNumber
    platform = $Platform
    testType = $TestType
    testFilter = $TestFilter
    testProject = $TestProject
    testProjectPath = $TestProjectPath
    testClass = $TestClass
    testMethod = $TestMethod
    expectedFailureSignature = $ExpectedFailureSignature
    actualFailureMessage = $actualFailureMessage
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
