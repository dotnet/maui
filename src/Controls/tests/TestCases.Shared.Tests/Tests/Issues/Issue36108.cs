using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

// Regression guard for https://github.com/dotnet/maui/issues/36108
// Verifies that after navigating between FlyoutItems (A → B → A), the Shell tab bar
// and page content are still correctly rendered — guarding against the stale
// _displayedPage guard in UpdateDisplayedPage skipping toolbar/tab configuration.
public class Issue36108 : _IssuesUITest
{
	public Issue36108(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Android Shell handler — OnDestroyView defensive cleanup regression guard";

	// Navigate Section A → Section B → Section A and verify Tab 1 content is shown.
	// If _displayedPage is not reset during ShellItem switch, UpdateDisplayedPage
	// early-returns on the stale same-page reference and the tab bar is misconfigured.
	[Test, Order(0)]
	[Category(UITestCategories.Shell)]
	public void TabContentVisibleAfterFlyoutItemRoundTrip()
	{
		App.WaitForElement("Tab1Label");

		// Open flyout and navigate to Section B
		App.TapShellFlyoutIcon();
		App.WaitForElement("Section B");
		App.Tap("Section B");
		App.WaitForElement("SectionBLabel");

		// Open flyout and navigate back to Section A
		App.TapShellFlyoutIcon();
		App.WaitForElement("Section A");
		App.Tap("Section A");

		// Tab 1 content must be visible — UpdateDisplayedPage must not early-return
		App.WaitForElement("Tab1Label");

		// Defensive: verify tab bar is functional after round-trip.
		// If UpdateTabBarVisibility was skipped (due to stale _displayedPage early-return),
		// the bottom nav tabs may be hidden or misconfigured — Tab 2 would be unreachable.
		App.WaitForElement("Tab 2");
	}

	// After navigating A → B → A, verify that tapping Tab 2 correctly shows Tab 2 content.
	// This guards against ViewPager2 page-change callbacks being dropped when the
	// ShellItem view is recreated after returning from Section B.
	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void TabSwitchingWorksAfterFlyoutItemRoundTrip()
	{
		App.WaitForElement("Tab1Label");

		// Round-trip A → B → A
		App.TapShellFlyoutIcon();
		App.WaitForElement("Section B");
		App.Tap("Section B");
		App.WaitForElement("SectionBLabel");

		App.TapShellFlyoutIcon();
		App.WaitForElement("Section A");
		App.Tap("Section A");
		App.WaitForElement("Tab1Label");

		// Switch to Tab 2 — must work correctly after the round-trip
		App.Tap("Tab 2");
		App.WaitForElement("Tab2Label");
	}
}
