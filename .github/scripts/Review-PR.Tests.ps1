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
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-ObjectMemberValue')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotUsageTokenFields')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-TokenFieldSum')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotTokenMetrics')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Convert-CopilotCompactNumber')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotCliUsageLineData')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-CopilotOtelTokenMetrics')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'New-CopilotTokenUsageRecord')
    Invoke-Expression (Get-FunctionBody -ScriptText $content -FunctionName 'Get-PhaseStateDir')
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
        $record.cliUsage.aicUsed | Should -Be 7.5
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

# ─── Phase-state security helpers (F1 hardening) ─────────────────────────
# These guard the cross-phase trust boundary: Gate writes verdict files,
# CopilotReview/Post restore them. Originally these defaulted to "SKIPPED"
# on missing file (fail open) and used the writable parent of
# $TrustedScriptsDir as the state dir.

Describe 'Get-PhaseStateDir' {
    BeforeAll {
        $script:tmpRoot = Join-Path ([System.IO.Path]::GetTempPath()) "phase-state-$(New-Guid)"
        New-Item -ItemType Directory -Force -Path $script:tmpRoot | Out-Null
    }
    AfterAll {
        Remove-Item -Recurse -Force $script:tmpRoot -ErrorAction SilentlyContinue
    }

    Context 'when -TrustedScriptsDir is provided' {
        It 'returns a dedicated phase-state subdirectory, not the writable parent' {
            $trusted = Join-Path $script:tmpRoot 'trusted-github'
            New-Item -ItemType Directory -Force -Path $trusted | Out-Null
            $d = Get-PhaseStateDir -TrustedScriptsDir $trusted -RepoRoot $script:tmpRoot -PRNumber '12345'
            $d  | Should -Be (Join-Path $script:tmpRoot 'phase-state')
            $d  | Should -Not -Be (Split-Path $trusted -Parent)
            Test-Path $d | Should -BeTrue
        }
        It 'returns the same path on repeated calls (idempotent)' {
            $trusted = Join-Path $script:tmpRoot 'trusted-github-2'
            New-Item -ItemType Directory -Force -Path $trusted | Out-Null
            $a = Get-PhaseStateDir -TrustedScriptsDir $trusted -RepoRoot $script:tmpRoot -PRNumber '1'
            $b = Get-PhaseStateDir -TrustedScriptsDir $trusted -RepoRoot $script:tmpRoot -PRNumber '1'
            $a | Should -Be $b
        }
    }

    Context 'when -TrustedScriptsDir is empty (local dev)' {
        It 'falls back to a per-PR path under CustomAgentLogsTmp/PRState' {
            $d = Get-PhaseStateDir -TrustedScriptsDir '' -RepoRoot $script:tmpRoot -PRNumber '42'
            $d | Should -Match 'CustomAgentLogsTmp[/\\]PRState[/\\]42[/\\]PRAgent[/\\]phase-state$'
            Test-Path $d | Should -BeTrue
        }
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
            Should -Throw -ExpectedMessage "*Invalid Gate result value 'GREEN'*"
    }

    It 'fails closed on an empty file' {
        '' | Set-Content (Join-Path $script:stateDir 'gate-result.txt') -Encoding UTF8 -NoNewline
        { Restore-GateResultOrFailClosed -StateDir $script:stateDir } |
            Should -Throw -ExpectedMessage '*Invalid Gate result value*'
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

    function script:Compute-TestHmac {
        param([string]$Path, [string]$KeyHex)
        $keyBytes = [byte[]]::new($KeyHex.Length / 2)
        for ($i = 0; $i -lt $keyBytes.Length; $i++) {
            $keyBytes[$i] = [Convert]::ToByte($KeyHex.Substring($i * 2, 2), 16)
        }
        $fileBytes = [System.IO.File]::ReadAllBytes($Path)
        $hmac = [System.Security.Cryptography.HMACSHA256]::new($keyBytes)
        try {
            $h = $hmac.ComputeHash($fileBytes)
            return ([System.BitConverter]::ToString($h) -replace '-','').ToLowerInvariant()
        } finally { $hmac.Dispose() }
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
            Should -Throw -ExpectedMessage '*HMAC key was supplied but*missing*'
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
}

Describe 'Restore-GateResultOrFailClosed — openssl ↔ .NET HMAC cross-check' {
    BeforeAll {
        # Locate openssl. Hosted Linux/Mac CI agents have it on PATH.
        # On Windows dev boxes we fall back to git-bash's openssl.
        $script:openssl = (Get-Command openssl -ErrorAction SilentlyContinue)?.Source
        if (-not $script:openssl) {
            $candidates = @(
                'C:\Program Files\Git\usr\bin\openssl.exe',
                'C:\Program Files\Git\mingw64\bin\openssl.exe',
                '/usr/bin/openssl',
                '/opt/homebrew/bin/openssl'
            )
            $script:openssl = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1
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

    It 'a verdict file sealed via openssl with hexkey verifies under the .NET verifier' {
        if (-not $script:openssl) {
            Set-ItResult -Skipped -Because 'openssl not available in this environment'
            return
        }
        # Reproduce the YAML pipeline byte-for-byte: write verdict, sign
        # with `openssl dgst -sha256 -mac HMAC -macopt hexkey:KEY`,
        # extract last token, write .hmac sibling.
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        $opensslOut = & $script:openssl dgst -sha256 -mac HMAC -macopt "hexkey:$script:testKey" $script:resultFile
        $hmac = ($opensslOut -split ' ')[-1].Trim()
        $hmac | Should -Match '^[0-9a-f]{64}$' -Because 'openssl should produce 64 lowercase hex chars'
        $hmac | Set-Content $script:hmacFile -Encoding UTF8

        Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey |
            Should -Be 'PASSED'
    }

    It 'a verdict file sealed via openssl WITHOUT hexkey (default -hmac form) is REJECTED — guards against accidental YAML downgrade' {
        if (-not $script:openssl) {
            Set-ItResult -Skipped -Because 'openssl not available in this environment'
            return
        }
        # If someone mistakenly drops `-mac HMAC -macopt hexkey:` and reverts
        # to the simpler `-hmac KEY` form, openssl treats the key as a UTF-8
        # string while our .NET verifier hex-decodes it — the two produce
        # different MACs. The verifier MUST reject this, otherwise a YAML
        # regression would silently disable F1.A protection.
        'PASSED' | Set-Content $script:resultFile -Encoding UTF8 -NoNewline
        $wrongFormOut = & $script:openssl dgst -sha256 -hmac $script:testKey $script:resultFile
        $wrongHmac = ($wrongFormOut -split ' ')[-1].Trim()
        $wrongHmac | Set-Content $script:hmacFile -Encoding UTF8

        { Restore-GateResultOrFailClosed -StateDir $script:stateDir -HmacKeyHex $script:testKey } |
            Should -Throw -ExpectedMessage '*HMAC verification failed*'
    }
}
