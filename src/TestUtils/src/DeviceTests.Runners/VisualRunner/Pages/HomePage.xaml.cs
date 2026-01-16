#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages
{
	partial class HomePage : ContentPage
	{
		static string? _logFile;
		
		static void LogToFile(string msg)
		{
#if WINDOWS
			try 
			{ 
				if (_logFile != null)
					File.AppendAllText(_logFile, $"{DateTime.Now:HH:mm:ss.fff} {msg}{Environment.NewLine}"); 
			}
			catch { }
#endif
		}
		
		public HomePage()
		{
			InitializeComponent();

			this.Loaded += HomePage_Loaded;
		}

		bool hasRunHeadless = false;

		private async void HomePage_Loaded(object? sender, EventArgs e)
		{
			string? testResultsFile = null;

#if WINDOWS
			var cliArgs = Environment.GetCommandLineArgs();
			
			// Set up log file based on test results file location
			if (cliArgs.Length > 1)
			{
				var resultsDir = Path.GetDirectoryName(cliArgs[1]);
				if (!string.IsNullOrEmpty(resultsDir))
				{
					_logFile = Path.Combine(resultsDir, "maui-test-startup.log");
				}
			}
			
			LogToFile("[MAUI-TEST] HomePage_Loaded fired!");
			LogToFile($"[MAUI-TEST] Command line args count: {cliArgs.Length}");
			for (int i = 0; i < cliArgs.Length; i++)
			{
				LogToFile($"[MAUI-TEST] Arg[{i}]: {cliArgs[i]}");
			}
			
			if (cliArgs.Length > 1)
			{
				testResultsFile = HeadlessTestRunner.TestResultsFile = ControlsHeadlessTestRunner.TestResultsFile = cliArgs.Skip(1).FirstOrDefault();
				ControlsHeadlessTestRunner.LoopCount = int.Parse(cliArgs.Skip(2).FirstOrDefault() ?? "-1");
				LogToFile($"[MAUI-TEST] TestResultsFile: {testResultsFile}");
				LogToFile($"[MAUI-TEST] LoopCount: {ControlsHeadlessTestRunner.LoopCount}");
			}
#endif

			LogToFile($"[MAUI-TEST] testResultsFile: {testResultsFile}, hasRunHeadless: {hasRunHeadless}");
			if (!string.IsNullOrEmpty(testResultsFile) && !hasRunHeadless)
			{
				hasRunHeadless = true;
				LogToFile("[MAUI-TEST] Starting headless runner...");

#if !WINDOWS
				var headlessRunner = Handler!.MauiContext!.Services.GetRequiredService<HeadlessTestRunner>();

				await headlessRunner.RunTestsAsync();
#else
				if (cliArgs.Length >= 3)
				{
					LogToFile("[MAUI-TEST] Using ControlsHeadlessTestRunner");
					var headlessRunner = Handler!.MauiContext!.Services.GetRequiredService<ControlsHeadlessTestRunner>();
					await headlessRunner.RunTestsAsync();
				}
				else
				{
					LogToFile("[MAUI-TEST] Using HeadlessTestRunner");
					var headlessRunner = Handler!.MauiContext!.Services.GetRequiredService<HeadlessTestRunner>();
					await headlessRunner.RunTestsAsync();
				}
#endif

				LogToFile("[MAUI-TEST] Headless runner completed, killing process");
				Process.GetCurrentProcess().Kill();
			}
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			assemblyList.SelectedItem = null;

			if (BindingContext is ViewModelBase vm)
				vm.OnAppearing();
		}
	}
}