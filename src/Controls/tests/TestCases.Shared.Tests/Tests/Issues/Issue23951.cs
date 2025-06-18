using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23951 : _IssuesUITest
	{
		public Issue23951(TestDevice device) : base(device) { }

		public override string Issue => "[Android] Frame disappears when assigning GradientStops to LinearGradientBrush inside this Frame";

		[Test]
		[Category(UITestCategories.Frame)]
		public void FrameCornerRadiusShouldnotChange()
		{
			App.WaitForElement("Button");
			App.Tap("Button");
			VerifyScreenshot();
		}
	}
}