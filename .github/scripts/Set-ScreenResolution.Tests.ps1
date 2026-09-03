#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $screenResolutionScript = Join-Path $PSScriptRoot '../../eng/scripts/Set-ScreenResolution.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $screenResolutionScript,
        [ref]$tokens,
        [ref]$parseErrors)

    if ($parseErrors.Count -gt 0) {
        throw "Set-ScreenResolution.ps1 has parse errors: $($parseErrors -join '; ')"
    }

    $functionDefinitions = $ast.FindAll({
        param($node)
        $node -is [System.Management.Automation.Language.FunctionDefinitionAst]
    }, $true)

    foreach ($functionName in @(
        'Get-ScreenResolutionProbeAction',
        'Test-ScreenResolutionApplySucceeded'
    )) {
        $definition = $functionDefinitions |
            Where-Object Name -EQ $functionName |
            Select-Object -First 1

        if ($null -eq $definition) {
            throw "Function '$functionName' was not found in Set-ScreenResolution.ps1"
        }

        Invoke-Expression $definition.Extent.Text
    }

    $setResolutionDefinition = $functionDefinitions |
        Where-Object Name -EQ 'Set-ScreenResolution' |
        Select-Object -First 1

    if ($null -eq $setResolutionDefinition) {
        throw "Function 'Set-ScreenResolution' was not found in Set-ScreenResolution.ps1"
    }

    $setResolutionBody = $setResolutionDefinition.Extent.Text
}

Describe 'Set-ScreenResolution native result decisions' {
    It 'returns <Expected> for CDS_TEST result <Result>' -ForEach @(
        @{ Result = 0; Expected = 'Apply' }
        @{ Result = -1; Expected = 'Apply' }
        @{ Result = -2; Expected = 'Reject' }
        @{ Result = -3; Expected = 'Apply' }
        @{ Result = -4; Expected = 'Apply' }
        @{ Result = -5; Expected = 'Apply' }
    ) {
        Get-ScreenResolutionProbeAction -Result $Result | Should -Be $Expected
    }

    It 'returns <Expected> for real apply result <Result>' -ForEach @(
        @{ Result = 0; Expected = $true }
        @{ Result = 1; Expected = $true }
        @{ Result = -1; Expected = $false }
        @{ Result = -2; Expected = $false }
        @{ Result = -3; Expected = $false }
        @{ Result = -4; Expected = $false }
        @{ Result = -5; Expected = $false }
    ) {
        Test-ScreenResolutionApplySucceeded -Result $Result | Should -Be $Expected
    }

    It 'uses the pure decisions in the native flow' {
        $setResolutionBody | Should -Match 'Get-ScreenResolutionProbeAction\s+-Result\s+\$testResult'
        $setResolutionBody | Should -Match 'Test-ScreenResolutionApplySucceeded\s+-Result\s+\$changeResult'
    }
}
