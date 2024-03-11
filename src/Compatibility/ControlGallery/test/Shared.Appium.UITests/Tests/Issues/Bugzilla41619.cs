using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla41619 : IssuesUITest
	{
		const double Success = 6;

		public Bugzilla41619(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[WinRT/UWP] Slider binding works incorrectly";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		public void AppDoesntCrashWhenResettingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForNoElement(Success.ToString());
		}
	}
}