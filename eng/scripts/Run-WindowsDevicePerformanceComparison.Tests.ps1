#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"
$script = Join-Path $PSScriptRoot "Run-WindowsDevicePerformanceComparison.ps1"
$repositoryRoot = [IO.Path]::GetFullPath([IO.Path]::Combine($PSScriptRoot, "..", ".."))
$testRoot = Join-Path ([IO.Path]::GetTempPath()) ("maui-windows-device-perf-" + [Guid]::NewGuid().ToString("N"))
$isWindowsHost = [Environment]::OSVersion.Platform -eq [PlatformID]::Win32NT
$assertionCount = 0

function Assert-Equal($expected, $actual, [string]$message) {
    $script:assertionCount++
    if ($expected -ne $actual) {
        throw "$message. Expected '$expected', actual '$actual'."
    }
}

function Assert-Rejected([hashtable]$arguments, [string]$pattern, [string]$message) {
    $failure = $null
    try {
        & $script @arguments 6>$null | Out-Null
        if ($LASTEXITCODE -ne 0) {
            $failure = "exit code $LASTEXITCODE"
        }
    }
    catch {
        $failure = $_.Exception.Message
    }
    Assert-Equal $true ($null -ne $failure -and $failure -like $pattern) "$message (failure: $failure)"
}

$savedEnvironment = @{}
foreach ($name in @(
    "MAUI_INCLUDE_PERFORMANCE_TESTS", "MAUI_PERF_RUN_ID", "MAUI_PERF_RESULT_FILE", "MAUI_PERF_VARIANT",
    "MAUI_PERF_FIXTURE_MODE", "MAUI_PERF_FIXTURE_INVOCATIONS"
)) {
    $savedEnvironment[$name] = [Environment]::GetEnvironmentVariable($name)
}

New-Item -ItemType Directory -Force -Path $testRoot | Out-Null
try {
    $baseApp = Join-Path $testRoot "base app\WindowsDevicePerfFixture.exe"
    $headApp = Join-Path $testRoot "head app\WindowsDevicePerfFixture.exe"
    New-Item -ItemType Directory -Force -Path (Split-Path -Parent $baseApp), (Split-Path -Parent $headApp) | Out-Null
    New-Item -ItemType File -Force -Path $baseApp, $headApp | Out-Null
    $runArguments = @{
        BaseApp = $baseApp
        HeadApp = $headApp
        BaseCommitSha = "abc123"
        HeadCommitSha = "def456"
        ExpectedScenario = "carouselview-wheel-snap-windows"
        Repository = "dotnet/maui"
        PullRequestNumber = 42
        PullRequestAuthor = "perf-author"
        HarnessSha = "harness123"
        BaseRuntimeVariant = "coreclr"
        HeadRuntimeVariant = "coreclr"
        BaseSdkVersion = "10.0.100"
        HeadSdkVersion = "10.0.100"
        OutputDirectory = Join-Path $testRoot "output"
        DiscoveryTimeoutSeconds = 5
        RunTimeoutSeconds = 5
        DryRun = $true
    }

    & $script @runArguments 6>$null | Out-Null
    Assert-Equal 0 $LASTEXITCODE "Windows driver dry-run"
    $plan = Get-Content (Join-Path $runArguments.OutputDirectory "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal 4 @($plan).Count "Windows ABBA run count"
    Assert-Equal "base,head,head,base" ($plan.variant -join ",") "Windows ABBA order"
    Assert-Equal "PerformanceCarouselViewWheelSnap" $plan[0].category "Windows category"
    $driverSource = Get-Content $script -Raw
    Assert-Equal $true $driverSource.Contains('-PullRequestAuthor $PullRequestAuthor') "Windows driver forwards author to the shared renderer"

    $handlerArguments = $runArguments.Clone()
    $handlerArguments.ExpectedScenario = "handler-property-update-batch"
    $handlerArguments.OutputDirectory = Join-Path $testRoot "handler-output"
    & $script @handlerArguments 6>$null | Out-Null
    $handlerPlan = Get-Content (Join-Path $handlerArguments.OutputDirectory "run-plan.json") -Raw | ConvertFrom-Json
    Assert-Equal "PerformanceHandlerPropertyUpdate" $handlerPlan[0].category "Windows handler family category"

    foreach ($prNumber in @(0, -1, [int]::MinValue)) {
        $invalidArguments = $runArguments.Clone()
        $invalidArguments.PullRequestNumber = $prNumber
        $invalidArguments.BaseApp = Join-Path $testRoot "app-must-not-be-inspected.exe"
        $invalidArguments.OutputDirectory = Join-Path $testRoot "invalid-pr-$prNumber"
        $invalidArguments.DryRun = $false
        Assert-Rejected $invalidArguments "*PullRequestNumber*" "PR $prNumber must fail parameter binding before app discovery"
        Assert-Equal $false (Test-Path $invalidArguments.OutputDirectory) "Invalid PR must not create output"
    }

    if ($isWindowsHost) {
        $fixtureSource = Join-Path $testRoot "WindowsDevicePerfFixture.cs"
        @'
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Web.Script.Serialization;

class WindowsDevicePerfFixture
{
    static string Env(string name) { return Environment.GetEnvironmentVariable(name); }

    static int Main(string[] args)
    {
        bool discovery = args[1] == "-1";
        string phase = discovery ? "discovery" : "run";
        string runId = Env("MAUI_PERF_RUN_ID");
        Guid parsedRunId;
        if (!Path.IsPathRooted(args[0]) ||
            !String.Equals(Environment.CurrentDirectory, AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase) ||
            Env("MAUI_INCLUDE_PERFORMANCE_TESTS") != "1" || !Guid.TryParseExact(runId, "N", out parsedRunId))
            return 98;

        File.AppendAllText(Env("MAUI_PERF_FIXTURE_INVOCATIONS"), String.Join("|", new[] {
            phase, Environment.CurrentDirectory, args[0], discovery ? "" : Env("MAUI_PERF_RESULT_FILE"),
            runId, Process.GetCurrentProcess().Id.ToString(), args[1]
        }) + Environment.NewLine);
        string mode = Env("MAUI_PERF_FIXTURE_MODE") ?? "success";
        mode = mode.StartsWith(phase + "-", StringComparison.Ordinal) ? mode.Substring(phase.Length + 1) : "success";
        string categoryFile = Path.Combine(Path.GetDirectoryName(args[0]), "devicetestcategories.txt");
        if (discovery)
        {
            if (mode != "missing-output")
                File.WriteAllLines(categoryFile, mode == "empty-output" ? new string[0] :
                    mode == "missing-category" ? new[] { "Button" } :
                    new[] { "Button", "PerformanceCarouselViewWheelSnap", "PerformanceHandlerPropertyUpdate" });
        }
        else
        {
            if (!Path.IsPathRooted(Env("MAUI_PERF_RESULT_FILE")))
                return 99;
            string category = File.ReadAllLines(categoryFile)[Int32.Parse(args[1])];
            string xmlPath = Path.Combine(Path.GetDirectoryName(args[0]), Path.GetFileNameWithoutExtension(args[0]) + "_" + category + ".xml");
            if (mode != "missing-xml")
                File.WriteAllText(xmlPath, mode == "partial-xml" ? "<assemblies>" :
                    mode == "empty-xml" ? "" : "<assemblies><assembly total='1' passed='1' failed='0' skipped='0'/></assemblies>");
            string json = new JavaScriptSerializer().Serialize(new {
                schemaVersion = 3,
                repository = Env("MAUI_PERF_REPOSITORY"),
                pullRequestNumber = Int32.Parse(Env("MAUI_PERF_PR_NUMBER")),
                scenario = category == "PerformanceCarouselViewWheelSnap" ? "carouselview-wheel-snap-windows" : "handler-property-update-batch",
                platform = "windows",
                variant = Env("MAUI_PERF_VARIANT"),
                commitSha = Env("MAUI_PERF_COMMIT_SHA"),
                harnessSha = Env("MAUI_PERF_HARNESS_SHA"),
                runOrdinal = Int32.Parse(Env("MAUI_PERF_RUN_ORDINAL")),
                expectedVariantRuns = Int32.Parse(Env("MAUI_PERF_EXPECTED_VARIANT_RUNS")),
                environment = new { executionKind = "desktop", deviceModel = "fixture", osVersion = "Windows",
                    runtimeFramework = ".NET 10", processArchitecture = "X64",
                    runtimeVariant = Env("MAUI_PERF_RUNTIME_VARIANT"), sdkVersion = Env("MAUI_PERF_SDK_VERSION") },
                correctness = new { passed = mode != "incorrect-result", accessibilityStatus = "not-assessed" },
                timestampUtc = DateTimeOffset.UtcNow.ToString("O"),
                warmupCount = 2,
                measurementsMilliseconds = new[] { 100, 101, 102 },
                statistics = new { median = 101 },
                counters = new { maximumCenterError = 0, positionsOutsideTolerance = 0, positionMismatchCount = 0,
                    completedUpdateBatches = 5, nativeValueMismatchCount = 0 }
            });
            Console.WriteLine("MAUI_PERF_RESULT:" + json);
            if (mode != "missing-output")
                File.WriteAllText(Env("MAUI_PERF_RESULT_FILE"), mode == "empty-output" ? "" :
                    "MAUI_PERF_RESULT:" + (mode == "partial-output" ? json.Substring(0, json.Length - 1) : json));
        }
        if (mode != "missing-completion" && mode != "failed-tests")
            File.WriteAllText(args[0] + ".completed", mode == "stale-completion" ? new string('0', 32) :
                mode == "partial-completion" ? runId.Substring(0, 16) : runId);
        if (mode == "timeout")
            Thread.Sleep(30000);
        if (mode == "killed")
            Process.GetCurrentProcess().Kill();
        if (mode == "bootstrap")
            return -1073741189;
        return mode == "nonzero" ? 7 : mode == "failed-tests" ? 1 : 0;
    }
}
'@ | Set-Content -LiteralPath $fixtureSource -Encoding UTF8
        $compiler = Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"
        $compilerOutput = & $compiler /nologo /target:exe /reference:System.Web.Extensions.dll "/out:$baseApp" $fixtureSource 2>&1
        Assert-Equal 0 $LASTEXITCODE "Compile the fake Windows app: $compilerOutput"
        Copy-Item -LiteralPath $baseApp -Destination $headApp -Force

        $invocations = Join-Path $testRoot "invocations.txt"
        $env:MAUI_PERF_FIXTURE_INVOCATIONS = $invocations
        $env:MAUI_PERF_FIXTURE_MODE = "success"
        $env:MAUI_INCLUDE_PERFORMANCE_TESTS = "caller-include"
        $env:MAUI_PERF_RUN_ID = "caller-run-id"
        $env:MAUI_PERF_RESULT_FILE = "caller-result-file"
        $env:MAUI_PERF_VARIANT = "caller-variant"
        $liveArguments = $runArguments.Clone()
        $liveArguments.DryRun = $false

        $callerDirectory = Join-Path $testRoot "caller location.xml"
        New-Item -ItemType Directory -Path $callerDirectory | Out-Null
        $savedProcessDirectory = [Environment]::CurrentDirectory
        Push-Location $callerDirectory
        try {
            [Environment]::CurrentDirectory = Split-Path -Parent $baseApp
            $relativeArguments = $liveArguments.Clone()
            $relativeArguments.BaseApp = "..\base app\WindowsDevicePerfFixture.exe"
            $relativeArguments.HeadApp = "..\head app\WindowsDevicePerfFixture.exe"
            $relativeArguments.OutputDirectory = ".\relative results"
            & $script @relativeArguments 6>$null | Out-Null
            Assert-Equal 0 $LASTEXITCODE "A complete ABBA comparison succeeds with different caller, process, and app working directories"
        }
        finally {
            [Environment]::CurrentDirectory = $savedProcessDirectory
            Pop-Location
        }
        $relativeOutput = Join-Path $callerDirectory "relative results"
        $results = Get-Content (Join-Path $relativeOutput "results.json") -Raw | ConvertFrom-Json
        Assert-Equal 4 @($results).Count "All four fresh result logs are parsed"
        Assert-Equal "base,base,head,head" (($results.variant | Sort-Object) -join ",") "Both variants are measured"
        Assert-Equal 0 @($results | Where-Object { $_.warmupCount -ne 2 }).Count "Every fake record includes integer warmupCount"
        $calls = @(Get-Content $invocations | ForEach-Object { ,($_ -split '\|') })
        Assert-Equal 6 $calls.Count "Two discoveries and four measured processes"
        Assert-Equal "discovery,discovery,run,run,run,run" (($calls | ForEach-Object { $_[0] }) -join ",") "Discovery precedes ABBA execution"
        Assert-Equal 6 @($calls | ForEach-Object { $_[4] } | Sort-Object -Unique).Count "Each child receives a distinct completion token"
        foreach ($call in $calls) {
            Assert-Equal $true $call[2].StartsWith($relativeOutput, [StringComparison]::OrdinalIgnoreCase) "Absolute XML output belongs to the caller"
            if ($call[0] -eq "run") {
                Assert-Equal $true $call[3].StartsWith($relativeOutput, [StringComparison]::OrdinalIgnoreCase) "Absolute measurement output belongs to the caller"
                Assert-Equal "1" $call[6] "The discovered category index is used"
            }
        }
        $successfulRecord = Get-Content (Join-Path $relativeOutput "base-run1\maui-perf-result.log") -Raw

        $handlerArguments.DryRun = $false
        $unrelatedDirectory = Join-Path $handlerArguments.OutputDirectory "previous-run"
        New-Item -ItemType Directory -Force -Path $unrelatedDirectory | Out-Null
        "MAUI_PERF_RESULT:{stale,invalid}" | Set-Content (Join-Path $unrelatedDirectory "old.log")
        & $script @handlerArguments 6>$null | Out-Null
        Assert-Equal 0 $LASTEXITCODE "Handler comparison reads only the current four authoritative result files"
        $summary = Get-Content (Join-Path $handlerArguments.OutputDirectory "comparison-summary.json") -Raw | ConvertFrom-Json
        Assert-Equal $true $summary.provenanceValidated "The successful run reaches the shared comparator"

        foreach ($prNumber in @(0, -42)) {
            $callCount = @(Get-Content $invocations).Count
            $invalidArguments = $liveArguments.Clone()
            $invalidArguments.PullRequestNumber = $prNumber
            $invalidArguments.OutputDirectory = Join-Path $testRoot "live-invalid-pr-$prNumber"
            Assert-Rejected $invalidArguments "*PullRequestNumber*" "Invalid PR must not start the fake app"
            Assert-Equal $callCount @(Get-Content $invocations).Count "Invalid PR must not launch discovery"
            Assert-Equal $false (Test-Path $invalidArguments.OutputDirectory) "Invalid PR must not create run artifacts"
        }

        $failureCases = @(
            @{ Mode = "discovery-nonzero"; Error = "*exited with code 7*" },
            @{ Mode = "discovery-bootstrap"; Error = "*Windows App SDK bootstrap failed*" },
            @{ Mode = "discovery-killed"; Error = "*exited with code*" },
            @{ Mode = "discovery-timeout"; Error = "*timed out after 1 seconds*" },
            @{ Mode = "discovery-missing-completion"; Error = "*did not report successful completion*" },
            @{ Mode = "discovery-stale-completion"; Error = "*did not report successful completion*" },
            @{ Mode = "discovery-partial-completion"; Error = "*did not report successful completion*" },
            @{ Mode = "discovery-missing-output"; Error = "*category discovery failed*" },
            @{ Mode = "discovery-empty-output"; Error = "*was not discovered*" },
            @{ Mode = "discovery-missing-category"; Error = "*was not discovered*" },
            @{ Mode = "run-nonzero"; Error = "*exited with code 7*" },
            @{ Mode = "run-bootstrap"; Error = "*Windows App SDK bootstrap failed*" },
            @{ Mode = "run-killed"; Error = "*exited with code*" },
            @{ Mode = "run-timeout"; Error = "*timed out after 1 seconds*" },
            @{ Mode = "run-failed-tests"; Error = "*exited with code 1*" },
            @{ Mode = "run-missing-completion"; Error = "*did not report successful completion*" },
            @{ Mode = "run-stale-completion"; Error = "*did not report successful completion*" },
            @{ Mode = "run-partial-completion"; Error = "*did not report successful completion*" },
            @{ Mode = "run-missing-output"; Error = "*result was not created or is empty*" },
            @{ Mode = "run-empty-output"; Error = "*result was not created or is empty*" },
            @{ Mode = "run-partial-output"; Error = "*Invalid performance result JSON*" },
            @{ Mode = "run-missing-xml"; Error = "*test results were not created*" },
            @{ Mode = "run-partial-xml"; Error = "*Invalid Windows test results*" },
            @{ Mode = "run-empty-xml"; Error = "*Invalid Windows test results*" }
        )
        foreach ($case in $failureCases) {
            $env:MAUI_PERF_FIXTURE_MODE = $case.Mode
            $arguments = $liveArguments.Clone()
            $arguments.OutputDirectory = Join-Path $testRoot $case.Mode
            if ($case.Mode.EndsWith("-timeout")) {
                $arguments.DiscoveryTimeoutSeconds = 1
                $arguments.RunTimeoutSeconds = 1
            }
            $staleDiscovery = Join-Path $arguments.OutputDirectory "base-discovery"
            $staleRun = Join-Path $arguments.OutputDirectory "base-run1"
            New-Item -ItemType Directory -Force -Path $staleDiscovery, $staleRun | Out-Null
            "PerformanceCarouselViewWheelSnap" | Set-Content (Join-Path $staleDiscovery "devicetestcategories.txt")
            foreach ($directory in @($staleDiscovery, $staleRun)) {
                "old-invocation" | Set-Content (Join-Path $directory "TestResults.xml.completed")
                "<assemblies/>" | Set-Content (Join-Path $directory "TestResults.xml")
            }
            "<assemblies/>" | Set-Content (Join-Path $staleRun "TestResults_PerformanceCarouselViewWheelSnap.xml")
            $successfulRecord | Set-Content (Join-Path $staleRun "maui-perf-result.log")
            Assert-Rejected $arguments $case.Error "$($case.Mode) must not be rescued by stale files or valid console output"
            Assert-Equal "caller-include" $env:MAUI_INCLUDE_PERFORMANCE_TESTS "Restore include opt-in after $($case.Mode)"
            Assert-Equal "caller-run-id" $env:MAUI_PERF_RUN_ID "Restore completion token after $($case.Mode)"
            Assert-Equal "caller-result-file" $env:MAUI_PERF_RESULT_FILE "Restore result path after $($case.Mode)"
            Assert-Equal "caller-variant" $env:MAUI_PERF_VARIANT "Restore variant after $($case.Mode)"
            if ($case.Mode.EndsWith("-timeout")) {
                $lastCall = (Get-Content $invocations | Select-Object -Last 1) -split '\|'
                Assert-Equal $null (Get-Process -Id ([int]$lastCall[5]) -ErrorAction SilentlyContinue) "The timed-out child must be stopped"
            }
        }

        $env:MAUI_PERF_FIXTURE_MODE = "run-incorrect-result"
        $incorrectArguments = $liveArguments.Clone()
        $incorrectArguments.OutputDirectory = Join-Path $testRoot "incorrect-result"
        & $script @incorrectArguments 6>$null | Out-Null
        $summary = Get-Content (Join-Path $incorrectArguments.OutputDirectory "comparison-summary.json") -Raw | ConvertFrom-Json
        Assert-Equal $false $summary.correctnessPassed "Completed tests must not turn failed scenario correctness into a successful result"
        Assert-Equal "inconclusive" $summary.verdict "The shared comparator still reports head correctness failures"

        $env:MAUI_PERF_FIXTURE_MODE = "success"
        $lockedArguments = $liveArguments.Clone()
        $lockedArguments.OutputDirectory = Join-Path $testRoot "locked-stale-output"
        $lockedDirectory = Join-Path $lockedArguments.OutputDirectory "base-discovery"
        New-Item -ItemType Directory -Path $lockedDirectory -Force | Out-Null
        $lockedPath = Join-Path $lockedDirectory "devicetestcategories.txt"
        $lockedFile = [IO.File]::Open($lockedPath, [IO.FileMode]::Create, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
        try {
            $callCount = @(Get-Content $invocations).Count
            Assert-Rejected $lockedArguments "*devicetestcategories.txt*" "An undeletable stale file must fail closed"
            Assert-Equal $callCount @(Get-Content $invocations).Count "Stale cleanup failure must not launch a process"
        }
        finally {
            $lockedFile.Dispose()
        }
    }

    if ($PSVersionTable.PSVersion.Major -ge 7) {
        $producerSource = Join-Path $repositoryRoot "src\TestUtils\src\DeviceTests.Runners\HeadlessRunner\Windows\ControlsHeadlessTestRunner.cs"
        $homePageSource = Join-Path $repositoryRoot "src\TestUtils\src\DeviceTests.Runners\VisualRunner\Pages\HomePage.xaml.cs"
        $stubs = Join-Path $testRoot "WindowsRunnerStubs.cs"
        @'
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

namespace WindowsRunnerFixture
{
    public static class State
    {
        public static string Mode = "";
        public static string[] Arguments = Array.Empty<string>();
        public static int DiscoveryCount, UiExits, Kills;
        public static int? ExplicitExit;
        public static bool CompletedBeforeWriterClosed;
        public static ControlsHeadlessTestRunner? Runner;

        public static void Run(string mode, string path, int categoryIndex, bool controls = true)
        {
            Mode = mode;
            Arguments = controls ? new[] { "app.exe", path, categoryIndex.ToString() } : new[] { "app.exe", path };
            DiscoveryCount = UiExits = Kills = 0;
            ExplicitExit = null;
            CompletedBeforeWriterClosed = false;
            Runner = null;
            new Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages.HomePage().RaiseLoaded();
        }
    }
}
namespace Microsoft.DotNet.XHarness.TestRunners.Common
{
    public interface IDevice { }
    public class LogWriter { }
    public class TestRunner { public void SkipCategories(IEnumerable<string> categories) { } }
    public class TestAssemblyInfo
    {
        public Assembly Assembly { get; }
        public TestAssemblyInfo(Assembly assembly, string location) { Assembly = assembly; }
    }
    public struct TestRunResult
    {
        public long ExecutedTests, PassedTests, FailedTests, InconclusiveTests, SkippedTests;
    }
}
namespace Microsoft.DotNet.XHarness.TestRunners.Xunit
{
    public abstract class AndroidApplicationEntryPoint
    {
        public event EventHandler<TestRunResult>? TestsCompleted;
        protected virtual bool LogExcludedTests => false;
        public abstract TextWriter? Logger { get; }
        public abstract string TestsResultsFinalPath { get; }
        protected abstract int? MaxParallelThreads { get; }
        protected abstract IDevice Device { get; }
        protected abstract IEnumerable<TestAssemblyInfo> GetTestAssemblies();
        protected abstract void TerminateWithSuccess();
        protected virtual TestRunner GetTestRunner(LogWriter writer) => new();

        public Task RunAsync()
        {
            string mode = WindowsRunnerFixture.State.Mode;
            if (mode == "runner-error")
                throw new InvalidOperationException("Fixture runner error.");
            if (mode != "no-completion")
                TestsCompleted?.Invoke(this, new TestRunResult {
                    ExecutedTests = mode == "zero-tests" ? 0 : 1,
                    PassedTests = mode == "zero-tests" || mode == "failed-tests" || mode == "inconclusive" ? 0 : 1,
                    FailedTests = mode == "failed-tests" ? 1 : 0,
                    InconclusiveTests = mode == "inconclusive" ? 1 : 0,
                    SkippedTests = mode == "some-skipped" ? 1 : 0
                });
            if (mode == "writer-error")
                throw new IOException("Fixture writer error after TestsCompleted.");
            using (var writer = mode == "missing-xml" ? null : File.CreateText(TestsResultsFinalPath))
            {
                writer?.Write(mode == "partial-xml" ? "<assemblies>" :
                    mode == "empty-xml" ? "" : "<assemblies><assembly total='1' passed='1' failed='0'/></assemblies>");
                // XHarness can request termination before its XML writer has been disposed.
                TerminateWithSuccess();
                WindowsRunnerFixture.State.CompletedBeforeWriterClosed =
                    File.Exists(WindowsRunnerFixture.State.Arguments[1] + ".completed");
            }
            return Task.CompletedTask;
        }
    }
}
namespace Xunit
{
    public enum AppDomainSupport { Denied }
    public static class TestFrameworkOptions { public static object ForDiscovery() => new(); }
    public class TestCase
    {
        public Dictionary<string, List<string>> Traits { get; } = new() {
            ["Category"] = new List<string> { "Button", "PerformanceCarouselViewWheelSnap" }
        };
    }
    public class TestDiscoverySink : IDisposable
    {
        public List<TestCase> TestCases { get; } = new();
        public ManualResetEvent Finished { get; } = new(false);
        public void Dispose() => Finished.Dispose();
    }
    public class XunitFrontController : IDisposable
    {
        public XunitFrontController(AppDomainSupport support, string path, object? config, bool shadowCopy) { }
        public void Find(bool source, TestDiscoverySink sink, object options)
        {
            WindowsRunnerFixture.State.DiscoveryCount++;
            if (WindowsRunnerFixture.State.Mode == "discovery-error" ||
                (WindowsRunnerFixture.State.Mode == "partial-discovery" && WindowsRunnerFixture.State.DiscoveryCount == 2))
                throw new InvalidOperationException("Fixture discovery error.");
            if (WindowsRunnerFixture.State.Mode != "empty-discovery")
                sink.TestCases.Add(new TestCase());
            sink.Finished.Set();
        }
        public void Dispose() { }
    }
}
namespace Microsoft.UI.Xaml
{
    public class Application
    {
        public static Application Current { get; } = new();
        public void Exit() => WindowsRunnerFixture.State.UiExits++;
    }
}
namespace Microsoft.Maui.Storage
{
    public static class FileSystemUtils { public static string PlatformGetFullAppPackageFilePath(string path) => path; }
}
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
    public class TestOptions
    {
        public Assembly[] Assemblies { get; } = new[] { typeof(TestOptions).Assembly, typeof(object).Assembly };
    }
}
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
    public class HeadlessRunnerOptions { }
    public class TestDevice : IDevice { }
    public class TestLogger : StringWriter { }
    public class HeadlessTestRunner
    {
        public static string? TestResultsFile;
        public Task RunTestsAsync() => Task.CompletedTask;
    }
}
namespace Microsoft.Maui.Controls
{
    public class ContentPage
    {
        public event EventHandler? Loaded;
        public FixtureHandler Handler { get; } = new();
        public object? BindingContext { get; set; }
        public void RaiseLoaded() => Loaded?.Invoke(this, EventArgs.Empty);
        protected virtual void OnAppearing() { }
    }
    public class FixtureHandler { public FixtureContext MauiContext { get; } = new(); }
    public class FixtureContext { public object Services { get; } = new(); }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceExtensions
    {
        public static T GetRequiredService<T>(this object services) where T : class
        {
            if (typeof(T) == typeof(ControlsHeadlessTestRunner))
            {
                var runner = new ControlsHeadlessTestRunner(new(), new());
                WindowsRunnerFixture.State.Runner = runner;
                return (T)(object)runner;
            }
            return (T)(object)new HeadlessTestRunner();
        }
    }
}
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages
{
    public class ViewModelBase { public void OnAppearing() { } }
    partial class HomePage
    {
        class AssemblyList { public object? SelectedItem { get; set; } }
        readonly AssemblyList assemblyList = new();
        void InitializeComponent() { }
    }
    // These stand-ins observe the real HomePage shutdown branch without exiting the test host.
    static class Environment
    {
        public static string[] GetCommandLineArgs() => WindowsRunnerFixture.State.Arguments;
        public static void Exit(int code) => WindowsRunnerFixture.State.ExplicitExit = code;
    }
    class Process
    {
        public static Process GetCurrentProcess() => new();
        public void Kill() => WindowsRunnerFixture.State.Kills++;
    }
}
'@ | Set-Content -LiteralPath $stubs -Encoding UTF8
        Add-Type -Path $producerSource, $homePageSource, $stubs -CompilerOptions "/define:WINDOWS"

        $producerCases = @(
            @{ Mode = "success"; Index = -1; Exit = 0 },
            @{ Mode = "success"; Index = 1; Exit = 0 },
            @{ Mode = "some-skipped"; Index = 1; Exit = 0 },
            @{ Mode = "failed-tests"; Index = 1; Exit = 1 },
            @{ Mode = "inconclusive"; Index = 1; Exit = 1 },
            @{ Mode = "zero-tests"; Index = 1; Exit = 1 },
            @{ Mode = "no-completion"; Index = 1; Exit = 1 },
            @{ Mode = "runner-error"; Index = 1; Exit = 1 },
            @{ Mode = "writer-error"; Index = 1; Exit = 1 },
            @{ Mode = "missing-xml"; Index = 1; Exit = 1 },
            @{ Mode = "partial-xml"; Index = 1; Exit = 1 },
            @{ Mode = "empty-xml"; Index = 1; Exit = 1 },
            @{ Mode = "out-of-range"; Index = 9; Exit = 1 },
            @{ Mode = "ordinary-category"; Index = 0; Exit = 1 },
            @{ Mode = "empty-discovery"; Index = -1; Exit = 1 },
            @{ Mode = "discovery-error"; Index = -1; Exit = 1 },
            @{ Mode = "partial-discovery"; Index = -1; Exit = 1 },
            @{ Mode = "completion-write-error"; Index = 1; Exit = 1 }
        )
        foreach ($case in $producerCases) {
            $directory = Join-Path $testRoot "producer-$($case.Mode)-$($case.Index)"
            New-Item -ItemType Directory -Path $directory | Out-Null
            $xmlPath = Join-Path $directory "TestResults.xml"
            if ($case.Index -ne -1) {
                @("Button", "PerformanceCarouselViewWheelSnap") | Set-Content (Join-Path $directory "devicetestcategories.txt")
            }
            if ($case.Mode -eq "completion-write-error") {
                New-Item -ItemType Directory -Path "$xmlPath.completed" | Out-Null
            }
            $env:MAUI_INCLUDE_PERFORMANCE_TESTS = "1"
            $env:MAUI_PERF_RUN_ID = [Guid]::NewGuid().ToString("N")
            [WindowsRunnerFixture.State]::Run($case.Mode, $xmlPath, $case.Index)
            Assert-Equal $case.Exit ([WindowsRunnerFixture.State]::ExplicitExit) "Producer $($case.Mode) reports an explicit exit status"
            Assert-Equal 0 ([WindowsRunnerFixture.State]::UiExits) "Performance mode must not exit while XHarness owns the XML writer"
            Assert-Equal 0 ([WindowsRunnerFixture.State]::Kills) "Performance mode must not use HomePage's ordinary Kill path"
            Assert-Equal $false ([WindowsRunnerFixture.State]::CompletedBeforeWriterClosed) "Completion follows disposal of the XML writer"
            Assert-Equal ($case.Exit -eq 0) (Test-Path -LiteralPath "$xmlPath.completed" -PathType Leaf) "Only successful producer runs create a completion marker"
            if ($case.Exit -eq 0) {
                Assert-Equal $env:MAUI_PERF_RUN_ID (Get-Content -LiteralPath "$xmlPath.completed" -Raw) "Producer writes the exact current invocation token"
            }
            else {
                Assert-Equal $true ([WindowsRunnerFixture.State]::Runner.Logger.ToString().Length -gt 0) "Producer failure is logged"
            }
        }

        foreach ($index in @(-1, 1)) {
            $directory = Join-Path $testRoot "ordinary-runner-$index"
            New-Item -ItemType Directory -Path $directory | Out-Null
            $xmlPath = Join-Path $directory "TestResults.xml"
            @("Button", "PerformanceCarouselViewWheelSnap") | Set-Content (Join-Path $directory "devicetestcategories.txt")
            [Environment]::SetEnvironmentVariable("MAUI_INCLUDE_PERFORMANCE_TESTS", $null)
            [Environment]::SetEnvironmentVariable("MAUI_PERF_RUN_ID", $null)
            [WindowsRunnerFixture.State]::Run("success", $xmlPath, $index)
            Assert-Equal $null ([WindowsRunnerFixture.State]::ExplicitExit) "Ordinary Controls runs retain their exit contract"
            Assert-Equal 1 ([WindowsRunnerFixture.State]::UiExits) "Ordinary Controls termination remains unchanged"
            Assert-Equal 1 ([WindowsRunnerFixture.State]::Kills) "Ordinary HomePage termination remains unchanged"
            Assert-Equal $false (Test-Path "$xmlPath.completed") "Ordinary runs do not opt into completion markers"
            if ($index -eq -1) {
                $categories = Get-Content (Join-Path $directory "devicetestcategories.txt")
                Assert-Equal $false ($categories -contains "PerformanceCarouselViewWheelSnap") "Ordinary discovery still excludes performance categories"
            }
        }
        $env:MAUI_PERF_RUN_ID = [Guid]::NewGuid().ToString("N")
        [WindowsRunnerFixture.State]::Run("success", (Join-Path $testRoot "generic.xml"), 0, $false)
        Assert-Equal $null ([WindowsRunnerFixture.State]::ExplicitExit) "The non-Controls headless runner is not opted into the performance contract"
        Assert-Equal 1 ([WindowsRunnerFixture.State]::Kills) "The non-Controls headless runner retains its shutdown path"
    }
    else {
        Write-Host "Producer source compilation requires PowerShell 7; fake-process driver cases ran on Windows PowerShell."
    }

    Write-Host "Windows device performance tests passed ($assertionCount assertions; PowerShell $($PSVersionTable.PSVersion))."
}
finally {
    foreach ($entry in $savedEnvironment.GetEnumerator()) {
        [Environment]::SetEnvironmentVariable($entry.Key, $entry.Value)
    }
    if ($isWindowsHost -and (Test-Path (Join-Path $testRoot "invocations.txt"))) {
        foreach ($line in Get-Content (Join-Path $testRoot "invocations.txt")) {
            $fields = $line -split '\|'
            $process = Get-Process -Id ([int]$fields[5]) -ErrorAction SilentlyContinue
            if ($null -ne $process -and $process.Path -in @($baseApp, $headApp)) {
                Stop-Process -Id $process.Id -Force
            }
        }
    }
    Remove-Item $testRoot -Recurse -Force -ErrorAction SilentlyContinue
}
