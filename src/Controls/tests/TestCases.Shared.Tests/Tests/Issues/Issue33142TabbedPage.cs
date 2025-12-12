#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33142TabbedPage : _IssuesUITest
{
	public override string Issue => "TabbedPage - Keyboard handling with SafeAreaEdges.All";

	public Issue33142TabbedPage(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageContainerResizesWithSafeAreaEdges()
	{
		// Wait for the main container to be visible
		App.WaitForElement("MainContainer");

		// Get the container's initial height before keyboard appears
		var containerBeforeKeyboard = App.FindElement("MainContainer").GetRect();
		var initialHeight = containerBeforeKeyboard.Height;

		// Get the bottom marker's initial Y position before keyboard appears
		var bottomMarkerBefore = App.FindElement("BottomMarker").GetRect();
		var initialBottomY = bottomMarkerBefore.Y;

		// Tap the entry to show the keyboard
		App.Tap("TestEntry");

		// Wait for keyboard to appear and layout to adjust
		Task.Delay(1500).Wait();

		// Get the container's height after keyboard appears
		var containerAfterKeyboard = App.FindElement("MainContainer").GetRect();
		var resizedHeight = containerAfterKeyboard.Height;

		// With AdjustNothing, the container itself doesn't resize (height stays the same)
		// Instead, SafeArea padding is applied which pushes content up
		Assert.That(resizedHeight, Is.EqualTo(initialHeight),
			$"TabbedPage container should NOT resize with AdjustNothing. Before: {initialHeight}px, After: {resizedHeight}px");

		// Get the bottom marker's Y position after keyboard appears
		var bottomMarkerAfter = App.FindElement("BottomMarker").GetRect();
		var finalBottomY = bottomMarkerAfter.Y;

		// The bottom marker should move UP (Y position decreases) due to SafeArea bottom padding
		Assert.That(finalBottomY, Is.LessThan(initialBottomY),
			$"Bottom marker should move up when keyboard appears. Before Y: {initialBottomY}px, After Y: {finalBottomY}px");

		// The Y position change should be significant (at least 200px for a typical keyboard)
		var yPositionChange = initialBottomY - finalBottomY;
		Assert.That(yPositionChange, Is.GreaterThan(200),
			$"Bottom marker should move up by at least 200px (typical keyboard height). Actual movement: {yPositionChange}px");

		// Verify the bottom marker is still visible and accessible
		var bottomMarker = App.FindElement("BottomMarker");
		Assert.That(bottomMarker, Is.Not.Null, 
			"Bottom marker should remain visible and accessible after keyboard appears");

		// Dismiss keyboard
		App.DismissKeyboard();
		Task.Delay(1500).Wait();

		// Get the bottom marker's position after keyboard dismisses
		var bottomMarkerAfterDismiss = App.FindElement("BottomMarker").GetRect();
		var restoredBottomY = bottomMarkerAfterDismiss.Y;

		// Bottom marker should return to its original Y position (within 10px tolerance)
		Assert.That(restoredBottomY, Is.EqualTo(initialBottomY).Within(10),
			$"Bottom marker should return to original position when keyboard closes. Initial Y: {initialBottomY}px, Restored Y: {restoredBottomY}px");
	}
}
#endif
