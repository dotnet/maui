using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30483 : _IssuesUITest
{
	public override string Issue => "[iOS] Flyout Menu CollectionView First Item Misaligned";

	public Issue30483(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutCollectionViewFirstItemShouldBeAlignedBelowHeader()
	{
		// Wait for flyout to open (auto-opens in test)
		App.WaitForElement("FirstItemLabel");

		// Get the positions of key elements
		var header = App.WaitForElement("FlyoutHeader");
		var firstItem = App.WaitForElement("FirstItem");
		var firstItemLabel = App.WaitForElement("FirstItemLabel");

		var headerRect = header.GetRect();
		var firstItemRect = firstItem.GetRect();
		var firstItemLabelRect = firstItemLabel.GetRect();

		var headerBottom = headerRect.Y + headerRect.Height;
		var firstItemTop = firstItemRect.Y;
		var gap = firstItemTop - headerBottom;

		// The first item should be positioned immediately after the header (within a small tolerance)
		// If there's a large gap (> 50 pixels), the bug is present
		Assert.That(gap, Is.LessThan(50),
			$"First CollectionView item is misaligned. Gap of {gap}px is too large (should be < 50px)");

		// Verify the item is visible and not pushed off-screen
		Assert.That(firstItemRect.Y, Is.GreaterThan(0),
			"First item should be visible on screen");
		Assert.That(firstItemLabelRect.Y, Is.GreaterThan(0),
			"First item label should be visible on screen");
	}
}
