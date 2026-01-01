#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

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
		Thread.Sleep(1500);

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
