#Requires -Version 7.2
#Requires -Modules Pester

Describe 'Windows headless runner termination' {
    BeforeAll {
        $powershellPath = (Get-Process -Id $PID).Path
        $sourceDirectory = Join-Path $PSScriptRoot '../../src/TestUtils/src/DeviceTests.Runners/HeadlessRunner/Windows'
        $stubsPath = Join-Path $TestDrive 'RunnerDependencies.cs'
        $harnessPath = Join-Path $TestDrive 'Run-HeadlessRunner.ps1'
        @'
#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.DotNet.XHarness.TestRunners.Common
{
    public interface IDevice { }
    public class LogWriter { }
    public class TestRunner { public void SkipCategories(IEnumerable<string> categories) { } }
    public class TestAssemblyInfo
    {
        public Assembly Assembly { get; }
        public TestAssemblyInfo(Assembly assembly, string path) => Assembly = assembly;
    }
    public class ApplicationOptions
    {
        public static ApplicationOptions Current { get; } = new();
        public bool TerminateAfterExecution { get; set; }
    }
    public class TestRunResult : EventArgs
    {
        public int ExecutedTests => 1;
        public int PassedTests => FailedTests == 0 ? 1 : 0;
        public int InconclusiveTests => 0;
        public int FailedTests { get; set; }
        public int SkippedTests => 0;
    }
}
namespace Microsoft.DotNet.XHarness.TestRunners.Xunit
{
    using Microsoft.DotNet.XHarness.TestRunners.Common;
    public abstract class AndroidApplicationEntryPoint
    {
        public static int FailedTests;
        public static bool ThrowDuringExecution;
        public event EventHandler<TestRunResult>? TestsCompleted;
        public abstract TextWriter? Logger { get; }
        public abstract string TestsResultsFinalPath { get; }
        protected abstract int? MaxParallelThreads { get; }
        protected abstract IDevice Device { get; }
        protected abstract IEnumerable<TestAssemblyInfo> GetTestAssemblies();
        protected abstract void TerminateWithSuccess();
        protected virtual bool LogExcludedTests => false;
        protected virtual TestRunner GetTestRunner(LogWriter writer) => new();

        public Task RunAsync()
        {
            // Match XHarness ownership: termination is requested before its XML writer is disposed.
            using (var writer = new StreamWriter(TestsResultsFinalPath))
            {
                writer.Write("flushed results");
                if (ThrowDuringExecution)
                    throw new InvalidOperationException("Test execution failed");
                TestsCompleted?.Invoke(this, new TestRunResult { FailedTests = FailedTests });
                if (ApplicationOptions.Current.TerminateAfterExecution)
                    TerminateWithSuccess();
            }
            return Task.CompletedTask;
        }
    }
}
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
    public class HeadlessRunnerOptions { }
    public class TestOptions
    {
        public List<Assembly> Assemblies { get; } = new() { typeof(TestOptions).Assembly };
        public List<string> SkipCategories { get; } = new();
    }
    public class TestLogger : StringWriter { }
    public class TestDevice : Microsoft.DotNet.XHarness.TestRunners.Common.IDevice { }
}
namespace Microsoft.UI.Xaml
{
    public class Application
    {
        public static Application Current { get; } = new();
        public void Exit() => Environment.Exit(-1);
    }
}
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static T GetRequiredService<T>(this IServiceProvider services) =>
            (T)services.GetService(typeof(T))!;
    }
}
namespace Microsoft.Maui.Controls
{
    using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
    public class ContentPage
    {
        public event EventHandler? Loaded;
        public TestHandler Handler { get; } = new();
        public object? BindingContext { get; set; }
        public void RaiseLoaded() => Loaded?.Invoke(this, EventArgs.Empty);
        protected virtual void OnAppearing() { }
    }
    public class TestHandler { public TestContext MauiContext { get; } = new(); }
    public class TestContext { public IServiceProvider Services { get; } = new TestServices(); }
    public class TestServices : IServiceProvider
    {
        public object? GetService(Type type) =>
            Activator.CreateInstance(type, new HeadlessRunnerOptions(), new TestOptions());
    }
}
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
    public class ViewModelBase { public void OnAppearing() { } }
}
namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages
{
    // Supply CLI arguments to the real HomePage without changing the subprocess's own arguments.
    static class Environment
    {
        public static string[] GetCommandLineArgs() => HomePageTestHost.Arguments;
    }
    public static class HomePageTestHost
    {
        public static string[] Arguments = Array.Empty<string>();
        public static void Run() => new HomePage().RaiseLoaded();
    }
    partial class HomePage
    {
        readonly TestAssemblyList assemblyList = new();
        void InitializeComponent() { }
    }
    class TestAssemblyList { public object? SelectedItem { get; set; } }
}
namespace Microsoft.Maui.Storage
{
    public static class FileSystemUtils
    {
        public static string PlatformGetFullAppPackageFilePath(string path) => path;
    }
}
namespace Xunit
{
    public enum AppDomainSupport { Denied }
    public static class TestFrameworkOptions { public static object ForDiscovery() => new(); }
    public class TestCase
    {
        public Dictionary<string, List<string>> Traits { get; } = new() { ["Category"] = new() { "Test" } };
    }
    public class TestDiscoverySink : IDisposable
    {
        public ManualResetEvent Finished { get; } = new(false);
        public List<TestCase> TestCases { get; } = new();
        public void Dispose() => Finished.Dispose();
    }
    public class XunitFrontController : IDisposable
    {
        public XunitFrontController(AppDomainSupport support, string path, object? config, bool shadowCopy) { }
        public void Find(bool includeSource, TestDiscoverySink sink, object options)
        {
            sink.TestCases.Add(new());
            sink.Finished.Set();
        }
        public void Dispose() { }
    }
}
'@ | Set-Content -LiteralPath $stubsPath
        @'
param([string]$SourceDirectory, [string]$StubsPath, [string]$ResultsPath, [string]$RunnerType, [string]$Mode, [string]$EntryPoint)
$ErrorActionPreference = 'Stop'
Add-Type -Path @(
    $StubsPath
    (Join-Path $SourceDirectory 'HeadlessTestRunner.cs')
    (Join-Path $SourceDirectory 'ControlsHeadlessTestRunner.cs')
    (Join-Path $SourceDirectory '../../VisualRunner/Pages/HomePage.xaml.cs')
) -CompilerOptions '/define:WINDOWS'
$type = "Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.$RunnerType" -as [type]
$type.GetField('TestResultsFile').SetValue($null, $ResultsPath)
[Microsoft.DotNet.XHarness.TestRunners.Xunit.AndroidApplicationEntryPoint]::FailedTests = [int]($Mode -eq 'failing')
[Microsoft.DotNet.XHarness.TestRunners.Xunit.AndroidApplicationEntryPoint]::ThrowDuringExecution = $Mode -eq 'throwing'
[Microsoft.DotNet.XHarness.TestRunners.Common.ApplicationOptions]::Current.TerminateAfterExecution = $EntryPoint -eq 'runner' -and $Mode -ne 'interactive'
if ($RunnerType -eq 'ControlsHeadlessTestRunner') {
    if ($Mode -eq 'discovery') {
        [Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.ControlsHeadlessTestRunner]::LoopCount = -1
    } else {
        'Test' | Set-Content (Join-Path (Split-Path $ResultsPath) 'devicetestcategories.txt')
    }
}
if ($EntryPoint -eq 'page') {
    $cliArgs = @('DeviceTests.exe')
    if ($Mode -ne 'interactive') {
        $cliArgs += $ResultsPath
        if ($RunnerType -eq 'ControlsHeadlessTestRunner') {
            $cliArgs += $(if ($Mode -eq 'discovery') { '-1' } elseif ($Mode -eq 'invalid-category') { '99' } else { '0' })
        }
    }
    [Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages.HomePageTestHost]::Arguments = $cliArgs
    [Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages.HomePageTestHost]::Run()
    exit 99
}
$runner = [Activator]::CreateInstance($type, @(
    [Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.HeadlessRunnerOptions]::new()
    [Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.TestOptions]::new()
))
$runner.RunTestsAsync().GetAwaiter().GetResult() | Out-Null
exit 99
'@ | Set-Content -LiteralPath $harnessPath
    }

    It '<RunnerType> via <EntryPoint> reports <Expected> for <Mode> after closing its output file' -ForEach @(
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'passing'; Expected = 0 }
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'failing'; Expected = 1 }
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'throwing'; Expected = 1 }
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'interactive'; Expected = 99 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'passing'; Expected = 0 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'failing'; Expected = 1 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'throwing'; Expected = 1 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'interactive'; Expected = 99 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'runner'; Mode = 'discovery'; Expected = 0 }
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'page'; Mode = 'passing'; Expected = 0 }
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'page'; Mode = 'failing'; Expected = 1 }
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'page'; Mode = 'throwing'; Expected = 1 }
        @{ RunnerType = 'HeadlessTestRunner'; EntryPoint = 'page'; Mode = 'interactive'; Expected = 99 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'page'; Mode = 'passing'; Expected = 0 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'page'; Mode = 'failing'; Expected = 1 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'page'; Mode = 'throwing'; Expected = 1 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'page'; Mode = 'discovery'; Expected = 0 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; EntryPoint = 'page'; Mode = 'invalid-category'; Expected = 1 }
    ) {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $directory | Out-Null
        $resultsPath = Join-Path $directory 'TestResults.xml'
        $output = & $powershellPath -NoProfile -File $harnessPath $sourceDirectory $stubsPath $resultsPath $RunnerType $Mode $EntryPoint 2>&1
        $LASTEXITCODE | Should -Be $Expected -Because ($output | Out-String)

        if ($Mode -eq 'discovery') {
            Get-Content (Join-Path $directory 'devicetestcategories.txt') -Raw | Should -Match '^Test\s*$'
        } elseif ($EntryPoint -eq 'page' -and $Mode -in @('interactive', 'invalid-category')) {
            Test-Path -LiteralPath $resultsPath | Should -BeFalse
        } else {
            $resultFile = if ($RunnerType -eq 'ControlsHeadlessTestRunner') { 'TestResults_Test.xml' } else { 'TestResults.xml' }
            Get-Content (Join-Path $directory $resultFile) -Raw | Should -Be 'flushed results'
        }
    }
}
