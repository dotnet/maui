<#
.SYNOPSIS
    Wrapper script for 'dotnet android sdk install' that correctly returns non-zero exit code on failure.
    
.DESCRIPTION
    The 'dotnet android' tool incorrectly returns exit code 0 even when package installation fails.
    This script captures the output and checks for error patterns, returning exit code 1 if errors are detected.
    
.PARAMETER Package
    The Android SDK package to install (e.g., 'platform-tools;35.0.2', 'system-images;android-30;google_apis_playstore;x86_64')
    
.PARAMETER ErrorPatterns
    Array of regex patterns to check for in the output. If any match, the script exits with code 1.
    Default patterns: 'SdkToolFailedExitException', 'Failed to find package', 'An error occurred while preparing SDK package'
#>
param(
    [Parameter(Mandatory=$true)]
    [ValidatePattern('^[a-zA-Z0-9;_\-\.]+$')]
    [string]$Package,
    
    [string[]]$ErrorPatterns = @(
        'SdkToolFailedExitException',
        'Failed to find package',
        'An error occurred while preparing SDK package'
    )
)

$ErrorActionPreference = 'Continue'

# Run the command and capture output
$output = & dotnet android sdk install --package $Package 2>&1

# Display the output
Write-Host $output

# Check for error patterns
$combinedPattern = ($ErrorPatterns -join '|')
if ($output -match $combinedPattern) {
    Write-Error "Android SDK package installation failed for '$Package'"
    exit 1
}

exit 0
