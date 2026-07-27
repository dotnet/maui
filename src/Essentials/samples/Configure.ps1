#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Configures (and optionally hosts a dev tunnel for) the .NET MAUI Essentials Passkeys sample,
    which is served by the Samples.WebServer relying-party web app.

.DESCRIPTION
    Passkeys are bound to a domain (the RP ID), so `localhost` will not work from a
    real device. This script provisions a dev tunnel with a *persistent* tunnel id — so the public
    domain stays the same every time — and writes the resulting configuration into the SERVER's
    user-secrets (the MAUI app itself reads no secrets; you type the URL into its UI):

      - the passkeys relying-party domain + web origin,
      - the Android package name (read from the sample app's project) plus the debug-signing-key
        SHA-256 fingerprint and `android:apk-key-hash:` origin (so Digital Asset Links validate), and
      - (optionally, with -AppleTeamId) the Apple app-id `<TeamID>.<BundleID>` for the App Site
        Association, plus the `webcredentials:<domain>` associated-domains entitlement in the sample
        app's iOS Entitlements.plist. See README.md (Apple section) for the remaining Apple steps
        (App ID registration + signing) that only you can do in your Apple Developer account.

    It does NOT run the web server — that is a separate `dotnet run` (see the printed next steps).

    You run this once. After that, the same domain is reused on every run.

    Cross-platform: run with PowerShell 7+ (`pwsh`) on macOS, Windows, or Linux.

.PARAMETER TunnelId
    The dev tunnel id/name to create or reuse. Defaults to 'maui-essentials'. Keep it constant to
    keep the same public domain.

.PARAMETER Port
    The local HTTP port the server listens on. Defaults to 5177 (matches the project's
    launchSettings.json "http" profile).

.PARAMETER AndroidPackage
    The Android application id. Defaults to the sample app's <ApplicationId> read from its project.

.PARAMETER DebugKeystore
    Path to the Android debug keystore. Defaults to the keystore .NET for Android actually signs debug
    builds with: <LocalApplicationData>/Xamarin/Mono for Android/debug.keystore (e.g. on macOS
    ~/Library/Application Support/Xamarin/Mono for Android/debug.keystore). This is NOT
    ~/.android/debug.keystore.

.PARAMETER AppleTeamId
    Your 10-character Apple Developer Team ID (developer.apple.com -> Membership). When supplied, the
    script writes the Apple app-id `<TeamID>.<BundleID>` into the server's App Site Association config
    and adds the `webcredentials:<domain>` associated-domains entitlement to the sample app's iOS
    Entitlements.plist. Omit to skip Apple setup.

.PARAMETER AppleBundleId
    The Apple bundle identifier of the sample app. Defaults to the app's <ApplicationId>. Override if
    that identifier is registered to another team and you need to use your own (also change the app's
    <ApplicationId> to match).

.PARAMETER StartHost
    If set, starts hosting the tunnel (blocking) at the end. Otherwise prints the host command.

.EXAMPLE
    ./Configure.ps1
    # Provisions the tunnel, writes the server config into user-secrets, prints next steps.

.EXAMPLE
    ./Configure.ps1 -AppleTeamId 42GDTGK33W
    # Also writes the Apple App Site Association app-id and the associated-domains entitlement.

.EXAMPLE
    ./Configure.ps1 -StartHost
    # Provisions the tunnel and starts hosting it.
#>
[CmdletBinding()]
param(
    [string]$TunnelId = 'maui-essentials',
    [int]$Port = 5177,
    [string]$AndroidPackage,
    [string]$DebugKeystore,
    [string]$AppleTeamId,
    [string]$AppleBundleId,
    [string]$AppleSigningIdentity,
    [string]$AppleProvisioningProfile,
    [switch]$StartHost
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $here 'Samples.WebServer' 'Essentials.Samples.WebServer.csproj'

# Default the Android package to the sample app's <ApplicationId> so the two never drift.
if (-not $AndroidPackage) {
    $appCsproj = Join-Path $here 'Samples' 'Essentials.Sample.csproj'
    if (Test-Path $appCsproj) {
        $m = [regex]::Match((Get-Content -Raw $appCsproj), '<ApplicationId>\s*([^<]+?)\s*</ApplicationId>')
        if ($m.Success) { $AndroidPackage = $m.Groups[1].Value.Trim() }
    }
    if (-not $AndroidPackage) { $AndroidPackage = 'com.microsoft.maui.essentials' }
}

# Default to the .NET for Android debug keystore — the key the build actually signs the APK with.
# .NET Android resolves this as <LocalApplicationData>/Xamarin/Mono for Android/debug.keystore, which
# maps per-OS to:
#   macOS   : ~/Library/Application Support/Xamarin/Mono for Android/debug.keystore
#   Windows : %LOCALAPPDATA%\Xamarin\Mono for Android\debug.keystore
#   Linux   : ~/.local/share/Xamarin/Mono for Android/debug.keystore
# This is deliberately NOT ~/.android/debug.keystore — that is Android Studio's key and does NOT sign
# the .NET MAUI app. Reading the wrong keystore makes assetlinks.json advertise a fingerprint the APK
# isn't signed with, and passkey creation then fails on-device with
# "the incoming request could not be validated".
if (-not $DebugKeystore) {
    $localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
    $DebugKeystore = Join-Path $localAppData 'Xamarin' 'Mono for Android' 'debug.keystore'
}

# Computes the Android debug-signing-key fingerprints needed for passkeys: the colon-hex SHA-256
# (for assetlinks.json) and the "android:apk-key-hash:<base64url>" origin (for ValidateOrigin).
# Returns $null if keytool or the keystore is unavailable (e.g. before the first Android build).
function Get-AndroidKeyInfo($keystore) {
    if (-not (Get-Command 'keytool' -ErrorAction SilentlyContinue)) {
        Write-Warning "keytool not found (install a JDK). Skipping Android fingerprint setup."
        return $null
    }
    if (-not (Test-Path $keystore)) {
        Write-Warning "Debug keystore not found at '$keystore' (build the Android app once to create it). Skipping Android fingerprint setup."
        return $null
    }
    $out = & keytool -list -v -keystore $keystore -alias androiddebugkey -storepass android -keypass android 2>$null
    $line = $out | Where-Object { $_ -match 'SHA256:' } | Select-Object -First 1
    if (-not $line) { Write-Warning "Could not read SHA-256 from the debug keystore."; return $null }
    $hex = ($line -replace '.*SHA256:\s*', '').Trim()
    $bytes = [byte[]]($hex.Split(':') | ForEach-Object { [Convert]::ToByte($_, 16) })
    $b64url = [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
    return [pscustomobject]@{ Hex = $hex; Origin = "android:apk-key-hash:$b64url" }
}

# Adds (or replaces) the webcredentials associated-domains entitlement in the sample app's iOS
# Entitlements.plist so Apple passkeys bind to the relying-party domain. This is a LOCAL edit to a
# committed file (like the server URL you type into the app) — do not commit it. Uses PlistBuddy on
# macOS (the only OS that can build/sign Apple apps) and falls back to editing the XML directly.
function Set-AppleAssociatedDomain($entitlementsPath, $domain) {
    if (-not (Test-Path $entitlementsPath)) {
        Write-Warning "Entitlements file not found at '$entitlementsPath'. Skipping Apple associated-domains."
        return $false
    }
    $entry = "webcredentials:$domain"
    $plistBuddy = '/usr/libexec/PlistBuddy'
    if (Test-Path $plistBuddy) {
        & $plistBuddy -c "Delete :com.apple.developer.associated-domains" $entitlementsPath 2>$null | Out-Null
        & $plistBuddy -c "Add :com.apple.developer.associated-domains array" $entitlementsPath | Out-Null
        & $plistBuddy -c "Add :com.apple.developer.associated-domains:0 string $entry" $entitlementsPath | Out-Null
        return $true
    }
    try {
        $xml = New-Object System.Xml.XmlDocument
        $xml.XmlResolver = $null
        $xml.Load($entitlementsPath)
        $dict = $xml.plist.dict
        $existing = $dict.SelectNodes('key') | Where-Object { $_.InnerText -eq 'com.apple.developer.associated-domains' } | Select-Object -First 1
        if ($existing) {
            $arr = $existing.NextSibling
            [void]$arr.RemoveAll()
        }
        else {
            $k = $xml.CreateElement('key'); $k.InnerText = 'com.apple.developer.associated-domains'; [void]$dict.AppendChild($k)
            $arr = $xml.CreateElement('array'); [void]$dict.AppendChild($arr)
        }
        $s = $xml.CreateElement('string'); $s.InnerText = $entry; [void]$arr.AppendChild($s)
        $xml.Save($entitlementsPath)
        return $true
    }
    catch {
        Write-Warning "Could not update associated-domains in '$entitlementsPath': $($_.Exception.Message)"
        return $false
    }
}

function Require-Command($name, $hint) {
    if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
        Write-Error "'$name' is not installed. $hint"
    }
}

# --- Apple signing auto-detection (macOS only) ------------------------------------------------
# These make Configure a one-stop shop: they find the Apple Development identity in the keychain and
# the provisioning profile that already matches this app-id + associated-domains, so you don't have to
# copy names by hand. Everything is written to a LOCAL, git-ignored Signing.local.props (never committed).

# Returns the first "Apple Development" codesigning identity name, or $null.
function Get-AppleSigningIdentity {
    if (-not (Get-Command 'security' -ErrorAction SilentlyContinue)) { return $null }
    $out = & security find-identity -v -p codesigning 2>$null
    $matches = @($out | Where-Object { $_ -match '"(Apple Development:[^"]+)"' } | ForEach-Object {
        [regex]::Match($_, '"(Apple Development:[^"]+)"').Groups[1].Value
    } | Select-Object -Unique)
    if ($matches.Count -eq 0) { return $null }
    return $matches[0]
}

# Scans installed provisioning profiles for one whose application-identifier equals <appId> (explicit)
# and that carries the associated-domains entitlement. Returns the profile Name, or $null.
function Find-AppleProvisioningProfile($appId) {
    if (-not (Get-Command 'security' -ErrorAction SilentlyContinue)) { return $null }
    $dir = Join-Path $HOME 'Library' 'MobileDevice' 'Provisioning Profiles'
    if (-not (Test-Path $dir)) { return $null }
    $files = Get-ChildItem -Path (Join-Path $dir '*') -Include '*.provisionprofile', '*.mobileprovision' -File -ErrorAction SilentlyContinue
    foreach ($f in $files) {
        $xml = (& security cms -D -i $f.FullName 2>$null) -join "`n"
        if (-not $xml) { continue }
        if ($xml -notmatch 'com\.apple\.developer\.associated-domains') { continue }
        # application-identifier looks like "<TeamId>.<BundleId>"; match the explicit app id.
        $m = [regex]::Match($xml, '<key>application-identifier</key>\s*<string>([^<]+)</string>')
        if (-not $m.Success) {
            $m = [regex]::Match($xml, '<key>com\.apple\.application-identifier</key>\s*<string>([^<]+)</string>')
        }
        if ($m.Success -and $m.Groups[1].Value -eq $appId) {
            $n = [regex]::Match($xml, '<key>Name</key>\s*<string>([^<]+)</string>')
            if ($n.Success) { return $n.Groups[1].Value }
        }
    }
    return $null
}

# Writes the git-ignored Samples/Signing.local.props that the app csproj imports. Scoped to Mac
# Catalyst (which needs an explicit provisioning profile for the associated-domains entitlement and
# fails to launch under the default linker with this sample's runtime XAML, hence MtouchLink=None).
function Write-AppleSigningProps($appDir, $identity, $profileName) {
    $path = Join-Path $appDir 'Signing.local.props'
    $entitlements = 'Platforms/iOS/Entitlements.plist'
    $props = @"
<!-- AUTO-GENERATED by Configure.ps1 for local Apple passkey testing. DO NOT COMMIT (git-ignored).
     Re-run Configure.ps1 to refresh. Delete this file to build unsigned again. -->
<Project>
  <PropertyGroup Condition="`$([MSBuild]::GetTargetPlatformIdentifier('`$(TargetFramework)')) == 'maccatalyst'">
    <CodesignEntitlements>$entitlements</CodesignEntitlements>
    <CodesignKey>$identity</CodesignKey>
    <CodesignProvision>$profileName</CodesignProvision>
    <!-- This sample uses runtime XAML inflation; the default Mac Catalyst linker trims types it needs. -->
    <MtouchLink>None</MtouchLink>
  </PropertyGroup>
</Project>
"@
    Set-Content -Path $path -Value $props -Encoding UTF8
    return $path
}
# ---------------------------------------------------------------------------------------------

Require-Command 'devtunnel' @'
Install the dev tunnels CLI:
  macOS:   brew install --cask devtunnel
  Windows: winget install Microsoft.devtunnel
  Linux:   https://aka.ms/devtunnels/download
'@
Require-Command 'dotnet' 'Install the .NET SDK from https://dotnet.microsoft.com/download.'

$loggedInUser = $null
try {
    $u = devtunnel user show --json 2>$null | ConvertFrom-Json
    if ($u.status -eq 'Logged in') { $loggedInUser = $u.username }
}
catch { }

if ($loggedInUser) {
    Write-Host "==> Already signed in to dev tunnels as $loggedInUser." -ForegroundColor Cyan
}
else {
    Write-Host "==> Signing in to dev tunnels (a browser window may open)…" -ForegroundColor Cyan
    devtunnel user login | Out-Host
}

Write-Host "==> Ensuring tunnel '$TunnelId' exists…" -ForegroundColor Cyan
# IMPORTANT: `devtunnel create` always makes a NEW tunnel — running it when the tunnel already
# exists creates a duplicate (in another cluster). So only create when `show` can't find it.
$tunnelJson = devtunnel show $TunnelId --json 2>$null | ConvertFrom-Json
if (-not $tunnelJson.tunnel) {
    devtunnel create $TunnelId --allow-anonymous | Out-Host
    $tunnelJson = devtunnel show $TunnelId --json 2>$null | ConvertFrom-Json
}
if (-not ($tunnelJson.tunnel.ports | Where-Object { $_.portNumber -eq $Port })) {
    devtunnel port create $TunnelId -p $Port --protocol http | Out-Host
}

Write-Host "==> Resolving the public tunnel URL…" -ForegroundColor Cyan

# Use the tunnel's own per-tunnel public URL (`portUri`), which is unique to this tunnel. We do NOT
# use the tunnel-id-derived URL (https://<name>-<port>.<cluster>.devtunnels.ms): although it's nicer,
# the tunnel name is a shared/global resource, so hardcoding it would collide across developers and
# machines. The random-looking portUri is stable for the life of the tunnel and safe for everyone.
#
# `portUri` is only assigned after the tunnel has been hosted once (then it persists), so on a brand
# new tunnel we briefly host it in the background to materialize the URL, then re-read it.
function Get-PortUri($tunnelId, $port) {
    try {
        $json = devtunnel show $tunnelId --json 2>$null | ConvertFrom-Json
        $p = $json.tunnel.ports | Where-Object { $_.portNumber -eq $port } | Select-Object -First 1
        if ($p -and $p.portUri) { return ([string]$p.portUri).TrimEnd('/') }
    }
    catch { }
    return $null
}

$uri = Get-PortUri $TunnelId $Port
if (-not $uri) {
    Write-Host "    New tunnel — starting a brief host session to obtain the URL…" -ForegroundColor DarkGray
    $job = Start-Job -ScriptBlock { param($t) devtunnel host $t } -ArgumentList $TunnelId
    try {
        for ($i = 0; $i -lt 15 -and -not $uri; $i++) {
            Start-Sleep -Seconds 2
            $uri = Get-PortUri $TunnelId $Port
        }
    }
    finally {
        Stop-Job $job -ErrorAction SilentlyContinue
        Remove-Job $job -Force -ErrorAction SilentlyContinue
    }
}

if (-not $uri) {
    Write-Warning "Could not auto-detect the tunnel URL. Run 'devtunnel host $TunnelId' once, note the 'Connect via browser' URL, then re-run this script."
}
else {
    $domain = ([Uri]$uri).Host
    Write-Host "    Public URL : $uri" -ForegroundColor Green
    Write-Host "    RP ID/host : $domain" -ForegroundColor Green

    Write-Host "==> Writing server user-secrets (passkeys ServerDomain + web origin)…" -ForegroundColor Cyan
    dotnet user-secrets --project $project set 'Passkeys:ServerDomain' $domain | Out-Null
    dotnet user-secrets --project $project set 'Passkeys:AllowedOrigins:0' $uri | Out-Null
    Write-Host "    Done. The passkeys RP ID is '$domain'." -ForegroundColor Green

    # Android: compute + write the debug-key fingerprint (assetlinks) and apk-key-hash origin.
    $android = Get-AndroidKeyInfo $DebugKeystore
    if ($android) {
        dotnet user-secrets --project $project set 'Passkeys:Android:PackageName' $AndroidPackage | Out-Null
        dotnet user-secrets --project $project set 'Passkeys:Android:Sha256CertFingerprints:0' $android.Hex | Out-Null
        dotnet user-secrets --project $project set 'Passkeys:AllowedOrigins:1' $android.Origin | Out-Null
        Write-Host "    Android configured: package '$AndroidPackage'" -ForegroundColor Green
        Write-Host "      SHA-256 : $($android.Hex)" -ForegroundColor DarkGray
        Write-Host "      origin  : $($android.Origin)" -ForegroundColor DarkGray
    }
    # Apple: write the App Site Association app-id and add the associated-domains entitlement.
    # The remaining Apple steps (App ID registration + signing) are in README.md (Apple section).
    if ($AppleTeamId) {
        if (-not $AppleBundleId) { $AppleBundleId = $AndroidPackage }
        $appleAppId = "$AppleTeamId.$AppleBundleId"
        dotnet user-secrets --project $project set 'Passkeys:Apple:AppIds:0' $appleAppId | Out-Null
        $entPath = Join-Path $here 'Samples' 'Platforms' 'iOS' 'Entitlements.plist'
        $ok = Set-AppleAssociatedDomain $entPath $domain
        Write-Host "    Apple configured: app-id '$appleAppId'" -ForegroundColor Green
        if ($ok) {
            Write-Host "      entitlement: webcredentials:$domain  (Entitlements.plist — LOCAL edit, do not commit)" -ForegroundColor DarkGray
        }

        # Resolve the signing identity and provisioning profile (params win; otherwise auto-detect).
        $appDir = Join-Path $here 'Samples'
        if (-not $AppleSigningIdentity) { $AppleSigningIdentity = Get-AppleSigningIdentity }
        if (-not $AppleProvisioningProfile) { $AppleProvisioningProfile = Find-AppleProvisioningProfile $appleAppId }

        if ($AppleSigningIdentity -and $AppleProvisioningProfile) {
            $propsPath = Write-AppleSigningProps $appDir $AppleSigningIdentity $AppleProvisioningProfile
            Write-Host "      signing identity : $AppleSigningIdentity" -ForegroundColor DarkGray
            Write-Host "      provisioning     : $AppleProvisioningProfile" -ForegroundColor DarkGray
            Write-Host "      wrote $([IO.Path]::GetFileName($propsPath)) (git-ignored) — Mac Catalyst is ready to build/run." -ForegroundColor Green
        }
        else {
            if (-not $AppleSigningIdentity) {
                Write-Host "      No 'Apple Development' signing identity found in the keychain." -ForegroundColor Yellow
            }
            if (-not $AppleProvisioningProfile) {
                Write-Host "      No installed provisioning profile matches '$appleAppId' with Associated Domains." -ForegroundColor Yellow
            }
            Write-Host "      iOS Simulator still works unsigned. For Mac Catalyst / iOS device, create the profile and" -ForegroundColor DarkGray
            Write-Host "      re-run with -AppleSigningIdentity / -AppleProvisioningProfile — see README.md (Apple section)." -ForegroundColor DarkGray
        }
    }
    else {
        Write-Host "    (Apple: pass -AppleTeamId <TEAMID> to set up iOS/Mac Catalyst — see README.md (Apple section).)" -ForegroundColor DarkGray
    }

    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "  1) In THIS terminal, host the tunnel:   devtunnel host $TunnelId"
    Write-Host "  2) In ANOTHER terminal, run the server: dotnet run --project `"$project`" --launch-profile http"
    Write-Host "  3) In the MAUI Essentials sample, set BOTH the Passkeys and Web Authenticator server URLs to: $uri"
}

if ($StartHost) {
    Write-Host ""
    Write-Host "==> Hosting tunnel '$TunnelId' (Ctrl+C to stop). Run the server in another terminal." -ForegroundColor Cyan
    devtunnel host $TunnelId
}
