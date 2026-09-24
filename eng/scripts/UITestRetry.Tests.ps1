#Requires -Modules Pester

BeforeAll {
    Add-Type -Path (Join-Path $PSScriptRoot 'UITestRetry.cs')

    function New-Case([string]$Name, [string]$Outcome, [string]$Fixture = 'Example.Screen(Mac)') {
        @{ Name = $Name; Outcome = $Outcome; Fixture = $Fixture }
    }

    function New-Report([string]$Path, [object[]]$Cases, [string]$Deployment = 'deployment') {
        $results = @()
        $definitions = @()
        $entries = @()
        foreach ($case in $Cases) {
            $id = [guid]::NewGuid().ToString()
            $execution = [guid]::NewGuid().ToString()
            $name = [Security.SecurityElement]::Escape($case.Name)
            $fixture = [Security.SecurityElement]::Escape($case.Fixture)
            $files = if ($case.Attachment) {
                '<ResultFiles><ResultFile path="' + [Security.SecurityElement]::Escape($case.Attachment) + '" /></ResultFiles>'
            } else { '' }
            $results += "<UnitTestResult testId='$id' executionId='$execution' testName='$name' outcome='$($case.Outcome)' relativeResultsDirectory='$execution'>$files</UnitTestResult>"
            $definitions += "<UnitTest id='$id' name='$name'><Execution id='$execution'/><TestMethod className='$fixture' name='$name'/></UnitTest>"
            $entries += "<TestEntry testId='$id' executionId='$execution'/>"
        }
        $passed = @($Cases | Where-Object Outcome -eq 'Passed').Count
        $failed = @($Cases | Where-Object Outcome -eq 'Failed').Count
        $skipped = @($Cases | Where-Object Outcome -eq 'NotExecuted').Count
        $outcome = if ($failed) { 'Failed' } else { 'Completed' }
        @"
<TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <Times finish="2026-01-01T12:00:00Z"/>
  <TestSettings><Deployment runDeploymentRoot="$Deployment"/></TestSettings>
  <Results>$($results -join '')</Results>
  <TestDefinitions>$($definitions -join '')</TestDefinitions>
  <TestEntries>$($entries -join '')</TestEntries>
  <ResultSummary outcome="$outcome">
    <Counters total="$($Cases.Count)" executed="$($passed + $failed)" passed="$passed" failed="$failed" notExecuted="$skipped" aborted="0"/>
    <Output><StdOut>Original diagnostic output</StdOut></Output>
  </ResultSummary>
</TestRun>
"@ | Set-Content -LiteralPath $Path
    }
}

Describe 'UI test fixture retry reports' {
    BeforeEach {
        $original = Join-Path $TestDrive 'original.trx'
        $retry = Join-Path $TestDrive 'retry.trx'
        New-Report $original @(
            (New-Case 'Broken(1)' 'Failed')
            (New-Case 'OrderedSetup' 'Passed')
            (New-Case 'Ignored' 'NotExecuted')
            (New-Case 'Broken(1)' 'Passed' 'Other.Screen(Mac)')
        )
        New-Report $retry @(
            (New-Case 'Broken(1)' 'Passed')
            (New-Case 'OrderedSetup' 'Passed')
            (New-Case 'Ignored' 'NotExecuted')
        )
    }

    It 'selects the entire failed fixture within the original category filter' {
        [UITestRetry]::GetFilter($original, 'TestCategory=Shell|TestCategory=WebView') |
            Should -Be '(TestCategory=Shell|TestCategory=WebView)&(FullyQualifiedName~Example.Screen\(Mac\).)'
    }

    It 'deduplicates failed fixtures without selecting passing fixtures with the same short name' {
        New-Report $original @(
            (New-Case 'First' 'Failed')
            (New-Case 'Second' 'Failed')
            (New-Case 'First' 'Passed' 'Other.Screen(Mac)')
        )
        [UITestRetry]::GetFilter($original, '') | Should -Be 'FullyQualifiedName~Example.Screen\(Mac\).'
    }

    It 'selects distinct failed fixtures in a stable order' {
        New-Report $original @(
            (New-Case 'First' 'Failed' 'Example.Z(Android)')
            (New-Case 'Second' 'Failed' 'Example.A(Android)')
        )
        [UITestRetry]::GetFilter($original, '') |
            Should -Be 'FullyQualifiedName~Example.A\(Android\).|FullyQualifiedName~Example.Z\(Android\).'
    }

    It 'preserves skips when VSTest leaves notExecuted at zero in <Report> reports' -ForEach @(
        @{ Report = 'initial'; ZeroOriginal = $true; ZeroRetry = $false }
        @{ Report = 'retry'; ZeroOriginal = $false; ZeroRetry = $true }
        @{ Report = 'both'; ZeroOriginal = $true; ZeroRetry = $true }
    ) {
        foreach ($path in @(
            if ($ZeroOriginal) { $original }
            if ($ZeroRetry) { $retry }
        )) {
            [xml]$xml = Get-Content -Raw $path
            $xml.TestRun.ResultSummary.Counters.notExecuted = '0'
            $xml.Save($path)
        }

        [UITestRetry]::GetFilter($original, 'TestCategory=Shell') |
            Should -Be '(TestCategory=Shell)&(FullyQualifiedName~Example.Screen\(Mac\).)'
        [UITestRetry]::Merge($original, $retry, $original) | Should -BeTrue
        [xml]$merged = Get-Content -Raw $original
        $merged.TestRun.Results.UnitTestResult.Count | Should -Be 4
        @($merged.TestRun.Results.UnitTestResult | Where-Object outcome -eq 'NotExecuted').Count | Should -Be 1
        $merged.TestRun.ResultSummary.Counters.executed | Should -Be '3'
        $merged.TestRun.ResultSummary.Counters.passed | Should -Be '3'
        $merged.TestRun.ResultSummary.Counters.failed | Should -Be '0'
        $merged.TestRun.ResultSummary.Counters.notExecuted | Should -Be '1'
    }

    It 'preserves unselected passes and skips while replacing all retried execution metadata' {
        [xml]$before = Get-Content -Raw $original
        [xml]$retried = Get-Content -Raw $retry
        [UITestRetry]::Merge($original, $retry, $original) | Should -BeTrue
        [xml]$merged = Get-Content -Raw $original
        $merged.TestRun.Results.UnitTestResult.Count | Should -Be 4
        $merged.TestRun.ResultSummary.outcome | Should -Be 'Completed'
        $merged.TestRun.ResultSummary.Counters.total | Should -Be '4'
        $merged.TestRun.ResultSummary.Counters.executed | Should -Be '3'
        $merged.TestRun.ResultSummary.Counters.passed | Should -Be '3'
        $merged.TestRun.ResultSummary.Counters.notExecuted | Should -Be '1'
        $merged.TestRun.ResultSummary.Counters.failed | Should -Be '0'
        $merged.TestRun.Results.UnitTestResult.executionId |
            Should -Contain $before.TestRun.Results.UnitTestResult[3].executionId
        foreach ($result in $retried.TestRun.Results.UnitTestResult) {
            $merged.TestRun.Results.UnitTestResult.executionId | Should -Contain $result.executionId
            $merged.TestRun.TestDefinitions.UnitTest.Execution.id | Should -Contain $result.executionId
            $merged.TestRun.TestEntries.TestEntry.executionId | Should -Contain $result.executionId
        }
    }

    It 'retains a new failure in a previously passing case from the retried fixture' {
        New-Report $retry @(
            (New-Case 'Broken(1)' 'Passed')
            (New-Case 'OrderedSetup' 'Failed')
            (New-Case 'Ignored' 'NotExecuted')
        )
        [UITestRetry]::Merge($original, $retry, $original, $false) | Should -BeFalse
        [xml]$merged = Get-Content -Raw $original
        $merged.TestRun.ResultSummary.outcome | Should -Be 'Failed'
        $merged.TestRun.ResultSummary.Counters.failed | Should -Be '1'
        ($merged.TestRun.Results.UnitTestResult | Where-Object outcome -eq 'Failed').testName | Should -Be 'OrderedSetup'
    }

    It 'leaves the original report untouched when any selected case is absent' -ForEach @(
        @{ Missing = 'Broken(1)' }
        @{ Missing = 'OrderedSetup' }
        @{ Missing = 'Ignored' }
    ) {
        $cases = @(
            (New-Case 'Broken(1)' 'Passed')
            (New-Case 'OrderedSetup' 'Passed')
            (New-Case 'Ignored' 'NotExecuted')
        ) | Where-Object Name -ne $Missing
        New-Report $retry $cases
        $before = Get-Content -Raw $original
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*every selected fixture case*'
        Get-Content -Raw $original | Should -Be $before
    }

    It 'rejects extra cases rather than broadening coverage through a prefix collision' {
        New-Report $retry @(
            (New-Case 'Broken(1)' 'Passed')
            (New-Case 'OrderedSetup' 'Passed')
            (New-Case 'Ignored' 'NotExecuted')
            (New-Case 'Unexpected' 'Passed' 'Example.ScreenExtra(Mac)')
        )
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*no additional cases*'
    }

    It 'does not turn a failed retry command into a passing report' {
        $before = Get-Content -Raw $original
        { [UITestRetry]::Merge($original, $retry, $original, $false) } | Should -Throw '*command failed*'
        Get-Content -Raw $original | Should -Be $before
    }

    It 'does not replace an executed case with a skipped retry' -ForEach @(
        @{ Skipped = 'Broken(1)' }
        @{ Skipped = 'OrderedSetup' }
    ) {
        $cases = @(
            (New-Case 'Broken(1)' 'Passed')
            (New-Case 'OrderedSetup' 'Passed')
            (New-Case 'Ignored' 'NotExecuted')
        )
        ($cases | Where-Object Name -eq $Skipped).Outcome = 'NotExecuted'
        New-Report $retry $cases
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*skipped retry*'
    }

    It 'rejects duplicate identities' {
        New-Report $retry @((New-Case 'Duplicate' 'Passed'), (New-Case 'Duplicate' 'Passed'))
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*duplicate test identities*'
    }

    It 'rejects missing execution metadata' {
        [xml]$xml = Get-Content -Raw $retry
        $null = $xml.TestRun.TestEntries.RemoveChild($xml.TestRun.TestEntries.FirstChild)
        $xml.Save($retry)
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*execution metadata*'
    }

    It 'rejects mismatched execution metadata' {
        [xml]$xml = Get-Content -Raw $retry
        $xml.TestRun.TestDefinitions.UnitTest[0].Execution.id = [guid]::NewGuid().ToString()
        $xml.Save($retry)
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*execution metadata*'
    }

    It 'rejects invalid <Counter> counters with value <Value>' -ForEach @(
        @{ Counter = 'total'; Value = '99' }
        @{ Counter = 'executed'; Value = '4' }
        @{ Counter = 'passed'; Value = '3' }
        @{ Counter = 'failed'; Value = '0' }
        @{ Counter = 'notExecuted'; Value = '2' }
        @{ Counter = 'notExecuted'; Value = '-1' }
        @{ Counter = 'notExecuted'; Value = 'invalid' }
    ) {
        [xml]$xml = Get-Content -Raw $original
        $xml.TestRun.ResultSummary.Counters.SetAttribute($Counter, $Value)
        $xml.Save($original)
        { [UITestRetry]::GetFilter($original, '') } | Should -Throw '*counters*'
    }

    It 'rejects aborted or incomplete runs' -ForEach @(
        @{ Outcome = 'Aborted' }
        @{ Outcome = 'InProgress' }
    ) {
        [xml]$xml = Get-Content -Raw $original
        $xml.TestRun.ResultSummary.outcome = $Outcome
        $xml.Save($original)
        { [UITestRetry]::GetFilter($original, '') } | Should -Throw '*incomplete test run*'
    }

    It 'rejects a host error even when individual results look complete' {
        [xml]$xml = Get-Content -Raw $retry
        $info = $xml.CreateElement('RunInfo', $xml.DocumentElement.NamespaceURI)
        $info.SetAttribute('outcome', 'Error')
        $null = $xml.TestRun.ResultSummary.AppendChild($info)
        $xml.Save($retry)
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*test-host error*'
    }

    It 'rejects an unsuccessful command with no failed tests to retry' {
        { [UITestRetry]::GetFilter($retry, '') } | Should -Throw '*did not report any failed fixtures*'
    }

    It 'rejects a failed summary even when every retry result passed' {
        [xml]$xml = Get-Content -Raw $retry
        $xml.TestRun.ResultSummary.outcome = 'Failed'
        $xml.Save($retry)
        $before = Get-Content -Raw $original
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*outcome does not match*'
        Get-Content -Raw $original | Should -Be $before
    }

    It 'rejects a completed summary that contains failed results' {
        [xml]$xml = Get-Content -Raw $original
        $xml.TestRun.ResultSummary.outcome = 'Completed'
        $xml.Save($original)
        { [UITestRetry]::GetFilter($original, '') } | Should -Throw '*outcome does not match*'
    }

    It 'rejects an empty retry report' {
        New-Report $retry @()
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw '*no results*'
    }

    It 'rejects DTDs and malformed XML' -ForEach @(
        @{ Content = '<!DOCTYPE TestRun [<!ENTITY external SYSTEM "file:///not-read">]><TestRun/>' }
        @{ Content = '<TestRun' }
    ) {
        Set-Content -LiteralPath $retry -Value $Content
        { [UITestRetry]::Merge($original, $retry, $original) } | Should -Throw
    }

    It 'rejects fixture names containing filter operators' {
        New-Report $original @((New-Case 'Broken' 'Failed' 'Example.Screen(Mac)|FullyQualifiedName~Other'))
        { [UITestRetry]::GetFilter($original, '') } | Should -Throw '*unsupported name*'
    }

    It 'keeps attachments resolvable from the retry deployment root' {
        $case = New-Case 'Broken(1)' 'Failed'
        $case.Attachment = 'host/failure.png'
        New-Report $retry @($case, (New-Case 'OrderedSetup' 'Passed'), (New-Case 'Ignored' 'NotExecuted')) 'retry-deployment'
        [xml]$source = Get-Content -Raw $retry
        $execution = $source.TestRun.Results.UnitTestResult[0].relativeResultsDirectory
        $expected = Join-Path $TestDrive "retry-deployment/In/$execution/host/failure.png"
        $null = New-Item -ItemType Directory -Path (Split-Path $expected) -Force
        Set-Content -LiteralPath $expected -Value 'diagnostic attachment'
        [UITestRetry]::Merge($original, $retry, $original, $false) | Should -BeFalse
        [xml]$merged = Get-Content -Raw $original
        $attachment = ($merged.TestRun.Results.UnitTestResult | Where-Object outcome -eq 'Failed').ResultFiles.ResultFile.path
        $attachment | Should -Be $expected
        Get-Content -LiteralPath $attachment | Should -Be 'diagnostic attachment'
        $merged.TestRun.TestSettings.Deployment.runDeploymentRoot | Should -Be 'deployment'
    }

    It 'rejects retry attachments outside the results directory' {
        $case = New-Case 'Broken(1)' 'Failed'
        $case.Attachment = '../../../../outside.png'
        New-Report $retry @($case, (New-Case 'OrderedSetup' 'Passed'), (New-Case 'Ignored' 'NotExecuted'))
        $before = Get-Content -Raw $original
        { [UITestRetry]::Merge($original, $retry, $original, $false) } | Should -Throw '*outside the test results directory*'
        Get-Content -Raw $original | Should -Be $before
    }
}

Describe 'UI test retry execution' {
    BeforeEach {
        $script:Attempts = [Collections.Generic.List[object]]::new()
        $script:RetryExitCode = 0
        $script:OmitRetryReport = $false
        $script:RetryOutcome = 'Passed'
        $script:InitialOutcome = 'Failed'
        $script:ResultsPath = Join-Path $TestDrive ("run-" + [guid]::NewGuid() + '.trx')
        $script:Runner = [Func[string, string, int]] {
            param($filter, $path)
            $script:Attempts.Add(@{ Filter = $filter; Path = $path })
            if ($script:Attempts.Count -eq 1) {
                New-Report $path @((New-Case 'Broken' $script:InitialOutcome), (New-Case 'Other' 'Passed' 'Other.Screen(Mac)'))
                return $(if ($script:InitialOutcome -eq 'Passed') { 0 } else { 1 })
            }
            if (-not $script:OmitRetryReport) {
                New-Report $path @((New-Case 'Broken' $script:RetryOutcome))
            }
            return $script:RetryExitCode
        }
    }

    It 'executes one retry, retains first-run passes, and archives both attempts' {
        [UITestRetry]::Run($script:ResultsPath, 'TestCategory=Shell', $true, $script:Runner)
        $script:Attempts.Count | Should -Be 2
        $script:Attempts[1].Filter | Should -Be '(TestCategory=Shell)&(FullyQualifiedName~Example.Screen\(Mac\).)'
        [xml]$merged = Get-Content -Raw $script:ResultsPath
        $merged.TestRun.ResultSummary.Counters.passed | Should -Be '2'
        Test-Path -LiteralPath $script:Attempts[1].Path | Should -BeFalse
        @(Get-ChildItem (Join-Path $TestDrive 'TestResultsFailures') -Recurse -Filter '*.trx').Count | Should -Be 2
    }

    It 'does not retry a passing run' {
        $script:InitialOutcome = 'Passed'
        [UITestRetry]::Run($script:ResultsPath, '', $true, $script:Runner)
        $script:Attempts.Count | Should -Be 1
    }

    It 'does not retry local failures when CI retrying is disabled' {
        { [UITestRetry]::Run($script:ResultsPath, '', $false, $script:Runner) } | Should -Throw '*UI test command failed*'
        $script:Attempts.Count | Should -Be 1
    }

    It 'does not reuse stale reports from an earlier command' {
        New-Report $script:ResultsPath @((New-Case 'Stale' 'Failed'))
        $runner = [Func[string, string, int]] { param($filter, $path); return 1 }
        { [UITestRetry]::Run($script:ResultsPath, '', $true, $runner) } | Should -Throw '*without a retryable test report*'
        Test-Path -LiteralPath $script:ResultsPath | Should -BeFalse
    }

    It 'fails when a retry produces no report' {
        $script:OmitRetryReport = $true
        { [UITestRetry]::Run($script:ResultsPath, '', $true, $script:Runner) } | Should -Throw
        [xml]$original = Get-Content -Raw $script:ResultsPath
        $original.TestRun.ResultSummary.outcome | Should -Be 'Failed'
        $script:Attempts.Count | Should -Be 2
    }

    It 'does not add another retry when tests remain failed' {
        $script:RetryExitCode = 1
        $script:RetryOutcome = 'Failed'
        { [UITestRetry]::Run($script:ResultsPath, '', $true, $script:Runner) } | Should -Throw '*failed after retrying*'
        $script:Attempts.Count | Should -Be 2
    }

    It 'fails when a successful command contains failed test results' {
        $script:RetryOutcome = 'Failed'
        { [UITestRetry]::Run($script:ResultsPath, '', $true, $script:Runner) } | Should -Throw '*failed after retrying*'
    }
}

Describe 'UI pipeline retry wiring' {
    It 'uses the same fixture retry runner on every platform' -ForEach @(
        @{ Platform = 'android' }
        @{ Platform = 'ios' }
        @{ Platform = 'catalyst' }
        @{ Platform = 'windows' }
    ) {
        $source = Get-Content -Raw (Join-Path $PSScriptRoot "../devices/$Platform.cake")
        $source | Should -Match 'RunUITestsWithRetry\('
        $source | Should -Not -Match 'RunTestWithLocalDotNet\('
    }

    It 'does not repeat the entire UI pipeline task and excludes intermediate retry reports' {
        $pipeline = Get-Content -Raw (Join-Path $PSScriptRoot '../pipelines/common/ui-tests-steps.yml')
        $pipeline | Should -Not -Match '(?s)displayName: \$\(Agent.JobName\).*?retryCountOnTaskFailure'
        $pipeline | Should -Match '!\$\(TestResultsDirectory\)/\*\.retry\.trx'
    }

    It 'disables Play Store updates only on a newly created CI emulator and verifies the result' {
        $source = Get-Content -Raw (Join-Path $PSScriptRoot '../devices/android.cake')
        $source | Should -Match '(?s)if \(IsCIBuild\(\) && deviceCreate && emulatorProcess != null\).*?pm disable-user --user 0 com.android.vending'
        $source | Should -Match 'pm list packages -d --user 0 com.android.vending'
        $source | Should -Match 'throw new InvalidOperationException\("Could not disable Play Store'
    }
}
