using OpenQA.Selenium.Appium.Android;
using UITest.Core;

namespace UITest.Appium
{
	public class AppiumAndroidVirtualKeyboardActions : AppiumVirtualKeyboardActions
	{
		public AppiumAndroidVirtualKeyboardActions(AppiumApp app)
			: base(app)
		{
		}

		protected override CommandResponse DismissKeyboard(IDictionary<string, object> parameters)
		{
			if (_app.Driver.IsKeyboardShown())
			{
				_app.Driver.HideKeyboard();
			}
			return CommandResponse.SuccessEmptyResponse;
		}

		protected override CommandResponse PressVolumeDown(IDictionary<string, object> parameters)
		{
			if (_app.Driver is AndroidDriver android)
			{
				// 25 - KEYCODE_VOLUME_DOWN
				// More information: https://developer.android.com/reference/android/view/KeyEvent.html
				android.PressKeyCode(25);
				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}

		protected override CommandResponse PressVolumeUp(IDictionary<string, object> parameters)
		{
			if (_app.Driver is AndroidDriver android)
			{
				// 24 - KEYCODE_VOLUME_UP
				// More information: https://developer.android.com/reference/android/view/KeyEvent.html
				android.PressKeyCode(24);
				return CommandResponse.SuccessEmptyResponse;
			}

			return CommandResponse.FailedEmptyResponse;
		}
	}
}