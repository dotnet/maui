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
					_app.Driver.HideKeyboard("return");
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