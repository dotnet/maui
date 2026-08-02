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
