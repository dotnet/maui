#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32586 : _IssuesUITest
{
	public override string Issue => "[iOS] Layout issue using TranslateToAsync causes infinite property changed cycle";

	public Issue32586(TestDevice device)
	: base(device)
	{ }

	void WaitForText(string elementId, string expectedText, int timeoutSec = 5)
	{
		var endTime = DateTime.Now.AddSeconds(timeoutSec);
		while (DateTime.Now < endTime)
		{
			var text = App.WaitForElement(elementId).GetText();
			if (text == expectedText) return;
			Thread.Sleep(100);
		}
		var finalText = App.WaitForElement(elementId).GetText();
		Assert.That(finalText, Is.EqualTo(expectedText), $"Timed out waiting for {elementId} text to be '{expectedText}'");
	}

	[Test, Order(1)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyFooterAnimationCompletes()
	{
		// The core bug: TranslateToAsync on the footer causes an infinite layout cycle.
		// If the animation completes and the label updates, the cycle is broken.
		App.WaitForElement("FooterButton");
		App.Tap("FooterButton");
		
		// If the animation is stuck in an infinite loop, this will time out
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);
		
		// Verify the footer is actually visible on screen
		var footerRect = App.WaitForElement("FooterContentButton").GetRect();
		Assert.That(footerRect.Height, Is.GreaterThan(0), "Footer should be visible with non-zero height");
		
		// Hide footer and verify
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now hidden", timeoutSec: 10);
	}

	[Test, Order(2)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyFooterPositionRespectsSafeArea()
	{
		// Verifies that the footer reaches the bottom of its container (MainGrid)
		// when SafeAreaEdges=None is set on both the parent Grid and child layout.
		// Note: The ContentPage itself still handles safe area, so MainGrid's bottom
		// is already inset from the screen edge. The footer should fill MainGrid fully.

		// Get container dimensions from the main grid
		var gridRect = App.WaitForElement("MainGrid").GetRect();
		var gridBottom = gridRect.Y + gridRect.Height;

		// Step 1: Set both parent and child SafeAreaEdges to None
		App.Tap("ParentSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: Container");
		App.Tap("ChildSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: None");

		// Step 2: Show footer with SafeAreaEdges=None on Grid and StackLayout
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);

		// Step 3: Measure footer position — should reach MainGrid's bottom edge
		var footerRect = App.WaitForElement("FooterContentButton").GetRect();
		var footerBottom = footerRect.Y + footerRect.Height;

		// Footer should reach close to the grid's bottom edge.
		// The grid's bottom may be inset from screen edge due to ContentPage safe area,
		// but the footer should fill within the grid without additional insets.
		var distanceFromGridBottom = gridBottom - footerBottom;

		// On Android, grid.GetRect() may include area behind the system navigation bar,
		// so the gap is larger (nav bar is ~48dp). On iOS the grid is already inset.
		var maxAllowedGap = 40;
#if ANDROID
		maxAllowedGap = 130; // Account for Android system navigation bar
#endif
		Assert.That(distanceFromGridBottom, Is.LessThan(maxAllowedGap),
			$"Footer bottom ({footerBottom}) should reach near grid bottom ({gridBottom}) " +
			$"when SafeAreaEdges=None on Grid, but is {distanceFromGridBottom}pt short.");
	}

	[Test, Order(3)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyRuntimeSafeAreaEdgesChange()
	{
		// Reset to initial state in case previous tests left state changes
		var currentStatus = App.WaitForElement("SafeAreaStatusLabel").GetText();
		if (currentStatus != "Parent: Container, Child: Container")
		{
			// If parent is None, toggle it back to Container
			if (currentStatus?.Contains("Parent: None", StringComparison.OrdinalIgnoreCase) == true)
			{
				App.Tap("ParentSafeAreaToggleButton");
			}
			// If child is None, toggle it back to Container
			currentStatus = App.WaitForElement("SafeAreaStatusLabel").GetText();
			if (currentStatus?.Contains("Child: None", StringComparison.OrdinalIgnoreCase) == true)
			{
				App.Tap("ChildSafeAreaToggleButton");
			}
			WaitForText("SafeAreaStatusLabel", "Parent: Container, Child: Container");
		}

		// Step 1: Default state - Parent Grid handles safe area (Container)
		var statusLabel = App.WaitForElement("SafeAreaStatusLabel");
		Assert.That(statusLabel.GetText(), Is.EqualTo("Parent: Container, Child: Container"));

		var topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var initialY = topMarkerRect.Y;
		Assert.That(initialY, Is.GreaterThan(0), "Content should be below safe area when parent handles it");

		// Step 2: Set parent Grid SafeAreaEdges to None
		App.Tap("ParentSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: Container");

		topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var childHandlingY = topMarkerRect.Y;
		Assert.That(childHandlingY, Is.GreaterThan(0), "Child should handle safe area when parent doesn't");

		// Step 3: Set child SafeAreaEdges to None too — content should move under safe area
		App.Tap("ChildSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: None");

		topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var noSafeAreaY = topMarkerRect.Y;
		Assert.That(noSafeAreaY, Is.LessThan(childHandlingY), "Content should move up under safe area when no one handles it");

		// Step 4: Restore parent to Container
		App.Tap("ParentSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: Container, Child: None");

		topMarkerRect = App.WaitForElement("TopMarker").GetRect();
		var restoredY = topMarkerRect.Y;
		Assert.That(restoredY, Is.GreaterThan(noSafeAreaY), "Parent should push content below safe area again");

		// Step 5: Verify UI is still responsive
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now hidden", timeoutSec: 10);
	}

	[Test, Order(4)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyRotationDuringAnimationPreservesSafeArea()
	{
		// Regression test: rotation during an active TranslateToAsync animation
		// should still update safe area correctly. The Window-level SafeAreaInsets
		// comparison fix must not suppress genuine rotation-induced changes.

		// Reset to Container state
		var currentStatus = App.WaitForElement("SafeAreaStatusLabel").GetText();
		if (currentStatus?.Contains("Parent: None", StringComparison.OrdinalIgnoreCase) == true)
			App.Tap("ParentSafeAreaToggleButton");
		currentStatus = App.WaitForElement("SafeAreaStatusLabel").GetText();
		if (currentStatus?.Contains("Child: None", StringComparison.OrdinalIgnoreCase) == true)
			App.Tap("ChildSafeAreaToggleButton");

		// Step 1: Record portrait safe area position
		App.SetOrientationPortrait();
		Thread.Sleep(1000);
		var portraitTopY = App.WaitForElement("TopMarker").GetRect().Y;
		Assert.That(portraitTopY, Is.GreaterThan(0), "Content should be below safe area in portrait");

		// Step 2: Start footer animation (triggers TranslateToAsync)
		App.Tap("FooterButton");

		// Step 3: Rotate to landscape DURING animation
		App.SetOrientationLandscape();
		Thread.Sleep(2000);

		// Step 4: Wait for animation to complete
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);

		// Step 5: Verify safe area still applies correctly in landscape
		var landscapeTopY = App.WaitForElement("TopMarker").GetRect().Y;
		Assert.That(landscapeTopY, Is.GreaterThan(0),
			"Content should still respect safe area after rotation during animation");

		// Step 6: Rotate back to portrait and verify
		App.SetOrientationPortrait();
		Thread.Sleep(2000);

		var restoredTopY = App.WaitForElement("TopMarker").GetRect().Y;
		Assert.That(restoredTopY, Is.EqualTo(portraitTopY).Within(5),
			"Safe area should restore to original portrait position after rotation cycle");

		// Cleanup: hide footer
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now hidden", timeoutSec: 10);
	}

	[Test, Order(5)]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyRapidSafeAreaToggleCycling()
	{
		// Regression test: rapidly cycling SafeAreaEdges between None and Container
		// should always produce correct layout. The _safeAreaInvalidated bug fix in
		// MauiScrollView could unmask missing invalidation paths if any exist.

		// Reset to known state
		var currentStatus = App.WaitForElement("SafeAreaStatusLabel").GetText();
		if (currentStatus?.Contains("Parent: None", StringComparison.OrdinalIgnoreCase) == true)
			App.Tap("ParentSafeAreaToggleButton");
		currentStatus = App.WaitForElement("SafeAreaStatusLabel").GetText();
		if (currentStatus?.Contains("Child: None", StringComparison.OrdinalIgnoreCase) == true)
			App.Tap("ChildSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: Container, Child: Container");

		var containerY = App.WaitForElement("TopMarker").GetRect().Y;

		// Cycle 3 times: Container → None → Container
		for (int i = 0; i < 3; i++)
		{
			// Toggle parent to None
			App.Tap("ParentSafeAreaToggleButton");
			WaitForText("SafeAreaStatusLabel", "Parent: None, Child: Container");

			// Toggle child to None
			App.Tap("ChildSafeAreaToggleButton");
			WaitForText("SafeAreaStatusLabel", "Parent: None, Child: None");

			var noneY = App.WaitForElement("TopMarker").GetRect().Y;
			Assert.That(noneY, Is.LessThan(containerY),
				$"Cycle {i + 1}: Content should be under safe area when both are None");

			// Toggle parent back to Container
			App.Tap("ParentSafeAreaToggleButton");
			WaitForText("SafeAreaStatusLabel", "Parent: Container, Child: None");

			// Toggle child back to Container
			App.Tap("ChildSafeAreaToggleButton");
			WaitForText("SafeAreaStatusLabel", "Parent: Container, Child: Container");

			var restoredY = App.WaitForElement("TopMarker").GetRect().Y;
			Assert.That(restoredY, Is.EqualTo(containerY).Within(5),
				$"Cycle {i + 1}: Content should return to original safe area position");
		}

		// Verify app is still responsive after rapid cycling
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now hidden", timeoutSec: 10);
	}
}
#endif