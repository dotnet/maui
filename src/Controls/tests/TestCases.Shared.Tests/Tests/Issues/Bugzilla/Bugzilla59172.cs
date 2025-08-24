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

		[Test, Order(1)]
		[Category(UITestCategories.Navigation)]
		public void Issue59172Test()
		{
			App.WaitForElement("GoForward");
			App.Tap("GoForward");
			App.WaitForElementTillPageNavigationSettled("GoBackDelayed");
			App.Tap("GoBackDelayed");
			App.TapBackArrow();

			// App should not have crashed
			App.WaitForElementTillPageNavigationSettled("GoForward");
		}

		[Test, Order(2)]
		[Category(UITestCategories.Navigation)]
		public void Issue59172RecoveryTest()
		{
			App.WaitForElement("GoForward");
			App.Tap("GoForward");
			App.WaitForElementTillPageNavigationSettled("GoBackDelayedSafe");
			App.Tap("GoBackDelayedSafe");
			App.TapBackArrow();

			App.WaitForElementTillPageNavigationSettled("GoForward");
			App.Tap("GoForward");

			// App should navigate
			App.WaitForElementTillPageNavigationSettled("GoBackDelayedSafe");
		}
	}
}