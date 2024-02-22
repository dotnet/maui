using OpenQA.Selenium.Appium.Android;
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

			_app.Driver.LaunchApp();

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse ForegroundApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			_app.Driver.ActivateApp(_app.GetAppId());

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse BackgroundApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			_app.Driver.BackgroundApp();

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse ResetApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			// Terminate App not supported on Mac
			if (_app.GetTestDevice() == TestDevice.Mac)
			{
				_app.Driver.ResetApp();
			}
			else if (_app.GetTestDevice() == TestDevice.Windows)
			{
				CloseApp(parameters);
				_app.Driver.LaunchApp();
			}
			else
			{
				_app.Driver.TerminateApp(_app.GetAppId());

				if (_app.GetTestDevice() == TestDevice.iOS)
					_app.Driver.ActivateApp(_app.GetAppId());
				else
					_app.Driver.LaunchApp();
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse CloseApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			_app.Driver.CloseApp();

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