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

    It 'names the credential an operator must rotate, not just the raw error' {
        # Every run of every platform now dies on this exact message. "Failed
        # to read the authenticated GitHub login: gh: Bad credentials (HTTP
        # 401)" reads like a defect in the replication code, and the log has
        # to answer "why did it fail" on its own.
        $script:calls = 0
        function global:gh {
            $script:calls++
            $global:LASTEXITCODE = 1
            return 'gh: Bad credentials (HTTP 401)'
        }

        try {
            $thrown = $null
            try { Get-ReplicationGitHubLogin -MaximumAttempts 3 -RetryDelaysSeconds @(0, 0) }
            catch { $thrown = $_ }

            $thrown.Exception.Message | Should -Match 'GH_COMMENT_TOKEN'
            $thrown.Exception.Message | Should -Match 'rotated'
            $thrown.Exception.Message | Should -Match 'Bad credentials'
            $script:calls | Should -Be 1
        }
        finally {
            Remove-Item Function:global:gh -ErrorAction SilentlyContinue
        }
    }

    It 'names the credential from the bounded helper too' {
        $script:calls = 0
        function global:gh {
            $script:calls++
            $global:LASTEXITCODE = 1
            return 'gh: HTTP 401: Requires authentication'
        }

        try {
            $thrown = $null
            try {
                Invoke-ReplicationGitHubCli `
                    -Arguments @('api', 'user') `
                    -Description 'inspect repositories available to MauiBot' `
                    -MaximumAttempts 3 -RetryDelaysSeconds @(0, 0)
            } catch { $thrown = $_ }

            $thrown.Exception.Message | Should -Match 'GH_COMMENT_TOKEN'
            $thrown.Exception.Message | Should -Match 'inspect repositories available to MauiBot'
            $script:calls | Should -Be 1
        }
        finally {
            Remove-Item Function:global:gh -ErrorAction SilentlyContinue
        }
    }

    It 'does not blame the credential for a failure it did not cause' {
        $message = New-ReplicationGitHubFailureMessage `
            -Description 'read the authenticated GitHub login' `
            -Detail 'gh: Could not resolve to a Repository with the name.'

        $message | Should -Not -Match 'GH_COMMENT_TOKEN'
        $message | Should -Match 'Could not resolve to a Repository'
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

    It 'never lets a raw pipeline-inline gh call decide the run on its own' {
        # The probe this test used to guard is gone: Publish-ReplicationPR
        # re-checks identity and fork anyway, so the probe only cost an agent
        # and, once the token expired, blocked feedback collection outright.
        # The property worth keeping is the general one -- a GitHub call made
        # inline in the pipeline either goes through the bounded helper, which
        # retries transient failures, or degrades to a warning. Neither may
        # end the run on a single unlucky response.
        $pipeline = Get-Content -LiteralPath (
            Join-Path $PSScriptRoot '../../../eng/pipelines/ci-copilot.yml') -Raw

        $pipeline | Should -Not -Match "Probe MauiBot identity and writable fork"

        $rawCalls = @([regex]::Matches($pipeline, '&\s+gh\s+[a-z]'))
        # If this drops to zero the assertions below stop testing anything.
        $rawCalls.Count | Should -BeGreaterThan 0

        foreach ($call in $rawCalls) {
            $tryStart = $pipeline.LastIndexOf('try {', $call.Index)
            $tryStart | Should -BeGreaterThan -1 -Because (
                "the raw gh call at offset $($call.Index) must sit inside a try")

            # Walk to the brace that closes this try, so the catch examined is
            # the one that actually handles this call and not a later sibling.
            $depth = 0
            $tryEnd = -1
            for ($i = $pipeline.IndexOf('{', $tryStart); $i -lt $pipeline.Length; $i++) {
                if ($pipeline[$i] -eq '{') { $depth++ }
                elseif ($pipeline[$i] -eq '}') {
                    $depth--
                    if ($depth -eq 0) { $tryEnd = $i; break }
                }
            }
            $tryEnd | Should -BeGreaterThan $call.Index -Because (
                "the try enclosing offset $($call.Index) must close after it")

            $catchStart = $pipeline.IndexOf('catch', $tryEnd)
            $catchStart | Should -BeGreaterThan -1
            $depth = 0
            $catchEnd = -1
            for ($i = $pipeline.IndexOf('{', $catchStart); $i -lt $pipeline.Length; $i++) {
                if ($pipeline[$i] -eq '{') { $depth++ }
                elseif ($pipeline[$i] -eq '}') {
                    $depth--
                    if ($depth -eq 0) { $catchEnd = $i; break }
                }
            }
            $catchBody = $pipeline.Substring($catchStart, $catchEnd - $catchStart + 1)

            $catchBody | Should -Match 'task\.logissue type=warning' -Because (
                "the raw gh call at offset $($call.Index) must degrade to a warning")
            $catchBody | Should -Not -Match '(?m)^\s*throw\b' -Because (
                "the raw gh call at offset $($call.Index) must not end the run")
        }
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

Describe 'Distinguishing an unprovided secret from an expired one' {
    BeforeAll { . "$PSScriptRoot/Get-ReplicationGitHubLogin.ps1" }
    AfterEach { Remove-Item -LiteralPath 'env:GH_TOKEN' -ErrorAction SilentlyContinue }

    It 'says the variable was never substituted rather than blaming the token' {
        # Azure Pipelines leaves '$(Name)' in place when Name is not defined for
        # the run. GitHub then answers 401 exactly as it would for a revoked
        # token, but the remedies are opposite: rotating a healthy secret cannot
        # fix a secret that is not being delivered to the ref being built.
        $env:GH_TOKEN = '$(GH_COMMENT_TOKEN)'
        $message = New-ReplicationGitHubFailureMessage `
            -Description 'read the authenticated GitHub login' `
            -Detail 'gh: Bad credentials (HTTP 401)'

        $message | Should -Match 'was not substituted'
        $message | Should -Match 'not being provided to the ref'
        $message | Should -Not -Match 'expired or been revoked'
    }

    It 'still reports a real 401 as a credential that must be rotated' {
        $env:GH_TOKEN = 'ghp_a_real_looking_token_value'
        $message = New-ReplicationGitHubFailureMessage `
            -Description 'read the authenticated GitHub login' `
            -Detail 'gh: Bad credentials (HTTP 401)'

        $message | Should -Match 'expired or been revoked'
        $message | Should -Not -Match 'was not substituted'
    }

    It 'never echoes the credential itself into the message' {
        $env:GH_TOKEN = 'ghp_supersecretvalue'
        $message = New-ReplicationGitHubFailureMessage `
            -Description 'read the authenticated GitHub login' `
            -Detail 'gh: Bad credentials (HTTP 401)'

        $message | Should -Not -Match 'supersecretvalue'
    }
}
