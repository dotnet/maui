#requires -Version 5
<#
.SYNOPSIS
    Records the owned Windows Blazor test process while preserving its exit code.

.DESCRIPTION
    Diagnostic-only launcher. The two-hour observation limit does not extend the
    enclosing Helix work item's existing two-hour timeout. No results are generated.
#>
param(
    [Parameter(Mandatory = $true)][string]$Executable,
    [Parameter(Mandatory = $true)][string]$ResultsFile
)

$ErrorActionPreference = 'Stop'

$uploadRoot = $env:HELIX_WORKITEM_UPLOAD_ROOT
if (-not $uploadRoot -or -not (Test-Path -LiteralPath $uploadRoot -PathType Container)) {
    throw 'HELIX_WORKITEM_UPLOAD_ROOT must be an existing upload directory.'
}
if (-not (Test-Path -LiteralPath $Executable -PathType Leaf)) {
    throw "Test executable does not exist: $Executable"
}

$processLog = [System.IO.StreamWriter]::new(
    (Join-Path $uploadRoot 'blazor-process.log'), $false, [System.Text.UTF8Encoding]::new($false))
$processLog.AutoFlush = $true
$process = $null
$clock = [System.Diagnostics.Stopwatch]::StartNew()

function Write-ProcessEvent([string]$message) {
    $line = "$([DateTime]::UtcNow.ToString('O')) $message"
    $processLog.WriteLine($line)
    Write-Host $line
}

try {
    Write-ProcessEvent "launch-request executable=$Executable results=$ResultsFile"
    $process = Start-Process -FilePath $Executable -ArgumentList "`"$ResultsFile`"" `
        -WorkingDirectory (Split-Path -Parent $Executable) -PassThru `
        -RedirectStandardOutput (Join-Path $uploadRoot 'blazor-stdout.log') `
        -RedirectStandardError (Join-Path $uploadRoot 'blazor-stderr.log')
    $null = $process.get_Handle()
    Write-ProcessEvent "launch-return pid=$($process.Id) start=$($process.StartTime.ToUniversalTime().ToString('O'))"

    while ($true) {
        $remaining = 7200000 - $clock.ElapsedMilliseconds
        if ($remaining -le 0) {
            throw "Observation reached the existing two-hour work-item limit for PID $($process.Id)."
        }
        if ($process.WaitForExit([int][Math]::Min(30000, $remaining))) {
            break
        }

        $process.Refresh()
        if ($process.HasExited) {
            break
        }
        $progressPath = Join-Path $uploadRoot "blazor-progress-$($process.Id).log"
        $progress = if (Test-Path -LiteralPath $progressPath -PathType Leaf) {
            $file = Get-Item -LiteralPath $progressPath
            "bytes=$($file.Length) lastWrite=$($file.LastWriteTimeUtc.ToString('O'))"
        } else {
            'not-created'
        }
        Write-ProcessEvent "process-alive pid=$($process.Id) cpu=$($process.TotalProcessorTime) responding=$($process.Responding) progress=$progress"
    }

    $processExitCode = $process.get_ExitCode()
    Write-ProcessEvent "process-exit pid=$($process.Id) code=$processExitCode elapsedMs=$($clock.ElapsedMilliseconds)"
} catch {
    try {
        Write-ProcessEvent "ERROR launch-or-observation-failed: $_"
    } finally {
        if ($null -ne $process -and -not $process.HasExited) {
            $process.Kill()
            if (-not $process.WaitForExit(5000)) {
                throw "Owned process $($process.Id) did not exit after diagnostic failure."
            }
        }
    }
    throw
} finally {
    if ($null -ne $process) {
        $process.Dispose()
    }
    $processLog.Dispose()
}

# WinUI may exit -1 after producing real XML; the caller retains that result guard.
exit $processExitCode
