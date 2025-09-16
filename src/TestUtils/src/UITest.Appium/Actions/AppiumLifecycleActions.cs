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
				// TODO: Pass in logger so we can log these exceptions

				// Occasionally the app seems to get so locked up it can't 
				// even report back the appstate. In that case, we'll just
				// try to trigger a reset.
			}

			try
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
					// This is still here for now, but it looks like it will get removed just like
					// LaunchApp was in 5.0.0, in which case we may need to use:
					// windowsDriver.ExecuteScript("windows: closeApp", [_app.GetAppId()]);
					windowsDriver.CloseApp();
				}
				else
				{
					_app.Driver.TerminateApp(_app.GetAppId());
				}
			}
			catch (Exception)
			{
				// TODO Pass in logger so we can log these exceptions

				// Occasionally the app seems like it's already closed before we get here
				// and then this throws an exception.
				return CommandResponse.FailedEmptyResponse;
			}

			return CommandResponse.SuccessEmptyResponse;
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