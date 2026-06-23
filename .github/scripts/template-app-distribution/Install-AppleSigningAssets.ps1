#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$Variant
)

$ErrorActionPreference = "Stop"

function Assert-EnvironmentValue([string]$Name) {
    $value = [Environment]::GetEnvironmentVariable($Name)
    if ([string]::IsNullOrWhiteSpace($value)) {
        throw "Required environment variable '$Name' is not set."
    }

    return $value
}

function Get-SecretText([string]$Value) {
    $trimmed = $Value.Trim()
    if ($trimmed.StartsWith("{") -or $trimmed.StartsWith("-----BEGIN")) {
        return $Value
    }

    try {
        return [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String($trimmed))
    } catch {
        return $Value
    }
}

function Get-VariantProvisioningProfile([string]$VariantName) {
    if (-not [string]::IsNullOrWhiteSpace($env:IOS_PROVISIONING_PROFILES_JSON)) {
        $profiles = Get-SecretText $env:IOS_PROVISIONING_PROFILES_JSON | ConvertFrom-Json
        $property = $profiles.PSObject.Properties | Where-Object { $_.Name -eq $VariantName } | Select-Object -First 1
        if ($property -and -not [string]::IsNullOrWhiteSpace([string]$property.Value)) {
            return [string]$property.Value
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($env:IOS_PROVISIONING_PROFILE_BASE64)) {
        return $env:IOS_PROVISIONING_PROFILE_BASE64
    }

    throw "No iOS provisioning profile was provided for variant '$VariantName'. Set IOS_PROVISIONING_PROFILES_JSON or IOS_PROVISIONING_PROFILE_BASE64."
}

function Write-Base64File([string]$Base64Value, [string]$Path) {
    $bytes = [Convert]::FromBase64String($Base64Value.Trim())
    [System.IO.File]::WriteAllBytes($Path, $bytes)
}

if (-not $IsMacOS) {
    throw "Apple signing assets can only be installed on macOS runners."
}

$tempDirectory = Join-Path $env:RUNNER_TEMP "template-app-apple-signing"
New-Item -ItemType Directory -Path $tempDirectory -Force | Out-Null

$certificatePath = Join-Path $tempDirectory "certificate.p12"
$profilePath = Join-Path $tempDirectory "$Variant.mobileprovision"
$profilePlistPath = Join-Path $tempDirectory "$Variant.plist"
$keychainPath = Join-Path $tempDirectory "template-app-distribution.keychain-db"

Write-Base64File (Assert-EnvironmentValue "IOS_CERTIFICATE_BASE64") $certificatePath
Write-Base64File (Get-VariantProvisioningProfile $Variant) $profilePath

$certificatePassword = Assert-EnvironmentValue "IOS_CERTIFICATE_PASSWORD"
$keychainPassword = Assert-EnvironmentValue "IOS_KEYCHAIN_PASSWORD"

& security create-keychain -p $keychainPassword $keychainPath
& security set-keychain-settings -lut 21600 $keychainPath
& security unlock-keychain -p $keychainPassword $keychainPath

$existingKeychains = & security list-keychains -d user | ForEach-Object { $_.Trim().Trim('"') }
& security list-keychains -d user -s $keychainPath @existingKeychains
& security import $certificatePath -k $keychainPath -P $certificatePassword -T /usr/bin/codesign -T /usr/bin/security
& security set-key-partition-list -S apple-tool:,apple: -s -k $keychainPassword $keychainPath

& security cms -D -i $profilePath | Out-File -FilePath $profilePlistPath -Encoding utf8
$profileUuid = (& /usr/libexec/PlistBuddy -c "Print :UUID" $profilePlistPath).Trim()
$profileName = (& /usr/libexec/PlistBuddy -c "Print :Name" $profilePlistPath).Trim()

if ([string]::IsNullOrWhiteSpace($profileUuid) -or [string]::IsNullOrWhiteSpace($profileName)) {
    throw "Could not read UUID and Name from provisioning profile '$profilePath'."
}

$profilesDirectory = Join-Path $HOME "Library/MobileDevice/Provisioning Profiles"
New-Item -ItemType Directory -Path $profilesDirectory -Force | Out-Null
Copy-Item -Path $profilePath -Destination (Join-Path $profilesDirectory "$profileUuid.mobileprovision") -Force

Write-Host "Installed provisioning profile '$profileName' ($profileUuid)"

if ($env:GITHUB_ENV) {
    "IOS_CODESIGN_PROVISION=$profileName" >> $env:GITHUB_ENV
    "IOS_KEYCHAIN_PATH=$keychainPath" >> $env:GITHUB_ENV
}

if ($env:GITHUB_OUTPUT) {
    "codesign_provision=$profileName" >> $env:GITHUB_OUTPUT
    "keychain_path=$keychainPath" >> $env:GITHUB_OUTPUT
}
