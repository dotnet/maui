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
		// Note: With AdjustPan, Appium coordinates change because the window pans,
		// but the key is that no SafeArea padding is applied to the container
		App.WaitForElement("BottomMarkerPan");
		var bottomMarkerAfter = App.FindElement("BottomMarkerPan").GetRect();
		
		// Verify the bottom marker is still present and has the same dimensions
		// With AdjustPan, the marker's size shouldn't change (only screen position changes due to panning)
		Assert.That(bottomMarkerAfter.Height, Is.EqualTo(bottomMarkerBefore.Height).Within(5),
			"Bottom marker height should remain the same with AdjustPan (no padding applied, just panning)");
		
		Assert.That(bottomMarkerAfter.Width, Is.EqualTo(bottomMarkerBefore.Width).Within(5),
			"Bottom marker width should remain the same with AdjustPan");
		
		// Verify the entry field is still interactive after panning
		Assert.DoesNotThrow(() => App.FindElement("TestEntryPan"),
			"Entry should remain accessible after keyboard appears with AdjustPan");
	}
}
#endif
