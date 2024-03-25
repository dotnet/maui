using System;
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
				Console.WriteLine(
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
				Console.WriteLine(
					$"WinUI crash: {exception.Message}\n" +
					$"{exception.StackTrace}");
			}
		}

		protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
	}
}
