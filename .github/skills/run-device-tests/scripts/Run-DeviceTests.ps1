<#
.SYNOPSIS
    Builds and runs .NET MAUI device tests locally using xharness (Apple/Android) or vstest (Windows).

.DESCRIPTION
    This script builds a specified MAUI device test project for the target platform
    and runs the tests. It handles device/emulator/simulator selection, build configuration,
    and test execution.

    Platform support by OS:
    - macOS: ios, maccatalyst, android
    - Windows: android, windows

.PARAMETER Project
    The device test project to run. Valid values: Controls, Core, Essentials, Graphics, BlazorWebView

.PARAMETER Platform
    Target platform. Valid values depend on OS:
    - macOS: ios (default), maccatalyst, android
    - Windows: android, windows (default)

.PARAMETER iOSVersion
    Optional iOS version to target (e.g., "26", "18"). Only applies to ios platform.

.PARAMETER Configuration
    Build configuration. Defaults to "Release".

.PARAMETER TestFilter
    Optional test filter to run specific tests (e.g., "Category=Button").

.PARAMETER BuildOnly
    If specified, only builds the project without running tests.

.PARAMETER OutputDirectory
    Directory for test logs and results. Defaults to "artifacts/log".

.PARAMETER Timeout
    Test timeout in format HH:MM:SS. Defaults to "01:00:00" (1 hour).

.PARAMETER DeviceUdid
    Optional specific device UDID to use. If not provided, auto-detects appropriate device.

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -Platform ios

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Core -Platform maccatalyst

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -Platform android

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -Platform windows

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -Platform ios -iOSVersion 26

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -Platform ios -TestFilter "Category=Button"

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -Platform android -BuildOnly
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("Controls", "Core", "Essentials", "Graphics", "BlazorWebView")]
    [string]$Project,

    [Parameter(Mandatory = $false)]
    [ValidateSet("ios", "maccatalyst", "android", "windows")]
    [string]$Platform,

    [Parameter(Mandatory = $false)]
    [string]$iOSVersion,

    [Parameter(Mandatory = $false)]
    [string]$Configuration = "Release",

    [Parameter(Mandatory = $false)]
    [string]$TestFilter,

    [Parameter(Mandatory = $false)]
    [switch]$BuildOnly,

    [Parameter(Mandatory = $false)]
    [string]$OutputDirectory = "artifacts/log",

    [Parameter(Mandatory = $false)]
    [string]$Timeout = "01:00:00",

    [Parameter(Mandatory = $false)]
    [string]$DeviceUdid,

    [Parameter(Mandatory = $false)]
    [switch]$SkipXcodeVersionCheck
)

$ErrorActionPreference = "Stop"

# Determine default platform based on OS
if (-not $Platform) {
    if ($IsWindows) {
        $Platform = "windows"
    } else {
        $Platform = "ios"
    }
}

# Validate platform availability on current OS
$validPlatforms = if ($IsWindows) { @("android", "windows") } else { @("ios", "maccatalyst", "android") }
if ($Platform -notin $validPlatforms) {
    Write-Error "Platform '$Platform' is not supported on this OS. Valid platforms: $($validPlatforms -join ', ')"
    exit 1
}

# iOSVersion only applies to ios platform
if ($iOSVersion -and $Platform -ne "ios") {
    Write-Warning "-iOSVersion parameter is only applicable to ios platform. Ignoring."
    $iOSVersion = $null
}

# Project paths mapping
$ProjectPaths = @{
    "Controls"      = "src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj"
    "Core"          = "src/Core/tests/DeviceTests/Core.DeviceTests.csproj"
    "Essentials"    = "src/Essentials/test/DeviceTests/Essentials.DeviceTests.csproj"
    "Graphics"      = "src/Graphics/tests/DeviceTests/Graphics.DeviceTests.csproj"
    "BlazorWebView" = "src/BlazorWebView/tests/DeviceTests/MauiBlazorWebView.DeviceTests.csproj"
}

$AppNames = @{
    "Controls"      = "Microsoft.Maui.Controls.DeviceTests"
    "Core"          = "Microsoft.Maui.Core.DeviceTests"
    "Essentials"    = "Microsoft.Maui.Essentials.DeviceTests"
    "Graphics"      = "Microsoft.Maui.Graphics.DeviceTests"
    "BlazorWebView" = "Microsoft.Maui.MauiBlazorWebView.DeviceTests"
}

# Platform-specific configurations
$PlatformConfigs = @{
    "ios" = @{
        Tfm = "net10.0-ios"
        RuntimeIdentifier = "iossimulator-arm64"
        AppExtension = ".app"
        XHarnessTarget = "ios-simulator-64"
        UsesXHarness = $true
        EmulatorPlatform = "ios"
    }
    "maccatalyst" = @{
        Tfm = "net10.0-maccatalyst"
        RuntimeIdentifier = "maccatalyst-arm64"
        AppExtension = ".app"
        XHarnessTarget = "maccatalyst"
        UsesXHarness = $true
        EmulatorPlatform = $null  # No emulator needed for Mac Catalyst
    }
    "android" = @{
        Tfm = "net10.0-android"
        RuntimeIdentifier = $null  # Let MSBuild choose
        AppExtension = "-Signed.apk"
        XHarnessTarget = "android-emulator-64"
        UsesXHarness = $true
        EmulatorPlatform = "android"
    }
    "windows" = @{
        Tfm = "net10.0-windows10.0.19041.0"
        RuntimeIdentifier = "win10-x64"
        AppExtension = ".exe"
        XHarnessTarget = $null
        UsesXHarness = $false
        EmulatorPlatform = $null  # No emulator needed for Windows
    }
}

# Find repository root
$RepoRoot = $PSScriptRoot
while ($RepoRoot -and -not (Test-Path (Join-Path $RepoRoot ".git"))) {
    $RepoRoot = Split-Path $RepoRoot -Parent
}

if (-not $RepoRoot) {
    Write-Error "Could not find repository root. Run this script from within the maui repository."
    exit 1
}

# Import shared utilities
$SharedScriptsDir = Join-Path $RepoRoot ".github/scripts/shared"
. (Join-Path $SharedScriptsDir "shared-utils.ps1")

Push-Location $RepoRoot

$platformConfig = $PlatformConfigs[$Platform]

try {
    # Validate prerequisites
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  MAUI Device Tests Runner" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    # Check for xharness if needed (try local tool first, then global)
    $useLocalXharness = $false
    if ($platformConfig.UsesXHarness) {
        $xharness = Get-Command "xharness" -ErrorAction SilentlyContinue
        
        if (-not $xharness) {
            # Try dotnet tool (local tool manifest)
            try {
                $null = & dotnet xharness help 2>&1
                Write-Host "✓ xharness found: local dotnet tool" -ForegroundColor Green
                $useLocalXharness = $true
            } catch {
                Write-Error "xharness is not installed. Install with: dotnet tool install --global Microsoft.DotNet.XHarness.CLI"
                exit 1
            }
        } else {
            Write-Host "✓ xharness found: $($xharness.Source)" -ForegroundColor Green
        }
    }

    # Check for dotnet
    $dotnet = Get-Command "dotnet" -ErrorAction SilentlyContinue
    if (-not $dotnet) {
        Write-Error "dotnet is not installed."
        exit 1
    }
    Write-Host "✓ dotnet found: $($dotnet.Source)" -ForegroundColor Green

    $projectPath = $ProjectPaths[$Project]
    $appName = $AppNames[$Project]
    
    Write-Host ""
    Write-Host "Project:       $Project" -ForegroundColor Yellow
    Write-Host "Project Path:  $projectPath" -ForegroundColor Yellow
    Write-Host "Platform:      $Platform" -ForegroundColor Yellow
    Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
    if ($iOSVersion) {
        Write-Host "iOS Version:   $iOSVersion" -ForegroundColor Yellow
    }
    if ($TestFilter) {
        Write-Host "Test Filter:   $TestFilter" -ForegroundColor Yellow
    }
    Write-Host ""

    # ═══════════════════════════════════════════════════════════
    # BUILD PHASE
    # ═══════════════════════════════════════════════════════════
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Building $Project Device Tests for $Platform" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

    $buildArgs = @(
        "build"
        $projectPath
        "-c", $Configuration
        "-f", $platformConfig.Tfm
        "/p:TreatWarningsAsErrors=false"
    )

    # Add RuntimeIdentifier if specified
    if ($platformConfig.RuntimeIdentifier) {
        $buildArgs += "-r", $platformConfig.RuntimeIdentifier
    }

    # Platform-specific build properties
    switch ($Platform) {
        "ios" {
            $buildArgs += "/p:CodesignRequireProvisioningProfile=false"
            if ($SkipXcodeVersionCheck) {
                $buildArgs += "/p:ValidateXcodeVersion=false"
            }
        }
        "maccatalyst" {
            $buildArgs += "/p:CodesignRequireProvisioningProfile=false"
            if ($SkipXcodeVersionCheck) {
                $buildArgs += "/p:ValidateXcodeVersion=false"
            }
        }
        "android" {
            $buildArgs += "/p:AndroidPackageFormat=apk"
        }
        "windows" {
            $buildArgs += "/p:WindowsPackageType=None"
            $buildArgs += "/p:WindowsAppSDKSelfContained=true"
        }
    }

    Write-Host "Running: dotnet $($buildArgs -join ' ')" -ForegroundColor Gray
    Write-Host ""

    & dotnet @buildArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build failed with exit code $LASTEXITCODE"
        exit $LASTEXITCODE
    }

    Write-Host ""
    Write-Host "✓ Build succeeded" -ForegroundColor Green

    # Find the built app
    $tfmFolder = $platformConfig.Tfm
    $ridFolder = if ($platformConfig.RuntimeIdentifier) { $platformConfig.RuntimeIdentifier } else { "" }
    
    # Construct app path based on platform
    switch ($Platform) {
        "ios" {
            $appPath = "artifacts/bin/$Project.DeviceTests/$Configuration/$tfmFolder/$ridFolder/$appName.app"
        }
        "maccatalyst" {
            # MacCatalyst apps may have different names - search for .app bundle
            $appSearchPath = "artifacts/bin/$Project.DeviceTests/$Configuration/$tfmFolder/$ridFolder"
            $appBundle = Get-ChildItem -Path $appSearchPath -Filter "*.app" -Directory -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($appBundle) {
                $appPath = $appBundle.FullName
            } else {
                $appPath = "$appSearchPath/$appName.app"
            }
        }
        "android" {
            # Android APK path - look for signed APK
            $apkSearchPath = "artifacts/bin/$Project.DeviceTests/$Configuration/$tfmFolder"
            $apkFile = Get-ChildItem -Path $apkSearchPath -Filter "*-Signed.apk" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($apkFile) {
                $appPath = $apkFile.FullName
            } else {
                # Fall back to unsigned APK
                $apkFile = Get-ChildItem -Path $apkSearchPath -Filter "*.apk" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
                if ($apkFile) {
                    $appPath = $apkFile.FullName
                } else {
                    $appPath = "$apkSearchPath/$appName.apk"
                }
            }
        }
        "windows" {
            $appPath = "artifacts/bin/$Project.DeviceTests/$Configuration/$tfmFolder/$ridFolder/$appName.exe"
        }
    }
    
    if (-not (Test-Path $appPath)) {
        Write-Error "Built app not found at: $appPath"
        Write-Info "Searching for app in artifacts..."
        Get-ChildItem -Path "artifacts/bin/$Project.DeviceTests" -Recurse -ErrorAction SilentlyContinue | 
            Where-Object { $_.Name -match "$appName" } |
            ForEach-Object { Write-Host "  Found: $($_.FullName)" }
        exit 1
    }

    Write-Host "✓ App found: $appPath" -ForegroundColor Green

    if ($BuildOnly) {
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host "  Build completed (BuildOnly mode)" -ForegroundColor Green
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        exit 0
    }

    # ═══════════════════════════════════════════════════════════
    # DEVICE/EMULATOR SETUP (if needed)
    # ═══════════════════════════════════════════════════════════
    $deviceUdidToUse = $DeviceUdid
    $DetectedIOSVersion = $null

    if ($platformConfig.EmulatorPlatform) {
        Write-Host ""
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host "  Starting $Platform Device/Emulator" -ForegroundColor Cyan
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
        Write-Host ""

        # Use Start-Emulator.ps1 to detect/boot device
        $startEmulatorPath = Join-Path $SharedScriptsDir "Start-Emulator.ps1"
        
        $emulatorArgs = @("-File", $startEmulatorPath, "-Platform", $platformConfig.EmulatorPlatform)
        if ($DeviceUdid) {
            $emulatorArgs += "-DeviceUdid", $DeviceUdid
        }
        
        $emulatorOutput = & pwsh @emulatorArgs 2>&1
        
        # Extract UDID from output (last line, trimmed)
        $deviceUdidToUse = ($emulatorOutput | Select-Object -Last 1).ToString().Trim()
        
        # Validate UDID format based on platform
        $validUdid = $false
        switch ($Platform) {
            "ios" {
                $validUdid = $deviceUdidToUse -match '^[A-F0-9]{8}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{4}-[A-F0-9]{12}$'
            }
            "android" {
                $validUdid = $deviceUdidToUse -match '^emulator-\d+$' -or $deviceUdidToUse -match '^[a-zA-Z0-9]+$'
            }
        }
        
        if (-not $validUdid) {
            Write-Error "Failed to get valid device UDID. Got: $deviceUdidToUse"
            Write-Host "Full output:" -ForegroundColor Red
            $emulatorOutput | ForEach-Object { Write-Host $_ }
            exit 1
        }
        
        Write-Host ""
        Write-Host "✓ Device ready: $deviceUdidToUse" -ForegroundColor Green

        # Extract iOS version from the booted simulator for XHarness targeting
        if ($Platform -eq "ios" -and -not $iOSVersion) {
            Write-Host ""
            Write-Host "Detecting iOS version from simulator..." -ForegroundColor Gray
            
            try {
                $simListJson = xcrun simctl list devices available -j | ConvertFrom-Json
                
                foreach ($runtime in $simListJson.devices.PSObject.Properties) {
                    $device = $runtime.Value | Where-Object { $_.udid -eq $deviceUdidToUse }
                    if ($device) {
                        # Extract version from runtime key (e.g., "com.apple.CoreSimulator.SimRuntime.iOS-18-5" -> "18.5")
                        if ($runtime.Name -match 'iOS-(\d+)-(\d+)') {
                            $DetectedIOSVersion = "$($matches[1]).$($matches[2])"
                            Write-Host "✓ Detected iOS version: $DetectedIOSVersion" -ForegroundColor Green
                        }
                        break
                    }
                }
            } catch {
                Write-Warning "Could not detect iOS version from simulator. Continuing without version in target."
            }
        }
    }

    # ═══════════════════════════════════════════════════════════
    # TEST PHASE
    # ═══════════════════════════════════════════════════════════
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Running Tests" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

    # Create output directory
    if (-not (Test-Path $OutputDirectory)) {
        New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    }

    $testExitCode = 0

    if ($platformConfig.UsesXHarness) {
        # ═══════════════════════════════════════════════════════════
        # XHARNESS TEST EXECUTION (iOS, MacCatalyst, Android)
        # ═══════════════════════════════════════════════════════════
        
        # Determine target
        $target = $platformConfig.XHarnessTarget
        
        # Add iOS version to target if available
        if ($Platform -eq "ios") {
            $targetVersion = if ($iOSVersion) { $iOSVersion } else { $DetectedIOSVersion }
            if ($targetVersion) {
                $target = "ios-simulator-64_$targetVersion"
            }
        }

        # Build xharness arguments based on platform
        switch ($Platform) {
            "ios" {
                $xharnessArgs = @(
                    "apple", "test"
                    "--app", $appPath
                    "--target", $target
                    "--device", $deviceUdidToUse
                    "-o", $OutputDirectory
                    "--timeout", $Timeout
                    "-v"
                )
            }
            "maccatalyst" {
                $xharnessArgs = @(
                    "apple", "test"
                    "--app", $appPath
                    "--target", "maccatalyst"
                    "-o", $OutputDirectory
                    "--timeout", $Timeout
                    "-v"
                )
            }
            "android" {
                $xharnessArgs = @(
                    "android", "test"
                    "--app", $appPath
                    "--package-name", $appName
                    "--device-id", $deviceUdidToUse
                    "-o", $OutputDirectory
                    "--timeout", $Timeout
                    "-v"
                )
            }
        }

        if ($TestFilter) {
            $xharnessArgs += "--set-env=TestFilter=$TestFilter"
        }

        if ($useLocalXharness) {
            $xharnessCommand = "dotnet xharness"
        } else {
            $xharnessCommand = "xharness"
        }
        
        Write-Host "Running: $xharnessCommand $($xharnessArgs -join ' ')" -ForegroundColor Gray
        Write-Host ""
        Write-Host "Target:  $target" -ForegroundColor Yellow
        if ($deviceUdidToUse) {
            Write-Host "Device:  $deviceUdidToUse" -ForegroundColor Yellow
        }
        Write-Host ""

        if ($useLocalXharness) {
            & dotnet xharness @xharnessArgs
        } else {
            & xharness @xharnessArgs
        }

        $testExitCode = $LASTEXITCODE
    } else {
        # ═══════════════════════════════════════════════════════════
        # VSTEST EXECUTION (Windows)
        # ═══════════════════════════════════════════════════════════
        
        Write-Host "Running tests with vstest..." -ForegroundColor Gray
        Write-Host ""
        
        $vstestArgs = @(
            "test"
            $projectPath
            "-c", $Configuration
            "-f", $platformConfig.Tfm
            "--no-build"
            "--logger", "trx;LogFileName=TestResults.trx"
            "--results-directory", $OutputDirectory
        )

        if ($TestFilter) {
            $vstestArgs += "--filter", $TestFilter
        }

        Write-Host "Running: dotnet $($vstestArgs -join ' ')" -ForegroundColor Gray
        Write-Host ""

        & dotnet @vstestArgs

        $testExitCode = $LASTEXITCODE
    }

    # ═══════════════════════════════════════════════════════════
    # RESULTS
    # ═══════════════════════════════════════════════════════════
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Test Results" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

    # Try to find and parse the log file
    $logFile = Get-ChildItem -Path $OutputDirectory -Filter "$appName.log" -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($logFile) {
        $logContent = Get-Content $logFile.FullName -Raw
        $passCount = ([regex]::Matches($logContent, '\[PASS\]')).Count
        $failCount = ([regex]::Matches($logContent, '\[FAIL\]')).Count
        
        Write-Host ""
        Write-Host "  Passed: $passCount" -ForegroundColor Green
        Write-Host "  Failed: $failCount" -ForegroundColor $(if ($failCount -gt 0) { "Red" } else { "Green" })
        Write-Host ""
        Write-Host "  Log file: $($logFile.FullName)" -ForegroundColor Gray
        
        if ($failCount -gt 0) {
            Write-Host ""
            Write-Host "  Failed tests:" -ForegroundColor Red
            $logContent -split "`n" | Where-Object { $_ -match '\[FAIL\]' } | 
                ForEach-Object { $_ -replace '.*\[FAIL\]\s*', '' } |
                Select-Object -Unique |
                ForEach-Object { Write-Host "    - $_" -ForegroundColor Red }
        }
    }

    Write-Host ""
    if ($testExitCode -eq 0) {
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
        Write-Host "  Tests completed successfully" -ForegroundColor Green
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
    } else {
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Host "  Tests completed with exit code: $testExitCode" -ForegroundColor Yellow
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    }

    exit $testExitCode

} finally {
    Pop-Location
}
