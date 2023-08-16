#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using Windows.ApplicationModel;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public class HeadlessTestRunner : AndroidApplicationEntryPoint
	{
		public static string? TestResultsFile;

		readonly HeadlessRunnerOptions _runnerOptions;
		readonly TestOptions _options;
		readonly string? _resultsPath;
		TestLogger _logger;

		public HeadlessTestRunner(HeadlessRunnerOptions runnerOptions, TestOptions options)
		{
			_runnerOptions = runnerOptions;
			_options = options;
			_resultsPath = TestResultsFile;

			var logFileName = $"test-output-{DateTime.Now:yyyyMMdd_hhmmss}.log";
			if (!string.IsNullOrEmpty(_resultsPath))
			{
				var dir = Path.GetDirectoryName(_resultsPath);
				if (!string.IsNullOrEmpty(dir))
					_logger = new(Path.Combine(dir, logFileName));
				else
					_logger = new(logFileName);
			}
			else
			{
				_logger = new(logFileName);
			}
		}

		protected override bool LogExcludedTests => true;

		public override TextWriter? Logger => _logger;

		public override string TestsResultsFinalPath => _resultsPath!;

		protected override int? MaxParallelThreads => System.Environment.ProcessorCount;

		protected override IDevice Device { get; } = new TestDevice();

		protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
			_options.Assemblies
				.Distinct()
				.Select(assembly => new TestAssemblyInfo(assembly, assembly.Location));

		protected override void TerminateWithSuccess()
		{
			Microsoft.UI.Xaml.Application.Current.Exit();
		}

		protected override TestRunner GetTestRunner(LogWriter logWriter)
		{
			var testRunner = base.GetTestRunner(logWriter);

			if (_options.SkipCategories?.Count > 0)
				testRunner.SkipCategories(_options.SkipCategories);

			return testRunner;
		}

		public async Task<string?> RunTestsAsync()
		{
			var dir = Path.GetDirectoryName(TestResultsFile);
			if (!string.IsNullOrEmpty(dir))
				Directory.CreateDirectory(dir);

			TestsCompleted += OnTestsCompleted;

			try
			{
				_logger.WriteLine("Starting tests...");

				await RunAsync();

				_logger.WriteLine("Tests completed.");
			}
			catch (System.Exception ex)
			{
				_logger.WriteLine(ex.ToString());
			}
			TestsCompleted -= OnTestsCompleted;

			if (File.Exists(TestsResultsFinalPath))
				return TestsResultsFinalPath;

			return null;

			void OnTestsCompleted(object? sender, TestRunResult results)
			{
				var message =
					$"Tests run: {results.ExecutedTests} " +
					$"Passed: {results.PassedTests} " +
					$"Inconclusive: {results.InconclusiveTests} " +
					$"Failed: {results.FailedTests} " +
					$"Ignored: {results.SkippedTests}";

				_logger.WriteLine("test-execution-summary => " + message);
				_logger.WriteLine("return-code => " + (results.FailedTests == 0 ? 0 : 1));
			}
		}
	}

	public class TestLogger : System.IO.TextWriter
	{
		string? _logFile;

		public TestLogger(string? logFile)
		{
			_logFile = logFile;

			if (string.IsNullOrEmpty(_logFile))
			{
				var dir = Path.GetDirectoryName(_logFile);
				if (!string.IsNullOrEmpty(dir))
					Directory.CreateDirectory(dir);
			}
		}

		public override void Write(char value)
		{
			Console.Write(value);
			System.Diagnostics.Debug.Write(value);

			if (!string.IsNullOrEmpty(_logFile))
				File.AppendAllText(_logFile, value.ToString());
		}

		public override void WriteLine(string? value)
		{
			Console.WriteLine(value);
			System.Diagnostics.Debug.WriteLine(value);
			
			if (!string.IsNullOrEmpty(_logFile))
				File.AppendAllLines(_logFile, new[] { value?.ToString() ?? "" });
		}

		public override Encoding Encoding => Encoding.Default;
	}
}