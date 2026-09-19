#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public class HeadlessTestRunner : AndroidApplicationEntryPoint
	{
		public static string? TestResultsFile;

		readonly HeadlessRunnerOptions _runnerOptions;
		readonly TestOptions _options;
		readonly string? _resultsPath;
		TestLogger _logger;
		bool _terminateAfterExecution;
		int _exitCode = 1;

		public HeadlessTestRunner(HeadlessRunnerOptions runnerOptions, TestOptions options)
		{
			_runnerOptions = runnerOptions;
			_options = options;
			_resultsPath = TestResultsFile;
			_logger = new();
		}

		protected override bool LogExcludedTests => true;

		public override TextWriter? Logger => _logger;

		public override string TestsResultsFinalPath => _resultsPath!;

		protected override int? MaxParallelThreads => Environment.ProcessorCount;

		protected override IDevice Device { get; } = new TestDevice();

		protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
			_options.Assemblies
				.Distinct()
				.Select(assembly => new TestAssemblyInfo(assembly, assembly.Location));

		protected override void TerminateWithSuccess()
		{
			// XHarness still owns the XML writer here; exit only after RunAsync disposes it.
			_terminateAfterExecution = true;
		}

		protected override TestRunner GetTestRunner(LogWriter logWriter)
		{
			var testRunner = base.GetTestRunner(logWriter);

			if (_options.SkipCategories?.Count > 0)
				testRunner.SkipCategories(_options.SkipCategories);

			return testRunner;
		}

		public async Task<string?> RunTestsAsync(bool terminateAfterExecution = false)
		{
			_terminateAfterExecution = terminateAfterExecution;
			TestsCompleted += OnTestsCompleted;

			try
			{
				await RunAsync();
			}
			catch (Exception ex)
			{
				_logger.WriteLine(ex.ToString());
				_exitCode = 1;
				_terminateAfterExecution |= ApplicationOptions.Current.TerminateAfterExecution;
			}
			TestsCompleted -= OnTestsCompleted;

			if (_terminateAfterExecution)
				Environment.Exit(_exitCode);

			if (File.Exists(TestsResultsFinalPath))
				return TestsResultsFinalPath;

			return null;

			void OnTestsCompleted(object? sender, TestRunResult results)
			{
				_exitCode = results.FailedTests == 0 ? 0 : 1;
				var message =
					$"Tests run: {results.ExecutedTests} " +
					$"Passed: {results.PassedTests} " +
					$"Inconclusive: {results.InconclusiveTests} " +
					$"Failed: {results.FailedTests} " +
					$"Ignored: {results.SkippedTests}";

				_logger.WriteLine("test-execution-summary" + message);
				_logger.WriteLine("return-code " + _exitCode);
			}
		}
	}
}