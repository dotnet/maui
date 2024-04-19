using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue968 : IssuesUITest
	{
		public Issue968(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "StackLayout does not relayout on device rotation";

		[Test]
		[Description("Verify the layout lays out on rotations")]
		[Category(UITestCategories.Layout)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue968TestsRotationRelayoutIssue()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("TestReady");
			RunningApp.SetOrientationLandscape();
			RunningApp.Screenshot("Rotated to Landscape");
			RunningApp.WaitForNoElement("You should see me after rotating");
			RunningApp.Screenshot("StackLayout in Modal respects rotation");
			RunningApp.SetOrientationPortrait();
		}
	}
}