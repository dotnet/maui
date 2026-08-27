#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Copy-BoundedDiagnosticFile.ps1')
    . (Join-Path $PSScriptRoot 'Import-ExpectedPRAgentArtifact.ps1')

    $script:ScratchRoot = Join-Path $PSScriptRoot '../../../artifacts/tests/Import-ExpectedPRAgentArtifact'
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
}

Describe 'Import-ExpectedPRAgentArtifact' {
    BeforeEach {
        $script:CaseRoot = Join-Path $script:ScratchRoot ([Guid]::NewGuid().ToString('N'))
        $script:ArtifactRoot = Join-Path $script:CaseRoot 'CopilotLogs'
        $script:ExpectedPRAgent = Join-Path $script:ArtifactRoot 'CustomAgentLogsTmp/PRState/36473/PRAgent'
        $script:Destination = Join-Path $script:CaseRoot 'bounded-pr-agent'
        New-Item -ItemType Directory -Path $script:ExpectedPRAgent -Force | Out-Null
    }

    It 'imports the unique canonical PRAgent tree through regular-file bounds' {
        $contentDirectory = Join-Path $script:ExpectedPRAgent 'summary'
        New-Item -ItemType Directory -Path $contentDirectory -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $contentDirectory 'content.md') -Value 'review'

        $result = Import-ExpectedPRAgentArtifact `
            -ArtifactRoot $script:ArtifactRoot `
            -PRNumber 36473 `
            -DestinationDirectory $script:Destination

        $result.CopiedFiles | Should -Be 1
        Get-Content -LiteralPath (Join-Path $script:Destination 'summary/content.md') |
            Should -Be 'review'
    }

    It 'rejects a competing PRAgent directory instead of selecting the first match' {
        Set-Content -LiteralPath (Join-Path $script:ExpectedPRAgent 'winner.json') -Value '{}'
        $competing = Join-Path $script:ArtifactRoot '000-attacker/PRAgent'
        New-Item -ItemType Directory -Path $competing -Force | Out-Null
        Set-Content -LiteralPath (Join-Path $competing 'winner.json') -Value '{"isPRFix":false}'

        {
            Import-ExpectedPRAgentArtifact `
                -ArtifactRoot $script:ArtifactRoot `
                -PRNumber 36473 `
                -DestinationDirectory $script:Destination
        } | Should -Throw '*exactly one PRAgent directory*'

        Test-Path -LiteralPath $script:Destination | Should -BeFalse
    }

    It 'tail-truncates an oversized diagnostic log without rejecting review content' {
        $log = Join-Path $script:ExpectedPRAgent 'pr-plus-reviewer/entry-validation.log'
        $patch = Join-Path $script:ExpectedPRAgent 'pr-plus-reviewer/candidate.patch'
        New-Item -ItemType Directory -Path (Split-Path -Parent $log) -Force | Out-Null
        ('begin-' + ('x' * 1024) + '-FINAL-MARKER') |
            Set-Content -LiteralPath $log -NoNewline
        ('patch-' + ('y' * 1024) + '-PATCH-END') |
            Set-Content -LiteralPath $patch -NoNewline
        Set-Content -LiteralPath (Join-Path $script:ExpectedPRAgent 'winner.json') -Value '{}'

        $result = Import-ExpectedPRAgentArtifact `
            -ArtifactRoot $script:ArtifactRoot `
            -PRNumber 36473 `
            -DestinationDirectory $script:Destination `
            -MaxFileBytes 256 `
            -MaxTotalBytes 1024
        $copiedLog = Get-Content -Raw -LiteralPath (
            Join-Path $script:Destination 'pr-plus-reviewer/entry-validation.log')
        $copiedPatch = Get-Content -Raw -LiteralPath (
            Join-Path $script:Destination 'pr-plus-reviewer/candidate.patch')

        $result.CopiedFiles | Should -Be 3
        $result.TruncatedFiles | Should -Be 2
        $copiedLog | Should -Match '^--- Diagnostic log truncated from '
        $copiedLog | Should -Match '-FINAL-MARKER$'
        $copiedPatch | Should -Match '^--- Diagnostic log truncated from '
        $copiedPatch | Should -Match '-PATCH-END$'
        Test-Path -LiteralPath (Join-Path $script:Destination 'winner.json') |
            Should -BeTrue
    }

    It 'rejects a lone PRAgent directory outside the expected PR path' {
        Remove-Item -LiteralPath $script:ExpectedPRAgent -Recurse -Force
        $wrong = Join-Path $script:ArtifactRoot 'CustomAgentLogsTmp/PRState/99999/PRAgent'
        New-Item -ItemType Directory -Path $wrong -Force | Out-Null

        {
            Import-ExpectedPRAgentArtifact `
                -ArtifactRoot $script:ArtifactRoot `
                -PRNumber 36473 `
                -DestinationDirectory $script:Destination
        } | Should -Throw '*not at the expected path*'
    }

    It 'rejects an oversized data file' {
        Set-Content -LiteralPath (Join-Path $script:ExpectedPRAgent 'content.md') -Value ('x' * 1024)

        {
            Import-ExpectedPRAgentArtifact `
                -ArtifactRoot $script:ArtifactRoot `
                -PRNumber 36473 `
                -DestinationDirectory $script:Destination `
                -MaxFileBytes 256
        } | Should -Throw '*per-file limit*'
    }
}

AfterAll {
    Remove-Item -LiteralPath $script:ScratchRoot -Recurse -Force -ErrorAction SilentlyContinue
}
