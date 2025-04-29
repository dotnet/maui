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

    # Add mappings for any other packages as needed
}

# Create a function to find or add a package entry
function Set-PackageVersion {
    param(
        [string]$PackageName,
        [string]$PackageVersion
    )
    
    # Check if the package already exists in the manifest
    $existingEntry = $cgManifest.registrations | Where-Object { 
        $_.component.type -eq 'nuget' -and $_.component.nuget.name -eq $PackageName
    }
    
    if ($existingEntry) {
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

# Update the manifest with versions from Versions.props
foreach ($package in $packageVersionMappings.GetEnumerator()) {
    $packageName = $package.Key
    $versionPropertyName = $package.Value
    
    # Find the version in the Versions.props file
    $versionNode = $versionsProps.Project.PropertyGroup.SelectSingleNode("//*[local-name()='$versionPropertyName']")
    
    if ($versionNode -ne $null) {
        $version = $versionNode.InnerText
        if (-not [string]::IsNullOrWhiteSpace($version)) {
            Set-PackageVersion -PackageName $packageName -PackageVersion $version
        }
        else {
            Write-Warning "Property $versionPropertyName has no value in Versions.props"
        }
    }
    else {
        Write-Warning "Could not find property $versionPropertyName in Versions.props"
    }
}

# Save the updated manifest
$cgManifest | ConvertTo-Json -Depth 10 | Out-File $cgManifestPath -Encoding utf8
Write-Host "Updated cgmanifest.json saved to: $cgManifestPath"
