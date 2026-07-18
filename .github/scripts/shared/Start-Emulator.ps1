#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts Android or iOS emulator/simulator if not already running.

.DESCRIPTION
    Handles device detection and startup for both Android and iOS platforms.
    - Android: Automatically selects and starts emulator with priority: API 30 Nexus > API 30 > Nexus > First available
    - iOS: Automatically selects iPhone Xs with iOS 18.x (or iPhone 11 Pro with iOS 26.x) to match CI

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
    [string]$DeviceUdid,

    [Parameter(Mandatory=$false)]
    [switch]$Headless
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
    
    # Get Android SDK path
    $androidSdkRoot = $env:ANDROID_SDK_ROOT
    if (-not $androidSdkRoot) {
        $androidSdkRoot = $env:ANDROID_HOME
    }
    if (-not $androidSdkRoot) {
        $androidSdkRoot = "$env:HOME/Library/Android/sdk"
    }
    
    # Track which AVD to boot (may be set from DeviceUdid parameter if it's an AVD name)
    $selectedAvd = $null
    
    # Check if DeviceUdid is an AVD name (not an emulator-XXXX format)
    if ($DeviceUdid -and $DeviceUdid -notmatch "^emulator-\d+$") {
        # DeviceUdid is likely an AVD name - check if it's in the AVD list
        # Force array output - single AVD returns a string which breaks -contains
        [string[]]$avdList = @(emulator -list-avds 2>$null)
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
            
            # Get list of available AVDs (if not already set from parameter)
            if (-not $selectedAvd) {
                # Force array output - single AVD returns a string which breaks indexing
                [string[]]$avdList = @(emulator -list-avds 2>$null)
                
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
                # 3. Any API 30 device (matches names like "Emulator_30", "API_30_xxx", etc.)
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
                    $api30Nexus = $avdList | Where-Object { $_ -match "30" -and $_ -match "Nexus" } | Select-Object -First 1
                    if ($api30Nexus) {
                        $selectedAvd = $api30Nexus
                        Write-Info "Selected API 30 Nexus device: $selectedAvd"
                    }
                }
                
                # Try to find any API 30 device (match "30" anywhere in name)
                if (-not $selectedAvd) {
                    $api30Device = $avdList | Where-Object { $_ -match "30" } | Select-Object -First 1
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
            
            # Guard against launching a SECOND emulator for an AVD that is already
            # booting. The stage-level "Create AVD and Boot Android Emulator" task
            # (or a prior gate test run in the same job) may have already started
            # $selectedAvd while it is still *offline* mid cold-boot — a state the
            # online-only "adb devices ... device" probe above does not match. Two
            # emulators sharing one AVD abort with FATAL "Running multiple emulators
            # with the same AVD", leaving every instance offline until the timeout
            # and failing the gate with a false INCONCLUSIVE. If a process for this
            # AVD already exists, reuse it and skip straight to the wait loop.
            $emulatorLog = Join-Path ([System.IO.Path]::GetTempPath()) "emulator-$selectedAvd.log"
            if ($IsWindows) {
                $existingAvdProc = (Get-Process -Name "emulator*","qemu*" -ErrorAction SilentlyContinue |
                    Where-Object { $_.CommandLine -match [regex]::Escape($selectedAvd) }).Id -join "`n"
            } else {
                $existingAvdProc = bash -c "pgrep -f 'qemu.*$selectedAvd' || pgrep -f 'emulator.*$selectedAvd' || true" 2>&1
            }
            $reuseExistingEmulator = -not [string]::IsNullOrWhiteSpace($existingAvdProc)

            if ($reuseExistingEmulator) {
                Write-Info "Emulator for AVD '$selectedAvd' is already running (PIDs: $existingAvdProc). Reusing it instead of starting a duplicate (avoids the same-AVD FATAL conflict)."
            }
            else {
            Write-Info "Starting emulator: $selectedAvd"
            Write-Info "This may take 1-2 minutes..."
            
            # Use swiftshader for software rendering (more reliable on CI without GPU)
            # Redirect output to a log file for debugging
            $emulatorLog = Join-Path ([System.IO.Path]::GetTempPath()) "emulator-$selectedAvd.log"
            
            # Use -no-window only when explicitly headless or running in CI
            $useHeadless = $Headless -or $env:CI -or $env:TF_BUILD -or $env:GITHUB_ACTIONS
            
            if ($IsWindows) {
                $windowStyle = if ($useHeadless) { "Hidden" } else { "Normal" }
                Start-Process $emulatorBin -ArgumentList "-avd", $selectedAvd, "-no-snapshot-load", "-no-boot-anim", "-gpu", "swiftshader_indirect" -WindowStyle $windowStyle
            }
            else {
                # macOS/Linux: Use nohup to detach from terminal
                # Use -no-snapshot (not -no-snapshot-load) to ensure clean emulator state for CI/testing.
                # This disables both snapshot load and save, so each boot is a cold boot.
                # Trade-off: slower boots, but guarantees no stale state between test runs.
                $windowFlag = if ($useHeadless) { "-no-window" } else { "" }
                $startScript = "nohup '$emulatorBin' -avd '$selectedAvd' $windowFlag -no-snapshot -no-audio -no-boot-anim -gpu swiftshader_indirect > '$emulatorLog' 2>&1 &"
                bash -c $startScript
                if ($useHeadless) {
                    Write-Info "Emulator started headless (no window). Log file: $emulatorLog"
                } else {
                    Write-Info "Emulator started with window. Log file: $emulatorLog"
                }
            }
            
            # Give the emulator process time to start
            Start-Sleep -Seconds 5
            
            # Check if emulator process is running
            if ($IsWindows) {
                $emulatorProcs = (Get-Process -Name "emulator*","qemu*" -ErrorAction SilentlyContinue | 
                    Where-Object { $_.CommandLine -match [regex]::Escape($selectedAvd) }).Id -join "`n"
            } else {
                $emulatorProcs = bash -c "pgrep -f 'qemu.*$selectedAvd' || pgrep -f 'emulator.*$selectedAvd' || true" 2>&1
            }
            if ([string]::IsNullOrWhiteSpace($emulatorProcs)) {
                Write-Error "Emulator process did not start. Checking log..."
                if (Test-Path $emulatorLog) {
                    Get-Content $emulatorLog | Select-Object -Last 50 | ForEach-Object { Write-Info "  $_" }
                }
                exit 1
            }
            Write-Info "Emulator process started (PIDs: $emulatorProcs)"
            }
            
            # Wait for device to appear with timeout
            # 240s (4 min): CI agents here have only 2 CPU cores (the emulator log
            # warns "will run more smoothly with 4 CPU cores"), so a cold
            # -no-snapshot boot can take well over 2 minutes to register an ADB
            # device. A too-short timeout turns a slow-but-healthy boot into a
            # false gate INCONCLUSIVE.
            Write-Info "Waiting for emulator device to appear..."
            $deviceTimeout = 240
            $deviceWaited = 0
            $offlineStreak = 0
            $wedgedRecoveryDone = $false
            
            while ($deviceWaited -lt $deviceTimeout) {
                # Match any emulator device line
                $devices = adb devices | Select-String "^emulator-\d+\s+device"
                if ($devices.Count -gt 0) {
                    $DeviceUdid = ($devices[0].Line -split '\s+')[0]
                    Write-Info "Emulator detected: $DeviceUdid"
                    break
                }
                
                # Check for offline state
                $offlineDevices = adb devices | Select-String "^emulator-\d+\s+offline"
                if ($offlineDevices.Count -gt 0) {
                    Write-Info "Device found but offline, waiting..."
                    $offlineStreak += 5
                    # A booted emulator can drop to 'offline' when the ADB channel
                    # stalls under CPU pressure (2-core agents building the app).
                    # This is the exact state that used to trip the duplicate-AVD
                    # FATAL. Actively nudge adb to reconnect the offline transport
                    # (cheap, non-destructive) instead of only waiting — this often
                    # recovers the existing emulator without a kill+reboot cycle.
                    if ($deviceWaited % 30 -eq 0) {
                        adb reconnect offline 2>&1 | ForEach-Object { Write-Info "  adb reconnect: $_" }
                    }
                    # WEDGED-EMULATOR RECOVERY (build 14689943, #36586 android: a prior
                    # 'Warm Up' step launched Emulator_30 but the process wedged
                    # 'offline' permanently — adb only ever saw it reconnecting. The
                    # reuse path above trusted the live process and waited the full
                    # 240s on ALL 3 gate retries -> false INCONCLUSIVE). adb reconnect
                    # cannot revive a truly wedged emulator, so ONCE, after a long
                    # *continuous* offline streak on a REUSED emulator (which already
                    # had the warmup's head-start to boot, so a 150s+ offline streak
                    # means wedged, not merely slow), hard-kill it by PID and cold-boot
                    # a fresh instance, then restart the wait clock. Single-shot
                    # (guarded) so it can never loop; if the fresh boot also fails we
                    # fall through to the same timeout/exit 1 as before -> never worse
                    # than today. Gated on $reuseExistingEmulator so a fresh (possibly
                    # just-slow) cold boot is never killed prematurely.
                    if ((-not $wedgedRecoveryDone) -and $reuseExistingEmulator -and ($offlineStreak -ge 150)) {
                        Write-Info "Reused emulator '$selectedAvd' stuck offline ${offlineStreak}s despite adb reconnect — killing it by PID and cold-booting a fresh instance (one-time recovery)."
                        if ($IsWindows) {
                            $wedgedPids = (Get-Process -Name "emulator*","qemu*" -ErrorAction SilentlyContinue |
                                Where-Object { $_.CommandLine -match [regex]::Escape($selectedAvd) }).Id
                            foreach ($wp in $wedgedPids) { Stop-Process -Id $wp -Force -ErrorAction SilentlyContinue }
                        } else {
                            bash -c "pgrep -f 'qemu.*$selectedAvd' | xargs -r kill -9; pgrep -f 'emulator.*$selectedAvd' | xargs -r kill -9; true" 2>&1 | Out-Null
                        }
                        Start-Sleep -Seconds 5
                        adb kill-server 2>&1 | Out-Null
                        adb start-server 2>&1 | Out-Null
                        $useHeadless = $Headless -or $env:CI -or $env:TF_BUILD -or $env:GITHUB_ACTIONS
                        if ($IsWindows) {
                            $windowStyle = if ($useHeadless) { "Hidden" } else { "Normal" }
                            Start-Process $emulatorBin -ArgumentList "-avd", $selectedAvd, "-no-snapshot", "-no-boot-anim", "-gpu", "swiftshader_indirect" -WindowStyle $windowStyle
                        } else {
                            $windowFlag = if ($useHeadless) { "-no-window" } else { "" }
                            bash -c "nohup '$emulatorBin' -avd '$selectedAvd' $windowFlag -no-snapshot -no-audio -no-boot-anim -gpu swiftshader_indirect > '$emulatorLog' 2>&1 &"
                        }
                        Write-Info "Fresh emulator cold-boot issued after wedged recovery; resetting wait clock (up to $deviceTimeout s)."
                        $wedgedRecoveryDone = $true
                        $offlineStreak = 0
                        $deviceWaited = 0
                        Start-Sleep -Seconds 5
                        continue
                    }
                }
                else {
                    $offlineStreak = 0
                }
                
                Start-Sleep -Seconds 5
                $deviceWaited += 5
                
                if ($deviceWaited % 30 -eq 0) {
                    Write-Info "Still waiting... ($deviceWaited seconds elapsed)"
                    # Show emulator log tail if taking too long
                    if ((Test-Path $emulatorLog)) {
                        Write-Info "Emulator log (last 5 lines):"
                        Get-Content $emulatorLog | Select-Object -Last 5 | ForEach-Object { Write-Info "  $_" }
                    }
                }
            }
            
            if (-not $DeviceUdid) {
                Write-Error "Emulator failed to start within $deviceTimeout seconds. Please try starting it manually."
                Write-Info "Current adb devices:"
                adb devices -l
                if (Test-Path $emulatorLog) {
                    Write-Info "Emulator log (last 30 lines):"
                    Get-Content $emulatorLog | Select-Object -Last 30 | ForEach-Object { Write-Info "  $_" }
                }
                exit 1
            }
            
            # Wait for boot to complete
            Write-Info "Waiting for emulator to finish booting..."
            $bootTimeout = 600
            $bootElapsed = 0
            
            while ($bootElapsed -lt $bootTimeout) {
                $bootStatus = adb -s $DeviceUdid shell getprop sys.boot_completed 2>$null
                if ($bootStatus -match "1") {
                    Write-Success "Emulator fully booted: $DeviceUdid"
                    break
                }
                
                Start-Sleep -Seconds 5
                $bootElapsed += 5
                
                if ($bootElapsed % 30 -eq 0) {
                    Write-Info "Still booting... ($bootElapsed seconds elapsed)"
                }
            }
            
            if ($bootElapsed -ge $bootTimeout) {
                Write-Error "Emulator failed to complete boot within $bootTimeout seconds."
                Write-Info "You can check status with: adb -s $DeviceUdid shell getprop sys.boot_completed"
                if (Test-Path $emulatorLog) {
                    Write-Info "Emulator log (last 30 lines):"
                    Get-Content $emulatorLog | Select-Object -Last 30 | ForEach-Object { Write-Info "  $_" }
                }
                exit 1
            }
            
            # Wait for package manager service to be available (critical for app installation)
            Write-Info "Waiting for package manager service..."
            $pmTimeout = 120
            $pmWaited = 0
            
            while ($pmWaited -lt $pmTimeout) {
                $pmOutput = adb -s $DeviceUdid shell pm list packages 2>$null
                if ($pmOutput -match "package:") {
                    Write-Info "Package manager service is ready"
                    break
                }
                Start-Sleep -Seconds 3
                $pmWaited += 3
                if ($pmWaited % 15 -eq 0) {
                    Write-Info "Waiting for package manager... ($pmWaited seconds elapsed)"
                }
            }
            
            if ($pmWaited -ge $pmTimeout) {
                Write-Error "Package manager service did not start within $pmTimeout seconds."
                Write-Info "Checking services:"
                adb -s $DeviceUdid shell service list 2>$null | Select-Object -First 20 | ForEach-Object { Write-Info "  $_" }
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
    
    # Get device UDID if not provided - check env var first
    if (-not $DeviceUdid -and $env:DEVICE_UDID) {
        Write-Info "Using DEVICE_UDID from environment: $($env:DEVICE_UDID)"
        $DeviceUdid = $env:DEVICE_UDID
    }

    if (-not $DeviceUdid) {
        Write-Info "Auto-detecting iOS simulator..."
        $simList = xcrun simctl list devices available --json | ConvertFrom-Json
        
        # Preferred iOS versions in order — match main CI ui-tests pipeline (defaultiOSVersion: '26.0')
        # iOS 26 snapshots live in src/Controls/tests/TestCases.iOS.Tests/snapshots/ios-26
        # and UITest.cs selects ios-26 environment when platformVersion starts with "26."
        #
        # iOS-26-4 is pinned FIRST (ahead of the generic iOS-26): the deep stage's
        # "Install iOS simulator runtimes" step installs the runtime matching the
        # build SDK (26.5) so actool can compile — but that ALSO makes the generic
        # "iOS-26" tier's descending sort prefer 26.5. The ios-26 visual baselines
        # were captured on iOS 26.4 (PR #35061), so rendering on 26.5 would produce
        # spurious pixel diffs. Selecting 26.4 explicitly keeps the RUN on the
        # baseline OS while the build still uses the 26.5 SDK. Falls back to the
        # newest iOS-26 (then 18/17) if 26.4 is ever absent.
        $preferredVersions = @("iOS-26-4", "iOS-26", "iOS-18", "iOS-17")
        # Preferred devices per iOS version. Every iOS UI-test snapshot baseline
        # (both snapshots/ios and snapshots/ios-26) was captured at 1124x2286 —
        # a 375pt-wide device (iPhone Xs / iPhone 11 Pro, 1125x2436). The baselines
        # are NOT device-agnostic: a 393pt (iPhone 15/16) or 402pt (iPhone 16 Pro)
        # simulator renders 1179/1206-wide screenshots and EVERY visual test then
        # fails with a size mismatch. So only 375pt devices are eligible here; when
        # none is pre-installed the create-fallback below makes an iPhone 11 Pro /
        # iPhone Xs. (Do NOT add larger devices — that reintroduces the run-wide
        # "actual 1206x2472 vs baseline 1124x2286" failure the deep UI-test stage hit.)
        $preferredDevicesPerVersion = @{
            "iOS-26-4" = @("iPhone 11 Pro", "iPhone Xs")
            "iOS-26" = @("iPhone 11 Pro", "iPhone Xs")
            "iOS-18" = @("iPhone Xs", "iPhone 11 Pro")
            "iOS-17" = @("iPhone Xs", "iPhone 11 Pro")
        }
        
        $selectedDevice = $null
        $selectedVersion = $null
        
        # Try each preferred version
        foreach ($version in $preferredVersions) {
            if ($selectedDevice) { break }
            
            # Get all runtimes matching this version prefix.
            # Sort descending so the HIGHEST minor version wins (e.g. iOS-26-4
            # over iOS-26-0). AcesShared agents ship iOS 26.4 pre-installed and
            # PR #35061 resaved ios-26 baselines for 26.4 — using an older
            # runtime (26.0) causes pixel-diff failures on every visual test.
            $matchingRuntimes = $simList.devices.PSObject.Properties | 
                Where-Object { $_.Name -match $version } |
                Sort-Object { $_.Name } -Descending
            
            if ($matchingRuntimes) {
                # Try each preferred device for this version
                $devicesForVersion = if ($preferredDevicesPerVersion.ContainsKey($version)) { $preferredDevicesPerVersion[$version] } else { @("iPhone Xs", "iPhone 16 Pro") }
                foreach ($deviceName in $devicesForVersion) {
                    $device = $null
                    $deviceRuntime = $null
                    foreach ($rt in $matchingRuntimes) {
                        $found = $rt.Value | Where-Object { $_.name -eq $deviceName -and $_.isAvailable -eq $true } | Select-Object -First 1
                        if ($found) {
                            $device = $found
                            $deviceRuntime = $rt.Name
                            break
                        }
                    }
                    
                    if ($device) {
                        $selectedDevice = $device
                        $selectedVersion = $deviceRuntime
                        Write-Info "Found preferred device: $deviceName on $selectedVersion"
                        break
                    }
                }
                
                # If no preferred device found, attempt to CREATE the right-size
                # device for visual snapshot tests instead of falling back to a
                # random iPhone (which would have wrong screen dimensions and
                # cause every visual test to fail with "size differs").
                #
                # Resolution mapping (must match snapshots/<env>/ baselines):
                #   iOS-26 baselines: 1124x1126 → iPhone 11 Pro / iPhone Xs (1125x2436 device)
                #   iOS-18 baselines: matches iPhone Xs default
                #   iOS-17 baselines: matches iPhone Xs
                if (-not $selectedDevice) {
                    $createDevice = $null
                    $createDeviceTypeId = $null
                    # Match by version PREFIX, not exact equality: the
                    # $preferredVersions list leads with a minor-qualified entry
                    # ("iOS-26-4") so the highest installed runtime wins. An exact
                    # `-eq "iOS-26"` test never matches "iOS-26-4", which skipped
                    # the create step and fell back to a wrong-size device (e.g.
                    # iPhone 17 Pro -> 1206x2472 screenshots, breaking every visual
                    # snapshot test with "size differs"). Prefix-match so every
                    # iOS-26* runtime maps to iPhone 11 Pro and iOS-18*/iOS-17* to
                    # iPhone Xs.
                    if ($version -match '^iOS-26') {
                        $createDevice = "iPhone 11 Pro"
                        $createDeviceTypeId = "com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro"
                    }
                    elseif ($version -match '^iOS-18' -or $version -match '^iOS-17') {
                        $createDevice = "iPhone Xs"
                        $createDeviceTypeId = "com.apple.CoreSimulator.SimDeviceType.iPhone-Xs"
                    }

                    if ($createDevice) {
                        # Resolve the create-runtime from `simctl list runtimes
                        # available` (actually-INSTALLED runtimes) rather than the
                        # device-list bucket keys used for detection above. The
                        # device list can surface a runtime bucket (observed in CI:
                        # com.apple.CoreSimulator.SimRuntime.iOS-26-5) that is NOT an
                        # installed runtime, so `simctl create <device> <type>
                        # <that-runtime>` fails with "Invalid runtime" and we fall
                        # through to a wrong-size device (e.g. iPhone 17 Pro ->
                        # 1206px screenshots, which breaks every visual snapshot
                        # test with "size differs"). This mirrors the gate stage's
                        # proven boot logic (eng/pipelines/ci-copilot.yml), which
                        # selects its runtime from `list runtimes available`.
                        $createRuntimeIds = @()
                        try {
                            $rtList = xcrun simctl list runtimes available --json | ConvertFrom-Json
                            $createRuntimeIds = @(
                                $rtList.runtimes |
                                    Where-Object { $_.isAvailable -eq $true -and $_.identifier -match $version } |
                                    Sort-Object { $_.version } -Descending |
                                    ForEach-Object { $_.identifier }
                            )
                        } catch {
                            Write-Info "Could not enumerate installed runtimes: $_"
                        }
                        # Fail-safe: if the runtimes query yielded nothing, fall back
                        # to the device-bucket runtimes so behaviour is never worse
                        # than before.
                        if ($createRuntimeIds.Count -eq 0) {
                            $createRuntimeIds = @($matchingRuntimes | ForEach-Object { $_.Name })
                        }

                        # Try to create the right-size device on each installed
                        # runtime (highest first) until one succeeds — an "Invalid
                        # runtime" (or any transient failure) on one candidate then
                        # falls through to the next installed runtime instead of
                        # giving up and booting a wrong-size device.
                        foreach ($createRuntime in $createRuntimeIds) {
                            if ($selectedDevice) { break }
                            Write-Info "No preferred device pre-installed for $version; creating $createDevice on $createRuntime to match snapshot baselines..."
                            $createOutput = & xcrun simctl create $createDevice $createDeviceTypeId $createRuntime 2>&1
                            if ($LASTEXITCODE -eq 0 -and $createOutput -match '^[0-9A-F-]{36}$') {
                                $newUdid = $createOutput.Trim()
                                Write-Info "Created $createDevice : $newUdid on $createRuntime"
                                # Re-query so we have the full device object
                                $simList = xcrun simctl list devices available --json | ConvertFrom-Json
                                foreach ($rtProp in $simList.devices.PSObject.Properties) {
                                    if ($rtProp.Name -eq $createRuntime) {
                                        $found = $rtProp.Value | Where-Object { $_.udid -eq $newUdid } | Select-Object -First 1
                                        if ($found) {
                                            $selectedDevice = $found
                                            $selectedVersion = $rtProp.Name
                                            break
                                        }
                                    }
                                }
                            }
                            else {
                                Write-Info "Failed to create $createDevice on $createRuntime`: $createOutput"
                            }
                        }
                    }
                }

                # Last-resort: prefer a device whose logical size matches the
                # snapshot baselines (375pt-wide @3x = 1125x2436 -> 1124x2286
                # screenshots) so visual tests still get correct-size coverage even
                # when the create step above could not run. Only if no correct-size
                # device exists do we take an arbitrary iPhone (visual tests will
                # then report 'size differs', but non-visual tests can still run).
                if (-not $selectedDevice) {
                    # Correct-size existing device matching the baselines (375pt-wide
                    # @3x = 1125x2436 -> 1124x2286 screenshots). Do NOT fall back to a
                    # wrong-size iPhone in this per-version block — doing so would lock
                    # in e.g. iPhone 17 Pro during the FIRST (highest) runtime
                    # iteration and `break` out before the create step runs for the
                    # remaining preferred versions. The outer last-resort block (after
                    # this foreach) takes an arbitrary iPhone only once every preferred
                    # version's create attempt has been exhausted.
                    $preferredSizeNames = @("iPhone 11 Pro", "iPhone Xs", "iPhone X", "iPhone 13 mini", "iPhone 12 mini")
                    foreach ($rt in $matchingRuntimes) {
                        $found = $rt.Value | Where-Object { $_.isAvailable -eq $true -and $preferredSizeNames -contains $_.name } | Select-Object -First 1
                        if ($found) {
                            $selectedDevice = $found
                            $selectedVersion = $rt.Name
                            Write-Info "Using correct-size iPhone matching baselines: $($found.name) on $selectedVersion"
                            break
                        }
                    }
                }
            }
        }
        
        # Last resort: find ANY available iPhone simulator, still preferring a
        # correct-size device (matching snapshot baselines) over an arbitrary one.
        if (-not $selectedDevice) {
            $preferredSizeNames = @("iPhone 11 Pro", "iPhone Xs", "iPhone X", "iPhone 13 mini", "iPhone 12 mini")
            $allDevices = $simList.devices.PSObject.Properties | ForEach-Object { 
                $runtime = $_.Name
                $_.Value | Where-Object { $_.name -match "iPhone" -and $_.isAvailable -eq $true } | 
                    ForEach-Object { $_ | Add-Member -NotePropertyName "runtime" -NotePropertyValue $runtime -PassThru }
            }
            
            if ($allDevices) {
                $selectedDevice = ($allDevices | Where-Object { $preferredSizeNames -contains $_.name } | Select-Object -First 1)
                if (-not $selectedDevice) {
                    $selectedDevice = $allDevices | Select-Object -First 1
                }
                $selectedVersion = $selectedDevice.runtime
                Write-Info "Fallback: Using $($selectedDevice.name) on $selectedVersion"
            }
        }
        
        # LAST-RESORT recovery — parity with the deep stage's "THIRD RECOVERY" in
        # eng/pipelines/ci-copilot.yml. When every runtime from `simctl list runtimes
        # available` rejected `simctl create` with "Invalid runtime" and no usable device
        # exists, the agent may still have a runtime DISK IMAGE that is "Ready" but not yet
        # enrolled in the legacy simruntime registry. The newer `simctl runtime list` shows
        # it, and `simctl create` accepts its runtimeIdentifier and mounts it on demand — so
        # recover a bootable sim here instead of dead-ending at "No iPhone simulator found"
        # and degrading the gate to INCONCLUSIVE. Without this, the GATE iOS boot fails while
        # the DEEP stage boots fine on the SAME agent (the gate boots via this script; the
        # deep stage had its own recovery). If no runtime image is Ready this yields empty and
        # the existing fatal below still fires — strictly additive, no happy-path change.
        # (PR #35706 build 14680958: all runtimes "Invalid", gate went INCONCLUSIVE.)
        #
        # Enumerate ALL Ready iOS runtime disk images (newest first), not just the newest
        # one, and try `simctl create` on each until one succeeds. Observed in CI (PR #35706
        # build 14689719): the agent has iOS 26.3.1 (23D8133) AND iOS 26.5 (23F77) both
        # "Ready", but `simctl create` on the NEWEST (iOS-26-5) can fail "Invalid runtime"
        # while the OLDER, stable iOS-26-3-1 is create-usable. Try every Ready runtime
        # (highest version first, to keep the iOS-26 snapshot-baseline size when possible).
        # Factored into a function so the ENROLLMENT-RECOVERY retry below can re-run the
        # exact same logic after a CoreSimulatorService restart without duplicating it.
        # Returns [PSCustomObject]@{ Device; Version } on success, or $null.
        function Invoke-IosReadyRuntimeRescue {
            $readyRuntimeIds = @()
            try {
                $rtImages = xcrun simctl runtime list --json 2>$null | ConvertFrom-Json
                $readyRuntimeIds = @(
                    $rtImages.PSObject.Properties.Value |
                        Where-Object { $_.state -eq 'Ready' -and $_.runtimeIdentifier -match 'iOS' } |
                        Sort-Object { $_.version } -Descending |
                        ForEach-Object { $_.runtimeIdentifier }
                )
            } catch {
                Write-Info "Could not enumerate runtime disk images: $_"
            }
            if ($readyRuntimeIds.Count -eq 0) { return $null }
            Write-Info "Recovered $($readyRuntimeIds.Count) Ready (unenrolled) iOS runtime disk image(s): $($readyRuntimeIds -join ', ') - attempting to create a device on each (newest first) until one succeeds..."
            foreach ($readyRuntimeId in $readyRuntimeIds) {
                foreach ($rescueType in @(
                    @{ Name = 'iPhone 11 Pro'; Id = 'com.apple.CoreSimulator.SimDeviceType.iPhone-11-Pro' },
                    @{ Name = 'iPhone Xs';     Id = 'com.apple.CoreSimulator.SimDeviceType.iPhone-Xs' })) {
                    $createOutput = & xcrun simctl create $rescueType.Name $rescueType.Id $readyRuntimeId 2>&1
                    $udidLine = ("$createOutput" -split "`n" |
                        Where-Object { $_ -match '^[0-9A-Fa-f]{8}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{4}-[0-9A-Fa-f]{12}$' } |
                        Select-Object -Last 1)
                    if ($LASTEXITCODE -eq 0 -and $udidLine) {
                        $newUdid = $udidLine.Trim()
                        Write-Info "Created $($rescueType.Name) : $newUdid on $readyRuntimeId"
                        # The device may not surface under `list devices available` until its
                        # runtime is enrolled, but `simctl boot <udid>` mounts it on demand, so
                        # use the UDID directly (re-query only for a nicer display object).
                        $reList = xcrun simctl list devices --json 2>$null | ConvertFrom-Json
                        $found = $reList.devices.PSObject.Properties.Value | ForEach-Object { $_ } |
                            Where-Object { $_.udid -eq $newUdid } | Select-Object -First 1
                        $dev = if ($found) { $found } else { [PSCustomObject]@{ udid = $newUdid; name = $rescueType.Name } }
                        return [PSCustomObject]@{ Device = $dev; Version = $readyRuntimeId }
                    }
                    else {
                        Write-Info "Failed to create $($rescueType.Name) on $readyRuntimeId`: $createOutput"
                    }
                }
            }
            return $null
        }

        # DIAGNOSE why a "Ready" runtime is not create-usable. A freshly downloaded iOS
        # runtime can be "Ready" on disk yet isAvailable=false (not staged/verified/mounted
        # into CoreSimulator), so `simctl create` rejects it "Invalid runtime" and `simctl
        # list runtimes available` is empty (PR #35706 build 14695557: gate INCONCLUSIVE with
        # iOS-26-5/26-3 both "Ready" on disk). The existing CoreSimulatorService restart alone
        # does NOT enroll them. Logging availabilityError + signature/mount state pinpoints the
        # real reason (not mounted vs signature vs incompatible Xcode) instead of guessing.
        function Write-IosRuntimeDiag([string]$Label) {
            Write-Info "==== iOS runtime diagnostics ($Label) ===="
            Write-Info ("  xcode-select -p: " + ((& xcode-select -p 2>&1) -join ' '))
            Write-Info ("  xcrun -f simctl:  " + ((& xcrun -f simctl 2>&1) -join ' '))
            # Human-readable runtime list annotates the exact reason a runtime is unusable
            # e.g. "(unavailable, runtime not mounted)" / "(invalid)" — the single most useful
            # signal, and it prints even when the JSON enumeration is empty.
            $rtText = (& xcrun simctl runtime list 2>&1) -join "`n"
            $rtLines = @(($rtText -split "`n") | Where-Object { $_ -match 'iOS' })
            Write-Info ("  simctl runtime list: {0} iOS line(s)" -f $rtLines.Count)
            foreach ($ln in ($rtLines | Select-Object -First 12)) { Write-Info "    $($ln.Trim())" }
            # CoreSimulator's enrolled runtimes + per-runtime availabilityError.
            try {
                $rts = xcrun simctl list runtimes -j 2>$null | ConvertFrom-Json
                $iosRts = @($rts.runtimes | Where-Object { $_.identifier -match 'iOS' })
                Write-Info ("  simctl list runtimes -j: {0} iOS runtime(s) enrolled" -f $iosRts.Count)
                foreach ($rt in $iosRts) {
                    $err = if ($rt.availabilityError) { $rt.availabilityError } else { 'none' }
                    Write-Info ("    {0} v{1} isAvailable={2} err={3}" -f $rt.identifier, $rt.version, $rt.isAvailable, $err)
                }
            } catch { Write-Info "    (could not parse simctl list runtimes -j: $_)" }
            # Runtime disk images (Xcode 15+ subsystem): state / signature / mount / path.
            # A null .path explains why 'simctl runtime add' below would be a no-op.
            try {
                $imgs = xcrun simctl runtime list --json 2>$null | ConvertFrom-Json
                $iosImgs = @($imgs.PSObject.Properties.Value | Where-Object { $_.runtimeIdentifier -match 'iOS' })
                Write-Info ("  simctl runtime list --json: {0} iOS image(s) on disk" -f $iosImgs.Count)
                foreach ($img in $iosImgs) {
                    Write-Info ("    {0} state={1} sig={2} mounted={3} path={4}" -f $img.runtimeIdentifier, $img.state, $img.signatureState, [bool]$img.mountPath, $img.path)
                }
            } catch { Write-Info "    (could not parse simctl runtime list --json: $_)" }
            Write-Info "==== end diagnostics ($Label) ===="
        }
        # ENROLL Ready-but-unavailable runtimes: `simctl runtime add <path>` stages, verifies,
        # and mounts a runtime disk image — the step CoreSimulator skips when the image is
        # "Ready" on disk but unenrolled. Best-effort and only over runtimes NOT already in the
        # available list, so healthy runtimes are never re-added. Returns $true if it attempted
        # any enrollment (so the caller can re-scan + retry create).
        function Invoke-IosRuntimeEnroll {
            $attempted = $false
            try {
                # MULTI-XCODE HYPOTHESIS: CoreSimulator is Xcode-version-specific. If a NEWER
                # Xcode downloaded/enrolled the runtimes but `simctl` here runs under an OLDER
                # xcode-select path, the runtimes look "Ready" on disk yet "Invalid" to create.
                # Point xcode-select at the newest installed Xcode before enrolling (best-effort).
                $newestXcode = & bash -c 'ls -d /Applications/Xcode_26*.app 2>/dev/null | sort -V | tail -1'
                if ($newestXcode) {
                    $curDev = (& xcode-select -p 2>$null)
                    if ($curDev -notlike "$newestXcode*") {
                        Write-Info "Enroll: switching xcode-select to newest Xcode ($newestXcode) before enrolling..."
                        & sudo -n xcode-select -s "$newestXcode/Contents/Developer" 2>&1 | ForEach-Object { Write-Info "  $_" }
                    }
                }
                $rts = xcrun simctl list runtimes -j 2>$null | ConvertFrom-Json
                $availableIds = @($rts.runtimes | Where-Object { $_.isAvailable -eq $true } | ForEach-Object { $_.identifier })
                $imgs = xcrun simctl runtime list --json 2>$null | ConvertFrom-Json
                $readyImgs = @($imgs.PSObject.Properties.Value | Where-Object { $_.runtimeIdentifier -match 'iOS' -and $_.state -eq 'Ready' })
                Write-Info ("Enroll: {0} Ready iOS image(s) on disk, {1} already available/enrolled" -f $readyImgs.Count, $availableIds.Count)
                foreach ($img in $readyImgs) {
                    if ($availableIds -contains $img.runtimeIdentifier) { continue }
                    if (-not $img.path) {
                        Write-Info "  $($img.runtimeIdentifier): no .path field on this Xcode - cannot 'simctl runtime add'; skipping"
                        continue
                    }
                    Write-Info "Re-staging Ready-but-unavailable runtime $($img.runtimeIdentifier) via 'simctl runtime add' ($($img.path))..."
                    & sudo -n xcrun simctl runtime add "$($img.path)" 2>&1 | ForEach-Object { Write-Info "  $_" }
                    if ($LASTEXITCODE -ne 0) { & xcrun simctl runtime add "$($img.path)" 2>&1 | ForEach-Object { Write-Info "  $_" } }
                    $attempted = $true
                }
            } catch { Write-Info "Runtime enroll attempt error: $_" }
            return $attempted
        }

        if (-not $selectedDevice) {
            # First pass: try to create on any Ready runtime as-is.
            $rescueResult = Invoke-IosReadyRuntimeRescue

            # ENROLLMENT RECOVERY (parity with eng/pipelines/ci-copilot.yml ed34ffa; PR
            # #35706 build 14694271): the GATE boots iOS via THIS script, but the
            # CoreSimulatorService-restart enrollment fix only landed in ci-copilot.yml (the
            # deep stage's boot), so the gate kept dead-ending at INCONCLUSIVE while the deep
            # stage recovered on the SAME agent. When every create above failed "Invalid
            # runtime", the Ready iOS images are on disk but NOT enrolled in CoreSimulator's
            # registry (`simctl list runtimes available` is empty); restarting
            # CoreSimulatorService forces a re-scan that enrolls them. Retry the create loop
            # once afterward. Only runs when the first pass produced no device, so the healthy
            # path is untouched; non-destructive (the daemon auto-relaunches on the next
            # simctl call). sudo -n avoids any password prompt hang; falls back to non-sudo.
            if (-not $rescueResult) {
                Write-Info "All Ready-runtime create attempts failed 'Invalid runtime' - restarting CoreSimulatorService to enroll Ready-but-unenrolled runtimes and retrying once..."
                Write-IosRuntimeDiag "before restart/enroll"
                & sudo -n killall -9 com.apple.CoreSimulator.CoreSimulatorService 2>$null
                if ($LASTEXITCODE -ne 0) { & killall -9 com.apple.CoreSimulator.CoreSimulatorService 2>$null }
                Start-Sleep -Seconds 8
                # Nudge CoreSimulator to relaunch and re-scan the on-disk runtime images.
                xcrun simctl list runtimes *> $null
                Start-Sleep -Seconds 4
                # A restart alone often does NOT make Ready images create-usable (PR #35706
                # build 14695557: still "Invalid runtime" after restart). Explicitly re-stage /
                # verify / mount each unavailable Ready runtime via `simctl runtime add`, then
                # re-scan before the final create retry.
                if (Invoke-IosRuntimeEnroll) {
                    xcrun simctl list runtimes *> $null
                    Start-Sleep -Seconds 4
                }
                Write-IosRuntimeDiag "after restart/enroll"
                $rescueResult = Invoke-IosReadyRuntimeRescue
            }

            if ($rescueResult) {
                $selectedDevice = $rescueResult.Device
                $selectedVersion = $rescueResult.Version
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
    
    # Shutdown any OTHER booted simulators to avoid Appium connecting to the wrong device
    $bootedSims = xcrun simctl list devices --json | ConvertFrom-Json
    $otherBooted = $bootedSims.devices.PSObject.Properties.Value |
        ForEach-Object { $_ } |
        Where-Object { $_.state -eq "Booted" -and $_.udid -ne $DeviceUdid }
    
    if ($otherBooted) {
        foreach ($sim in $otherBooted) {
            Write-Info "Shutting down other booted simulator: $($sim.name) ($($sim.udid))"
            xcrun simctl shutdown $sim.udid 2>$null
        }
    }

    # Boot simulator if not already booted.
    #
    # Robustness: `simctl boot` transitions the device Booting -> Booted
    # asynchronously. The previous code queried state ONCE immediately after
    # `simctl boot` and did `exit 1` if it was not yet "Booted", which could
    # spuriously fail the deep iOS UI-test stage on a slow/loaded CI agent (an
    # infrastructure failure with no retry). Boot inside a bounded retry loop
    # that waits for the device to actually reach the Booted state; on the happy
    # path (already Booted) this returns on the first iteration with no
    # behavioural change.
    Write-Info "Booting simulator (if not already running)..."
    $bootDeadlineSeconds = 90
    $bootWaited = 0
    $device = $null
    xcrun simctl boot $DeviceUdid 2>$null
    while ($bootWaited -lt $bootDeadlineSeconds) {
        $simState = xcrun simctl list devices --json | ConvertFrom-Json
        $device = $simState.devices.PSObject.Properties.Value |
            ForEach-Object { $_ } |
            Where-Object { $_.udid -eq $DeviceUdid } |
            Select-Object -First 1
        if ($device -and $device.state -eq "Booted") { break }
        Start-Sleep -Seconds 3
        $bootWaited += 3
        # Re-issue boot periodically in case the device slipped back to Shutdown
        # (a transient CoreSimulator hiccup) rather than progressing to Booted.
        if ($bootWaited % 15 -eq 0) {
            Write-Info "Simulator still not Booted after ${bootWaited}s (state: $($device.state)); re-issuing boot..."
            xcrun simctl boot $DeviceUdid 2>$null
        }
    }

    if (-not $device -or $device.state -ne "Booted") {
        Write-Error "Simulator failed to boot within ${bootDeadlineSeconds}s. Current state: $($device.state)"
        exit 1
    }

    # The device reaches the Booted state a few seconds before SpringBoard /
    # CoreSimulator services are fully up; give them a brief settle so Appium /
    # WebDriverAgent can attach on the first try instead of erroring out.
    Start-Sleep -Seconds 5

    Write-Success "Simulator is booted and ready: $deviceName"
    
    #endregion
}

# Export device UDID as environment variable
$env:DEVICE_UDID = $DeviceUdid
Write-Success "DEVICE_UDID environment variable set: $DeviceUdid"

# Ensure clean exit code (adb commands above may leave $LASTEXITCODE non-zero)
$global:LASTEXITCODE = 0

# Return UDID for callers
return $DeviceUdid
