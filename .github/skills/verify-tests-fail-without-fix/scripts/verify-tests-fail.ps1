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
                    Write-Host "❌ Failed to boot device" -ForegroundColor Red
                    exit 1
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

            $testArgs = @(
                "test", $projectPath,
                "--configuration", "Debug",
                "--logger", "console;verbosity=normal"
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

            $testArgs = @(
                "test", $projectPath,
                "--configuration", "Debug",
                "--logger", "console;verbosity=normal"
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

            $deviceParams = @{
                Project       = $deviceProject
                Platform      = $devicePlatform
                # Build device tests in DEBUG, not Release. The gate only needs to verify test
                # BEHAVIOUR (does it fail without the fix, pass with it) — not Release/AOT/trim.
                # On iOS/MacCatalyst, Release does FULL ILLink trimming (links every assembly),
                # which both massively slows the build (the gate builds twice per test) and
                # maximizes the chance of hitting the ILLink "IL1012 IL Trimmer has encountered
                # an unexpected error" crash — surfacing as an INCONCLUSIVE that has nothing to
                # do with the PR (e.g. dotnet/maui#36328, #35892). Debug matches the UI-test /
                # HostApp path above, which already builds --configuration Debug.
                Configuration = "Debug"
            }

            # Pass filter through — detection ensures it's Category= format
            if ($Filter) {
                $deviceParams.TestFilter = $Filter
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
function Invoke-TestRunWithRetry {
    param(
        [hashtable]$TestEntry,
        [string]$LogFile,
        [int]$MaxRetries = 3
    )

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

        $testOutputLog = Invoke-TestRun `
            -DetectedTestType $TestEntry.Type `
            -Filter $TestEntry.Filter `
            -DetectedProject $TestEntry.Project `
            -DetectedProjectPath $TestEntry.ProjectPath `
            -LogFile $logFileAttempt

        $result = Get-TestResultFromOutput -LogFile $testOutputLog -TestFilter $TestEntry.Filter

        # A missing snapshot baseline is deterministic (the baseline PNG simply isn't in the
        # repo yet) — retrying re-runs the whole test for the same guaranteed result. Return
        # immediately so it flows straight to the INCONCLUSIVE classification without burning
        # retry attempts (and device reboots).
        if (-not $result.EnvError -or $result.SnapshotBaselineMissing) {
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
                return @{
                    Passed = $false; FailCount = $deviceFailCount; Failed = $deviceFailCount
                    PassCount = $devicePassCount; Total = $deviceTotal; Skipped = 0
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
    )
    foreach ($envErr in $envErrorPatterns) {
        if ($content -match $envErr.Pattern) {
            return @{ Passed = $false; EnvError = $true; Error = $envErr.Message; FailCount = 0; Failed = 0; Total = 0; Skipped = 0 }
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

    $results = & $DetectTestsScript @params 6>$null
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
    foreach ($t in $Tests) {
        $base = (($t.TestName -split ' \(')[0]).Trim()
        if ($base -and $errText -match [regex]::Escape($base)) { return $true }
    }
    return $false
}

function Write-MarkdownReport {
    param(
        [bool]$VerificationPassed,
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

    # A baseline build error located in the PR's OWN test file is NOT pre-existing/infra — the
    # PR broke its own test (test files are never reverted). Attribute it to the PR (FAILED),
    # not INCONCLUSIVE.
    $prTestBuildError = $baselineBuildError -and (Test-BuildErrorIsInDetectedTest -Results $WithoutFixResultsList -Tests $Tests)

    # A FILTER MISMATCH (0 tests matched the -filter) on a deciding test means nothing was
    # verified, so the headline must read INCONCLUSIVE to match the exit code ($gateInfraError).
    # Apply the SAME guard as the exit-code logic: only downgrade to INCONCLUSIVE when NO genuine
    # failure remains with the fix, so a real FAIL→FAIL in another detected test is never masked
    # by an unrelated filter mismatch.
    $hasFilterMismatch = (@($WithoutFixResultsList) + @($WithFixResultsList) | Where-Object { $_.FilterMismatch }).Count -gt 0
    $reportWithFixGenuineFail = $false
    foreach ($gt in $Tests) {
        $woG = $WithoutFixResultsList | Where-Object { $_.TestName -eq $gt.TestName } | Select-Object -First 1
        $wG  = $WithFixResultsList    | Where-Object { $_.TestName -eq $gt.TestName } | Select-Object -First 1
        if (-not $woG -or -not $wG) { continue }
        $wGInc = $wG.EnvError -or $wG.BuildError -or $wG.FilterMismatch
        if ((-not $wGInc) -and (-not $wG.Passed)) { $reportWithFixGenuineFail = $true }
    }

    # Platform-affinity FALSE-FAILED guard (mirror of the exit-code $fixPlatformMismatch):
    # when every changed code file targets a DIFFERENT platform than this gate, the fix is a
    # no-op here, so "passes without fix" is expected -> INCONCLUSIVE, not FAILED.
    $fixFilesForPlatform = @($ReportRevertableFiles) + @($ReportNewFiles)
    $fixPlatformMismatch = (-not $reportWithFixGenuineFail) -and (Test-FixIrrelevantToPlatform -FixFiles $fixFilesForPlatform -Platform $ReportPlatform)

    $status = if ($VerificationPassed) { "✅ PASSED" } elseif ($prTestBuildError) { "❌ FAILED" } elseif ($hasEnvError -or $baselineBuildError -or ($hasFilterMismatch -and -not $reportWithFixGenuineFail) -or $fixPlatformMismatch) { "⚠️ INCONCLUSIVE" } else { "❌ FAILED" }
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
    $snapshotNote = if ($snapshotBaselineMissing) {
        "📷 **New snapshot test — no baseline yet** — the test calls ``VerifyScreenshot`` but its baseline image is not committed (brand-new snapshot tests get their baseline added separately). The gate cannot validate a snapshot with nothing to compare against, so this is **inconclusive, not a fix failure**. Download the ``snapshots-diff`` artifact, confirm the rendering, and commit the baseline PNG."
    } elseif ($snapshotEnvResidual) {
        "📷 **Environmental snapshot residual — not a fix failure** — with the fix applied, the only remaining ``VerifyScreenshot`` differences are no larger than the WITHOUT-fix run (the fix worsened no snapshot and added no new failing one) and are all below ~1%. The fix resolves the bug's visual difference; the residual is a constant cross-agent baseline offset (anti-aliasing / font hinting differ between the machine that captured the baseline and this agent), so this is **inconclusive, not a fix failure**. Regenerate the affected baseline PNG(s) on the target agent."
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
    if (-not $hasEnvError -and -not $VerificationPassed -and -not $fixPlatformMismatch -and $WithoutFixResultsList -and $WithFixResultsList) {
        # Build error in the with-fix run trumps every other classification — if
        # the fix doesn't compile, no per-test outcome is meaningful.
        $wBuildError    = @($WithFixResultsList    | Where-Object { $_.BuildError })
        $woBuildError   = @($WithoutFixResultsList | Where-Object { $_.BuildError })
        $wFilterMiss    = @($WithFixResultsList    | Where-Object { $_.FilterMismatch })
        $woFilterMiss   = @($WithoutFixResultsList | Where-Object { $_.FilterMismatch })

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
        for ($i = 0; $i -lt $woStates.Count -and $i -lt $wStates.Count; $i++) {
            if ($woStates[$i] -eq "FAIL" -and $wStates[$i] -eq "PASS") { $hasFixedTest = $true }
        }

        if ($woBuildError.Count -gt 0) {
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
                $failureClassification = "🩺 **Base branch does not compile** — the without-fix build failed. The gate's ""does the test fail without the fix"" check is unreliable here; this usually means ``main`` is broken or a merge-base file went missing. Investigate before trusting this gate.$woExcerptLine"
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
        } elseif ($allWoPass) {
            $failureClassification = "🩺 **Test does not reproduce the bug** — ran the same in both states (PASS without fix, PASS with fix). The repro test is not exercising the issue. Strengthen the test before reviewing the fix."
        } elseif ($allWoFail -and $allWFail) {
            $failureClassification = "🩺 **Fix does not pass the tests** — every test still fails after applying the fix. The PR's change does not resolve the failure(s)."
        } elseif ($hasFixedTest -and $hasRegression) {
            $failureClassification = "🩺 **Regression in another test** — at least one test goes FAIL→PASS (fix works there), but another test FAILs both with and without the fix. The fix breaks a pre-existing or sibling test."
        } elseif ($hasRegression -and -not $hasFixedTest) {
            $failureClassification = "🩺 **Fix breaks tests** — one or more tests fail with the fix applied, and none of the failures are resolved by the fix."
        }
        # else: leave $failureClassification unset; the per-test table + Failure Details below tell the story.
    }

    $lines = @()
    $lines += "### Gate Result: $status"
    $lines += ""
    $platformDisplay = if ($ReportPlatform) { $ReportPlatform.ToUpper() } else { "N/A" }
    $lines += "**Platform:** $platformDisplay · **Base:** $ReportBaseBranch · **Merge base:** ``$mergeBaseShort``"
    if ($mixedPassNote) {
        $lines += ""
        $lines += $mixedPassNote
    }
    if ($snapshotNote) {
        $lines += ""
        $lines += $snapshotNote
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
                # Truncate if too large for a PR comment (GitHub limit ~65k chars total)
                if ($logContent.Length -gt 15000) {
                    $logContent = $logContent.Substring($logContent.Length - 15000)
                    $lines += "*(truncated to last 15,000 chars)*"
                    $lines += ""
                }
                $lines += '```'
                $lines += $logContent
                $lines += '```'
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
                if ($logContent.Length -gt 15000) {
                    $logContent = $logContent.Substring($logContent.Length - 15000)
                    $lines += "*(truncated to last 15,000 chars)*"
                    $lines += ""
                }
                $lines += '```'
                $lines += $logContent
                $lines += '```'
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
    # Check if file has uncommitted changes (staged or unstaged)
    $status = git status --porcelain -- $file 2>$null
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

# Step 1: Revert fix files to merge-base state
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 1: Reverting fix files to merge-base ($($MergeBase.Substring(0, 8)))"
Write-Log "=========================================="

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

foreach ($file in $RevertableFiles) {
    if ($DeletedByPrFiles -contains $file) {
        # The PR deleted this file; its with-fix state is "absent". STEP 1
        # restored it from the merge-base for the baseline run, so re-delete it
        # (worktree + index) to match HEAD — `git checkout HEAD -- $file` would
        # fail here because HEAD has no copy of a PR-deleted file.
        Write-Log "  Re-removing (deleted by PR): $file"
        git rm -f --ignore-unmatch -- $file 2>&1 | Out-Null
        $wtPath = Join-Path $RepoRoot $file
        if (Test-Path $wtPath) { Remove-Item -LiteralPath $wtPath -Force -ErrorAction SilentlyContinue }
    } else {
        Write-Log "  Restoring: $file"
        $gitOutput = git checkout HEAD -- $file 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Log "  ERROR: Failed to restore $file from HEAD"
            Write-Log "  Git output: $gitOutput"
            exit 1
        }
    }
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) restored from HEAD"

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
    $buildOut = Invoke-WithoutGhTokens { & dotnet build $projFull -c Debug -t:Rebuild 2>&1 }
    $buildExit = $LASTEXITCODE
    $combined = @($buildOut)
    if ($buildExit -eq 0) {
        $testOut = Invoke-WithoutGhTokens { & dotnet test $projFull -c Debug --logger "console;verbosity=normal" --filter $retryEntry.Filter 2>&1 }
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
}

$reproducingCount = 0
$withFixGenuineFailCount = 0
foreach ($t in $AllDetectedTests) {
    $wo = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    $w  = $withFixResults    | Where-Object { $_.TestName -eq $t.TestName } | Select-Object -First 1
    if (-not $wo -or -not $w) { continue }
    $woInconclusive = $wo.EnvError -or $wo.BuildError -or $wo.FilterMismatch
    $wInconclusive  = $w.EnvError  -or $w.BuildError  -or $w.FilterMismatch
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
# A baseline build error inside the PR's OWN test file is the PR's fault (test files are never
# reverted, so it breaks identically in both states) — a real FAILED, not an infra flake. Keep
# it OUT of $gateInfraError so it exits 1 (FAILED), matching the report status.
$prTestBuildError   = $baselineBuildError -and (Test-BuildErrorIsInDetectedTest -Results $withoutFixResults -Tests $AllDetectedTests)
# A PLATFORM MISMATCH false-FAILED: every changed *code* file (fix files; test files excluded)
# is platform-specific for a DIFFERENT platform than this gate, so the fix is a no-op here and
# the repro test necessarily passes without it. Treat as INCONCLUSIVE (exit 3), like a filter
# mismatch — guarded by $withFixGenuineFailCount -eq 0 so a real FAIL->FAIL is never masked.
$fixPlatformMismatch = ($withFixGenuineFailCount -eq 0) -and (Test-FixIrrelevantToPlatform -FixFiles $FixFiles -Platform $Platform)
$gateInfraError     = $anyEnvError -or ($anyFilterMismatch -and $withFixGenuineFailCount -eq 0) -or ($baselineBuildError -and -not $prTestBuildError) -or $fixPlatformMismatch

Write-Log ""
Write-Log "Summary:"
Write-Log "  - Tests WITHOUT fix: $(if ($failedWithoutFix) { 'ALL FAIL ✅ (expected)' } else { 'SOME PASS ❌ (should all fail!)' })"
Write-Log "  - Tests WITH fix: $(if ($passedWithFix) { 'ALL PASS ✅ (expected)' } else { 'SOME FAIL ❌ (should all pass!)' })"

# Generate markdown report
Write-MarkdownReport `
    -VerificationPassed $verificationPassed `
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
