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
$installer = Join-Path $RepoRoot 'temp/dotnet-install.sh'
if (-not (Test-Path -LiteralPath $installer -PathType Leaf)) {
    throw 'Pinned repository SDK provisioning must complete before sample SDK installation.'
}

# Repository provisioning removes the stock MAUI manifests to build MAUI from source.
& bash $installer --version $sdkVersion --install-dir $InstallDirectory --no-path `
    --azure-feed https://ci.dot.net/public
if ($LASTEXITCODE -ne 0) { throw 'Clean public sample SDK installation failed.' }
$dotnet = Join-Path $InstallDirectory 'dotnet'
& $dotnet workload install "maui-$Platform" --skip-manifest-update `
    --configfile (Join-Path $RepoRoot 'NuGet.config') --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw 'Public MAUI sample workload installation failed.' }
exit 0
