using System;
using System.IO;
using System.Reflection.Metadata;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		private Exception _lastFirstChanceException;

		public App()
		{
			InitializeComponent();

			LogToTestResultsDir("test",
					$"test to make sure this works on CI");

			AppDomain.CurrentDomain.FirstChanceException += (_, e) => _lastFirstChanceException = e.Exception;
			UI.Xaml.Application.Current.UnhandledException += Current_UnhandledException;
		}

		private void Current_UnhandledException(object sender, UI.Xaml.UnhandledExceptionEventArgs e)
		{
			bool handled;
			Exception exception;

			try
			{
				var eventArgsType = e.GetType();
				handled = (bool)eventArgsType.GetProperty("Handled")!.GetValue(e)!;
				exception = (Exception)eventArgsType.GetProperty("Exception")!.GetValue(e)!;
			}
			catch (Exception ex)
			{
				LogToTestResultsDir("crash_log",
					$"Could not get exception details in WinUIUnhandledExceptionHandler: {ex.Message}\n" +
					$"{ex.StackTrace}");
				return;
			}

			if (exception.StackTrace is null)
			{
				exception = _lastFirstChanceException!;
			}

			if (exception != null)
			{
				LogToTestResultsDir("crash_log", $"WinUI crash: {exception.Message}\n{exception.StackTrace}");
				throw new Exception(
					$"WinUI crash: {exception.Message}\n" +
					$"{exception.StackTrace}");
			}
		}

		private void LogToTestResultsDir(string file, string output)
		{
			var testResultDir = $"{Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY")}/test-results";
			File.WriteAllText($"{testResultDir}/{file}.txt", output);
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
