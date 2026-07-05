#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for pure-function helpers in Review-PR.ps1.
    Currently covers:
      - Get-TrxResults  (parses VSTest TRX produced by `dotnet test --logger trx`)
      - Get-DotNetTestResults  (legacy console-output scraper, still used as fallback
                                when TRX is missing)
      - Copilot token usage helpers

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
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Test-IsNumericValue')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'ConvertTo-AzdoSafeConsole')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-ObjectMemberValue')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotUsageTokenFields')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-TokenFieldSum')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-TokenFieldPathDepth')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Select-CanonicalTokenFields')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotTokenMetrics')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Convert-CopilotCompactNumber')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotCliUsageLineData')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotOtelTokenMetrics')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'New-CopilotTokenUsageRecord')
}

Describe 'Copilot token usage helpers' {
    It 'normalizes known token fields while preserving raw token field paths' {
        $usage = [pscustomobject]@{
            inputTokens        = 100
            outputTokens       = 40
            totalApiDurationMs = 1234
            nested             = [pscustomobject]@{
                cachedInputTokens = 12
            }
        }

        $metrics = Get-CopilotTokenMetrics -Usage $usage

        $metrics.inputTokens | Should -Be 100
        $metrics.outputTokens | Should -Be 40
        $metrics.cachedInputTokens | Should -Be 12
        $metrics.totalTokens | Should -Be 140
        @($metrics.rawTokenFields).Count | Should -Be 3
        @($metrics.rawTokenFields | Where-Object { $_.Path -eq 'nested.cachedInputTokens' }).Count | Should -Be 1
    }

    It 'prefers the root token aggregate over a nested per-model breakdown (no double-count)' {
        # Regression guard: a payload carrying BOTH a root aggregate and a per-model
        # breakdown must not sum both (1000 + 600 + 400 = 2000); the root wins.
        $usage = [pscustomobject]@{
            inputTokens  = 1000
            outputTokens = 200
            perModel     = @(
                [pscustomobject]@{ inputTokens = 600; outputTokens = 120 },
                [pscustomobject]@{ inputTokens = 400; outputTokens = 80 }
            )
        }

        $metrics = Get-CopilotTokenMetrics -Usage $usage

        $metrics.inputTokens | Should -Be 1000
        $metrics.outputTokens | Should -Be 200
    }

    It 'sums a nested-only token breakdown when no root aggregate exists' {
        # When only the per-model breakdown is present, it should be summed.
        $usage = [pscustomobject]@{
            perModel = @(
                [pscustomobject]@{ inputTokens = 600 },
                [pscustomobject]@{ inputTokens = 400 }
            )
        }

        $metrics = Get-CopilotTokenMetrics -Usage $usage

        $metrics.inputTokens | Should -Be 1000
    }

    It 'parses Copilot CLI AIC and context footer lines' {
        $aicLine = Get-CopilotCliUsageLineData -Line 'Session: 1030 AIC used'
        $contextLine = Get-CopilotCliUsageLineData -Line 'GPT-5.5 • 1.1M context'

        $aicLine.aicUsed | Should -Be 1030
        $contextLine.model | Should -Be 'GPT-5.5'
        $contextLine.contextWindowRaw | Should -Be '1.1M'
        $contextLine.contextWindow | Should -Be 1100000
    }

    It 'reads token counts from Copilot OTel spans with both cache/reasoning naming variants' {
        $otelPath = Join-Path ([System.IO.Path]::GetTempPath()) "copilot-otel-$([guid]::NewGuid()).jsonl"
        try {
            @(
                [ordered]@{
                    type       = 'span'
                    attributes = [ordered]@{
                        'gen_ai.usage.input_tokens'            = 1000
                        'gen_ai.usage.output_tokens'           = 200
                        'gen_ai.usage.cache_read.input_tokens' = 800
                        'gen_ai.usage.reasoning.output_tokens' = 50
                        'github.copilot.cost'                  = 7.5
                    }
                },
                [ordered]@{
                    type       = 'span'
                    attributes = [ordered]@{
                        'gen_ai.usage.input_tokens'            = 500
                        'gen_ai.usage.output_tokens'           = 40
                        'gen_ai.usage.cache_read_input_tokens' = 400
                        'gen_ai.usage.reasoning_output_tokens' = 10
                    }
                }
            ) | ForEach-Object { $_ | ConvertTo-Json -Depth 10 -Compress } | Set-Content $otelPath -Encoding UTF8

            $metrics = Get-CopilotOtelTokenMetrics -Path $otelPath

            $metrics.available | Should -Be $true
            $metrics.inputTokens | Should -Be 1500
            $metrics.outputTokens | Should -Be 240
            $metrics.cachedInputTokens | Should -Be 1200
            $metrics.reasoningOutputTokens | Should -Be 60
            $metrics.totalTokens | Should -Be 1740
            $metrics.copilotCost | Should -Be 7.5
        } finally {
            Remove-Item $otelPath -Force -ErrorAction SilentlyContinue
        }
    }

    It 'builds a telemetry record with raw usage and no hardcoded cost estimate' {
        $usage = [pscustomobject]@{
            prompt_tokens      = 25
            completion_tokens  = 15
            total_tokens       = 45
            totalApiDurationMs = 2000
        }

        $record = New-CopilotTokenUsageRecord `
            -PRNumber 35677 `
            -Platform 'android' `
            -Phase 'CopilotReview' `
            -StepName 'STEP 5a: TRY-FIX' `
            -ModelName 'gpt-5.5' `
            -StartedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:00Z')) `
            -EndedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:05Z')) `
            -DurationMs 5000 `
            -TurnCount 2 `
            -ToolCount 3 `
            -FailedToolCount 1 `
            -Usage $usage `
            -OtelMetrics $null `
            -AicUsed 1030 `
            -ContextWindow 1100000 `
            -ContextWindowRaw '1.1M' `
            -ResultEventSeen $true `
            -ExitCode 0

        $record.prNumber | Should -Be 35677
        $record.scriptPhase | Should -Be 'CopilotReview'
        $record.copilotStep | Should -Be 'STEP 5a: TRY-FIX'
        $record.apiDurationMs | Should -Be 2000
        $record.normalizedTokens.inputTokens | Should -Be 25
        $record.normalizedTokens.outputTokens | Should -Be 15
        $record.normalizedTokens.totalTokens | Should -Be 45
        $record.cliUsage.aicUsed | Should -Be 1030
        $record.cliUsage.contextWindow | Should -Be 1100000
        $record.cliUsage.contextWindowRaw | Should -Be '1.1M'
        $record.usage.total_tokens | Should -Be 45
        $record.costEstimateAvailable | Should -Be $false
    }

    It 'uses OTel token metrics when result usage has no token fields' {
        $otelMetrics = [ordered]@{
            inputTokens           = 500
            outputTokens          = 75
            cachedInputTokens     = 400
            reasoningOutputTokens = 25
            totalTokens           = 575
            copilotCost           = 7.5
            file                  = '/tmp/copilot-otel.jsonl'
        }

        $record = New-CopilotTokenUsageRecord `
            -PRNumber 35677 `
            -Platform 'android' `
            -Phase 'CopilotReview' `
            -StepName 'STEP 5a: TRY-FIX' `
            -ModelName 'gpt-5.5' `
            -StartedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:00Z')) `
            -EndedAtUtc ([DateTimeOffset]::Parse('2026-06-05T10:00:05Z')) `
            -DurationMs 5000 `
            -TurnCount 2 `
            -ToolCount 3 `
            -FailedToolCount 0 `
            -Usage ([pscustomobject]@{ totalApiDurationMs = 1000 }) `
            -OtelMetrics $otelMetrics `
            -AicUsed $null `
            -ContextWindow $null `
            -ContextWindowRaw $null `
            -ResultEventSeen $true `
            -ExitCode 0

        $record.normalizedTokens.inputTokens | Should -Be 500
        $record.normalizedTokens.outputTokens | Should -Be 75
        $record.normalizedTokens.cachedInputTokens | Should -Be 400
        $record.normalizedTokens.reasoningOutputTokens | Should -Be 25
        $record.normalizedTokens.totalTokens | Should -Be 575
        $record.normalizedTokens.otelFile | Should -Be '/tmp/copilot-otel.jsonl'
        # aicUsed stays AIC-only (null here); the dollar cost is reported in its own field,
        # never conflated into aicUsed.
        $record.cliUsage.aicUsed | Should -BeNullOrEmpty
        $record.cliUsage.copilotCost | Should -Be 7.5
    }
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

Describe 'ConvertTo-AzdoSafeConsole' {
    It 'defangs ##vso[ and ##[ logging-command prefixes' {
        ConvertTo-AzdoSafeConsole '##vso[task.setvariable variable=x]y' | Should -Be '## vso[task.setvariable variable=x]y'
        ConvertTo-AzdoSafeConsole '##[command]z' | Should -Be '## [command]z'
    }

    It 'collapses CR/LF that could fabricate a fresh column-0 log line' {
        ConvertTo-AzdoSafeConsole "safe`r##vso[task.complete]" | Should -Be 'safe ## vso[task.complete]'
        ConvertTo-AzdoSafeConsole "Reviewing`n##vso[task.complete result=Succeeded;]done" | Should -Be 'Reviewing ## vso[task.complete result=Succeeded;]done'
    }

    It 'leaves ordinary text untouched' {
        ConvertTo-AzdoSafeConsole 'Reading file src/Foo.cs (## of total)' | Should -Be 'Reading file src/Foo.cs (## of total)'
    }
}
