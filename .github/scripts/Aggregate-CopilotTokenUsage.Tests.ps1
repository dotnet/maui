#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for Aggregate-CopilotTokenUsage.ps1.
#>

Describe 'Aggregate-CopilotTokenUsage.ps1' {
    BeforeEach {
        $script:fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) "token-usage-fixtures-$([guid]::NewGuid())"
        $script:inputRoot = Join-Path $script:fixtureRoot 'input'
        $script:outputRoot = Join-Path $script:fixtureRoot 'output'
        New-Item -ItemType Directory -Path $script:inputRoot -Force | Out-Null
    }

    AfterEach {
        Remove-Item -Path $script:fixtureRoot -Recurse -Force -ErrorAction SilentlyContinue
    }

    It 'writes raw and summarized artifacts with zero rows for stages without Copilot invocations' {
        $nested = Join-Path $script:inputRoot 'CopilotLogs/copilot-token-usage/raw'
        New-Item -ItemType Directory -Path $nested -Force | Out-Null

        [ordered]@{
            schemaVersion    = 1
            prNumber         = 35677
            pipeline         = [ordered]@{ stageName = 'ReviewPR' }
            scriptPhase      = 'CopilotReview'
            copilotStep      = 'STEP 5a: TRY-FIX'
            model            = 'gpt-5.5'
            durationMs       = 5000
            apiDurationMs    = 2000
            turnCount        = 2
            toolCount        = 3
            cliUsage         = [ordered]@{
                aicUsed          = 7.5
                contextWindow    = 1100000
                contextWindowRaw = '1.1M'
            }
            normalizedTokens = [ordered]@{
                inputTokens       = 100
                outputTokens      = 40
                cachedInputTokens = 10
                reasoningOutputTokens = 5
                totalTokens       = 140
            }
        } | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $nested 'copilot-token-usage-a.json') -Encoding UTF8

        $scriptPath = Join-Path $PSScriptRoot 'shared/Aggregate-CopilotTokenUsage.ps1'
        & $scriptPath `
            -InputRoot $script:inputRoot `
            -OutputDir $script:outputRoot `
            -PRNumber '35677' `
            -ExpectedStages @('ReviewPR', 'RunDeepUITests', 'UpdateAISummaryComment', 'AnalyzeCopilotTokenUsage')

        Test-Path (Join-Path $script:outputRoot 'token-usage-raw.jsonl') | Should -Be $true
        Test-Path (Join-Path $script:outputRoot 'token-usage-summary.md') | Should -Be $true
        Test-Path (Join-Path $script:outputRoot 'token-usage-by-step.csv') | Should -Be $true

        $summary = Get-Content (Join-Path $script:outputRoot 'token-usage-summary.json') -Raw | ConvertFrom-Json
        $summary.recordCount | Should -Be 1
        $summary.totals.inputTokens | Should -Be 100
        $summary.totals.outputTokens | Should -Be 40
        $summary.totals.cachedInputTokens | Should -Be 10
        $summary.totals.reasoningOutputTokens | Should -Be 5
        $summary.totals.totalTokens | Should -Be 140
        $summary.totals.aicUsed | Should -Be 7.5

        $reviewStage = $summary.stages | Where-Object { $_.stageName -eq 'ReviewPR' }
        $reviewStage.invocationCount | Should -Be 1
        $reviewStage.totalTokens | Should -Be 140
        $reviewStage.reasoningOutputTokens | Should -Be 5
        $reviewStage.aicUsed | Should -Be 7.5

        $deepStage = $summary.stages | Where-Object { $_.stageName -eq 'RunDeepUITests' }
        $deepStage.invocationCount | Should -Be 0
        $deepStage.totalTokens | Should -Be 0
        $deepStage.reasoningOutputTokens | Should -Be 0
        $deepStage.aicUsed | Should -Be 0
        $deepStage.note | Should -Be 'No Copilot invocation observed in this stage.'
    }

    It 'emits a no-record summary when the input artifact is missing' {
        $scriptPath = Join-Path $PSScriptRoot 'shared/Aggregate-CopilotTokenUsage.ps1'
        & $scriptPath `
            -InputRoot (Join-Path $script:fixtureRoot 'missing') `
            -OutputDir $script:outputRoot `
            -PRNumber '35677'

        $summary = Get-Content (Join-Path $script:outputRoot 'token-usage-summary.json') -Raw | ConvertFrom-Json
        $summary.recordCount | Should -Be 0
        ($summary.stages | Where-Object { $_.stageName -eq 'ReviewPR' }).invocationCount | Should -Be 0
        Test-Path (Join-Path $script:outputRoot 'token-usage-by-step.csv') | Should -Be $true
    }
}
