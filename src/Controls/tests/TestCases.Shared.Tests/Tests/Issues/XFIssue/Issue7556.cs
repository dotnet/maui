#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS
// Orientation nott supported in MacCatalyt and Windows. 
// on iOS detail page elements are not accessible through appium when flyout is open. Issue: https://github.com/dotnet/maui/issues/16245
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
		App.WaitForElement("FlyoutVisible");
		App.WaitForElementTillPageNavigationSettled("PushModalPage");
		App.Tap("PushModalPage");
		App.WaitForElementTillPageNavigationSettled("PopModalPage");
		App.Tap("PopModalPage");
		App.WaitForElement("FlyoutVisible");
	}

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void SplitOnLandscapeFailsToDetectClose()
	{
		while (App.WaitForElement("CurrentMasterBehavior").ReadText() != Microsoft.Maui.Controls.FlyoutLayoutBehavior.SplitOnLandscape.ToString())
		{
			if (App.FindElements("FlyoutVisible").Count > 0)
				App.Tap("CloseFlyout");

			App.Tap("ChangeMasterBehavior");
		}

		App.TapFlyoutPageIcon("Flyout");

		App.WaitForElement("FlyoutVisible");
		App.Tap("CloseFlyout");
		App.WaitForNoElement("FlyoutVisible");

		App.SetOrientationLandscape();
		AssertDetailIsAccessible();
		App.SetOrientationPortrait();
		AssertDetailIsAccessible();
		App.SetOrientationLandscape();
		AssertDetailIsAccessible();
		App.SetOrientationPortrait();
		AssertDetailIsAccessible();

		App.TapFlyoutPageIcon("Flyout");
		App.WaitForElement("FlyoutVisible");
		App.Tap("CloseFlyout");
		App.TapFlyoutPageIcon("Flyout");
		App.WaitForElement("FlyoutVisible");
	}

	void AssertDetailIsAccessible()
	{
		App.WaitForElement("DetailSentinel");
		if (App.FindElements("FlyoutVisible").Count > 0)
		{
			Assert.That(App.WaitForElement("CloseFlyout").IsEnabled(), Is.False,
				"The detail sentinel must not be covered by an open overlay flyout.");
		}
	}

	[TearDown]
	public void TearDown()
	{
		App.SetOrientationPortrait();
	}
}
#endif
