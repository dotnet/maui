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

    It 'counts only tests of the requested class when -IncludeClasses is set (matches on the xUnit type attribute)' {
        $file = Join-Path $script:testDir 'TestResults-Suite.xml'

        # Real xUnit v2 shape: the fully-qualified class is in `type`; `name` is the
        # (often theory/DisplayName) label, NOT the FQN.
        @'
<assemblies>
  <assembly total="5" passed="3" failed="1" skipped="1" errors="0">
    <collection>
      <test name="OneA" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneA" result="Pass" />
      <test name="OneB" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneB" result="Fail" />
      <test name="OneC" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneC" result="Skip" />
      <test name="TwoA" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoA" result="Pass" />
      <test name="TwoB" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoB" result="Pass" />
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

    It 'matches the class even when the test name is a theory/DisplayName string (regression: false INCONCLUSIVE #36577)' {
        $file = Join-Path $script:testDir 'TestResults-Theory.xml'

        # These `name` values never start with the FQN — the original name-based matcher
        # counted 0 here and forced a false INCONCLUSIVE even though the tests ran.
        @'
<assemblies>
  <assembly total="3" passed="2" failed="1" skipped="0" errors="0">
    <collection>
      <test name="PlatformView Transforms are not empty(size: 1)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Transforms" result="Pass" />
      <test name="CompletedFiresOnRealEnterKeyPress" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Updating Font Does Not Affect Alignment(initialSize: 10, newSize: 20)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Font" result="Fail" />
      <test name="Unrelated" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="Unrelated" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 3
        $summary.Passed | Should -Be 2
        $summary.Failed | Should -Be 1
    }

    It 'does not treat a class name as a prefix substring of another class' {
        $file = Join-Path $script:testDir 'TestResults-Prefix.xml'

        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="OneA" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneA" result="Pass" />
      <test name="OneB" type="Microsoft.Maui.DeviceTests.EntryHandlerTestsExtra" method="OneB" result="Fail" />
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

    It 'falls back to the fully-qualified name when a runner omits the type attribute' {
        $file = Join-Path $script:testDir 'TestResults-NoType.xml'

        @'
<assemblies>
  <assembly total="2" passed="1" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.OneA" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.LabelHandlerTests.TwoA" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'

        $summary.Total | Should -Be 1
        $summary.Passed | Should -Be 1
    }

    It 'supports multiple comma/semicolon-separated classes in -IncludeClasses' {
        $file = Join-Path $script:testDir 'TestResults-Multi.xml'

        @'
<assemblies>
  <assembly total="3" passed="3" failed="0" skipped="0" errors="0">
    <collection>
      <test name="OneA" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="OneA" result="Pass" />
      <test name="TwoA" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoA" result="Pass" />
      <test name="ThreeA" type="Microsoft.Maui.DeviceTests.ButtonHandlerTests" method="ThreeA" result="Pass" />
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

    It 'throws (not a false pass) when the requested class produced no tests, with diagnostics naming the classes present' {
        $file = Join-Path $script:testDir 'TestResults-Missing.xml'

        @'
<assemblies>
  <assembly total="1" passed="1" failed="0" skipped="0" errors="0">
    <collection>
      <test name="TwoA" type="Microsoft.Maui.DeviceTests.LabelHandlerTests" method="TwoA" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        # The throw must distinguish "target class absent" from "no results at all": it
        # reports the total tests found and a sample of the CLASSES present for diagnosis.
        { Get-WindowsDeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' } |
            Should -Throw -ExpectedMessage '*did not run*Total tests found in result file(s): 1*Sample classes present*LabelHandlerTests*'
    }

    It 'reports a zero total when the result file has no <test> nodes at all' {
        $file = Join-Path $script:testDir 'TestResults-NoTests.xml'

        @'
<assemblies>
  <assembly total="0" passed="0" failed="0" skipped="0" errors="0">
    <collection />
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-WindowsDeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' } |
            Should -Throw -ExpectedMessage '*Total tests found in result file(s): 0*'
    }
}
