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
    [switch]$Rebuild,

    [Parameter(Mandatory=$false)]
    [switch]$EnforceNetworkIsolation,

    [Parameter(Mandatory=$false)]
    [string]$NetworkIsolationManifestPath,

    [Parameter(Mandatory=$false)]
    [switch]$NoRestore,

    [Parameter(Mandatory=$false)]
    [string]$WindowsAppContainerManifestPath,

    [Parameter(Mandatory=$false)]
    [string]$WindowsPackageStatePath
)

# Import shared utilities
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
. (Join-Path $scriptDir "shared-utils.ps1")
. (Join-Path $scriptDir "Assert-ReplicationWindowsAppContainer.ps1")

function Test-TransientAndroidDeployFailure {
    param(
        [AllowEmptyString()]
        [string]$Output
    )

    foreach ($pattern in @(
        '(?im)\bADB0010\b',
        '(?im)\bInstallFailedException\b',
        '(?im)\bbroken pipe\b',
        '(?im)\bdevice offline\b',
        '(?im)\bno devices?/emulators? found\b',
        '(?im)\bconnection (?:reset|closed)\b'
    )) {
        if ($Output -match $pattern) {
            return $true
        }
    }

    return $false
}

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
    if ($EnforceNetworkIsolation) {
        if ([string]::IsNullOrWhiteSpace($NetworkIsolationManifestPath)) {
            throw 'Android replication requires a trusted network-isolation manifest path.'
        }
        $isolationManifest = [IO.Path]::GetFullPath($NetworkIsolationManifestPath)
        if (-not (Test-Path -LiteralPath $isolationManifest -PathType Leaf)) {
            throw 'Android replication network-isolation manifest is missing.'
        }
        $isolationManifestItem = Get-Item -LiteralPath $isolationManifest -Force
        if ($isolationManifestItem.Attributes -band [IO.FileAttributes]::ReparsePoint) {
            throw 'Android replication network-isolation manifest must be a regular file.'
        }
        # Microsoft.Android resolves AndroidManifest relative to the project
        # directory even when MSBuild receives an absolute path.
        $relativeIsolationManifest = [IO.Path]::GetRelativePath(
            (Split-Path -Parent $ProjectPath),
            $isolationManifest
        ).Replace('\', '/')
        if ([IO.Path]::IsPathRooted($relativeIsolationManifest)) {
            throw 'Android replication network-isolation manifest must be reachable relative to the project directory.'
        }
        $buildArgs += "-p:_MauiReplicationAndroidManifest=$relativeIsolationManifest"
    }
    if ($NoRestore) { $buildArgs += "--no-restore" }
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
        
        $buildOutput = $null
        & dotnet build @buildArgs 2>&1 |
            Tee-Object -Variable buildOutput
        $buildExitCode = $LASTEXITCODE
        
        if ($buildExitCode -eq 0) {
            break
        }

        $buildText = ($buildOutput | ForEach-Object { [string]$_ }) -join "`n"
        $isTransientAndroidDeployFailure =
            Test-TransientAndroidDeployFailure -Output $buildText

        if (-not $isTransientAndroidDeployFailure) {
            Write-Error "Build/deploy failed with a deterministic build or configuration error; skipping ADB retries."
            break
        }
        
        if ($attempt -lt $maxAttempts) {
            Write-Warn "Build/deploy failed with a recognized transient Android deployment error (attempt $attempt); will retry."
        }
    }
    
    $buildDuration = (Get-Date) - $buildStartTime
    
    if ($buildExitCode -ne 0) {
        Write-Error "Build/deploy failed after $attempt attempt(s) with exit code $buildExitCode"
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
    
    # Build the iOS HostApp using the SAME proven recipe the MAIN maui-pr-uitests
    # pipeline uses (eng/devices/ios.cake -> ExecuteBuildUITestApp), so the deep
    # stage builds byte-for-byte the way the shipping UI-test lane does:
    #
    #     dotnet build <HostApp> -c Debug -f net-ios \
    #        -p:BuildIpa=true -p:_UseNativeAot=false -r iossimulator-<arch>
    #
    #  * BuildIpa=true — runs the FULL iOS app-packaging pipeline, which is what
    #    compiles + links the native launcher stub that provides the executable's
    #    `main` symbol. This is the load-bearing flag.
    #  * _UseNativeAot=false — Debug simulator uses Mono (NativeAOT is Release-only
    #    for UI tests); mirrors the cake recipe's USE_NATIVE_AOT=false default.
    #  * ValidateXcodeVersion=false — harmless extra guard that skips the SDK's
    #    early Xcode-version gate on heterogeneous agents (the Tahoe image demand
    #    already pins a current-Xcode agent, so ILLink's own SDK check passes).
    #
    # DO NOT set _MustTrim=false here. It was tried (commit a00af5df24) to dodge an
    # intermittent MT0180 from ILLink's Xcode SetupStep, but it ALSO short-circuits
    # the app-packaging path that emits `main`, so the native link then hard-fails
    # with `Undefined symbols for architecture arm64: "_main"` (build 14662537 —
    # managed .dll built fine, then clang++ ld error, ZERO results EVERY run). The
    # main pipeline never sets _MustTrim and does not hit MT0180 on the Tahoe pool,
    # so matching its recipe fixes the link failure without reintroducing MT0180.
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration, "-r", $runtimeId, "-p:BuildIpa=true", "-p:_UseNativeAot=false", "-p:ValidateXcodeVersion=false") + $hostAppBuildProps
    if ($NoRestore) { $buildArgs += "--no-restore" }
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
    
    # Build the MacCatalyst HostApp with the SAME proven recipe the MAIN pipeline
    # uses (eng/devices/catalyst.cake): dotnet build -c Debug -f net-maccatalyst
    #   -p:BuildIpa=true -r maccatalyst-<arch>
    # BuildIpa=true runs the full app-packaging pipeline that emits the native
    # launcher `main` symbol — see the iOS block above: omitting it caused an
    # "Undefined symbols for architecture arm64: _main" hard link failure. The
    # ValidateXcodeVersion=false guard harmlessly skips the SDK's early Xcode gate
    # on heterogeneous agents. Do NOT set _MustTrim=false: it short-circuits the
    # very packaging step that produces `main`, so the native link would fail.
    $macArch = [System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString().ToLower()
    $macRid = if ($macArch -eq "x64") { "maccatalyst-x64" } else { "maccatalyst-arm64" }
    Write-Info "MacCatalyst RuntimeIdentifier: $macRid"
    $buildArgs = @($ProjectPath, "-f", $TargetFramework, "-c", $Configuration, "-r", $macRid, "-p:BuildIpa=true", "-p:ValidateXcodeVersion=false") + $hostAppBuildProps
    if ($NoRestore) { $buildArgs += "--no-restore" }
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
    
    # MacCatalyst apps run directly on the Mac, so there is no install step -
    # but "no install" is not the same as "nothing to do". The Appium mac2
    # driver (WebDriverAgentMac) resolves the app through LaunchServices via
    # the bundleId capability, and a freshly built Catalyst .app has never been
    # registered there. When the lookup fails, OneTimeSetUp fails for EVERY
    # test in the run with "The app representing com.microsoft.maui.uitests
    # could not be found", which reads like a harness outage rather than a
    # missing registration. Setting MAC_APP_PATH / options.App alone is NOT
    # sufficient, because the driver still resolves by bundleId first.
    #
    # BuildAndRunHostApp.ps1 already documents this and registers the bundle,
    # but the replication verification path never calls it: across the 22
    # cached runs that hit this error - every one of them catalyst, and every
    # one blocked with nothing published - `lsregister` appears zero times.
    # That is 14% of all cached catalyst runs lost to a missing one-line step.
    # It is intermittent rather than universal because agents are reused, so a
    # warm agent can still carry the registration from an earlier run.
    #
    # Registration failure is deliberately a warning and not a build failure:
    # on an agent where the bundle is already registered the run still works,
    # and turning that into a hard failure would break runs that pass today.
    $catalystAppPath = $null
    $searchPath = Split-Path -Parent $ProjectPath
    $artifactsDir = $null
    while ($searchPath -and -not $artifactsDir) {
        $testPath = Join-Path $searchPath "artifacts"
        if (Test-Path $testPath) { $artifactsDir = $testPath; break }
        $parent = Split-Path -Parent $searchPath
        if ($parent -eq $searchPath) { break }
        $searchPath = $parent
    }

    if ($artifactsDir) {
        $catalystAppPath = Get-ChildItem -Path $artifactsDir -Filter "*.app" -Recurse -ErrorAction SilentlyContinue |
            Where-Object {
                $_.FullName -match "$Configuration.*$macRid.*$projectName" -and
                $_.FullName -notmatch "[\\/]obj[\\/]"
            } |
            Select-Object -First 1
    }

    if ($catalystAppPath) {
        $env:MAC_APP_PATH = $catalystAppPath.FullName
        Write-Info "MacCatalyst app bundle: $($catalystAppPath.FullName)"

        # Probe both known lsregister locations so a differing framework
        # symlink layout on any agent macOS version cannot silently skip
        # registration.
        $lsregisterCandidates = @(
            "/System/Library/Frameworks/CoreServices.framework/Frameworks/LaunchServices.framework/Support/lsregister",
            "/System/Library/Frameworks/CoreServices.framework/Versions/A/Frameworks/LaunchServices.framework/Versions/A/Support/lsregister"
        )
        $lsregister = $lsregisterCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
        if ($lsregister) {
            & $lsregister -f $catalystAppPath.FullName 2>&1 | Out-Null
            # Report the actual result. Claiming success unconditionally is how
            # a failed registration turns into an unexplained Appium error four
            # attempts later.
            if ($LASTEXITCODE -eq 0) {
                # lsregister exiting 0 only means *an* app was registered. The
                # mac2 driver resolves by bundleId, so registering the wrong
                # bundle looks exactly like success here and surfaces four
                # attempts later as "The app representing <id> could not be
                # found". Build 15090165 spent a full catalyst run that way with
                # only Maui.Controls.Sample.Sandbox.app ever registered, and the
                # console said "Registered MacCatalyst app with LaunchServices".
                # Reading the identifier back turns that into one honest line.
                $registeredBundleId = ''
                $infoPlist = Join-Path $catalystAppPath.FullName 'Contents/Info.plist'
                if (Test-Path -LiteralPath $infoPlist) {
                    $registeredBundleId = (& /usr/libexec/PlistBuddy -c 'Print :CFBundleIdentifier' $infoPlist 2>$null)
                    if ($LASTEXITCODE -ne 0) { $registeredBundleId = '' }
                }
                $registeredBundleId = ([string]$registeredBundleId).Trim()

                if ($registeredBundleId) {
                    Write-Success "Registered MacCatalyst app with LaunchServices: $registeredBundleId"
                } else {
                    Write-Success "Registered MacCatalyst app with LaunchServices"
                    Write-Warn "Could not read CFBundleIdentifier from $infoPlist; the bundle the driver resolves is unverified"
                }

                if ($BundleId -and $registeredBundleId -and $registeredBundleId -ne $BundleId) {
                    Write-Warn ("Registered '$registeredBundleId' but the test driver resolves '$BundleId'; " +
                        "the Appium session will fail with 'The app representing $BundleId " +
                        "could not be found' until the right bundle is registered.")
                }
            } else {
                Write-Warn "lsregister exited $LASTEXITCODE; the mac2 driver may not resolve the bundle by id"
            }
        } else {
            Write-Warn "lsregister not found at any known path; skipping LaunchServices registration"
        }
    } else {
        Write-Warn "Could not locate the built MacCatalyst .app under $artifactsDir; skipping LaunchServices registration"
    }

    Write-Success "MacCatalyst app ready (runs on host Mac)"
    
    #endregion
} elseif ($Platform -eq "windows") {
    #region Windows Build
    
    Write-Step "Building $projectName for Windows..."

    if (-not $EnforceNetworkIsolation) {
        # The ordinary developer path remains unpackaged. Replication never uses
        # this branch because model-authored code may not execute with host trust.
        $buildArgs = @(
            $ProjectPath,
            "-f", $TargetFramework,
            "-c", $Configuration,
            "-p:RuntimeIdentifierOverride=win-x64",
            "-p:PublishReadyToRun=false",
            "-p:WindowsPackageType=None",
            "-p:_MauiReplicationUnpackaged=true"
        ) + $hostAppBuildProps
        if ($NoRestore) { $buildArgs += "--no-restore" }
        if ($Rebuild) { $buildArgs += "--no-incremental" }

        Write-Info "Build command: dotnet build $($buildArgs -join ' ')"
        $buildStartTime = Get-Date
        & dotnet build @buildArgs
        $buildExitCode = $LASTEXITCODE
        $buildDuration = (Get-Date) - $buildStartTime
        if ($buildExitCode -ne 0) {
            throw "Build failed with exit code $buildExitCode"
        }
        Write-Success "Build completed in $($buildDuration.TotalSeconds) seconds"
        Write-Success "Windows app ready (runs on host Windows)"
    } else {
        if (-not [OperatingSystem]::IsWindows()) {
            throw 'Windows replication AppContainer packaging requires a Windows host.'
        }
        if (-not $NoRestore) {
            throw 'Windows replication AppContainer packaging requires --no-restore.'
        }
        if ([string]::IsNullOrWhiteSpace($WindowsAppContainerManifestPath) -or
            [string]::IsNullOrWhiteSpace($WindowsPackageStatePath)) {
            throw 'Windows replication requires trusted manifest and package-state paths.'
        }

        $manifestPath = [IO.Path]::GetFullPath($WindowsAppContainerManifestPath)
        $statePath = [IO.Path]::GetFullPath($WindowsPackageStatePath)
        $null = Assert-ReplicationWindowsAppContainerManifest -Path $manifestPath
        $stateDirectory = Split-Path -Parent $statePath
        New-Item -ItemType Directory -Path $stateDirectory -Force | Out-Null
        $packageOutput = Join-Path $stateDirectory 'windows-appcontainer-package'
        if (Test-Path -LiteralPath $packageOutput) {
            Remove-Item -LiteralPath $packageOutput -Recurse -Force
        }
        New-Item -ItemType Directory -Path $packageOutput -Force | Out-Null
        Remove-Item -LiteralPath $statePath -Force -ErrorAction SilentlyContinue

        $signingCertificate = $null
        try {
            $graphBuildArgs = @(
                $ProjectPath,
                "-f", $TargetFramework,
                "-c", $Configuration,
                "-p:RuntimeIdentifierOverride=win-x64",
                "-p:PublishReadyToRun=false",
                "-p:WindowsPackageType=None",
                "-p:_MauiReplicationUnpackaged=true",
                "-p:SelfContained=true",
                "-p:BuildProjectReferences=true",
                "--no-restore"
            ) + $hostAppBuildProps
            if ($Rebuild) { $graphBuildArgs += "--no-incremental" }

            Write-Info "Prebuilding Windows project graph: dotnet build $($graphBuildArgs -join ' ')"
            & dotnet build @graphBuildArgs
            if ($LASTEXITCODE -ne 0) {
                throw "Unpackaged Windows graph build failed with exit code $LASTEXITCODE"
            }

            $signingCertificate = New-ReplicationWindowsSigningCertificate
            $buildArgs = @(
                $ProjectPath,
                "-f", $TargetFramework,
                "-c", $Configuration,
                "-p:RuntimeIdentifierOverride=win-x64",
                "-p:PublishReadyToRun=false",
                "-p:WindowsPackageType=MSIX",
                "-p:GenerateAppxPackageOnBuild=true",
                "-p:AppxPackageSigningEnabled=true",
                "-p:PackageCertificateThumbprint=$($signingCertificate.Thumbprint)",
                "-p:SelfContained=true",
                "-p:ExtraDefineConstants=PACKAGED",
                "-p:PackageManifest=$manifestPath",
                "-p:AppxPackageDir=$($packageOutput.TrimEnd('\', '/'))$([IO.Path]::DirectorySeparatorChar)",
                "-p:BuildProjectReferences=false",
                "--no-restore"
            ) + $hostAppBuildProps
            if ($Rebuild) { $buildArgs += "--no-incremental" }

            Write-Info "Build command: dotnet publish $($buildArgs -join ' ')"
            $buildStartTime = Get-Date
            & dotnet publish @buildArgs
            $buildExitCode = $LASTEXITCODE
            $buildDuration = (Get-Date) - $buildStartTime
            if ($buildExitCode -ne 0) {
                throw "Packaged Windows build failed with exit code $buildExitCode"
            }

            $packages = @(
                Get-ChildItem -LiteralPath $packageOutput -Filter '*.msix' -File -Recurse |
                    Where-Object {
                        $_.FullName -notmatch '(?i)[\\/]Dependencies[\\/]'
                    }
            )
            if ($packages.Count -ne 1) {
                throw "Expected exactly one Windows replication MSIX under '$packageOutput'; found $($packages.Count)."
            }
            $installedPackage = Install-ReplicationWindowsAppContainerPackage `
                -PackagePath $packages[0].FullName
            [ordered]@{
                schemaVersion = 1
                packageName = $installedPackage.Name
                packageFullName = $installedPackage.PackageFullName
                packageFamilyName = $installedPackage.PackageFamilyName
                packagePath = $installedPackage.PackagePath
                packageSha256 = $installedPackage.PackageSha256
                processId = 0
                mainWindowHandle = 0
            } | ConvertTo-Json -Depth 4 |
                Set-Content -LiteralPath $statePath -Encoding utf8NoBOM

            Write-Success "Packaged Windows AppContainer built, audited, and installed in $($buildDuration.TotalSeconds) seconds"
        } finally {
            if ($null -ne $signingCertificate) {
                Remove-ReplicationWindowsSigningCertificate `
                    -SigningCertificate $signingCertificate
            }
        }
    }
    
    #endregion
}
