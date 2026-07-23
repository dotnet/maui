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

      - the passkeys relying-party domain + web origin, and
      - the Android package name (read from the sample app's project) plus the debug-signing-key
        SHA-256 fingerprint and `android:apk-key-hash:` origin (so Digital Asset Links validate).

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
    Path to the Android debug keystore. Defaults to the .NET for Android location
    (<userhome>/.android/debug.keystore), resolved per-platform.

.PARAMETER StartHost
    If set, starts hosting the tunnel (blocking) at the end. Otherwise prints the host command.

.EXAMPLE
    ./Configure.ps1
    # Provisions the tunnel, writes the server config into user-secrets, prints next steps.

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

# Default to the .NET for Android debug keystore location (all platforms). $HOME is cross-platform
# in PowerShell; use Join-Path so the separators are correct on Windows too.
if (-not $DebugKeystore) {
    $DebugKeystore = Join-Path $HOME '.android' 'debug.keystore'
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

function Require-Command($name, $hint) {
    if (-not (Get-Command $name -ErrorAction SilentlyContinue)) {
        Write-Error "'$name' is not installed. $hint"
    }
}

Require-Command 'devtunnel' @'
Install the dev tunnels CLI:
  macOS:   brew install --cask devtunnel
  Windows: winget install Microsoft.devtunnel
  Linux:   https://aka.ms/devtunnels/download
'@
Require-Command 'dotnet' 'Install the .NET SDK from https://dotnet.microsoft.com/download.'

Write-Host "==> Signing in to dev tunnels (a browser window may open)…" -ForegroundColor Cyan
devtunnel user login | Out-Host

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
    Write-Host "    (Apple app-ids are added separately — see README.md.)" -ForegroundColor DarkGray

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
