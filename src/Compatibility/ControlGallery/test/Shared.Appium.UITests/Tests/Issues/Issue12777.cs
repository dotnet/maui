using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue12777 : IssuesUITest
	{
		public Issue12777(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CarouselView NRE if item template is not specified";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnIOS]
		public void Issue12777Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("TestCarouselView");
			RunningApp.Screenshot("Test passed");
		}
	}
}