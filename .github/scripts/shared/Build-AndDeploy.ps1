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

# The deep/gate builds compile the MAUI product (Core, Controls, ...) FROM SOURCE via
# project references — unlike the main maui-pr-uitests pipeline, which builds the
# HostApp against pre-built product PACKAGES. Building from source re-runs the
# product's analyzers, and Directory.Build.props sets TreatWarningsAsErrors=true
# repo-wide, so ANY PublicAPI bookkeeping gap in the PR — a public symbol missing from
# PublicAPI.Unshipped.txt, or even a trivial 'IView' vs 'IView!' nullability mismatch —
# surfaces as RS0016/RS0017: a *warning* elevated to a build-breaking *error*. The
# HostApp then never builds, the UI tests can't run, and the review reports "no UI test
# results" (observed on PR #34883 net10.0-windows: WindowsLifecycle.OnAppInstanceActivated
# not in Core's PublicAPI.Unshipped.txt; and PR #36130: IView vs IView!). Main maui-pr
# passes on the same commit because it never recompiles the product with the analyzer.
# A PublicAPI declaration gap is bookkeeping, not a functional/runtime defect, and it is
# already enforced as a REQUIRED check by the main maui-pr build — so for a UI-test build
# whose only job is to run the app, we stop treating warnings as errors. Genuine compile
# ERRORS (CS-level, a truly broken app) still fail the build; only warnings (including
# the PublicAPI analyzer) stop blocking the app from building and running.
$hostAppBuildProps = @("-p:TreatWarningsAsErrors=false")

if ($Platform -eq "android") {
    #region Android Build and Deploy
    
    Write-Step "Building and deploying $projectName for Android..."
    
    # EmbedAssembliesIntoApk=true is REQUIRED for Appium-driven UI test runs. A Debug
    # Android build defaults to Fast Deployment (EmbedAssembliesIntoApk=false), which keeps
    # the managed assemblies OUTSIDE the .apk and pushes them to the app's private
    # `.__override__/<abi>` directory during the MSBuild deploy. That works for a single
    # `-t:Run` launch, but Appium (and UITestBase's crash-recovery) re-install / re-launch
    # the app on its own — WITHOUT re-pushing the override assemblies — so monodroid finds
    # `.__override__/x86_64` empty and hard-aborts on startup:
    #   F monodroid: No assemblies found in '.../files/.__override__/x86_64'. Assuming this
    #               is part of Fast Deployment. Exiting...
    #   xamarin::android::Helpers::abort_application -> Force finishing MainActivity -> died
    # The app never shows its home screen, UITestBase.OneTimeSetup times out "waiting for
    # Go To Test button", and the WHOLE fixture is marked failed -> "setup failed; N marked
    # failed" (observed on PR #34637 Shape 61/61 and PR #35640 Material3 338/338, and the
    # root of many android "no UI test results" reports). Embedding the assemblies into the
    # APK makes it self-contained so any install/relaunch works — this is exactly what the
    # main maui-pr-uitests pipeline does (eng/devices/android.cake:168,329).
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration, "-t:Run", "-p:EmbedAssembliesIntoApk=true") + $hostAppBuildProps
    if ($Rebuild) {
        $buildArgs += "--no-incremental"
    }
    
    Write-Info "Build command: dotnet build $($buildArgs -join ' ')"
    
    $buildStartTime = Get-Date
    $maxAttempts = 3
    $buildExitCode = 1
    
    for ($attempt = 1; $attempt -le $maxAttempts; $attempt++) {
        if ($attempt -gt 1) {
            Write-Warn "Retrying build/deploy (attempt $attempt of $maxAttempts)..."
            
            # Uninstall any MAUI test packages to clear bad state
            $installedPkg = & adb shell pm list packages 2>$null | Select-String "maui" | ForEach-Object { ($_ -replace "package:", "").Trim() }
            if ($installedPkg) {
                foreach ($pkg in $installedPkg) {
                    Write-Info "Uninstalling $pkg before retry..."
                    & adb uninstall $pkg 2>$null
                }
            }
            
            # Restart ADB server to recover from broken pipe / transient errors
            Write-Info "Restarting ADB server..."
            & adb kill-server 2>$null
            Start-Sleep -Seconds 3
            & adb start-server
            Start-Sleep -Seconds 3
            
            # Wait for device and verify emulator is fully responsive
            Write-Info "Waiting for device to be fully ready..."
            & adb wait-for-device
            Start-Sleep -Seconds 5
            
            # Verify package manager is responsive before retrying build
            $pmReady = $false
            for ($pmCheck = 1; $pmCheck -le 10; $pmCheck++) {
                $pmOutput = & adb shell pm list packages -3 2>&1
                if ($LASTEXITCODE -eq 0 -and $pmOutput -notmatch 'Broken pipe|error') {
                    $pmReady = $true
                    Write-Info "Package manager responsive (check $pmCheck)"
                    break
                }
                Write-Warn "Package manager not ready (check $pmCheck/10), waiting..."
                Start-Sleep -Seconds 3
            }
            
            if (-not $pmReady) {
                Write-Warn "Package manager still unresponsive — attempting build anyway"
            }
        }
        
        & dotnet build @buildArgs
        $buildExitCode = $LASTEXITCODE
        
        if ($buildExitCode -eq 0) {
            break
        }
        
        if ($attempt -lt $maxAttempts) {
            Write-Warn "Build/deploy failed (attempt $attempt). ADB0010/broken-pipe errors are transient on API 30 — will retry."
        }
    }
    
    $buildDuration = (Get-Date) - $buildStartTime
    
    if ($buildExitCode -ne 0) {
        Write-Error "Build/deploy failed after $maxAttempts attempts with exit code $buildExitCode"
        exit $buildExitCode
    }
    
    if ($attempt -gt 1) {
        Write-Success "Build and deploy succeeded on attempt $attempt in $($buildDuration.TotalSeconds) seconds"
    } else {
        Write-Success "Build and deploy completed in $($buildDuration.TotalSeconds) seconds"
    }
    
    #endregion
    
} elseif ($Platform -eq "ios") {
    #region iOS Build and Deploy
    
    Write-Step "Building $projectName for iOS..."
    
    # Detect host architecture for simulator builds
    $hostArch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLower()
    $runtimeId = if ($hostArch -eq "x64") { "iossimulator-x64" } else { "iossimulator-arm64" }
    $simArch = if ($hostArch -eq "x64") { "x64" } else { "arm64" }
    Write-Info "Host architecture: $hostArch, RuntimeIdentifier: $runtimeId"
    
    # Two INDEPENDENT guards keep the iOS HostApp build resilient to a
    # Microsoft.iOS workload (e.g. net11.0-ios 26.5, which advertises a
    # requirement of Xcode 26.6) landing ahead of the agent's installed Xcode:
    #
    #  1. ValidateXcodeVersion=false — skips the SDK's EARLY, up-front
    #     Xcode-version gate (the eager MT0180 "requires the iOS X SDK / Xcode Y"
    #     check that fires before any compilation).
    #
    #  2. PublishTrimmed=false — skips the ILLink trimmer ("Optimizing assemblies
    #     for size"), whose LATE pass re-resolves the device SDK headers and
    #     crashes IL1012 -> MT0180 -> NETSDK1144 when they are older than the
    #     workload wants. ValidateXcodeVersion=false does NOT cover this later
    #     pass, so BOTH guards are required.
    #
    # ROOT CAUSE is AGENT HETEROGENEITY in the AcesShared iOS pool, proven by
    # three identical-command builds that landed on different agents:
    #   * AcesShared 6 (build 14657467) -> trimmer did NOT run -> 216 tests passed
    #   * AcesShared 2 (build 14660995) -> trimmer ran         -> MT0180
    #   * AcesShared 4 (build 14660232) -> trimmer ran         -> MT0180
    # i.e. some agents run the ILLink trimmer for a Debug SIMULATOR build (and
    # lack Xcode 26.6), others don't. PublishTrimmed=false forces the trimmer OFF
    # on ALL agents, so the outcome no longer depends on which agent we land on.
    #
    # NOTE: MtouchLink (None/SdkOnly/Full) is the LEGACY Xamarin linker knob and
    # is IGNORED by the modern .NET-for-iOS trimmer — build 14660232 ran the
    # trimmer and MT0180'd *with* MtouchLink=None. Do NOT reach for it here;
    # PublishTrimmed is the correct lever. This script is shared by the gate and
    # deep stages, so both iOS paths stay resilient.
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration, "-r", $runtimeId, "-p:ValidateXcodeVersion=false", "-p:PublishTrimmed=false") + $hostAppBuildProps
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

    # Shutdown any OTHER booted simulators to avoid Appium connecting to the wrong device
    $bootedSims = xcrun simctl list devices --json | ConvertFrom-Json
    $otherBooted = $bootedSims.devices.PSObject.Properties.Value |
        ForEach-Object { $_ } |
        Where-Object { $_.state -eq "Booted" -and $_.udid -ne $DeviceUdid }
    
    if ($otherBooted) {
        foreach ($sim in $otherBooted) {
            Write-Info "Shutting down other booted simulator: $($sim.name) ($($sim.udid))"
            xcrun simctl shutdown $sim.udid 2>$null
        }
    }

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
    
    # ValidateXcodeVersion=false + PublishTrimmed=false: same two-guard resilience
    # as the iOS build above — MacCatalyst also builds through the Microsoft.MacCatalyst
    # SDK and can hit BOTH the early Xcode gate AND the late ILLink trimmer
    # (MT0180/IL1012/NETSDK1144) when the workload outpaces the agent's Xcode. See
    # the iOS block for the full agent-heterogeneity rationale (PR #35892 review
    # 4685252040).
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration, "-p:ValidateXcodeVersion=false", "-p:PublishTrimmed=false") + $hostAppBuildProps
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
    
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration) + $hostAppBuildProps
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
