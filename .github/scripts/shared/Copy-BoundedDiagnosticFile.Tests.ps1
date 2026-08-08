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

Describe 'Copy-BoundedDiagnosticFileSet' {
    BeforeEach {
        $script:fixture = Join-Path $TestDrive ([guid]::NewGuid().ToString('N'))
        $script:sourceDir = Join-Path $script:fixture 'source'
        $script:destinationDir = Join-Path $script:fixture 'destination'
        New-Item -ItemType Directory -Path $script:sourceDir -Force | Out-Null
    }

    It 'accepts an empty category without producing a manifest' {
        $result = Copy-BoundedDiagnosticFileSet `
            -Files @() `
            -DestinationDirectory $script:destinationDir

        $result.SourceFiles | Should -Be 0
        $result.CopiedFiles | Should -Be 0
        $result.CopiedBytes | Should -Be 0
        $result.ManifestPath | Should -BeNullOrEmpty
    }

    It 'keeps the oldest and newest representative files within the aggregate budget' {
        $baseTime = [datetime]::UtcNow.AddMinutes(-10)
        $files = @(
            foreach ($index in 0..3) {
                $path = Join-Path $script:sourceDir "screen-$index.png"
                [System.IO.File]::WriteAllText(
                    $path,
                    ([char](65 + $index)).ToString() * 300,
                    [System.Text.UTF8Encoding]::new($false))
                (Get-Item -LiteralPath $path).LastWriteTimeUtc = $baseTime.AddSeconds($index)
                Get-Item -LiteralPath $path
            }
        )

        $result = Copy-BoundedDiagnosticFileSet `
            -Files $files `
            -DestinationDirectory $script:destinationDir `
            -MaxTotalBytes 700 `
            -MaxBinaryFileBytes 512 `
            -TextFileNames @()

        $result.CopiedFiles | Should -Be 2
        $result.CopiedBytes | Should -BeLessOrEqual 700
        $result.BudgetFiles | Should -Be 2
        Test-Path -LiteralPath (Join-Path $script:destinationDir 'screen-0.png') | Should -BeTrue
        Test-Path -LiteralPath (Join-Path $script:destinationDir 'screen-3.png') | Should -BeTrue
    }

    It 'stores exact duplicate payloads once and records their test names in the manifest' {
        $first = Join-Path $script:sourceDir 'first.png'
        $second = Join-Path $script:sourceDir 'second.png'
        [System.IO.File]::WriteAllText($first, ('same' * 100), [System.Text.UTF8Encoding]::new($false))
        Copy-Item -LiteralPath $first -Destination $second

        $result = Copy-BoundedDiagnosticFileSet `
            -Files @((Get-Item $first), (Get-Item $second)) `
            -DestinationDirectory $script:destinationDir `
            -MaxTotalBytes 2048 `
            -MaxBinaryFileBytes 1024 `
            -TextFileNames @()

        $result.CopiedFiles | Should -Be 1
        $result.DuplicateFiles | Should -Be 1
        $result.ManifestPath | Should -Not -BeNullOrEmpty
        (Get-Content -LiteralPath $result.ManifestPath)[3] |
            Should -Be "Reason`tSource`tRetained-or-bytes"
        (Get-Content -Raw -LiteralPath $result.ManifestPath) |
            Should -Match "DUPLICATE`t(second|first)\.png`t(first|second)\.png"
    }

    It 'charges bounded text logs against the same aggregate limit' {
        $log = Join-Path $script:sourceDir 'appium.log'
        $screen = Join-Path $script:sourceDir 'screen.png'
        [System.IO.File]::WriteAllText($log, ('log' * 1000), [System.Text.UTF8Encoding]::new($false))
        [System.IO.File]::WriteAllText($screen, ('screen' * 100), [System.Text.UTF8Encoding]::new($false))

        $result = Copy-BoundedDiagnosticFileSet `
            -Files @((Get-Item $log), (Get-Item $screen)) `
            -DestinationDirectory $script:destinationDir `
            -MaxTotalBytes 700 `
            -MaxTextFileBytes 512 `
            -MaxBinaryFileBytes 1024

        $result.CopiedBytes | Should -BeLessOrEqual 700
        $result.TruncatedTextFiles | Should -Be 1
        $result.BudgetFiles | Should -Be 1
        (Get-Item -LiteralPath (Join-Path $script:destinationDir 'appium.log')).Length |
            Should -BeLessOrEqual 512
    }
}
