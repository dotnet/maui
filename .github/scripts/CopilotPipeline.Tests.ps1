#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $pipelinePath = Join-Path $PSScriptRoot '..' '..' 'eng' 'pipelines' 'ci-copilot.yml' |
        Resolve-Path |
        Select-Object -ExpandProperty Path
    $pipeline = Get-Content -Raw -LiteralPath $pipelinePath

    $script:BaseBranchPatterns = @(
        $pipeline -split '\r?\n' |
            Where-Object { $_ -match 'BASE_REF.*=~ \^' } |
            ForEach-Object {
                if ($_ -notmatch '=~ \^(.+?)\$ \]\]') {
                    throw "Could not extract the base-branch allowlist from: $_"
                }

                "^$($Matches[1])$"
            }
    )
}

Describe 'Copilot reviewer base branch allowlist' {
    It 'keeps every pipeline validator synchronized' {
        $script:BaseBranchPatterns.Count | Should -Be 5
        @($script:BaseBranchPatterns | Select-Object -Unique).Count | Should -Be 1
    }

    It 'accepts every supported base branch shape' {
        $pattern = $script:BaseBranchPatterns[0]

        @(
            'main'
            'net12.0'
            'inflight/current'
            'inflight/10.0.101'
            'inflight/11.0.100-preview.7'
            'release/10.0.1xx-sr10'
            'release/11.0.1xx-preview7.1'
        ) | ForEach-Object {
            [regex]::IsMatch($_, $pattern) | Should -BeTrue -Because "'$_' is supported"
        }
    }

    It 'rejects unsupported or unsafe base branch shapes' {
        $pattern = $script:BaseBranchPatterns[0]

        @(
            'improved-reviewer'
            'inflight/.hidden'
            'inflight/Future'
            'inflight/foo/bar'
            'inflight/../main'
            'refs/heads/inflight/current'
            'release/custom'
        ) | ForEach-Object {
            [regex]::IsMatch($_, $pattern) | Should -BeFalse -Because "'$_' is unsupported or unsafe"
        }
    }
}
