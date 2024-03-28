using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1700 : IssuesUITest
	{
		const string Success = "Success";

		public Issue1700(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Image fails loading from long URL";

		[Test]
		[Category(UITestCategories.Image)]
		[FailsOnIOS]
		public void LongImageURLsShouldNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			// Give the images some time to load (or fail)
			Task.Delay(3000).Wait();

			// If we can see this label at all, it means we didn't crash and the test is successful
			RunningApp.WaitForElement(Success);
		}
	}
}