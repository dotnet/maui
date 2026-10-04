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
from collections import deque

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
    os.lseek(fd, start, os.SEEK_SET)
    remaining = min(scan_limit, info.st_size - start) if mode == "appium-tail" else (
        limit if mode == "tail" else limit + 1)
    chunks = deque(maxlen=80) if mode == "appium-tail" else []
    line = bytearray()
    partial = mode == "appium-tail" and start > 0
    omitted = start > 0
    while remaining:
        chunk = os.read(fd, min(remaining, 8192))
        if not chunk:
            break
        if mode == "appium-tail":
            parts = chunk.split(b"\n")
            for index, part in enumerate(parts):
                if not partial:
                    if len(line) + len(part) > 2000:
                        omitted = True
                    line.extend(part[:max(0, 2000 - len(line))])
                if index < len(parts) - 1:
                    if not partial and b"[IOS_SYSLOG_ROW]" not in line:
                        omitted = omitted or len(chunks) == 80
                        chunks.append(bytes(line) + b"\n")
                    line.clear()
                    partial = False
        else:
            chunks.append(chunk)
        remaining -= len(chunk)
    if mode == "appium-tail" and line and not partial and b"[IOS_SYSLOG_ROW]" not in line:
        omitted = omitted or len(chunks) == 80
        chunks.append(bytes(line))
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
