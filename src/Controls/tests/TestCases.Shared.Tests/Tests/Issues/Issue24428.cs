#if ANDROID || IOS // Using AppThemeBinding and changing theme not working on Windows and ThemeChangeAction not implemented in Appium Catalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24428NavPage : _IssuesUITest
	{
		public Issue24428NavPage(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "AppThemeBinding BarBackground with GradientBrush in NavigationPage not working";

		[Test]
		[Category(UITestCategories.Page)]
		public void NavigationBarBackgroundShouldChange()
		{
			Exception? exception = null;

			try
			{
				App.WaitForElement("lightThemeLabel");
				VerifyScreenshotOrSetException(ref exception, "NavigationBarBackgroundShouldChangeLightTheme");
				App.SetDarkTheme();
				App.WaitForElement("darkThemeLabel");
				VerifyScreenshotOrSetException(ref exception, "NavigationBarBackgroundShouldChangeDarkTheme");
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