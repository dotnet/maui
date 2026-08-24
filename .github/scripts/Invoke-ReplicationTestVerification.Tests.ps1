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

    # The signature comparison is shared with the trusted publisher so the two
    # cannot disagree, so it is loaded from that module rather than this script.
    $matchPath = Join-Path $PSScriptRoot 'shared/Get-ReplicationSignatureMatch.ps1'
    $matchErrors = $null
    $matchAst = [System.Management.Automation.Language.Parser]::ParseFile(
        $matchPath, [ref]$null, [ref]$matchErrors)
    if ($matchErrors) {
        throw ($matchErrors | ForEach-Object Message) -join [Environment]::NewLine
    }
    $searchRoots = @($ast, $matchAst)

    foreach ($name in @(
        'ConvertTo-NormalizedReplicationSignature',
        'Get-ReplicationSignatureTokens',
        'Test-ReplicationSignatureEquivalent',
        'Test-ReplicationExpectedFailureSignature',
        'Test-ReplicationControlFailureModeChanged',
        'Test-ReplicationInfrastructureFailure',
        'ConvertTo-AzdoSafeReplicationOutput',
        'ConvertTo-BoundedVerificationFailureMessage',
        'Get-ReplicationVolatileFreeMessage',
        'Test-ReplicationFailureMessagesAreStable'
    )) {
        $function = $null
        foreach ($root in $searchRoots) {
            $function = $root.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $name
            }, $true)
            if ($function) {
                break
            }
        }
        if (-not $function) {
            throw "The verification suite could not find function $name."
        }
        Invoke-Expression $function.Extent.Text
    }

    function New-ReplicationVerifierStub {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$ActualFailureMessage,
            [int]$ExecutedTestCount = -1,
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
`$machineResult = [ordered]@{
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
}
if ($ExecutedTestCount -ge 0) {
    `$machineResult['executedTestCount'] = $ExecutedTestCount
}
`$machineResult | ConvertTo-Json | Set-Content -LiteralPath `$MachineResultPath -Encoding utf8NoBOM
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

    It 'rejects a run whose filter selected more than one test' {
        $verifier = Join-Path $TestDrive 'ambiguous-verifier.ps1'
        $output = Join-Path $TestDrive 'ambiguous'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Assert.Equal() Failure: Issue12345 expected red but was blue' `
            -ExecutedTestCount 2

        $stderr = & pwsh -NoProfile -File $scriptPath `
            -IssueNumber 12345 `
            -Platform android `
            -TestType UITest `
            -TestFilter 'FullyQualifiedName~Issue12345' `
            -TestProject Controls.TestCases.Shared.Tests `
            -TestProjectPath src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj `
            -TestClass Microsoft.Maui.TestCases.Tests.Issue12345 `
            -TestMethod ReproducesIssue12345 `
            -ExpectedFailureSignature Issue12345 `
            -VerifierPath $verifier `
            -OutputDirectory $output `
            -RunCount 2 2>&1

        $LASTEXITCODE | Should -Not -Be 0
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.selectionAmbiguous | Should -BeTrue
        $result.executedTestCounts | Should -Be @(2)
        $result.verificationPassed | Should -BeFalse
        ($stderr | Out-String) | Should -Match 'cannot be attributed to the named test'
    }

    It 'accepts a run whose filter selected exactly one test' {
        $verifier = Join-Path $TestDrive 'exact-one-verifier.ps1'
        $output = Join-Path $TestDrive 'exact-one'
        New-ReplicationVerifierStub `
            -Path $verifier `
            -ActualFailureMessage 'Assert.Equal() Failure: Issue12345 expected red but was blue' `
            -ExecutedTestCount 1

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
        $result = Get-Content -LiteralPath (Join-Path $output 'verification-result.json') -Raw |
            ConvertFrom-Json
        $result.selectionAmbiguous | Should -BeFalse
        $result.executedTestCounts | Should -Be @(1)
        $result.verificationPassed | Should -BeTrue
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

Describe 'The harness verdict decides whether a test was actually verified' {
    It 'treats an Appium session that never started as infrastructure, not a failed test' {
        # Live run 15006864 reported infrastructureFailure=False for this exact
        # output, so a UI test that never ran was returned to the agent as a
        # test that verified wrongly.
        $output = @'
  ⚠️ Environment error (attempt 1/3): Appium app/session did not initialize (InitialSetup/OneTimeSetup failed — test agent could not start the Appium session) — retrying in 30s...
  ⚠️ Environment error persisted after 3 attempts: Appium app/session did not initialize
  🖥️ [UITest] Issue36422: ⚠️ ENV ERROR — Appium app/session did not initialize
║              VERIFICATION INCONCLUSIVE ⚠️                  ║
║  Could not verify the test(s) — env/build/parse error.    ║
'@
        Test-ReplicationInfrastructureFailure -Content $output | Should -BeTrue
    }

    It 'recognises the harness inconclusive banner on its own' {
        Test-ReplicationInfrastructureFailure `
            -Content 'VERIFICATION INCONCLUSIVE' | Should -BeTrue
    }

    It 'still calls a genuine assertion failure a real result' {
        $output = @'
  Failed Issue36422_TitleIsVisible [412 ms]
  Error Message:
   Expected: True
  But was:  False
'@
        Test-ReplicationInfrastructureFailure -Content $output | Should -BeFalse
    }
}

Describe 'Replication negative control' {
    BeforeAll {
        $script:ControlScriptPath = Join-Path $PSScriptRoot 'shared/Invoke-ReplicationTestVerification.ps1'

        function New-ReplicationControlStub {
            <#
                .SYNOPSIS
                A verifier that reports the targeted test running and passing.
            #>
            param(
                [Parameter(Mandatory = $true)][string]$Path,
                [switch]$Infrastructure,
                [switch]$NeverClears,
                [string]$FailureMessage = '',
                [int]$PassUntilRun = 0
            )

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
Add-Content -LiteralPath (Join-Path (Split-Path -Parent `$MachineResultPath) 'invocations.txt') -Value 'run' -Encoding utf8NoBOM
`$run = @(Get-Content -LiteralPath (Join-Path (Split-Path -Parent `$MachineResultPath) 'invocations.txt')).Count
if ('$Infrastructure' -eq 'True') {
    Write-Host 'VERIFY FAILURE ONLY MODE'
    Write-Host 'error CS0103: The name does not exist in the current context'
    Write-Host 'VERIFICATION FAILED'
    exit 1
}
if ('$NeverClears' -eq 'True' -or ($PassUntilRun -gt 0 -and `$run -gt $PassUntilRun)) {
    if ('$FailureMessage' -ne '') {
        `$machineFilter = if (`$TestType -in @('UnitTest', 'XamlUnitTest')) {
            "FullyQualifiedName=`$TestClass.`$TestMethod"
        } else {
            `$TestFilter
        }
        [ordered]@{
            schemaVersion = 1
            failed = `$true
            testType = `$TestType
            testFilter = `$machineFilter
            testProject = `$TestProject
            testProjectPath = `$TestProjectPath
            testClass = `$TestClass
            testMethod = `$TestMethod
            actualFailureMessage = '$FailureMessage'
        } | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath `$MachineResultPath -Encoding utf8NoBOM
    }
    Write-Host 'VERIFY FAILURE ONLY MODE'
    Write-Host "[`$TestType] `$TestFilter FAILED"
    Write-Host 'VERIFICATION PASSED'
    exit 0
}
Write-Host 'VERIFY FAILURE ONLY MODE'
Write-Host 'VERIFICATION FAILED'
Write-Host '1/1 test(s) PASSED but should FAIL!'
exit 1
"@ | Set-Content -LiteralPath $Path -Encoding utf8NoBOM
        }

        function Invoke-ReplicationControl {
            param(
                [Parameter(Mandatory = $true)][string]$Verifier,
                [Parameter(Mandatory = $true)][string]$Output,
                [int]$RunCount = 2
            )

            & pwsh -NoProfile -File $script:ControlScriptPath `
                -IssueNumber 12345 `
                -Platform android `
                -TestType UnitTest `
                -TestFilter Issue12345 `
                -TestProject Controls.Core.UnitTests `
                -TestProjectPath src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj `
                -TestClass Microsoft.Maui.Controls.Tests.Issue12345Tests `
                -TestMethod ReproducesIssue12345 `
                -ExpectedFailureSignature Issue12345 `
                -VerifierPath $Verifier `
                -OutputDirectory $Output `
                -RunCount $RunCount `
                -ExpectPass *>&1 | Out-String
        }
    }

    It 'records a control that passes in every run' {
        $verifier = Join-Path $TestDrive 'control-pass.ps1'
        $output = Join-Path $TestDrive 'control-pass-out'
        New-ReplicationControlStub -Path $verifier

        Invoke-ReplicationControl -Verifier $verifier -Output $output | Out-Null
        $LASTEXITCODE | Should -Be 0

        $control = Get-Content -LiteralPath (Join-Path $output 'negative-control-result.json') -Raw |
            ConvertFrom-Json
        $control.runCount | Should -Be 2
        $control.passCount | Should -Be 2
        $control.infrastructureFailure | Should -BeFalse
    }

    It 'never writes the baseline verification result from a control run' {
        # The control shares the reproduction's output directory shape, so a
        # control that overwrote verification-result.json would replace the
        # evidence of the red baseline with evidence of a green one.
        $verifier = Join-Path $TestDrive 'control-isolated.ps1'
        $output = Join-Path $TestDrive 'control-isolated-out'
        New-ReplicationControlStub -Path $verifier

        Invoke-ReplicationControl -Verifier $verifier -Output $output | Out-Null

        Test-Path -LiteralPath (Join-Path $output 'verification-result.json') | Should -BeFalse
    }

    It 'calls a control that passes once and fails once inconsistent, not refuting' {
        # This stub passes run 1 and fails run 2. Before the control ran to
        # completion that looked identical to a refutation, because the loop
        # stopped on the red run and graded a single sample. A control that
        # disagrees with itself measures flakiness, not attribution.
        $verifier = Join-Path $TestDrive 'control-red.ps1'
        $output = Join-Path $TestDrive 'control-red-out'
        New-ReplicationControlStub -Path $verifier -PassUntilRun 1

        $log = Invoke-ReplicationControl -Verifier $verifier -Output $output
        $LASTEXITCODE | Should -Not -Be 0
        $log | Should -Match 'inconsistent'
        $log | Should -Not -Match 'does not depend'

        $control = Get-Content -LiteralPath (Join-Path $output 'negative-control-result.json') -Raw |
            ConvertFrom-Json
        $control.runCount | Should -Be 2
        $control.passCount | Should -Be 1
    }

    It 'rejects a control that stays red across every requested run' {
        # A control that stays red proves the reproduction does not depend on
        # the reported trigger, which is the case worth catching. It is only
        # trustworthy once every requested run has been observed.
        $verifier = Join-Path $TestDrive 'control-all-red.ps1'
        $output = Join-Path $TestDrive 'control-all-red-out'
        New-ReplicationControlStub -Path $verifier -NeverClears `
            -FailureMessage 'Expected 10 but was 0'
        # Refutation needs both sides of the comparison: the control failing for
        # the same reason the reproduction did. Without the reproduction's own
        # message there is nothing to compare against, and the run is correctly
        # reported as unattributable rather than as a refutation.
        New-Item -ItemType Directory -Path $output -Force | Out-Null
        [ordered]@{
            schemaVersion = 1
            observedFailureMessages = @('Expected 10 but was 0')
        } | ConvertTo-Json -Depth 5 |
            Set-Content -LiteralPath (Join-Path $output 'verification-result.json') -Encoding utf8NoBOM

        $log = Invoke-ReplicationControl -Verifier $verifier -Output $output
        $LASTEXITCODE | Should -Not -Be 0
        $log | Should -Match 'does not depend'

        $control = Get-Content -LiteralPath (Join-Path $output 'negative-control-result.json') -Raw |
            ConvertFrom-Json
        # The whole point of the change: the refutation rests on both runs, not
        # on the first one to come back red.
        $control.runCount | Should -Be 2
        $control.passCount | Should -Be 0
    }

    It 'reports a red control with nothing to compare as unattributable, not a refutation' {
        # The failure-mode check answers 'not changed' when either side is
        # unknown, so before the guard this run fell through to the refutation
        # and destroyed a reproduction on a comparison that was never made.
        $verifier = Join-Path $TestDrive 'control-no-message.ps1'
        $output = Join-Path $TestDrive 'control-no-message-out'
        New-ReplicationControlStub -Path $verifier -NeverClears

        $log = Invoke-ReplicationControl -Verifier $verifier -Output $output
        $LASTEXITCODE | Should -Not -Be 0
        $log | Should -Match 'no comparable'
        $log | Should -Not -Match 'does not depend'
    }

    It 'refuses to count a build break as a passing control' {
        # "Did not fail" and "passed" are the same observation to a failure-only
        # verifier, so a control that never compiled must not certify anything.
        $verifier = Join-Path $TestDrive 'control-infra.ps1'
        $output = Join-Path $TestDrive 'control-infra-out'
        New-ReplicationControlStub -Path $verifier -Infrastructure

        $log = Invoke-ReplicationControl -Verifier $verifier -Output $output
        $LASTEXITCODE | Should -Not -Be 0
        $log | Should -Match 'build or infrastructure'

        $control = Get-Content -LiteralPath (Join-Path $output 'negative-control-result.json') -Raw |
            ConvertFrom-Json
        $control.passCount | Should -Be 0
        $control.infrastructureFailure | Should -BeTrue
    }
}

Describe 'The negative control keeps its own console evidence' {
    It 'never writes the reproduction console names in control mode' {
        # Sharing the output directory is deliberate so the gate can compare the
        # two arms, which means the control must not overwrite the arm it
        # corroborates.
        $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'shared/Invoke-ReplicationTestVerification.ps1') -Raw
        $source | Should -Match "\`$consolePrefix = if \(\`$ExpectPass\) \{ 'negative-control-console' \}"
        $source | Should -Match '"\$consolePrefix\.log"'
        $source | Should -Match '"\$consolePrefix-run-\$run\.log"'
    }
}


Describe 'Telling a refuted reproduction apart from a broken control' {
    It 'reports a refutation when the control failed the same way' {
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('Expected 40 but was 20') `
            -ReproductionMessages @('Expected 40 but was 20') |
            Should -BeFalse
    }

    It 'reports a changed mode when the control failed a different way' {
        # The reproduction measured a wrong size; the control could not even
        # find the element. Nothing about attribution follows from that.
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('Element BugLabel was not found') `
            -ReproductionMessages @('Expected 40 but was 20') |
            Should -BeTrue
    }

    It 'reports a refutation when any control run matched the reproduction' {
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('Element BugLabel was not found', 'Expected 40 but was 20') `
            -ReproductionMessages @('Expected 40 but was 20') |
            Should -BeFalse
    }

    It 'reports a refutation when the reproduction itself varied and the control matched one' {
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('Expected 40 but was 20') `
            -ReproductionMessages @('Expected 40 but was 20', 'Expected 40 but was 21') |
            Should -BeFalse
    }

    It 'never claims a changed mode when the control reported nothing' {
        # A control that passed has no failure message at all, and a control
        # with no message is an absent measurement, not a new failure mode.
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @() `
            -ReproductionMessages @('Expected 40 but was 20') |
            Should -BeFalse
    }

    It 'never claims a changed mode when the reproduction message is unknown' {
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('Element BugLabel was not found') `
            -ReproductionMessages @() |
            Should -BeFalse
    }

    It 'ignores empty strings on either side rather than matching on them' {
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('', 'Element BugLabel was not found') `
            -ReproductionMessages @('', 'Expected 40 but was 20') |
            Should -BeTrue
    }

    It 'treats a control whose only message is blank as an absent measurement' {
        # A blank message means the run told us nothing, not that it failed a
        # new way. Calling it a changed mode would spend a control round
        # rewriting an edit that may have been correct.
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('') `
            -ReproductionMessages @('Expected 40 but was 20') |
            Should -BeFalse
    }

    It 'treats a reproduction whose only message is blank as an absent measurement' {
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('Element BugLabel was not found') `
            -ReproductionMessages @('') |
            Should -BeFalse
    }

    It 'is not fooled by a message that merely contains the other' {
        # Substring similarity is not sameness: an element-lookup failure that
        # happens to quote the expected value is still a different failure.
        Test-ReplicationControlFailureModeChanged `
            -ControlMessages @('Element not found while asserting Expected 40 but was 20') `
            -ReproductionMessages @('Expected 40 but was 20') |
            Should -BeTrue
    }
}

Describe 'A refutation is measured across every requested run' {
    It 'stops early only when verifying the reproduction, never the control' {
        # The reproduction repeats nothing after a red run because the
        # orchestrator repairs the test and starts over. The control must not
        # share that shortcut: its red run is evidence, and one sample of it was
        # enough to destroy 43 sound reproductions in a single wave.
        $script:Source | Should -Match '\$runSucceeded -and -not \$ExpectPass'
        $breakIndex = $script:Source.IndexOf('$runSucceeded -and -not $ExpectPass')
        $breakIndex | Should -BeGreaterThan 0
    }

    It 'keeps running the control after a red run' {
        $script:Source | Should -Match 'the control keeps going'
        $guard = $script:Source.IndexOf('$runSucceeded -and -not $ExpectPass')
        $continue = $script:Source.IndexOf('the control keeps going')
        $continue | Should -BeGreaterThan $guard
    }

    It 'refuses to refute when neither side recorded a message to compare' {
        # Refutation means the control stayed red for the *same* reason. The
        # failure-mode check reports 'not changed' when either side is unknown,
        # so without this the unknown case falls straight through to the
        # refutation and destroys a reproduction on a comparison never made.
        $script:Source | Should -Match '\$controlFailureMessages\.Count -eq 0 -or \$reproductionMessages\.Count -eq 0'
        $script:Source | Should -Match 'no comparable failure message was recorded'

        $guard = $script:Source.IndexOf('no comparable failure message was recorded')
        $refutation = $script:Source.IndexOf('therefore does not depend')
        $guard | Should -BeGreaterThan 0
        $refutation | Should -BeGreaterThan $guard
    }

    It 'calls an unfinished control unmeasured rather than refuting' {
        $script:Source | Should -Match 'completed only \$controlRuns of \$RunCount run\(s\)'
        $script:Source | Should -Match 'never measured'
    }

    It 'calls a control that disagrees with itself inconsistent rather than refuting' {
        $script:Source | Should -Match '\$controlPasses -gt 0 -and \$controlPasses -lt \$RunCount'
        $script:Source | Should -Match 'negative control is inconsistent'
    }

    It 'reaches the refutation only after both unmeasured cases are excluded' {
        $incomplete = $script:Source.IndexOf('completed only $controlRuns of $RunCount run(s)')
        $inconsistent = $script:Source.IndexOf('negative control is inconsistent')
        $refutation = $script:Source.IndexOf('therefore does not depend')
        $incomplete | Should -BeGreaterThan 0
        $inconsistent | Should -BeGreaterThan 0
        $refutation | Should -BeGreaterThan 0
        $incomplete | Should -BeLessThan $refutation
        $inconsistent | Should -BeLessThan $refutation
    }

    It 'no longer lets an incomplete run count trigger the refutation message' {
        # The defect: '$controlRuns -ne $RunCount' sat in the refutation
        # condition, so a control that completed 1 of 3 runs was reported as
        # proof that the reproduction does not depend on the trigger.
        $script:Source | Should -Not -Match '\$controlRuns -ne \$RunCount -or \$controlPasses -ne \$RunCount'
    }
}
