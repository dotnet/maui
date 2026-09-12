#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
#if ANDROID
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
#endif

namespace Microsoft.Maui.DeviceTests
{
	public sealed class DevicePerformanceResult
	{
		public const int CurrentSchemaVersion = 2;

		public int SchemaVersion { get; init; } = CurrentSchemaVersion;
		public string Repository { get; init; } = DevicePerformanceEnvironment.GetValue("MAUI_PERF_REPOSITORY") ?? "unknown";
		public int PullRequestNumber { get; init; } = DevicePerformanceEnvironment.GetInt32("MAUI_PERF_PR_NUMBER");
		public string Scenario { get; init; } = string.Empty;
		public string Platform { get; init; } = DevicePerformanceEnvironment.Platform;
		public string Variant { get; init; } = DevicePerformanceEnvironment.GetValue("MAUI_PERF_VARIANT") ?? "unknown";
		public string CommitSha { get; init; } = DevicePerformanceEnvironment.GetValue("MAUI_PERF_COMMIT_SHA") ?? "unknown";
		public string HarnessSha { get; init; } = DevicePerformanceEnvironment.GetValue("MAUI_PERF_HARNESS_SHA") ?? "unknown";
		public int RunOrdinal { get; init; } = DevicePerformanceEnvironment.GetInt32("MAUI_PERF_RUN_ORDINAL");
		public int ExpectedVariantRuns { get; init; } = DevicePerformanceEnvironment.GetInt32("MAUI_PERF_EXPECTED_VARIANT_RUNS");
		public DevicePerformanceBuildIdentity Build { get; init; } = DevicePerformanceBuildIdentity.Create();
		public DevicePerformanceEnvironmentInfo Environment { get; init; } = DevicePerformanceEnvironmentInfo.Create();
		public DevicePerformanceCorrectness Correctness { get; init; } = new();
		public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
		public int WarmupCount { get; init; }
		public double[] MeasurementsMilliseconds { get; init; } = [];
		public DevicePerformanceStatistics Statistics { get; init; } = new();
		public Dictionary<string, double> Counters { get; init; } =
			new Dictionary<string, double>(StringComparer.Ordinal);
	}

	public sealed class DevicePerformanceBuildIdentity
	{
		public string AzdoBuildId { get; init; } = string.Empty;
		public string AzdoBuildUrl { get; init; } = string.Empty;
		public string HelixJobId { get; init; } = string.Empty;
		public string HelixWorkItem { get; init; } = string.Empty;

		internal static DevicePerformanceBuildIdentity Create() =>
			new()
			{
				AzdoBuildId = DevicePerformanceEnvironment.GetValue("MAUI_PERF_AZDO_BUILD_ID") ?? "unknown",
				AzdoBuildUrl = DevicePerformanceEnvironment.GetValue("MAUI_PERF_AZDO_BUILD_URL") ?? "unknown",
				HelixJobId = DevicePerformanceEnvironment.GetValue("MAUI_PERF_HELIX_JOB_ID") ?? "local",
				HelixWorkItem = DevicePerformanceEnvironment.GetValue("MAUI_PERF_HELIX_WORK_ITEM") ?? "local"
			};
	}

	public sealed class DevicePerformanceEnvironmentInfo
	{
		public string ExecutionKind { get; init; } = string.Empty;
		public string DeviceModel { get; init; } = string.Empty;
		public string OsVersion { get; init; } = string.Empty;
		public string RuntimeFramework { get; init; } = string.Empty;
		public string ProcessArchitecture { get; init; } = string.Empty;
		public string RuntimeVariant { get; init; } = string.Empty;
		public string SdkVersion { get; init; } = string.Empty;

		internal static DevicePerformanceEnvironmentInfo Create()
		{
			IDeviceInfo deviceInfo = DeviceInfo.Current;
			string executionKind = deviceInfo.DeviceType == DeviceType.Virtual
				? DevicePerformanceEnvironment.Platform switch
				{
					"android" => "emulator",
					"ios" => "simulator",
					_ => "virtual"
				}
				: DevicePerformanceEnvironment.Platform switch
				{
					"maccatalyst" => "native-host",
					"windows" => "native-host",
					_ => "physical-device"
				};

			return new DevicePerformanceEnvironmentInfo
			{
				ExecutionKind = executionKind,
				DeviceModel = deviceInfo.Model,
				OsVersion = deviceInfo.VersionString,
				RuntimeFramework = RuntimeInformation.FrameworkDescription,
				ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
				RuntimeVariant = DevicePerformanceEnvironment.GetValue("MAUI_PERF_RUNTIME_VARIANT") ?? "unknown",
				SdkVersion = DevicePerformanceEnvironment.GetValue("MAUI_PERF_SDK_VERSION") ?? "unknown"
			};
		}
	}

	public sealed class DevicePerformanceCorrectness
	{
		public bool Passed { get; init; } = true;
		public string AccessibilityStatus { get; init; } = "not-assessed";
	}

	public sealed class DevicePerformanceStatistics
	{
		public double MinimumMilliseconds { get; init; }
		public double MaximumMilliseconds { get; init; }
		public double MedianMilliseconds { get; init; }
		public double P95Milliseconds { get; init; }
		public double MeanMilliseconds { get; init; }
	}

	public static class DevicePerformanceMeasurement
	{
		public static async Task<DevicePerformanceResult> MeasureAsync(
			string scenario,
			int warmupCount,
			int iterationCount,
			Func<int, Task> operation,
			IReadOnlyDictionary<string, double>? counters = null,
			Func<int, Task>? verification = null)
		{
			ArgumentException.ThrowIfNullOrWhiteSpace(scenario);
			ArgumentNullException.ThrowIfNull(operation);

			if (warmupCount < 0)
				throw new ArgumentOutOfRangeException(nameof(warmupCount));

			if (iterationCount <= 0)
				throw new ArgumentOutOfRangeException(nameof(iterationCount));

			for (int i = 0; i < warmupCount; i++)
			{
				await operation(i).ConfigureAwait(false);
				if (verification is not null)
					await verification(i).ConfigureAwait(false);
			}

			var measurements = new double[iterationCount];
			for (int i = 0; i < iterationCount; i++)
			{
				long start = Stopwatch.GetTimestamp();
				await operation(i).ConfigureAwait(false);
				measurements[i] = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
				if (verification is not null)
					await verification(i).ConfigureAwait(false);
			}

			return new DevicePerformanceResult
			{
				Scenario = scenario,
				WarmupCount = warmupCount,
				MeasurementsMilliseconds = measurements,
				Statistics = CalculateStatistics(measurements),
				Counters = counters?.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal)
					?? new Dictionary<string, double>(StringComparer.Ordinal)
			};
		}

		public static DevicePerformanceStatistics CalculateStatistics(IReadOnlyList<double> measurements)
		{
			ArgumentNullException.ThrowIfNull(measurements);

			if (measurements.Count == 0)
				throw new ArgumentException("At least one measurement is required.", nameof(measurements));

			double[] sorted = measurements.OrderBy(value => value).ToArray();
			int middle = sorted.Length / 2;
			double median = sorted.Length % 2 == 0
				? (sorted[middle - 1] + sorted[middle]) / 2
				: sorted[middle];
			int p95Index = Math.Max(0, (int)Math.Ceiling(sorted.Length * 0.95) - 1);

			return new DevicePerformanceStatistics
			{
				MinimumMilliseconds = sorted[0],
				MaximumMilliseconds = sorted[^1],
				MedianMilliseconds = median,
				P95Milliseconds = sorted[p95Index],
				MeanMilliseconds = sorted.Average()
			};
		}
	}

	public static class DevicePerformanceReporter
	{
		public const string ResultPrefix = "MAUI_PERF_RESULT:";
		public const string ChunkPrefix = "MAUI_PERF_CHUNK:";

		public static void Write(DevicePerformanceResult result)
		{
			ArgumentNullException.ThrowIfNull(result);

			if (result.SchemaVersion != DevicePerformanceResult.CurrentSchemaVersion)
				throw new ArgumentException($"Unsupported schema version: {result.SchemaVersion}", nameof(result));

			string serializedResult =
				$"{ResultPrefix}{JsonSerializer.Serialize(result, DevicePerformanceJsonContext.Default.DevicePerformanceResult)}";
#if ANDROID
			const int chunkLength = 700;
			if (serializedResult.Length > chunkLength)
			{
				string chunkId = $"{result.Variant}-{result.RunOrdinal}-{result.TimestampUtc.UtcTicks}";
				int chunkCount = (serializedResult.Length + chunkLength - 1) / chunkLength;
				for (int index = 0; index < chunkCount; index++)
				{
					int start = index * chunkLength;
					int length = Math.Min(chunkLength, serializedResult.Length - start);
					string chunk =
						$"{ChunkPrefix}{chunkId}:{index + 1}/{chunkCount}:{serializedResult.Substring(start, length)}";
					Console.WriteLine(chunk);
					Console.Error.WriteLine(chunk);
				}
			}
			else
			{
				Console.WriteLine(serializedResult);
				Console.Error.WriteLine(serializedResult);
			}
#else
			Console.WriteLine(serializedResult);
#endif

			string? resultFile = DevicePerformanceEnvironment.GetValue("MAUI_PERF_RESULT_FILE");
			if (!string.IsNullOrWhiteSpace(resultFile))
				File.WriteAllText(resultFile, serializedResult);
		}
	}

	[JsonSourceGenerationOptions(
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
			WriteIndented = false)]
	[JsonSerializable(typeof(DevicePerformanceResult))]
	[JsonSerializable(typeof(DevicePerformanceBuildIdentity))]
	[JsonSerializable(typeof(DevicePerformanceEnvironmentInfo))]
	[JsonSerializable(typeof(DevicePerformanceCorrectness))]
	[JsonSerializable(typeof(DevicePerformanceStatistics))]
	[JsonSerializable(typeof(Dictionary<string, double>))]
	partial class DevicePerformanceJsonContext : JsonSerializerContext
	{
	}

	static class DevicePerformanceEnvironment
	{
		public static string Platform
		{
			get
			{
#if ANDROID
				return "android";
#elif MACCATALYST
				return "maccatalyst";
#elif IOS
				return "ios";
#elif WINDOWS
				return "windows";
#else
				return "unknown";
#endif
			}
		}

		public static string? GetValue(string name)
		{
#if ANDROID
			string? instrumentationValue = MauiTestInstrumentation.Current?.Arguments?.GetString(name);
			if (!string.IsNullOrWhiteSpace(instrumentationValue))
				return instrumentationValue;
#endif

			return Environment.GetEnvironmentVariable(name);
		}

		public static int GetInt32(string name)
		{
			string? value = GetValue(name);
			return int.TryParse(value, out int result) ? result : 0;
		}
	}
}
