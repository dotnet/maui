#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Unified test runner for .NET MAUI — run any test type from a single entry point.

.DESCRIPTION
    This script provides a single entry point for running all types of tests in the
    .NET MAUI repository: unit tests, device tests, UI tests, and integration tests.

    For unit tests, the script runs dotnet test directly.
    For device tests, UI tests, and integration tests, the script delegates to the
    appropriate existing scripts (Run-DeviceTests.ps1, BuildAndRunHostApp.ps1,
    Run-IntegrationTests.ps1).

.PARAMETER TestType
    The type of tests to run:
    - Unit:        Unit tests (runs on build machine via dotnet test)
    - Device:      Device tests (runs on device/emulator via xharness)
    - UI:          UI tests with Appium (builds HostApp, deploys, runs dotnet test)
    - Integration: Integration tests (template builds, workload validation)

.PARAMETER Project
    The project/module to test. Available values depend on TestType:

    Unit tests:   Controls, Xaml, BindingSourceGen, SourceGen, ControlsDesign,
                  Core, Essentials, Graphics, Resizetizer, Compatibility, DualScreen, AI
                  (omit to run ALL unit test projects)

    Device tests: Controls, Core, Essentials, Graphics, BlazorWebView, AI

.PARAMETER Platform
    Target platform (required for Device and UI tests):
    android, ios, catalyst (or maccatalyst), windows

.PARAMETER TestFilter
    Test filter expression passed to dotnet test --filter (e.g., "FullyQualifiedName~Issue12345")

.PARAMETER Category
    Test category filter. Converted to --filter "Category=<value>" for unit/UI tests,
    or passed as -Category for integration tests.
    Cannot be used together with -TestFilter.

.PARAMETER Configuration
    Build configuration: Debug or Release (default: Debug for Unit/UI, Release for Device)

.PARAMETER DeviceUdid
    Specific device UDID for Device or UI tests (optional — auto-detected if omitted)

.PARAMETER Rebuild
    Force a clean rebuild (no incremental build)

.PARAMETER List
    Show all available test types, projects, and usage examples, then exit.

.PARAMETER ResultsDirectory
    Directory for test results (default: artifacts/test-results)

.EXAMPLE
    ./RunTests.ps1 -List
    # Show all available test types and projects

.EXAMPLE
    ./RunTests.ps1 -TestType Unit -Project Controls
    # Run Controls unit tests

.EXAMPLE
    ./RunTests.ps1 -TestType Unit -Project Xaml -TestFilter "Maui12345"
    # Run a specific XAML unit test

.EXAMPLE
    ./RunTests.ps1 -TestType Unit
    # Run ALL unit test projects

.EXAMPLE
    ./RunTests.ps1 -TestType Device -Project Controls -Platform ios
    # Run Controls device tests on iOS simulator

.EXAMPLE
    ./RunTests.ps1 -TestType Device -Project Core -Platform android -TestFilter "Category=Button"
    # Run Core device tests on Android filtered by category

.EXAMPLE
    ./RunTests.ps1 -TestType UI -Platform android -TestFilter "FullyQualifiedName~Issue12345"
    # Run a specific UI test on Android

.EXAMPLE
    ./RunTests.ps1 -TestType UI -Platform ios -Category "SafeAreaEdges"
    # Run UI tests by category on iOS

.EXAMPLE
    ./RunTests.ps1 -TestType Integration -Category Build
    # Run integration tests for the Build category
#>

[CmdletBinding(DefaultParameterSetName = "Run")]
param(
    [Parameter(Mandatory = $true, ParameterSetName = "Run")]
    [ValidateSet("Unit", "Device", "UI", "Integration")]
    [string]$TestType,

    [Parameter(ParameterSetName = "Run")]
    [string]$Project,

    [Parameter(ParameterSetName = "Run")]
    [ValidateSet("android", "ios", "catalyst", "maccatalyst", "windows")]
    [string]$Platform,

    [Parameter(ParameterSetName = "Run")]
    [string]$TestFilter,

    [Parameter(ParameterSetName = "Run")]
    [string]$Category,

    [Parameter(ParameterSetName = "Run")]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration,

    [Parameter(ParameterSetName = "Run")]
    [string]$DeviceUdid,

    [Parameter(ParameterSetName = "Run")]
    [switch]$Rebuild,

    [Parameter(ParameterSetName = "Run")]
    [string]$ResultsDirectory,

    [Parameter(Mandatory = $true, ParameterSetName = "List")]
    [switch]$List
)

$ErrorActionPreference = "Stop"
$RepoRoot = Resolve-Path "$PSScriptRoot/../.."

# Import shared utilities
. "$PSScriptRoot/shared/shared-utils.ps1"

#region Project Definitions

# Unit test projects: Key → relative csproj path
$UnitTestProjects = [ordered]@{
    "Controls"          = "src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj"
    "Xaml"              = "src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj"
    "BindingSourceGen"  = "src/Controls/tests/BindingSourceGen.UnitTests/Controls.BindingSourceGen.UnitTests.csproj"
    "SourceGen"         = "src/Controls/tests/SourceGen.UnitTests/SourceGen.UnitTests.csproj"
    "ControlsDesign"    = "src/Controls/tests/Core.Design.UnitTests/Controls.Core.Design.UnitTests.csproj"
    "Core"              = "src/Core/tests/UnitTests/Core.UnitTests.csproj"
    "Essentials"        = "src/Essentials/test/UnitTests/Essentials.UnitTests.csproj"
    "Graphics"          = "src/Graphics/tests/Graphics.Tests/Graphics.Tests.csproj"
    "Resizetizer"       = "src/SingleProject/Resizetizer/test/UnitTests/Resizetizer.UnitTests.csproj"
    "Compatibility"     = "src/Compatibility/Core/tests/Compatibility.UnitTests/Compatibility.Core.UnitTests.csproj"
    "DualScreen"        = "src/Controls/Foldable/test/Controls.DualScreen.UnitTests.csproj"
    "AI"                = "src/AI/tests/Essentials.AI.UnitTests/Essentials.AI.UnitTests.csproj"
}

# Device test projects: Key → matches Run-DeviceTests.ps1 -Project parameter
$DeviceTestProjects = [ordered]@{
    "Controls"     = "src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj"
    "Core"         = "src/Core/tests/DeviceTests/Core.DeviceTests.csproj"
    "Essentials"   = "src/Essentials/test/DeviceTests/Essentials.DeviceTests.csproj"
    "Graphics"     = "src/Graphics/tests/DeviceTests/Graphics.DeviceTests.csproj"
    "BlazorWebView"= "src/BlazorWebView/tests/DeviceTests/MauiBlazorWebView.DeviceTests.csproj"
    "AI"           = "src/AI/tests/Essentials.AI.DeviceTests/Essentials.AI.DeviceTests.csproj"
}

# Integration test categories
$IntegrationCategories = @(
    "Build", "WindowsTemplates", "macOSTemplates", "Blazor",
    "MultiProject", "Samples", "AOT", "RunOnAndroid", "RunOniOS"
)

#endregion

#region List Mode

if ($List) {
    Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║        .NET MAUI Unified Test Runner                      ║
║        Available Test Types & Projects                    ║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

    # Unit Tests
    Write-Host "  Unit Tests  " -ForegroundColor Black -BackgroundColor Cyan
    Write-Host "  Runs on build machine via dotnet test. No device required." -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Project Key         Test Project" -ForegroundColor Yellow
    Write-Host "  ────────────────    ──────────────────────────────────────────" -ForegroundColor DarkGray
    foreach ($key in $UnitTestProjects.Keys) {
        Write-Host "  $($key.PadRight(20))" -ForegroundColor White -NoNewline
        Write-Host "$($UnitTestProjects[$key])" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "  Usage:" -ForegroundColor DarkCyan
    Write-Host "    ./RunTests.ps1 -TestType Unit -Project Controls" -ForegroundColor White
    Write-Host "    ./RunTests.ps1 -TestType Unit -Project Xaml -TestFilter `"Maui12345`"" -ForegroundColor White
    Write-Host "    ./RunTests.ps1 -TestType Unit                              # run ALL" -ForegroundColor White
    Write-Host ""

    # Device Tests
    Write-Host "  Device Tests  " -ForegroundColor Black -BackgroundColor Green
    Write-Host "  Runs on device/emulator via xharness. Requires -Platform." -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Project Key         Test Project" -ForegroundColor Yellow
    Write-Host "  ────────────────    ──────────────────────────────────────────" -ForegroundColor DarkGray
    foreach ($key in $DeviceTestProjects.Keys) {
        Write-Host "  $($key.PadRight(20))" -ForegroundColor White -NoNewline
        Write-Host "$($DeviceTestProjects[$key])" -ForegroundColor Gray
    }
    Write-Host ""
    Write-Host "  Platforms: android, ios, maccatalyst, windows" -ForegroundColor Gray
    Write-Host "  Usage:" -ForegroundColor DarkCyan
    Write-Host "    ./RunTests.ps1 -TestType Device -Project Controls -Platform ios" -ForegroundColor White
    Write-Host "    ./RunTests.ps1 -TestType Device -Project Core -Platform android -TestFilter `"Category=Button`"" -ForegroundColor White
    Write-Host ""

    # UI Tests
    Write-Host "  UI Tests  " -ForegroundColor Black -BackgroundColor Yellow
    Write-Host "  Appium-based UI automation. Requires -Platform and -TestFilter or -Category." -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Builds TestCases.HostApp, deploys to device, runs NUnit tests via Appium." -ForegroundColor Gray
    Write-Host "  Platforms: android, ios, catalyst, windows" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Usage:" -ForegroundColor DarkCyan
    Write-Host "    ./RunTests.ps1 -TestType UI -Platform android -TestFilter `"FullyQualifiedName~Issue12345`"" -ForegroundColor White
    Write-Host "    ./RunTests.ps1 -TestType UI -Platform ios -Category `"SafeAreaEdges`"" -ForegroundColor White
    Write-Host ""

    # Integration Tests
    Write-Host "  Integration Tests  " -ForegroundColor Black -BackgroundColor DarkYellow
    Write-Host "  Template builds, workload validation. Requires -Category or -TestFilter." -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Categories: $($IntegrationCategories -join ', ')" -ForegroundColor Gray
    Write-Host ""
    Write-Host "  Usage:" -ForegroundColor DarkCyan
    Write-Host "    ./RunTests.ps1 -TestType Integration -Category Build" -ForegroundColor White
    Write-Host "    ./RunTests.ps1 -TestType Integration -Category macOSTemplates" -ForegroundColor White
    Write-Host ""

    exit 0
}

#endregion

#region Parameter Validation

# Validate TestFilter and Category are not both specified
if ($TestFilter -and $Category) {
    throw "Cannot specify both -TestFilter and -Category. Use one or the other."
}

# Build the effective filter
$effectiveFilter = $null
if ($Category) {
    $effectiveFilter = "Category=$Category"
} elseif ($TestFilter) {
    $effectiveFilter = $TestFilter
}

# Set default Configuration per test type if not specified
if (-not $Configuration) {
    $Configuration = switch ($TestType) {
        "Device" { "Release" }
        default  { "Debug" }
    }
}

# Set default results directory
if (-not $ResultsDirectory) {
    $ResultsDirectory = Join-Path $RepoRoot "artifacts/test-results"
}

# Validate platform requirement for Device and UI tests
if ($TestType -eq "Device" -and -not $Platform) {
    throw "Device tests require -Platform (android, ios, maccatalyst, windows)"
}
if ($TestType -eq "UI" -and -not $Platform) {
    throw "UI tests require -Platform (android, ios, catalyst, windows)"
}

# Validate filter requirement for UI tests
if ($TestType -eq "UI" -and -not $effectiveFilter) {
    throw "UI tests require -TestFilter or -Category"
}

# Validate filter/category requirement for Integration tests
if ($TestType -eq "Integration" -and -not $effectiveFilter) {
    throw "Integration tests require -Category or -TestFilter"
}

# Validate Project for Device tests
if ($TestType -eq "Device") {
    if (-not $Project) {
        throw "Device tests require -Project ($($DeviceTestProjects.Keys -join ', '))"
    }
    if (-not $DeviceTestProjects.Contains($Project)) {
        throw "Unknown device test project '$Project'. Valid values: $($DeviceTestProjects.Keys -join ', ')"
    }
}

# Validate Project for Unit tests (if specified)
if ($TestType -eq "Unit" -and $Project -and -not $UnitTestProjects.Contains($Project)) {
    throw "Unknown unit test project '$Project'. Valid values: $($UnitTestProjects.Keys -join ', ')"
}

#endregion

#region Banner

$testTypeDisplay = switch ($TestType) {
    "Unit"        { "UNIT TESTS" }
    "Device"      { "DEVICE TESTS" }
    "UI"          { "UI TESTS" }
    "Integration" { "INTEGRATION TESTS" }
}

$projectDisplay = if ($Project) { $Project } elseif ($TestType -eq "Unit") { "ALL" } else { "-" }
$platformDisplay = if ($Platform) { $Platform.ToUpper() } else { "N/A" }
$filterDisplay = if ($effectiveFilter) { $effectiveFilter } else { "None" }

Write-Host @"

╔═══════════════════════════════════════════════════════════╗
║        .NET MAUI Unified Test Runner                      ║
╠═══════════════════════════════════════════════════════════╣
║  Test Type:    $($testTypeDisplay.PadRight(42))║
║  Project:      $($projectDisplay.PadRight(42))║
║  Platform:     $($platformDisplay.PadRight(42))║
║  Filter:       $($filterDisplay.Substring(0, [Math]::Min(42, $filterDisplay.Length)).PadRight(42))║
║  Config:       $($Configuration.PadRight(42))║
╚═══════════════════════════════════════════════════════════╝

"@ -ForegroundColor Magenta

#endregion

#region Run Unit Tests

function Invoke-UnitTests {
    param(
        [string]$ProjectKey,
        [string]$Filter,
        [string]$Config
    )

    # Determine which projects to run
    $projectsToRun = [ordered]@{}
    if ($ProjectKey) {
        $projectsToRun[$ProjectKey] = $UnitTestProjects[$ProjectKey]
    } else {
        $projectsToRun = $UnitTestProjects
    }

    Write-Step "Running $($projectsToRun.Count) unit test project(s)..."

    # Ensure results directory exists
    if (-not (Test-Path $ResultsDirectory)) {
        New-Item -Path $ResultsDirectory -ItemType Directory -Force | Out-Null
    }

    $totalPassed = 0
    $totalFailed = 0
    $totalSkipped = 0
    $failedProjects = @()
    $results = @()

    foreach ($key in $projectsToRun.Keys) {
        $projectPath = Join-Path $RepoRoot $projectsToRun[$key]

        if (-not (Test-Path $projectPath)) {
            Write-Warn "Project not found: $projectPath — skipping"
            continue
        }

        Write-Host ""
        Write-Host "─────────────────────────────────────────────────────" -ForegroundColor DarkGray
        Write-Step "Testing: $key"
        Write-Info "Project: $($projectsToRun[$key])"

        $testArgs = @(
            "test", $projectPath,
            "--configuration", $Config,
            "--no-restore",
            "--logger", "console;verbosity=normal",
            "--logger", "trx;LogFileName=$key.trx",
            "--results-directory", $ResultsDirectory
        )

        if ($Filter) {
            $testArgs += @("--filter", $Filter)
        }

        $startTime = Get-Date
        $output = & dotnet @testArgs 2>&1
        $exitCode = $LASTEXITCODE
        $duration = (Get-Date) - $startTime

        # Parse test counts from output
        # dotnet test outputs lines like:
        #   "Total tests: 65"
        #   "     Passed: 65"
        #   "     Failed: 2"
        #   "    Skipped: 3"
        # Or single-line: "Passed! - Failed: 0, Passed: 123, Skipped: 0, Total: 123"
        $passed = 0
        $failed = 0
        $skipped = 0

        foreach ($line in $output) {
            $lineStr = "$line"
            # Match standalone summary lines (with leading whitespace)
            if ($lineStr -match "^\s+Passed:\s*(\d+)") { $passed = [int]$Matches[1] }
            if ($lineStr -match "^\s+Failed:\s*(\d+)") { $failed = [int]$Matches[1] }
            if ($lineStr -match "^\s+Skipped:\s*(\d+)") { $skipped = [int]$Matches[1] }
            # Match single-line summary: "Failed: 0, Passed: 123, Skipped: 0"
            if ($lineStr -match "Failed:\s*(\d+),\s*Passed:\s*(\d+),\s*Skipped:\s*(\d+)") {
                $failed = [int]$Matches[1]
                $passed = [int]$Matches[2]
                $skipped = [int]$Matches[3]
            }
        }

        $totalPassed += $passed
        $totalFailed += $failed
        $totalSkipped += $skipped

        $result = @{
            Project  = $key
            Passed   = $passed
            Failed   = $failed
            Skipped  = $skipped
            Duration = $duration
            ExitCode = $exitCode
        }
        $results += $result

        if ($exitCode -ne 0) {
            $failedProjects += $key
            Write-Error "$key — FAILED ($failed failed, $passed passed) [$([math]::Round($duration.TotalSeconds))s]"
            # Show failure output
            $output | Where-Object { $_ -match "Failed|Error|Exception" } | Select-Object -First 20 | ForEach-Object {
                Write-Host "    $_" -ForegroundColor Red
            }
        } else {
            Write-Success "$key — PASSED ($passed passed, $skipped skipped) [$([math]::Round($duration.TotalSeconds))s]"
        }
    }

    # Summary
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "                    Unit Test Summary                       " -ForegroundColor Cyan
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""

    # Results table
    Write-Host "  Project               Passed   Failed   Skipped  Time" -ForegroundColor Yellow
    Write-Host "  ────────────────────  ───────  ───────  ───────  ─────" -ForegroundColor DarkGray
    foreach ($r in $results) {
        $statusColor = if ($r.ExitCode -eq 0) { "Green" } else { "Red" }
        $line = "  $($r.Project.PadRight(22)) $($r.Passed.ToString().PadLeft(7))  $($r.Failed.ToString().PadLeft(7))  $($r.Skipped.ToString().PadLeft(7))  $([math]::Round($r.Duration.TotalSeconds))s"
        Write-Host $line -ForegroundColor $statusColor
    }

    Write-Host "  ────────────────────  ───────  ───────  ───────  ─────" -ForegroundColor DarkGray
    $totalColor = if ($totalFailed -eq 0) { "Green" } else { "Red" }
    Write-Host "  $("TOTAL".PadRight(22)) $($totalPassed.ToString().PadLeft(7))  $($totalFailed.ToString().PadLeft(7))  $($totalSkipped.ToString().PadLeft(7))" -ForegroundColor $totalColor
    Write-Host ""

    if ($failedProjects.Count -gt 0) {
        Write-Host "  ❌ FAILED projects: $($failedProjects -join ', ')" -ForegroundColor Red
    } else {
        Write-Host "  ✅ All unit tests passed" -ForegroundColor Green
    }

    Write-Host ""
    Write-Host "  Results: $ResultsDirectory" -ForegroundColor Gray
    Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan

    return ($failedProjects.Count -eq 0)
}

#endregion

#region Run Device Tests

function Invoke-DeviceTests {
    $deviceTestScript = Join-Path $RepoRoot ".github/skills/run-device-tests/scripts/Run-DeviceTests.ps1"

    if (-not (Test-Path $deviceTestScript)) {
        Write-Warning "Device test script not found: $deviceTestScript"
        return $false
    }

    Write-Step "Delegating to Run-DeviceTests.ps1..."
    Write-Info "Project: $Project, Platform: $Platform"

    # Normalize platform for the device test script
    $devicePlatform = $Platform
    if ($devicePlatform -eq "catalyst") { $devicePlatform = "maccatalyst" }

    $params = @{
        Project       = $Project
        Platform      = $devicePlatform
        Configuration = $Configuration
    }

    if ($effectiveFilter) {
        $params.TestFilter = $effectiveFilter
    }
    if ($DeviceUdid) {
        $params.DeviceUdid = $DeviceUdid
    }

    & $deviceTestScript @params
    return ($LASTEXITCODE -eq 0)
}

#endregion

#region Run UI Tests

function Invoke-UITests {
    $uiTestScript = Join-Path $RepoRoot ".github/scripts/BuildAndRunHostApp.ps1"

    if (-not (Test-Path $uiTestScript)) {
        Write-Warning "UI test script not found: $uiTestScript"
        return $false
    }

    Write-Step "Delegating to BuildAndRunHostApp.ps1..."
    Write-Info "Platform: $Platform, Filter: $effectiveFilter"

    $params = @{
        Platform      = $Platform
        Configuration = $Configuration
    }

    if ($Category) {
        $params.Category = $Category
    } else {
        $params.TestFilter = $TestFilter
    }

    if ($DeviceUdid) {
        $params.DeviceUdid = $DeviceUdid
    }
    if ($Rebuild) {
        $params.Rebuild = $true
    }

    & $uiTestScript @params
    return ($LASTEXITCODE -eq 0)
}

#endregion

#region Run Integration Tests

function Invoke-IntegrationTests {
    $integrationScript = Join-Path $RepoRoot ".github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1"

    if (-not (Test-Path $integrationScript)) {
        Write-Warning "Integration test script not found: $integrationScript"
        return $false
    }

    Write-Step "Delegating to Run-IntegrationTests.ps1..."

    $params = @{
        Configuration = $Configuration
        SkipBuild     = $true
        SkipInstall   = $true
    }

    if ($Category) {
        $params.Category = $Category
    } elseif ($TestFilter) {
        $params.TestFilter = $TestFilter
    }

    if ($ResultsDirectory) {
        $params.ResultsDirectory = $ResultsDirectory
    }

    & $integrationScript @params
    return ($LASTEXITCODE -eq 0)
}

#endregion

#region Main Execution

$success = $false

switch ($TestType) {
    "Unit" {
        $success = Invoke-UnitTests -ProjectKey $Project -Filter $effectiveFilter -Config $Configuration
    }
    "Device" {
        $success = Invoke-DeviceTests
    }
    "UI" {
        $success = Invoke-UITests
    }
    "Integration" {
        $success = Invoke-IntegrationTests
    }
}

if (-not $success) {
    exit 1
}

#endregion
