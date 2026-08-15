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
}
