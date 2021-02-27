using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.DotNet.XHarness.TestRunners.Xunit;
using Microsoft.Maui.Essentials;

namespace Microsoft.Maui.TestUtils
{
	public class TestEntryPoint : AndroidApplicationEntryPoint
	{
		public const string DefaultTestResultsFilename = "TestResults.xml";

		readonly ITestEntryPoint _testEntryPoint;
		readonly string _resultsPath;

		public TestEntryPoint(ITestEntryPoint testEntryPoint, string resultsFileName)
		{
#pragma warning disable CS0618 // Type or member is obsolete
			var root = ((int)Build.VERSION.SdkInt) >= 30
				? global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath
				: Application.Context.GetExternalFilesDir(null)?.AbsolutePath ?? FileSystem.AppDataDirectory;
#pragma warning restore CS0618 // Type or member is obsolete

			var docsDir = Path.Combine(root, "Documents");

			if (!Directory.Exists(docsDir))
				Directory.CreateDirectory(docsDir);

			_resultsPath = Path.Combine(docsDir, resultsFileName);
			_testEntryPoint = testEntryPoint;
		}

		protected override bool LogExcludedTests => true;

		public override TextWriter Logger => null;

		public override string TestsResultsFinalPath => _resultsPath;

		protected override int? MaxParallelThreads => System.Environment.ProcessorCount;

		protected override IDevice Device { get; } = new TestDevice();

		protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
			_testEntryPoint.GetTestAssemblies();

		protected override void TerminateWithSuccess() =>
			_testEntryPoint.TerminateWithSuccess();

		protected override TestRunner GetTestRunner(LogWriter logWriter) =>
			_testEntryPoint.GetTestRunner(base.GetTestRunner(logWriter), logWriter);

		public static async Task<Bundle> RunTestsAsync(ITestEntryPoint testEntryPoint, Bundle arguments = null)
		{
			var resultsFileName = arguments?.GetString("results-file-name", DefaultTestResultsFilename) ?? DefaultTestResultsFilename;

			var bundle = new Bundle();

			var entryPoint = new TestEntryPoint(testEntryPoint, resultsFileName);
			entryPoint.TestsCompleted += (sender, results) =>
			{
				var message =
					$"Tests run: {results.ExecutedTests} " +
					$"Passed: {results.PassedTests} " +
					$"Inconclusive: {results.InconclusiveTests} " +
					$"Failed: {results.FailedTests} " +
					$"Ignored: {results.SkippedTests}";
				bundle.PutString("test-execution-summary", message);

				bundle.PutLong("return-code", results.FailedTests == 0 ? 0 : 1);
			};

			await entryPoint.RunAsync();

			if (File.Exists(entryPoint.TestsResultsFinalPath))
				bundle.PutString("test-results-path", entryPoint.TestsResultsFinalPath);

			if (bundle.GetLong("return-code", -1) == -1)
				bundle.PutLong("return-code", 1);

			return bundle;
		}
	}
}