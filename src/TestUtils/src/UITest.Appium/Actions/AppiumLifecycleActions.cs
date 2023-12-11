using UITest.Core;

namespace UITest.Appium
{
	public class AppiumLifecycleActions : ICommandExecutionGroup
	{
		const string LaunchAppCommand = "launchApp";
		const string BackgroundAppCommand = "backgroundApp";
		const string ResetAppCommand = "resetApp";
		const string CloseAppCommand = "closeApp";

		protected readonly AppiumApp _app;

		readonly List<string> _commands = new()
		{
			LaunchAppCommand,
			BackgroundAppCommand,
			ResetAppCommand,
			CloseAppCommand,
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
				BackgroundAppCommand => BackgroundApp(parameters),
				ResetAppCommand => ResetApp(parameters),
				CloseAppCommand => CloseApp(parameters),
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

			_app.Driver.ResetApp();

			return CommandResponse.SuccessEmptyResponse;
		}

		CommandResponse CloseApp(IDictionary<string, object> parameters)
		{
			if (_app?.Driver is null)
				return CommandResponse.FailedEmptyResponse;

			_app.Driver.CloseApp();

			return CommandResponse.SuccessEmptyResponse;
		}
	}
}