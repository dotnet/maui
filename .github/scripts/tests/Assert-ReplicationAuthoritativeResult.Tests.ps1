#!/usr/bin/env pwsh

Set-StrictMode -Version Latest

BeforeAll {
    # The script cannot be dot-sourced: its param block is mandatory and would
    # prompt. The functions under test are lifted out of the real source by AST
    # so the test can never drift from a copy.
    $script:SourcePath = Join-Path $PSScriptRoot '..' 'shared' 'Validate-ReplicationCandidate.ps1'
    $tokens = $null
    $errors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:SourcePath, [ref]$tokens, [ref]$errors)
    if ($errors.Count -gt 0) { throw "The production script does not parse: $($errors[0].Message)" }

    $wanted = @(
        'Assert-ReplicationAuthoritativeResult',
        'Get-ReplicationAuthoritativeTestEntries',
        'Get-ReportableArtifactName')
    foreach ($name in $wanted) {
        $found = $ast.FindAll({
            param($node)
            $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $node.Name -eq $name
        }, $true)
        if ($found.Count -ne 1) {
            throw "Expected exactly one definition of $name in the production script, found $($found.Count)."
        }
        . ([scriptblock]::Create($found[0].Extent.Text))
    }

    function New-VerificationRoot {
        param([Parameter(Mandatory = $true)][string]$Content,
              [string]$Extension = 'xml')

        $root = Join-Path ([IO.Path]::GetTempPath()) ("authresult-" + [guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Force -Path $root | Out-Null
        Set-Content -LiteralPath (Join-Path $root "verification-test-result.$Extension") `
            -Value $Content -Encoding utf8
        return $root
    }
}

Describe 'Assert-ReplicationAuthoritativeResult identity matching' {
    It 'accepts the exact document build 15073781 published' {
        # Verbatim from the run that reached certified-oracle: name equals method.
        $root = New-VerificationRoot -Content @'
<assemblies><assembly><collection>
<test name="ProportionalChildrenPreserveNaturalAbsoluteLayoutHeight" type="Microsoft.Maui.DeviceTests.Issue17673" method="ProportionalChildrenPreserveNaturalAbsoluteLayoutHeight" time="0.31" result="Fail"><failure><message>boom</message></failure></test>
</collection></assembly></assemblies>
'@
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $root `
            -TestClass 'Microsoft.Maui.DeviceTests.Issue17673' `
            -TestMethod 'ProportionalChildrenPreserveNaturalAbsoluteLayoutHeight' } | Should -Not -Throw
    }

    It 'accepts the exact document build 15073806 was refused for' {
        # Verbatim from a run that passed all four fix arms and was destroyed at
        # this gate: the runner humanized the display name, the method attribute
        # holds the identity.
        $root = New-VerificationRoot -Content @'
<assemblies><assembly><collection>
<test name="Picker ItemsSource Does Not Retain Picker After Unload" type="Microsoft.Maui.DeviceTests.Memory.Issue36272" method="PickerItemsSourceDoesNotRetainPickerAfterUnload" time="1.03" result="Fail"><failure><message>boom</message></failure></test>
</collection></assembly></assemblies>
'@
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $root `
            -TestClass 'Microsoft.Maui.DeviceTests.Memory.Issue36272' `
            -TestMethod 'PickerItemsSourceDoesNotRetainPickerAfterUnload' } | Should -Not -Throw
    }

    It 'still refuses a document recording a different method' {
        $root = New-VerificationRoot -Content @'
<assemblies><assembly><collection>
<test name="Some Other Test" type="Microsoft.Maui.DeviceTests.Memory.Issue36272" method="SomeOtherTest" time="1.03" result="Fail"><failure><message>boom</message></failure></test>
</collection></assembly></assemblies>
'@
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $root `
            -TestClass 'Microsoft.Maui.DeviceTests.Memory.Issue36272' `
            -TestMethod 'PickerItemsSourceDoesNotRetainPickerAfterUnload' } |
            Should -Throw -ExpectedMessage '*records a test method*'
    }

    It 'refuses a method that merely contains the claimed name' {
        # The old substring rule accepted this. An identity comparison must not.
        $root = New-VerificationRoot -Content @'
<assemblies><assembly><collection>
<test name="x" type="Microsoft.Maui.DeviceTests.Issue1" method="PickerItemsSourceDoesNotRetainPickerAfterUnloadTwice" time="1.03" result="Fail"><failure><message>boom</message></failure></test>
</collection></assembly></assemblies>
'@
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $root `
            -TestClass 'Microsoft.Maui.DeviceTests.Issue1' `
            -TestMethod 'PickerItemsSourceDoesNotRetainPickerAfterUnload' } |
            Should -Throw -ExpectedMessage '*records a test method*'
    }

    It 'falls back to the recorded name for a format carrying no method' {
        # TRX records only testName, so the fallback is the only rule available.
        $root = New-VerificationRoot -Content @'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results>
<UnitTestResult testName="Microsoft.Maui.DeviceTests.Issue1.OnlyName" outcome="Failed" />
</Results></TestRun>
'@ -Extension 'trx'
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $root `
            -TestClass 'Microsoft.Maui.DeviceTests.Issue1' -TestMethod 'OnlyName' } | Should -Not -Throw

        $wrong = New-VerificationRoot -Content @'
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results>
<UnitTestResult testName="Microsoft.Maui.DeviceTests.Issue1.Elsewhere" outcome="Failed" />
</Results></TestRun>
'@ -Extension 'trx'
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $wrong `
            -TestClass 'Microsoft.Maui.DeviceTests.Issue1' -TestMethod 'OnlyName' } |
            Should -Throw -ExpectedMessage '*records a test named*'
    }

    It 'reads the method name NUnit records for a UI test' {
        $root = New-VerificationRoot -Content @'
<test-run><test-case fullname="Microsoft.Maui.TestCases.Tests.Issue1.SomeUiTest" classname="Microsoft.Maui.TestCases.Tests.Issue1" methodname="SomeUiTest" result="Failed"><failure><message>boom</message></failure></test-case></test-run>
'@
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $root `
            -TestClass 'Microsoft.Maui.TestCases.Tests.Issue1' -TestMethod 'SomeUiTest' } | Should -Not -Throw

        $wrong = New-VerificationRoot -Content @'
<test-run><test-case fullname="Microsoft.Maui.TestCases.Tests.Issue1.SomeUiTest" classname="Microsoft.Maui.TestCases.Tests.Issue1" methodname="AnotherUiTest" result="Failed"><failure><message>boom</message></failure></test-case></test-run>
'@
        { Assert-ReplicationAuthoritativeResult -VerificationRoot $wrong `
            -TestClass 'Microsoft.Maui.TestCases.Tests.Issue1' -TestMethod 'SomeUiTest' } |
            Should -Throw -ExpectedMessage '*records a test method*'
    }
}

Describe 'Get-ReplicationAuthoritativeTestEntries' {
    It 'keeps the display name and the method apart' {
        $document = [System.Xml.XmlDocument]::new()
        $document.LoadXml(@'
<assemblies><assembly><collection>
<test name="Picker ItemsSource Does Not Retain" type="T" method="PickerItemsSourceDoesNotRetain" result="Fail" />
</collection></assembly></assemblies>
'@)
        $entries = @(Get-ReplicationAuthoritativeTestEntries -Document $document)
        $entries.Count | Should -Be 1
        $entries[0].Name | Should -Be 'Picker ItemsSource Does Not Retain'
        $entries[0].Method | Should -Be 'PickerItemsSourceDoesNotRetain'
    }

    It 'reports a method every shape defines, so a consumer never reads a missing property' {
        # The caller reads $entry.Method on every path, and production runs under
        # StrictMode where a missing property is fatal. Asserting "reading it does
        # not throw" is vacuous here, because Pester does not carry the file's
        # StrictMode into the run phase -- the same trap the $ErrorActionPreference
        # sweep found. Assert the property is present instead; that holds under
        # any mode.
        $shapes = @(
            '<assemblies><assembly><collection><test name="a" type="T" result="Fail" /></collection></assembly></assemblies>',
            '<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"><Results><UnitTestResult testName="a" outcome="Failed" /></Results></TestRun>',
            '<test-run><test-case fullname="a" classname="T" result="Failed" /></test-run>')
        foreach ($shape in $shapes) {
            $document = [System.Xml.XmlDocument]::new()
            $document.LoadXml($shape)
            $entries = @(Get-ReplicationAuthoritativeTestEntries -Document $document)
            $entries.Count | Should -Be 1
            $entries[0].PSObject.Properties.Name | Should -Contain 'Method' -Because "the shape $shape must define it"
        }
    }
}
