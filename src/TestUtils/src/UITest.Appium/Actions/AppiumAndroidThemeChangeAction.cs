using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidThemeChangeAction : ICommandExecutionGroup
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
				ShellHelper.ExecuteShellCommand($"adb shell cmd uimode night no");
				return CommandResponse.SuccessEmptyResponse;
			}
			else if (commandName == SetDarkTheme)
			{
				ShellHelper.ExecuteShellCommand($"adb shell cmd uimode night yes");
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