#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$RepositoryPath,

    [Parameter(Mandatory)]
    [string]$DotNetSdk
)

$ErrorActionPreference = "Stop"

if ($DotNetSdk -eq "global-json") {
    $globalJsonPath = Join-Path $RepositoryPath "global.json"
    if (-not (Test-Path $globalJsonPath)) {
        throw "Cannot resolve dotnet SDK from global.json because '$globalJsonPath' does not exist."
    }

    $globalJson = Get-Content $globalJsonPath -Raw | ConvertFrom-Json
    if ($globalJson.tools -and $globalJson.tools.dotnet) {
        $DotNetSdk = [string]$globalJson.tools.dotnet
    } elseif ($globalJson.sdk -and $globalJson.sdk.version) {
        $DotNetSdk = [string]$globalJson.sdk.version
    } else {
        throw "global.json does not contain tools.dotnet or sdk.version."
    }
}

if ($DotNetSdk -notmatch "^(\d+)\.(\d+)") {
    throw "Unable to derive a target framework from dotnet SDK version '$DotNetSdk'."
}

$dotNetTfm = "net$($Matches[1]).$($Matches[2])"

Write-Host "Resolved .NET SDK: $DotNetSdk"
Write-Host "Resolved .NET TFM: $dotNetTfm"

if ($env:GITHUB_OUTPUT) {
    "dotnet_sdk=$DotNetSdk" >> $env:GITHUB_OUTPUT
    "dotnet_tfm=$dotNetTfm" >> $env:GITHUB_OUTPUT
}
