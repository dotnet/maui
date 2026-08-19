#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Shared utility functions for MAUI test scripts

.DESCRIPTION
    Common functions used across BuildAndRunHostApp.ps1 and BuildAndRunSandbox.ps1
#>

# Color output functions
function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-Host "🔹 $Message" -ForegroundColor Cyan
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor Gray
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Write-Warn {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Stop-ProcessTree {
    param([int]$ProcessId)

    if ($ProcessId -le 0) {
        return $true
    }

    $stopped = $true
    try {
        if ($IsWindows) {
            $children = Get-CimInstance Win32_Process -Filter "ParentProcessId=$ProcessId" -ErrorAction SilentlyContinue
            foreach ($child in @($children)) {
                if (-not (Stop-ProcessTree -ProcessId ([int]$child.ProcessId))) {
                    $stopped = $false
                }
            }
        }
        else {
            foreach ($childIdText in @(& pgrep -P $ProcessId 2>$null)) {
                $childProcessId = 0
                if ([int]::TryParse("$childIdText", [ref]$childProcessId) -and
                    -not (Stop-ProcessTree -ProcessId $childProcessId)) {
                    $stopped = $false
                }
            }
        }
    }
    catch {
        $stopped = $false
    }

    Stop-Process -Id $ProcessId -Force -ErrorAction SilentlyContinue
    if (-not $IsWindows -and (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue)) {
        # sudo-launched xcodebuild descendants can run as root. Use the same non-interactive
        # elevation mode as the download itself, scoped to the exact PID discovered above.
        & sudo -n kill -9 $ProcessId 2>$null | Out-Null
    }
    for ($attempt = 0; $attempt -lt 10 -and (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue); $attempt++) {
        Start-Sleep -Milliseconds 100
    }
    if (Get-Process -Id $ProcessId -ErrorAction SilentlyContinue) {
        $stopped = $false
    }

    return $stopped
}

function Invoke-ProcessWithTimeout {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [string[]]$ArgumentList = @(),

        [Parameter(Mandatory = $true)]
        [ValidateRange(1, 86400)]
        [int]$TimeoutSeconds
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = $FilePath
    foreach ($argument in $ArgumentList) {
        $startInfo.ArgumentList.Add($argument)
    }
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true

    $process = [System.Diagnostics.Process]::new()
    $process.StartInfo = $startInfo
    $stdoutTask = $null
    $stderrTask = $null

    try {
        if (-not $process.Start()) {
            throw "Failed to start process '$FilePath'."
        }

        $stdoutTask = $process.StandardOutput.ReadToEndAsync()
        $stderrTask = $process.StandardError.ReadToEndAsync()
        $timeoutMilliseconds = [int][Math]::Min([int]::MaxValue, [int64]$TimeoutSeconds * 1000)
        $timedOut = -not $process.WaitForExit($timeoutMilliseconds)

        if ($timedOut) {
            $treeStopped = Stop-ProcessTree -ProcessId $process.Id
            if (-not $treeStopped) {
                try {
                    $process.Kill($true)
                }
                catch {
                    # The exact-PID tree walk above already attempted elevated termination.
                }
            }

            if (-not $process.WaitForExit(10000)) {
                [void](Stop-ProcessTree -ProcessId $process.Id)
                if (-not $process.WaitForExit(5000)) {
                    throw "Timed-out process tree rooted at PID $($process.Id) did not terminate."
                }
            }
        }
        else {
            $process.WaitForExit()
        }

        $stdout = $stdoutTask.GetAwaiter().GetResult()
        $stderr = $stderrTask.GetAwaiter().GetResult()
        $output = @()
        foreach ($text in @($stdout, $stderr)) {
            if (-not [string]::IsNullOrEmpty($text)) {
                $output += @($text -split "\r?\n" | Where-Object { $_ -ne '' })
            }
        }

        return [pscustomobject]@{
            Output = $output
            ExitCode = if ($timedOut) { 124 } else { $process.ExitCode }
            TimedOut = $timedOut
        }
    }
    finally {
        $process.Dispose()
    }
}

function Get-MauiTfmVersion {
    <#
    .SYNOPSIS
        Returns the repo's MAUI .NET TFM version (e.g. "10.0" or "11.0").
    .DESCRIPTION
        Reads <_MauiDotNetVersionMajor>/<_MauiDotNetVersionMinor> from Directory.Build.props so
        app/test TargetFrameworks follow the checked-out branch instead of being hardcoded
        (e.g. a net11.0 PR builds net11.0-android, not net10.0-android). Searches the supplied
        RepoRoot, then $env:BUILD_SOURCESDIRECTORY (the AzDO working tree), then the current
        directory — so it still resolves when the caller runs from a trusted-copy location
        outside the working tree. Falls back to "10.0".
    #>
    param([string]$RepoRoot)

    $candidates = @()
    if ($RepoRoot) { $candidates += $RepoRoot }
    if ($env:BUILD_SOURCESDIRECTORY) { $candidates += $env:BUILD_SOURCESDIRECTORY }
    $candidates += (Get-Location).Path

    foreach ($root in $candidates) {
        if (-not $root) { continue }
        $propsPath = Join-Path $root 'Directory.Build.props'
        if (Test-Path $propsPath) {
            $content = Get-Content $propsPath -Raw
            if ($content -match '<_MauiDotNetVersionMajor[^>]*>\s*(\d+)\s*<') {
                $major = $Matches[1]
                $minor = if ($content -match '<_MauiDotNetVersionMinor[^>]*>\s*(\d+)\s*<') { $Matches[1] } else { '0' }
                return "$major.$minor"
            }
        }
    }

    # Secondary source: global.json's SDK band, so a Directory.Build.props parse-miss on a
    # net11+ branch doesn't silently build net10. Only major.minor is used.
    # NOTE: global.json's `tools.dotnet` is the build-SDK *band* — a proxy for the MAUI TFM,
    # which move in lockstep in this repo. The regex keys on that `tools.dotnet` convention
    # (not `sdk.version`) and the trailing `.` after the minor; revisit if the pin format changes.
    foreach ($root in $candidates) {
        if (-not $root) { continue }
        $gjPath = Join-Path $root 'global.json'
        if (Test-Path $gjPath) {
            $gj = Get-Content $gjPath -Raw
            if ($gj -match '"dotnet"\s*:\s*"(\d+)\.(\d+)\.') {
                Write-Warn "Get-MauiTfmVersion: Directory.Build.props parse failed; using global.json SDK band ($($Matches[1]).$($Matches[2]))"
                return "$($Matches[1]).$($Matches[2])"
            }
        }
    }

    Write-Warn "Could not find <_MauiDotNetVersionMajor> in Directory.Build.props or global.json — falling back to '10.0'"
    return '10.0'
}
