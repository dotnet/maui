#Requires -Modules Pester

# Focused tests for the Android per-test flaky-retry classification in
# BuildAndRunHostApp.ps1: "Baseline snapshot not yet created" failures are
# brand-new VerifyScreenshot tests (no committed baseline). They are
# deterministic new-baseline results, NOT emulator flake, and must be excluded
# from the flaky-retry set (retrying them wastes a full re-run and can exhaust
# the deep category time budget on snapshot-heavy PRs).
#
# The retry logic is embedded in a large script rather than a callable function,
# so these tests exercise the exact classification predicate used there.

Describe 'Android flaky-retry new-baseline exclusion' {
    BeforeAll {
        $script:BaselineRegex = '(?i)Baseline snapshot not yet created'

        # Mirrors the predicate in BuildAndRunHostApp.ps1: given TRX-style
        # results ({ status; name; error }), return the names to retry
        # (Failed and NOT a new-baseline failure).
        function Get-RetryNames {
            param([object[]]$Results)
            $failed = @($Results | Where-Object { $_.status -eq 'Failed' })
            @($failed |
                Where-Object { ($_.error -as [string]) -notmatch $script:BaselineRegex } |
                ForEach-Object { $_.name })
        }

        function Get-BaselineCount {
            param([object[]]$Results)
            @($Results | Where-Object {
                $_.status -eq 'Failed' -and (($_.error -as [string]) -match $script:BaselineRegex)
            }).Count
        }
    }

    It 'excludes baseline-not-created failures from the retry set' {
        $results = @(
            [pscustomobject]@{ status='Failed'; name='SearchBar_Material3_A'; error='Baseline snapshot not yet created: /snapshots/android/SearchBar_A.png' }
            [pscustomobject]@{ status='Failed'; name='SearchBar_Material3_B'; error='Baseline snapshot not yet created: /snapshots/android/SearchBar_B.png' }
            [pscustomobject]@{ status='Passed'; name='Switch_C'; error='' }
        )
        (Get-RetryNames -Results $results).Count | Should -Be 0
        Get-BaselineCount -Results $results | Should -Be 2
    }

    It 'keeps genuine (non-baseline) flaky failures in the retry set' {
        $results = @(
            [pscustomobject]@{ status='Failed'; name='Flaky_Timeout'; error='System.TimeoutException: element not found' }
            [pscustomobject]@{ status='Failed'; name='New_Snapshot'; error='Baseline snapshot not yet created: /snapshots/android/New.png' }
            [pscustomobject]@{ status='Passed'; name='Ok_Test'; error='' }
        )
        $retry = Get-RetryNames -Results $results
        $retry | Should -Contain 'Flaky_Timeout'
        $retry | Should -Not -Contain 'New_Snapshot'
        $retry.Count | Should -Be 1
        Get-BaselineCount -Results $results | Should -Be 1
    }

    It 'is case-insensitive on the baseline signature' {
        $results = @(
            [pscustomobject]@{ status='Failed'; name='X'; error='BASELINE SNAPSHOT NOT YET CREATED: /p.png' }
        )
        (Get-RetryNames -Results $results).Count | Should -Be 0
        Get-BaselineCount -Results $results | Should -Be 1
    }

    It 'treats null/empty error as a retryable (non-baseline) failure' {
        $results = @(
            [pscustomobject]@{ status='Failed'; name='NoErrText'; error=$null }
        )
        (Get-RetryNames -Results $results) | Should -Contain 'NoErrText'
    }
}

Describe 'MacCatalyst blocking-dialog dismissal' {
    It 'prefers trusted staged scripts before repository fallbacks' {
        $scriptContent = Get-Content (Join-Path $PSScriptRoot 'BuildAndRunHostApp.ps1') -Raw

        foreach ($scriptName in @(
            'dismiss-apple-account-dialog.sh',
            'dismiss-maccatalyst-app-recovery-dialog.sh'
        )) {
            $scriptContent | Should -Match ([regex]::Escape("FileName = `"$scriptName`""))
        }

        $trustedPath = '../eng-scripts/$($dialog.FileName)'
        $fallbackPath = '../../eng/scripts/$($dialog.FileName)'
        $scriptContent.IndexOf($trustedPath) | Should -BeGreaterOrEqual 0
        $scriptContent.IndexOf($trustedPath) | Should -BeLessThan $scriptContent.IndexOf($fallbackPath)
    }

    It 'prevents and dismisses the HostApp reopen-windows recovery alert' {
        $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
        $recoveryScript = Get-Content (Join-Path $repoRoot 'eng/scripts/dismiss-maccatalyst-app-recovery-dialog.sh') -Raw

        $recoveryScript | Should -Match 'com\.microsoft\.maui\.uitests'
        $recoveryScript | Should -Match 'ApplePersistenceIgnoreState'
        $recoveryScript | Should -Match 'NSQuitAlwaysKeepsWindows'
        $recoveryScript | Should -Match 'unexpectedly quit'
        $recoveryScript | Should -Match "Don['’]t Reopen"
        $recoveryScript | Should -Match 'Saved Application State'
        $recoveryScript | Should -Match 'pgrep -f "\$processPattern"'
        $recoveryScript | Should -Match 'ps -p "\$processId" -o command='
        $recoveryScript | Should -Match 'kill "\$processId"'
        $recoveryScript | Should -Not -Match '\b(?:pkill|killall)\b'
    }
}

Describe 'MacCatalyst Retina screenshot cropping' {
    BeforeAll {
        $uiTestPath = Join-Path $PSScriptRoot '..' '..' 'src' 'Controls' 'tests' 'TestCases.Shared.Tests' 'UITest.cs'
        $script:UiTestContent = Get-Content $uiTestPath -Raw
        $script:RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path
    }

    It 'maps logical Appium window bounds to physical screenshot pixels' {
        $script:UiTestContent | Should -Match 'CGDisplayBounds\(CGMainDisplayID\(\)\)'
        $script:UiTestContent | Should -Match 'double scaleX = image\.Width / displayBounds\.Size\.Width'
        $script:UiTestContent | Should -Match 'double scaleY = image\.Height / displayBounds\.Size\.Height'
        $script:UiTestContent | Should -Match 'surface\.Composite\(image, -pixelX, -pixelY, CompositeOperator\.SrcAtop\)'
    }

    It 'normalizes the physical crop back to logical snapshot dimensions' {
        $script:UiTestContent | Should -Match 'var logicalWidth = \(int\)Math\.Round\(width\)'
        $script:UiTestContent | Should -Match 'var logicalHeight = \(int\)Math\.Round\(height\)'
        $script:UiTestContent | Should -Match 'x \+ logicalWidth'
        $script:UiTestContent | Should -Match 'y \+ logicalHeight'
        $script:UiTestContent | Should -Match 'new MagickGeometry\(\(uint\)logicalWidth, \(uint\)logicalHeight\)'
        $script:UiTestContent | Should -Match 'IgnoreAspectRatio = true'
        $script:UiTestContent | Should -Match 'surface\.Resize\(logicalSize\)'
        $script:UiTestContent | Should -Not -Match 'new MagickGeometry\(\(uint\)width, \(uint\)height\)'
        $script:UiTestContent | Should -Not -Match 'int scaleFactor = \(int\)Math\.Round'
    }

    It 'keeps the trusted post-merge source patch synchronized with the harness' {
        $patchPath = Join-Path $script:RepoRoot '.github/patches/catalyst-retina-screenshot.patch'
        Push-Location $script:RepoRoot
        try {
            git apply --reverse --check --whitespace=nowarn -- $patchPath
            $LASTEXITCODE | Should -Be 0
        } finally {
            Pop-Location
        }
    }
}

Describe 'Android retry TRX merging' {
        BeforeAll {
            $script:BuildAndRunHostAppPath = Join-Path $PSScriptRoot 'BuildAndRunHostApp.ps1'
            $script:BuildAndRunHostAppContent = Get-Content -Raw -LiteralPath $script:BuildAndRunHostAppPath

            $tokens = $null
            $parseErrors = $null
            $ast = [System.Management.Automation.Language.Parser]::ParseInput(
                $script:BuildAndRunHostAppContent,
                [ref]$tokens,
                [ref]$parseErrors)
            if ($parseErrors.Count -gt 0) {
                throw "Could not parse BuildAndRunHostApp.ps1: $($parseErrors[0].Message)"
            }

            $functionAst = $ast.Find(
                {
                    param($node)
                    $node -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
                        $node.Name -eq 'Merge-RetryTrxResults'
                },
                $true)
            if (-not $functionAst) {
                throw "Merge-RetryTrxResults was not found."
            }
            Invoke-Expression $functionAst.Extent.Text
        }

        BeforeEach {
            $script:RetryFixtureDir = Join-Path ([IO.Path]::GetTempPath()) "retry-trx-$(New-Guid)"
            New-Item -ItemType Directory -Path $script:RetryFixtureDir -Force | Out-Null
            $script:OriginalTrxPath = Join-Path $script:RetryFixtureDir 'original.trx'
            $script:RetryTrxPath = Join-Path $script:RetryFixtureDir 'retry.trx'
        }

        AfterEach {
            Remove-Item -LiteralPath $script:RetryFixtureDir -Recurse -Force -ErrorAction SilentlyContinue
        }

        It 'writes a coherent completed TRX when all original failures pass on retry' {
            @'
    <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
      <Results>
        <UnitTestResult testName="Case(a)" outcome="Failed" />
        <UnitTestResult testName="Case(b)" outcome="Passed" />
      </Results>
      <ResultSummary outcome="Failed">
        <Counters total="2" executed="2" passed="1" failed="1" notExecuted="0" inconclusive="0" />
        <Output>Test Run Failed.</Output>
      </ResultSummary>
    </TestRun>
'@ | Set-Content -LiteralPath $script:OriginalTrxPath -Encoding UTF8

            @'
    <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
      <Results>
        <UnitTestResult testName="Case(a)" outcome="Passed" />
        <UnitTestResult testName="Case(b)" outcome="Failed" />
      </Results>
      <ResultSummary outcome="Failed">
        <Counters total="2" executed="2" passed="1" failed="1" />
      </ResultSummary>
    </TestRun>
'@ | Set-Content -LiteralPath $script:RetryTrxPath -Encoding UTF8

            $merged = Merge-RetryTrxResults `
                -OriginalTrxPath $script:OriginalTrxPath `
                -RetryTrxPath $script:RetryTrxPath `
                -FailedNames @('Case(a)')

            $merged.Total | Should -Be 2
            $merged.Passed | Should -Be 2
            $merged.Failed | Should -Be 0
            $merged.Replaced | Should -Be 1
            $merged.Outcome | Should -Be 'Completed'

            [xml]$xml = Get-Content -Raw -LiteralPath $script:OriginalTrxPath
            $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
            $ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
            $summary = $xml.SelectSingleNode('//t:ResultSummary', $ns)
            $summary.outcome | Should -Be 'Completed'
            $summary.Counters.total | Should -Be '2'
            $summary.Counters.passed | Should -Be '2'
            $summary.Counters.failed | Should -Be '0'
            $summaryOutput = $summary.SelectSingleNode('t:Output', $ns).InnerText
            $summaryOutput | Should -Match 'Final merged result: Completed'
            $summaryOutput | Should -Not -Match 'Test Run Failed'

            $results = @($xml.SelectNodes('//t:UnitTestResult', $ns))
            ($results | Where-Object testName -eq 'Case(a)').outcome | Should -Be 'Passed'
            ($results | Where-Object testName -eq 'Case(b)').outcome | Should -Be 'Passed'
        }

        It 'keeps the merged TRX failed when an original failure is absent from the retry' {
            @'
    <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
      <Results>
        <UnitTestResult testName="Retried" outcome="Failed" />
        <UnitTestResult testName="NotRetried" outcome="Failed" />
      </Results>
      <ResultSummary outcome="Failed">
        <Counters total="2" executed="2" passed="0" failed="2" />
      </ResultSummary>
    </TestRun>
'@ | Set-Content -LiteralPath $script:OriginalTrxPath -Encoding UTF8

            @'
    <TestRun xmlns="http://microsoft.com/schemas/VisualStudio/TeamTest/2010">
      <Results>
        <UnitTestResult testName="Retried" outcome="Passed" />
      </Results>
      <ResultSummary outcome="Completed">
        <Counters total="1" executed="1" passed="1" failed="0" />
      </ResultSummary>
    </TestRun>
'@ | Set-Content -LiteralPath $script:RetryTrxPath -Encoding UTF8

            $merged = Merge-RetryTrxResults `
                -OriginalTrxPath $script:OriginalTrxPath `
                -RetryTrxPath $script:RetryTrxPath `
                -FailedNames @('Retried', 'NotRetried')

            $merged.Passed | Should -Be 1
            $merged.Failed | Should -Be 1
            $merged.Replaced | Should -Be 1
            $merged.Outcome | Should -Be 'Failed'

            [xml]$xml = Get-Content -Raw -LiteralPath $script:OriginalTrxPath
            $ns = New-Object System.Xml.XmlNamespaceManager($xml.NameTable)
            $ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
            $summary = $xml.SelectSingleNode('//t:ResultSummary', $ns)
            $summary.outcome | Should -Be 'Failed'
            $summary.Counters.failed | Should -Be '1'
            $summary.SelectSingleNode('t:Output', $ns).InnerText | Should -Match 'Final merged result: Failed'
        }

        It 'does not replace a failed full TRX with a retry-only TRX' {
            $script:BuildAndRunHostAppContent | Should -Not -Match ([regex]::Escape('Copy-Item $retryTrxPath $trxFilePath'))
            $script:BuildAndRunHostAppContent | Should -Match 'preserving the original failing result'
            $script:BuildAndRunHostAppContent | Should -Match ([regex]::Escape('if ($merged.Failed -eq 0)'))
        }
}
