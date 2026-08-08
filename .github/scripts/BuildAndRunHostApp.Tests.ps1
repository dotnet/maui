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
        $script:UiTestContent | Should -Match 'new MagickGeometry\(\(uint\)width, \(uint\)height\)'
        $script:UiTestContent | Should -Match 'IgnoreAspectRatio = true'
        $script:UiTestContent | Should -Match 'surface\.Resize\(logicalSize\)'
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
