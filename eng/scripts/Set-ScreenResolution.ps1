<#
.SYNOPSIS

Sets the screen resolution on Windows.

.DESCRIPTION

This script programmatically sets the screen resolution on Windows machines
using the ChangeDisplaySettingsEx Windows API. It's designed to work on
Azure DevOps hosted and self-hosted Windows agents.

.PARAMETER Width

The desired screen width in pixels (e.g., 1920)

.PARAMETER Height

The desired screen height in pixels (e.g., 1080)

.EXAMPLE

PS> .\Set-ScreenResolution.ps1 -Width 1920 -Height 1080

This would set the screen resolution to 1920x1080

#>

[CmdletBinding()]
param (
    [Parameter(Mandatory = $true)]
    [int]$Width,
    
    [Parameter(Mandatory = $true)]
    [int]$Height
)

Set-StrictMode -Version 2.0
$ErrorActionPreference = "Stop"

function Set-ScreenResolution {
    param (
        [int]$Width,
        [int]$Height
    )
    
    Write-Host "Setting screen resolution to ${Width}x${Height}..."
    
    # Define the necessary Windows API structures and functions using Add-Type
    $pinvokeCode = @"
using System;
using System.Runtime.InteropServices;

namespace DisplaySettings
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmDeviceName;
        public short dmSpecVersion;
        public short dmDriverVersion;
        public short dmSize;
        public short dmDriverExtra;
        public int dmFields;
        public int dmPositionX;
        public int dmPositionY;
        public int dmDisplayOrientation;
        public int dmDisplayFixedOutput;
        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string dmFormName;
        public short dmLogPixels;
        public int dmBitsPerPel;
        public int dmPelsWidth;
        public int dmPelsHeight;
        public int dmDisplayFlags;
        public int dmDisplayFrequency;
        public int dmICMMethod;
        public int dmICMIntent;
        public int dmMediaType;
        public int dmDitherType;
        public int dmReserved1;
        public int dmReserved2;
        public int dmPanningWidth;
        public int dmPanningHeight;
    }

    public class User32
    {
        [DllImport("user32.dll")]
        public static extern int EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);
        
        [DllImport("user32.dll")]
        public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);
        
        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;
    }
}
"@

    # Add the type if it hasn't been added already
    if (-not ([System.Management.Automation.PSTypeName]'DisplaySettings.User32').Type) {
        try {
            Add-Type -TypeDefinition $pinvokeCode -ErrorAction Stop
        }
        catch {
            Write-Error "Failed to add Windows API type definitions: $_"
            return $false
        }
    }

    try {
        # Get current display settings
        $devMode = New-Object DisplaySettings.DEVMODE
        $devMode.dmSize = [System.Runtime.InteropServices.Marshal]::SizeOf($devMode)
        
        $result = [DisplaySettings.User32]::EnumDisplaySettings($null, [DisplaySettings.User32]::ENUM_CURRENT_SETTINGS, [ref]$devMode)
        
        if ($result -eq 0) {
            Write-Error "Failed to enumerate current display settings"
            return $false
        }
        
        Write-Host "Current resolution: $($devMode.dmPelsWidth)x$($devMode.dmPelsHeight)"
        
        # Check if the resolution is already set to the desired values
        if ($devMode.dmPelsWidth -eq $Width -and $devMode.dmPelsHeight -eq $Height) {
            Write-Host "Screen resolution is already set to ${Width}x${Height}"
            return $true
        }
        
        # Set new resolution
        $devMode.dmPelsWidth = $Width
        $devMode.dmPelsHeight = $Height
        
        # Test if the resolution is supported
        $testResult = [DisplaySettings.User32]::ChangeDisplaySettings([ref]$devMode, [DisplaySettings.User32]::CDS_TEST)
        
        if ($testResult -eq [DisplaySettings.User32]::DISP_CHANGE_FAILED) {
            Write-Error "The resolution ${Width}x${Height} is not supported by this display"
            return $false
        }
        
        # Apply the resolution change
        $changeResult = [DisplaySettings.User32]::ChangeDisplaySettings([ref]$devMode, [DisplaySettings.User32]::CDS_UPDATEREGISTRY)
        
        switch ($changeResult) {
            ([DisplaySettings.User32]::DISP_CHANGE_SUCCESSFUL) {
                Write-Host "Successfully set screen resolution to ${Width}x${Height}"
                return $true
            }
            ([DisplaySettings.User32]::DISP_CHANGE_RESTART) {
                Write-Warning "Screen resolution change requires a restart to take effect"
                return $true
            }
            ([DisplaySettings.User32]::DISP_CHANGE_FAILED) {
                Write-Error "Failed to change screen resolution"
                return $false
            }
            default {
                Write-Error "Unknown result code: $changeResult"
                return $false
            }
        }
    }
    catch {
        Write-Error "An error occurred while setting screen resolution: $_"
        Write-Error $_.ScriptStackTrace
        return $false
    }
}

# Main execution
try {
    $success = Set-ScreenResolution -Width $Width -Height $Height
    
    if (-not $success) {
        exit 1
    }
    
    exit 0
}
catch {
    Write-Error "Script execution failed: $_"
    Write-Error $_.ScriptStackTrace
    exit 1
}
