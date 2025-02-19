#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla59172 : _IssuesUITest
	{
		public Bugzilla59172(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Popped page does not appear on top of current navigation stack, please file a bug.";

		// Test scenario: Tapping the GoBack link triggers a PopAsync 2500ms after the tap event.
		//   Right before PopAsync is triggered, manually navigate back pressing the back arrow in the navigation bar

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public async Task Issue59172Test()
		{
			App.Tap("GoForward");
			App.Tap("GoBackDelayed");
			App.Back();

			await Task.Delay(1000);

			// App should not have crashed
			App.WaitForElement("GoForward");
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[Category(UITestCategories.Compatibility)]
		public async Task Issue59172RecoveryTest()
		{
			App.Tap("GoForward");
			App.Tap("GoBackDelayedSafe");
			App.Back();

			await Task.Delay(1000);

			App.Tap("GoForward");

			// App should navigate
			App.WaitForElement("GoBackDelayedSafe");
		}
	}
}
#endif