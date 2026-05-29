<#
.SYNOPSIS
    Generates a catalog (.cat) file for customer-modifiable template content.
.DESCRIPTION
    Recursively scans a directory for files matching a filter and produces a
    Catalog Definition File (.cdf), then runs makecat.exe to create the .cat.

    Used to catalog-sign files that cannot use direct Authenticode signing
    because customers are expected to modify them (e.g., template .js files).
.PARAMETER RootPath
    The directory containing files to include in the catalog.
.PARAMETER CatOutputPath
    The path where makecat.exe will create the .cat file.
.PARAMETER Filter
    Comma-separated file filter patterns (e.g., '*.js' or '*.js,*.ttf'). Default: '*.*'.
.PARAMETER ErrorIfMakecatNotFound
    Throws an error when makecat.exe is not found instead of warning and skipping.
    Use in CI/official builds.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string]$RootPath,

    [Parameter(Mandatory)]
    [string]$CatOutputPath,

    [string]$Filter = '*.*',

    [switch]$ErrorIfMakecatNotFound
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $RootPath)) {
    Write-Error "Root path not found: $RootPath"
    return
}

$CdfPath = [System.IO.Path]::ChangeExtension($CatOutputPath, '.cdf')

# Ensure output directory exists
$catDir = Split-Path $CatOutputPath -Parent
if ($catDir -and -not (Test-Path $catDir)) {
    New-Item -ItemType Directory -Path $catDir -Force | Out-Null
}

$files = Get-ChildItem -Path $RootPath -Recurse -Include ($Filter -split ',') -File
if ($files.Count -eq 0) {
    Write-Warning "No files matching '$Filter' found under $RootPath - skipping catalog generation."
    return
}

# Build the CDF content
$cdfContent = @()
$cdfContent += "[CatalogHeader]"
$cdfContent += "Name=$CatOutputPath"
$cdfContent += "CatalogVersion=2"
$cdfContent += "HashAlgorithms=SHA256"
$cdfContent += ""
$cdfContent += "[CatalogFiles]"

$i = 0
foreach ($f in $files) {
    $ext = $f.Extension.TrimStart('.').ToLower()
    $label = "${ext}_${i}_" + ($f.Name -replace '[^\w\.-]', '_')
    $cdfContent += "<hash>$label=$($f.FullName)"
    $i++
}

$cdfContent | Set-Content -Path $CdfPath -Encoding ASCII
Write-Host "Generated CDF with $($files.Count) file(s) matching '$Filter' at $CdfPath"

# Find makecat.exe (ships with Windows SDK)
$makecat = Get-Command makecat.exe -ErrorAction SilentlyContinue
if (-not $makecat) {
    $sdkRoot = "${env:ProgramFiles(x86)}\Windows Kits\10\bin"
    if (Test-Path $sdkRoot) {
        $makecat = Get-ChildItem -Path $sdkRoot -Recurse -Filter 'makecat.exe' -File |
            Where-Object { $_.DirectoryName -match 'x64' } |
            Sort-Object DirectoryName -Descending |
            Select-Object -First 1
    }
}

if (-not $makecat) {
    if ($ErrorIfMakecatNotFound) {
        throw "makecat.exe not found. Catalog signing requires the Windows SDK."
    }
    Write-Warning "makecat.exe not found - skipping catalog generation. Install Windows SDK for catalog signing."
    return
}

$makecatPath = if ($makecat -is [System.Management.Automation.CommandInfo]) { $makecat.Source } else { $makecat.FullName }
Write-Host "Using makecat.exe at: $makecatPath"

& $makecatPath $CdfPath
if ($LASTEXITCODE -ne 0) {
    throw "makecat.exe failed with exit code $LASTEXITCODE"
}

Write-Host "Generated catalog file: $CatOutputPath"
