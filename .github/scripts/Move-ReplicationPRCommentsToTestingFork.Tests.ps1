#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    function Get-ScriptFunctionText {
        param(
            [Parameter(Mandatory = $true)][string]$Path,
            [Parameter(Mandatory = $true)][string]$Name
        )

        $tokens = $null
        $errors = $null
        $ast = [System.Management.Automation.Language.Parser]::ParseFile($Path, [ref]$tokens, [ref]$errors)
        if ($errors) {
            throw ($errors | ForEach-Object Message) -join [Environment]::NewLine
        }
        $function = $ast.Find({
            $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
            $args[0].Name -eq $Name
        }, $true)
        if (-not $function) {
            throw "Function '$Name' was not found in $Path"
        }
        return $function.Extent.Text
    }

    $script:MigrationScript = Join-Path $PSScriptRoot 'shared/Move-ReplicationPRCommentsToTestingFork.ps1'
    $script:MigrationSource = Get-Content -LiteralPath $script:MigrationScript -Raw
    foreach ($name in @(
        'Get-ReplicationCommentSourceMarker',
        'New-ReplicationMigratedCommentBody'
    )) {
        Invoke-Expression (Get-ScriptFunctionText -Path $script:MigrationScript -Name $name)
    }
}

Describe 'Replication PR comment migration' {
    It 'creates stable source markers and attributed comment copies' {
        $comment = [pscustomobject]@{
            id = 5309153462
            user = [pscustomobject]@{ login = 'reviewer' }
            created_at = '2026-08-16T12:34:56Z'
            html_url = 'https://github.com/dotnet/maui/pull/37494#issuecomment-5309153462'
            body = 'The test does not exercise the reported behavior.'
        }

        $body = New-ReplicationMigratedCommentBody -Kind issue -Comment $comment

        $body | Should -Match 'MAUI_COPILOT_MIGRATED_COMMENT kind=issue id=5309153462'
        $body | Should -Match '\*\*@reviewer\*\*'
        $body | Should -Match ([regex]::Escape($comment.html_url))
        $body | Should -Match ([regex]::Escape($comment.body))
    }

    It 'preserves inline review location metadata' {
        $comment = [pscustomobject]@{
            id = 42
            user = [pscustomobject]@{ login = 'reviewer' }
            created_at = '2026-08-16T12:34:56Z'
            html_url = 'https://github.com/dotnet/maui/pull/1#discussion_r42'
            body = 'This assertion is manufactured.'
            path = 'src/Test.cs'
            line = 27
            original_line = 27
        }

        New-ReplicationMigratedCommentBody -Kind inline -Comment $comment |
            Should -Match 'Path: `src/Test\.cs`, line 27'
    }

    It 'requires MauiBot auth, deduplicates markers, and migrates all feedback kinds' {
        $script:MigrationSource | Should -Match "GH_TOKEN must authenticate as 'MauiBot'"
        $script:MigrationSource | Should -Match 'MAUI_COPILOT_MIGRATED_COMMENT'
        $script:MigrationSource | Should -Match "Kind = 'issue'"
        $script:MigrationSource | Should -Match "Kind = 'review'"
        $script:MigrationSource | Should -Match "Kind = 'inline'"
        $script:MigrationSource | Should -Match 'issues/\$TargetPullRequestNumber/comments'
        $script:MigrationSource | Should -Match 'unmatchedSourcePullRequestCount'
        $script:MigrationSource | Should -Match 'no open testing-fork PR has the same replication marker'
    }
}
