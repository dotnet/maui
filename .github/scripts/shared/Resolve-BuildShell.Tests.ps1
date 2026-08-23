Describe 'Choosing a shell that can actually run the build' {
    BeforeAll {
        . (Join-Path $PSScriptRoot 'Resolve-BuildShell.ps1')
    }

    Context 'Rejecting the Windows stubs that masquerade as bash' {
        It 'rejects the WSL launcher in System32' {
            Test-BuildShellPathUsable -Path 'C:\Windows\System32\bash.exe' | Should -BeFalse
        }

        It 'rejects the WSL launcher whatever the drive or casing' {
            Test-BuildShellPathUsable -Path 'D:\WINDOWS\SYSTEM32\BASH.EXE' | Should -BeFalse
        }

        It 'rejects the Sysnative view of the same launcher' {
            Test-BuildShellPathUsable -Path 'C:\Windows\Sysnative\bash.exe' | Should -BeFalse
        }

        It 'rejects the SysWOW64 view of the same launcher' {
            Test-BuildShellPathUsable -Path 'C:\Windows\SysWOW64\bash.exe' | Should -BeFalse
        }

        It 'rejects a Store execution alias' {
            Test-BuildShellPathUsable -Path 'C:\Users\a\AppData\Local\Microsoft\WindowsApps\bash.exe' |
                Should -BeFalse
        }

        It 'rejects a stub reached through forward slashes' {
            Test-BuildShellPathUsable -Path 'C:/Windows/System32/bash.exe' | Should -BeFalse
        }

        It 'accepts Git Bash' {
            Test-BuildShellPathUsable -Path 'C:\Program Files\Git\bin\bash.exe' | Should -BeTrue
        }

        It 'accepts a POSIX bash' {
            Test-BuildShellPathUsable -Path '/bin/bash' | Should -BeTrue
        }

        It 'treats a blank path as unusable' {
            Test-BuildShellPathUsable -Path '  ' | Should -BeFalse
        }

        It 'does not reject a shell merely because a parent directory is named after one' {
            # 'system32-tools' is not the System32 directory; matching on the bare
            # word would strand a perfectly good shell.
            Test-BuildShellPathUsable -Path 'C:\tools\system32-tools\bash.exe' | Should -BeTrue
        }
    }

    Context 'Selecting from the candidate list' {
        It 'skips the WSL stub and picks the real shell behind it' {
            $selected = Resolve-BuildShellPath `
                -CandidatePath @('C:\Windows\System32\bash.exe', 'C:\Program Files\Git\bin\bash.exe') `
                -Exists { param($candidate) $true }

            $selected | Should -Be 'C:\Program Files\Git\bin\bash.exe'
        }

        It 'honours candidate order rather than preferring a later match' {
            $selected = Resolve-BuildShellPath `
                -CandidatePath @('C:\Program Files\Git\bin\bash.exe', 'C:\other\bash.exe') `
                -Exists { param($candidate) $true }

            $selected | Should -Be 'C:\Program Files\Git\bin\bash.exe'
        }

        It 'skips a candidate that does not exist' {
            $selected = Resolve-BuildShellPath `
                -CandidatePath @('C:\missing\bash.exe', 'C:\Program Files\Git\bin\bash.exe') `
                -Exists { param($candidate) $candidate -ne 'C:\missing\bash.exe' }

            $selected | Should -Be 'C:\Program Files\Git\bin\bash.exe'
        }

        It 'never returns a stub even when it is the only candidate present' {
            $selected = Resolve-BuildShellPath `
                -CandidatePath @('C:\Windows\System32\bash.exe') `
                -Exists { param($candidate) $true }

            $selected | Should -BeNullOrEmpty
        }

        It 'reports no shell rather than throwing when nothing qualifies' {
            $selected = Resolve-BuildShellPath `
                -CandidatePath @('C:\missing\bash.exe') `
                -Exists { param($candidate) $false }

            $selected | Should -BeNullOrEmpty
        }

        It 'survives an empty candidate list' {
            Resolve-BuildShellPath -CandidatePath @() -Exists { param($candidate) $true } |
                Should -BeNullOrEmpty
        }

        It 'keeps probing after an existence check throws' {
            # A malformed PATH entry must not be able to abort the build.
            $selected = Resolve-BuildShellPath `
                -CandidatePath @('C:\explodes\bash.exe', '/bin/bash') `
                -Exists {
                    param($candidate)
                    if ($candidate -eq 'C:\explodes\bash.exe') { throw 'unreadable' }
                    $true
                }

            $selected | Should -Be '/bin/bash'
        }

        It 'ignores blank entries in the candidate list' {
            $selected = Resolve-BuildShellPath `
                -CandidatePath @('', '/bin/bash') `
                -Exists { param($candidate) $true }

            $selected | Should -Be '/bin/bash'
        }
    }

    Context 'Building the candidate list for the host' {
        It 'puts Git Bash ahead of whatever PATH resolved on Windows' {
            $candidates = Get-BuildShellCandidatePath `
                -WindowsHost `
                -CommandLookup { @('C:\Windows\System32\bash.exe') }

            $gitIndex = [array]::FindIndex([string[]]$candidates, [Predicate[string]] { param($p) $p -like '*Git*bash.exe' })
            $stubIndex = [array]::IndexOf([string[]]$candidates, 'C:\Windows\System32\bash.exe')

            $gitIndex | Should -BeGreaterOrEqual 0
            $stubIndex | Should -BeGreaterThan $gitIndex
        }

        It 'still offers the PATH result on Windows so a custom install is reachable' {
            $candidates = Get-BuildShellCandidatePath `
                -WindowsHost `
                -CommandLookup { @('C:\custom\bash.exe') }

            $candidates | Should -Contain 'C:\custom\bash.exe'
        }

        It 'does not invent Git paths on a non-Windows host' {
            $candidates = Get-BuildShellCandidatePath -CommandLookup { @('/bin/bash') }

            $candidates | Should -Be @('/bin/bash')
        }

        It 'does not abort when PATH discovery throws' {
            $candidates = Get-BuildShellCandidatePath -WindowsHost -CommandLookup { throw 'no path' }

            @($candidates).Count | Should -BeGreaterThan 0
        }

        It 'does not offer the same path twice' {
            $candidates = Get-BuildShellCandidatePath `
                -WindowsHost `
                -CommandLookup { @('/bin/bash', '/bin/bash') }

            @($candidates | Group-Object | Where-Object { $_.Count -gt 1 }) | Should -BeNullOrEmpty
        }
    }

    Context 'Neutralising pipeline commands echoed by the build' {
        It 'strips a logging command so build output cannot rewrite pipeline state' {
            Remove-VsoLoggingCommand -Line 'prefix ##vso[task.complete result=Failed;] suffix' |
                Should -Be 'prefix  suffix'
        }

        It 'strips every logging command on the line, not just the first' {
            Remove-VsoLoggingCommand -Line '##vso[task.setvariable variable=a]1##vso[task.setvariable variable=b]2' |
                Should -Be '12'
        }

        It 'stops at the first closing bracket so ordinary text after it survives' {
            Remove-VsoLoggingCommand -Line '##vso[task.debug]kept [bracketed] text' |
                Should -Be 'kept [bracketed] text'
        }

        It 'drops carriage returns the way the shell pipeline does' {
            Remove-VsoLoggingCommand -Line "line`r" | Should -Be 'line'
        }

        It 'leaves an ordinary line untouched' {
            Remove-VsoLoggingCommand -Line 'Build succeeded.' | Should -Be 'Build succeeded.'
        }

        It 'returns an empty string for an empty line' {
            Remove-VsoLoggingCommand -Line '' | Should -Be ''
        }
    }

    Context 'Running the build under a wall-clock bound' {
        BeforeAll {
            $script:Pwsh = (Get-Process -Id $PID).Path
        }

        It 'propagates the exit code when a shell runs the build' {
            $exit = Invoke-BuildTasksWatchdog `
                -ShellPath '/bin/bash' `
                -ShellCommand 'exit 7' `
                -DirectFileName $script:Pwsh `
                -DirectArgument @('-NoProfile', '-Command', 'exit 7') `
                -TimeoutMinutes 1

            $exit | Should -Be 7
        }

        It 'propagates success when a shell runs the build' {
            $exit = Invoke-BuildTasksWatchdog `
                -ShellPath '/bin/bash' `
                -ShellCommand 'exit 0' `
                -DirectFileName $script:Pwsh `
                -DirectArgument @('-NoProfile', '-Command', 'exit 0') `
                -TimeoutMinutes 1

            $exit | Should -Be 0
        }

        It 'still runs the build when no shell is available' {
            $exit = Invoke-BuildTasksWatchdog `
                -ShellPath $null `
                -ShellCommand 'exit 0' `
                -DirectFileName $script:Pwsh `
                -DirectArgument @('-NoProfile', '-Command', 'exit 5') `
                -TimeoutMinutes 1

            $exit | Should -Be 5
        }

        It 'filters logging commands out of the output it relays' {
            $output = Invoke-BuildTasksWatchdog `
                -ShellPath '' `
                -ShellCommand 'true' `
                -DirectFileName $script:Pwsh `
                -DirectArgument @('-NoProfile', '-Command', "Write-Output 'a##vso[task.complete result=Failed;]b'") `
                -TimeoutMinutes 1 6>&1 | Out-String

            $output | Should -Match 'ab'
            $output | Should -Not -Match '##vso'
        }

        It 'kills a build that outruns its wall clock and reports the timeout code' {
            $exit = Invoke-BuildTasksWatchdog `
                -ShellPath '/bin/bash' `
                -ShellCommand 'sleep 120' `
                -DirectFileName $script:Pwsh `
                -DirectArgument @('-NoProfile', '-Command', 'Start-Sleep -Seconds 120') `
                -TimeoutMinutes 0.05 `
                -WarningAction SilentlyContinue 6>&1 | Select-Object -Last 1

            # A 3-second bound keeps the kill path honest without making the suite
            # wait a real minute for it.
            $exit | Should -Be 124
        }

        It 'kills a silent hang in the no-shell path instead of blocking forever' {
            $exit = Invoke-BuildTasksWatchdog `
                -ShellPath $null `
                -ShellCommand 'true' `
                -DirectFileName $script:Pwsh `
                -DirectArgument @('-NoProfile', '-Command', 'Start-Sleep -Seconds 120') `
                -TimeoutMinutes 0.05 6>&1 | Select-Object -Last 1

            $exit | Should -Be 124
        }
    }
}
