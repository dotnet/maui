#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'shared/Invoke-ReplicationTestVerification.ps1'
    $script:Source = Get-Content -LiteralPath $scriptPath -Raw
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$errors)
    if ($errors) {
        throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
    }

    foreach ($name in @(
        'Test-ReplicationExpectedFailureSignature',
        'Test-ReplicationInfrastructureFailure',
        'ConvertTo-AzdoSafeReplicationOutput'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $name
        }, $true)
        Invoke-Expression $function.Extent.Text
    }

    function New-ReplicationVerifierStub {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$ActualFailureMessage
        )

        $encodedMessage = [Convert]::ToBase64String(
            [Text.Encoding]::UTF8.GetBytes($ActualFailureMessage))
        @"
param(
    [string]`$Platform,
    [string]`$TestType,
    [string]`$TestFilter,
    [string]`$TestProject,
    [string]`$TestProjectPath,
    [string]`$TestClass,
    [string]`$TestMethod,
    [string]`$MachineResultPath,
    [string]`$PRNumber
)
`$message = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('$encodedMessage'))
[ordered]@{
    schemaVersion = 1
    testType = `$TestType
    testFilter = if (`$TestType -in @('UnitTest', 'XamlUnitTest')) {
        "FullyQualifiedName=`$TestClass.`$TestMethod"
    } else {
        `$TestFilter
    }
    testProject = `$TestProject
    testProjectPath = `$TestProjectPath
    testClass = `$TestClass
    testMethod = `$TestMethod
    failed = `$true
    actualFailureMessage = `$message
} | ConvertTo-Json | Set-Content -LiteralPath `$MachineResultPath -Encoding utf8NoBOM
Write-Host 'VERIFY FAILURE ONLY MODE'
Write-Host "[`$TestType] `$TestFilter FAILED"
Write-Host 'VERIFICATION PASSED'
exit 0
"@ | Set-Content -LiteralPath $Path -Encoding utf8NoBOM
    }
}

Describe 'Replication failure-only verification' {
    It 'matches the expected assertion literally rather than as a regex' {
        Test-ReplicationExpectedFailureSignature `
            -Content 'Assert.Equal failed. Expected: [a+b]; Actual: [aab]' `
            -Signature 'Expected: [a+b]; Actual: [aab]' |
            Should -BeTrue

        Test-ReplicationExpectedFailureSignature `
            -Content 'Expected: ab; Actual: aab' `
            -Signature 'Expected: [a+b]; Actual: [aab]' |
            Should -BeFalse
    }

    It 'rejects compilation, timeout, missing baseline, and device failures' {
        Test-ReplicationInfrastructureFailure -Content 'Build FAILED.' | Should -BeTrue
        Test-ReplicationInfrastructureFailure -Content 'error CS1002: ; expected' | Should -BeTrue
        Test-ReplicationInfrastructureFailure -Content 'Test run timed out' | Should -BeTrue
        Test-ReplicationInfrastructureFailure -Content 'snapshot baseline was not found' | Should -BeTrue
        Test-ReplicationInfrastructureFailure -Content 'Android emulator failed to boot' | Should -BeTrue
        Test-ReplicationInfrastructureFailure -Content 'Xunit.Sdk.EqualException: expected 1 actual 0' | Should -BeFalse
    }

    It 'removes Azure logging directives from verifier console output' {
        ConvertTo-AzdoSafeReplicationOutput -Value 'bad ##vso[task.setvariable variable=X]secret and ##[error]fake' |
            Should -BeExactly 'bad secret and fake'
    }

    It 'sets the issue guard only around a child verifier and clears all publisher tokens' {
        $script:Source | Should -Match "MAUI_REPRODUCTION_ISSUE"
        $script:Source | Should -Match "GH_REPLICATION_TOKEN"
        $script:Source | Should -Match "& pwsh @arguments"
        $script:Source | Should -Match "signatureMatched"
        $script:Source | Should -Match "infrastructureFailure"
    }

    It 'persists and matches only the verifier machine result failure message' {
        $verifier = Join-Path $TestDrive 'verifier.ps1'
        $output = Join-Path $TestDrive 'success'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Assert.Equal() Failure: Issue12345 expected red but was blue'

        & pwsh -NoProfile -File $scriptPath `
            -IssueNumber 12345 `
            -Platform android `
            -TestType UnitTest `
            -TestFilter Issue12345 `
            -TestProject Controls.Core.UnitTests `
            -TestProjectPath src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj `
            -TestClass Microsoft.Maui.Controls.Tests.Issue12345Tests `
            -TestMethod ReproducesIssue12345 `
            -ExpectedFailureSignature Issue12345 `
            -VerifierPath $verifier `
            -OutputDirectory $output *> $null

        $LASTEXITCODE | Should -Be 0
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.actualFailureMessage |
            Should -BeExactly 'Assert.Equal() Failure: Issue12345 expected red but was blue'
        $result.signatureMatched | Should -BeTrue
        $result.testProject | Should -BeExactly 'Controls.Core.UnitTests'
        $result.testClass |
            Should -BeExactly 'Microsoft.Maui.Controls.Tests.Issue12345Tests'
        $result.testMethod | Should -BeExactly 'ReproducesIssue12345'
        Test-Path -LiteralPath (Join-Path $output 'verifier-machine-result.json') |
            Should -BeFalse
    }

    It 'does not match a signature present only in test metadata and console output' {
        $verifier = Join-Path $TestDrive 'metadata-verifier.ps1'
        $output = Join-Path $TestDrive 'metadata-mismatch'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Assert.Equal() Failure: expected red but was blue'

        & pwsh -NoProfile -File $scriptPath `
            -IssueNumber 12345 `
            -Platform android `
            -TestType UnitTest `
            -TestFilter Issue12345 `
            -TestProject Controls.Core.UnitTests `
            -TestProjectPath src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj `
            -TestClass Microsoft.Maui.Controls.Tests.Issue12345Tests `
            -TestMethod ReproducesIssue12345 `
            -ExpectedFailureSignature Issue12345 `
            -VerifierPath $verifier `
            -OutputDirectory $output *> $null

        $LASTEXITCODE | Should -Not -Be 0
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.actualFailureMessage |
            Should -BeExactly 'Assert.Equal() Failure: expected red but was blue'
        $result.signatureMatched | Should -BeFalse
        $result.verificationPassed | Should -BeFalse
    }
}
