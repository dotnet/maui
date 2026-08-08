#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    . (Join-Path $PSScriptRoot 'Copy-BoundedDiagnosticFile.ps1')
}

Describe 'Copy-BoundedDiagnosticFile' {
    BeforeEach {
        $script:fixture = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        New-Item -ItemType Directory -Path $script:fixture -Force | Out-Null
    }

    It 'copies a small diagnostic file without changing it' {
        $source = Join-Path $script:fixture 'small.log'
        $destination = Join-Path $script:fixture 'out/small.log'
        "first`nlast`n" | Set-Content -LiteralPath $source -Encoding UTF8 -NoNewline

        $result = Copy-BoundedDiagnosticFile -Source $source -Destination $destination -MaxBytes 512

        $result.Truncated | Should -BeFalse
        (Get-Content -Raw -LiteralPath $destination) | Should -BeExactly (Get-Content -Raw -LiteralPath $source)
    }

    It 'preserves the final bytes and stays within the artifact limit' {
        $source = Join-Path $script:fixture 'large.log'
        $destination = Join-Path $script:fixture 'large-copy.log'
        $content = ('begin-' + ('x' * 4000) + '-FINAL-MARKER')
        [System.IO.File]::WriteAllText($source, $content, [System.Text.UTF8Encoding]::new($false))

        $result = Copy-BoundedDiagnosticFile -Source $source -Destination $destination -MaxBytes 1024
        $copied = Get-Content -Raw -LiteralPath $destination

        $result.Truncated | Should -BeTrue
        $result.SourceBytes | Should -BeGreaterThan 1024
        $result.CopiedBytes | Should -BeLessOrEqual 1024
        $copied | Should -Match '^--- Diagnostic log truncated from '
        $copied | Should -Match '-FINAL-MARKER$'
        $copied | Should -Not -Match 'begin-'
    }

    It 'rejects reparse-point sources instead of copying through them' {
        $target = Join-Path $script:fixture 'target.log'
        $source = Join-Path $script:fixture 'link.log'
        $destination = Join-Path $script:fixture 'copied.log'
        'secret' | Set-Content -LiteralPath $target -Encoding UTF8
        New-Item -ItemType SymbolicLink -Path $source -Target $target | Out-Null

        { Copy-BoundedDiagnosticFile -Source $source -Destination $destination -MaxBytes 512 } |
            Should -Throw '*must not be a reparse point*'
        Test-Path -LiteralPath $destination | Should -BeFalse
    }
}
