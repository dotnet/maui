#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Get-ReplicationGitHubLogin.ps1')
}

Describe 'Replication GitHub login probe' {
    It 'classifies transient GitHub service failures' {
        foreach ($output in @(
            'gh: No server is currently available to service your request. (HTTP 503)',
            'HTTP 429: rate limited',
            'HTTP 502 Bad Gateway',
            'connection reset by peer',
            'Temporary failure in name resolution'
        )) {
            Test-TransientReplicationGitHubFailure -Output $output | Should -BeTrue
        }
    }

    It 'does not classify a rejected credential as transient' {
        foreach ($output in @(
            'HTTP 401: Bad credentials',
            'HTTP 403: Resource not accessible by personal access token'
        )) {
            Test-TransientReplicationGitHubFailure -Output $output | Should -BeFalse
        }
    }

    It 'returns the login once GitHub recovers' {
        $script:calls = 0
        function global:gh {
            $script:calls++
            if ($script:calls -lt 3) {
                $global:LASTEXITCODE = 1
                return 'gh: No server is currently available to service your request. (HTTP 503)'
            }
            $global:LASTEXITCODE = 0
            return 'MauiBot'
        }

        try {
            Get-ReplicationGitHubLogin -MaximumAttempts 3 -RetryDelaysSeconds @(0, 0) |
                Should -BeExactly 'MauiBot'
            $script:calls | Should -Be 3
        }
        finally {
            Remove-Item Function:global:gh -ErrorAction SilentlyContinue
        }
    }

    It 'reports the underlying GitHub error instead of blaming the credential' {
        function global:gh {
            $global:LASTEXITCODE = 1
            return 'gh: No server is currently available to service your request. (HTTP 503)'
        }

        try {
            { Get-ReplicationGitHubLogin -MaximumAttempts 2 -RetryDelaysSeconds @(0) } |
                Should -Throw '*Failed to read the authenticated GitHub login*HTTP 503*'
        }
        finally {
            Remove-Item Function:global:gh -ErrorAction SilentlyContinue
        }
    }

    It 'fails fast on a genuinely rejected credential' {
        $script:calls = 0
        function global:gh {
            $script:calls++
            $global:LASTEXITCODE = 1
            return 'gh: HTTP 401: Bad credentials'
        }

        try {
            { Get-ReplicationGitHubLogin -MaximumAttempts 3 -RetryDelaysSeconds @(0, 0) } |
                Should -Throw '*Bad credentials*'
            $script:calls | Should -Be 1
        }
        finally {
            Remove-Item Function:global:gh -ErrorAction SilentlyContinue
        }
    }

    It 'is staged into the trusted publisher root' {
        $pipeline = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../../../eng/pipelines/ci-copilot.yml') -Raw
        $pipeline | Should -Match "'Get-ReplicationGitHubLogin\.ps1',"
    }
}
