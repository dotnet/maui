using UITest.Core;

namespace UITest.Appium;

public class AppiumCatalystSpecificActions : ICommandExecutionGroup
{
	const string EnterFullScreenCommand = "enterFullScreen";
	const string ExitFullScreenCommand = "exitFullScreen";
	const string ToggleSystemAnimationsCommand = "toggleSystemAnimations";

	readonly AppiumApp _appiumApp;

	readonly List<string> _commands = new()
	{
		EnterFullScreenCommand,
		ExitFullScreenCommand,
		ToggleSystemAnimationsCommand,
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
			ToggleSystemAnimationsCommand => ToggleSystemAnimations(parameters),
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

	CommandResponse ToggleSystemAnimations(IDictionary<string, object> parameters)
	{
		try
		{
			bool enableSystemAnimations = (bool)parameters["enableSystemAnimations"];

			if (enableSystemAnimations)
			{
				// Disable Window Animations.
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSAutomaticWindowAnimationsEnabled -bool false");

				// Increase the speed of OSX dialogs boxes.
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSWindowResizeTime .1");

				return CommandResponse.SuccessEmptyResponse;
			}
			else
			{
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSAutomaticWindowAnimationsEnabled -bool true");
				ShellHelper.ExecuteShellCommand($"defaults write NSGlobalDomain NSWindowResizeTime .5");

				return CommandResponse.SuccessEmptyResponse;
			}
		}
		catch
		{
			return CommandResponse.FailedEmptyResponse;
		}
	}
}