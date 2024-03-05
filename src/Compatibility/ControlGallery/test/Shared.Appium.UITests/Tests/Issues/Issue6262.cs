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
		public void ImageShouldLayoutOnTopOfButton()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("ClickMe");
			App.Click("ClickMe");
			App.WaitForElement("ClickMe");
			App.WaitForNoElement("Fail");
			App.Click("RetryTest");
			App.WaitForElement("ClickMe");
			App.Click("ClickMe");
			App.WaitForElement("ClickMe");
			App.WaitForNoElement("Fail");
		}
	}
}