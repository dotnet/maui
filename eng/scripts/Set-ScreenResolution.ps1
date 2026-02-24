<#
.SYNOPSIS

Sets the screen resolution on Windows.

.DESCRIPTION

This script programmatically sets the screen resolution on Windows machines
using WMI and Windows API. It's designed to work on Azure DevOps hosted 
and self-hosted Windows agents.

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
    
    # Define the complete DEVMODE structure with correct size and layout
    $pinvokeCode = @"
using System;
using System.Runtime.InteropServices;

namespace DisplaySettings
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct DEVMODE
    {
        private const int CCHDEVICENAME = 32;
        private const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
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

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
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

    public class NativeMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool EnumDisplaySettings(string lpszDeviceName, int iModeNum, ref DEVMODE lpDevMode);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int ChangeDisplaySettings(ref DEVMODE lpDevMode, int dwFlags);

        public const int ENUM_CURRENT_SETTINGS = -1;
        public const int ENUM_REGISTRY_SETTINGS = -2;
        public const int CDS_UPDATEREGISTRY = 0x01;
        public const int CDS_TEST = 0x02;
        public const int DISP_CHANGE_SUCCESSFUL = 0;
        public const int DISP_CHANGE_RESTART = 1;
        public const int DISP_CHANGE_FAILED = -1;
        public const int DISP_CHANGE_BADMODE = -2;
        public const int DISP_CHANGE_NOTUPDATED = -3;
        public const int DISP_CHANGE_BADFLAGS = -4;
        public const int DISP_CHANGE_BADPARAM = -5;
        
        public const int DM_PELSWIDTH = 0x80000;
        public const int DM_PELSHEIGHT = 0x100000;
    }
}
"@

    # Add the type if it hasn't been added already
    if (-not ([System.Management.Automation.PSTypeName]'DisplaySettings.NativeMethods').Type) {
        try {
            Add-Type -TypeDefinition $pinvokeCode -ErrorAction Stop
        }
        catch {
            Write-Error "Failed to add Windows API type definitions: $_"
            return $false
        }
    }

    try {
        # Initialize DEVMODE structure
        $devMode = New-Object DisplaySettings.DEVMODE
        $devMode.dmSize = [System.Runtime.InteropServices.Marshal]::SizeOf($devMode)
        
        # Get current display settings
        $result = [DisplaySettings.NativeMethods]::EnumDisplaySettings($null, [DisplaySettings.NativeMethods]::ENUM_CURRENT_SETTINGS, [ref]$devMode)
        
        if (-not $result) {
            Write-Warning "Could not enumerate current display settings. Attempting to set resolution anyway..."
            # Initialize a new DEVMODE for setting resolution
            $devMode = New-Object DisplaySettings.DEVMODE
            $devMode.dmSize = [System.Runtime.InteropServices.Marshal]::SizeOf($devMode)
        }
        else {
            Write-Host "Current resolution: $($devMode.dmPelsWidth)x$($devMode.dmPelsHeight)"
            
            # Check if the resolution is already set to the desired values
            if ($devMode.dmPelsWidth -eq $Width -and $devMode.dmPelsHeight -eq $Height) {
                Write-Host "Screen resolution is already set to ${Width}x${Height}"
                return $true
            }
        }
        
        # Set new resolution
        $devMode.dmPelsWidth = $Width
        $devMode.dmPelsHeight = $Height
        $devMode.dmFields = [DisplaySettings.NativeMethods]::DM_PELSWIDTH -bor [DisplaySettings.NativeMethods]::DM_PELSHEIGHT
        
        # Test if the resolution is supported
        $testResult = [DisplaySettings.NativeMethods]::ChangeDisplaySettings([ref]$devMode, [DisplaySettings.NativeMethods]::CDS_TEST)
        
        if ($testResult -ne [DisplaySettings.NativeMethods]::DISP_CHANGE_SUCCESSFUL) {
            Write-Warning "Resolution test returned code: $testResult"
            
            switch ($testResult) {
                ([DisplaySettings.NativeMethods]::DISP_CHANGE_BADMODE) {
                    Write-Error "The resolution ${Width}x${Height} is not supported by this display (BADMODE)"
                    return $false
                }
                ([DisplaySettings.NativeMethods]::DISP_CHANGE_FAILED) {
                    Write-Error "The resolution change test failed (FAILED)"
                    return $false
                }
                default {
                    Write-Warning "Unexpected test result, attempting to apply anyway..."
                }
            }
        }
        
        # Apply the resolution change
        $changeResult = [DisplaySettings.NativeMethods]::ChangeDisplaySettings([ref]$devMode, [DisplaySettings.NativeMethods]::CDS_UPDATEREGISTRY)
        
        switch ($changeResult) {
            ([DisplaySettings.NativeMethods]::DISP_CHANGE_SUCCESSFUL) {
                Write-Host "Successfully set screen resolution to ${Width}x${Height}"
                return $true
            }
            ([DisplaySettings.NativeMethods]::DISP_CHANGE_RESTART) {
                Write-Host "Screen resolution set to ${Width}x${Height}. A restart may be required for some applications."
                return $true
            }
            ([DisplaySettings.NativeMethods]::DISP_CHANGE_BADMODE) {
                Write-Error "The resolution ${Width}x${Height} is not supported by this display"
                return $false
            }
            ([DisplaySettings.NativeMethods]::DISP_CHANGE_FAILED) {
                Write-Error "Failed to change screen resolution"
                return $false
            }
            ([DisplaySettings.NativeMethods]::DISP_CHANGE_NOTUPDATED) {
                Write-Error "Failed to update registry with new resolution"
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
