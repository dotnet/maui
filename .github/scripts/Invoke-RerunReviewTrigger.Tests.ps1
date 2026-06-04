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

    foreach ($functionName in @('Get-ReviewTriggerRateLimitStatus', 'ConvertTo-SafeLogValue', 'Get-MatchingCandidate')) {
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
