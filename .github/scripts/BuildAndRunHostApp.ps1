#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automated UI testing for .NET MAUI TestCases.HostApp using dotnet test.

.DESCRIPTION
    This script automates the complete workflow for running MAUI UI tests:
    1. Builds the TestCases.HostApp for the target platform (Android or iOS)
    2. Deploys the app to the target device/simulator
    3. Runs dotnet test with the specified filter against the appropriate test project
    4. Captures all device logs and test output

.PARAMETER Platform
    Target platform: "android" or "ios"

.PARAMETER TestFilter
    Test filter to pass to dotnet test (e.g., "FullyQualifiedName~Issue12345")
    This is passed directly to the --filter parameter of dotnet test

.PARAMETER Category
    Test category to filter by (e.g., "SafeAreaEdges", "Button", "Layout")
    This is converted to --filter "Category=<value>"
    Cannot be used with -TestFilter

.PARAMETER Configuration
    Build configuration: "Debug" or "Release" (default: Debug)

.PARAMETER DeviceUdid
    Specific device UDID to target (optional - will auto-detect if not provided)

.EXAMPLE
    ./BuildAndRunHostApp.ps1 -Platform android -TestFilter "FullyQualifiedName~Issue12345"
    
.EXAMPLE
    ./BuildAndRunHostApp.ps1 -Platform ios -TestFilter "Issue12345" -DeviceUdid "12345678-1234567890ABCDEF"
    
.EXAMPLE
    ./BuildAndRunHostApp.ps1 -Platform android -Category "SafeAreaEdges"
    
.EXAMPLE
    ./BuildAndRunHostApp.ps1 -Platform ios -Category "Button"
#>

[CmdletBinding(DefaultParameterSetName = "TestFilter")]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "ios")]
    [string]$Platform,

    [Parameter(Mandatory = $true, ParameterSetName = "TestFilter")]
    [string]$TestFilter,

    [Parameter(Mandatory = $true, ParameterSetName = "Category")]
    [string]$Category,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [string]$DeviceUdid
)

# Script configuration
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path "$PSScriptRoot/../.."
$HostAppProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj"
$HostAppLogsDir = Join-Path $RepoRoot "HostAppCustomAgentTmpLogs"

# Color output helpers
function Write-Success { param($Message) Write-Host "✅ $Message" -ForegroundColor Green }
function Write-Error { param($Message) Write-Host "❌ $Message" -ForegroundColor Red }
function Write-Info { param($Message) Write-Host "ℹ️  $Message" -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host "⚠️  $Message" -ForegroundColor Yellow }
function Write-Step { param($Message) Write-Host "`n🔹 $Message" -ForegroundColor Blue }

# Banner
Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║     .NET MAUI HostApp Build and Test Script              ║
║     Platform: $($Platform.ToUpper())                                      ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

#region Validation

Write-Step "Validating prerequisites..."

# Create HostAppCustomAgentTmpLogs directory if it doesn't exist
if (-not (Test-Path $HostAppLogsDir)) {
    New-Item -Path $HostAppLogsDir -ItemType Directory -Force | Out-Null
    Write-Info "Created HostAppCustomAgentTmpLogs directory"
}

# Clean up old log files from previous runs
$deviceLogFile = Join-Path $HostAppLogsDir "$Platform-device.log"
$testOutputFile = Join-Path $HostAppLogsDir "test-output.log"

if (Test-Path $deviceLogFile) {
    Remove-Item $deviceLogFile -Force
    Write-Info "Cleaned up old $Platform-device.log"
}

if (Test-Path $testOutputFile) {
    Remove-Item $testOutputFile -Force
    Write-Info "Cleaned up old test-output.log"
}

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
    $AppPackage = "com.microsoft.maui.uitests"
    $AppActivity = "com.microsoft.maui.uitests.MainActivity"
    
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
    $AppBundleId = "com.microsoft.maui.uitests"
    
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

Write-Step "Building TestCases.HostApp for $Platform..."

$buildArgs = @(
    "build"
    $HostAppProject
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
    $appPath = Join-Path $RepoRoot "artifacts/bin/Controls.TestCases.HostApp/$Configuration/$TargetFramework/iossimulator-arm64/Controls.TestCases.HostApp.app"
    
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

#region Determine Test Project

Write-Step "Determining test project..."

if ($Platform -eq "android") {
    $TestProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj"
} elseif ($Platform -eq "ios") {
    $TestProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj"
}

if (-not (Test-Path $TestProject)) {
    Write-Error "Test project not found: $TestProject"
    exit 1
}

Write-Success "Test project: $TestProject"

#endregion

#region Run Tests

# Determine the filter to use
if ($Category) {
    $effectiveFilter = "Category=$Category"
    Write-Step "Running UI tests with category: $Category"
} else {
    $effectiveFilter = $TestFilter
    Write-Step "Running UI tests with filter: $TestFilter"
}

# Clear device logs before test
if ($Platform -eq "android") {
    Write-Info "Clearing Android logcat buffer before test..."
    & adb -s $DeviceUdid logcat -c
}

# Capture test start time for iOS logs
$testStartTime = Get-Date

Write-Info "Executing: dotnet test --filter `"$effectiveFilter`""
Write-Host ""

try {
    # Run dotnet test and capture output
    $testOutput = & dotnet test $TestProject --filter $effectiveFilter --logger "console;verbosity=detailed" 2>&1
    
    # Save test output to file
    $testOutput | Out-File -FilePath $testOutputFile -Encoding UTF8
    
    # Display test output
    $testOutput | ForEach-Object { Write-Host $_ }
    
    $testExitCode = $LASTEXITCODE
    
    Write-Host ""
    Write-Info "Test output saved to: $testOutputFile"
    
} catch {
    Write-Error "Failed to run tests: $_"
    exit 1
}

#endregion

#region Capture Device Logs

Write-Step "Capturing device logs..."

if ($Platform -eq "android") {
    Write-Info "Dumping Android logcat buffer (filtered to HostApp)..."
    
    # Try to filter by package name (HostApp)
    & adb -s $DeviceUdid logcat -d | Select-String "com.microsoft.maui.uitests" > $deviceLogFile
    
    if ((Get-Item $deviceLogFile).Length -eq 0) {
        Write-Warning "No logs found for com.microsoft.maui.uitests, dumping entire logcat..."
        & adb -s $DeviceUdid logcat -d > $deviceLogFile
    }
    
    Write-Info "Android logcat saved to: $deviceLogFile"
    
} elseif ($Platform -eq "ios") {
    Write-Info "Capturing iOS simulator logs..."
    
    # Capture logs from when test started
    $logStartTimeStr = $testStartTime.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")
    
    $iosLogCommand = "xcrun simctl spawn booted log show --predicate 'processImagePath contains `"Controls.TestCases.HostApp`"' --start `"$logStartTimeStr`" --style compact"
    
    Invoke-Expression "$iosLogCommand > `"$deviceLogFile`" 2>&1"
    
    Write-Info "iOS logs saved to: $deviceLogFile"
}

#endregion

#region Display Logs

if (Test-Path $deviceLogFile) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    if ($Platform -eq "android") {
        Write-Host "  Android Device Logs (Last 100 lines)" -ForegroundColor Cyan
    } else {
        Write-Host "  iOS Simulator Logs (Last 100 lines)" -ForegroundColor Cyan
    }
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    $logContent = Get-Content $deviceLogFile -ErrorAction SilentlyContinue
    if ($logContent) {
        $recentLogs = $logContent | Select-Object -Last 100
        
        if ($recentLogs) {
            $recentLogs | ForEach-Object { Write-Host $_ }
        } else {
            Write-Host "No device logs captured" -ForegroundColor Yellow
        }
        
        Write-Host ""
        Write-Info "Full device log: $deviceLogFile"
    } else {
        Write-Warning "Could not read device log file"
    }
    
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
}

#endregion

#region Test Result

if ($testExitCode -eq 0) {
    Write-Success "All tests passed"
} else {
    Write-Error "Tests failed with exit code $testExitCode"
    Write-Info "Review logs at: $HostAppLogsDir"
    exit $testExitCode
}

#endregion

#region Summary

Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║                    Test Summary                           ║
╠═══════════════════════════════════════════════════════════╣
║  Platform:     $($Platform.ToUpper().PadRight(10))                             ║
║  Device:       $($DeviceUdid.Substring(0, [Math]::Min(40, $DeviceUdid.Length)).PadRight(40))      ║
║  Test Filter:  $($effectiveFilter.Substring(0, [Math]::Min(40, $effectiveFilter.Length)).PadRight(40))      ║
║  Result:       SUCCESS ✅                                 ║
║  Logs:         $HostAppLogsDir
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green

#endregion
