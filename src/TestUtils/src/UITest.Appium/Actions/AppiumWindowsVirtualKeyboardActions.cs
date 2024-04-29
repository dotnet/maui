using OpenQA.Selenium;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumWindowsVirtualKeyboardActions : AppiumVirtualKeyboardActions
	{
		public AppiumWindowsVirtualKeyboardActions(AppiumApp app)
			: base(app)
		{
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
	}
}