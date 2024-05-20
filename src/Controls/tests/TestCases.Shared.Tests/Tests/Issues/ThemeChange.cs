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
		public async Task AppThemeShouldChange()
		{
			_ = App.WaitForElement("label");

			App.SetDarkTheme();
			await Task.Delay(200);
			VerifyScreenshot();

			App.SetLightTheme();
			await Task.Delay(200);
			VerifyScreenshot();
		}
	}
}
#endif