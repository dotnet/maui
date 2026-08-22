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

.PARAMETER Rebuild
    Rebuilds the full project-reference graph instead of using incremental outputs.

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
    [switch]$Rebuild,

    [Parameter(Mandatory = $false)]
    [string]$TestFilter,

    [Parameter(Mandatory = $false)]
    # Comma/semicolon-separated fully-qualified test class names to run exclusively
    # (Android/iOS/MacCatalyst/Windows). Additive include filter used by the Copilot review
    # gate to narrow a run to a PR's specific test class instead of its whole Category.
    [string]$IncludeClasses,

    [Parameter(Mandatory = $false)]
    # Comma/semicolon-separated test METHOD names (e.g. "CompletedFiresOnRealEnterKeyPress").
    # Additive post-hoc result scoping within -IncludeClasses. On Windows full-suite
    # fallbacks and XHarness class-isolated runs, only these methods contribute to the
    # Gate pass/fail tally, so an unrelated sibling failure cannot falsely redden the
    # A/B verdict. Empty = fall back to whole-class scoping.
    [string]$IncludeMethods,

    [Parameter(Mandatory = $false)]
    [switch]$BuildOnly,

    [Parameter(Mandatory = $false)]
    [string]$OutputDirectory = "artifacts/log",

    [Parameter(Mandatory = $false)]
    [string]$Timeout = "01:00:00",

    [Parameter(Mandatory = $false)]
    [string]$DeviceUdid,

    [Parameter(Mandatory = $false)]
    [string]$RepositoryRoot,

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

$WindowsDeviceNoResultsMarker = "WINDOWS_DEVICE_TEST_NO_RESULTS:"
$WindowsDeviceTargetTimeoutMarker = "WINDOWS_DEVICE_TEST_TARGET_TIMEOUT:"

function ConvertTo-AzdoSafeConsole {
    param([string]$Text)

    # Test result XML is PR-controlled. Collapse line separators so an entity-decoded
    # newline cannot create a column-zero command, then defang Azure command prefixes.
    return ($Text -replace '[\r\n\f\v]+', ' ') -replace '##(?=\[|vso\[)', '## '
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

function ConvertTo-DeviceTestClassFilterValue {
    param([string]$Value)

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }

    if ($Value.Length -gt 32768) {
        throw "Device test class filter is too long."
    }

    $classNames = [System.Collections.Generic.List[string]]::new()
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

    foreach ($candidate in $Value -split '[,;]') {
        $className = $candidate.Trim()
        if ([string]::IsNullOrWhiteSpace($className)) {
            continue
        }
        if ($className -match '[\x00-\x1F\x7F]') {
            throw "Device test class filter contains a control character."
        }
        if ($className.Length -gt 512) {
            throw "Device test class name is too long."
        }
        if ($seen.Add($className)) {
            if ($classNames.Count -ge 64) {
                throw "Device test class filter contains more than 64 classes."
            }
            $classNames.Add($className)
        }
    }

    if ($classNames.Count -eq 0) {
        return $null
    }

    # XHarness ApplicationOptions parses NUNIT_SKIPPED_CLASSES as comma-separated.
    # Despite the historical environment-variable name, these are INCLUDE filters:
    # ConfigureRunnerFilters sets RunAllTestsByDefault=false and calls
    # SkipClass(className, isExcluded: false) for each value.
    return [string]::Join(',', $classNames)
}

function New-AndroidDeviceTestClassFilterInjection {
    param(
        [Parameter(Mandatory = $true)]
        [string]$IncludeClasses,

        [Parameter(Mandatory = $true)]
        [string]$TempRoot
    )

    if ([string]::IsNullOrWhiteSpace($IncludeClasses)) {
        throw "A non-empty class filter is required for Android class-filter injection."
    }

    $directory = Join-Path ([System.IO.Path]::GetFullPath($TempRoot)) "maui-device-test-class-filter-$([guid]::NewGuid().ToString('N'))"
    $sourcePath = Join-Path $directory "MauiCopilotClassFilter.g.cs"
    $targetsPath = Join-Path $directory "MauiCopilotClassFilter.targets"
    $typeSuffix = [guid]::NewGuid().ToString("N")
    $encodedClassNames = [Convert]::ToBase64String([Text.Encoding]::UTF8.GetBytes($IncludeClasses))
    $utf8NoBom = [Text.UTF8Encoding]::new($false)

    $sourceContent = @"
#nullable enable
#pragma warning disable CA2255
internal static class MauiCopilotClassFilter_$typeSuffix
{
    [global::System.Runtime.CompilerServices.ModuleInitializer]
    internal static void Initialize()
    {
        var classNames = global::System.Text.Encoding.UTF8.GetString(
            global::System.Convert.FromBase64String("$encodedClassNames"));
        global::System.Environment.SetEnvironmentVariable("NUNIT_SKIPPED_CLASSES", classNames);
        global::System.Console.WriteLine("[Maui Copilot Gate] XHarness class filter: " + classNames);
    }
}
"@

    # The custom C# targets hook is a command-line global property, so it is evaluated
    # for project references too. Scope the generated source to the shared runner project:
    # its module initializer runs as soon as MauiTestInstrumentation is loaded, before
    # XHarness first constructs ApplicationOptions.Current and reads the environment.
    $targetsContent = @'
<Project>
  <ItemGroup Condition="'$(MSBuildProjectName)' == '$(MauiCopilotClassFilterTargetProject)'">
    <Compile Include="$(MauiCopilotClassFilterSourcePath)" Link="MauiCopilotClassFilter.g.cs" />
  </ItemGroup>
</Project>
'@

    try {
        New-Item -ItemType Directory -Path $directory -Force | Out-Null
        [System.IO.File]::WriteAllText($sourcePath, $sourceContent, $utf8NoBom)
        [System.IO.File]::WriteAllText($targetsPath, $targetsContent, $utf8NoBom)
    } catch {
        Remove-Item -LiteralPath $directory -Recurse -Force -ErrorAction SilentlyContinue
        throw
    }

    return [pscustomobject]@{
        Directory = $directory
        SourcePath = $sourcePath
        TargetsPath = $targetsPath
        TargetProject = "TestUtils.DeviceTests.Runners"
    }
}

function Get-XHarnessTestResultSnapshot {
    param(
        [string]$OutputDirectory,
        [string]$ResultFileName = "testResults.xml"
    )

    $snapshot = @{}
    if (-not (Test-Path $OutputDirectory -PathType Container)) {
        return $snapshot
    }

    foreach ($file in @(Get-ChildItem -Path $OutputDirectory -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like $ResultFileName })) {
        $snapshot[$file.FullName] = "$($file.Length):$($file.LastWriteTimeUtc.Ticks)"
    }

    return $snapshot
}

function Get-FreshXHarnessTestResultFiles {
    param(
        [string]$OutputDirectory,
        [hashtable]$BeforeSnapshot,
        [string]$ResultFileName = "testResults.xml"
    )

    if (-not (Test-Path $OutputDirectory -PathType Container)) {
        return @()
    }

    $freshFiles = [System.Collections.Generic.List[string]]::new()
    foreach ($file in @(Get-ChildItem -Path $OutputDirectory -File -Recurse -ErrorAction SilentlyContinue |
            Where-Object { $_.Name -like $ResultFileName })) {
        $fingerprint = "$($file.Length):$($file.LastWriteTimeUtc.Ticks)"
        if (-not $BeforeSnapshot.ContainsKey($file.FullName) -or $BeforeSnapshot[$file.FullName] -ne $fingerprint) {
            $freshFiles.Add($file.FullName)
        }
    }

    return @($freshFiles)
}

function New-XHarnessRunOutputDirectory {
    param(
        [Parameter(Mandatory = $true)]
        [string]$OutputDirectory
    )

    $root = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
    if (-not (Test-Path $root -PathType Container)) {
        New-Item -ItemType Directory -Path $root -Force | Out-Null
    }

    # Gate retries intentionally reuse their top-level diagnostics directory. XHarness
    # also stores its own logs there and can rediscover a prior instrumentation result
    # path when a later launch produces no result. Keep every invocation isolated so
    # neither old logs nor old XML can masquerade as evidence from the current run.
    $runDirectory = Join-Path $root "xharness-run-$([guid]::NewGuid().ToString('N'))"
    New-Item -ItemType Directory -Path $runDirectory -Force -ErrorAction Stop | Out-Null
    return $runDirectory
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

function Test-WindowsDeviceTestCategoryDiscovery {
    param(
        [string]$Project,
        [string]$TestFilter,
        [string]$IncludeClasses
    )

    # Controls registers only the discovery/index runner on Windows. Other projects
    # also register the normal full-suite runner, which XHarness can class-filter
    # directly through NUNIT_SKIPPED_CLASSES. Prefer that reliable path whenever the
    # Gate supplied an exact class, and retain category discovery only for standalone
    # filtered runs that lack class metadata.
    return $Project -eq "Controls" -or (
        [string]::IsNullOrWhiteSpace($IncludeClasses) -and
        -not [string]::IsNullOrWhiteSpace($TestFilter))
}

function Start-WindowsDeviceTestProcess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$AppPath,

        [Parameter(Mandatory = $true)]
        [string[]]$ArgumentList,

        [string]$IncludeClasses
    )

    $startInfo = [System.Diagnostics.ProcessStartInfo]::new()
    $startInfo.FileName = [System.IO.Path]::GetFullPath($AppPath)
    $startInfo.WorkingDirectory = [System.IO.Path]::GetDirectoryName($startInfo.FileName)
    $startInfo.UseShellExecute = $false

    foreach ($argument in $ArgumentList) {
        [void]$startInfo.ArgumentList.Add($argument)
    }

    if (-not [string]::IsNullOrWhiteSpace($IncludeClasses)) {
        # XHarness treats NUNIT_SKIPPED_CLASSES as an include list and disables
        # RunAllTestsByDefault when it contains at least one class.
        $startInfo.Environment["NUNIT_SKIPPED_CLASSES"] = $IncludeClasses
    } else {
        [void]$startInfo.Environment.Remove("NUNIT_SKIPPED_CLASSES")
    }

    $process = [System.Diagnostics.Process]::Start($startInfo)
    if (-not $process) {
        throw "Failed to start Windows device test app '$AppPath'."
    }

    return $process
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
    if ([int]::TryParse([string]$Value, [ref]$parsed)) { return [Math]::Max(0, $parsed) }
    return 0
}

function Get-DeviceTestResultSummary {
    param(
        [Parameter(Mandatory = $true)][string[]]$ResultFiles,

        # When set, the pass/fail tallies count ONLY tests whose fully-qualified name
        # belongs to one of these classes (comma/semicolon separated). Used to scope a
        # full-suite result file down to the class(es) under test — see the call site.
        [string]$IncludeClasses,

        # When set (in addition to -IncludeClasses), narrows the tally further to ONLY these
        # method names (comma/semicolon separated). This is the precise scope the gate wants:
        # a full-suite run of a no-discovery app contains every method of the target class,
        # but the PR only added/changed specific methods — counting the whole class lets an
        # unrelated pre-existing/flaky failure in a sibling method falsely redden the verdict.
        # Empty = fall back to whole-class scoping.
        [string]$IncludeMethods,

        # XHarness must execute only the requested classes. If its runtime include filter
        # was ignored and the result XML contains any other class, fail descriptively
        # instead of accepting a broad-suite result as Gate evidence.
        [switch]$RequireClassIsolation
    )

    $classList = @()
    if (-not [string]::IsNullOrWhiteSpace($IncludeClasses)) {
        $classList = @($IncludeClasses -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }

    $methodList = @()
    if (-not [string]::IsNullOrWhiteSpace($IncludeMethods)) {
        $methodList = @($IncludeMethods -split '[,;]' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
    }

    $summary = @{
        Total = 0
        Passed = 0
        Failed = 0
        Skipped = 0
        Errors = 0
        FailedTests = [System.Collections.Generic.List[string]]::new()
    }

    # Diagnostics for the class-filtered path: how many <test> nodes the file(s) held in
    # total (regardless of class) and a small sample of the DISTINCT classes they belong
    # to. When the class filter matches nothing, these disambiguate "the app produced no
    # results at all" from "the results are there but under classes we didn't expect" —
    # see the throw below. $diagClassMatchCount counts tests whose CLASS matched (before
    # any method narrowing) so we can further distinguish "class present but none of the
    # target methods ran" when method-scoping is active.
    $diagTotalTests = 0
    $diagClassMatchCount = 0
    $diagSampleClasses = [System.Collections.Generic.List[string]]::new()
    $matchedClassNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $matchedMethodNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    $unexpectedTestCount = 0
    $unexpectedClasses = [System.Collections.Generic.List[string]]::new()

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
            # Consumed by verify-tests-fail.ps1. Keep the marker stable: after three
            # baseline-only occurrences followed by a clean with-fix pass, the Gate can
            # safely treat the source-dependent app exit as the expected failing repro.
            throw "$WindowsDeviceNoResultsMarker Windows device test result file '$file' is empty or not valid XML (the device-test app likely crashed or exited before writing results)."
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
                $matchedClassName = $null
                foreach ($cls in $classList) {
                    # xUnit's `type` attribute is the exact fully-qualified declaring
                    # class used by XUnitFilter.CreateClassFilter. Only use name-prefix
                    # recovery when an older runner omitted `type`; prefix matching on
                    # `type` would incorrectly accept a different class such as Foo.Bar.
                    if ((-not [string]::IsNullOrWhiteSpace($testType) -and $testType -eq $cls) -or
                        ([string]::IsNullOrWhiteSpace($testType) -and
                            ($testName -eq $cls -or
                             (-not [string]::IsNullOrWhiteSpace($testName) -and $testName.StartsWith("$cls.", [System.StringComparison]::Ordinal))))) {
                        $isMatch = $true
                        $matchedClassName = $cls
                        break
                    }
                }
                if (-not $isMatch) {
                    if ($RequireClassIsolation) {
                        $unexpectedTestCount++
                        if ($diagLabel -and $unexpectedClasses.Count -lt 8 -and -not $unexpectedClasses.Contains($diagLabel)) {
                            $unexpectedClasses.Add($diagLabel)
                        }
                    }
                    continue
                }
                $null = $matchedClassNames.Add($matchedClassName)
                $diagClassMatchCount++

                # Optional method-level narrowing: when the gate knows the PR's specific
                # methods, count ONLY those (matched on the xUnit `method` attribute, which
                # is the real C# method name — for a [Theory] every data-case <test> row
                # shares the same `method`, so all cases of a target method are counted).
                # This keeps an unrelated pre-existing/flaky failure in a sibling method of
                # the same class from falsely reddening the A/B verdict.
                if ($methodList.Count -gt 0) {
                    $testMethod = $test.GetAttribute('method')
                    if ([string]::IsNullOrWhiteSpace($testMethod)) {
                        # Runner omitted `method` — recover it from the FQN tail of `type.method`
                        # or a "Class.Method" display name so method-scoping still works.
                        $probe = if (-not [string]::IsNullOrWhiteSpace($testName)) { $testName } else { $testType }
                        if ($probe -and $probe.Contains('.')) { $testMethod = $probe.Substring($probe.LastIndexOf('.') + 1) }
                    }
                    if ($methodList -notcontains $testMethod) { continue }
                    $null = $matchedMethodNames.Add($testMethod)
                }

                $summary.Total++
                switch ([string]$test.GetAttribute('result')) {
                    'Pass' { $summary.Passed++ }
                    'Fail' {
                        $summary.Failed++
                        # Capture the identity of each failing test so a FAILED verdict is
                        # auditable from the gate log (target-method failure vs unrelated).
                        $failId = if (-not [string]::IsNullOrWhiteSpace($testType)) {
                            $m = $test.GetAttribute('method')
                            if (-not [string]::IsNullOrWhiteSpace($m)) { "$testType.$m" } else { $testType }
                        } elseif (-not [string]::IsNullOrWhiteSpace($testName)) { $testName } else { '(unnamed)' }
                        $failId = ConvertTo-AzdoSafeConsole -Text $failId
                        if ($summary.FailedTests.Count -lt 20 -and -not $summary.FailedTests.Contains($failId)) {
                            $summary.FailedTests.Add($failId)
                        }
                    }
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

    if ($classList.Count -gt 0 -and $RequireClassIsolation -and $unexpectedTestCount -gt 0) {
        $sample = if ($unexpectedClasses.Count -gt 0) { " Unexpected classes: " + ($unexpectedClasses -join '; ') + '.' } else { '' }
        throw "XHarness class filter was not enforced: result file(s) contained $unexpectedTestCount test(s) outside requested class(es) '$IncludeClasses'.$sample"
    }

    if ($classList.Count -gt 0) {
        $missingClasses = @($classList | Where-Object { -not $matchedClassNames.Contains($_) })
        if ($missingClasses.Count -gt 0 -and $diagClassMatchCount -gt 0) {
            throw "Device test result file(s) contained no tests for requested class(es): $($missingClasses -join ', ') (the target tests did not run)."
        }
    }

    if ($methodList.Count -gt 0 -and $diagClassMatchCount -gt 0) {
        $missingMethods = @($methodList | Where-Object { -not $matchedMethodNames.Contains($_) })
        if ($missingMethods.Count -eq $methodList.Count) {
            throw "Device test result file(s) contained the class(es) '$IncludeClasses' ($diagClassMatchCount test(s)) but none of the target method(s) '$IncludeMethods' ran (the target tests did not run)."
        }
        if ($missingMethods.Count -gt 0) {
            throw "Device test result file(s) did not contain every requested method. Missing: $($missingMethods -join ', '); matched: $($matchedMethodNames -join ', ') (the target tests did not all run)."
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
        throw "Device test result file(s) contained no tests for class(es) '$IncludeClasses' (the target tests did not run). Total tests found in result file(s): $diagTotalTests.$sample"
    }

    if ($classList.Count -gt 0 -and ($summary.Passed + $summary.Failed) -eq 0) {
        throw "Device test result file(s) contained only skipped tests for class(es) '$IncludeClasses' (the target tests did not execute)."
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

        [string]$IncludeMethods,

        [string]$Timeout = "01:00:00"
    )

    $timeoutSeconds = [int][TimeSpan]::Parse($Timeout).TotalSeconds
    if ($timeoutSeconds -le 0) {
        $timeoutSeconds = 3600
    }
    $classRunTimeoutSeconds = [Math]::Min($timeoutSeconds, 600)

    # The app must run from its executable directory, but OutputDirectory is commonly
    # supplied as a repo-relative path. Canonicalize it before passing result paths to
    # the child so the app and this process always observe the same files.
    $OutputDirectory = $ExecutionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($OutputDirectory)
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
    #   - Core/Essentials/Graphics/BlazorWebView: use their normal runner with the exact
    #     XHarness class include whenever the Gate supplied one. Their discovery/index
    #     path can stall before producing devicetestcategories.txt; falling back from
    #     that stall to an unfiltered full suite consumed an hour on PR #36884.
    #   - A standalone filtered run without class metadata still attempts discovery.
    $requireDiscovery = ($Project -eq "Controls")
    $attemptDiscovery = Test-WindowsDeviceTestCategoryDiscovery `
        -Project $Project `
        -TestFilter $TestFilter `
        -IncludeClasses $IncludeClasses
    $useCategoryFiltering = $false
    if ($attemptDiscovery) {
        Write-Host "Discovering Windows device test categories..." -ForegroundColor Gray
        $discoveryProcess = Start-WindowsDeviceTestProcess `
            -AppPath $AppPath `
            -ArgumentList @($resultFile, "-1") `
            -IncludeClasses $IncludeClasses
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
            $categoryRunTimeoutSeconds = if ($IncludeClasses) { $classRunTimeoutSeconds } else { $timeoutSeconds }
            Remove-Item -LiteralPath $categoryResultFile -Force -ErrorAction SilentlyContinue
            Write-Host "Running Windows device test category '$category' (index $categoryIndex)..." -ForegroundColor Gray
            $process = Start-WindowsDeviceTestProcess `
                -AppPath $AppPath `
                -ArgumentList @($resultFile, [string]$categoryIndex) `
                -IncludeClasses $IncludeClasses
            $categoryStopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            if (-not (Wait-ForPath -Path $categoryResultFile -TimeoutSeconds $categoryRunTimeoutSeconds -Process $process)) {
                if ($process -and $process.HasExited) {
                    throw "$WindowsDeviceNoResultsMarker Windows device test category '$category' exited without creating $categoryResultFile."
                }
                if ($process) {
                    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                }
                if ($IncludeClasses) {
                    $methodScope = if ($IncludeMethods) { " and method(s) '$IncludeMethods'" } else { "" }
                    throw "$WindowsDeviceTargetTimeoutMarker Windows device test category '$category' did not create $categoryResultFile within ${categoryRunTimeoutSeconds}s while running requested class(es) '$IncludeClasses'$methodScope."
                }
                throw "Windows device test category '$category' did not create $categoryResultFile within ${categoryRunTimeoutSeconds}s."
            }

            # The Windows runner creates the category result file before the test run has
            # finished writing it. Do not parse on file appearance: wait for the app process
            # to exit within the original category budget so the XML writer is complete.
            $remainingMilliseconds = [Math]::Max(
                1,
                [int](($categoryRunTimeoutSeconds - $categoryStopwatch.Elapsed.TotalSeconds) * 1000))
            $exitedInTime = $process.HasExited -or $process.WaitForExit($remainingMilliseconds)
            if (-not $exitedInTime -and $process.HasExited) {
                $exitedInTime = $true
            }
            if (-not $exitedInTime) {
                if (-not $process.HasExited) {
                    Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
                }
                if ($IncludeClasses) {
                    if (Test-Path -LiteralPath $categoryResultFile) {
                        try {
                            $completedSummary = Get-DeviceTestResultSummary -ResultFiles @($categoryResultFile)
                            if ($completedSummary.Total -le 0) {
                                throw "The result file contained no tests."
                            }
                            $resultFiles += $categoryResultFile
                            Write-Warning "Windows device test category '$category' exceeded ${categoryRunTimeoutSeconds}s after writing complete results; using the completed file and validating the requested target after all categories."
                            continue
                        } catch {
                            $resultEvidenceError = $_.Exception.Message
                            Write-Warning "Timed-out Windows category process did not leave complete results: $resultEvidenceError"
                        }
                    }
                    $methodScope = if ($IncludeMethods) { " and method(s) '$IncludeMethods'" } else { "" }
                    throw "$WindowsDeviceTargetTimeoutMarker Windows device test category '$category' did not exit within ${categoryRunTimeoutSeconds}s while running requested class(es) '$IncludeClasses'$methodScope."
                }
                throw "Windows device test category '$category' did not exit within ${categoryRunTimeoutSeconds}s."
            }

            $resultFiles += $categoryResultFile
        }
    } else {
        # Normal runner: this is a true full suite only when IncludeClasses is empty.
        # Otherwise Start-WindowsDeviceTestProcess passes XHarness's exact class include,
        # so the app executes only the requested class without category discovery.
        Remove-Item -LiteralPath $resultFile -Force -ErrorAction SilentlyContinue

        if ($IncludeClasses) {
            Write-Host "Running Windows device test app with class isolation: $IncludeClasses" -ForegroundColor Gray
        } else {
            Write-Host "Running Windows device test app directly..." -ForegroundColor Gray
        }
        $process = Start-WindowsDeviceTestProcess `
            -AppPath $AppPath `
            -ArgumentList @($resultFile) `
            -IncludeClasses $IncludeClasses

        # A full-suite app creates its single results file and finalizes it only when the
        # whole run completes, so waiting for the file to merely APPEAR races the writer
        # and reads an empty/partial XML — surfacing as a false
        # "empty or not valid XML" ENV ERROR even though the run is healthy (PR #36577: the
        # Core Windows full run was read at 247s while it was still executing). Wait for the
        # process to EXIT instead, mirroring how eng/devices/windows.cake launches the
        # unpackaged app with a blocking StartProcess and only then checks the result file.
        $processTimeoutSeconds = if ($IncludeClasses) { $classRunTimeoutSeconds } else { $timeoutSeconds }
        $exitedInTime = $process.WaitForExit($processTimeoutSeconds * 1000)
        if (-not $exitedInTime -and $process.HasExited) {
            $exitedInTime = $true
        }
        if (-not $exitedInTime) {
            if (-not $process.HasExited) {
                Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
            }
            if ($IncludeClasses) {
                if (Test-Path -LiteralPath $resultFile) {
                    try {
                        $completedSummary = Get-DeviceTestResultSummary `
                            -ResultFiles @($resultFile) `
                            -IncludeClasses $IncludeClasses `
                            -IncludeMethods $IncludeMethods `
                            -RequireClassIsolation
                        $script:WindowsDeviceTestSummary = $completedSummary
                        $script:WindowsDeviceTestResultFiles = @($resultFile)
                        Write-Warning "Windows device test process exceeded ${processTimeoutSeconds}s after writing complete scoped results; using the verified target-test result."
                        return $(if (($completedSummary.Failed + $completedSummary.Errors) -eq 0) { 0 } else { 1 })
                    } catch {
                        $resultEvidenceError = $_.Exception.Message
                        Write-Warning "Timed-out Windows target process did not leave complete scoped results: $resultEvidenceError"
                    }
                }
                $methodScope = if ($IncludeMethods) { " and method(s) '$IncludeMethods'" } else { "" }
                throw "$WindowsDeviceTargetTimeoutMarker Windows device test app did not exit within ${processTimeoutSeconds}s while running requested class(es) '$IncludeClasses'$methodScope."
            }
            throw "Windows device test app did not exit within ${processTimeoutSeconds}s while running the full suite."
        }
        if (-not (Test-Path $resultFile)) {
            throw "$WindowsDeviceNoResultsMarker Windows device test app exited without creating $resultFile."
        }

        $resultFiles += $resultFile
    }

    # Always narrow the pass/fail summary to the requested class/method when supplied.
    # Category isolation limits the run to (for example) Map, but that category can still
    # contain sibling classes/methods. Accepting its whole-file aggregate would let an
    # unrelated passing sibling hide that the target method never ran.
    $summaryClassFilter = $IncludeClasses
    $summaryMethodFilter = $IncludeMethods
    $summary = Get-DeviceTestResultSummary `
        -ResultFiles $resultFiles `
        -IncludeClasses $summaryClassFilter `
        -IncludeMethods $summaryMethodFilter `
        -RequireClassIsolation:(-not [string]::IsNullOrWhiteSpace($IncludeClasses))
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

# Find repository root. Trusted pipeline copies can live outside the checkout,
# so replication passes the already resolved source root explicitly.
if (-not [string]::IsNullOrWhiteSpace($RepositoryRoot)) {
    $RepoRoot = (Resolve-Path -LiteralPath $RepositoryRoot).Path
} else {
    $RepoRoot = $PSScriptRoot
    while ($RepoRoot -and -not (Test-Path (Join-Path $RepoRoot ".git"))) {
        $RepoRoot = Split-Path $RepoRoot -Parent
    }
}

if (-not $RepoRoot -or -not (Test-Path -LiteralPath (Join-Path $RepoRoot '.git'))) {
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
$classFilterInjection = $null

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
    $IncludeClasses = ConvertTo-DeviceTestClassFilterValue -Value $IncludeClasses
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

    if ($Platform -eq "android" -and $IncludeClasses) {
        $filterTempRoot = if (-not [string]::IsNullOrWhiteSpace($env:AGENT_TEMPDIRECTORY)) {
            $env:AGENT_TEMPDIRECTORY
        } else {
            [System.IO.Path]::GetTempPath()
        }
        $classFilterInjection = New-AndroidDeviceTestClassFilterInjection `
            -IncludeClasses $IncludeClasses `
            -TempRoot $filterTempRoot
        Write-Host "✓ Prepared trusted Android XHarness class-filter injection" -ForegroundColor Green
    }

    $buildArgs = @(
        "build"
        $projectPath
        "-c", $Configuration
        "-f", $platformConfig.Tfm
        "/p:TreatWarningsAsErrors=false"
    )

    if ($Rebuild) {
        $buildArgs += "-t:Rebuild"
    }

    if ($classFilterInjection) {
        $buildArgs += "/p:CustomAfterMicrosoftCSharpTargets=$($classFilterInjection.TargetsPath)"
        $buildArgs += "/p:MauiCopilotClassFilterSourcePath=$($classFilterInjection.SourcePath)"
        $buildArgs += "/p:MauiCopilotClassFilterTargetProject=$($classFilterInjection.TargetProject)"
    }

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
    $script:XHarnessDeviceTestSummary = $null
    $script:XHarnessDeviceTestResultFiles = @()
    $testOutputDirectory = $OutputDirectory
    $xharnessResultFileName = if ($Platform -in @("ios", "maccatalyst")) {
        "xunit-test-*.xml"
    } else {
        "testResults.xml"
    }

    if ($platformConfig.UsesXHarness) {
        # ═══════════════════════════════════════════════════════════
        # XHARNESS TEST EXECUTION (iOS, MacCatalyst, Android)
        # ═══════════════════════════════════════════════════════════

        if ($IncludeClasses) {
            $testOutputDirectory = New-XHarnessRunOutputDirectory -OutputDirectory $OutputDirectory
            Write-Host "XHarness run output: $testOutputDirectory" -ForegroundColor Gray

            if ($Platform -eq "android") {
                # The Gate can retry into the same emulator after a timed-out launch.
                # Give the Android runner a trusted per-invocation filename so a stale
                # device-side result from an earlier run cannot satisfy this run.
                $xharnessResultFileName = "testResults-$([guid]::NewGuid().ToString('N')).xml"
            }
        }
        
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
                    "-o", $testOutputDirectory
                    "--timeout", $Timeout
                    "-v"
                )
            }
            "maccatalyst" {
                $xharnessArgs = @(
                    "apple", "test"
                    "--app", $appPath
                    "--target", "maccatalyst"
                    "-o", $testOutputDirectory
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
                    "-o", $testOutputDirectory
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

        if ($IncludeClasses -and $Platform -eq "android") {
            $xharnessArgs += "--arg", "results-file-name=$xharnessResultFileName"
        }

        if ($IncludeClasses -and $Platform -ne "android") {
            $xharnessArgs += "--set-env=NUNIT_SKIPPED_CLASSES=$IncludeClasses"
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

        $xharnessResultSnapshot = if ($IncludeClasses) {
            Get-XHarnessTestResultSnapshot `
                -OutputDirectory $testOutputDirectory `
                -ResultFileName $xharnessResultFileName
        } else {
            $null
        }

        if ($useLocalXharness) {
            & dotnet xharness @xharnessArgs
        } else {
            & xharness @xharnessArgs
        }

        $rawXHarnessExitCode = $LASTEXITCODE
        $testExitCode = $rawXHarnessExitCode

        if ($IncludeClasses) {
            $xharnessResultFiles = @(Get-FreshXHarnessTestResultFiles `
                -OutputDirectory $testOutputDirectory `
                -BeforeSnapshot $xharnessResultSnapshot `
                -ResultFileName $xharnessResultFileName)
            if ($xharnessResultFiles.Count -eq 0) {
                throw "XHarness did not produce the expected fresh result '$xharnessResultFileName' for requested class(es) '$IncludeClasses' (the target tests did not run)."
            }

            $script:XHarnessDeviceTestSummary = Get-DeviceTestResultSummary `
                -ResultFiles $xharnessResultFiles `
                -IncludeClasses $IncludeClasses `
                -IncludeMethods $IncludeMethods `
                -RequireClassIsolation
            $script:XHarnessDeviceTestResultFiles = $xharnessResultFiles
            $testExitCode = if (($script:XHarnessDeviceTestSummary.Failed + $script:XHarnessDeviceTestSummary.Errors) -eq 0) { 0 } else { 1 }

            if ($rawXHarnessExitCode -ne 0 -and $testExitCode -eq 0) {
                # The MAUI runner writes/copies testResults.xml only after runner.Run()
                # completes. A fresh, isolated XML file therefore provides stronger
                # target-test evidence than XHarness cleanup/teardown exit codes.
                Write-Warning "XHarness exited with code $rawXHarnessExitCode after the scoped target tests completed successfully; using the verified target-test result."
            }
        }
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
            -IncludeMethods $IncludeMethods `
            -Timeout $Timeout

        if ($script:WindowsDeviceTestSummary) {
            Write-Host ""
            Write-Output "  Passed: $($script:WindowsDeviceTestSummary.Passed)"
            Write-Output "  Failed: $($script:WindowsDeviceTestSummary.Failed + $script:WindowsDeviceTestSummary.Errors)"
            Write-Output "  Skipped: $($script:WindowsDeviceTestSummary.Skipped)"
            Write-Output "  Total: $($script:WindowsDeviceTestSummary.Total)"
            if ($IncludeMethods) {
                Write-Host "  Scoped to method(s): $IncludeMethods" -ForegroundColor Gray
            }
            # Naming the failing tests makes a FAILED verdict auditable from the gate log
            # (distinguishes a genuine target-method failure from an unrelated one).
            if ($script:WindowsDeviceTestSummary.FailedTests -and $script:WindowsDeviceTestSummary.FailedTests.Count -gt 0) {
                Write-Host "  Failed test(s): $($script:WindowsDeviceTestSummary.FailedTests -join '; ')" -ForegroundColor Gray
            }
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
    $logFile = Get-ChildItem -Path $testOutputDirectory -Filter "$appName.log" -ErrorAction SilentlyContinue | Select-Object -First 1
    
    if ($script:XHarnessDeviceTestSummary) {
        Write-Host ""
        Write-Output "  Passed: $($script:XHarnessDeviceTestSummary.Passed)"
        Write-Output "  Failed: $($script:XHarnessDeviceTestSummary.Failed + $script:XHarnessDeviceTestSummary.Errors)"
        Write-Output "  Skipped: $($script:XHarnessDeviceTestSummary.Skipped)"
        Write-Output "  Total: $($script:XHarnessDeviceTestSummary.Total)"
        Write-Host "  Class isolation verified: $IncludeClasses" -ForegroundColor Green
        if ($IncludeMethods) {
            Write-Host "  Scoped to method(s): $IncludeMethods" -ForegroundColor Gray
        }
        if ($script:XHarnessDeviceTestSummary.FailedTests -and $script:XHarnessDeviceTestSummary.FailedTests.Count -gt 0) {
            Write-Host "  Failed test(s): $($script:XHarnessDeviceTestSummary.FailedTests -join '; ')" -ForegroundColor Gray
        }
        Write-Host "  Result file(s): $($script:XHarnessDeviceTestResultFiles -join ', ')" -ForegroundColor Gray
    } elseif ($logFile) {
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
    if ($classFilterInjection -and $classFilterInjection.Directory -and (Test-Path $classFilterInjection.Directory)) {
        Remove-Item -LiteralPath $classFilterInjection.Directory -Recurse -Force -ErrorAction SilentlyContinue
    }
    Pop-Location
}
