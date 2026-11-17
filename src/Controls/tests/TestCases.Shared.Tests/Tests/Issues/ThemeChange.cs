#if ANDROID || IOS // Using AppThemeBinding and changing theme not working on Windows and ThemeChangeAction not implemented in Appium Catalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class ThemeChange : _IssuesUITest
	{
		public override string Issue => "UI theme change during the runtime";

		public ThemeChange(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void AppThemeShouldChange()
		{
			Exception? exception = null;

			try
			{
				App.SetLightTheme();
				_ = App.WaitForElement("labelVisibleOnlyInLightMode");

				App.SetDarkTheme();
				_ = App.WaitForElement("labelVisibleOnlyInDarkMode");
				VerifyScreenshotOrSetException(ref exception, "AppThemeShouldChangeDarkTheme");

				App.SetLightTheme();
				_ = App.WaitForElement("labelVisibleOnlyInLightMode");
				VerifyScreenshotOrSetException(ref exception, "AppThemeShouldChangeLightTheme");
			}
			finally
			{
				App.SetLightTheme();
			}

			if (exception is not null)
			{
				throw exception;
			}
		}
	}
}
#endif