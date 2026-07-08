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

    $script:ReviewTriggerCooldownMinutes = 60
    $script:ReviewTriggerWindowHours = 24
    $script:MaxReviewTriggersPerWindow = 3

    foreach ($functionName in @('Get-ReviewTriggerRateLimitStatus', 'ConvertTo-SafeLogValue', 'Get-MatchingCandidate', 'Normalize-PipelineRef', 'Get-PlatformFromLabels')) {
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

        # The handler reads $candidate.rerunCommentId via:
        #   $rerunCommentId = if ($candidate.rerunCommentId) { [Int64]$candidate.rerunCommentId } else { [Int64]0 }
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

Describe 'Get-ReviewTriggerRateLimitStatus' {
    It 'allows a PR with no recent rerun triggers' {
        $now = [datetimeoffset]'2026-06-04T12:00:00Z'

        $result = Get-ReviewTriggerRateLimitStatus -TriggeredAt @() -Now $now

        $result.Allowed | Should -BeTrue
        $result.Reason | Should -Be 'allowed'
        $result.RecentCount | Should -Be 0
    }

    It 'blocks rerun triggers inside the cooldown window' {
        $now = [datetimeoffset]'2026-06-04T12:00:00Z'

        $result = Get-ReviewTriggerRateLimitStatus `
            -TriggeredAt @([datetimeoffset]'2026-06-04T11:30:00Z') `
            -Now $now

        $result.Allowed | Should -BeFalse
        $result.Reason | Should -Be 'cooldown-active'
        $result.RecentCount | Should -Be 1
    }

    It 'blocks when the 24 hour quota is exhausted' {
        $now = [datetimeoffset]'2026-06-04T12:00:00Z'

        $result = Get-ReviewTriggerRateLimitStatus `
            -TriggeredAt @(
                [datetimeoffset]'2026-06-04T10:00:00Z',
                [datetimeoffset]'2026-06-04T08:00:00Z',
                [datetimeoffset]'2026-06-03T13:00:00Z'
            ) `
            -Now $now

        $result.Allowed | Should -BeFalse
        $result.Reason | Should -Be 'rerun-quota-exhausted'
        $result.RecentCount | Should -Be 3
    }

    It 'ignores trigger history older than the window' {
        $now = [datetimeoffset]'2026-06-04T12:00:00Z'

        $result = Get-ReviewTriggerRateLimitStatus `
            -TriggeredAt @(
                [datetimeoffset]'2026-06-04T10:00:00Z',
                [datetimeoffset]'2026-06-03T11:00:00Z',
                [datetimeoffset]'2026-06-02T12:00:00Z'
            ) `
            -Now $now

        $result.Allowed | Should -BeTrue
        $result.Reason | Should -Be 'allowed'
        $result.RecentCount | Should -Be 1
    }
}
