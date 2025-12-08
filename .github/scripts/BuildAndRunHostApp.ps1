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
$HostAppLogsDir = Join-Path $RepoRoot "CustomAgentLogsTmp/UITests"

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

# Set target framework and app identifiers
if ($Platform -eq "android") {
    $TargetFramework = "net10.0-android"
    $AppPackage = "com.microsoft.maui.uitests"
    $AppActivity = "com.microsoft.maui.uitests.MainActivity"
} elseif ($Platform -eq "ios") {
    $TargetFramework = "net10.0-ios"
    $AppBundleId = "com.microsoft.maui.uitests"
}

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

#endregion

#region Build and Deploy

# Use shared Build-AndDeploy script
$buildDeployParams = @{
    Platform = $Platform
    ProjectPath = $HostAppProject
    TargetFramework = $TargetFramework
    Configuration = $Configuration
    DeviceUdid = $DeviceUdid
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
