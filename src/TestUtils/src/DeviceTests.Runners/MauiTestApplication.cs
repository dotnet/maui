#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
#if WINDOWS
using System.Linq;
using System.Reflection;
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
				};
				AppDomain.CurrentDomain.UnhandledException += (_, args) =>
					File.AppendAllText(fatalLog, args.ExceptionObject + Environment.NewLine);
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
