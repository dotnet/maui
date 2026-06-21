using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class FlyoutBehaviorShell : _IssuesUITest
{
	const string title = "Basic Test";
	const string FlyoutItem = "Flyout Item";
	const string EnableFlyoutBehavior = "EnableFlyoutBehavior";
	const string DisableFlyoutBehavior = "DisableFlyoutBehavior";
	const string LockFlyoutBehavior = "LockFlyoutBehavior";
	const string OpenFlyout = "OpenFlyout";
	const string EnableBackButtonBehavior = "EnableBackButtonBehavior";
	const string DisableBackButtonBehavior = "DisableBackButtonBehavior";
	public FlyoutBehaviorShell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Behavior";


	//	[Test]
	// 	[Category(UITestCategories.Shell)]
	public void FlyoutTests()
	{
		// Flyout is visible
		App.WaitForElement(EnableFlyoutBehavior);
#if !MACCATALYST && !WINDOWS //Swipe Options for Shell are not applicable for Desktop Platforms.
		// Starting shell out as disabled correctly disables flyout
		App.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible on Startup");
		App.ShowFlyout(usingSwipe: true, waitForFlyoutIcon: false);
		App.WaitForNoElement(FlyoutItem, "Flyout Visible on Startup");

		// Enable Flyout Test
		App.Tap(EnableFlyoutBehavior);
		App.ShowFlyout(usingSwipe: true);
		App.WaitForElement(FlyoutItem, "Flyout Not Visible after Enabled");
		App.Tap(FlyoutItem);

		// Flyout Icon is not visible but you can still swipe open
		App.Tap(DisableFlyoutBehavior);
		App.WaitForNoElement(FlyoutIconAutomationId, "Flyout Icon Visible after being Disabled");
		App.ShowFlyout(usingSwipe: true, waitForFlyoutIcon: false);
		App.WaitForNoElement(FlyoutItem, "Flyout Visible after being Disabled");


		// enable flyout and make sure disabling back button behavior doesn't hide icon
		App.Tap(EnableFlyoutBehavior);
		App.WaitForFlyoutIcon();
		App.Tap(DisableBackButtonBehavior);
		App.ShowFlyout(usingSwipe: true);
		App.WaitForElement(FlyoutItem, "Flyout swipe not working after Disabling Back Button Behavior");
		App.Tap(FlyoutItem);
#endif
		// // make sure you can still open flyout via code
		App.Tap(EnableFlyoutBehavior);
		App.Tap(EnableBackButtonBehavior);
		App.Tap(OpenFlyout);
		App.WaitForElement(FlyoutItem, "Flyout not opening via code");
		App.Tap(FlyoutItem);

#if !IOS && !MACCATALYST // When DisableFlyoutBehavior is set, flyout items become inaccessible via inspection tools, leading to timeout exceptions on both iOS and Catalyst.

		// make sure you can still open flyout via code if flyout behavior is disabled
		App.Tap(DisableFlyoutBehavior);
		App.Tap(EnableBackButtonBehavior);
		App.Tap(OpenFlyout);
		App.WaitForElement(FlyoutItem, "Flyout not opening via code when flyout behavior disabled");
		App.Tap(FlyoutItem);
#endif

		// make sure you can still open flyout via code if back button behavior is disabled
		App.Tap(EnableFlyoutBehavior);
		App.Tap(DisableBackButtonBehavior);
		App.Tap(OpenFlyout);
		App.WaitForElement(FlyoutItem, "Flyout not opening via code when back button behavior is disabled");
		App.Tap(FlyoutItem);

	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void WhenFlyoutIsLockedButtonsAreStillVisible()
	{
		// FlyoutLocked ensure that the flyout and buttons are still visible
		App.WaitForElement(EnableBackButtonBehavior);
		App.Tap(EnableBackButtonBehavior);
		App.Tap(LockFlyoutBehavior);
		App.WaitForElement(title, "Flyout Locked hiding content");
		App.Tap(EnableFlyoutBehavior);
		App.WaitForNoElement(FlyoutItem);
	}
}
