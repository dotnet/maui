using Xunit;
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

		[Fact]
		[Category(UITestCategories.Slider)]
		public void SettingMaximumTrackColorOnSliderWorks()
		{
			App.WaitForElement("WaitForSliderControl");
			VerifyScreenshot();
		}
	}
}