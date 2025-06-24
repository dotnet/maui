#!/usr/bin/env pwsh

<#
.SYNOPSIS
    Updates the cgmanifest.json file with package versions from Versions.props
.DESCRIPTION
    This script reads the Versions.props file to extract NuGet package versions
    and updates the cgmanifest.json file with these versions.
.NOTES
    This ensures that the Component Governance manifest is kept in sync with the actual package versions used in the project.
#>

$ErrorActionPreference = 'Stop'

# Get the paths to the files
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "../..")
$versionsPropsPath = Join-Path $repoRoot "eng/Versions.props"
$cgManifestPath = Join-Path $repoRoot "src/Templates/src/cgmanifest.json"

# Read the Versions.props file
Write-Host "Reading versions from: $versionsPropsPath"
[xml]$versionsProps = Get-Content $versionsPropsPath -Raw

# Check if cgmanifest.json exists
if (Test-Path $cgManifestPath) {
    Write-Host "Reading existing cgmanifest.json: $cgManifestPath"
    $cgManifest = Get-Content $cgManifestPath -Raw | ConvertFrom-Json
}
else {
    Write-Host "Creating a new cgmanifest.json file"
    $cgManifest = @{
        '$schema' = 'https://json.schemastore.org/component-detection-manifest.json'
        version = 1
        registrations = @()
    } | ConvertTo-Json -Depth 10 | ConvertFrom-Json
}

# Create a mapping of package names to version property names in Versions.props
$packageVersionMappings = @{
    # Microsoft.NET.Test.Sdk and test-related packages
    'Microsoft.NET.Test.Sdk' = 'MicrosoftNETTestSdkPackageVersion'
    'xunit' = 'XunitPackageVersion'
    'xunit.runner.visualstudio' = 'XunitRunnerVisualStudioPackageVersion'
    'xunit.analyzer' = 'XUnitAnalyzersPackageVersion'
    'coverlet.collector' = 'CoverletCollectorPackageVersion'
    
    # Microsoft Extensions packages
    'Microsoft.Extensions.Logging.Debug' = 'MicrosoftExtensionsLoggingDebugVersion'
    'Microsoft.Extensions.Configuration' = 'MicrosoftExtensionsConfigurationVersion'
    'Microsoft.Extensions.DependencyInjection' = 'MicrosoftExtensionsDependencyInjectionVersion'
    
    # Other packages
    'Microsoft.WindowsAppSDK' = 'MicrosoftWindowsAppSDKPackageVersion'
    'Microsoft.Graphics.Win2D' = 'MicrosoftGraphicsWin2DPackageVersion'
    'Microsoft.Windows.SDK.BuildTools' = 'MicrosoftWindowsSDKBuildToolsPackageVersion'
    
    # Additional packages specified by the user
    'Syncfusion.Maui.Toolkit' = 'SyncfusionMauiToolkitPackageVersion'
    'Microsoft.Data.Sqlite.Core' = 'MicrosoftDataSqliteCorePackageVersion'
    'SQLitePCLRaw.bundle_green' = 'SQLitePCLRawBundleGreenPackageVersion'
    'CommunityToolkit.Mvvm' = 'CommunityToolkitMvvmPackageVersion'
}

# Initialize new registrations list
$newRegistrations = New-Object System.Collections.ArrayList

# Function to create a new package entry
function New-PackageEntry {
    param(
        [string]$PackageName,
        [string]$PackageVersion
    )
    
    return @{
        component = @{
            type = 'nuget'
            nuget = @{
                name = $PackageName
                version = $PackageVersion
            }
        }
    }
}

# Handle CommunityToolkit.Maui versions first
Write-Host "Setting up CommunityToolkit.Maui entries..."

# Get current version
$currentVersion = $null
foreach ($propertyGroup in $versionsProps.Project.PropertyGroup) {
    if ($null -ne $propertyGroup.CommunityToolkitMauiPackageVersion) {
        $currentVersion = $propertyGroup.CommunityToolkitMauiPackageVersion.ToString().Trim()
        if (-not [string]::IsNullOrEmpty($currentVersion)) {
            Write-Host "Found current CommunityToolkit.Maui version: $currentVersion"
            [void]$newRegistrations.Add((New-PackageEntry -PackageName 'CommunityToolkit.Maui' -PackageVersion $currentVersion))
            break
        }
    }
}

# Get previous version
$previousVersion = $null
foreach ($propertyGroup in $versionsProps.Project.PropertyGroup) {
    if ($null -ne $propertyGroup.CommunityToolkitMauiPreviousPackageVersion) {
        $previousVersion = $propertyGroup.CommunityToolkitMauiPreviousPackageVersion.ToString().Trim()
        if (-not [string]::IsNullOrEmpty($previousVersion)) {
            Write-Host "Found previous CommunityToolkit.Maui version: $previousVersion"
            [void]$newRegistrations.Add((New-PackageEntry -PackageName 'CommunityToolkit.Maui' -PackageVersion $previousVersion))
            break
        }
    }
}

# Process other packages
foreach ($package in $packageVersionMappings.GetEnumerator()) {
    $packageName = $package.Key
    $versionPropertyName = $package.Value
    $version = $null

    # Look for the version in PropertyGroups
    foreach ($propertyGroup in $versionsProps.Project.PropertyGroup) {
        if ($propertyGroup.$versionPropertyName) {
            $version = $propertyGroup.$versionPropertyName.ToString().Trim()
            if (-not [string]::IsNullOrEmpty($version)) {
                Write-Host "Found $packageName version: $version"
                [void]$newRegistrations.Add((New-PackageEntry -PackageName $packageName -PackageVersion $version))
                break
            }
        }
    }

    if ([string]::IsNullOrEmpty($version)) {
        Write-Warning "Could not find version for $packageName (property: $versionPropertyName)"
    }
}

# Update the manifest
$cgManifest.registrations = $newRegistrations

# Save the updated manifest
$cgManifest | ConvertTo-Json -Depth 10 | Out-File $cgManifestPath -Encoding utf8
Write-Host "Updated cgmanifest.json saved to: $cgManifestPath"

# Print summary
Write-Host "Successfully added $($newRegistrations.Count) package registrations to cgmanifest.json"
