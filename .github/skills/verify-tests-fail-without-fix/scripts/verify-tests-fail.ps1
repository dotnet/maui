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
    [Parameter(Mandatory = $true)]
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
            Write-Error "Could not auto-detect PR number. Please provide -PRNumber parameter."
            exit 1
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
    "Essentials.AI.UnitTests"          = "src/AI/tests/Essentials.AI.UnitTests/Essentials.AI.UnitTests.csproj"
}

# Maps device test project keys to the -Project parameter of Run-DeviceTests.ps1
$script:DeviceTestProjectMap = @{
    "Controls.DeviceTests"             = "Controls"
    "Core.DeviceTests"                 = "Core"
    "Essentials.DeviceTests"           = "Essentials"
    "Graphics.DeviceTests"             = "Graphics"
    "MauiBlazorWebView.DeviceTests"    = "BlazorWebView"
    "Essentials.AI.DeviceTests"        = "AI"
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
            & $buildScript @uiParams 2>&1 | Tee-Object -FilePath $LogFile
            return Join-Path $RepoRoot "CustomAgentLogsTmp/UITests/test-output.log"
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

            & dotnet @testArgs 2>&1 | Tee-Object -FilePath $LogFile
            # Also save just the test output for result parsing
            Copy-Item -Path $LogFile -Destination $testOutputFile -Force
            return $testOutputFile
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

            & dotnet @testArgs 2>&1 | Tee-Object -FilePath $LogFile
            Copy-Item -Path $LogFile -Destination $testOutputFile -Force
            return $testOutputFile
        }

        "DeviceTest" {
            if (-not $Platform) {
                Write-Host "❌ Device tests require -Platform (android, ios, maccatalyst, windows)" -ForegroundColor Red
                exit 1
            }

            $devicePlatform = if ($Platform -eq "catalyst") { "maccatalyst" } else { $Platform }
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
                Configuration = "Release"
            }

            # Pass filter through — detection ensures it's Category= format
            if ($Filter) {
                $deviceParams.TestFilter = $Filter
            }

            if ($script:BootedDeviceUdid -and $script:BootedDeviceUdid -ne "host") {
                $deviceParams.DeviceUdid = $script:BootedDeviceUdid
            }

            $scriptOutput = & $deviceTestScript @deviceParams 2>&1
            $scriptOutput | Out-File -FilePath $LogFile -Force -Encoding utf8
            $scriptOutput | ForEach-Object { Write-Host $_ }
            if (Test-Path $LogFile) {
                Copy-Item -Path $LogFile -Destination $testOutputFile -Force
            }

            return $testOutputFile
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

        $testOutputLog = Invoke-TestRun `
            -DetectedTestType $TestEntry.Type `
            -Filter $TestEntry.Filter `
            -DetectedProject $TestEntry.Project `
            -DetectedProjectPath $TestEntry.ProjectPath `
            -LogFile $logFileAttempt

        $result = Get-TestResultFromOutput -LogFile $testOutputLog -TestFilter $TestEntry.Filter

        if (-not $result.EnvError) {
            return $result
        }

        if ($attempt -lt $MaxRetries) {
            Write-Host "  ⚠️ Environment error (attempt $attempt/$MaxRetries): $($result.Error) — retrying..." -ForegroundColor Yellow
            Start-Sleep -Seconds 5
        } else {
            Write-Host "  ⚠️ Environment error persisted after $MaxRetries attempts: $($result.Error)" -ForegroundColor Yellow
            return $result
        }
    }
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

    # Check for environment/infrastructure errors (not real test failures)
    $envErrorPatterns = @(
        @{ Pattern = "error ADB0010.*InstallFailedException"; Message = "App install failed (ADB broken pipe)" }
        @{ Pattern = "APP_LAUNCH_FAILURE|exit code:?\s*83"; Message = "App failed to launch (XHarness exit 83)" }
        @{ Pattern = "SIGABRT.*load_aot_module"; Message = "App crashed during AOT loading" }
        @{ Pattern = "AppiumServerHasNotBeenStartedLocally"; Message = "Appium server failed to start" }
        @{ Pattern = "no such element.*could not be located"; Message = "Test element not found (app may not have loaded)" }
    )
    foreach ($envErr in $envErrorPatterns) {
        if ($content -match $envErr.Pattern) {
            return @{ Passed = $false; EnvError = $true; Error = $envErr.Message; FailCount = 0; Failed = 0; Total = 0; Skipped = 0 }
        }
    }

    # Check for build failures (before any test results)
    if ($content -match "Build FAILED" -or $content -match "Build failed with exit code" -or $content -match "error MSB\d+" -or $content -match "error CS\d+") {
        return @{ Passed = $false; Error = "Build failed before tests could run"; FailCount = 0; Failed = 0; Total = 0; Skipped = 0 }
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
            Write-Host "❌ Could not auto-detect test filter. No test files found in changed files." -ForegroundColor Red
            Write-Host "   Searched for: UI tests, unit tests, XAML tests, device tests" -ForegroundColor Yellow
            Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
            exit 1
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
    $hasErrors = ($allResults | Where-Object { $_.Error }).Count -gt 0

    if ($hasErrors) {
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
        Write-Host "║              ERROR PARSING TEST RESULTS                   ║" -ForegroundColor Red
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
        foreach ($r in ($allResults | Where-Object { $_.Error })) {
            Write-Host "  [$($r.TestType)] $($r.TestName): $($r.Error)" -ForegroundColor Red
        }
        exit 1
    }

    # Show per-test results
    foreach ($r in $allResults) {
        $icon = switch ($r.TestType) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }
        if (-not $r.Passed) {
            Write-Host "  $icon [$($r.TestType)] $($r.TestName): FAILED ✅ (expected)" -ForegroundColor Green
        } else {
            Write-Host "  $icon [$($r.TestType)] $($r.TestName): PASSED ❌ (should fail!)" -ForegroundColor Red
        }
    }
    Write-Host ""

    if ($allFailed) {
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
        Write-Host "║              VERIFICATION PASSED ✅                       ║" -ForegroundColor Green
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
        Write-Host "║  All $($allResults.Count) test(s) FAILED as expected!                      ║" -ForegroundColor Green
        Write-Host "║  This proves the tests correctly reproduce the bug.       ║" -ForegroundColor Green
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
        exit 0
    } else {
        $passedCount = ($allResults | Where-Object { $_.Passed }).Count
        Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Red
        Write-Host "║              VERIFICATION FAILED ❌                       ║" -ForegroundColor Red
        Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Red
        Write-Host "║  $passedCount/$($allResults.Count) test(s) PASSED but should FAIL!                   ║" -ForegroundColor Red
        Write-Host "║  Those tests don't reproduce the bug. Revise them!        ║" -ForegroundColor Red
        Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
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
        Write-Host "❌ Could not auto-detect test filter. No test files found in changed files." -ForegroundColor Red
        Write-Host "   Searched for: UI tests, unit tests, XAML tests, device tests" -ForegroundColor Yellow
        Write-Host "   Please provide -TestFilter parameter explicitly." -ForegroundColor Yellow
        exit 1
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

function Write-MarkdownReport {
    param(
        [bool]$VerificationPassed,
        [bool]$FailedWithoutFix,
        [bool]$PassedWithFix,
        [hashtable]$WithoutFixResult,
        [hashtable]$WithFixResult
    )
    
    # Check for environment errors in results
    $hasEnvError = ($withoutFixResults | Where-Object { $_.EnvError }) -or ($withFixResults | Where-Object { $_.EnvError })
    
    $status = if ($hasEnvError) { "⚠️ ENV ERROR" } elseif ($VerificationPassed) { "✅ PASSED" } else { "❌ FAILED" }
    $mergeBaseShort = if ($MergeBase -and $MergeBase.Length -ge 8) { $MergeBase.Substring(0, 8) } else { "$MergeBase" }

    $lines = @()
    $lines += "### Gate Result: $status"
    $lines += ""
    $lines += "**Platform:** $($Platform.ToUpper())"
    $lines += ""
    $lines += "#### Tests Detected"
    $lines += ""
    $lines += "| # | Type | Test Name | Filter |"
    $lines += "|---|------|-----------|--------|"
    $i = 0
    foreach ($t in $AllDetectedTests) {
        $i++
        $lines += "| $i | $($t.Type) | $($t.TestName) | ``$($t.Filter)`` |"
    }
    $lines += ""
    $lines += "#### Verification"
    $lines += ""
    $lines += "| Step | Expected | Actual | Result |"
    $lines += "|------|----------|--------|--------|"

    # Without fix row
    $woResult = $withoutFixResults | Select-Object -First 1
    if ($woResult.EnvError) {
        $lines += "| Without fix | FAIL | ENV ERROR | ⚠️ |"
    } elseif ($FailedWithoutFix) {
        $lines += "| Without fix | FAIL | FAIL | ✅ |"
    } else {
        $lines += "| Without fix | FAIL | PASS | ❌ |"
    }

    # With fix row
    $wResult = $withFixResults | Select-Object -First 1
    if ($wResult.EnvError) {
        $lines += "| With fix | PASS | ENV ERROR | ⚠️ |"
    } elseif ($PassedWithFix) {
        $lines += "| With fix | PASS | PASS | ✅ |"
    } else {
        $lines += "| With fix | PASS | FAIL | ❌ |"
    }

    # Show failure details if any
    $allResults = @($withoutFixResults) + @($withFixResults)
    $detailLines = @()
    foreach ($r in $allResults) {
        if ($r.EnvError -and $r.Error) {
            $detailLines += "- ⚠️ **Environment error:** $($r.Error)"
        }
        if ($r.FailureReason) {
            $detailLines += "- ❌ **Failed:** $($r.FailureReason)"
        }
        if ($r.FailureMessage) {
            $detailLines += "- 📋 **Error:** $($r.FailureMessage)"
        }
    }
    if ($detailLines.Count -gt 0) {
        $lines += ""
        $lines += "#### Details"
        $lines += ""
        $lines += ($detailLines | Select-Object -Unique)
    }

    $lines += ""
    $lines += "#### Fix Files Reverted"
    $lines += ""
    foreach ($f in $RevertableFiles) {
        $lines += "- ``$f``"
    }
    if ($NewFiles.Count -gt 0) {
        $lines += ""
        $lines += "#### New Files (Not Reverted)"
        $lines += ""
        foreach ($f in $NewFiles) {
            $lines += "- ``$f``"
        }
    }
    $lines += ""
    $lines += "---"
    $lines += ""
    $lines += "**Base Branch:** $BaseBranchName | **Merge Base:** $mergeBaseShort"

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

# Verify fix files exist
Write-Log "Verifying fix files exist..."
foreach ($file in $FixFiles) {
    $fullPath = Join-Path $RepoRoot $file
    if (-not (Test-Path $fullPath)) {
        Write-Log "ERROR: Fix file not found: $file"
        exit 1
    }
    Write-Log "  ✓ $file exists"
}

# Determine which files exist at the merge-base (can be reverted)
Write-Log ""
Write-Log "Checking which fix files exist at merge-base ($($MergeBase.Substring(0, 8)))..."
$RevertableFiles = @()
$NewFiles = @()

foreach ($file in $FixFiles) {
    # Check if file exists at merge-base commit
    $existsInBase = git ls-tree -r $MergeBase --name-only -- $file 2>$null

    if ($existsInBase) {
        $RevertableFiles += $file
        Write-Log "  ✓ $file (exists at merge-base - will revert)"
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

# Step 2: Run ALL tests WITHOUT fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 2: Running tests WITHOUT fix (should FAIL)"
Write-Log "=========================================="

$withoutFixResults = @()
$testIndex = 0
foreach ($testEntry in $AllDetectedTests) {
    $testIndex++
    $icon = switch ($testEntry.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }
    Write-Log ""
    Write-Log "--- Test $testIndex/$($AllDetectedTests.Count): $icon [$($testEntry.Type)] $($testEntry.TestName) ---"

    $sanitizedName = ($testEntry.TestName -replace '[^a-zA-Z0-9_\-\.]', '_')
    if ($sanitizedName.Length -gt 60) { $sanitizedName = $sanitizedName.Substring(0, 60) }
    $testLogFile = Join-Path $OutputPath "test-without-fix-$sanitizedName.log"

    $result = Invoke-TestRunWithRetry -TestEntry $testEntry -LogFile $testLogFile
    $result.TestName = $testEntry.TestName
    $result.TestType = $testEntry.Type
    $withoutFixResults += $result

    if ($result.EnvError) {
        Write-Log "  ⚠️ $($testEntry.TestName) ENV ERROR without fix: $($result.Error)"
    } elseif (-not $result.Passed) {
        Write-Log "  ✅ $($testEntry.TestName) FAILED without fix (expected)"
    } else {
        Write-Log "  ❌ $($testEntry.TestName) PASSED without fix (unexpected!)"
    }
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
    Write-Log "  Restoring: $file"
    $gitOutput = git checkout HEAD -- $file 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Log "  ERROR: Failed to restore $file from HEAD"
        Write-Log "  Git output: $gitOutput"
        exit 1
    }
}

Write-Log "  ✓ $($RevertableFiles.Count) fix file(s) restored from HEAD"

# Step 4: Run ALL tests WITH fix
Write-Log ""
Write-Log "=========================================="
Write-Log "STEP 4: Running tests WITH fix (should PASS)"
Write-Log "=========================================="

$withFixResults = @()
$testIndex = 0
foreach ($testEntry in $AllDetectedTests) {
    $testIndex++
    $icon = switch ($testEntry.Type) { "UITest" { "🖥️" } "DeviceTest" { "📱" } "UnitTest" { "🧪" } "XamlUnitTest" { "📄" } default { "❓" } }
    Write-Log ""
    Write-Log "--- Test $testIndex/$($AllDetectedTests.Count): $icon [$($testEntry.Type)] $($testEntry.TestName) ---"

    $sanitizedName = ($testEntry.TestName -replace '[^a-zA-Z0-9_\-\.]', '_')
    if ($sanitizedName.Length -gt 60) { $sanitizedName = $sanitizedName.Substring(0, 60) }
    $testLogFile = Join-Path $OutputPath "test-with-fix-$sanitizedName.log"

    $result = Invoke-TestRunWithRetry -TestEntry $testEntry -LogFile $testLogFile
    $result.TestName = $testEntry.TestName
    $result.TestType = $testEntry.Type
    $withFixResults += $result

    if ($result.EnvError) {
        Write-Log "  ⚠️ $($testEntry.TestName) ENV ERROR with fix: $($result.Error)"
    } elseif ($result.Passed) {
        Write-Log "  ✅ $($testEntry.TestName) PASSED with fix (expected)"
    } else {
        Write-Log "  ❌ $($testEntry.TestName) FAILED with fix (unexpected!)"
    }
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
Write-Log ""
Write-Log "=========================================="
Write-Log "VERIFICATION RESULTS"
Write-Log "=========================================="

$verificationPassed = $false
# "Without fix" should FAIL → all tests should NOT pass
$failedWithoutFix = ($withoutFixResults | Where-Object { $_.Passed }).Count -eq 0
# "With fix" should PASS → all tests should pass
$passedWithFix = ($withFixResults | Where-Object { -not $_.Passed }).Count -eq 0

Write-Log ""
Write-Log "Per-test results:"
foreach ($t in $AllDetectedTests) {
    $woResult = $withoutFixResults | Where-Object { $_.TestName -eq $t.TestName }
    $wResult = $withFixResults | Where-Object { $_.TestName -eq $t.TestName }
    $woStatus = if ($woResult -and -not $woResult.Passed) { "FAIL ✅" } else { "PASS ❌" }
    $wStatus = if ($wResult -and $wResult.Passed) { "PASS ✅" } else { "FAIL ❌" }
    Write-Log "  [$($t.Type)] $($t.TestName): without fix=$woStatus, with fix=$wStatus"
}

if ($failedWithoutFix) {
    Write-Log ""
    Write-Log "✅ All tests FAILED without fix (expected - issues detected)"
} else {
    Write-Log ""
    Write-Log "❌ Some tests PASSED without fix (unexpected!)"
}

if ($passedWithFix) {
    Write-Log "✅ All tests PASSED with fix (expected - fixes work)"
} else {
    Write-Log "❌ Some tests FAILED with fix (unexpected!)"
}

$verificationPassed = $failedWithoutFix -and $passedWithFix

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
    -WithFixResult $withFixResult

if ($verificationPassed) {
    Write-Host ""
    Write-Host "╔═══════════════════════════════════════════════════════════╗" -ForegroundColor Green
    Write-Host "║              VERIFICATION PASSED ✅                       ║" -ForegroundColor Green
    Write-Host "╠═══════════════════════════════════════════════════════════╣" -ForegroundColor Green
    Write-Host "║  Tests correctly detect the issue:                        ║" -ForegroundColor Green
    Write-Host "║  - FAIL without fix (as expected)                         ║" -ForegroundColor Green
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Green
    exit 0
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
    Write-Host "║                                                           ║" -ForegroundColor Red
    Write-Host "║  Possible causes:                                         ║" -ForegroundColor Red
    Write-Host "║  1. Wrong fix files specified                             ║" -ForegroundColor Red
    Write-Host "║  2. Tests don't actually test the fixed behavior          ║" -ForegroundColor Red
    Write-Host "║  3. The issue was already fixed in base branch            ║" -ForegroundColor Red
    Write-Host "╚═══════════════════════════════════════════════════════════╝" -ForegroundColor Red
    exit 1
}
