#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners
{
	public static class AppHostBuilderExtensions
	{
		public static MauiAppBuilder ConfigureTests(this MauiAppBuilder appHostBuilder, TestOptions options)
		{
			appHostBuilder.Services.AddSingleton(options);

			appHostBuilder.Logging.AddConsole();
			// appHostBuilder.Logging.SetMinimumLevel(LogLevel.Debug);
			return appHostBuilder;
		}

		public static MauiAppBuilder ConfigureTests(this MauiAppBuilder appHostBuilder, Func<IServiceProvider, TestOptions> options)
		{
			appHostBuilder.Services.AddSingleton(options);

			appHostBuilder.Logging.AddConsole();
			// appHostBuilder.Logging.SetMinimumLevel(LogLevel.Debug);
			return appHostBuilder;
		}

		public static MauiAppBuilder UseVisualRunner(this MauiAppBuilder appHostBuilder)
		{
			appHostBuilder.UseMauiApp(svc => new MauiVisualRunnerApp(
				svc.GetRequiredService<TestOptions>(),
				svc.GetRequiredService<ILoggerFactory>().CreateLogger("TestRun")));

			return appHostBuilder;
		}

		public static MauiAppBuilder UseHeadlessRunner(this MauiAppBuilder appHostBuilder, HeadlessRunnerOptions options)
		{
			appHostBuilder.Services.AddSingleton(options);

#if __ANDROID__ || __IOS__ || MACCATALYST || WINDOWS
			appHostBuilder.Services.AddTransient<HeadlessTestRunner>(svc => new ReplicationWindowsExactHeadlessTestRunner(
					svc.GetRequiredService<HeadlessRunnerOptions>(),
					svc.GetRequiredService<TestOptions>()));
#endif

#if WINDOWS
			// Also register the discovery/index-capable runner so a single-category run
			// ("App.exe <resultsFile> <categoryIndex>", e.g. how the Copilot review gate
			// verifies just the changed test category) works for ANY Windows device-test
			// app — not only Controls. HomePage resolves this runner solely when a
			// category-index CLI arg is supplied; the default full-suite run
			// ("App.exe <resultsFile>") still resolves HeadlessTestRunner, so the behavior
			// of the real device-test pipeline (which never passes a category index) is
			// unchanged. This lets the gate filter Core/Essentials/Graphics/BlazorWebView
			// Windows device tests instead of running the whole app (which can crash and
			// yield empty results, forcing an inconclusive gate).
			appHostBuilder.Services.AddTransient(svc => new ControlsHeadlessTestRunner(
					svc.GetRequiredService<HeadlessRunnerOptions>(),
					svc.GetRequiredService<TestOptions>()));
#endif

			appHostBuilder.Logging.AddConsole();

			return appHostBuilder;
		}

#if WINDOWS
		public static MauiAppBuilder UseControlsHeadlessRunner(this MauiAppBuilder appHostBuilder, HeadlessRunnerOptions options)
		{
			appHostBuilder.Services.AddSingleton(options);

			appHostBuilder.Services.AddTransient(svc => new ControlsHeadlessTestRunner(
					svc.GetRequiredService<HeadlessRunnerOptions>(),
					svc.GetRequiredService<TestOptions>()));
			appHostBuilder.Services.AddTransient<HeadlessTestRunner>(svc => new ReplicationWindowsExactHeadlessTestRunner(
					svc.GetRequiredService<HeadlessRunnerOptions>(),
					svc.GetRequiredService<TestOptions>()));

			return appHostBuilder;
		}
#endif
	}

#if WINDOWS
	internal sealed class ReplicationWindowsExactHeadlessTestRunner : HeadlessTestRunner
	{
		readonly ReplicationWindowsExactRunnerDiagnostics _diagnostics;
		readonly TestOptions _options;

		public ReplicationWindowsExactHeadlessTestRunner(
			HeadlessRunnerOptions runnerOptions,
			TestOptions options)
			: base(runnerOptions, options)
		{
			_options = options;
			_diagnostics = new(options);
		}

		protected override TestRunner GetTestRunner(LogWriter logWriter)
		{
			_diagnostics.BeginRunnerConfiguration();
			try
			{
				var selection = DiscoverExpectedTest(_options, _diagnostics);
				ReplicationWindowsDeviceTestClassFilter.RefreshApplicationOptions(
					selection.ClassName,
					selectedMethod: null);
				var runner = base.GetTestRunner(logWriter);
				ConfigureExactRunner(runner, selection);
				_diagnostics.RecordRunner(runner);
				return runner;
			}
			catch (Exception ex)
			{
				_diagnostics.RecordConfigurationFailure(ex);
				throw;
			}
		}

		internal static void ConfigureExactRunner(
			TestRunner runner,
			DiscoveredTestSelection selection)
		{
			var selectedClass = ReplicationWindowsDeviceTestClassFilter.SelectedClass;
			var selectedMethod = ReplicationWindowsDeviceTestClassFilter.SelectedMethod;
			if (!ReplicationWindowsDeviceTestClassFilter.UsesExactMethodSelector ||
				string.IsNullOrWhiteSpace(selectedClass) ||
				string.IsNullOrWhiteSpace(selectedMethod))
			{
				throw new InvalidOperationException("The exact Windows runner requires trusted class and method selectors.");
			}

			runner.RunAllTestsByDefault = false;
			runner.SkipClass(selectedClass, isExcluded: false);
			runner.SkipMethod(selection.DisplayName, isExcluded: false);
		}

		internal static DiscoveredTestSelection DiscoverExpectedTest(
			TestOptions options,
			ReplicationWindowsExactRunnerDiagnostics diagnostics)
		{
			var selectedClass = ReplicationWindowsDeviceTestClassFilter.SelectedClass;
			var selectedMethod = ReplicationWindowsDeviceTestClassFilter.SelectedMethod;
			if (!ReplicationWindowsDeviceTestClassFilter.UsesExactMethodSelector ||
				string.IsNullOrWhiteSpace(selectedClass) ||
				string.IsNullOrWhiteSpace(selectedMethod))
			{
				throw new InvalidOperationException("The exact Windows runner requires trusted class and method selectors.");
			}

			var assemblies = options.Assemblies
				.Distinct()
				.Where(assembly => assembly.GetType(selectedClass, throwOnError: false) is not null)
				.ToArray();
			if (assemblies.Length != 1)
				throw new InvalidOperationException("The exact Windows test class must resolve in exactly one test assembly.");

			var assemblyPath = assemblies[0].Location;
			if (string.IsNullOrWhiteSpace(assemblyPath))
				throw new InvalidOperationException("The exact Windows test assembly must have an on-disk location.");

			using var controller = new XunitFrontController(
				AppDomainSupport.Denied,
				assemblyPath,
				configFileName: null,
				shadowCopy: false);
			using var discoverySink = new TestDiscoverySink();
			var configuration = new TestAssemblyConfiguration
			{
				PreEnumerateTheories = false,
			};
			var discoveryOptions = TestFrameworkOptions.ForDiscovery(configuration);
			discoveryOptions.SetSynchronousMessageReporting(true);
			controller.Find(
				includeSourceInformation: false,
				discoverySink,
				discoveryOptions);
			if (!discoverySink.Finished.WaitOne(TimeSpan.FromMinutes(2)))
				throw new TimeoutException("Exact Windows xUnit discovery exceeded its fixed time bound.");

			var discoveredCases = discoverySink.TestCases ?? new List<ITestCase>();
			var classCases = discoveredCases
				.Where(testCase => string.Equals(
					testCase.TestMethod.TestClass.Class.Name,
					selectedClass,
					StringComparison.Ordinal))
				.ToArray();
			var methodCases = classCases
				.Where(testCase => string.Equals(
					testCase.TestMethod.Method.Name,
					selectedMethod,
					StringComparison.Ordinal))
				.ToArray();
			string? displayName = null;
			if (methodCases.Length == 1)
			{
				displayName = methodCases[0].DisplayName;
				if (string.IsNullOrWhiteSpace(displayName) ||
					displayName.Length > 1024 ||
					displayName.Any(char.IsControl))
				{
					throw new InvalidOperationException("The exact Windows test case has an invalid xUnit display name.");
				}

				var displayNameCaseCount = classCases.Count(testCase => string.Equals(
					testCase.DisplayName,
					displayName,
					StringComparison.Ordinal));
				if (displayNameCaseCount != 1)
					throw new InvalidOperationException("The exact Windows xUnit display name must be unique within its test class.");
			}
			diagnostics.RecordDiscovery(
				discoveredCases.Count,
				classCases.Length,
				methodCases);
			if (methodCases.Length != 1)
				throw new InvalidOperationException("The exact Windows test method must discover exactly one xUnit test case.");

			return new(
				selectedClass,
				selectedMethod,
				displayName!,
				discoveredCases.Count,
				classCases.Length,
				methodCases.Length);
		}
	}

	internal sealed record DiscoveredTestSelection(
		string ClassName,
		string MethodName,
		string DisplayName,
		int DiscoveredCaseCount,
		int ClassCaseCount,
		int MethodCaseCount);

	internal sealed class ReplicationWindowsExactRunnerDiagnostics
	{
		internal const string FileName = "maui-replication-windows-diagnostics.json";
		const int MaxExceptions = 16;
		const int MaxDiagnosticBytes = 64 * 1024;
		const int MaxExceptionMessageCharacters = 1024;

		readonly object _gate = new();
		readonly string? _path;
		readonly List<ExceptionDiagnostic> _exceptions = new();
		int _handlingException;
		string? _runnerType;
		int _expectedTypeCount;
		int _expectedMethodCount;
		string _stage = "startup";
		string? _expectedTestInspectionFailureType;
		string? _configurationFailureType;
		int _discoveredCaseCount;
		int _discoveredClassCaseCount;
		int _discoveredMethodCaseCount;
		int _discoveredDisplayNameLength;
		string? _discoveredDisplayNameSha256;
		bool _discoveredDisplayNameEndsWithMethod;
		bool _discoveredDisplayNameEqualsMethod;

		public ReplicationWindowsExactRunnerDiagnostics(TestOptions options)
		{
			_path = GetDiagnosticPath();
			AppDomain.CurrentDomain.FirstChanceException += OnFirstChanceException;
			try
			{
				InspectExpectedTest(options);
			}
			catch (Exception ex)
			{
				_expectedTestInspectionFailureType = ex.GetType().FullName;
			}
			WriteSnapshot();
		}

		public void BeginRunnerConfiguration()
		{
			Volatile.Write(ref _stage, "runner-configuration");
			WriteSnapshot();
		}

		public void RecordDiscovery(
			int discoveredCaseCount,
			int discoveredClassCaseCount,
			IReadOnlyList<ITestCase> discoveredMethodCases)
		{
			_discoveredCaseCount = discoveredCaseCount;
			_discoveredClassCaseCount = discoveredClassCaseCount;
			_discoveredMethodCaseCount = discoveredMethodCases.Count;
			if (discoveredMethodCases.Count == 1)
			{
				var displayName = discoveredMethodCases[0].DisplayName;
				var selectedMethod = ReplicationWindowsDeviceTestClassFilter.SelectedMethod;
				_discoveredDisplayNameLength = displayName.Length;
				_discoveredDisplayNameSha256 = Convert.ToHexString(
						SHA256.HashData(Encoding.UTF8.GetBytes(displayName)))
					.ToLowerInvariant();
				_discoveredDisplayNameEndsWithMethod = displayName.EndsWith(
					selectedMethod,
					StringComparison.Ordinal);
				_discoveredDisplayNameEqualsMethod = string.Equals(
					displayName,
					selectedMethod,
					StringComparison.Ordinal);
			}
			WriteSnapshot();
		}

		public void RecordRunner(TestRunner runner)
		{
			_runnerType = runner.GetType().FullName;
			Volatile.Write(ref _stage, "discovery-execution");
			WriteSnapshot();
		}

		public void RecordConfigurationFailure(Exception exception)
		{
			_configurationFailureType = exception.GetType().FullName;
			Volatile.Write(ref _stage, "runner-configuration-failed");
			WriteSnapshot();
		}

		void InspectExpectedTest(TestOptions options)
		{
			var selectedClass = ReplicationWindowsDeviceTestClassFilter.SelectedClass;
			var selectedMethod = ReplicationWindowsDeviceTestClassFilter.SelectedMethod;
			if (string.IsNullOrWhiteSpace(selectedClass) ||
				string.IsNullOrWhiteSpace(selectedMethod))
			{
				return;
			}

			foreach (var assembly in options.Assemblies.Distinct())
			{
				var type = assembly.GetType(selectedClass, throwOnError: false);
				if (type is null)
				{
					continue;
				}

				_expectedTypeCount++;
				_expectedMethodCount += type.GetMethods(
						BindingFlags.Instance |
						BindingFlags.Static |
						BindingFlags.Public |
						BindingFlags.NonPublic)
					.Count(method => string.Equals(
						method.Name,
						selectedMethod,
						StringComparison.Ordinal));
			}
		}

		void OnFirstChanceException(object? sender, FirstChanceExceptionEventArgs eventArgs)
		{
			if (Interlocked.Exchange(ref _handlingException, 1) != 0)
			{
				return;
			}

			try
			{
				lock (_gate)
				{
					if (_exceptions.Count >= MaxExceptions)
					{
						return;
					}

					var message = eventArgs.Exception.Message ?? string.Empty;
					var hashedCharacterLength = Math.Min(
						message.Length,
						MaxExceptionMessageCharacters);
					var messagePrefix = message.AsSpan(0, hashedCharacterLength);
					var hashedByteLength = Encoding.UTF8.GetByteCount(messagePrefix);
					var messageBytes = new byte[hashedByteLength];
					Encoding.UTF8.GetBytes(messagePrefix, messageBytes);
					_exceptions.Add(new(
						Volatile.Read(ref _stage),
						eventArgs.Exception.GetType().FullName ?? eventArgs.Exception.GetType().Name,
						eventArgs.Exception.HResult,
						Convert.ToHexString(SHA256.HashData(messageBytes))
							.ToLowerInvariant(),
						message.Length,
						hashedCharacterLength,
						hashedByteLength,
						message.Length > hashedCharacterLength));
					WriteSnapshotCore();
				}
			}
			catch (Exception diagnosticException)
			{
				Debug.WriteLine(
					"Exact Windows runner diagnostic failure: " +
					diagnosticException.GetType().FullName);
			}
			finally
			{
				Volatile.Write(ref _handlingException, 0);
			}
		}

		void WriteSnapshot()
		{
			if (Interlocked.Exchange(ref _handlingException, 1) != 0)
			{
				return;
			}

			try
			{
				lock (_gate)
				{
					WriteSnapshotCore();
				}
			}
			catch (Exception diagnosticException)
			{
				Debug.WriteLine(
					"Exact Windows runner diagnostic failure: " +
					diagnosticException.GetType().FullName);
			}
			finally
			{
				Volatile.Write(ref _handlingException, 0);
			}
		}

		void WriteSnapshotCore()
		{
			if (_path is null)
			{
				return;
			}

			var selectedClass = ReplicationWindowsDeviceTestClassFilter.SelectedClass;
			var selectedMethod = ReplicationWindowsDeviceTestClassFilter.SelectedMethod;
			var classFilters = ApplicationOptions.Current.ClassMethodFilters;
			var methodFilters = ApplicationOptions.Current.SingleMethodFilters;
			var document = new
			{
				schemaVersion = 1,
				authoritative = false,
				selectedClass,
				selectedMethod,
				applicationOptions = new
				{
					classFilterCount = classFilters.Count,
					methodFilterCount = methodFilters.Count,
					containsExpectedClass = classFilters.Contains(selectedClass),
					containsExpectedMethod = methodFilters.Contains(selectedMethod),
					unexpectedClassFilterCount = classFilters.Count(filter =>
						!string.Equals(filter, selectedClass, StringComparison.Ordinal)),
					unexpectedMethodFilterCount = methodFilters.Count(filter =>
						!string.Equals(filter, selectedMethod, StringComparison.Ordinal)),
				},
				dynamicCodeSupported = global::System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported,
				stage = Volatile.Read(ref _stage),
				runnerType = _runnerType,
				expectedTypeCount = _expectedTypeCount,
				expectedMethodCount = _expectedMethodCount,
				discoveredCaseCount = _discoveredCaseCount,
				discoveredClassCaseCount = _discoveredClassCaseCount,
				discoveredMethodCaseCount = _discoveredMethodCaseCount,
				discoveredDisplayNameLength = _discoveredDisplayNameLength,
				discoveredDisplayNameSha256 = _discoveredDisplayNameSha256,
				discoveredDisplayNameEndsWithMethod = _discoveredDisplayNameEndsWithMethod,
				discoveredDisplayNameEqualsMethod = _discoveredDisplayNameEqualsMethod,
				expectedTestInspectionFailureType = _expectedTestInspectionFailureType,
				configurationFailureType = _configurationFailureType,
				firstChanceExceptions = _exceptions,
			};
			var json = JsonSerializer.Serialize(
				document,
				new JsonSerializerOptions { WriteIndented = true });
			if (Encoding.UTF8.GetByteCount(json) > MaxDiagnosticBytes)
			{
				throw new InvalidOperationException("Exact Windows runner diagnostics exceeded the fixed size bound.");
			}

			File.WriteAllText(_path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
		}

		static string? GetDiagnosticPath()
		{
			var resultPath = HeadlessTestRunner.TestResultsFile;
			if (string.IsNullOrWhiteSpace(resultPath))
			{
				return null;
			}

			var directory = Path.GetDirectoryName(resultPath);
			return string.IsNullOrWhiteSpace(directory)
				? null
				: Path.Combine(directory, FileName);
		}

		sealed record ExceptionDiagnostic(
			string Stage,
			string Type,
			int HResult,
			string MessageSha256,
			int MessageLength,
			int HashedCharacterLength,
			int HashedByteLength,
			bool MessageTruncated);
	}
#endif
}
