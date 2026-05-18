using System.Diagnostics;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
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
		public bool IsCommandSupported(string commandName) =>
			_commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
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
				_ => CommandResponse.FailedEmptyResponse
			};
		}
		#region Lifecycle
		CommandResponse LaunchApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;
			var appId = _app.GetAppId();
			// macOS
			if (_app.GetTestDevice() == TestDevice.Mac)
			{
				var args =
					_app.Config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs")
					?? new Dictionary<string, string>();
				if (args.ContainsKey("test") &&
					parameters.TryGetValue("testName", out var testNameObj) &&
					testNameObj is string testName &&
					!string.IsNullOrEmpty(testName))
				{
					args["test"] = testName;
				}
				_app.Driver.ExecuteScript(
					"macos: launchApp",
					new Dictionary<string, object>
					{
						{ "bundleId", appId },
						{ "environment", args }
					});
			}
			// Windows ✅ FIXED: must be a plain JS object
			else if (_app.Driver is WindowsDriver windowsDriver)
			{
				windowsDriver.ExecuteScript(
					"windows: launchApp",
					new Dictionary<string, object>
					{
						{ "app", appId }
					});
			}
			// iOS
			else if (_app.Driver is IOSDriver iosDriver)
			{
				var args =
					_app.Config.GetProperty<Dictionary<string, string>>("TestConfigurationArgs")
					?? new Dictionary<string, string>();
				iosDriver.ExecuteScript(
					"mobile: launchApp",
					new Dictionary<string, object>
					{
						{ "bundleId", appId },
						{ "environment", args }
					});
			}
			// Android / generic
			else
			{
				_app.Driver.ActivateApp(appId);
			}
			return CommandResponse.SuccessEmptyResponse;
		}
		CommandResponse ForegroundApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;
			if (_app.Driver is WindowsDriver wd)
			{
				if (wd.WindowHandles.Any())
					wd.SwitchTo().Window(wd.WindowHandles.First());
			}
			else
			{
				_app.Driver.ActivateApp(_app.GetAppId());
				Thread.Sleep(150); // animation settle
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
			CloseApp(parameters);
			return LaunchApp(parameters);
		}
		CommandResponse CloseApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;
			try
			{
				if (_app.AppState == ApplicationState.NotRunning)
					return CommandResponse.SuccessEmptyResponse;
			}
			catch
			{
				return ForceCloseApp(parameters);
			}
			try
			{
				var closeTask = Task.Run(() =>
				{
					if (_app.GetTestDevice() == TestDevice.Mac)
					{
						_app.Driver.ExecuteScript(
							"macos: terminateApp",
							new Dictionary<string, object>
							{
								{ "bundleId", _app.GetAppId() }
							});
					}
					else if (_app.Driver is WindowsDriver wd)
					{
						wd.CloseApp();
					}
					else
					{
						_app.Driver.TerminateApp(_app.GetAppId());
					}
				});
				if (closeTask.Wait(TimeSpan.FromSeconds(15)))
					return CommandResponse.SuccessEmptyResponse;
			}
			catch { }
			return ForceCloseApp(parameters);
		}
		#endregion
		#region Force Close
		CommandResponse ForceCloseApp(IDictionary<string, object> parameters)
		{
			try
			{
				var appId = _app.GetAppId();
				var device = _app.GetTestDevice();
				if (device == TestDevice.iOS)
				{
					var udid = _app.Config.GetProperty<string>("Udid");
					if (!string.IsNullOrEmpty(udid))
					{
						Process.Start(new ProcessStartInfo
						{
							FileName = "xcrun",
							ArgumentList = { "simctl", "terminate", udid, appId },
							UseShellExecute = false
						});
						return CommandResponse.SuccessEmptyResponse;
					}
				}
				else if (device == TestDevice.Android)
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "adb",
						ArgumentList = { "shell", "am", "force-stop", appId },
						UseShellExecute = false
					});
					return CommandResponse.SuccessEmptyResponse;
				}
				else if (device == TestDevice.Mac)
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "pkill",
						ArgumentList = { "-9", "-f", appId },
						UseShellExecute = false
					});
					return CommandResponse.SuccessEmptyResponse;
				}
				else if (device == TestDevice.Windows)
				{
					// NOTE: AppUserModelID != process name
					Process.Start(new ProcessStartInfo
					{
						FileName = "taskkill",
						ArgumentList = { "/F", "/IM", "*.exe" }, // safe fallback
						UseShellExecute = false
					});
					return CommandResponse.SuccessEmptyResponse;
				}
			}
			catch { }
			return CommandResponse.FailedEmptyResponse;
		}
		#endregion
		#region Navigation
		CommandResponse Back(IDictionary<string, object> parameters)
		{
			try
			{
				_app?.Driver?.Navigate().Back();
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
			_app.Driver.Navigate().Refresh();
			return CommandResponse.SuccessEmptyResponse;
		}
		#endregion
	}
}
