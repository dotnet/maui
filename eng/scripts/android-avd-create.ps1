<#
.SYNOPSIS
    Wrapper script for 'dotnet android avd create' that correctly returns non-zero exit code on failure.
    
.DESCRIPTION
    The 'dotnet android' tool incorrectly returns exit code 0 even when AVD creation fails.
    This script captures the output and checks for error patterns, returning exit code 1 if errors are detected.
    
.PARAMETER Name
    The name for the AVD (e.g., 'Emulator_30')
    
.PARAMETER Sdk
    The SDK system image (e.g., 'system-images;android-30;google_apis_playstore;x86_64')
    
.PARAMETER Device
    The device type (e.g., 'Nexus 5X')

.PARAMETER ErrorPatterns
    Array of regex patterns to check for in the output. If any match, the script exits with code 1.
    Default patterns: 'SdkToolFailedExitException', 'Package path is not valid'
#>
param(
    [Parameter(Mandatory=$true)]
    [string]$Name,
    
    [Parameter(Mandatory=$true)]
    [string]$Sdk,
    
    [Parameter(Mandatory=$true)]
    [string]$Device,
    
    [string[]]$ErrorPatterns = @(
        'SdkToolFailedExitException',
        'Package path is not valid'
    )
)

$ErrorActionPreference = 'Continue'

# Run the command and capture output
$output = & dotnet android avd create --name $Name --sdk $Sdk --device $Device --force 2>&1

# Display the output
Write-Host $output

# Check for error patterns
$combinedPattern = ($ErrorPatterns -join '|')
if ($output -match $combinedPattern) {
    Write-Error "Android AVD creation failed for '$Name'"
    exit 1
}

exit 0
