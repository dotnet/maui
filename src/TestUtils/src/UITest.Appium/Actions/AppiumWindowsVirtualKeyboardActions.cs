using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Windows;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumWindowsVirtualKeyboardActions : AppiumVirtualKeyboardActions
	{
		readonly AppiumApp _appiumApp;

		public AppiumWindowsVirtualKeyboardActions(AppiumApp app)
			: base(app)
		{
			_appiumApp = app;
		}

		protected override CommandResponse PressEnter(IDictionary<string, object> parameters)
		{
			try
			{
				// Virtual-Key Codes: https://learn.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes			
				_appiumApp.Driver.ExecuteScript("windows: keys", new Dictionary<string, object>
				{
					["actions"] = new[]
					{
						new Dictionary<string, object> { ["virtualKeyCode"] = 0x0D, ["down"]=false }, // Enter
					}
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
