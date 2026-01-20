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
    Target platform: "android", "ios", or "catalyst" (MacCatalyst)

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
    
.EXAMPLE
    ./BuildAndRunHostApp.ps1 -Platform catalyst -TestFilter "Issue12345"
#>

[CmdletBinding(DefaultParameterSetName = "TestFilter")]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "ios", "catalyst", "maccatalyst")]
    [string]$Platform,

    [Parameter(Mandatory = $true, ParameterSetName = "TestFilter")]
    [string]$TestFilter,

    [Parameter(Mandatory = $true, ParameterSetName = "Category")]
    [string]$Category,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [string]$DeviceUdid,

    [switch]$Rebuild
)

# Script configuration
$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path "$PSScriptRoot/../.."
$HostAppProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj"
$HostAppLogsDir = Join-Path $RepoRoot "CustomAgentLogsTmp/UITests"

# Normalize platform name (accept both "catalyst" and "maccatalyst")
if ($Platform -eq "maccatalyst") {
    $Platform = "catalyst"
}

# Import shared utilities
. "$PSScriptRoot/shared/shared-utils.ps1"

# Banner
Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║     .NET MAUI HostApp Build and Test Script              ║
║     Platform: $($Platform.ToUpper())                                      ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

#region Validation

Write-Step "Validating prerequisites..."

# Create CustomAgentLogsTmp/UITests directory if it doesn't exist
if (-not (Test-Path $HostAppLogsDir)) {
    New-Item -Path $HostAppLogsDir -ItemType Directory -Force | Out-Null
    Write-Info "Created CustomAgentLogsTmp/UITests directory"
}

# Clean up ALL old log files from previous runs to avoid confusion
$deviceLogFile = Join-Path $HostAppLogsDir "$Platform-device.log"
$testOutputFile = Join-Path $HostAppLogsDir "test-output.log"

# Remove all files in the logs directory
$existingFiles = Get-ChildItem -Path $HostAppLogsDir -File -ErrorAction SilentlyContinue
if ($existingFiles) {
    $existingFiles | Remove-Item -Force
    Write-Info "Cleaned up $($existingFiles.Count) old log file(s) from previous runs"
}

# Check if dotnet is available
if (-not (Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
    Write-Error ".NET SDK not found. Please install .NET SDK and ensure 'dotnet' is in PATH."
    exit 1
}

Write-Success "Prerequisites validated"

#endregion

#region Platform-Specific Configuration

# Set target framework and app identifiers
if ($Platform -eq "android") {
    $TargetFramework = "net10.0-android"
    $AppPackage = "com.microsoft.maui.uitests"
    $AppActivity = "com.microsoft.maui.uitests.MainActivity"
} elseif ($Platform -eq "ios") {
    $TargetFramework = "net10.0-ios"
    $AppBundleId = "com.microsoft.maui.uitests"
} elseif ($Platform -eq "catalyst") {
    $TargetFramework = "net10.0-maccatalyst"
    $AppBundleId = "com.microsoft.maui.uitests"
}

# Start emulator/simulator (skip for catalyst - runs on desktop)
if ($Platform -ne "catalyst") {
    # Use shared Start-Emulator script to detect and start device
    $startEmulatorParams = @{
        Platform = $Platform
    }

    if ($DeviceUdid) {
        $startEmulatorParams.DeviceUdid = $DeviceUdid
    }

    $DeviceUdid = & "$PSScriptRoot/shared/Start-Emulator.ps1" @startEmulatorParams

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to start or detect device"
        exit 1
    }
} else {
    # MacCatalyst runs directly on the Mac - use "host" as placeholder
    $DeviceUdid = "host"
    Write-Success "MacCatalyst will run on host Mac (no device needed)"
}

#endregion

#region Build and Deploy

# Use shared Build-AndDeploy script
$buildDeployParams = @{
    Platform = $Platform
    ProjectPath = $HostAppProject
    TargetFramework = $TargetFramework
    Configuration = $Configuration
    DeviceUdid = $DeviceUdid
    Rebuild = $Rebuild
}

if ($Platform -eq "ios") {
    $buildDeployParams.BundleId = $AppBundleId
}

& "$PSScriptRoot/shared/Build-AndDeploy.ps1" @buildDeployParams

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build or deployment failed"
    exit 1
}

#endregion

#region Determine Test Project

Write-Step "Determining test project..."

if ($Platform -eq "android") {
    $TestProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj"
} elseif ($Platform -eq "ios") {
    $TestProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj"
} elseif ($Platform -eq "catalyst") {
    $TestProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj"
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

# For MacCatalyst, launch the app BEFORE running tests so Appium finds the correct bundle
# This is critical because both maui and maui2 repos may share the same bundle ID
# MacCatalyst: Just ensure the app is ready - Appium will launch it with the test name
# The app has built-in file logging that writes directly to MAUI_LOG_FILE path
$catalystAppProcess = $null
if ($Platform -eq "catalyst") {
    # Determine runtime identifier
    $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLower()
    $rid = if ($arch -eq "arm64") { "maccatalyst-arm64" } else { "maccatalyst-x64" }
    
    # Build app path - matches Build-AndDeploy.ps1 output location
    $appPath = Join-Path $PSScriptRoot "../../artifacts/bin/Controls.TestCases.HostApp/Debug/$TargetFramework/$rid/Controls.TestCases.HostApp.app"
    $appPath = [System.IO.Path]::GetFullPath($appPath)
    
    if (Test-Path $appPath) {
        Write-Info "MacCatalyst app ready at: $appPath"
        
        # Make executable (like CI does)
        $executablePath = Join-Path $appPath "Contents/MacOS/Controls.TestCases.HostApp"
        if (Test-Path $executablePath) {
            & chmod +x $executablePath
        }
        
        Write-Success "MacCatalyst app prepared (Appium will launch with test name)"
    } else {
        Write-Warning "MacCatalyst app not found at: $appPath"
        Write-Warning "Test may use wrong app bundle if another version is registered"
    }
    
    # Set log file path directly - app will write ILogger output here
    $env:MAUI_LOG_FILE = $deviceLogFile
}

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
} finally {
    # Stop MacCatalyst app process if we started it
    if ($catalystAppProcess) {
        # Re-fetch the process since the original reference may be stale
        $runningApp = Get-Process -Id $catalystAppProcess.Id -ErrorAction SilentlyContinue
        if ($runningApp -and -not $runningApp.HasExited) {
            Write-Info "Stopping MacCatalyst app process (PID: $($catalystAppProcess.Id))..."
            $runningApp.Kill()
            $runningApp.WaitForExit(5000) | Out-Null
            Write-Success "App process stopped"
        }
    }
}

#endregion

#region Capture Device Logs

Write-Step "Capturing device logs..."

if ($Platform -eq "android") {
    Write-Info "Dumping Android logcat buffer (filtered to HostApp)..."
    
    # Try to filter by package name (HostApp)
    # Include DOTNET tag for Console.WriteLine and package name for Debug.WriteLine
    & adb -s $DeviceUdid logcat -d | Select-String "com.microsoft.maui.uitests|DOTNET" > $deviceLogFile
    
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
} elseif ($Platform -eq "catalyst") {
    # App writes directly to $deviceLogFile via MAUI_LOG_FILE env var
    # Just verify the file exists and has content
    if ((Test-Path $deviceLogFile) -and ((Get-Item $deviceLogFile).Length -gt 0)) {
        Write-Success "MacCatalyst logs written directly to: $deviceLogFile"
    } else {
        # Fall back to os_log if file logging didn't work
        Write-Info "File logging output was minimal, using os_log fallback..."
        $logStartTimeStr = $testStartTime.AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss")
        $catalystLogCommand = "log show --level debug --predicate 'process contains `"Controls.TestCases.HostApp`" OR processImagePath contains `"Controls.TestCases.HostApp`"' --start `"$logStartTimeStr`" --style compact"
        Invoke-Expression "$catalystLogCommand > `"$deviceLogFile`" 2>&1"
    }
    
    Write-Info "MacCatalyst logs saved to: $deviceLogFile"
}

#endregion

#region Display Logs

if (Test-Path $deviceLogFile) {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
    if ($Platform -eq "android") {
        Write-Host "  Android Device Logs (Last 100 lines)" -ForegroundColor Cyan
    } elseif ($Platform -eq "ios") {
        Write-Host "  iOS Simulator Logs (Last 100 lines)" -ForegroundColor Cyan
    } elseif ($Platform -eq "catalyst") {
        Write-Host "  MacCatalyst App Logs (Last 100 lines)" -ForegroundColor Cyan
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
