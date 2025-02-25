#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24356 : _IssuesUITest
	{
		public Issue24356(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "AppThemeBinding BarBackground with Brush in TabbedPage not working";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void GradientInTabBarShouldChange()
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