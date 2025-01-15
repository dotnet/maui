using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18111 : _IssuesUITest
	{
		public Issue18111(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting MaximumTrackColor on Slider has no effect";

		[Test]
		[Category(UITestCategories.Slider)]
		[FailsOnAndroidWhenRunningOnXamarinUITest("Regression test validating the design differences between iOS and Mac specifically")]
		[FailsOnWindowsWhenRunningOnXamarinUITest("Regression test validating the design differences between iOS and Mac specifically")]
		public void SettingMaximumTrackColorOnSliderWorks()
		{
			App.WaitForElement("WaitForSliderControl");
			VerifyScreenshot();
		}
	}
}