#!/usr/bin/env powershell.exe
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Query', 'Install', 'Remove')]
    [string]$Operation,

    [ValidatePattern('^[A-Za-z0-9.-]{3,50}$')]
    [string]$PackageName = '',

    [string]$PackagePath = ''
)

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'
Set-StrictMode -Version 3.0

function Get-BoundedPackage {
    param([Parameter(Mandatory = $true)][string]$Name)

    $packages = @(Get-AppxPackage -Name $Name -ErrorAction Stop)
    if ($packages.Count -gt 1) {
        throw "Expected at most one installed package '$Name'; found $($packages.Count)."
    }
    if ($packages.Count -eq 0) {
        return $null
    }
    $package = $packages[0]
    $manifest = Get-AppxPackageManifest -Package $package -ErrorAction Stop
    $manifestXml = [string]$manifest.OuterXml
    if ([string]::IsNullOrWhiteSpace($manifestXml) -or
        [Text.Encoding]::UTF8.GetByteCount($manifestXml) -gt 512KB) {
        throw "Installed package '$Name' has no bounded manifest."
    }
    return [ordered]@{
        name = [string]$package.Name
        publisher = [string]$package.Publisher
        packageFullName = [string]$package.PackageFullName
        packageFamilyName = [string]$package.PackageFamilyName
        installLocation = [string]$package.InstallLocation
        manifestXml = $manifestXml
    }
}

switch ($Operation) {
    'Query' {
        if ([string]::IsNullOrWhiteSpace($PackageName)) {
            throw 'Query requires PackageName.'
        }
        $result = Get-BoundedPackage -Name $PackageName
        if ($null -eq $result) {
            Write-Output 'null'
        } else {
            $result | ConvertTo-Json -Compress -Depth 5
        }
    }
    'Install' {
        if ([string]::IsNullOrWhiteSpace($PackagePath) -or
            -not [IO.Path]::IsPathRooted($PackagePath)) {
            throw 'Install requires an absolute package path.'
        }
        $fullPath = [IO.Path]::GetFullPath($PackagePath)
        $item = Get-Item -LiteralPath $fullPath -Force -ErrorAction Stop
        if ($item.PSIsContainer -or
            $item.Attributes -band [IO.FileAttributes]::ReparsePoint -or
            $item.Length -le 0 -or $item.Length -gt 2GB -or
            $item.Extension -cne '.msix') {
            throw 'Install requires a bounded regular MSIX file.'
        }
        Add-AppxPackage -Path $fullPath -ErrorAction Stop
    }
    'Remove' {
        if ([string]::IsNullOrWhiteSpace($PackageName)) {
            throw 'Remove requires PackageName.'
        }
        foreach ($package in @(Get-AppxPackage -Name $PackageName -ErrorAction Stop)) {
            Remove-AppxPackage -Package $package.PackageFullName -ErrorAction Stop
        }
    }
}
