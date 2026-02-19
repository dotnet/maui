#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Creates a .NET MAUI template project configured for size measurement.

.PARAMETER Template
    Template to use (maui, maui-blazor).

.PARAMETER DotNetVersion
    .NET version (e.g., "9.0", "10.0").

.PARAMETER Description
    Platform description (e.g., "android", "ios", "windows-packaged-aot").

.PARAMETER Framework
    Target framework (e.g., "net10.0-android").
#>

param(
    [Parameter(Mandatory)][string]$Template,
    [Parameter(Mandatory)][string]$DotNetVersion,
    [Parameter(Mandatory)][string]$Description,
    [Parameter(Mandatory)][string]$Framework
)

$ErrorActionPreference = "Stop"

# Project name: hyphens are fine for MSIX identity ([-.A-Za-z0-9]).
# Remove hyphens from template name to keep it short.
$shortTemplate = $Template -replace '-', ''
$projectName = "Ma-$shortTemplate-$Description"

# Create project in $HOME to avoid inheriting repo's Directory.Build.props
# (which imports Arcade SDK and causes build failures)
$buildRoot = Join-Path $HOME "template-builds"
New-Item -ItemType Directory -Path $buildRoot -Force | Out-Null
$projectDir = Join-Path $buildRoot $projectName

# Place NuGet.config in the PARENT directory (not project dir) to avoid
# macOS build system bundling it into .app packages
$nugetConfig = @"
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="dotnet-public" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json" />
  </packageSources>
</configuration>
"@
$nugetConfig | Out-File -FilePath (Join-Path $buildRoot "NuGet.config") -Encoding UTF8

Write-Host "Creating project: dotnet new $Template -o $projectDir --framework net$DotNetVersion"
dotnet new $Template -o $projectDir --framework "net$DotNetVersion"

# Pin SDK version to match the target .NET version.
# Runners may have multiple SDKs installed; without pinning, the highest
# SDK resolves workload manifests that conflict with older TFMs.
$sdkVersion = dotnet --list-sdks `
    | Where-Object { $_ -match "^$DotNetVersion" } `
    | ForEach-Object { ($_ -split ' ')[0] } `
    | Select-Object -Last 1

if ($sdkVersion) {
    Write-Host "Pinning SDK to $sdkVersion via global.json"
    @{ sdk = @{ version = $sdkVersion; rollForward = "latestPatch" } } `
        | ConvertTo-Json `
        | Out-File -FilePath (Join-Path $projectDir "global.json") -Encoding UTF8
}

# Restrict TargetFrameworks to only the platform we're building for.
# MAUI templates target all platforms (android, ios, maccatalyst, windows).
# Building on Ubuntu for Android would fail trying to resolve iOS workloads.
$csproj = Get-ChildItem -Path $projectDir -Filter "*.csproj" -Recurse | Select-Object -First 1
if ($csproj) {
    $content = Get-Content $csproj.FullName -Raw
    $content = $content -replace '<TargetFrameworks>[^<]+</TargetFrameworks>', "<TargetFrameworks>$Framework</TargetFrameworks>"
    Set-Content -Path $csproj.FullName -Value $content
    Write-Host "Restricted TargetFrameworks to: $Framework"
}

echo "PROJECT_PATH=$projectDir" >> $env:GITHUB_ENV
echo "PROJECT_NAME=$projectName" >> $env:GITHUB_ENV
