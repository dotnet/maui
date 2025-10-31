using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumCatalystVirtualKeyboardActions : AppiumVirtualKeyboardActions
	{
		readonly AppiumApp _appiumApp;

		public AppiumCatalystVirtualKeyboardActions(AppiumApp appiumApp)
			: base(appiumApp)
		{
			_appiumApp = appiumApp;
		}

		protected override CommandResponse PressEnter(IDictionary<string, object> parameters)
		{
			try
			{
				// https://developer.apple.com/documentation/xctest/xcuikeyboardkey?language=objc
				string[] keys = ["XCUIKeyboardKeyEnter"]; // Enter Key

				_appiumApp.Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
 				{
 					{ "keys", keys },
 				});
			}
			catch (InvalidElementStateException)
			{
				return CommandResponse.FailedEmptyResponse;
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		protected override CommandResponse SendTabKey(IDictionary<string, object> parameters)
		{
			try
			{
				// https://developer.apple.com/documentation/xctest/xcuikeyboardkey?language=objc
				string[] keys = ["XCUIKeyboardKeyTab"]; // Tab Key

				_appiumApp.Driver.ExecuteScript("macos: keys", new Dictionary<string, object>
 				{
 					{ "keys", keys },
 				});
			}
			catch (InvalidElementStateException)
			{
				return CommandResponse.FailedEmptyResponse;
			}

			return CommandResponse.SuccessEmptyResponse;
		}
	}
}