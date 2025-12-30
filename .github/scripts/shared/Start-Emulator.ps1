#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts Android or iOS emulator/simulator if not already running.

.DESCRIPTION
    Handles device detection and startup for both Android and iOS platforms.
    - Android: Automatically selects and starts emulator with priority: API 30 Nexus > API 30 > Nexus > First available
    - iOS: Automatically selects iPhone Xs with iOS 18.5 by default

.PARAMETER Platform
    Target platform: "android" or "ios"

.PARAMETER DeviceUdid
    (Optional) Specific device UDID to use. If not provided, auto-detects/starts appropriate device.

.OUTPUTS
    Returns device UDID via $env:DEVICE_UDID environment variable

.EXAMPLE
    pwsh Start-Emulator.ps1 -Platform android
    # Auto-detects or starts Android emulator, returns UDID

.EXAMPLE
    pwsh Start-Emulator.ps1 -Platform ios -DeviceUdid "AC8BCB28-A72D-4A2D-90E7-E78FF0BA07EE"
    # Uses specific iOS simulator
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("android", "ios")]
    [string]$Platform,
    
    [Parameter(Mandatory=$false)]
    [string]$DeviceUdid
)

# Import shared utilities
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptDir "shared-utils.ps1")

Write-Step "Detecting and starting $Platform device..."

if ($Platform -eq "android") {
    #region Android Device Detection and Startup
    
    # Check adb
    if (-not (Get-Command "adb" -ErrorAction SilentlyContinue)) {
        Write-Error "Android SDK (adb) not found. Please install Android SDK and ensure 'adb' is in PATH."
        exit 1
    }
    
    # Get device UDID if not provided OR if it's an AVD name that needs to be booted
    # Check if DeviceUdid is an AVD name (not an emulator-XXXX format)
    if ($DeviceUdid -and $DeviceUdid -notmatch "^emulator-\d+$") {
        # DeviceUdid is likely an AVD name - check if it's in the AVD list
        $avdList = emulator -list-avds
        if ($avdList -contains $DeviceUdid) {
            Write-Info "DeviceUdid '$DeviceUdid' is an AVD name. Will boot this emulator..."
            $selectedAvd = $DeviceUdid
            $DeviceUdid = $null  # Clear so we boot and get actual device ID below
        } else {
            Write-Error "DeviceUdid '$DeviceUdid' is not a valid emulator ID or AVD name."
            Write-Info "Available AVDs: $($avdList -join ', ')"
            exit 1
        }
    }
    
    if (-not $DeviceUdid) {
        Write-Info "Auto-detecting Android device..."
        
        # Check for running devices first
        $runningDevices = adb devices | Select-String "device$"
        
        if ($runningDevices.Count -gt 0) {
            # Use first running device
            $DeviceUdid = ($runningDevices[0] -split '\s+')[0]
            Write-Success "Found running Android device: $DeviceUdid"
        }
        else {
            Write-Info "No running devices found. Looking for available emulators..."
            
            # Check if emulator command exists
            if (-not (Get-Command "emulator" -ErrorAction SilentlyContinue)) {
                Write-Error "No running Android devices and 'emulator' command not found. Please start an emulator or connect a device."
                exit 1
            }
            
            # Get list of available AVDs
            $avdList = emulator -list-avds
            
            if (-not $avdList -or $avdList.Count -eq 0) {
                Write-Error "No Android emulators found. Please create an Android Virtual Device (AVD) using Android Studio."
                Write-Info "To create an AVD:"
                Write-Info "  1. Open Android Studio"
                Write-Info "  2. Go to Tools > Device Manager"
                Write-Info "  3. Click 'Create Device' and follow the wizard"
                exit 1
            }
            
            Write-Info "Available emulators: $($avdList -join ', ')"
            
            # Selection priority:
            # 1. API 30 Nexus device
            # 2. Any API 30 device
            # 3. Any Nexus device
            # 4. First available device
            
            # $selectedAvd may already be set if AVD name was provided
            # Only run auto-selection if not already set
            if (-not $selectedAvd) {
                # Try to find API 30 Nexus device
                $api30Nexus = $avdList | Where-Object { $_ -match "API.*30" -and $_ -match "Nexus" } | Select-Object -First 1
                if ($api30Nexus) {
                    $selectedAvd = $api30Nexus
                    Write-Info "Selected API 30 Nexus device: $selectedAvd"
                }
            }
            
            # Try to find any API 30 device
            if (-not $selectedAvd) {
                $api30Device = $avdList | Where-Object { $_ -match "API.*30" } | Select-Object -First 1
                if ($api30Device) {
                    $selectedAvd = $api30Device
                    Write-Info "Selected API 30 device: $selectedAvd"
                }
            }
            
            # Try to find any Nexus device
            if (-not $selectedAvd) {
                $nexusDevice = $avdList | Where-Object { $_ -match "Nexus" } | Select-Object -First 1
                if ($nexusDevice) {
                    $selectedAvd = $nexusDevice
                    Write-Info "Selected Nexus device: $selectedAvd"
                }
            }
            
            # Fall back to first available device
            if (-not $selectedAvd) {
                $selectedAvd = $avdList[0]
                Write-Info "Selected first available device: $selectedAvd"
            }
            
            Write-Info "Starting emulator: $selectedAvd"
            Write-Info "This may take 1-2 minutes..."
            
            # CRITICAL: Must use correct startup pattern for emulator to work
            # On macOS/Linux, need to cd to emulator directory and use subshell
            if ($IsWindows) {
                Start-Process "emulator" -ArgumentList "-avd", $selectedAvd, "-no-snapshot-load", "-no-boot-anim" -WindowStyle Hidden
            }
            else {
                # macOS/Linux: Use bash subshell pattern from platform-workflows.md
                # This ensures emulator binary can find its dependencies
                $androidHome = $env:ANDROID_HOME
                if (-not $androidHome) {
                    $androidHome = "$env:HOME/Library/Android/sdk"
                }
                
                $emulatorDir = Join-Path $androidHome "emulator"
                $emulatorBin = Join-Path $emulatorDir "emulator"
                
                if (-not (Test-Path $emulatorBin)) {
                    Write-Error "Emulator binary not found at: $emulatorBin"
                    Write-Info "Please ensure ANDROID_HOME is set correctly or Android SDK is installed."
                    exit 1
                }
                
                # Start emulator using bash subshell pattern (works correctly on macOS)
                $startScript = "cd '$emulatorDir' && (./emulator -avd '$selectedAvd' -no-snapshot-load -no-audio -no-boot-anim > /tmp/emulator.log 2>&1 &)"
                bash -c $startScript
                
                Write-Info "Emulator started in background. Log file: /tmp/emulator.log"
            }
            
            # Wait for emulator to appear in adb devices
            Write-Info "Waiting for emulator to start..."
            $timeout = 120
            $elapsed = 0
            $emulatorStarted = $false
            
            while ($elapsed -lt $timeout) {
                Start-Sleep -Seconds 2
                $elapsed += 2
                
                $devices = adb devices | Select-String "emulator.*device$"
                if ($devices.Count -gt 0) {
                    $DeviceUdid = ($devices[0] -split '\s+')[0]
                    $emulatorStarted = $true
                    Write-Info "Emulator detected: $DeviceUdid"
                    break
                }
                
                if ($elapsed % 10 -eq 0) {
                    Write-Info "Still waiting... ($elapsed seconds elapsed)"
                }
            }
            
            if (-not $emulatorStarted) {
                Write-Error "Emulator failed to start within $timeout seconds. Please try starting it manually."
                exit 1
            }
            
            # Wait for boot to complete
            Write-Info "Waiting for emulator to finish booting..."
            $bootTimeout = 120
            $bootElapsed = 0
            $bootCompleted = $false
            
            while ($bootElapsed -lt $bootTimeout) {
                Start-Sleep -Seconds 2
                $bootElapsed += 2
                
                $bootStatus = adb -s $DeviceUdid shell getprop sys.boot_completed 2>$null
                if ($bootStatus -match "1") {
                    $bootCompleted = $true
                    Write-Success "Emulator fully booted: $DeviceUdid"
                    break
                }
                
                if ($bootElapsed % 10 -eq 0) {
                    Write-Info "Still booting... ($bootElapsed seconds elapsed)"
                }
            }
            
            if (-not $bootCompleted) {
                Write-Error "Emulator failed to complete boot within $bootTimeout seconds. It may still be starting."
                Write-Info "You can check status with: adb -s $DeviceUdid shell getprop sys.boot_completed"
                exit 1
            }
        }
    }
    
    Write-Success "Using Android device: $DeviceUdid"
    
    #endregion
    
} elseif ($Platform -eq "ios") {
    #region iOS Simulator Detection and Startup
    
    # Check xcrun (iOS tools)
    if (-not (Get-Command "xcrun" -ErrorAction SilentlyContinue)) {
        Write-Error "Xcode command line tools not found. This script requires macOS with Xcode installed."
        exit 1
    }
    
    # Get device UDID if not provided
    if (-not $DeviceUdid) {
        Write-Info "Auto-detecting iOS simulator..."
        $simList = xcrun simctl list devices available --json | ConvertFrom-Json
        
        # Find iPhone Xs with iOS 18.5 (default for UI tests)
        $iPhoneXs = $simList.devices.PSObject.Properties | 
            Where-Object { $_.Name -match "iOS-18-5" } |
            ForEach-Object { 
                $_.Value | Where-Object { $_.name -eq "iPhone Xs" }
            } | 
            Select-Object -First 1
        
        if (-not $iPhoneXs) {
            Write-Error "No iPhone Xs simulator found with iOS 18.5. Please create one in Xcode."
            Write-Info "Available iOS 18.5 simulators:"
            $simList.devices.PSObject.Properties | 
                Where-Object { $_.Name -match "iOS-18-5" } |
                ForEach-Object { 
                    $_.Value | ForEach-Object { Write-Info "  - $($_.name) ($($_.udid))" }
                }
            exit 1
        }
        
        $DeviceUdid = $iPhoneXs.udid
    }
    
    Write-Success "iOS simulator: $DeviceUdid (iOS 18.5)"
    
    # Boot simulator if not already booted
    Write-Info "Booting simulator (if not already running)..."
    xcrun simctl boot $DeviceUdid 2>$null
    
    # Verify booted
    $simState = xcrun simctl list devices --json | ConvertFrom-Json
    $device = $simState.devices.PSObject.Properties.Value | 
        ForEach-Object { $_ } | 
        Where-Object { $_.udid -eq $DeviceUdid } | 
        Select-Object -First 1
    
    if ($device.state -ne "Booted") {
        Write-Error "Simulator failed to boot. Current state: $($device.state)"
        exit 1
    }
    
    Write-Success "Simulator is booted and ready"
    
    #endregion
}

# Export device UDID as environment variable
$env:DEVICE_UDID = $DeviceUdid
Write-Success "DEVICE_UDID environment variable set: $DeviceUdid"

# Return UDID for callers
return $DeviceUdid
