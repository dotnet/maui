using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla27731 : IssuesUITest
	{
		public Bugzilla27731(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Action Bar can not be controlled reliably on FlyoutPage";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Bugzilla27731Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("Click");
			RunningApp.WaitForNoElement("PageTitle");
		}
	}
}