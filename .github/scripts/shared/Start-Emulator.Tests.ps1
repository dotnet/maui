#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Start-Emulator.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $function = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Test-ShouldRestartReusedAndroidEmulator'
    }, $true)
    if (-not $function) {
        throw "Function 'Test-ShouldRestartReusedAndroidEmulator' not found"
    }

    Invoke-Expression $function.Extent.Text
    $scriptContent = Get-Content -Raw -Path $scriptPath
}

Describe 'Reused Android emulator recovery' {
    It 'does not restart a fresh emulator' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $false `
            -RecoveryAlreadyAttempted $false `
            -UnreadySeconds 240 | Should -BeFalse
    }

    It 'waits for the bounded reused-emulator threshold' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $true `
            -RecoveryAlreadyAttempted $false `
            -UnreadySeconds 145 | Should -BeFalse
    }

    It 'restarts a reused emulator after the threshold' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $true `
            -RecoveryAlreadyAttempted $false `
            -UnreadySeconds 150 | Should -BeTrue
    }

    It 'never restarts the replacement emulator a second time' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $true `
            -RecoveryAlreadyAttempted $true `
            -UnreadySeconds 240 | Should -BeFalse
    }

    It 'uses total unready time instead of an offline-only streak' {
        $scriptContent | Should -Match ([regex]::Escape('-UnreadySeconds $deviceWaited'))
        $scriptContent | Should -Not -Match '\$offlineStreak'
    }
}
