function Invoke-IssueReplicateUIRunner {
    param(
        [Parameter(Mandatory)][string]$Runner,
        [Parameter(Mandatory)][string[]]$Arguments,
        [Parameter(Mandatory)][hashtable]$State,
        [ValidateRange(1, 3600)][int]$TimeoutSeconds = 2700,
        [ValidateRange(1, 900)][int]$IdleTimeoutSeconds = 600
    )

    $pwsh = Get-Command pwsh -CommandType Application -ErrorAction Stop | Select-Object -First 1
    $start = [Diagnostics.ProcessStartInfo]::new($pwsh.Source)
    $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in @('-NoProfile', '-NonInteractive', '-File', $Runner) + $Arguments) {
        $start.ArgumentList.Add($argument)
    }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    $timer = [Diagnostics.Stopwatch]::StartNew()
    $lastOutput = 0.0
    $exitedAt = $null
    $started = $false
    $State.ExitCode = 124
    try {
        $started = $process.Start()
        if (-not $started) { throw 'Could not start the bounded UI runner.' }
        $readers = @($process.StandardOutput, $process.StandardError)
        $pending = @($readers[0].ReadLineAsync(), $readers[1].ReadLineAsync())
        while (@($pending | Where-Object { $null -ne $_ }).Count) {
            for ($stream = 0; $stream -lt 2; $stream++) {
                if ($null -eq $pending[$stream] -or -not $pending[$stream].IsCompleted) { continue }
                $line = $pending[$stream].GetAwaiter().GetResult()
                if ($null -eq $line) { $pending[$stream] = $null; continue }
                $lastOutput = $timer.Elapsed.TotalSeconds
                if ($line.Length -gt 8000) { $line = $line.Substring(0, 8000) + ' [line truncated]' }
                Write-Output $line
                $pending[$stream] = $readers[$stream].ReadLineAsync()
            }
            if ($process.HasExited) {
                if ($null -eq $exitedAt) { $exitedAt = $timer.Elapsed.TotalSeconds }
                if ($timer.Elapsed.TotalSeconds - $exitedAt -gt 2) {
                    Write-Output 'Verification incomplete: UI runner exited with inherited output handles still open; capture was bounded.'
                    return
                }
            }
            if ($timer.Elapsed.TotalSeconds -ge $TimeoutSeconds -or
                $timer.Elapsed.TotalSeconds - $lastOutput -ge $IdleTimeoutSeconds) {
                Write-Output "Verification incomplete: UI runner exceeded its ${TimeoutSeconds}s wall or ${IdleTimeoutSeconds}s no-output deadline; no result is manufactured."
                return
            }
            Start-Sleep -Milliseconds 20
        }
        if (-not $process.WaitForExit(2000)) {
            Write-Output 'Verification incomplete: UI runner closed output without terminating; no result is manufactured.'
            return
        }
        $State.ExitCode = $process.ExitCode
    } finally {
        try {
            if ($started -and -not $process.HasExited) {
                $process.Kill($true)
                if (-not $process.WaitForExit(10000)) { throw 'Could not terminate the owned timed-out UI runner tree.' }
            }
        } finally { $process.Dispose() }
    }
}

function Get-IssueReplicateAndroidCrashDiagnostic {
    param([Parameter(Mandatory)][string]$OutputDirectory)

    if ($env:DEVICE_UDID -cnotmatch '^emulator-[0-9]+$') {
        throw 'Android crash diagnostics require the explicitly owned emulator serial.'
    }
    $capture = Invoke-ProcessWithTimeout -FilePath 'adb' -TimeoutSeconds 20 `
        -ArgumentList @('-s', $env:DEVICE_UDID, 'logcat', '-b', 'crash', '-d', '-v', 'threadtime', '-t', '500')
    if ($capture.TimedOut -or $capture.OutputDrainTimedOut -or $capture.ExitCode -ne 0) {
        throw 'The owned emulator crash buffer could not be captured within its deadline.'
    }
    $text = ($capture.Output -join "`n").Replace("`r", '') -replace '##vso\[[^]]*\]', ''
    if ([Text.Encoding]::UTF8.GetByteCount($text) -gt 65536) {
        throw 'The bounded Android crash buffer exceeds its diagnostic file limit.'
    }
    $blocks = @([regex]::Split($text, '(?m)(?=^.*\bDEBUG\s*:\s*\*{3}\s+\*{3})') |
        Where-Object { $_ -cmatch '>>>\s*com\.microsoft\.maui\.uitests\s*<<<' })
    if (-not $blocks.Count) { return @() }
    $selected = $blocks -join "`n"
    $path = Join-Path $OutputDirectory 'android-hostapp-crash.log'
    $bytes = [Text.Encoding]::UTF8.GetBytes($selected)
    [IO.File]::WriteAllBytes($path, $bytes)
    $hash = [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant()
    Write-Host "Unqualified HostApp crash buffer, emulator $env:DEVICE_UDID, SHA256 $hash; not an issue assertion:"
    Write-Host $selected
    return @($selected.Split("`n") | Where-Object {
        $_ -match 'pid:|tid:|name:|Cmdline:|signal [0-9]+|backtrace:|#[0-9]{2}\s+pc'
    } | Select-Object -First 12 | ForEach-Object { "Native crash diagnostic (unqualified): $_" })
}

function Invoke-IssueReplicateDiagnosticProcess {
    param(
        [Parameter(Mandatory)][string]$Code,
        [string[]]$Arguments = @(),
        [Parameter(Mandatory)][ValidateRange(1, 2098176)][int]$MaxOutputBytes,
        [ValidateRange(1, 30)][int]$TimeoutSeconds = 20
    )

    $python = Get-Command python3 -CommandType Application -ErrorAction Stop | Select-Object -First 1
    $start = [Diagnostics.ProcessStartInfo]::new($python.Source)
    $start.UseShellExecute = $false
    $start.RedirectStandardOutput = $true
    $start.RedirectStandardError = $true
    foreach ($argument in @('-I', '-S', '-c', $Code) + $Arguments) {
        $start.ArgumentList.Add($argument)
    }
    $process = [Diagnostics.Process]::new()
    $process.StartInfo = $start
    $timer = [Diagnostics.Stopwatch]::StartNew()
    $started = $false
    try {
        $started = $process.Start()
        if (-not $started) { throw 'Could not start the diagnostic reader.' }
        $bytes = [byte[]]::new($MaxOutputBytes + 1)
        $count = 0
        while ($true) {
            $read = $process.StandardOutput.BaseStream.ReadAsync($bytes, $count, $bytes.Length - $count)
            $remaining = [int][Math]::Ceiling($TimeoutSeconds * 1000 - $timer.Elapsed.TotalMilliseconds)
            if ($remaining -le 0 -or -not $read.Wait($remaining)) {
                throw 'The diagnostic reader exceeded its hard deadline.'
            }
            $length = $read.GetAwaiter().GetResult()
            if ($length -eq 0) { break }
            $count += $length
            if ($count -gt $MaxOutputBytes) { throw 'The diagnostic reader exceeded its output bound.' }
        }
        $remaining = [int][Math]::Ceiling($TimeoutSeconds * 1000 - $timer.Elapsed.TotalMilliseconds)
        if ($remaining -le 0 -or -not $process.WaitForExit($remaining)) {
            throw 'The diagnostic reader exceeded its hard deadline.'
        }
        if ($process.ExitCode -ne 0) {
            $errorBytes = [byte[]]::new(8192)
            $readError = $process.StandardError.BaseStream.ReadAsync($errorBytes, 0, $errorBytes.Length)
            $remaining = [int][Math]::Ceiling($TimeoutSeconds * 1000 - $timer.Elapsed.TotalMilliseconds)
            if ($remaining -le 0 -or -not $readError.Wait($remaining)) {
                throw 'The diagnostic reader exceeded its hard deadline.'
            }
            $message = [Text.Encoding]::UTF8.GetString($errorBytes, 0, $readError.GetAwaiter().GetResult())
            $message = $message.Replace("`r", '') -replace '##vso\[[^]]*\]', ''
            if ($message.Length -gt 1000) { $message = $message.Substring(0, 1000) }
            throw "The diagnostic reader failed: $($message.Trim())"
        }
        return [Text.UTF8Encoding]::new($false, $true).GetString($bytes, 0, $count)
    } finally {
        try {
            if ($started -and -not $process.HasExited) {
                $process.Kill($true)
                if (-not $process.WaitForExit(2000)) { throw 'Could not stop the diagnostic reader.' }
            }
        } finally { $process.Dispose() }
    }
}

function Read-IssueReplicateNativeDiagnostic {
    param(
        [Parameter(Mandatory)][string]$Path,
        [Parameter(Mandatory)][ValidateRange(1, 1048576)][int]$MaxBytes,
        [switch]$Tail,
        [switch]$AppiumLog,
        [ValidateRange(1, 30)][int]$TimeoutSeconds = 20
    )

    if ($IsWindows) { throw 'Native diagnostic readers require a Unix host.' }
    if ($AppiumLog -and -not $Tail) { throw 'Filtered Appium diagnostics require a bounded tail scan.' }
    $reader = @'
import base64
import json
import os
import stat
import sys

path, mode, limit = sys.argv[1], sys.argv[2], int(sys.argv[3])
fd = os.open(path, os.O_RDONLY | os.O_NONBLOCK | os.O_NOFOLLOW)
try:
    info = os.fstat(fd)
    if not stat.S_ISREG(info.st_mode):
        raise ValueError("Native diagnostics must be a regular file.")
    scan_limit = 64 * 1024 * 1024 if mode == "appium-tail" else limit
    start = max(0, info.st_size - scan_limit) if mode in ("tail", "appium-tail") else 0
    if mode == "whole" and info.st_size > limit:
        raise ValueError("Native diagnostics exceed the file bound.")
    omitted = start > 0
    if mode == "appium-tail":
        position = info.st_size
        rows = []
        prefix = b""
        line_size = 0
        terminated = False
        while position > start and len(rows) <= 80:
            count = min(position - start, 8192)
            position -= count
            chunk = os.pread(fd, count, position)
            if len(chunk) != count:
                raise ValueError("Native diagnostics changed during the bounded read.")
            parts = chunk.split(b"\n")
            for index in range(len(parts) - 1, -1, -1):
                part = parts[index]
                prefix = (part + prefix)[:2000]
                line_size += len(part)
                omitted = omitted or line_size > 2000
                if index > 0:
                    if (line_size or terminated) and b"[IOS_SYSLOG_ROW]" not in prefix:
                        rows.append(prefix + (b"\n" if terminated else b""))
                        if len(rows) > 80:
                            omitted = True
                            break
                    prefix = b""
                    line_size = 0
                    terminated = True
        if position == 0 and len(rows) <= 80 and (line_size or terminated):
            if b"[IOS_SYSLOG_ROW]" not in prefix:
                rows.append(prefix + (b"\n" if terminated else b""))
                omitted = omitted or len(rows) > 80
        data = b"".join(reversed(rows[:80]))
    else:
        os.lseek(fd, start, os.SEEK_SET)
        remaining = limit if mode == "tail" else limit + 1
        chunks = []
        while remaining:
            chunk = os.read(fd, min(remaining, 8192))
            if not chunk:
                break
            chunks.append(chunk)
            remaining -= len(chunk)
        data = b"".join(chunks)
    if mode == "appium-tail" and len(data) > limit:
        data = data[-limit:]
        omitted = True
    if len(data) > limit:
        raise ValueError("Native diagnostics exceed the read bound.")
    print(json.dumps({"data": base64.b64encode(data).decode("ascii"), "omitted": omitted}))
finally:
    os.close(fd)
'@
    $mode = if ($AppiumLog) { 'appium-tail' } elseif ($Tail) { 'tail' } else { 'whole' }
    $json = Invoke-IssueReplicateDiagnosticProcess -Code $reader -Arguments @($Path, $mode, "$MaxBytes") `
        -MaxOutputBytes ($MaxBytes * 2 + 1024) -TimeoutSeconds $TimeoutSeconds
    $record = $json | ConvertFrom-Json -Depth 3
    if ($record.data -isnot [string] -or $record.omitted -isnot [bool]) {
        throw 'The diagnostic reader returned an invalid record.'
    }
    $bytes = [Convert]::FromBase64String($record.data)
    if ($bytes.Length -gt $MaxBytes) { throw 'The diagnostic reader returned oversized data.' }
    return [pscustomobject]@{
        Text = [Text.Encoding]::UTF8.GetString($bytes)
        Omitted = $record.omitted
    }
}
