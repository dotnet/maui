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

Describe 'The publisher accepts what the gate accepted' {
    It 'shares one signature comparison with the verifier' {
        # Build 15030627 reproduced its issue and passed the credential-free
        # gate, then died in the publisher for a wording difference the gate
        # deliberately allows.
        $module = Join-Path $PSScriptRoot 'shared/Get-ReplicationSignatureMatch.ps1'
        Test-Path -LiteralPath $module -PathType Leaf | Should -BeTrue

        foreach ($name in @(
                'shared/Invoke-ReplicationTestVerification.ps1',
                'shared/Publish-ReplicationEvidence.ps1')) {
            $source = Get-Content -LiteralPath (Join-Path $PSScriptRoot $name) -Raw
            $source | Should -Match 'Get-ReplicationSignatureMatch\.ps1' -Because `
                "$name must use the shared comparison rather than its own"
            $source | Should -Not -Match 'function Test-ReplicationSignatureEquivalent'
        }

        $publisher = Get-Content `
            -LiteralPath (Join-Path $PSScriptRoot 'shared/Publish-ReplicationEvidence.ps1') -Raw
        $publisher | Should -Match 'Test-ReplicationSignatureEquivalent'
    }

    It 'still rejects a message that shares nothing with the signature' {
        . (Join-Path $PSScriptRoot 'shared/Get-ReplicationSignatureMatch.ps1')
        Test-ReplicationSignatureEquivalent `
            -Declared 'On-state render mismatch: switch thumb offset' `
            -Observed 'The app was expected to be running still' | Should -BeFalse
    }

    It 'accepts a reworded rendering of the same declared failure' {
        . (Join-Path $PSScriptRoot 'shared/Get-ReplicationSignatureMatch.ps1')
        Test-ReplicationSignatureEquivalent `
            -Declared 'Switch thumb offset mismatch after toggle' `
            -Observed '  Switch thumb offset mismatch  after  toggle (native)' | Should -BeTrue
    }
}
