using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla32615 : IssuesUITest
	{
		public Bugzilla32615(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "OnAppearing is not called on previous page when modal page is popped";

		[Test]
		[Category(UITestCategories.Navigation)]
		public async Task Bugzilla32615Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("btnModal");
			RunningApp.Tap("btnPop");
			await Task.Delay(1000);
			RunningApp.WaitForNoElement("1");
		}
	}
}