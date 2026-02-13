using System.Diagnostics;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Windows;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumLifecycleActions : ICommandExecutionGroup
	{
		const string LaunchAppCommand = "launchApp";
		const string BackgroundAppCommand = "backgroundApp";
		const string ForegroundAppCommand = "foregroundApp";
		const string ResetAppCommand = "resetApp";
		const string CloseAppCommand = "closeApp";
		const string ForceCloseAppCommand = "forceCloseApp";
		const string BackCommand = "back";
		const string RefreshCommand = "refresh";

		protected readonly AppiumApp _app;

		readonly List<string> _commands = new()
		{
			LaunchAppCommand,
			ForegroundAppCommand,
			BackgroundAppCommand,
			ResetAppCommand,
			CloseAppCommand,
			ForceCloseAppCommand,
			BackCommand,
			RefreshCommand
		};

		public AppiumLifecycleActions(AppiumApp app)
		{
			_app = app;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			return commandName switch
			{
				LaunchAppCommand => LaunchApp(parameters),
				ForegroundAppCommand => ForegroundApp(parameters),
				BackgroundAppCommand => BackgroundApp(parameters),
				ResetAppCommand => ResetApp(parameters),
				CloseAppCommand => CloseApp(parameters),
				ForceCloseAppCommand => ForceCloseApp(parameters),
				BackCommand => Back(parameters),
				RefreshCommand => Refresh(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse LaunchApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			if (_app.GetTestDevice() == TestDevice.Mac)
			{		
				var args = _app.Config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs") ?? new Dictionary<string, string>();

				if (args.ContainsKey("test") && parameters.ContainsKey("testName") && parameters["testName"] is string testName && !string.IsNullOrEmpty(testName))
				{
					args["test"] = testName;
				}

				_app.Driver.ExecuteScript("macos: launchApp", new Dictionary<string, object>
				{
					{ "bundleId", _app.GetAppId() },
					{ "environment", args},
				});
			}
			else if (_app.Driver is WindowsDriver windowsDriver)
			{
				// Appium driver removed the LaunchApp method in 5.0.0, so we need to use the executeScript method instead
				// Currently the appium-windows-driver reports the following commands as compatible:
				//   startRecordingScreen,stopRecordingScreen,launchApp,closeApp,deleteFile,deleteFolder,
				//   click,scroll,clickAndDrag,hover,keys,setClipboard,getClipboard
				windowsDriver.ExecuteScript("windows: launchApp", [_app.GetAppId()]);
			}
			else if (_app.Driver is IOSDriver iOSDriver)
			{
				var args = _app.Config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs") ?? new Dictionary<string, string>();
				iOSDriver.ExecuteScript("mobile: launchApp", new Dictionary<string, object>
				{
					{ "bundleId", _app.GetAppId() },
					{ "environment", args },
				});
			}
			else
			{
				_app.Driver.ActivateApp(_app.GetAppId());
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse ForegroundApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			if (_app.Driver is WindowsDriver wd)
			{
				wd.SwitchTo().Window(wd.WindowHandles.First());
			}
			else
			{
				_app.Driver.ActivateApp(_app.GetAppId());
				// Give it time for the animation to settle, otherwise there's a risk
				// of picking wrong elements coordinates and `Tap`s will fail silently.
				Thread.Sleep(100);
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse BackgroundApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			_app.Driver.BackgroundApp();
			if (_app.GetTestDevice() == TestDevice.Android)
				Thread.Sleep(500);

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse ResetApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			CloseApp(parameters);
			LaunchApp(parameters);

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse CloseApp(IDictionary<string, object> parameters)
		{
			try
			{
				if (_app is null || _app.Driver is null)
					return CommandResponse.FailedEmptyResponse;

				if (_app.AppState == ApplicationState.NotRunning)
					return CommandResponse.SuccessEmptyResponse;
			}
			catch (Exception)
			{
				// App might be too locked up to even report state — try force close
				return ForceCloseApp(parameters);
			}

			// Try normal Appium termination with a timeout to prevent hanging when app is unresponsive
			try
			{
				var closeTask = Task.Run(() =>
				{
					if (_app.GetTestDevice() == TestDevice.Mac)
					{
						_app.Driver.ExecuteScript("macos: terminateApp", new Dictionary<string, object>
						{
							{ "bundleId", _app.GetAppId() },
						});
					}
					else if (_app.Driver is WindowsDriver windowsDriver)
					{
						windowsDriver.CloseApp();
					}
					else
					{
						_app.Driver.TerminateApp(_app.GetAppId());
					}
				});

				if (closeTask.Wait(TimeSpan.FromSeconds(15)))
				{
					return CommandResponse.SuccessEmptyResponse;
				}

				// Normal close timed out — app is likely unresponsive, use force close
				Debug.WriteLine(">>>>> CloseApp timed out after 15s, falling back to ForceCloseApp");
			}
			catch (Exception ex)
			{
				// Normal close failed — fall through to force close
				Debug.WriteLine($">>>>> CloseApp threw an exception, falling back to ForceCloseApp: {ex.Message}");
			}

			return ForceCloseApp(parameters);
		}

		/// <summary>
		/// Force-terminates the app using platform-specific OS commands.
		/// This bypasses Appium/WDA which may be stuck waiting for the app to become idle.
		/// Use when the app is unresponsive (e.g., stuck in an infinite layout loop).
		/// </summary>
		CommandResponse ForceCloseApp(IDictionary<string, object> parameters)
		{
			try
			{
				var appId = _app.GetAppId();
				var testDevice = _app.GetTestDevice();

				if (testDevice == TestDevice.iOS)
				{
					var udid = _app.Config.GetProperty<string>("Udid");
					if (!string.IsNullOrEmpty(udid))
					{
						Debug.WriteLine($">>>>> ForceCloseApp: xcrun simctl terminate {udid} {appId}");
						using var process = Process.Start(new ProcessStartInfo
						{
							FileName = "xcrun",
							ArgumentList = { "simctl", "terminate", udid, appId },
							RedirectStandardOutput = true,
							RedirectStandardError = true,
							UseShellExecute = false,
						});
						if (process is null || !process.WaitForExit(10000) || process.ExitCode != 0)
						{
							Debug.WriteLine($">>>>> ForceCloseApp: xcrun simctl terminate failed (process={process is not null}, exitCode={process?.ExitCode})");
							return CommandResponse.FailedEmptyResponse;
						}
						return CommandResponse.SuccessEmptyResponse;
					}
				}
				else if (testDevice == TestDevice.Android)
				{
					Debug.WriteLine($">>>>> ForceCloseApp: adb shell am force-stop {appId}");
					using var process = Process.Start(new ProcessStartInfo
					{
						FileName = "adb",
						ArgumentList = { "shell", "am", "force-stop", appId },
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
					});
					if (process is null || !process.WaitForExit(10000) || process.ExitCode != 0)
					{
						Debug.WriteLine($">>>>> ForceCloseApp: adb force-stop failed (process={process is not null}, exitCode={process?.ExitCode})");
						return CommandResponse.FailedEmptyResponse;
					}
					return CommandResponse.SuccessEmptyResponse;
				}
				else if (testDevice == TestDevice.Mac)
				{
					Debug.WriteLine($">>>>> ForceCloseApp: macOS kill for {appId}");
					using var process = Process.Start(new ProcessStartInfo
					{
						FileName = "osascript",
						ArgumentList = { "-e", $"tell application id \"{appId}\" to quit" },
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
					});
					if (process is null || !process.WaitForExit(10000) || process.ExitCode != 0)
					{
						Debug.WriteLine($">>>>> ForceCloseApp: osascript quit failed (process={process is not null}, exitCode={process?.ExitCode})");
						return CommandResponse.FailedEmptyResponse;
					}
					return CommandResponse.SuccessEmptyResponse;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($">>>>> ForceCloseApp failed: {ex.Message}");
			}

			return CommandResponse.FailedEmptyResponse;
		}

		CommandResponse Back(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			try
			{
				// Navigate backwards in the history, if possible.
				_app.Driver.Navigate().Back();

				return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		CommandResponse Refresh(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			// Refresh the current page.
			_app.Driver.Navigate().Refresh();

			return CommandResponse.SuccessEmptyResponse;
		}
	}
}