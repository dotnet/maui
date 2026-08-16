#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $script:OutcomeScript = Join-Path $PSScriptRoot 'shared/Publish-ReplicationOutcome.ps1'
    $script:OutcomeSource = Get-Content -LiteralPath $script:OutcomeScript -Raw
}

Describe 'Trusted replication issue outcome publishing' {
    It 'handles only a genuine sandbox non-reproduction' {
        $candidatePath = Join-Path $TestDrive 'candidate.json'
        @{
            issueNumber = 12345
            platform = 'ios'
            status = 'blocked'
            blocked = @{
                stage = 'sandbox'
                code = 'sandbox_not_reproduced'
                reason = 'REPLICATION_NOT_REPRODUCED'
            }
        } | ConvertTo-Json -Depth 5 |
            Set-Content -LiteralPath $candidatePath -Encoding utf8NoBOM

        $result = & $script:OutcomeScript `
            -CandidatePath $candidatePath `
            -IssueNumber 12345 `
            -Platform ios `
            -BuildId 14980000 `
            -BuildUrl 'https://devdiv.visualstudio.com/DevDiv/_build/results?buildId=14980000' `
            -DryRun

        $result.handled | Should -BeTrue
        $result.label | Should -BeExactly 's/try-latest-version'
    }

    It 'does not handle infrastructure or verification failures' {
        foreach ($code in @('sandbox_inconclusive', 'verification_inconclusive')) {
            $candidatePath = Join-Path $TestDrive "$code.json"
            @{
                issueNumber = 12345
                platform = 'android'
                status = 'blocked'
                blocked = @{ stage = 'sandbox'; code = $code; reason = 'failure' }
            } | ConvertTo-Json -Depth 5 |
                Set-Content -LiteralPath $candidatePath -Encoding utf8NoBOM

            $result = & $script:OutcomeScript `
                -CandidatePath $candidatePath `
                -IssueNumber 12345 `
                -Platform android `
                -BuildId 14980001 `
                -BuildUrl 'https://devdiv.visualstudio.com/DevDiv/_build/results?buildId=14980001' `
                -DryRun

            $result.handled | Should -BeFalse
            $result.label | Should -BeNullOrEmpty
        }
    }

    It 'requires MauiBot authentication and uses the exact repository label' {
        $script:OutcomeSource | Should -Match "ExpectedLogin = 'Maui-Bot'"
        $script:OutcomeSource | Should -Match "Label = 's/try-latest-version'"
        $script:OutcomeSource | Should -Match 'GH_TOKEN is required'
        $script:OutcomeSource | Should -Match 'gh issue comment'
        $script:OutcomeSource | Should -Match 'gh issue edit'
        $script:OutcomeSource | Should -Match 'MAUI_COPILOT_NOT_REPRODUCED'
    }
}
