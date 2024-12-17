#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Orientation nott supported in MacCatalyt and Windows. In IOS Time ou Exception for "Split"
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7556 : _IssuesUITest
{
	public Issue7556(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Masterbehavior.popover not being observed on iOS 13";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void MasterStillVisibleAfterPushingAndPoppingModalPage()
	{
		App.SetOrientationLandscape();
		App.WaitForElement("Split");
		App.TapFlyoutPageIcon("Flyout");
		App.WaitForElement("Flyout Visible");
		App.WaitForElementTillPageNavigationSettled("PushModalPage");
		App.Tap("PushModalPage");
		App.WaitForElementTillPageNavigationSettled("PopModalPage");
		App.Tap("PopModalPage");
		App.WaitForElement("Flyout Visible");
	}

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void SplitOnLandscapeFailsToDetectClose()
	{
		while (App.WaitForElement("CurrentMasterBehavior").ReadText() != Microsoft.Maui.Controls.FlyoutLayoutBehavior.SplitOnLandscape.ToString())
		{
			App.Tap("ChangeMasterBehavior");

			if (App.FindElements("Flyout Visible").Count > 0)
				App.Tap("Close Flyout");
		}

		App.TapFlyoutPageIcon("Flyout");

		App.WaitForElement("Flyout Visible");
		App.Tap("Close Flyout");

		App.SetOrientationLandscape();
		App.SetOrientationPortrait();
		App.SetOrientationLandscape();
		App.SetOrientationPortrait();

		if (App.FindElements("Flyout Visible").Count > 0)
			App.Tap("Close Flyout");

		App.TapFlyoutPageIcon("Flyout");
		App.WaitForElement("Flyout Visible");
		App.Tap("Close Flyout");
		App.TapFlyoutPageIcon("Flyout");
		App.WaitForElement("Flyout Visible");
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif
