using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6262 : IssuesUITest
	{
		public Issue6262(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Button in Grid gets wrong z-index";

		[Test]
		[Category(UITestCategories.Button)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void ImageShouldLayoutOnTopOfButton()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("ClickMe");
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement("ClickMe");
			RunningApp.WaitForNoElement("Fail");
			RunningApp.Tap("RetryTest");
			RunningApp.WaitForElement("ClickMe");
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForElement("ClickMe");
			RunningApp.WaitForNoElement("Fail");
		}
	}
}