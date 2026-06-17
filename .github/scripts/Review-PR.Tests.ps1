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
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Restore-GateResultOrFailClosed')
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

Describe 'Restore-GateResultOrFailClosed' {
    BeforeEach {
        $script:stateDir = Join-Path ([System.IO.Path]::GetTempPath()) "gate-restore-$(New-Guid)"
        New-Item -ItemType Directory -Force -Path $script:stateDir | Out-Null
    }
    AfterEach {
        Remove-Item -Recurse -Force $script:stateDir -ErrorAction SilentlyContinue
    }

    It 'returns the gate verdict when the file exists with a valid value' {
        'PASSED' | Set-Content (Join-Path $script:stateDir 'gate-result.txt') -Encoding UTF8
        Restore-GateResultOrFailClosed -StateDir $script:stateDir | Should -Be 'PASSED'
    }

    It 'accepts each whitelisted value' {
        foreach ($v in @('PASSED','FAILED','SKIPPED','SKIP_NO_TESTS')) {
            $v | Set-Content (Join-Path $script:stateDir 'gate-result.txt') -Encoding UTF8
            Restore-GateResultOrFailClosed -StateDir $script:stateDir | Should -Be $v
        }
    }

    It 'fails closed (throws) when the file is missing' {
        { Restore-GateResultOrFailClosed -StateDir $script:stateDir } |
            Should -Throw -ExpectedMessage '*failing closed*'
    }

    It 'fails closed on an unrecognised verdict value (attacker tries arbitrary string)' {
        'GREEN' | Set-Content (Join-Path $script:stateDir 'gate-result.txt') -Encoding UTF8
        { Restore-GateResultOrFailClosed -StateDir $script:stateDir } |
            Should -Throw -ExpectedMessage "*Invalid Gate result 'GREEN'*"
    }

    It 'fails closed on an empty file' {
        '' | Set-Content (Join-Path $script:stateDir 'gate-result.txt') -Encoding UTF8 -NoNewline
        { Restore-GateResultOrFailClosed -StateDir $script:stateDir } |
            Should -Throw -ExpectedMessage '*Invalid Gate result*'
    }

    It 'trims surrounding whitespace before validating (real Set-Content adds trailing newline)' {
        "PASSED`n" | Set-Content (Join-Path $script:stateDir 'gate-result.txt') -Encoding UTF8 -NoNewline
        Restore-GateResultOrFailClosed -StateDir $script:stateDir | Should -Be 'PASSED'
    }
}

Describe 'Restore-GateResultOrFailClosed — F1.A HMAC verification' {
    BeforeEach {
        $script:stateDir = Join-Path ([System.IO.Path]::GetTempPath()) "gate-hmac-$(New-Guid)"
        New-Item -ItemType Directory -Force -Path $script:stateDir | Out-Null
        $script:resultFile = Join-Path $script:stateDir 'gate-result.txt'
        $script:hmacFile = "$script:resultFile.hmac"
        # 32-byte / 64-hex-char test key (matches `openssl rand -hex 32`).
        $script:testKey = '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef'
    }
    AfterEach {
        Remove-Item -Recurse -Force $script:stateDir -ErrorAction SilentlyContinue
        Remove-Item Env:GATE_HMAC_KEY -ErrorAction SilentlyContinue
    }

    function script:Compute-TestHmac([string]$Path, [string]$KeyHex) {
        $h = [System.Security.Cryptography.HMACSHA256]::HashData(
            [System.Convert]::FromHexString($KeyHex),
            [System.IO.File]::ReadAllBytes($Path))
        [System.Convert]::ToHexString($h).ToLowerInvariant()
    }

    It 'returns verdict when HMAC is correct (param form)' {
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        (Compute-TestHmac -Path $script:resultFile -KeyHex $script:testKey) | Set-Content $script:hmacFile -Encoding UTF8
        Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey |
            Should -Be 'PASSED'
    }

    It 'returns verdict when HMAC is correct (env form)' {
        'FAILED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        (Compute-TestHmac -Path $script:resultFile -KeyHex $script:testKey) | Set-Content $script:hmacFile -Encoding UTF8
        $env:GATE_HMAC_KEY = $script:testKey
        Restore-GateResultOrFailClosed -StateDir $script:stateDir | Should -Be 'FAILED'
    }

    It 'skips HMAC verification when key is not supplied (local dev)' {
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        # No .hmac file, no env var — should still succeed
        Restore-GateResultOrFailClosed -StateDir $script:stateDir | Should -Be 'PASSED'
    }

    It 'fails closed when HMAC key is supplied but .hmac file is missing' {
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        { Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey } |
            Should -Throw -ExpectedMessage '*HMAC key supplied but*missing*'
    }

    It 'fails closed when the verdict file is tampered after HMAC was sealed (forgery attack)' {
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        (Compute-TestHmac -Path $script:resultFile -KeyHex $script:testKey) | Set-Content $script:hmacFile -Encoding UTF8
        # Simulate attacker overwriting verdict but unable to recompute HMAC
        # without the key — old HMAC no longer matches the new file content.
        'FAILED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        { Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey } |
            Should -Throw -ExpectedMessage '*HMAC verification failed*'
    }

    It 'fails closed when the .hmac file is forged with wrong key' {
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        $wrongKey = 'ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff'
        (Compute-TestHmac -Path $script:resultFile -KeyHex $wrongKey) | Set-Content $script:hmacFile -Encoding UTF8
        { Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey } |
            Should -Throw -ExpectedMessage '*HMAC verification failed*'
    }

    It 'accepts openssl-format HMAC output (lowercase hex, no newline normalisation)' {
        # `openssl dgst -sha256 -hmac KEY file | awk '{print $NF}'` produces
        # exactly 64 lowercase hex chars. Verify our verifier accepts that
        # without leading whitespace, prefix, or trailing newline issues.
        'SKIPPED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        $h = Compute-TestHmac -Path $script:resultFile -KeyHex $script:testKey
        # Set-Content -Encoding UTF8 normally appends newline → simulate that
        # path too (the bash `echo ... > file` does the same).
        Set-Content -Path $script:hmacFile -Value $h -Encoding UTF8
        Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey |
            Should -Be 'SKIPPED'
    }

    Context 'CI_REQUIRE_GATE_HMAC contract — Attack 2 (suppress-signing downgrade)' {

        AfterEach {
            Remove-Item Env:CI_REQUIRE_GATE_HMAC -ErrorAction SilentlyContinue
            Remove-Item Env:GATE_HMAC_KEY -ErrorAction SilentlyContinue
        }

        It 'fails closed when CI_REQUIRE_GATE_HMAC=1 and GATE_HMAC_KEY is empty (suppress-signing downgrade)' {
            'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
            # Simulate the attack: daemon deleted gate-result.txt during the
            # bash gating window so the `if [ -f ... ]` skipped signing and
            # GATE_HMAC_KEY was never set. CI signals that signing is REQUIRED
            # via CI_REQUIRE_GATE_HMAC=1 — the function must refuse to
            # downgrade to no-verification.
            $env:CI_REQUIRE_GATE_HMAC = '1'
            $env:GATE_HMAC_KEY = ''
            { Restore-GateResultOrFailClosed -StateDir $script:stateDir } |
                Should -Throw -ExpectedMessage '*Gate HMAC key missing in CI*sealing was suppressed*'
        }

        It 'fails closed when CI_REQUIRE_GATE_HMAC=1 and GATE_HMAC_KEY is whitespace' {
            'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
            $env:CI_REQUIRE_GATE_HMAC = '1'
            $env:GATE_HMAC_KEY = "   `t  "
            { Restore-GateResultOrFailClosed -StateDir $script:stateDir } |
                Should -Throw -ExpectedMessage '*Gate HMAC key missing in CI*'
        }

        It 'fails closed when CI_REQUIRE_GATE_HMAC=true (string form) and key is empty' {
            'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
            $env:CI_REQUIRE_GATE_HMAC = 'true'
            $env:GATE_HMAC_KEY = ''
            { Restore-GateResultOrFailClosed -StateDir $script:stateDir } |
                Should -Throw -ExpectedMessage '*Gate HMAC key missing in CI*'
        }

        It 'still allows local dev (CI_REQUIRE_GATE_HMAC unset) with no key' {
            'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
            # No env vars set (AfterEach cleans them up)
            Restore-GateResultOrFailClosed -StateDir $script:stateDir | Should -Be 'PASSED'
        }

        It 'passes verification in CI when key + .hmac are valid' {
            'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
            (Compute-TestHmac -Path $script:resultFile -KeyHex $script:testKey) | Set-Content $script:hmacFile -Encoding UTF8
            $env:CI_REQUIRE_GATE_HMAC = '1'
            $env:GATE_HMAC_KEY = $script:testKey
            Restore-GateResultOrFailClosed -StateDir $script:stateDir | Should -Be 'PASSED'
        }
    }
}

Describe 'Restore-GateResultOrFailClosed — python3 ↔ .NET HMAC cross-check' {
    BeforeAll {
        # Locate a WORKING python interpreter the Gate task uses to seal
        # the verdict. CI agents (ubuntu-latest, where this Pester
        # workflow runs) always ship python3; Windows dev boxes expose it
        # as `python` (and may have a non-functional `python3` Microsoft
        # Store app-execution stub that must be rejected). This test
        # deliberately does NOT skip when no interpreter is found — the
        # security-scripts-pester gate rejects ANY skipped/inconclusive
        # test, so a silent skip here would be a contradiction. If no
        # working interpreter is found the test fails loudly.
        $script:python = $null
        $candidates = @(
            (Get-Command python3 -ErrorAction SilentlyContinue)?.Source,
            (Get-Command python  -ErrorAction SilentlyContinue)?.Source,
            '/usr/bin/python3', '/usr/local/bin/python3', '/opt/homebrew/bin/python3'
        ) | Where-Object { $_ }
        foreach ($c in $candidates) {
            try {
                $v = & $c --version 2>&1
                if ($LASTEXITCODE -eq 0 -and "$v" -match 'Python 3') { $script:python = $c; break }
            } catch { }
        }
    }

    BeforeEach {
        $script:stateDir = Join-Path ([System.IO.Path]::GetTempPath()) "gate-hmac-x-$(New-Guid)"
        New-Item -ItemType Directory -Force -Path $script:stateDir | Out-Null
        $script:resultFile = Join-Path $script:stateDir 'gate-result.txt'
        $script:hmacFile = "$script:resultFile.hmac"
        $script:testKey = '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef'
    }
    AfterEach {
        Remove-Item -Recurse -Force $script:stateDir -ErrorAction SilentlyContinue
    }

    It 'a verdict file sealed via python3 stdlib hmac verifies under the .NET verifier' {
        $script:python | Should -Not -BeNullOrEmpty `
            -Because 'python3 is required by the Gate HMAC seal and is present on all CI agents; a missing interpreter must fail (not skip), since the security-scripts-pester gate forbids skipped tests'

        # Reproduce the Gate task byte-for-byte: write verdict, compute
        # HMAC-SHA256 via python stdlib with the key fed on stdin (NOT
        # argv — matches the /proc/cmdline-safe production path), write
        # the .hmac sibling, then verify with the .NET-side verifier.
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        $pyScript = @'
import sys, hmac, hashlib
key = bytes.fromhex(sys.stdin.readline().strip())
with open(sys.argv[1], 'rb') as f:
    print(hmac.new(key, f.read(), hashlib.sha256).hexdigest())
'@
        $pyFile = Join-Path $script:stateDir 'sign.py'
        Set-Content -Path $pyFile -Value $pyScript -Encoding UTF8
        $hmac = ($script:testKey | & $script:python $pyFile $script:resultFile).Trim()
        $hmac | Should -Match '^[0-9a-f]{64}$' -Because 'python stdlib hmac should produce 64 lowercase hex chars'
        $hmac | Set-Content $script:hmacFile -Encoding UTF8

        Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey |
            Should -Be 'PASSED'
    }
}

Describe 'Review-PR.ps1 CopilotReview phase — undefined variable scan under StrictMode' {
    # End-to-end safety: even if a future change brings back Set-StrictMode
    # (or Pester invokes the script with strict mode), the CopilotReview
    # phase restore must not throw "variable cannot be retrieved because
    # it has not been set". This test simulates the exact devdiv 14313972
    # scenario: phase-state contains gate-result.txt but NO regression-*.json
    # files (the common case when Gate runs but finds no regression risks).

    BeforeAll {
        $script:reviewPRPath = Join-Path $PSScriptRoot 'Review-PR.ps1'
    }

    It 'compiles cleanly under Set-StrictMode Latest' {
        # The script itself must parse without strict-mode-violations at
        # parse time. This catches typos / clearly-undefined references.
        $tokens = $null; $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:reviewPRPath, [ref]$tokens, [ref]$errors)
        if ($errors -and $errors.Count -gt 0) {
            throw "Parse errors: $($errors[0].Message)"
        }
        $ast | Should -Not -BeNullOrEmpty
    }

    It 'CopilotReview phased restore initializes $risksData, $regressionTests, $regrPlatform, $uitestCategories, $detectScript before any conditional restore' {
        # Static AST check: in the nested `if ($Phase -eq 'CopilotReview')`
        # block (inside `if ($runCopilotReview)`), all variables that the
        # restore reads / writes conditionally must have unconditional
        # initialization BEFORE the conditional file-presence check.
        # Regression guard: the previous "init at top of $runCopilotReview"
        # form clobbered Gate's in-process data in non-phased / -DryRun
        # runs. Init lives in the phased branch only — in non-phased mode Gate has already
        # populated these in-process.
        $tokens = $null; $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:reviewPRPath, [ref]$tokens, [ref]$errors)

        # Find the `if ($runCopilotReview)` block
        $copilotReviewIf = $ast.FindAll(
            { param($n)
              $n -is [System.Management.Automation.Language.IfStatementAst] -and
              $n.Clauses.Count -ge 1 -and
              $n.Clauses[0].Item1.Extent.Text -match '\$runCopilotReview\b'
            },
            $true) | Sort-Object { $_.Extent.Text.Length } -Descending | Select-Object -First 1

        $copilotReviewIf | Should -Not -BeNullOrEmpty -Because 'must find the if ($runCopilotReview) block'

        # Inside it, find the nested `if ($Phase -eq 'CopilotReview')` block
        $body = $copilotReviewIf.Clauses[0].Item2
        $phasedIf = $body.FindAll(
            { param($n)
              $n -is [System.Management.Automation.Language.IfStatementAst] -and
              $n.Clauses.Count -ge 1 -and
              $n.Clauses[0].Item1.Extent.Text -match "\`$Phase\s*-eq\s*'CopilotReview'"
            },
            $false) | Select-Object -First 1

        $phasedIf | Should -Not -BeNullOrEmpty -Because 'must find nested if ($Phase -eq ''CopilotReview'') block'

        $phasedBody = $phasedIf.Clauses[0].Item2
        # Walk statements up to (but not including) the first nested IfStatement
        # (the file-presence restore branch) — assignments before that are
        # unconditional within the phased branch.
        $unconditionalAssigns = @()
        foreach ($stmt in $phasedBody.Statements) {
            if ($stmt -is [System.Management.Automation.Language.IfStatementAst]) { break }
            $assigns = $stmt.FindAll(
                { param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                            $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] },
                $true)
            foreach ($a in $assigns) {
                $unconditionalAssigns += $a.Left.VariablePath.UserPath
            }
        }

        $required = @('risksData', 'regressionTests', 'regrPlatform', 'uitestCategories', 'detectScript')
        foreach ($v in $required) {
            $unconditionalAssigns | Should -Contain $v `
                -Because "CopilotReview phased restore must initialize `$$v before the file-presence check so the script is safe under Set-StrictMode (devdiv 14313972 regression guard)"
        }
    }

    It 'Gate phase initializes $risksData and $regressionTests before any conditional assignment' {
        # Defense-in-depth twin of the CopilotReview phased-restore test:
        # the Gate phase has its own `if (Test-Path $regressionRisksJson)`
        # block that conditionally sets $risksData, then reads it later via
        # `if ($risksData -and ...)`. Without an unconditional pre-init at
        # the top of `if ($runGate)`, a future Set-StrictMode change (or a
        # leak from a dot-sourced helper) would make the read throw
        # VariablePathNotFound when risks.json is absent.
        $tokens = $null; $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:reviewPRPath, [ref]$tokens, [ref]$errors)

        $gateIf = $ast.FindAll(
            { param($n)
              $n -is [System.Management.Automation.Language.IfStatementAst] -and
              $n.Clauses.Count -ge 1 -and
              $n.Clauses[0].Item1.Extent.Text -match '\$runGate\b'
            },
            $true) | Sort-Object { $_.Extent.Text.Length } -Descending | Select-Object -First 1

        $gateIf | Should -Not -BeNullOrEmpty -Because 'must find the if ($runGate) block'

        $gateBody = $gateIf.Clauses[0].Item2
        # Collect assignments BEFORE the first nested IfStatement — these
        # are the unconditional top-of-block initializations.
        $preIfAssigns = @()
        foreach ($stmt in $gateBody.Statements) {
            if ($stmt -is [System.Management.Automation.Language.IfStatementAst]) { break }
            $assigns = $stmt.FindAll(
                { param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                            $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] },
                $true)
            foreach ($a in $assigns) {
                $preIfAssigns += $a.Left.VariablePath.UserPath
            }
        }

        $required = @('risksData', 'regressionTests')
        foreach ($v in $required) {
            $preIfAssigns | Should -Contain $v `
                -Because "Gate must initialize `$$v unconditionally at the top of if (`$runGate) so the script is safe under Set-StrictMode when Find-RegressionRisks.ps1 fails / risks.json is absent (devdiv 14313972 bug class)"
        }
    }

    It 'CopilotReview restore does NOT clobber Gate in-process data in non-phased runs' {
        # Static AST check: the init of $risksData, $regressionTests, etc.
        # must live INSIDE the `if ($Phase -eq 'CopilotReview')` branch, NOT
        # in the outer `if ($runCopilotReview)` body. In non-phased / -DryRun
        # runs both $runGate and $runCopilotReview are true; Gate populates
        # these in-process and the CopilotReview block must not reset them.
        $tokens = $null; $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:reviewPRPath, [ref]$tokens, [ref]$errors)

        $copilotReviewIf = $ast.FindAll(
            { param($n)
              $n -is [System.Management.Automation.Language.IfStatementAst] -and
              $n.Clauses.Count -ge 1 -and
              $n.Clauses[0].Item1.Extent.Text -match '\$runCopilotReview\b'
            },
            $true) | Sort-Object { $_.Extent.Text.Length } -Descending | Select-Object -First 1

        $body = $copilotReviewIf.Clauses[0].Item2
        # Scan the ENTIRE outer body — both before AND after the nested
        # `if ($Phase -eq 'CopilotReview')` branch — for assignments to
        # the 5 phase-restored vars. Excludes the phased-if's own subtree
        # (where the assignments ARE allowed).
        $forbidden = @('risksData', 'regressionTests', 'regrPlatform', 'uitestCategories', 'detectScript')
        # Locate the nested `if ($Phase -eq 'CopilotReview')` to exclude its
        # subtree. There may legitimately be other IfStatements in the
        # outer body — we only exclude the phased one.
        $phasedIf = $body.FindAll(
            { param($n)
              $n -is [System.Management.Automation.Language.IfStatementAst] -and
              $n.Clauses.Count -ge 1 -and
              $n.Clauses[0].Item1.Extent.Text -match "\`$Phase\s*-eq\s*'CopilotReview'"
            },
            $false) | Select-Object -First 1
        $phasedIfStartOffset = if ($phasedIf) { $phasedIf.Extent.StartOffset } else { -1 }
        $phasedIfEndOffset   = if ($phasedIf) { $phasedIf.Extent.EndOffset }   else { -1 }

        # Walk the whole outer body. For each AssignmentStatement to a
        # forbidden name, check whether its source location is inside the
        # phased-if's extent — if so, allowed; otherwise, leaked.
        $leakedAssigns = @()
        $allAssigns = $body.FindAll(
            { param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                        $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
                        ($forbidden -contains $n.Left.VariablePath.UserPath) },
            $true)
        foreach ($a in $allAssigns) {
            $insidePhased = ($phasedIfStartOffset -ge 0) -and
                            ($a.Extent.StartOffset -ge $phasedIfStartOffset) -and
                            ($a.Extent.EndOffset   -le $phasedIfEndOffset)
            if (-not $insidePhased) {
                $leakedAssigns += "$($a.Left.VariablePath.UserPath) at offset $($a.Extent.StartOffset)"
            }
        }
        $leakedAssigns | Should -BeNullOrEmpty `
            -Because 'these 5 vars are Gate''s in-process outputs; any assignment in if ($runCopilotReview) outer scope (NOT inside the nested `$Phase -eq ''CopilotReview''` branch) clobbers non-phased / -DryRun runs (devdiv 14313972 class)'
    }

    It 'no script DOT-SOURCED into Review-PR.ps1 sets script-scope StrictMode / preference vars at top level' {
        # Scan only the helpers Review-PR.ps1 actually dot-sources (via `. $var`
        # at script scope). These leak state into Review-PR.ps1; other shared
        # scripts that are only ever launched as `pwsh -File` subprocesses do
        # not.
        $tokens = $null; $errors = $null
        $reviewAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $script:reviewPRPath, [ref]$tokens, [ref]$errors)

        # Find all dot-source CommandAst at script scope:
        #   `. $varHoldingPath`  or  `. "string-path"`
        # Then resolve each path. The right-hand expression is typically a
        # variable like $sanitizerScript that was assigned `Join-Path ...`
        # to a file under $ScriptsDir/shared.
        $dotSources = $reviewAst.FindAll(
            { param($n)
              $n -is [System.Management.Automation.Language.CommandAst] -and
              $n.InvocationOperator -eq [System.Management.Automation.Language.TokenKind]::Dot
            },
            $true)

        $sharedDir = Join-Path (Split-Path $script:reviewPRPath -Parent) 'shared'
        $sourced = New-Object System.Collections.Generic.HashSet[string]
        foreach ($ds in $dotSources) {
            $arg = $ds.CommandElements | Select-Object -First 1
            if (-not $arg) { continue }
            # Resolve `$xxxScript` to its assigned value by string-grep on the script text
            if ($arg -is [System.Management.Automation.Language.VariableExpressionAst]) {
                $varName = $arg.VariablePath.UserPath
                # Find an assignment $varName = Join-Path ... "shared/XYZ.ps1"
                $assigns = $reviewAst.FindAll(
                    { param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                                $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
                                $n.Left.VariablePath.UserPath -eq $varName },
                    $true)
                foreach ($a in $assigns) {
                    $rhsText = $a.Right.Extent.Text
                    if ($rhsText -match '"shared[/\\]([^"]+\.ps1)"') {
                        $leaf = $Matches[1]
                        $fp = Join-Path $sharedDir $leaf
                        if (Test-Path $fp) { $null = $sourced.Add($fp) }
                    }
                }
            }
        }

        $sourced.Count | Should -BeGreaterThan 0 -Because 'must find at least one dot-sourced helper'

        $leakingPattern = 'Set-StrictMode|Set-PSDebug|\$(?:Error|Warning|Verbose|Debug|Information|Progress|Confirm|WhatIf)ActionPreference\s*=|\[Environment\]::SetEnvironmentVariable|^\s*New-Alias|^\s*Set-Alias'

        $violations = @()
        foreach ($f in $sourced) {
            $ftokens = $null; $ferrs = $null
            $fileAst = [System.Management.Automation.Language.Parser]::ParseFile(
                $f, [ref]$ftokens, [ref]$ferrs)
            if (-not $fileAst) { continue }
            $scriptBlock = $fileAst.EndBlock
            if (-not $scriptBlock) { continue }
            foreach ($stmt in $scriptBlock.Statements) {
                if ($stmt -is [System.Management.Automation.Language.FunctionDefinitionAst]) { continue }
                if ($stmt.Extent.Text -match $leakingPattern) {
                    $violations += "$(Split-Path $f -Leaf):$($stmt.Extent.StartLineNumber): $($stmt.Extent.Text.Substring(0, [Math]::Min(80, $stmt.Extent.Text.Length)).Trim())"
                }
            }
        }

        $violations | Should -BeNullOrEmpty `
            -Because 'no dot-sourced helper may modify parent-script state at top level (devdiv 14313972 root cause class). Move the setting into a function, or guard by `$MyInvocation.InvocationName -ne "."`'
    }
}

Describe 'winner.json reporting — StrictMode property-presence guard' {
    # AST assertion against the PRODUCTION source — not a copied guard.
    # A previous form ran tests against a duplicate inline $guardBlock;
    # if production code reverted to broken `if ($winnerJson)`, those
    # tests still passed because they exercised the COPY. This version
    # walks the AST of the actual Review-PR.ps1, so a regression flips
    # the test red.

    BeforeAll {
        $reviewPRPath = Join-Path $PSScriptRoot 'Review-PR.ps1'
        $tokens = $null; $errors = $null
        $script:reviewAst = [System.Management.Automation.Language.Parser]::ParseFile(
            $reviewPRPath, [ref]$tokens, [ref]$errors)
    }

    It 'production code guards winner.json access with property-presence check (not just object truthiness)' {
        # Find the if-block that wraps the "🏆 winner.json:" Write-Host —
        # that's the diagnostic whose guard must be StrictMode-safe. We
        # search by BODY (not by condition referencing $winnerJson) so
        # the test handles both inline guards and helper-variable forms
        # like `if ($hasWinner) { ... 🏆 winner.json: ... }`.
        $allIfs = $script:reviewAst.FindAll(
            { param($n) $n -is [System.Management.Automation.Language.IfStatementAst] },
            $true)
        $diagnosticIf = $allIfs | Where-Object {
            $_.Clauses[0].Item2.Extent.Text -match 'winner\.json:\s*winner='
        } | Sort-Object { $_.Extent.Text.Length } | Select-Object -First 1

        $diagnosticIf | Should -Not -BeNullOrEmpty `
            -Because 'expected to find the if-block writing "🏆 winner.json: winner=" in Review-PR.ps1'

        # Acceptable: condition is the property-presence check inline, OR
        # condition references a helper variable defined in an enclosing
        # scope using the property-presence check.
        $conditionText = $diagnosticIf.Clauses[0].Item1.Extent.Text
        $hasInlineGuard = $conditionText -match 'PSObject\.Properties\.Name\s+-contains\s+''winner'''

        $hasHelperGuard = $false
        if (-not $hasInlineGuard) {
            $varMatch = [regex]::Match($conditionText, '\$(\w+)')
            if ($varMatch.Success) {
                $varName = $varMatch.Groups[1].Value
                $parent = $diagnosticIf.Parent
                while ($parent -and -not ($parent -is [System.Management.Automation.Language.ScriptBlockAst])) {
                    $parent = $parent.Parent
                }
                if ($parent) {
                    $assigns = $parent.FindAll(
                        { param($n) $n -is [System.Management.Automation.Language.AssignmentStatementAst] -and
                                    $n.Left -is [System.Management.Automation.Language.VariableExpressionAst] -and
                                    $n.Left.VariablePath.UserPath -eq $varName },
                        $true)
                    foreach ($a in $assigns) {
                        if ($a.Extent.StartOffset -lt $diagnosticIf.Extent.StartOffset -and
                            $a.Extent.Text -match 'PSObject\.Properties\.Name\s+-contains\s+''winner''') {
                            $hasHelperGuard = $true
                            break
                        }
                    }
                }
            }
        }

        ($hasInlineGuard -or $hasHelperGuard) | Should -BeTrue `
            -Because 'the winner.json diagnostic must guard property access with `PSObject.Properties.Name -contains ''winner''`. Bare `if ($winnerJson)` throws PropertyNotFoundException under StrictMode on partial-object input.'
    }

    It 'the property-presence guard pattern itself works under StrictMode (interaction sanity check)' {
        # Validates only that .NET / PowerShell semantics of
        # `.PSObject.Properties.Name -contains 'winner'` behave as expected
        # under StrictMode — not the production code itself. Catches a
        # future PS regression where the pattern stops short-circuiting.
        $partial = '{"score":0.8}' | ConvertFrom-Json
        $full    = '{"winner":"with-fix","isPRFix":true}' | ConvertFrom-Json
        $check = {
            param($j)
            Set-StrictMode -Version Latest
            $j -and ($j.PSObject.Properties.Name -contains 'winner')
        }
        (& $check $partial) | Should -BeFalse
        (& $check $full)    | Should -BeTrue
        (& $check $null)    | Should -BeFalse
    }
}
