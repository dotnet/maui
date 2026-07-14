#!/usr/bin/env pwsh

param(
    [Parameter(Mandatory)]
    [string]$RepositoryPath,

    [Parameter(Mandatory)]
    [string]$PackageVersion,

    [Parameter(Mandatory)]
    [string]$OutputPath,

    [Parameter(Mandatory)]
    [string]$DotNetCliHome,

    [Parameter(Mandatory)]
    [string]$NuGetPackages
)

$ErrorActionPreference = "Stop"

$templatesProject = Join-Path $RepositoryPath "src/Templates/src/Microsoft.Maui.Templates.csproj"
if (-not (Test-Path $templatesProject)) {
    throw "Template project was not found at '$templatesProject'."
}

New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
New-Item -ItemType Directory -Path $DotNetCliHome -Force | Out-Null
New-Item -ItemType Directory -Path $NuGetPackages -Force | Out-Null

$env:DOTNET_CLI_HOME = $DotNetCliHome
$env:NUGET_PACKAGES = $NuGetPackages

Write-Host "Building MAUI templates from $templatesProject"
dotnet build -t:Rebuild $templatesProject -p:PackageVersion=$PackageVersion -p:GenerateCgManifest=false

Write-Host "Packing MAUI templates with PackageVersion=$PackageVersion"
dotnet pack $templatesProject -p:PackageVersion=$PackageVersion -p:GenerateCgManifest=false -o $OutputPath

$package = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" -Recurse |
    Sort-Object LastWriteTimeUtc -Descending |
    Select-Object -First 1

if (-not $package) {
    throw "No template package was produced in '$OutputPath'."
}

Write-Host "Template package: $($package.FullName)"

if ($env:GITHUB_OUTPUT) {
    "template_package_path=$($package.FullName)" >> $env:GITHUB_OUTPUT
}
