using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	[Category(UITestCategories.Border)]
	public class Issue22606 : _IssuesUITest
	{
		public Issue22606(TestDevice device) : base(device) { }

		public override string Issue => "Border does not expand on Content size changed";

		[Test]
		public void BorderBackgroundExpandsOnContentSizeChanged()
		{
			App.WaitForElement("SetHeightTo200");
			App.Tap("SetHeightTo200");
			VerifyScreenshot("Issue22606_SetHeightTo200");

			App.Tap("SetHeightTo500");
			VerifyScreenshot("Issue22606_SetHeightTo500");
		}

#if ANDROID || IOS
		[Test]
		public void BorderBackgroundSizeUpdatesWhenRotatingScreen()
		{
			App.WaitForElement("SetHeightTo200");
			App.Tap("SetHeightTo200");
			App.SetOrientationLandscape();
			VerifyScreenshot();
		}
#endif
	}
}