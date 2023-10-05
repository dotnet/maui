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
	}
}