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
    Target platform: "android", "ios", "catalyst" (MacCatalyst), or "windows"

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
    
.EXAMPLE
    ./BuildAndRunHostApp.ps1 -Platform windows -TestFilter "Issue12345"
#>

[CmdletBinding(DefaultParameterSetName = "TestFilter")]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("android", "ios", "catalyst", "maccatalyst", "windows")]
    [string]$Platform,

    [Parameter(Mandatory = $false, ParameterSetName = "TestFilter")]
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

# Derive the .NET TFM version from the checked-out repo (Directory.Build.props) so the
# HostApp + test assemblies build for the branch's framework (e.g. net11.0-android on the
# net11.0 branch) instead of a hardcoded net10.0.
$DotNetTfm = Get-MauiTfmVersion -RepoRoot $RepoRoot
Write-Info "Using .NET TFM version: net$DotNetTfm (from Directory.Build.props)"

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
    $TargetFramework = "net$DotNetTfm-android"
    $AppPackage = "com.microsoft.maui.uitests"
    $AppActivity = "com.microsoft.maui.uitests.MainActivity"
} elseif ($Platform -eq "ios") {
    $TargetFramework = "net$DotNetTfm-ios"
    $AppBundleId = "com.microsoft.maui.uitests"
} elseif ($Platform -eq "catalyst") {
    $TargetFramework = "net$DotNetTfm-maccatalyst"
    $AppBundleId = "com.microsoft.maui.uitests"
} elseif ($Platform -eq "windows") {
    $TargetFramework = "net$DotNetTfm-windows10.0.19041.0"
    $AppPackage = "com.microsoft.maui.uitests"
}

# Start emulator/simulator (skip for catalyst and windows - runs on desktop)
if ($Platform -ne "catalyst" -and $Platform -ne "windows") {
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
} elseif ($Platform -eq "windows") {
    # Windows runs directly on the host - use "host" as placeholder
    $DeviceUdid = "host"
    Write-Success "Windows will run on host (no device needed)"
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
} elseif ($Platform -eq "windows") {
    $TestProject = Join-Path $RepoRoot "src/Controls/tests/TestCases.WinUI.Tests/Controls.TestCases.WinUI.Tests.csproj"
}

if (-not (Test-Path $TestProject)) {
    Write-Error "Test project not found: $TestProject"
    exit 1
}

Write-Success "Test project: $TestProject"

#endregion

#region Run Tests

# Determine the filter to use.
# NOTE: The CI pipeline `maui-pr-uitests` (definition 313) uses `TestCategory=`
# (see eng/pipelines/common/ui-tests-steps.yml lines 116-164). NUnit accepts
# both `Category=` and `TestCategory=` but Cake's RunTestWithLocalDotNet uses
# `TestCategory=` so we mirror that here for byte-for-byte parity with CI.
if ($Category) {
    $effectiveFilter = "TestCategory=$Category"
    Write-Step "Running UI tests with category: $Category"
} elseif ($TestFilter) {
    $effectiveFilter = $TestFilter
    Write-Step "Running UI tests with filter: $TestFilter"
} else {
    $effectiveFilter = $null
    Write-Step "Running ALL UI tests (no filter)"
}

# Clear device logs before test
if ($Platform -eq "android") {
    Write-Info "Clearing Android logcat buffer before test..."
    & adb -s $DeviceUdid logcat -c

    # Wait for Android settings service to be available.
    Write-Info "Waiting for Android settings service..."
    $settingsReady = $false
    for ($i = 0; $i -lt 30; $i++) {
        $settingsCheck = & adb -s $DeviceUdid shell settings get global device_name 2>&1
        if ($settingsCheck -and $settingsCheck -notmatch "Can't find service|error") {
            $settingsReady = $true
            Write-Success "Settings service ready (device_name=$settingsCheck)"
            break
        }
        Write-Info "  Settings service not ready yet (attempt $($i+1)/30)..."
        Start-Sleep -Seconds 5
    }
    if (-not $settingsReady) {
        Write-Warn "Settings service may not be ready — tests might fail"
    }

    # Warm up the emulator / SystemUI right before launching the app for tests.
    # On the deep-UI-test (platform-pool) stage the emulator may have sat idle
    # for ~15-20 min during workload install + the app build, after which SystemUI
    # can ANR — the app then launches but its first page never renders, so Appium's
    # OneTimeSetUp times out ("Timed out waiting for Go To Test button"). This
    # mirrors the gate's "Warm Up Android Emulator" step but runs at the precise
    # moment (right before dotnet test), independent of how long the build took.
    # We only touch SystemUI / the launcher here — never the HostApp itself
    # (Appium's UiAutomator2 driver owns the HostApp lifecycle).
    Write-Info "Warming up emulator/SystemUI before test..."
    $bootChk = & adb -s $DeviceUdid shell getprop sys.boot_completed 2>$null
    if ("$bootChk".Trim() -ne "1") {
        Write-Warn "Device not responding before test — restarting adb server..."
        & adb kill-server 2>$null; Start-Sleep -Seconds 2
        & adb start-server 2>$null; Start-Sleep -Seconds 2
        # Bound `adb wait-for-device` to 90s portably — the external `timeout` binary differs on
        # Windows (interactive countdown) and may be absent, so use the .NET process timeout.
        $waitProc = Start-Process -FilePath 'adb' -ArgumentList @('-s', $DeviceUdid, 'wait-for-device') -PassThru -NoNewWindow
        if (-not $waitProc.WaitForExit(90000)) {
            Write-Warn "adb wait-for-device timed out after 90s — killing"
            try { $waitProc.Kill() } catch { <# best effort #> }
        }
    }
    # Wake + dismiss any system dialogs (run twice for reliability).
    foreach ($pass in 1..2) {
        & adb -s $DeviceUdid shell input keyevent KEYCODE_WAKEUP 2>$null
        & adb -s $DeviceUdid shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS 2>$null
        & adb -s $DeviceUdid shell input keyevent KEYCODE_BACK 2>$null
        Start-Sleep -Seconds 1
    }
    # If a SystemUI ANR ("isn't responding") dialog is up, force it away. HOME only
    # backgrounds the launcher (the HostApp isn't running yet), so this is safe.
    $winState = & adb -s $DeviceUdid shell dumpsys window 2>$null
    if ("$winState" -match "Application Not Responding|ANR ") {
        Write-Warn "ANR dialog detected before test — dismissing (HOME + close dialogs)"
        & adb -s $DeviceUdid shell input keyevent KEYCODE_HOME 2>$null
        Start-Sleep -Seconds 2
        & adb -s $DeviceUdid shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS 2>$null
        & adb -s $DeviceUdid shell input keyevent KEYCODE_BACK 2>$null
    }
    # Exercise the system briefly to confirm SystemUI is responsive, then clean up
    # (force-stop targets the Settings app, never the HostApp).
    & adb -s $DeviceUdid shell am start -a android.settings.SETTINGS 2>$null
    Start-Sleep -Seconds 2
    & adb -s $DeviceUdid shell am force-stop com.android.settings 2>$null
    & adb -s $DeviceUdid shell am broadcast -a android.intent.action.CLOSE_SYSTEM_DIALOGS 2>$null
    & adb -s $DeviceUdid shell input keyevent KEYCODE_HOME 2>$null
    & adb -s $DeviceUdid logcat -c 2>$null
    Write-Success "Emulator warmed up and responsive"
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
        
        # Set MAC_APP_PATH so Appium mac2 driver can launch the app directly
        $env:MAC_APP_PATH = $appPath
        Write-Success "MacCatalyst app prepared (MAC_APP_PATH=$appPath)"
    } else {
        Write-Warn "MacCatalyst app not found at: $appPath"
        Write-Warn "Test may use wrong app bundle if another version is registered"
    }
    
    # Set log file path directly - app will write ILogger output here
    $env:MAUI_LOG_FILE = $deviceLogFile
}

# For Windows, point the test at the actual built HostApp .exe. UITest.cs
# (TestDevice.Windows) otherwise computes the app path RELATIVE to the test
# assembly ("../../../Controls.TestCases.HostApp/..."), which does NOT resolve to
# the repo's `artifacts/bin` output layout — so WinAppDriver fails OneTimeSetUp
# with "The system cannot find the file specified" and 0 tests run. Setting
# WINDOWS_APP_PATH (honored first by UITest.cs) to the known build output fixes it.
if ($Platform -eq "windows") {
    $hostAppBin = Join-Path $RepoRoot "artifacts/bin/Controls.TestCases.HostApp/Debug/$TargetFramework"
    $winAppExe = $null
    if (Test-Path $hostAppBin) {
        $winAppExe = Get-ChildItem -Path $hostAppBin -Filter "Controls.TestCases.HostApp.exe" -Recurse -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
    }
    if ($winAppExe) {
        $env:WINDOWS_APP_PATH = $winAppExe
        Write-Success "Set WINDOWS_APP_PATH=$winAppExe"
    } else {
        Write-Warn "Windows HostApp .exe not found under $hostAppBin — test will fall back to relative-path resolution (may fail to launch)"
    }
}

$filterDisplay = if ($effectiveFilter) { "--filter `"$effectiveFilter`"" } else { "(no filter — all tests)" }
Write-Info "Executing: dotnet test $filterDisplay"
Write-Host ""

# Set environment variables for the test
$env:DEVICE_UDID = $DeviceUdid
Write-Info "Set DEVICE_UDID environment variable: $DeviceUdid"

# Set APPIUM_LOG_FILE so UITestBase saves screenshots/page-source to our log directory
$appiumLogFile = Join-Path $HostAppLogsDir "appium.log"
$env:APPIUM_LOG_FILE = $appiumLogFile
Write-Info "Set APPIUM_LOG_FILE: $appiumLogFile (screenshots will be saved here)"

# ── TRX setup (mirrors CI: eng/cake/dotnet.cake `RunTestWithLocalDotNet`) ──
# CI writes one trx per test run via:
#   --logger "trx;LogFileName=<sanitized-name>.trx"
#   --logger "console;verbosity=normal"
#   --results-directory <test-results-dir>
#   /p:VStestUseMSBuildOutput=false
# We reproduce that here so STEP 3's renderer can parse authoritative
# pass/fail counts from the TRX (instead of scraping console output, which is
# fragile when many tests run and lines get interleaved or wrapped).
$trxResultsDir = Join-Path $HostAppLogsDir "TestResults"
if (-not (Test-Path $trxResultsDir)) {
    New-Item -ItemType Directory -Path $trxResultsDir -Force | Out-Null
}
# Sanitize the trx file name. NUnit/MSTest reject some characters. We keep
# alpha-numeric, dash, underscore and dot — same set Cake's
# SanitizeTestResultsFilename uses.
$trxBaseName = if ($Category) { "$Category-$Platform" }
               elseif ($TestFilter) { ($TestFilter -replace '[^A-Za-z0-9._-]', '_') }
               else { "ALL-$Platform" }
$trxBaseName = $trxBaseName -replace '[^A-Za-z0-9._-]', '_'
$trxFileName = "$trxBaseName.trx"
$trxFilePath = Join-Path $trxResultsDir $trxFileName
# Pre-clean stale TRX so we never read a previous run's results
if (Test-Path $trxFilePath) { Remove-Item $trxFilePath -Force -ErrorAction SilentlyContinue }

Write-Info "TRX file will be written to: $trxFilePath"

try {
    # Run dotnet test using the SAME loggers and arguments CI uses in
    # `RunTestWithLocalDotNet` (eng/cake/dotnet.cake line 943-981).
    $trxRunStart = Get-Date
    $testArgs = @($TestProject,
        "--logger", "trx;LogFileName=$trxFileName",
        "--logger", "console;verbosity=normal",
        "--results-directory", $trxResultsDir,
        "/p:VStestUseMSBuildOutput=false")
    if ($effectiveFilter) {
        $testArgs = @($TestProject, "--filter", $effectiveFilter) + $testArgs[1..($testArgs.Length-1)]
    }
    Write-Info "Actual dotnet test args: $($testArgs -join ' ')"
    $testOutput = & dotnet test @testArgs 2>&1
    
    # Save test output to file
    $testOutput | Out-File -FilePath $testOutputFile -Encoding UTF8
    
    # Output test results to the output stream so callers can capture them
    # (Write-Host goes to the Information stream which is not captured by 2>&1)
    $testOutput | ForEach-Object { Write-Output $_ }

    # Surface the TRX path on a marker line so callers (Invoke-UITestWithRetry
    # and Review-PR.ps1) can locate the authoritative results file regardless
    # of where the working directory was when this script ran.
    if (Test-Path $trxFilePath) {
        Write-Output ">>> TRX_RESULT_FILE: $trxFilePath"
    } else {
        # dotnet test may have written the TRX with a slightly different name
        # (e.g. LogFileName argument stripped on Windows, or it injected a
        # timestamp). Fall back to scanning the results dir for any .trx
        # written AFTER this run started — never pick up a stale TRX from a
        # previous category that shares the same results directory.
        $latestTrx = Get-ChildItem -Path $trxResultsDir -Filter "*.trx" -ErrorAction SilentlyContinue |
                     Where-Object { $_.LastWriteTime -ge $trxRunStart } |
                     Sort-Object LastWriteTime -Descending | Select-Object -First 1
        if ($latestTrx) {
            Write-Output ">>> TRX_RESULT_FILE: $($latestTrx.FullName)"
        }
    }

    $testExitCode = $LASTEXITCODE
    
    # ── Per-test retry for flaky failures (Android emulator instability) ──
    # Parse the TRX for failed tests and re-run them once. This catches
    # emulator-induced timeouts and transient ADB failures that aren't
    # real test bugs. Only retry on Android where flake rate is ~5%.
    if ($testExitCode -ne 0 -and $Platform -eq 'android' -and (Test-Path $trxFilePath)) {
        . "$PSScriptRoot/shared/Get-TrxResults.ps1"
        $firstRun = Get-TrxResults -TrxPath $trxFilePath
        if ($firstRun -and [int]$firstRun.Failed -gt 0 -and [int]$firstRun.Passed -gt 0) {
            $failedNames = @($firstRun.Results | Where-Object { $_.status -eq 'Failed' } | ForEach-Object { $_.name })
            Write-Host ""
            Write-Warn "🔄 Retrying $($failedNames.Count) failed test(s) on Android..."
            
            # Build a FullyQualifiedName filter for just the failed tests.
            # Strip parameter signatures (e.g. TestMethod(arg: "val")) because
            # VSTest filter grammar treats ( ) | & ! as operators. Using the
            # bare method name with ~ (contains) is safe and sufficient.
            $safeNames = @($failedNames | ForEach-Object { $_ -replace '\(.*$', '' } | Select-Object -Unique)
            $retryFilter = ($safeNames | ForEach-Object { "FullyQualifiedName~$_" }) -join ' | '
            $retryTrx = Join-Path $trxResultsDir "retry-$trxBaseName.trx"
            Remove-Item $retryTrx -Force -ErrorAction SilentlyContinue
            
            $retryArgs = @($TestProject, "--filter", $retryFilter,
                "--logger", "trx;LogFileName=retry-$trxFileName",
                "--logger", "console;verbosity=normal",
                "--results-directory", $trxResultsDir,
                "/p:VStestUseMSBuildOutput=false", "--no-build")
            Write-Info "Retry args: dotnet test --filter '$retryFilter' --no-build"
            $retryOutput = & dotnet test @retryArgs 2>&1
            $retryOutput | ForEach-Object { Write-Output $_ }
            $retryExitCode = $LASTEXITCODE
            
            # Parse retry TRX and count how many passed on retry
            $retryTrxPath = Join-Path $trxResultsDir "retry-$trxFileName"
            if (Test-Path $retryTrxPath) {
                $retryResults = Get-TrxResults -TrxPath $retryTrxPath
                if ($retryResults) {
                    $retryPassed = @($retryResults.Results | Where-Object { $_.status -eq 'Passed' }).Count
                    $retryFailed = @($retryResults.Results | Where-Object { $_.status -eq 'Failed' }).Count
                    Write-Host "  Retry results: $retryPassed passed, $retryFailed failed (of $($failedNames.Count) retried)" -ForegroundColor Cyan
                    
                    if ($retryFailed -eq 0) {
                        Write-Success "All $retryPassed flaky test(s) passed on retry!"
                        $testExitCode = 0
                    } else {
                        Write-Warn "$retryFailed test(s) still failing after retry (real failures)"
                    }
                    # Merge retry results into the original TRX: replace only the
                    # retried test entries in the original with their retry outcomes,
                    # preserving all tests that passed on the first run. This avoids
                    # the prior bug where Copy-Item overwrote the full TRX with the
                    # retry-only TRX, losing the first-run passing tests entirely.
                    try {
                        [xml]$origXml = Get-Content -Path $trxFilePath -Raw -Encoding UTF8
                        [xml]$retryXml = Get-Content -Path $retryTrxPath -Raw -Encoding UTF8
                        $nsUri = 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010'
                        $nsMgr = New-Object System.Xml.XmlNamespaceManager($origXml.NameTable)
                        $nsMgr.AddNamespace('t', $nsUri)
                        $retryNsMgr = New-Object System.Xml.XmlNamespaceManager($retryXml.NameTable)
                        $retryNsMgr.AddNamespace('t', $nsUri)

                        # Build a lookup of retry results by testName
                        $retryByName = @{}
                        foreach ($rr in $retryXml.SelectNodes('//t:UnitTestResult', $retryNsMgr)) {
                            $retryByName[$rr.GetAttribute('testName')] = $rr
                        }

                        # Only replace entries that were in the original failed set.
                        # The retry filter uses substring matching (~) so the retry TRX
                        # may contain tests that passed on the first run (e.g. other
                        # parameterizations of the same method). We must NOT overwrite
                        # those — only replace originally-failed entries.
                        $failedNameSet = New-Object 'System.Collections.Generic.HashSet[string]'
                        foreach ($fn in $failedNames) { [void]$failedNameSet.Add($fn) }

                        foreach ($origResult in $origXml.SelectNodes('//t:UnitTestResult', $nsMgr)) {
                            $tName = $origResult.GetAttribute('testName')
                            if ($failedNameSet.Contains($tName) -and $retryByName.ContainsKey($tName)) {
                                $imported = $origXml.ImportNode($retryByName[$tName], $true)
                                $origResult.ParentNode.ReplaceChild($imported, $origResult) | Out-Null
                            }
                        }

                        # Update counters to reflect merged results. Count outcomes
                        # using the same logic as Get-TrxResults: Passed stays Passed,
                        # NotExecuted/Inconclusive are Skipped, everything else is Failed.
                        $allResults = $origXml.SelectNodes('//t:UnitTestResult', $nsMgr)
                        $mergedTotal = $allResults.Count
                        $mergedPassed = @($allResults | Where-Object { $_.GetAttribute('outcome') -eq 'Passed' }).Count
                        $skippedOutcomes = @('NotExecuted', 'Inconclusive')
                        $mergedSkipped = @($allResults | Where-Object { $_.GetAttribute('outcome') -in $skippedOutcomes }).Count
                        $mergedFailed = $mergedTotal - $mergedPassed - $mergedSkipped
                        $mergedExecuted = $mergedPassed + $mergedFailed
                        $counters = $origXml.SelectSingleNode('//t:ResultSummary/t:Counters', $nsMgr)
                        if ($counters) {
                            $counters.SetAttribute('total', $mergedTotal)
                            $counters.SetAttribute('executed', $mergedExecuted)
                            $counters.SetAttribute('passed', $mergedPassed)
                            $counters.SetAttribute('failed', $mergedFailed)
                        }

                        $origXml.Save($trxFilePath)
                        Write-Info "Merged retry results into original TRX ($mergedTotal total, $mergedPassed passed, $mergedFailed failed)"
                    } catch {
                        Write-Warn "Failed to merge TRX — falling back to retry-only TRX: $_"
                        Copy-Item $retryTrxPath $trxFilePath -Force
                    }
                    # Remove the retry TRX to prevent double-counting by downstream aggregators
                    Remove-Item $retryTrxPath -Force -ErrorAction SilentlyContinue
                }
            }
        }
    }

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

#region Collect Test Artifacts (screenshots, page source)

Write-Step "Collecting test artifacts (screenshots, page source)..."

# Collect any screenshots/page source from the test assembly output directory
# UITestBase saves these via TestContext.AddTestAttachment to the assembly dir
$testAssemblyDirs = @(
    (Join-Path $RepoRoot "artifacts/bin/Controls.TestCases.Android.Tests/Debug/net$DotNetTfm"),
    (Join-Path $RepoRoot "artifacts/bin/Controls.TestCases.iOS.Tests/Debug/net$DotNetTfm"),
    (Join-Path $RepoRoot "artifacts/bin/Controls.TestCases.Mac.Tests/Debug/net$DotNetTfm"),
    (Join-Path $RepoRoot "artifacts/bin/Controls.TestCases.WinUI.Tests/Debug/net$DotNetTfm-windows10.0.19041.0")
)

$copiedCount = 0
foreach ($dir in $testAssemblyDirs) {
    if (Test-Path $dir) {
        $artifacts = Get-ChildItem -Path $dir -File -Include "*.png","*.txt" -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -match "ScreenShot|PageSource" }
        foreach ($artifact in $artifacts) {
            Copy-Item -Path $artifact.FullName -Destination $HostAppLogsDir -Force
            $copiedCount++
        }
    }
}

# Also check the HostAppLogsDir itself for screenshots saved via APPIUM_LOG_FILE
$screenshotCount = (Get-ChildItem -Path $HostAppLogsDir -Filter "*.png" -ErrorAction SilentlyContinue).Count
$pageSourceCount = (Get-ChildItem -Path $HostAppLogsDir -Filter "*PageSource*" -ErrorAction SilentlyContinue).Count
Write-Info "Test artifacts collected: $screenshotCount screenshot(s), $pageSourceCount page source(s) (copied $copiedCount from assembly dir)"

#endregion

#region Capture Device Logs

Write-Step "Capturing device logs..."

if ($Platform -eq "android") {
    Write-Info "Dumping Android logcat buffer (filtered to HostApp)..."
    
    # Try to filter by package name (HostApp)
    # Include DOTNET tag for Console.WriteLine and package name for Debug.WriteLine
    & adb -s $DeviceUdid logcat -d | Select-String "com.microsoft.maui.uitests|DOTNET" > $deviceLogFile
    
    if ((Get-Item $deviceLogFile).Length -eq 0) {
        Write-Warn "No logs found for com.microsoft.maui.uitests, dumping entire logcat..."
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
} elseif ($Platform -eq "windows") {
    # Windows logs - use Event Log or console output
    # For now, collect from test output since WinAppDriver doesn't provide separate device logs
    Write-Info "Windows platform - logs captured from test output"
    
    if ((Test-Path $deviceLogFile) -and ((Get-Item $deviceLogFile).Length -gt 0)) {
        Write-Success "Windows logs written to: $deviceLogFile"
    } else {
        # Create a minimal log file indicating Windows was tested
        "Windows UI Test run at $(Get-Date)" | Out-File $deviceLogFile
        Write-Info "Windows device log created: $deviceLogFile"
    }
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
    } elseif ($Platform -eq "windows") {
        Write-Host "  Windows App Logs (Last 100 lines)" -ForegroundColor Cyan
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
        Write-Warn "Could not read device log file"
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
║  Test Filter:  $($(if ($effectiveFilter) { $effectiveFilter.Substring(0, [Math]::Min(40, $effectiveFilter.Length)) } else { '(all tests)' }).PadRight(40))      ║
║  Result:       SUCCESS ✅                                 ║
║  Logs:         $HostAppLogsDir
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Green

#endregion
