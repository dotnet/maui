#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Assert-ReplicationGitHubCredential.ps1')

    function New-CredentialResponse {
        param(
            [int]$StatusCode = 200,
            [string]$Login = 'MauiBot',
            [string]$Message = '',
            [hashtable]$Headers = @{}
        )

        return @{
            StatusCode = $StatusCode
            Login      = $Login
            Message    = $Message
            Headers    = $Headers
        }
    }
}

Describe 'Assert-ReplicationGitHubCredential' {
    It 'returns the authenticated login when GitHub accepts the token' {
        $login = Assert-ReplicationGitHubCredential `
            -Token 'valid' `
            -Requester { New-CredentialResponse -Login 'MauiBot' } `
            -RetryDelaysSeconds @(0)

        $login | Should -Be 'MauiBot'
    }

    It 'refuses an empty token without asking GitHub' {
        $called = $false
        {
            Assert-ReplicationGitHubCredential `
                -Token '  ' `
                -Requester { $script:called = $true; New-CredentialResponse }
        } | Should -Throw '*is empty*'

        $called | Should -BeFalse
    }

    It 'names the pipeline variable that has to be rotated on a 401' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'dead' `
                -TokenVariableName 'GH_COMMENT_TOKEN' `
                -Requester { New-CredentialResponse -StatusCode 401 } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw "*GH_COMMENT_TOKEN*rotated*"
    }

    It 'does not retry a rejected credential' {
        $script:attempts = 0
        {
            Assert-ReplicationGitHubCredential `
                -Token 'dead' `
                -MaximumAttempts 5 `
                -Requester {
                    $script:attempts++
                    New-CredentialResponse -StatusCode 401
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw

        $script:attempts | Should -Be 1
    }

    It 'reports a spent quota as a rate limit rather than a dead credential' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester {
                    New-CredentialResponse `
                        -StatusCode 403 `
                        -Headers @{ 'x-ratelimit-remaining' = '0' }
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*rate limited*'
    }

    It 'does not tell anyone to rotate a token that is only rate limited' {
        $message = ''
        try {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester {
                    New-CredentialResponse `
                        -StatusCode 403 `
                        -Headers @{ 'X-RateLimit-Remaining' = '0' }
                } `
                -RetryDelaysSeconds @(0)
        } catch {
            $message = [string]$_.Exception.Message
        }

        $message | Should -Not -BeNullOrEmpty
        $message | Should -Not -Match 'rotate'
    }

    It 'reports a 403 with quota remaining as a missing permission' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester {
                    New-CredentialResponse `
                        -StatusCode 403 `
                        -Headers @{ 'x-ratelimit-remaining' = '4999' }
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*missing a permission*'
    }

    It 'retries a GitHub outage and succeeds when the service returns' {
        $script:attempts = 0
        $login = Assert-ReplicationGitHubCredential `
            -Token 'valid' `
            -Requester {
                $script:attempts++
                if ($script:attempts -lt 3) {
                    return New-CredentialResponse -StatusCode 503
                }

                return New-CredentialResponse -Login 'MauiBot'
            } `
            -RetryDelaysSeconds @(0)

        $login | Should -Be 'MauiBot'
        $script:attempts | Should -Be 3
    }

    It 'retries a network failure that never reached GitHub' {
        $script:attempts = 0
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -MaximumAttempts 3 `
                -Requester {
                    $script:attempts++
                    New-CredentialResponse `
                        -StatusCode 0 `
                        -Message 'The operation timed out'
                } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*timed out*'

        $script:attempts | Should -Be 3
    }

    It 'rejects a token that authenticates as the wrong account' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -ExpectedLogin 'MauiBot' `
                -Requester { New-CredentialResponse -Login 'someone-else' } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw "*authenticates as 'someone-else'*"
    }

    It 'accepts the expected login regardless of case' {
        $login = Assert-ReplicationGitHubCredential `
            -Token 'valid' `
            -ExpectedLogin 'mauibot' `
            -Requester { New-CredentialResponse -Login 'MauiBot' } `
            -RetryDelaysSeconds @(0)

        $login | Should -Be 'MauiBot'
    }

    It 'fails when GitHub accepts the token but returns no login' {
        {
            Assert-ReplicationGitHubCredential `
                -Token 'valid' `
                -Requester { New-CredentialResponse -Login '' } `
                -RetryDelaysSeconds @(0)
        } | Should -Throw '*did not*return a login*'
    }

    It 'never repeats the token in a failure message' {
        $secret = 'ghp_examplesecretvalue'
        $message = ''
        try {
            Assert-ReplicationGitHubCredential `
                -Token $secret `
                -Requester { New-CredentialResponse -StatusCode 401 } `
                -RetryDelaysSeconds @(0)
        } catch {
            $message = [string]$_.Exception.Message
        }

        $message | Should -Not -Match ([regex]::Escape($secret))
    }
}

Describe 'Get-ReplicationCredentialFailureKind' {
    It 'treats 429 as a rate limit' {
        Get-ReplicationCredentialFailureKind -StatusCode 429 |
            Should -Be 'ratelimited'
    }

    It 'treats a secondary rate limit message as a rate limit' {
        Get-ReplicationCredentialFailureKind `
            -StatusCode 403 `
            -Message 'You have exceeded a secondary rate limit' |
            Should -Be 'ratelimited'
    }

    It 'treats a server error as transient' {
        Get-ReplicationCredentialFailureKind -StatusCode 502 |
            Should -Be 'transient'
    }

    It 'does not treat a rejected credential as transient' {
        Get-ReplicationCredentialFailureKind -StatusCode 401 |
            Should -Be 'invalid'
    }
}

Describe 'Replication credential pre-flight wiring' {
    BeforeAll {
        $script:PipelinePath = Join-Path `
            (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) `
            'pipelines/ci-copilot.yml'
        if (-not (Test-Path -LiteralPath $script:PipelinePath)) {
            $script:PipelinePath = Join-Path `
                (Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $PSScriptRoot))) `
                'eng/pipelines/ci-copilot.yml'
        }
        $script:Pipeline = Get-Content -LiteralPath $script:PipelinePath -Raw
    }

    It 'checks the credential before the expensive setup steps' {
        $probeIndex = $script:Pipeline.IndexOf('Verify the GitHub credential')
        $probeIndex | Should -BeGreaterThan 0

        foreach ($expensive in @(
            'Capture trusted test infrastructure',
            'Install reproduction recording tools',
            'Install GitHub CLI',
            'Prepare sanitized issue context')) {
            $index = $script:Pipeline.IndexOf($expensive)
            $index | Should -BeGreaterThan 0
            $probeIndex | Should -BeLessThan $index
        }
    }

    It 'only runs the pre-flight for replicate runs' {
        $probeIndex = $script:Pipeline.IndexOf('Verify the GitHub credential')
        $window = $script:Pipeline.Substring($probeIndex, 700)
        $window | Should -Match "eq\('\$\{\{ parameters\.Mode \}\}', 'replicate'\)"
    }
}
