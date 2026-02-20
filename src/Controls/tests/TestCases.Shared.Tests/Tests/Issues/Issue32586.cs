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

	[Test]
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

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyFooterPositionRespectsSafeArea()
	{
		// Tester reported: "The footer view is always rendered above the safe area,
		// even when SafeAreaEdges is set to None for both parent and footer."
		// This test verifies the footer's bottom edge relative to the screen.

		// Step 1: Show footer with default SafeAreaEdges (Container on parent)
		App.WaitForElement("FooterButton");
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);

		var footerWithSafeArea = App.WaitForElement("FooterContentButton").GetRect();
		var screenBottom = App.WaitForElement("MainGrid").GetRect();
		
		// Record footer bottom position with safe area active
		var footerBottomWithSafeArea = footerWithSafeArea.Y + footerWithSafeArea.Height;

		// Step 2: Hide footer, set both parent and child SafeAreaEdges to None
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now hidden", timeoutSec: 10);

		App.Tap("ParentSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: Container");
		App.Tap("ChildSafeAreaToggleButton");
		WaitForText("SafeAreaStatusLabel", "Parent: None, Child: None");

		// Step 3: Show footer again with SafeAreaEdges=None
		App.Tap("FooterButton");
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);

		var footerWithoutSafeArea = App.WaitForElement("FooterContentButton").GetRect();
		var footerBottomWithoutSafeArea = footerWithoutSafeArea.Y + footerWithoutSafeArea.Height;

		// Step 4: With SafeAreaEdges=None, footer should extend further down
		// (closer to or into the bottom safe area)
		Assert.That(footerBottomWithoutSafeArea, Is.GreaterThanOrEqualTo(footerBottomWithSafeArea),
			$"Footer with SafeAreaEdges=None (bottom={footerBottomWithoutSafeArea}) should extend at least as far down as " +
			$"footer with SafeAreaEdges=Container (bottom={footerBottomWithSafeArea}). " +
			"The footer is being rendered above the safe area even when SafeAreaEdges=None.");
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyRuntimeSafeAreaEdgesChange()
	{
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
		WaitForText("TestLabel", "Footer is now visible", timeoutSec: 10);
	}
}
#endif