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
			try
			{
				App.WaitForElement("lightThemeLabel");
				VerifyScreenshot("NavigationBarBackgroundShouldChangeLightTheme");
				App.SetDarkTheme();
				App.WaitForElement("darkThemeLabel");
				VerifyScreenshot("NavigationBarBackgroundShouldChangeDarkTheme");
			}
			finally
			{
				App.SetLightTheme();
			}
		}
	}
}
#endif