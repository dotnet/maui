#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using Xunit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public class ControlsHeadlessTestRunner : AndroidApplicationEntryPoint
	{
		const string CategoriesFileName = "devicetestcategories.txt";
		readonly string _categoriesFilePath;

		public static string? TestResultsFile;
		public static int? LoopCount;

		readonly HeadlessRunnerOptions _runnerOptions;
		readonly TestOptions _options;
		string? _resultsPath;
		readonly int _loopCount;
		TestLogger _logger;
		List<string> _categoriesToSkip;

		public ControlsHeadlessTestRunner(HeadlessRunnerOptions runnerOptions, TestOptions options)
		{
			_runnerOptions = runnerOptions;
			_options = options;
			_resultsPath = TestResultsFile;
			_categoriesFilePath = Path.Combine(Path.GetDirectoryName(_resultsPath) ?? string.Empty, CategoriesFileName);
			_loopCount = LoopCount ?? 0;
			_logger = new();
			_categoriesToSkip = new List<string>();
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
			UI.Xaml.Application.Current.Exit();
		}

		protected override TestRunner GetTestRunner(LogWriter logWriter)
		{
			var testRunner = base.GetTestRunner(logWriter);

			testRunner.SkipCategories(_categoriesToSkip);

			return testRunner;
		}

		public async Task<string?> RunTestsAsync()
		{
			TestsCompleted += OnTestsCompleted;

			try
			{
				// Got called with -1 parameter, just discover the tests to run
				if (_loopCount == -1)
				{
					var categories = DiscoverTestsInAssemblies();
					File.WriteAllLines(_categoriesFilePath, categories.ToArray());

					TerminateWithSuccess();
					return null;
				}

				var allCategories = File.ReadAllLines(_categoriesFilePath);
				var categoriesToRun = allCategories.Skip(_loopCount).Take(1).ToArray();

				foreach (var test in allCategories.Except(categoriesToRun))
				{
					_categoriesToSkip.Add($"Category={test}");
				}

				var currentCategory = categoriesToRun[0];
				var resultPath = _resultsPath?.Split(".xml") ?? new[] { "" };
				_resultsPath = $"{resultPath[0]}_{currentCategory}.xml";

				await RunAsync();
			}
			catch (Exception ex)
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

				_logger.WriteLine("test-execution-summary" + message);
				_logger.WriteLine("return-code " + (results.FailedTests == 0 ? 0 : 1));
			}
		}

		IEnumerable<string> DiscoverTestsInAssemblies()
		{
			var result = new List<string>();

			try
			{
				foreach (var assm in GetTestAssemblies())
				{
					var nameWithoutExt = assm.Assembly.GetName().Name;
					var assemblyFileName = Storage.FileSystemUtils.PlatformGetFullAppPackageFilePath($"{nameWithoutExt}.dll");

					var discoveryOptions = TestFrameworkOptions.ForDiscovery();

					try
					{
						using (var framework = new XunitFrontController(AppDomainSupport.Denied, assemblyFileName, null, false))
						using (var sink = new TestDiscoverySink())
						{
							framework.Find(false, sink, discoveryOptions);
							sink.Finished.WaitOne();

							result.AddRange(sink.TestCases.SelectMany(tc => tc.Traits["Category"]).Distinct());
						}
					}
					catch (Exception e)
					{
						Debug.WriteLine(e);
					}
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
			}

			return result;
		}
	}
}