using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class FlyoutBehaviorShell : _IssuesUITest
{
	public FlyoutBehaviorShell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Behavior";

	// [FailsOnAndroid]
	// [Test]
	// [Category(UITestCategories.Shell)]
	// public void FlyoutTests()
	// {
	// 	// Flyout is visible
	// 	App.WaitForElement(EnableFlyoutBehavior);

	// 	// Starting shell out as disabled correctly disables flyout
	// 	App.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible on Startup");
	// 	ShowFlyout(usingSwipe: true, testForFlyoutIcon: false);
	// 	App.WaitForNoElement(FlyoutItem, "Flyout Visible on Startup");

	// 	// Enable Flyout Test
	// 	App.Tap(EnableFlyoutBehavior);
	// 	ShowFlyout(usingSwipe: true);
	// 	App.WaitForElement(FlyoutItem, "Flyout Not Visible after Enabled");
	// 	App.Tap(FlyoutItem);

	// 	// Flyout Icon is not visible but you can still swipe open
	// 	App.Tap(DisableFlyoutBehavior);
	// 	App.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible after being Disabled");
	// 	ShowFlyout(usingSwipe: true, testForFlyoutIcon: false);
	// 	App.WaitForNoElement(FlyoutItem, "Flyout Visible after being Disabled");


	// 	// enable flyout and make sure disabling back button behavior doesn't hide icon
	// 	App.Tap(EnableFlyoutBehavior);
	// 	App.WaitForElement(FlyoutIconAutomationId);
	// 	App.Tap(DisableBackButtonBehavior);
	// 	ShowFlyout(usingSwipe: true);
	// 	App.WaitForElement(FlyoutItem, "Flyout swipe not working after Disabling Back Button Behavior");
	// 	App.Tap(FlyoutItem);

	// 	// make sure you can still open flyout via code
	// 	App.Tap(EnableFlyoutBehavior);
	// 	App.Tap(EnableBackButtonBehavior);
	// 	App.Tap(OpenFlyout);
	// 	App.WaitForElement(FlyoutItem, "Flyout not opening via code");
	// 	App.Tap(FlyoutItem);

	// 	// make sure you can still open flyout via code if flyout behavior is disabled
	// 	App.Tap(DisableFlyoutBehavior);
	// 	App.Tap(EnableBackButtonBehavior);
	// 	App.Tap(OpenFlyout);
	// 	App.WaitForElement(FlyoutItem, "Flyout not opening via code when flyout behavior disabled");
	// 	App.Tap(FlyoutItem);

	// 	// make sure you can still open flyout via code if back button behavior is disabled
	// 	App.Tap(EnableFlyoutBehavior);
	// 	App.Tap(DisableBackButtonBehavior);
	// 	App.Tap(OpenFlyout);
	// 	App.WaitForElement(FlyoutItem, "Flyout not opening via code when back button behavior is disabled");
	// 	App.Tap(FlyoutItem);

	// }

	// [Test]
	// public void WhenFlyoutIsLockedButtonsAreStillVisible()
	// {
	// 	// FlyoutLocked ensure that the flyout and buttons are still visible
	// 	App.Tap(EnableBackButtonBehavior);
	// 	App.Tap(LockFlyoutBehavior);
	// 	App.WaitForElement(title, "Flyout Locked hiding content");
	// 	App.Tap(EnableFlyoutBehavior);
	// 	App.WaitForNoElement(FlyoutItem);
	// }
}
