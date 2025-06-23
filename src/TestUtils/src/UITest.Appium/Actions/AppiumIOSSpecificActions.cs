using OpenQA.Selenium;
using OpenQA.Selenium.Appium.iOS;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumIOSSpecificActions : ICommandExecutionGroup
	{
		const string ShakeDeviceCommand = "shake";
		const string InteractivePopGesture = "interactivePopGesture";

		readonly AppiumApp _appiumApp;

		readonly List<string> _commands = new()
		{
			ShakeDeviceCommand,
			InteractivePopGesture
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
				InteractivePopGesture => PerformInteractivePopGesture(parameters),
				_ => CommandResponse.FailedEmptyResponse,
			};
		}

		CommandResponse PerformInteractivePopGesture(IDictionary<string, object> parameters)
		{
			if (_appiumApp.Driver is IOSDriver iOSDriver)
			{
				var sceenSize = iOSDriver.Manage().Window.Size;
				var args = new Dictionary<string, object>
				{
					{ "fromX", 0 },
					{ "toX", sceenSize.Width * 0.8 },
					{ "fromY", sceenSize.Height / 2 },
					{ "toY", sceenSize.Height / 2 },
					{ "duration", 1 }
				};

				iOSDriver.ExecuteScript("mobile: dragFromToForDuration", args);
				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
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