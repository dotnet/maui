using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue13436 : IssuesUITest
	{
		public Issue13436(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] Java.Lang.IllegalArgumentException in CarouselView adjusting PeekAreaInsets in OnSizeAllocated using XF 5.0";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void ChangePeekAreaInsetsInOnSizeAllocatedTest()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("CarouselId");
		}
	}
}
