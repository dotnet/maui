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
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
{
	public class PerCategoryHeadlessTestRunner : AndroidApplicationEntryPoint
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

		public PerCategoryHeadlessTestRunner(HeadlessRunnerOptions runnerOptions, TestOptions options)
		{
			_runnerOptions = runnerOptions;
			_options = options;
			_resultsPath = TestResultsFile;
			_categoriesFilePath = Path.Combine(Path.GetDirectoryName(_resultsPath) ?? string.Empty, CategoriesFileName);
			_loopCount = LoopCount ?? 0;
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
			UI.Xaml.Application.Current.Exit();
		}

		protected override TestRunner GetTestRunner(LogWriter logWriter)
		{
			var testRunner = base.GetTestRunner(logWriter);

			var allCategories = File.ReadAllLines(_categoriesFilePath);
			var categoriesToRun = allCategories.Skip(_loopCount).Take(1).ToArray();

			List<string> categoriesToSkip = new();
			if (_options.SkipCategories?.Count > 0)
			{
				categoriesToSkip.AddRange(_options.SkipCategories);
			}

			foreach (var test in allCategories.Except(categoriesToRun))
			{
				categoriesToSkip.Add($"Category={test}");
			}

			var currentCategory = categoriesToRun[0];
			var resultPath = _resultsPath?.Split(".xml") ?? new[] { "" };
			_resultsPath = $"{resultPath[0]}_{currentCategory}.xml";

			testRunner.SkipCategories(categoriesToSkip);

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
					File.WriteAllLines(_categoriesFilePath, categories);

					TerminateWithSuccess();
					return null;
				}

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

		ICollection<string> DiscoverTestsInAssemblies()
		{
			var result = new HashSet<string>();

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

							var skipped = new HashSet<string>();

							foreach (var test in sink.TestCases)
							{
								if (test.Traits.TryGetValue("Category", out var categories))
								{
									foreach (var category in categories)
									{
										result.Add(category);
									}
								}
								else
								{
									skipped.Add($"{test.TestMethod.TestClass.Class.Name}");
								}
							}

							if (skipped.Count > 0)
							{
								throw new Exception($"Some tests do not have a category: {string.Join(", ", skipped)}");
							}
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