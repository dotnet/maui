using UITest.Core;

namespace UITest.Appium
{
	public class AppiumCatalystThemeChangeAction : ICommandExecutionGroup
	{
		const string SetLightTheme = "setLightTheme";
		const string SetDarkTheme = "setDarkTheme";

		readonly List<string> _commands = new()
		{
			SetLightTheme,
			SetDarkTheme
		};

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			if (commandName == SetLightTheme)
			{
				ShellHelper.ExecuteShellCommand($"osascript -e 'tell app \"System Events\" to tell appearance preferences to set dark mode to false'");
				return CommandResponse.SuccessEmptyResponse;
			}
			else if (commandName == SetDarkTheme)
			{
				ShellHelper.ExecuteShellCommand($"osascript -e 'tell app \"System Events\" to tell appearance preferences to set dark mode to true'");
				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		public bool IsCommandSupported(string commandName)
		{
			return _commands.Contains(commandName, StringComparer.OrdinalIgnoreCase);
		}
	}
}