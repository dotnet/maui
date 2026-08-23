# The buildtasks watchdog runs the cake build under `bash` so its output can be
# piped through `tr`/`sed`, which strips carriage returns and neutralises any
# `##vso[...]` string a diagnostic-verbosity build happens to echo.
#
# On a Windows agent the bare name `bash` is a trap. `C:\Windows\System32\bash.exe`
# is the Windows Subsystem for Linux launcher, it ships on the hosted images, and
# System32 sits early enough on PATH that it wins over Git Bash. With no WSL
# distribution installed it prints
#
#     Windows Subsystem for Linux has no installed distributions.
#
# and exits 1 without ever running the build. Process.Start succeeds, so the
# caller's "could not launch bash" fallback never fires: the failure looks like a
# failed build rather than a shell that was never capable of running one.
#
# Every Windows replicate run observed to date failed here, which means the
# MSBuild tasks were never built on that platform.

# A bash under these directories is not a POSIX shell. System32/Sysnative host the
# WSL launcher and SysWOW64 its 32-bit view; WindowsApps holds Store execution
# aliases, which are zero-byte reparse stubs that fail the same way.
$script:BuildShellDisqualifiedPattern = '(?i)[\\/](system32|syswow64|sysnative|windowsapps)[\\/]'

function Test-BuildShellPathUsable {
    <#
    .SYNOPSIS
        Reports whether a bash path is a real POSIX shell rather than a Windows stub.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string]$Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $false
    }

    return -not ($Path -match $script:BuildShellDisqualifiedPattern)
}

function Resolve-BuildShellPath {
    <#
    .SYNOPSIS
        Picks the first candidate that is both present and a real POSIX shell.

    .DESCRIPTION
        Candidate order is the caller's responsibility and is honoured exactly, so
        the caller can prefer a known-good Git Bash install over whatever PATH
        happens to resolve. Returns $null when no candidate qualifies, which the
        caller must treat as "run the build without a shell" rather than as an
        error: refusing to build is strictly worse than building unfiltered.

    .PARAMETER Exists
        Existence probe, injected so the selection rules can be tested on any host.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyCollection()]
        [AllowEmptyString()]
        [string[]]$CandidatePath,

        [Parameter(Mandatory = $false)]
        [scriptblock]$Exists = { param($candidate) Test-Path -LiteralPath $candidate -PathType Leaf }
    )

    foreach ($candidate in @($CandidatePath)) {
        if (-not (Test-BuildShellPathUsable -Path $candidate)) {
            continue
        }

        $present = $false
        try {
            $present = [bool](& $Exists $candidate)
        } catch {
            # An unreadable candidate is simply not a candidate. Probing the next
            # one is always better than aborting the build over a bad PATH entry.
            $present = $false
        }

        if ($present) {
            return $candidate
        }
    }

    return $null
}

function Get-BuildShellCandidatePath {
    <#
    .SYNOPSIS
        Builds the ordered candidate list for the current host.

    .DESCRIPTION
        Windows leads with the Git for Windows install locations because those are
        the only bash on a hosted Windows agent that can actually run a build. PATH
        discovery follows so a custom install is still found. Non-Windows hosts get
        PATH discovery alone, where `bash` has always been correct.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory = $false)]
        [switch]$WindowsHost,

        [Parameter(Mandatory = $false)]
        [scriptblock]$CommandLookup = {
            @(Get-Command -Name 'bash' -CommandType Application -All -ErrorAction SilentlyContinue |
                ForEach-Object { $_.Source })
        }
    )

    $candidates = [System.Collections.Generic.List[string]]::new()

    if ($WindowsHost) {
        $programFilesRoots = @(
            $env:ProgramW6432
            $env:ProgramFiles
            ${env:ProgramFiles(x86)}
            'C:\Program Files'
        )

        foreach ($root in $programFilesRoots) {
            if ([string]::IsNullOrWhiteSpace($root)) {
                continue
            }

            foreach ($relative in @('Git\bin\bash.exe', 'Git\usr\bin\bash.exe')) {
                # Composed as a string rather than with Join-Path: these are Windows
                # paths by construction, and Join-Path resolves the drive qualifier
                # against the host, which fails anywhere that has no C: drive.
                $candidate = ($root.TrimEnd('\', '/')) + '\' + $relative
                if (-not $candidates.Contains($candidate)) {
                    [void]$candidates.Add($candidate)
                }
            }
        }
    }

    $discovered = @()
    try {
        $discovered = @(& $CommandLookup)
    } catch {
        $discovered = @()
    }

    foreach ($source in $discovered) {
        if (-not [string]::IsNullOrWhiteSpace($source) -and -not $candidates.Contains($source)) {
            [void]$candidates.Add($source)
        }
    }

    return $candidates.ToArray()
}

function Remove-VsoLoggingCommand {
    <#
    .SYNOPSIS
        Neutralises Azure DevOps logging commands echoed by a diagnostic build.

    .DESCRIPTION
        This is the PowerShell equivalent of the `sed -E 's/##vso\[[^]]*\]//g'` the
        bash pipeline applies, so a host without a usable shell still cannot have
        its pipeline state rewritten by build output. Carriage returns are dropped
        to match the `tr -d '\r'` half of the same pipeline.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string]$Line
    )

    if ($null -eq $Line) {
        return ''
    }

    return ($Line -replace '##vso\[[^\]]*\]', '') -replace "`r", ''
}

function Invoke-BuildTasksWatchdog {
    <#
    .SYNOPSIS
        Runs a build under a wall-clock bound, killing the whole tree if it hangs.

    .DESCRIPTION
        Azure DevOps' own step timeout cannot kill a wedged dotnet/cake process
        tree, so the bound is enforced here and the tree is killed from inside.

        When a POSIX shell is available the build runs under it so its output can
        be piped through `tr`/`sed`. When none is (a Windows agent whose only bash
        is the WSL stub), the build runs directly and its stdout is filtered in
        process, which preserves both the wall-clock bound and the guarantee that
        build output cannot rewrite pipeline state.

    .OUTPUTS
        The child's exit code, or 124 if the wall clock expired.
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$ShellCommand,

        [Parameter(Mandatory = $true)]
        [string]$DirectFileName,

        [Parameter(Mandatory = $true)]
        [AllowEmptyCollection()]
        [string[]]$DirectArgument,

        [Parameter(Mandatory = $false)]
        [AllowNull()]
        [AllowEmptyString()]
        [string]$ShellPath,

        [Parameter(Mandatory = $false)]
        [ValidateRange(0.001, 1440)]
        [double]$TimeoutMinutes = 40
    )

    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.UseShellExecute = $false

    $filterInProcess = [string]::IsNullOrWhiteSpace($ShellPath)
    if ($filterInProcess) {
        $psi.FileName = $DirectFileName
        foreach ($argument in @($DirectArgument)) {
            [void]$psi.ArgumentList.Add($argument)
        }
        # Only stdout is redirected. stderr keeps the inherited console handle, so
        # there is no second pipe to drain and therefore no way to deadlock on a
        # full buffer while this thread is blocked reading the first.
        $psi.RedirectStandardOutput = $true
    } else {
        $psi.FileName = $ShellPath
        foreach ($argument in @('-o', 'pipefail', '-c', $ShellCommand)) {
            [void]$psi.ArgumentList.Add($argument)
        }
    }

    $process = [System.Diagnostics.Process]::Start($psi)
    $deadline = [DateTime]::UtcNow.AddMinutes($TimeoutMinutes)

    if ($filterInProcess) {
        $reader = $process.StandardOutput
        while ($true) {
            $remainingMs = [int][Math]::Max(0, [Math]::Min(
                    [int]::MaxValue,
                    ($deadline - [DateTime]::UtcNow).TotalMilliseconds))
            if ($remainingMs -le 0) {
                return (Stop-BuildTasksProcessTree -Process $process -TimeoutMinutes $TimeoutMinutes)
            }

            $read = $reader.ReadLineAsync()
            if (-not $read.Wait($remainingMs)) {
                return (Stop-BuildTasksProcessTree -Process $process -TimeoutMinutes $TimeoutMinutes)
            }

            if ($null -eq $read.Result) {
                break
            }

            Write-Host (Remove-VsoLoggingCommand -Line $read.Result)
        }
    }

    $remainingMs = [int][Math]::Max(0, [Math]::Min(
            [int]::MaxValue,
            ($deadline - [DateTime]::UtcNow).TotalMilliseconds))
    if (-not $process.WaitForExit($remainingMs)) {
        return (Stop-BuildTasksProcessTree -Process $process -TimeoutMinutes $TimeoutMinutes)
    }

    return [int]$process.ExitCode
}

function Stop-BuildTasksProcessTree {
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [Parameter(Mandatory = $true)]
        [System.Diagnostics.Process]$Process,

        [Parameter(Mandatory = $true)]
        [double]$TimeoutMinutes
    )

    Write-Host "##[error]dotnet-buildtasks exceeded $TimeoutMinutes min wall-clock — killing the process tree so the retry can start on a fresh agent."
    try {
        $Process.Kill($true)
    } catch {
        try {
            Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
        } catch {
        }
    }

    Start-Sleep -Seconds 3
    return 124
}

