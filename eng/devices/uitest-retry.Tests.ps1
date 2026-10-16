#Requires -Modules Pester

BeforeAll {
    Add-Type -TypeDefinition (Get-Content (Join-Path $PSScriptRoot 'uitest-retry.cake') -Raw)

    function New-NUnitReport {
        param([string] $Cases, [string] $Properties = '', [int] $Total = 2, [string] $Attributes = '')
        [System.Xml.Linq.XDocument]::Parse(@"
<test-run result="Failed" total="$Total" $Attributes>
  <test-suite type="TestFixture" result="Failed" site="Child">
    <properties>$Properties</properties>
    $Cases
  </test-suite>
</test-run>
"@)
    }

    function Get-RetryFilter {
        param([System.Xml.Linq.XDocument] $Report, [string] $Filter = 'TestCategory=Editor|TestCategory=Effects|TestCategory=Essentials')
        [UITestRetry]::GetFilter($Filter, [System.Xml.Linq.XDocument[]]@($Report))
    }

    function New-Trx {
        param([hashtable] $Outcomes, [string] $SummaryOutcome = 'Failed')
        $results = ''
        $definitions = ''
        $entries = ''
        foreach ($id in $Outcomes.Keys) {
            $results += "<UnitTestResult testId='$id' executionId='$id-execution' relativeResultsDirectory='$id-execution' outcome='$($Outcomes[$id])' />"
            $definitions += "<UnitTest id='$id'><Execution id='$id-execution'/></UnitTest>"
            $entries += "<TestEntry testId='$id' executionId='$id-execution' />"
        }
        [System.Xml.Linq.XDocument]::Parse(@"
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Times finish="2026-10-16T00:00:00Z"/>
  <Results>$results</Results>
  <TestDefinitions>$definitions</TestDefinitions>
  <TestEntries>$entries</TestEntries>
  <ResultSummary outcome="$SummaryOutcome">
    <Counters total="0" executed="0" passed="0" failed="0" notExecuted="0" />
  </ResultSummary>
</TestRun>
"@)
    }

    $ns = [System.Xml.Linq.XNamespace]::Get('http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
}

Describe 'UI test retry category selection' {
    It 'retries the failed fixture category, not passing categories in the group' {
        $report = New-NUnitReport -Properties '<property name="Category" value="Editor"/>' -Cases @'
<test-case result="Passed"/>
<test-case result="Failed"/>
'@
        Get-RetryFilter $report | Should -Be 'TestCategory=Editor'
    }

    It 'collects method categories, deduplicates them, and retains group order' {
        $report = New-NUnitReport -Total 3 -Cases @'
<test-case result="Failed"><properties><property name="Category" value="Essentials"/><property name="Category" value="Unrelated"/></properties></test-case>
<test-case result="Failed"><properties><property name="Category" value="Editor"/></properties></test-case>
<test-case result="Failed"><properties><property name="Category" value="Editor"/></properties></test-case>
'@
        Get-RetryFilter $report | Should -Be 'TestCategory=Editor|TestCategory=Essentials'
    }

    It 'keeps CI shard categories instead of expanding their umbrella category' {
        $report = New-NUnitReport -Total 1 -Properties '<property name="Category" value="Shell"/>' -Cases @'
<test-case result="Failed"><properties><property name="Category" value="Shell2"/></properties></test-case>
'@
        Get-RetryFilter $report 'TestCategory=Shell1|TestCategory=Shell2' | Should -Be 'TestCategory=Shell2'
    }

    It 'includes fixture teardown failures even when individual tests passed' {
        $report = [System.Xml.Linq.XDocument]::Parse(@'
<test-run result="Failed" total="1">
  <test-suite result="Failed" site="TearDown">
    <properties><property name="Category" value="Editor"/></properties>
    <test-case result="Passed"/>
  </test-suite>
</test-run>
'@)
        Get-RetryFilter $report | Should -Be 'TestCategory=Editor'
    }

    It 'rejects <Name> results so the caller retains the original filter' -TestCases @(
        @{ Name = 'incomplete'; Total = 3; Attributes = ''; Result = 'Failed'; Category = 'Editor' }
        @{ Name = 'canceled'; Total = 2; Attributes = 'label="Cancelled"'; Result = 'Failed'; Category = 'Editor' }
        @{ Name = 'unmapped'; Total = 2; Attributes = ''; Result = 'Failed'; Category = 'Unknown' }
        @{ Name = 'unfinished'; Total = 2; Attributes = ''; Result = 'Running'; Category = 'Editor' }
        @{ Name = 'no failures'; Total = 2; Attributes = ''; Result = 'Passed'; Category = 'Editor' }
    ) {
        param($Total, $Attributes, $Result, $Category)
        $report = New-NUnitReport -Total $Total -Attributes $Attributes `
            -Properties "<property name='Category' value='$Category'/>" `
            -Cases "<test-case result='Passed'/><test-case result='$Result'/>"
        { Get-RetryFilter $report } | Should -Throw
    }

    It 'does not silently drop an unmapped failure alongside a mapped failure' {
        $report = New-NUnitReport -Cases @'
<test-case result="Failed"><properties><property name="Category" value="Editor"/></properties></test-case>
<test-case result="Failed"/>
'@
        { Get-RetryFilter $report } | Should -Throw
    }

    It 'rejects missing results' {
        { [UITestRetry]::GetFilter('TestCategory=Editor', [System.Xml.Linq.XDocument[]]@()) } | Should -Throw
    }

    It 'does not broaden custom filters' -TestCases @(
        @{ Filter = '(TestCategory=Editor|TestCategory=Effects)&FullyQualifiedName~Issue' }
        @{ Filter = 'TestCategory~Editor' }
        @{ Filter = '' }
    ) {
        param($Filter)
        $report = New-NUnitReport -Total 1 -Properties '<property name="Category" value="Editor"/>' -Cases '<test-case result="Failed"/>'
        { Get-RetryFilter $report $Filter } | Should -Throw
    }
}

Describe 'UI test retry result publication' {
    It 'preserves untouched results while replacing retried tests and updating counters' {
        $original = New-Trx @{ editor = 'Failed'; effects = 'Passed'; essentials = 'NotExecuted' }
        $retry = New-Trx @{ editor = 'Passed'; newTest = 'Passed' } 'Completed'
        [UITestRetry]::MergeResults($original, $retry)
        $summary = $original.Root.Element($ns + 'ResultSummary')
        $counters = $summary.Element($ns + 'Counters')
        $summary.Attribute('outcome').Value | Should -Be 'Completed'
        $counters.Attribute('total').Value | Should -Be '4'
        $counters.Attribute('passed').Value | Should -Be '3'
        $counters.Attribute('failed').Value | Should -Be '0'
        $counters.Attribute('notExecuted').Value | Should -Be '1'
        $counters.Attribute('executed').Value | Should -Be '3'
        foreach ($section in @('Results', 'TestDefinitions', 'TestEntries')) {
            @($original.Root.Element($ns + $section).Elements()).Count | Should -Be 4
        }
        $editor = @($original.Root.Element($ns + 'Results').Elements() | Where-Object { $_.Attribute('testId').Value -eq 'editor' })[0]
        $editor.Attribute('relativeResultsDirectory').Value | Should -Be 'editor-execution'
    }

    It 'retains failures absent from an incomplete retry' {
        $original = New-Trx @{ editor = 'Failed'; effects = 'Failed'; essentials = 'Passed' }
        $retry = New-Trx @{ editor = 'Passed' } 'Completed'
        [UITestRetry]::MergeResults($original, $retry)
        $summary = $original.Root.Element($ns + 'ResultSummary')
        $summary.Attribute('outcome').Value | Should -Be 'Failed'
        $summary.Element($ns + 'Counters').Attribute('failed').Value | Should -Be '1'
    }

    It 'publishes persistent and newly introduced retry failures' {
        $original = New-Trx @{ editor = 'Failed'; effects = 'Passed' }
        $retry = New-Trx @{ editor = 'Failed'; newTest = 'Failed' }
        [UITestRetry]::MergeResults($original, $retry)
        $summary = $original.Root.Element($ns + 'ResultSummary')
        $summary.Attribute('outcome').Value | Should -Be 'Failed'
        $summary.Element($ns + 'Counters').Attribute('failed').Value | Should -Be '2'
    }
}

Describe 'Cross-platform UI test retry wiring' {
    It 'routes <Platform> UI tests through the shared retry runner' -TestCases @(
        @{ Platform = 'android' }
        @{ Platform = 'ios' }
        @{ Platform = 'catalyst' }
        @{ Platform = 'windows' }
    ) {
        param($Platform)
        $source = Get-Content (Join-Path $PSScriptRoot "$Platform.cake") -Raw
        $source | Should -Match '#load "\./uitests-shared\.cake"'
        $source | Should -Match '\bRunUITestsWithRetry\('
        $source | Should -Not -Match '\bRunTestWithLocalDotNet\('
    }

    It 'does not retry the entire pipeline task after the shared retry has finished' {
        $pipeline = Get-Content (Join-Path $PSScriptRoot '../pipelines/common/ui-tests-steps.yml') -Raw
        $runStep = [regex]::Match($pipeline, '(?s)- pwsh: \|\r?\n    Get-Content \$PSCommandPath.*?(?=\r?\n- bash:)')
        $runStep.Success | Should -BeTrue
        $runStep.Value | Should -Not -Match 'retryCountOnTaskFailure'
    }
}
