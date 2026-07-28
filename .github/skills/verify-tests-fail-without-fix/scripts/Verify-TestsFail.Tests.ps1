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
