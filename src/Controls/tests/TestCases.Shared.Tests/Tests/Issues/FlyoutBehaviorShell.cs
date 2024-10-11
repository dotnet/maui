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
	// 	RunningApp.WaitForElement(EnableFlyoutBehavior);

	// 	// Starting shell out as disabled correctly disables flyout
	// 	RunningApp.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible on Startup");
	// 	ShowFlyout(usingSwipe: true, testForFlyoutIcon: false);
	// 	RunningApp.WaitForNoElement(FlyoutItem, "Flyout Visible on Startup");

	// 	// Enable Flyout Test
	// 	RunningApp.Tap(EnableFlyoutBehavior);
	// 	ShowFlyout(usingSwipe: true);
	// 	RunningApp.WaitForElement(FlyoutItem, "Flyout Not Visible after Enabled");
	// 	RunningApp.Tap(FlyoutItem);

	// 	// Flyout Icon is not visible but you can still swipe open
	// 	RunningApp.Tap(DisableFlyoutBehavior);
	// 	RunningApp.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible after being Disabled");
	// 	ShowFlyout(usingSwipe: true, testForFlyoutIcon: false);
	// 	RunningApp.WaitForNoElement(FlyoutItem, "Flyout Visible after being Disabled");


	// 	// enable flyout and make sure disabling back button behavior doesn't hide icon
	// 	RunningApp.Tap(EnableFlyoutBehavior);
	// 	RunningApp.WaitForElement(FlyoutIconAutomationId);
	// 	RunningApp.Tap(DisableBackButtonBehavior);
	// 	ShowFlyout(usingSwipe: true);
	// 	RunningApp.WaitForElement(FlyoutItem, "Flyout swipe not working after Disabling Back Button Behavior");
	// 	RunningApp.Tap(FlyoutItem);

	// 	// make sure you can still open flyout via code
	// 	RunningApp.Tap(EnableFlyoutBehavior);
	// 	RunningApp.Tap(EnableBackButtonBehavior);
	// 	RunningApp.Tap(OpenFlyout);
	// 	RunningApp.WaitForElement(FlyoutItem, "Flyout not opening via code");
	// 	RunningApp.Tap(FlyoutItem);

	// 	// make sure you can still open flyout via code if flyout behavior is disabled
	// 	RunningApp.Tap(DisableFlyoutBehavior);
	// 	RunningApp.Tap(EnableBackButtonBehavior);
	// 	RunningApp.Tap(OpenFlyout);
	// 	RunningApp.WaitForElement(FlyoutItem, "Flyout not opening via code when flyout behavior disabled");
	// 	RunningApp.Tap(FlyoutItem);

	// 	// make sure you can still open flyout via code if back button behavior is disabled
	// 	RunningApp.Tap(EnableFlyoutBehavior);
	// 	RunningApp.Tap(DisableBackButtonBehavior);
	// 	RunningApp.Tap(OpenFlyout);
	// 	RunningApp.WaitForElement(FlyoutItem, "Flyout not opening via code when back button behavior is disabled");
	// 	RunningApp.Tap(FlyoutItem);

	// }

	// [Test]
	// public void WhenFlyoutIsLockedButtonsAreStillVisible()
	// {
	// 	// FlyoutLocked ensure that the flyout and buttons are still visible
	// 	RunningApp.Tap(EnableBackButtonBehavior);
	// 	RunningApp.Tap(LockFlyoutBehavior);
	// 	RunningApp.WaitForElement(title, "Flyout Locked hiding content");
	// 	RunningApp.Tap(EnableFlyoutBehavior);
	// 	RunningApp.WaitForNoElement(FlyoutItem);
	// }
}
