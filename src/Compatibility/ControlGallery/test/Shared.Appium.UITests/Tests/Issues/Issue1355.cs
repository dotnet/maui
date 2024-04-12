using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1355 : IssuesUITest
	{
		const string Success = "Success";

		public Issue1355(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Setting Main Page in quick succession causes crash on Android";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[FailsOnAndroid]
		public void SwitchMainPageOnAppearing()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			// Without the fix, this would crash. If we're here at all, the test passed.
			RunningApp.WaitForNoElement(Success);
		}
	}
}