using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla43519 : IssuesUITest
	{
		const string Pop = "PopModal";
		const string Push = "PushModal";
		const string Page2 = "Page 2";

		public Bugzilla43519(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] FlyoutPage ArgumentException when nested in a TabbedPage and returning from modal page";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void TabbedModalNavigation()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForElement(Page2);
			RunningApp.Tap(Page2);
			RunningApp.WaitForElement(Push);
			RunningApp.Tap(Push);
			RunningApp.WaitForElement(Pop);
			RunningApp.Tap(Pop);
			RunningApp.WaitForElement(Page2);
		}
	}
}