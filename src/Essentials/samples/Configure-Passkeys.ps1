#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Configures (and optionally hosts a dev tunnel for) the .NET MAUI Essentials Passkeys sample,
    which is served by the Samples.Server.Passkeys relying-party web app.

.DESCRIPTION
    Passkeys are bound to a domain (the RP ID), so `localhost` will not work from a
    real device. This script provisions a dev tunnel with a *persistent* tunnel id — so the public
    domain stays the same every time — and writes all developer-specific values into files that are
    NEVER committed: the SERVER's user-secrets, and the MAUI app's git-ignored Passkeys.Local.props
    (imported by the app csproj). No committed file is edited.

    Into the server user-secrets:
      - the passkeys relying-party domain + web origin,
      - the Android package name (read from the sample app's project) plus the debug-signing-key
        SHA-256 fingerprint and `android:apk-key-hash:` origin (so Digital Asset Links validate), and
      - on macOS (unless -NoApple), the Apple app-id `<TeamID>.<BundleID>` for the App Site Association.

    Into the git-ignored Samples/Passkeys.Local.props (and, for Apple, Samples/Platforms/iOS/
    Entitlements.Local.plist):
      - the default relying-party server URL (baked into the app via AssemblyMetadata), and
      - on macOS (unless -NoApple), the associated-domains entitlement plus the auto-detected Mac Catalyst
        signing identity + provisioning profile. See README-Passkeys.md (Apple section) for the App ID
        registration + profile steps that only you can do in your Apple Developer account.

    It does NOT run the web server — that is a separate `dotnet run` (see the printed next steps).

    You run this once. After that, the same domain is reused on every run.

    Cross-platform: run with PowerShell 7+ (`pwsh`) on macOS, Windows, or Linux.

.PARAMETER TunnelId
    The dev tunnel id/name to create or reuse. Defaults to 'maui-essentials-passkeys'. Keep it constant to
    keep the same public domain.

.PARAMETER Port
    The local HTTP port the server listens on. Defaults to 5177 (matches the project's
    launchSettings.json "http" profile).

.PARAMETER ApplicationId
    The app's application id (bundle id) shared by all platforms. Defaults to the sample app's
    <ApplicationId> read from its project. It's used for the Android package (assetlinks) and, when
    Apple is configured, the Apple app-id `<TeamID>.<ApplicationId>`. Apple setup is skipped
    automatically outside macOS unless AppleTeamId is passed explicitly.

.PARAMETER AndroidKeystore
    Path to the Android keystore whose signing-certificate SHA-256 goes into the Digital Asset Links
    (assetlinks.json). Defaults to the debug keystore .NET for Android signs debug builds with:
    <LocalApplicationData>/Xamarin/Mono for Android/debug.keystore (e.g. on macOS
    ~/Library/Application Support/Xamarin/Mono for Android/debug.keystore). This is NOT
    ~/.android/debug.keystore.

.PARAMETER NoApple
    Skip Apple (iOS / iPadOS / Mac Catalyst) setup. On macOS the script configures Apple by default,
    auto-detecting your Team ID (from the "Apple Development" signing certificate), signing identity,
    and provisioning profile. Apple setup is skipped automatically outside macOS because Apple targets
    require a Mac to build and sign. Pass AppleTeamId explicitly to prepare server trust and entitlements
    outside macOS for a subsequent build on a Mac.

.PARAMETER NoAndroid
    Skip Android setup. By default the script writes the Android debug-key SHA-256 fingerprint +
    apk-key-hash origin. If Android is not skipped but the debug key can't be read, the script FAILS —
    pass -NoAndroid to opt out.

.PARAMETER AppleTeamId
    Your 10-character Apple Developer Team ID (developer.apple.com -> Membership). Optional — it is
    auto-detected from your "Apple Development" signing certificate; pass this only to override the
    detected value. The Apple app-id `<TeamID>.<ApplicationId>` is written into the server's App Site
    Association config, and the git-ignored Apple entitlements/signing are generated.

.PARAMETER NoStartHost
    Skip hosting the tunnel. By default the script hosts the tunnel (blocking) at the end; pass this to
    just (re)configure and print the host command instead.

.EXAMPLE
    ./Configure-Passkeys.ps1
    # Configures Android and, on macOS, Apple; writes user-secrets and hosts the tunnel.

.EXAMPLE
    ./Configure-Passkeys.ps1 -NoApple
    # Android-only: skip Apple setup (e.g. on a machine with no Apple signing certificate).

.EXAMPLE
    ./Configure-Passkeys.ps1 -AppleTeamId 42GDTGK33W
    # Override the auto-detected Apple Team ID with an explicit one.

.EXAMPLE
    ./Configure-Passkeys.ps1 -NoStartHost
    # Configures without starting the blocking tunnel host (prints the host command instead).
#>
[CmdletBinding()]
param(
    [string]$TunnelId = 'maui-essentials-passkeys',
    [int]$Port = 5177,
    [string]$ApplicationId,
    [string]$AndroidKeystore,
    [string]$AppleTeamId,
    [string]$AppleSigningIdentity,
    [string]$AppleProvisioningProfile,
    [switch]$NoApple,
    [switch]$NoAndroid,
    [switch]$NoStartHost
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $here 'Samples.Server.Passkeys' 'Essentials.Samples.Server.Passkeys.csproj'
$appleSkippedForPlatform = $false

if (-not $IsMacOS -and -not $NoApple -and -not $AppleTeamId) {
    $NoApple = $true
    $appleSkippedForPlatform = $true
}

# Default the application id to the sample app's <ApplicationId> so the two never drift.
if (-not $ApplicationId) {
    $appCsproj = Join-Path $here 'Samples' 'Essentials.Sample.csproj'
    if (Test-Path $appCsproj) {
        $m = [regex]::Match((Get-Content -Raw $appCsproj), '<ApplicationId>\s*([^<]+?)\s*</ApplicationId>')
        if ($m.Success) { $ApplicationId = $m.Groups[1].Value.Trim() }
    }
    if (-not $ApplicationId) {
        throw "Could not read <ApplicationId> from '$appCsproj'. Pass -ApplicationId explicitly, or ensure the sample project defines <ApplicationId>."
    }
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
if (-not $AndroidKeystore) {
    $localAppData = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
    $AndroidKeystore = Join-Path $localAppData 'Xamarin' 'Mono for Android' 'debug.keystore'
}

# Computes the Android signing-key fingerprints needed for passkeys: the colon-hex SHA-256
# (for assetlinks.json) and the "android:apk-key-hash:<base64url>" origin (for ValidateOrigin).
# Returns $null if keytool or the keystore is unavailable (e.g. before the first Android build).
function Get-AndroidKeyInfo($keystore) {
    if (-not (Get-Command 'keytool' -ErrorAction SilentlyContinue)) {
        Write-Warning "keytool not found (install a JDK) — can't compute the Android signing fingerprint."
        return $null
    }
    if (-not (Test-Path $keystore)) {
        Write-Warning "Android keystore not found at '$keystore' (build the Android app once to create it)."
        return $null
    }
    $out = & keytool -list -v -keystore $keystore -alias androiddebugkey -storepass android -keypass android 2>$null
    $line = $out | Where-Object { $_ -match 'SHA256:' } | Select-Object -First 1
    if (-not $line) { Write-Warning "Could not read SHA-256 from the keystore '$keystore'."; return $null }
    $hex = ($line -replace '.*SHA256:\s*', '').Trim()
    $bytes = [byte[]]($hex.Split(':') | ForEach-Object { [Convert]::ToByte($_, 16) })
    $b64url = [Convert]::ToBase64String($bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_')
    return [pscustomobject]@{ Hex = $hex; Origin = "android:apk-key-hash:$b64url" }
}

# Generates a git-ignored Entitlements.Local.plist next to the committed Entitlements.plist: a copy of
# the base entitlements plus the webcredentials associated-domains entry for the relying-party domain.
# The committed Entitlements.plist is never modified. Pure XmlDocument (cross-platform, no external
# tools): XmlResolver is nulled so the plist DTD is never fetched, and the output is written with a
# fixed Apple-style header so it stays byte-clean (no empty-DOCTYPE-subset artifact).
function New-LocalEntitlements($basePlist, $outPlist, $domain) {
    if (-not (Test-Path $basePlist)) {
        Write-Warning "Base entitlements not found at '$basePlist'. Skipping Apple entitlements."
        return $false
    }
    $entry = "webcredentials:$domain"
    try {
        $xml = New-Object System.Xml.XmlDocument
        $xml.XmlResolver = $null
        $xml.Load($basePlist)

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

        # Serialize just the <plist> element (skipping the DOCTYPE node) with tab indentation, then
        # prepend the canonical Apple header, so the file matches the committed plist's format exactly.
        $settings = New-Object System.Xml.XmlWriterSettings
        $settings.Indent = $true
        $settings.IndentChars = "`t"
        $settings.OmitXmlDeclaration = $true
        $settings.NewLineChars = "`n"
        $sb = New-Object System.Text.StringBuilder
        $sw = New-Object System.IO.StringWriter($sb)
        $writer = [System.Xml.XmlWriter]::Create($sw, $settings)
        try { $xml.DocumentElement.WriteTo($writer) } finally { $writer.Dispose() }

        $header = "<?xml version=`"1.0`" encoding=`"UTF-8`"?>`n<!DOCTYPE plist PUBLIC `"-//Apple//DTD PLIST 1.0//EN`" `"http://www.apple.com/DTDs/PropertyList-1.0.dtd`">`n"
        [System.IO.File]::WriteAllText($outPlist, $header + $sb.ToString() + "`n", (New-Object System.Text.UTF8Encoding($false)))
        return $true
    }
    catch {
        Write-Warning "Could not write local entitlements '$outPlist': $($_.Exception.Message)"
        return $false
    }
}

function Require-Command($name, $hint) {
    if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
        throw "'$name' is not installed. $hint"
    }
}

# --- Apple signing auto-detection (macOS only) ------------------------------------------------
# These make Configure a one-stop shop: they find the Apple Development identity in the keychain and
# the provisioning profile that already matches this app-id + associated-domains, so you don't have to
# copy names by hand. Everything is written to a LOCAL, git-ignored Passkeys.Local.props (never committed).

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

# Derives the 10-char Apple Developer Team ID so it needn't be passed by hand: it's the Organizational
# Unit (OU) of the "Apple Development" signing certificate, and also a provisioning profile's
# TeamIdentifier. Returns the Team ID, or $null.
function Get-AppleTeamId {
    if (-not (Get-Command 'security' -ErrorAction SilentlyContinue)) { return $null }

    # Preferred: the OU of the Apple Development signing certificate.
    if (Get-Command 'openssl' -ErrorAction SilentlyContinue) {
        $subject = (& security find-certificate -a -c 'Apple Develop' -p 2>$null | & openssl x509 -noout -subject 2>$null) -join "`n"
        $m = [regex]::Match($subject, 'OU\s*=\s*([A-Z0-9]{10})')
        if ($m.Success) { return $m.Groups[1].Value }
    }

    # Fallback: a provisioning profile's TeamIdentifier.
    $dir = Join-Path $HOME 'Library' 'MobileDevice' 'Provisioning Profiles'
    if (Test-Path $dir) {
        $files = Get-ChildItem -Path (Join-Path $dir '*') -Include '*.provisionprofile', '*.mobileprovision' -File -ErrorAction SilentlyContinue
        foreach ($f in $files) {
            $xml = (& security cms -D -i $f.FullName 2>$null) -join "`n"
            $m = [regex]::Match($xml, '<key>TeamIdentifier</key>\s*<array>\s*<string>([A-Z0-9]{10})</string>')
            if ($m.Success) { return $m.Groups[1].Value }
        }
    }

    return $null
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

# Writes the git-ignored Samples/Passkeys.Local.props by loading the committed Passkeys.Local.in.props
# template and filling in the values via XML — so the template is the single source of truth for the
# file's shape and comments (tweak the .in file, not this script). Always sets the default server URL
# (baked into the app via AssemblyMetadata). When Apple signing was resolved it also fills the entitlements
# path and Mac Catalyst signing; otherwise it strips the Apple-only PropertyGroups.
function Write-PasskeysLocalProps($appDir, $serverUrl, $entitlementsRel, $identity, $profileName) {
    $template = Join-Path $appDir 'Passkeys.Local.in.props'
    $path = Join-Path $appDir 'Passkeys.Local.props'
    if (-not (Test-Path $template)) {
        throw "Template not found at '$template'. It should be committed alongside the app project."
    }

    $xml = New-Object System.Xml.XmlDocument
    $xml.PreserveWhitespace = $false
    $xml.Load($template)
    $project = $xml.DocumentElement

    # Replace the template's top-of-file comment(s) with a generated-file banner.
    foreach ($node in @($xml.ChildNodes)) {
        if ($node.NodeType -eq [System.Xml.XmlNodeType]::Comment) { [void]$xml.RemoveChild($node) }
    }
    $banner = $xml.CreateComment(" AUTO-GENERATED by Configure-Passkeys.ps1 from Passkeys.Local.in.props. DO NOT COMMIT (git-ignored).`n     Re-run Configure-Passkeys.ps1 to refresh; edit Passkeys.Local.in.props to change the file's shape. ")
    [void]$xml.InsertBefore($banner, $project)

    # Server URL (all platforms) always.
    foreach ($n in @($project.GetElementsByTagName('PasskeysServerUrl'))) { $n.InnerText = $serverUrl }

    $apple = $entitlementsRel -and $identity -and $profileName
    if ($apple) {
        foreach ($n in @($project.GetElementsByTagName('CodesignEntitlements'))) { $n.InnerText = $entitlementsRel }
        foreach ($n in @($project.GetElementsByTagName('CodesignKey'))) { $n.InnerText = $identity }
        foreach ($n in @($project.GetElementsByTagName('CodesignProvision'))) { $n.InnerText = $profileName }
    }
    else {
        # No Apple signing: drop the Apple-only property groups (each with its preceding comment).
        $appleProps = @('CodesignEntitlements', 'CodesignKey', 'CodesignProvision', 'MtouchLink')
        foreach ($pg in @($project.GetElementsByTagName('PropertyGroup'))) {
            $isApple = $false
            foreach ($child in $pg.ChildNodes) {
                if ($child.NodeType -eq [System.Xml.XmlNodeType]::Element -and $appleProps -contains $child.Name) { $isApple = $true; break }
            }
            if ($isApple) {
                $prev = $pg.PreviousSibling
                [void]$project.RemoveChild($pg)
                if ($prev -and $prev.NodeType -eq [System.Xml.XmlNodeType]::Comment) { [void]$project.RemoveChild($prev) }
            }
        }
    }

    $settings = New-Object System.Xml.XmlWriterSettings
    $settings.Indent = $true
    $settings.IndentChars = '  '
    $settings.OmitXmlDeclaration = $true
    $settings.NewLineChars = "`n"
    $sw = New-Object System.IO.StringWriter
    $writer = [System.Xml.XmlWriter]::Create($sw, $settings)
    try { $xml.Save($writer) } finally { $writer.Dispose() }
    [System.IO.File]::WriteAllText($path, $sw.ToString() + "`n", (New-Object System.Text.UTF8Encoding($false)))
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
    throw @"
Could not resolve the public dev tunnel URL for '$TunnelId'.

A public HTTPS domain is REQUIRED — there is no localhost fallback. Passkeys are bound to a domain: the
native authenticators need the relying party's well-known files (Android assetlinks.json, Apple AASA)
served over public HTTPS, so 'localhost' cannot work on Android, iOS, or Mac Catalyst.

Host the tunnel once to materialize its URL, then re-run this script:
    devtunnel host $TunnelId
"@
}

$domain = ([Uri]$uri).Host
Write-Host "    Public URL : $uri" -ForegroundColor Green
Write-Host "    RP ID/host : $domain" -ForegroundColor Green

Write-Host "==> Writing server user-secrets (passkeys ServerDomain + web origin)…" -ForegroundColor Cyan
dotnet user-secrets --project $project set 'Passkeys:ServerDomain' $domain | Out-Null
dotnet user-secrets --project $project set 'Passkeys:AllowedOrigins:0' $uri | Out-Null
Write-Host "    Done. The passkeys RP ID is '$domain'." -ForegroundColor Green

# Android: compute + write the debug-key fingerprint (assetlinks) and apk-key-hash origin. Configured
# by default; fails if the debug key can't be read (pass -NoAndroid to skip Android instead).
if ($NoAndroid) {
    Write-Host "    Android: skipped (-NoAndroid)." -ForegroundColor DarkGray
}
else {
    $android = Get-AndroidKeyInfo $AndroidKeystore
    if (-not $android) {
        throw "Could not read the Android debug-signing key (needed for the Digital Asset Links fingerprint). Build the Android app once to generate the debug keystore, pass -AndroidKeystore <path>, or pass -NoAndroid to skip Android."
    }
    dotnet user-secrets --project $project set 'Passkeys:Android:PackageName' $ApplicationId | Out-Null
    dotnet user-secrets --project $project set 'Passkeys:Android:Sha256CertFingerprints:0' $android.Hex | Out-Null
    dotnet user-secrets --project $project set 'Passkeys:AllowedOrigins:1' $android.Origin | Out-Null
    Write-Host "    Android configured: package '$ApplicationId'" -ForegroundColor Green
    Write-Host "      SHA-256 : $($android.Hex)" -ForegroundColor DarkGray
    Write-Host "      origin  : $($android.Origin)" -ForegroundColor DarkGray
}
# Compose the git-ignored Passkeys.Local.props for the MAUI app: the default server URL always, plus
# the Apple entitlements/signing (configured by default on macOS; skipped with -NoApple or automatically
# on other operating systems). The committed files are never edited.
$appDir = Join-Path $here 'Samples'
$entitlementsRel = $null
$resolvedIdentity = $null
$resolvedProfile = $null

if (-not $NoApple -and -not $AppleTeamId) {
    $AppleTeamId = Get-AppleTeamId
    if ($AppleTeamId) {
        Write-Host "    Apple Team ID auto-detected from your signing cert: $AppleTeamId" -ForegroundColor DarkGray
    }
}

if ($NoApple) {
    if ($appleSkippedForPlatform) {
        Write-Host "    Apple: skipped automatically (Apple targets require macOS to build and sign)." -ForegroundColor DarkGray
    }
    else {
        Write-Host "    Apple: skipped (-NoApple)." -ForegroundColor DarkGray
    }
}
elseif (-not $AppleTeamId) {
    throw "Could not determine your Apple Team ID: no 'Apple Development' signing certificate found. Install one (Xcode -> Settings -> Accounts -> Manage Certificates), pass -AppleTeamId <TEAMID>, or pass -NoApple to skip Apple."
}
else {
    $appleAppId = "$AppleTeamId.$ApplicationId"
    dotnet user-secrets --project $project set 'Passkeys:Apple:AppIds:0' $appleAppId | Out-Null
    Write-Host "    Apple configured: app-id '$appleAppId'" -ForegroundColor Green

    # Generate the git-ignored local entitlements (the committed Entitlements.plist is never touched).
    $baseEnt = Join-Path $appDir 'Platforms' 'iOS' 'Entitlements.plist'
    $localEnt = Join-Path $appDir 'Platforms' 'iOS' 'Entitlements.Local.plist'
    if (New-LocalEntitlements $baseEnt $localEnt $domain) {
        $entitlementsRel = 'Platforms/iOS/Entitlements.Local.plist'
        Write-Host "      entitlement: webcredentials:$domain  (Entitlements.Local.plist — git-ignored)" -ForegroundColor DarkGray
    }

    # Resolve the signing identity and provisioning profile (params win; otherwise auto-detect).
    if (-not $AppleSigningIdentity) { $AppleSigningIdentity = Get-AppleSigningIdentity }
    if (-not $AppleProvisioningProfile) { $AppleProvisioningProfile = Find-AppleProvisioningProfile $appleAppId }
    if ($AppleSigningIdentity -and $AppleProvisioningProfile) {
        $resolvedIdentity = $AppleSigningIdentity
        $resolvedProfile = $AppleProvisioningProfile
        Write-Host "      signing identity : $AppleSigningIdentity" -ForegroundColor DarkGray
        Write-Host "      provisioning     : $AppleProvisioningProfile" -ForegroundColor DarkGray
    }
    else {
        if ($IsMacOS) {
            if (-not $AppleSigningIdentity) {
                Write-Host "      No 'Apple Development' signing identity found in the keychain." -ForegroundColor Yellow
            }
            if (-not $AppleProvisioningProfile) {
                Write-Host "      No installed provisioning profile matches '$appleAppId' with Associated Domains." -ForegroundColor Yellow
            }
            Write-Host "      iOS Simulator still works. For Mac Catalyst / iOS device, create the profile and re-run" -ForegroundColor DarkGray
            Write-Host "      (or pass -AppleSigningIdentity / -AppleProvisioningProfile) — see README-Passkeys.md (Apple section)." -ForegroundColor DarkGray
        }
        else {
            Write-Host "      Server trust and entitlements generated. Build and sign the Apple app on a Mac." -ForegroundColor DarkGray
        }
    }
}

$propsPath = Write-PasskeysLocalProps $appDir $uri $entitlementsRel $resolvedIdentity $resolvedProfile
Write-Host "    Wrote $([IO.Path]::GetFileName($propsPath)) (git-ignored): the app defaults to $uri." -ForegroundColor Green

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
if ($NoStartHost) {
    Write-Host "  1) In THIS terminal, host the tunnel:   devtunnel host $TunnelId"
    Write-Host "  2) In ANOTHER terminal, run the server: dotnet run --project `"$project`" --launch-profile http"
}
else {
    Write-Host "  In ANOTHER terminal, run the server: dotnet run --project `"$project`" --launch-profile http"
    Write-Host "  (this terminal is about to host the tunnel — pass -NoStartHost to skip that)"
}
Write-Host "  Then build/run the sample — its Passkeys page defaults to $uri."

if (-not $NoStartHost) {
    Write-Host ""
    Write-Host "==> Hosting tunnel '$TunnelId' (Ctrl+C to stop). Run the server in another terminal." -ForegroundColor Cyan
    devtunnel host $TunnelId
}
