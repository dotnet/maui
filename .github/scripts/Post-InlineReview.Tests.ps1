#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:ScriptPath = Join-Path $PSScriptRoot 'post-inline-review.ps1'
    $script:Content = Get-Content -Path $script:ScriptPath -Raw
}

Describe 'post-inline-review findings JSON handling' {
    It 'checks for empty findings before parsing JSON' {
        $emptyGuard = $script:Content.IndexOf('[string]::IsNullOrWhiteSpace($rawJson)')
        $parseCall = $script:Content.IndexOf('ConvertFrom-Json -ErrorAction Stop')

        $emptyGuard | Should -BeGreaterOrEqual 0
        $parseCall | Should -BeGreaterThan $emptyGuard
    }

    It 'does not silently ignore malformed non-empty JSON' {
        $script:Content | Should -Match 'ConvertFrom-Json -ErrorAction Stop'
        $script:Content | Should -Match 'contains malformed JSON'
    }
}
