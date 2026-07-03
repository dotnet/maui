#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for pure-function helpers in Review-PR.ps1.
    Currently covers:
      - Get-TrxResults  (parses VSTest TRX produced by `dotnet test --logger trx`)
      - Get-DotNetTestResults  (legacy console-output scraper, still used as fallback
                                when TRX is missing)

    These functions sit on the critical path of STEP 3 (UI Test Execution
    Results in the AI summary review). A regression here can silently
    misrender per-test counts (e.g. "1/1 (1 ❌)" instead of "75/619 (544 ❌)")
    so they're worth pinning with focused tests.

.EXAMPLE
    Invoke-Pester ./Review-PR.Tests.ps1
    Invoke-Pester ./Review-PR.Tests.ps1 -Output Detailed
#>

BeforeAll {
    # Source just the helper functions we want to test out of Review-PR.ps1.
    # We can't dot-source the entire script because it has top-level imperative
    # logic (banner, prerequisites, step driver) that runs at parse time.
    $reviewScript = Join-Path $PSScriptRoot 'Review-PR.ps1'
    $content = Get-Content -Raw $reviewScript

    function Get-FunctionBody {
        param([string]$ScriptText, [string]$FunctionName)
        $start = $ScriptText.IndexOf("function $FunctionName")
        if ($start -lt 0) { throw "Function '$FunctionName' not found" }
        $i = $ScriptText.IndexOf('{', $start)
        $depth = 0; $end = -1
        for (; $i -lt $ScriptText.Length; $i++) {
            $c = $ScriptText[$i]
            if ($c -eq '{') { $depth++ }
            elseif ($c -eq '}') { $depth--; if ($depth -eq 0) { $end = $i; break } }
        }
        return $ScriptText.Substring($start, $end - $start + 1)
    }

    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-TrxResults')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-DotNetTestResults')
}

Describe 'Get-TrxResults' {
    BeforeAll {
        $script:fixtureDir = Join-Path ([System.IO.Path]::GetTempPath()) "trx-fixtures-$(New-Guid)"
        New-Item -ItemType Directory -Path $script:fixtureDir -Force | Out-Null
    }

    AfterAll {
        Remove-Item -Path $script:fixtureDir -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'returns null for a missing file' {
        $r = Get-TrxResults -TrxPath '/does/not/exist.trx'
        $r | Should -BeNullOrEmpty
    }

    It 'returns null for an empty path' {
        Get-TrxResults -TrxPath '' | Should -BeNullOrEmpty
        Get-TrxResults -TrxPath $null | Should -BeNullOrEmpty
    }

    It 'parses aggregate counters from ResultSummary/Counters' {
        $trx = Join-Path $script:fixtureDir 'aggregate.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="1" name="r" runUser="ci" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Failed">
    <Counters total="619" executed="619" passed="75" failed="544" />
  </ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Total   | Should -Be 619
        $r.Passed  | Should -Be 75
        $r.Failed  | Should -Be 544
        $r.Skipped | Should -Be 0
    }

    It 'computes Skipped as Total-Executed when not separately tracked' {
        $trx = Join-Path $script:fixtureDir 'skipped.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="2" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="100" executed="93" passed="90" failed="3" />
  </ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Total   | Should -Be 100
        $r.Skipped | Should -Be 7   # 100 - 93
    }

    It 'parses individual UnitTestResult nodes into the Results list' {
        $trx = Join-Path $script:fixtureDir 'individual.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="3" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Failed">
    <Counters total="3" executed="3" passed="1" failed="1" />
  </ResultSummary>
  <Results>
    <UnitTestResult testName="Foo" duration="00:00:01.0" outcome="Passed" />
    <UnitTestResult testName="Bar" duration="00:00:02.0" outcome="Failed">
      <Output>
        <ErrorInfo>
          <Message>Expected: True; Actual: False</Message>
          <StackTrace>at Bar() in F.cs:line 42</StackTrace>
        </ErrorInfo>
      </Output>
    </UnitTestResult>
    <UnitTestResult testName="Baz" duration="00:00:00.5" outcome="NotExecuted" />
  </Results>
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Results.Count | Should -Be 3

        $foo = $r.Results | Where-Object { $_.name -eq 'Foo' }
        $foo.status | Should -Be 'Passed'

        $bar = $r.Results | Where-Object { $_.name -eq 'Bar' }
        $bar.status   | Should -Be 'Failed'
        $bar.error    | Should -Be 'Expected: True; Actual: False'
        $bar.stack    | Should -Be 'at Bar() in F.cs:line 42'

        $baz = $r.Results | Where-Object { $_.name -eq 'Baz' }
        $baz.status | Should -Be 'Skipped'   # NotExecuted normalized to Skipped
    }

    It 'normalizes Inconclusive outcome to Skipped' {
        $trx = Join-Path $script:fixtureDir 'inconclusive.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="4" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="1" executed="0" passed="0" failed="0" />
  </ResultSummary>
  <Results>
    <UnitTestResult testName="Maybe" duration="00:00:00" outcome="Inconclusive" />
  </Results>
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        (Get-TrxResults -TrxPath $trx).Results[0].status | Should -Be 'Skipped'
    }

    It 'returns an empty Results array when there are no UnitTestResult nodes' {
        $trx = Join-Path $script:fixtureDir 'empty.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="5" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed">
    <Counters total="0" executed="0" passed="0" failed="0" />
  </ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r.Results.Count | Should -Be 0
        $r.Total | Should -Be 0
    }

    It 'gracefully handles malformed XML (returns null, does not throw)' {
        $trx = Join-Path $script:fixtureDir 'bad.trx'
        '<TestRun><not-closed' | Set-Content -Path $trx -Encoding UTF8

        $r = Get-TrxResults -TrxPath $trx
        $r | Should -BeNullOrEmpty
    }

    It 'returns the original TrxPath in the result for round-tripping' {
        $trx = Join-Path $script:fixtureDir 'pathtrack.trx'
        @'
<?xml version="1.0" encoding="utf-8"?>
<TestRun id="6" xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
  <ResultSummary outcome="Completed"><Counters total="0" executed="0" passed="0" failed="0" /></ResultSummary>
  <Results />
</TestRun>
'@ | Set-Content -Path $trx -Encoding UTF8

        (Get-TrxResults -TrxPath $trx).TrxPath | Should -Be $trx
    }
}

Describe 'Get-DotNetTestResults (console-scrape fallback)' {
    It 'parses a single Passed entry' {
        $lines = @(
            '  Passed Foo.Bar [12 ms]'
        )
        $r = Get-DotNetTestResults -Lines $lines
        $r.Count | Should -Be 1
        $r[0].status | Should -Be 'Passed'
        $r[0].name   | Should -Be 'Foo.Bar'
    }

    It 'parses multiple consecutive results' {
        $lines = @(
            '  Passed One [1 ms]',
            '  Passed Two [2 ms]',
            '  Failed Three [3 ms]'
        )
        $r = Get-DotNetTestResults -Lines $lines
        $r.Count | Should -Be 3
        ($r | Where-Object { $_.status -eq 'Failed' }).name | Should -Be 'Three'
    }

    It 'captures error message and stack between two results' {
        $lines = @(
            '  Passed Alpha [10 ms]',
            '  Failed Beta [20 ms]',
            '   Error Message:',
            '   Expected: 1; Actual: 2',
            '   Stack Trace:',
            '      at Beta() in B.cs:line 99',
            '  Passed Gamma [5 ms]'
        )
        $r = Get-DotNetTestResults -Lines $lines
        $beta = $r | Where-Object { $_.name -eq 'Beta' }
        $beta.error | Should -Match 'Expected: 1; Actual: 2'
        $beta.stack | Should -Match 'at Beta\(\) in B\.cs:line 99'
    }

    It 'returns an empty array for empty input' {
        (Get-DotNetTestResults -Lines @()).Count | Should -Be 0
    }
}
