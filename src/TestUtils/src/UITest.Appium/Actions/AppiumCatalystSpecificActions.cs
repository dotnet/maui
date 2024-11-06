using UITest.Core;

namespace UITest.Appium;

public class AppiumCatalystSpecificActions : ICommandExecutionGroup
{
	const string EnterFullScreenCommand = "enterFullScreen";
	const string ExitFullScreenCommand = "exitFullScreen";
	
	readonly AppiumApp _appiumApp;

	readonly List<string> _commands = new()
	{
		EnterFullScreenCommand,
		ExitFullScreenCommand,
	};

	public AppiumCatalystSpecificActions(AppiumApp appiumApp)
	{
		_appiumApp = appiumApp;
	}

	public bool IsCommandSupported(string commandName)
	{
		return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
	}

	public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
	{
		return commandName switch
		{
			EnterFullScreenCommand => EnterFullScreen(parameters),
			ExitFullScreenCommand => ExitFullScreen(parameters),
			_ => CommandResponse.FailedEmptyResponse,
		};
	}
	
	CommandResponse EnterFullScreen(IDictionary<string, object> parameters)
	{
		try
		{
			_appiumApp.Driver.Manage().Window.FullScreen();

			return CommandResponse.SuccessEmptyResponse;
		}
		catch
		{
			return CommandResponse.FailedEmptyResponse;
		}
	}
	
	CommandResponse ExitFullScreen(IDictionary<string, object> parameters)
	{
		try
		{
			string[] keys = ["XCUIKeyboardKeyEscape"];
			_appiumApp.Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
			{
				{ "keys", keys },
			});

			return CommandResponse.SuccessEmptyResponse;
		}
		catch
		{
			return CommandResponse.FailedEmptyResponse;
		}
	}
}