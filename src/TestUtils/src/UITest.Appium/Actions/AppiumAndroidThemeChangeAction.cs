using System;
using System.Diagnostics;
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

		protected readonly AppiumApp _app;

		public AppiumAndroidThemeChangeAction(AppiumApp app)
		{
			_app = app;
		}

		public CommandResponse Execute(string commandName, IDictionary<string, object> parameters)
		{
			if (commandName == SetLightTheme)
			{
				var args = new Dictionary<string, string>
				{
					{ "mode", "night" },
					{ "value", "no" }
				};

				_app.Driver.ExecuteScript("mobile: setUiMode", args);
				return CommandResponse.SuccessEmptyResponse;
			}
			else if(commandName == SetDarkTheme)
			{
				var args = new Dictionary<string, string>
				{
					{ "mode", "night" },
					{ "value", "yes" }
				};

				_app.Driver.ExecuteScript("mobile: setUiMode", args);
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

