#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for the ambiguous-startup retry decision shared by
    Get-EnvErrorPatterns.ps1 and Invoke-UITestWithRetry.ps1.

    "Timed out waiting for Go To Test button" / "did not recover after crash-recovery
    attempts" are emitted BOTH for a broken emulator and for a PR that deterministically
    breaks HostApp startup. They stay retryable so a real infra flake still recovers, but
    a recurrence AFTER the device reboot + fresh rebuild must be reported as a genuine
    failure instead of consuming the retry budget and landing as INCONCLUSIVE.
.EXAMPLE
    Invoke-Pester ./InvokeUITestWithRetry.Tests.ps1
#>

BeforeAll {
    . (Join-Path $PSScriptRoot 'Get-EnvErrorPatterns.ps1')

    $script:RetryScriptPath = Join-Path $PSScriptRoot 'Invoke-UITestWithRetry.ps1'
    $script:RetryScriptSource = Get-Content -Raw -LiteralPath $script:RetryScriptPath

    # Mirrors the decision made in the Invoke-UITestWithRetry.ps1 retry loop: an ambiguous
    # startup signature that already appeared in this category's history has survived the
    # device reboot + rebuild recovery, so it is deterministic (a real failure), not infra.
    function Test-AmbiguousStartupIsDeterministic {
        param([string]$EnvHit, [string[]]$History)
        $ambiguous = Get-AmbiguousStartupPatterns
        return ([bool]$EnvHit -and ($ambiguous -contains $EnvHit) -and (@($History) -contains $EnvHit))
    }
}

Describe 'Get-AmbiguousStartupPatterns' {
    It 'is a strict subset of the retryable env-error patterns' {
        $env = Get-EnvErrorPatterns
        $ambiguous = Get-AmbiguousStartupPatterns
        $ambiguous.Count | Should -BeGreaterThan 0
        $ambiguous.Count | Should -BeLessThan $env.Count
        foreach ($p in $ambiguous) { $env | Should -Contain $p }
    }

    It 'covers both HostApp startup signatures whose producer text is cause-ambiguous' {
        $ambiguous = Get-AmbiguousStartupPatterns
        $ambiguous | Should -Contain 'did not recover after crash-recovery attempts'
        $ambiguous | Should -Contain 'Timed out waiting for Go To Test button'
    }

    It 'does not mark unambiguous infrastructure signatures as ambiguous' {
        $ambiguous = Get-AmbiguousStartupPatterns
        $ambiguous | Should -Not -Contain 'no devices/emulators found'
        $ambiguous | Should -Not -Contain 'InstallFailedException'
        $ambiguous | Should -Not -Contain 'device offline'
    }
}

Describe 'Ambiguous startup retry decision' {
    It 'allows the first occurrence to retry (one device-recovery attempt)' {
        Test-AmbiguousStartupIsDeterministic -EnvHit 'Timed out waiting for Go To Test button' -History @() |
            Should -BeFalse
    }

    It 'treats a recurrence after recovery as deterministic (PR-caused), not infrastructure' {
        Test-AmbiguousStartupIsDeterministic `
            -EnvHit 'Timed out waiting for Go To Test button' `
            -History @('Timed out waiting for Go To Test button') |
            Should -BeTrue
    }

    It 'keeps retrying when a DIFFERENT ambiguous signature follows the first one' {
        Test-AmbiguousStartupIsDeterministic `
            -EnvHit 'did not recover after crash-recovery attempts' `
            -History @('Timed out waiting for Go To Test button') |
            Should -BeFalse
    }

    It 'never short-circuits an unambiguous infrastructure error, however often it repeats' {
        Test-AmbiguousStartupIsDeterministic `
            -EnvHit 'no devices/emulators found' `
            -History @('no devices/emulators found', 'no devices/emulators found') |
            Should -BeFalse
    }
}

Describe 'Invoke-UITestWithRetry wiring' {
    It 'loads the ambiguous pattern list from the shared single source of truth' {
        $script:RetryScriptSource | Should -Match '\$ambiguousStartupPatterns\s*=\s*Get-AmbiguousStartupPatterns'
    }

    It 'clears EnvErrorHit on a confirmed deterministic startup failure so callers see a real failure' {
        $script:RetryScriptSource | Should -Match '\$ambiguousStartupPatterns\s*-contains\s*\$envHit'
        $script:RetryScriptSource | Should -Match '\$envErrorHistory\s*-contains\s*\$envHit'
    }
}
