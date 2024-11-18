using System;
using System.Diagnostics;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumIOSThemeChangeAction : ICommandExecutionGroup
	{
		const string SetLightTheme = "setLightTheme";
		const string SetDarkTheme = "setDarkTheme";

		protected readonly AppiumApp _app;

		public AppiumIOSThemeChangeAction(AppiumApp app)
		{
			_app = app;
		}

		readonly List<string> _commands = new()
		{
			SetLightTheme,
			SetDarkTheme
		};

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			if (commandName == SetLightTheme)
			{
				var args = new Dictionary<string, string> { { "style", "light" } };
				_app.Driver.ExecuteScript("mobile: setAppearance", args);
				return CommandResponse.SuccessEmptyResponse;
			}
			else if (commandName == SetDarkTheme)
			{
				var args = new Dictionary<string, string> { { "style", "dark" } };
				_app.Driver.ExecuteScript("mobile: setAppearance", args);
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

