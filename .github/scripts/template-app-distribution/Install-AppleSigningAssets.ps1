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
$installedProfileExtension = if ($Platform -eq "maccatalyst") { ".provisionprofile" } else { ".mobileprovision" }
Copy-Item -Path $profilePath -Destination (Join-Path $profilesDirectory "$profileUuid$installedProfileExtension") -Force

$codesignProvision = if ($Platform -eq "maccatalyst") {
    $profileUuid
} else {
    $profileName
}

Write-Host "Installed provisioning profile '$profileName' ($profileUuid)"
Write-Host "Using code signing identity '$codesignIdentity'"
if ($Platform -eq "maccatalyst") {
    Write-Host "Using package signing identity '$packageSigningIdentity'"
}

# Optional (secret-gated): Developer ID Application identity + provisioning profile so we can
# also produce a notarizable, directly-launchable macOS app (the Mac App Store .pkg cannot be
# launched outside the store).
$developerIdIdentity = $null
$developerIdProvisionValue = $null
if ($Platform -eq "maccatalyst" -and -not [string]::IsNullOrWhiteSpace($env:APPLE_DEVELOPERID_CERTIFICATE_BASE64)) {
    $developerIdCertPath = Join-Path $tempDirectory "developer-id-certificate.p12"
    Write-Base64File $env:APPLE_DEVELOPERID_CERTIFICATE_BASE64 $developerIdCertPath
    $developerIdCertPassword = if ([string]::IsNullOrWhiteSpace($env:APPLE_DEVELOPERID_CERTIFICATE_PASSWORD)) {
        $certificatePassword
    } else {
        $env:APPLE_DEVELOPERID_CERTIFICATE_PASSWORD
    }

    & security import $developerIdCertPath -k $keychainPath -P $developerIdCertPassword -T /usr/bin/codesign -T /usr/bin/security
    & security set-key-partition-list -S apple-tool:,apple: -s -k $keychainPassword $keychainPath | Out-Null

    $developerIdIdentity = (Get-SecurityIdentities @("-v")) |
        Where-Object { $_ -match "Developer ID Application" } |
        Select-Object -First 1
    if ([string]::IsNullOrWhiteSpace($developerIdIdentity)) {
        throw "APPLE_DEVELOPERID_CERTIFICATE_BASE64 was provided but no 'Developer ID Application' identity was found in it."
    }

    if ([string]::IsNullOrWhiteSpace($env:APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64)) {
        throw "APPLE_DEVELOPERID_CERTIFICATE_BASE64 also requires APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64 (a Developer ID Mac Catalyst provisioning profile)."
    }

    $developerIdProfilePath = Join-Path $tempDirectory "$Variant-developerid.provisionprofile"
    Write-Base64File $env:APPLE_DEVELOPERID_PROVISIONING_PROFILE_BASE64 $developerIdProfilePath
    $developerIdPlistPath = Join-Path $tempDirectory "$Variant-developerid.plist"
    & security cms -D -i $developerIdProfilePath | Out-File -FilePath $developerIdPlistPath -Encoding utf8
    $developerIdUuid = (& /usr/libexec/PlistBuddy -c "Print :UUID" $developerIdPlistPath).Trim()
    if ([string]::IsNullOrWhiteSpace($developerIdUuid)) {
        throw "Could not read UUID from the Developer ID provisioning profile."
    }

    Copy-Item -Path $developerIdProfilePath -Destination (Join-Path $profilesDirectory "$developerIdUuid.provisionprofile") -Force
    $developerIdProvisionValue = $developerIdUuid
    Write-Host "Installed Developer ID provisioning profile ($developerIdUuid)"
    Write-Host "Using Developer ID signing identity '$developerIdIdentity'"
}

# Optional (secret-gated): ad-hoc distribution provisioning profile so we can also produce a
# directly-installable IPA (the App Store IPA cannot be sideloaded). Reuses the Apple
# Distribution certificate already imported above.
$adhocProvisionValue = $null
if ($Platform -eq "ios" -and -not [string]::IsNullOrWhiteSpace($env:APPLE_ADHOC_PROVISIONING_PROFILE_BASE64)) {
    $adhocProfilePath = Join-Path $tempDirectory "$Variant-adhoc.mobileprovision"
    Write-Base64File $env:APPLE_ADHOC_PROVISIONING_PROFILE_BASE64 $adhocProfilePath
    $adhocPlistPath = Join-Path $tempDirectory "$Variant-adhoc.plist"
    & security cms -D -i $adhocProfilePath | Out-File -FilePath $adhocPlistPath -Encoding utf8
    $adhocUuid = (& /usr/libexec/PlistBuddy -c "Print :UUID" $adhocPlistPath).Trim()
    $adhocName = (& /usr/libexec/PlistBuddy -c "Print :Name" $adhocPlistPath).Trim()
    if ([string]::IsNullOrWhiteSpace($adhocUuid) -or [string]::IsNullOrWhiteSpace($adhocName)) {
        throw "Could not read UUID/Name from the ad-hoc provisioning profile."
    }

    Copy-Item -Path $adhocProfilePath -Destination (Join-Path $profilesDirectory "$adhocUuid.mobileprovision") -Force
    $adhocProvisionValue = $adhocName
    Write-Host "Installed ad-hoc provisioning profile '$adhocName' ($adhocUuid)"
}

if ($env:GITHUB_ENV) {
    "IOS_CODESIGN_KEY=$codesignIdentity" >> $env:GITHUB_ENV
    "IOS_CODESIGN_PROVISION=$codesignProvision" >> $env:GITHUB_ENV
    "APPLE_CODESIGN_KEY=$codesignIdentity" >> $env:GITHUB_ENV
    "APPLE_CODESIGN_PROVISION=$codesignProvision" >> $env:GITHUB_ENV
    if ($Platform -eq "maccatalyst") {
        "APPLE_PACKAGE_SIGNING_KEY=$packageSigningIdentity" >> $env:GITHUB_ENV
    }
    if (-not [string]::IsNullOrWhiteSpace($developerIdIdentity)) {
        "APPLE_DEVELOPERID_CODESIGN_KEY=$developerIdIdentity" >> $env:GITHUB_ENV
        "APPLE_DEVELOPERID_CODESIGN_PROVISION=$developerIdProvisionValue" >> $env:GITHUB_ENV
    }
    if (-not [string]::IsNullOrWhiteSpace($adhocProvisionValue)) {
        "IOS_ADHOC_CODESIGN_PROVISION=$adhocProvisionValue" >> $env:GITHUB_ENV
    }
    "IOS_KEYCHAIN_PATH=$keychainPath" >> $env:GITHUB_ENV
}

if ($env:GITHUB_OUTPUT) {
    "codesign_key=$codesignIdentity" >> $env:GITHUB_OUTPUT
    "codesign_provision=$codesignProvision" >> $env:GITHUB_OUTPUT
    if ($Platform -eq "maccatalyst") {
        "package_signing_key=$packageSigningIdentity" >> $env:GITHUB_OUTPUT
    }
    if (-not [string]::IsNullOrWhiteSpace($developerIdIdentity)) {
        "developerid_codesign_key=$developerIdIdentity" >> $env:GITHUB_OUTPUT
        "developerid_codesign_provision=$developerIdProvisionValue" >> $env:GITHUB_OUTPUT
    }
    if (-not [string]::IsNullOrWhiteSpace($adhocProvisionValue)) {
        "adhoc_codesign_provision=$adhocProvisionValue" >> $env:GITHUB_OUTPUT
    }
    "keychain_path=$keychainPath" >> $env:GITHUB_OUTPUT
}
