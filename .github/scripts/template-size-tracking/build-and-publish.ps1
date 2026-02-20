#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and publishes a .NET MAUI template project for size measurement.

.PARAMETER ProjectPath
    Path to the project directory.

.PARAMETER Platform
    Target platform (android, ios, maccatalyst, windows-packaged, windows-unpackaged).

.PARAMETER Framework
    Target framework (e.g., "net10.0-android").

.PARAMETER Rid
    Runtime identifier (e.g., "android-arm64", "ios-arm64", "win-x64").

.PARAMETER IsAot
    Whether to build with Native AOT ("True" or "False").
#>

param(
    [Parameter(Mandatory)][string]$ProjectPath,
    [Parameter(Mandatory)][string]$Platform,
    [Parameter(Mandatory)][string]$Framework,
    [Parameter(Mandatory)][string]$Rid,
    [Parameter(Mandatory)][string]$IsAot
)

$ErrorActionPreference = "Stop"

$isAotBool = $IsAot -eq "True"
$startTime = Get-Date

$projectFile = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse | Select-Object -First 1
if (-not $projectFile) {
    Write-Error "Could not find project file in $ProjectPath"
    exit 1
}

Write-Host "Found project: $($projectFile.FullName)"

switch ($Platform) {
    "android" {
        if ($isAotBool) {
            Write-Host "Building Android AAB with Native AOT..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release -r $Rid `
                -p:PublishAot=true `
                -p:AndroidPackageFormat=aab -p:AndroidKeyStore=false `
                -o publish /bl:build.binlog
        } else {
            Write-Host "Building Android AAB..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release `
                -p:AndroidPackageFormat=aab -p:AndroidKeyStore=false `
                -o publish /bl:build.binlog
        }
    }

    "ios" {
        # iOS publish requires device RID + signing. Use ad-hoc signing
        # (CodesignKey=-) with BuildIpa=false to skip IPA creation.
        Write-Host "Building iOS with ad-hoc signing..."
        dotnet publish $projectFile.FullName `
            -f $Framework -c Release -r $Rid `
            -p:_RequireCodeSigning=false `
            -p:EnableCodeSigning=false `
            '-p:CodesignKey=-' `
            -p:BuildIpa=false `
            -p:ValidateXcodeVersion=false `
            /bl:build.binlog

        # With BuildIpa=false, the .app stays in bin/ (not copied to -o path).
        # Find and copy it to publish/ for consistent measurement.
        $binPath = Join-Path $ProjectPath "bin/Release/$Framework/$Rid"
        Write-Host "Looking for .app in: $binPath"
        $appBundle = Get-ChildItem -Path $binPath -Filter "*.app" -Directory -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($appBundle) {
            New-Item -ItemType Directory -Path "publish" -Force | Out-Null
            Copy-Item -Path $appBundle.FullName -Destination "publish/" -Recurse
            Write-Host "Copied $($appBundle.Name) to publish/"
        } else {
            Write-Host "Warning: No .app bundle found in $binPath, listing contents:"
            Get-ChildItem -Path $binPath -Recurse -ErrorAction SilentlyContinue |
                Select-Object -First 20 | ForEach-Object { Write-Host "  $_" }
        }
    }

    "maccatalyst" {
        if ($isAotBool) {
            Write-Host "Building MacCatalyst with Native AOT..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release -r $Rid `
                -p:PublishAot=true `
                -p:_RequireCodeSigning=false -p:ValidateXcodeVersion=false `
                -o publish /bl:build.binlog
        } else {
            Write-Host "Building MacCatalyst (unsigned)..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release -r $Rid `
                -p:_RequireCodeSigning=false -p:ValidateXcodeVersion=false `
                -o publish /bl:build.binlog
        }
    }

    "windows-packaged" {
        if ($isAotBool) {
            Write-Host "Building Windows MSIX with Native AOT..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release -r $Rid `
                -p:PublishAot=true `
                -p:WindowsPackageType=MSIX `
                -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false `
                -o publish /bl:build.binlog
        } else {
            Write-Host "Building Windows MSIX..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release `
                -p:WindowsPackageType=MSIX `
                -p:GenerateAppxPackageOnBuild=true -p:AppxPackageSigningEnabled=false `
                -o publish /bl:build.binlog
        }
    }

    "windows-unpackaged" {
        if ($isAotBool) {
            Write-Host "Building Windows unpackaged with Native AOT..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release -r $Rid `
                --self-contained true -p:PublishAot=true `
                -p:WindowsPackageType=None `
                -o publish /bl:build.binlog
        } else {
            # Framework-dependent publish without explicit RID.
            # Specifying -r win-x64 forces self-contained which needs
            # Mono.win-x64 runtime package (not available for .NET 10 GA).
            Write-Host "Building Windows unpackaged (framework-dependent)..."
            dotnet publish $projectFile.FullName `
                -f $Framework -c Release `
                -p:WindowsPackageType=None `
                -o publish /bl:build.binlog
        }
    }
}

$buildTime = [math]::Round(((Get-Date) - $startTime).TotalSeconds, 2)
Write-Host "Build completed in $buildTime seconds"
echo "BUILD_TIME=$buildTime" >> $env:GITHUB_ENV
