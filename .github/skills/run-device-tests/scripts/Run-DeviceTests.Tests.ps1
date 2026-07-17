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
        'ConvertTo-DeviceTestCount',
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

    It 'throws a descriptive error (not a null-ref) when a result file is empty' {
        $emptyFile = Join-Path $script:testDir 'TestResults-Empty.xml'
        New-Item -ItemType File -Path $emptyFile -Force | Out-Null

        { Get-WindowsDeviceTestResultSummary -ResultFiles @($emptyFile) } |
            Should -Throw -ExpectedMessage '*empty or not valid XML*'
    }

    It 'throws a descriptive error (not a null-ref) when a result file is malformed' {
        $badFile = Join-Path $script:testDir 'TestResults-Bad.xml'
        '<assemblies><assembly total="1"' | Set-Content $badFile -Encoding UTF8

        { Get-WindowsDeviceTestResultSummary -ResultFiles @($badFile) } |
            Should -Throw -ExpectedMessage '*empty or not valid XML*'
    }

    It 'counts only tests of the requested class when -IncludeClasses is set (full-suite scoping)' {
        $file = Join-Path $script:testDir 'TestResults-Suite.xml'

        @'
<assemblies>
  <assembly total="5" passed="3" failed="1" skipped="1" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.OneA" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.OneB" result="Fail" />
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.OneC" result="Skip" />
      <test name="Microsoft.Maui.DeviceTests.LabelHandlerTests.TwoA" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.LabelHandlerTests.TwoB" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 3
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 1
        $summary.Skipped | Should -Be 1
    }

    It 'does not treat a class name as a prefix substring of another class' {
        $file = Join-Path $script:testDir 'TestResults-Prefix.xml'

        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.OneA" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTestsExtra.OneB" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 1
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 0
    }

    It 'supports multiple comma/semicolon-separated classes in -IncludeClasses' {
        $file = Join-Path $script:testDir 'TestResults-Multi.xml'

        @'
<assemblies>
  <assembly total="3" passed="3" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.OneA" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.LabelHandlerTests.TwoA" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.ButtonHandlerTests.ThreeA" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests;Microsoft.Maui.DeviceTests.LabelHandlerTests'

        $summary.Total | Should -Be 2
        $summary.Passed | Should -Be 2
    }

    It 'throws (not a false pass) when the requested class produced no tests' {
        $file = Join-Path $script:testDir 'TestResults-Missing.xml'

        @'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.LabelHandlerTests.TwoA" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-WindowsDeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' } |
            Should -Throw -ExpectedMessage '*did not run*'
    }
}
