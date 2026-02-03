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
    # This matches the CI bash script approach exactly
    
    Write-Info "=== Booting Android Emulator ==="
    
    # Check adb
    if (-not (Get-Command "adb" -ErrorAction SilentlyContinue)) {
        Write-Error "Android SDK (adb) not found. Please install Android SDK and ensure 'adb' is in PATH."
        exit 1
    }
    
    # Get Android SDK path
    $androidSdkRoot = $env:ANDROID_SDK_ROOT
    if (-not $androidSdkRoot) {
        $androidSdkRoot = $env:ANDROID_HOME
    }
    if (-not $androidSdkRoot) {
        $androidSdkRoot = "$env:HOME/Library/Android/sdk"
    }
    
    # Check for already running device
    $runningDevices = adb devices | Select-String "emulator.*device$"
    if ($runningDevices.Count -gt 0) {
        $DeviceUdid = ($runningDevices[0] -split '\s+')[0]
        Write-Success "Found running Android device: $DeviceUdid"
    }
    else {
        # Start emulator in background, detached from terminal (exactly like CI)
        $emulatorBin = Join-Path $androidSdkRoot "emulator/emulator"
        $avdName = if ($DeviceUdid) { $DeviceUdid } else { "Emulator_34" }
        
        # Check emulator binary exists
        if (-not (Test-Path $emulatorBin)) {
            Write-Error "Emulator binary not found at: $emulatorBin"
            Write-Info "Looking for emulator in SDK..."
            Get-ChildItem -Path $androidSdkRoot -Filter "emulator*" -Recurse -Depth 2 -ErrorAction SilentlyContinue | ForEach-Object { Write-Info "  Found: $($_.FullName)" }
            exit 1
        }
        
        # Check AVD exists
        Write-Info "Available AVDs:"
        $avdListOutput = & "$androidSdkRoot/cmdline-tools/latest/bin/avdmanager" list avd 2>&1
        Write-Info $avdListOutput
        
        Write-Info "Starting emulator: $avdName"
        Write-Info "Emulator command: $emulatorBin -avd $avdName -no-window -no-snapshot -no-audio -no-boot-anim -gpu swiftshader_indirect"
        
        # Use swiftshader for software graphics rendering (more reliable on CI without GPU)
        # Use swiftshader for software rendering (more reliable on CI without GPU)
        # Redirect output to a log file for debugging
        $emulatorLog = "/tmp/emulator-$avdName.log"
        $startScript = "nohup '$emulatorBin' -avd '$avdName' -no-window -no-snapshot -no-audio -no-boot-anim -gpu swiftshader_indirect > '$emulatorLog' 2>&1 &"
        bash -c $startScript
        
        # Give the emulator process time to start
        Start-Sleep -Seconds 5
        
        # Check if emulator process is running
        $emulatorProcs = bash -c "pgrep -f 'qemu.*$avdName' || pgrep -f 'emulator.*$avdName' || true" 2>&1
        if ([string]::IsNullOrWhiteSpace($emulatorProcs)) {
            Write-Error "Emulator process did not start. Checking log..."
            if (Test-Path $emulatorLog) {
                Get-Content $emulatorLog | Select-Object -Last 50 | ForEach-Object { Write-Info "  $_" }
            }
            exit 1
        }
        Write-Info "Emulator process started (PIDs: $emulatorProcs)"
        
        # Wait for device to appear with timeout (don't use adb wait-for-device - it can hang forever)
        # Increased timeout to 300s as emulator on CI can take 2-5 minutes to boot on slow hardware
        Write-Info "Waiting for emulator device to appear..."
        $deviceTimeout = 300
        $deviceWaited = 0
        $DeviceUdid = $null
        
        while ($deviceWaited -lt $deviceTimeout) {
            $runningDevices = adb devices | Select-String "emulator"
            if ($runningDevices.Count -gt 0) {
                $firstDevice = ($runningDevices[0] -split '\s+')[0]
                $deviceState = ($runningDevices[0] -split '\s+')[1]
                if ($deviceState -eq "device") {
                    $DeviceUdid = $firstDevice
                    break
                }
                Write-Info "Device found but state is '$deviceState', waiting..."
            }
            Start-Sleep -Seconds 5
            $deviceWaited += 5
            Write-Info "Waiting for device... ($deviceWaited/$deviceTimeout seconds)"
            
            # Show emulator log tail if taking too long
            if ($deviceWaited -ge 30 -and ($deviceWaited % 30 -eq 0) -and (Test-Path $emulatorLog)) {
                Write-Info "Emulator log (last 10 lines):"
                Get-Content $emulatorLog | Select-Object -Last 10 | ForEach-Object { Write-Info "  $_" }
            }
        }
        
        if (-not $DeviceUdid) {
            Write-Error "Emulator device did not appear in time"
            Write-Info "Current adb devices:"
            adb devices -l
            if (Test-Path $emulatorLog) {
                Write-Info "Emulator log (last 30 lines):"
                Get-Content $emulatorLog | Select-Object -Last 30 | ForEach-Object { Write-Info "  $_" }
            }
            exit 1
        }
        
        Write-Info "Emulator started with device ID: $DeviceUdid"
        
        # Wait for boot_completed (exactly like CI)
        Write-Info "Waiting for emulator to finish booting..."
        $timeout = 180
        $waited = 0
        
        while ($waited -lt $timeout) {
            $bootStatus = adb -s $DeviceUdid shell getprop sys.boot_completed 2>$null
            if ($bootStatus -match "1") {
                break
            }
            Start-Sleep -Seconds 5
            $waited += 5
            Write-Info "Waiting for boot... ($waited/$timeout seconds)"
        }
        
        if ($waited -ge $timeout) {
            Write-Error "Emulator did not boot in time"
            adb devices -l
            if (Test-Path $emulatorLog) {
                Write-Info "Emulator log (last 30 lines):"
                Get-Content $emulatorLog | Select-Object -Last 30 | ForEach-Object { Write-Info "  $_" }
            }
            exit 1
        }
    }
    
    Write-Success "=== Emulator booted successfully! ==="
    adb devices -l
    
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
