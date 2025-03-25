using OpenQA.Selenium.Appium.iOS;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumIOSSpecificActions : ICommandExecutionGroup
	{
		const string ShakeDeviceCommand = "shake";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ShakeDeviceCommand,
		};

		public AppiumIOSSpecificActions(AppiumApp appiumApp)
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
				ShakeDeviceCommand => ShakeDevice(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse ShakeDevice(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is IOSDriver iOSDriver)
			{
				iOSDriver.ShakeDevice();

				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}
	}
}