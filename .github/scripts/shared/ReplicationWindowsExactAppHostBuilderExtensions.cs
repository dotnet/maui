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

		public ReplicationWindowsExactHeadlessTestRunner(
			HeadlessRunnerOptions runnerOptions,
			TestOptions options)
			: base(runnerOptions, options)
		{
			_diagnostics = new(options);
		}

		protected override TestRunner GetTestRunner(LogWriter logWriter)
		{
			_diagnostics.BeginRunnerConfiguration();
			try
			{
				var runner = base.GetTestRunner(logWriter);
				ConfigureExactRunner(runner);
				_diagnostics.RecordRunner(runner);
				return runner;
			}
			catch (Exception ex)
			{
				_diagnostics.RecordConfigurationFailure(ex);
				throw;
			}
		}

		internal static void ConfigureExactRunner(TestRunner runner)
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
			runner.SkipMethod(selectedClass + "." + selectedMethod, isExcluded: false);
		}
	}

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
			var expectedFullyQualifiedMethod = selectedClass + "." + selectedMethod;
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
					containsExpectedMethod = methodFilters.Contains(expectedFullyQualifiedMethod),
					unexpectedClassFilterCount = classFilters.Count(filter =>
						!string.Equals(filter, selectedClass, StringComparison.Ordinal)),
					unexpectedMethodFilterCount = methodFilters.Count(filter =>
						!string.Equals(filter, expectedFullyQualifiedMethod, StringComparison.Ordinal)),
				},
				dynamicCodeSupported = global::System.Runtime.CompilerServices.RuntimeFeature.IsDynamicCodeSupported,
				stage = Volatile.Read(ref _stage),
				runnerType = _runnerType,
				expectedTypeCount = _expectedTypeCount,
				expectedMethodCount = _expectedMethodCount,
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
