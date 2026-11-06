#nullable enable
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Testing.Extensions;
using Microsoft.Testing.Platform.Extensions;
using Microsoft.Testing.Platform.Extensions.Messages;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

public sealed class HeadlessTestRunner(HeadlessRunnerOptions runnerOptions, TestOptions options)
{
	int _started;

	public async Task<NativeTestResult> RunAsync(string resultsDirectory, string[] arguments)
	{
		if (Interlocked.Exchange(ref _started, 1) != 0)
			throw new InvalidOperationException("The native test application has already started a test run.");
		if (options.TestAssembly is null)
			throw new InvalidOperationException("ConfigureTests must specify the application's test assembly.");

		var args = arguments.ToList();
		var metadata = options.TestAssembly.GetCustomAttributes<AssemblyMetadataAttribute>().ToArray();
		var configuredResults = metadata.SingleOrDefault(attribute => attribute.Key == "MauiTestResultsDirectory")?.Value;
		if (!string.IsNullOrEmpty(configuredResults))
			resultsDirectory = configuredResults;

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
#if ANDROID
		// Android has no console input; xUnit still constructs its console helper.
		Console.SetIn(TextReader.Null);
#endif
		var exitCode = await Xunit.MicrosoftTestingPlatform.TestPlatformTestFramework.RunAsync(
			args.ToArray(), (builder, _) =>
			{
				builder.AddTrxReportProvider();
				builder.TestHost.AddDataConsumer(_ => consumer);
			});

		if (consumer.TrxReportPath is null || !File.Exists(consumer.TrxReportPath))
			throw new InvalidOperationException($"The test runner exited with {exitCode} without producing a TRX report.");

		return new NativeTestResult(exitCode, consumer.TrxReportPath, consumer.Passed, consumer.Failed, consumer.Skipped);
	}

	sealed class ResultConsumer : IDataConsumer
	{
		readonly ConcurrentDictionary<TestNodeUid, string> _outcomes = new();
		public string? TrxReportPath { get; private set; }
		public int Passed => _outcomes.Values.Count(value => value == "passed");
		public int Failed => _outcomes.Values.Count(value => value == "failed");
		public int Skipped => _outcomes.Values.Count(value => value == "skipped");
		public string Uid => nameof(ResultConsumer);
		public string DisplayName => nameof(ResultConsumer);
		public string Description => "Native device test results";
		public string Version => "1.0";
		public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage), typeof(SessionFileArtifact)];
		public Task<bool> IsEnabledAsync() => Task.FromResult(true);

		public Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
		{
			if (value is SessionFileArtifact artifact && artifact.FileInfo.Extension.Equals(".trx", StringComparison.OrdinalIgnoreCase))
				TrxReportPath = artifact.FileInfo.FullName;
			else if (value is TestNodeUpdateMessage { TestNode: var node })
			{
				var outcome = node.Properties.SingleOrDefault<TestNodeStateProperty>() switch
				{
					PassedTestNodeStateProperty => "passed",
					FailedTestNodeStateProperty or ErrorTestNodeStateProperty or TimeoutTestNodeStateProperty => "failed",
					SkippedTestNodeStateProperty => "skipped",
					_ => null
				};
				if (outcome is not null)
				{
					_outcomes[node.Uid] = outcome;
				}
			}
			return Task.CompletedTask;
		}
	}
}

public sealed record NativeTestResult(int ExitCode, string ResultsPath, int Passed, int Failed, int Skipped);
