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
        public bool TerminateAfterExecution { get; set; } = true;
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
param([string]$SourceDirectory, [string]$StubsPath, [string]$ResultsPath, [string]$RunnerType, [string]$Mode)
$ErrorActionPreference = 'Stop'
Add-Type -Path @(
    $StubsPath
    (Join-Path $SourceDirectory 'HeadlessTestRunner.cs')
    (Join-Path $SourceDirectory 'ControlsHeadlessTestRunner.cs')
)
$type = "Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.$RunnerType" -as [type]
$type.GetField('TestResultsFile').SetValue($null, $ResultsPath)
[Microsoft.DotNet.XHarness.TestRunners.Xunit.AndroidApplicationEntryPoint]::FailedTests = [int]($Mode -eq 'failing')
[Microsoft.DotNet.XHarness.TestRunners.Xunit.AndroidApplicationEntryPoint]::ThrowDuringExecution = $Mode -eq 'throwing'
[Microsoft.DotNet.XHarness.TestRunners.Common.ApplicationOptions]::Current.TerminateAfterExecution = $Mode -ne 'interactive'
if ($RunnerType -eq 'ControlsHeadlessTestRunner') {
    if ($Mode -eq 'discovery') {
        [Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.ControlsHeadlessTestRunner]::LoopCount = -1
    } else {
        'Test' | Set-Content (Join-Path (Split-Path $ResultsPath) 'devicetestcategories.txt')
    }
}
$runner = [Activator]::CreateInstance($type, @(
    [Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.HeadlessRunnerOptions]::new()
    [Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.TestOptions]::new()
))
$runner.RunTestsAsync().GetAwaiter().GetResult() | Out-Null
exit 99
'@ | Set-Content -LiteralPath $harnessPath
    }

    It '<RunnerType> reports <Expected> for <Mode> after closing its output file' -ForEach @(
        @{ RunnerType = 'HeadlessTestRunner'; Mode = 'passing'; Expected = 0 }
        @{ RunnerType = 'HeadlessTestRunner'; Mode = 'failing'; Expected = 1 }
        @{ RunnerType = 'HeadlessTestRunner'; Mode = 'throwing'; Expected = 1 }
        @{ RunnerType = 'HeadlessTestRunner'; Mode = 'interactive'; Expected = 99 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; Mode = 'passing'; Expected = 0 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; Mode = 'failing'; Expected = 1 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; Mode = 'throwing'; Expected = 1 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; Mode = 'interactive'; Expected = 99 }
        @{ RunnerType = 'ControlsHeadlessTestRunner'; Mode = 'discovery'; Expected = 0 }
    ) {
        $directory = Join-Path $TestDrive ([guid]::NewGuid().ToString())
        New-Item -ItemType Directory -Path $directory | Out-Null
        $resultsPath = Join-Path $directory 'TestResults.xml'
        $output = & $powershellPath -NoProfile -File $harnessPath $sourceDirectory $stubsPath $resultsPath $RunnerType $Mode 2>&1
        $LASTEXITCODE | Should -Be $Expected -Because ($output | Out-String)

        if ($Mode -eq 'discovery') {
            Get-Content (Join-Path $directory 'devicetestcategories.txt') -Raw | Should -Match '^Test\s*$'
        } else {
            $resultFile = if ($RunnerType -eq 'ControlsHeadlessTestRunner') { 'TestResults_Test.xml' } else { 'TestResults.xml' }
            Get-Content (Join-Path $directory $resultFile) -Raw | Should -Be 'flushed results'
        }
    }
}
