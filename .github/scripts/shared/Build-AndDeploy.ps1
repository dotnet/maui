#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and deploys a .NET MAUI project to Android or iOS device/simulator.

.DESCRIPTION
    Handles building and deployment for both Android and iOS platforms.
    - Android: Uses dotnet build with -t:Run target
    - iOS: Builds app, then installs to simulator using xcrun simctl

.PARAMETER Platform
    Target platform: "android" or "ios"

.PARAMETER ProjectPath
    Full path to the .csproj file to build

.PARAMETER TargetFramework
    Target framework (e.g., "net10.0-android", "net10.0-ios")

.PARAMETER Configuration
    Build configuration: "Debug" or "Release" (default: Debug)

.PARAMETER DeviceUdid
    Device UDID to deploy to (must be set, typically from Start-Emulator.ps1)

.PARAMETER BundleId
    (iOS only) Bundle ID of the app to install

.EXAMPLE
    pwsh Build-AndDeploy.ps1 -Platform android -ProjectPath "./App.csproj" -TargetFramework "net10.0-android" -DeviceUdid "emulator-5554"

.EXAMPLE
    pwsh Build-AndDeploy.ps1 -Platform ios -ProjectPath "./App.csproj" -TargetFramework "net10.0-ios" -DeviceUdid "AC8BCB28..." -BundleId "com.example.app"
#>

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("android", "ios")]
    [string]$Platform,
    
    [Parameter(Mandatory=$true)]
    [string]$ProjectPath,
    
    [Parameter(Mandatory=$true)]
    [string]$TargetFramework,
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Debug",
    
    [Parameter(Mandatory=$true)]
    [string]$DeviceUdid,
    
    [Parameter(Mandatory=$false)]
    [string]$BundleId
)

# Import shared utilities
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptDir "shared-utils.ps1")

# Verify project exists
if (-not (Test-Path $ProjectPath)) {
    Write-Error "Project file not found: $ProjectPath"
    exit 1
}

$projectName = (Get-Item $ProjectPath).BaseName

if ($Platform -eq "android") {
    #region Android Build and Deploy
    
    Write-Step "Building and deploying $projectName for Android..."
    Write-Info "Build command: dotnet build $ProjectPath -f $TargetFramework -c $Configuration -t:Run"
    
    $buildStartTime = Get-Date
    
    # Build and deploy in one step (Run target handles both)
    dotnet build $ProjectPath -f $TargetFramework -c $Configuration -t:Run
    
    $buildExitCode = $LASTEXITCODE
    $buildDuration = (Get-Date) - $buildStartTime
    
    if ($buildExitCode -ne 0) {
        Write-Error "Build/deploy failed with exit code $buildExitCode"
        exit $buildExitCode
    }
    
    Write-Success "Build and deploy completed in $($buildDuration.TotalSeconds) seconds"
    
    #endregion
    
} elseif ($Platform -eq "ios") {
    #region iOS Build and Deploy
    
    Write-Step "Building $projectName for iOS..."
    Write-Info "Build command: dotnet build $ProjectPath -f $TargetFramework -c $Configuration"
    
    $buildStartTime = Get-Date
    
    # Build app
    dotnet build $ProjectPath -f $TargetFramework -c $Configuration
    
    $buildExitCode = $LASTEXITCODE
    $buildDuration = (Get-Date) - $buildStartTime
    
    if ($buildExitCode -ne 0) {
        Write-Error "Build failed with exit code $buildExitCode"
        exit $buildExitCode
    }
    
    Write-Success "Build completed in $($buildDuration.TotalSeconds) seconds"
    
    # Deploy to iOS simulator
    Write-Step "Deploying to iOS simulator..."
    Write-Info "Booting simulator (if not already running)..."
    xcrun simctl boot $DeviceUdid 2>$null
    
    # Verify simulator is booted
    $simState = xcrun simctl list devices --json | ConvertFrom-Json
    $device = $simState.devices.PSObject.Properties.Value | 
        ForEach-Object { $_ } | 
        Where-Object { $_.udid -eq $DeviceUdid } | 
        Select-Object -First 1
    
    if ($device.state -ne "Booted") {
        Write-Error "Simulator not booted. Current state: $($device.state)"
        exit 1
    }
    
    Write-Success "Simulator is booted"
    
    # Find the built app bundle - search from project directory upwards for artifacts
    $searchPath = Split-Path -Parent $ProjectPath
    $artifactsDir = $null
    
    # Walk up directory tree to find artifacts folder
    while ($searchPath -and -not $artifactsDir) {
        $testPath = Join-Path $searchPath "artifacts"
        if (Test-Path $testPath) {
            $artifactsDir = $testPath
            break
        }
        $parent = Split-Path -Parent $searchPath
        if ($parent -eq $searchPath) { break }  # Reached root
        $searchPath = $parent
    }
    
    if (-not $artifactsDir) {
        Write-Error "Could not find artifacts directory"
        exit 1
    }
    
    Write-Info "Searching for app bundle in: $artifactsDir"
    $appPath = Get-ChildItem -Path $artifactsDir -Filter "*.app" -Recurse -ErrorAction SilentlyContinue | 
        Where-Object { 
            $_.FullName -match "$Configuration.*iossimulator.*$projectName" -and 
            $_.FullName -notmatch "\\obj\\" -and 
            $_.FullName -notmatch "/obj/"
        } |
        Select-Object -First 1
    
    if (-not $appPath) {
        Write-Error "Could not find built app bundle in artifacts directory"
        Write-Info "Searched in: $artifactsDir"
        Write-Info "Looking for pattern: $Configuration.*iossimulator.*$projectName"
        exit 1
    }
    
    Write-Info "Installing app: $($appPath.FullName)"
    xcrun simctl install $DeviceUdid $appPath.FullName
    
    if ($LASTEXITCODE -ne 0) {
        Write-Error "App installation failed"
        exit $LASTEXITCODE
    }
    
    Write-Success "App installed successfully"
    
    #endregion
}
