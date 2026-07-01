#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$ProjectPath,

    [Parameter(Mandatory)]
    [ValidateSet("android", "ios", "maccatalyst", "windows")]
    [string]$Platform,

    [Parameter(Mandatory)]
    [string]$TargetFramework,

    [Parameter(Mandatory)]
    [AllowEmptyString()]
    [string]$RuntimeIdentifier,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [Parameter(Mandatory)]
    [string]$AppDisplayVersion,

    [Parameter(Mandatory)]
    [string]$AppBuildNumber,

    [string]$Configuration = "Release",

    [switch]$Publish,

    [switch]$CreateBinlog
)

$ErrorActionPreference = "Stop"

function Assert-EnvironmentValue([string]$Name) {
    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "Required environment variable '$Name' is not set."
    }

    return $value
}

function Write-Base64File([string]$Base64Value, [string]$Path) {
    $bytes = [Convert]::FromBase64String($Base64Value)
    [System.IO.File]::WriteAllBytes($Path, $bytes)
}

function Get-NewestBuildOutput([string]$Root, [string]$Filter, [switch]$Directory) {
    $itemType = if ($Directory) { "Directory" } else { "File" }
    return Get-ChildItem -Path $Root -Filter $Filter -Recurse -ErrorAction SilentlyContinue |
        Where-Object { $_.PSIsContainer -eq [bool]$Directory -and $_.FullName -notmatch "[\\/](obj)[\\/]" } |
        Sort-Object LastWriteTimeUtc -Descending |
        Select-Object -First 1
}

function Invoke-DotNetPublish([string[]]$Arguments, [string]$Description) {
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Description failed with exit code $LASTEXITCODE."
    }
}

function Test-IsNet11OrLater([string]$TargetFramework) {
    if ($TargetFramework -notmatch "^net(?<Major>\d+)\.") {
        return $false
    }

    return [int]$Matches.Major -ge 11
}

$projectFile = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse | Select-Object -First 1
if (-not $projectFile) {
    throw "No project file was found in '$ProjectPath'."
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$binlogPath = if ($CreateBinlog) { Join-Path $OutputPath "build.binlog" } else { $null }
$binlogArguments = if ($CreateBinlog) { @("/bl:$binlogPath") } else { @() }

switch ($Platform) {
    "android" {
        $arguments = @(
            "publish", $projectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-p:AndroidPackageFormat=aab",
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-o", $OutputPath
        ) + $binlogArguments

        if ($Publish) {
            $keystorePath = $env:ANDROID_KEYSTORE_PATH
            if ([string]::IsNullOrWhiteSpace($keystorePath)) {
                $keystoreBase64 = Assert-EnvironmentValue "ANDROID_KEYSTORE_BASE64"
                $keystorePath = Join-Path $env:RUNNER_TEMP "template-app-distribution.keystore"
                Write-Base64File $keystoreBase64 $keystorePath
            }

            $env:ANDROID_SIGNING_STORE_PASS = Assert-EnvironmentValue "ANDROID_KEYSTORE_PASSWORD"
            $env:ANDROID_SIGNING_KEY_PASS = if ([string]::IsNullOrWhiteSpace($env:ANDROID_KEY_PASSWORD)) {
                $env:ANDROID_SIGNING_STORE_PASS
            } else {
                $env:ANDROID_KEY_PASSWORD
            }

            $keyAlias = Assert-EnvironmentValue "ANDROID_KEY_ALIAS"
            $keystoreType = [Environment]::GetEnvironmentVariable("ANDROID_KEYSTORE_TYPE")

            $arguments += @(
                "-p:AndroidKeyStore=true",
                "-p:AndroidSigningKeyStore=$keystorePath",
                "-p:AndroidSigningKeyAlias=$keyAlias",
                "-p:AndroidSigningStorePass=env:ANDROID_SIGNING_STORE_PASS",
                "-p:AndroidSigningKeyPass=env:ANDROID_SIGNING_KEY_PASS"
            )

            if (-not [string]::IsNullOrWhiteSpace($keystoreType)) {
                $arguments += "-p:AndroidSigningStoreType=$keystoreType"
            }
        } else {
            $arguments += "-p:AndroidKeyStore=false"
        }

        Write-Host "Building Android package for $($projectFile.FullName)"
        Invoke-DotNetPublish $arguments "Android publish"

        $package = Get-NewestBuildOutput $ProjectPath "*.aab"
        if (-not $package) {
            $package = Get-NewestBuildOutput $OutputPath "*.aab"
        }
    }

    "ios" {
        $arguments = @(
            "publish", $projectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-r", $RuntimeIdentifier,
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-p:ValidateXcodeVersion=false"
        ) + $binlogArguments

        if (Test-IsNet11OrLater $TargetFramework) {
            $arguments += "-p:UseMonoRuntime=false"
        }

        if ($Publish) {
            $codesignKey = Assert-EnvironmentValue "IOS_CODESIGN_KEY"
            $codesignProvision = Assert-EnvironmentValue "IOS_CODESIGN_PROVISION"
            $arguments += @(
                "-p:BuildIpa=true",
                "-p:ArchiveOnBuild=true",
                "-p:CodesignKey=$codesignKey",
                "-p:CodesignProvision=$codesignProvision",
                "-o", $OutputPath
            )
        } else {
            $arguments += @(
                "-p:_RequireCodeSigning=false",
                "-p:EnableCodeSigning=false",
                "-p:CodesignKey=-",
                "-p:BuildIpa=false"
            )
        }

        Write-Host "Building iOS package for $($projectFile.FullName)"
        Invoke-DotNetPublish $arguments "iOS publish"

        if ($Publish) {
            $package = Get-NewestBuildOutput $ProjectPath "*.ipa"
            if (-not $package) {
                $package = Get-NewestBuildOutput $OutputPath "*.ipa"
            }
        } else {
            $appBundle = Get-NewestBuildOutput $ProjectPath "*.app" -Directory
            if ($appBundle) {
                $zipPath = Join-Path $OutputPath "$($appBundle.Name).zip"
                Compress-Archive -Path $appBundle.FullName -DestinationPath $zipPath -Force
                $package = Get-Item $zipPath
            }
        }
    }

    "maccatalyst" {
        $arguments = @(
            "publish", $projectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-p:MtouchLink=SdkOnly",
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-p:ValidateXcodeVersion=false"
        ) + $binlogArguments

        if (Test-IsNet11OrLater $TargetFramework) {
            $arguments += "-p:UseMonoRuntime=false"
        }

        if (-not [string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
            $arguments += @("-r", $RuntimeIdentifier)
        }

        if ($Publish) {
            $codesignKey = Assert-EnvironmentValue "APPLE_CODESIGN_KEY"
            $codesignProvision = Assert-EnvironmentValue "APPLE_CODESIGN_PROVISION"
            $packageSigningKey = Assert-EnvironmentValue "APPLE_PACKAGE_SIGNING_KEY"
            # App Store profiles include get-task-allow=false; the SDK validator still warns on that key for Mac Catalyst.
            $arguments += @(
                "-p:CreatePackage=true",
                "-p:EnableCodeSigning=true",
                "-p:EnablePackageSigning=true",
                "-p:ValidateEntitlements=disable",
                "-p:CodesignKey=$codesignKey",
                "-p:CodesignProvision=$codesignProvision",
                "-p:CodesignEntitlements=Platforms/MacCatalyst/Entitlements.plist",
                "-p:PackageSigningKey=$packageSigningKey",
                "-o", $OutputPath
            )
        } else {
            $arguments += @(
                "-p:CreatePackage=false",
                "-p:_RequireCodeSigning=false",
                "-p:EnableCodeSigning=false",
                "-p:CodesignKey=-",
                "-o", $OutputPath
            )
        }

        Write-Host "Building Mac Catalyst package for $($projectFile.FullName)"
        Invoke-DotNetPublish $arguments "Mac Catalyst publish"

        if ($Publish) {
            $package = Get-NewestBuildOutput $ProjectPath "*.pkg"
            if (-not $package) {
                $package = Get-NewestBuildOutput $OutputPath "*.pkg"
            }
        } else {
            $appBundle = Get-NewestBuildOutput $OutputPath "*.app" -Directory
            if (-not $appBundle) {
                $appBundle = Get-NewestBuildOutput $ProjectPath "*.app" -Directory
            }

            if ($appBundle) {
                $zipPath = Join-Path $OutputPath "$($appBundle.Name).zip"
                Compress-Archive -Path $appBundle.FullName -DestinationPath $zipPath -Force
                $package = Get-Item $zipPath
            }
        }
    }

    "windows" {
        $publishOutputPath = Join-Path $OutputPath "publish"
        Remove-Item -Path $publishOutputPath -Recurse -Force -ErrorAction SilentlyContinue
        New-Item -ItemType Directory -Path $publishOutputPath -Force | Out-Null

        $arguments = @(
            "publish", $projectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-p:RuntimeIdentifierOverride=$RuntimeIdentifier",
            "-p:WindowsPackageType=None",
            "-p:WindowsAppSDKSelfContained=true",
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-o", $publishOutputPath
        ) + $binlogArguments

        Write-Host "Building Windows unpackaged app for $($projectFile.FullName)"
        Invoke-DotNetPublish $arguments "Windows unpackaged publish"

        $zipPath = Join-Path $OutputPath "$($projectFile.BaseName)-windows-unpackaged.zip"
        Remove-Item -Path $zipPath -Force -ErrorAction SilentlyContinue
        Compress-Archive -Path (Join-Path $publishOutputPath "*") -DestinationPath $zipPath -Force
        $package = Get-Item $zipPath
    }
}

if (-not $package) {
    throw "Build completed but no package artifact was found for platform '$Platform'."
}

Write-Host "Package artifact: $($package.FullName)"
if ($CreateBinlog) {
    Write-Host "Build binlog: $binlogPath"
}

if ($env:GITHUB_OUTPUT) {
    "package_path=$($package.FullName)" >> $env:GITHUB_OUTPUT
    if ($CreateBinlog) {
        "binlog_path=$binlogPath" >> $env:GITHUB_OUTPUT
    }
}
