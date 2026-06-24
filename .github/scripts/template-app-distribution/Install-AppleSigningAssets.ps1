#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$Variant,

    [ValidateSet("ios", "maccatalyst")]
    [string]$Platform = "ios"
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
    if (-not [string]::IsNullOrWhiteSpace($env:APPLE_PROVISIONING_PROFILES_JSON)) {
        $profiles = Get-SecretText $env:APPLE_PROVISIONING_PROFILES_JSON | ConvertFrom-Json
        $property = $profiles.PSObject.Properties | Where-Object { $_.Name -eq $VariantName } | Select-Object -First 1
        if ($property -and -not [string]::IsNullOrWhiteSpace([string]$property.Value)) {
            return [string]$property.Value
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($env:APPLE_PROVISIONING_PROFILE_BASE64)) {
        return $env:APPLE_PROVISIONING_PROFILE_BASE64
    }

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

    throw "No Apple provisioning profile was provided for variant '$VariantName' on '$Platform'. Set APPLE_PROVISIONING_PROFILES_JSON or APPLE_PROVISIONING_PROFILE_BASE64."
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
$keychainPassword = [Environment]::GetEnvironmentVariable("IOS_KEYCHAIN_PASSWORD")
if ([string]::IsNullOrWhiteSpace($keychainPassword)) {
    $keychainPassword = [guid]::NewGuid().ToString("N")
}

& security create-keychain -p $keychainPassword $keychainPath
& security set-keychain-settings -lut 21600 $keychainPath
& security unlock-keychain -p $keychainPassword $keychainPath

$existingKeychains = & security list-keychains -d user | ForEach-Object { $_.Trim().Trim('"') }
& security list-keychains -d user -s $keychainPath @existingKeychains
& security import $certificatePath -k $keychainPath -P $certificatePassword -T /usr/bin/codesign -T /usr/bin/security

if ($Platform -eq "maccatalyst" -and -not [string]::IsNullOrWhiteSpace($env:MAC_INSTALLER_CERTIFICATE_BASE64)) {
    $installerCertificatePath = Join-Path $tempDirectory "mac-installer-certificate.p12"
    Write-Base64File $env:MAC_INSTALLER_CERTIFICATE_BASE64 $installerCertificatePath
    $installerCertificatePassword = if ([string]::IsNullOrWhiteSpace($env:MAC_INSTALLER_CERTIFICATE_PASSWORD)) {
        $certificatePassword
    } else {
        $env:MAC_INSTALLER_CERTIFICATE_PASSWORD
    }

    & security import $installerCertificatePath -k $keychainPath -P $installerCertificatePassword -T /usr/bin/productbuild -T /usr/bin/security
}

& security set-key-partition-list -S apple-tool:,apple: -s -k $keychainPassword $keychainPath

function Get-SecurityIdentities([string[]]$Arguments) {
    $result = @()
    foreach ($line in (& security find-identity @Arguments $keychainPath)) {
        if ($line -match '"(.+)"') {
            $result += $Matches[1]
        }
    }

    return $result
}

$identities = Get-SecurityIdentities @("-v", "-p", "codesigning")
$allIdentities = Get-SecurityIdentities @("-v")

$codesignIdentity = $identities |
    Where-Object { $_ -match "Apple Distribution|iPhone Distribution" } |
    Select-Object -First 1

if ([string]::IsNullOrWhiteSpace($codesignIdentity)) {
    $codesignIdentity = $identities | Select-Object -First 1
}

if ([string]::IsNullOrWhiteSpace($codesignIdentity)) {
    throw "No code signing identity was found in the imported certificate."
}

$packageSigningIdentity = $null
if ($Platform -eq "maccatalyst") {
    $packageSigningIdentity = $allIdentities |
        Where-Object { $_ -match "3rd Party Mac Developer Installer|Mac Installer Distribution" } |
        Select-Object -First 1

    if ([string]::IsNullOrWhiteSpace($packageSigningIdentity)) {
        throw "No Mac installer package signing identity was found. Import a p12 containing a '3rd Party Mac Developer Installer' identity with MAC_INSTALLER_CERTIFICATE_BASE64."
    }
}

foreach ($line in (& security find-identity -v -p codesigning $keychainPath)) {
    if ($line -match '"(.+)"') {
        Write-Host "Code signing identity: $($Matches[1])"
    }
}

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
Write-Host "Using code signing identity '$codesignIdentity'"
if ($Platform -eq "maccatalyst") {
    Write-Host "Using package signing identity '$packageSigningIdentity'"
}

if ($env:GITHUB_ENV) {
    "IOS_CODESIGN_KEY=$codesignIdentity" >> $env:GITHUB_ENV
    "IOS_CODESIGN_PROVISION=$profileName" >> $env:GITHUB_ENV
    "APPLE_CODESIGN_KEY=$codesignIdentity" >> $env:GITHUB_ENV
    "APPLE_CODESIGN_PROVISION=$profileName" >> $env:GITHUB_ENV
    if ($Platform -eq "maccatalyst") {
        "APPLE_PACKAGE_SIGNING_KEY=$packageSigningIdentity" >> $env:GITHUB_ENV
    }
    "IOS_KEYCHAIN_PATH=$keychainPath" >> $env:GITHUB_ENV
}

if ($env:GITHUB_OUTPUT) {
    "codesign_key=$codesignIdentity" >> $env:GITHUB_OUTPUT
    "codesign_provision=$profileName" >> $env:GITHUB_OUTPUT
    if ($Platform -eq "maccatalyst") {
        "package_signing_key=$packageSigningIdentity" >> $env:GITHUB_OUTPUT
    }
    "keychain_path=$keychainPath" >> $env:GITHUB_OUTPUT
}
