#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory = $true)]
    [string]$FeedDirectory,

    [Parameter(Mandatory = $false)]
    [string]$ExpectedCommit,

    [Parameter(Mandatory = $false)]
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"
. (Join-Path $PSScriptRoot "UiEvidence.Common.ps1")

try {
    $feed = (Resolve-Path -LiteralPath $FeedDirectory).Path
    $manifestPath = Join-Path $feed "devflow-manifest.json"
    $manifest = Read-UiEvidenceJson $manifestPath
    if ($manifest.schemaVersion -ne 1 -or
        $manifest.mauiDependenciesRemoved -ne $true -or
        $manifest.windowsNativeUiAutomationDisabled -ne $true -or
        [string]$manifest.dependencyBaseline -ne "dotnet-maui") {
        throw "DevFlow feed manifest is invalid."
    }
    if ($ExpectedCommit) {
        Assert-UiEvidenceSha $ExpectedCommit "ExpectedCommit"
        if ([string]$manifest.commit -ne $ExpectedCommit.ToLowerInvariant()) {
            throw "DevFlow feed commit does not match the expected commit."
        }
    }

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $packageNames = @{}
    foreach ($entry in @($manifest.packages)) {
        $fileName = [IO.Path]::GetFileName([string]$entry.fileName)
        if ($fileName -ne [string]$entry.fileName -or -not $fileName.EndsWith(".nupkg", [StringComparison]::Ordinal)) {
            throw "DevFlow manifest contains an invalid package name."
        }
        $packageKey = $fileName.ToLowerInvariant()
        if ($packageNames.ContainsKey($packageKey)) {
            throw "DevFlow manifest contains duplicate package '$fileName'."
        }
        $packageNames[$packageKey] = $true

        $path = Join-Path $feed $fileName
        if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
            throw "DevFlow package is missing: $fileName"
        }
        $file = Get-Item -LiteralPath $path
        if ([long]$entry.sizeBytes -ne $file.Length -or [string]$entry.sha256 -ne (Get-UiEvidenceSha256 $path)) {
            throw "DevFlow package hash or size is invalid: $fileName"
        }

        $archive = [IO.Compression.ZipFile]::OpenRead($path)
        try {
            $nuspecEntries = @($archive.Entries | Where-Object { $_.FullName.EndsWith(".nuspec", [StringComparison]::OrdinalIgnoreCase) })
            if ($nuspecEntries.Count -ne 1) {
                throw "DevFlow package '$fileName' must contain one nuspec."
            }
            $reader = [IO.StreamReader]::new($nuspecEntries[0].Open())
            try {
                [xml]$nuspec = $reader.ReadToEnd()
            }
            finally {
                $reader.Dispose()
            }
            $mauiDependencies = @($nuspec.SelectNodes("//*[local-name()='dependency']") | Where-Object {
                [string]$_.id -in @("Microsoft.Maui.Controls", "Microsoft.Maui.Essentials")
            })
            if ($mauiDependencies.Count -ne 0) {
                throw "DevFlow package '$fileName' still contains packaged MAUI dependencies."
            }
        }
        finally {
            $archive.Dispose()
        }
    }

    foreach ($packageId in @("Microsoft.Maui.DevFlow.Driver")) {
        $pattern = "^$([Regex]::Escape($packageId))\.\d"
        if (@($manifest.packages | Where-Object { [string]$_.fileName -match $pattern }).Count -ne 1) {
            throw "DevFlow feed is missing '$packageId'."
        }
    }

    $result = [PSCustomObject][ordered]@{
        schemaVersion = 1
        valid = $true
        commit = [string]$manifest.commit
        packageVersion = [string]$manifest.packageVersion
        packageCount = @($manifest.packages).Count
        errors = @()
    }
    if ($OutputPath) {
        Write-UiEvidenceJson $result $OutputPath
    }
    else {
        $result | ConvertTo-Json -Depth 8
    }
    exit 0
}
catch {
    $result = [PSCustomObject][ordered]@{
        schemaVersion = 1
        valid = $false
        commit = $null
        packageVersion = $null
        packageCount = 0
        errors = @($_.Exception.Message)
    }
    if ($OutputPath) {
        Write-UiEvidenceJson $result $OutputPath
    }
    else {
        $result | ConvertTo-Json -Depth 8
    }
    [Console]::Error.WriteLine($_.Exception.Message)
    exit 2
}
