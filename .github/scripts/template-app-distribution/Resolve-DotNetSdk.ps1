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

if ($DotNetSdk -notmatch "\A(?<Major>0|[1-9]\d*)\.(?<Minor>0|[1-9]\d*)\.(?<Patch>0|[1-9]\d*)(?:-[0-9A-Za-z]+(?:[.-][0-9A-Za-z]+)*)?\z") {
    throw "Dotnet SDK version '$DotNetSdk' must be a complete SDK version such as '11.0.100' or '11.0.100-preview.1.25120.13'."
}

$dotNetTfm = "net$($Matches.Major).$($Matches.Minor)"

Write-Host "Resolved .NET SDK: $DotNetSdk"
Write-Host "Resolved .NET TFM: $dotNetTfm"

if ($env:GITHUB_OUTPUT) {
    "dotnet_sdk=$DotNetSdk" >> $env:GITHUB_OUTPUT
    "dotnet_tfm=$dotNetTfm" >> $env:GITHUB_OUTPUT
}
