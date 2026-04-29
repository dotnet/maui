#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19866 : _IssuesUITest
{
	public Issue19866(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "CollectionView does not scroll to top on iOS status bar tap";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void StatusBarTapScrollsCollectionViewToTop()
	{
		// Verify first item is visible initially
		App.WaitForElement("Item 0");

		// Scroll down multiple times to move well past the first item
		App.ScrollDown("TestCollectionView", ScrollStrategy.Gesture);
		App.ScrollDown("TestCollectionView", ScrollStrategy.Gesture);
		App.ScrollDown("TestCollectionView", ScrollStrategy.Gesture);

		// First item should no longer be visible after scrolling
		App.WaitForNoElement("Item 0");

		// Tap the status bar area to trigger iOS scroll-to-top
		var rect = App.WaitForElement("TestCollectionView").GetRect();
		App.TapCoordinates(rect.X + (rect.Width / 2), 5);

		// Verify first item is visible again after scroll-to-top
		App.WaitForElement("Item 0", timeout: TimeSpan.FromSeconds(5));
	}
}
#endif
