#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Issue only affects iOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.RefreshView)]
public class Issue34165 : _IssuesUITest
{
	public Issue34165(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "CollectionView is scrolling left/right when the collection is empty and inside a RefreshView";

	[Test]
	// Regression: empty CollectionView inside RefreshView should not allow horizontal scrolling on iOS.
	// Detection strategy: measure the screen X position of the EmptyView label before and after
	// a horizontal swipe — if the X coordinate shifts, the native scroll container moved (the bug).
	public void EmptyCollectionViewInsideRefreshViewShouldNotScrollHorizontally()
	{
		App.WaitForElement("CollectionView");

		// Record the EmptyView label's initial on-screen position
		var rectBefore = App.WaitForElement("EmptyViewLabel").GetRect();

		// Swipe right inside the CollectionView — this is the gesture that triggers the bug
		App.ScrollRight("CollectionView", ScrollStrategy.Gesture, swipePercentage: 0.8, swipeSpeed: 300);

		// Small wait for any scroll momentum to settle
		Thread.Sleep(500);

		// Re-measure — X must not have changed (no horizontal scroll should have occurred)
		var rectAfter = App.WaitForElement("EmptyViewLabel").GetRect();

		Assert.That(rectAfter.X, Is.EqualTo(rectBefore.X),
			$"EmptyViewLabel X position changed from {rectBefore.X} to {rectAfter.X}. " +
			"CollectionView must NOT scroll horizontally when empty inside a RefreshView.");
	}
}
#endif
