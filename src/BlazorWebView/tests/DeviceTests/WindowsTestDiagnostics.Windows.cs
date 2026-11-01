#nullable enable
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Microsoft.Maui.MauiBlazorWebView.DeviceTests
{
	internal static class WindowsTestDiagnostics
	{
		static bool _enabled;
		static int _operationId;

		internal static void Initialize(string? resultsFile = null)
		{
			if (_enabled)
				return;

			var uploadRoot = Environment.GetEnvironmentVariable("HELIX_WORKITEM_UPLOAD_ROOT");
			var activation = "environment";
			if (string.IsNullOrEmpty(uploadRoot))
			{
				if (resultsFile is null)
				{
					var arguments = Environment.GetCommandLineArgs();
					resultsFile = arguments.Length > 1 ? arguments[1] : null;
				}
				if (string.IsNullOrEmpty(resultsFile) || !Path.IsPathFullyQualified(resultsFile))
					return;

				var marker = resultsFile + ".blazor-diagnostics";
				if (!File.Exists(marker))
					return;
				if (File.ReadAllText(marker) != "windows-blazor-diagnostics-v1")
					throw new InvalidDataException($"Invalid Windows Blazor diagnostic marker: {marker}");

				uploadRoot = Path.GetDirectoryName(resultsFile)
					?? throw new InvalidDataException($"Diagnostic results path has no directory: {resultsFile}");
				activation = "results-marker";
			}

			var path = Path.Combine(uploadRoot, $"blazor-progress-{Environment.ProcessId}.log");
			var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
			var writer = TextWriter.Synchronized(new StreamWriter(stream, new UTF8Encoding(false))
			{
				AutoFlush = true
			});
			Console.SetOut(writer);
			Console.SetError(writer);
			Environment.SetEnvironmentVariable("XHARNESS_LOG_TEST_START", "true");
			Environment.SetEnvironmentVariable("XHARNESS_LOG_THREAD_ID", "true");
			Environment.SetEnvironmentVariable("MAUI_BLAZOR_TEST_DIAGNOSTICS", "1");
			_enabled = true;
			WriteStage($"diagnostics-enabled activation={activation} path={path}");
			AppDomain.CurrentDomain.UnhandledException += (_, args) =>
				WriteStage($"unhandled-exception terminating={args.IsTerminating} exception={args.ExceptionObject}");
		}

		internal static void WriteStage(string stage)
		{
			if (_enabled)
				Console.WriteLine($"{DateTime.UtcNow:O} pid={Environment.ProcessId} tid={Environment.CurrentManagedThreadId} stage={stage}");
		}

		internal static int BeginOperation(string name)
		{
			var id = Interlocked.Increment(ref _operationId);
			WriteStage($"{name}.start operation={id}");
			return id;
		}
	}
}
