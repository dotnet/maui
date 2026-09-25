#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $false)]
    [ValidateRange(1, 65535)]
    [int]$Port = 4723,

    [Parameter(Mandatory = $true)]
    [string]$LogPath,

    [Parameter(Mandatory = $true)]
    [string]$StatePath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

$statusUrl = "http://127.0.0.1:$Port/wd/hub/status"
$StatePath = Assert-UiEvidenceLocalPath $StatePath
$LogPath = Assert-UiEvidenceLocalPath $LogPath
if ((Test-Path -LiteralPath $StatePath) -or (Test-Path -LiteralPath $LogPath)) {
    throw "Appium state and log paths must be fresh."
}
$probe = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Any, $Port)
$probe.ExclusiveAddressUse = $true
try {
    $probe.Start()
}
catch [Net.Sockets.SocketException] {
    throw "Appium port $Port is unavailable; do not reuse an unrelated server. $($_.Exception.Message)"
}
finally {
    $probe.Stop()
}

$logDirectory = Split-Path -Parent $LogPath
if ($logDirectory) {
    New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
}
if ($IsWindows) {
    $appiumCommand = Get-Command "appium.cmd" -ErrorAction SilentlyContinue
    if ($null -ne $appiumCommand) {
        $commandLine = "/d /s /c call `"$($appiumCommand.Source)`" --address 127.0.0.1 --base-path /wd/hub --port $Port --log `"$LogPath`""
        $startParameters = @{
            FilePath = $env:ComSpec
            ArgumentList = $commandLine
            PassThru = $true
            WindowStyle = "Hidden"
        }
    }
    else {
        $appiumScript = Get-Command "appium.ps1" -ErrorAction SilentlyContinue
        if ($null -eq $appiumScript) {
            throw "Neither appium.cmd nor appium.ps1 is available on PATH."
        }
        $startParameters = @{
            FilePath = (Get-Command pwsh -ErrorAction Stop).Source
            ArgumentList = @(
                "-NoProfile",
                "-File", $appiumScript.Source,
                "--address", "127.0.0.1",
                "--base-path", "/wd/hub",
                "--port", "$Port",
                "--log", $LogPath
            )
            PassThru = $true
            WindowStyle = "Hidden"
        }
    }
}
else {
    $appium = Get-Command "appium" -ErrorAction SilentlyContinue
    if ($null -eq $appium) {
        throw "The Appium executable is not available on PATH."
    }
    $startParameters = @{
        FilePath = $appium.Source
        ArgumentList = @("--address", "127.0.0.1", "--base-path", "/wd/hub", "--port", "$Port", "--log", $LogPath)
        PassThru = $true
    }
}
$process = Start-Process @startParameters

$started = $false
try {
    $deadline = [DateTimeOffset]::UtcNow.AddSeconds(60)
    while ([DateTimeOffset]::UtcNow -lt $deadline) {
        if ($process.HasExited) {
            throw "Appium exited before becoming ready with code $($process.ExitCode)."
        }
        $ready = $false
        try {
            $status = Invoke-RestMethod -Uri $statusUrl -TimeoutSec 3
            $ready = $status.value.ready -eq $true
        }
        catch {
            Write-Verbose "Waiting for Appium readiness: $($_.Exception.Message)"
        }
        if ($ready) {
            Write-UiEvidenceJson ([PSCustomObject]@{
                schemaVersion = 2
                started = $true
                processId = $process.Id
                processStartedAtUtc = $process.StartTime.ToUniversalTime().ToString("O")
                port = $Port
                statusUrl = $statusUrl
            }) $StatePath
            $started = $true
            Write-Host "Appium is ready at $statusUrl."
            exit 0
        }
        Start-Sleep -Seconds 1
    }
    throw "Timed out waiting for Appium at $statusUrl."
}
finally {
    try {
        if (-not $started -and -not $process.HasExited) {
            $process.Kill($true)
            if (-not $process.WaitForExit(10000)) {
                throw "The owned Appium process did not exit after failed startup."
            }
        }
    }
    catch {
        Write-Warning "Appium startup cleanup failed: $($_.Exception.Message)" -WarningAction Continue
    }
    finally {
        try {
            $process.Dispose()
        }
        catch {
            Write-Warning "Appium process disposal failed: $($_.Exception.Message)" -WarningAction Continue
        }
    }
}
