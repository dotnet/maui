#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Measures package size and collects metrics for .NET MAUI templates.

.PARAMETER PublishPath
    Path to the publish output directory.

.PARAMETER Platform
    Target platform (android, ios, maccatalyst, windows-packaged, windows-unpackaged).

.PARAMETER Description
    Full description including platform and build type (e.g., windows-packaged-aot).

.PARAMETER OS
    Runner operating system (ubuntu-latest, macos-latest, windows-latest).

.PARAMETER IsAot
    Whether this is a Native AOT build.

.PARAMETER Template
    Template type (maui, maui-blazor).

.PARAMETER DotNetVersion
    .NET version used for the build.

.PARAMETER MauiVersion
    .NET MAUI Templates version used.

.PARAMETER BuildTime
    Build time in seconds.

.PARAMETER OutputPath
    Path where the metrics JSON file will be saved.
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$PublishPath,

    [Parameter(Mandatory=$true)]
    [string]$Platform,

    [Parameter(Mandatory=$true)]
    [string]$Description,

    [Parameter(Mandatory=$true)]
    [string]$OS,

    [Parameter(Mandatory=$true)]
    [ValidateSet('True', 'False', 'true', 'false')]
    [string]$IsAot,

    [Parameter(Mandatory=$true)]
    [string]$Template,

    [Parameter(Mandatory=$true)]
    [string]$DotNetVersion,

    [Parameter(Mandatory=$true)]
    [string]$MauiVersion,

    [Parameter(Mandatory=$true)]
    [decimal]$BuildTime,

    [Parameter(Mandatory=$true)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

Write-Host "=== Measuring Package Size ===" -ForegroundColor Cyan
Write-Host "Platform: $Platform"
Write-Host "Description: $Description"
Write-Host "OS: $OS"
Write-Host "Is AOT: $IsAot"
Write-Host "Template: $Template"
Write-Host "Publish Path: $PublishPath"

# Initialize metrics object
$metrics = @{
    timestamp = (Get-Date).ToUniversalTime().ToString("o")
    template = $Template
    platform = $Platform
    description = $Description
    os = $OS
    dotnetVersion = $DotNetVersion
    mauiVersion = $MauiVersion
    buildTimeSeconds = [math]::Round($BuildTime, 2)
    isAot = [System.Convert]::ToBoolean($IsAot)
}

function Get-DirectorySize {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return 0 }
    $size = (Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue |
        Measure-Object -Property Length -Sum).Sum
    if ($null -eq $size) { return 0 }
    return $size
}

function Get-FileCount {
    param([string]$Path, [string]$Filter = "*")
    if (-not (Test-Path $Path)) { return 0 }
    return (Get-ChildItem -Path $Path -Filter $Filter -Recurse -File -ErrorAction SilentlyContinue |
        Measure-Object).Count
}

function Get-LargestFile {
    param([string]$Path)
    if (-not (Test-Path $Path)) { return $null }
    $largest = Get-ChildItem -Path $Path -Recurse -File -ErrorAction SilentlyContinue |
        Sort-Object Length -Descending |
        Select-Object -First 1
    if ($largest) {
        return @{ name = $largest.Name; size = $largest.Length }
    }
    return $null
}

switch ($Platform) {
    "android" {
        Write-Host "Measuring Android AAB package..." -ForegroundColor Yellow

        $aabFile = Get-ChildItem -Path $PublishPath -Filter "*.aab" -Recurse | Select-Object -First 1

        if ($aabFile) {
            $metrics.packageSize = $aabFile.Length
            $metrics.packagePath = $aabFile.Name
            Write-Host "AAB Size: $([math]::Round($aabFile.Length / 1MB, 2)) MB"
        } else {
            # Fall back to APK
            $apkFile = Get-ChildItem -Path $PublishPath -Filter "*-Signed.apk" -Recurse | Select-Object -First 1
            if (-not $apkFile) {
                $apkFile = Get-ChildItem -Path $PublishPath -Filter "*.apk" -Recurse | Select-Object -First 1
            }
            if ($apkFile) {
                $metrics.packageSize = $apkFile.Length
                $metrics.packagePath = $apkFile.Name
                Write-Host "APK Size: $([math]::Round($apkFile.Length / 1MB, 2)) MB"
            } else {
                Write-Warning "No AAB or APK file found"
                $metrics.packageSize = 0
            }
        }

        $metrics.totalPublishSize = Get-DirectorySize -Path $PublishPath
        $metrics.fileCount = Get-FileCount -Path $PublishPath
        $metrics.assemblyCount = Get-FileCount -Path $PublishPath -Filter "*.dll"
    }

    "ios" {
        Write-Host "Measuring iOS package..." -ForegroundColor Yellow

        # Look for .ipa first, then .app bundle
        $ipaFile = Get-ChildItem -Path $PublishPath -Filter "*.ipa" -Recurse | Select-Object -First 1

        if ($ipaFile) {
            $metrics.packageSize = $ipaFile.Length
            $metrics.packagePath = $ipaFile.Name
            Write-Host "IPA Size: $([math]::Round($ipaFile.Length / 1MB, 2)) MB"
        } else {
            $appBundle = Get-ChildItem -Path $PublishPath -Filter "*.app" -Recurse -Directory | Select-Object -First 1
            if ($appBundle) {
                $metrics.packageSize = Get-DirectorySize -Path $appBundle.FullName
                $metrics.packagePath = $appBundle.Name
                Write-Host "App Bundle Size: $([math]::Round($metrics.packageSize / 1MB, 2)) MB"
            } else {
                Write-Warning "No IPA or .app bundle found, measuring publish directory"
                $metrics.packageSize = Get-DirectorySize -Path $PublishPath
            }
        }

        $metrics.totalPublishSize = Get-DirectorySize -Path $PublishPath
        $metrics.fileCount = Get-FileCount -Path $PublishPath
        $metrics.assemblyCount = Get-FileCount -Path $PublishPath -Filter "*.dll"
    }

    "maccatalyst" {
        Write-Host "Measuring MacCatalyst package..." -ForegroundColor Yellow

        # Look for .pkg first, then .app bundle
        $pkgFile = Get-ChildItem -Path $PublishPath -Filter "*.pkg" -Recurse | Select-Object -First 1

        if ($pkgFile) {
            $metrics.packageSize = $pkgFile.Length
            $metrics.packagePath = $pkgFile.Name
            Write-Host "PKG Size: $([math]::Round($pkgFile.Length / 1MB, 2)) MB"
        } else {
            $appBundle = Get-ChildItem -Path $PublishPath -Filter "*.app" -Recurse -Directory | Select-Object -First 1
            if ($appBundle) {
                $metrics.packageSize = Get-DirectorySize -Path $appBundle.FullName
                $metrics.packagePath = $appBundle.Name
                Write-Host "App Bundle Size: $([math]::Round($metrics.packageSize / 1MB, 2)) MB"
            } else {
                Write-Warning "No PKG or .app bundle found, measuring publish directory"
                $metrics.packageSize = Get-DirectorySize -Path $PublishPath
            }
        }

        $metrics.totalPublishSize = Get-DirectorySize -Path $PublishPath
        $metrics.fileCount = Get-FileCount -Path $PublishPath
        $metrics.assemblyCount = Get-FileCount -Path $PublishPath -Filter "*.dll"
    }

    "windows-packaged" {
        Write-Host "Measuring Windows MSIX package..." -ForegroundColor Yellow

        $msixFile = Get-ChildItem -Path $PublishPath -Filter "*.msix" -Recurse | Select-Object -First 1

        if ($msixFile) {
            $metrics.packageSize = $msixFile.Length
            $metrics.packagePath = $msixFile.Name
            Write-Host "MSIX Size: $([math]::Round($msixFile.Length / 1MB, 2)) MB"
        } else {
            # Fall back to msixbundle or appx
            $bundleFile = Get-ChildItem -Path $PublishPath -Filter "*.msixbundle" -Recurse | Select-Object -First 1
            if ($bundleFile) {
                $metrics.packageSize = $bundleFile.Length
                $metrics.packagePath = $bundleFile.Name
                Write-Host "MSIX Bundle Size: $([math]::Round($bundleFile.Length / 1MB, 2)) MB"
            } else {
                Write-Warning "No MSIX file found, measuring publish directory"
                $metrics.packageSize = Get-DirectorySize -Path $PublishPath
            }
        }

        $metrics.totalPublishSize = Get-DirectorySize -Path $PublishPath
        $metrics.fileCount = Get-FileCount -Path $PublishPath
        $metrics.assemblyCount = Get-FileCount -Path $PublishPath -Filter "*.dll"
    }

    "windows-unpackaged" {
        Write-Host "Measuring Windows unpackaged publish folder..." -ForegroundColor Yellow

        $metrics.packageSize = Get-DirectorySize -Path $PublishPath
        $metrics.packagePath = "publish"
        Write-Host "Publish Folder Size: $([math]::Round($metrics.packageSize / 1MB, 2)) MB"

        $metrics.totalPublishSize = $metrics.packageSize
        $metrics.fileCount = Get-FileCount -Path $PublishPath

        if ($metrics.isAot) {
            $mainExe = Get-ChildItem -Path $PublishPath -Filter "*.exe" -File |
                Where-Object { $_.Length -gt 1MB } |
                Sort-Object Length -Descending |
                Select-Object -First 1
            if ($mainExe) {
                $metrics.assemblyCount = 1
                $metrics.mainExecutableSize = $mainExe.Length
                $metrics.mainExecutableName = $mainExe.Name
            } else {
                $metrics.assemblyCount = 0
            }
        } else {
            $metrics.assemblyCount = (Get-ChildItem -Path $PublishPath -Filter "*.dll" -Recurse -File | Measure-Object).Count
            $mainExe = Get-ChildItem -Path $PublishPath -Filter "*.exe" -File |
                Where-Object { $_.Length -gt 1MB } |
                Sort-Object Length -Descending |
                Select-Object -First 1
            if ($mainExe) {
                $metrics.mainExecutableSize = $mainExe.Length
                $metrics.mainExecutableName = $mainExe.Name
            }
        }
    }
}

# Common metrics
$metrics.totalAssemblySize = (Get-ChildItem -Path $PublishPath -Filter "*.dll" -Recurse -ErrorAction SilentlyContinue |
    Measure-Object -Property Length -Sum).Sum

$largestFile = Get-LargestFile -Path $PublishPath
if ($largestFile) {
    $metrics.largestFile = $largestFile
}

# Compressed size
if ($Platform -eq "android" -or $Platform -eq "windows-packaged") {
    # AAB and MSIX are already compressed archives
    $metrics.compressedSize = $metrics.packageSize
    Write-Host "Compressed Size (same as package): $([math]::Round($metrics.compressedSize / 1MB, 2)) MB"
} elseif ($Platform -eq "ios" -or $Platform -eq "maccatalyst") {
    # .app bundles are directories, not compressed archives — measure actual compressed size
    $tempZip = Join-Path ([System.IO.Path]::GetTempPath()) "package.zip"
    if (Test-Path $tempZip) { Remove-Item $tempZip -Force }
    Write-Host "Creating compressed archive for .app bundle measurement..." -ForegroundColor Yellow
    try {
        Compress-Archive -Path "$PublishPath/*" -DestinationPath $tempZip -CompressionLevel Optimal -Force
        $metrics.compressedSize = (Get-Item $tempZip).Length
        Write-Host "Compressed Size: $([math]::Round($metrics.compressedSize / 1MB, 2)) MB"
        Remove-Item $tempZip -Force
    } catch {
        Write-Warning "Could not create compressed archive: $_"
        $metrics.compressedSize = 0
    }
} else {
    $tempZip = Join-Path ([System.IO.Path]::GetTempPath()) "package.zip"
    if (Test-Path $tempZip) { Remove-Item $tempZip -Force }
    Write-Host "Creating compressed archive for measurement..." -ForegroundColor Yellow
    try {
        Compress-Archive -Path "$PublishPath/*" -DestinationPath $tempZip -CompressionLevel Optimal -Force
        $metrics.compressedSize = (Get-Item $tempZip).Length
        Write-Host "Compressed Size: $([math]::Round($metrics.compressedSize / 1MB, 2)) MB"
        Remove-Item $tempZip -Force
    } catch {
        Write-Warning "Could not create compressed archive: $_"
        $metrics.compressedSize = 0
    }
}

if ($metrics.compressedSize -gt 0 -and $metrics.packageSize -gt 0) {
    $metrics.compressionRatio = [math]::Round(($metrics.compressedSize / $metrics.packageSize) * 100, 2)
}

# SDK version
try {
    $sdkVersion = dotnet --version
    $metrics.dotnetSdkVersion = $sdkVersion.Trim()
} catch {
    $metrics.dotnetSdkVersion = "unknown"
}

# Build time formatting
$ts = [System.TimeSpan]::FromSeconds($metrics.buildTimeSeconds)
$metrics.buildTimeFormatted = $ts.ToString("hh\:mm\:ss")

# Summary
Write-Host "`n=== Metrics Summary ===" -ForegroundColor Green
Write-Host "Package Size: $([math]::Round($metrics.packageSize / 1MB, 2)) MB"
Write-Host "Compressed Size: $([math]::Round($metrics.compressedSize / 1MB, 2)) MB"
Write-Host "File Count: $($metrics.fileCount)"
Write-Host "Assembly Count: $($metrics.assemblyCount)"
Write-Host "Build Time: $($metrics.buildTimeFormatted)"

$metricsJson = $metrics | ConvertTo-Json -Depth 10
$metricsJson | Out-File -FilePath $OutputPath -Encoding UTF8

Write-Host "`nMetrics saved to: $OutputPath" -ForegroundColor Green
