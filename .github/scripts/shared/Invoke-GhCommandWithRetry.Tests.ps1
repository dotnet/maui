#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Invoke-GhCommandWithRetry.ps1')
}

Describe 'Invoke-GhCommandWithRetry' {
    BeforeEach {
        $script:ghAttempts = 0
        Mock Start-Sleep {}
    }

    It 'retries a transient HTTP 503 and returns the successful response' {
        Mock gh {
            $script:ghAttempts++
            if ($script:ghAttempts -eq 1) {
                $global:LASTEXITCODE = 1
                return 'gh: HTTP 503: No server is currently available'
            }

            $global:LASTEXITCODE = 0
            return '{"state":"open"}'
        }

        $result = Invoke-GhCommandWithRetry `
            -Arguments @('api', 'repos/dotnet/maui/pulls/1') `
            -Description 'read PR #1' `
            -RequireOutput

        $result | Should -Be '{"state":"open"}'
        Should -Invoke gh -Times 2 -Exactly
        Should -Invoke Start-Sleep -Times 1 -Exactly -ParameterFilter { $Seconds -eq 2 }
    }

    It 'does not convert repeated HTTP 503 failures into a not-found result' {
        Mock gh {
            $global:LASTEXITCODE = 1
            return 'gh: HTTP 503: No server is currently available'
        }

        {
            Invoke-GhCommandWithRetry `
                -Arguments @('api', 'repos/dotnet/maui/pulls/1') `
                -Description 'read PR #1' `
                -AllowNotFound `
                -MaxAttempts 3 `
                -BaseDelaySeconds 0
        } | Should -Throw '*HTTP 503*'

        Should -Invoke gh -Times 3 -Exactly
    }

    It 'returns null only for a confirmed HTTP 404 when not-found is allowed' {
        Mock gh {
            $global:LASTEXITCODE = 1
            return 'gh: Not Found (HTTP 404)'
        }

        $result = Invoke-GhCommandWithRetry `
            -Arguments @('api', 'repos/dotnet/maui/pulls/1') `
            -Description 'read PR #1' `
            -AllowNotFound

        $result | Should -BeNullOrEmpty
        Should -Invoke gh -Times 1 -Exactly
        Should -Invoke Start-Sleep -Times 0 -Exactly
    }

    It 'does not retry a permanent authorization failure' {
        Mock gh {
            $global:LASTEXITCODE = 1
            return 'gh: Resource not accessible by integration (HTTP 403)'
        }

        {
            Invoke-GhCommandWithRetry `
                -Arguments @('api', 'repos/dotnet/maui/pulls/1') `
                -Description 'read PR #1'
        } | Should -Throw '*HTTP 403*'

        Should -Invoke gh -Times 1 -Exactly
        Should -Invoke Start-Sleep -Times 0 -Exactly
    }

    It 'retries an HTTP 403 only when GitHub identifies it as rate limiting' {
        Mock gh {
            $script:ghAttempts++
            if ($script:ghAttempts -eq 1) {
                $global:LASTEXITCODE = 1
                return 'gh: API rate limit exceeded (HTTP 403)'
            }

            $global:LASTEXITCODE = 0
            return '{"state":"open"}'
        }

        Invoke-GhCommandWithRetry `
            -Arguments @('api', 'repos/dotnet/maui/pulls/1') `
            -Description 'read PR #1' `
            -RequireOutput |
            Should -Be '{"state":"open"}'

        Should -Invoke gh -Times 2 -Exactly
        Should -Invoke Start-Sleep -Times 1 -Exactly
    }
}
