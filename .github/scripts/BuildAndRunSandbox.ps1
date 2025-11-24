#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automated issue reproduction testing for .NET MAUI using Appium.

.DESCRIPTION
    This script automates the complete workflow for testing MAUI issues:
    1. Builds the Sandbox app for the target platform (Android or iOS)
    2. Starts Appium server if not already running
    3. Deploys and launches the app using Appium
    4. Runs the Appium test script to validate the issue

.PARAMETER Platform
    Target platform: "android" or "ios"

.PARAMETER Configuration
    Build configuration: "Debug" or "Release" (default: Debug)

.PARAMETER DeviceUdid
    Specific device UDID to target (optional - will auto-detect if not provided)

.EXAMPLE
    ./BuildAndRunSandbox.ps1 -Platform android
    
.EXAMPLE
    ./BuildAndRunSandbox.ps1 -Platform android -DeviceUdid emulator-5554
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "ios")]
    [string]$Platform,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [string]$DeviceUdid
)

# Script configuration
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path "$PSScriptRoot/../.."
$SandboxProject = Join-Path $RepoRoot "src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj"
$SandboxAppiumDir = Join-Path $RepoRoot "SandboxAppium"
$AppiumTestScript = Join-Path $SandboxAppiumDir "RunWithAppiumTest.cs"
$AppiumPort = 4723

# Color output helpers
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Step { param($Message) Write-Host "`n🔹 $Message" -ForegroundColor Blue }

# Banner
Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║     .NET MAUI Sandbox Build and Test Script              ║
║     Platform: $($Platform.ToUpper())                                      ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

#region Validation

Write-Step "Validating prerequisites..."

# Check if SandboxAppium directory exists
if (-not (Test-Path $SandboxAppiumDir)) {
    Write-Error "SandboxAppium directory not found at: $SandboxAppiumDir"
    Write-Info "The agent must create this directory with an Appium test script first."
    exit 1
}

# Clean up old log files and screenshots from previous runs
Write-Step "Cleaning up old test artifacts..."

# Clean up ALL device logs (android-device.log, ios-device.log, etc.)
$deviceLogs = Get-ChildItem -Path $SandboxAppiumDir -Filter "*-device.log" -ErrorAction SilentlyContinue
if ($deviceLogs) {
    foreach ($log in $deviceLogs) {
        Remove-Item $log.FullName -Force
        Write-Info "Cleaned up old $($log.Name)"
    }
} else {
    Write-Info "No old device logs to clean up"
}

$appiumLogFile = Join-Path $SandboxAppiumDir "appium.log"
if (Test-Path $appiumLogFile) {
    Remove-Item $appiumLogFile -Force
    Write-Info "Cleaned up old appium.log"
}

# Clean up all screenshots (*.png files) in SandboxAppium directory
$screenshots = Get-ChildItem -Path $SandboxAppiumDir -Filter "*.png" -ErrorAction SilentlyContinue
if ($screenshots) {
    foreach ($screenshot in $screenshots) {
        Remove-Item $screenshot.FullName -Force
        Write-Info "Removed old screenshot: $($screenshot.Name)"
    }
} else {
    Write-Info "No old screenshots to clean up"
}

# Check if RunWithAppiumTest.cs exists
if (-not (Test-Path $AppiumTestScript)) {
    Write-Error "Appium test script not found: RunWithAppiumTest.cs"
    Write-Info @"

Required file missing: $AppiumTestScript

The agent must create an Appium test script named 'RunWithAppiumTest.cs' 
in the SandboxAppium directory before running this test script.

This file should:
1. Use the #:package directive for Appium.WebDriver
2. Connect to Appium at http://localhost:4723
3. Launch and interact with the Sandbox app
4. Validate the issue behavior

Example structure:
    #:package Appium.WebDriver@8.0.1
    
    using OpenQA.Selenium.Appium;
    using OpenQA.Selenium.Appium.Android;  // or iOS
    
    var udid = Environment.GetEnvironmentVariable("DEVICE_UDID");
    // ... Appium test logic ...
"@
    exit 1
}

Write-Success "Appium test script found: RunWithAppiumTest.cs"

# Check if dotnet is available
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK not found. Please install .NET SDK and ensure 'dotnet' is in PATH."
    exit 1
}

Write-Success "Prerequisites validated"

#endregion

#region Platform-Specific Configuration

Write-Step "Configuring platform-specific settings..."

if ($Platform -eq "android") {
    $TargetFramework = "net10.0-android"
    $AppPackage = "com.microsoft.maui.sandbox"
    $AppActivity = "com.microsoft.maui.sandbox.MainActivity"
    
    # Check adb
    if (-not (Get-Command "adb" -ErrorAction SilentlyContinue)) {
        Write-Error "Android SDK (adb) not found. Please install Android SDK and ensure 'adb' is in PATH."
        exit 1
    }
    
    # Get device UDID if not provided
    if (-not $DeviceUdid) {
        Write-Info "Auto-detecting Android device..."
        $devices = adb devices | Select-String "device$"
        if ($devices.Count -eq 0) {
            Write-Error "No Android devices found. Please start an emulator or connect a device."
            exit 1
        }
        $DeviceUdid = ($devices[0] -split '\s+')[0]
    }
    
    Write-Success "Android device: $DeviceUdid"
    
} elseif ($Platform -eq "ios") {
    $TargetFramework = "net10.0-ios"
    $AppBundleId = "com.microsoft.maui.sandbox"
    
    # Check xcrun (iOS tools)
    if (-not (Get-Command "xcrun" -ErrorAction SilentlyContinue)) {
        Write-Error "Xcode command line tools not found. This script requires macOS with Xcode installed."
        exit 1
    }
    
    # Get device UDID if not provided
    if (-not $DeviceUdid) {
        Write-Info "Auto-detecting iOS simulator..."
        $simList = xcrun simctl list devices available --json | ConvertFrom-Json
        
        # Find iPhone Xs with highest iOS version
        $iPhoneXs = $simList.devices.PSObject.Properties | 
            Where-Object { $_.Name -match "iOS" } |
            ForEach-Object { 
                $_.Value | Where-Object { $_.name -eq "iPhone Xs" }
            } | 
            Select-Object -First 1
        
        if (-not $iPhoneXs) {
            Write-Error "No iPhone Xs simulator found. Please create one in Xcode."
            exit 1
        }
        
        $DeviceUdid = $iPhoneXs.udid
    }
    
    Write-Success "iOS simulator: $DeviceUdid"
}

# Set DEVICE_UDID environment variable for Appium
$env:DEVICE_UDID = $DeviceUdid
Write-Info "DEVICE_UDID environment variable set: $DeviceUdid"

#endregion

#region Build

Write-Step "Building Sandbox app for $Platform..."

$buildArgs = @(
    "build"
    $SandboxProject
    "-f", $TargetFramework
    "-c", $Configuration
)

# Add Run target for Android to install the app
if ($Platform -eq "android") {
    $buildArgs += "-t:Run"
}

Write-Info "Build command: dotnet $($buildArgs -join ' ')"

$buildStart = Get-Date
& dotnet @buildArgs

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed with exit code $LASTEXITCODE"
    exit 1
}

$buildDuration = (Get-Date) - $buildStart
Write-Success "Build completed in $($buildDuration.TotalSeconds) seconds"

#endregion

#region iOS Deployment

if ($Platform -eq "ios") {
    Write-Step "Deploying to iOS simulator..."
    
    # Boot simulator if not already booted
    Write-Info "Booting simulator (if not already running)..."
    xcrun simctl boot $DeviceUdid 2>$null
    
    # Wait for boot
    Start-Sleep -Seconds 2
    
    # Verify simulator is booted
    $simState = (xcrun simctl list devices --json | ConvertFrom-Json).devices.PSObject.Properties.Value | 
        Where-Object { $_.udid -eq $DeviceUdid } | 
        Select-Object -First 1
    
    if ($simState.state -ne "Booted") {
        Write-Error "Simulator failed to boot. Current state: $($simState.state)"
        exit 1
    }
    
    Write-Success "Simulator is booted"
    
    # Install app
    $appPath = Join-Path $RepoRoot "artifacts/bin/Maui.Controls.Sample.Sandbox/$Configuration/$TargetFramework/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app"
    
    if (-not (Test-Path $appPath)) {
        Write-Error "App bundle not found at: $appPath"
        exit 1
    }
    
    Write-Info "Installing app: $appPath"
    xcrun simctl install $DeviceUdid $appPath
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "App installation failed"
        exit 1
    }
    
    Write-Success "App installed successfully"
}

#endregion

#region Appium Server

Write-Step "Checking Appium server..."

# Check if Appium is installed
if (-not (Get-Command "appium" -ErrorAction SilentlyContinue)) {
    Write-Error "Appium not found. Please install Appium: npm install -g appium"
    exit 1
}

# Check if Appium is already running
$appiumWasRunning = $false
$appiumJob = $null

try {
    $response = Invoke-WebRequest -Uri "http://localhost:$AppiumPort/status" -TimeoutSec 2 -ErrorAction Stop
    Write-Success "Appium is already running on port $AppiumPort"
    $appiumWasRunning = $true
} catch {
    Write-Info "Appium not running, starting server on port $AppiumPort..."
    
    # Start Appium in background with logging (appiumLogFile already defined above)
    $appiumJob = Start-Job -ScriptBlock {
        param($logFile)
        appium --log-level info > $logFile 2>&1
    } -ArgumentList $appiumLogFile
    
    Write-Info "Appium logs → $appiumLogFile"
    
    # Wait for Appium to be ready
    $maxWait = 30
    $waited = 0
    $ready = $false
    
    while ($waited -lt $maxWait -and -not $ready) {
        Start-Sleep -Seconds 1
        $waited++
        
        try {
            $response = Invoke-WebRequest -Uri "http://localhost:$AppiumPort/status" -TimeoutSec 1 -ErrorAction Stop
            $ready = $true
        } catch {
            # Continue waiting
        }
    }
    
    if (-not $ready) {
        Stop-Job $appiumJob
        Remove-Job $appiumJob
        Write-Error "Appium failed to start within $maxWait seconds"
        exit 1
    }
    
    Write-Success "Appium server started (Job ID: $($appiumJob.Id))"
}

#endregion

#region Run Appium Test

Write-Step "Running Appium test..."

# Clear logs before test
if ($Platform -eq "android") {
    Write-Info "Clearing Android logcat buffer before test..."
    & adb -s $DeviceUdid logcat -c
} elseif ($Platform -eq "ios") {
    Write-Info "iOS logs will be captured from Appium during test execution..."
}

Push-Location $SandboxAppiumDir

try {
    Write-Info "Executing: dotnet run RunWithAppiumTest.cs"
    Write-Info "Test will connect to device: $DeviceUdid"
    Write-Host ""
    
    # Run the Appium test and capture output to extract PID
    # Suppress CA1307 (culture) and CS0162 (unreachable code due to const platform)
    $appiumOutput = "" | & dotnet run RunWithAppiumTest.cs /p:NoWarn="CA1307;CS0162" 2>&1
    
    # Display appium test output
    $appiumOutput | ForEach-Object { Write-Host $_ }
    
    $testExitCode = $LASTEXITCODE
    
    # Extract PID from Appium test output (Android)
    $sandboxPid = $null
    $pidLine = $appiumOutput | Select-String -Pattern "SANDBOX_APP_PID=(\d+)"
    if ($pidLine -and $pidLine.Matches.Groups.Count -gt 1) {
        $sandboxPid = $pidLine.Matches.Groups[1].Value
        Write-Host ""
        Write-Info "Captured Sandbox app PID from Appium test: $sandboxPid"
        
        # Dump logcat buffer for this PID to file (Android)
        if ($Platform -eq "android" -and $sandboxPid) {
            Write-Info "Dumping logcat buffer for PID $sandboxPid..."
            & adb -s $DeviceUdid logcat -d --pid=$sandboxPid > $deviceLogFile
            Write-Info "Logcat dumped to: $deviceLogFile"
        }
    }
    elseif ($Platform -eq "android") {
        # Fallback: If we couldn't get PID, dump entire logcat buffer (unfiltered)
        # This ensures we always have logs for the agent to analyze
        Write-Host ""
        Write-Warning "Could not capture app PID from Appium test output"
        Write-Info "Dumping entire logcat buffer (unfiltered)..."
        & adb -s $DeviceUdid logcat -d > $deviceLogFile
        Write-Info "Logcat dumped to: $deviceLogFile (UNFILTERED - contains all apps)"
    }
    
    # Capture iOS logs after test completes
    if ($Platform -eq "ios") {
        Write-Host ""
        Write-Info "Capturing iOS simulator logs for Sandbox app..."
        
        # Use log show to capture recent logs from Sandbox app
        $logStartTime = (Get-Date).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss")
        
        $iosLogCommand = "xcrun simctl spawn booted log show --predicate 'processImagePath contains `"Maui.Controls.Sample.Sandbox`"' --start `"$logStartTime`" --style compact"
        
        Write-Info "Capturing logs from last 2 minutes..."
        Invoke-Expression "$iosLogCommand > `"$deviceLogFile`" 2>&1"
        
        Write-Info "iOS logs saved to: $deviceLogFile"
    }
    
    # Show device logs
    if (Test-Path $deviceLogFile) {
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
        if ($Platform -eq "android") {
            Write-Host "  Android Logcat Output (Filtered to Sandbox App)" -ForegroundColor Cyan
        } else {
            Write-Host "  iOS Simulator Logs (Filtered to Sandbox App)" -ForegroundColor Cyan
        }
        Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
        
        # Show ALL logs from Sandbox app (no content filtering)
        $logContent = Get-Content $deviceLogFile -ErrorAction SilentlyContinue
        if ($logContent) {
            # Display last 100 lines (no filtering - already filtered to Sandbox app only)
            $recentLogs = $logContent | Select-Object -Last 100
            
            if ($recentLogs) {
                $recentLogs | ForEach-Object { Write-Host $_ }
            } else {
                Write-Host "No logs captured from Sandbox app" -ForegroundColor Yellow
            }
            
            Write-Host ""
            Write-Info "Full device log saved to: $deviceLogFile"
            if ($Platform -eq "android") {
                Write-Info "All logs are from Sandbox app only (com.microsoft.maui.sandbox)"
            } else {
                Write-Info "All logs are from Sandbox app only (Maui.Controls.Sample.Sandbox)"
            }
        } else {
            Write-Warning "Could not read device log file"
        }
        
        Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host ""
    }
    
    # Display Appium logs if we started the server
    if ($appiumJob -and -not $appiumWasRunning -and (Test-Path $appiumLogFile)) {
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host "  Appium Server Output (All Logs)" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
        
        $appiumContent = Get-Content $appiumLogFile -ErrorAction SilentlyContinue
        if ($appiumContent) {
            # Display last 100 lines (no filtering)
            $recentAppium = $appiumContent | Select-Object -Last 100
            
            if ($recentAppium) {
                $recentAppium | ForEach-Object { Write-Host $_ }
            } else {
                Write-Host "No Appium logs captured" -ForegroundColor Yellow
            }
            
            Write-Host ""
            Write-Info "Full Appium log saved to: $appiumLogFile"
        }
        
        Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host ""
    }
    
    if ($testExitCode -eq 0) {
        Write-Success "Test completed successfully"
    } else {
        Write-Error "Test failed with exit code $testExitCode"
        Pop-Location
        exit $testExitCode
    }
    
} catch {
    # Stop logcat if still running
    if ($logcatJob) {
        Stop-Job $logcatJob
        Remove-Job $logcatJob
    }
    
    # Stop Appium if we started it
    if ($appiumJob -and -not $appiumWasRunning) {
        Write-Info "Stopping Appium server (we started it)..."
        Stop-Job $appiumJob
        Remove-Job $appiumJob
    }
    
    Write-Error "Failed to run Appium test: $_"
    Pop-Location
    exit 1
}

Pop-Location

#endregion

#region Cleanup

# Stop Appium if we started it
if ($appiumJob -and -not $appiumWasRunning) {
    Write-Host ""
    Write-Info "Stopping Appium server (started by this script)..."
    Stop-Job $appiumJob
    Remove-Job $appiumJob
    Write-Success "Appium server stopped"
}

#endregion

#region Summary

Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║                    Test Summary                           ║
╠═══════════════════════════════════════════════════════════╣
║  Platform:     $($Platform.ToUpper().PadRight(10))                             ║
║  Device:       $($DeviceUdid.Substring(0, [Math]::Min(40, $DeviceUdid.Length)).PadRight(40))      ║
║  Result:       SUCCESS ✅                                 ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green

#endregion
