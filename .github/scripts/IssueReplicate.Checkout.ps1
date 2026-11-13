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
$destinationPath = [IO.Path]::GetFullPath($Destination)
$parentPath = Split-Path -Parent $destinationPath
if (-not (Test-Path -LiteralPath $parentPath -PathType Container)) {
    throw 'The target checkout parent directory does not exist.'
}
for ($attempt = 1; $attempt -le 3; $attempt++) {
    $attemptPath = Join-Path $parentPath ('.issue-maui-checkout-' + [guid]::NewGuid().ToString('N'))
    if (Test-Path -LiteralPath $attemptPath) { throw 'The temporary checkout directory already exists.' }
    try {
        & git -c fetch.fsckObjects=true -c transfer.fsckObjects=true clone --quiet --no-checkout `
            --depth=1 --branch $manifest.targetRef https://github.com/dotnet/maui.git $attemptPath
        $ready = $LASTEXITCODE -eq 0
        if ($ready) {
            & git -C $attemptPath -c fetch.fsckObjects=true fetch --quiet --depth=1 origin $manifest.targetSha
            $ready = $LASTEXITCODE -eq 0
        }
        if ($ready) {
            & git -C $attemptPath checkout --quiet --detach $manifest.targetSha
            $ready = $LASTEXITCODE -eq 0
        }
        if ($ready) {
            $head = & git -C $attemptPath rev-parse HEAD
            if ($LASTEXITCODE -ne 0 -or ([string]$head).Trim() -cne $manifest.targetSha) {
                throw 'The MAUI checkout does not match the pinned commit.'
            }
            $localExtraheader = & git -C $attemptPath config --local --get-regexp '^http\..*\.extraheader$' 2>$null
            if ($LASTEXITCODE -notin @(0, 1)) { throw 'Could not inspect the checkout credential configuration.' }
            if ($localExtraheader) { throw 'A credential was persisted in the execution checkout.' }
            [IO.Directory]::Move($attemptPath, $destinationPath)
            Write-Host "Checked out the public MAUI revision $($manifest.targetSha) without credentials (attempt $attempt)."
            exit 0
        }
        Write-Warning "Public MAUI checkout attempt $attempt failed; discarding its owned temporary checkout."
    } finally {
        if (Test-Path -LiteralPath $attemptPath) {
            if ((Get-Item -LiteralPath $attemptPath -Force).Attributes -band [IO.FileAttributes]::ReparsePoint) {
                throw 'The owned temporary checkout became a link; refusing recursive cleanup.'
            }
            Remove-Item -LiteralPath $attemptPath -Recurse -Force
        }
    }
    if ($attempt -lt 3) { Start-Sleep -Seconds (3 * $attempt) }
}
throw 'Could not prepare the pinned public MAUI checkout after three fresh attempts.'
