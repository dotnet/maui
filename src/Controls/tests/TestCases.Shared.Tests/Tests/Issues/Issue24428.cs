#if ANDROID || IOS
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
				App.SetDarkTheme();
				App.WaitForElement("darkThemeLabel");
				VerifyScreenshot();
			}
			finally
			{
				App.SetLightTheme();
			}
		}
	}
}
#endif