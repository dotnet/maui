using OpenQA.Selenium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumIOSVirtualKeyboardActions : AppiumVirtualKeyboardActions
	{
		public AppiumIOSVirtualKeyboardActions(AppiumApp app)
			: base(app)
		{
		}

		protected override CommandResponse DismissKeyboard(IDictionary<string, object> parameters)
		{
			try
			{
				if (_app.Driver.IsKeyboardShown())
				{
					var keyboard = _app.Driver.FindElement(By.XPath("//XCUIElementTypeKeyboard"));
					// Look for button with name='Return' within keyboard
					var returnElements = keyboard.FindElement(By.XPath(".//XCUIElementTypeButton[@name='Return']"));
					returnElements.Click();
					return CommandResponse.SuccessEmptyResponse;
				}
			}
			catch (InvalidElementStateException)
			{
				return CommandResponse.FailedEmptyResponse;
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		protected override CommandResponse PressEnter(IDictionary<string, object> parameters)
		{
			try
			{
				if (_app.Driver.IsKeyboardShown())
				{
					_app.Driver.SwitchTo().ActiveElement().SendKeys(Keys.Return);
				}
			}
			catch (InvalidElementStateException)
			{
				return CommandResponse.FailedEmptyResponse;
			}

			return CommandResponse.SuccessEmptyResponse;
		}

		protected override CommandResponse PressVolumeDown(IDictionary<string, object> parameters)
		{
			try
			{
				// Press a physical button. The supported button name is home, volumedown, volumeup.
				// volumedown is only available for real devices.
				_app.Driver.ExecuteScript("mobile: pressButton", new Dictionary<string, object>
				{
					{ "name", "volumedown" },
				});

				return CommandResponse.SuccessEmptyResponse;
			}
			catch (Exception)
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}

		protected override CommandResponse PressVolumeUp(IDictionary<string, object> parameters)
		{
			try
			{
				// Press a physical button. The supported button name is home, volumedown, volumeup.	
				// volumeup is only available for real devices.
				_app.Driver.ExecuteScript("mobile: pressButton", new Dictionary<string, object>
				{
					{ "name", "volumeup" },
				});

				return CommandResponse.SuccessEmptyResponse;
			}
			catch (Exception)
			{
				return CommandResponse.FailedEmptyResponse;
			}
		}
	}
}