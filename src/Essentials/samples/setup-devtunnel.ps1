#!/usr/bin/env pwsh
<#
.SYNOPSIS
    One-shot setup for testing the .NET MAUI Essentials auth samples (Passkeys + WebAuthenticator)
    against the single Sample.Server web app, exposed on a STABLE public HTTPS domain via a Microsoft
    dev tunnel.

.DESCRIPTION
    Passkeys (and OAuth redirect URIs) are bound to a domain, so `localhost` will not work from a
    real device. This script provisions a dev tunnel with a *persistent* tunnel id — so the public
    domain stays the same every time — and writes that domain straight into the server's user-secrets
    (for the passkeys relying party) so you don't have to edit any files.

    One server hosts both samples, so you configure one tunnel and use one URL for both the Passkeys
    and Web Authenticator pages.

    You run this once. After that, the same domain is reused on every run.

    Cross-platform: run with PowerShell 7+ (`pwsh`) on macOS, Windows, or Linux.

.PARAMETER TunnelId
    The dev tunnel id/name to create or reuse. Defaults to 'maui-essentials'. Keep it constant to
    keep the same public domain.

.PARAMETER Port
    The local HTTP port the server listens on. Defaults to 5177 (matches the project's
    launchSettings.json "http" profile).

.PARAMETER StartHost
    If set, starts hosting the tunnel (blocking) at the end. Otherwise prints the host command.

.EXAMPLE
    ./setup-devtunnel.ps1
    # Provisions the tunnel, writes ServerDomain into user-secrets, prints next steps.

.EXAMPLE
    ./setup-devtunnel.ps1 -StartHost
    # Provisions the tunnel and starts hosting it.
#>
[CmdletBinding()]
param(
    [string]$TunnelId = 'maui-essentials',
    [int]$Port = 5177,
    [switch]$StartHost
)

$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$project = Join-Path $here 'Sample.Server/Essentials.Sample.Server.csproj'

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

Write-Host "==> Ensuring tunnel '$TunnelId' exists (persistent domain)…" -ForegroundColor Cyan
# Create is idempotent-ish; ignore 'already exists'.
devtunnel create $TunnelId --allow-anonymous 2>$null | Out-Host
devtunnel port create $TunnelId -p $Port --protocol http 2>$null | Out-Host

Write-Host "==> Resolving the public tunnel URL…" -ForegroundColor Cyan
$uri = $null
try {
    $json = devtunnel show $TunnelId --json 2>$null | ConvertFrom-Json
    # The forwarding URIs live under the tunnel/ports depending on CLI version; search broadly.
    $candidates = @()
    if ($json.tunnel.portForwardingUris) { $candidates += $json.tunnel.portForwardingUris }
    foreach ($p in @($json.tunnel.ports) + @($json.ports)) {
        if ($p.portForwardingUris) { $candidates += $p.portForwardingUris }
    }
    $uri = $candidates | Where-Object { $_ -match "^https://.*-$Port\..*devtunnels\.ms" } | Select-Object -First 1
    if (-not $uri) { $uri = $candidates | Where-Object { $_ -match '^https://.*devtunnels\.ms' } | Select-Object -First 1 }
}
catch { }

if (-not $uri) {
    Write-Warning "Could not auto-detect the tunnel URL. Run 'devtunnel show $TunnelId' and copy the https://…devtunnels.ms URL."
}
else {
    $uri = $uri.TrimEnd('/')
    $domain = ([Uri]$uri).Host
    Write-Host "    Public URL : $uri" -ForegroundColor Green
    Write-Host "    RP ID/host : $domain" -ForegroundColor Green

    Write-Host "==> Writing server user-secrets (passkeys ServerDomain + web origin)…" -ForegroundColor Cyan
    dotnet user-secrets --project $project set 'Passkeys:ServerDomain' $domain | Out-Null
    dotnet user-secrets --project $project set 'Passkeys:AllowedOrigins:0' $uri | Out-Null
    Write-Host "    Done. The passkeys RP ID is '$domain'." -ForegroundColor Green
    Write-Host "    (Add Android apk-key-hash origins + Apple app-ids, and OAuth client ids/secrets, later — see README.md.)" -ForegroundColor DarkGray

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
