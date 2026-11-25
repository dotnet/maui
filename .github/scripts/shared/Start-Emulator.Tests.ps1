#!/usr/bin/env pwsh
#Requires -Modules Pester

BeforeAll {
    $scriptPath = Join-Path $PSScriptRoot 'Start-Emulator.ps1'
    . (Join-Path $PSScriptRoot 'shared-utils.ps1')

    $tokens = $null
    $parseErrors = $null
    $ast = [System.Management.Automation.Language.Parser]::ParseFile($scriptPath, [ref]$tokens, [ref]$parseErrors)
    if ($parseErrors -and $parseErrors.Count -gt 0) {
        throw ($parseErrors | ForEach-Object { $_.Message }) -join [Environment]::NewLine
    }

    $function = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Test-ShouldRestartReusedAndroidEmulator'
    }, $true)
    if (-not $function) {
        throw "Function 'Test-ShouldRestartReusedAndroidEmulator' not found"
    }

    Invoke-Expression $function.Extent.Text
    $processLookupFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Get-AndroidEmulatorProcessIds'
    }, $true)
    if (-not $processLookupFunction) {
        throw "Function 'Get-AndroidEmulatorProcessIds' not found"
    }
    Invoke-Expression $processLookupFunction.Extent.Text
    $scriptContent = Get-Content -Raw -Path $scriptPath
    $downloadFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Invoke-IosRuntimeDownload'
    }, $true)
    $downloadProcessFunction = $ast.Find({
        $args[0] -is [System.Management.Automation.Language.FunctionDefinitionAst] -and
        $args[0].Name -eq 'Invoke-IosRuntimeDownloadProcess'
    }, $true)
    if (-not $downloadFunction -or -not $downloadProcessFunction) {
        throw 'iOS runtime download functions not found'
    }
}

Describe 'Reused Android emulator recovery' {
    It 'does not restart a fresh emulator' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $false `
            -RecoveryAlreadyAttempted $false `
            -UnreadySeconds 240 | Should -BeFalse
    }

    It 'waits for the bounded reused-emulator threshold' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $true `
            -RecoveryAlreadyAttempted $false `
            -UnreadySeconds 145 | Should -BeFalse
    }

    It 'restarts a reused emulator after the threshold' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $true `
            -RecoveryAlreadyAttempted $false `
            -UnreadySeconds 150 | Should -BeTrue
    }

    It 'never restarts the replacement emulator a second time' {
        Test-ShouldRestartReusedAndroidEmulator `
            -ReuseExistingEmulator $true `
            -RecoveryAlreadyAttempted $true `
            -UnreadySeconds 240 | Should -BeFalse
    }

    It 'uses total unready time instead of an offline-only streak' {
        $scriptContent | Should -Match ([regex]::Escape('-UnreadySeconds $deviceWaited'))
        $scriptContent | Should -Not -Match '\$offlineStreak'
    }
}

Describe 'Android emulator process discovery' {
    It 'does not match the lookup process itself on Unix' -Skip:$IsWindows {
        $avdName = "NoSuchAvd_$([guid]::NewGuid().ToString('N'))"

        @(Get-AndroidEmulatorProcessIds -AvdName $avdName).Count |
            Should -Be 0
    }

    It 'keeps the pgrep pattern out of an ancestor bash command line' {
        $processLookupFunction.Extent.Text |
            Should -Match '& pgrep -f \$processPattern'
        $processLookupFunction.Extent.Text |
            Should -Not -Match 'bash\s+-c'
        $scriptContent |
            Should -Not -Match 'bash\s+-c\s+"pgrep\s+-f'
    }
}

Describe 'Bounded iOS runtime download recovery' {
    It 'kills the exact child process tree when the deadline expires' {
        $pwshPath = (Get-Command pwsh -ErrorAction Stop).Source
        $parentCommand = @'
$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = $env:START_EMULATOR_TEST_PWSH
foreach ($argument in @('-NoProfile', '-NonInteractive', '-Command', 'Start-Sleep -Seconds 30')) {
    $startInfo.ArgumentList.Add($argument)
}
$startInfo.UseShellExecute = $false
$child = [System.Diagnostics.Process]::Start($startInfo)
[Console]::Out.WriteLine($child.Id)
[Console]::Out.Flush()
Start-Sleep -Seconds 30
'@

        $previousPwshPath = $env:START_EMULATOR_TEST_PWSH
        try {
            $env:START_EMULATOR_TEST_PWSH = $pwshPath
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            $result = Invoke-ProcessWithTimeout `
                -FilePath $pwshPath `
                -ArgumentList @('-NoProfile', '-NonInteractive', '-Command', $parentCommand) `
                -TimeoutSeconds 2
            $stopwatch.Stop()
        }
        finally {
            $env:START_EMULATOR_TEST_PWSH = $previousPwshPath
        }

        $result.TimedOut | Should -BeTrue
        $result.ExitCode | Should -Be 124
        $stopwatch.Elapsed.TotalSeconds | Should -BeLessThan 12
        $childPid = @($result.Output | Where-Object { $_ -match '^\d+$' } | Select-Object -First 1)
        $childPid.Count | Should -Be 1

        for ($attempt = 0; $attempt -lt 20 -and (Get-Process -Id ([int]$childPid[0]) -ErrorAction SilentlyContinue); $attempt++) {
            Start-Sleep -Milliseconds 100
        }
        Get-Process -Id ([int]$childPid[0]) -ErrorAction SilentlyContinue | Should -BeNullOrEmpty
    }

    It 'bounds output draining when a reparented descendant retains the pipe' -Skip:$IsWindows {
        $pwshPath = (Get-Command pwsh -ErrorAction Stop).Source
        $pidFile = Join-Path $TestDrive 'reparented-grandchild.pid'
        $childCommand = @'
$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = $env:PROCESS_DRAIN_TEST_PWSH
foreach ($argument in @('-NoProfile', '-NonInteractive', '-Command', 'Start-Sleep -Seconds 30')) {
    $startInfo.ArgumentList.Add($argument)
}
$startInfo.UseShellExecute = $false
$grandchild = [System.Diagnostics.Process]::Start($startInfo)
Set-Content -LiteralPath $env:PROCESS_DRAIN_PID_FILE -Value $grandchild.Id -NoNewline
'@
        $rootCommand = @'
$startInfo = [System.Diagnostics.ProcessStartInfo]::new()
$startInfo.FileName = $env:PROCESS_DRAIN_TEST_PWSH
foreach ($argument in @('-NoProfile', '-NonInteractive', '-Command', $env:PROCESS_DRAIN_CHILD_COMMAND)) {
    $startInfo.ArgumentList.Add($argument)
}
$startInfo.UseShellExecute = $false
$child = [System.Diagnostics.Process]::Start($startInfo)
$child.WaitForExit()
Start-Sleep -Seconds 30
'@

        $previousPwsh = $env:PROCESS_DRAIN_TEST_PWSH
        $previousPidFile = $env:PROCESS_DRAIN_PID_FILE
        $previousChildCommand = $env:PROCESS_DRAIN_CHILD_COMMAND
        $grandchildPid = 0
        try {
            $env:PROCESS_DRAIN_TEST_PWSH = $pwshPath
            $env:PROCESS_DRAIN_PID_FILE = $pidFile
            $env:PROCESS_DRAIN_CHILD_COMMAND = $childCommand

            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            $result = Invoke-ProcessWithTimeout `
                -FilePath $pwshPath `
                -ArgumentList @('-NoProfile', '-NonInteractive', '-Command', $rootCommand) `
                -TimeoutSeconds 1 `
                -OutputDrainTimeoutMilliseconds 500
            $stopwatch.Stop()

            $grandchildPid = [int](Get-Content -Raw -LiteralPath $pidFile)
            $result.TimedOut | Should -BeTrue
            $result.OutputDrainTimedOut | Should -BeTrue
            $result.Output | Should -Contain (
                'Output capture stopped after 500ms because an inherited pipe handle remained open.')
            $stopwatch.Elapsed.TotalSeconds | Should -BeLessThan 8
            Get-Process -Id $grandchildPid -ErrorAction SilentlyContinue |
                Should -Not -BeNullOrEmpty
        }
        finally {
            if ($grandchildPid -le 0 -and (Test-Path -LiteralPath $pidFile)) {
                $grandchildPid = [int](Get-Content -Raw -LiteralPath $pidFile)
            }
            if ($grandchildPid -gt 0) {
                Stop-Process -Id $grandchildPid -Force -ErrorAction SilentlyContinue
            }
            $env:PROCESS_DRAIN_TEST_PWSH = $previousPwsh
            $env:PROCESS_DRAIN_PID_FILE = $previousPidFile
            $env:PROCESS_DRAIN_CHILD_COMMAND = $previousChildCommand
        }
    }

    It 'bounds every xcodebuild path and aborts into the Gate environment retry path' {
        $downloadFunction.Extent.Text | Should -Match 'Invoke-IosRuntimeDownloadProcess'
        $downloadFunction.Extent.Text | Should -Not -Match '(?m)&\s+(?:sudo\s+-n\s+)?xcodebuild\b'
        $downloadProcessFunction.Extent.Text | Should -Match 'Invoke-ProcessWithTimeout'
        $scriptContent | Should -Match '(?s)\$downloadResult = Invoke-IosRuntimeDownload.*?\$downloadResult\.TimedOut.*?ENV ERROR: iOS runtime download exceeded.*?exit 1'
    }
}
