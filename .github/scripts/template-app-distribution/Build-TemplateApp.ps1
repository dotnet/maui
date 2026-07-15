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

function Compress-AppBundle([string]$AppBundlePath, [string]$ZipPath) {
    # macOS .app bundles rely on symlinks (e.g. a framework's Versions/Current),
    # executable permission bits, and embedded (ad-hoc) code signatures. PowerShell's
    # Compress-Archive drops symlinks + exec bits and mangles the framework layout,
    # which invalidates the code signature so the unzipped app is SIGKILL'd on launch
    # with "Code Signature Invalid" / "Invalid Page". ditto preserves the bundle
    # exactly (symlinks, perms, xattrs, bytes), keeping the app launchable.
    Remove-Item -Path $ZipPath -Force -ErrorAction SilentlyContinue
    if (Get-Command ditto -ErrorAction SilentlyContinue) {
        & ditto -c -k --sequesterRsrc --keepParent $AppBundlePath $ZipPath
        if ($LASTEXITCODE -ne 0) {
            throw "ditto failed to archive '$AppBundlePath' (exit code $LASTEXITCODE)."
        }
    } else {
        Compress-Archive -Path $AppBundlePath -DestinationPath $ZipPath -Force
    }
}

function Repair-AppleAdhocSignature([string]$AppBundlePath) {
    # The .NET iOS-Simulator / Mac Catalyst build leaves the app *linker-signed* ad-hoc
    # (flags 0x20002), while its bundled native libraries (*.dylib/*.so — e.g. libcoreclr,
    # libxamarin-dotnet-coreclr) carry a mix of signatures. macOS 15+/26 and the modern iOS
    # Simulator refuse to launch that inconsistent bundle: dyld kills it at load with
    # CODESIGNING "Invalid Page" / SIGKILL "Code Signature Invalid" (empirically reproduced
    # on macOS 26.5.2 / M2 for both a Mac Catalyst .app launch and a Simulator install+launch).
    # Re-signing every Mach-O ad-hoc from the inside out (loose dylibs first, since
    # `codesign --deep` does not reach every nested lib, then a deep sign of the whole app)
    # gives all components a consistent, valid ad-hoc signature (flags 0x2) so the app
    # launches. It is still ad-hoc (not notarized): the Simulator accepts ad-hoc directly, and
    # a Mac tester clears quarantine / uses "Open Anyway". The notarized Developer ID path
    # (secret-gated, below) is the seamless option for a direct-download macOS launch.
    if (-not (Get-Command codesign -ErrorAction SilentlyContinue)) {
        Write-Warning "codesign not available; skipping ad-hoc re-sign of '$AppBundlePath'."
        return
    }
    Get-ChildItem -Path $AppBundlePath -Recurse -Include *.dylib, *.so -File -ErrorAction SilentlyContinue |
        ForEach-Object { & codesign --force --sign - $_.FullName 2>$null }
    & codesign --force --deep --sign - $AppBundlePath
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Ad-hoc re-sign of '$AppBundlePath' failed (exit $LASTEXITCODE); the app may not launch until it is signed."
    }
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

function Add-NativeAotArguments([string[]]$Arguments) {
    return $Arguments + @(
        "-p:PublishAot=true",
        "-p:PublishAotUsingRuntimePack=true",
        "-p:_IsPublishing=true",
        "-p:IlcTreatWarningsAsErrors=false",
        "-p:TrimmerSingleWarn=false"
    )
}

function Invoke-MacNotarization([string]$AppBundlePath) {
    # Notarize + staple a Developer ID-signed .app so Gatekeeper lets it launch on other Macs.
    # Reuses the App Store Connect API key already configured for TestFlight publishing.
    $keyId = Assert-EnvironmentValue "APPSTORE_CONNECT_KEY_ID"
    $issuerId = Assert-EnvironmentValue "APPSTORE_CONNECT_ISSUER_ID"

    $keyPath = $env:APPSTORE_CONNECT_PRIVATE_KEY_PATH
    if ([string]::IsNullOrWhiteSpace($keyPath) -or -not (Test-Path $keyPath)) {
        $rawKey = Assert-EnvironmentValue "APPSTORE_CONNECT_PRIVATE_KEY"
        $keyPath = Join-Path $env:RUNNER_TEMP "notarytool-key.p8"
        $trimmed = $rawKey.Trim()
        if ($trimmed.StartsWith("-----BEGIN")) {
            Set-Content -Path $keyPath -Value $rawKey -NoNewline
        } else {
            [System.IO.File]::WriteAllBytes($keyPath, [Convert]::FromBase64String($trimmed))
        }
    }

    $notarizeZip = Join-Path ([System.IO.Path]::GetDirectoryName($AppBundlePath)) "notarize-upload.zip"
    Remove-Item -Path $notarizeZip -Force -ErrorAction SilentlyContinue
    & ditto -c -k --keepParent $AppBundlePath $notarizeZip
    if ($LASTEXITCODE -ne 0) { throw "Failed to create the notarization upload archive." }

    Write-Host "Submitting '$AppBundlePath' to the Apple notary service (this can take a few minutes)..."
    & xcrun notarytool submit $notarizeZip --key $keyPath --key-id $keyId --issuer $issuerId --wait
    if ($LASTEXITCODE -ne 0) { throw "notarytool submit failed." }

    & xcrun stapler staple $AppBundlePath
    if ($LASTEXITCODE -ne 0) { throw "stapler staple failed." }

    Remove-Item -Path $notarizeZip -Force -ErrorAction SilentlyContinue
}

function New-MacCatalystDeveloperIdSideload {
    param(
        [System.IO.FileInfo]$ProjectFile,
        [string]$TargetFramework,
        [string]$Configuration,
        [string]$OutputPath,
        [string]$AppDisplayVersion,
        [string]$AppBuildNumber,
        [string]$RuntimeIdentifier,
        [switch]$UseNet11OrLater
    )

    # Secret-gated. Only runs when a Developer ID Application identity + provisioning profile
    # were installed (Install-AppleSigningAssets.ps1). Produces a Developer ID-signed,
    # notarized, stapled .app that launches directly on any Mac. The Mac App Store .pkg is
    # killed with "Code Signature Invalid" when installed outside the store, so it stays
    # TestFlight-only.
    $devIdKey = [Environment]::GetEnvironmentVariable("APPLE_DEVELOPERID_CODESIGN_KEY")
    $devIdProvision = [Environment]::GetEnvironmentVariable("APPLE_DEVELOPERID_CODESIGN_PROVISION")
    if ([string]::IsNullOrWhiteSpace($devIdKey) -or [string]::IsNullOrWhiteSpace($devIdProvision)) {
        Write-Host "No Developer ID signing assets provided; skipping the notarized macOS sideload build."
        return $null
    }

    try {
        $devIdOutput = Join-Path $OutputPath "developer-id"
        New-Item -ItemType Directory -Path $devIdOutput -Force | Out-Null

        $devIdArgs = @(
            "publish", $ProjectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-p:MtouchLink=SdkOnly",
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-p:ValidateXcodeVersion=false",
            "-p:CreatePackage=false",
            "-p:EnableCodeSigning=true",
            "-p:EnableHardenedRuntime=true",
            "-p:ValidateEntitlements=disable",
            "-p:CodesignKey=$devIdKey",
            "-p:CodesignProvision=$devIdProvision",
            "-p:CodesignEntitlements=Platforms/MacCatalyst/Entitlements.plist",
            "-o", $devIdOutput
        )

        if ($UseNet11OrLater) { $devIdArgs += "-p:UseMonoRuntime=false" }
        if (-not [string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
            $devIdArgs += @("-r", $RuntimeIdentifier)
        } elseif ($UseNet11OrLater) {
            # Match the dry-run: pin arm64-native for net11+ (the universal multi-RID publish
            # trips PublishReadyToRun inference). arm64 runs natively on Apple Silicon Macs.
            $devIdArgs += @("-r", "maccatalyst-arm64")
        }

        Write-Host "Building Developer ID (notarizable) Mac Catalyst app for $($ProjectFile.FullName)"
        Invoke-DotNetPublish $devIdArgs "Mac Catalyst Developer ID publish"

        $devIdApp = Get-NewestBuildOutput $devIdOutput "*.app" -Directory
        if (-not $devIdApp) {
            Write-Warning "Developer ID publish did not produce a .app; skipping notarized sideload."
            return $null
        }

        # Hardened runtime is required for notarization. Re-sign deeply, preserving entitlements.
        $entitlementsPath = Join-Path $devIdOutput "developerid-entitlements.plist"
        & codesign -d "--entitlements" ":$entitlementsPath" $devIdApp.FullName 2>$null
        $signArgs = @("--force", "--deep", "--options", "runtime", "--timestamp", "--sign", $devIdKey)
        if (Test-Path $entitlementsPath) { $signArgs += @("--entitlements", $entitlementsPath) }
        & codesign @signArgs $devIdApp.FullName
        if ($LASTEXITCODE -ne 0) { throw "Developer ID hardened-runtime re-sign failed." }

        Invoke-MacNotarization $devIdApp.FullName

        $devIdZip = Join-Path $OutputPath "$($devIdApp.BaseName)-macos-developerid.zip"
        Remove-Item -Path $devIdZip -Force -ErrorAction SilentlyContinue
        & ditto -c -k --keepParent $devIdApp.FullName $devIdZip
        if ($LASTEXITCODE -ne 0) { throw "Failed to archive the notarized macOS app." }

        Write-Host "Notarized macOS sideload artifact: $devIdZip"
        return (Get-Item $devIdZip)
    } catch {
        Write-Warning "Developer ID / notarized macOS sideload build failed: $($_.Exception.Message). The Mac App Store .pkg (TestFlight) build is unaffected."
        return $null
    }
}

function New-IosAdHocSideload {
    param(
        [System.IO.FileInfo]$ProjectFile,
        [string]$TargetFramework,
        [string]$Configuration,
        [string]$RuntimeIdentifier,
        [string]$OutputPath,
        [string]$AppDisplayVersion,
        [string]$AppBuildNumber
    )

    # Secret-gated. Only runs when an ad-hoc distribution provisioning profile was installed
    # (Install-AppleSigningAssets.ps1). Produces an ad-hoc-signed IPA that installs directly on
    # registered devices. The App Store IPA fails direct install with "Attempted to install a
    # Beta profile without the proper entitlement", so it stays TestFlight-only.
    $adhocProvision = [Environment]::GetEnvironmentVariable("IOS_ADHOC_CODESIGN_PROVISION")
    if ([string]::IsNullOrWhiteSpace($adhocProvision)) {
        Write-Host "No ad-hoc provisioning profile provided; skipping the sideloadable iOS IPA."
        return $null
    }

    try {
        $adhocKey = Assert-EnvironmentValue "IOS_CODESIGN_KEY"
        $adhocOutput = Join-Path $OutputPath "adhoc"
        New-Item -ItemType Directory -Path $adhocOutput -Force | Out-Null

        $adhocArgs = @(
            "publish", $ProjectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-r", $RuntimeIdentifier,
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-p:ValidateXcodeVersion=false",
            "-p:BuildIpa=true",
            "-p:ArchiveOnBuild=true",
            "-p:CodesignKey=$adhocKey",
            "-p:CodesignProvision=$adhocProvision",
            "-o", $adhocOutput
        )

        if (Test-IsNet11OrLater $TargetFramework) {
            $adhocArgs += "-p:UseMonoRuntime=false"
            $adhocArgs = Add-NativeAotArguments $adhocArgs
        }

        Write-Host "Building ad-hoc iOS IPA (sideloadable on registered devices) for $($ProjectFile.FullName)"
        Invoke-DotNetPublish $adhocArgs "iOS ad-hoc publish"

        $adhocIpa = Get-NewestBuildOutput $adhocOutput "*.ipa"
        if (-not $adhocIpa) { $adhocIpa = Get-NewestBuildOutput $ProjectFile.DirectoryName "*.ipa" }
        if (-not $adhocIpa) {
            Write-Warning "Ad-hoc publish did not produce an IPA; skipping the sideloadable iOS artifact."
            return $null
        }

        Write-Host "Ad-hoc iOS sideload artifact: $($adhocIpa.FullName)"
        return $adhocIpa
    } catch {
        Write-Warning "Ad-hoc iOS sideload build failed: $($_.Exception.Message). The App Store IPA (TestFlight) build is unaffected."
        return $null
    }
}

function New-IosUnsignedDeviceIpa {
    param(
        [System.IO.FileInfo]$ProjectFile,
        [string]$TargetFramework,
        [string]$Configuration,
        [string]$RuntimeIdentifier,
        [string]$OutputPath,
        [string]$AppDisplayVersion,
        [string]$AppBuildNumber
    )

    # A dry-run has no Apple signing secrets, so we cannot produce an IPA that installs
    # *directly* on a device (that needs an ad-hoc profile listing the device UDID, or
    # TestFlight - both live on the secret-gated publish path). We can still build the
    # unsigned device (iphoneos/arm64) .app and wrap it as a Payload/*.app IPA so a tester
    # can install it with AltStore or Sideloadly, which re-signs the app with the tester's
    # own Apple ID. Without this the iOS dry-run only produced a Simulator .app - i.e. there
    # was no .ipa in the artifact at all, which is exactly what testers reported missing.
    try {
        if ([string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
            $RuntimeIdentifier = "ios-arm64"
        }

        $deviceArgs = @(
            "build", $ProjectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-r", $RuntimeIdentifier,
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber",
            "-p:ValidateXcodeVersion=false",
            "-p:EnableCodeSigning=false",
            "-p:_RequireCodeSigning=false",
            "-p:CodesignKey=-",
            "-p:BuildIpa=false"
        )

        if (Test-IsNet11OrLater $TargetFramework) {
            # net11+ iOS can't build with Mono (NETSDK1242); use CoreCLR. NativeAOT is not
            # needed for an unsigned, sideload-only artifact.
            $deviceArgs += "-p:UseMonoRuntime=false"
        }

        Write-Host "Building unsigned iOS device app (for a sideloadable IPA) for $($ProjectFile.FullName)"
        Invoke-DotNetPublish $deviceArgs "iOS unsigned device build"

        $ridEscaped = [regex]::Escape($RuntimeIdentifier)
        $deviceApp = Get-ChildItem -Path $ProjectFile.DirectoryName -Filter "*.app" -Recurse -Directory -ErrorAction SilentlyContinue |
            Where-Object { $_.FullName -match "[\\/]$ridEscaped[\\/]" -and $_.FullName -notmatch "[\\/]obj[\\/]" } |
            Sort-Object LastWriteTimeUtc -Descending |
            Select-Object -First 1

        if (-not $deviceApp) {
            Write-Warning "Unsigned iOS device build did not produce a device .app; skipping the sideloadable IPA."
            return $null
        }

        $stageRoot = Join-Path $OutputPath "device-ipa"
        Remove-Item -Path $stageRoot -Recurse -Force -ErrorAction SilentlyContinue
        $payloadDir = Join-Path $stageRoot "Payload"
        New-Item -ItemType Directory -Path $payloadDir -Force | Out-Null

        $stagedApp = Join-Path $payloadDir $deviceApp.Name
        if (Get-Command ditto -ErrorAction SilentlyContinue) {
            & ditto $deviceApp.FullName $stagedApp
            if ($LASTEXITCODE -ne 0) { throw "ditto failed to stage the device .app (exit $LASTEXITCODE)." }
        } else {
            Copy-Item -Path $deviceApp.FullName -Destination $stagedApp -Recurse -Force
        }

        # An IPA is a zip whose root contains Payload/<App>.app. --keepParent embeds the
        # "Payload" directory as the top-level entry, giving a valid IPA layout.
        $ipaPath = Join-Path $OutputPath "$($deviceApp.BaseName).ipa"
        Remove-Item -Path $ipaPath -Force -ErrorAction SilentlyContinue
        if (Get-Command ditto -ErrorAction SilentlyContinue) {
            & ditto -c -k --keepParent $payloadDir $ipaPath
            if ($LASTEXITCODE -ne 0) { throw "ditto failed to archive the IPA (exit $LASTEXITCODE)." }
        } else {
            Compress-Archive -Path $payloadDir -DestinationPath $ipaPath -Force
        }

        Write-Host "Unsigned iOS device IPA (install with AltStore/Sideloadly): $ipaPath"
        return Get-Item $ipaPath
    } catch {
        Write-Warning "Unsigned iOS device IPA build failed: $($_.Exception.Message). The Simulator app artifact is unaffected."
        return $null
    }
}

$projectFile = Get-ChildItem -Path $ProjectPath -Filter "*.csproj" -Recurse | Select-Object -First 1
if (-not $projectFile) {
    throw "No project file was found in '$ProjectPath'."
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
$binlogPath = if ($CreateBinlog) { Join-Path $OutputPath "build.binlog" } else { $null }
$binlogArguments = if ($CreateBinlog) { @("/bl:$binlogPath") } else { @() }

# $package          => the "store" package (aab/ipa/pkg/zip) consumed by the Play/TestFlight steps.
# $sideloadPackage  => a directly-installable artifact for testers (apk / ad-hoc ipa / notarized app).
#                      When no distinct sideload artifact exists it falls back to $package on emit.
# $additionalPackage => an optional extra artifact uploaded alongside the sideload one (e.g. the iOS
#                      Simulator .app.zip that accompanies the device .ipa on a dry-run).
$sideloadPackage = $null
$additionalPackage = $null

switch ($Platform) {
    "android" {
        $commonArgs = @(
            "publish", $projectFile.FullName,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-p:ApplicationDisplayVersion=$AppDisplayVersion",
            "-p:ApplicationVersion=$AppBuildNumber"
        )

        # Signing arguments shared by the APK (sideload) and AAB (Play) builds.
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

            $signingArgs = @(
                "-p:AndroidKeyStore=true",
                "-p:AndroidSigningKeyStore=$keystorePath",
                "-p:AndroidSigningKeyAlias=$keyAlias",
                "-p:AndroidSigningStorePass=env:ANDROID_SIGNING_STORE_PASS",
                "-p:AndroidSigningKeyPass=env:ANDROID_SIGNING_KEY_PASS"
            )

            if (-not [string]::IsNullOrWhiteSpace($keystoreType)) {
                $signingArgs += "-p:AndroidSigningStoreType=$keystoreType"
            }
        } else {
            # A debug-signed APK is directly installable on devices/emulators for testing.
            $signingArgs = @("-p:AndroidKeyStore=false")
        }

        # 1) Always build an installable APK. This is what testers sideload; an .aab cannot be
        #    installed directly (only Google Play can consume it), which is why the previous
        #    artifact ZIP had nothing installable in it.
        $apkOutput = Join-Path $OutputPath "apk"
        New-Item -ItemType Directory -Path $apkOutput -Force | Out-Null
        $apkArgs = $commonArgs + @("-p:AndroidPackageFormat=apk", "-o", $apkOutput) + $signingArgs + $binlogArguments

        Write-Host "Building installable Android APK for $($projectFile.FullName)"
        Invoke-DotNetPublish $apkArgs "Android APK publish"

        $apkPackage = Get-NewestBuildOutput $apkOutput "*-Signed.apk"
        if (-not $apkPackage) { $apkPackage = Get-NewestBuildOutput $apkOutput "*.apk" }
        if (-not $apkPackage) { $apkPackage = Get-NewestBuildOutput $ProjectPath "*-Signed.apk" }
        if (-not $apkPackage) { $apkPackage = Get-NewestBuildOutput $ProjectPath "*.apk" }
        $sideloadPackage = $apkPackage

        if ($Publish) {
            # 2) Also build an .aab for the Google Play upload step.
            $aabOutput = Join-Path $OutputPath "aab"
            New-Item -ItemType Directory -Path $aabOutput -Force | Out-Null
            $aabArgs = $commonArgs + @("-p:AndroidPackageFormat=aab", "-o", $aabOutput) + $signingArgs

            Write-Host "Building Android App Bundle (Google Play) for $($projectFile.FullName)"
            Invoke-DotNetPublish $aabArgs "Android AAB publish"

            $package = Get-NewestBuildOutput $aabOutput "*.aab"
            if (-not $package) { $package = Get-NewestBuildOutput $ProjectPath "*.aab" }
        } else {
            # Dry-run: the installable APK is the primary artifact.
            $package = $apkPackage
        }
    }

    "ios" {
        if ($Publish) {
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
                $arguments = Add-NativeAotArguments $arguments
            }

            $codesignKey = Assert-EnvironmentValue "IOS_CODESIGN_KEY"
            $codesignProvision = Assert-EnvironmentValue "IOS_CODESIGN_PROVISION"
            $arguments += @(
                "-p:BuildIpa=true",
                "-p:ArchiveOnBuild=true",
                "-p:CodesignKey=$codesignKey",
                "-p:CodesignProvision=$codesignProvision",
                "-o", $OutputPath
            )

            Write-Host "Building iOS package for $($projectFile.FullName)"
            Invoke-DotNetPublish $arguments "iOS publish"

            $package = Get-NewestBuildOutput $ProjectPath "*.ipa"
            if (-not $package) {
                $package = Get-NewestBuildOutput $OutputPath "*.ipa"
            }

            $sideloadPackage = New-IosAdHocSideload `
                -ProjectFile $projectFile `
                -TargetFramework $TargetFramework `
                -Configuration $Configuration `
                -RuntimeIdentifier $RuntimeIdentifier `
                -OutputPath $OutputPath `
                -AppDisplayVersion $AppDisplayVersion `
                -AppBuildNumber $AppBuildNumber
        } else {
            # A dry-run has no signing secrets. We produce two complementary iOS artifacts:
            #
            #   1. A Simulator app (.app.zip) - runnable in the iOS Simulator on any Mac, so a
            #      maintainer can smoke-test the build with no device. `dotnet publish` rejects
            #      simulator RIDs, so use `dotnet build` + iossimulator-arm64 (macos-15 runners
            #      and Apple Silicon testers are arm64).
            #   2. An unsigned device IPA (.ipa) - what testers install on real hardware via
            #      AltStore/Sideloadly (which re-signs with their own Apple ID). A *directly*
            #      installable IPA needs an ad-hoc profile with the device UDID, or TestFlight,
            #      both of which are on the secret-gated publish path.
            $simulatorRuntimeIdentifier = "iossimulator-arm64"
            $arguments = @(
                "build", $projectFile.FullName,
                "-f", $TargetFramework,
                "-c", $Configuration,
                "-r", $simulatorRuntimeIdentifier,
                "-p:ApplicationDisplayVersion=$AppDisplayVersion",
                "-p:ApplicationVersion=$AppBuildNumber",
                "-p:ValidateXcodeVersion=false",
                "-p:_RequireCodeSigning=false",
                "-p:EnableCodeSigning=false",
                "-p:CodesignKey=-",
                "-p:BuildIpa=false"
            ) + $binlogArguments

            if (Test-IsNet11OrLater $TargetFramework) {
                # net11+ iOS can't build with Mono (NETSDK1242). Use CoreCLR (JIT in the
                # Simulator); NativeAOT is device-only and isn't needed for a dry-run.
                $arguments += "-p:UseMonoRuntime=false"
            }

            Write-Host "Building iOS Simulator app for $($projectFile.FullName)"
            Invoke-DotNetPublish $arguments "iOS simulator build"

            $appBundle = Get-NewestBuildOutput $ProjectPath "*.app" -Directory
            if ($appBundle) {
                $zipPath = Join-Path $OutputPath "$($appBundle.Name).zip"
                Repair-AppleAdhocSignature $appBundle.FullName
                Compress-AppBundle $appBundle.FullName $zipPath
                $package = Get-Item $zipPath
                $sideloadPackage = $package
            }

            # Best-effort: also build the unsigned device IPA testers asked for. If it fails,
            # the Simulator app above is still uploaded, so the dry-run never regresses.
            $deviceIpa = New-IosUnsignedDeviceIpa `
                -ProjectFile $projectFile `
                -TargetFramework $TargetFramework `
                -Configuration $Configuration `
                -RuntimeIdentifier $RuntimeIdentifier `
                -OutputPath $OutputPath `
                -AppDisplayVersion $AppDisplayVersion `
                -AppBuildNumber $AppBuildNumber

            if ($deviceIpa) {
                # The installable IPA becomes the primary sideload artifact; keep the Simulator
                # app as an additional upload for Mac-only smoke testing.
                $additionalPackage = $package
                $sideloadPackage = $deviceIpa
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

        $useNet11OrLater = Test-IsNet11OrLater $TargetFramework
        if ($useNet11OrLater) {
            $arguments += "-p:UseMonoRuntime=false"
        }

        if (-not [string]::IsNullOrWhiteSpace($RuntimeIdentifier)) {
            $arguments += @("-r", $RuntimeIdentifier)
        } elseif ($useNet11OrLater) {
            # net11+ Mac Catalyst cannot publish the SDK's default universal
            # RuntimeIdentifiers=maccatalyst-x64;maccatalyst-arm64 unattended: the multi-RID
            # publish trips NETSDK "PublishReadyToRun couldn't be inferred". Pin a single RID.
            # arm64 (the original forced x64) runs NATIVELY on Apple Silicon with no Rosetta,
            # which is exactly what the crashing M2 (Mac14,7) tester needs.
            $arguments += @("-r", "maccatalyst-arm64")
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

            $sideloadPackage = New-MacCatalystDeveloperIdSideload `
                -ProjectFile $projectFile `
                -TargetFramework $TargetFramework `
                -Configuration $Configuration `
                -OutputPath $OutputPath `
                -AppDisplayVersion $AppDisplayVersion `
                -AppBuildNumber $AppBuildNumber `
                -RuntimeIdentifier $RuntimeIdentifier `
                -UseNet11OrLater:$useNet11OrLater
        } else {
            $appBundle = Get-NewestBuildOutput $OutputPath "*.app" -Directory
            if (-not $appBundle) {
                $appBundle = Get-NewestBuildOutput $ProjectPath "*.app" -Directory
            }

            if ($appBundle) {
                $zipPath = Join-Path $OutputPath "$($appBundle.Name).zip"
                Repair-AppleAdhocSignature $appBundle.FullName
                Compress-AppBundle $appBundle.FullName $zipPath
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
            "-p:SelfContained=true",
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
$sideloadResolved = if ($sideloadPackage) { $sideloadPackage.FullName } else { $package.FullName }
Write-Host "Sideload artifact: $sideloadResolved"
if ($additionalPackage) {
    Write-Host "Additional artifact: $($additionalPackage.FullName)"
}
if ($CreateBinlog) {
    Write-Host "Build binlog: $binlogPath"
}

if ($env:GITHUB_OUTPUT) {
    "package_path=$($package.FullName)" >> $env:GITHUB_OUTPUT
    "sideload_package_path=$sideloadResolved" >> $env:GITHUB_OUTPUT
    if ($additionalPackage) {
        "additional_package_path=$($additionalPackage.FullName)" >> $env:GITHUB_OUTPUT
    }
    if ($CreateBinlog) {
        "binlog_path=$binlogPath" >> $env:GITHUB_OUTPUT
    }
}
