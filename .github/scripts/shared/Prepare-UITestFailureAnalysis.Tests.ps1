#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Prepare-UITestFailureAnalysis.ps1'
    $script:content = Get-Content -Path $scriptPath -Raw
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $scriptPath,
        [ref] $tokens,
        [ref] $parseErrors)

    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $function = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'ConvertTo-UiFailureSafeConsoleText'
    }, $true)

    if (-not $function) {
        throw 'ConvertTo-UiFailureSafeConsoleText not found'
    }

    Invoke-Expression $function.Extent.Text
}

Describe 'Prepare UI test failure analysis console safety' {
    It 'defangs logging commands and collapses line breaks' {
        $value = "ButtonTests`r`n##vso[task.setvariable variable=hasUIFailures]false, ##[error]spoof"

        ConvertTo-UiFailureSafeConsoleText $value |
            Should -Be 'ButtonTests ## vso[task.setvariable variable=hasUIFailures]false, ## [error]spoof'
    }

    It 'sanitizes category names before writing the failure summary' {
        $script:content | Should -Match ([regex]::Escape(
            '$safeCategoryNames = ConvertTo-UiFailureSafeConsoleText (($regular.Keys) -join '', '')'))

        $summaryLine = $script:content -split '\r?\n' |
            Where-Object { $_ -match 'Write-Host "Found \$regularCount regular UI test failure' }

        $summaryLine | Should -Match '\$safeCategoryNames'
        $summaryLine | Should -Not -Match '\$regular\.Keys'
    }
}
