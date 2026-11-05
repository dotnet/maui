#nullable enable
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;
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
				var directory =
#if IOS || MACCATALYST
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
#else
					Environment.CurrentDirectory;
#endif
				var result = await runner.RunAsync(
					Path.Combine(directory, "TestResults"),
#if WINDOWS
					WindowsTestActivation.GetArguments(services.GetRequiredService<TestOptions>().TestAssembly));
#else
					Environment.GetCommandLineArgs()[1..]);
#endif
#if IOS || MACCATALYST
				WriteAppleResult(
					$"TEST_RESULT: passed={result.Passed}\n" +
					$"TEST_RESULT: failed={result.Failed}\n" +
					$"TEST_RESULT: skipped={result.Skipped}\n" +
					$"TEST_RESULT: resultsPath={result.ResultsPath}\n" +
					(result.ExitCode != 0 && result.Failed == 0 ? $"TEST_RESULT: error=The test runner exited with code {result.ExitCode}.\n" : string.Empty));
#endif
				Environment.Exit(result.ExitCode);
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
