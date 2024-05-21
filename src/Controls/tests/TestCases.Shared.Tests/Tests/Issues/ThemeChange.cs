#if ANDROID || IOS
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
		public void AppThemeShouldChange()
		{
			_ = App.WaitForElement("labelVisibleOnlyInLightMode");

			App.SetDarkTheme();
			_ = App.WaitForElement("labelVisibleOnlyInDarkMode");
			VerifyScreenshot();

			App.SetLightTheme();
			_ = App.WaitForElement("labelVisibleOnlyInLightMode");
			VerifyScreenshot();
		}
	}
}
#endif