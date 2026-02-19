#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Generates the GitHub Actions build matrix for template size tracking.

.PARAMETER DotNetVersions
    Comma-separated .NET versions to test (e.g., "9.0,10.0").

.PARAMETER Templates
    Comma-separated templates to test (e.g., "maui,maui-blazor").
#>

param(
    [string]$DotNetVersions = "9.0,10.0",
    [string]$Templates = "maui,maui-blazor"
)

$ErrorActionPreference = "Stop"

$dotnetVersions = $DotNetVersions.Split(',') | ForEach-Object { $_.Trim() }
$templates = $Templates.Split(',') | ForEach-Object { $_.Trim() }

$matrix = @{
    include = @()
}

foreach ($dotnet in $dotnetVersions) {
    foreach ($template in $templates) {
        # Android
        $matrix.include += @{
            description = 'android'
            dotnet = $dotnet
            template = $template
            platform = 'android'
            os = 'ubuntu-latest'
            framework = "net$dotnet-android"
            rid = 'android-arm64'
            aot = $false
        }

        # Android Native AOT (.NET 10+ only)
        if ($dotnet -ne "9.0") {
            $matrix.include += @{
                description = 'android-aot'
                dotnet = $dotnet
                template = $template
                platform = 'android'
                os = 'ubuntu-latest'
                framework = "net$dotnet-android"
                rid = 'android-arm64'
                aot = $true
            }
        }

        # iOS
        $matrix.include += @{
            description = 'ios'
            dotnet = $dotnet
            template = $template
            platform = 'ios'
            os = 'macos-latest'
            framework = "net$dotnet-ios"
            rid = 'ios-arm64'
            aot = $false
        }

        # MacCatalyst
        $matrix.include += @{
            description = 'maccatalyst'
            dotnet = $dotnet
            template = $template
            platform = 'maccatalyst'
            os = 'macos-latest'
            framework = "net$dotnet-maccatalyst"
            rid = 'maccatalyst-arm64'
            aot = $false
        }

        # MacCatalyst Native AOT
        $matrix.include += @{
            description = 'maccatalyst-aot'
            dotnet = $dotnet
            template = $template
            platform = 'maccatalyst'
            os = 'macos-latest'
            framework = "net$dotnet-maccatalyst"
            rid = 'maccatalyst-arm64'
            aot = $true
        }

        # Windows Packaged (MSIX)
        $matrix.include += @{
            description = 'windows-packaged'
            dotnet = $dotnet
            template = $template
            platform = 'windows-packaged'
            os = 'windows-latest'
            framework = "net$dotnet-windows10.0.19041.0"
            rid = 'win-x64'
            aot = $false
        }

        # Windows Packaged (MSIX) Native AOT
        $matrix.include += @{
            description = 'windows-packaged-aot'
            dotnet = $dotnet
            template = $template
            platform = 'windows-packaged'
            os = 'windows-latest'
            framework = "net$dotnet-windows10.0.19041.0"
            rid = 'win-x64'
            aot = $true
        }

        # Windows Unpackaged
        $matrix.include += @{
            description = 'windows-unpackaged'
            dotnet = $dotnet
            template = $template
            platform = 'windows-unpackaged'
            os = 'windows-latest'
            framework = "net$dotnet-windows10.0.19041.0"
            rid = 'win-x64'
            aot = $false
        }

        # Windows Unpackaged Native AOT
        $matrix.include += @{
            description = 'windows-unpackaged-aot'
            dotnet = $dotnet
            template = $template
            platform = 'windows-unpackaged'
            os = 'windows-latest'
            framework = "net$dotnet-windows10.0.19041.0"
            rid = 'win-x64'
            aot = $true
        }
    }
}

$matrixJson = $matrix | ConvertTo-Json -Compress -Depth 10
Write-Host "Matrix: $matrixJson"
echo "matrix=$matrixJson" >> $env:GITHUB_OUTPUT
