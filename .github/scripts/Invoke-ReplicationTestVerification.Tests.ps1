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
        'ConvertTo-AzdoSafeReplicationOutput',
        'ConvertTo-BoundedVerificationFailureMessage'
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
    [string]`$DeviceTestScriptPath,
    [string]`$PRNumber
)
if (-not [string]::IsNullOrWhiteSpace(`$DeviceTestScriptPath)) { Set-Content -LiteralPath (`$MachineResultPath + '.device-runner-path') -Value `$DeviceTestScriptPath -Encoding utf8NoBOM }
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

    It 'allows the verifier to persist an empty failure message for non-test failures' {
        $verifierSource = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1'
        ) -Raw

        $verifierSource |
            Should -Match '\[AllowEmptyString\(\)\]\[string\]\$ActualFailureMessage'
    }

    It 'removes Azure logging directives from verifier console output' {
        ConvertTo-AzdoSafeReplicationOutput -Value 'bad ##vso[task.setvariable variable=X]secret and ##[error]fake' |
            Should -BeExactly 'bad secret and fake'
    }

    It 'bounds repeated verifier failures while retaining the expected signature' {
        $signature = 'Fitting ScrollView must not expose a reachable vertical range.'
        $content = ('unrelated retry failure' * 250) + $signature + ('final context' * 250)

        $bounded = ConvertTo-BoundedVerificationFailureMessage `
            -Content $content `
            -Signature $signature

        $bounded.Length | Should -BeLessOrEqual 4096
        $bounded.Contains($signature, [StringComparison]::Ordinal) | Should -BeTrue
        $bounded | Should -Match 'verifier output truncated'
    }

    It 'runs the child verifier without an issue guard and clears all publisher tokens' {
        $script:Source | Should -Not -Match "MAUI_REPRODUCTION_ISSUE"
        $script:Source | Should -Match "'GH_TOKEN'"
        $script:Source | Should -Not -Match "GH_REPLICATION_TOKEN"
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

    It 'persists a bounded failure message when test retries repeat the same assertion' {
        $signature = 'Fitting ScrollView must not expose a reachable vertical range.'
        $failure = ((1..30 | ForEach-Object {
            "Retry $_ failed. $signature Expected PASS but was FAIL."
        }) -join [Environment]::NewLine)
        $verifier = Join-Path $TestDrive 'repeated-verifier.ps1'
        $output = Join-Path $TestDrive 'repeated-success'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage $failure

        & pwsh -NoProfile -File $scriptPath `
            -IssueNumber 36800 `
            -Platform ios `
            -TestType UITest `
            -TestFilter Issue36800 `
            -TestClass Microsoft.Maui.TestCases.Tests.Issues.Issue36800 `
            -TestMethod FittingScrollViewDoesNotExposeSafeAreaAsScrollableRange `
            -ExpectedFailureSignature $signature `
            -VerifierPath $verifier `
            -OutputDirectory $output *> $null

        $LASTEXITCODE | Should -Be 0
        $resultText = Get-Content -LiteralPath (
            Join-Path $output 'verification-result.json') -Raw
        $result = $resultText | ConvertFrom-Json
        $result.actualFailureMessage.Length | Should -BeLessOrEqual 4096
        $result.actualFailureMessage.Contains($signature, [StringComparison]::Ordinal) |
            Should -BeTrue
        $result.signatureMatched | Should -BeTrue
        $result.verificationPassed | Should -BeTrue
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

    It 'passes the trusted pre-baseline device runner to the verifier' {
        $trustedRoot = Join-Path $TestDrive 'trusted-github'
        $verifier = Join-Path `
            $trustedRoot `
            'skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1'
        $deviceRunner = Join-Path `
            $trustedRoot `
            'skills/run-device-tests/scripts/Run-DeviceTests.ps1'
        New-Item -ItemType Directory -Path (Split-Path -Parent $verifier) -Force |
            Out-Null
        New-Item -ItemType Directory -Path (Split-Path -Parent $deviceRunner) -Force |
            Out-Null
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Issue31059 expected item 4 but observed item 1'
        'trusted runner' | Set-Content -LiteralPath $deviceRunner -Encoding utf8NoBOM

        $output = Join-Path $TestDrive 'device-success'
        & pwsh -NoProfile -File $scriptPath `
            -IssueNumber 31059 `
            -Platform ios `
            -TestType DeviceTest `
            -TestFilter Issue31059 `
            -TestProject Controls `
            -TestProjectPath src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj `
            -TestClass Microsoft.Maui.DeviceTests.Issue31059 `
            -TestMethod RetainsCenteredItemAfterPortraitToLandscapeResize `
            -ExpectedFailureSignature Issue31059 `
            -VerifierPath $verifier `
            -OutputDirectory $output *> $null

        $LASTEXITCODE | Should -Be 0
        $capturedPath = Get-Content -LiteralPath (
            Join-Path $output 'verifier-machine-result.json.device-runner-path') -Raw
        $capturedPath.Trim() | Should -BeExactly $deviceRunner
    }
}
