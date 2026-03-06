using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellFlyoutContent : _IssuesUITest
{

#if WINDOWS //In Windows AutomationId for FlyoutItems not works in Appium.
	const string FlyoutItem = "Flyout Item Top";
	const string ResetButton = "Click to Reset";
#else
	const string FlyoutItem = "FlyoutItem";
	const string ResetButton = "Reset";
#endif

	public ShellFlyoutContent(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Content";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutContentTests()
	{
		App.WaitForElement("PageLoaded");
		App.TapInShellFlyout(FlyoutItem);
		App.Tap("ToggleContent");
		App.TapInShellFlyout("ContentView");
		App.Tap(FlyoutItem);
		App.Tap("ToggleFlyoutContentTemplate");
		App.TapInShellFlyout(ResetButton);
		App.Tap(FlyoutItem);
	}

	// https://github.com/dotnet/maui/issues/32883
	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutFooterAreaClearedAfterRemoval()
	{
		App.WaitForElement("PageLoaded");

		// The bug: When footer is removed, UpdateContentPadding() is NOT called,
		// so the bottom padding that was added for the footer remains.
		// This leaves extra empty space at the bottom of the flyout, preventing
		// proper scrolling and content positioning.
		//
		// Test strategy: Verify flyout footer area is properly cleared after removal:
		// 1. Add header/footer to flyout
		// 2. Remove header/footer (bug occurs here - padding not cleared)
		// 3. Open flyout and verify content positioning
		// 4. Scroll to bottom item to confirm proper layout restoration
		//
		// With bug: Extra padding remains
		// With fix: Padding is cleared and content positions correctly

		// Step 1: Add header/footer to flyout
		App.Tap("ToggleHeaderFooter");
		App.TapShellFlyoutIcon();
		App.WaitForElement(FlyoutItem);

		// Close flyout
		App.CloseFlyout();

		// Step 2: Remove header/footer - THIS IS WHERE THE BUG MANIFESTS
		App.Tap("ToggleHeaderFooter");

		// Step 3: Open flyout and verify content positioning
		App.TapShellFlyoutIcon();
		App.WaitForElement(FlyoutItem);

		// Step 4: Scroll to bottom item to confirm proper layout restoration
		App.ScrollToBottom(FlyoutItem, "Flyout Item Bottom", strategy: ScrollStrategy.Gesture, swipeSpeed: 50, maxScrolls: 100);

		VerifyScreenshot();
	}
}