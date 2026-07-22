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
            'Get-SafeAssetSlug',
            'Invoke-GhApiJson',
            'Test-PngFile',
            'Test-AzDoAttachmentUrl',
            'Get-SnapshotRoot',
            'Get-SnapshotCandidatePaths',
            'Get-VisualEvidenceDedupKey',
            'Invoke-DownloadFile'
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

Describe 'Visual evidence deduplication' {
    It 'keeps same snapshot failures distinct across runtime environments' {
        $ios26 = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $ios16 = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-iphonex'
            buildId = 100
            runId = 201
            resultId = 301
        }

        Get-VisualEvidenceDedupKey -Evidence $ios26 |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $ios16)
    }

    It 'collapses retry attempts of the same snapshot in the same environment to one key' {
        $firstAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $retryAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
            buildId = 101
            runId = 205
            resultId = 999
        }

        Get-VisualEvidenceDedupKey -Evidence $firstAttempt |
            Should -Be (Get-VisualEvidenceDedupKey -Evidence $retryAttempt)
    }

    It 'keeps same snapshot failures distinct across platforms' {
        $ios = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
        }
        $android = [pscustomobject]@{
            platform = 'android'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = 'ios-26'
        }

        Get-VisualEvidenceDedupKey -Evidence $ios |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $android)
    }

    It 'keeps distinct legs separate when the environment is unresolved (multi-hint build)' {
        # The gatherer sets environmentName to null when a build exposes multiple environment
        # hints for one platform. Two distinct iOS legs failing the same snapshot must not
        # collapse onto "ios|name.png|" as if one were a retry of the other.
        $legA = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            buildId = 100
            runId = 200
            resultId = 300
        }
        $legB = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            buildId = 100
            runId = 201
            resultId = 301
        }

        Get-VisualEvidenceDedupKey -Evidence $legA |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $legB)
    }

    It 'collapses same-leg retry attempts when the environment is unresolved but the run name is stable' {
        # A retry re-runs the same pipeline job/leg and reuses its test-run name, so even though the
        # environment could not be resolved (multi-hint build) the two attempts share a stable leg
        # identity and must collapse to one key -- otherwise each retry consumes a MaxComparisons slot
        # and crowds out genuinely distinct failures.
        $firstAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-26)'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $retryAttempt = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-26)'
            buildId = 101
            runId = 205
            resultId = 999
        }

        Get-VisualEvidenceDedupKey -Evidence $firstAttempt |
            Should -Be (Get-VisualEvidenceDedupKey -Evidence $retryAttempt)
    }

    It 'keeps distinct legs separate when the environment is unresolved but the run names differ' {
        # Distinct legs (e.g. two iOS device queues) run under different pipeline jobs, so their
        # test-run names differ even when the environment cannot be resolved. They must stay separate.
        $legA = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-26)'
            buildId = 100
            runId = 200
            resultId = 300
        }
        $legB = [pscustomobject]@{
            platform = 'ios'
            snapshotFileName = 'Controls.Sample.png'
            environmentName = $null
            runName = 'TestCases.iOS.Tests (ios-iphonex)'
            buildId = 100
            runId = 200
            resultId = 300
        }

        Get-VisualEvidenceDedupKey -Evidence $legA |
            Should -Not -Be (Get-VisualEvidenceDedupKey -Evidence $legB)
    }
}

Describe 'Download budget enforcement' {
    It 'throws without attempting a network call when the publish deadline has already passed' {
        # F2: the aggregate publish budget must gate each download. A deadline in the past has to
        # fail fast -- before any HttpClient work -- so a stalled host cannot hold the Helix job
        # open past its ceiling. The guard sits ahead of the try/catch, so it surfaces the budget
        # message rather than a swallowed connection error.
        $destination = Join-Path ([System.IO.Path]::GetTempPath()) ([System.IO.Path]::GetRandomFileName())
        {
            Invoke-DownloadFile `
                -Url 'https://dev.azure.com/should-not-be-contacted.png' `
                -Path $destination `
                -MaximumBytes 1048576 `
                -Deadline ((Get-Date).AddSeconds(-1))
        } | Should -Throw '*Publish budget exhausted*'

        Test-Path -LiteralPath $destination | Should -BeFalse
    }
}
