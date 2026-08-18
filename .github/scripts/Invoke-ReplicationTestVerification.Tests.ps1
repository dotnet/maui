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
        'ConvertTo-NormalizedReplicationSignature',
        'Get-ReplicationSignatureTokens',
        'Test-ReplicationSignatureEquivalent',
        'Test-ReplicationExpectedFailureSignature',
        'Test-ReplicationInfrastructureFailure',
        'ConvertTo-AzdoSafeReplicationOutput',
        'ConvertTo-BoundedVerificationFailureMessage',
        'Get-ReplicationVolatileFreeMessage',
        'Test-ReplicationFailureMessagesAreStable'
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
            [Parameter(Mandatory = $true)][string]$ActualFailureMessage,
            [string[]]$PerRunFailureMessages
        )

        $encodedMessage = [Convert]::ToBase64String(
            [Text.Encoding]::UTF8.GetBytes($ActualFailureMessage))
        $encodedPerRun = if ($PerRunFailureMessages) {
            ($PerRunFailureMessages | ForEach-Object {
                "'" + [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($_)) + "'"
            }) -join ','
        } else {
            ''
        }
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
Add-Content -LiteralPath (Join-Path (Split-Path -Parent `$MachineResultPath) 'invocations.txt') -Value 'run' -Encoding utf8NoBOM
if (Test-Path -LiteralPath (Join-Path (Split-Path -Parent `$MachineResultPath) 'fail-after-first.flag')) {
    if ((Get-Content -LiteralPath (Join-Path (Split-Path -Parent `$MachineResultPath) 'invocations.txt')).Count -gt 1) {
        Write-Host 'VERIFY FAILURE ONLY MODE'
        Write-Host 'VERIFICATION FAILED'
        exit 1
    }
}
`$message = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('$encodedMessage'))
`$perRun = @($encodedPerRun)
if (`$perRun.Count -gt 0) {
    `$invocationCount = @(Get-Content -LiteralPath (Join-Path (Split-Path -Parent `$MachineResultPath) 'invocations.txt')).Count
    `$index = [Math]::Min(`$invocationCount - 1, `$perRun.Count - 1)
    `$message = [Text.Encoding]::UTF8.GetString([Convert]::FromBase64String(`$perRun[`$index]))
}
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

    It 'accepts a re-wrapped assertion that reports the same failure' {
        # Verifier output re-indents long messages, so build 14999429 rejected a
        # test that had genuinely failed at its intended assertion.
        Test-ReplicationExpectedFailureSignature `
            -Content "Assert.Equal() Failure`n  Expected:   UIColor red`n  Actual:     UIColor blue" `
            -Signature 'Expected: UIColor red Actual: UIColor blue' |
            Should -BeTrue

        Test-ReplicationExpectedFailureSignature `
            -Content 'Expected: UIColor red; Actual: UIColor green' `
            -Signature 'Expected: UIColor red; Actual: UIColor blue' |
            Should -BeFalse
    }

    It 'treats a differently worded report of the same defect as equivalent' {
        # Issue 36697: the test failed at its intended assertion but the message
        # named the attributed title, so an exact match discarded a working
        # reproduction and the repair round then broke it.
        Test-ReplicationSignatureEquivalent `
            -Declared 'CurrentAttributedTitle foreground stayed UIColor blue after CharacterSpacing changed' `
            -Observed ('Assert.Equal() Failure: attributed title foreground for ' +
                'CurrentAttributedTitle stayed blue once CharacterSpacing changed at runtime') |
            Should -BeTrue

        # A different defect must still be rejected.
        Test-ReplicationSignatureEquivalent `
            -Declared 'CurrentAttributedTitle foreground stayed UIColor blue after CharacterSpacing changed' `
            -Observed 'Assert.True() Failure: ScrollView content offset was 44 instead of 0' |
            Should -BeFalse

        # Boilerplate alone can never establish equivalence.
        Test-ReplicationSignatureEquivalent `
            -Declared 'Expected true but was false' `
            -Observed 'Expected true but was false for something entirely different' |
            Should -BeFalse
    }

    It 'rejects compilation, timeout, missing baseline, and device failures' {
        Test-ReplicationInfrastructureFailure -Content 'Build FAILED.' | Should -BeTrue
        Test-ReplicationInfrastructureFailure -Content 'error CS1002: ; expected' | Should -BeTrue
        Test-ReplicationInfrastructureFailure `
            -Content 'HandlerNotFoundException: Unable to find a IElementHandler corresponding to Grid.' |
            Should -BeTrue
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

    It 'executes the targeted test once for every requested run' {
        $verifier = Join-Path $TestDrive 'repeat-verifier.ps1'
        $output = Join-Path $TestDrive 'repeat'
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
            -OutputDirectory $output `
            -RunCount 2 *> $null

        $LASTEXITCODE | Should -Be 0
        @(Get-Content -LiteralPath (Join-Path $output 'invocations.txt')).Count | Should -Be 2
        Test-Path -LiteralPath (Join-Path $output 'verification-console.log') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $output 'verification-console-run-2.log') | Should -BeTrue
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.requestedRunCount | Should -Be 2
        $result.completedRunCount | Should -Be 2
        $result.consistentRuns | Should -BeTrue
        $result.verificationPassed | Should -BeTrue
    }

    It 'fails when a later run does not reproduce the same failure' {
        $verifier = Join-Path $TestDrive 'flaky-verifier.ps1'
        $output = Join-Path $TestDrive 'flaky'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Assert.Equal() Failure: Issue12345 expected red but was blue'
        New-Item -ItemType Directory -Path $output -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $output 'fail-after-first.flag') -Value 'x'

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
            -OutputDirectory $output `
            -RunCount 2 *> $null

        $LASTEXITCODE | Should -Not -Be 0
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.consistentRuns | Should -BeFalse
        $result.verificationPassed | Should -BeFalse
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

Describe 'Replication failure message determinism' {
    It 'ignores values that change between identical runs' {
        $first = 'Assert.Equal() Failure: expected 40 but was 0 at 0x7ffd1a2b after 132 ms'
        $second = 'Assert.Equal() Failure: expected 40 but was 0 at 0x00ab99cd after 87 ms'
        (Get-ReplicationVolatileFreeMessage -Value $first) |
            Should -BeExactly (Get-ReplicationVolatileFreeMessage -Value $second)
    }

    It 'keeps the measured values the assertion reports' {
        (Get-ReplicationVolatileFreeMessage -Value 'expected 40 but was 0') |
            Should -Not -BeExactly (Get-ReplicationVolatileFreeMessage -Value 'expected 40 but was 12')
    }

    It 'accepts a single run because there is nothing to compare' {
        Test-ReplicationFailureMessagesAreStable -Outcomes @(
            [pscustomobject]@{ ActualFailureMessage = 'expected 40 but was 0' }) |
            Should -BeTrue
    }

    It 'accepts runs that differ only in volatile detail' {
        Test-ReplicationFailureMessagesAreStable -Outcomes @(
            [pscustomobject]@{ ActualFailureMessage = 'expected 40 but was 0 in 12 ms' },
            [pscustomobject]@{ ActualFailureMessage = 'expected 40 but was 0 in 900 ms' }) |
            Should -BeTrue
    }

    It 'rejects runs whose measured values drift' {
        Test-ReplicationFailureMessagesAreStable -Outcomes @(
            [pscustomobject]@{ ActualFailureMessage = 'expected 40 but was 0' },
            [pscustomobject]@{ ActualFailureMessage = 'expected 40 but was 7' }) |
            Should -BeFalse
    }
}

Describe 'Replication test verification determinism enforcement' {
    It 'fails when every run is red but the reported value drifts' {
        $verifier = Join-Path $TestDrive 'drift-verifier.ps1'
        $output = Join-Path $TestDrive 'drift'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Issue12345 expected 40 but was 0' `
            -PerRunFailureMessages @(
                'Issue12345 expected 40 but was 0',
                'Issue12345 expected 40 but was 7')

        $stderr = & pwsh -NoProfile -File $scriptPath `
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
            -RunCount 2 `
            -OutputDirectory $output 2>&1

        $LASTEXITCODE | Should -Not -Be 0
        ($stderr | Out-String) | Should -Match 'not\s+deterministic'
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.stableFailureMessage | Should -BeFalse
        $result.verifierPassed | Should -BeTrue
        $result.verificationPassed | Should -BeFalse
    }

    It 'passes when repeated runs report the same measured value' {
        $verifier = Join-Path $TestDrive 'stable-verifier.ps1'
        $output = Join-Path $TestDrive 'stable'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Issue12345 expected 40 but was 0' `
            -PerRunFailureMessages @(
                'Issue12345 expected 40 but was 0 after 12 ms',
                'Issue12345 expected 40 but was 0 after 480 ms')

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
            -RunCount 2 `
            -OutputDirectory $output *> $null

        $LASTEXITCODE | Should -Be 0
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.stableFailureMessage | Should -BeTrue
        $result.verificationPassed | Should -BeTrue
    }
}
