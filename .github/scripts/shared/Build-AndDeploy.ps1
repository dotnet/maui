#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Builds and deploys a .NET MAUI project to Android, iOS device/simulator, or Windows.

.DESCRIPTION
    Handles building and deployment for Android, iOS, MacCatalyst, and Windows platforms.
    - Android: Uses dotnet build with -t:Run target
    - iOS: Builds app, then installs to simulator using xcrun simctl
    - MacCatalyst: Builds app (runs on host Mac)
    - Windows: Builds app (runs on host Windows)

.PARAMETER Platform
    Target platform: "android", "ios", "catalyst", or "windows"

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
    [ValidateSet("android", "ios", "catalyst", "windows")]
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
    [string]$BundleId,

    [Parameter(Mandatory=$false)]
    [switch]$Rebuild
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
    
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration, "-t:Run")
    if ($Rebuild) {
        $buildArgs += "--no-incremental"
    }
    
    Write-Info "Build command: dotnet build $($buildArgs -join ' ')"
    
    $buildStartTime = Get-Date
    
    # Build and deploy in one step (Run target handles both)
    & dotnet build @buildArgs
    
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
    
    # Detect host architecture for simulator builds
    $hostArch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLower()
    $runtimeId = if ($hostArch -eq "x64") { "iossimulator-x64" } else { "iossimulator-arm64" }
    Write-Info "Host architecture: $hostArch, RuntimeIdentifier: $runtimeId"
    
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration, "-r", $runtimeId)
    if ($Rebuild) {
        $buildArgs += "--no-incremental"
    }
    
    Write-Info "Build command: dotnet build $($buildArgs -join ' ')"
    
    $buildStartTime = Get-Date
    
    # Build app
    & dotnet build @buildArgs
    
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
    
    # Detect simulator architecture to pick the correct app bundle
    $simArch = "arm64"  # Default to arm64 for Apple Silicon
    try {
        # Get the simulator's device type to determine architecture
        $deviceInfo = xcrun simctl list devices --json | ConvertFrom-Json
        $simDevice = $deviceInfo.devices.PSObject.Properties.Value | 
            ForEach-Object { $_ } | 
            Where-Object { $_.udid -eq $DeviceUdid } | 
            Select-Object -First 1
        
        if ($simDevice) {
            # Check if the host machine is x64 or arm64
            $hostArch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLower()
            if ($hostArch -eq "x64") {
                $simArch = "x64"
            }
            Write-Info "Host architecture: $hostArch, using simulator arch: $simArch"
        }
    } catch {
        Write-Info "Could not detect architecture, defaulting to arm64"
    }
    
    $appPath = Get-ChildItem -Path $artifactsDir -Filter "*.app" -Recurse -ErrorAction SilentlyContinue | 
        Where-Object { 
            $_.FullName -match "$Configuration.*iossimulator-$simArch.*$projectName" -and 
            $_.FullName -notmatch "\\obj\\" -and 
            $_.FullName -notmatch "/obj/"
        } |
        Select-Object -First 1
    
    # Fallback: try any iossimulator build if specific arch not found
    if (-not $appPath) {
        Write-Info "Specific arch ($simArch) not found, trying any iossimulator build..."
        $appPath = Get-ChildItem -Path $artifactsDir -Filter "*.app" -Recurse -ErrorAction SilentlyContinue | 
            Where-Object { 
                $_.FullName -match "$Configuration.*iossimulator.*$projectName" -and 
                $_.FullName -notmatch "\\obj\\" -and 
                $_.FullName -notmatch "/obj/"
            } |
            Select-Object -First 1
    }
    
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
} elseif ($Platform -eq "catalyst") {
    #region MacCatalyst Build (no deploy step - runs on host)
    
    Write-Step "Building $projectName for MacCatalyst..."
    
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration)
    if ($Rebuild) {
        $buildArgs += "--no-incremental"
    }
    
    Write-Info "Build command: dotnet build $($buildArgs -join ' ')"
    
    $buildStartTime = Get-Date
    
    # Build app
    & dotnet build @buildArgs
    
    $buildExitCode = $LASTEXITCODE
    $buildDuration = (Get-Date) - $buildStartTime
    
    if ($buildExitCode -ne 0) {
        Write-Error "Build failed with exit code $buildExitCode"
        exit $buildExitCode
    }
    
    Write-Success "Build completed in $($buildDuration.TotalSeconds) seconds"
    
    # MacCatalyst apps run directly on the Mac - no install step needed
    # The test framework (Appium) will launch the app directly
    Write-Success "MacCatalyst app ready (runs on host Mac)"
    
    #endregion
} elseif ($Platform -eq "windows") {
    #region Windows Build (no deploy step - runs on host)
    
    Write-Step "Building $projectName for Windows..."
    
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration)
    if ($Rebuild) {
        $buildArgs += "--no-incremental"
    }
    
    Write-Info "Build command: dotnet build $($buildArgs -join ' ')"
    
    $buildStartTime = Get-Date
    
    # Build app
    & dotnet build @buildArgs
    
    $buildExitCode = $LASTEXITCODE
    $buildDuration = (Get-Date) - $buildStartTime
    
    if ($buildExitCode -ne 0) {
        Write-Error "Build failed with exit code $buildExitCode"
        exit $buildExitCode
    }
    
    Write-Success "Build completed in $($buildDuration.TotalSeconds) seconds"
    
    # Windows apps run directly on the host - no install step needed
    # The test framework (Appium/WinAppDriver) will launch the app directly
    Write-Success "Windows app ready (runs on host Windows)"
    
    #endregion
}
