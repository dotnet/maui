#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

public sealed class HeadlessTestRunner(HeadlessRunnerOptions runnerOptions, TestOptions options)
{
	int _started;

	public async Task<NativeTestResult> RunAsync(string resultsDirectory, string[] arguments)
	{
		var (exitCode, consumer) = await RunTestsAsync(resultsDirectory, arguments);
		if (consumer.TrxReportPath is null || !File.Exists(consumer.TrxReportPath))
			throw new InvalidOperationException($"The test runner exited with {exitCode} without producing a TRX report.");

		return NativeTestResult.ReadTrx(exitCode, consumer.TrxReportPath);
	}

	internal async Task<int> RunWithControllerAsync(string resultsDirectory, string[] arguments)
	{
		// The TRX controller publishes the report after its child exits, on a separate message bus.
		// Both processes must preserve MTP's exit code; the launching runner validates the final report.
		var (exitCode, _) = await RunTestsAsync(resultsDirectory, arguments);
		return exitCode;
	}

	async Task<(int ExitCode, ResultConsumer Consumer)> RunTestsAsync(string resultsDirectory, string[] arguments)
	{
		if (Interlocked.Exchange(ref _started, 1) != 0)
			throw new InvalidOperationException("The native test application has already started a test run.");
		if (options.TestAssembly is null)
			throw new InvalidOperationException("ConfigureTests must specify the application's test assembly.");

		var args = arguments.ToList();
		args.AddRange(NativeTestArguments.GetDiagnosticArguments(args));
		var metadata = options.TestAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToArray();
		var configuredResults = metadata.SingleOrDefault(attribute => attribute.Key == "MauiTestResultsDirectory")?.Value;
		if (!string.IsNullOrEmpty(configuredResults))
			resultsDirectory = configuredResults;

#if WINDOWS
		if (args.Contains("--diagnostic"))
			MauiTestApplication.ConfigureWindowsResourceDiagnostics(resultsDirectory);
#endif

		if (!args.Contains("--results-directory") && !args.Any(arg => arg.StartsWith("--results-directory=", StringComparison.Ordinal)))
			args.AddRange(["--results-directory", resultsDirectory]);
		if (!args.Contains("--report-trx"))
			args.Add("--report-trx");
		if (!args.Contains("--report-trx-filename") && !args.Any(arg => arg.StartsWith("--report-trx-filename=", StringComparison.Ordinal)))
			args.AddRange(["--report-trx-filename", runnerOptions.TestResultsFilename]);

		var filter = metadata.SingleOrDefault(attribute => attribute.Key == "MauiTestFilter")?.Value;
		args.AddRange(NativeTestArguments.GetFilterArguments(filter));
		args.AddRange(NativeTestArguments.GetNameArguments(
			metadata.SingleOrDefault(attribute => attribute.Key == "MauiTestClasses")?.Value, "--filter-class"));
		args.AddRange(NativeTestArguments.GetNameArguments(
			metadata.SingleOrDefault(attribute => attribute.Key == "MauiTestMethods")?.Value, "--filter-method"));
		foreach (var category in options.SkipCategories.Distinct())
			args.AddRange(["--filter-not-trait", category]);

		var consumer = new ResultConsumer();
		Assembly.SetEntryAssembly(options.TestAssembly);
#if ANDROID || IOS || MACCATALYST
		// These native hosts have no console input; xUnit still constructs its console helper.
		Console.SetIn(TextReader.Null);
#endif
		var exitCode = await Xunit.MicrosoftTestingPlatform.TestPlatformTestFramework.RunAsync(
			args.ToArray(), (builder, _) =>
			{
				builder.AddTrxReportProvider();
#if WINDOWS
				builder.AddCrashDumpProvider();
#endif
				builder.TestHost.AddDataConsumer(_ => consumer);
			});

		return (exitCode, consumer);
	}

	sealed class ResultConsumer : IDataConsumer
	{
		public string? TrxReportPath { get; private set; }
		public string Uid => nameof(ResultConsumer);
		public string DisplayName => nameof(ResultConsumer);
		public string Description => "Native device test results";
		public string Version => "1.0";
		public Type[] DataTypesConsumed => [typeof(SessionFileArtifact), typeof(TestNodeUpdateMessage)];
		public Task<bool> IsEnabledAsync() => Task.FromResult(true);

		public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
		{
			if (value is SessionFileArtifact artifact && artifact.FileInfo.Extension.Equals(".trx", StringComparison.OrdinalIgnoreCase))
				TrxReportPath = artifact.FileInfo.FullName;
			else if (value is TestNodeUpdateMessage { TestNode: var node } &&
				node.Properties.SingleOrDefault<TestNodeStateProperty>() is InProgressTestNodeStateProperty)
			{
				Console.WriteLine($"TEST_START: {node.DisplayName}");
#if WINDOWS
				MauiTestApplication.CaptureWindowsResourcePressureDump();
#endif
			}
			return Task.CompletedTask;
		}
	}
}

public sealed record NativeTestResult(int ExitCode, string ResultsPath, int Passed, int Failed, int Skipped)
{
	internal static NativeTestResult ReadTrx(int exitCode, string resultsPath)
	{
		using var reader = XmlReader.Create(resultsPath, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
		var report = XDocument.Load(reader);
		XNamespace ns = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";
		var counters = report.Root?.Element(ns + "ResultSummary")?.Element(ns + "Counters")
			?? throw new InvalidDataException("The native TRX report has no result counters.");

		// Different nonserializable theory rows can share a TestNodeUid.
		return new(exitCode, resultsPath, ReadCounter("passed"), ReadCounter("failed"), ReadCounter("notExecuted"));

		int ReadCounter(string name) =>
			int.Parse(counters.Attribute(name)?.Value
				?? throw new InvalidDataException($"The native TRX report has no '{name}' counter."),
				NumberStyles.None, CultureInfo.InvariantCulture);
	}
}
