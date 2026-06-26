#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Invoke-RerunReviewTrigger.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @('ConvertTo-SafeLogValue', 'ConvertTo-TrimmedString', 'Get-MatchingCandidate', 'Normalize-PipelineRef', 'Get-PlatformFromLabels', 'Expand-RerunDecisionItems', 'Get-RerunActions')) {
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $functionName
        }, $true)

        if (-not $function) {
            throw "Function '$functionName' not found"
        }

        Invoke-Expression $function.Extent.Text
    }

    function New-TestCandidate {
        param(
            [int]$PRNumber = 123,
            [string]$HeadSha = 'abc123def',
            [string]$Platform = 'android',
            [string]$PipelineRef = 'main',
            [Int64]$RerunCommentId = 9001
        )

        [pscustomobject]@{
            prNumber       = $PRNumber
            headSha        = $HeadSha
            platform       = $Platform
            pipelineRef    = $PipelineRef
            rerunCommentId = $RerunCommentId
        }
    }

    function New-TestDecision {
        param(
            [string]$PRNumber = '123',
            [string]$Decision = 'trigger',
            [string]$ExpectedHeadSha = 'abc123def'
        )

        [pscustomobject]@{
            pr_number         = $PRNumber
            decision          = $Decision
            expected_head_sha = $ExpectedHeadSha
        }
    }
}

Describe 'ConvertTo-SafeLogValue' {
    It 'removes newlines and breaks workflow command tokens' {
        $safe = ConvertTo-SafeLogValue "ok`n::stop-commands::x"

        $safe | Should -Be 'ok : :stop-commands: :x'
        $safe | Should -Not -Match '[\r\n]'
        $safe | Should -Not -Match '::'
    }

    It 'caps long log values' {
        $safe = ConvertTo-SafeLogValue ('a' * 200) -MaxLength 20

        $safe.Length | Should -Be 20
        $safe | Should -Match '\.\.\.$'
    }

    It 'sanitizes embedded workflow commands in error-shaped strings' {
        $exceptionText = "Cannot convert value `"99`n::stop-commands::pwn`" to type System.Int32"

        $safe = ConvertTo-SafeLogValue $exceptionText

        $safe | Should -Not -Match '[\r\n]'
        $safe | Should -Not -Match '::'
        $safe | Should -Match 'stop-commands'
    }
}

Describe 'Expand-RerunDecisionItems' {
    It 'expands a single item carrying a JSON-string decisions array' {
        $json = '[{"pr_number":"1","decision":"trigger"},{"pr_number":"2","decision":"skip"}]'
        $item = [pscustomobject]@{ type = 'trigger_rerun_review'; decisions = $json }
        $result = Expand-RerunDecisionItems -Items @($item)
        $result.Count | Should -Be 2
        $result[0].pr_number | Should -Be '1'
        $result[0].decision | Should -Be 'trigger'
        $result[1].pr_number | Should -Be '2'
        $result[1].decision | Should -Be 'skip'
    }

    It 'expands a decisions array that is already an object array' {
        $item = [pscustomobject]@{
            type      = 'trigger_rerun_review'
            decisions = @(
                [pscustomobject]@{ pr_number = '7'; decision = 'trigger' }
            )
        }
        $result = Expand-RerunDecisionItems -Items @($item)
        $result.Count | Should -Be 1
        $result[0].pr_number | Should -Be '7'
    }

    It 'aggregates decisions across multiple items' {
        $a = [pscustomobject]@{ type = 'trigger_rerun_review'; decisions = '[{"pr_number":"1","decision":"trigger"}]' }
        $b = [pscustomobject]@{ type = 'trigger_rerun_review'; decisions = '[{"pr_number":"2","decision":"skip"}]' }
        $result = Expand-RerunDecisionItems -Items @($a, $b)
        $result.Count | Should -Be 2
        ($result | ForEach-Object { $_.pr_number }) | Should -Be @('1', '2')
    }

    It 'passes through a legacy scalar item without a decisions field' {
        $item = [pscustomobject]@{ type = 'trigger_rerun_review'; pr_number = '9'; decision = 'trigger' }
        $result = Expand-RerunDecisionItems -Items @($item)
        $result.Count | Should -Be 1
        $result[0].pr_number | Should -Be '9'
    }

    It 'ignores empty or null decisions payloads' {
        $empty = [pscustomobject]@{ type = 'trigger_rerun_review'; decisions = '' }
        $nullItem = [pscustomobject]@{ type = 'trigger_rerun_review'; decisions = $null }
        $result = Expand-RerunDecisionItems -Items @($empty, $nullItem)
        $result.Count | Should -Be 0
    }
}

Describe 'ConvertTo-TrimmedString' {
    It 'returns empty string for null values' {
        ConvertTo-TrimmedString $null | Should -Be ''
    }

    It 'trims non-null values' {
        ConvertTo-TrimmedString "  ok`n" | Should -Be 'ok'
    }
}

Describe 'Get-MatchingCandidate' {
    It 'matches only PRs in the deterministic candidate set' {
        $candidates = @(
            [pscustomobject]@{ prNumber = 123; headSha = 'abc' }
            [pscustomobject]@{ prNumber = 456; headSha = 'def' }
        )

        (Get-MatchingCandidate -Candidates $candidates -PRNumber 456).headSha | Should -Be 'def'
        Get-MatchingCandidate -Candidates $candidates -PRNumber 789 | Should -BeNullOrEmpty
    }
}

Describe 'Normalize-PipelineRef' {
    It 'strips refs/heads/ prefix' {
        Normalize-PipelineRef -Value 'refs/heads/feature/x' | Should -Be 'feature/x'
    }

    It 'returns fallback for empty input' {
        Normalize-PipelineRef -Value '' -Fallback 'main' | Should -Be 'main'
    }

    It 'returns fallback when traversal or anchor patterns are present' {
        Normalize-PipelineRef -Value '../etc/passwd' -Fallback 'main' | Should -Be 'main'
        Normalize-PipelineRef -Value '/feature' -Fallback 'main' | Should -Be 'main'
        Normalize-PipelineRef -Value 'feature/' -Fallback 'main' | Should -Be 'main'
    }

    It 'strips characters outside the safe set and returns fallback when result ends with slash' {
        Normalize-PipelineRef -Value 'feature/x; rm -rf /' -Fallback 'main' | Should -Be 'main'
    }

    It 'strips invalid characters while keeping a valid ref intact' {
        Normalize-PipelineRef -Value 'feat ure/x' -Fallback 'main' | Should -Be 'feature/x'
    }
}

Describe 'Get-PlatformFromLabels' {
    It 'prefers a valid fallback over labels' {
        Get-PlatformFromLabels -Labels @('platform/ios') -Fallback 'android' | Should -Be 'android'
    }

    It 'ignores an invalid fallback and falls back to labels' {
        Get-PlatformFromLabels -Labels @('platform/ios') -Fallback '../etc/passwd' | Should -Be 'ios'
    }

    It 'maps platform/macos to catalyst' {
        Get-PlatformFromLabels -Labels @('platform/macos') | Should -Be 'catalyst'
    }

    It 'defaults to android when no platform label is present' {
        Get-PlatformFromLabels -Labels @('area-controls') | Should -Be 'android'
    }
}

Describe 'Candidate-sourced values' {
    It 'produces the rerun comment id from candidate data, not the agent emission' {
        $candidate = New-TestCandidate -RerunCommentId 4242

        $rerunCommentId = if ($candidate.rerunCommentId) { [Int64]$candidate.rerunCommentId } else { [Int64]0 }

        $rerunCommentId | Should -Be 4242
    }

    It 'falls back to zero when candidate has no rerun comment id' {
        $candidate = New-TestCandidate -RerunCommentId 0
        $rerunCommentId = if ($candidate.rerunCommentId) { [Int64]$candidate.rerunCommentId } else { [Int64]0 }

        $rerunCommentId | Should -Be 0
    }

    It 'normalizes the candidate pipeline ref against the configured default' {
        $candidate = New-TestCandidate -PipelineRef 'refs/heads/feature/x'

        Normalize-PipelineRef -Value ([string]$candidate.pipelineRef) -Fallback 'main' | Should -Be 'feature/x'
    }

    It 'falls back to the configured default when candidate pipeline ref is unsafe' {
        $candidate = New-TestCandidate -PipelineRef '../escape'

        Normalize-PipelineRef -Value ([string]$candidate.pipelineRef) -Fallback 'main' | Should -Be 'main'
    }

    It 'lets label-derived platform override an invalid candidate platform' {
        $candidate = New-TestCandidate -Platform ''

        Get-PlatformFromLabels -Labels @('platform/ios') -Fallback ([string]$candidate.platform) | Should -Be 'ios'
    }
}

Describe 'Get-RerunActions' {
    It 'produces a normalized action for a valid trigger decision, sourcing values from the candidate' {
        $items = @(New-TestDecision -PRNumber '123' -Decision 'trigger' -ExpectedHeadSha 'abc123def')
        $candidates = @(New-TestCandidate -PRNumber 123 -HeadSha 'abc123def' -Platform 'ios' -PipelineRef 'main' -RerunCommentId 4242)

        $result = Get-RerunActions -Items $items -Candidates $candidates -DefaultPipelineRef 'main'

        $result.HadFailure | Should -BeFalse
        $result.Actions.Count | Should -Be 1
        $result.Actions[0].prNumber | Should -Be 123
        $result.Actions[0].decision | Should -Be 'trigger'
        $result.Actions[0].platform | Should -Be 'ios'
        $result.Actions[0].pipelineRef | Should -Be 'main'
        $result.Actions[0].rerunCommentId | Should -Be 4242
    }

    It 'produces an action for a valid skip decision even without a rerun comment id' {
        $items = @(New-TestDecision -PRNumber '50' -Decision 'skip' -ExpectedHeadSha 'sha50')
        $candidates = @(New-TestCandidate -PRNumber 50 -HeadSha 'sha50' -RerunCommentId 0)

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeFalse
        $result.Actions.Count | Should -Be 1
        $result.Actions[0].decision | Should -Be 'skip'
        $result.Actions[0].rerunCommentId | Should -Be 0
    }

    It 'normalizes the pipeline ref and resolves an invalid candidate platform to android' {
        $items = @(New-TestDecision -PRNumber '7' -Decision 'trigger' -ExpectedHeadSha 's7')
        $candidates = @(New-TestCandidate -PRNumber 7 -HeadSha 's7' -Platform 'macos' -PipelineRef 'refs/heads/feature/x' -RerunCommentId 9)

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeFalse
        $result.Actions[0].platform | Should -Be 'android'
        $result.Actions[0].pipelineRef | Should -Be 'feature/x'
    }

    It 'aggregates multiple decisions in one pass' {
        $items = @(
            (New-TestDecision -PRNumber '2' -Decision 'skip' -ExpectedHeadSha 's2'),
            (New-TestDecision -PRNumber '1' -Decision 'trigger' -ExpectedHeadSha 's1')
        )
        $candidates = @(
            (New-TestCandidate -PRNumber 1 -HeadSha 's1' -RerunCommentId 11),
            (New-TestCandidate -PRNumber 2 -HeadSha 's2' -RerunCommentId 22)
        )

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeFalse
        $result.Actions.Count | Should -Be 2
    }

    It 'flags a failure and drops the action when pr_number is not a positive integer' {
        $items = @(New-TestDecision -PRNumber '0' -Decision 'trigger' -ExpectedHeadSha 'x')

        $result = Get-RerunActions -Items $items -Candidates @()

        $result.HadFailure | Should -BeTrue
        $result.Actions.Count | Should -Be 0
    }

    It 'flags a failure for an unknown decision verb' {
        $items = @(New-TestDecision -PRNumber '5' -Decision 'launch' -ExpectedHeadSha 'x')
        $candidates = @(New-TestCandidate -PRNumber 5 -HeadSha 'x')

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeTrue
        $result.Actions.Count | Should -Be 0
    }

    It 'flags a failure when the expected head SHA is missing' {
        $items = @(New-TestDecision -PRNumber '5' -Decision 'trigger' -ExpectedHeadSha '')
        $candidates = @(New-TestCandidate -PRNumber 5 -HeadSha 'x' -RerunCommentId 1)

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeTrue
        $result.Actions.Count | Should -Be 0
    }

    It 'rejects a decision for a PR outside the deterministic candidate set' {
        $items = @(New-TestDecision -PRNumber '999' -Decision 'trigger' -ExpectedHeadSha 'x')
        $candidates = @(New-TestCandidate -PRNumber 5 -HeadSha 'x')

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeTrue
        $result.Actions.Count | Should -Be 0
    }

    It 'rejects a decision whose head SHA does not match the candidate (anti-stale / anti-hallucination)' {
        $items = @(New-TestDecision -PRNumber '5' -Decision 'trigger' -ExpectedHeadSha 'STALE')
        $candidates = @(New-TestCandidate -PRNumber 5 -HeadSha 'FRESH' -RerunCommentId 1)

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeTrue
        $result.Actions.Count | Should -Be 0
    }

    It 'refuses to trigger when the candidate has no rerun comment id' {
        $items = @(New-TestDecision -PRNumber '5' -Decision 'trigger' -ExpectedHeadSha 'x')
        $candidates = @(New-TestCandidate -PRNumber 5 -HeadSha 'x' -RerunCommentId 0)

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeTrue
        $result.Actions.Count | Should -Be 0
    }

    It 'continues processing valid decisions after a failed one' {
        $items = @(
            (New-TestDecision -PRNumber 'bad' -Decision 'trigger' -ExpectedHeadSha 'x'),
            (New-TestDecision -PRNumber '5' -Decision 'skip' -ExpectedHeadSha 's5')
        )
        $candidates = @(New-TestCandidate -PRNumber 5 -HeadSha 's5' -RerunCommentId 1)

        $result = Get-RerunActions -Items $items -Candidates $candidates

        $result.HadFailure | Should -BeTrue
        $result.Actions.Count | Should -Be 1
        $result.Actions[0].prNumber | Should -Be 5
    }
}
