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
    
    # Restart ADB server to ensure clean connection state
    # This prevents "Broken pipe" errors from stale connections
    Write-Info "Restarting ADB server for clean connection..."
    adb kill-server 2>$null
    Start-Sleep -Seconds 2
    adb start-server 2>$null
    Start-Sleep -Seconds 2
    Write-Success "ADB server restarted"
    
    # Get device UDID if not provided OR if it's an AVD name that needs to be booted
    # Check if DeviceUdid is an AVD name (not an emulator-XXXX format)
    if ($DeviceUdid -and $DeviceUdid -notmatch "^emulator-\d+$") {
        # DeviceUdid is likely an AVD name - check if it's in the AVD list
        $avdList = emulator -list-avds 2>$null
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
        # Note: adb devices output can be:
        #   emulator-5554	device    (basic)
        #   emulator-5554          device product:... model:...    (with -l flag or some environments)
        # We match any line starting with emulator- and containing "device" as the state
        $runningDevices = adb devices | Select-String "^emulator-\d+\s+device"
        
        if ($runningDevices.Count -gt 0) {
            # Use first running device - extract just the emulator-XXXX part
            $DeviceUdid = ($runningDevices[0].Line -split '\s+')[0]
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
            # Force array even with single result to avoid string indexing issues
            $avdList = @(emulator -list-avds)
            
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
                $avdList = emulator -list-avds 2>$null
                
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
                # 1. API 34 device (matches CI provisioning)
                # 2. API 30 Nexus device
                # 3. Any API 30 device
                # 4. Any Nexus device
                # 5. First available device
                
                # Try to find API 34 device (CI default)
                $api34Device = $avdList | Where-Object { $_ -match "34|API.*34" } | Select-Object -First 1
                if ($api34Device) {
                    $selectedAvd = $api34Device
                    Write-Info "Selected API 34 device: $selectedAvd"
                }
                
                # Try to find API 30 Nexus device
                if (-not $selectedAvd) {
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
            }
            
            # Start emulator with selected AVD
            $emulatorBin = Join-Path $androidSdkRoot "emulator/emulator"
            if ($IsWindows) {
                $emulatorBin = "$emulatorBin.exe"
            }
            
            # Check emulator binary exists
            if (-not (Test-Path $emulatorBin)) {
                # Fallback: try to find emulator on PATH
                $emulatorCmd = Get-Command emulator -ErrorAction SilentlyContinue
                if ($emulatorCmd) {
                    $emulatorBin = $emulatorCmd.Source
                    Write-Info "Using emulator from PATH: $emulatorBin"
                } else {
                    Write-Error "Emulator binary not found at: $emulatorBin"
                    Write-Info "Looking for emulator in SDK..."
                    Get-ChildItem -Path $androidSdkRoot -Filter "emulator*" -Recurse -Depth 2 -ErrorAction SilentlyContinue | ForEach-Object { Write-Info "  Found: $($_.FullName)" }
                    exit 1
                }
            }
            
            Write-Info "Starting emulator: $selectedAvd"
            Write-Info "This may take 1-2 minutes..."
            
            # CRITICAL: Must use nohup to properly detach emulator process
            # This prevents STDIO stream inheritance issues in CI environments
            if ($IsWindows) {
                Start-Process "emulator" -ArgumentList "-avd", $selectedAvd, "-no-window", "-no-snapshot", "-no-audio", "-no-boot-anim" -WindowStyle Hidden
            }
            else {
                # macOS/Linux: Use nohup to fully detach the emulator process
                # This ensures the process doesn't inherit STDIO streams and can run independently
                $androidHome = $env:ANDROID_HOME
                if (-not $androidHome) {
                    $androidHome = $env:ANDROID_SDK_ROOT
                }
                if (-not $androidHome) {
                    $androidHome = "$env:HOME/Library/Android/sdk"
                }
                
                $emulatorBin = Join-Path $androidHome "emulator/emulator"
                
                if (-not (Test-Path $emulatorBin)) {
                    Write-Error "Emulator binary not found at: $emulatorBin"
                    Write-Info "Please ensure ANDROID_HOME or ANDROID_SDK_ROOT is set correctly."
                    exit 1
                }
                
                # Use nohup to fully detach the emulator process from the terminal
                # Include -no-window for headless CI environments
                # Redirect all output to /dev/null to prevent STDIO inheritance issues
                $startScript = "nohup '$emulatorBin' -avd '$selectedAvd' -no-window -no-snapshot -no-audio -no-boot-anim > /dev/null 2>&1 &"
                bash -c $startScript
                
                Write-Info "Emulator started in background (fully detached with nohup)"
            }
            
            # Use adb wait-for-device first (like the working bash script)
            Write-Info "Waiting for emulator device to appear..."
            adb wait-for-device
            
            # Wait for emulator to appear in adb devices with proper state
            Write-Info "Waiting for emulator to be ready..."
            $timeout = 120
            $elapsed = 0
            $emulatorStarted = $false
            
            while ($elapsed -lt $timeout) {
                $devices = adb devices | Select-String "emulator.*device$"
                if ($devices.Count -gt 0) {
                    $DeviceUdid = ($devices[0].Line -split '\s+')[0]
                    Write-Info "Emulator detected: $DeviceUdid"
                    break
                }
                
                Start-Sleep -Seconds 2
                $elapsed += 2
                
                if ($elapsed % 10 -eq 0) {
                    Write-Info "Still waiting... ($elapsed seconds elapsed)"
                }
            }
            
            if (-not $emulatorStarted) {
                Write-Error "Emulator failed to start within $timeout seconds. Please try starting it manually."
                adb devices -l
                exit 1
            }
            
            # Wait for boot to complete (poll sys.boot_completed like bash script)
            Write-Info "Waiting for emulator to finish booting..."
            $bootTimeout = 600
            $bootElapsed = 0
            
            while ($bootElapsed -lt $bootTimeout) {
                $bootStatus = adb shell getprop sys.boot_completed 2>$null
                if ($bootStatus -match "1") {
                    Write-Success "Emulator fully booted: $DeviceUdid"
                    break
                }
                
                Start-Sleep -Seconds 2
                $bootElapsed += 2
                
                if ($bootElapsed % 10 -eq 0) {
                    Write-Info "Waiting for boot... ($bootElapsed/$bootTimeout seconds)"
                }
            }
            
            if (-not $bootCompleted) {
                Write-Error "Emulator failed to complete boot within $bootTimeout seconds."
                adb devices -l
                exit 1
            }
        }
    }
    
    # Verify ADB connectivity is healthy (prevents "Broken pipe" errors during build)
    Write-Info "Verifying ADB connectivity to device..."
    $connectivityCheck = adb -s $DeviceUdid shell echo "ping" 2>&1
    if ($connectivityCheck -notmatch "ping") {
        Write-Info "ADB connectivity issue detected, restarting ADB server..."
        adb kill-server 2>$null
        Start-Sleep -Seconds 2
        adb start-server 2>$null
        Start-Sleep -Seconds 3
        
        # Retry connectivity check
        $retryCheck = adb -s $DeviceUdid shell echo "ping" 2>&1
        if ($retryCheck -notmatch "ping") {
            Write-Error "ADB connectivity failed after restart. Device may need to be rebooted."
            Write-Info "Try: adb -s $DeviceUdid reboot"
            exit 1
        }
        Write-Success "ADB connectivity restored after restart"
    } else {
        Write-Success "ADB connectivity verified"
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
        
        # Preferred devices in order of priority
        $preferredDevices = @("iPhone 16 Pro", "iPhone 15 Pro", "iPhone 14 Pro", "iPhone Xs")
        # Preferred iOS versions in order (newest first)
        $preferredVersions = @("iOS-18", "iOS-17", "iOS-26")
        
        $selectedDevice = $null
        $selectedVersion = $null
        
        # Try each preferred version
        foreach ($version in $preferredVersions) {
            if ($selectedDevice) { break }
            
            # Get all runtimes matching this version prefix
            $matchingRuntimes = $simList.devices.PSObject.Properties | 
                Where-Object { $_.Name -match $version }
            
            if ($matchingRuntimes) {
                # Try each preferred device
                foreach ($deviceName in $preferredDevices) {
                    $device = $matchingRuntimes | ForEach-Object { 
                        $_.Value | Where-Object { $_.name -eq $deviceName -and $_.isAvailable -eq $true }
                    } | Select-Object -First 1
                    
                    if ($device) {
                        $selectedDevice = $device
                        $selectedVersion = ($matchingRuntimes | Select-Object -First 1).Name
                        Write-Info "Found preferred device: $deviceName on $selectedVersion"
                        break
                    }
                }
                
                # If no preferred device found, take first available iPhone
                if (-not $selectedDevice) {
                    $anyiPhone = $matchingRuntimes | ForEach-Object { 
                        $_.Value | Where-Object { $_.name -match "iPhone" -and $_.isAvailable -eq $true }
                    } | Select-Object -First 1
                    
                    if ($anyiPhone) {
                        $selectedDevice = $anyiPhone
                        $selectedVersion = ($matchingRuntimes | Select-Object -First 1).Name
                        Write-Info "Using available iPhone: $($anyiPhone.name) on $selectedVersion"
                    }
                }
            }
        }
        
        # Last resort: find ANY available iPhone simulator
        if (-not $selectedDevice) {
            $allDevices = $simList.devices.PSObject.Properties | ForEach-Object { 
                $runtime = $_.Name
                $_.Value | Where-Object { $_.name -match "iPhone" -and $_.isAvailable -eq $true } | 
                    ForEach-Object { $_ | Add-Member -NotePropertyName "runtime" -NotePropertyValue $runtime -PassThru }
            }
            
            if ($allDevices) {
                $selectedDevice = $allDevices | Select-Object -First 1
                $selectedVersion = $selectedDevice.runtime
                Write-Info "Fallback: Using $($selectedDevice.name) on $selectedVersion"
            }
        }
        
        if (-not $selectedDevice) {
            Write-Error "No iPhone simulator found. Please create one in Xcode."
            Write-Info "Available simulators:"
            $simList.devices.PSObject.Properties | ForEach-Object { 
                $runtime = $_.Name
                $_.Value | Where-Object { $_.isAvailable -eq $true } | ForEach-Object { 
                    Write-Info "  - $($_.name) ($runtime) - $($_.udid)" 
                }
            }
            exit 1
        }
        
        $DeviceUdid = $selectedDevice.udid
    }
    
    # Get device name for display
    $simState = xcrun simctl list devices --json | ConvertFrom-Json
    $deviceInfo = $simState.devices.PSObject.Properties.Value | 
        ForEach-Object { $_ } | 
        Where-Object { $_.udid -eq $DeviceUdid } | 
        Select-Object -First 1
    $deviceName = if ($deviceInfo) { $deviceInfo.name } else { "Unknown" }
    
    Write-Success "iOS simulator: $deviceName ($DeviceUdid)"
    
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
