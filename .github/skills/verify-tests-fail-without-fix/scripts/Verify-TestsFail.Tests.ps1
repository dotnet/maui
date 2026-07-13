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
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $function = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Get-TestResultFromOutput'
    }, $true)
    if (-not $function) { throw "Function 'Get-TestResultFromOutput' not found" }
    Invoke-Expression $function.Extent.Text

    function New-LogFile {
        param([string]$Content)
        $f = Join-Path ([System.IO.Path]::GetTempPath()) ("verifylog-" + [Guid]::NewGuid().ToString('N') + ".log")
        $Content | Set-Content -LiteralPath $f -Encoding UTF8
        return $f
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

    It 'flags a brand-new snapshot with no committed baseline as env/SnapshotBaselineMissing' {
        $log = New-LogFile "VisualTestFailedException : Baseline snapshot not yet created for MyNewTest"
        $r = Get-TestResultFromOutput -LogFile $log
        $r.EnvError | Should -BeTrue
        $r.SnapshotBaselineMissing | Should -BeTrue
        Remove-Item -LiteralPath $log -Force
    }
}
