#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Android-only fix; exclude MacCatalyst (flaky) and Windows (scroll doesn't recycle items)
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34783 : _IssuesUITest
{
	public override string Issue => "CollectionView Dynamic item sizing - After dragging the scrollbar all images return to their original size";

	public Issue34783(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ImagesKeepSizeAfterScrolling()
	{
		// Wait for the CollectionView to load; first item "Baboon" image is used as the anchor
		App.WaitForElement("Baboon");

		// Record the initial rendered height of the first image (should be ~60)
		var initialRect = App.WaitForElement("Baboon").GetRect();

		// Tap the first image to enlarge it (60 → 100) using coordinates to reliably
		// hit the Image element (which has a TapGestureRecognizer, not a Button click)
		App.TapCoordinates(initialRect.X + initialRect.Width / 2, initialRect.Y + initialRect.Height / 2);

		// Confirm the image has grown; layout/measure can complete asynchronously after the tap
		App.RetryAssert(() =>
		{
			var enlargedRect = App.WaitForElement("Baboon").GetRect();
			Assert.That(enlargedRect.Height, Is.GreaterThan(initialRect.Height),
				"Image should be enlarged after tap");
		});

		// Scroll down to push the first item off screen (simulates dragging the scrollbar,
		// which triggers RecyclerView item recycling on Android)
		App.ScrollDown("Issue34783CollectionView", ScrollStrategy.Gesture, 0.9);

		// Verify "Baboon" has left the viewport, confirming RecyclerView recycling will occur
		App.WaitForNoElement("Baboon");

		// Scroll back up to bring Baboon back into view
		App.ScrollUp("Issue34783CollectionView", ScrollStrategy.Gesture, 0.9);
		App.ScrollUp("Issue34783CollectionView", ScrollStrategy.Gesture, 0.9);

		// Verify the image is still at the enlarged size and was not reset to the original
		App.WaitForElement("Baboon");
		var afterScrollRect = App.WaitForElement("Baboon").GetRect();
		Assert.That(afterScrollRect.Height, Is.GreaterThan(initialRect.Height),
			"Image should keep its enlarged size after scrolling; resetting to original size is the bug");
	}
}
#endif
