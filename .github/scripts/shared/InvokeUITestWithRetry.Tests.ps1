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
    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile(
        $script:RetryScriptPath,
        [ref]$tokens,
        [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    function Get-FunctionBody {
        param([string]$ScriptText, [string]$FunctionName)
        $start = $ScriptText.IndexOf("function $FunctionName")
        if ($start -lt 0) { throw "Function '$FunctionName' not found" }
        $i = $ScriptText.IndexOf('{', $start)
        $depth = 0
        for (; $i -lt $ScriptText.Length; $i++) {
            if ($ScriptText[$i] -eq '{') { $depth++ }
            elseif ($ScriptText[$i] -eq '}') {
                $depth--
                if ($depth -eq 0) {
                    return $ScriptText.Substring($start, $i - $start + 1)
                }
            }
        }
        throw "Function '$FunctionName' has no closing brace"
    }

    Invoke-Expression (Get-FunctionBody -ScriptText $script:RetryScriptSource -FunctionName 'ConvertTo-AzdoSafeConsole')
    $boundedFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Invoke-BuildScriptBounded'
    }, $true)
    if (-not $boundedFunction) {
        throw "Function 'Invoke-BuildScriptBounded' not found"
    }
    Invoke-Expression $boundedFunction.Extent.Text

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

Describe 'Verified crash/startup retry history' {
    It 'is a strict subset of retryable environment-error patterns' {
        $env = Get-EnvErrorPatterns
        $verified = Get-VerifiedCrashStartupEnvErrorPatterns
        $verified.Count | Should -BeGreaterThan 0
        $verified.Count | Should -BeLessThan $env.Count
        foreach ($p in $verified) { $env | Should -Contain $p }
    }

    It 'keeps install failure followed by timeout in the generic timeout bucket' {
        Test-EnvErrorHistoryHasVerifiedCrashStartup `
            -EnvErrorHistory @('InstallFailedException', 'timeout') |
            Should -BeFalse
    }

    It 'recognizes a verified app-startup failure followed by timeout' {
        Test-EnvErrorHistoryHasVerifiedCrashStartup `
            -EnvErrorHistory @('did not recover after crash-recovery attempts', 'timeout') |
            Should -BeTrue
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

    It 'defangs directive-shaped captured output before Write-Host' {
        ConvertTo-AzdoSafeConsole "safe`r`n##vso[task.setvariable variable=GateFailed]false" |
            Should -Be 'safe ## vso[task.setvariable variable=GateFailed]false'
        ConvertTo-AzdoSafeConsole '##[error]spoof' | Should -Be '## [error]spoof'
        $script:RetryScriptSource | Should -Match ([regex]::Escape('Write-Host (ConvertTo-AzdoSafeConsole $l)'))
        $script:RetryScriptSource | Should -Not -Match 'foreach \(\$l in \$lines\) \{ Write-Host \$l \}'
    }

    It 'preserves a bounded-path test filter containing spaces as one argument' {
        $childScript = Join-Path $TestDrive 'capture-filter.ps1'
        @'
param([string]$TestFilter)
Write-Output "filter=$TestFilter"
if ($TestFilter -ne 'Name = Foo Bar') {
    exit 17
}
'@ | Set-Content -LiteralPath $childScript -Encoding utf8

        $result = Invoke-BuildScriptBounded `
            -ScriptPath $childScript `
            -Params @{ TestFilter = 'Name = Foo Bar' } `
            -TimeoutSeconds 30

        $result.ExitCode | Should -Be 0
        ($result.Output -join "`n") | Should -Match 'filter=Name = Foo Bar'
        $script:RetryScriptSource | Should -Match '\.ArgumentList\.Add\(\[string\]\$argument\)'
        $script:RetryScriptSource | Should -Not -Match 'Start-Process\s+-FilePath\s+\$pwshExe\s+-ArgumentList'
    }
}
