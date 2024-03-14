/*
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue13390 : IssuesUITest
	{
		public Issue13390(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Custom SlideFlyoutTransition is not working";

		[Test]
		[Category(UITestCategories.Shell)]
		public void CustomSlideFlyoutTransitionCausesCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			// If this hasn't already crashed, the test is passing
			RunningApp.WaitForElement("Success");
		}
	}
}
*/