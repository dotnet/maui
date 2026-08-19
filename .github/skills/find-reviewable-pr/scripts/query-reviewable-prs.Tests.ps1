#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'query-reviewable-prs.ps1'
}

AfterAll {
    Remove-Item Function:\global:gh -ErrorAction SilentlyContinue
    Remove-Variable ReviewableGhCalls, ReviewableMockPrJson -Scope Global -ErrorAction SilentlyContinue
}

Describe 'query-reviewable-prs credential-free contract' {
    BeforeEach {
        $global:ReviewableGhCalls = [System.Collections.Generic.List[string]]::new()
        $global:ReviewableMockPrJson = @(
            [pscustomobject]@{
                number         = 42
                title          = 'Mocked contract PR'
                labels         = @([pscustomobject]@{ name = 'platform/android' })
                createdAt      = '2026-08-18T00:00:00Z'
                updatedAt      = '2026-08-19T00:00:00Z'
                isDraft        = $false
                author         = [pscustomobject]@{ login = 'mock-contributor' }
                assignees      = @()
                additions      = 10
                deletions      = 2
                changedFiles   = 1
                milestone      = $null
                url            = 'https://github.com/dotnet/maui/pull/42'
                reviewDecision = 'REVIEW_REQUIRED'
                reviews        = @()
                comments       = @()
                projectItems   = @()
            }
        ) | ConvertTo-Json -Depth 10 -Compress

        function global:gh {
            param(
                [Parameter(ValueFromRemainingArguments = $true)]
                [string[]]$GhArgs
            )

            $command = $GhArgs -join ' '
            [void]$global:ReviewableGhCalls.Add($command)
            $global:LASTEXITCODE = 0

            if ($command -match '^pr list --repo dotnet/maui --state open --limit 1 --json projectItems$') {
                return '[]'
            }
            if ($command -match '^api graphql ') {
                return '{"data":{"node":{"items":{"pageInfo":{"hasNextPage":false,"endCursor":null},"nodes":[]}}}}'
            }
            if ($command -match '^api repos/dotnet/maui/milestones ') {
                return ''
            }
            if ($command -match '^pr list --repo dotnet/docs-maui ') {
                return '[]'
            }
            if ($command -match '^pr list --repo dotnet/maui ') {
                return $global:ReviewableMockPrJson
            }

            throw "Unexpected mocked gh call: $command"
        }

        Remove-Item Env:\GH_TOKEN -ErrorAction SilentlyContinue
        Remove-Item Env:\GITHUB_TOKEN -ErrorAction SilentlyContinue
    }

    It 'queries the expected repositories and emits the markdown queue schema' {
        $output = & $scriptPath -Category all -OutputFormat markdown -Limit 5 6>&1 | Out-String

        $global:ReviewableGhCalls | Should -Contain 'pr list --repo dotnet/maui --state open --limit 1 --json projectItems'
        @($global:ReviewableGhCalls | Where-Object {
            $_ -match '^pr list --repo dotnet/maui --state open --search ' -and
            $_ -match '--json number title labels createdAt updatedAt isDraft author assignees additions deletions changedFiles milestone url reviewDecision reviews comments( projectItems)?$'
        }).Count | Should -BeGreaterThan 0
        @($global:ReviewableGhCalls | Where-Object {
            $_ -match '^pr list --repo dotnet/docs-maui --state open '
        }).Count | Should -Be 1

        $output | Should -Match '(?m)^<!-- PR_REVIEW_QUEUE_BEGIN -->\r?$'
        $output | Should -Match '(?m)^# PR Review Queue'
        $output | Should -Match '(?m)^## Actionability Summary\r?$'
        $output | Should -Match '\[#42\]\(https://github\.com/dotnet/maui/pull/42\)'
        $output | Should -Match 'Mocked contract PR'
        $output | Should -Match '(?m)^## Queue Health\r?$'
    }
}
