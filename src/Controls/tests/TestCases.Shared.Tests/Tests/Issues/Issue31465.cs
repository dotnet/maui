using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31465 : _IssuesUITest
{
	public Issue31465(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "CollectionView with null ItemsSource and Header can be dragged down creating extra space";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewWithNullBindingContextShouldDisplayHeaderAndEmptyViewCorrectly()
	{
		// Wait for the page to load
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("HeaderLabel");
		App.WaitForElement("EmptyViewLabel");

		// Verify the header text is displayed
		var headerLabel = App.FindElement("HeaderLabel");
		Assert.That(headerLabel.GetText(), Is.EqualTo("Header: This should show for all cases."));

		// Verify the empty view text is displayed
		var emptyViewLabel = App.FindElement("EmptyViewLabel");
		Assert.That(emptyViewLabel.GetText(), Is.EqualTo("EmptyView: This should show when no data."));

		// Test passes if both header and empty view are visible and properly positioned
		// The fix should prevent extra space from appearing between them when dragging
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewShouldNotBeDraggableWhenEmpty()
	{
		// Wait for the page to load
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("HeaderLabel");
		App.WaitForElement("EmptyViewLabel");

		var collectionView = App.FindElement("TestCollectionView");
		var headerLabel = App.FindElement("HeaderLabel");
		var emptyViewLabel = App.FindElement("EmptyViewLabel");

		// Get initial positions of header and empty view
		var initialHeaderRect = headerLabel.GetRect();
		var initialEmptyViewRect = emptyViewLabel.GetRect();

		// Verify initial state - both elements should be visible with correct text
		Assert.That(headerLabel.GetText(), Is.EqualTo("Header: This should show for all cases."));
		Assert.That(emptyViewLabel.GetText(), Is.EqualTo("EmptyView: This should show when no data."));

		// Perform a scroll down gesture on the CollectionView
		// This attempts to reproduce the issue where users could drag down and create space
		var collectionViewRect = collectionView.GetRect();
		var startX = collectionViewRect.X + collectionViewRect.Width / 2;
		var startY = collectionViewRect.Y + 50; // Start near the top
		var endY = startY + 200; // Drag down significantly

		// Perform the drag gesture
		App.DragCoordinates(startX, startY, startX, endY);

		// Wait a moment for any potential animation/layout changes
		Thread.Sleep(500);

		// Verify elements are still properly positioned and haven't been separated
		App.WaitForElement("HeaderLabel");
		App.WaitForElement("EmptyViewLabel");

		var afterDragHeaderLabel = App.FindElement("HeaderLabel");
		var afterDragEmptyViewLabel = App.FindElement("EmptyViewLabel");

		// Verify text content is still correct
		Assert.That(afterDragHeaderLabel.GetText(), Is.EqualTo("Header: This should show for all cases."));
		Assert.That(afterDragEmptyViewLabel.GetText(), Is.EqualTo("EmptyView: This should show when no data."));

		// Get positions after drag attempt
		var afterDragHeaderRect = afterDragHeaderLabel.GetRect();
		var afterDragEmptyViewRect = afterDragEmptyViewLabel.GetRect();

		// The key test: positions should remain stable (no extra space created)
		// Allow for minor variations due to platform differences, but positions should be essentially the same
		var headerYDifference = Math.Abs(afterDragHeaderRect.Y - initialHeaderRect.Y);
		var emptyViewYDifference = Math.Abs(afterDragEmptyViewRect.Y - initialEmptyViewRect.Y);

		// Positions should not have changed significantly (tolerance for floating point precision)
		Assert.That(headerYDifference, Is.LessThan(5),
			$"Header moved too much: initial Y={initialHeaderRect.Y}, after drag Y={afterDragHeaderRect.Y}");
		Assert.That(emptyViewYDifference, Is.LessThan(5),
			$"EmptyView moved too much: initial Y={initialEmptyViewRect.Y}, after drag Y={afterDragEmptyViewRect.Y}");

		// Additional verification: EmptyView should still be positioned directly after Header
		// (accounting for any margins/padding)
		var spaceBetween = afterDragEmptyViewRect.Y - (afterDragHeaderRect.Y + afterDragHeaderRect.Height);
		Assert.That(spaceBetween, Is.LessThan(50),
			$"Too much space between Header and EmptyView: {spaceBetween}px");
	}
}