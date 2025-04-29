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
    'xunit.analyzer' = 'XUnitAnalyzersPackageVersion'  # named differently
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
    'CommunityToolkit.Maui' = 'CommunityToolkitMauiPackageVersion'
    'CommunityToolkit.Mvvm' = 'CommunityToolkitMvvmPackageVersion'

    # Add mappings for any other packages as needed
}

# Create a function to find or add a package entry
function Set-PackageVersion {
    param(
        [string]$PackageName,
        [string]$PackageVersion,
        [switch]$AllowDuplicate
    )
    
    # Check if the package already exists in the manifest
    $existingEntry = $cgManifest.registrations | Where-Object { 
        $_.component.type -eq 'nuget' -and 
        $_.component.nuget.name -eq $PackageName -and 
        (!$AllowDuplicate -or $_.component.nuget.version -eq $PackageVersion)
    }
    
    if ($existingEntry -and !$AllowDuplicate) {
        # Update the existing entry
        $existingEntry.component.nuget.version = $PackageVersion
        Write-Host "Updated $PackageName to version $PackageVersion"
    }
    else {
        # Create a new entry
        $newEntry = @{
            component = @{
                type = 'nuget'
                nuget = @{
                    name = $PackageName
                    version = $PackageVersion
                }
            }
        }
        
        $cgManifest.registrations += $newEntry
        Write-Host "Added $PackageName with version $PackageVersion"
    }
}

# Handle additional explicit entries that need to be added
# Note: CommunityToolkit.Maui is handled both here with its previous version and in the main mapping with current version
$additionalEntries = @(
    @{ Name = 'CommunityToolkit.Maui'; Version = 'CommunityToolkitMauiPreviousPackageVersion' }
)

# Add all the additional entries directly
foreach ($entry in $additionalEntries) {
    # Find the version in the Versions.props file for the entry
    $versionPropertyName = $entry.Version
    $version = $null

    # Try to find the property in PropertyGroups
    foreach ($propertyGroup in $versionsProps.Project.PropertyGroup) {
        if ($null -ne $propertyGroup.$versionPropertyName) {
            $version = $propertyGroup.$versionPropertyName.ToString()
            if (-not [string]::IsNullOrEmpty($version)) {
                break
            }
        }
    }

    if (-not [string]::IsNullOrWhiteSpace($version)) {
        # Ensure version doesn't have duplicated values
        $cleanVersion = ($version -split ' ')[0]
        Set-PackageVersion -PackageName $entry.Name -PackageVersion $cleanVersion
    }
    else {
        Write-Warning "Could not find property $versionPropertyName in Versions.props for $($entry.Name)"
    }
}

# Update the manifest with versions from Versions.props
foreach ($package in $packageVersionMappings.GetEnumerator()) {
    $packageName = $package.Key
    $versionPropertyName = $package.Value
    
    # For packages that have both current and previous versions (like CommunityToolkit.Maui),
# we want both entries, so we don't skip them
# We only skip exact duplicates
    if ($packageName -ne 'CommunityToolkit.Maui' && 
        ($additionalEntries | Where-Object { $_.Name -eq $packageName })) {
        continue
    }
    
    # Find the version in the Versions.props file
    $version = $null

    # Try to find the property in the first level of PropertyGroups
    foreach ($propertyGroup in $versionsProps.Project.PropertyGroup) {
        $propNode = $propertyGroup.SelectSingleNode($versionPropertyName)
        if ($null -ne $propNode) {
            # Found it directly
            $version = $propNode.'#text'
            if (-not [string]::IsNullOrEmpty($version)) {
                break
            }
        }
    }

    # If not found, try with the standard method
    if ([string]::IsNullOrEmpty($version)) {
        # This approach avoids the issue with duplicate versions
        foreach ($propertyGroup in $versionsProps.Project.PropertyGroup) {
            if ($null -ne $propertyGroup.$versionPropertyName) {
                $version = $propertyGroup.$versionPropertyName.ToString()
                if (-not [string]::IsNullOrEmpty($version)) {
                    break
                }
            }
        }
    }

    # Another attempt - use XPath to search the entire document
    if ([string]::IsNullOrEmpty($version)) {
        $node = $versionsProps.SelectSingleNode("//Project/PropertyGroup/$versionPropertyName")
        if ($null -ne $node) {
            $version = $node.InnerText
        }
    }
    
    if (-not [string]::IsNullOrWhiteSpace($version)) {
        # Ensure version doesn't have duplicated values (clean it up if necessary)
        $cleanVersion = ($version -split ' ')[0]
        Set-PackageVersion -PackageName $packageName -PackageVersion $cleanVersion
    }
    else {
        Write-Warning "Could not find property $versionPropertyName in Versions.props or it has no value"
    }
}

# Save the updated manifest
$cgManifest | ConvertTo-Json -Depth 10 | Out-File $cgManifestPath -Encoding utf8
Write-Host "Updated cgmanifest.json saved to: $cgManifestPath"
