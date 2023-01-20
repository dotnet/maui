# Copied from ASP.NET:
# https://github.com/dotnet/aspnetcore/blob/a24dd9e870bf713487e5cca46075ff3ee2c3ddc8/eng/scripts/mark-shipped.ps1
[CmdletBinding(PositionalBinding=$false)]
param ()

Set-StrictMode -version 2.0
$ErrorActionPreference = "Stop"

function MarkShipped([string]$dir) {
    Write-Host "Processing $dir"

    $removedPrefix = "*REMOVED*";
    $shipped = @()
    $removed = @()

    $shippedFilePath = Join-Path $dir "PublicAPI.Shipped.txt"
    $shippedContents = Get-Content $shippedFilePath
    foreach ($item in $shippedContents) {
        $shipped += $item.Trim()
    }

    $unshippedFilePath = Join-Path $dir "PublicAPI.Unshipped.txt"
    $unshippedContents = Get-Content $unshippedFilePath
    foreach ($item in $unshippedContents) {
        $item = $item.Trim()
        if ($item.Length -gt 0) {
            if ($item.StartsWith($removedPrefix)) {
                $item = $item.Substring($removedPrefix.Length).Trim()
                $removed += $item
            }
            else {
                $shipped += $item
            }
        }
    }

    Remove-Item $shippedFilePath -Force
    $shipped |
        Sort-Object -Unique |
        Where-Object { -not $removed.Contains($_) } |
        Out-File $shippedFilePath -Encoding Ascii

    Copy-Item eng/PublicAPI.empty.txt $unshippedFilePath
}

try {
    foreach ($file in Get-ChildItem -re -in "PublicApi.Shipped.txt") {
        $dir = Split-Path -parent $file
        MarkShipped $dir
    }
}
catch {
    Write-Host $_
    Write-Host $_.Exception
    exit 1
}
