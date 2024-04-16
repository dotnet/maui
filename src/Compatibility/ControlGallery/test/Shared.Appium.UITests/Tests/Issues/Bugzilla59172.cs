using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla59172 : IssuesUITest
	{
		public Bugzilla59172(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Popped page does not appear on top of current navigation stack, please file a bug.";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void AppDoesntCrashWhenResettingPage()
		{
			this.IgnoreIfPlatform(TestDevice.Android, "MultiWindowService not implemented.");
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("Success");
		}

		// Test scenario: Tapping the GoBack link triggers a PopAsync 2500ms after the tap event.
		//   Right before PopAsync is triggered, manually navigate back pressing the back arrow in the navigation bar

		[Test]
		[Category(UITestCategories.Navigation)]
		public async Task Issue59172Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("GoForward");
			RunningApp.Tap("GoBackDelayed");
			RunningApp.Back();

			await Task.Delay(1000);

			// App should not have crashed
			RunningApp.WaitForElement("GoForward");
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		public async Task Issue59172RecoveryTest()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("GoForward");
			RunningApp.Tap("GoBackDelayedSafe");
			RunningApp.Back();

			await Task.Delay(1000);

			RunningApp.Tap("GoForward");

			// App should navigate
			RunningApp.WaitForElement("GoBackDelayedSafe");
		}
	}
}