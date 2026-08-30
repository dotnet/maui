#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Automated issue reproduction testing for .NET MAUI using Appium.

.DESCRIPTION
    This script automates the complete workflow for testing MAUI issues:
    1. Builds the Sandbox app for the target platform (Android, iOS, MacCatalyst, or Windows)
    2. Starts Appium server if not already running
    3. Deploys and launches the app using Appium
    4. Runs the Appium test script to validate the issue

.PARAMETER Platform
    Target platform: "android", "ios", "catalyst", or "windows"

.PARAMETER Configuration
    Build configuration: "Debug" or "Release" (default: Debug)

.PARAMETER DeviceUdid
    Specific device UDID to target (optional - will auto-detect if not provided)

.PARAMETER RepoRoot
    Repository root to operate on. Defaults to the script's repository location.

.EXAMPLE
    ./BuildAndRunSandbox.ps1 -Platform android
    
.EXAMPLE
    ./BuildAndRunSandbox.ps1 -Platform catalyst

.EXAMPLE
    ./BuildAndRunSandbox.ps1 -Platform android -DeviceUdid emulator-5554

.EXAMPLE
    ./BuildAndRunSandbox.ps1 -Platform windows
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "ios", "catalyst", "windows")]
    [string]$Platform,

    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",

    [string]$DeviceUdid,

    [string]$RepoRoot,

    [switch]$PrepareOnly,

    [switch]$SkipBuildDeploy,

    [switch]$LaunchOnly,

    [switch]$EnforceNetworkIsolation
)

if ($PrepareOnly -and ($SkipBuildDeploy -or $LaunchOnly)) {
    throw 'PrepareOnly cannot be combined with SkipBuildDeploy or LaunchOnly.'
}
if ($LaunchOnly -and -not $SkipBuildDeploy) {
    throw 'LaunchOnly requires SkipBuildDeploy.'
}

# Script configuration
$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($RepoRoot)) {
    $RepoRoot = [IO.Path]::GetFullPath(
        [string](Resolve-Path "$PSScriptRoot/../.."))
} else {
    $RepoRoot = [IO.Path]::GetFullPath($RepoRoot)
}
if (-not (Test-Path -LiteralPath $RepoRoot -PathType Container)) {
    throw "Repository root does not exist: $RepoRoot"
}
$SandboxProject = Join-Path $RepoRoot "src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj"
$SandboxAppiumDir = Join-Path $RepoRoot "CustomAgentLogsTmp/Sandbox"
$AppiumTestScript = Join-Path $SandboxAppiumDir "RunWithAppiumTest.cs"
$AppiumPort = 4723

function Resolve-CatalystSandboxAppPath {
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$BuildConfiguration,
        [Parameter(Mandatory = $true)][string]$Framework,
        [Parameter(Mandatory = $true)][string]$RuntimeIdentifier
    )

    $outputDirectory = Join-Path $RepositoryRoot (
        "artifacts/bin/Maui.Controls.Sample.Sandbox/" +
        "$BuildConfiguration/$Framework/$RuntimeIdentifier")
    $apps = @(
        Get-ChildItem `
            -LiteralPath $outputDirectory `
            -Filter '*.app' `
            -Directory `
            -ErrorAction SilentlyContinue
    )
    if ($apps.Count -ne 1) {
        throw "Expected exactly one MacCatalyst Sandbox app under '$outputDirectory'; found $($apps.Count)."
    }
    if ($apps[0].Attributes -band [IO.FileAttributes]::ReparsePoint) {
        throw "MacCatalyst Sandbox app must not be a symbolic link: $($apps[0].FullName)"
    }
    return $apps[0].FullName
}

# Import shared utilities
. "$PSScriptRoot/shared/shared-utils.ps1"

# Banner
Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║     .NET MAUI Sandbox Build and Test Script              ║
║     Platform: $($Platform.ToUpper())                                      ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

#region Validation

Write-Step "Validating prerequisites..."

# Create CustomAgentLogsTmp/Sandbox directory if it doesn't exist
if (-not (Test-Path $SandboxAppiumDir)) {
    New-Item -Path $SandboxAppiumDir -ItemType Directory -Force | Out-Null
    Write-Info "Created CustomAgentLogsTmp/Sandbox directory"
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
in the CustomAgentLogsTmp/Sandbox directory before running this test script.

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

# Set target framework and app identifiers
if ($Platform -eq "android") {
    $TargetFramework = "net10.0-android"
    $AppPackage = "com.microsoft.maui.sandbox"
    $AppActivity = "com.microsoft.maui.sandbox.MainActivity"
} elseif ($Platform -eq "ios") {
    $TargetFramework = "net10.0-ios"
    $AppBundleId = "com.microsoft.maui.sandbox"
} elseif ($Platform -eq "catalyst") {
    $TargetFramework = "net10.0-maccatalyst"
    $AppBundleId = "com.microsoft.maui.sandbox"
} elseif ($Platform -eq "windows") {
    $TargetFramework = "net10.0-windows10.0.19041.0"
    $AppPackage = "com.microsoft.maui.sandbox"
}

# For catalyst and windows, skip emulator detection - runs on host
if ($Platform -eq "catalyst") {
    $DeviceUdid = "host"
    Write-Info "MacCatalyst runs on the host Mac - no device/emulator needed"
} elseif ($Platform -eq "windows") {
    $DeviceUdid = "host"
    Write-Info "Windows runs on the host Windows - no device/emulator needed"
} else {
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
}

#endregion

#region Build and Deploy

# Use shared Build-AndDeploy script
$buildDeployParams = @{
    Platform = $Platform
    ProjectPath = $SandboxProject
    TargetFramework = $TargetFramework
    Configuration = $Configuration
    DeviceUdid = $DeviceUdid
    EnforceNetworkIsolation = $EnforceNetworkIsolation
    NoRestore = $EnforceNetworkIsolation
}
if ($EnforceNetworkIsolation) {
    $buildDeployParams.NetworkIsolationManifestPath = Join-Path (
        Split-Path -Parent $PSScriptRoot
    ) 'source-overrides/ReplicationNetworkIsolationManifest.xml'
}

if ($Platform -eq "ios" -or $Platform -eq "catalyst") {
    $buildDeployParams.BundleId = $AppBundleId
}

if (-not $SkipBuildDeploy) {
    & "$PSScriptRoot/shared/Build-AndDeploy.ps1" @buildDeployParams

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build or deployment failed"
        exit 1
    }
} else {
    Write-Info "Skipping Sandbox build/deploy; using the prepared app."
}

if ($PrepareOnly) {
    Write-Success "Sandbox build and deployment preparation completed"
    return
}

$windowsApp = $null
if ($Platform -eq "windows") {
    $windowsBin = Join-Path $RepoRoot "artifacts/bin/Maui.Controls.Sample.Sandbox/$Configuration/$TargetFramework"
    $windowsApp = if (Test-Path -LiteralPath $windowsBin) {
        Get-ChildItem `
            -LiteralPath $windowsBin `
            -Filter "Maui.Controls.Sample.Sandbox.exe" `
            -Recurse `
            -File `
            -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
    }
    if (-not $windowsApp) {
        throw "Windows Sandbox executable was not found under '$windowsBin'."
    }
}

if ($LaunchOnly) {
    Write-Step "Launching the prepared Sandbox before evidence recording..."
    switch ($Platform) {
        "android" {
            # `am start` on a running task resumes it, so the Sandbox would keep
            # the state the previous run left behind -- including a result label
            # already reading BUG REPRODUCED:. Force a cold start.
            & adb -s $DeviceUdid shell am force-stop com.microsoft.maui.sandbox | Out-Null
            & adb -s $DeviceUdid shell am start -W `
                -n "com.microsoft.maui.sandbox/com.microsoft.maui.sandbox.MainActivity"
            if ($LASTEXITCODE -ne 0) {
                throw "Launching the prepared Android Sandbox failed."
            }
        }
        "ios" {
            & xcrun simctl launch --terminate-running-process `
                $DeviceUdid $AppBundleId
            if ($LASTEXITCODE -ne 0) {
                throw "Launching the prepared iOS Sandbox failed."
            }
        }
        "windows" {
            Start-Process -FilePath $windowsApp -WorkingDirectory (Split-Path -Parent $windowsApp)
        }
    }
}

# For MacCatalyst, launch the app BEFORE Appium test (similar to BuildAndRunHostApp.ps1)
# We use dotnet run with StandardOutputPath/StandardErrorPath to capture Console.WriteLine
# See: https://github.com/dotnet/macios/blob/main/docs/building-apps/build-properties.md#runwithopen
$catalystAppProcess = $null
if ($Platform -eq "catalyst") {
    # Determine runtime identifier
    $arch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLower()
    $rid = if ($arch -eq "arm64") { "maccatalyst-arm64" } else { "maccatalyst-x64" }
    
    $appPath = Resolve-CatalystSandboxAppPath `
        -RepositoryRoot $RepoRoot `
        -BuildConfiguration $Configuration `
        -Framework $TargetFramework `
        -RuntimeIdentifier $rid
    Write-Info "Launching MacCatalyst Sandbox app with dotnet run..."
    Write-Info "App path: $appPath"
        
    # Make executable
    $executablePath = Join-Path $appPath "Contents/MacOS/Maui.Controls.Sample.Sandbox"
    if (Test-Path $executablePath) {
        & chmod +x $executablePath
    }
        
    # Use dotnet run with StandardOutputPath/StandardErrorPath
    # This launches the app via 'open' but captures stdout/stderr to files
    # Console.WriteLine on MacCatalyst goes to stderr
    $deviceLogFile = Join-Path $SandboxAppiumDir "catalyst-device.log"
    $stderrFile = "$deviceLogFile.stderr"
    $sandboxProject = Join-Path $RepoRoot "src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj"
        
    $catalystAppProcess = Get-Process -Name "Maui.Controls.Sample.Sandbox" -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $catalystAppProcess) {
        Write-Info "Starting app with dotnet run (logs to $stderrFile)..."
        & dotnet run --project $sandboxProject -f $TargetFramework --no-build `
            -p:StandardOutputPath=$deviceLogFile `
            -p:StandardErrorPath=$stderrFile 2>&1 | Out-Null
        Start-Sleep -Seconds 3
        $catalystAppProcess = Get-Process -Name "Maui.Controls.Sample.Sandbox" -ErrorAction SilentlyContinue | Select-Object -First 1
    }
    Write-Success "MacCatalyst Sandbox app is running$(if ($catalystAppProcess) { " (PID: $($catalystAppProcess.Id))" })."
}

if ($LaunchOnly) {
    Start-Sleep -Seconds 5
    Write-Success "Prepared Sandbox launch settled before evidence recording"
    return
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
    $response = Invoke-WebRequest -Uri "http://127.0.0.1:$AppiumPort/status" -NoProxy -TimeoutSec 2 -ErrorAction Stop
    Write-Success "Appium is already running on port $AppiumPort"
    $appiumWasRunning = $true
} catch {
    Write-Info "Appium not running, starting server on port $AppiumPort..."

    $appiumExecutable = (Get-Command appium -ErrorAction Stop).Source
    $appiumPath = $env:PATH
    $appiumHome = $env:APPIUM_HOME

    # Start Appium in background with logging (appiumLogFile already defined above)
    $appiumJob = Start-Job -ScriptBlock {
        param($executable, $logFile, $pathValue, $homeValue)
        $env:PATH = $pathValue
        $env:APPIUM_HOME = $homeValue
        & $executable --log-level info > $logFile 2>&1
    } -ArgumentList $appiumExecutable, $appiumLogFile, $appiumPath, $appiumHome
    
    Write-Info "Appium logs → $appiumLogFile"
    
    # Wait for Appium to be ready
    $maxWait = 30
    $waited = 0
    $ready = $false
    
    while ($waited -lt $maxWait -and -not $ready) {
        Start-Sleep -Seconds 1
        $waited++
        
        try {
            $response = Invoke-WebRequest -Uri "http://127.0.0.1:$AppiumPort/status" -NoProxy -TimeoutSec 1 -ErrorAction Stop
            $ready = $true
        } catch {
            # Continue waiting
        }
    }
    
    if (-not $ready) {
        if ($appiumJob.State -eq 'Running') {
            Stop-Job $appiumJob
        }
        if (Test-Path -LiteralPath $appiumLogFile -PathType Leaf) {
            Write-Host "Appium startup log:"
            Get-Content -LiteralPath $appiumLogFile -Tail 100 |
                ForEach-Object { Write-Host $_ }
        } else {
            Receive-Job $appiumJob -Keep -ErrorAction SilentlyContinue |
                Select-Object -Last 100 |
                ForEach-Object { Write-Host $_ }
        }
        Remove-Job $appiumJob
        $appiumJob = $null
        Write-Error "Appium failed to start within $maxWait seconds"
        exit 1
    }
    
    Write-Success "Appium server started (Job ID: $($appiumJob.Id))"
}

#endregion

#region Run Appium Test

Write-Step "Running Appium test..."

# Define device log file path based on platform
$deviceLogFile = Join-Path $SandboxAppiumDir "$Platform-device.log"

# Clear logs before test (skip for catalyst and windows - already capturing stdout/stderr)
if ($Platform -eq "android") {
    Write-Info "Clearing Android logcat buffer before test..."
    & adb -s $DeviceUdid logcat -c
} elseif ($Platform -eq "ios") {
    Write-Info "iOS logs will be captured from Appium during test execution..."
} elseif ($Platform -eq "catalyst") {
    Write-Info "MacCatalyst logs are being captured via stdout/stderr redirect..."
} elseif ($Platform -eq "windows") {
    Write-Info "Windows logs will be captured from test output..."
}

Push-Location $SandboxAppiumDir

try {
    # Set trusted adapter inputs for the Appium runner.
    $env:DEVICE_UDID = $DeviceUdid
    $env:REPLICATION_PLATFORM = $Platform
    if ($Platform -eq "windows") {
        $env:REPLICATION_WINDOWS_APP_PATH = $windowsApp
    }
    
    Write-Info "Executing trusted file-based runner: dotnet run --file RunWithAppiumTest.cs"
    Write-Info "Test will connect to device: $DeviceUdid"
    Write-Host ""
    
    # Force file-based mode so project files in the working directory cannot
    # bypass the trusted runner's package and Windows App SDK properties.
    $appiumRunArguments = @('run', '--file', $AppiumTestScript)
    if ($EnforceNetworkIsolation) {
        $appiumRunArguments += '--no-restore'
    }
    $appiumOutput = "" | & dotnet @appiumRunArguments 2>&1
    
    # Display appium test output
    $appiumOutput | ForEach-Object { Write-Host $_ }
    
    $testExitCode = $LASTEXITCODE
    
    # Resolve the Android app PID through trusted adb after the plan completes.
    $sandboxPid = $null
    if ($Platform -eq "android") {
        $pidOutput = @(
            & adb -s $DeviceUdid shell pidof -s com.microsoft.maui.sandbox 2>$null
        )
        if ($LASTEXITCODE -eq 0) {
            $pidText = ($pidOutput -join '').Trim()
            if ($pidText -match '^[1-9][0-9]*$') {
                $sandboxPid = $pidText
            }
        }
    }
    if ($sandboxPid) {
        Write-Host ""
        Write-Info "Resolved Sandbox app PID after Appium plan: $sandboxPid"
        
        Write-Info "Dumping logcat buffer for PID $sandboxPid..."
        & adb -s $DeviceUdid logcat -d --pid=$sandboxPid > $deviceLogFile
        Write-Info "Logcat dumped to: $deviceLogFile"
    }
    elseif ($Platform -eq "android") {
        # Fallback: If we couldn't get PID, dump entire logcat buffer (unfiltered)
        # This ensures we always have logs for the agent to analyze
        Write-Host ""
        Write-Warn "Could not resolve the Sandbox app PID after the Appium plan"
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
    
    # Capture MacCatalyst logs after test completes
    if ($Platform -eq "catalyst") {
        Write-Host ""
        Write-Info "Processing MacCatalyst logs..."
        
        # On macOS, Console.WriteLine goes to stderr, not stdout
        $stderrFile = "$deviceLogFile.stderr"
        if ((Test-Path $stderrFile) -and ((Get-Item $stderrFile).Length -gt 0)) {
            Write-Info "Console.WriteLine output found in stderr..."
            # Copy stderr content to main log file (stderr is where Console.WriteLine goes on macOS)
            Get-Content $stderrFile | Set-Content -Path $deviceLogFile -Encoding UTF8
        }
        
        # If log file is still empty or small, try log show as fallback
        $logFileSize = 0
        if (Test-Path $deviceLogFile) {
            $logFileSize = (Get-Item $deviceLogFile).Length
        }
        
        if ($logFileSize -lt 100) {
            Write-Info "Console output was minimal, using os_log fallback..."
            $logStartTime = (Get-Date).AddMinutes(-2).ToString("yyyy-MM-dd HH:mm:ss")
            $catalystLogCommand = "log show --debug --predicate 'process contains `"Maui.Controls.Sample.Sandbox`" OR processImagePath contains `"Maui.Controls.Sample.Sandbox`"' --start `"$logStartTime`" --style compact"
            Invoke-Expression "$catalystLogCommand > `"$deviceLogFile`" 2>&1"
        }
        
        Write-Info "MacCatalyst logs saved to: $deviceLogFile"
    }
    
    # Show device logs
    if (Test-Path $deviceLogFile) {
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor Cyan
        if ($Platform -eq "android") {
            Write-Host "  Android Logcat Output (Filtered to Sandbox App)" -ForegroundColor Cyan
        } elseif ($Platform -eq "ios") {
            Write-Host "  iOS Simulator Logs (Filtered to Sandbox App)" -ForegroundColor Cyan
        } elseif ($Platform -eq "catalyst") {
            Write-Host "  MacCatalyst App Logs (Console.WriteLine output)" -ForegroundColor Cyan
        } elseif ($Platform -eq "windows") {
            Write-Host "  Windows App Logs (Console.WriteLine output)" -ForegroundColor Cyan
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
            } elseif ($Platform -eq "catalyst") {
                Write-Info "All logs are from Sandbox app only (Console.WriteLine output)"
            } elseif ($Platform -eq "windows") {
                Write-Info "All logs are from Sandbox app only (Console.WriteLine output)"
            } else {
                Write-Info "All logs are from Sandbox app only (Maui.Controls.Sample.Sandbox)"
            }
        } else {
            Write-Warn "Could not read device log file"
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
    
    # Stop MacCatalyst app if we started it
    if ($catalystAppProcess) {
        $runningApp = Get-Process -Id $catalystAppProcess.Id -ErrorAction SilentlyContinue
        if ($runningApp -and -not $runningApp.HasExited) {
            Write-Info "Stopping MacCatalyst Sandbox app (we started it)..."
            Stop-Process -Id $catalystAppProcess.Id -Force -ErrorAction SilentlyContinue
        }
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

# Stop MacCatalyst app if we started it
if ($catalystAppProcess) {
    $runningApp = Get-Process -Id $catalystAppProcess.Id -ErrorAction SilentlyContinue
    if ($runningApp -and -not $runningApp.HasExited) {
        Write-Host ""
        Write-Info "Stopping MacCatalyst Sandbox app (PID: $($catalystAppProcess.Id))..."
        Stop-Process -Id $catalystAppProcess.Id -Force -ErrorAction SilentlyContinue
        Write-Success "MacCatalyst Sandbox app stopped"
    }
}

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
