#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$ProjectPath,

    [Parameter(Mandatory)]
    [ValidateSet("android", "ios")]
    [string]$Platform,

    [Parameter(Mandatory)]
    [string]$TargetFramework,

    [Parameter(Mandatory)]
    [string]$RuntimeIdentifier,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [Parameter(Mandatory)]
    [string]$AppDisplayVersion,

    [Parameter(Mandatory)]
    [string]$AppBuildNumber,

    [string]$Configuration = "Release",

    [switch]$Publish
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

$projectFile = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse | Select-Object -First 1
if (-not $projectFile) {
    throw "No project file was found in '$ProjectPath'."
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$binlogPath = Join-Path $OutputPath "build.binlog"

switch ($Platform) {
    "android" {
        $arguments = @(
            "publish", $projectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-p:AndroidPackageFormat=aab",
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-o", $OutputPath,
            "/bl:$binlogPath"
        )

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

            $arguments += @(
                "-p:AndroidKeyStore=true",
                "-p:AndroidSigningKeyStore=$keystorePath",
                "-p:AndroidSigningKeyAlias=$keyAlias",
                "-p:AndroidSigningStorePass=env:ANDROID_SIGNING_STORE_PASS",
                "-p:AndroidSigningKeyPass=env:ANDROID_SIGNING_KEY_PASS"
            )
        } else {
            $arguments += "-p:AndroidKeyStore=false"
        }

        Write-Host "Building Android package for $($projectFile.FullName)"
        & dotnet @arguments

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
            "-p:ValidateXcodeVersion=false",
            "/bl:$binlogPath"
        )

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
        & dotnet @arguments

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
}

if (-not $package) {
    throw "Build completed but no package artifact was found for platform '$Platform'."
}

Write-Host "Package artifact: $($package.FullName)"
Write-Host "Build binlog: $binlogPath"

if ($env:GITHUB_OUTPUT) {
    "package_path=$($package.FullName)" >> $env:GITHUB_OUTPUT
    "binlog_path=$binlogPath" >> $env:GITHUB_OUTPUT
}
