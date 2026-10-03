#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$RepoRoot,
    [Parameter(Mandatory)][string]$InstallDirectory,
    [Parameter(Mandatory)][ValidateSet('android', 'ios')][string]$Platform
)

$ErrorActionPreference = 'Stop'
if (Test-Path -LiteralPath $InstallDirectory) {
    throw 'The author sample SDK must be installed in a new directory, separate from verification.'
}
$sdkVersion = (Get-Content -Raw -LiteralPath (Join-Path $RepoRoot 'global.json') | ConvertFrom-Json).tools.dotnet
if ($sdkVersion -notmatch '^[0-9]+\.[0-9]+\.[0-9]+(?:-[A-Za-z0-9.-]+)?$') {
    throw 'The pinned public SDK distribution version is invalid.'
}
$installers = @('temp/dotnet-install.sh', 'bin/dotnet-install.sh' | ForEach-Object {
    Join-Path $RepoRoot $_
} | Where-Object { Test-Path -LiteralPath $_ -PathType Leaf })
if ($installers.Count -ne 1) {
    throw 'Pinned repository SDK provisioning must complete before sample SDK installation.'
}
$installer = $installers[0]

# Repository provisioning removes the stock MAUI manifests to build MAUI from source.
$feed = if ($sdkVersion -match '-') { @('--azure-feed', 'https://ci.dot.net/public') } else { @() }
& bash $installer --version $sdkVersion --install-dir $InstallDirectory --no-path @feed
if ($LASTEXITCODE -ne 0) { throw 'Clean public sample SDK installation failed.' }
$dotnet = Join-Path $InstallDirectory 'dotnet'
& $dotnet workload install "maui-$Platform" --skip-manifest-update `
    --configfile (Join-Path $RepoRoot 'NuGet.config') --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw 'Public MAUI sample workload installation failed.' }
exit 0
