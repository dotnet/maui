<#
.SYNOPSIS
    Builds and runs .NET MAUI device tests on iOS simulators using xharness.

.DESCRIPTION
    This script builds a specified MAUI device test project for iOS simulator
    and runs the tests using xharness. It handles simulator selection,
    build configuration, and test execution.

.PARAMETER Project
    The device test project to run. Valid values: Controls, Core, Essentials, Graphics, BlazorWebView

.PARAMETER iOSVersion
    Optional iOS version to target (e.g., "26", "18"). If not specified, uses default simulator.

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

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Core -iOSVersion 26

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -TestFilter "Category=Button"

.EXAMPLE
    ./Run-DeviceTests.ps1 -Project Controls -BuildOnly
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true, Position = 0)]
    [ValidateSet("Controls", "Core", "Essentials", "Graphics", "BlazorWebView")]
    [string]$Project,

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
    [string]$Timeout = "01:00:00"
)

$ErrorActionPreference = "Stop"

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

try {
    # Validate prerequisites
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  MAUI Device Tests Runner" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    # Check for xharness (try local tool first, then global)
    $xharness = Get-Command "xharness" -ErrorAction SilentlyContinue
    $useLocalXharness = $false
    
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
    Write-Host "  Building $Project Device Tests" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

    $buildArgs = @(
        "build"
        $projectPath
        "-c", $Configuration
        "-f", "net10.0-ios"
        "-r", "iossimulator-arm64"
        "/p:CodesignRequireProvisioningProfile=false"
        "/p:TreatWarningsAsErrors=false"
    )

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
    $appPath = "artifacts/bin/$Project.DeviceTests/$Configuration/net10.0-ios/iossimulator-arm64/$appName.app"
    
    if (-not (Test-Path $appPath)) {
        Write-Error "Built app not found at: $appPath"
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
    # TEST PHASE
    # ═══════════════════════════════════════════════════════════
    # ═══════════════════════════════════════════════════════════
    # SIMULATOR SETUP
    # ═══════════════════════════════════════════════════════════
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Starting iOS Simulator" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    # Use Start-Emulator.ps1 to detect/boot iOS simulator
    # This ensures the simulator is ready before xharness runs
    if ($iOSVersion) {
        Write-Info "Requesting iOS version: $iOSVersion"
        # Note: Start-Emulator.ps1 doesn't support version filtering yet
        # For now, we'll let xharness handle version targeting via --target flag
    }
    
    # Call Start-Emulator.ps1 directly (not dot-sourced)
    # This will export $env:DEVICE_UDID
    $startEmulatorPath = Join-Path $SharedScriptsDir "Start-Emulator.ps1"
    $SimulatorUdid = & pwsh -File $startEmulatorPath -Platform "ios"
    
    if (-not $SimulatorUdid) {
        Write-Error "Failed to start iOS simulator"
        exit 1
    }
    
    Write-Host ""
    Write-Host "✓ Simulator ready: $SimulatorUdid" -ForegroundColor Green

    # ═══════════════════════════════════════════════════════════
    # TEST PHASE
    # ═══════════════════════════════════════════════════════════
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "  Running Tests with XHarness" -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

    # Create output directory
    if (-not (Test-Path $OutputDirectory)) {
        New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    }

    # Determine target
    # xharness will use its own simulator management, but we've ensured one is booted
    $target = "ios-simulator-64"
    if ($iOSVersion) {
        $target = "ios-simulator-64_$iOSVersion"
    }

    # Build xharness arguments
    $xharnessArgs = @(
        "apple", "test"
        "--app", $appPath
        "--target", $target
        "-o", $OutputDirectory
        "--timeout", $Timeout
        "-v"
    )

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
    Write-Host "Target: $target" -ForegroundColor Yellow
    Write-Host ""

    if ($useLocalXharness) {
        & dotnet xharness @xharnessArgs
    } else {
        & xharness @xharnessArgs
    }

    $testExitCode = $LASTEXITCODE

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
