#nullable enable
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner.Pages;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	partial class MauiVisualRunnerApp : Application
	{
		readonly TestOptions _options;
		readonly ILogger _logger;
		
#if WINDOWS
		static string? _earlyLogFile;
		
		static void EarlyLog(string msg)
		{
			try
			{
				if (_earlyLogFile == null)
				{
					// Try to determine log file from command line args
					var cliArgs = Environment.GetCommandLineArgs();
					if (cliArgs.Length > 1)
					{
						var resultsDir = Path.GetDirectoryName(cliArgs[1]);
						if (!string.IsNullOrEmpty(resultsDir))
						{
							_earlyLogFile = Path.Combine(resultsDir, "maui-test-startup.log");
						}
					}
					
					// Fallback to temp directory
					if (_earlyLogFile == null)
					{
						_earlyLogFile = Path.Combine(Path.GetTempPath(), "maui-test-startup.log");
					}
				}
				
				File.AppendAllText(_earlyLogFile, $"{DateTime.Now:HH:mm:ss.fff} {msg}{Environment.NewLine}");
			}
			catch { }
		}
#endif

		public MauiVisualRunnerApp(TestOptions options, ILogger logger)
		{
#if WINDOWS
			EarlyLog("[MAUI-TEST] MauiVisualRunnerApp constructor starting...");
			EarlyLog($"[MAUI-TEST] Command line args: {string.Join(", ", Environment.GetCommandLineArgs())}");
#endif
			
			_options = options;
			_logger = logger;

			InitializeComponent();
			
#if WINDOWS
			EarlyLog("[MAUI-TEST] MauiVisualRunnerApp constructor completed, InitializeComponent done");
#endif
		}

		protected override Window CreateWindow(IActivationState? activationState)
		{
#if WINDOWS
			EarlyLog("[MAUI-TEST] CreateWindow called!");
#endif
			
			var hp = new HomePage();
			
#if WINDOWS
			EarlyLog("[MAUI-TEST] HomePage created");
#endif

			var nav = new TestNavigator(hp.Navigation);

			var runner = new DeviceRunner(_options.Assemblies, nav, _logger);

			var vm = new HomeViewModel(nav, runner);

			hp.BindingContext = vm;

			var navPage = new NavigationPage(hp);
			
#if WINDOWS
			EarlyLog("[MAUI-TEST] NavigationPage created, returning Window");
#endif

			return new Window(navPage);
		}
	}
}