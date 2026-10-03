#!/usr/bin/env pwsh
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$ManifestPath,
    [Parameter(Mandatory)][string]$Destination
)

$ErrorActionPreference = 'Stop'
$manifest = Get-Content -Raw -LiteralPath $ManifestPath | ConvertFrom-Json
if ($manifest.schemaVersion -ne 1 -or $manifest.targetRef -cnotmatch '^(main|net[0-9]+\.0)$' -or
    $manifest.targetSha -cnotmatch '^[0-9a-f]{40}$') {
    throw 'Invalid MAUI target revision.'
}
if (Test-Path -LiteralPath $Destination) { throw 'The target checkout directory already exists.' }
& git clone --quiet --no-checkout --depth=1 --branch $manifest.targetRef `
    https://github.com/dotnet/maui.git $Destination
if ($LASTEXITCODE -ne 0) { throw 'Could not clone the public MAUI branch.' }
& git -C $Destination fetch --quiet --depth=1 origin $manifest.targetSha
if ($LASTEXITCODE -ne 0) { throw 'Could not fetch the immutable MAUI commit.' }
& git -C $Destination checkout --quiet --detach $manifest.targetSha
if ($LASTEXITCODE -ne 0 -or (git -C $Destination rev-parse HEAD).Trim() -cne $manifest.targetSha) {
    throw 'The MAUI checkout does not match the pinned commit.'
}
$localExtraheader = & git -C $Destination config --local --get-regexp '^http\..*\.extraheader$' 2>$null
if ($LASTEXITCODE -notin @(0, 1)) { throw 'Could not inspect the checkout credential configuration.' }
if ($localExtraheader) { throw 'A credential was persisted in the execution checkout.' }
Write-Host "Checked out the public MAUI revision $($manifest.targetSha) without credentials."
exit 0
