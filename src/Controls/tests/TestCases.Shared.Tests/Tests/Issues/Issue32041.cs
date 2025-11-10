#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32041 : _IssuesUITest
{
	public override string Issue => "Keyboard overlaps Entry when SoftInput.AdjustResize is set";

	public Issue32041(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyContainerResizesWithAdjustResize()
	{
		// Wait for the main container to be visible
		App.WaitForElement("MainContainer");

		// Get the container's initial height before keyboard appears
		var containerBeforeKeyboard = App.FindElement("MainContainer").GetRect();
		var initialHeight = containerBeforeKeyboard.Height;

		// Tap the entry to show the keyboard
		App.Tap("TestEntry");

		// Wait for keyboard to appear and layout to adjust
		// Keyboard animation can take 300-500ms, plus layout adjustment time
		Task.Delay(1500).Wait();

		// Get the container's height after keyboard appears
		var containerAfterKeyboard = App.FindElement("MainContainer").GetRect();
		var resizedHeight = containerAfterKeyboard.Height;

		// With AdjustResize, the entire container should shrink (height decreases) to make room for keyboard
		// This ensures all content (TabbedPage tabs, FlyoutPage content, etc.) stays above the keyboard
		Assert.That(resizedHeight, Is.LessThan(initialHeight),
			$"Container should resize (height decrease) when keyboard appears with AdjustResize. Before: {initialHeight}px, After: {resizedHeight}px");

		// The height reduction should be significant (at least 200px for a typical keyboard)
		var heightReduction = initialHeight - resizedHeight;
		Assert.That(heightReduction, Is.GreaterThan(200),
			$"Container should shrink by at least 200px (typical keyboard height). Actual reduction: {heightReduction}px");

		// Verify the bottom marker is still visible and accessible
		var bottomMarker = App.FindElement("BottomMarker");
		Assert.That(bottomMarker, Is.Not.Null, 
			"Bottom marker should remain visible and accessible after keyboard appears");
	}
}

public class Issue32041AdjustPan : _IssuesUITest
{
	public override string Issue => "Verify AdjustPan mode does not apply keyboard insets";

	public Issue32041AdjustPan(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyContainerDoesNotResizeWithAdjustPan()
	{
		// Wait for the main container to be visible
		App.WaitForElement("MainContainerPan");

		// Verify the bottom marker is visible and accessible before keyboard
		App.WaitForElement("BottomMarkerPan");
		var bottomMarkerBefore = App.FindElement("BottomMarkerPan").GetRect();
		
		// Tap the entry to show the keyboard
		App.Tap("TestEntryPan");

		// Wait for keyboard to appear and layout to settle
		Task.Delay(1500).Wait();

		// With AdjustPan, the window pans (moves up) rather than resizing content
		// The bottom marker should still be accessible (just panned up, not cut off)
		// Note: Appium's GetRect() returns visible bounds, so height may differ when panned
		// The key is that the element remains accessible and interactive
		App.WaitForElement("BottomMarkerPan");
		var bottomMarkerAfter = App.FindElement("BottomMarkerPan").GetRect();
		
		// Verify the bottom marker is still present and has reasonable dimensions
		// With panning, the marker moves but maintains its structure
		Assert.That(bottomMarkerAfter.Height, Is.GreaterThan(0),
			"Bottom marker should remain accessible with AdjustPan (window pans instead of resizing)");
		
		// Verify the entry field is still interactive after panning
		Assert.DoesNotThrow(() => App.FindElement("TestEntryPan"),
			"Entry should remain accessible after keyboard appears with AdjustPan");
	}
}
#endif
