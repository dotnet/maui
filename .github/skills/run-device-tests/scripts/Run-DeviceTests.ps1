<#
.SYNOPSIS
    Builds and runs .NET MAUI device tests locally using xharness (Apple/Android) or the Windows device-test app directly.

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
    # Comma/semicolon-separated fully-qualified test class names to run exclusively
    # (Android/iOS/MacCatalyst only). Additive include filter used by the Copilot review
    # gate to narrow a run to a PR's specific test class instead of its whole Category.
    [string]$IncludeClasses,

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

$WindowsDeviceTestPackageIds = @{
    "Controls"      = "Microsoft.Maui.Controls.DeviceTests"
    "Core"          = "Microsoft.Maui.Core.DeviceTests"
    "Essentials"    = "Microsoft.Maui.Essentials.DeviceTests"
    "Graphics"      = "Microsoft.Maui.Graphics.DeviceTests"
    "BlazorWebView" = "Microsoft.Maui.MauiBlazorWebView.DeviceTests"
}

function Get-CategoryFiltersFromTestFilter {
    param([string]$Filter)

    if ([string]::IsNullOrWhiteSpace($Filter)) {
        return @()
    }

    $categories = @()
    $matches = [regex]::Matches($Filter, '(?i)\bCategory\s*=\s*([^\|&(),]+)')
    foreach ($match in $matches) {
        $value = $match.Groups[1].Value.Trim().Trim('"', "'")
        if (-not [string]::IsNullOrWhiteSpace($value)) {
            $categories += $value
        }
    }

    if ($categories.Count -eq 0 -and $Filter -notmatch '[=~]') {
        $categories = @($Filter -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }

    return @($categories | Select-Object -Unique)
}

function Select-WindowsDeviceTestCategories {
    param(
        [string[]]$AllCategories,
        [string]$Filter
    )

    $filters = @(Get-CategoryFiltersFromTestFilter -Filter $Filter)
    if ($filters.Count -eq 0) {
        return @($AllCategories)
    }

    # Match each filter token EXACTLY first, falling back to substring matching only
    # when no category equals the token. A bare category name is frequently a substring
    # of many others — "View" is contained in BoxView, CarouselView, CollectionView,
    # ScrollView, WebView, … — so a naive substring match fans a single
    # "Category=View" filter out to every *View* category. That runs a dozen unrelated
    # categories (minutes of wasted device time) and, when their result files are
    # aggregated, previously surfaced as a spurious gate "ENV ERROR". Preferring an
    # exact match keeps "Category=View" scoped to the View category while still
    # allowing genuine partial filters (no exact category) to substring-match.
    $selected = [System.Collections.Generic.List[string]]::new()
    foreach ($token in $filters) {
        $exact = @($AllCategories | Where-Object { $_.Equals($token, [System.StringComparison]::OrdinalIgnoreCase) })
        $candidates = if ($exact.Count -gt 0) {
            $exact
        } else {
            @($AllCategories | Where-Object { $_.IndexOf($token, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 })
        }
        foreach ($c in $candidates) {
            if (-not $selected.Contains($c)) { $selected.Add($c) }
        }
    }

    # Return in discovery order for deterministic, stable output.
    return @($AllCategories | Where-Object { $selected.Contains($_) })
}

function Wait-ForPath {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path,

        [Parameter(Mandatory = $true)]
        [int]$TimeoutSeconds,

        [System.Diagnostics.Process]$Process
    )

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    while ($stopwatch.Elapsed.TotalSeconds -lt $TimeoutSeconds) {
        if (Test-Path $Path) {
            return $true
        }

        if ($Process -and $Process.HasExited) {
            Start-Sleep -Seconds 1
            if (Test-Path $Path) {
                return $true
            }
            return $false
        }

        Start-Sleep -Seconds 1
    }

    return (Test-Path $Path)
}

function ConvertTo-DeviceTestCount {
    <#
    .SYNOPSIS
        Coerces an xUnit result-XML count attribute to a non-negative [int], safely.
    .DESCRIPTION
        PowerShell's XML adapter returns an [object[]] for a property when the element
        exposes it more than once (e.g. an attribute AND a like-named child element).
        A direct [int](...) cast of that array throws
        "Cannot convert the ""System.Object[]"" value ... to type ""System.Int32""",
        which the gate surfaces as a spurious "ENV ERROR" with no results. Take the
        first value, tolerate nulls/blanks, and default to 0 so aggregation can never
        throw on an unexpected result-file shape.
    #>
    param($Value)

    if ($null -eq $Value) { return 0 }
    if ($Value -is [System.Array]) { $Value = @($Value)[0] }
    $parsed = 0
    if ([int]::TryParse([string]$Value, [ref]$parsed)) { return $parsed }
    return 0
}

function Get-WindowsDeviceTestResultSummary {
    param(
        [Parameter(Mandatory = $true)][string[]]$ResultFiles,

        # When set, the pass/fail tallies count ONLY tests whose fully-qualified name
        # belongs to one of these classes (comma/semicolon separated). Used to scope a
        # full-suite result file down to the class(es) under test — see the call site.
        [string]$IncludeClasses
    )

    $classList = @()
    if (-not [string]::IsNullOrWhiteSpace($IncludeClasses)) {
        $classList = @($IncludeClasses -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }

    $summary = @{
        Total = 0
        Passed = 0
        Failed = 0
        Skipped = 0
        Errors = 0
    }

    # Diagnostics for the class-filtered path: how many <test> nodes the file(s) held in
    # total (regardless of class) and a small sample of the DISTINCT classes they belong
    # to. When the class filter matches nothing, these disambiguate "the app produced no
    # results at all" from "the results are there but under classes we didn't expect" —
    # see the throw below.
    $diagTotalTests = 0
    $diagSampleClasses = [System.Collections.Generic.List[string]]::new()

    foreach ($file in $ResultFiles) {
        if (-not (Test-Path $file)) {
            continue
        }

        # The result file can be observed on disk (Test-Path true) a moment
        # before the device-test app has finished flushing its XML, so a naive
        # `Get-Content` can read empty or partial content. Casting null/blank
        # content to [xml] yields $null, and the subsequent .SelectNodes() call
        # throws the cryptic "You cannot call a method on a null-valued
        # expression" — which the gate surfaces as an opaque ENV ERROR with no
        # results (observed on Windows Controls device-test gates). Retry the
        # read briefly to absorb that write race (recovering the REAL results,
        # so a transient race no longer collapses to an inconclusive verdict),
        # then fail with a descriptive message if the file is genuinely empty or
        # malformed (e.g. the app crashed before writing results).
        $xml = $null
        for ($attempt = 1; $attempt -le 5; $attempt++) {
            $raw = Get-Content $file -Raw -ErrorAction SilentlyContinue
            if (-not [string]::IsNullOrWhiteSpace($raw)) {
                try {
                    $xml = [xml]$raw
                    break
                } catch {
                    # Partial/malformed XML — may still be mid-write; retry.
                    $xml = $null
                }
            }
            Start-Sleep -Milliseconds 500
        }

        if ($null -eq $xml) {
            throw "Windows device test result file '$file' is empty or not valid XML (the device-test app likely crashed or exited before writing results)."
        }

        if ($classList.Count -gt 0) {
            # Per-test counting, filtered to the class(es) under test. A full-suite result
            # file contains every test in the suite; counting only the requested classes
            # keeps the gate's A/B verdict focused on what the PR changed and immune to
            # unrelated/flaky suite failures.
            foreach ($test in @($xml.SelectNodes('//test'))) {
                # xUnit v2 records the fully-qualified CLASS in the `type` attribute and a
                # display/theory name in `name` (e.g. "PlatformView Transforms are not
                # empty(size: 1)"), so the class filter MUST match on `type`. Matching on
                # `name` misses every MAUI test that uses theory data or a [Fact]/[Theory]
                # DisplayName — which produced a false INCONCLUSIVE when the Core Windows
                # full run of 2090 tests reported 0 EntryHandlerTests even though they ran
                # (build 14695285, #36577). GetAttribute is used so the lookup is
                # unambiguous (avoids XmlElement's CLR .Name shadowing the `name` attribute)
                # and yields '' when the attribute is absent.
                $testType = $test.GetAttribute('type')
                $testName = $test.GetAttribute('name')
                if ([string]::IsNullOrWhiteSpace($testType) -and [string]::IsNullOrWhiteSpace($testName)) { continue }
                $diagTotalTests++
                # Sample DISTINCT class names (fall back to the raw name when a runner omits
                # `type`) so a no-match throw shows which classes the suite actually ran.
                $diagLabel = if (-not [string]::IsNullOrWhiteSpace($testType)) { $testType } else { $testName }
                if ($diagLabel -and $diagSampleClasses.Count -lt 8 -and -not $diagSampleClasses.Contains($diagLabel)) {
                    $diagSampleClasses.Add($diagLabel)
                }
                $isMatch = $false
                foreach ($cls in $classList) {
                    if ($testType -eq $cls -or
                        $testName -eq $cls -or
                        (-not [string]::IsNullOrWhiteSpace($testType) -and $testType.StartsWith("$cls.", [System.StringComparison]::Ordinal)) -or
                        (-not [string]::IsNullOrWhiteSpace($testName) -and $testName.StartsWith("$cls.", [System.StringComparison]::Ordinal))) {
                        $isMatch = $true
                        break
                    }
                }
                if (-not $isMatch) { continue }

                $summary.Total++
                switch ([string]$test.GetAttribute('result')) {
                    'Pass' { $summary.Passed++ }
                    'Fail' { $summary.Failed++ }
                    'Skip' { $summary.Skipped++ }
                    default { }
                }
            }
        }
        else {
            $assemblies = @($xml.SelectNodes('/assemblies/assembly'))
            foreach ($assembly in $assemblies) {
                $summary.Total += ConvertTo-DeviceTestCount $assembly.total
                $summary.Passed += ConvertTo-DeviceTestCount $assembly.passed
                $summary.Failed += ConvertTo-DeviceTestCount $assembly.failed
                $summary.Skipped += ConvertTo-DeviceTestCount $assembly.skipped
                $summary.Errors += ConvertTo-DeviceTestCount $assembly.errors
            }
        }
    }

    if ($classList.Count -gt 0 -and $summary.Total -eq 0) {
        # The class(es) under test produced no results in the suite output — treat this as
        # an environment/harness error (INCONCLUSIVE) rather than silently reporting a
        # false pass (0 failed) for tests that never actually ran. Include diagnostics so
        # the two distinct causes are distinguishable from the gate log:
        #   * total <test> nodes = 0  -> the app produced no results (crash/early exit)
        #   * total > 0 but no match  -> results exist under classes we didn't expect
        #     (namespace/name-format mismatch, or the target class was not in this suite).
        $sample = if ($diagSampleClasses.Count -gt 0) { " Sample classes present: " + ($diagSampleClasses -join '; ') + '.' } else { '' }
        throw "Windows device test result file(s) contained no tests for class(es) '$IncludeClasses' (the target tests did not run). Total tests found in result file(s): $diagTotalTests.$sample"
    }

    return $summary
}

function Invoke-WindowsDeviceTestApp {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AppPath,

        [Parameter(Mandatory = $true)]
        [string]$Project,

        [Parameter(Mandatory = $true)]
        [string]$AppName,

        [Parameter(Mandatory = $true)]
        [string]$OutputDirectory,

        [string]$TestFilter,

        [string]$IncludeClasses,

        [string]$Timeout = "01:00:00"
    )

    $timeoutSeconds = [int][TimeSpan]::Parse($Timeout).TotalSeconds
    if ($timeoutSeconds -le 0) {
        $timeoutSeconds = 3600
    }

    if (-not (Test-Path $OutputDirectory)) {
        New-Item -ItemType Directory -Path $OutputDirectory -Force | Out-Null
    }

    $packageId = $WindowsDeviceTestPackageIds[$Project]
    if (-not $packageId) {
        $packageId = $AppName
    }

    $resultBase = Join-Path $OutputDirectory "TestResults-$($packageId.Replace('.', '_'))"
    $resultFile = "$resultBase.xml"
    $categoriesFile = Join-Path $OutputDirectory "devicetestcategories.txt"
    Remove-Item -LiteralPath $categoriesFile -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "$resultBase*.xml" -Force -ErrorAction SilentlyContinue

    $resultFiles = @()

    # Decide whether to drive the app via per-category discovery/index runs instead of a
    # single full-suite launch:
    #   - Controls: ALWAYS. Its Windows app registers only the discovery/index runner, so
    #     a plain full launch has no runner and exits without results.
    #   - Core/Essentials/Graphics/BlazorWebView: only when a filter is supplied. Their
    #     apps gained the discovery/index runner once AppHostBuilderExtensions
    #     .UseHeadlessRunner registers it on Windows, so we can now run ONLY the changed
    #     category instead of launching the entire app — which, for large suites like
    #     Core, can crash/exit before writing results and collapse the gate to an
    #     inconclusive "empty results" verdict (see PR #36577).
    #
    # Only the Controls device-test app registers the discovery-capable
    # ControlsHeadlessTestRunner (MauiProgramDefaults.cs gates UseControlsHeadlessRunner on
    # the Controls.DeviceTests assembly). Core/Essentials/Graphics/etc. use the generic
    # HeadlessTestRunner, which has NO category discovery — so a discovery attempt there is
    # guaranteed to time out after 120s AND leaves a half-written result file from the
    # killed discovery process, before the inevitable full-suite fallback. Skip discovery
    # entirely for those apps: go straight to a clean full run and scope the summary to the
    # class(es) under test. Controls keeps strict discovery (throw on failure) because it
    # has no full-suite runner to fall back to.
    $requireDiscovery = ($Project -eq "Controls")
    $attemptDiscovery = $requireDiscovery
    $useCategoryFiltering = $false
    if ($attemptDiscovery) {
        Write-Host "Discovering Windows device test categories..." -ForegroundColor Gray
        $discoveryProcess = Start-Process -FilePath $AppPath -ArgumentList @($resultFile, "-1") -PassThru
        if (Wait-ForPath -Path $categoriesFile -TimeoutSeconds 120 -Process $discoveryProcess) {
            $useCategoryFiltering = $true
        } else {
            if ($discoveryProcess -and -not $discoveryProcess.HasExited) {
                Stop-Process -Id $discoveryProcess.Id -Force -ErrorAction SilentlyContinue
            }
            if ($requireDiscovery) {
                throw "Windows device test category discovery did not create $categoriesFile"
            }
            Write-Warning "Windows '$Project' device test app did not produce a category list within 120s; falling back to a full device-test run."
        }
    } elseif ($TestFilter) {
        Write-Host "Windows '$Project' device test app has no category discovery; running the full suite and scoping results to the class(es) under test." -ForegroundColor Gray
    }

    if ($useCategoryFiltering) {
        $allCategories = @(Get-Content $categoriesFile | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
        $selectedCategories = @(Select-WindowsDeviceTestCategories -AllCategories $allCategories -Filter $TestFilter)
        if ($selectedCategories.Count -eq 0) {
            throw "Test filter '$TestFilter' matched 0 Windows device test categories. Available categories: $($allCategories -join ', ')"
        }

        Write-Host "Running $($selectedCategories.Count) of $($allCategories.Count) Windows device test categor$(if ($selectedCategories.Count -eq 1) { 'y' } else { 'ies' }): $($selectedCategories -join ', ')" -ForegroundColor Yellow

        foreach ($category in $selectedCategories) {
            $categoryIndex = [Array]::IndexOf($allCategories, $category)
            if ($categoryIndex -lt 0) {
                throw "Could not find category '$category' in discovered category list."
            }

            $categoryResultFile = "$resultBase`_$category.xml"
            Remove-Item -LiteralPath $categoryResultFile -Force -ErrorAction SilentlyContinue
            Write-Host "Running Windows device test category '$category' (index $categoryIndex)..." -ForegroundColor Gray
            $process = Start-Process -FilePath $AppPath -ArgumentList @($resultFile, [string]$categoryIndex) -PassThru
            if (-not (Wait-ForPath -Path $categoryResultFile -TimeoutSeconds $timeoutSeconds -Process $process)) {
                if ($process -and -not $process.HasExited) {
                    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                }
                throw "Windows device test category '$category' did not create $categoryResultFile"
            }

            $resultFiles += $categoryResultFile
        }
    } else {
        # Full-suite run: either no filter was requested, or category discovery was not
        # available for this app. Remove any partial result file a failed discovery
        # attempt may have left behind so the summary reflects only this run.
        Remove-Item -LiteralPath $resultFile -Force -ErrorAction SilentlyContinue

        Write-Host "Running Windows device test app directly..." -ForegroundColor Gray
        $process = Start-Process -FilePath $AppPath -ArgumentList @($resultFile) -PassThru

        # A full-suite app creates its single results file and finalizes it only when the
        # whole run completes, so waiting for the file to merely APPEAR (as the per-category
        # path can, because each category file is written at that category's completion)
        # races the writer and reads an empty/partial XML — surfacing as a false
        # "empty or not valid XML" ENV ERROR even though the run is healthy (PR #36577: the
        # Core Windows full run was read at 247s while it was still executing). Wait for the
        # process to EXIT instead, mirroring how eng/devices/windows.cake launches the
        # unpackaged app with a blocking StartProcess and only then checks the result file.
        if (-not $process.WaitForExit($timeoutSeconds * 1000)) {
            if (-not $process.HasExited) {
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }
            throw "Windows device test app did not exit within ${timeoutSeconds}s while running the full suite."
        }
        if (-not (Test-Path $resultFile)) {
            throw "Windows device test app exited without creating $resultFile"
        }

        $resultFiles += $resultFile
    }

    # When a full-suite fallback ran (no per-category isolation available for this app —
    # e.g. a Core/Essentials/Graphics app built from a PR tree that predates the
    # discovery-runner registration), the result file holds EVERY test in the suite, not
    # just the changed area. Narrow the pass/fail summary to the class(es) under test so
    # the gate's A/B verdict reflects only the tests the PR actually changed instead of
    # being polluted (or falsely reddened) by unrelated/flaky suite tests. Category-
    # isolated runs already scope the result file, so they keep the whole-file aggregate.
    $summaryClassFilter = if (-not $useCategoryFiltering) { $IncludeClasses } else { $null }
    $summary = Get-WindowsDeviceTestResultSummary -ResultFiles $resultFiles -IncludeClasses $summaryClassFilter
    $script:WindowsDeviceTestSummary = $summary
    $script:WindowsDeviceTestResultFiles = $resultFiles

    if (($summary.Failed + $summary.Errors) -eq 0) {
        return 0
    }

    return 1
}

# Android package names (lowercase)
$AndroidPackageNames = @{
    "Controls"      = "com.microsoft.maui.controls.devicetests"
    "Core"          = "com.microsoft.maui.core.devicetests"
    "Essentials"    = "com.microsoft.maui.essentials.devicetests"
    "Graphics"      = "com.microsoft.maui.graphics.devicetests"
    "BlazorWebView" = "com.microsoft.maui.mauiblazorwebview.devicetests"
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
        RuntimeIdentifier = "win-x64"
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

# Align device-test TargetFrameworks with the checked-out branch (e.g. net11.0-android on the
# net11.0 branch) instead of the hardcoded net10.0 defaults in $PlatformConfigs above.
$DotNetTfm = Get-MauiTfmVersion -RepoRoot $RepoRoot
foreach ($plat in @($PlatformConfigs.Keys)) {
    $PlatformConfigs[$plat].Tfm = $PlatformConfigs[$plat].Tfm -replace '^net\d+\.\d+', "net$DotNetTfm"
}

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
    # Derive artifact folder name from the project file name.
    $artifactName = [System.IO.Path]::GetFileNameWithoutExtension($projectPath)
    
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
    if ($IncludeClasses) {
        Write-Host "Include Class: $IncludeClasses" -ForegroundColor Yellow
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
    # NOTE: For Windows we deliberately do NOT pass `-r` here; RuntimeIdentifierOverride
    # is set in the windows-specific block below to ensure the RID propagates to ALL
    # referenced projects (e.g. TestUtils.DeviceTests). Plain `-r` is suppressed on
    # non-leaf project references and causes PRI/asset file resolution failures.
    if ($platformConfig.RuntimeIdentifier -and $Platform -ne "windows") {
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
            # NOTE: WindowsAppSDKSelfContained MUST NOT be passed via command line because it
            # propagates to ALL referenced projects (including library dependencies like
            # Graphics.csproj) and breaks them with:
            #   "WindowsAppSDKSelfContained requires a supported Windows architecture"
            # Instead, pass _MauiDeviceTestUnpackaged=true. The
            # Microsoft.Maui.TestUtils.DeviceTests.Runners.props file (imported from each
            # device test csproj) converts that signal into WindowsAppSDKSelfContained=true
            # ONLY on the device test project itself.
            #
            # Also: use RuntimeIdentifierOverride (NOT `-r`/RuntimeIdentifier) so the RID
            # propagates to every ProjectReference (e.g. TestUtils.DeviceTests). Plain
            # RuntimeIdentifier is auto-suppressed on non-leaf project references, which
            # leaves dependency PRI/asset files in the non-RID output folder while the
            # test app itself is built at the RID-specific path, producing PRI175 errors.
            #
            # See eng/devices/windows.cake (buildOnly task, lines 145-188) for the
            # canonical CI pattern.
            $buildArgs += "/p:RuntimeIdentifierOverride=$($platformConfig.RuntimeIdentifier)"
            $buildArgs += "/p:WindowsPackageType=None"
            $buildArgs += "/p:SelfContained=true"
            $buildArgs += "/p:_MauiDeviceTestUnpackaged=true"
            $buildArgs += "/p:UseMonoRuntime=false"
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
            $appPath = "artifacts/bin/$artifactName/$Configuration/$tfmFolder/$ridFolder/$appName.app"
        }
        "maccatalyst" {
            # MacCatalyst apps may have different names - search for .app bundle
            $appSearchPath = "artifacts/bin/$artifactName/$Configuration/$tfmFolder/$ridFolder"
            $appBundle = Get-ChildItem -Path $appSearchPath -Filter "*.app" -Directory -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($appBundle) {
                $appPath = $appBundle.FullName
            } else {
                $appPath = "$appSearchPath/$appName.app"
            }
        }
        "android" {
            # Android APK path - look for signed APK
            $apkSearchPath = "artifacts/bin/$artifactName/$Configuration/$tfmFolder"
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
            $exeSearchPath = "artifacts/bin/$artifactName/$Configuration/$tfmFolder"
            $exeFile = Get-ChildItem -Path $exeSearchPath -Filter "$appName.exe" -Recurse -ErrorAction SilentlyContinue | Select-Object -First 1
            if ($exeFile) {
                $appPath = $exeFile.FullName
            } else {
                $appPath = "$exeSearchPath/$ridFolder/$appName.exe"
            }
        }
    }
    
    if (-not (Test-Path $appPath)) {
        Write-Error "Built app not found at: $appPath"
        Write-Info "Searching for app in artifacts..."
        Get-ChildItem -Path "artifacts/bin/$artifactName" -Recurse -ErrorAction SilentlyContinue | 
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
                $androidPackageName = $AndroidPackageNames[$Project]
                $xharnessArgs = @(
                    "android", "test"
                    "--app", $appPath
                    "--package-name", $androidPackageName
                    "--device-id", $deviceUdidToUse
                    "-o", $OutputDirectory
                    "--timeout", $Timeout
                    "-v"
                )
            }
        }

        if ($TestFilter) {
            if ($Platform -eq "android") {
                # Android uses --arg for instrumentation arguments
                $xharnessArgs += "--arg", "TestFilter=$TestFilter"
            } else {
                # iOS/MacCatalyst uses --set-env
                $xharnessArgs += "--set-env=TestFilter=$TestFilter"
            }
        }

        if ($IncludeClasses) {
            if ($Platform -eq "android") {
                $xharnessArgs += "--arg", "IncludeClasses=$IncludeClasses"
            } else {
                $xharnessArgs += "--set-env=IncludeClasses=$IncludeClasses"
            }
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
        # WINDOWS DEVICE TEST EXECUTION
        # ═══════════════════════════════════════════════════════════

        Write-Host "Running Windows device test app directly..." -ForegroundColor Gray
        Write-Host "This matches eng/devices/windows.cake and avoids VSTest/testhost for MAUI Windows device apps." -ForegroundColor Gray
        Write-Host ""

        $testExitCode = Invoke-WindowsDeviceTestApp `
            -AppPath $appPath `
            -Project $Project `
            -AppName $appName `
            -OutputDirectory $OutputDirectory `
            -TestFilter $TestFilter `
            -IncludeClasses $IncludeClasses `
            -Timeout $Timeout

        if ($script:WindowsDeviceTestSummary) {
            Write-Host ""
            Write-Output "  Passed: $($script:WindowsDeviceTestSummary.Passed)"
            Write-Output "  Failed: $($script:WindowsDeviceTestSummary.Failed + $script:WindowsDeviceTestSummary.Errors)"
            Write-Output "  Skipped: $($script:WindowsDeviceTestSummary.Skipped)"
            Write-Output "  Total: $($script:WindowsDeviceTestSummary.Total)"
            Write-Host "  Result file(s): $($script:WindowsDeviceTestResultFiles -join ', ')" -ForegroundColor Gray
        }
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
        
        # Use Write-Output for results so they're captured by callers (not just Write-Host)
        Write-Host ""
        Write-Output "  Passed: $passCount"
        Write-Output "  Failed: $failCount"
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
        Write-Output "  Tests completed successfully"
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Green
    } else {
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
        Write-Output "  Tests completed with exit code: $testExitCode"
        Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Yellow
    }

    exit $testExitCode

} finally {
    Pop-Location
}
