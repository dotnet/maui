#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Copy-BoundedDiagnosticFile.ps1')
    . (Join-Path $PSScriptRoot 'Export-ExpectedPRAgentArtifact.ps1')
    . (Join-Path $PSScriptRoot 'Import-ExpectedPRAgentArtifact.ps1')
}

Describe 'Export-ExpectedPRAgentArtifact' {
    BeforeEach {
        $script:RepositoryRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $script:PRAgentRoot = Join-Path `
            $script:RepositoryRoot `
            'CustomAgentLogsTmp/PRState/38022/PRAgent'
        $script:DiagnosticsRoot = Join-Path $script:RepositoryRoot 'diagnostics'
        $script:TokenUsageRoot = Join-Path $script:RepositoryRoot 'token-usage'
        $script:DestinationRoot = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:PRAgentRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $script:DiagnosticsRoot -Force | Out-Null
        New-Item -ItemType Directory -Path $script:TokenUsageRoot -Force | Out-Null
    }

    It 'exports canonical files and ignores a polluted try-fix workspace' {
        $gate = Join-Path $script:PRAgentRoot 'gate'
        $report = Join-Path $script:PRAgentRoot 'report'
        $prPlus = Join-Path $script:PRAgentRoot 'pr-plus-reviewer'
        $polluted = Join-Path $script:PRAgentRoot 'try-fix/attempt-1'
        New-Item -ItemType Directory -Path $gate, $report, $prPlus, $polluted -Force |
            Out-Null
        'gate' | Set-Content -LiteralPath (Join-Path $gate 'content.md') -NoNewline
        'PASSED' | Set-Content -LiteralPath (Join-Path $gate 'gate-result.txt') -NoNewline
        'report' | Set-Content -LiteralPath (Join-Path $report 'content.md') -NoNewline
        '{}' | Set-Content -LiteralPath (Join-Path $script:PRAgentRoot 'winner.json') -NoNewline
        'validation' | Set-Content -LiteralPath (Join-Path $prPlus 'validation.log') -NoNewline
        'transcript' | Set-Content `
            -LiteralPath (Join-Path $script:DiagnosticsRoot 'copilot_review_output_CopilotReview.md') `
            -NoNewline
        '{}' | Set-Content `
            -LiteralPath (Join-Path $script:TokenUsageRoot 'copilot-token-usage-review.json') `
            -NoNewline

        1..1025 | ForEach-Object {
            New-Item -ItemType Directory -Path (Join-Path $polluted "nested-$_") | Out-Null
        }
        'do not publish' | Set-Content -LiteralPath (Join-Path $polluted 'workspace.txt')

        $result = Export-ExpectedPRAgentArtifact `
            -RepositoryRoot $script:RepositoryRoot `
            -PRNumber 38022 `
            -DestinationRoot $script:DestinationRoot `
            -DiagnosticsRoot $script:DiagnosticsRoot `
            -TokenUsageRoot $script:TokenUsageRoot
        $importDestination = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $importResult = Import-ExpectedPRAgentArtifact `
            -ArtifactRoot $script:DestinationRoot `
            -PRNumber 38022 `
            -DestinationDirectory $importDestination

        $result.CopiedFiles | Should -Be 7
        $importResult.CopiedFiles | Should -Be 5
        Test-Path -LiteralPath (
            Join-Path $script:DestinationRoot 'CustomAgentLogsTmp/PRState/38022/PRAgent/try-fix/attempt-1'
        ) | Should -BeFalse
        Get-Content -Raw -LiteralPath (Join-Path $importDestination 'report/content.md') |
            Should -BeExactly 'report'
        Test-Path -LiteralPath (
            Join-Path $script:DestinationRoot 'copilot_review_output_CopilotReview.md'
        ) | Should -BeTrue
        Test-Path -LiteralPath (
            Join-Path $script:DestinationRoot 'copilot-token-usage/raw/copilot-token-usage-review.json'
        ) | Should -BeTrue
    }

    It 'rejects a reparse-point parent before creating the artifact' {
        $outside = Join-Path $script:RepositoryRoot 'outside'
        New-Item -ItemType Directory -Path $outside -Force | Out-Null
        'report' | Set-Content -LiteralPath (Join-Path $outside 'content.md') -NoNewline
        New-Item `
            -ItemType SymbolicLink `
            -Path (Join-Path $script:PRAgentRoot 'report') `
            -Target $outside |
            Out-Null

        {
            Export-ExpectedPRAgentArtifact `
                -RepositoryRoot $script:RepositoryRoot `
                -PRNumber 38022 `
                -DestinationRoot $script:DestinationRoot
        } | Should -Throw '*unsupported reparse point*'

        Test-Path -LiteralPath $script:DestinationRoot | Should -BeFalse
    }

    It 'rejects an oversized structured file before creating the artifact' {
        ('x' * 300) |
            Set-Content -LiteralPath (Join-Path $script:PRAgentRoot 'winner.json') -NoNewline

        {
            Export-ExpectedPRAgentArtifact `
                -RepositoryRoot $script:RepositoryRoot `
                -PRNumber 38022 `
                -DestinationRoot $script:DestinationRoot `
                -MaxFileBytes 256 `
                -MaxTotalBytes 1024
        } | Should -Throw '*winner.json*256-byte per-file limit*'

        Test-Path -LiteralPath $script:DestinationRoot | Should -BeFalse
    }

    It 'fails closed when the allowlisted file-count limit is exceeded' {
        $gate = Join-Path $script:PRAgentRoot 'gate'
        New-Item -ItemType Directory -Path $gate -Force | Out-Null
        'gate' | Set-Content -LiteralPath (Join-Path $gate 'content.md') -NoNewline
        '{}' | Set-Content -LiteralPath (Join-Path $script:PRAgentRoot 'winner.json') -NoNewline

        {
            Export-ExpectedPRAgentArtifact `
                -RepositoryRoot $script:RepositoryRoot `
                -PRNumber 38022 `
                -DestinationRoot $script:DestinationRoot `
                -MaxFileCount 1
        } | Should -Throw '*1-file limit*'

        Test-Path -LiteralPath $script:DestinationRoot | Should -BeFalse
    }

    It 'fails closed when allowlisted files exceed the aggregate limit' {
        $gate = Join-Path $script:PRAgentRoot 'gate'
        New-Item -ItemType Directory -Path $gate -Force | Out-Null
        ('a' * 200) |
            Set-Content -LiteralPath (Join-Path $gate 'content.md') -NoNewline
        ('b' * 200) |
            Set-Content -LiteralPath (Join-Path $script:PRAgentRoot 'winner.json') -NoNewline

        {
            Export-ExpectedPRAgentArtifact `
                -RepositoryRoot $script:RepositoryRoot `
                -PRNumber 38022 `
                -DestinationRoot $script:DestinationRoot `
                -MaxFileBytes 256 `
                -MaxTotalBytes 300
        } | Should -Throw '*300-byte aggregate limit*'

        Test-Path -LiteralPath $script:DestinationRoot | Should -BeFalse
    }

    It 'tail-truncates only an allowlisted diagnostic file' {
        $prPlus = Join-Path $script:PRAgentRoot 'pr-plus-reviewer'
        New-Item -ItemType Directory -Path $prPlus -Force | Out-Null
        ('begin-' + ('x' * 1024) + '-FINAL-MARKER') |
            Set-Content -LiteralPath (Join-Path $prPlus 'validation.log') -NoNewline

        $result = Export-ExpectedPRAgentArtifact `
            -RepositoryRoot $script:RepositoryRoot `
            -PRNumber 38022 `
            -DestinationRoot $script:DestinationRoot `
            -MaxFileBytes 256 `
            -MaxTotalBytes 1024
        $copied = Get-Content -Raw -LiteralPath (
            Join-Path `
                $script:DestinationRoot `
                'CustomAgentLogsTmp/PRState/38022/PRAgent/pr-plus-reviewer/validation.log')

        $result.CopiedFiles | Should -Be 1
        $result.TruncatedFiles | Should -Be 1
        $copied | Should -Match '^--- Diagnostic log truncated from '
        $copied | Should -Match '-FINAL-MARKER$'
    }
}
