#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Query-RerunReadyPRs.ps1'
    $outputDir = Join-Path $PSScriptRoot '../../CustomAgentLogsTmp/QueryRerunReadyTests'
    New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

    . $scriptPath -Owner 'test-owner' -Repo 'test-repo'

    function ConvertTo-GhLines {
        param([object[]]$Items)
        return @($Items | ForEach-Object { $_ | ConvertTo-Json -Depth 10 -Compress })
    }
}

AfterAll {
    Remove-Item -LiteralPath $outputDir -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Query-RerunReadyPRs' {
    BeforeEach {
        $script:OutputPath = Join-Path $outputDir "$([Guid]::NewGuid().ToString('N')).json"
        $script:issueComments = @(
            [pscustomobject]@{
                id = 100
                body = "<!-- AI Summary -->`n<!-- SESSION:1111111 START -->"
                created_at = '2026-05-31T09:00:00Z'
                updated_at = '2026-05-31T09:00:00Z'
                user = [pscustomobject]@{ login = 'MauiBot'; type = 'User' }
                author_association = 'MEMBER'
            },
            [pscustomobject]@{
                id = 200
                body = '/review rerun'
                created_at = '2026-05-31T10:00:00Z'
                updated_at = '2026-05-31T10:00:00Z'
                user = [pscustomobject]@{ login = 'maintainer'; type = 'User' }
                author_association = 'MEMBER'
            }
        )

        Mock Get-LatestReviewCommandOptions {
            [pscustomobject]@{
                Platform = ''
                PipelineRef = 'main'
                CommentId = 200
                Body = '/review rerun'
            }
        }

        Mock gh {
            param(
                [Parameter(ValueFromRemainingArguments = $true)]
                [string[]]$GhArgs
            )

            $command = $GhArgs -join ' '
            $global:LASTEXITCODE = 0

            if ($command -match '^pr list ') {
                return ([pscustomobject]@{
                    number = 1
                    title = 'Autonomously queued PR'
                    url = 'https://example.test/1'
                    headRefOid = '2222222abcdef'
                    isDraft = $false
                    labels = @([pscustomobject]@{ name = 's/agent-ready-for-rerun' })
                    author = [pscustomobject]@{ login = 'dev-user' }
                } | ConvertTo-Json -Depth 10 -Compress)
            }
            if ($command -match '/issues/1/labels') {
                return 's/agent-ready-for-rerun'
            }
            if ($command -match '/issues/1/comments') {
                return ConvertTo-GhLines $script:issueComments
            }
            if ($command -match '/pulls/1/(reviews|comments|commits)') {
                return @()
            }

            throw "Unexpected gh call: $command"
        }
    }

    It 'does not reuse a historical rerun command as the current queue reaction target' {
        Invoke-RerunReadyPRQuery `
            -QueryMaxPRs 5 `
            -QueryOwner 'test-owner' `
            -QueryRepo 'test-repo' `
            -QueryOutputPath $script:OutputPath | Out-Null

        $result = Get-Content -Raw -LiteralPath $script:OutputPath | ConvertFrom-Json
        $result.candidates.Count | Should -Be 1
        $result.candidates[0].reviewCommandId | Should -Be 200
        $result.candidates[0].rerunCommentId | Should -Be 0
        $result.candidates[0].activityCheckpoint | Should -BeGreaterThan 0
    }
}
