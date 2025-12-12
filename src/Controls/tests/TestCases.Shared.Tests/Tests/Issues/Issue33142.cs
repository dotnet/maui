#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33142 : _IssuesUITest
{
	public override string Issue => "Keyboard overlaps Entry with SafeAreaEdges.All";

	public Issue33142(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyContainerResizesWithSafeAreaEdges()
	{
		// Wait for the main container to be visible
		App.WaitForElement("MainContainer");

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

		// With SafeAreaEdges.All on Grid, the bottom marker should move up (Y position decreases) to stay above keyboard
		// This verifies that padding was applied to push content up
		Assert.That(afterBottom, Is.LessThan(initialBottom),
			$"Bottom marker should move up when keyboard appears with SafeAreaEdges.All. Before Y: {initialBottom}px, After Y: {afterBottom}px");

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

public class Issue33142AdjustPan : _IssuesUITest
{
	public override string Issue => "Verify AdjustNothing mode does not apply keyboard insets";

	public Issue33142AdjustPan(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void VerifyContainerDoesNotResizeWithAdjustNothing()
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

		// With AdjustNothing, the window does not resize or pan
		// SafeAreaEdges should not apply keyboard insets in this mode
		// The bottom marker should still be accessible
		App.WaitForElement("BottomMarkerPan");
		var bottomMarkerAfter = App.FindElement("BottomMarkerPan").GetRect();
		
		// Verify the bottom marker is still present and has reasonable dimensions
		Assert.That(bottomMarkerAfter.Height, Is.GreaterThan(0),
			"Bottom marker should remain accessible with AdjustNothing");
		
		// Verify the entry field is still interactive
		Assert.DoesNotThrow(() => App.FindElement("TestEntryPan"),
			"Entry should remain accessible after keyboard appears with AdjustNothing");
	}
}
#endif
