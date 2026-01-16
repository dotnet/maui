using System;
using System.IO;
using System.Linq;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests.WinUI
{
	public partial class App : MauiWinUIApplication
	{
		static readonly string LogFile;
		
		static App()
		{
			// Earliest possible logging - in static constructor
			try
			{
				var cliArgs = Environment.GetCommandLineArgs();
				if (cliArgs.Length > 1)
				{
					var resultsDir = Path.GetDirectoryName(cliArgs[1]);
					if (!string.IsNullOrEmpty(resultsDir))
					{
						LogFile = Path.Combine(resultsDir, "maui-test-startup.log");
					}
				}
				
				LogFile ??= Path.Combine(Path.GetTempPath(), "maui-test-startup.log");
				
				File.AppendAllText(LogFile, $"{DateTime.Now:HH:mm:ss.fff} [MAUI-TEST] Static constructor - App class loading{Environment.NewLine}");
				File.AppendAllText(LogFile, $"{DateTime.Now:HH:mm:ss.fff} [MAUI-TEST] Args: {string.Join(", ", cliArgs)}{Environment.NewLine}");
			}
			catch { }
		}
		
		static void Log(string msg)
		{
			try { File.AppendAllText(LogFile, $"{DateTime.Now:HH:mm:ss.fff} {msg}{Environment.NewLine}"); }
			catch { }
		}
		
		public App()
		{
			Log("[MAUI-TEST] App constructor starting...");
			InitializeComponent();
			Log("[MAUI-TEST] App constructor - InitializeComponent done");
		}

		protected override MauiApp CreateMauiApp()
		{
			Log("[MAUI-TEST] CreateMauiApp called");
			try
			{
				var app = MauiProgram.CreateMauiApp();
				Log("[MAUI-TEST] CreateMauiApp completed successfully");
				return app;
			}
			catch (Exception ex)
			{
				Log($"[MAUI-TEST] CreateMauiApp EXCEPTION: {ex}");
				throw;
			}
		}
	}
}
