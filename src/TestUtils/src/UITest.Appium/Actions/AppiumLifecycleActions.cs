using OpenQA.Selenium.Appium.Android;
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

		protected readonly AppiumApp _app;

		readonly List<string> _commands = new()
		{
			LaunchAppCommand,
			ForegroundAppCommand,
			BackgroundAppCommand,
			ResetAppCommand,
			CloseAppCommand,
			BackCommand
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
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse LaunchApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			if (_app.GetTestDevice() == TestDevice.Mac)
			{	
				_app.Driver.ExecuteScript("macos: activateApp", new Dictionary<string, object>
				{
					{ "bundleId", _app.GetAppId() },
				});
			}
			else if (_app.GetTestDevice() == TestDevice.Windows)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				_app.Driver.LaunchApp();
#pragma warning restore CS0618 // Type or member is obsolete
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
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			if (_app.AppState == ApplicationState.NotRunning)
				return CommandResponse.SuccessEmptyResponse;

			if (_app.GetTestDevice() == TestDevice.Mac)
			{
				_app.Driver.ExecuteScript("macos: terminateApp", new Dictionary<string, object>
				{
					{ "bundleId", _app.GetAppId() },
				});
			}
			else if (_app.GetTestDevice() == TestDevice.Windows)
			{
#pragma warning disable CS0618 // Type or member is obsolete
				_app.Driver.CloseApp();
#pragma warning restore CS0618 // Type or member is obsolete
			}
			else
				_app.Driver.TerminateApp(_app.GetAppId());

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse Back(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			// Navigate backwards in the history, if possible.
			_app.Driver.Navigate().Back();

			return CommandResponse.SuccessEmptyResponse;
		}
	}
}