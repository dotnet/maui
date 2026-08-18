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
            -Repository 'dotnet/maui' `
            -DryRun

        $result.handled | Should -BeTrue
        $result.label | Should -BeExactly 's/try-latest-version'
    }

    It 'does not handle infrastructure or verification failures' {
        foreach ($code in @(
            'copilot_cli_unavailable',
            'copilot_service_unavailable',
            'unsupported_scenario',
            'sandbox_inconclusive',
            'verification_inconclusive'
        )) {
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
            -Repository 'dotnet/maui' `
                -DryRun

            $result.handled | Should -BeFalse
            $result.label | Should -BeNullOrEmpty
        }
    }

    It 'does not handle a reproduced candidate with a null blocked value' {
        $candidatePath = Join-Path $TestDrive 'reproduced.json'
        @{
            issueNumber = 12345
            platform = 'ios'
            status = 'reproduced'
            blocked = $null
        } | ConvertTo-Json -Depth 5 |
            Set-Content -LiteralPath $candidatePath -Encoding utf8NoBOM

        $result = & $script:OutcomeScript `
            -CandidatePath $candidatePath `
            -IssueNumber 12345 `
            -Platform ios `
            -BuildId 14980002 `
            -BuildUrl 'https://devdiv.visualstudio.com/DevDiv/_build/results?buildId=14980002' `
            -Repository 'dotnet/maui' `
            -DryRun

        $result.handled | Should -BeFalse
    }

    It 'requires MauiBot authentication and uses the exact repository label' {
        $script:OutcomeSource | Should -Match "ExpectedLogin = 'MauiBot'"
        $script:OutcomeSource | Should -Match "Label = 's/try-latest-version'"
        $script:OutcomeSource | Should -Match 'GH_TOKEN is required'
        $script:OutcomeSource | Should -Match 'gh issue comment'
        $script:OutcomeSource | Should -Match 'gh issue edit'
        $script:OutcomeSource | Should -Match 'MAUI_COPILOT_NOT_REPRODUCED'
    }

    It 'retries transient GitHub service failures without misclassifying the token' {
        $script:OutcomeSource | Should -Match '\$serviceRetryDelaysSeconds = @\(30, 60\)'
        $script:OutcomeSource | Should -Match ([regex]::Escape(
            "`$failureText -match '(?im)\bHTTP\s*(?:429|50[234])\b'"))
        $script:OutcomeSource | Should -Match 'GitHub service unavailable while validating MauiBot authentication'
    }

    It 'stays quiet when the run never produced a candidate' {
        # Build 15001512 failed early because the requested number was a pull
        # request, then failed a second time here on the missing candidate,
        # hiding the real cause behind a file-not-found error.
        $missing = Join-Path $TestDrive 'absent-candidate.json'
        Test-Path -LiteralPath $missing | Should -BeFalse

        $result = & $script:OutcomeScript `
            -CandidatePath $missing `
            -IssueNumber 12345 `
            -Platform ios `
            -BuildId 14980000 `
            -BuildUrl 'https://devdiv.visualstudio.com/DevDiv/_build/results?buildId=14980000' `
            -Repository 'dotnet/maui' `
            -DryRun

        $result.handled | Should -BeFalse
        $result.reason | Should -BeExactly 'no-candidate'
    }
}
