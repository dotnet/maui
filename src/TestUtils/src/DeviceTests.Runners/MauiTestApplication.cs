#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
#if WINDOWS
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Maui.Dispatching;
#endif
#if IOS || MACCATALYST
using Microsoft.Win32.SafeHandles;
#endif

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners;

class MauiTestApplication : Application
{
	protected override Window CreateWindow(IActivationState? activationState)
	{
		var page = new ContentPage();
#if WINDOWS || IOS || MACCATALYST
		int started = 0;
		page.Loaded += async (_, _) =>
		{
			if (Interlocked.Exchange(ref started, 1) != 0)
				return;

			try
			{
				var services = IPlatformApplication.Current!.Services;
				var runner = services.GetRequiredService<HeadlessTestRunner>();
#if WINDOWS
				WindowsUiTestFramework.ConfigureDispatcher(action => page.Dispatcher.DispatchAsync(action));
				var errorDirectory = services.GetRequiredService<TestOptions>().TestAssembly
					.GetCustomAttributes<AssemblyMetadataAttribute>()
					.SingleOrDefault(attribute => attribute.Key == "MauiTestResultsDirectory")?.Value;
				if (string.IsNullOrEmpty(errorDirectory))
					errorDirectory = Path.Combine(Environment.CurrentDirectory, "TestResults");
				Directory.CreateDirectory(errorDirectory);
				var fatalLog = Path.Combine(errorDirectory, $"native-unhandled-{Environment.ProcessId}.log");
				global::Microsoft.UI.Xaml.Application.Current.UnhandledException += (_, args) =>
				{
					File.AppendAllText(fatalLog, args.Exception + Environment.NewLine);
					using var output = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
					output.WriteLine(args.Exception);
					WriteWindowsProcessDump(errorDirectory);
				};
				AppDomain.CurrentDomain.UnhandledException += (_, args) =>
				{
					File.AppendAllText(fatalLog, args.ExceptionObject + Environment.NewLine);
					WriteWindowsProcessDump(errorDirectory);
				};
#endif
				var directory =
#if IOS || MACCATALYST
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
					Environment.CurrentDirectory;
#endif
#if WINDOWS
				var exitCode = await runner.RunWithControllerAsync(
					Path.Combine(directory, "TestResults"),
					WindowsTestActivation.GetArguments(services.GetRequiredService<TestOptions>().TestAssembly));
				Environment.Exit(exitCode);
#else
				var result = await runner.RunAsync(
					Path.Combine(directory, "TestResults"),
					Environment.GetCommandLineArgs()[1..]);
#if IOS || MACCATALYST
				WriteAppleResult(
					$"TEST_RESULT: passed={result.Passed}\n" +
					$"TEST_RESULT: failed={result.Failed}\n" +
					$"TEST_RESULT: skipped={result.Skipped}\n" +
					$"TEST_RESULT: resultsPath={result.ResultsPath}\n" +
					(result.ExitCode != 0 && result.Failed == 0 ? $"TEST_RESULT: error=The test runner exited with code {result.ExitCode}.\n" : string.Empty));
#endif
				Environment.Exit(result.ExitCode);
#endif
			}
			catch (Exception exception)
			{
				Console.Error.WriteLine(exception);
#if IOS || MACCATALYST
				WriteAppleResult($"TEST_RESULT: error={exception}\n");
#endif
				Environment.Exit(1);
			}
		};
#endif
		return new Window(page);
	}

#if WINDOWS
	const uint GdiObjectSnapshotThreshold = 8000;
	static int _windowsDumpStarted;
	static string? _windowsResourceDumpDirectory;

	internal static void ConfigureWindowsResourceDiagnostics(string directory)
	{
		Volatile.Write(ref _windowsResourceDumpDirectory, directory);
		Console.WriteLine($"Native resource diagnostics: process={Environment.ProcessId}, GDI snapshot threshold={GdiObjectSnapshotThreshold}.");
	}

	/// <summary>Requests an owned-process snapshot for a test failure when native diagnostics are enabled.</summary>
	public static void CaptureWindowsTestFailureDump(string testName)
	{
		var directory = Volatile.Read(ref _windowsResourceDumpDirectory);
		if (directory is null || Volatile.Read(ref _windowsDumpStarted) != 0)
			return;

		Console.Error.WriteLine($"Native test failure: {testName}. Capturing an owned-process snapshot.");
		WriteWindowsProcessDump(directory);
	}

	internal static void CaptureWindowsResourcePressureDump()
	{
		var directory = Volatile.Read(ref _windowsResourceDumpDirectory);
		if (directory is null || Volatile.Read(ref _windowsDumpStarted) != 0)
			return;

		try
		{
			using var process = Process.GetCurrentProcess();
			var gdiObjects = GetGuiResources(process.Handle, 0);
			var error = Marshal.GetLastPInvokeError();
			if (gdiObjects == 0 && error != 0)
				throw new Win32Exception(error);
			if (gdiObjects >= GdiObjectSnapshotThreshold)
			{
				Console.Error.WriteLine($"Native resource pressure: process={process.Id}, GDI objects={gdiObjects}. Capturing an owned-process snapshot.");
				WriteWindowsProcessDump(directory);
			}
		}
		catch (Win32Exception exception)
		{
			Volatile.Write(ref _windowsResourceDumpDirectory, null);
			Console.Error.WriteLine($"Could not inspect native GDI resource usage: {exception}");
		}
	}

	static void WriteWindowsProcessDump(string directory)
	{
		if (Interlocked.Exchange(ref _windowsDumpStarted, 1) != 0)
			return;

		var path = Path.Combine(directory, $"native-process-{Environment.ProcessId}.dmp");
		try
		{
			// DbgHelp must run off the faulting UI thread; never mark the exception handled.
			Task.Factory.StartNew(() =>
			{
				var pending = path + ".pending";
				using (var process = Process.GetCurrentProcess())
				using (var dump = new FileStream(pending, FileMode.CreateNew, FileAccess.Write, FileShare.None))
				{
					const MiniDumpType flags = MiniDumpType.WithFullMemory | MiniDumpType.WithHandleData |
						MiniDumpType.WithFullMemoryInfo | MiniDumpType.WithThreadInfo;
					if (!MiniDumpWriteDump(process.Handle, process.Id, dump.SafeFileHandle, flags,
						IntPtr.Zero, IntPtr.Zero, IntPtr.Zero))
					{
						var error = Marshal.GetLastPInvokeError();
						throw new Win32Exception(error, $"MiniDumpWriteDump failed with 0x{error:X8}.");
					}
				}
				File.Move(pending, path);
			}, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)
				.WaitAsync(TimeSpan.FromSeconds(30)).GetAwaiter().GetResult();
			Console.Error.WriteLine($"Native process dump: {path}");
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or Win32Exception or TimeoutException)
		{
			Console.Error.WriteLine($"Could not capture native process dump '{path}': {exception}");
		}
	}

	[Flags]
	enum MiniDumpType : uint
	{
		WithFullMemory = 0x00000002,
		WithHandleData = 0x00000004,
		WithFullMemoryInfo = 0x00000800,
		WithThreadInfo = 0x00001000
	}

	[DllImport("dbghelp.dll", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	[return: MarshalAs(UnmanagedType.Bool)]
	static extern bool MiniDumpWriteDump(IntPtr process, int processId,
		Microsoft.Win32.SafeHandles.SafeFileHandle file, MiniDumpType dumpType,
		IntPtr exceptionParameters, IntPtr userStreams, IntPtr callback);

	[DllImport("user32.dll", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	static extern uint GetGuiResources(IntPtr process, uint flags);
#endif

#if IOS || MACCATALYST
	static void WriteAppleResult(string result)
	{
		// Avoid the NSLog prefix so the native proxy can read its result records.
		using var output = new StreamWriter(new FileStream(
			new SafeFileHandle((nint)1, ownsHandle: false), FileAccess.Write));
		output.Write(result);
	}
#endif
}
