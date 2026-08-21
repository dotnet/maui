#!/usr/bin/env pwsh
#Requires -Modules Pester
<#
.SYNOPSIS
    Pester tests for the ambiguous-startup retry decision shared by
    Get-EnvErrorPatterns.ps1 and Invoke-UITestWithRetry.ps1.

    "Timed out waiting for Go To Test button to appear" / "did not recover after crash-recovery
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
    $script:ResetScriptPath = Join-Path $PSScriptRoot 'Reset-DeviceState.ps1'
    $script:ResetScriptSource = Get-Content -Raw -LiteralPath $script:ResetScriptPath
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
    $script:BoundedFunctionSource = $boundedFunction.Extent.Text
    $saveDiagnosticsFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Save-AndroidHangDiagnostics'
    }, $true)
    if (-not $saveDiagnosticsFunction) {
        throw "Function 'Save-AndroidHangDiagnostics' not found"
    }
    $script:SaveDiagnosticsFunctionSource = $saveDiagnosticsFunction.Extent.Text
    Invoke-Expression $boundedFunction.Extent.Text

    # Mirrors the decision made in the Invoke-UITestWithRetry.ps1 retry loop: an ambiguous
    # startup signature that already appeared in this category's history has survived the
    # device reboot + rebuild recovery, so it is deterministic (a real failure), not infra.
    function Test-AmbiguousStartupIsDeterministic {
        param([string]$EnvHit, [string[]]$History, [bool]$RecoveryVerified)
        $ambiguous = Get-AmbiguousStartupPatterns
        return ($RecoveryVerified -and [bool]$EnvHit -and ($ambiguous -contains $EnvHit) -and (@($History) -contains $EnvHit))
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
        $ambiguous | Should -Contain 'Timed out waiting for Go To Test button to appear'
    }

    It 'does not classify a normal page-navigation disappearance timeout as startup failure' {
        $startupMessage = 'Timed out waiting for Go To Test button to appear'
        $navigationMessage = 'Timed out waiting for Go To Test button to disappear'

        @(Get-EnvErrorPatterns | Where-Object { $startupMessage -match $_ }).Count |
            Should -BeGreaterThan 0
        @(Get-AmbiguousStartupPatterns | Where-Object { $startupMessage -match $_ }).Count |
            Should -BeGreaterThan 0
        @(Get-EnvErrorPatterns | Where-Object { $navigationMessage -match $_ }).Count |
            Should -Be 0
        @(Get-AmbiguousStartupPatterns | Where-Object { $navigationMessage -match $_ }).Count |
            Should -Be 0
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
        Test-AmbiguousStartupIsDeterministic -EnvHit 'Timed out waiting for Go To Test button to appear' -History @() -RecoveryVerified $false |
            Should -BeFalse
    }

    It 'treats a recurrence after verified recovery as deterministic (PR-caused), not infrastructure' {
        Test-AmbiguousStartupIsDeterministic `
            -EnvHit 'Timed out waiting for Go To Test button to appear' `
            -History @('Timed out waiting for Go To Test button to appear') `
            -RecoveryVerified $true |
            Should -BeTrue
    }

    It 'preserves the remaining retry when recovery was not verified' {
        Test-AmbiguousStartupIsDeterministic `
            -EnvHit 'Timed out waiting for Go To Test button to appear' `
            -History @('Timed out waiting for Go To Test button to appear') `
            -RecoveryVerified $false |
            Should -BeFalse
    }

    It 'keeps retrying when a DIFFERENT ambiguous signature follows the first one' {
        Test-AmbiguousStartupIsDeterministic `
            -EnvHit 'did not recover after crash-recovery attempts' `
            -History @('Timed out waiting for Go To Test button to appear') `
            -RecoveryVerified $true |
            Should -BeFalse
    }

    It 'never short-circuits an unambiguous infrastructure error, however often it repeats' {
        Test-AmbiguousStartupIsDeterministic `
            -EnvHit 'no devices/emulators found' `
            -History @('no devices/emulators found', 'no devices/emulators found') `
            -RecoveryVerified $true |
            Should -BeFalse
    }
}

Describe 'Invoke-UITestWithRetry wiring' {
    It 'loads the ambiguous pattern list from the shared single source of truth' {
        $script:RetryScriptSource | Should -Match '\$ambiguousStartupPatterns\s*=\s*Get-AmbiguousStartupPatterns'
    }

    It 'clears EnvErrorHit on a confirmed deterministic startup failure so callers see a real failure' {
        $script:RetryScriptSource | Should -Match '\$recoveryVerified\s+-and\s+\$envHit'
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

    It 'tree-kills the hung run before bounded Android diagnostic capture' {
        $killIndex = $script:BoundedFunctionSource.IndexOf('Stop-ProcessTree -ProcessId $proc.Id')
        $captureIndex = $script:BoundedFunctionSource.IndexOf('Save-AndroidHangDiagnostics -RepoRoot $RepoRoot -Attempt $Attempt')

        $killIndex | Should -BeGreaterThan -1
        $captureIndex | Should -BeGreaterThan $killIndex
        $script:SaveDiagnosticsFunctionSource | Should -Match 'Invoke-AndroidDiagnosticCommand'
        $script:SaveDiagnosticsFunctionSource | Should -Not -Match '(?m)&\s*adb\b'
    }

    It 'retains hang diagnostics in an attempt-numbered directory until artifact capture' {
        $script:SaveDiagnosticsFunctionSource |
            Should -Match ([regex]::Escape('CustomAgentLogsTmp/UITests/hang-diagnostics/attempt-$Attempt'))
        $script:RetryScriptSource |
            Should -Match ([regex]::Escape('-CrashLoopAbortThreshold 10 -Attempt $attempt'))
    }

    It 'uses the bounded shared reset helper between retry attempts' {
        $recoveryStart = $script:RetryScriptSource.IndexOf('# Same recovery as Gate')
        if ($recoveryStart -lt 0) {
            $recoveryStart = $script:RetryScriptSource.IndexOf('$recoveryBudgetSeconds = 180')
        }
        $recoveryEnd = $script:RetryScriptSource.IndexOf('Start-Sleep -Seconds $RetryDelaySeconds', $recoveryStart)
        $recoveryBlock = $script:RetryScriptSource.Substring($recoveryStart, $recoveryEnd - $recoveryStart)

        $recoveryBlock | Should -Match ([regex]::Escape("Join-Path `$PSScriptRoot 'Reset-DeviceState.ps1'"))
        $recoveryBlock | Should -Match ([regex]::Escape('-BootTimeoutSeconds $recoveryBudgetSeconds'))
        $recoveryBlock | Should -Match ([regex]::Escape('-PassThruStatus'))
        $recoveryBlock | Should -Match '\$recoveryVerified'
        $recoveryBlock | Should -Not -Match '(?m)&\s*(adb|xcrun)\b'
    }

    It 'bounds all reset-native commands and verifies iOS boot completion' {
        $script:ResetScriptSource | Should -Match 'Invoke-ProcessWithTimeout'
        $script:ResetScriptSource | Should -Not -Match '(?m)^\s*&\s*(adb|xcrun)\b'
        $script:ResetScriptSource |
            Should -Match ([regex]::Escape("@('simctl', 'bootstatus', `$sim, '-b', '-t', `"`$bootStatusTimeout`")"))
        $script:ResetScriptSource | Should -Match '\$bootStatus\.ExitCode\s*-eq\s*0'
        $script:ResetScriptSource | Should -Match 'Complete-DeviceReset -Succeeded \$true'
        $script:ResetScriptSource | Should -Match 'Complete-DeviceReset -Succeeded \$false'
    }

    It 'reports an unverified host-platform reset only when status is requested' {
        $defaultOutput = @(& $script:ResetScriptPath -Platform windows)
        $statusOutput = @(& $script:ResetScriptPath -Platform windows -PassThruStatus)

        $defaultOutput.Count | Should -Be 0
        $statusOutput.Count | Should -Be 1
        $statusOutput[0] | Should -BeFalse
    }
}
