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

    # ─────────────────────────────────────────────────────────────────────────────
    # Method-level scoping: when the gate knows the PR's specific methods, the tally
    # counts ONLY those methods within the class — so a pre-existing/flaky failure in
    # an unrelated sibling method of the same class cannot falsely redden the verdict.
    # ─────────────────────────────────────────────────────────────────────────────

    It 'counts only the requested methods within the class when -IncludeMethods is set' {
        $file = Join-Path $script:testDir 'TestResults-Methods.xml'

        @'
<assemblies>
  <assembly total="4" passed="3" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Completed fires on real Enter key press" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Completed does not fire on IME candidate confirmation Enter" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedDoesNotFireOnIMECandidateEnter" result="Pass" />
      <test name="Unrelated sibling A" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SomeOtherEntryTest" result="Pass" />
      <test name="Unrelated sibling B" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="AnotherEntryTest" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress;CompletedDoesNotFireOnIMECandidateEnter'

        $summary.Total | Should -Be 2
        $summary.Passed | Should -Be 2
        $summary.Failed | Should -Be 0
    }

    It 'excludes an unrelated sibling failure in the same class (regression: no false FAILED from method-scoping)' {
        $file = Join-Path $script:testDir 'TestResults-Sibling.xml'

        # The target method passes; a DIFFERENT method in the same class fails. Class-only
        # scoping would report Failed=1 -> false FAILED. Method-scoping must report PASSED.
        @'
<assemblies>
  <assembly total="2" passed="1" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Completed fires on real Enter key press" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Pre-existing flaky Windows test" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UnrelatedFlakyTest" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        # Sanity: class-only scoping DOES see the sibling failure (the false FAILED we fix).
        $classOnly = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests'
        $classOnly.Failed | Should -Be 1

        # Method-scoping ignores the unrelated sibling -> clean PASSED.
        $scoped = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress'
        $scoped.Total | Should -Be 1
        $scoped.Passed | Should -Be 1
        $scoped.Failed | Should -Be 0
    }

    It 'preserves a GENUINE target-method failure under method-scoping (does not mask fix-incomplete)' {
        $file = Join-Path $script:testDir 'TestResults-Genuine.xml'

        # Mirrors build 14695686 (#36577): the PR added two methods; with the fix applied
        # one target method still fails. Method-scoping must STILL report that failure.
        @'
<assemblies>
  <assembly total="3" passed="2" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Completed fires on real Enter key press" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedFiresOnRealEnterKeyPress" result="Pass" />
      <test name="Completed does not fire on IME candidate confirmation Enter" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="CompletedDoesNotFireOnIMECandidateEnter" result="Fail" />
      <test name="Unrelated sibling" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SomeOtherEntryTest" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress;CompletedDoesNotFireOnIMECandidateEnter'

        $summary.Total | Should -Be 2
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 1
        # The failing test must be named (type.method) so the verdict is auditable.
        ($summary.FailedTests -join ';') | Should -BeLike '*EntryHandlerTests.CompletedDoesNotFireOnIMECandidateEnter*'
    }

    It 'counts every data-case of a target [Theory] method (same method attribute, different display names)' {
        $file = Join-Path $script:testDir 'TestResults-Theory-Method.xml'

        @'
<assemblies>
  <assembly total="4" passed="3" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Updating Font(initialSize: 10, newSize: 20)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UpdatingFont" result="Pass" />
      <test name="Updating Font(initialSize: 12, newSize: 24)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UpdatingFont" result="Pass" />
      <test name="Updating Font(initialSize: 14, newSize: 28)" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="UpdatingFont" result="Fail" />
      <test name="Unrelated" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="Unrelated" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'UpdatingFont'

        $summary.Total | Should -Be 3
        $summary.Passed | Should -Be 2
        $summary.Failed | Should -Be 1
    }

    It 'recovers the method from the FQN name when a runner omits the method attribute' {
        $file = Join-Path $script:testDir 'TestResults-Method-NoAttr.xml'

        @'
<assemblies>
  <assembly total="2" passed="1" failed="1" skipped="0" errors="0">
    <collection>
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.CompletedFiresOnRealEnterKeyPress" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" result="Pass" />
      <test name="Microsoft.Maui.DeviceTests.EntryHandlerTests.UnrelatedFlakyTest" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" result="Fail" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        $summary = Get-WindowsDeviceTestResultSummary `
            -ResultFiles @($file) `
            -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
            -IncludeMethods 'CompletedFiresOnRealEnterKeyPress'

        $summary.Total | Should -Be 1
        $summary.Passed | Should -Be 1
        $summary.Failed | Should -Be 0
    }

    It 'throws a method-aware error when the class ran but none of the target methods did' {
        $file = Join-Path $script:testDir 'TestResults-Method-Missing.xml'

        # The class IS present (2 tests) but neither is a target method -> distinct from
        # "class absent"; the throw must name the methods, not just the class.
        @'
<assemblies>
  <assembly total="2" passed="2" failed="0" skipped="0" errors="0">
    <collection>
      <test name="Sibling one" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SiblingOne" result="Pass" />
      <test name="Sibling two" type="Microsoft.Maui.DeviceTests.EntryHandlerTests" method="SiblingTwo" result="Pass" />
    </collection>
  </assembly>
</assemblies>
'@ | Set-Content $file -Encoding UTF8

        { Get-WindowsDeviceTestResultSummary `
                -ResultFiles @($file) `
                -IncludeClasses 'Microsoft.Maui.DeviceTests.EntryHandlerTests' `
                -IncludeMethods 'CompletedFiresOnRealEnterKeyPress' } |
            Should -Throw -ExpectedMessage '*contained the class(es)*but none of the target method(s)*CompletedFiresOnRealEnterKeyPress*did not run*'
    }
}
