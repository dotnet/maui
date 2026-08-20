#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for Get-TestResultFromOutput build-error classification in
    verify-tests-fail.ps1.

    A test whose log shows a COMPILE error (any `error <ABBR><NNNN>` — MAUIX, CS, MSB,
    NETSDK, XA, NU, …) never produced a runnable test result, so the gate must classify it
    as a build error (-> INCONCLUSIVE), not as a failing test (-> FAILED). This guards the
    fix for the net11 Controls.Xaml.UnitTests MAUIX2017 baseline break, where an unrelated
    fixture (Bz40906.xaml) fails to compile and takes the whole assembly down.
.EXAMPLE
    Invoke-Pester ./Verify-TestsFail.Tests.ps1
#>

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'verify-tests-fail.ps1'
    $script:VerifierSource = Get-Content -LiteralPath $scriptPath -Raw
    $script:verifyScriptText = Get-Content -Raw -LiteralPath $scriptPath
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($fnName in @('Get-GateDeviceTestConfiguration', 'Limit-ExpensiveGateTests', 'Get-GateTestDetectionParameters', 'Get-TargetedTestFailureMessage', 'Get-TestResultFromOutput', 'Get-SnapshotDiffMap', 'Test-SnapshotEnvironmentalResidual', 'Write-MarkdownReport', 'Test-BuildErrorIsInDetectedTest', 'Test-FixIrrelevantToPlatform', 'Format-GateLogExcerpt', 'Test-IsWindowsDeviceNoResultsError', 'Test-IsWindowsDeviceTargetTimeoutError', 'Convert-WindowsBaselineNoResultsToFailure', 'Convert-WindowsTargetTimeoutToFailure', 'Test-GateHasDefinitiveFailure', 'Invoke-TestRunWithRetry', 'Get-HostOnlyTargetFrameworkArgs')) {
        $fn = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $fnName
        }, $true)
        if (-not $fn) { throw "Function '$fnName' not found" }
        Invoke-Expression $fn.Extent.Text
    }

    $autoDetectionFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Get-AutoDetectedTests'
    }, $true)
    if (-not $autoDetectionFunction) { throw "Function 'Get-AutoDetectedTests' not found" }
    Invoke-Expression $autoDetectionFunction.Extent.Text

    $invokeTestRunFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Invoke-TestRun'
    }, $true)
    if (-not $invokeTestRunFunction) { throw "Function 'Invoke-TestRun' not found" }
    $script:invokeTestRunText = $invokeTestRunFunction.Extent.Text

    function New-LogFile {
        param([string]$Content)
        $f = Join-Path ([System.IO.Path]::GetTempPath()) ("verifylog-" + [Guid]::NewGuid().ToString('N') + ".log")
        $Content | Set-Content -LiteralPath $f -Encoding UTF8
        return $f
    }

    function Invoke-TestRun {
        throw 'Invoke-TestRun must be mocked by tests that exercise retry orchestration.'
    }
}

Describe 'Get-GateDeviceTestConfiguration — platform-safe packaging' {
    It 'uses Release for Android so the standalone XHarness APK contains its managed payload' {
        Get-GateDeviceTestConfiguration -DevicePlatform 'android' | Should -Be 'Release'
    }

    It 'uses Release for Windows' {
        Get-GateDeviceTestConfiguration -DevicePlatform 'windows' | Should -Be 'Release'
    }

    It 'keeps Apple device-test Gates on Debug to avoid full ILLink trimming' {
        Get-GateDeviceTestConfiguration -DevicePlatform 'ios' | Should -Be 'Debug'
        Get-GateDeviceTestConfiguration -DevicePlatform 'maccatalyst' | Should -Be 'Debug'
    }
}

Describe 'Gate test detection snapshot pinning' {
    It 'prefers committed snapshot files over the live PR number' {
        $params = Get-GateTestDetectionParameters `
            -MergeBase '0123456789abcdef' `
            -ChangedFiles @('src/Core/tests/DeviceTests/PickerTests.cs') `
            -PullRequestNumber '37232'

        $params.DiffBase | Should -Be '0123456789abcdef'
        @($params.ChangedFiles) | Should -Be @('src/Core/tests/DeviceTests/PickerTests.cs')
        $params.ContainsKey('PRNumber') | Should -BeFalse
    }

    It 'falls back to live PR metadata only without a usable local snapshot' {
        $params = Get-GateTestDetectionParameters `
            -MergeBase '' `
            -ChangedFiles @() `
            -PullRequestNumber '37232'

        $params.PRNumber | Should -Be '37232'
        $params.ContainsKey('DiffBase') | Should -BeFalse
    }

    It 'keeps an empty committed snapshot authoritative' {
        $params = Get-GateTestDetectionParameters `
            -MergeBase '0123456789abcdef' `
            -ChangedFiles @() `
            -PullRequestNumber '37232'

        $params.DiffBase | Should -Be '0123456789abcdef'
        @($params.ChangedFiles).Count | Should -Be 0
        $params.ContainsKey('PRNumber') | Should -BeFalse
    }
}

Describe 'Targeted failure message extraction' {
    It 'returns only the exact target assertion message, not names or sibling failures' {
        $log = New-LogFile @'
[UnitTest] Issue12345 FAILED
  Failed Microsoft.Maui.Tests.Issue12345Tests.ReproducesIssue [12 ms]
  Error Message:
   Xunit.Sdk.EqualException: expected red but was blue
  Stack Trace:
     at Microsoft.Maui.Tests.Issue12345Tests.ReproducesIssue()
  Failed Microsoft.Maui.Tests.OtherTests.Unrelated [8 ms]
  Error Message:
   Issue12345 appeared only in an unrelated sibling assertion
  Stack Trace:
     at Microsoft.Maui.Tests.OtherTests.Unrelated()
Total tests: 2
'@

        Get-TargetedTestFailureMessage `
            -LogFile $log `
            -TargetClass 'Microsoft.Maui.Tests.Issue12345Tests' `
            -TargetMethod 'ReproducesIssue' |
            Should -BeExactly 'Xunit.Sdk.EqualException: expected red but was blue'
        Remove-Item -LiteralPath $log -Force
    }

    It 'uses trusted project and class/method metadata for replication-scoped entries' {
        $script:VerifierSource |
            Should -Match 'Project\s*=\s*if \(\$MachineResultPath\) \{ \$TestProject \}'
        $script:VerifierSource |
            Should -Match 'ProjectPath\s*=\s*if \(\$MachineResultPath\) \{ \$TestProjectPath \}'
        $script:VerifierSource |
            Should -Match 'ClassFilter\s*=\s*if \(\$MachineResultPath\) \{ \$TestClass \}'
        $script:VerifierSource |
            Should -Match 'Methods\s*=\s*if \(\$MachineResultPath\) \{ @\(\$TestMethod\) \}'
        $script:VerifierSource |
            Should -Match '"FullyQualifiedName=\$TestClass\.\$TestMethod"'
        $script:VerifierSource |
            Should -Match '\$deviceTestScript\s*=\s*\$resolvedDeviceTestScriptPath'
        $script:VerifierSource |
            Should -Match 'RepositoryRoot\s*=\s*\$RepoRoot'
    }

    It 'reads only the exact device target failure message from scoped xUnit XML' {
        $RepoRoot = [IO.Path]::GetTempPath()
        $log = New-LogFile '[DeviceTest] Issue12345 FAILED'
        $diagnostics = "$log.diagnostics"
        New-Item -ItemType Directory -Path $diagnostics | Out-Null
        $resultFile = Join-Path $diagnostics 'device-target-results.xml'
        @'
<assemblies>
  <assembly>
    <collection>
      <test type="Microsoft.Maui.DeviceTests.Issue12345Tests" method="ReproducesIssue" result="Fail">
        <failure exception-type="Xunit.Sdk.EqualException">
          <message><![CDATA[Expected red but was blue]]></message>
          <stack-trace><![CDATA[at target]]></stack-trace>
        </failure>
      </test>
      <test type="Microsoft.Maui.DeviceTests.OtherTests" method="Unrelated" result="Fail">
        <failure exception-type="Xunit.Sdk.TrueException">
          <message><![CDATA[Issue12345 appeared only in a sibling failure]]></message>
        </failure>
      </test>
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content -LiteralPath $resultFile -Encoding utf8NoBOM

        Get-TargetedTestFailureMessage `
            -LogFile $log `
            -TargetClass 'Microsoft.Maui.DeviceTests.Issue12345Tests' `
            -TargetMethod 'ReproducesIssue' `
            -TargetTestType DeviceTest `
            -TargetFilter Issue12345 |
            Should -BeExactly 'Expected red but was blue'

        Remove-Item -LiteralPath $log -Force
        Remove-Item -LiteralPath $diagnostics -Recurse -Force
    }

    It 'prefers the newest device result when repair attempts share diagnostics' {
        $RepoRoot = [IO.Path]::GetTempPath()
        $log = New-LogFile '[DeviceTest] Issue12345 FAILED'
        $diagnostics = "$log.diagnostics"
        $oldDirectory = Join-Path $diagnostics 'xharness-run-old'
        $newDirectory = Join-Path $diagnostics 'xharness-run-new'
        New-Item -ItemType Directory -Path $oldDirectory, $newDirectory | Out-Null

        $oldResult = Join-Path $oldDirectory 'xunit-test-old.xml'
        $newResult = Join-Path $newDirectory 'xunit-test-new.xml'
        foreach ($result in @(
            @{ Path = $oldResult; Message = 'System.InvalidCastException : Specified cast is not valid.' },
            @{ Path = $newResult; Message = 'Expected centered item index to remain 5 after orientation change, but was 4.' }
        )) {
            @"
<assemblies>
  <assembly>
    <collection>
      <test type="Microsoft.Maui.DeviceTests.Issue12345Tests" method="ReproducesIssue" result="Fail">
        <failure><message>$($result.Message)</message></failure>
      </test>
    </collection>
  </assembly>
</assemblies>
"@ | Set-Content -LiteralPath $result.Path -Encoding utf8NoBOM
        }
        (Get-Item -LiteralPath $oldResult).LastWriteTimeUtc = [DateTime]::UtcNow.AddMinutes(-1)
        (Get-Item -LiteralPath $newResult).LastWriteTimeUtc = [DateTime]::UtcNow

        Get-TargetedTestFailureMessage `
            -LogFile $log `
            -TargetClass 'Microsoft.Maui.DeviceTests.Issue12345Tests' `
            -TargetMethod 'ReproducesIssue' `
            -TargetTestType DeviceTest `
            -TargetFilter Issue12345 |
            Should -BeExactly 'Expected centered item index to remain 5 after orientation change, but was 4.'

        Remove-Item -LiteralPath $log -Force
        Remove-Item -LiteralPath $diagnostics -Recurse -Force
    }

    It 'reads the exact UI target exception message from the authoritative TRX' {
        $RepoRoot = Join-Path ([IO.Path]::GetTempPath()) ([guid]::NewGuid().ToString('N'))
        $trxPath = Join-Path $RepoRoot 'CustomAgentLogsTmp/UITests/TestResults/Issue12345.trx'
        New-Item -ItemType Directory -Path (Split-Path -Parent $trxPath) -Force |
            Out-Null
        @'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Results>
    <UnitTestResult testName="Microsoft.Maui.UITests.Issue12345Tests.ReproducesIssue" outcome="Failed">
      <Output>
        <ErrorInfo>
          <Message>Expected red but was blue</Message>
          <StackTrace>at target</StackTrace>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
    <UnitTestResult testName="Microsoft.Maui.UITests.OtherTests.Unrelated" outcome="Failed">
      <Output>
        <ErrorInfo>
          <Message>Issue12345 appeared only in a sibling failure</Message>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
  </Results>
</TestRun>
'@ | Set-Content -LiteralPath $trxPath -Encoding utf8NoBOM
        $log = New-LogFile '[UITest] Issue12345 FAILED'

        Get-TargetedTestFailureMessage `
            -LogFile $log `
            -TargetClass 'Microsoft.Maui.UITests.Issue12345Tests' `
            -TargetMethod 'ReproducesIssue' `
            -TargetTestType UITest `
            -TargetFilter Issue12345 |
            Should -BeExactly 'Expected red but was blue'

        Remove-Item -LiteralPath $log -Force
        Remove-Item -LiteralPath $RepoRoot -Recurse -Force
    }
}

Describe 'Invoke-TestRun — host-only target frameworks' {
    It 'applies the shared platform exclusions to unit and XAML unit tests' {
        ([regex]::Matches($script:invokeTestRunText, '\+\s*\$hostOnlyTargetFrameworkArgs')).Count | Should -Be 2
        ([regex]::Matches($script:invokeTestRunText, '-p:TreatWarningsAsErrors=false')).Count | Should -Be 2
        @(Get-HostOnlyTargetFrameworkArgs) | Should -Be @(
            '-p:IncludeAndroidTargetFrameworks=false',
            '-p:IncludeIosTargetFrameworks=false',
            '-p:IncludeMacCatalystTargetFrameworks=false',
            '-p:IncludeWindowsTargetFrameworks=false',
            '-p:IncludeTizenTargetFrameworks=false'
        )
    }

    It 'applies the same platform exclusions to both clean-rebuild retry commands' {
        $retryStart = $script:verifyScriptText.IndexOf(
            '# ── Clean-rebuild retry for with-fix-only build errors')
        $retryEnd = $script:verifyScriptText.IndexOf(
            '# Combine into a single summary for backward compatibility',
            $retryStart)
        $retryStart | Should -BeGreaterOrEqual 0
        $retryEnd | Should -BeGreaterThan $retryStart
        $retryText = $script:verifyScriptText.Substring(
            $retryStart,
            $retryEnd - $retryStart)

        ([regex]::Matches(
            $retryText,
            '\+\s*\$hostOnlyTargetFrameworkArgs')).Count |
            Should -Be 2
        $retryText | Should -Match (
            '\$hostOnlyTargetFrameworkArgs\s*=\s*Get-HostOnlyTargetFrameworkArgs')
    }
}

Describe 'Get-TestResultFromOutput — build error classification' {
    It 'flags the net11 Xaml.UnitTests MAUIX2017 baseline break as a build error (not a test failure)' {
        $log = New-LogFile @"
  Controls.Xaml -> /a/b/Microsoft.Maui.Controls.Xaml.dll
/s/src/Controls/tests/Xaml.UnitTests/Issues/Bz40906.xaml(6,4): error MAUIX2017: Property 'ContentPage.Content' is being set multiple times. Only the last value will be used. [/s/src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj]
Build FAILED.
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.BuildError | Should -BeTrue
        $r.Passed | Should -BeFalse
        $r.Error | Should -Match 'MAUIX2017'
        Remove-Item -LiteralPath $log -Force
    }

    It 'spends the excerpt budget on the message, not the agent directory' {
        # Catalyst build 15031426 burned five attempts on the diagnosis
        # "'TestDevice' could not be found (are you missing a u" because 120 of
        # the 200 characters went to a path prefix the reader already knows.
        $log = New-LogFile @"
Build FAILED.
/Users/cloudtest/vss/_work/1/s/src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue30163.cs(9,20): error CS0246: The type or namespace name 'TestDevice' could not be found (are you missing a using directive or an assembly reference?) [/Users/cloudtest/vss/_work/1/s/src/Controls/tests/TestCases.Shared.Tests/Controls.TestCases.Shared.Tests.csproj]
"@
        $r = Get-TestResultFromOutput -LogFile $log

        $r.BuildError | Should -BeTrue
        $r.Error | Should -Match 'Issue30163\.cs\(9,20\)'
        $r.Error | Should -Match 'are you missing a using directive or an assembly reference'
        $r.Error | Should -Not -Match '_work'
        $r.Error | Should -Not -Match 'csproj'
        Remove-Item -LiteralPath $log -Force
    }

    It 'flags CS / MSB / NETSDK / XA compile errors as build errors' {
        foreach ($err in @(
            "error CS0234: The type or namespace name 'CodeAnalysis' does not exist",
            "error MSB3073: The command exited with code 1",
            "error NETSDK1005: Assets file doesn't have a target for 'net11.0-android'",
            "error XA4210: missing UsesLibrary"
        )) {
            $log = New-LogFile "Build FAILED.`n$err"
            (Get-TestResultFromOutput -LogFile $log).BuildError | Should -BeTrue
            Remove-Item -LiteralPath $log -Force
        }
    }

    It 'does NOT flag a clean passing run as a build error' {
        $log = New-LogFile "Build succeeded.`n    0 Error(s)`n  Passed:  57`n  Failed:  0"
        $r = Get-TestResultFromOutput -LogFile $log
        $r.BuildError | Should -Not -BeTrue
        $r.Passed | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'does NOT flag a warning-only build as a build error' {
        $log = New-LogFile @"
Gh2517.xaml(6,13): warning MAUIG2045: Binding: Property "MissingProperty" not found
Build succeeded.
    0 Error(s)
  Passed:  10
  Failed:  0
"@
        (Get-TestResultFromOutput -LogFile $log).BuildError | Should -Not -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'still reports a genuine ran-and-failed test as a failure, not a build error' {
        $log = New-LogFile "Build succeeded.`n    0 Error(s)`n  Passed:  3`n  Failed:  2"
        $r = Get-TestResultFromOutput -LogFile $log
        $r.BuildError | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Get-TestResultFromOutput — filter mismatch classification' {
    # A -filter that matches 0 test cases means the deciding test never ran, so the gate
    # verified nothing. The parser MUST flag this as FilterMismatch (not EnvError, not
    # BuildError, not a plain FAIL) because the verdict logic routes FilterMismatch to
    # INCONCLUSIVE via $gateInfraError (guarded by $withFixGenuineFailCount -eq 0). Without
    # this contract a platform-gated test — e.g. one excluded on Android by a category or
    # #if TEST_FAILS_ON_ANDROID — falsely blocks the PR. Guards real build 14634904
    # (#35998 android, Issue26049: both runs "No test matches ... 'Issue26049'").
    It 'flags "No test matches the given testcase filter" as FilterMismatch (real 14634904 #35998 Issue26049)' {
        $log = New-LogFile @'
  A total of 1 test files matched the specified pattern.
No test matches the given testcase filter `Issue26049` in /a/b/Controls.TestCases.Android.Tests.dll
'@
        $r = Get-TestResultFromOutput -LogFile $log -TestFilter 'Issue26049'
        $r.FilterMismatch | Should -BeTrue
        $r.Passed        | Should -BeFalse
        $r.EnvError      | Should -Not -BeTrue
        $r.BuildError    | Should -Not -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'extracts the single-quoted filter name from the runner message' {
        $log = New-LogFile "No test matches the given testcase filter 'SomeMissingTest' in x.dll"
        $r = Get-TestResultFromOutput -LogFile $log
        $r.FilterMismatch | Should -BeTrue
        $r.Error | Should -Match 'SomeMissingTest'
        Remove-Item -LiteralPath $log -Force
    }

    It 'flags "Test count: 0" as FilterMismatch' {
        $log = New-LogFile "Starting test execution, please wait...`nTest count: 0"
        (Get-TestResultFromOutput -LogFile $log -TestFilter 'X').FilterMismatch | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Get-TestResultFromOutput — environment/infra classification' {
    # These lock in the campaign's env-class fixes: an Appium/Selenium fixture setup flake or
    # a brand-new snapshot with no committed baseline is NOT a fix failure — the gate could
    # not verify, so it must be EnvError (-> INCONCLUSIVE), never a plain FAIL that blocks.
    It 'flags an Appium OneTimeSetUp Selenium error as an env error (real #27477 Issue19752)' {
        $log = New-LogFile @'
OneTimeSetUp: OpenQA.Selenium.UnknownErrorException : An unknown server-side error occurred while processing the command. Original error: The app representing com.microsoft.maui.uitests could not be found.
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.Passed   | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }

    It 'flags "Call InitialSetup before accessing the App property" as an env error' {
        $log = New-LogFile "System.InvalidOperationException : Call InitialSetup before accessing the App property"
        (Get-TestResultFromOutput -LogFile $log).EnvError | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'flags a fixture-wide OneTimeSetUp app-launch/crash-recovery timeout as env, even WITH failure counts (real #35640 build 14844563)' {
        # The app crashed on launch and never recovered, so every test in the fixture failed at
        # OneTimeSetUp before any assertion ran (Passed=0/Failed=N). This must be EnvError
        # (INCONCLUSIVE), never a plain FAIL — the fix was never actually verified.
        $log = New-LogFile @'
  Passed: 0
  Failed: 17
OneTimeSetUp: System.TimeoutException : Timed out waiting for Go To Test button to appear (the app did not recover after crash-recovery attempts)
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.Passed   | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }

    It 'still treats a fixture with at least one PASS as real results (crash-recovery phrase must NOT override a partial pass)' {
        # Guard: the app-launch env-class only applies when NO test passed. If some tests passed,
        # the app clearly launched, so trust the counts (a real failure must still block).
        $log = New-LogFile @'
  Passed: 5
  Failed: 2
Some later flake mentioned the app did not recover after crash-recovery attempts
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }

    It 'flags a brand-new snapshot with no committed baseline as env/SnapshotBaselineMissing' {
        $log = New-LogFile @'
VisualTestFailedException : Baseline snapshot not yet created for MyNewTest
[UITest] SnapshotCategory: Passed=False Failed=1 [12s]
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.SnapshotBaselineMissing | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not let one missing baseline mask a sibling count-less assertion failure' {
        $log = New-LogFile @'
VisualTestFailedException : Baseline snapshot not yet created for MyNewTest
AssertionException: Expected: 42 But was: 17
[UITest] SnapshotCategory: Passed=False Failed=2 [12s]
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -Not -BeTrue
        $r.SnapshotBaselineMissing | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }

    # A native shared-library load failure (DllNotFoundException / "Unable to load shared
    # library") means the test process could not load a required NATIVE dependency (e.g.
    # libSkiaSharp on a Linux/android gate agent). The test COULD NOT RUN, so nothing about
    # the fix was verified -> EnvError (INCONCLUSIVE), never a plain FAIL that blocks. Guards
    # real build 14699033 (#36653 [Build] Resizetizer external backend): the gate detected
    # ResizetizeImagesTests at CLASS level and ran the whole class on an android agent with no
    # SkiaSharp runtime, so image-rasterization tests threw DllNotFoundException in BOTH the
    # without-fix and with-fix runs -> false FAILED, even though the PR's logic tests passed
    # and real maui-pr CI (Windows Helix Unit Tests) passes these.
    It 'flags a libSkiaSharp DllNotFoundException as env/NativeLibLoadFailure (real #36653 build 14699033)' {
        $log = New-LogFile @'
  Failed BasicImageProcessingWorks [1 s]
  Error Message:
   System.DllNotFoundException : Unable to load shared library 'libSkiaSharp' or one of its dependencies. In order to help diagnose loading problems, consider setting the LD_DEBUG environment variable: liblibSkiaSharp: cannot open shared object file: No such file or directory
Test Run Failed.
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.NativeLibLoadFailure | Should -BeTrue
        $r.Passed   | Should -BeFalse
        $r.Error    | Should -Match 'libSkiaSharp'
        Remove-Item -LiteralPath $log -Force
    }

    It 'flags a Windows "Unable to load DLL" native-load failure as an env error' {
        $log = New-LogFile "System.DllNotFoundException : Unable to load DLL 'libHarfBuzzSharp': The specified module could not be found. (0x8007007E)`nTest Run Failed."
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.NativeLibLoadFailure | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'annotates NativeLibLoadFailure when every failed case is a native-load failure' {
        $log = New-LogFile @'
[xUnit.net 00:00:01.43]     ResizetizeImagesTests.FirstImage [FAIL]
      There was an exception processing the image ''. System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
      /home/vsts/work/1/s/artifacts/bin/Resizetizer.UnitTests/Debug/net11.0/libSkiaSharp.so: cannot open shared object file: No such file or directory
[xUnit.net 00:00:01.44]     ResizetizeImagesTests.SecondImage [FAIL]
      System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
  Failed!  - Failed:     2, Passed:     1, Skipped:     0, Total:     3
  Test Run Failed.
  Total tests: 3
       Passed: 1
       Failed: 2
'@
        $r = Get-TestResultFromOutput -LogFile $log -TestFilter 'ResizetizeImagesTests'
        $r.Passed               | Should -BeFalse
        $r.NativeLibLoadFailure | Should -BeTrue
        $r.NativeLibFailureCount | Should -Be 2
        $r.FailCount            | Should -Be 2
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not hide a genuine failure mixed with a native-load failure' {
        $log = New-LogFile @'
[xUnit.net 00:00:01.43]     ResizetizeImagesTests.NativeDependency [FAIL]
      System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
[xUnit.net 00:00:01.44]     ResizetizeImagesTests.RealRegression [FAIL]
      Assert.Equal() Failure: Expected 5, Actual 4
  Failed!  - Failed:     2, Passed:     1, Skipped:     0, Total:     3
  Test Run Failed.
  Total tests: 3
       Passed: 1
       Failed: 2
'@
        $r = Get-TestResultFromOutput -LogFile $log -TestFilter 'ResizetizeImagesTests'
        $r.Passed                | Should -BeFalse
        $r.EnvError              | Should -Not -BeTrue
        $r.NativeLibLoadFailure  | Should -Not -BeTrue
        $r.NativeLibFailureCount | Should -Be 1
        $r.FailCount             | Should -Be 2
        Remove-Item -LiteralPath $log -Force
    }

    It 'treats missing-output failures downstream of a native-load failure as one environment cascade' {
        $log = New-LogFile @'
[xUnit.net 00:00:01.43]     ResizetizeImagesTests.GenerateImage [FAIL]
      System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
[xUnit.net 00:00:01.44]     ResizetizeImagesTests.VerifyGeneratedImage [FAIL]
      Xunit.Sdk.TrueException: File did not exist: /tmp/output/resized.png
  Failed!  - Failed:     2, Passed:     1, Skipped:     0, Total:     3
  Test Run Failed.
  Total tests: 3
       Passed: 1
       Failed: 2
'@
        $r = Get-TestResultFromOutput -LogFile $log -TestFilter 'ResizetizeImagesTests'
        $r.Passed                | Should -BeFalse
        $r.NativeLibLoadFailure  | Should -BeTrue
        $r.NativeLibFailureCount | Should -Be 2
        $r.FailCount             | Should -Be 2
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not hide an unrelated missing-output regression beside a native-load failure' {
        $log = New-LogFile @'
[xUnit.net 00:00:01.43]     UnrelatedTests.OptionalNativeFeature [FAIL]
      System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
[xUnit.net 00:00:01.44]     UnrelatedTests.RealOutputRegression [FAIL]
      Xunit.Sdk.TrueException: File did not exist: /tmp/output/required.txt
  Failed!  - Failed:     2, Passed:     1, Skipped:     0, Total:     3
  Test Run Failed.
  Total tests: 3
       Passed: 1
       Failed: 2
'@
        $r = Get-TestResultFromOutput -LogFile $log -TestFilter 'UnrelatedTests'
        $r.Passed                | Should -BeFalse
        $r.EnvError              | Should -Not -BeTrue
        $r.NativeLibLoadFailure  | Should -Not -BeTrue
        $r.NativeLibFailureCount | Should -Be 1
        $r.FailCount             | Should -Be 2
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not trust an incidental whole-log native marker when the failed case did not parse' {
        $log = New-LogFile @'
Optional diagnostics: Unable to load shared library 'libOptionalTelemetry' or one of its dependencies.
Assert.Equal() Failure: Expected 5, Actual 4
  Total tests: 2
       Passed: 1
       Failed: 1
'@
        $r = Get-TestResultFromOutput -LogFile $log -TestFilter 'RealRegression'
        $r.Passed                | Should -BeFalse
        $r.EnvError              | Should -Not -BeTrue
        $r.NativeLibLoadFailure  | Should -Not -BeTrue
        $r.NativeLibFailureCount | Should -Be 0
        $r.FailCount             | Should -Be 1
        Remove-Item -LiteralPath $log -Force
    }

    # SAFETY counterpart: a MIXED pass+fail run whose failures are GENUINE managed assertions
    # (no native lib in the log) must NOT be annotated, so a real regression is never masked by
    # the both-states native-lib exclusion. (#36653 DpiPathTests: NullReference/ArgumentNull —
    # the actual bug the fix resolves; it must still drive the FAIL->PASS repro count.)
    It 'does NOT annotate NativeLibLoadFailure on a mixed run of genuine NRE/assert failures (real #36653 DpiPathTests)' {
        $log = New-LogFile @'
  Failed DpiPathTests+GetAppIconDpis.ReturnsGenericDesktopFallback(platform: "gtk") [< 1 ms]
  Error Message:
   System.NullReferenceException : Object reference not set to an instance of an object.
  Failed DpiPathTests+GetDpis.ReturnsGenericDesktopFallback(platform: "gtk") [14 ms]
  Error Message:
   System.ArgumentNullException : Value cannot be null. (Parameter 'collection')
  Test Run Failed.
  Total tests: 22
       Passed: 13
       Failed: 9
'@
        $r = Get-TestResultFromOutput -LogFile $log -TestFilter 'DpiPathTests'
        $r.Passed               | Should -BeFalse
        $r.NativeLibLoadFailure | Should -Not -BeTrue
        $r.FailCount            | Should -Be 9
        Remove-Item -LiteralPath $log -Force
    }

    It 'does NOT flag a genuine ran-and-failed assertion as a native-lib env error' {
        $log = New-LogFile "Build succeeded.`n    0 Error(s)`n  Failed:  2`n  Passed:  3`nAssert.Equal() Failure: Expected 5 but got 4"
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -Not -BeTrue
        $r.NativeLibLoadFailure | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Get-SnapshotDiffMap — snapshot diff extraction' {
    It 'extracts { filename -> percent } from "Snapshot different than baseline" lines' {
        $log = New-LogFile @'
  Snapshot different than baseline: Issue33037NonShell_ListView_AfterScroll.png (0.65% difference)
  Snapshot different than baseline: Issue33037NonShell_GridScrollView_AfterScroll.png (2.63% difference)
'@
        $m = Get-SnapshotDiffMap -LogFile $log
        $m.Count | Should -Be 2
        $m['issue33037nonshell_listview_afterscroll.png'] | Should -Be 0.65
        $m['issue33037nonshell_gridscrollview_afterscroll.png'] | Should -Be 2.63
        Remove-Item -LiteralPath $log -Force
    }

    It 'keeps the MAX percent when the same file appears more than once' {
        $log = New-LogFile @'
  Snapshot different than baseline: a.png (0.40% difference)
  Snapshot different than baseline: a.png (0.90% difference)
'@
        (Get-SnapshotDiffMap -LogFile $log)['a.png'] | Should -Be 0.90
        Remove-Item -LiteralPath $log -Force
    }

    It 'returns an empty map for a log with no snapshot diffs' {
        $log = New-LogFile "everything is fine, no visual failures here"
        (Get-SnapshotDiffMap -LogFile $log).Count | Should -Be 0
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Test-SnapshotEnvironmentalResidual — FAIL->FAIL environmental downgrade' {
    # Guards commit ecf272c7a8. The gate runs the SAME visual test WITHOUT and WITH the fix,
    # so it can tell a fix-caused diff (present without, gone/smaller with) from an
    # environmental one (present at ~the same magnitude in BOTH runs). The downgrade to
    # INCONCLUSIVE must fire ONLY for a genuine environmental residual and must NEVER mask a
    # real regression — these tests pin both directions. Data mirrors real iOS #36511
    # (build 14635697) Issue33037NonShell.
    It 'returns TRUE for the real #36511 case (fix collapses the 2 real diffs; 4 sub-1% residuals no larger than without-fix)' {
        $wo = @{ FailCount = 5; SnapshotDiffMap = @{
            'direct.png' = 0.70; 'grid.png' = 2.63; 'contentviewgrid.png' = 3.01; 'listview.png' = 0.65; 'collectionview.png' = 0.77 } }
        $w  = @{ FailCount = 4; SnapshotDiffMap = @{
            'direct.png' = 0.70; 'contentviewgrid.png' = 0.54; 'listview.png' = 0.65; 'collectionview.png' = 0.77 } }
        Test-SnapshotEnvironmentalResidual -WithoutFixResult $wo -WithFixResult $w | Should -BeTrue
    }

    It 'returns FALSE when the fix WORSENS a snapshot (real regression, not environmental)' {
        $wo = @{ FailCount = 1; SnapshotDiffMap = @{ 'direct.png' = 0.70 } }
        $w  = @{ FailCount = 1; SnapshotDiffMap = @{ 'direct.png' = 0.90 } }
        Test-SnapshotEnvironmentalResidual -WithoutFixResult $wo -WithFixResult $w | Should -BeFalse
    }

    It 'returns FALSE when the fix NEWLY breaks a snapshot absent from the without-fix run' {
        $wo = @{ FailCount = 1; SnapshotDiffMap = @{ 'direct.png' = 0.70 } }
        $w  = @{ FailCount = 1; SnapshotDiffMap = @{ 'newlybroken.png' = 0.30 } }
        Test-SnapshotEnvironmentalResidual -WithoutFixResult $wo -WithFixResult $w | Should -BeFalse
    }

    It 'returns FALSE when any residual exceeds the ~1% environmental ceiling' {
        $wo = @{ FailCount = 1; SnapshotDiffMap = @{ 'direct.png' = 2.00 } }
        $w  = @{ FailCount = 1; SnapshotDiffMap = @{ 'direct.png' = 1.50 } }
        Test-SnapshotEnvironmentalResidual -WithoutFixResult $wo -WithFixResult $w | Should -BeFalse
    }

    It 'returns FALSE when a non-snapshot failure hides among the diffs (FailCount > snapshot files)' {
        $wo = @{ FailCount = 5; SnapshotDiffMap = @{ 'direct.png' = 0.70; 'listview.png' = 0.65 } }
        $w  = @{ FailCount = 2; SnapshotDiffMap = @{ 'direct.png' = 0.70 } }
        Test-SnapshotEnvironmentalResidual -WithoutFixResult $wo -WithFixResult $w | Should -BeFalse
    }

    It 'is fail-safe: returns FALSE for null inputs and an empty with-fix map' {
        Test-SnapshotEnvironmentalResidual -WithoutFixResult $null -WithFixResult $null | Should -BeFalse
        $wo = @{ FailCount = 1; SnapshotDiffMap = @{ 'direct.png' = 0.70 } }
        $w  = @{ FailCount = 0; SnapshotDiffMap = @{} }
        Test-SnapshotEnvironmentalResidual -WithoutFixResult $wo -WithFixResult $w | Should -BeFalse
    }
}

Describe 'Write-MarkdownReport — compile-coupled new-API verification' {
    BeforeAll {
        $script:OutputPath = [System.IO.Path]::GetTempPath()
        function New-Report {
            param([bool]$CompileCoupledVerified)
            $script:MarkdownReport = Join-Path ([System.IO.Path]::GetTempPath()) ("gate-" + [Guid]::NewGuid().ToString('N') + ".md")
            # Without-fix: build error in the PR's OWN detected test (compile-coupling).
            $wo = @(@{ TestName = 'MediaPicker_Tests'; Passed = $false; BuildError = $true; EnvError = $false; FilterMismatch = $false;
                      FailureMessage = "MediaPicker_Tests.cs(43,54): error CS0117: 'MediaPickerImplementation' does not contain a definition for 'ProcessImage'"; Error = 'MediaPicker_Tests' })
            # With-fix: compiles and passes cleanly.
            $w  = @(@{ TestName = 'MediaPicker_Tests'; Passed = $true; BuildError = $false; EnvError = $false; FilterMismatch = $false; FailureMessage = ''; Error = '' })
            $tests = @([pscustomobject]@{ TestName = 'MediaPicker_Tests' })
            Write-MarkdownReport `
                -VerificationPassed $false `
                -CompileCoupledVerified:$CompileCoupledVerified `
                -FailedWithoutFix $false -PassedWithFix $true `
                -WithoutFixResult $wo[0] -WithFixResult $w[0] `
                -WithoutFixResultsList $wo -WithFixResultsList $w `
                -Tests $tests -ReportMergeBase '0123456789abcdef' -ReportPlatform 'android' `
                -ReportBaseBranch 'net11.0' -ReportRevertableFiles @('src/Essentials/src/MediaPicker/MediaPicker.android.cs') -ReportNewFiles @()
            return (Get-Content -LiteralPath $script:MarkdownReport -Raw)
        }
    }

    It 'reports PASSED with a new-API/feature note when compile-coupled and with-fix passes' {
        $report = New-Report -CompileCoupledVerified $true
        $report | Should -Match '### Gate Result: ✅ PASSED'
        $report | Should -Match 'Verified \(new API / feature\)'
        # Must NOT emit the misleading baseline-build-failure classification on a PASS.
        $report | Should -Not -Match 'Base branch does not compile'
    }

    It 'stays INCONCLUSIVE for the same inputs when compile-coupling is NOT credited' {
        $report = New-Report -CompileCoupledVerified $false
        $report | Should -Match '### Gate Result: ⚠️ INCONCLUSIVE'
        $report | Should -Not -Match 'Verified \(new API / feature\)'
    }
}

Describe 'Test-BuildErrorIsInDetectedTest — platform-prefixed device classes' {
    It 'matches the exact detected test file when the class has an Android prefix' {
        $results = @(
            @{
                BuildError = $true
                FailureMessage = "/s/src/Essentials/test/DeviceTests/Tests/Android/MediaPicker_Tests.cs(43,54): error CS0117: 'MediaPickerImplementation' does not contain a definition for 'ProcessImage'"
                Error = ''
            }
        )
        $tests = @(
            [pscustomobject]@{
                TestName = 'Android_MediaPicker_Tests (ProcessImage_Rotation_DoesNotModifySource_And_WritesSingleCacheFile)'
                Files = @('src/Essentials/test/DeviceTests/Tests/Android/MediaPicker_Tests.cs')
            }
        )

        Test-BuildErrorIsInDetectedTest -Results $results -Tests $tests | Should -BeTrue
    }

    It 'does not credit the same filename from a different source path' {
        $results = @(
            @{
                BuildError = $true
                FailureMessage = "/s/src/OtherTests/Android/MediaPicker_Tests.cs(43,54): error CS0117: 'MediaPickerImplementation' does not contain a definition for 'ProcessImage'"
                Error = ''
            }
        )
        $tests = @(
            [pscustomobject]@{
                TestName = 'Android_MediaPicker_Tests (ProcessImage_Rotation_DoesNotModifySource_And_WritesSingleCacheFile)'
                Files = @('src/Essentials/test/DeviceTests/Tests/Android/MediaPicker_Tests.cs')
            }
        )

        Test-BuildErrorIsInDetectedTest -Results $results -Tests $tests | Should -BeFalse
    }
}

Describe 'Write-MarkdownReport — new-snapshot-no-baseline does not double-message as infra error' {
    It 'shows the snapshot note but NOT the generic env-error/retry message (PR #35491 pattern)' {
        $md = Join-Path ([System.IO.Path]::GetTempPath()) ("gate-" + [Guid]::NewGuid().ToString('N') + ".md")
        $script:MarkdownReport = $md
        $script:OutputPath = [System.IO.Path]::GetTempPath()
        # Without-fix: compile-coupled build error in the PR's own test (new API).
        $wo = @(@{ TestName = 'Issue10445'; Passed = $false; BuildError = $true; EnvError = $false; FilterMismatch = $false;
                  FailureMessage = "Issue10445.cs(20,9): error CS0117: 'Shell' does not contain a definition for 'SetBackground'"; Error = 'Issue10445' })
        # With-fix: brand-new snapshot test, no committed baseline (EnvError + SnapshotBaselineMissing).
        $w  = @(@{ TestName = 'Issue10445'; Passed = $false; BuildError = $false; EnvError = $true; SnapshotBaselineMissing = $true; FilterMismatch = $false;
                  FailureMessage = 'New snapshot test — baseline image not yet created'; Error = 'New snapshot test — baseline image not yet created' })
        $tests = @([pscustomobject]@{ TestName = 'Issue10445' })
        Write-MarkdownReport `
            -VerificationPassed $false -CompileCoupledVerified:$false `
            -FailedWithoutFix $false -PassedWithFix $false `
            -WithoutFixResult $wo[0] -WithFixResult $w[0] `
            -WithoutFixResultsList $wo -WithFixResultsList $w `
            -Tests $tests -ReportMergeBase '0123456789abcdef' -ReportPlatform 'ios' `
            -ReportBaseBranch 'net11.0' -ReportRevertableFiles @('src/Controls/src/Core/Shell/Shell.cs') -ReportNewFiles @()
        $report = Get-Content -LiteralPath $md -Raw
        $report | Should -Match '### Gate Result: ⚠️ INCONCLUSIVE'
        $report | Should -Match 'New snapshot test — no baseline yet'
        $report | Should -Not -Match 'Could not verify — environment/infrastructure error'
    }
}

Describe 'Write-MarkdownReport — NETSDK1178 host limitation is permanent and actionable' {
    It 'reports INCONCLUSIVE without recommending another run on the same host (build 14907252 #37176)' {
        $md = Join-Path ([System.IO.Path]::GetTempPath()) ("gate-" + [Guid]::NewGuid().ToString('N') + ".md")
        $script:MarkdownReport = $md
        $script:OutputPath = [System.IO.Path]::GetTempPath()
        $wo = @(@{
            TestName = 'MSBuildTests'; TestType = 'XamlUnitTest'; Passed = $false
            BuildError = $false; EnvError = $false; FilterMismatch = $false
            Failed = 3; Error = ''
        })
        $w = @(@{
            TestName = 'MSBuildTests'; TestType = 'XamlUnitTest'; Passed = $false
            BuildError = $false; EnvError = $true; FilterMismatch = $false
            UnsupportedWorkloadPackFailure = $true; Failed = 0
            Error = 'Gate host limitation: iOS and MacCatalyst SDK packs are unavailable (NETSDK1178).'
        })
        $tests = @([pscustomobject]@{ TestName = 'MSBuildTests'; Type = 'XamlUnitTest'; Filter = 'MSBuildTests' })
        Write-MarkdownReport `
            -VerificationPassed $false -CompileCoupledVerified:$false `
            -FailedWithoutFix $true -PassedWithFix $false `
            -WithoutFixResult $wo[0] -WithFixResult $w[0] `
            -WithoutFixResultsList $wo -WithFixResultsList $w `
            -Tests $tests -ReportMergeBase '0123456789abcdef' -ReportPlatform 'android' `
            -ReportBaseBranch 'main' -ReportRevertableFiles @('src/Controls/src/Build.Tasks/Test.targets') -ReportNewFiles @()
        $report = Get-Content -LiteralPath $md -Raw
        $report | Should -Match '### Gate Result: ⚠️ INCONCLUSIVE'
        $report | Should -Match 'Platform workload unavailable on this gate host'
        $report | Should -Match 'Re-running on the same host cannot help'
        $report | Should -Not -Match 'Comment ``/review`` to retry on a fresh agent'
        $report | Should -Match '<!-- GATE-RETRY-CLASS: skip-permanent -->'
    }
}

Describe 'Write-MarkdownReport — persisted APP_CRASH gets an honest (non-"just retry") message' {
    It 'shows the app-crash message, not the generic transient-flake retry wording (PR #36572 / build 14846070)' {
        $md = Join-Path ([System.IO.Path]::GetTempPath()) ("gate-" + [Guid]::NewGuid().ToString('N') + ".md")
        $script:MarkdownReport = $md
        $script:OutputPath = [System.IO.Path]::GetTempPath()
        # Without-fix: compile-coupled build error (new ProcessImage API removed).
        $wo = @(@{ TestName = 'Android_MediaPicker_Tests'; Passed = $false; BuildError = $true; EnvError = $false; FilterMismatch = $false;
                  FailureMessage = "MediaPicker_Tests.cs(43,54): error CS0117: 'MediaPickerImplementation' does not contain a definition for 'ProcessImage'"; Error = 'Android_MediaPicker_Tests' })
        # With-fix: the app under test crashed (SIGABRT) — persisted APP_CRASH env error.
        $w  = @(@{ TestName = 'Android_MediaPicker_Tests'; Passed = $false; BuildError = $false; EnvError = $true; FilterMismatch = $false;
                  FailureMessage = 'App crashed during test run (XHarness exit 80 APP_CRASH)'; Error = 'App crashed during test run (XHarness exit 80 APP_CRASH)' })
        $tests = @([pscustomobject]@{ TestName = 'Android_MediaPicker_Tests' })
        Write-MarkdownReport `
            -VerificationPassed $false -CompileCoupledVerified:$false `
            -FailedWithoutFix $false -PassedWithFix $false `
            -WithoutFixResult $wo[0] -WithFixResult $w[0] `
            -WithoutFixResultsList $wo -WithFixResultsList $w `
            -Tests $tests -ReportMergeBase '0123456789abcdef' -ReportPlatform 'android' `
            -ReportBaseBranch 'net11.0' -ReportRevertableFiles @('src/Essentials/src/MediaPicker/MediaPicker.android.cs') -ReportNewFiles @()
        $report = Get-Content -LiteralPath $md -Raw
        $report | Should -Match '### Gate Result: ⚠️ INCONCLUSIVE'
        $report | Should -Match 'the app under test crashed \(APP_CRASH\)'
        $report | Should -Match 'persisted across every attempt'
        $report | Should -Match ([regex]::Escape('Download `CopilotLogs`'))
        $report | Should -Match 'log\.diagnostics'
        # Must NOT use the transient-flake "retry on a fresh agent" wording for a crash.
        $report | Should -Not -Match 'Comment ``/review`` to retry on a fresh agent'
    }
}

Describe 'Write-MarkdownReport — genuine failures outrank unrelated environment errors' {
    It 'persists FAILED and a definitive retry class for a confirmed with-fix target timeout' {
        $md = Join-Path ([System.IO.Path]::GetTempPath()) ("gate-" + [Guid]::NewGuid().ToString('N') + ".md")
        $script:MarkdownReport = $md
        $script:OutputPath = [System.IO.Path]::GetTempPath()
        $wo = @(
            @{ TestName = 'InfraCase'; TestType = 'DeviceTest'; Passed = $false; BuildError = $false; EnvError = $true; FilterMismatch = $false; Total = 0; Failed = 0; Error = 'ENV ERROR: emulator unavailable' },
            @{ TestName = 'TargetCase'; TestType = 'DeviceTest'; Passed = $false; BuildError = $false; EnvError = $false; FilterMismatch = $false; Total = 1; Failed = 1; Error = '' }
        )
        $w = @(
            @{ TestName = 'InfraCase'; TestType = 'DeviceTest'; Passed = $true; BuildError = $false; EnvError = $false; FilterMismatch = $false; Total = 1; Failed = 0; Error = '' },
            @{ TestName = 'TargetCase'; TestType = 'DeviceTest'; Passed = $false; BuildError = $false; EnvError = $false; FilterMismatch = $false; Total = 1; Failed = 1; Error = ''; WindowsDeviceTargetTimeoutConfirmed = $true; FailureMessage = 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: scoped target timed out' }
        )
        $tests = @(
            [pscustomobject]@{ TestName = 'InfraCase'; Type = 'DeviceTest'; Filter = 'InfraCase' },
            [pscustomobject]@{ TestName = 'TargetCase'; Type = 'DeviceTest'; Filter = 'TargetCase' }
        )

        Write-MarkdownReport `
            -VerificationPassed $false -CompileCoupledVerified:$false `
            -FailedWithoutFix $true -PassedWithFix $false `
            -WithoutFixResult $wo[0] -WithFixResult $w[0] `
            -WithoutFixResultsList $wo -WithFixResultsList $w `
            -Tests $tests -ReportMergeBase '0123456789abcdef' -ReportPlatform 'windows' `
            -ReportBaseBranch 'main' -ReportRevertableFiles @('src/Core/src/Test.cs') -ReportNewFiles @()

        $report = Get-Content -LiteralPath $md -Raw
        $report | Should -Match '### Gate Result: ❌ FAILED'
        $report | Should -Match '⚠️ ENV ERROR'
        $report | Should -Match 'Fix does not complete the targeted Windows tests'
        $report | Should -Match '<!-- GATE-RETRY-CLASS: definitive-failure -->'
        $report | Should -Not -Match 'Could not verify — environment/infrastructure error'
    }

    It 'describes PASS-to-FAIL as a regression instead of claiming both states passed' {
        $md = Join-Path ([System.IO.Path]::GetTempPath()) ("gate-" + [Guid]::NewGuid().ToString('N') + ".md")
        $script:MarkdownReport = $md
        $script:OutputPath = [System.IO.Path]::GetTempPath()
        $wo = @(
            @{ TestName = 'InfraCase'; TestType = 'DeviceTest'; Passed = $true; BuildError = $false; EnvError = $false; FilterMismatch = $false; Total = 1; Failed = 0; Error = '' },
            @{ TestName = 'RegressionCase'; TestType = 'DeviceTest'; Passed = $true; BuildError = $false; EnvError = $false; FilterMismatch = $false; Total = 1; Failed = 0; Error = '' }
        )
        $w = @(
            @{ TestName = 'InfraCase'; TestType = 'DeviceTest'; Passed = $false; BuildError = $false; EnvError = $true; FilterMismatch = $false; Total = 0; Failed = 0; Error = 'ENV ERROR: emulator unavailable' },
            @{ TestName = 'RegressionCase'; TestType = 'DeviceTest'; Passed = $false; BuildError = $false; EnvError = $false; FilterMismatch = $false; Total = 1; Failed = 1; Error = ''; FailureMessage = 'assertion failed' }
        )
        $tests = @(
            [pscustomobject]@{ TestName = 'InfraCase'; Type = 'DeviceTest'; Filter = 'InfraCase' },
            [pscustomobject]@{ TestName = 'RegressionCase'; Type = 'DeviceTest'; Filter = 'RegressionCase' }
        )

        Write-MarkdownReport `
            -VerificationPassed $false -CompileCoupledVerified:$false `
            -FailedWithoutFix $false -PassedWithFix $false `
            -WithoutFixResult $wo[0] -WithFixResult $w[0] `
            -WithoutFixResultsList $wo -WithFixResultsList $w `
            -Tests $tests -ReportMergeBase '0123456789abcdef' -ReportPlatform 'windows' `
            -ReportBaseBranch 'main' -ReportRevertableFiles @('src/Core/src/Test.cs') -ReportNewFiles @()

        $report = Get-Content -LiteralPath $md -Raw
        $report | Should -Match '### Gate Result: ❌ FAILED'
        $report | Should -Match 'Fix introduces a regression'
        $report | Should -Not -Match 'PASS without fix, PASS with fix'
        $report | Should -Match '<!-- GATE-RETRY-CLASS: definitive-failure -->'
    }
}

Describe 'Invoke-TestRun — device diagnostics are retained with Gate logs' {
    It 'routes each device-test attempt to a unique published diagnostics directory' {
        $content = Get-Content -LiteralPath $scriptPath -Raw
        $content | Should -Match 'OutputDirectory\s*=\s*"\$LogFile\.diagnostics"'
        $content | Should -Match 'writing its diagnostics beside it keeps every attempt under CustomAgentLogsTmp'
    }

    It 'rebuilds the full device-test graph after each baseline/fix source swap' {
        Get-Content -LiteralPath $scriptPath -Raw |
            Should -Match '(?s)\$deviceParams\s*=\s*@\{.*?Rebuild\s*=\s*\$true'
    }

    It 'retries marked Windows no-result exits and correlates persistent baseline exits with a clean fix run' {
        $content = Get-Content -LiteralPath $scriptPath -Raw
        $content | Should -Match 'Test-IsWindowsDeviceNoResultsError'
        $content | Should -Match 'WindowsDeviceNoResults\s*=\s*\$isWindowsNoResults'
        $content | Should -Match 'RetriesExhausted\s*=\s*\$true'
        $content | Should -Match 'Convert-WindowsBaselineNoResultsToFailure'
    }
}

Describe 'Windows baseline no-result correlation' {
    It 'recognizes only the trusted Windows device-test no-results marker' {
        $entry = @{ Type = 'DeviceTest' }
        Test-IsWindowsDeviceNoResultsError `
            -RunPlatform 'windows' `
            -TestEntry $entry `
            -Message 'WINDOWS_DEVICE_TEST_NO_RESULTS: Windows device test result file x is empty or not valid XML.' |
            Should -BeTrue

        Test-IsWindowsDeviceNoResultsError `
            -RunPlatform 'android' `
            -TestEntry $entry `
            -Message 'WINDOWS_DEVICE_TEST_NO_RESULTS: Windows device test result file x is empty or not valid XML.' |
            Should -BeFalse

        Test-IsWindowsDeviceNoResultsError `
            -RunPlatform 'windows' `
            -TestEntry $entry `
            -Message 'Windows device test category Map did not create results within 3600s.' |
            Should -BeFalse
    }

    Describe 'Windows scoped target timeout correlation' {
        It 'recognizes only the trusted exact-target timeout marker' {
            $entry = @{ Type = 'DeviceTest' }
            Test-IsWindowsDeviceTargetTimeoutError `
                -RunPlatform windows `
                -TestEntry $entry `
                -Message 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: exact class timed out' |
                Should -BeTrue

            Test-IsWindowsDeviceTargetTimeoutError `
                -RunPlatform android `
                -TestEntry $entry `
                -Message 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: exact class timed out' |
                Should -BeFalse
        }

        It 'converts three repeated with-fix target timeouts to a deterministic failure' {
            $result = @{
                Passed = $false
                EnvError = $true
                WindowsDeviceTargetTimeout = $true
                RetriesExhausted = $true
                AttemptCount = 3
                WindowsDeviceTargetTimeoutAttemptCount = 3
                Error = 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: exact class timed out'
                Failed = 0
                Total = 0
            }

            Convert-WindowsTargetTimeoutToFailure `
                -Result $result `
                -CounterpartResult @{ Passed = $false; BuildError = $true } `
                -Phase WithFix `
                -RunPlatform windows `
                -TestType DeviceTest |
                Should -BeTrue

            $result.EnvError | Should -BeFalse
            $result.Passed | Should -BeFalse
            $result.Failed | Should -Be 1
            $result.WindowsDeviceTargetTimeoutConfirmed | Should -BeTrue
        }

        It 'credits repeated baseline target timeouts only after a definitive with-fix result' {
            $baseline = @{
                Passed = $false
                EnvError = $true
                WindowsDeviceTargetTimeout = $true
                RetriesExhausted = $true
                AttemptCount = 3
                WindowsDeviceTargetTimeoutAttemptCount = 3
                Error = 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: exact class timed out'
                Failed = 0
                Total = 0
            }

            Convert-WindowsTargetTimeoutToFailure `
                -Result $baseline `
                -CounterpartResult @{ Passed = $true; EnvError = $false; BuildError = $false; FilterMismatch = $false; Total = 1; Failed = 0 } `
                -Phase WithoutFix `
                -RunPlatform windows `
                -TestType DeviceTest |
                Should -BeTrue

            $baseline.EnvError | Should -BeFalse
            $baseline.Failed | Should -Be 1
        }

        It 'keeps a baseline target timeout inconclusive when the counterpart ran zero tests' {
            $baseline = @{
                Passed = $false
                EnvError = $true
                WindowsDeviceTargetTimeout = $true
                RetriesExhausted = $true
                AttemptCount = 3
                WindowsDeviceTargetTimeoutAttemptCount = 3
                Error = 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: exact class timed out'
                Failed = 0
                Total = 0
            }

            Convert-WindowsTargetTimeoutToFailure `
                -Result $baseline `
                -CounterpartResult @{ Passed = $true; EnvError = $false; BuildError = $false; FilterMismatch = $false; Total = 0; Failed = 0 } `
                -Phase WithoutFix `
                -RunPlatform windows `
                -TestType DeviceTest |
                Should -BeFalse

            $baseline.EnvError | Should -BeTrue
            $baseline.Failed | Should -Be 0
        }

        It 'rejects mixed timeout evidence' {
            $result = @{
                Passed = $false
                EnvError = $true
                WindowsDeviceTargetTimeout = $true
                RetriesExhausted = $true
                AttemptCount = 3
                WindowsDeviceTargetTimeoutAttemptCount = 2
            }

            Convert-WindowsTargetTimeoutToFailure `
                -Result $result `
                -CounterpartResult @{ Passed = $true } `
                -Phase WithFix `
                -RunPlatform windows `
                -TestType DeviceTest |
                Should -BeFalse
        }
    }

    It 'credits a persistent baseline app exit only after the scoped with-fix test passes' {
        $without = @{
            Passed = $false
            EnvError = $true
            WindowsDeviceNoResults = $true
            RetriesExhausted = $true
            AttemptCount = 3
            WindowsDeviceNoResultAttemptCount = 3
            Error = 'WINDOWS_DEVICE_TEST_NO_RESULTS: empty XML'
            Failed = 0
            Total = 0
        }
        $with = @{
            Passed = $true
            EnvError = $false
            BuildError = $false
            FilterMismatch = $false
        }

        Convert-WindowsBaselineNoResultsToFailure `
            -WithoutFixResult $without `
            -WithFixResult $with `
            -RunPlatform 'windows' `
            -TestType 'DeviceTest' |
            Should -BeTrue

        $without.EnvError | Should -BeFalse
        $without.Passed | Should -BeFalse
        $without.Failed | Should -Be 1
        $without.Total | Should -Be 1
        $without.WindowsBaselineAppExit | Should -BeTrue
        $without.FailureReason | Should -Match 'all 3 baseline attempts'
    }

    It 'remains inconclusive when retries were not exhausted or the fix did not pass' {
        foreach ($case in @(
            @{
                Without = @{ Passed = $false; EnvError = $true; WindowsDeviceNoResults = $true; RetriesExhausted = $true; AttemptCount = 2 }
                With = @{ Passed = $true; EnvError = $false; BuildError = $false; FilterMismatch = $false }
            },
            @{
                Without = @{ Passed = $false; EnvError = $true; WindowsDeviceNoResults = $true; RetriesExhausted = $true; AttemptCount = 3 }
                With = @{ Passed = $false; EnvError = $false; BuildError = $false; FilterMismatch = $false }
            }
        )) {
            Convert-WindowsBaselineNoResultsToFailure `
                -WithoutFixResult $case.Without `
                -WithFixResult $case.With `
                -RunPlatform 'windows' `
                -TestType 'DeviceTest' |
                Should -BeFalse
            $case.Without.EnvError | Should -BeTrue
        }
    }
}

Describe 'Gate failure precedence' {
    It 'does not classify a mixed environment error and genuine with-fix failure as infrastructure-only' {
        Test-GateHasDefinitiveFailure `
            -WithFixGenuineFailCount 1 `
            -WithFixBuildError:$false `
            -BaselineBuildError:$false `
            -PrTestBuildError:$false |
            Should -BeTrue
    }

    It 'keeps an environment-only result non-definitive' {
        Test-GateHasDefinitiveFailure `
            -WithFixGenuineFailCount 0 `
            -WithFixBuildError:$false `
            -BaselineBuildError:$false `
            -PrTestBuildError:$false |
            Should -BeFalse
    }
}

Describe 'Invoke-TestRunWithRetry — Windows no-result exits' {
    BeforeEach {
        $script:Platform = 'windows'
        $script:RepoRoot = $TestDrive
        Mock Start-Sleep {}
    }

    It 'retries the trusted no-results marker three times and returns durable evidence' {
        Mock Invoke-TestRun {
            throw 'WINDOWS_DEVICE_TEST_NO_RESULTS: Windows device test result file x is empty or not valid XML.'
        }

        $entry = @{
            Type = 'DeviceTest'
            Filter = 'Category=Map'
            ClassFilter = 'Microsoft.Maui.DeviceTests.MapTests'
            Methods = @('RemovingMapFromVisualTreeDoesNotCrash')
            Project = 'Controls'
            ProjectPath = 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
        }
        $log = Join-Path $TestDrive 'windows-no-results.log'

        $result = Invoke-TestRunWithRetry -TestEntry $entry -LogFile $log -MaxRetries 3

        $result.EnvError | Should -BeTrue
        $result.WindowsDeviceNoResults | Should -BeTrue
        $result.RetriesExhausted | Should -BeTrue
        $result.AttemptCount | Should -Be 3
        $result.WindowsDeviceNoResultAttemptCount | Should -Be 3
        Should -Invoke Invoke-TestRun -Times 3 -Exactly
        (Get-Content -LiteralPath $log -Raw) | Should -Match '^WINDOWS_DEVICE_TEST_NO_RESULTS:'
    }

    It 'records mixed environment attempts without claiming all retries were app exits' {
        $script:retryInvocation = 0
        $unrelatedLog = Join-Path $TestDrive 'unrelated-env.log'
        'XHarness exit code: 83' | Set-Content -LiteralPath $unrelatedLog -Encoding UTF8

        Mock Invoke-TestRun {
            $script:retryInvocation++
            if ($script:retryInvocation -in @(1, 3)) {
                throw 'WINDOWS_DEVICE_TEST_NO_RESULTS: Windows device test result file x is empty or not valid XML.'
            }
            return $unrelatedLog
        }

        $entry = @{
            Type = 'DeviceTest'
            Filter = 'Category=Map'
            ClassFilter = 'Microsoft.Maui.DeviceTests.MapTests'
            Methods = @('RemovingMapFromVisualTreeDoesNotCrash')
            Project = 'Controls'
            ProjectPath = 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
        }

        $result = Invoke-TestRunWithRetry `
            -TestEntry $entry `
            -LogFile (Join-Path $TestDrive 'mixed-no-results.log') `
            -MaxRetries 3

        $result.AttemptCount | Should -Be 3
        $result.WindowsDeviceNoResultAttemptCount | Should -Be 2

        $with = @{ Passed = $true; EnvError = $false; BuildError = $false; FilterMismatch = $false }
        Convert-WindowsBaselineNoResultsToFailure `
            -WithoutFixResult $result `
            -WithFixResult $with `
            -RunPlatform 'windows' `
            -TestType 'DeviceTest' |
            Should -BeFalse
    }

    It 'does not swallow an unrelated device-test exception' {
        Mock Invoke-TestRun { throw 'unrelated runner failure' }

        $entry = @{
            Type = 'DeviceTest'
            Filter = 'Category=Map'
            ClassFilter = 'Microsoft.Maui.DeviceTests.MapTests'
            Methods = @()
            Project = 'Controls'
            ProjectPath = 'src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj'
        }

        { Invoke-TestRunWithRetry -TestEntry $entry -LogFile (Join-Path $TestDrive 'other.log') -MaxRetries 3 } |
            Should -Throw -ExpectedMessage '*unrelated runner failure*'
        Should -Invoke Invoke-TestRun -Times 1 -Exactly
    }

    It 'retries a trusted exact-target timeout three times and records durable evidence' {
        Mock Invoke-TestRun {
            throw 'WINDOWS_DEVICE_TEST_TARGET_TIMEOUT: exact class timed out'
        }

        $entry = @{
            Type = 'DeviceTest'
            Filter = 'Category=Window'
            ClassFilter = 'Microsoft.Maui.DeviceTests.WindowHandlerTests'
            Methods = @('TargetMethod')
            Project = 'Core'
            ProjectPath = 'src/Core/tests/DeviceTests/Core.DeviceTests.csproj'
        }

        $result = Invoke-TestRunWithRetry `
            -TestEntry $entry `
            -LogFile (Join-Path $TestDrive 'windows-target-timeout.log') `
            -MaxRetries 3

        $result.EnvError | Should -BeTrue
        $result.WindowsDeviceTargetTimeout | Should -BeTrue
        $result.RetriesExhausted | Should -BeTrue
        $result.AttemptCount | Should -Be 3
        $result.WindowsDeviceTargetTimeoutAttemptCount | Should -Be 3
        Should -Invoke Invoke-TestRun -Times 3 -Exactly
    }
}


Describe 'Get-TestResultFromOutput — MSBuild-server/BuildTasks infra flake is ENV, not BUILD error' {
    It 'classifies "required MSBuild tasks are not yet built or out of date" as EnvError' {
        $log = New-LogFile -Content @"
❌ Build failed with exit code 1
MSBuild server unavailable: could not connect to the server within the timeout window; the server may have failed to start. Falling back to an in-process build.
/home/vsts/work/1/s/src/Maui.InTree.targets(34,5): error : We have detected that the required MSBuild tasks are not yet built or they are out of date. [/home/vsts/work/1/s/src/Essentials/test/DeviceTests/Essentials.DeviceTests.csproj::TargetFramework=net11.0-android]
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.BuildError | Should -Not -BeTrue
        $r.Error | Should -Match 'Gate infrastructure'
    }

    It 'keeps compiler errors authoritative when the build server falls back in-process' {
        $log = New-LogFile -Content @"
MSBuild server unavailable: could not connect to the server within the timeout window; the server may have failed to start. Falling back to an in-process build.
D:\a\1\s\src\Core\tests\DeviceTests\Stubs\SwipeItemMenuItemStub.cs(5,72): error CS0246: The type or namespace name 'ISwipeItemMenuItemIconColor' could not be found
Build FAILED.
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -Not -BeTrue
        $r.BuildError | Should -BeTrue
        $r.Error | Should -Match 'CS0246'
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not let the BuildTasks message mask a coded compiler error from the same build' {
        $log = New-LogFile -Content @"
D:\a\1\s\src\Maui.InTree.targets(34,5): error : We have detected that the required MSBuild tasks are not yet built or they are out of date.
D:\a\1\s\src\Core\tests\DeviceTests\Stubs\SwipeItemMenuItemStub.cs(5,72): error CS0246: The type or namespace name 'ISwipeItemMenuItemIconColor' could not be found
Build FAILED.
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -Not -BeTrue
        $r.BuildError | Should -BeTrue
        $r.Error | Should -Match 'CS0246'
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Get-TestResultFromOutput — NETSDK1147 missing-workload infra flake is ENV, not BUILD error' {
    It 'classifies "the following workloads must be installed: android" as EnvError' {
        $log = New-LogFile -Content @"
Running: dotnet build src/Essentials/test/DeviceTests/Essentials.DeviceTests.csproj -c Debug -f net11.0-android
❌ Build failed with exit code 1
/home/vsts/work/1/s/.dotnet/sdk/11.0.100-rc.1.26379.102/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.Sdk.ImportWorkloads.targets(38,5): error NETSDK1147: To build this project, the following workloads must be installed: android [/home/vsts/work/1/s/src/Core/src/Core.csproj::TargetFramework=net11.0-android37.0]
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.BuildError | Should -Not -BeTrue
        $r.Error | Should -Match 'workload was not installed'
        $r.Error | Should -Match 'android'
    }

    It 'does NOT mask a genuine CS compile error that co-occurs with NETSDK1147' {
        $log = New-LogFile -Content @"
❌ Build failed with exit code 1
error NETSDK1147: the following workloads must be installed: android
/s/src/Foo.cs(10,5): error CS0117: 'Bar' does not contain a definition for 'Baz'
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.BuildError | Should -BeTrue
        $r.EnvError | Should -Not -BeTrue
    }
}

Describe 'Get-TestResultFromOutput — NETSDK1178 host-incompatible workload packs' {
    It 'classifies all failed xUnit cases as environment when each one is NETSDK1178 (build 14907252 #37176)' {
        $log = New-LogFile -Content @'
[xUnit.net 00:00:17.70]     CrossPlatformBuild(target: "ios") [FAIL]
[xUnit.net 00:00:17.70]       Output:
[xUnit.net 00:00:17.70]         error NETSDK1178: The project depends on the following workload packs that do not exist in any of the workloads available in this installation: Microsoft.iOS.Sdk.net10.0_26.0
[xUnit.net 00:00:21.56]     CrossPlatformBuild(target: "maccatalyst") [FAIL]
[xUnit.net 00:00:21.56]       Output:
[xUnit.net 00:00:21.56]         error NETSDK1178: The project depends on the following workload packs that do not exist in any of the workloads available in this installation: Microsoft.MacCatalyst.Sdk.net10.0_26.0
  Failed CrossPlatformBuild(target: "ios") [1 s]
  Error Message:
   Assert.Equal() Failure
  Standard Output Messages:
 error NETSDK1178: The project depends on the following workload packs that do not exist in any of the workloads available in this installation: Microsoft.iOS.Sdk.net10.0_26.0
  Failed CrossPlatformBuild(target: "maccatalyst") [1 s]
  Error Message:
   Assert.Equal() Failure
  Standard Output Messages:
 error NETSDK1178: The project depends on the following workload packs that do not exist in any of the workloads available in this installation: Microsoft.MacCatalyst.Sdk.net10.0_26.0
Total tests: 22
     Passed: 18
     Failed: 2
    Skipped: 2
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.UnsupportedWorkloadPackFailure | Should -BeTrue
        $r.BuildError | Should -Not -BeTrue
        $r.Failed | Should -Be 0
        $r.Error | Should -Match 'NETSDK1178'
        $r.Error | Should -Match 'Microsoft\.iOS\.Sdk'
        $r.Error | Should -Match 'Microsoft\.MacCatalyst\.Sdk'
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not mask a mixed run that also contains a genuine failed case' {
        $log = New-LogFile -Content @'
[xUnit.net 00:00:17.70]     CrossPlatformBuild(target: "ios") [FAIL]
[xUnit.net 00:00:17.70]       Output:
[xUnit.net 00:00:17.70]         error NETSDK1178: The project depends on workload packs that do not exist: Microsoft.iOS.Sdk.net10.0_26.0
[xUnit.net 00:00:19.00]     CrossPlatformBuild(target: "android") [FAIL]
[xUnit.net 00:00:19.00]       Assert.Equal() Failure: Values differ
  Failed CrossPlatformBuild(target: "ios") [1 s]
  Error Message:
   Assert.Equal() Failure
  Standard Output Messages:
 error NETSDK1178: The project depends on workload packs that do not exist: Microsoft.iOS.Sdk.net10.0_26.0
  Failed CrossPlatformBuild(target: "android") [1 s]
  Error Message:
   Assert.Equal() Failure: Expected 0, Actual 1
Total tests: 20
     Passed: 18
     Failed: 2
'@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -Not -BeTrue
        $r.UnsupportedWorkloadPackFailure | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        $r.Failed | Should -Be 2
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Get-TestResultFromOutput — snapshot size-mismatch classification' {
    It 'classifies a UITest snapshot SIZE mismatch as INCONCLUSIVE (env), not a failure (build 14850018 #37032)' {
        $log = New-LogFile -Content @"
  [UITest] Issue36422 (filter: Issue36422)
  FixtureSetup for Issue36422(iOS)
      Error Message:
   Snapshot different than baseline: ChangingItemSpacingDoesNotShiftFirstItemOutOfView.png (size differs - baseline is 1206x2472 pixels, actual is 1124x2286 pixels)
   at VisualTestUtils.VisualRegressionTester.VerifyMatchesSnapshot(...)
  [UITest] Issue36422: Passed=False Failed=1 [303s]
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.SnapshotSizeMismatch | Should -BeTrue
        $r.Error | Should -Match 'size'
        Remove-Item -LiteralPath $log -Force
    }

    It 'classifies device-test size mismatches (Passed:/Failed: counts) as INCONCLUSIVE (env)' {
        $log = New-LogFile -Content @"
  Passed: 3
  Failed: 2
  Snapshot different than baseline: A.png (size differs - baseline is 1206x2472 pixels, actual is 1124x2286 pixels)
  Snapshot different than baseline: B.png (size differs - baseline is 1206x2472 pixels, actual is 1124x2286 pixels)
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.SnapshotSizeMismatch | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'does NOT mask a genuine pixel DIFF (N% difference against a same-size baseline)' {
        $log = New-LogFile -Content @"
  [UITest] IssueReal (filter: IssueReal)
   Snapshot different than baseline: RealRegression.png (17.08% difference)
  [UITest] IssueReal: Passed=False Failed=1 [40s]
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.SnapshotSizeMismatch | Should -Not -BeTrue
        $r.EnvError | Should -Not -BeTrue
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not let one size mismatch mask a sibling count-less assertion failure' {
        $log = New-LogFile -Content @"
  Snapshot different than baseline: DifferentDevice.png (size differs - baseline is 1206x2472 pixels, actual is 1124x2286 pixels)
  AssertionException: Expected: 42 But was: 17
  [UITest] MixedCategory: Passed=False Failed=2 [40s]
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.SnapshotSizeMismatch | Should -Not -BeTrue
        $r.EnvError | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }

    It 'does not let one size mismatch mask a sibling count-less pixel diff' {
        $log = New-LogFile -Content @"
  Snapshot different than baseline: DifferentDevice.png (size differs - baseline is 1206x2472 pixels, actual is 1124x2286 pixels)
  Snapshot different than baseline: RealRegression.png (17.08% difference)
  [UITest] MixedVisualCategory: Passed=False Failed=2 [40s]
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.SnapshotSizeMismatch | Should -Not -BeTrue
        $r.EnvError | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }

    It 'keeps a count-less run benign when every failure is a missing baseline or size mismatch' {
        $log = New-LogFile -Content @"
  VisualTestFailedException : Baseline snapshot not yet created for NewSnapshot.png
  Snapshot different than baseline: DifferentDevice.png (size differs - baseline is 1206x2472 pixels, actual is 1124x2286 pixels)
  [UITest] MixedBenignSnapshots: Passed=False Failed=2 [40s]
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.SnapshotSizeMismatch | Should -BeTrue
        $r.Failed | Should -Be 0
        Remove-Item -LiteralPath $log -Force
    }

    It 'fails closed when a size marker has no count-less UITest failure summary' {
        $log = New-LogFile -Content @"
  Snapshot different than baseline: Unknown.png (size differs - baseline is 1206x2472 pixels, actual is 1124x2286 pixels)
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.SnapshotSizeMismatch | Should -Not -BeTrue
        $r.EnvError | Should -Not -BeTrue
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Get-TestResultFromOutput — native-lib load failure flag (feeds with-fix env reclassify)' {
    It 'flags a fully accounted libSkiaSharp failure in the device-count FAIL path' {
        $log = New-LogFile -Content @"
  [xUnit.net 00:00:00.94]     Microsoft.Maui.Resizetizer.Tests.GenerateSplashAndroidResourcesTests.SplashScreenResectsAlias [FAIL]
  [xUnit.net 00:00:00.94]       Error occurred in processing Android-specific image resources. System.DllNotFoundException: Unable to load shared library 'libSkiaSharp' or one of its dependencies.
  [xUnit.net 00:00:00.94]       /home/vsts/work/1/s/artifacts/bin/Resizetizer.UnitTests/Debug/net11.0/libSkiaSharp.so: cannot open shared object file: No such file or directory
  Passed: 3
  Failed: 1
"@
        $r = Get-TestResultFromOutput -LogFile $log
        $r.NativeLibLoadFailure | Should -BeTrue
        $r.NativeLibFailureCount | Should -Be 1
        $r.Passed | Should -BeFalse
        Remove-Item -LiteralPath $log -Force
    }
}

Describe 'Baseline mutation window — guaranteed restoration' {
    BeforeAll {
        $script:GateSource = Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'verify-tests-fail.ps1')
        $gateTokens = $null
        $gateErrors = $null
        $script:GateAst = [System.Management.Automation.Language.Parser]::ParseFile(
            (Join-Path $PSScriptRoot 'verify-tests-fail.ps1'), [ref]$gateTokens, [ref]$gateErrors)

        $fn = $script:GateAst.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq 'Restore-BaselineMutationFromHead'
        }, $true)
        if (-not $fn) { throw "Function 'Restore-BaselineMutationFromHead' not found" }
        Invoke-Expression $fn.Extent.Text

        function Write-Log { param([Parameter(ValueFromPipeline)][string]$Message) }

        function New-GateRepo {
            # A tiny repo with: modified.txt (changed by the "PR"), added.txt (added by the
            # "PR") and deleted.txt (deleted by the "PR"). Returns the repo root + merge base.
            $root = Join-Path ([System.IO.Path]::GetTempPath()) ("gaterepo-" + [Guid]::NewGuid().ToString('N'))
            New-Item -ItemType Directory -Path $root -Force | Out-Null
            Push-Location $root
            try {
                git init -q 2>&1 | Out-Null
                git config user.email t@t.t; git config user.name t
                'base' | Set-Content (Join-Path $root 'modified.txt') -Encoding UTF8
                'gone' | Set-Content (Join-Path $root 'deleted.txt') -Encoding UTF8
                git add -A 2>&1 | Out-Null
                git commit -q -m base 2>&1 | Out-Null
                $mergeBase = (git rev-parse HEAD).Trim()

                'fixed' | Set-Content (Join-Path $root 'modified.txt') -Encoding UTF8
                'new'   | Set-Content (Join-Path $root 'added.txt') -Encoding UTF8
                Remove-Item (Join-Path $root 'deleted.txt') -Force
                git add -A 2>&1 | Out-Null
                git commit -q -m fix 2>&1 | Out-Null
            } finally { Pop-Location }
            return @{ Root = $root; MergeBase = $mergeBase }
        }

        function Invoke-BaselineMutation {
            param([string]$Root, [string]$MergeBase)
            Push-Location $Root
            try {
                git checkout $MergeBase -- modified.txt 2>&1 | Out-Null
                git checkout $MergeBase -- deleted.txt 2>&1 | Out-Null
                git rm -f --ignore-unmatch -- added.txt 2>&1 | Out-Null
                if (Test-Path (Join-Path $Root 'added.txt')) { Remove-Item (Join-Path $Root 'added.txt') -Force }
            } finally { Pop-Location }
        }
    }

    It 'restores reverted, PR-deleted and PR-added files back to their HEAD state' {
        $repo = New-GateRepo
        try {
            Invoke-BaselineMutation -Root $repo.Root -MergeBase $repo.MergeBase
            # Sanity: the tree really is mutated away from HEAD.
            (Get-Content (Join-Path $repo.Root 'modified.txt') -Raw).Trim() | Should -Be 'base'
            Test-Path (Join-Path $repo.Root 'added.txt') | Should -BeFalse
            Test-Path (Join-Path $repo.Root 'deleted.txt') | Should -BeTrue

            Push-Location $repo.Root
            try {
                $ok = Restore-BaselineMutationFromHead `
                    -RevertableFiles @('modified.txt', 'deleted.txt') `
                    -DeletedByPrFiles @('deleted.txt') `
                    -NewFiles @('added.txt') `
                    -RepoRoot $repo.Root
                $ok | Should -BeTrue
                # Worktree AND index must match HEAD again.
                (git status --porcelain | Out-String).Trim() | Should -BeNullOrEmpty
            } finally { Pop-Location }

            (Get-Content (Join-Path $repo.Root 'modified.txt') -Raw).Trim() | Should -Be 'fixed'
            (Get-Content (Join-Path $repo.Root 'added.txt') -Raw).Trim() | Should -Be 'new'
            Test-Path (Join-Path $repo.Root 'deleted.txt') | Should -BeFalse
        } finally {
            Remove-Item -LiteralPath $repo.Root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'reports failure (never throws) in BestEffort mode so it cannot mask an in-flight exit code' {
        $repo = New-GateRepo
        try {
            Push-Location $repo.Root
            try {
                { Restore-BaselineMutationFromHead `
                    -RevertableFiles @('does/not/exist.txt') `
                    -RepoRoot $repo.Root -BestEffort } | Should -Not -Throw
                Restore-BaselineMutationFromHead `
                    -RevertableFiles @('does/not/exist.txt') `
                    -RepoRoot $repo.Root -BestEffort | Should -BeFalse
            } finally { Pop-Location }
        } finally {
            Remove-Item -LiteralPath $repo.Root -Recurse -Force -ErrorAction SilentlyContinue
        }
    }

    It 'runs the WITHOUT-fix phase inside a try whose finally restores the tree' {
        # A nested `exit` (e.g. device boot failure -> exit 3) bypasses the per-test catch, so
        # the mutation window must be closed by a finally rather than by STEP 3 alone.
        $tryStatements = $script:GateAst.FindAll({
            $args[0] -is [System.Management.Automation.Language.TryStatementAst]
        }, $true)

        $guarded = @($tryStatements | Where-Object {
            $_.Finally -and
            $_.Finally.Extent.Text -match 'Restore-BaselineMutationFromHead' -and
            $_.Body.Extent.Text -match 'STEP 2: Running tests WITHOUT fix' -and
            $_.Body.Extent.Text -match 'STEP 3: Restoring fix files from HEAD'
        })

        $guarded.Count | Should -Be 1
    }

    It 'only skips the emergency restore once STEP 3 has closed the window' {
        $script:GateSource | Should -Match '\$script:BaselineMutationActive\s*=\s*\$true'
        $script:GateSource | Should -Match 'if\s*\(\$script:BaselineMutationActive\)'
    }

    It 'fails the gate when a PR-added file cannot be removed for the baseline' {
        # `git rm` used to be fire-and-forget; a stale copy left on disk silently poisons the
        # without-fix baseline build.
        $script:GateSource | Should -Match 'WARNING: git rm failed for \$file'
        $script:GateSource | Should -Match 'ERROR: Failed to remove PR-added file \$file for the baseline'
    }
}

Describe 'Get-AutoDetectedTests — frozen worktree isolation' {
    It 'prefers the immutable explicit-base local diff over PR metadata' {
        $repo = Join-Path ([System.IO.Path]::GetTempPath()) ("verifyrepo-" + [Guid]::NewGuid().ToString('N'))
        $detector = Join-Path $repo 'detect.ps1'
        try {
            New-Item -ItemType Directory -Path $repo | Out-Null
            git -C $repo init --quiet
            'base' | Set-Content -LiteralPath (Join-Path $repo 'README.md')
            git -C $repo add README.md
            git -C $repo -c user.name='Vally Test' -c user.email='vally-test@example.invalid' commit --quiet -m base
            $base = git -C $repo rev-parse HEAD

            $testPath = 'src/Controls/tests/Core.UnitTests/VallyFixtureTests.cs'
            New-Item -ItemType Directory -Path (Split-Path (Join-Path $repo $testPath)) -Force | Out-Null
            'fixture' | Set-Content -LiteralPath (Join-Path $repo $testPath)
            git -C $repo add $testPath
            git -C $repo -c user.name='Vally Test' -c user.email='vally-test@example.invalid' commit --quiet -m fixture

            @(
                'param([string]$PRNumber, [string[]]$ChangedFiles, [string]$DiffBase)'
                '[pscustomobject]@{'
                '    PRNumber = $PRNumber'
                '    ChangedFiles = @($ChangedFiles)'
                '    DiffBase = $DiffBase'
                '}'
            ) | Set-Content -LiteralPath $detector

            $script:PRNumber = '33134'
            $script:ExplicitBaseBranch = $base
            $script:DetectTestsScript = $detector
            Push-Location $repo
            try {
                $result = Get-AutoDetectedTests -MergeBase $base
            } finally {
                Pop-Location
            }

            $result.PRNumber | Should -BeNullOrEmpty
            @($result.ChangedFiles) | Should -Contain $testPath
            $result.DiffBase | Should -Be $base
        } finally {
            Remove-Item -LiteralPath $repo -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}

Describe 'Get-AutoDetectedTests — PR metadata fallback' {
    It 'uses PR metadata when no committed snapshot is available' {
        $detector = Join-Path ([System.IO.Path]::GetTempPath()) ("detect-" + [Guid]::NewGuid().ToString('N') + ".ps1")
        try {
            @(
                'param([string]$PRNumber, [string[]]$ChangedFiles)'
                '[pscustomobject]@{'
                '    PRNumber = $PRNumber'
                '    ChangedFiles = @($ChangedFiles)'
                '}'
            ) | Set-Content -LiteralPath $detector
            $script:PRNumber = '33134'
            $script:PRNumber = '33134'
            $script:DetectTestsScript = $detector

            $result = Get-AutoDetectedTests -MergeBase $null

            $result.PRNumber | Should -Be '33134'
            @($result.ChangedFiles) | Should -BeNullOrEmpty
        } finally {
            Remove-Item -LiteralPath $detector -Force -ErrorAction SilentlyContinue
        }
    }
}
