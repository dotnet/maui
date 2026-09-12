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

    foreach ($functionName in @(
        'Resolve-AgentTempOutputPath',
        'ConvertTo-UiFailureSafeConsoleText'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "$functionName not found"
        }

        Invoke-Expression $function.Extent.Text
    }

    $script:pipelineContent = Get-Content -Raw (
        Join-Path $PSScriptRoot '../../../eng/pipelines/ci-copilot.yml')
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

Describe 'Prepare UI test failure analysis output safety' {
    It 'accepts an output nested under the trusted agent temp root' {
        $tempRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'agent-temp'))
        $output = Join-Path $tempRoot 'nested/uifail-input.md'

        Resolve-AgentTempOutputPath -OutputPath $output -TrustedTempRoot $tempRoot |
            Should -Be ([System.IO.Path]::GetFullPath($output))
    }

    It 'rejects an output outside or beside the trusted agent temp root' {
        $parent = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot 'path-fixture'))
        $tempRoot = Join-Path $parent 'agent-temp'

        {
            Resolve-AgentTempOutputPath `
                -OutputPath (Join-Path $parent 'checkout/uifail-input.md') `
                -TrustedTempRoot $tempRoot
        } | Should -Throw '*must be contained by AgentTempDirectory*'

        {
            Resolve-AgentTempOutputPath `
                -OutputPath (Join-Path $parent 'agent-temp-sibling/uifail-input.md') `
                -TrustedTempRoot $tempRoot
        } | Should -Throw '*must be contained by AgentTempDirectory*'
    }

    It 'fails closed when the trusted agent temp root is unavailable' {
        {
            Resolve-AgentTempOutputPath `
                -OutputPath (Join-Path $PSScriptRoot 'uifail-input.md') `
                -TrustedTempRoot ''
        } | Should -Throw '*AgentTempDirectory is required*'
    }

    It 'passes the trusted Azure temp root explicitly from the pipeline' {
        $script:pipelineContent | Should -Match ([regex]::Escape(
            '-AgentTempDirectory "$(Agent.TempDirectory)"'))
    }
}
