#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using Xunit;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public class ControlsHeadlessTestRunner : AndroidApplicationEntryPoint
	{
		const string CategoriesFileName = "devicetestcategories.txt";
		const string PerformanceCategoryPrefix = "Performance";
		const string IncludePerformanceTestsEnvironmentVariable = "MAUI_INCLUDE_PERFORMANCE_TESTS";
		readonly string _categoriesFilePath;
		readonly string _completionFilePath;
		readonly string? _performanceRunId = Environment.GetEnvironmentVariable("MAUI_PERF_RUN_ID");

		internal bool IsPerformanceRun => !string.IsNullOrEmpty(_performanceRunId);
		internal int PerformanceExitCode { get; private set; } = 1;

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
			_completionFilePath = _resultsPath + ".completed";
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
			// The performance host exits only after RunAsync has closed the result writer.
			if (!IsPerformanceRun)
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
			bool testsPassed = false;
			TestsCompleted += OnTestsCompleted;

			try
			{
				// Got called with -1 parameter, just discover the tests to run
				if (_loopCount == -1)
				{
					var categories = DiscoverTestsInAssemblies().ToArray();
					if (IsPerformanceRun && categories.Length == 0)
						throw new InvalidOperationException("No Windows test categories were discovered.");

					File.WriteAllLines(_categoriesFilePath, categories);
					CompletePerformanceRun();

					TerminateWithSuccess();
					return null;
				}

				var allCategories = File.ReadAllLines(_categoriesFilePath);
				var categoriesToRun = allCategories.Skip(_loopCount).Take(1).ToArray();

				if (categoriesToRun.Length == 0)
				{
					_logger.WriteLine($"ERROR: Category index {_loopCount} out of range (categories file has {allCategories.Length} entries at '{_categoriesFilePath}').");
					return null;
				}

				foreach (var test in allCategories.Except(categoriesToRun))
				{
					_categoriesToSkip.Add($"Category={test}");
				}

				var currentCategory = categoriesToRun[0];
				if (IsPerformanceRun && !currentCategory.StartsWith(PerformanceCategoryPrefix, StringComparison.Ordinal))
					throw new InvalidOperationException($"'{currentCategory}' is not a Windows performance test category.");

				_resultsPath = $"{Path.ChangeExtension(_resultsPath, null)}_{currentCategory}.xml";

				await RunAsync();

				if (IsPerformanceRun)
				{
					if (!testsPassed)
						throw new InvalidOperationException("Windows performance tests did not complete successfully.");

					XDocument.Load(TestsResultsFinalPath);
					CompletePerformanceRun();
				}
			}
			catch (Exception ex)
			{
				_logger.WriteLine(ex.ToString());
			}
			finally
			{
				TestsCompleted -= OnTestsCompleted;
			}

			if (File.Exists(TestsResultsFinalPath))
				return TestsResultsFinalPath;

			return null;

			void CompletePerformanceRun()
			{
				if (IsPerformanceRun)
				{
					File.WriteAllText(_completionFilePath, _performanceRunId);
					PerformanceExitCode = 0;
				}
			}

			void OnTestsCompleted(object? sender, TestRunResult results)
			{
				testsPassed = results.ExecutedTests > 0 && results.PassedTests > 0 &&
					results.FailedTests == 0 && results.InconclusiveTests == 0;
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

							var categories = sink.TestCases.SelectMany(tc => tc.Traits["Category"]).Distinct();
							if (!string.Equals(
								Environment.GetEnvironmentVariable(IncludePerformanceTestsEnvironmentVariable),
								"1",
								StringComparison.Ordinal))
							{
								categories = categories.Where(category =>
									!category.StartsWith(PerformanceCategoryPrefix, StringComparison.Ordinal));
							}

							result.AddRange(categories);
						}
					}
					catch (Exception e) when (!IsPerformanceRun)
					{
						Debug.WriteLine(e);
					}
				}
			}
			catch (Exception e) when (!IsPerformanceRun)
			{
				Debug.WriteLine(e);
			}

			return result;
		}
	}
}