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
		// the "Flyout Item Bottom" from scrolling to its proper position.
		//
		// Test strategy: Measure the Y position of "Flyout Item Bottom" in three states:
		// 1. Without header/footer (baseline)
		// 2. With header/footer (pushed up by footer)
		// 3. After removing header/footer (should return to baseline)
		//
		// With bug: Position stays at #2 (high) because padding not cleared
		// With fix: Position returns to #1 (low) because padding is cleared

		// Step 1: Get baseline position WITHOUT header/footer
		App.TapShellFlyoutIcon();
		App.WaitForElement(FlyoutItem);
		
		// Scroll to bottom to reach "Flyout Item Bottom"
		// Scroll to bottom of flyout
		var itemsBefore = App.FindElements(FlyoutItem).Count();
		Console.WriteLine($"[Baseline] Items visible before scroll: {itemsBefore}");
		
		App.ScrollToBottom(FlyoutItem, "Flyout Item Bottom", strategy: ScrollStrategy.Gesture, swipeSpeed: 50, maxScrolls: 100);
		
		var itemsAfter = App.FindElements(FlyoutItem).Count();
		Console.WriteLine($"[Baseline] Items visible after scroll: {itemsAfter}");
		
		var baselineBottomItem = App.WaitForElement("Flyout Item Bottom");
		float baselineY = baselineBottomItem.GetRect().Y;
		Console.WriteLine($"[Baseline] Bottom item Y position: {baselineY}");
		
		// Close flyout
		App.CloseFlyout();

		// Step 2: Add header/footer and measure position
		App.Tap("ToggleHeaderFooter");
		App.TapShellFlyoutIcon();
		App.WaitForElement(FlyoutItem);
		
		// Scroll to bottom again
		itemsBefore = App.FindElements(FlyoutItem).Count();
		Console.WriteLine($"[WithFooter] Items visible before scroll: {itemsBefore}");
		
		App.ScrollToBottom(FlyoutItem, "Flyout Item Bottom", strategy: ScrollStrategy.Gesture, swipeSpeed: 50, maxScrolls: 100);
		
		itemsAfter = App.FindElements(FlyoutItem).Count();
		Console.WriteLine($"[WithFooter] Items visible after scroll: {itemsAfter}");
		
		var withFooterBottomItem = App.WaitForElement("Flyout Item Bottom");
		float withFooterY = withFooterBottomItem.GetRect().Y;
		Console.WriteLine($"[WithFooter] Bottom item Y position: {withFooterY}");
		
		// Verify footer pushed content up (Y position should be higher/smaller)
		Assert.That(withFooterY, Is.LessThan(baselineY), 
			"With footer, bottom item should be pushed up (lower Y value)");
		
		// Close flyout
		App.CloseFlyout();

		// Step 3: Remove header/footer - THIS IS WHERE THE BUG MANIFESTS
		App.Tap("ToggleHeaderFooter");
		App.TapShellFlyoutIcon();
		App.WaitForElement(FlyoutItem);
		
		// Scroll to bottom
		itemsBefore = App.FindElements(FlyoutItem).Count();
		Console.WriteLine($"[AfterRemoval] Items visible before scroll: {itemsBefore}");
		
		App.ScrollToBottom(FlyoutItem, "Flyout Item Bottom", strategy: ScrollStrategy.Gesture, swipeSpeed: 50, maxScrolls: 100);
		
		itemsAfter = App.FindElements(FlyoutItem).Count();
		Console.WriteLine($"[AfterRemoval] Items visible after scroll: {itemsAfter}");
		
		var afterRemovalBottomItem = App.WaitForElement("Flyout Item Bottom");
		float afterRemovalY = afterRemovalBottomItem.GetRect().Y;
		Console.WriteLine($"[AfterRemoval] Bottom item Y position: {afterRemovalY}");
		
		// THE VALIDATION: After removing footer, position should return close to baseline
		// With bug: afterRemovalY stays near withFooterY (padding NOT cleared)
		// With fix: afterRemovalY returns near baselineY (padding cleared)
		//
		// We allow some tolerance for rendering differences, but the Y position
		// should be much closer to baseline than to withFooter
		
		float baselineDistance = Math.Abs(afterRemovalY - baselineY);
		float withFooterDistance = Math.Abs(afterRemovalY - withFooterY);
		
		Assert.That(baselineDistance, Is.LessThan(withFooterDistance),
			$"After footer removal, bottom item position should return close to baseline. " +
			$"Baseline Y: {baselineY}, With Footer Y: {withFooterY}, After Removal Y: {afterRemovalY}. " +
			$"If after-removal is closer to with-footer position, the footer padding was NOT cleared.");
		
		// Close flyout
		App.CloseFlyout();
	}
}