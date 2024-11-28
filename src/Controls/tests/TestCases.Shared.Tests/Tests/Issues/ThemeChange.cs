#if ANDROID || IOS // Using AppThemeBinding and changing theme not working on Windows
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
			try
			{
				App.SetLightTheme();
				_ = App.WaitForElement("labelVisibleOnlyInLightMode");

				App.SetDarkTheme();
				_ = App.WaitForElement("labelVisibleOnlyInDarkMode");
				VerifyScreenshot("AppThemeShouldChangeDarkTheme");

				App.SetLightTheme();
				_ = App.WaitForElement("labelVisibleOnlyInLightMode");
				VerifyScreenshot("AppThemeShouldChangeLightTheme");
			}
			finally
			{
				App.SetLightTheme();
			}
		}
	}
}
#endif