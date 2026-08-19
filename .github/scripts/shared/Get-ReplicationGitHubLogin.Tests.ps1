#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Get-ReplicationGitHubLogin.ps1')
}

Describe 'Replication GitHub login probe' {
    # The stubs below are global so the dot-sourced production functions
    # resolve them. A global function outranks an executable of the same name
    # on PATH, so leaving one behind silently shadows the real gh for every
    # test file that runs afterwards in the same Pester session.
    AfterEach {
        Remove-Item -LiteralPath 'function:gh' -ErrorAction SilentlyContinue
    }
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

    It 'retries the exact 503 that blocked reproduced runs 14994333 and 14994436' {
        $script:calls = 0
        function global:gh {
            $script:calls++
            if ($script:calls -lt 3) {
                $global:LASTEXITCODE = 1
                return 'gh: No server is currently available to service your request. Sorry about that. Please try resubmitting your request and contact us if the problem persists. (HTTP 503)'
            }
            $global:LASTEXITCODE = 0
            return '{"data":{"ok":true}}'
        }

        try {
            $result = Invoke-ReplicationGitHubCli `
                -Arguments @('api', 'graphql', '-f', 'query=x') `
                -Description 'inspect repositories available to MauiBot' `
                -MaximumAttempts 4 -RetryDelaysSeconds @(0, 0, 0)
            ($result -join '') | Should -Match 'ok'
            $script:calls | Should -Be 3
        }
        finally {
            Remove-Item Function:global:gh -ErrorAction SilentlyContinue
        }
    }

    It 'does not retry a non-transient GitHub CLI failure' {
        $script:calls = 0
        function global:gh {
            $script:calls++
            $global:LASTEXITCODE = 1
            return 'gh: HTTP 404: Not Found'
        }

        try {
            {
                Invoke-ReplicationGitHubCli -Arguments @('api', 'repos/x/y') `
                    -Description 'read a repository' `
                    -MaximumAttempts 4 -RetryDelaysSeconds @(0, 0, 0)
            } | Should -Throw '*read a repository*Not Found*'
            $script:calls | Should -Be 1
        }
        finally {
            Remove-Item Function:global:gh -ErrorAction SilentlyContinue
        }
    }

    It 'routes every publisher preflight GitHub call through the bounded helper' {
        $pipeline = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../../../eng/pipelines/ci-copilot.yml') -Raw
        $probe = [regex]::Match(
            $pipeline,
            "(?s)- pwsh: \|(?<body>.*?)displayName: 'Probe MauiBot identity and writable fork'")
        $probe.Success | Should -BeTrue
        $body = $probe.Groups['body'].Value
        $body | Should -Match 'Get-ReplicationGitHubLogin'
        $body | Should -Match "Invoke-ReplicationGitHubCli"
        $body | Should -Not -Match '&\s+gh\s+api\s+graphql'
        $body | Should -Not -Match '&\s+gh\s+api\s+-X\s+POST'
    }

    It 'stages every shared script the trusted publishers dot-source' {
        $sharedDir = $PSScriptRoot
        $pipelinePath = Join-Path $PSScriptRoot '../../../eng/pipelines/ci-copilot.yml'
        $pipeline = Get-Content -LiteralPath $pipelinePath -Raw

        $stageBlock = [regex]::Match(
            $pipeline,
            "(?s)trusted-replication-publisher(?<body>.*?)displayName: 'Stage trusted replication publisher'")
        $stageBlock.Success | Should -BeTrue
        $staged = @([regex]::Matches($stageBlock.Groups['body'].Value, "'(?<name>[\w-]+\.ps1)'") |
            ForEach-Object { $_.Groups['name'].Value })
        $staged | Should -Contain 'Get-ReplicationGitHubLogin.ps1'

        # Anything a staged publisher dot-sources must itself be staged, or the
        # publisher dies at runtime on a missing file (run 14994436).
        foreach ($name in $staged) {
            $path = Join-Path $sharedDir $name
            if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { continue }
            $content = Get-Content -LiteralPath $path -Raw
            foreach ($m in [regex]::Matches($content, '(?m)^\s*\.\s+.*?(?<dep>[\w-]+\.ps1)')) {
                $dep = $m.Groups['dep'].Value
                if ($dep -eq $name) { continue }
                $staged |
                    Should -Contain $dep -Because "$name dot-sources $dep, so it must be staged too"
            }
        }
    }

    It 'is staged into the trusted publisher root' {
        $pipeline = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../../../eng/pipelines/ci-copilot.yml') -Raw
        $pipeline | Should -Match "'Get-ReplicationGitHubLogin\.ps1',"
    }
}
