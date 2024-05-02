using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18111 : _IssuesUITest
	{
		public Issue18111(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting MaximumTrackColor on Slider has no effect";

		[Test]
		[Category(UITestCategories.Slider)]
		public void SettingMaximumTrackColorOnSliderWorks()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Windows }, "Regression test validating the design differences between iOS and Mac specifically");
			App.WaitForElement("WaitForSliderControl");
			VerifyScreenshot();
		}
	}
}