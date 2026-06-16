#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Run-DeviceTests.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
        'Get-CategoryFiltersFromTestFilter',
        'Select-WindowsDeviceTestCategories',
        'Get-WindowsDeviceTestResultSummary'
    )) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Windows device test category filtering' {
    It 'extracts Category filters from VSTest-style expressions' {
        Get-CategoryFiltersFromTestFilter -Filter 'Category=Window|Category=Button' |
            Should -Be @('Window', 'Button')
    }

    It 'selects matching discovered categories case-insensitively' {
        Select-WindowsDeviceTestCategories `
            -AllCategories @('Button', 'Window', 'Shell') `
            -Filter 'Category=window' |
            Should -Be @('Window')
    }

    It 'returns all categories when no category filter is supplied' {
        Select-WindowsDeviceTestCategories `
            -AllCategories @('Button', 'Window') `
            -Filter '' |
            Should -Be @('Button', 'Window')
    }
}

Describe 'Get-WindowsDeviceTestResultSummary' {
    BeforeEach {
        $script:testDir = Join-Path ([System.IO.Path]::GetTempPath()) "windows-device-results-$([guid]::NewGuid())"
        New-Item -ItemType Directory -Path $script:testDir -Force | Out-Null
    }

    AfterEach {
        Remove-Item -LiteralPath $script:testDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'aggregates xUnit assembly counters from Windows device-test XML files' {
        $file1 = Join-Path $script:testDir 'TestResults-One.xml'
        $file2 = Join-Path $script:testDir 'TestResults-Two.xml'

        @'
<assemblies>
  <assembly total="3" passed="2" failed="1" skipped="0" errors="0" />
</assemblies>
'@ | Set-Content $file1 -Encoding UTF8

        @'
<assemblies>
  <assembly total="2" passed="1" failed="0" skipped="1" errors="0" />
</assemblies>
'@ | Set-Content $file2 -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary -ResultFiles @($file1, $file2)

        $summary.Total | Should -Be 5
        $summary.Passed | Should -Be 3
        $summary.Failed | Should -Be 1
        $summary.Skipped | Should -Be 1
        $summary.Errors | Should -Be 0
    }
}
