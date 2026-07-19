#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Publish-TestVisualAssets.ps1'
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    foreach ($functionName in @(
            'Escape-Html',
            'Get-SafeAssetSlug',
            'Invoke-GhApiJson',
            'Test-PngFile',
            'Test-AzDoAttachmentUrl',
            'Get-SnapshotRoot',
            'Get-SnapshotCandidatePaths',
            'New-VisualComparisonsMarkdown'
        )) {
        $function = $ast.Find({
                $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                $args[0].Name -eq $functionName
            }, $true)
        if (-not $function) { throw "Function '$functionName' not found in $scriptPath" }
        Invoke-Expression $function.Extent.Text
    }
}

Describe 'Visual asset input validation' {
    It 'accepts only the exact public AzDO attachment URL for the expected result' {
        Test-AzDoAttachmentUrl `
            -Url 'https://dev.azure.com/dnceng-public/public/_apis/test/Runs/12/Results/34/Attachments/56' `
            -RunId 12 `
            -ResultId 34 `
            -AttachmentId 56 | Should -BeTrue

        Test-AzDoAttachmentUrl `
            -Url 'https://evil.example/dnceng-public/public/_apis/test/Runs/12/Results/34/Attachments/56' `
            -RunId 12 `
            -ResultId 34 `
            -AttachmentId 56 | Should -BeFalse

        Test-AzDoAttachmentUrl `
            -Url 'https://dev.azure.com/dnceng-public/public/_apis/test/Runs/12/Results/99/Attachments/56' `
            -RunId 12 `
            -ResultId 34 `
            -AttachmentId 56 | Should -BeFalse
    }

    It 'validates the PNG signature and size bound without decoding untrusted image data' {
        $valid = Join-Path $TestDrive 'valid.png'
        [System.IO.File]::WriteAllBytes(
            $valid,
            [Convert]::FromBase64String('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M/wHwAF/gL+XyO8WQAAAABJRU5ErkJggg=='))
        Test-PngFile -Path $valid -MaximumBytes 1024 | Should -BeTrue

        $invalid = Join-Path $TestDrive 'invalid.png'
        [System.IO.File]::WriteAllText($invalid, '<html>not an image</html>')
        Test-PngFile -Path $invalid -MaximumBytes 1024 | Should -BeFalse
        Test-PngFile -Path $valid -MaximumBytes 8 | Should -BeFalse

        $oversizedDimensions = Join-Path $TestDrive 'oversized-dimensions.png'
        $bytes = [System.IO.File]::ReadAllBytes($valid)
        $bytes[16] = 0x00
        $bytes[17] = 0x01
        $bytes[18] = 0x00
        $bytes[19] = 0x01
        [System.IO.File]::WriteAllBytes($oversizedDimensions, $bytes)
        Test-PngFile -Path $oversizedDimensions -MaximumBytes 1024 | Should -BeFalse
    }

    It 'normalizes untrusted names into bounded asset slugs' {
        Get-SafeAssetSlug -Value '../My <Snapshot> Name?!' | Should -Be 'my-snapshot-name'
        (Get-SafeAssetSlug -Value ('A' * 200)).Length | Should -BeLessOrEqual 72
    }
}

Describe 'Snapshot baseline candidates' {
    It 'puts the runtime environment and trusted path hint before fallback directories' {
        $root = Join-Path $TestDrive 'repo'
        $snapshotRoot = Join-Path $root 'src/Controls/tests/TestCases.iOS.Tests/snapshots'
        New-Item -ItemType Directory -Force -Path (Join-Path $snapshotRoot 'ios') | Out-Null
        New-Item -ItemType Directory -Force -Path (Join-Path $snapshotRoot 'ios-26') | Out-Null

        $paths = @(Get-SnapshotCandidatePaths `
            -Platform 'ios' `
            -SnapshotFileName 'Sample.png' `
            -EnvironmentName 'ios-26' `
            -BaselinePathHint 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/Sample.png' `
            -RepositoryRoot $root)

        $paths[0] | Should -Be 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26/Sample.png'
        $paths | Should -Contain 'src/Controls/tests/TestCases.iOS.Tests/snapshots/ios/Sample.png'
        $paths.Count | Should -Be 2
    }

    It 'rejects unsafe filenames and path hints' {
        @(Get-SnapshotCandidatePaths `
            -Platform 'ios' `
            -SnapshotFileName '../payload.png' `
            -EnvironmentName 'ios' `
            -BaselinePathHint '../../payload.png' `
            -RepositoryRoot $TestDrive).Count | Should -Be 0
    }
}

Describe 'Visual comparison markdown' {
    It 'renders escaped, collapsed three-column comparison panels' {
        $markdown = New-VisualComparisonsMarkdown -Comparisons @(
            [pscustomobject]@{
                testName = '<script>alert(1)</script>'
                platform = 'iOS'
                description = '2.08% difference'
                buildId = 123
                buildUrl = 'https://dev.azure.com/example'
                baselineUrl = 'https://raw.githubusercontent.com/org/repo/sha/base.png'
                baselineStatus = 'resolved'
                actualUrl = 'https://raw.githubusercontent.com/org/repo/sha/actual.png'
                diffUrl = 'https://raw.githubusercontent.com/org/repo/sha/diff.png'
            }
        ) -OmittedCount 0 -MaximumCharacters 5000

        $markdown | Should -Match '<details>'
        $markdown | Should -Not -Match '<details open'
        $markdown | Should -Match '<th>CI baseline</th><th>Fresh PR actual</th><th>CI diff</th>'
        $markdown | Should -Match '&lt;script&gt;alert\(1\)&lt;/script&gt;'
        $markdown | Should -Not -Match '<script>'
    }

    It 'bounds complete panels instead of truncating HTML mid-panel' {
        $items = @(1..8 | ForEach-Object {
            [pscustomobject]@{
                testName = "VeryLongSnapshotName$_" + ('x' * 80)
                platform = 'windows'
                description = '1.00% difference'
                buildId = 123
                buildUrl = 'https://dev.azure.com/example'
                baselineUrl = "https://raw.githubusercontent.com/org/repo/sha/base$_.png"
                baselineStatus = 'resolved'
                actualUrl = "https://raw.githubusercontent.com/org/repo/sha/actual$_.png"
                diffUrl = "https://raw.githubusercontent.com/org/repo/sha/diff$_.png"
            }
        })

        $markdown = New-VisualComparisonsMarkdown -Comparisons $items -OmittedCount 0 -MaximumCharacters 1800
        $markdown.Length | Should -BeLessOrEqual 1800
        $markdown | Should -Match 'additional comparison\(s\) were omitted'
        ([regex]::Matches($markdown, '<details>').Count) | Should -Be ([regex]::Matches($markdown, '</details>').Count)
    }
}
