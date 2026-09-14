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
try {
    Invoke-RestMethod -Uri $statusUrl -TimeoutSec 3 | Out-Null
    Write-UiEvidenceJson ([PSCustomObject]@{
        schemaVersion = 1
        started = $false
        processId = $null
        port = $Port
        statusUrl = $statusUrl
    }) $StatePath
    exit 0
}
catch {
}

$logDirectory = Split-Path -Parent $LogPath
if ($logDirectory) {
    New-Item -ItemType Directory -Force -Path $logDirectory | Out-Null
}
if ($IsWindows) {
    $appiumCommand = Get-Command "appium.cmd" -ErrorAction SilentlyContinue
    if ($null -ne $appiumCommand) {
        $commandLine = "/d /s /c call `"$($appiumCommand.Source)`" --base-path /wd/hub --port $Port --log `"$LogPath`""
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
        ArgumentList = @("--base-path", "/wd/hub", "--port", "$Port", "--log", $LogPath)
        PassThru = $true
    }
}
$process = Start-Process @startParameters

$deadline = [DateTimeOffset]::UtcNow.AddSeconds(60)
while ([DateTimeOffset]::UtcNow -lt $deadline) {
    if ($process.HasExited) {
        throw "Appium exited before becoming ready with code $($process.ExitCode)."
    }
    try {
        Invoke-RestMethod -Uri $statusUrl -TimeoutSec 3 | Out-Null
        Write-UiEvidenceJson ([PSCustomObject]@{
            schemaVersion = 1
            started = $true
            processId = $process.Id
            port = $Port
            statusUrl = $statusUrl
        }) $StatePath
        Write-Host "Appium is ready at $statusUrl."
        exit 0
    }
    catch {
        Start-Sleep -Seconds 1
    }
}

try {
    $process.Kill($true)
}
catch {
}
throw "Timed out waiting for Appium at $statusUrl."
