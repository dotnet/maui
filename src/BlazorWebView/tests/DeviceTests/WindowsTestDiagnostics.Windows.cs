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

		internal static void Initialize()
		{
			var uploadRoot = Environment.GetEnvironmentVariable("HELIX_WORKITEM_UPLOAD_ROOT");
			if (string.IsNullOrEmpty(uploadRoot))
				return;

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
			WriteStage($"diagnostics-enabled path={path}");
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
