using OpenQA.Selenium.Appium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumCatalystMouseActions : ICommandExecutionGroup
	{
		const string DoubleClickCommand = "doubleClick";

		readonly List<string> _commands = new()
		{
			DoubleClickCommand,
		};
		readonly AppiumApp _appiumApp;

		public AppiumCatalystMouseActions(AppiumApp appiumApp)
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
				DoubleClickCommand => DoubleClick(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse DoubleClick(IDictionary<string, object> parameters)
		{
			var element = GetAppiumElement(parameters["element"]);

			if (element != null)
			{
				_appiumApp.Driver.ExecuteScript("macos: doubleClick", new Dictionary<string, object>
				{
					{ "elementId", element.Id },
				});
			}
			return CommandResponse.SuccessEmptyResponse;
		}

		static AppiumElement? GetAppiumElement(object element)
		{
			if (element is AppiumElement appiumElement)
			{
				return appiumElement;
			}
			else if (element is AppiumDriverElement driverElement)
			{
				return driverElement.AppiumElement;
			}

			return null;
		}
	}
}