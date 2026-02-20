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
		// Tester reported REGRESSION: "The footer view is always rendered above the safe area,
		// even when SafeAreaEdges is set to None for both the parent and the footer view itself."
		// With SafeAreaEdges=None, the footer should extend into the bottom safe area
		// (i.e., its bottom edge should reach the screen bottom).

		// Get screen dimensions from the main grid
		var screenRect = App.WaitForElement("MainGrid").GetRect();
		var screenBottom = screenRect.Y + screenRect.Height;

		// Step 1: Set both parent and child SafeAreaEdges to None
		App.Tap("ParentSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: Container");
		App.Tap("ChildSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: None");

		// Step 2: Show footer with SafeAreaEdges=None on everything
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);

		// Step 3: Measure footer position
		var footerRect = App.WaitForElement("FooterContentButton").GetRect();
		var footerBottom = footerRect.Y + footerRect.Height;

		// Step 4: Footer bottom should reach close to the screen bottom.
		// On iPhone Xs, bottom safe area is ~34pt. If footer is still insetted
		// by safe area despite SafeAreaEdges=None, it will be ~34pt short.
		// Allow small tolerance (5pt) for padding/margins on the button itself.
		var distanceFromBottom = screenBottom - footerBottom;
		Assert.That(distanceFromBottom, Is.LessThan(20),
			$"Footer bottom ({footerBottom}) should reach near screen bottom ({screenBottom}) " +
			$"when SafeAreaEdges=None, but is {distanceFromBottom}pt short. " +
			"The footer is still being insetted by the safe area despite SafeAreaEdges=None.");
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
}
#endif