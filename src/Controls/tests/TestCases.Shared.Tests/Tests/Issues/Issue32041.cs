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

		// To make initial CL bottom margin not applied before keyboard appears
		VerifyScreenshot();

		// Get the bottom marker's initial position before keyboard appears
		var bottomMarkerBefore = App.FindElement("BottomMarker").GetRect();
		var initialBottom = bottomMarkerBefore.Y + bottomMarkerBefore.Height;

		// Tap the entry to show the keyboard
		App.Tap("TestEntry");

		// Wait for keyboard to appear and layout to adjust
		// Keyboard animation can take 300-500ms, plus layout adjustment time
		Task.Delay(1500).Wait();

		// Get the bottom marker's position after keyboard appears
		var bottomMarkerAfter = App.FindElement("BottomMarker").GetRect();
		var afterBottom = bottomMarkerAfter.Y + bottomMarkerAfter.Height;

		// With AdjustResize, the bottom marker should move up (Y position decreases) to stay above keyboard
		// This verifies that padding was applied to push content up
		Assert.That(afterBottom, Is.LessThan(initialBottom),
			$"Bottom marker should move up when keyboard appears with AdjustResize. Before Y: {initialBottom}px, After Y: {afterBottom}px");

		// The upward movement should be significant (at least 200px for a typical keyboard)
		var upwardMovement = initialBottom - afterBottom;
		Assert.That(upwardMovement, Is.GreaterThan(200),
			$"Bottom marker should move up by at least 200px (typical keyboard height). Actual movement: {upwardMovement}px");

		// Verify the bottom marker is still visible and accessible
		var bottomMarker = App.FindElement("BottomMarker");
		Assert.That(bottomMarker, Is.Not.Null, 
			"Bottom marker should remain visible and accessible after keyboard appears");

		// Verify the entry field is still interactive after closing the keyboard
		App.DismissKeyboard();
		Thread.Sleep(500); // Wait for keyboard to dismiss

		var bottomMarkerAfterKeyboardClose = App.FindElement("BottomMarker").GetRect();
		var afterKeyboardCloseBottom = bottomMarkerAfterKeyboardClose.Y + bottomMarkerAfterKeyboardClose.Height;

		Assert.That(afterKeyboardCloseBottom, Is.EqualTo(initialBottom),
			"Bottom marker should return to initial position after keyboard is dismissed");
	}
}
#endif
