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
		public void SettingMaximumTrackColorOnSliderWorks()
		{
			App.WaitForElement("WaitForSliderControl");
			VerifyScreenshot();
		}
	}
}