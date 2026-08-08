#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verifies that tests catch the bug. Supports all test types and two verification modes.

.DESCRIPTION
    This script verifies that tests actually catch the issue. It supports two modes:
    
    VERIFY FAILURE ONLY MODE (no fix files detected):
    - Runs tests to verify they FAIL (proving they catch the bug)
    - Used when creating tests before writing a fix
    
    FULL VERIFICATION MODE (fix files detected):
    1. Reverting fix files to base branch
    2. Running tests WITHOUT fix (should FAIL)
    3. Restoring fix files
    4. Running tests WITH fix (should PASS)
    
    The script auto-detects which mode to use based on whether fix files are present.
    Fix files and test filters are auto-detected from the git diff (non-test files that changed).

    SUPPORTED TEST TYPES (auto-detected from changed files):
    - UITest:       Appium UI tests (TestCases.HostApp / TestCases.Shared.Tests)
    - UnitTest:     xUnit unit tests (*.UnitTests projects)
    - XamlUnitTest: XAML unit tests (Xaml.UnitTests)
    - DeviceTest:   Device tests (*.DeviceTests projects)

.PARAMETER Platform
    Target platform: "android", "ios", "catalyst" (MacCatalyst), or "windows"
    Required for all test types.

.PARAMETER TestType
    Explicit test type override. If not provided, auto-detected from changed files.
    Valid values: UITest, UnitTest, XamlUnitTest, DeviceTest

.PARAMETER TestFilter
    Test filter to pass to dotnet test (e.g., "FullyQualifiedName~Issue12345").
    If not provided, auto-detects from test files in the git diff.

.PARAMETER FixFiles
    (Optional) Array of file paths to revert. If not provided, auto-detects from git diff
    by excluding test directories. If no fix files are found, runs in verify failure only mode.

.PARAMETER BaseBranch
    Branch to revert files from. Auto-detected from PR if not specified.

.PARAMETER RequireFullVerification
    If set, the script will fail if it cannot run full verification mode
    (i.e., if no fix files are detected). Without this flag, the script will
    automatically run in verify failure only mode when no fix files are found.

.EXAMPLE
    # Auto-detect everything (test type, filter, platform)
    ./verify-tests-fail.ps1 -Platform android

.EXAMPLE
    # Verify unit tests (no platform needed)
    ./verify-tests-fail.ps1 -TestType UnitTest -TestFilter "Maui12345"

.EXAMPLE
    # Verify XAML unit tests
    ./verify-tests-fail.ps1 -TestType XamlUnitTest

.EXAMPLE
    # Full verification mode for UI tests
    ./verify-tests-fail.ps1 -Platform android -RequireFullVerification

.EXAMPLE
    # Specify everything explicitly
    ./verify-tests-fail.ps1 -Platform ios -TestType UITest -TestFilter "Issue12345" `
        -FixFiles @("src/Controls/src/Core/SomeFile.cs")
#>

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("android", "ios", "catalyst", "maccatalyst", "windows")]
    [string]$Platform,

    [Parameter(Mandatory = $false)]
    [string]$TestFilter,

    [Parameter(Mandatory = $false)]
    [string[]]$FixFiles,

    [Parameter(Mandatory = $false)]
    [string]$BaseBranch,

    [Parameter(Mandatory = $false)]
    [string]$PRNumber,

    [Parameter(Mandatory = $false)]
    [switch]$RequireFullVerification,

    [Parameter(Mandatory = $false)]
    [ValidateSet("UITest", "UnitTest", "XamlUnitTest", "DeviceTest")]
    [string]$TestType
)

$ErrorActionPreference = "Stop"
$RepoRoot = git rev-parse --show-toplevel

# Normalize platform name (accept both "catalyst" and "maccatalyst")
if ($Platform -eq "maccatalyst") {
    $Platform = "catalyst"
}

# ============================================================
# Platform-affinity gate: decide whether a PR's fix can possibly affect the
# gate's run platform. When EVERY changed *code* file is unambiguously
# platform-specific for a DIFFERENT platform (e.g. iOS/MacCatalyst handler
# files reviewed on the WINDOWS gate), the fix is a no-op on the gate platform,
# so the repro test necessarily "passes without the fix" — which the gate would
# otherwise misread as VERIFICATION FAILED ("test passed without fix"). That is
# a FALSE FAILED: nothing about the fix is verifiable on this platform, so the
# correct verdict is INCONCLUSIVE (non-blocking).
#
# CONSERVATIVE by design — only returns $true when we are CERTAIN the fix cannot
# touch the gate platform:
#   * any file with NO platform marker (shared/neutral) → affects ALL platforms → $false
#   * any single file whose affinity includes the gate platform            → $false
# so a real, verifiable failure is never masked.
#
# Affinity rules (folder OR filename-suffix OR net-<plat> PublicAPI path):
#   iOS      (.ios.cs, /iOS/, net-ios)                     → { ios, catalyst }  (.ios.cs compiles for MacCatalyst too)
#   Catalyst (.maccatalyst.cs, /MacCatalyst/, net-maccatalyst) → { catalyst }
#   Android  (.android.cs, /Android/, net-android)         → { android }
#   Windows  (.windows.cs, /Windows/, net-windows)         → { windows }
#   Tizen    (.tizen.cs, /Tizen/, net-tizen)               → { tizen }  (never a gate platform)
# $Platform is already normalized to one of: android | ios | catalyst | windows.
function Test-FixIrrelevantToPlatform {
    param([string[]]$FixFiles, [string]$Platform)

    # No fix files (verify-failure-only mode) or no known platform → cannot claim
    # irrelevance; fall back to the normal verdict so nothing is masked.
    if (-not $FixFiles -or @($FixFiles).Count -eq 0) { return $false }
    if ([string]::IsNullOrWhiteSpace($Platform)) { return $false }

    # Platform affinity is decided by the *product/source* code that gets toggled,
    # not by the test harness. Test-project files and snapshot baselines compile/run
    # on every platform, so if they were counted as "shared" they would force a
    # single-platform product fix (e.g. a [Windows]-only fix in /Platform/Windows/)
    # to look relevant on an unrelated gate platform. Skip them here; the safety
    # guard below keeps the normal verdict for a pure test/snapshot change.
    $sawProductFile = $false
    foreach ($file in $FixFiles) {
        if ([string]::IsNullOrWhiteSpace($file)) { return $false }
        $p = $file.Replace('\', '/').ToLowerInvariant()

        if ($p -match '/tests?/' -or $p -match '/snapshots?/' -or $p -match '\.(png|jpg|jpeg|gif|webp)$') { continue }

        $sawProductFile = $true

        $isIos   = ($p -match '\.ios\.(cs|xaml|fs|vb|razor)$')         -or ($p -match '/ios/')         -or ($p -match 'net-ios')
        $isCat   = ($p -match '\.maccatalyst\.(cs|xaml|fs|vb|razor)$') -or ($p -match '/maccatalyst/') -or ($p -match 'net-maccatalyst')
        $isDroid = ($p -match '\.android\.(cs|xaml|fs|vb|razor)$')     -or ($p -match '/android/')     -or ($p -match 'net-android')
        $isWin   = ($p -match '\.windows\.(cs|xaml|fs|vb|razor)$')     -or ($p -match '/windows/')     -or ($p -match 'net-windows')
        $isTizen = ($p -match '\.tizen\.(cs|xaml|fs|vb|razor)$')       -or ($p -match '/tizen/')       -or ($p -match 'net-tizen')

        # No platform marker at all → shared/neutral code → affects EVERY platform.
        if (-not ($isIos -or $isCat -or $isDroid -or $isWin -or $isTizen)) { return $false }

        $affinity = New-Object System.Collections.Generic.HashSet[string]
        if ($isIos)   { [void]$affinity.Add('ios'); [void]$affinity.Add('catalyst') }
        if ($isCat)   { [void]$affinity.Add('catalyst') }
        if ($isDroid) { [void]$affinity.Add('android') }
        if ($isWin)   { [void]$affinity.Add('windows') }
        if ($isTizen) { [void]$affinity.Add('tizen') }

        # This file DOES target the gate platform → the fix is verifiable here → not irrelevant.
        if ($affinity.Contains($Platform)) { return $false }
    }

    # Pure test/snapshot change (no product/source code) → cannot claim the fix is
    # irrelevant to this platform; keep the normal verdict so nothing is masked.
    if (-not $sawProductFile) { return $false }

    # Every product fix file is platform-specific for a platform OTHER than the gate platform.
    return $true
}

# ============================================================
# Strip GH/Copilot tokens from environment for the duration of a
# scriptblock that invokes PR-controlled code (dotnet test, MSBuild,
# host-app, device tests). Trusted metadata fetches via `gh` CLI
# (Detect-TestsInDiff, gh pr view) keep the token because they run
# OUTSIDE this wrapper. See .github/instructions/ci-copilot-pipeline-security.instructions.md.
# ============================================================
function Invoke-WithoutGhTokens {
    param([Parameter(Mandatory)][scriptblock]$ScriptBlock)
    $saved = @{}
    foreach ($n in @('GH_TOKEN','GITHUB_TOKEN','COPILOT_GITHUB_TOKEN')) {
        $saved[$n] = [Environment]::GetEnvironmentVariable($n)
        [Environment]::SetEnvironmentVariable($n, $null)
    }
    try { & $ScriptBlock }
    finally {
        foreach ($n in $saved.Keys) {
            [Environment]::SetEnvironmentVariable($n, $saved[$n])
        }
    }
}

# Platform is required for UI and device tests, optional for unit/XAML tests
if ($TestType -in @("UITest", "DeviceTest") -and -not $Platform) {
    throw "$TestType requires -Platform parameter (android, ios, catalyst, windows)."
}

# ============================================================
# Detect PR number if not provided
# ============================================================
if (-not $PRNumber) {
    # Try to get PR number from branch name (e.g., pr-27847)
    $currentBranch = git branch --show-current 2>$null
    if ($currentBranch -match "^pr-(\d+)") {
        $PRNumber = $matches[1]
        Write-Host "✅ Auto-detected PR #$PRNumber from branch name" -ForegroundColor Green
    } else {
        $foundPR = $false
        # Try gh cli - first try 'gh pr view' for current branch
        try {
            $prInfo = gh pr view --json number 2>$null | ConvertFrom-Json
            if ($prInfo -and $prInfo.number) {
                $PRNumber = $prInfo.number
                $foundPR = $true
                Write-Host "✅ Auto-detected PR #$PRNumber from gh cli (pr view)" -ForegroundColor Green
            }
        } catch {
            # gh pr view failed, will try fallback
        }
        
        # Fallback: search for PRs with this branch as head (works across forks)
        if (-not $foundPR) {
            try {
                $prList = gh pr list --head $currentBranch --json number --limit 1 2>$null | ConvertFrom-Json
                if ($prList -and $prList.Count -gt 0 -and $prList[0].number) {
                    $PRNumber = $prList[0].number
                    $foundPR = $true
                    Write-Host "✅ Auto-detected PR #$PRNumber from gh cli (pr list --head)" -ForegroundColor Green
                }
            } catch {
                # gh pr list also failed
            }
        }
        
        if (-not $foundPR) {
            throw "Could not auto-detect PR number. Please provide -PRNumber parameter."
        }
    }
}

# Set output directory based on PR number
$OutputDir = "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/gate/verify-tests-fail"
Write-Host "📁 Output directory: $OutputDir" -ForegroundColor Cyan

# ============================================================
# Import shared baseline script for merge-base and file detection
# ============================================================
$BaselineScript = Join-Path $RepoRoot ".github/scripts/EstablishBrokenBaseline.ps1"

# Import Test-IsTestFile and Find-MergeBase from shared script
. $BaselineScript

# Import the shared test detection script
$DetectTestsScript = Join-Path $RepoRoot ".github/scripts/shared/Detect-TestsInDiff.ps1"


# ============================================================
# Test type detection from changed files
# ============================================================

# Maps file path patterns to test types
$script:TestTypePatterns = @(
    @{ Pattern = "TestCases\.(Shared\.Tests|HostApp|Android\.Tests|iOS\.Tests|Mac\.Tests|WinUI\.Tests)"; Type = "UITest" }
    @{ Pattern = "Xaml\.UnitTests/"; Type = "XamlUnitTest" }
    @{ Pattern = "DeviceTests/"; Type = "DeviceTest" }
    @{ Pattern = "(?<!\w)UnitTests/|Graphics\.Tests/"; Type = "UnitTest" }
)

# Maps test types to their project paths (relative to repo root)
$script:UnitTestProjectMap = @{
    "Controls.Core.UnitTests"          = "src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj"
    "Controls.Xaml.UnitTests"          = "src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj"
    "Controls.BindingSourceGen.UnitTests" = "src/Controls/tests/BindingSourceGen.UnitTests/Controls.BindingSourceGen.UnitTests.csproj"
    "SourceGen.UnitTests"              = "src/Controls/tests/SourceGen.UnitTests/SourceGen.UnitTests.csproj"
    "Core.UnitTests"                   = "src/Core/tests/UnitTests/Core.UnitTests.csproj"
    "Essentials.UnitTests"             = "src/Essentials/test/UnitTests/Essentials.UnitTests.csproj"
    "Graphics.Tests"                   = "src/Graphics/tests/Graphics.Tests/Graphics.Tests.csproj"
    "Resizetizer.UnitTests"            = "src/SingleProject/Resizetizer/test/UnitTests/Resizetizer.UnitTests.csproj"
    "Compatibility.Core.UnitTests"     = "src/Compatibility/Core/tests/Compatibility.UnitTests/Compatibility.Core.UnitTests.csproj"
}

# Maps device test project keys to the -Project parameter of Run-DeviceTests.ps1
$script:DeviceTestProjectMap = @{
    "Controls.DeviceTests"             = "Controls"
    "Core.DeviceTests"                 = "Core"
    "Essentials.DeviceTests"           = "Essentials"
    "Graphics.DeviceTests"             = "Graphics"
    "MauiBlazorWebView.DeviceTests"    = "BlazorWebView"
}

function Get-GateDeviceTestConfiguration {
    param([string]$DevicePlatform)

    # Apple Release device-test builds run full ILLink trimming, making the A/B Gate much
    # slower and vulnerable to unrelated IL1012 trimmer crashes. Keep those on Debug.
    #
    # Android is the opposite: XHarness installs the APK directly, without Visual Studio's
    # fast-deployment side channel. A Debug APK can therefore launch with its managed payload
    # unavailable and crash immediately at instrumentation startup ("shortMsg=Process
    # crashed", no DOTNET log). Builds 14907169 and 14907191 reproduced this on every Debug
    # A/B attempt, then passed the identical category/class command in Release on the same
    # agent (276/276 Essentials and 16/16 Controls). Use the runner's proven Release default
    # for Android and Windows while preserving the Apple-specific Debug workaround.
    if ($DevicePlatform -in @("ios", "maccatalyst")) {
        return "Debug"
    }

    return "Release"
}

function Get-TestTypeFromFiles {
    <#
    .SYNOPSIS
        Detects which test type a set of changed files belong to.
    .DESCRIPTION
        Returns a hashtable with:
        - Type: UITest, UnitTest, XamlUnitTest, or DeviceTest
        - TestFiles: list of test files
        - Project: (for UnitTest/DeviceTest) which test project to run
    #>
    param([string[]]$ChangedFiles)

    $result = @{
        Type = $null
        TestFiles = @()
        Project = $null
        ProjectPath = $null
    }

    foreach ($file in $ChangedFiles) {
        if ($file -notmatch "\.cs$" -and $file -notmatch "\.xaml$") { continue }

        foreach ($mapping in $script:TestTypePatterns) {
            if ($file -match $mapping.Pattern) {
                $result.TestFiles += $file

                # First match wins for type (priority order in $TestTypePatterns)
                if (-not $result.Type) {
                    $result.Type = $mapping.Type
                } elseif ($result.Type -ne $mapping.Type) {
                    # Multiple test types detected — warn and keep the first (highest priority)
                    Write-Host "⚠️  Multiple test types detected ($($result.Type) and $($mapping.Type)). Using $($result.Type)." -ForegroundColor Yellow
                    Write-Host "   To override, use -TestType parameter explicitly." -ForegroundColor Yellow
                    continue
                }

                # Detect specific project for unit tests
                if ($mapping.Type -eq "UnitTest") {
                    foreach ($projName in $script:UnitTestProjectMap.Keys) {
                        if ($file -match [regex]::Escape($projName) -or $file -match ($projName -replace '\.', '/')) {
                            $result.Project = $projName
                            $result.ProjectPath = $script:UnitTestProjectMap[$projName]
                        }
                    }
                    # Fallback: infer project from directory structure
                    if (-not $result.Project) {
                        foreach ($projName in $script:UnitTestProjectMap.Keys) {
                            $projDir = Split-Path $script:UnitTestProjectMap[$projName]
                            if ($file -like "$projDir/*") {
                                $result.Project = $projName
                                $result.ProjectPath = $script:UnitTestProjectMap[$projName]
                                break
                            }
                        }
                    }
                }

                # Detect specific project for device tests
                if ($mapping.Type -eq "DeviceTest") {
                    foreach ($projName in $script:DeviceTestProjectMap.Keys) {
                        $projNamePattern = $projName -replace '\.', '[\./]'
                        if ($file -match $projNamePattern) {
                            $result.Project = $script:DeviceTestProjectMap[$projName]
                            break
                        }
                    }
                }

                break  # file matched a pattern, move to next file
            }
        }
    }

    return $result
}

# ============================================================
# Run tests based on detected type
# ============================================================
function Invoke-TestRun {
    <#
    .SYNOPSIS
        Runs tests using the appropriate runner for the detected test type.
    .DESCRIPTION
        Routes to BuildAndRunHostApp.ps1 for UI tests, dotnet test for unit/XAML tests,
        or Run-DeviceTests.ps1 for device tests. Uses Start-Emulator.ps1 for consistent
        device booting across all test types that need a platform.
    .OUTPUTS
        Returns the path to the test output log file.
    #>
    param(
        [string]$DetectedTestType,
        [string]$Filter,
        [string]$ClassFilter,
        [string[]]$Methods,
        [string]$DetectedProject,
        [string]$DetectedProjectPath,
        [string]$LogFile
    )

    # Boot device/simulator once for test types that need a platform.
    # Both BuildAndRunHostApp.ps1 and Run-DeviceTests.ps1 use Start-Emulator.ps1
    # internally, but we pre-boot here to ensure a consistent UDID is shared
    # across multiple test runs in the same gate session.
    if ($DetectedTestType -in @("UITest", "DeviceTest") -and -not $script:BootedDeviceUdid) {
        if (-not $Platform) {
            Write-Host "❌ $DetectedTestType tests require -Platform (android, ios, catalyst, windows)" -ForegroundColor Red
            exit 1
        }

        # catalyst/maccatalyst/windows run on host — no emulator needed
        $emulatorPlatform = switch ($Platform) {
            "catalyst" { $null }
            "windows"  { $null }
            default    { $Platform }
        }

        if ($emulatorPlatform) {
            if ($DeviceUdid) {
                $script:BootedDeviceUdid = $DeviceUdid
            } else {
                Write-Host "🔹 Booting $Platform device/simulator (shared across all test runs)..." -ForegroundColor Cyan
                $startEmulatorScript = Join-Path $RepoRoot ".github/scripts/shared/Start-Emulator.ps1"
                $emulatorParams = @{ Platform = $emulatorPlatform }
                $script:BootedDeviceUdid = & $startEmulatorScript @emulatorParams
                if ($LASTEXITCODE -ne 0) {
                    # A device/simulator that will not boot is a GATE-AGENT infrastructure
                    # failure: it happens BEFORE the PR's code is built or run, so it can NEVER
                    # be caused by the fix. Exit 3 (INCONCLUSIVE), NOT 1 (FAILED) — every other
                    # environment failure in this script exits 3, and Review-PR.ps1 maps 3 →
                    # INCONCLUSIVE deterministically. Relying on the caller's "missing report
                    # after a non-zero exit" heuristic (or a log-tail regex) to reclassify an
                    # exit-1 boot failure is fragile: a partial/prior report without the
                    # `ENV ERROR` marker would break the heuristic and surface a FALSE FAILED.
                    # Keep the literal "Failed to boot device" phrase so the caller's fallback
                    # diagnostics still recognise it. (PR #35668 iOS build 14719xxx: agent
                    # CoreSimulatorService wedge — "No iPhone simulator found" after the full
                    # create/enroll recovery — must be a non-blocking INCONCLUSIVE, not FAILED.)
                    Write-Host "❌ ENV ERROR: Failed to boot device — the $Platform simulator/emulator did not come up on the gate agent, so the PR's tests could not run (agent infrastructure, not a fix problem). Reporting INCONCLUSIVE." -ForegroundColor Yellow
                    exit 3
                }
            }
            Write-Host "✅ Device ready: $($script:BootedDeviceUdid)" -ForegroundColor Green
        } else {
            $script:BootedDeviceUdid = "host"
        }
    }

    switch ($DetectedTestType) {
        "UITest" {
            if (-not $Platform) {
                Write-Host "❌ UI tests require -Platform (android, ios, catalyst, windows)" -ForegroundColor Red
                exit 1
            }
            $buildScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"
            $uiParams = @{
                Platform   = $Platform
                TestFilter = $Filter
                Rebuild    = $true
            }
            if ($script:BootedDeviceUdid -and $script:BootedDeviceUdid -ne "host") {
                $uiParams.DeviceUdid = $script:BootedDeviceUdid
            }
            # Capture all output — includes build, deploy, and test results
            $scriptOutput = Invoke-WithoutGhTokens { & $buildScript @uiParams 2>&1 }
            $scriptOutput | Out-File -FilePath $LogFile -Force -Encoding utf8
            return $LogFile
        }

        "XamlUnitTest" {
            $projectPath = Join-Path $RepoRoot "src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj"
            Write-Host "🧪 Running XAML unit tests: $projectPath" -ForegroundColor Cyan
            Write-Host "   Filter: $Filter" -ForegroundColor Gray

            $testOutputFile = Join-Path $RepoRoot "CustomAgentLogsTmp/UnitTests/test-output.log"
            $testOutputDir = Split-Path $testOutputFile
            if (-not (Test-Path $testOutputDir)) {
                New-Item -ItemType Directory -Force -Path $testOutputDir | Out-Null
            }

            # The gate recompiles the MAUI product (Controls.Core, ...) FROM SOURCE via the
            # test project's P2P references, which re-runs the PublicAPI analyzer under the
            # repo-wide TreatWarningsAsErrors=true. A leak-fix PR that adds a finalizer (e.g.
            # #36605 ~SwipeView()) can surface RS0016/RS0017 as a build-breaking ERROR during
            # the revert→build→restore→build cycle even though the PR's own maui-pr build (a
            # REQUIRED check that separately enforces PublicAPI bookkeeping) is green — a false
            # FAILED. The gate verifies TEST BEHAVIOR, not API bookkeeping, so drop
            # warnings-as-errors here (matches Build-AndDeploy.ps1's deep-stage rationale).
            # Genuine CS-level compile ERRORS still fail the build.
            $testArgs = @(
                "test", $projectPath,
                "--configuration", "Debug",
                "--logger", "console;verbosity=normal",
                "-p:TreatWarningsAsErrors=false"
            )
            if ($Filter) {
                $testArgs += @("--filter", $Filter)
            }

            $scriptOutput = Invoke-WithoutGhTokens { & dotnet @testArgs 2>&1 }
            $scriptOutput | Out-File -FilePath $LogFile -Force -Encoding utf8
            return $LogFile
        }

        "UnitTest" {
            $projectPath = if ($DetectedProjectPath) {
                Join-Path $RepoRoot $DetectedProjectPath
            } else {
                # Fallback: try to find project from filter
                $null
            }

            if (-not $projectPath -or -not (Test-Path $projectPath)) {
                Write-Host "❌ Could not determine unit test project to run." -ForegroundColor Red
                Write-Host "   Detected project: $DetectedProject" -ForegroundColor Yellow
                Write-Host "   Path: $projectPath" -ForegroundColor Yellow
                exit 1
            }

            Write-Host "🧪 Running unit tests: $projectPath" -ForegroundColor Cyan
            Write-Host "   Filter: $Filter" -ForegroundColor Gray

            $testOutputFile = Join-Path $RepoRoot "CustomAgentLogsTmp/UnitTests/test-output.log"
            $testOutputDir = Split-Path $testOutputFile
            if (-not (Test-Path $testOutputDir)) {
                New-Item -ItemType Directory -Force -Path $testOutputDir | Out-Null
            }

            # Drop warnings-as-errors so the product recompile's PublicAPI analyzer
            # bookkeeping (RS0016/RS0017 on a PR-added finalizer/public symbol) can't
            # false-FAIL the gate — see the XAML block above and Build-AndDeploy.ps1.
            $testArgs = @(
                "test", $projectPath,
                "--configuration", "Debug",
                "--logger", "console;verbosity=normal",
                "-p:TreatWarningsAsErrors=false"
            )
            if ($Filter) {
                $testArgs += @("--filter", $Filter)
            }

            $scriptOutput = Invoke-WithoutGhTokens { & dotnet @testArgs 2>&1 }
            $scriptOutput | Out-File -FilePath $LogFile -Force -Encoding utf8
            return $LogFile
        }

        "DeviceTest" {
            if (-not $Platform) {
                Write-Host "❌ Device tests require -Platform (android, ios, maccatalyst, windows)" -ForegroundColor Red
                exit 1
            }

            $devicePlatform = if ($Platform -eq "catalyst") { "maccatalyst" } else { $Platform }
            if (-not $DetectedProject) {
                Write-Warning "Could not determine device test project — defaulting to 'Controls'."
            }
            $deviceProject = if ($DetectedProject) { $DetectedProject } else { "Controls" }

            $deviceTestScript = Join-Path $RepoRoot ".github/skills/run-device-tests/scripts/Run-DeviceTests.ps1"
            Write-Host "🧪 Running device tests: $deviceProject on $devicePlatform" -ForegroundColor Cyan
            Write-Host "   Filter: $Filter" -ForegroundColor Gray

            $testOutputFile = Join-Path $RepoRoot "CustomAgentLogsTmp/DeviceTests/test-output.log"
            $testOutputDir = Split-Path $testOutputFile
            if (-not (Test-Path $testOutputDir)) {
                New-Item -ItemType Directory -Force -Path $testOutputDir | Out-Null
            }

            $deviceConfiguration = Get-GateDeviceTestConfiguration -DevicePlatform $devicePlatform
            $deviceParams = @{
                Project       = $deviceProject
                Platform      = $devicePlatform
                # Preserve XHarness/ADB crash diagnostics with the Gate artifact. The default
                # `artifacts/log` directory is not published by this pipeline and is shared
                # across A/B retries, so persistent APP_CRASH runs previously told maintainers
                # to inspect adb-logcat/bugreport files that were both overwritten and absent
                # from every artifact. LogFile is unique for each without-fix/with-fix attempt;
                # writing its diagnostics beside it keeps every attempt under CustomAgentLogsTmp
                # and therefore inside CopilotLogs.
                OutputDirectory = "$LogFile.diagnostics"
                Configuration = $deviceConfiguration
                # Gate swaps the worktree from merge-base to PR HEAD while sharing one
                # artifacts/obj tree. Always rebuild the full P2P graph so a dependency
                # compiled for the baseline cannot be reused for the with-fix run.
                Rebuild = $true
            }
            Write-Host "   Configuration: $deviceConfiguration" -ForegroundColor Gray

            # Pass filter through — detection ensures it's Category= format
            if ($Filter) {
                $deviceParams.TestFilter = $Filter
            }

            # Additive class-level include narrowing (Android/iOS/MacCatalyst/Windows). When the gate
            # knows the PR's specific test class, run only that class instead of the whole
            # Category — so an unrelated crashing sibling test in the same category can't turn
            # the verdict INCONCLUSIVE. Empty on the main pipeline, so behaviour is unchanged.
            if ($ClassFilter) {
                $deviceParams.IncludeClasses = $ClassFilter
                Write-Host "   Include class: $ClassFilter" -ForegroundColor Gray
            }

            # Additive method-level narrowing. When the gate knows the PR's specific methods,
            # scope the post-hoc Windows/XHarness pass/fail tally to exactly those methods
            # within the class — so a pre-existing/flaky failure in an unrelated sibling
            # method cannot falsely redden the A/B verdict. Empty (or on a Windows
            # category-isolated run) leaves behaviour unchanged.
            if ($Methods -and $Methods.Count -gt 0) {
                $deviceParams.IncludeMethods = ($Methods -join ';')
                Write-Host "   Include method(s): $($Methods -join ', ')" -ForegroundColor Gray
            }

            if ($script:BootedDeviceUdid -and $script:BootedDeviceUdid -ne "host") {
                $deviceParams.DeviceUdid = $script:BootedDeviceUdid
            }

            $scriptOutput = Invoke-WithoutGhTokens { & $deviceTestScript @deviceParams 2>&1 }
            $scriptOutput | Out-File -FilePath $LogFile -Force -Encoding utf8
            return $LogFile
        }

        default {
            Write-Host "❌ Unknown test type: $DetectedTestType" -ForegroundColor Red
            exit 1
        }
    }
}

# ============================================================
# Run test with retry on environment errors
# ============================================================
function Test-IsWindowsDeviceNoResultsError {
    param(
        [string]$RunPlatform,
        [hashtable]$TestEntry,
        [string]$Message
    )

    return (
        $RunPlatform -eq 'windows' -and
        $TestEntry.Type -eq 'DeviceTest' -and
        -not [string]::IsNullOrWhiteSpace($Message) -and
        $Message.StartsWith('WINDOWS_DEVICE_TEST_NO_RESULTS:', [System.StringComparison]::Ordinal)
    )
}

function Test-IsWindowsDeviceTargetTimeoutError {
    param(
        [string]$RunPlatform,
        [hashtable]$TestEntry,
        [string]$Message
    )

    return (
        $RunPlatform -eq 'windows' -and
        $TestEntry.Type -eq 'DeviceTest' -and
        -not [string]::IsNullOrWhiteSpace($Message) -and
        $Message.StartsWith('WINDOWS_DEVICE_TEST_TARGET_TIMEOUT:', [System.StringComparison]::Ordinal)
    )
}

function Convert-WindowsBaselineNoResultsToFailure {
    param(
        [hashtable]$WithoutFixResult,
        [hashtable]$WithFixResult,
        [string]$RunPlatform,
        [string]$TestType
    )

    if ($RunPlatform -ne 'windows' -or $TestType -ne 'DeviceTest') { return $false }
    if (-not $WithoutFixResult.EnvError -or -not $WithoutFixResult.WindowsDeviceNoResults) { return $false }
    $attemptCount = [int]$WithoutFixResult.AttemptCount
    $noResultAttemptCount = [int]$WithoutFixResult.WindowsDeviceNoResultAttemptCount
    if (-not $WithoutFixResult.RetriesExhausted -or $attemptCount -lt 3) { return $false }
    if ($noResultAttemptCount -ne $attemptCount) { return $false }
    if ($WithFixResult.EnvError -or $WithFixResult.BuildError -or $WithFixResult.FilterMismatch -or -not $WithFixResult.Passed) { return $false }

    $evidence = $WithoutFixResult.Error
    $WithoutFixResult.EnvError = $false
    $WithoutFixResult.Passed = $false
    $WithoutFixResult.PassCount = 0
    $WithoutFixResult.FailCount = 1
    $WithoutFixResult.Failed = 1
    $WithoutFixResult.Total = 1
    $WithoutFixResult.Skipped = 0
    $WithoutFixResult.Error = $null
    $WithoutFixResult.WindowsBaselineAppExit = $true
    $WithoutFixResult.FailureReason = "Windows device-test app repeatedly exited before writing valid results in all $attemptCount baseline attempts; the same scoped test passed with the fix."
    $WithoutFixResult.FailureMessage = $evidence
    return $true
}

function Convert-WindowsTargetTimeoutToFailure {
    param(
        [hashtable]$Result,
        [hashtable]$CounterpartResult,
        [ValidateSet('WithoutFix', 'WithFix')][string]$Phase,
        [string]$RunPlatform,
        [string]$TestType
    )

    if ($RunPlatform -ne 'windows' -or $TestType -ne 'DeviceTest') { return $false }
    if (-not $Result.EnvError -or -not $Result.WindowsDeviceTargetTimeout) { return $false }

    $attemptCount = [int]$Result.AttemptCount
    $timeoutAttemptCount = [int]$Result.WindowsDeviceTargetTimeoutAttemptCount
    if (-not $Result.RetriesExhausted -or $attemptCount -lt 3) { return $false }
    if ($timeoutAttemptCount -ne $attemptCount) { return $false }

    # A repeated baseline timeout is trustworthy only after the same scoped target produces
    # a definitive with-fix result on the same agent. A repeated with-fix timeout is itself a
    # definitive failure: the Gate contract requires the target tests to complete and pass.
    if ($Phase -eq 'WithoutFix') {
        if (-not $CounterpartResult -or
            $CounterpartResult.EnvError -or
            $CounterpartResult.BuildError -or
            $CounterpartResult.FilterMismatch -or
            [int]$CounterpartResult.Total -le 0) {
            return $false
        }
    }

    $evidence = $Result.Error
    $Result.EnvError = $false
    $Result.Passed = $false
    $Result.PassCount = 0
    $Result.FailCount = 1
    $Result.Failed = 1
    $Result.Total = 1
    $Result.Skipped = 0
    $Result.Error = $null
    $Result.WindowsDeviceTargetTimeoutConfirmed = $true
    $Result.FailureReason = "Windows scoped target timed out in all $attemptCount attempts during the $Phase phase."
    $Result.FailureMessage = $evidence
    return $true
}

function Test-GateHasDefinitiveFailure {
    param(
        [int]$WithFixGenuineFailCount,
        [bool]$WithFixBuildError,
        [bool]$BaselineBuildError,
        [bool]$PrTestBuildError
    )

    return (
        $WithFixGenuineFailCount -gt 0 -or
        ($WithFixBuildError -and -not $BaselineBuildError) -or
        $PrTestBuildError
    )
}

function Invoke-TestRunWithRetry {
    param(
        [hashtable]$TestEntry,
        [string]$LogFile,
        [int]$MaxRetries = 3
    )

    $windowsDeviceNoResultAttemptCount = 0
    $windowsDeviceTargetTimeoutAttemptCount = 0
    for ($attempt = 1; $attempt -le $MaxRetries; $attempt++) {
        $logFileAttempt = if ($attempt -gt 1) { "$LogFile.attempt$attempt" } else { $LogFile }

        # Clear stale test output files before each run to prevent
        # reading results from the previous run
        $staleOutputPaths = @(
            (Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log"),
            (Join-Path $RepoRoot "CustomAgentLogsTmp/DeviceTests/test-output.log"),
            (Join-Path $RepoRoot "CustomAgentLogsTmp/UnitTests/test-output.log")
        )
        foreach ($stale in $staleOutputPaths) {
            if (Test-Path $stale) { Remove-Item $stale -Force }
        }

        try {
            $testOutputLog = Invoke-TestRun `
                -DetectedTestType $TestEntry.Type `
                -Filter $TestEntry.Filter `
                -ClassFilter $TestEntry.ClassFilter `
                -Methods $TestEntry.Methods `
                -DetectedProject $TestEntry.Project `
                -DetectedProjectPath $TestEntry.ProjectPath `
                -LogFile $logFileAttempt

            $result = Get-TestResultFromOutput -LogFile $testOutputLog -TestFilter $TestEntry.Filter
        } catch {
            $message = $_.Exception.Message
            $isWindowsNoResults = Test-IsWindowsDeviceNoResultsError `
                -RunPlatform $Platform `
                -TestEntry $TestEntry `
                -Message $message
            $isWindowsTargetTimeout = Test-IsWindowsDeviceTargetTimeoutError `
                -RunPlatform $Platform `
                -TestEntry $TestEntry `
                -Message $message

            if (-not $isWindowsNoResults -and -not $isWindowsTargetTimeout) {
                throw
            }

            if ($isWindowsNoResults) {
                $windowsDeviceNoResultAttemptCount++
            }
            if ($isWindowsTargetTimeout) {
                $windowsDeviceTargetTimeoutAttemptCount++
            }
            $message |
                Add-Content -LiteralPath $logFileAttempt -Encoding UTF8
            $result = @{
                Passed = $false
                EnvError = $true
                WindowsDeviceNoResults = $isWindowsNoResults
                WindowsDeviceTargetTimeout = $isWindowsTargetTimeout
                Error = $message
                FailCount = 0
                Failed = 0
                Total = 0
                Skipped = 0
            }
        }

        # Some environment outcomes are deterministic: a missing snapshot baseline cannot
        # appear on retry, and NETSDK1178 means this operating system cannot provide the
        # requested platform SDK pack. Return immediately so they flow to INCONCLUSIVE without
        # burning repeated full test runs.
        if (-not $result.EnvError -or $result.SnapshotBaselineMissing -or $result.UnsupportedWorkloadPackFailure) {
            return $result
        }

        if ($attempt -lt $MaxRetries) {
            Write-Host "  ⚠️ Environment error (attempt $attempt/$MaxRetries): $($result.Error) — retrying in 30s..." -ForegroundColor Yellow

            # Device test environment failures can leave the emulator/simulator in
            # a bad package-manager state for the next without/with-fix attempt.
            if ($result.Error -match "APP_LAUNCH_FAILURE|exit code.*83|app.*crash|package.*install|package.*operation|command timed out|XHarness exit 78|could not find/launch the app|InitialSetup/OneTimeSetup failed|OneTimeSetUp" -and $script:BootedDeviceUdid -and $script:BootedDeviceUdid -ne "host") {
                Write-Host "  🔄 Rebooting device ($($script:BootedDeviceUdid)) to recover from environment error: $($result.Error)" -ForegroundColor Yellow
                if ($Platform -in @("ios", "catalyst", "maccatalyst")) {
                    xcrun simctl shutdown $script:BootedDeviceUdid 2>$null
                    # Boot and block until the simulator has finished booting (services ready),
                    # not just powered on, before the next attempt.
                    xcrun simctl bootstatus $script:BootedDeviceUdid -b 2>$null
                } elseif ($Platform -eq "android") {
                    adb -s $script:BootedDeviceUdid reboot 2>$null
                    adb -s $script:BootedDeviceUdid wait-for-device 2>$null
                    # wait-for-device only waits for adbd to respond; the package manager,
                    # installer and launcher aren't ready until boot actually completes, so
                    # poll sys.boot_completed + bootanim (up to 180s) before retrying —
                    # otherwise the next attempt hits the same install/launch failure.
                    $bootDeadline = (Get-Date).AddSeconds(180)
                    while ((Get-Date) -lt $bootDeadline) {
                        $bootCompleted = (adb -s $script:BootedDeviceUdid shell getprop sys.boot_completed 2>$null | Out-String).Trim()
                        $bootAnim = (adb -s $script:BootedDeviceUdid shell getprop init.svc.bootanim 2>$null | Out-String).Trim()
                        if ($bootCompleted -eq '1' -and $bootAnim -eq 'stopped') { break }
                        Start-Sleep -Seconds 3
                    }
                }
            }

            Start-Sleep -Seconds 30
        } else {
            $result.AttemptCount = $attempt
            $result.RetriesExhausted = $true
            $result.WindowsDeviceNoResultAttemptCount = $windowsDeviceNoResultAttemptCount
            $result.WindowsDeviceTargetTimeoutAttemptCount = $windowsDeviceTargetTimeoutAttemptCount
            Write-Host "  ⚠️ Environment error persisted after $MaxRetries attempts: $($result.Error)" -ForegroundColor Yellow
            return $result
        }
    }
}

# ============================================================
# Run a test and, when the observed outcome does NOT match the expected one,
# re-run to confirm — making the gate DETERMINISTIC in the face of flaky tests.
#
# The gate contract is: the test(s) must FAIL without the fix and PASS with it.
# A single run can flip on a flaky test (a bug-reproducing test that passes once
# without the fix, or a real fix whose test fails once with it), which previously
# produced spurious "Tests PASSED without fix" / "FAILED with fix" gate blocks.
#
# Decision rule (credit the EXPECTED direction if ANY run confirms it):
#   - Expected 'Fail' (without-fix run): one FAIL proves the test reproduces the
#     bug, so we only trust an unexpected PASS after every confirmation run also
#     passes.
#   - Expected 'Pass' (with-fix run): one PASS proves the fix makes the test green,
#     so we only trust an unexpected FAIL after every confirmation run also fails.
# Env/build/filter errors are never "confirmed" here — they are handled upstream as
# INCONCLUSIVE so infra noise can't be mistaken for a flaky product outcome.
# ============================================================
function Invoke-TestRunConfirmed {
    param(
        [hashtable]$TestEntry,
        [string]$LogFile,
        [ValidateSet('Fail', 'Pass')][string]$Expected,
        [int]$MaxConfirm = 2
    )

    $result = Invoke-TestRunWithRetry -TestEntry $TestEntry -LogFile $LogFile

    # Only a clean pass/fail can be flaky; infra/build/filter problems are decided elsewhere.
    if ($result.EnvError -or $result.BuildError -or $result.FilterMismatch) { return $result }

    $matched = if ($Expected -eq 'Fail') { -not $result.Passed } else { $result.Passed }
    if ($matched) { return $result }

    $observed = if ($result.Passed) { 'PASS' } else { 'FAIL' }
    Write-Host "  🔁 Observed unexpected '$observed' (expected $Expected) — confirming with up to $MaxConfirm re-run(s) to rule out flakiness" -ForegroundColor Yellow
    Write-Log "  Unexpected '$observed' (expected $Expected) for $($TestEntry.TestName) — running up to $MaxConfirm confirmation re-run(s)"

    for ($c = 1; $c -le $MaxConfirm; $c++) {
        $confirmLog = "$LogFile.confirm$c"
        $r = Invoke-TestRunWithRetry -TestEntry $TestEntry -LogFile $confirmLog
        if ($r.EnvError -or $r.BuildError -or $r.FilterMismatch) {
            # No clean confirmation run available — don't let infra noise overturn the
            # original observation; keep looking.
            Write-Host "  ⚠️ Confirmation run $c hit an env/build error — ignoring for the flakiness check" -ForegroundColor Yellow
            continue
        }
        $rMatched = if ($Expected -eq 'Fail') { -not $r.Passed } else { $r.Passed }
        if ($rMatched) {
            Write-Host "  ✅ Confirmation run $c matched expected '$Expected' — test is FLAKY; crediting the expected outcome" -ForegroundColor Green
            Write-Log "  Confirmation run $c matched '$Expected' — $($TestEntry.TestName) is flaky; crediting expected outcome"
            $r.TestName = $TestEntry.TestName
            $r.TestType = $TestEntry.Type
            $r.Flaky = $true
            return $r
        }
    }

    Write-Host "  ❌ All $MaxConfirm confirmation run(s) still '$observed' — trusting the unexpected outcome as genuine" -ForegroundColor Red
    Write-Log "  All $MaxConfirm confirmation run(s) still '$observed' — $($TestEntry.TestName) verdict is genuine"
    $result.Confirmed = $true
    return $result
}

# ============================================================
# Parse test results from output (supports all test types)
# ============================================================
function Get-TestResultFromOutput {
    <#
    .SYNOPSIS
        Parses test results from a log file. Supports dotnet test, BuildAndRunHostApp,
        and device test (xharness) output formats.
    .DESCRIPTION
        When TestFilter is provided and the log contains device test output with
        [PASS]/[FAIL] markers, checks only whether the specific filtered test(s)
        passed — ignoring unrelated test failures.
    .OUTPUTS
        Hashtable with keys: Passed (bool), Total, PassCount (alias: Passed count),
        FailCount (alias: Failed count), Skipped, Error, FailureReason
    #>
    param(
        [string]$LogFile,
        [string]$TestFilter
    )

    if (-not (Test-Path $LogFile)) {
        return @{ Passed = $false; Error = "Test output log not found: $LogFile"; Total = 0; Failed = 0; Skipped = 0 }
    }

    $content = Get-Content $LogFile -Raw

    # Does this run contain a NATIVE shared-library load failure (e.g. libSkiaSharp /
    # libHarfBuzzSharp DllNotFoundException) because the GATE AGENT lacks the native runtime?
    # This is categorically environmental — a C# PR fix can neither add nor remove a native .so
    # — so image/rasterization tests (Resizetizer/Graphics) fail identically with AND without
    # the fix on a Linux (android) gate agent. Detected once here and used both by the dedicated
    # env return below (no test counts case) and to ANNOTATE the trust-the-counts FAIL return, so
    # the aggregation can exclude a test whose native-lib failure appears in BOTH states.
    # (build 14699033, PR #36653: ResizetizeImagesTests DllNotFoundException 'libSkiaSharp' in
    # both runs falsely counted as a with-fix failure → blocking FAILED, while the real repro
    # DpiPathTests correctly went FAIL→PASS.)
    $hasNativeLibLoadFailure = ($content -match '(?i)Unable to load (?:shared library|DLL)' -or
                                $content -match '(?is)DllNotFoundException.{0,120}Unable to load')

    # ── First, check if tests actually ran and produced results ──
    # This must come BEFORE env error checks because xharness can report
    # exit code 83 (APP_LAUNCH_FAILURE) even when tests ran successfully
    # (e.g., due to cleanup/teardown issues after test completion).

    # Device test output: check Passed/Failed counts from Run-DeviceTests.ps1
    # Format: "  Passed: 57\n  Failed: 0"
    # Run-DeviceTests.ps1 may retry internally, producing multiple Passed:/Failed: blocks.
    # Use the LAST block where tests actually ran (Passed > 0), so pass/fail counts
    # come from the same run. Taking MAX independently across blocks can mix results
    # from different runs (e.g., Run1: Passed=57,Failed=3 + Run2: Passed=60,Failed=0
    # would incorrectly yield Passed=60,Failed=3).
    $allPassMatches = [regex]::Matches($content, "(?m)^\s*Passed:\s*(\d+)")
    $allFailMatches = [regex]::Matches($content, "(?m)^\s*Failed:\s*(\d+)")

    if ($allPassMatches.Count -gt 0) {
        # Walk blocks in reverse to find the last one where tests actually ran
        $devicePassCount = 0
        $deviceFailCount = 0
        for ($i = $allPassMatches.Count - 1; $i -ge 0; $i--) {
            $p = [int]$allPassMatches[$i].Groups[1].Value
            $f = if ($i -lt $allFailMatches.Count) { [int]$allFailMatches[$i].Groups[1].Value } else { 0 }
            if ($p -gt 0 -or $f -gt 0) {
                $devicePassCount = $p
                $deviceFailCount = $f
                break
            }
        }
        $deviceTotal = $devicePassCount + $deviceFailCount

        Write-Host "  📊 Parsed test results: Passed=$devicePassCount Failed=$deviceFailCount Total=$deviceTotal (from $($allPassMatches.Count) result blocks)" -ForegroundColor Gray

        # If tests actually ran (passed > 0), trust the results over exit codes
        if ($devicePassCount -gt 0) {
            if ($deviceFailCount -gt 0) {
                # Some host-based MSBuild/XAML test classes exercise multiple target
                # platforms in one run. On a Linux/Android gate agent, the Android cases
                # can run while iOS/MacCatalyst cases fail before their assertions because
                # the corresponding SDK packs are unavailable on this OS (NETSDK1178:
                # "workload packs that do not exist ... build on another operating
                # system"). The aggregate still contains real pass/fail counts, so the
                # generic parser below would otherwise trust "Failed: N" and falsely blame
                # the fix.
                #
                # Downgrade ONLY when every failed xUnit case has its own NETSDK1178
                # signature. If even one failed case has a normal assertion/compiler
                # failure, preserve the genuine FAIL. Prefer xUnit's live [FAIL] blocks
                # because their output is isolated per theory case; fall back to VSTest's
                # final "Failed TestName [duration]" blocks for runners without live output.
                # (build 14907252, PR #37176: the fix made the Android case pass, while the
                # only two remaining failures were iOS/MacCatalyst NETSDK1178 on Linux.)
                $xunitFailurePattern = '(?ms)^\[xUnit\.net[^\r\n]*\]\s+[^\r\n]+\[FAIL\]\s*\r?\n.*?(?=^\[xUnit\.net[^\r\n]*\]\s+[^\r\n]+\[FAIL\]\s*\r?$|^\s*Failed\s+[^\r\n]+?\[[^\]\r\n]*(?:ms|s|m|h)\]\s*\r?$|^\s*Total tests:\s*\d+|\z)'
                $failedCaseBlocks = @([regex]::Matches($content, $xunitFailurePattern))
                if ($failedCaseBlocks.Count -eq 0) {
                    $summaryFailurePattern = '(?ms)^\s*Failed\s+[^\r\n]+?\[[^\]\r\n]*(?:ms|s|m|h)\]\s*\r?\n.*?(?=^\s*(?:Failed|Passed|Skipped)\s+[^\r\n]+?\[[^\]\r\n]*(?:ms|s|m|h)\]\s*\r?$|^\s*Total tests:\s*\d+|\z)'
                    $failedCaseBlocks = @([regex]::Matches($content, $summaryFailurePattern))
                }
                if ($failedCaseBlocks.Count -eq $deviceFailCount) {
                    $unsupportedWorkloadFailures = @($failedCaseBlocks | Where-Object { $_.Value -match '(?i)\bNETSDK1178\b' })
                    if ($unsupportedWorkloadFailures.Count -eq $deviceFailCount) {
                        $unavailablePacks = @([regex]::Matches($content, '(?i)workload packs that do not exist[^:]*:\s*([^\[\r\n]+)') |
                            ForEach-Object { $_.Groups[1].Value.Trim() } |
                            Where-Object { $_ } |
                            Sort-Object -Unique)
                        $packSuffix = if ($unavailablePacks.Count -gt 0) { " (unavailable: $($unavailablePacks -join ', '))" } else { "" }
                        Write-Host "  ⚠️  All $deviceFailCount failing test case(s) require platform workload packs unavailable on this gate host (NETSDK1178) — INCONCLUSIVE, not a fix failure" -ForegroundColor Yellow
                        return @{
                            Passed = $false; EnvError = $true; UnsupportedWorkloadPackFailure = $true
                            Error = "Gate host limitation: all $deviceFailCount failing test case(s) require .NET workload SDK packs unavailable on this operating system$packSuffix (NETSDK1178). Those cases could not execute, so the fix is unverifiable here; run them on a compatible host."
                            PassCount = $devicePassCount; FailCount = 0; Failed = 0
                            Total = $deviceTotal; Skipped = 0
                        }
                    }
                }

                # A run can report real passes AND failures where EVERY failure is a brand-new
                # VerifyScreenshot test whose baseline PNG isn't committed yet ("Baseline
                # snapshot not yet created"). That is NOT a genuine failure — the gate simply
                # has nothing to compare against — so it must be INCONCLUSIVE, not FAILED. This
                # check has to run HERE (inside the trust-the-counts path); otherwise a PR that
                # adds many new snapshot tests plus a couple that already have baselines (e.g.
                # PR #36448: Passed=2, Failed=30, all 30 baseline-missing) falls straight through
                # to the plain-FAIL return below and is falsely blocked. A real pixel DIFF
                # against an EXISTING baseline prints "Snapshot different than baseline" (NOT
                # "not yet created"), so baselineMissing < deviceFailCount and we correctly fall
                # through to a genuine failure.
                $baselineMissingCount = ([regex]::Matches($content, '(?i)Baseline snapshot not yet created')).Count
                if ($baselineMissingCount -ge $deviceFailCount) {
                    Write-Host "  ⚠️  All $deviceFailCount failing test(s) are new snapshots with no committed baseline — INCONCLUSIVE (gate cannot validate a brand-new VerifyScreenshot)" -ForegroundColor Yellow
                    return @{
                        Passed = $false; EnvError = $true; SnapshotBaselineMissing = $true
                        Error = "New snapshot test(s) — baseline image not yet created for $deviceFailCount test(s); the gate cannot validate brand-new VerifyScreenshot tests (baseline PNGs are added separately by a maintainer)"
                        FailCount = 0; Failed = 0; Total = $deviceTotal; Skipped = 0
                    }
                }
                # A snapshot "size differs" failure means the committed baseline PNG has DIFFERENT
                # pixel DIMENSIONS than the gate simulator's screenshot — i.e. the baseline was
                # captured on a different-sized device than the gate boots (e.g. an iPhone 16 Pro
                # 1206x2472 baseline vs the gate's pinned iPhone 11 Pro 1124x2286). A PR *code* fix
                # can never change screenshot dimensions, so this failure is environmental in BOTH
                # the without-fix and with-fix runs and the gate cannot A/B verify the test. When
                # every remaining failure is a size mismatch (alone or together with new-baseline
                # tests), report INCONCLUSIVE, never FAILED. This is distinct from a real pixel
                # DIFF ("N% difference"), which is a genuine visual regression and falls through.
                # (build 14850018, PR #37032: ChangingItemSpacing... baseline 1206x2472 vs gate
                # 1124x2286 — identical size mismatch in both legs, wrongly reported FAILED.)
                $sizeMismatchCount = ([regex]::Matches($content, '(?i)size differs\s*-\s*baseline is \d+x\d+ pixels?, actual is \d+x\d+ pixels?')).Count
                if ($sizeMismatchCount -gt 0 -and ($sizeMismatchCount + $baselineMissingCount) -ge $deviceFailCount) {
                    Write-Host "  ⚠️  All $deviceFailCount failing test(s) are snapshot SIZE mismatches (baseline captured on a different device size than the gate simulator) — INCONCLUSIVE (a code fix cannot change screenshot dimensions)" -ForegroundColor Yellow
                    return @{
                        Passed = $false; EnvError = $true; SnapshotSizeMismatch = $true
                        Error = "Snapshot size mismatch for $sizeMismatchCount test(s): the committed baseline PNG dimensions differ from the gate simulator's screenshot size (baseline captured on a different-sized device). A PR code fix cannot change screenshot dimensions, so the gate cannot A/B verify these tests — the baseline needs regenerating on the current device."
                        FailCount = 0; Failed = 0; Total = $deviceTotal; Skipped = 0
                    }
                }
                return @{
                    Passed = $false; FailCount = $deviceFailCount; Failed = $deviceFailCount
                    PassCount = $devicePassCount; Total = $deviceTotal; Skipped = 0
                    NativeLibLoadFailure = $hasNativeLibLoadFailure
                    FailureReason = "Device tests: $deviceFailCount of $deviceTotal failed"
                }
            }
            return @{
                Passed = $true; PassCount = $devicePassCount; Failed = 0
                FailCount = 0; Total = $deviceTotal; Skipped = 0
            }
        }
    }

    # ── Environment/infrastructure errors (only if no real test results above) ──
    $envErrorPatterns = @(
        @{ Pattern = "error ADB0010.*InstallFailedException"; Message = "App install failed (ADB broken pipe)" }
        @{ Pattern = "XHarness exit code:\s*83"; Message = "App failed to launch (XHarness exit 83)" }
        @{ Pattern = "XHarness exit code:\s*80"; Message = "App crashed during test run (XHarness exit 80 APP_CRASH)" }
        @{ Pattern = "XHarness exit code:\s*78"; Message = "Package installation failed (XHarness exit 78)" }
        @{ Pattern = "PACKAGE_INSTALLATION_FAILURE"; Message = "Package installation failed (XHarness package installation failure)" }
        @{ Pattern = "Waiting for command timed out: execution may be compromised"; Message = "Device package operation timed out" }
        @{ Pattern = "Application test run crashed"; Message = "App crashed during test run" }
        @{ Pattern = "SIGABRT.*load_aot_module"; Message = "App crashed during AOT loading" }
        @{ Pattern = "AppiumServerHasNotBeenStartedLocally"; Message = "Appium server failed to start" }
        @{ Pattern = "no such element.*could not be located"; Message = "Test element not found (app may not have loaded)" }
        # Appium/NUnit fixture setup failures: when [OneTimeSetUp]/InitialSetup can't establish
        # the Appium session or launch the app under test, EVERY test in the fixture fails before
        # a single assertion runs — the harness then throws "Call InitialSetup before accessing the
        # App property" in TearDown/SaveDeviceDiagnosticInfo. That is an infrastructure failure of
        # the test agent (Appium/mac2/WebDriverAgent flakiness or the app bundle not registering),
        # NOT a genuine product failure of the PR's fix. Without this the gate misreads a with-fix
        # session-start flake as "fix does not pass the tests" and blocks the PR (false FAILED,
        # e.g. MacCatalyst PR #27477 Issue19752: OneTimeSetUp UnknownErrorException "The app
        # representing com.microsoft.maui.uitests could not be found"). Classify as env/INCONCLUSIVE
        # so it is retried and, if persistent, surfaced as non-blocking.
        @{ Pattern = "Call InitialSetup before accessing the App property"; Message = "Appium app/session did not initialize (InitialSetup/OneTimeSetup failed — test agent could not start the Appium session)" }
        @{ Pattern = "The app representing .+ could not be found"; Message = "Appium could not find/launch the app under test (mac2/simulator driver could not resolve the app bundle)" }
        @{ Pattern = "OneTimeSetUp:\s*OpenQA\.Selenium"; Message = "Appium/Selenium error during fixture OneTimeSetUp (session/app setup failed before any test ran)" }
        # App CRASHED ON LAUNCH and the harness's crash-recovery relaunch attempts were exhausted:
        # the fixture's [OneTimeSetUp] then times out waiting for the app's navigation UI (e.g.
        # "Timed out waiting for Go To Test button to appear (the app did not recover after
        # crash-recovery attempts)"), so EVERY test in the fixture fails at setup before a single
        # assertion runs. The app under test never came up, so the gate verified NOTHING about the
        # fix — reporting FAILED here is a FALSE FAILED (build 14844563 / #35640 android: all 17
        # Material3CarouselViewFeatureTests failed identically at OneTimeSetUp on an agent that had
        # also just flaked the emulator boot). This env-pattern is only reachable when NO test
        # passed (Passed=0); if any test had launched+passed we would have trusted the counts above,
        # so it cannot mask a partial real failure. Classify as env/INCONCLUSIVE (retryable).
        @{ Pattern = "(?i)the app did not recover after crash-recovery attempts"; Message = "The test app crashed on launch and did not recover after the harness's crash-recovery relaunch attempts, so the fixture's OneTimeSetUp timed out and NO test could run (agent/app-launch infrastructure, not a fix problem). Retry on a fresh agent." }
    )
    foreach ($envErr in $envErrorPatterns) {
        if ($content -match $envErr.Pattern) {
            return @{ Passed = $false; EnvError = $true; Error = $envErr.Message; FailCount = 0; Failed = 0; Total = 0; Skipped = 0 }
        }
    }

    # ── Native shared-library load failure with NO parsed test counts (total load crash) ──
    # Reaches here only when the run produced no "Passed:/Failed:" block at all — i.e. the test
    # host crashed on native-lib load before any test ran. (The MIXED case — some tests pass and
    # some fail on the missing lib — is handled by the trust-the-counts FAIL return above, which
    # annotates NativeLibLoadFailure so the aggregation can exclude it when the failure appears in
    # BOTH the without-fix and with-fix runs.) A missing NATIVE library (libSkiaSharp,
    # libHarfBuzzSharp) is the GATE AGENT's problem, NOT the fix's: common for Resizetizer/Graphics
    # image tests on a Linux (android) gate agent with no SkiaSharp native runtime. The test COULD
    # NOT RUN, so nothing about the fix was verified → INCONCLUSIVE (env-class, non-blocking). SAFE:
    # a genuine "fix does not work" surfaces as an assertion diff, never as a missing native library
    # (build 14699033, PR #36653: libSkiaSharp DllNotFoundException).
    if ($hasNativeLibLoadFailure) {
        $nativeLib = $null
        $libMatch = [regex]::Match($content, "(?i)Unable to load (?:shared library|DLL) '([^']+)'")
        if ($libMatch.Success) { $nativeLib = $libMatch.Groups[1].Value }
        return @{
            Passed = $false; EnvError = $true; NativeLibLoadFailure = $true
            Error = if ($nativeLib) { "Native library '$nativeLib' could not be loaded on the gate agent (DllNotFoundException) — the test could not run, so the fix is unverifiable here" } else { "A native shared library could not be loaded on the gate agent (DllNotFoundException) — the test could not run" }
            FailCount = 0; Failed = 0; Total = 0; Skipped = 0
        }
    }

    # ── New snapshot/visual UI test with no committed baseline ──
    # A brand-new VerifyScreenshot test has no baseline PNG in the repo yet — maintainers
    # add the baseline in a follow-up commit after visually confirming it — so VisualTestUtils
    # throws "Baseline snapshot not yet created". This is NOT a fix failure: the gate simply
    # cannot validate a snapshot that has nothing to compare against, so a PR that ADDS new
    # snapshot tests would otherwise be falsely blocked with "Fix does not pass the tests"
    # (e.g. PR #36442's Border_StrokeDashArrayWithStrokeLineCap_* tests). Treat a missing
    # baseline as INCONCLUSIVE (env-class, non-blocking).
    # IMPORTANT: this matches a MISSING baseline only. A real pixel DIFF against an EXISTING
    # baseline (VisualTestFailedException without "not yet created") is a genuine failure and
    # must still be counted — it can be a real visual regression.
    if ($content -match '(?i)Baseline snapshot not yet created') {
        return @{
            Passed = $false; EnvError = $true; SnapshotBaselineMissing = $true
            Error = "New snapshot test — baseline image not yet created; the gate cannot validate a brand-new VerifyScreenshot test (the baseline PNG is added separately by a maintainer)"
            FailCount = 0; Failed = 0; Total = 0; Skipped = 0
        }
    }

    # ── Snapshot SIZE mismatch (baseline captured on a different-sized device) ──
    # "Snapshot different than baseline: X.png (size differs - baseline is WxH pixels, actual is
    # WxH pixels)" means the committed baseline PNG has different DIMENSIONS than the gate
    # simulator's screenshot — the baseline was captured on a different device size than the gate
    # boots (e.g. an iPhone 16 Pro 1206x2472 baseline vs the pinned iPhone 11 Pro 1124x2286). A PR
    # *code* fix can never change screenshot dimensions, so this failure is environmental in BOTH
    # the without-fix and with-fix runs and the gate cannot A/B verify the test → INCONCLUSIVE,
    # never FAILED. This is DISTINCT from a real pixel DIFF ("N% difference") against a same-size
    # baseline, which is a genuine visual regression and is NOT matched here. Reachable on the
    # UITest path (NUnit "Passed=False", no "Passed:/Failed:" counts). (build 14850018, PR #37032:
    # ChangingItemSpacingDoesNotShiftFirstItemOutOfView.png baseline 1206x2472 vs gate 1124x2286 —
    # identical size mismatch in both legs, wrongly reported FAILED.)
    if ($content -match '(?i)size differs\s*-\s*baseline is \d+x\d+ pixels?, actual is \d+x\d+ pixels?') {
        return @{
            Passed = $false; EnvError = $true; SnapshotSizeMismatch = $true
            Error = "Snapshot size mismatch: the committed baseline PNG dimensions differ from the gate simulator's screenshot size — the baseline was captured on a different-sized device. A PR code fix cannot change screenshot dimensions, so the gate cannot A/B verify this test; the baseline needs regenerating on the current device."
            FailCount = 0; Failed = 0; Total = 0; Skipped = 0
        }
    }

    # A build failure caused by the in-repo MSBuild BuildTasks up-to-date check misfiring is a
    # GATE INFRASTRUCTURE flake, NOT a code error, so it must be checked BEFORE the generic
    # build-error branch below. The gate's own git revert/restore cycle can change timestamps
    # and make Maui.InTree.targets report "required MSBuild tasks are not yet built or they are
    # out of date" even though the Build MSBuild Tasks step succeeded.
    #
    # Do NOT classify the standalone "MSBuild server unavailable ... falling back to an
    # in-process build" message as infrastructure. That fallback is benign and the in-process
    # build can still produce authoritative compiler errors or test results. Broad-matching it
    # hid real CS/WMC errors in build 14910465 and prevented the Gate from diagnosing stale
    # baseline outputs.
    $hasCodedBuildError = $content -match '(?im)\berror\s+[A-Z]{2,}\d+\b'
    if ($content -match '(?i)required MSBuild tasks are not yet built or they are out of date' -and
        -not $hasCodedBuildError) {
        return @{
            Passed = $false; EnvError = $true
            Error = "Gate infrastructure: the in-repo BuildTasks up-to-date check (Maui.InTree.targets) misfired, so the PR's code was never actually compiled. This is NOT a code build error — retry on a fresh agent."
            FailCount = 0; Failed = 0; Total = 0; Skipped = 0
        }
    }

    # A build failure from a MISSING .NET WORKLOAD on the gate agent (NETSDK1147 "the following
    # workloads must be installed: android/ios/maccatalyst ... run dotnet workload restore") is a
    # GATE INFRASTRUCTURE flake, never the PR author's code: the agent's workload restore did not
    # complete or did not persist into the gate's build, so the project can't build regardless of
    # the PR. Like the MSBuild-server flake above, it must be checked BEFORE the generic
    # build-error branch and classified as ENV ERROR (INCONCLUSIVE, retryable), never a code
    # BUILD ERROR / FAILED. (build 14824785, PR #36572: both without-fix AND with-fix legs failed
    # with 16× NETSDK1147 each and zero CS-errors — the android workload was simply absent.)
    if ($content -match '(?i)\bNETSDK1147\b' -or
        $content -match '(?i)the following workloads must be installed') {
        # Guard: if a GENUINE source compile error (C# CS#### or MAUI XAML MAUIX####) is ALSO
        # present, that is a real code problem and must not be masked by the workload-infra
        # classification — fall through to the generic build-error branch below.
        $hasRealCompileError = $content -match '(?im)\berror\s+(CS|MAUIX)\d+\b'
        if (-not $hasRealCompileError) {
            $missingWl = $null
            $wlMatch = [regex]::Match($content, '(?i)workloads must be installed:\s*([a-z0-9 ,\-]+)')
            if ($wlMatch.Success) { $missingWl = $wlMatch.Groups[1].Value.Trim() }
            $wlSuffix = if ($missingWl) { " (missing: $missingWl)" } else { "" }
            return @{
                Passed = $false; EnvError = $true
                Error = "Gate infrastructure: a required .NET workload was not installed on the gate agent$wlSuffix, so the project could not be built (NETSDK1147). The agent's ``dotnet workload restore`` did not take effect — this is NOT a code build error. Retry on a fresh agent."
                FailCount = 0; Failed = 0; Total = 0; Skipped = 0
            }
        }
    }

    # Check for build failures (before any test results)
    # Mark these explicitly with BuildError = $true so Write-MarkdownReport can
    # surface them as "Fix does not compile" instead of "Fix does not pass the tests".
    # Match coded build errors generally — `error <ABBR><NNNN>` — so the MAUI XAML
    # compiler (MAUIX####, e.g. MAUIX2017 "set multiple times"), MSBuild (MSB####),
    # C# (CS####), .NET SDK (NETSDK####), NuGet (NU####) and Android (XA####) diagnostics
    # are all caught. This matters on branches where an unrelated test fixture fails to
    # compile (e.g. the net11 Controls.Xaml.UnitTests MAUIX2017 baseline break): the whole
    # test assembly won't build, so the PR's own test can't run — that is INCONCLUSIVE, not
    # "the fix does not pass". The negative lookahead on `0 error(s)` avoids false positives
    # on MSBuild summary lines like "Build succeeded. 0 Error(s)".
    if ($content -match "Build FAILED" -or
        $content -match "Build failed with exit code" -or
        $content -match '(?im)\berror\s+[A-Z]{2,}\d+\b') {
        # Capture the first compile error so the diagnosis is concrete.
        $buildErrorExcerpt = $null
        $errMatch = [regex]::Match($content, '(?m)^.*\berror\s+[A-Z]{2,}\d+\b.*$')
        if ($errMatch.Success) {
            $excerpt = $errMatch.Value.Trim()
            if ($excerpt.Length -gt 200) { $excerpt = $excerpt.Substring(0, 200) + "..." }
            $buildErrorExcerpt = $excerpt
        }
        return @{
            Passed = $false; BuildError = $true
            Error = if ($buildErrorExcerpt) { "Build failed: $buildErrorExcerpt" } else { "Build failed before tests could run" }
            FailureMessage = $buildErrorExcerpt
            FailCount = 0; Failed = 0; Total = 0; Skipped = 0
        }
    }

    # Check for filter mismatch — the test runner ran successfully but the supplied
    # -filter expression matched zero test cases. Without this branch the gate
    # would treat "0 tests ran" as ENV ERROR (or worse, silently as a failed
    # test) — both misclassifications. The fix is to surface this as a separate
    # "FilterMismatch" outcome so Write-MarkdownReport can label it accurately.
    if ($content -match 'No test matches the given testcase filter' -or
        $content -match '(?im)^\s*Test count:\s*0\b') {
        $attemptedFilter = $null
        $fmMatch = [regex]::Match($content, "No test matches the given testcase filter '([^']+)'")
        if ($fmMatch.Success) { $attemptedFilter = $fmMatch.Groups[1].Value }
        elseif ($TestFilter) { $attemptedFilter = $TestFilter }
        return @{
            Passed = $false; FilterMismatch = $true
            Error = if ($attemptedFilter) { "Test filter '$attemptedFilter' matched 0 tests" } else { "Test filter matched 0 tests" }
            FailureMessage = $attemptedFilter
            FailCount = 0; Failed = 0; Total = 0; Skipped = 0
        }
    }

    # --- Device test output: [PASS]/[FAIL] markers from xharness ---
    # When TestFilter is specified and the log contains device test markers,
    # check only the filtered test results. Device tests run ALL tests regardless
    # of filter, so unrelated failures must be ignored.
    if ($TestFilter -and $content -match '\[PASS\]|\[FAIL\]') {
        $filterNames = $TestFilter -split '\|'
        $passedTests = @()
        $failedTests = @()

        foreach ($name in $filterNames) {
            $name = $name.Trim()
            if (-not $name) { continue }
            # Match lines like: [PASS] Share_MultipleFilesIntent_HasClipData
            #                    [FAIL] Share_MultipleFilesIntent_HasClipData
            if ($content -match "\[PASS\]\s+$([regex]::Escape($name))\b") {
                $passedTests += $name
            }
            elseif ($content -match "\[FAIL\]\s+$([regex]::Escape($name))\b") {
                $failedTests += $name
            }
        }

        $totalFound = $passedTests.Count + $failedTests.Count
        if ($totalFound -gt 0) {
            if ($failedTests.Count -gt 0) {
                return @{
                    Passed = $false
                    FailCount = $failedTests.Count
                    Failed = $failedTests.Count
                    PassCount = $passedTests.Count
                    Total = $totalFound
                    Skipped = 0
                    FailureReason = "Filtered test(s) failed: $($failedTests -join ', ')"
                }
            }
            return @{
                Passed = $true
                PassCount = $passedTests.Count
                Failed = 0
                FailCount = 0
                Total = $totalFound
                Skipped = 0
            }
        }
        # Filter specified but tests not found in output — fall through to general parsing
    }

    # --- dotnet test output ---
    # Check for "Test Run Failed" (dotnet test)
    if ($content -match "Test Run Failed") {
        $failCount = 0
        $passCount = 0
        $skipped = 0
        if ($content -match "^\s+Failed:\s*(\d+)") { $failCount = [int]$matches[1] }
        elseif ($content -match "Failed:\s*(\d+)") { $failCount = [int]$matches[1] }
        if ($content -match "^\s+Passed:\s*(\d+)") { $passCount = [int]$matches[1] }
        if ($content -match "^\s+Skipped:\s*(\d+)") { $skipped = [int]$matches[1] }

        # Extract failure details: test name, duration, error message
        $failureDetails = @()
        # Match: "Failed TestName [duration]" followed by "Error Message:" block
        $failedTestMatches = [regex]::Matches($content, '(?m)^\s*Failed\s+(\S+)\s*\[([^\]]+)\]')
        foreach ($m in $failedTestMatches) {
            $failureDetails += "$($m.Groups[1].Value) [$($m.Groups[2].Value)]"
        }
        # Extract error messages
        $errorMsgMatches = [regex]::Matches($content, '(?ms)Error Message:\s*\n\s*(.+?)(?=\n\s*Stack Trace:|\n\s*$|\n\s*\d+\))')
        $errorMessages = @()
        foreach ($m in $errorMsgMatches) {
            $msg = $m.Groups[1].Value.Trim()
            if ($msg.Length -gt 200) { $msg = $msg.Substring(0, 200) + "..." }
            $errorMessages += $msg
        }

        $failureReason = if ($failureDetails.Count -gt 0) { $failureDetails -join "; " } else { $null }
        $failureMessage = if ($errorMessages.Count -gt 0) { $errorMessages -join "; " } else { $null }

        return @{
            Passed = $false; FailCount = $failCount; PassCount = $passCount; Failed = $failCount
            Total = $failCount + $passCount + $skipped; Skipped = $skipped
            FailureReason = $failureReason; FailureMessage = $failureMessage
        }
    }

    # Check for "Test Run Successful" (dotnet test)
    if ($content -match "Test Run Successful") {
        $passCount = 0
        $skipped = 0
        if ($content -match "^\s+Passed:\s*(\d+)") { $passCount = [int]$matches[1] }
        elseif ($content -match "Total tests:\s*(\d+)") { $passCount = [int]$matches[1] }
        if ($content -match "^\s+Skipped:\s*(\d+)") { $skipped = [int]$matches[1] }
        return @{ Passed = $true; PassCount = $passCount; Failed = 0; Skipped = $skipped; Total = $passCount + $skipped }
    }

    # Check for failures first - but only if count > 0
    if ($content -match "Failed:\s*(\d+)") {
        $failCount = [int]$matches[1]
        if ($failCount -gt 0) {
            return @{ Passed = $false; FailCount = $failCount; Failed = $failCount; PassCount = 0; Total = $failCount; Skipped = 0 }
        }
    }

    # Check for passes
    if ($content -match "Passed:\s*(\d+)") {
        $passCount = [int]$matches[1]
        if ($passCount -gt 0) {
            return @{ Passed = $true; PassCount = $passCount; Failed = 0; Total = $passCount; Skipped = 0 }
        }
    }

    # Zero tests ran (Passed: 0, Failed: 0) — treat as env error, not success
    if ($content -match "Passed:\s*0" -and $content -match "Failed:\s*0") {
        return @{ Passed = $false; EnvError = $true; Error = "Zero tests ran (Passed: 0, Failed: 0)"; Total = 0; Failed = 0; Skipped = 0 }
    }

    return @{ Passed = $false; Error = "Could not parse test results"; Total = 0; Failed = 0; Skipped = 0 }
}


# ============================================================
# Auto-detect tests from changed files using shared detection
# ============================================================
function Limit-ExpensiveGateTests {
    <#
    .SYNOPSIS
        Caps the number of expensive (DeviceTest/UITest) entries the gate will
        verify so the two-phase A/B run stays within the AzDO task timeout.
    .DESCRIPTION
        Each DeviceTest/UITest is a full build+deploy+run, and the gate runs
        every detected test TWICE (STEP 2 without-fix + STEP 4 with-fix). A PR
        that touches many device-test files (e.g. a broad refactor) enumerates
        10+ expensive tests → the serial A/B runs blow past the task timeout →
        AzDO hard-kills the task → a "The task has timed out" FAILED verdict
        with no analysis (observed on build 14676353 / PR #36109: 11 device
        tests → 120-min timeout). This caps the expensive tests, prioritising
        the PR's own newly-added (fix-authored) regression tests. The Deep UI
        Tests stage still exercises the full category matrix. Cheap unit/XAML
        tests are never capped (they are fast). Caps are env-overridable via
        GATE_MAX_DEVICE_TESTS / GATE_MAX_UI_TESTS.
    #>
    param(
        [object[]]$Tests,
        [string[]]$AddedFiles = @()
    )
    if (-not $Tests -or @($Tests).Count -le 1) { return $Tests }

    $maxDevice = if ($env:GATE_MAX_DEVICE_TESTS) { [int]$env:GATE_MAX_DEVICE_TESTS } else { 2 }
    $maxUi     = if ($env:GATE_MAX_UI_TESTS)     { [int]$env:GATE_MAX_UI_TESTS }     else { 2 }

    $addedSet = @{}
    foreach ($f in @($AddedFiles)) { if ($f) { $addedSet[$f] = $true } }

    # Rank 0 = the test references a file newly added in this PR (very likely
    # the fix's own regression test); rank 1 = everything else. All rank-0
    # tests sort ahead of rank-1 tests, so fix-authored tests survive the cap.
    foreach ($t in $Tests) {
        $rank = 1
        foreach ($f in @($t.Files)) {
            if ($f -and $addedSet.ContainsKey($f)) { $rank = 0; break }
        }
        $t.GateRank = $rank
    }

    $cheap  = @($Tests | Where-Object { $_.Type -in @('UnitTest','XamlUnitTest') })
    $device = @($Tests | Where-Object { $_.Type -eq 'DeviceTest' } | Sort-Object { $_.GateRank })
    $ui     = @($Tests | Where-Object { $_.Type -eq 'UITest' }     | Sort-Object { $_.GateRank })

    $keptDevice = @($device | Select-Object -First $maxDevice)
    $keptUi     = @($ui     | Select-Object -First $maxUi)

    $dropped = (@($device).Count - @($keptDevice).Count) + (@($ui).Count - @($keptUi).Count)
    if ($dropped -gt 0) {
        Write-Host "⚠️  Gate work-cap: PR touches $(@($device).Count) device + $(@($ui).Count) UI test(s); the gate verifies the first $(@($keptDevice).Count) device + $(@($keptUi).Count) UI test(s) (fix-authored/newly-added tests prioritised) to stay within the task timeout. The remaining $dropped expensive test(s) are exercised by the Deep UI Tests stage." -ForegroundColor Yellow
    }

    # Cheap tests first (fast red/green signal), then the capped expensive set.
    return @($cheap + $keptDevice + $keptUi)
}

function Get-AutoDetectedTests {
    <#
    .SYNOPSIS
        Detects all tests in the current diff using the shared Detect-TestsInDiff.ps1 script.
    .OUTPUTS
        Array of test group hashtables from Detect-TestsInDiff.ps1
    #>
    param([string]$MergeBase)

    $params = @{}

    # Prefer PR number (GitHub API gives exact PR files, not polluted by branch diff)
    if ($PRNumber) {
        $params.PRNumber = $PRNumber
    } elseif ($MergeBase) {
        $changedFiles = git diff $MergeBase HEAD --name-only 2>$null
        if (-not $changedFiles -or $changedFiles.Count -eq 0) {
            $changedFiles = git diff --name-only 2>$null
            if (-not $changedFiles -or $changedFiles.Count -eq 0) {
                $changedFiles = git diff --cached --name-only 2>$null
            }
        }
        if ($changedFiles) {
            $params.ChangedFiles = $changedFiles
        }
    }

    # Fall back to PR number if no changed files from git diff
    if (-not $params.ContainsKey("ChangedFiles") -and $PRNumber) {
        $params.PRNumber = $PRNumber
    }

    if (-not [string]::IsNullOrWhiteSpace($Platform)) {
        $params.Platform = $Platform
    }

    $results = & $DetectTestsScript @params 6>$null

    # Bound the gate's workload to avoid the AzDO task hard-timeout on PRs that
    # touch many device-test files (see Limit-ExpensiveGateTests for details).
    # Newly-added files are the fix's own regression tests → prioritise them.
    $addedFiles = @()
    if ($MergeBase) {
        $addedFiles = @(git diff "$MergeBase" HEAD --diff-filter=A --name-only 2>$null | Where-Object { $_ })
    }
    $results = Limit-ExpensiveGateTests -Tests $results -AddedFiles $addedFiles
    return $results
}

# Keep the old function for backward compatibility but delegate to new detection
function Get-AutoDetectedTestFilter {
    param([string]$MergeBase)

    $tests = Get-AutoDetectedTests -MergeBase $MergeBase
    if (-not $tests -or $tests.Count -eq 0) {
        return $null
    }

    # Return the first test's info for single-test backward compatibility
    $first = $tests[0]
    return @{
        Filter = $first.Filter
        ClassNames = @($first.TestName)
        TestType = $first.Type
        Project = $first.Project
        ProjectPath = $first.ProjectPath
        AllTests = $tests
    }
}

# ============================================================
# Parse test results from log file
# ============================================================
function Get-TestResultFromLog {
    param([string]$LogFile)

    if (-not (Test-Path $LogFile)) {
        return @{ Passed = $false; Error = "Test output log not found: $LogFile" }
    }

    $content = Get-Content $LogFile -Raw

    # Check for failures first - but only if count > 0
    if ($content -match "Failed:\s*(\d+)") {
        $failCount = [int]$matches[1]
        if ($failCount -gt 0) {
            return @{ Passed = $false; FailCount = $failCount }
        }
    }

    # Check for passes
    if ($content -match "Passed:\s*(\d+)") {
        $passCount = [int]$matches[1]
        if ($passCount -gt 0) {
            return @{ Passed = $true; PassCount = $passCount }
        }
    }

    return @{ Passed = $false; Error = "Could not parse test results" }
}

# ============================================================
# AUTO-DETECT MODE: Find merge-base and fix files
# ============================================================

Write-Host ""
Write-Host "🔍 Detecting base branch and merge point..." -ForegroundColor Cyan

# Resolve the PR's ACTUAL base branch from its number before falling back to the
# closest-merge-base heuristic. Find-MergeBase's step-2 auto-detect calls
# `gh pr view` with NO number, which returns nothing in CI (the gate runs on a
# synthetic review branch that isn't PR-linked), so it drops to step 3 and picks
# whichever common branch is FEWEST commits away — almost always `main`, even for
# PRs that target `inflight/current`. Diffing against main's merge-base then makes
# the "fix files" set span ALL of inflight/current's divergence (200+ files) and
# flags files that exist on the real base as "new in PR", removing them and
# breaking the WITHOUT-fix build (build 14670709, #36274: BooleanBoxes.cs removed
# -> BooleanBoxesTests.cs CS0103 -> gate INCONCLUSIVE instead of a real verdict).
# Passing the explicit PR number makes `gh pr view` reliable; force-fetch the
# tracking ref so Find-MergeBase step 1 (origin/<base>) resolves it directly.
if (-not $BaseBranch -and $PRNumber) {
    $detectedBase = gh pr view $PRNumber --json baseRefName -q .baseRefName 2>$null
    if ($detectedBase) {
        git fetch origin "+$($detectedBase):refs/remotes/origin/$detectedBase" --no-tags 2>$null | Out-Null
        $BaseBranch = $detectedBase
        Write-Host "✅ Resolved PR #$PRNumber base branch: $BaseBranch (fetched origin/$BaseBranch)" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Could not resolve base branch for PR #$PRNumber; falling back to auto-detect" -ForegroundColor Yellow
    }
}

$baseInfo = Find-MergeBase -ExplicitBaseBranch $BaseBranch

if (-not $baseInfo) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║         ERROR: COULD NOT FIND MERGE BASE                  ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  Could not determine where this branch diverged from.     ║" -ForegroundColor Red
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Tried:                                                   ║" -ForegroundColor Red
    Write-Host "║  - PR metadata (gh pr view)                               ║" -ForegroundColor Red
    Write-Host "║  - Common base branches (main, net*.0, release/*)         ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "To fix, specify -BaseBranch explicitly:" -ForegroundColor Cyan
    Write-Host "  ./verify-tests-fail.ps1 -Platform android -BaseBranch main" -ForegroundColor White
    exit 1
}

$MergeBase = $baseInfo.MergeBase
$BaseBranchName = $baseInfo.BaseBranch

if ($baseInfo.TargetRepo) {
    Write-Host "✅ PR target: $($baseInfo.TargetRepo) ($BaseBranchName branch)" -ForegroundColor Green
} else {
    Write-Host "✅ Base branch: $BaseBranchName (via $($baseInfo.Source))" -ForegroundColor Green
}
Write-Host "✅ Merge base commit: $($MergeBase.Substring(0, 8))" -ForegroundColor Green
if ($baseInfo.Distance) {
    Write-Host "   ($($baseInfo.Distance) commits ahead of $BaseBranchName)" -ForegroundColor Gray
}

# Check for fix files (non-test files that changed since merge-base)
$DetectedFixFiles = @()
$changedFiles = git diff $MergeBase HEAD --name-only 2>$null

if ($changedFiles) {
    foreach ($file in $changedFiles) {
        if (-not (Test-IsTestFile $file)) {
            $DetectedFixFiles += $file
        }
    }
}

# Override with explicitly provided fix files
if ($FixFiles -and $FixFiles.Count -gt 0) {
    $DetectedFixFiles = $FixFiles
}

# Error if no fix files detected and RequireFullVerification is set
if ($DetectedFixFiles.Count -eq 0 -and $RequireFullVerification) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║         ERROR: NO FIX FILES DETECTED                      ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  Full verification mode required but no fix files found.  ║" -ForegroundColor Red
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Possible causes:                                         ║" -ForegroundColor Red
    Write-Host "║  - No non-test files changed since merge-base             ║" -ForegroundColor Red
    Write-Host "║  - All changes are in test directories                    ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "Debug info:" -ForegroundColor Yellow
    Write-Host "  Merge base: $MergeBase" -ForegroundColor Yellow
    Write-Host "  Base branch: $BaseBranchName" -ForegroundColor Yellow
    Write-Host "  Current branch: $(git rev-parse --abbrev-ref HEAD)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To fix, try one of:" -ForegroundColor Cyan
    Write-Host "  1. Specify fix files explicitly: -FixFiles @('path/to/fix.cs')" -ForegroundColor White
    Write-Host "  2. Remove -RequireFullVerification to run in failure-only mode" -ForegroundColor White
    exit 1
}

# If no fix files and not requiring full verification, run in "verify failure only" mode
if ($DetectedFixFiles.Count -eq 0) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║         VERIFY FAILURE ONLY MODE                          ║" -ForegroundColor Cyan
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
    Write-Host "║  No fix files detected - will only verify:                ║" -ForegroundColor Cyan
    Write-Host "║  1. Tests FAIL (proving they catch the bug)               ║" -ForegroundColor Cyan
    Write-Host "║                                                           ║" -ForegroundColor Cyan
    Write-Host "║  Use this mode when creating tests before writing a fix.  ║" -ForegroundColor Cyan
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""

    # Auto-detect tests if filter not provided
    $AllDetectedTests = @()

    if (-not $TestFilter) {
        Write-Host "🔍 Auto-detecting test filter from changed test files..." -ForegroundColor Cyan
        $filterResult = Get-AutoDetectedTestFilter -MergeBase $MergeBase

        if (-not $filterResult) {
            Write-Host "⚠️ No tests detected in this PR." -ForegroundColor Yellow
            Write-Host "   Searched for: UI tests, unit tests, XAML tests, device tests" -ForegroundColor Yellow
            Write-Host "   Consider adding tests via write-tests-agent." -ForegroundColor Cyan
            # Exit code 2 = no tests found (distinct from 1 = verification failed)
            exit 2
        }

        $AllDetectedTests = @($filterResult.AllTests)

        Write-Host "✅ Auto-detected $($AllDetectedTests.Count) test(s):" -ForegroundColor Green
        foreach ($t in $AllDetectedTests) {
            $icon = switch ($t.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }
            Write-Host "   $icon [$($t.Type)] $($t.TestName) (filter: $($t.Filter))" -ForegroundColor White
        }
    } else {
        $effectiveType = if ($TestType) { $TestType } else { "UITest" }
        $AllDetectedTests = @(@{
            Type = $effectiveType
            TestName = $TestFilter
            Filter = $TestFilter
            Project = $null
            ProjectPath = $null
        })
    }

    # Create output directory
    $OutputPath = Join-Path $RepoRoot $OutputDir
    New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

    $ValidationLog = Join-Path $OutputPath "verification-log.txt"
    # Failure-only mode must ALSO write verification-report.md. Without it the caller
    # (Review-PR.ps1) sees exit 0 and labels the gate "PASSED" while simultaneously
    # warning "verify-tests-fail.ps1 exited before writing a verification report" — a
    # confusing false-positive for test-only PRs. Define the path here and emit a report
    # on every exit path below.
    $FailureOnlyReport = Join-Path $OutputPath "verification-report.md"

    function Write-FailureOnlyReport {
        param(
            [string]$ReportStatus,   # "✅ PASSED" | "❌ FAILED" | "⚠️ INCONCLUSIVE"
            [array]$Results
        )
        $mergeBaseShort = if ($MergeBase -and $MergeBase.Length -ge 8) { $MergeBase.Substring(0, 8) } else { "$MergeBase" }
        $lines = @()
        $lines += "## Gate: Test Verification (Failure-Only Mode)"
        $lines += ""
        $lines += "**Result:** $ReportStatus"
        $lines += ""
        $lines += "This is a **test-only** change (no fix files detected in the diff), so the gate only verifies that the new/changed tests **fail** against the merge base — proving they reproduce the bug they target."
        $lines += ""
        $lines += "**Platform:** $($Platform.ToUpper())  "
        $lines += "**Merge base:** ``$mergeBaseShort``"
        $lines += ""
        $lines += "| Test | Type | Outcome |"
        $lines += "|------|------|---------|"
        foreach ($r in $Results) {
            $outcome = if ($r.EnvError) { "⚠️ ENV ERROR" }
                elseif ($r.BuildError) { "🛠️ BUILD ERROR" }
                elseif ($r.FilterMismatch) { "🔍 NO MATCH" }
                elseif (-not $r.Passed) { "FAIL ✅ (expected)" }
                else { "PASS ❌ (should fail)" }
            $lines += "| ``$($r.TestName)`` | $($r.TestType) | $outcome |"
        }
        $problem = @($Results | Where-Object { $_.Error })
        if ($problem.Count -gt 0) {
            $lines += ""
            $lines += "<details>"
            $lines += "<summary>Diagnostics</summary>"
            $lines += ""
            foreach ($r in $problem) {
                $lines += "- **$($r.TestName)**: ``$($r.Error)``"
            }
            $lines += ""
            $lines += "</details>"
        }
        # Machine-readable retry class (consumed by Review-PR.ps1's gate retry loop). A
        # missing snapshot baseline and an OS-incompatible NETSDK1178 workload pack are
        # DETERMINISTIC across retries on the same agent, so re-running can never flip the
        # outcome. Only TRANSIENT infra flakes (emulator/sim boot, ADB, Appium, XHarness
        # crash, install/timeout) are worth retrying. Emit skip-permanent ONLY when there is
        # at least one env error AND none of them are transient.
        $foEnv = @($Results | Where-Object { $_.EnvError })
        $foTransient = @($foEnv | Where-Object { -not ($_.SnapshotBaselineMissing -or $_.SnapshotEnvResidual -or $_.SnapshotBaselineUnresolved -or $_.UnsupportedWorkloadPackFailure) })
        $foClass = if ($foEnv.Count -gt 0 -and $foTransient.Count -eq 0) { 'skip-permanent' } else { 'retryable' }
        $lines += ""
        $lines += "<!-- GATE-RETRY-CLASS: $foClass -->"
        ($lines -join "`n") | Set-Content -Path $FailureOnlyReport -Encoding UTF8
        Write-Host ""
        Write-Host "📄 Markdown report saved to: $FailureOnlyReport" -ForegroundColor Cyan
    }

    # Initialize log
    "" | Set-Content $ValidationLog
    "=========================================" | Add-Content $ValidationLog
    "Verify Tests Fail (Failure Only Mode)" | Add-Content $ValidationLog
    "=========================================" | Add-Content $ValidationLog
    "Tests: $($AllDetectedTests.Count)" | Add-Content $ValidationLog
    "Platform: $Platform" | Add-Content $ValidationLog
    "MergeBase: $MergeBase" | Add-Content $ValidationLog
    "" | Add-Content $ValidationLog

    Write-Host ""
    Write-Host "🧪 Running $($AllDetectedTests.Count) test(s) (expecting them to FAIL)..." -ForegroundColor Cyan
    Write-Host ""

    # Run ALL detected tests
    $allResults = @()
    $testIndex = 0
    foreach ($testEntry in $AllDetectedTests) {
        $testIndex++
        $icon = switch ($testEntry.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }
        Write-Host "─────────────────────────────────────────────────" -ForegroundColor DarkGray
        Write-Host "$icon Test $testIndex/$($AllDetectedTests.Count): [$($testEntry.Type)] $($testEntry.TestName)" -ForegroundColor Cyan

        $sanitizedName = ($testEntry.TestName -replace '[^a-zA-Z0-9_\-\.]', '_')
        if ($sanitizedName.Length -gt 60) { $sanitizedName = $sanitizedName.Substring(0, 60) }
        $TestLog = Join-Path $OutputPath "test-failure-$sanitizedName.log"

        $testResult = Invoke-TestRunWithRetry -TestEntry $testEntry -LogFile $TestLog
        $testResult.TestName = $testEntry.TestName
        $testResult.TestType = $testEntry.Type
        $allResults += $testResult
    }

    # Evaluate results
    Write-Host ""
    Write-Host "=========================================="
    Write-Host "VERIFICATION RESULTS"
    Write-Host "=========================================="
    Write-Host ""

    $allFailed = ($allResults | Where-Object { $_.Passed }).Count -eq 0
    # Env/build/parse errors mean the gate could NOT verify the test's behaviour. Those
    # must surface as INCONCLUSIVE (exit 3), not FAILED, so infra/build flakes don't
    # masquerade as a broken test — mirroring the full-verification mode's classification.
    $hasEnvError   = @($allResults | Where-Object { $_.EnvError }).Count -gt 0
    $hasBuildError = @($allResults | Where-Object { $_.BuildError }).Count -gt 0
    $hasOtherError = @($allResults | Where-Object { $_.Error -and -not $_.EnvError -and -not $_.BuildError }).Count -gt 0

    # Show per-test results
    foreach ($r in $allResults) {
        $icon = switch ($r.TestType) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }
        if ($r.EnvError) {
            Write-Host "  $icon [$($r.TestType)] $($r.TestName): ⚠️ ENV ERROR — $($r.Error)" -ForegroundColor Yellow
        } elseif ($r.BuildError) {
            Write-Host "  $icon [$($r.TestType)] $($r.TestName): 🛠️ BUILD ERROR — $($r.Error)" -ForegroundColor Yellow
        } elseif ($r.Error) {
            Write-Host "  $icon [$($r.TestType)] $($r.TestName): ⚠️ ERROR — $($r.Error)" -ForegroundColor Yellow
        } elseif (-not $r.Passed) {
            Write-Host "  $icon [$($r.TestType)] $($r.TestName): FAILED ✅ (expected)" -ForegroundColor Green
        } else {
            Write-Host "  $icon [$($r.TestType)] $($r.TestName): PASSED ❌ (should fail!)" -ForegroundColor Red
        }
    }
    Write-Host ""

    if ($hasEnvError -or $hasBuildError -or $hasOtherError) {
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
        Write-Host "║              VERIFICATION INCONCLUSIVE ⚠️                  ║" -ForegroundColor Yellow
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Yellow
        Write-Host "║  Could not verify the test(s) — env/build/parse error.    ║" -ForegroundColor Yellow
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
        Write-FailureOnlyReport -ReportStatus "⚠️ INCONCLUSIVE" -Results $allResults
        # Exit 3 = inconclusive (build/env error). The report keeps the literal "ENV ERROR"
        # marker so the caller's retry loop can distinguish transient infra flakes.
        exit 3
    }

    if ($allFailed) {
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║              VERIFICATION PASSED ✅                       ║" -ForegroundColor Green
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
        Write-Host "║  All $($allResults.Count) test(s) FAILED as expected!                      ║" -ForegroundColor Green
        Write-Host "║  This proves the tests correctly reproduce the bug.       ║" -ForegroundColor Green
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
        Write-FailureOnlyReport -ReportStatus "✅ PASSED" -Results $allResults
        exit 0
    } else {
        $passedCount = ($allResults | Where-Object { $_.Passed }).Count
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
        Write-Host "║              VERIFICATION FAILED ❌                       ║" -ForegroundColor Red
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
        Write-Host "║  $passedCount/$($allResults.Count) test(s) PASSED but should FAIL!                   ║" -ForegroundColor Red
        Write-Host "║  Those tests don't reproduce the bug. Revise them!        ║" -ForegroundColor Red
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
        Write-FailureOnlyReport -ReportStatus "❌ FAILED" -Results $allResults
        exit 1
    }
}

# ============================================================
# FULL VERIFICATION MODE (fix files detected)
# ============================================================

Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║         FULL VERIFICATION MODE                            ║" -ForegroundColor Cyan
Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Cyan
Write-Host "║  Fix files detected - will verify:                        ║" -ForegroundColor Cyan
Write-Host "║  1. Tests FAIL without fix                                ║" -ForegroundColor Cyan
Write-Host "║  2. Tests PASS with fix                                   ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

$FixFiles = $DetectedFixFiles

Write-Host "✅ Fix files ($($FixFiles.Count)):" -ForegroundColor Green
foreach ($file in $FixFiles) {
    Write-Host "   - $file" -ForegroundColor White
}

# Auto-detect test filter from test files if not provided
$AllDetectedTests = @()

if (-not $TestFilter) {
    Write-Host "🔍 Auto-detecting test filter from changed test files..." -ForegroundColor Cyan
    $filterResult = Get-AutoDetectedTestFilter -MergeBase $MergeBase

    if (-not $filterResult) {
        Write-Host "⚠️ No tests detected in this PR." -ForegroundColor Yellow
        Write-Host "   Searched for: UI tests, unit tests, XAML tests, device tests" -ForegroundColor Yellow
        Write-Host "   Consider adding tests via write-tests-agent." -ForegroundColor Cyan
        # Exit code 2 = no tests found (distinct from 1 = verification failed)
        exit 2
    }

    $AllDetectedTests = @($filterResult.AllTests)

    Write-Host "✅ Auto-detected $($AllDetectedTests.Count) test(s):" -ForegroundColor Green
    foreach ($t in $AllDetectedTests) {
        $icon = switch ($t.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }
        Write-Host "   $icon [$($t.Type)] $($t.TestName) (filter: $($t.Filter))" -ForegroundColor White
    }
} else {
    # Explicit filter provided — use single test entry with given/detected type
    $effectiveType = if ($TestType) { $TestType } else { "UITest" }
    $AllDetectedTests = @(@{
        Type = $effectiveType
        TestName = $TestFilter
        Filter = $TestFilter
        Project = $null
        ProjectPath = $null
        Runner = switch ($effectiveType) {
            "UITest" { "BuildAndRunHostApp" }
            "DeviceTest" { "Run-DeviceTests" }
            default { "dotnet-test" }
        }
        NeedsPlatform = ($effectiveType -in @("UITest", "DeviceTest"))
    })
}

# Create output directory
$OutputPath = Join-Path $RepoRoot $OutputDir
New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null

$ValidationLog = Join-Path $OutputPath "verification-log.txt"
$WithoutFixLog = Join-Path $OutputPath "test-without-fix.log"
$WithFixLog = Join-Path $OutputPath "test-with-fix.log"
$MarkdownReport = Join-Path $OutputPath "verification-report.md"

function Write-Log {
    param([string]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logLine = "[$timestamp] $Message"
    Write-Host $logLine
    Add-Content -Path $ValidationLog -Value $logLine
}

# Does a set of build-error results point at one of the PR's OWN detected test files?
# The gate reverts only FIX files, never test files, so a test file always compiles at the
# PR's HEAD in BOTH the without-fix and with-fix states. When a self-contained compile error
# lives in the PR's test (e.g. PR #36170 added `using Microsoft.UI.Xaml.Controls;`, making
# `SelectionMode` ambiguous → CS0104), it fails identically in both states and would otherwise
# be mislabeled "pre-existing build failure (not the fix)". Matching the build-error text
# against a detected test's class name lets us attribute it to the PR (a real, blocking
# FAILED) instead of downgrading it to a non-blocking INCONCLUSIVE.
function Test-BuildErrorIsInDetectedTest {
    param([array]$Results, [array]$Tests)
    $errText = (@($Results) | Where-Object { $_.BuildError } | ForEach-Object { "$($_.FailureMessage) $($_.Error)" }) -join "`n"
    if (-not $errText -or -not $Tests) { return $false }
    $normalizedErrText = $errText -replace '\\', '/'
    foreach ($t in $Tests) {
        $base = (($t.TestName -split ' \(')[0]).Trim()
        if ($base -and $errText -match [regex]::Escape($base)) { return $true }

        # Device-test class names can carry a platform prefix that their source
        # files do not (for example Android_MediaPicker_Tests is declared in
        # MediaPicker_Tests.cs). Detect-TestsInDiff already provides the exact
        # changed test file, so matching its repository-relative path safely
        # establishes that the compile error came from the PR's own detected test.
        foreach ($testFile in @($t.Files)) {
            if ([string]::IsNullOrWhiteSpace([string]$testFile)) { continue }
            $normalizedTestFile = ([string]$testFile -replace '\\', '/').TrimStart('/')
            if ($normalizedTestFile -and $normalizedErrText -match [regex]::Escape($normalizedTestFile)) {
                return $true
            }
        }
    }
    return $false
}

# Condense a raw build/test log into an error-relevant excerpt for a PR comment.
# Dumping the full transcript (warnings, DLL output paths, ##vso commands) bloated the
# AI summary to tens of KB of noise (e.g. PR #34883 review = 56 KB) and buried the real
# failure. This keeps only lines that actually explain a failure — coded compiler/MSBuild
# errors, exceptions, stack frames, "Build FAILED" — capped to a small budget; if none
# match, it falls back to a short raw tail.
function Format-GateLogExcerpt {
    param(
        [string]$LogContent,
        [int]$MaxChars = 4000,
        [int]$RawTailChars = 1200,
        [int]$MaxLines = 40
    )
    if ([string]::IsNullOrWhiteSpace($LogContent)) { return @() }
    $out = @()
    # Strip ANSI/VT color escape codes up front — device-test runtime logs are full of them
    # (e.g. `^[[40m^[[37mdbug^[[39m`), which otherwise render as garbage in the PR comment.
    $ansiRx = "$([char]27)\[[0-9;]*[A-Za-z]"
    $logLines = ($LogContent -split "`r?`n") | ForEach-Object { $_ -replace $ansiRx, '' }
    # Lines that actually explain a failure.
    $errRx = '(:\s*error\s|error\s(CS|MSB|MT|NETSDK|XA|NU|CA|APT|AMM|IL)\d|MSBUILD\s*:\s*error|Build FAILED|##\[error\]|Unhandled exception|^\s*at\s+\S+\(|\.Exception:|Exception has been thrown)'
    # Noise to drop even when a line otherwise matches $errRx. Besides build warnings and
    # ##vso/dll-output lines, this drops iOS/mac simulator *runtime* teardown spam: benign
    # `dbug:`/`trce:` logging and Apple NSError descriptions ("... process: Error Domain=..."
    # / "Client not entitled" / "No such process found") that merely CONTAIN the word "Error"
    # and used to flood the summary with hundreds of identical lines (75 KB on PR #36109),
    # burying the real failure. None of these are ever a real compiler/test failure.
    $noiseRx = '(^\s*\d+ Warning\(s\)|->\s+\S+\.dll\s*$|##vso\[|:\s*warning\s|^\s*(dbug|trce):|Error Domain=|Failed to terminate process|Client not entitled|RBS(Service|Request)ErrorDomain|No such process found|NSUnderlyingError|runningboard)'
    $errLines = @($logLines | Where-Object { $_ -match $errRx -and $_ -notmatch $noiseRx })
    if ($errLines.Count -gt 0) {
        # De-dup (MSBuild repeats each error once per target framework).
        $seen = [System.Collections.Generic.HashSet[string]]::new()
        $uniq = @()
        foreach ($l in $errLines) { $k = $l.Trim(); if ($k -and $seen.Add($k)) { $uniq += $l } }
        # Keep the LAST lines within both a char AND a line budget (failures surface at the
        # end of a build); the line cap stops a single pathological run from ballooning.
        $buf = @(); $len = 0
        for ($i = $uniq.Count - 1; $i -ge 0; $i--) {
            $l = $uniq[$i]
            if ($len + $l.Length + 1 -gt $MaxChars -or $buf.Count -ge $MaxLines) { break }
            $buf = , $l + $buf; $len += $l.Length + 1
        }
        $out += "**Error-relevant lines** (filtered from the build log):"
        $out += ""
        $out += '```'
        $out += $buf
        $out += '```'
        return $out
    }
    # No coded error matched — show a short raw tail as a fallback, from the ANSI-stripped and
    # noise-filtered content (so the tail is not just simulator teardown spam).
    $cleanTail = ($logLines | Where-Object { $_ -notmatch $noiseRx -and -not [string]::IsNullOrWhiteSpace($_) }) -join [Environment]::NewLine
    if ([string]::IsNullOrWhiteSpace($cleanTail)) { return @() }
    if ($cleanTail.Length -gt $RawTailChars) {
        $out += "*(no coded error found; showing last $RawTailChars chars)*"
        $out += ""
        $out += '```'
        $out += $cleanTail.Substring($cleanTail.Length - $RawTailChars)
        $out += '```'
    } else {
        $out += '```'
        $out += $cleanTail
        $out += '```'
    }
    return $out
}

function Write-MarkdownReport {
    param(
        [bool]$VerificationPassed,
        [bool]$CompileCoupledVerified,
        [bool]$FailedWithoutFix,
        [bool]$PassedWithFix,
        [hashtable]$WithoutFixResult,
        [hashtable]$WithFixResult,
        [array]$WithoutFixResultsList,
        [array]$WithFixResultsList,
        [array]$Tests,
        [string]$ReportMergeBase,
        [string]$ReportPlatform,
        [string]$ReportBaseBranch,
        [array]$ReportRevertableFiles,
        [array]$ReportNewFiles
    )
    
    # Check for environment / build errors in results — a test that could not be built or
    # run never verified anything, so the gate is INCONCLUSIVE (not a genuine FAILED).
    $hasEnvError = ($WithoutFixResultsList | Where-Object { $_.EnvError }) -or ($WithFixResultsList | Where-Object { $_.EnvError })
    # Only a BASELINE (without-fix) build error, or an env error, leaves the gate genuinely
    # unable to verify → INCONCLUSIVE. A with-fix-ONLY build error (baseline compiles, the PR's
    # own fix does not) is a definitive FAILED — mirror the exit-code split (see $gateInfraError)
    # so the report headline and the Gate status chip don't frame a non-compiling fix as a
    # non-blocking infra flake.
    $baselineBuildError = @($WithoutFixResultsList | Where-Object { $_.BuildError }).Count -gt 0

    # A baseline (without-fix) build error located in the PR's OWN detected test file is only a
    # genuine FAILED when the test ALSO fails to build WITH the fix (a truly broken test that
    # breaks identically in both states). If the test build-errors WITHOUT the fix but compiles
    # and PASSES WITH it, the error is compile-coupling — the PR adds new API AND a new test
    # referencing it in the SAME project, so reverting the fix un-compiles the test through no
    # fault of its own — which is UNVERIFIABLE (INCONCLUSIVE), not FAILED. (PR #36521.)
    $prTestBuildError = $baselineBuildError -and (Test-BuildErrorIsInDetectedTest -Results $WithoutFixResultsList -Tests $Tests) -and (Test-BuildErrorIsInDetectedTest -Results $WithFixResultsList -Tests $Tests)

    # A FILTER MISMATCH (0 tests matched the -filter) on a deciding test means nothing was
    # verified, so the headline must read INCONCLUSIVE to match the exit code ($gateInfraError).
    # Apply the SAME guard as the exit-code logic: only downgrade to INCONCLUSIVE when NO genuine
    # failure remains with the fix, so a real FAIL→FAIL in another detected test is never masked
    # by an unrelated filter mismatch.
    $hasFilterMismatch = (@($WithoutFixResultsList) + @($WithFixResultsList) | Where-Object { $_.FilterMismatch }).Count -gt 0
    $reportWithFixGenuineFailCount = 0
    foreach ($gt in $Tests) {
        $woG = $WithoutFixResultsList | Where-Object { $_.TestName -eq $gt.TestName } | Select-Object -First 1
        $wG  = $WithFixResultsList    | Where-Object { $_.TestName -eq $gt.TestName } | Select-Object -First 1
        if (-not $woG -or -not $wG) { continue }
        $wGInc = $wG.EnvError -or $wG.BuildError -or $wG.FilterMismatch
        if ((-not $wGInc) -and (-not $wG.Passed)) { $reportWithFixGenuineFailCount++ }
    }
    $reportWithFixGenuineFail = $reportWithFixGenuineFailCount -gt 0
    $reportWithFixBuildError = @($WithFixResultsList | Where-Object { $_.BuildError }).Count -gt 0
    $reportDefinitiveFailure = Test-GateHasDefinitiveFailure `
        -WithFixGenuineFailCount $reportWithFixGenuineFailCount `
        -WithFixBuildError $reportWithFixBuildError `
        -BaselineBuildError $baselineBuildError `
        -PrTestBuildError $prTestBuildError

    # Platform-affinity FALSE-FAILED guard (mirror of the exit-code $fixPlatformMismatch):
    # when every changed code file targets a DIFFERENT platform than this gate, the fix is a
    # no-op here, so "passes without fix" is expected -> INCONCLUSIVE, not FAILED.
    $fixFilesForPlatform = @($ReportRevertableFiles) + @($ReportNewFiles)
    $fixPlatformMismatch = (-not $reportWithFixGenuineFail) -and (Test-FixIrrelevantToPlatform -FixFiles $fixFilesForPlatform -Platform $ReportPlatform)

    $status = if ($VerificationPassed) { "✅ PASSED" } elseif ($CompileCoupledVerified) { "✅ PASSED" } elseif ($reportDefinitiveFailure) { "❌ FAILED" } elseif ($hasEnvError -or $baselineBuildError -or $hasFilterMismatch -or $fixPlatformMismatch) { "⚠️ INCONCLUSIVE" } else { "❌ FAILED" }
    $mergeBaseShort = if ($ReportMergeBase -and $ReportMergeBase.Length -ge 8) { $ReportMergeBase.Substring(0, 8) } else { "$ReportMergeBase" }

    # When the gate PASSED under the relaxed "at least one test reproduces the bug, none
    # regress" rule but some tests pass in both states, note it so a PASS with an always-green
    # row in the table doesn't look inconsistent.
    $reproPairs = 0; $alwaysGreenPairs = 0
    foreach ($t in $Tests) {
        $woP = $WithoutFixResultsList | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
        $wP  = $WithFixResultsList    | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
        if (-not $woP -or -not $wP) { continue }
        $woInc = $woP.EnvError -or $woP.BuildError -or $woP.FilterMismatch
        $wInc  = $wP.EnvError  -or $wP.BuildError  -or $wP.FilterMismatch
        if ($woInc -or $wInc) { continue }
        if ((-not $woP.Passed) -and $wP.Passed) { $reproPairs++ }
        if ($woP.Passed -and $wP.Passed) { $alwaysGreenPairs++ }
    }
    $mixedPassNote = if ($VerificationPassed -and $reproPairs -gt 0 -and $alwaysGreenPairs -gt 0) {
        "✅ **Fix verified** — $reproPairs test(s) reproduce the bug (FAIL without the fix → PASS with it). $alwaysGreenPairs test(s) pass in both states and are not bug-reproducing; under the ""at least one test reproduces the bug and none regress"" rule they don't block the gate."
    } else { $null }

    # A brand-new snapshot test with no committed baseline drives the INCONCLUSIVE above via
    # its EnvError flag. Give it a dedicated, actionable headline instead of the generic
    # "environment error" framing so the reader knows the fix is fine — only the baseline is
    # missing.
    $snapshotBaselineMissing = (@($WithoutFixResultsList) + @($WithFixResultsList) | Where-Object { $_.SnapshotBaselineMissing }).Count -gt 0
    $snapshotEnvResidual = (@($WithFixResultsList) | Where-Object { $_.SnapshotEnvResidual }).Count -gt 0
    $snapshotBaselineUnresolved = (@($WithFixResultsList) | Where-Object { $_.SnapshotBaselineUnresolved }).Count -gt 0
    $unsupportedWorkloadPackFailure = (@($WithoutFixResultsList) + @($WithFixResultsList) | Where-Object { $_.UnsupportedWorkloadPackFailure }).Count -gt 0
    # Whether ANY env error is a "real" infra error (app crash / Appium flake / empty result)
    # rather than a snapshot-class one that already has its own dedicated $snapshotNote below.
    # A pure new-snapshot-no-baseline / snapshot-residual run must NOT also print the generic
    # "environment/infrastructure error … comment /review to retry" message: retrying never
    # creates the missing baseline, so that advice is wrong and makes an expected, non-failing
    # result look like an infra failure (PR #35491: new Shell.SetBackground API + a brand-new
    # VerifyScreenshot with no committed baseline → INCONCLUSIVE, correctly, but double-messaged).
    $nonSnapshotEnvError = @($WithoutFixResultsList + $WithFixResultsList | Where-Object {
        $_.EnvError -and -not ($_.SnapshotBaselineMissing -or $_.SnapshotEnvResidual -or $_.SnapshotBaselineUnresolved -or $_.UnsupportedWorkloadPackFailure)
    }).Count -gt 0
    $snapshotNote = if ($snapshotBaselineMissing) {
        "📷 **New snapshot test — no baseline yet** — the test calls ``VerifyScreenshot`` but its baseline image is not committed (brand-new snapshot tests get their baseline added separately). The gate cannot validate a snapshot with nothing to compare against, so this is **inconclusive, not a fix failure**. Download the ``snapshots-diff`` artifact, confirm the rendering, and commit the baseline PNG."
    } elseif ($snapshotEnvResidual) {
        "📷 **Environmental snapshot residual — not a fix failure** — with the fix applied, the only remaining ``VerifyScreenshot`` differences are no larger than the WITHOUT-fix run (the fix worsened no snapshot and added no new failing one) and are all below ~1%. The fix resolves the bug's visual difference; the residual is a constant cross-agent baseline offset (anti-aliasing / font hinting differ between the machine that captured the baseline and this agent), so this is **inconclusive, not a fix failure**. Regenerate the affected baseline PNG(s) on the target agent."
    } elseif ($snapshotBaselineUnresolved) {
        "📷 **Snapshot baseline not reproducible on this agent — inconclusive** — with the fix applied, the only remaining ``VerifyScreenshot`` failure is a LARGE diff (tens of percent) that is essentially UNCHANGED from the WITHOUT-fix run — the fix moved the pixel difference by under 1 percentage point. That is the signature of a cross-machine baseline mismatch: the committed baseline PNG was captured on a different machine and this CI agent renders the control (commonly the macOS TitleBar / window chrome) differently, swamping any fix effect. The gate cannot tell an environmental mismatch from an ineffective fix, so this is **inconclusive, not a confirmed fix failure** — inspect the ``snapshots-diff`` artifact manually and, if the render is correct, regenerate the baseline PNG on the target agent."
    } else { $null }
    $unsupportedWorkloadNote = if ($unsupportedWorkloadPackFailure) {
        "🧰 **Platform workload unavailable on this gate host — inconclusive** — every remaining failed test case stopped with ``NETSDK1178`` because it targets an SDK pack this operating system cannot provide (for example, iOS/MacCatalyst cases on the Linux Android gate). Those cases never executed their assertions, so this is **not a fix failure**. Re-running on the same host cannot help; verify the affected cases on a compatible platform agent."
    } else { $null }

    # A flaky GC memory-leak reclassification (with-fix leak FAIL→FAIL on a DoesNotLeak assert)
    # gets a dedicated headline so the reader knows the fix is likely fine — the WaitForGC check
    # is just non-deterministic and could not be verified by the gate.
    $leakFlaky = (@($WithFixResultsList) | Where-Object { $_.LeakFlaky }).Count -gt 0
    $leakNote = if ($leakFlaky) {
        "🧪 **Flaky GC memory-leak assertion — not a fix failure** — the only remaining with-fix failure is a ``DoesNotLeak`` test asserting via ``AssertionExtensions.WaitForGC`` (""Expected all references to be collected, but some are still alive""). That GC check is non-deterministic: even a correct fix can leave a reference briefly uncollected on a given run, so a persistent leak FAIL is **inconclusive, not proof the fix is broken**. Verify the leak fix manually (heap snapshot or repeated device-test runs)."
    } else { $null }

    # A platform-mismatch FALSE-FAILED (every fix file targets another platform) gets a
    # dedicated, actionable headline so the reader knows the fix is fine — it's just not
    # verifiable on THIS gate's platform.
    $platformMismatchNote = if ($fixPlatformMismatch) {
        "🌐 **Fix not relevant to the $($ReportPlatform.ToUpper()) gate** — every changed code file is platform-specific for a *different* platform (an iOS/MacCatalyst/Android/Windows-only change). On $($ReportPlatform.ToUpper()) the change is a no-op, so the repro test behaves identically **with and without** the fix and the gate cannot verify it here. This is **inconclusive, not a fix failure** — verify this PR on its own platform."
    } else { $null }

    # ─── Improvement #2: classify the failure mode so the headline matches the cause ───
    # Without this, every non-PASSED gate just says "tests did not behave as expected".
    # Map the without/with-fix outcomes per test into a concrete diagnosis the
    # downstream Try-Fix×4 stage and the human reader can act on.
    #
    # Reliability extensions:
    # - BuildError flag → headline says "Fix does not compile" (was conflated
    #   with "Fix does not pass the tests" because the test runner can't load
    #   an assembly that doesn't compile, so every test in it appears to fail).
    # - FilterMismatch flag → headline says "Test filter matched 0 tests"
    #   (was misclassified as ENV ERROR or as a generic FAIL because zero
    #   tests ran but exit code was non-zero).
    $failureClassification = $null
    if (($reportDefinitiveFailure -or -not $hasEnvError) -and -not $VerificationPassed -and -not $CompileCoupledVerified -and -not $fixPlatformMismatch -and $WithoutFixResultsList -and $WithFixResultsList) {
        # Build error in the with-fix run trumps every other classification — if
        # the fix doesn't compile, no per-test outcome is meaningful.
        $wBuildError    = @($WithFixResultsList    | Where-Object { $_.BuildError })
        $woBuildError   = @($WithoutFixResultsList | Where-Object { $_.BuildError })
        $wFilterMiss    = @($WithFixResultsList    | Where-Object { $_.FilterMismatch })
        $woFilterMiss   = @($WithoutFixResultsList | Where-Object { $_.FilterMismatch })
        $confirmedTargetTimeout = @($WithFixResultsList | Where-Object { $_.WindowsDeviceTargetTimeoutConfirmed }).Count -gt 0

        $woStates = @($WithoutFixResultsList | ForEach-Object { if ($_.EnvError) { "ENV" } elseif ($_.BuildError) { "BUILD" } elseif ($_.FilterMismatch) { "NOMATCH" } elseif ($_.Passed) { "PASS" } else { "FAIL" } })
        $wStates  = @($WithFixResultsList    | ForEach-Object { if ($_.EnvError) { "ENV" } elseif ($_.BuildError) { "BUILD" } elseif ($_.FilterMismatch) { "NOMATCH" } elseif ($_.Passed) { "PASS" } else { "FAIL" } })

        $allWoPass   = ($woStates | Where-Object { $_ -ne "PASS" }).Count -eq 0
        $allWoFail   = ($woStates | Where-Object { $_ -ne "FAIL" }).Count -eq 0
        $allWFail    = ($wStates  | Where-Object { $_ -ne "FAIL" }).Count -eq 0
        $hasRegression = $false
        # Regression: at least one test fixes (FAIL→PASS) AND at least one regresses (FAIL→FAIL)
        for ($i = 0; $i -lt $woStates.Count -and $i -lt $wStates.Count; $i++) {
            if ($woStates[$i] -eq "FAIL" -and $wStates[$i] -eq "FAIL") { $hasRegression = $true }
        }
        $hasFixedTest = $false
        $hasPassToFail = $false
        for ($i = 0; $i -lt $woStates.Count -and $i -lt $wStates.Count; $i++) {
            if ($woStates[$i] -eq "FAIL" -and $wStates[$i] -eq "PASS") { $hasFixedTest = $true }
            if ($woStates[$i] -eq "PASS" -and $wStates[$i] -eq "FAIL") { $hasPassToFail = $true }
        }

        if ($confirmedTargetTimeout) {
            $failureClassification = "🩺 **Fix does not complete the targeted Windows tests** — the scoped target timed out in every retry with the fix applied. This is deterministic blocking evidence even if another detected test hit an unrelated environment error."
        } elseif ($woBuildError.Count -gt 0) {
            # Baseline (without-fix / merge-base) does not build. The gate cannot establish a
            # working "before" state, so it can NEVER attribute the failure to the PR's fix —
            # even when the with-fix build ALSO errors (which is the common case: the SAME
            # pre-existing/toolchain failure hits both states, e.g. an ILLink IL1012 trimmer
            # crash). This branch MUST be evaluated before the with-fix branch so a
            # both-states build error is reported as a pre-existing/inconclusive failure, not
            # mislabeled "Fix does not compile" (which blames the PR for a baseline breakage).
            $woExcerpt = ($woBuildError | ForEach-Object { $_.FailureMessage } | Where-Object { $_ } | Select-Object -First 1)
            $woExcerptLine = if ($woExcerpt) { "`n> ``$woExcerpt``" } else { "" }
            if ($prTestBuildError) {
                $failureClassification = "🩺 **The PR's test does not compile** — the build error is in one of the PR's own test files, which the gate never reverts, so it fails identically without and with the fix. This is NOT a pre-existing/environment failure — the PR must fix its test (e.g. an ambiguous ``using`` / type collision). Investigate the PR's test code.$woExcerptLine"
            } elseif ($wBuildError.Count -gt 0) {
                $failureClassification = "🩺 **Pre-existing build failure (not the fix)** — both the without-fix baseline AND the with-fix build fail with a build error, so the PR's fix is NOT the cause. This is a broken ``main``/merge-base or a toolchain/environment failure (e.g. an ILLink IL1012 trimmer crash). The gate cannot verify anything; investigate the build environment rather than the PR.$woExcerptLine"
            } else {
                $newFileNote = if ($ReportNewFiles.Count -gt 0) { " Note: this PR ADDS $($ReportNewFiles.Count) new file(s), which the gate removes to reconstruct the pre-fix baseline; if the PR's own (never-reverted) test files reference types defined in those new files, the baseline cannot compile — that reflects a **new-feature PR the gate cannot isolate a ""before"" state for**, not necessarily a broken ``main``. The with-fix result below is the reliable signal." } else { "" }
                $failureClassification = "🩺 **Base branch does not compile** — the without-fix build failed. The gate's ""does the test fail without the fix"" check is unreliable here; this usually means ``main`` is broken or a merge-base file went missing.$newFileNote Investigate before trusting this gate.$woExcerptLine"
            }
        } elseif ($wBuildError.Count -gt 0) {
            # Reached only when the baseline builds cleanly but the PR's fix does NOT — a
            # genuine, PR-caused compile failure (FAILED, not inconclusive).
            $excerpt = ($wBuildError | ForEach-Object { $_.FailureMessage } | Where-Object { $_ } | Select-Object -First 1)
            $excerptLine = if ($excerpt) { "`n> ``$excerpt``" } else { "" }
            $failureClassification = "🩺 **Fix does not compile** — applying the PR's fix produces a build error before tests can run (the baseline builds fine). The earlier-than-test failure is the root cause; the per-test ❌ FAIL marks are downstream effects, not real test failures.$excerptLine"
        } elseif ($wFilterMiss.Count -gt 0 -or $woFilterMiss.Count -gt 0) {
            $missing = ($wFilterMiss + $woFilterMiss | ForEach-Object { $_.FailureMessage } | Where-Object { $_ } | Select-Object -First 1)
            $hint = if ($missing) { " — filter ``$missing`` matched 0 tests" } else { "" }
            $failureClassification = "🩺 **Test filter mismatch**$hint. The test runner produced zero results because no test class or method matched the filter. Common causes: the gate filter was derived from the file name but the actual test class is named differently, or the test was renamed/moved without updating the auto-detection. Verify the test class name matches what the gate is searching for."
        } elseif ($hasPassToFail) {
            $failureClassification = "🩺 **Fix introduces a regression** — at least one targeted test passes without the fix but fails with it. Unrelated environment errors in other detected tests do not make that PASS→FAIL result inconclusive."
        } elseif ($allWoPass) {
            $failureClassification = "🩺 **Test does not reproduce the bug** — ran the same in both states (PASS without fix, PASS with fix). The repro test is not exercising the issue. Strengthen the test before reviewing the fix."
        } elseif ($allWoFail -and $allWFail) {
            $failureClassification = "🩺 **Fix does not pass the tests** — every test still fails after applying the fix. The PR's change does not resolve the failure(s)."
        } elseif ($hasFixedTest -and $hasRegression) {
            $failureClassification = "🩺 **Regression in another test** — at least one test goes FAIL→PASS (fix works there), but another test FAILs both with and without the fix. The fix breaks a pre-existing or sibling test."
        } elseif ($hasRegression -and -not $hasFixedTest) {
            $failureClassification = "🩺 **Fix breaks tests** — one or more tests fail with the fix applied, and none of the failures are resolved by the fix."
        } elseif ($reportWithFixGenuineFail) {
            $failureClassification = "🩺 **Fix does not pass all targeted tests** — at least one with-fix run produced a genuine test failure. Unrelated environment errors in other detected tests do not make that blocking failure inconclusive."
        }
        # else: leave $failureClassification unset; the per-test table + Failure Details below tell the story.
    }
    elseif ($hasEnvError -and -not $VerificationPassed -and $nonSnapshotEnvError) {
        # The classification chain above is skipped when $hasEnvError is set, which
        # previously left the INCONCLUSIVE report with no explanation — only bare
        # "⚠️ ENV ERROR" cells (e.g. #36209: the Windows device-test app crashed
        # before writing its result XML). Surface a clear, honest cause so the reader
        # knows it is infrastructure, not a test/PR failure, and what to do next.
        # Guarded by $nonSnapshotEnvError: a pure snapshot-baseline case is explained by
        # $snapshotNote instead (retrying won't create the baseline), so don't double-message.
        $envExcerpt = @($WithoutFixResultsList + $WithFixResultsList |
            Where-Object { $_.EnvError -and $_.Error -and -not ($_.SnapshotBaselineMissing -or $_.SnapshotEnvResidual -or $_.SnapshotBaselineUnresolved) } | ForEach-Object { $_.Error } | Select-Object -First 1)
        $envExcerptLine = if ($envExcerpt) { "`n> ``$envExcerpt``" } else { "" }
        # An APP_CRASH (the app under test SIGABRT/exited mid-run) is NOT always a
        # transient infra flake: it can be deterministic and rooted in the code under
        # test or the runtime/native libraries it exercises. The gate already retries
        # env errors up to 3x (rebooting the device between attempts), so if it still
        # reached INCONCLUSIVE the crash PERSISTED across all attempts — telling the
        # author "not a problem with your PR, just retry" is then misleading (a plain
        # retry is unlikely to help, and it may be the code under test). Give an honest,
        # non-accusatory message for the crash case (build 14846070 / #36572: the
        # with-fix MediaPicker ProcessImage test SIGABRT'd on a fresh agent on all 3
        # attempts). Non-crash env errors (emulator/sim boot, Appium, empty result
        # file) keep the transient-flake "retry on a fresh agent" wording.
        $isAppCrash = $envExcerpt -and ($envExcerpt -match '(?i)APP_CRASH|crashed during test run|exit code 80')
        if ($isAppCrash) {
            $failureClassification = "🩺 **Could not verify — the app under test crashed (APP_CRASH).** The app SIGABRT'd / exited before the test produced a pass/fail, so the gate could not record a real result. The gate already retried on a rebooted device up to 3×; if it still reports this, the crash **persisted across every attempt** — a plain ``/review`` retry is unlikely to change it. This is **not necessarily** a problem with your PR, but it is also **not** a transient flake: the crash is either in the runtime/native libraries the test exercises **or** in the code under test. Download ``CopilotLogs`` and inspect the matching ``test-with*-*.log.diagnostics`` directory for the preserved ``adb-logcat`` / ``adb-bugreport`` native diagnostics before retrying.$envExcerptLine"
        } else {
            $failureClassification = "🩺 **Could not verify — environment/infrastructure error.** The gate ran the tests but hit an environment error (an emulator/simulator/Appium/XHarness flake, a device that would not boot, or an empty/invalid result file), so it could not record a real pass/fail. The ⚠️ ENV ERROR marks below are **infrastructure**, not test failures — this is **not** a problem with your PR. Comment ``/review`` to retry on a fresh agent.$envExcerptLine"
        }
    }

    $lines = @()
    $lines += "### Gate Result: $status"
    $lines += ""
    $platformDisplay = if ($ReportPlatform) { $ReportPlatform.ToUpper() } else { "N/A" }
    $lines += "**Platform:** $platformDisplay · **Base:** $ReportBaseBranch · **Merge base:** ``$mergeBaseShort``"
    if ($CompileCoupledVerified) {
        $lines += ""
        $lines += "✅ **Verified (new API / feature)** — this PR adds new API **and** a test that references it in the same project, so reverting the fix un-compiles the test: there is no valid ""fails without the fix"" baseline to establish (a *compile-coupled* baseline). The gate instead verified the fix by a clean **build + pass with the fix**, so this is a real PASS rather than a non-committal INCONCLUSIVE."
    }
    if ($mixedPassNote) {
        $lines += ""
        $lines += $mixedPassNote
    }
    if ($snapshotNote) {
        $lines += ""
        $lines += $snapshotNote
    }
    if ($unsupportedWorkloadNote) {
        $lines += ""
        $lines += $unsupportedWorkloadNote
    }
    if ($leakNote) {
        $lines += ""
        $lines += $leakNote
    }
    if ($platformMismatchNote) {
        $lines += ""
        $lines += $platformMismatchNote
    }
    if ($failureClassification) {
        $lines += ""
        $lines += $failureClassification
    }
    $lines += ""

    # ── Side-by-side per-test comparison table ──
    $lines += "| Test | Without Fix (expect FAIL) | With Fix (expect PASS) |"
    $lines += "|------|--------------------------|------------------------|"

    foreach ($t in $Tests) {
        $woResult = $WithoutFixResultsList | Where-Object { $_.TestName -eq $t.TestName }
        $wResult = $WithFixResultsList | Where-Object { $_.TestName -eq $t.TestName }

        # Without fix cell
        $woDur = if ($woResult.Duration) { "$([math]::Round($woResult.Duration.TotalSeconds))s" } else { "" }
        if ($woResult.SnapshotBaselineMissing) {
            $woCell = "📷 NEW SNAPSHOT (no baseline)"
        } elseif ($woResult.EnvError) {
            $woCell = "⚠️ ENV ERROR"
        } elseif ($woResult.BuildError) {
            $woCell = "🛠️ BUILD ERROR"
        } elseif ($woResult.FilterMismatch) {
            $woCell = "🔍 NO MATCH"
        } elseif (-not $woResult.Passed) {
            $woCell = "✅ FAIL — $woDur"
        } else {
            $woCell = "❌ PASS — $woDur"
        }

        # With fix cell
        $wDur = if ($wResult.Duration) { "$([math]::Round($wResult.Duration.TotalSeconds))s" } else { "" }
        if ($wResult.SnapshotBaselineMissing) {
            $wCell = "📷 NEW SNAPSHOT (no baseline)"
        } elseif ($wResult.EnvError) {
            $wCell = "⚠️ ENV ERROR"
        } elseif ($wResult.BuildError) {
            $wCell = "🛠️ BUILD ERROR"
        } elseif ($wResult.FilterMismatch) {
            $wCell = "🔍 NO MATCH"
        } elseif ($wResult.Passed) {
            $wCell = "✅ PASS — $wDur"
        } else {
            $wCell = "❌ FAIL — $wDur"
        }

        $icon = switch ($t.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "" } }
        $lines += "| $icon **$($t.TestName)** ``$($t.Filter)`` | $woCell | $wCell |"
    }

    # ── Per-test logs (collapsible) ──
    foreach ($t in $Tests) {
        $sanitizedName = ($t.TestName -replace '[^a-zA-Z0-9_\-\.]', '_')
        if ($sanitizedName.Length -gt 60) { $sanitizedName = $sanitizedName.Substring(0, 60) }

        $woResult = $WithoutFixResultsList | Where-Object { $_.TestName -eq $t.TestName }
        $wResult = $WithFixResultsList | Where-Object { $_.TestName -eq $t.TestName }
        $icon = switch ($t.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "" } }

        # Without fix log
        $woLogFile = Join-Path $OutputPath "test-without-fix-$sanitizedName.log"
        $woStatus = if ($woResult.EnvError) { "⚠️ ENV ERROR" } elseif ($woResult.BuildError) { "🛠️ BUILD ERROR" } elseif ($woResult.FilterMismatch) { "🔍 NO MATCH" } elseif (-not $woResult.Passed) { "FAIL ✅" } else { "PASS ❌" }
        $woDur = if ($woResult.Duration) { " · $([math]::Round($woResult.Duration.TotalSeconds))s" } else { "" }
        $lines += ""
        $lines += "<details>"
        $lines += "<summary>🔴 <strong>Without fix</strong> — $icon $($t.TestName): $woStatus$woDur</summary>"
        $lines += ""
        if (Test-Path $woLogFile) {
            $logContent = Get-Content $woLogFile -Raw -ErrorAction SilentlyContinue
            if ($logContent) {
                $lines += Format-GateLogExcerpt -LogContent $logContent
            } else {
                $lines += "*Log file empty*"
            }
        } else {
            $lines += "*No log file found*"
        }
        $lines += ""
        $lines += "</details>"

        # With fix log
        $wLogFile = Join-Path $OutputPath "test-with-fix-$sanitizedName.log"
        $wStatus = if ($wResult.EnvError) { "⚠️ ENV ERROR" } elseif ($wResult.BuildError) { "🛠️ BUILD ERROR" } elseif ($wResult.FilterMismatch) { "🔍 NO MATCH" } elseif ($wResult.Passed) { "PASS ✅" } else { "FAIL ❌" }
        $wDur = if ($wResult.Duration) { " · $([math]::Round($wResult.Duration.TotalSeconds))s" } else { "" }
        $lines += ""
        $lines += "<details>"
        $lines += "<summary>🟢 <strong>With fix</strong> — $icon $($t.TestName): $wStatus$wDur</summary>"
        $lines += ""
        if (Test-Path $wLogFile) {
            $logContent = Get-Content $wLogFile -Raw -ErrorAction SilentlyContinue
            if ($logContent) {
                $lines += Format-GateLogExcerpt -LogContent $logContent
            } else {
                $lines += "*Log file empty*"
            }
        } else {
            $lines += "*No log file found*"
        }
        $lines += ""
        $lines += "</details>"
    }

    # ── Failure details (shown directly — not collapsed) ──
    $failureLines = @()
    foreach ($r in $WithoutFixResultsList) {
        if ($r.BuildError) {
            $failureLines += "- 🛠️ **$($r.TestName)** without fix: build failed before tests could run"
            if ($r.FailureMessage) {
                $msg = if ($r.FailureMessage.Length -gt 300) { $r.FailureMessage.Substring(0, 300) + "..." } else { $r.FailureMessage }
                $failureLines += "  - ``$msg``"
            }
        } elseif ($r.FilterMismatch) {
            $failureLines += "- 🔍 **$($r.TestName)** without fix: test filter matched 0 tests"
            if ($r.FailureMessage) { $failureLines += "  - filter: ``$($r.FailureMessage)``" }
        } elseif ($r.Passed) {
            $failureLines += "- ❌ **$($r.TestName)** PASSED without fix (should fail) — tests don't catch the bug"
        }
        if ($r.EnvError) { $failureLines += "- ⚠️ **$($r.TestName)** without fix: ``$($r.Error)``" }
    }
    foreach ($r in $WithFixResultsList) {
        if ($r.BuildError) {
            $failureLines += "- 🛠️ **$($r.TestName)** with fix: build failed (fix does not compile)"
            if ($r.FailureMessage) {
                $msg = if ($r.FailureMessage.Length -gt 300) { $r.FailureMessage.Substring(0, 300) + "..." } else { $r.FailureMessage }
                $failureLines += "  - ``$msg``"
            }
        } elseif ($r.FilterMismatch) {
            $failureLines += "- 🔍 **$($r.TestName)** with fix: test filter matched 0 tests"
            if ($r.FailureMessage) { $failureLines += "  - filter: ``$($r.FailureMessage)``" }
        } elseif (-not $r.Passed -and -not $r.EnvError) {
            $failureLines += "- ❌ **$($r.TestName)** FAILED with fix (should pass)"
            if ($r.FailureReason) { $failureLines += "  - ``$($r.FailureReason)``" }
            if ($r.FailureMessage) {
                $msg = if ($r.FailureMessage.Length -gt 300) { $r.FailureMessage.Substring(0, 300) + "..." } else { $r.FailureMessage }
                $failureLines += "  - ``$msg``"
            }
        }
        if ($r.EnvError) { $failureLines += "- ⚠️ **$($r.TestName)** with fix: ``$($r.Error)``" }
    }

    if ($failureLines.Count -gt 0) {
        # Count actual failed tests (lines beginning with "- ❌"/"- ⚠️"/"- 🛠️"/"- 🔍")
        # to decide whether to collapse. Sub-bullets (FailureReason / FailureMessage)
        # start with two leading spaces so they don't match.
        $failedTestCount = @($failureLines | Where-Object { $_ -match '^- (❌|⚠️|🛠️|🔍)' }).Count
        # Threshold: if more than 5 tests failed, collapse the section so the gate
        # summary stays visible above the fold in PR comments. Below the threshold,
        # show details inline so reviewers don't need an extra click.
        $collapseFailures = $failedTestCount -gt 5

        $lines += ""
        if ($collapseFailures) {
            $lines += "<details>"
            $lines += "<summary>⚠️ Failure Details ($failedTestCount tests)</summary>"
            $lines += ""
        } else {
            $lines += "#### ⚠️ Failure Details"
            $lines += ""
        }
        $lines += ($failureLines -join "`n")
        if ($collapseFailures) {
            $lines += ""
            $lines += "</details>"
        }
    }

    # ── Fix files (collapsible) ──
    $lines += ""
    $lines += "<details>"
    $lines += "<summary>📁 Fix files reverted ($($ReportRevertableFiles.Count) files)</summary>"
    $lines += ""
    foreach ($f in $ReportRevertableFiles) {
        $lines += "- ``$f``"
    }
    if ($ReportNewFiles.Count -gt 0) {
        $lines += ""
        $lines += "**New files (not reverted):**"
        foreach ($f in $ReportNewFiles) {
            $lines += "- ``$f``"
        }
    }
    $lines += ""
    $lines += "</details>"

    # Machine-readable retry class (consumed by Review-PR.ps1's gate retry loop). A PERMANENT
    # env error — missing snapshot baseline, cross-machine baseline residual/mismatch, an
    # OS-incompatible workload pack, or a fix that only touches a different platform — is
    # DETERMINISTIC across retries on the same agent, so re-running it up to 3× just burns
    # ~16min/attempt for the identical INCONCLUSIVE (Windows
    # #36561/14687382 wasted ~48min retrying a "Baseline snapshot not yet created" 3×). Only
    # TRANSIENT infra flakes (emulator/sim boot, ADB, Appium, XHarness crash, install/timeout) are
    # worth retrying. Emit skip-permanent ONLY when there is a permanent signal AND no transient
    # infra env error remains to retry.
    $abEnv = @(@($WithoutFixResultsList) + @($WithFixResultsList) | Where-Object { $_.EnvError })
    $abTransient = @($abEnv | Where-Object { -not ($_.SnapshotBaselineMissing -or $_.SnapshotEnvResidual -or $_.SnapshotBaselineUnresolved -or $_.UnsupportedWorkloadPackFailure) })
    $abPermanentSignal = $snapshotBaselineMissing -or $snapshotEnvResidual -or $snapshotBaselineUnresolved -or $unsupportedWorkloadPackFailure -or $fixPlatformMismatch
    # NON-DIFFERENTIAL env failure: when the SAME test env-errors on BOTH the without-fix AND
    # with-fix runs (e.g. a device-test APP_CRASH that recurs identically on each side), the fix
    # cannot change the outcome — the gate is INCONCLUSIVE no matter what. Each side already
    # exhausted its per-test retry loop (3× with device reboots), so a whole-gate retry just
    # re-runs the identical both-sides crash for the identical INCONCLUSIVE. android #36616/
    # 14688269 burned ~92min running a persistent Category=Shell APP_CRASH through 3 full A/B
    # retries (6 runs × 13-20min) for the same verdict. Treat as permanent ONLY when EVERY
    # env-errored gate test is two-sided — a ONE-SIDED env error may be a transient flake that a
    # retry can clear, so those still fall through to the retry path.
    $abBothSidesEnv = $false; $abOneSidedEnv = $false
    foreach ($gt in $Tests) {
        $woG = $WithoutFixResultsList | Where-Object { $_.TestName -eq $gt.TestName } | Select-Object -First 1
        $wG  = $WithFixResultsList    | Where-Object { $_.TestName -eq $gt.TestName } | Select-Object -First 1
        if (-not $woG -or -not $wG) { continue }
        $woE = [bool]$woG.EnvError; $wE = [bool]$wG.EnvError
        if ($woE -and $wE) { $abBothSidesEnv = $true }
        elseif ($woE -or $wE) { $abOneSidedEnv = $true }
    }
    $abNonDifferential = $abBothSidesEnv -and (-not $abOneSidedEnv)
    $abClass = if ($status -eq "❌ FAILED") { 'definitive-failure' } elseif (($abPermanentSignal -and $abTransient.Count -eq 0) -or $abNonDifferential) { 'skip-permanent' } else { 'retryable' }
    $lines += ""
    $lines += "<!-- GATE-RETRY-CLASS: $abClass -->"

    ($lines -join "`n") | Set-Content -Path $MarkdownReport -Encoding UTF8
    Write-Host ""
    Write-Host "📄 Markdown report saved to: $MarkdownReport" -ForegroundColor Cyan
}

# Reuse the Get-TestResultFromLog function defined earlier

# Initialize log
"" | Set-Content $ValidationLog
Write-Log "=========================================="
Write-Log "Verify Tests Fail Without Fix"
Write-Log "=========================================="
Write-Log "Tests detected: $($AllDetectedTests.Count)"
foreach ($t in $AllDetectedTests) {
    Write-Log "  - [$($t.Type)] $($t.TestName) (filter: $($t.Filter))"
}
Write-Log "Platform: $Platform"
Write-Log "FixFiles: $($FixFiles -join ', ')"
Write-Log "BaseBranch: $BaseBranchName"
Write-Log "MergeBase: $MergeBase"
Write-Log ""

# ─────────────────────────────────────────────────────────────────────────────
# EXCLUDE CI-infrastructure fix files the gate cannot A/B-verify
# ─────────────────────────────────────────────────────────────────────────────
# For security the gate overlays TRUSTED (review-branch) copies of .github/scripts,
# .github/skills and eng/scripts over the worktree (Review-PR.ps1 Restore-TrustedScripts),
# so a PR that itself MODIFIES a file under those paths ALWAYS shows it as an uncommitted
# worktree change (trusted content != the PR's committed content). The uncommitted-fix-files
# guard below then aborted with a misleading "Uncommitted changes detected / run git add &&
# commit" error that the caller treats as a missing-report infra failure and retries 3× before
# a bare INCONCLUSIVE (build 14699515, #35156 catalyst: eng/scripts/{disable,enable}-notification-
# center.sh). Worse, those files are force-restored to the SAME trusted version in BOTH the
# without-fix and with-fix runs, so reverting them changes nothing — they are not A/B-testable.
# The same holds for pipeline/workflow definitions (eng/pipelines, .github/workflows): the gate
# runs on an already-checked-out pipeline, so editing those YAMLs in the worktree cannot alter
# the gate's own execution. Drop all of these from the fix-file set. If real product/test fix
# files remain, the A/B runs on those; if NONE remain the change is CI-infra-only and the gate
# has no without-fix baseline it can build -> a deterministic, non-retried INCONCLUSIVE that
# defers to the Deep UI Tests stage (which DOES exercise the pipeline/script change end-to-end).
$infraFixPrefixes = @('.github/scripts/', '.github/skills/', 'eng/scripts/', 'eng/pipelines/', '.github/workflows/')
$infraFixFiles = @()
$productFixFiles = @()
foreach ($f in $FixFiles) {
    $norm = $f -replace '\\', '/'
    $isInfra = $false
    foreach ($p in $infraFixPrefixes) { if ($norm -like "$p*") { $isInfra = $true; break } }
    if ($isInfra) { $infraFixFiles += $f } else { $productFixFiles += $f }
}
if ($infraFixFiles.Count -gt 0) {
    Write-Log "Excluding $($infraFixFiles.Count) CI-infrastructure fix file(s) the gate force-restores or cannot toggle (not A/B-verifiable):"
    foreach ($f in $infraFixFiles) { Write-Log "  (excluded) $f" }
    $FixFiles = @($productFixFiles)
    Write-Log "Remaining product/test fix file(s) after infra exclusion: $($FixFiles.Count)"
}

if ($infraFixFiles.Count -gt 0 -and $FixFiles.Count -eq 0) {
    Write-Host ""
    Write-Host "ℹ️  This PR only changes CI infrastructure (.github/scripts, .github/skills, eng/scripts, eng/pipelines, .github/workflows) that the gate force-restores to trusted versions or cannot toggle at run time. There is no without-fix baseline the gate can build for those paths (they are identical in both runs), so the change is not A/B-verifiable here — its impact is exercised by the Deep UI Tests stage. Reporting INCONCLUSIVE (deferred to Deep)." -ForegroundColor Yellow
    # Write a minimal report WITHOUT the 'ENV ERROR' token so Review-PR.ps1's gate loop breaks
    # immediately (no 3× retry) and classifies exit 3 as a clean, deterministic INCONCLUSIVE.
    try {
        if (-not (Test-Path $OutputPath)) { New-Item -ItemType Directory -Force -Path $OutputPath | Out-Null }
        $infraReport = @()
        $infraReport += "## Gate: Test Verification"
        $infraReport += ""
        $infraReport += "**Result:** ⚠️ INCONCLUSIVE"
        $infraReport += ""
        $infraReport += "**Platform:** $($Platform.ToUpper())"
        $infraReport += ""
        $infraReport += "This PR only changes CI infrastructure the gate force-restores to trusted versions or cannot toggle at run time:"
        $infraReport += ""
        foreach ($f in $infraFixFiles) { $infraReport += "- ``$f``" }
        $infraReport += ""
        $infraReport += "These paths are identical (trusted) in both the without-fix and with-fix runs, so the gate cannot construct a without-fix baseline and the change is **not A/B-verifiable** here. Its behaviour is validated end-to-end by the **Deep UI Tests** stage."
        Set-Content -Path (Join-Path $OutputPath "verification-report.md") -Value ($infraReport -join "`n") -Encoding UTF8
    } catch {
        Write-Host "  (could not write INCONCLUSIVE report: $_)" -ForegroundColor DarkGray
    }
    exit 3
}

# Verify each fix file is usable. A PR can MODIFY, ADD, or DELETE a fix file:
#   - modified → exists on disk (HEAD) and at merge-base
#   - added    → exists on disk (HEAD), not at merge-base  → NewFiles (not reverted)
#   - deleted  → does NOT exist on disk (HEAD), exists at merge-base
# A PR-deleted file legitimately does not exist in the with-fix worktree, so a
# plain Test-Path is NOT a valid existence gate — it wrongly aborted (→ infra
# failure / INCONCLUSIVE, tests never run) PRs that delete a file as part of
# their fix. Only a file present in NEITHER the worktree NOR the merge-base is
# a genuine error.
Write-Log "Verifying fix files are present (on disk or at merge-base)..."
$missingFixFiles = @()
foreach ($file in $FixFiles) {
    $fullPath = Join-Path $RepoRoot $file
    if (Test-Path $fullPath) {
        Write-Log "  ✓ $file exists"
    } elseif (git ls-tree -r $MergeBase --name-only -- $file 2>$null) {
        Write-Log "  ○ $file (deleted by PR — exists at merge-base, will be restored to form the baseline)"
    } else {
        Write-Log "ERROR: Fix file not found on disk or at merge-base: $file"
        $missingFixFiles += $file
    }
}
if ($missingFixFiles.Count -gt 0) {
    Write-Log "ERROR: $($missingFixFiles.Count) fix file(s) exist in neither the worktree nor the merge-base ($($MergeBase.Substring(0, 8))) — cannot verify."
    exit 1
}

# Determine which files exist at the merge-base (can be reverted) and which of
# those the PR DELETED (absent at HEAD) so STEP 3 restores them correctly.
Write-Log ""
Write-Log "Checking which fix files exist at merge-base ($($MergeBase.Substring(0, 8)))..."
$RevertableFiles = @()
$NewFiles = @()
$DeletedByPrFiles = @()

foreach ($file in $FixFiles) {
    # Check if file exists at merge-base commit
    $existsInBase = git ls-tree -r $MergeBase --name-only -- $file 2>$null

    if ($existsInBase) {
        $RevertableFiles += $file
        $existsAtHead = git ls-tree -r HEAD --name-only -- $file 2>$null
        if ($existsAtHead) {
            Write-Log "  ✓ $file (exists at merge-base - will revert)"
        } else {
            $DeletedByPrFiles += $file
            Write-Log "  ✓ $file (deleted by PR - restore from merge-base for baseline, re-delete with fix)"
        }
    } else {
        $NewFiles += $file
        Write-Log "  ○ $file (new file - skipping revert)"
    }
}

if ($RevertableFiles.Count -eq 0) {
    Write-Host "❌ No revertable fix files found. All fix files are new." -ForegroundColor Red
    Write-Host "   Cannot verify test behavior without files to revert." -ForegroundColor Yellow
    exit 1
}

# Check for uncommitted changes ONLY on files we will revert
Write-Log ""
Write-Log "Checking for uncommitted changes on revertable files..."
$uncommittedFiles = @()
foreach ($file in $RevertableFiles) {
    # Check if file has uncommitted changes (staged or unstaged).
    # Use core.fileMode=false so an executable-bit-only change (100644->100755)
    # is NOT treated as an uncommitted change. On mac agents a prior setup step
    # chmod +x's committed shell scripts (e.g. eng/scripts/*.sh), which makes a
    # plain 'git status --porcelain' report them as ' M' (mode-only) even though
    # their CONTENT is fully committed and reverts cleanly via 'git checkout HEAD'.
    # That spuriously aborted the A/B gate with "Uncommitted changes detected in
    # fix files" -> a false INCONCLUSIVE (observed build 14699093, #35156 catalyst:
    # disable-/enable-notification-center.sh flagged mode-only on all 3 retries).
    # core.fileMode=false ignores the exec-bit diff but STILL catches any real
    # content change (verified), so genuine uncommitted edits are still blocked.
    $status = git -c core.fileMode=false status --porcelain -- $file 2>$null
    if ($status) {
        $uncommittedFiles += $file
    }
}

if ($uncommittedFiles.Count -gt 0) {
    Write-Host "" -ForegroundColor Red
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║  ERROR: Uncommitted changes detected in fix files         ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    Write-Host "║  This script requires revertable fix files to be          ║" -ForegroundColor Red
    Write-Host "║  committed so they can be restored via git checkout HEAD. ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    Write-Host ""
    Write-Host "Uncommitted files:" -ForegroundColor Yellow
    foreach ($file in $uncommittedFiles) {
        Write-Host "  - $file" -ForegroundColor Yellow
    }
    Write-Host ""
    Write-Host "Run 'git add <files> && git commit' to commit your changes." -ForegroundColor Cyan
    exit 1
}

Write-Log "  ✓ All revertable fix files are committed"

# ─────────────────────────────────────────────────────────────────────────────
# EARLY SKIP — fix is irrelevant to this gate's platform (skip build + test)
# ─────────────────────────────────────────────────────────────────────────────
# When EVERY changed product file is platform-specific for a DIFFERENT platform
# than this gate (e.g. an iOS-only fix reviewed on the ANDROID gate), those files
# are excluded from THIS platform's target framework — they never compile into its
# binary — so reverting them or not produces a byte-identical build. The without-fix
# and with-fix runs would be identical no-ops, and the gate can only ever reach an
# INCONCLUSIVE "no match" AFTER spending the full build + device-test budget twice
# (dotnet/maui#35998 ran the Android UI test 2×2342s ≈ 78 min to prove nothing).
# Detect this up front and skip the whole revert/build/run cycle, emitting the SAME
# INCONCLUSIVE verdict (exit 3) the post-hoc classifier ($fixPlatformMismatch) would
# produce — the verdict is unchanged; only the wasted device time is removed.
#
# Conservative by construction: Test-FixIrrelevantToPlatform returns $true ONLY when
# there is at least one product file AND every product file targets another platform.
# Any shared/neutral file, any file targeting THIS platform, a pure test/snapshot
# change, or fix-less (verify-failure-only) mode all return $false and fall through
# to the normal gate below.
if ((@($FixFiles).Count -gt 0) -and (Test-FixIrrelevantToPlatform -FixFiles $FixFiles -Platform $Platform)) {
    Write-Log ""
    Write-Log "=========================================="
    Write-Log "GATE SKIPPED: fix not relevant to the '$Platform' platform"
    Write-Log "=========================================="
    Write-Log "  Every changed product file is platform-specific for a different platform;"
    Write-Log "  the fix is a no-op on '$Platform'. Skipping build + test (would be a"
    Write-Log "  guaranteed INCONCLUSIVE no-op) and reporting INCONCLUSIVE (exit 3)."
    foreach ($f in $FixFiles) { Write-Log "    - $f" }

    $platformUpper = if ($Platform) { $Platform.ToUpper() } else { "THIS" }
    $skipMergeBaseShort = if ($MergeBase -and $MergeBase.Length -ge 8) { $MergeBase.Substring(0, 8) } else { "$MergeBase" }
    $skipBase = if ($BaseBranchName) { $BaseBranchName } elseif ($BaseBranch) { $BaseBranch } else { "N/A" }
    $skipTestRows = if (@($AllDetectedTests).Count -gt 0) {
        (@($AllDetectedTests) | ForEach-Object { "| ``$($_.TestName)`` ($($_.Type)) | ⏭️ SKIPPED (not run on this platform) |" }) -join "`n"
    } else { "| _(none detected)_ | ⏭️ SKIPPED |" }
    $skipFixRows = (@($FixFiles) | ForEach-Object { "- ``$_``" }) -join "`n"

    $skipReport = @"
### Gate Result: ⚠️ INCONCLUSIVE

**Platform:** $platformUpper · **Base:** $skipBase · **Merge base:** ``$skipMergeBaseShort``

🌐 **Fix not relevant to the $platformUpper gate** — every changed code file is platform-specific for a *different* platform (an iOS/MacCatalyst/Android/Windows-only change). On $platformUpper the change is a no-op, so the repro test behaves identically **with and without** the fix and the gate cannot verify it here. This is **inconclusive, not a fix failure** — verify this PR on its own platform.

⏭️ **Gate skipped up front** — because the fix cannot affect this platform's binary, the gate skipped the build + device-test cycle instead of running it to a guaranteed INCONCLUSIVE result, saving the full test-time budget.

| Test | Status |
|------|--------|
$skipTestRows

**Changed fix file(s) — all platform-specific for another platform:**
$skipFixRows
"@

    Set-Content -Path $MarkdownReport -Value $skipReport -Encoding UTF8
    Write-Log "  Wrote INCONCLUSIVE (skipped) report to $MarkdownReport"

    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
    Write-Host "║           GATE SKIPPED — INCONCLUSIVE ⚠️                  ║" -ForegroundColor Yellow
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Yellow
    Write-Host "║  Fix targets a different platform than this gate — a       ║" -ForegroundColor Yellow
    Write-Host "║  no-op here, so it can't be verified on this platform.     ║" -ForegroundColor Yellow
    Write-Host "║  Skipped build + test to save the device-time budget.     ║" -ForegroundColor Yellow
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
    exit 3
}

# ── Baseline mutation window ──────────────────────────────────────────────────
# STEP 1 mutates BOTH the worktree and the index (reverted files, removed PR-added
# files) and STEP 3 puts everything back. Any phase in between can terminate the whole
# script with `exit` (e.g. a device boot failure -> `exit 3`), and PowerShell's `exit`
# bypasses the surrounding per-test `catch` — so without a `finally` the process could
# leave the tree missing this PR's changes for the review phases that run afterwards in
# the same job. Restore-BaselineMutationFromHead is the single restore implementation,
# used by STEP 3 (strict: a failure is fatal) and by the mutation-window `finally`
# (best effort: it must never mask the original exit code).
$script:BaselineMutationActive = $false

function Restore-BaselineMutationFromHead {
    <#
    .SYNOPSIS
        Restores every file STEP 1 mutated back to its HEAD (with-fix) state.
    .DESCRIPTION
        - Reverted product files -> `git checkout HEAD -- <file>`
        - Files the PR DELETED   -> re-removed (their HEAD state is "absent"; STEP 1
                                    restored them from the merge-base for the baseline)
        - PR-added files removed -> `git checkout HEAD -- <file>` (committed at HEAD)
        Never throws and never exits: it returns $true only when EVERY file was restored,
        and $false (after logging each failure) otherwise, so the caller decides what a
        partial restore means. STEP 3 treats $false as fatal (`exit 1`); the mutation-window
        `finally` passes -BestEffort and only logs, so an unwinding `exit` keeps its original
        exit code. -BestEffort therefore documents caller intent, not control flow.
    #>
    param(
        [string[]] $RevertableFiles = @(),
        [string[]] $DeletedByPrFiles = @(),
        [string[]] $NewFiles = @(),
        [string]   $RepoRoot,
        [switch]   $BestEffort
    )

    # A strict caller (STEP 3) turns any failure into `exit 1`; the mutation-window
    # `finally` passes -BestEffort and only logs, so label the lines accordingly.
    $sev = if ($BestEffort) { 'WARNING' } else { 'ERROR' }
    $ok = $true
    foreach ($file in @($RevertableFiles)) {
        if (@($DeletedByPrFiles) -contains $file) {
            # The PR deleted this file; its with-fix state is "absent", and
            # `git checkout HEAD -- $file` would fail because HEAD has no copy.
            Write-Log "  Re-removing (deleted by PR): $file"
            git rm -f --ignore-unmatch -- $file 2>&1 | Out-Null
            $wtPath = if ($RepoRoot) { Join-Path $RepoRoot $file } else { $file }
            if (Test-Path $wtPath) { Remove-Item -LiteralPath $wtPath -Force -ErrorAction SilentlyContinue }
            if (Test-Path $wtPath) {
                Write-Log "  ${sev}: Failed to re-remove PR-deleted file $file"
                $ok = $false
            }
        } else {
            Write-Log "  Restoring: $file"
            $gitOutput = git checkout HEAD -- $file 2>&1
            if ($LASTEXITCODE -ne 0) {
                Write-Log "  ${sev}: Failed to restore $file from HEAD"
                Write-Log "  Git output: $gitOutput"
                $ok = $false
            }
        }
    }

    foreach ($file in @($NewFiles)) {
        Write-Log "  Restoring (new in PR): $file"
        $gitOutput = git checkout HEAD -- $file 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Log "  ${sev}: Failed to restore new file $file from HEAD"
            Write-Log "  Git output: $gitOutput"
            $ok = $false
        }
    }

    return $ok
}

# Step 1: Revert fix files to merge-base state
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 1: Reverting fix files to merge-base ($($MergeBase.Substring(0, 8)))"
Write-Log "=========================================="

# Everything from here until STEP 3 completes runs inside the mutation window; the
# `finally` at its end guarantees restoration even when a nested phase calls `exit`.
$script:BaselineMutationActive = $true
try {

foreach ($file in $RevertableFiles) {
    Write-Log "  Reverting: $file"
    $gitOutput = git checkout $MergeBase -- $file 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Log "  ERROR: Failed to revert $file from $MergeBase"
        Write-Log "  Git output: $gitOutput"
        exit 1
    }
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) reverted to merge-base state"

# A PR-ADDED product file did not exist at the merge-base, so the true
# "without-fix" baseline is that file being ABSENT — not left on disk at its
# HEAD (fixed) version. Leaving new files behind while reverting the files they
# depend on produces a SPURIOUS build break: e.g. a new derived class
# (MauiCarouselRecyclerView2) that `override`s a member whose declaration lives
# in a MODIFIED base class — once the base is reverted the override has nothing
# to override → CS0115, which the gate then mis-reports as "base branch does not
# compile / main is broken" and needlessly goes INCONCLUSIVE (dotnet/maui
# #35640). Reverted (base-version) product code can NEVER reference a PR-added
# file, so removing new files is always safe for the product baseline; STEP 3
# restores them from HEAD (they are committed there) for the with-fix run.
if ($NewFiles.Count -gt 0) {
    Write-Log ""
    Write-Log "  Removing $($NewFiles.Count) PR-added file(s) so the baseline matches the pre-fix tree:"
    foreach ($file in $NewFiles) {
        Write-Log "    Removing (new in PR): $file"
        $rmOutput = git rm -f --ignore-unmatch -- $file 2>&1
        if ($LASTEXITCODE -ne 0) {
            # --ignore-unmatch already returns 0 for an untracked path, so a non-zero
            # code is a real index failure. Fall through to the worktree removal and
            # only fail the gate when the file is still on disk afterwards (a stale
            # copy would silently poison the "without-fix" baseline build).
            Write-Log "    WARNING: git rm failed for $file — falling back to worktree removal"
            Write-Log "    Git output: $rmOutput"
        }
        $wtPath = Join-Path $RepoRoot $file
        if (Test-Path $wtPath) { Remove-Item -LiteralPath $wtPath -Force -ErrorAction SilentlyContinue }
        if (Test-Path $wtPath) {
            Write-Log "  ERROR: Failed to remove PR-added file $file for the baseline"
            exit 1
        }
    }
    Write-Log "  ✓ $($NewFiles.Count) PR-added file(s) removed for the baseline"
}

# ── Snapshot-diff A/B helpers (VerifyScreenshot environmental false-FAILED guard) ──
# A visual-fix PR whose committed baselines carry a small, CONSTANT cross-agent
# rendering offset (anti-aliasing / font hinting differ between the machine that
# captured the baseline PNG and the gate agent) makes even a CORRECT fix fail its
# VerifyScreenshot assertions by a fraction of a percent. Because the gate runs the
# SAME test both WITHOUT and WITH the fix, it can distinguish a fix-caused diff
# (present without the fix, gone/smaller with it) from an environmental diff
# (present at ~the same magnitude in BOTH runs — the fix does not touch it).
# Get-SnapshotDiffMap extracts { baseline.png -> max % diff } from a run log;
# Test-SnapshotEnvironmentalResidual returns $true only when the with-fix run's
# failures are ALL snapshot diffs that (a) also failed WITHOUT the fix, (b) are no
# LARGER than without the fix (the fix worsened nothing and added no new failing
# snapshot) and (c) are every one below a small environmental ceiling. In that case
# the residual is environmental, not a broken fix -> INCONCLUSIVE, NEVER PASS. Any
# parsing hiccup returns a safe default (empty map / $false) so the gate falls back
# to today's genuine-FAILED behavior. (Observed on iOS PR #36511 Issue33037NonShell:
# with-fix DirectScrollView/ListView/CollectionView diffs were byte-identical to the
# without-fix run at 0.65-0.77%, while the real bug diffs 2.63%/3.01% collapsed to
# pass/0.54% — i.e. the fix worked but sub-1% baseline offset false-FAILED the gate.)
function Get-SnapshotDiffMap {
    param([string] $LogFile)
    $map = @{}
    try {
        if (-not $LogFile -or -not (Test-Path $LogFile)) { return $map }
        $c = Get-Content $LogFile -Raw -ErrorAction SilentlyContinue
        if ([string]::IsNullOrWhiteSpace($c)) { return $map }
        # e.g. "Snapshot different than baseline: Issue33037NonShell_ListView_AfterScroll.png (0.65% difference)"
        $rx = [regex]'(?i)Snapshot different than baseline:\s*(?<file>[^\s()]+\.png)\s*\(\s*(?<pct>[0-9]+(?:\.[0-9]+)?)\s*%\s*difference\s*\)'
        foreach ($m in $rx.Matches($c)) {
            $file = ([System.IO.Path]::GetFileName($m.Groups['file'].Value)).ToLowerInvariant()
            $pct  = [double]$m.Groups['pct'].Value
            if (-not $map.ContainsKey($file) -or $pct -gt $map[$file]) { $map[$file] = $pct }
        }
    } catch { return @{} }
    return $map
}

function Get-LeakAssertCount {
    # Counts GC memory-leak assertion failures in a test log. The MAUI device/unit test helper
    # AssertionExtensions.WaitForGC emits exactly one "Expected all references to be collected,
    # but some are still alive" line per failed *DoesNotLeak* assert. That GC check is inherently
    # non-deterministic (even a correct fix can leave a reference briefly uncollected on a given
    # run), so the gate uses this count to treat a pure leak FAIL→FAIL as INCONCLUSIVE rather than
    # a genuine "fix does not pass" FAILED. Returns 0 on any read/parse issue (fail-safe).
    param([string] $LogFile)
    try {
        if (-not $LogFile -or -not (Test-Path $LogFile)) { return 0 }
        $c = Get-Content $LogFile -Raw -ErrorAction SilentlyContinue
        if ([string]::IsNullOrWhiteSpace($c)) { return 0 }
        return ([regex]::Matches($c, '(?i)Expected all references to be collected, but some are still alive')).Count
    } catch { return 0 }
}

function Test-SnapshotEnvironmentalResidual {
    param(
        [hashtable] $WithoutFixResult,
        [hashtable] $WithFixResult,
        [double]    $ResidualCeilingPercent = 1.0,
        [double]    $Epsilon = 0.02
    )
    try {
        if (-not $WithoutFixResult -or -not $WithFixResult) { return $false }
        $woMap = $WithoutFixResult.SnapshotDiffMap
        $wMap  = $WithFixResult.SnapshotDiffMap
        if ($null -eq $woMap -or $null -eq $wMap) { return $false }
        if ($wMap.Count -eq 0) { return $false }
        # Every with-fix failure must be a snapshot diff (guard against a non-visual
        # failure hiding among the snapshot diffs): #snapshot files >= reported FailCount.
        $wFail  = [int]($WithFixResult.FailCount)
        $woFail = [int]($WithoutFixResult.FailCount)
        if ($wFail  -le 0 -or $wMap.Count  -lt $wFail)  { return $false }
        if ($woFail -le 0 -or $woMap.Count -lt $woFail) { return $false }
        foreach ($file in $wMap.Keys) {
            # A snapshot the fix NEWLY breaks (absent without the fix) is a real regression.
            if (-not $woMap.ContainsKey($file)) { return $false }
            # The fix must not enlarge any diff, and every residual must be tiny.
            if ($wMap[$file] -gt ($woMap[$file] + $Epsilon)) { return $false }
            if ($wMap[$file] -gt $ResidualCeilingPercent)    { return $false }
        }
        return $true
    } catch { return $false }
}

# ── Large-diff cross-machine baseline mismatch (VerifyScreenshot false-FAILED guard #2) ──
# DISTINCT from Test-SnapshotEnvironmentalResidual (which catches a SUB-1% constant offset on
# a fix that clearly WORKED): this catches the case where a committed baseline PNG simply
# CANNOT be reproduced by the gate agent, so the SAME snapshot test fails by a LARGE amount in
# BOTH runs and the fix moves the diff by essentially nothing. macOS TitleBar / window-chrome
# snapshots are the classic offender — a baseline captured on the PR author's machine renders
# tens-of-percent differently on the CI agent (window size, screen scale, traffic-light
# buttons, menu bar), swamping any fix effect. Observed on catalyst PR #36541
# (TitleBarTrailingContentShouldRenderProperly: without-fix 42.91% ≈ with-fix 43.06%, Δ 0.15pp;
# the PR commits its own baseline PNG, which the gate keeps as a test asset while reverting the
# fix — so both runs compare the CI render against an author-machine baseline).
#
# In THIS state the gate CANNOT distinguish an environmental baseline mismatch from a genuine
# no-op fix — both look like "the fix changed the snapshot by ~nothing" — so a confident FAILED
# risks a false accusation against a correct fix. The honest verdict is INCONCLUSIVE (defer to a
# human who inspects the snapshot), NEVER PASS. Fires ONLY when every with-fix failure is a
# snapshot that (a) also failed WITHOUT the fix (not a new regression the fix introduced),
# (b) the fix changed by less than a tolerance (|with-without| <= max(AbsTol, RelTol*without) —
# essentially no effect), and (c) is well ABOVE the sub-1% offset zone owned by
# Test-SnapshotEnvironmentalResidual (> LargeDiffFloor, so a fix that shrank the diff toward the
# baseline is left as a genuine FAILED). Any parsing issue → $false (fail-safe to today's
# genuine-FAILED behavior).
function Test-SnapshotBaselineUnresolvable {
    param(
        [hashtable] $WithoutFixResult,
        [hashtable] $WithFixResult,
        [double]    $LargeDiffFloorPercent = 5.0,
        [double]    $AbsTolPercent = 1.0,
        [double]    $RelTol = 0.05
    )
    try {
        if (-not $WithoutFixResult -or -not $WithFixResult) { return $false }
        $woMap = $WithoutFixResult.SnapshotDiffMap
        $wMap  = $WithFixResult.SnapshotDiffMap
        if ($null -eq $woMap -or $null -eq $wMap) { return $false }
        if ($wMap.Count -eq 0) { return $false }
        # Every with-fix failure must be a snapshot diff (guard against a non-visual failure
        # hiding among the snapshot diffs): #snapshot files >= reported FailCount.
        $wFail  = [int]($WithFixResult.FailCount)
        $woFail = [int]($WithoutFixResult.FailCount)
        if ($wFail  -le 0 -or $wMap.Count  -lt $wFail)  { return $false }
        if ($woFail -le 0 -or $woMap.Count -lt $woFail) { return $false }
        foreach ($file in $wMap.Keys) {
            # A snapshot the fix NEWLY breaks (absent without the fix) is a real regression.
            if (-not $woMap.ContainsKey($file)) { return $false }
            $with    = [double]$wMap[$file]
            $without = [double]$woMap[$file]
            # Must be a LARGE diff — at/below this floor is the sub-1% AA zone owned by
            # Test-SnapshotEnvironmentalResidual (a fix that worked with a tiny residual).
            if ($with -le $LargeDiffFloorPercent) { return $false }
            # The fix must have changed the diff by essentially NOTHING (the environmental
            # mismatch dominates). A meaningful shrink means the fix DID move the render toward
            # the baseline — leave that as a genuine FAILED (partial/incomplete fix), not env.
            $tol = [math]::Max($AbsTolPercent, $RelTol * $without)
            if ([math]::Abs($with - $without) -gt $tol) { return $false }
        }
        return $true
    } catch { return $false }
}

# Step 2: Run ALL tests WITHOUT fix
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║  STEP 2: Running tests WITHOUT fix (expect FAIL)          ║" -ForegroundColor Magenta
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Magenta
Write-Log ""
Write-Log "STEP 2: Running tests WITHOUT fix (should FAIL)"

$withoutFixResults = @()
$testIndex = 0
foreach ($testEntry in $AllDetectedTests) {
    $testIndex++
    $icon = switch ($testEntry.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }

    $sanitizedName = ($testEntry.TestName -replace '[^a-zA-Z0-9_\-\.]', '_')
    if ($sanitizedName.Length -gt 60) { $sanitizedName = $sanitizedName.Substring(0, 60) }
    $testLogFile = Join-Path $OutputPath "test-without-fix-$sanitizedName.log"

    # AzDO collapsible group for raw test output
    Write-Host "##[group]🔴 WITHOUT FIX $testIndex/$($AllDetectedTests.Count): $icon $($testEntry.TestName) (filter: $($testEntry.Filter))"

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $result = Invoke-TestRunConfirmed -TestEntry $testEntry -LogFile $testLogFile -Expected 'Fail'
    } catch {
        $result = @{ Passed = $false; Failed = 0; Total = 0; PassCount = 0; FailCount = 0; Skipped = 0; EnvError = $true; Error = $_.Exception.Message }
        Write-Host "  ⚠️ Test invocation threw: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    $sw.Stop()
    $result.TestName = $testEntry.TestName
    $result.TestType = $testEntry.Type
    $result.Duration = $sw.Elapsed
    $result.SnapshotDiffMap = Get-SnapshotDiffMap -LogFile $testLogFile
    $result.LeakAssertCount = Get-LeakAssertCount -LogFile $testLogFile
    $withoutFixResults += $result

    # Print raw log inside the collapsible group so it's available but not noisy
    if (Test-Path $testLogFile) {
        $logLines = Get-Content $testLogFile -ErrorAction SilentlyContinue
        $lineCount = if ($logLines) { $logLines.Count } else { 0 }
        Write-Host "  ── Log ($lineCount lines) ──" -ForegroundColor DarkGray
        if ($logLines) { $logLines | ForEach-Object { Write-Host "  $_" } }
    }
    Write-Host "##[endgroup]"

    # Print result OUTSIDE the group so it's always visible
    $durStr = "$([math]::Round($sw.Elapsed.TotalSeconds))s"
    $counts = if ($result.Total -gt 0) { " ($($result.Total) total, $($result.Failed) failed)" } else { "" }
    if ($result.EnvError) {
        Write-Host "  ⚠️ $($testEntry.TestName): ENV ERROR$counts — $durStr — $($result.Error)" -ForegroundColor Yellow
    } elseif (-not $result.Passed) {
        Write-Host "  ✅ $($testEntry.TestName): FAILED$counts — $durStr (expected)" -ForegroundColor Green
        if ($result.FailureReason) { Write-Host "     └─ $($result.FailureReason)" -ForegroundColor DarkGray }
    } else {
        Write-Host "  ❌ $($testEntry.TestName): PASSED$counts — $durStr (unexpected!)" -ForegroundColor Red
    }
    Write-Log "  [$($testEntry.Type)] $($testEntry.TestName): Passed=$($result.Passed) Failed=$($result.Failed) [$durStr]"
}

# Combine into a single summary for backward compatibility
$withoutFixResult = @{
    Passed = ($withoutFixResults | Where-Object { $_.Passed }).Count -eq $withoutFixResults.Count
    PassCount = ($withoutFixResults | Measure-Object -Property PassCount -Sum).Sum
    FailCount = ($withoutFixResults | Measure-Object -Property FailCount -Sum).Sum
    Failed = ($withoutFixResults | Measure-Object -Property Failed -Sum).Sum
    Skipped = ($withoutFixResults | Measure-Object -Property Skipped -Sum).Sum
    Total = ($withoutFixResults | Measure-Object -Property Total -Sum).Sum
}

# Save combined log
$withoutFixResults | ForEach-Object { "[$($_.TestType)] $($_.TestName): Passed=$($_.Passed) Failed=$($_.Failed)" } | Out-File $WithoutFixLog -Append

# Step 3: Restore fix files from current branch HEAD
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 3: Restoring fix files from HEAD"
Write-Log "=========================================="

$restored = Restore-BaselineMutationFromHead `
    -RevertableFiles $RevertableFiles `
    -DeletedByPrFiles $DeletedByPrFiles `
    -NewFiles $NewFiles `
    -RepoRoot $RepoRoot
if (-not $restored) {
    Write-Log "  ERROR: Failed to restore the with-fix tree from HEAD"
    exit 1
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) restored from HEAD"
if ($NewFiles.Count -gt 0) {
    Write-Log "  ✓ $($NewFiles.Count) PR-added file(s) restored from HEAD"
}

# The tree matches HEAD again — the window is closed, so the `finally` below is a no-op.
$script:BaselineMutationActive = $false

} finally {
    # Reached on EVERY exit path out of the mutation window, including a nested `exit`
    # from a phase between STEP 1 and STEP 3 (PowerShell runs `finally` on `exit` and
    # preserves the exit code). Best effort: never throw here, so the original exit
    # code / error is what the caller sees.
    if ($script:BaselineMutationActive) {
        Write-Log ""
        Write-Log "⚠️  Verification ended inside the baseline mutation window — restoring the with-fix tree from HEAD"
        try {
            $emergencyRestored = Restore-BaselineMutationFromHead `
                -RevertableFiles $RevertableFiles `
                -DeletedByPrFiles $DeletedByPrFiles `
                -NewFiles $NewFiles `
                -RepoRoot $RepoRoot `
                -BestEffort
            if ($emergencyRestored) {
                Write-Log "  ✓ Worktree/index restored to HEAD"
            } else {
                Write-Log "  ⚠️  Emergency restore did not fully succeed — later phases may see a mutated tree"
            }
        } catch {
            Write-Log "  ⚠️  Emergency restore threw: $($_.Exception.Message)"
        }
        $script:BaselineMutationActive = $false
    }
}

# Step 4: Run ALL tests WITH fix
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║  STEP 4: Running tests WITH fix (expect PASS)            ║" -ForegroundColor Cyan
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Log ""
Write-Log "STEP 4: Running tests WITH fix (should PASS)"

$withFixResults = @()
$testIndex = 0
foreach ($testEntry in $AllDetectedTests) {
    $testIndex++
    $icon = switch ($testEntry.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }

    $sanitizedName = ($testEntry.TestName -replace '[^a-zA-Z0-9_\-\.]', '_')
    if ($sanitizedName.Length -gt 60) { $sanitizedName = $sanitizedName.Substring(0, 60) }
    $testLogFile = Join-Path $OutputPath "test-with-fix-$sanitizedName.log"

    # AzDO collapsible group for raw test output
    Write-Host "##[group]🟢 WITH FIX $testIndex/$($AllDetectedTests.Count): $icon $($testEntry.TestName) (filter: $($testEntry.Filter))"

    $sw = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $result = Invoke-TestRunConfirmed -TestEntry $testEntry -LogFile $testLogFile -Expected 'Pass'
    } catch {
        $result = @{ Passed = $false; Failed = 0; Total = 0; PassCount = 0; FailCount = 0; Skipped = 0; EnvError = $true; Error = $_.Exception.Message }
        Write-Host "  ⚠️ Test invocation threw: $($_.Exception.Message)" -ForegroundColor Yellow
    }
    $sw.Stop()
    $result.TestName = $testEntry.TestName
    $result.TestType = $testEntry.Type
    $result.Duration = $sw.Elapsed
    $result.SnapshotDiffMap = Get-SnapshotDiffMap -LogFile $testLogFile
    $result.LeakAssertCount = Get-LeakAssertCount -LogFile $testLogFile
    $withFixResults += $result

    # Print raw log inside the collapsible group
    if (Test-Path $testLogFile) {
        $logLines = Get-Content $testLogFile -ErrorAction SilentlyContinue
        $lineCount = if ($logLines) { $logLines.Count } else { 0 }
        Write-Host "  ── Log ($lineCount lines) ──" -ForegroundColor DarkGray
        if ($logLines) { $logLines | ForEach-Object { Write-Host "  $_" } }
    }
    Write-Host "##[endgroup]"

    # Print result OUTSIDE the group so it's always visible
    $durStr = "$([math]::Round($sw.Elapsed.TotalSeconds))s"
    $counts = if ($result.Total -gt 0) { " ($($result.Total) total, $($result.Failed) failed)" } else { "" }
    if ($result.EnvError) {
        Write-Host "  ⚠️ $($testEntry.TestName): ENV ERROR$counts — $durStr — $($result.Error)" -ForegroundColor Yellow
    } elseif ($result.Passed) {
        Write-Host "  ✅ $($testEntry.TestName): PASSED$counts — $durStr (expected)" -ForegroundColor Green
    } else {
        Write-Host "  ❌ $($testEntry.TestName): FAILED$counts — $durStr (unexpected!)" -ForegroundColor Red
        if ($result.FailureReason) { Write-Host "     └─ $($result.FailureReason)" -ForegroundColor DarkGray }
    }
    Write-Log "  [$($testEntry.Type)] $($testEntry.TestName): Passed=$($result.Passed) Failed=$($result.Failed) [$durStr]"
}

# ── Clean-rebuild retry for with-fix-only build errors (incremental-staleness guard) ──
# The gate reverts fix files to the merge-base, builds, then restores them to HEAD
# and builds again — all sharing one obj/. UI tests already Rebuild=$true, but
# UNIT/XAML tests use an INCREMENTAL `dotnet test`, so this revert→build→restore→
# build cycle can leave the with-fix build reusing stale intermediate state when the
# PR ADDS a type the baseline lacks — producing a PHANTOM compile error whose
# signature doesn't even match HEAD (observed on #36553: with-fix "CS8622 object
# sender" while HEAD actually declares "object? sender"). That would fail the gate on
# a PR that compiles cleanly. When a test shows a BuildError WITH the fix but the
# baseline (without-fix) compiled, force ONE clean rebuild (-t:Rebuild across the P2P
# graph) before trusting the failure. This can ONLY correct a false FAILED into the
# true verdict: a genuine PR compile break still fails the clean rebuild (stays
# FAILED), and a clean compile whose tests genuinely fail is preserved as FAILED.
for ($ri = 0; $ri -lt $withFixResults.Count; $ri++) {
    $wr = $withFixResults[$ri]
    if (-not $wr.BuildError) { continue }
    if ($wr.TestType -ne 'UnitTest' -and $wr.TestType -ne 'XamlUnitTest') { continue }
    $woMatch = @($withoutFixResults | Where-Object { $_.TestName -eq $wr.TestName }) | Select-Object -First 1
    if ($woMatch -and $woMatch.BuildError) { continue }   # baseline ALSO failed to compile → handled as INCONCLUSIVE, not staleness
    $retryEntry = @($AllDetectedTests | Where-Object { $_.TestName -eq $wr.TestName }) | Select-Object -First 1
    if (-not $retryEntry) { continue }
    $projRel = if ($retryEntry.Type -eq 'XamlUnitTest') { 'src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj' } else { $retryEntry.ProjectPath }
    if (-not $projRel) { continue }
    $projFull = Join-Path $RepoRoot $projRel
    if (-not (Test-Path $projFull)) { continue }

    Write-Host "##[group]♻️ CLEAN-REBUILD RETRY: $($retryEntry.TestName) (with-fix build error, baseline compiled)"
    Write-Host "  A with-fix-only compile error can be incremental-build staleness from the revert/restore cycle. Forcing a clean -t:Rebuild to confirm before trusting the failure." -ForegroundColor Yellow
    $rsan = ($retryEntry.TestName -replace '[^a-zA-Z0-9_\-\.]', '_'); if ($rsan.Length -gt 60) { $rsan = $rsan.Substring(0, 60) }
    $cleanLog = Join-Path $OutputPath "test-with-fix-cleanrebuild-$rsan.log"
    $rsw = [System.Diagnostics.Stopwatch]::StartNew()
    $buildOut = Invoke-WithoutGhTokens { & dotnet build $projFull -c Debug -t:Rebuild -p:TreatWarningsAsErrors=false 2>&1 }
    $buildExit = $LASTEXITCODE
    $combined = @($buildOut)
    if ($buildExit -eq 0) {
        $testOut = Invoke-WithoutGhTokens { & dotnet test $projFull -c Debug --logger "console;verbosity=normal" -p:TreatWarningsAsErrors=false --filter $retryEntry.Filter 2>&1 }
        $combined += @($testOut)
    }
    $combined | Out-File -FilePath $cleanLog -Force -Encoding utf8
    $rsw.Stop()
    Write-Host "##[endgroup]"

    $clean = Get-TestResultFromOutput -LogFile $cleanLog -TestFilter $retryEntry.Filter
    $clean.TestName = $retryEntry.TestName
    $clean.TestType = $retryEntry.Type
    $clean.Duration = $rsw.Elapsed
    $clean.SnapshotDiffMap = Get-SnapshotDiffMap -LogFile $cleanLog
    $durS = "$([math]::Round($rsw.Elapsed.TotalSeconds))s"
    if ($clean.BuildError) {
        Write-Host "  ❌ $($retryEntry.TestName): STILL a build error after a clean rebuild — genuine PR compile failure ($durS)." -ForegroundColor Red
        Write-Log "  [CleanRetry] $($retryEntry.TestName): build error persists after -t:Rebuild — genuine compile failure"
    } elseif ($clean.Passed) {
        Write-Host "  ✅ $($retryEntry.TestName): PASSED after clean rebuild — the incremental with-fix build error was STALE; false FAILED avoided ($durS)." -ForegroundColor Green
        Write-Log "  [CleanRetry] $($retryEntry.TestName): PASSED after -t:Rebuild — with-fix build error was incremental staleness"
    } else {
        Write-Host "  ❌ $($retryEntry.TestName): compiled clean but tests FAILED — genuine test failure ($durS)." -ForegroundColor Red
        Write-Log "  [CleanRetry] $($retryEntry.TestName): compiled clean, tests failed — genuine failure"
    }
    $withFixResults[$ri] = $clean
}

# Combine into a single summary for backward compatibility
$withFixResult = @{
    Passed = ($withFixResults | Where-Object { -not $_.Passed }).Count -eq 0
    PassCount = ($withFixResults | Measure-Object -Property PassCount -Sum).Sum
    FailCount = ($withFixResults | Measure-Object -Property FailCount -Sum).Sum
    Failed = ($withFixResults | Measure-Object -Property Failed -Sum).Sum
    Skipped = ($withFixResults | Measure-Object -Property Skipped -Sum).Sum
    Total = ($withFixResults | Measure-Object -Property Total -Sum).Sum
}

$withFixResults | ForEach-Object { "[$($_.TestType)] $($_.TestName): Passed=$($_.Passed) Failed=$($_.Failed)" } | Out-File $WithFixLog -Append

# A Windows crash-regression test can terminate the unpackaged device-test app before
# xUnit flushes XML. That is normally inconclusive. When the no-result marker persists
# through all three baseline retries and the identical scoped test then passes cleanly
# with the fix on the same agent, however, the source swap is the only changed variable:
# credit the repeated baseline app exit as the expected failure repro.
foreach ($t in $AllDetectedTests) {
    $wo = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    $w = $withFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    if (-not $wo -or -not $w) { continue }
    if (Convert-WindowsBaselineNoResultsToFailure `
            -WithoutFixResult $wo `
            -WithFixResult $w `
            -RunPlatform $Platform `
            -TestType $t.Type) {
        Write-Host "  ✅ $($t.TestName): baseline app exit reproduced in all $($wo.AttemptCount) attempts and the scoped with-fix run passed — crediting FAIL → PASS" -ForegroundColor Green
        Write-Log "  [$($t.Type)] $($t.TestName): persistent Windows baseline app exit → with-fix PASS; credited as a verified repro"
    }
}

# Exact Windows class runs have a shorter bounded timeout than broad suites. A single timeout
# remains environmental, but the retry layer gives us three separate attempts. Convert only
# all-timeout evidence: with-fix timeouts are a deterministic failure to satisfy the Gate
# contract; baseline timeouts become the expected repro after the same target produces any
# definitive with-fix result.
foreach ($t in $AllDetectedTests) {
    $wo = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    $w = $withFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    if (-not $wo -or -not $w) { continue }

    if (Convert-WindowsTargetTimeoutToFailure `
            -Result $w `
            -CounterpartResult $wo `
            -Phase WithFix `
            -RunPlatform $Platform `
            -TestType $t.Type) {
        Write-Host "  ❌ $($t.TestName): scoped with-fix run timed out in all $($w.AttemptCount) attempts — treating as a deterministic target failure" -ForegroundColor Red
        Write-Log "  [$($t.Type)] $($t.TestName): repeated Windows with-fix target timeout → definitive failure"
    }

    if (Convert-WindowsTargetTimeoutToFailure `
            -Result $wo `
            -CounterpartResult $w `
            -Phase WithoutFix `
            -RunPlatform $Platform `
            -TestType $t.Type) {
        Write-Host "  ✅ $($t.TestName): baseline target timed out in all $($wo.AttemptCount) attempts and the scoped with-fix run was definitive — crediting the baseline failure" -ForegroundColor Green
        Write-Log "  [$($t.Type)] $($t.TestName): repeated Windows baseline target timeout → verified failure repro"
    }
}

# Refresh the aggregate objects after trusted Windows evidence conversion mutates the
# per-test results. These aggregates feed the persisted Markdown report.
$withoutFixResult = @{
    Passed = ($withoutFixResults | Where-Object { -not $_.Passed }).Count -eq 0
    PassCount = ($withoutFixResults | Measure-Object -Property PassCount -Sum).Sum
    FailCount = ($withoutFixResults | Measure-Object -Property FailCount -Sum).Sum
    Failed = ($withoutFixResults | Measure-Object -Property Failed -Sum).Sum
    Skipped = ($withoutFixResults | Measure-Object -Property Skipped -Sum).Sum
    Total = ($withoutFixResults | Measure-Object -Property Total -Sum).Sum
}
$withFixResult = @{
    Passed = ($withFixResults | Where-Object { -not $_.Passed }).Count -eq 0
    PassCount = ($withFixResults | Measure-Object -Property PassCount -Sum).Sum
    FailCount = ($withFixResults | Measure-Object -Property FailCount -Sum).Sum
    Failed = ($withFixResults | Measure-Object -Property Failed -Sum).Sum
    Skipped = ($withFixResults | Measure-Object -Property Skipped -Sum).Sum
    Total = ($withFixResults | Measure-Object -Property Total -Sum).Sum
}

# Step 5: Evaluate results
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor White
Write-Host "║                  GATE SUMMARY                             ║" -ForegroundColor White
Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor White
Write-Log ""
Write-Log "VERIFICATION RESULTS"

$verificationPassed = $false
# "Without fix" should FAIL and "with fix" should PASS. These two aggregates are kept for the
# report/summary text, but the PASS/FAIL DECISION now uses the relaxed per-test rule below.
$failedWithoutFix = ($withoutFixResults | Where-Object { $_.Passed }).Count -eq 0
$passedWithFix = ($withFixResults | Where-Object { -not $_.Passed }).Count -eq 0

# Print a clear comparison table
Write-Host ""
Write-Host "  Test Name              │ Without Fix │  With Fix  " -ForegroundColor White
Write-Host "  ───────────────────────┼─────────────┼────────────" -ForegroundColor DarkGray
foreach ($t in $AllDetectedTests) {
    $woResult = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName }
    $wResult = $withFixResults | Where-Object { $_.TestName -eq $t.TestName }

    $woIcon = if ($woResult.EnvError) { "⚠️ ENV ERR" } elseif (-not $woResult.Passed) { "✅ FAIL   " } else { "❌ PASS   " }
    $wIcon = if ($wResult.EnvError) { "⚠️ ENV ERR" } elseif ($wResult.Passed) { "✅ PASS  " } else { "❌ FAIL  " }

    $nameDisplay = $t.TestName
    if ($nameDisplay.Length -gt 22) { $nameDisplay = $nameDisplay.Substring(0, 19) + "..." }
    $nameDisplay = $nameDisplay.PadRight(22)

    $woColor = if ($woResult.EnvError) { "Yellow" } elseif (-not $woResult.Passed) { "Green" } else { "Red" }
    $wColor = if ($wResult.EnvError) { "Yellow" } elseif ($wResult.Passed) { "Green" } else { "Red" }

    Write-Host "  $nameDisplay │ " -NoNewline -ForegroundColor White
    Write-Host "$woIcon" -NoNewline -ForegroundColor $woColor
    Write-Host "  │ " -NoNewline -ForegroundColor White
    Write-Host "$wIcon" -ForegroundColor $wColor

    Write-Log "  [$($t.Type)] $($t.TestName): without fix=$(if (-not $woResult.Passed) {'FAIL ✅'} else {'PASS ❌'}), with fix=$(if ($wResult.Passed) {'PASS ✅'} else {'FAIL ❌'})"
}
Write-Host "  ───────────────────────┼─────────────┼────────────" -ForegroundColor DarkGray
Write-Host "  Expected               │   FAIL      │   PASS     " -ForegroundColor DarkGray
Write-Host ""

$verificationPassed = $failedWithoutFix -and $passedWithFix

# ── Relaxed gate rule (user-selected) ──
# PASS when AT LEAST ONE test genuinely REPRODUCES the bug (FAIL without the fix → PASS with
# it) AND the fix leaves NO test failing (no genuine with-fix failure). A test that passes in
# both states (PASS→PASS) neither proves nor blocks the fix, so it's ignored. This replaces the
# old "ALL tests must fail without the fix" rule, which false-FAILED mixed PRs where a strong
# regression test coexists with an always-green test (e.g. PR #27477: VisualStateManagerTests
# FAIL→PASS but Issue19752 PASS→PASS). Env/build/filter results are inconclusive (handled
# below) and are excluded from both counts.

# ── VerifyScreenshot environmental-residual downgrade (see Get-SnapshotDiffMap) ──
# BEFORE counting genuine with-fix failures, reclassify any FAIL→FAIL test whose
# with-fix failures are purely environmental snapshot residue (the fix worsened /
# added no snapshot and every residual diff is sub-ceiling) as an env/INCONCLUSIVE
# result. Setting EnvError plugs into the existing inconclusive handling: the test
# drops out of the genuine-fail count and drives the overall verdict to INCONCLUSIVE
# (exit 3), NEVER to PASS. Fail-safe: Test-SnapshotEnvironmentalResidual returns
# $false on any parsing issue, leaving today's genuine-FAILED behavior intact.
foreach ($t in $AllDetectedTests) {
    $wo = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    $w  = $withFixResults    | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    if (-not $wo -or -not $w) { continue }
    # Only relevant when BOTH runs genuinely FAILED (FAIL→FAIL) with no prior inconclusive.
    if ($wo.EnvError -or $wo.BuildError -or $wo.FilterMismatch) { continue }
    if ($w.EnvError  -or $w.BuildError  -or $w.FilterMismatch)  { continue }
    if ($wo.Passed -or $w.Passed) { continue }
    if (Test-SnapshotEnvironmentalResidual -WithoutFixResult $wo -WithFixResult $w) {
        $maxResidual = 0.0
        foreach ($v in $w.SnapshotDiffMap.Values) { if ($v -gt $maxResidual) { $maxResidual = $v } }
        $w.EnvError = $true
        $w.SnapshotEnvResidual = $true
        $w.Error = "With-fix run only fails VerifyScreenshot snapshot diffs that are no larger than the without-fix run (max $($maxResidual)% <= 1%). The fix resolves the bug's visual difference; the residual is a constant cross-agent baseline offset, not a fix failure. Regenerate the baseline PNG(s) on the target agent."
        Write-Host "  📷 $($t.TestName): with-fix failures are environmental snapshot residue (max $($maxResidual)% <= 1%, none worsened vs without-fix) — reclassifying as INCONCLUSIVE, not FAILED" -ForegroundColor Yellow
        Write-Log "  [$($t.Type)] $($t.TestName): with-fix snapshot residual environmental (max $($maxResidual)%) — INCONCLUSIVE (not a fix failure)"
    }
    elseif (Test-SnapshotBaselineUnresolvable -WithoutFixResult $wo -WithFixResult $w) {
        $maxDiff = 0.0
        foreach ($v in $w.SnapshotDiffMap.Values) { if ($v -gt $maxDiff) { $maxDiff = $v } }
        $w.EnvError = $true
        $w.SnapshotBaselineUnresolved = $true
        $w.Error = "With-fix run fails only VerifyScreenshot snapshot diff(s) that are LARGE (max $($maxDiff)%) and essentially UNCHANGED from the without-fix run — the fix moved the pixel difference by under ~1 percentage point. The committed baseline PNG cannot be reproduced on this gate agent (a cross-machine rendering mismatch, e.g. macOS TitleBar / window chrome), which swamps any fix effect, so the gate cannot distinguish an environmental mismatch from an ineffective fix. INCONCLUSIVE — a human should inspect the snapshots-diff artifact; if the render is correct, regenerate the baseline on the target agent."
        Write-Host "  📷 $($t.TestName): with-fix snapshot diff is LARGE and unchanged vs without-fix (max $($maxDiff)%) — cross-machine baseline mismatch, reclassifying as INCONCLUSIVE, not FAILED" -ForegroundColor Yellow
        Write-Log "  [$($t.Type)] $($t.TestName): large unchanged snapshot diff (max $($maxDiff)%) — INCONCLUSIVE (cross-machine baseline mismatch, not a verifiable fix failure)"
    }
}

# ── Flaky GC memory-leak reclassification (INCONCLUSIVE, exit 3 — never a false FAILED) ──
# A "DoesNotLeak" device/unit test asserts via AssertionExtensions.WaitForGC, which is
# inherently non-deterministic: even a CORRECT fix can leave a reference momentarily
# uncollected on a given run ("Expected all references to be collected, but some are still
# alive"). So a with-fix FAIL on a pure leak assert is UNVERIFIABLE by the gate, not proof the
# fix is broken. Reclassify such a with-fix failure as INCONCLUSIVE — BUT only when EVERY
# remaining with-fix genuine failure is a leak assert (two-pass guard below), so a co-occurring
# real non-leak FAILED is never masked. (PR #36312: ShellRendererDoesNotLeakAfterNavigation
# FAIL→FAIL on iOS was wrongly reported "Fix does not pass the tests" / FAILED.)
$leakFlakyCandidates = @()
$nonLeakGenuineFail  = $false
foreach ($t in $AllDetectedTests) {
    $wo = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    $w  = $withFixResults    | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    if (-not $wo -or -not $w) { continue }
    # Only consider with-fix runs that GENUINELY failed (not already env/build/filter, not passing).
    if ($w.EnvError -or $w.BuildError -or $w.FilterMismatch -or $w.Passed) { continue }
    $wLeak   = [int]($w.LeakAssertCount)
    $woLeak  = [int]($wo.LeakAssertCount)
    $wFailed = [int]($w.Failed); if ($wFailed -le 0) { $wFailed = 1 }
    # Pure GC-leak flake: the leak assert is present in BOTH states (the bug under test IS a
    # leak) AND accounts for EVERY failing test in the with-fix run (so a non-leak failure
    # alongside it is never hidden).
    if (($wLeak -ge 1) -and ($woLeak -ge 1) -and ($wLeak -ge $wFailed) -and (-not $wo.Passed)) {
        $leakFlakyCandidates += $w
    } else {
        $nonLeakGenuineFail = $true
    }
}
if ($leakFlakyCandidates.Count -gt 0 -and -not $nonLeakGenuineFail) {
    foreach ($w in $leakFlakyCandidates) {
        $w.EnvError  = $true
        $w.LeakFlaky = $true
        $w.Error = "With-fix run only fails a GC memory-leak assertion (AssertionExtensions.WaitForGC: 'some are still alive'). This assert is non-deterministic — a correct fix can still leave a reference briefly uncollected — so a persistent leak FAIL is unverifiable by the gate, not proof the fix is broken. Verify the leak fix manually (heap snapshot / repeated runs)."
        Write-Host "  🧪 $($w.TestName): with-fix failure is a flaky GC memory-leak assert (leak signature in both states) — reclassifying as INCONCLUSIVE, not FAILED" -ForegroundColor Yellow
        Write-Log  "  $($w.TestName): with-fix GC-leak assert flaky — INCONCLUSIVE (not a fix failure)"
    }
}

$reproducingCount = 0
$withFixGenuineFailCount = 0
$bothNativeLibCount = 0
foreach ($t in $AllDetectedTests) {
    $wo = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    $w  = $withFixResults    | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    if (-not $wo -or -not $w) { continue }
    # A NATIVE shared-library load failure (libSkiaSharp etc.) that appears in BOTH the without-fix
    # AND with-fix runs is definitively environmental — the gate agent lacks the native runtime and
    # a C# fix can neither add nor remove a .so — so the test could not exercise the fixed code path
    # in either state. Exclude it from BOTH the repro count and the with-fix genuine-failure count so
    # it neither proves nor blocks the fix. Requiring the signature in BOTH states (not just one) is
    # the safe guard: a genuine assertion regression would differ between the runs, never present as
    # the identical missing-lib error in both. (build 14699033, PR #36653.)
    $bothNativeLib = [bool]$wo.NativeLibLoadFailure -and [bool]$w.NativeLibLoadFailure
    if ($bothNativeLib) { $bothNativeLibCount++ }
    # A with-fix NATIVE shared-library load failure (missing libSkiaSharp/libHarfBuzzSharp .so on
    # the gate agent) means the test HOST crashed before running the fixed code — the fix is
    # unverifiable via that test REGARDLESS of the without-fix leg. The $bothNativeLib guard above
    # only catches the case where BOTH legs hit the missing lib; it misses the (equally
    # environmental) case where the without-fix leg failed for a DIFFERENT reason — most commonly a
    # compile-coupled build error (new API + test in the same project), so the without-fix run
    # never reached the native-lib load at all. A genuine assertion regression never presents as a
    # DllNotFoundException, so reclassify a with-fix native-lib failure as env/INCONCLUSIVE.
    # (build 14850956, PR #35710: GenerateSplash* libSkiaSharp DllNotFound on the Linux android
    # gate; without-fix was compile-coupled Passed=False/Failed=0 so $bothNativeLib was false and
    # the with-fix native-lib failures were wrongly counted as a genuine FAILED.)
    if ([bool]$w.NativeLibLoadFailure -and -not $w.EnvError) {
        $w.EnvError = $true
        if (-not $w.Error) { $w.Error = "With-fix run failed to load a native shared library (e.g. libSkiaSharp/libHarfBuzzSharp) on the gate agent — the test host crashed before exercising the fix, so it is unverifiable here (environment, not a fix failure). Common for Resizetizer/Graphics image tests on a Linux (android) gate agent that lacks the SkiaSharp native runtime." }
        Write-Host "  🧩 $($t.TestName): with-fix failure is a native-library load error (missing .so on the gate agent) — reclassifying as INCONCLUSIVE, not FAILED" -ForegroundColor Yellow
    }
    $woInconclusive = $wo.EnvError -or $wo.BuildError -or $wo.FilterMismatch -or $bothNativeLib
    $wInconclusive  = $w.EnvError  -or $w.BuildError  -or $w.FilterMismatch  -or $bothNativeLib
    # FAIL → PASS: reproduces the bug and the fix resolves it.
    if ((-not $woInconclusive) -and (-not $wInconclusive) -and (-not $wo.Passed) -and $w.Passed) {
        $reproducingCount++
    }
    # A genuine failure that remains WITH the fix (FAIL→FAIL or a PASS→FAIL regression).
    if ((-not $wInconclusive) -and (-not $w.Passed)) {
        $withFixGenuineFailCount++
    }
}
$verificationPassed = ($reproducingCount -gt 0) -and ($withFixGenuineFailCount -eq 0)

# A test that hit an ENVIRONMENT error, or a BASELINE (without-fix) BUILD error, never
# established whether the bug reproduces, so the gate could not verify anything — treat that
# as INCONCLUSIVE (exit 3) so build/infra flakes don't masquerade as a broken fix.
#
# A with-fix-ONLY build error is different: the baseline compiles but the PR's own fix does
# NOT, which is a definitive FAILED (exit 1), not infra noise — so it must not be downgraded.
$baselineBuildError = (@($withoutFixResults) | Where-Object { $_.BuildError }).Count -gt 0
$withFixBuildError  = (@($withFixResults)    | Where-Object { $_.BuildError }).Count -gt 0
$anyEnvError        = (@($withoutFixResults) + @($withFixResults) | Where-Object { $_.EnvError }).Count -gt 0
# A FILTER MISMATCH (the -filter expression matched 0 test cases) means the deciding test
# never ran, so the gate verified NOTHING about it. This happens when the PR's test is
# platform-gated/excluded on the run platform (e.g. wrapped in #if TEST_FAILS_ON_ANDROID or a
# [Category] the run excludes) or the detected test name doesn't resolve in the built assembly.
# Both without-fix and with-fix then report "No test matches the given testcase filter" with
# Passed=False/Failed=0. Without routing this to INCONCLUSIVE the verdict falls through to a
# false FAILED (exit 1) even though no test executed — e.g. build 14634904 (#35998 android,
# Issue26049): both runs "No test matches ... 'Issue26049'", reported FAILED. Treat it as
# INCONCLUSIVE (exit 3), exactly like an env error — BUT only when there is no genuine failure
# remaining with the fix ($withFixGenuineFailCount -eq 0), so a real FAIL→FAIL in another
# detected test is never masked by an unrelated filter mismatch.
$anyFilterMismatch  = (@($withoutFixResults) + @($withFixResults) | Where-Object { $_.FilterMismatch }).Count -gt 0
# A baseline (without-fix) build error inside the PR's OWN detected test file is only a real
# FAILED when it is a GENUINELY BROKEN test — i.e. the test ALSO fails to build WITH the fix, so
# it breaks identically in both states (the original assumption). When the test build-errors
# WITHOUT the fix but compiles and PASSES WITH it, the error is compile-coupling: the PR adds
# new API AND a new test referencing it in the SAME test project, so reverting the fix
# un-compiles the test through no fault of its own. That leaves the without-fix RUNTIME
# behaviour UNVERIFIABLE -> INCONCLUSIVE (exit 3), never FAILED. (build 14662715, PR #36521:
# BindableObjectUnitTests referenced SetInheritedBindingContextForBinding, added by the fix ->
# CS0117/CS1061 WITHOUT the fix but PASSED 94/94 WITH it; was wrongly reported FAILED.)
$prTestBuildError   = $baselineBuildError -and (Test-BuildErrorIsInDetectedTest -Results $withoutFixResults -Tests $AllDetectedTests) -and (Test-BuildErrorIsInDetectedTest -Results $withFixResults -Tests $AllDetectedTests)
# Compile-coupled "new API / feature" PASS: the without-fix baseline build error is in the PR's
# OWN detected test (the test references API the fix introduces), the fix itself COMPILES (no
# with-fix build error), and every test runs and PASSES cleanly WITH the fix — no env/build/
# filter error and no genuine with-fix failure. The classic "fails without the fix" baseline is
# impossible for such a PR (reverting the fix un-compiles the test through no fault of its own),
# but a clean build+pass WITH the fix positively verifies the new functionality, so report a real
# PASS (exit 0) instead of a non-committal INCONCLUSIVE. Requires a genuinely clean with-fix run:
# a with-fix crash/env error (e.g. #36572's SIGABRT) keeps it INCONCLUSIVE. (PR #36572: MediaPicker
# ProcessImage — new API + test in the same project.)
$compileCoupledVerified = $baselineBuildError `
    -and (Test-BuildErrorIsInDetectedTest -Results $withoutFixResults -Tests $AllDetectedTests) `
    -and (-not $withFixBuildError) `
    -and (-not $anyEnvError) `
    -and (-not $anyFilterMismatch) `
    -and ($withFixGenuineFailCount -eq 0) `
    -and $passedWithFix
# A PLATFORM MISMATCH false-FAILED: every changed *code* file (fix files; test files excluded)
# is platform-specific for a DIFFERENT platform than this gate, so the fix is a no-op here and
# the repro test necessarily passes without it. Treat as INCONCLUSIVE (exit 3), like a filter
# mismatch — guarded by $withFixGenuineFailCount -eq 0 so a real FAIL->FAIL is never masked.
$fixPlatformMismatch = ($withFixGenuineFailCount -eq 0) -and (Test-FixIrrelevantToPlatform -FixFiles $FixFiles -Platform $Platform)
# A native shared-library load failure present in BOTH states (see loop above) is env-class: when
# it is the ONLY thing preventing a clean PASS (no genuine with-fix failure remains), the gate
# verified nothing → INCONCLUSIVE (exit 3), never a false FAILED. (PR #36653: a PR whose only
# detected test is an image/rasterization class that can't load libSkiaSharp on the gate agent.)
$hasDefinitiveGateFailure = Test-GateHasDefinitiveFailure `
    -WithFixGenuineFailCount $withFixGenuineFailCount `
    -WithFixBuildError $withFixBuildError `
    -BaselineBuildError $baselineBuildError `
    -PrTestBuildError $prTestBuildError
$gateInfraError = (-not $hasDefinitiveGateFailure) -and (
    $anyEnvError -or
    $anyFilterMismatch -or
    ($baselineBuildError -and -not $prTestBuildError -and -not $compileCoupledVerified) -or
    ($bothNativeLibCount -gt 0) -or
    $fixPlatformMismatch
)

Write-Log ""
Write-Log "Summary:"
Write-Log "  - Tests WITHOUT fix: $(if ($failedWithoutFix) { 'ALL FAIL ✅ (expected)' } else { 'SOME PASS ❌ (should all fail!)' })"
Write-Log "  - Tests WITH fix: $(if ($passedWithFix) { 'ALL PASS ✅ (expected)' } else { 'SOME FAIL ❌ (should all pass!)' })"

# Generate markdown report
Write-MarkdownReport `
    -VerificationPassed $verificationPassed `
    -CompileCoupledVerified $compileCoupledVerified `
    -FailedWithoutFix $failedWithoutFix `
    -PassedWithFix $passedWithFix `
    -WithoutFixResult $withoutFixResult `
    -WithFixResult $withFixResult `
    -WithoutFixResultsList $withoutFixResults `
    -WithFixResultsList $withFixResults `
    -Tests $AllDetectedTests `
    -ReportMergeBase $MergeBase `
    -ReportPlatform $Platform `
    -ReportBaseBranch $BaseBranchName `
    -ReportRevertableFiles $RevertableFiles `
    -ReportNewFiles $NewFiles

if ($verificationPassed) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║              VERIFICATION PASSED ✅                       ║" -ForegroundColor Green
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
    Write-Host "║  Tests correctly detect the issue:                        ║" -ForegroundColor Green
    Write-Host "║  - FAIL without fix (as expected)                         ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    exit 0
} elseif ($compileCoupledVerified) {
    # New-API / new-feature PR: the test references API the fix adds, so reverting the fix
    # un-compiles the baseline (no valid "fails without fix" state). The fix compiles and every
    # test PASSES cleanly WITH it, which positively verifies the new functionality → real PASS.
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║          VERIFICATION PASSED ✅ (new API / feature)       ║" -ForegroundColor Green
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
    Write-Host "║  Without-fix baseline was compile-coupled (test needs     ║" -ForegroundColor Green
    Write-Host "║  the fix's new API to compile); the fix builds and all    ║" -ForegroundColor Green
    Write-Host "║  tests PASS with it — new functionality verified.         ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    exit 0
} elseif ($gateInfraError) {
    # The deciding tests could not be built/run (build or environment error), so the gate
    # has NOT verified the fix. Report INCONCLUSIVE (exit 3) — not a real FAILED.
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
    Write-Host "║              VERIFICATION INCONCLUSIVE ⚠️                  ║" -ForegroundColor Yellow
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Yellow
    Write-Host "║  Tests could not be built/run (build or env error).       ║" -ForegroundColor Yellow
    Write-Host "║  The gate could not verify the fix — this is NOT a         ║" -ForegroundColor Yellow
    Write-Host "║  genuine test failure and must not block the PR.          ║" -ForegroundColor Yellow
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
    if ($fixPlatformMismatch) {
        Write-Host ""
        Write-Host "  * Fix targets a different platform than the '$Platform' gate — a no-op here, so the" -ForegroundColor Yellow
        Write-Host "    repro test passes with AND without the fix. Nothing is verifiable on this platform." -ForegroundColor Yellow
    }
    exit 3
} else {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
    Write-Host "║              VERIFICATION FAILED ❌                       ║" -ForegroundColor Red
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
    if (-not $failedWithoutFix) {
        Write-Host "║  Tests PASSED without fix (should fail)                   ║" -ForegroundColor Red
        Write-Host "║  - Tests don't actually detect the bug                    ║" -ForegroundColor Red
    }
    if (-not $passedWithFix) {
        Write-Host "║  Tests FAILED with fix (should pass)                      ║" -ForegroundColor Red
        Write-Host "║  - Fix doesn't resolve the issue or test is broken        ║" -ForegroundColor Red
    }
    if ($withFixBuildError -and -not $baselineBuildError) {
        Write-Host "║  - Fix does NOT compile (baseline builds fine) — this is  ║" -ForegroundColor Red
        Write-Host "║    a definitive failure, not a build/infra flake.         ║" -ForegroundColor Red
    }
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Possible causes:                                         ║" -ForegroundColor Red
    Write-Host "║  1. Wrong fix files specified                             ║" -ForegroundColor Red
    Write-Host "║  2. Tests don't actually test the fixed behavior          ║" -ForegroundColor Red
    Write-Host "║  3. The issue was already fixed in base branch            ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}
