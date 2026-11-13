#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot '..' '..' 'eng' 'scripts' 'dismiss-apple-account-dialog.sh'
    $scriptContent = Get-Content -Raw -LiteralPath $scriptPath
    $commandLines = @(
        $scriptContent -split '\r?\n' |
            Where-Object {
                $trimmed = $_.Trim()
                $trimmed -and -not $trimmed.StartsWith('#')
            }
    )
}

Describe 'dismiss-apple-account-dialog sudo safety' {
    It 'uses non-interactive sudo for every command' {
        $sudoLines = @($commandLines | Where-Object { $_ -match '\bsudo\b' })
        $sudoLines.Count | Should -BeGreaterThan 0

        $unsafeLines = @($sudoLines | Where-Object { $_ -notmatch '\bsudo\s+-n(?:\s|$)' })
        $unsafeLines | Should -BeNullOrEmpty
    }
}
