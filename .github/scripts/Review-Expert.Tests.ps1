#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for the expert-review dimension routing in Review-Expert.ps1.

    Get-ExpertExpectedDimensions mirrors the Dimension Routing section of
    .github/agents/maui-expert-reviewer.md and refuses to run when that section's
    pinned hash changes. These tests keep the mirrored rules, the hash, and the
    rulebook headings in sync.

.EXAMPLE
    Invoke-Pester ./Review-Expert.Tests.ps1 -Output Detailed
#>

BeforeAll {
    # Review-Expert.ps1 runs a stage at top level, so load only the routing functions from its AST.
    $scriptPath = Join-Path $PSScriptRoot 'Review-Expert.ps1'
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$null, [ref]$null)
    foreach ($name in 'Get-ExpertExpectedDimensions', 'Get-ExpertDimensionNames') {
        $function = $ast.Find({
                param($node)
                $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -eq $name
            }, $true)
        $definition = $function.Extent.Text.Replace('$PSScriptRoot', "'$PSScriptRoot'")
        . ([scriptblock]::Create($definition))
    }

    function Get-RoutedDimensions([string]$Path) {
        # The function returns its ids as a single array object; unroll it so Should sees the ids.
        $ids = Get-ExpertExpectedDimensions -Paths @($Path) -Diff ''
        $ids | ForEach-Object { [int]$_ }
    }
}

Describe 'Get-ExpertDimensionNames' {
    It 'reads all 32 dimension headings from the rulebook' {
        (Get-ExpertDimensionNames).Count | Should -Be 32
    }
}

Describe 'Get-ExpertExpectedDimensions' {
    It 'matches the pinned routing hash of the rulebook' {
        { Get-RoutedDimensions 'README.md' } | Should -Not -Throw
    }

    It 'routes <Path> to Handler Mapper and Property Patterns' -ForEach @(
        @{ Path = 'src/Controls/src/Core/Button/Button.Mapper.cs' }
        @{ Path = 'src/Core/src/PropertyMapperExtensions.cs' }
        @{ Path = 'src/Core/src/CommandMapper.cs' }
        @{ Path = 'src/Core/src/MappingCustomizationSequence.cs' }
        @{ Path = 'src/Controls/tests/Core.UnitTests/VisualElementMapperTests.cs' }
        @{ Path = 'src/Controls/tests/DeviceTests/MapperTests.cs' }
    ) {
        Get-RoutedDimensions $Path | Should -Contain 3
    }

    It 'does not route <Path> to Handler Mapper and Property Patterns' -ForEach @(
        @{ Path = 'src/Controls/src/Core/Button/Button.cs' }
        @{ Path = 'docs/design/HandlerResolution.md' }
        @{ Path = 'src/Controls/tests/Core.UnitTests/ButtonUnitTest.cs' }
    ) {
        Get-RoutedDimensions $Path | Should -Not -Contain 3
    }

    It 'routes any C# change, such as an [ElementHandler] declaration, to Backward Compatibility and Trimming/AOT' {
        $dimensions = Get-RoutedDimensions 'src/Controls/src/Core/Button/Button.cs'
        $dimensions | Should -Contain 12
        $dimensions | Should -Contain 18
    }

    It 'does not route a docs-only change to Backward Compatibility or Trimming/AOT' {
        $dimensions = Get-RoutedDimensions 'docs/design/HandlerResolution.md'
        $dimensions | Should -Not -Contain 12
        $dimensions | Should -Not -Contain 18
    }
}
