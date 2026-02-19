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
					if (HelperExtensions.IsIOS26OrHigher((AppiumIOSApp)_app))
					{
						var keyboard = _app.Driver.FindElements(By.XPath("//XCUIElementTypeKeyboard")).FirstOrDefault(k => k.Displayed && k.Enabled);
						if (keyboard == null)
						{
							return CommandResponse.FailedEmptyResponse;
						}
						// Look for button with name='Return' within keyboard
						keyboard?.FindElement(By.Id("Return"))?.Click();
					}
					else
					{
						_app.Driver.HideKeyboard("return");
					}
				}
			}
			catch (InvalidElementStateException)
			{
				return CommandResponse.FailedEmptyResponse;
			}
			catch (NoSuchElementException)
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