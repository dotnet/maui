using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class CollectionViewGroupHeaderItemSizingIssue : _IssuesUITest
{
	public override string Issue => "CollectionView group header size changes with ItemSizingStrategy";

	public CollectionViewGroupHeaderItemSizingIssue(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupHeaderSizeShouldNotChangeWithItemSizingStrategy()
	{
		// Wait for the CollectionView to load
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("GroupHeader");

		// Give time for layout
		Task.Delay(1000).Wait();

		// Get the initial header size (before changing ItemSizingStrategy)
		var headerElementBefore = App.FindElement("GroupHeader");
		var headerRectBefore = headerElementBefore.GetRect();
		
		Console.WriteLine($"Header size BEFORE strategy change: Width={headerRectBefore.Width}, Height={headerRectBefore.Height}");
		Assert.That(headerRectBefore.Height, Is.GreaterThan(0), "Header should have a height before strategy change");

		// Switch ItemSizingStrategy
		App.WaitForElement("SwitchStrategyButton");
		App.Tap("SwitchStrategyButton");

		// Wait for layout to update
		Task.Delay(1000).Wait();

		// Get the header size after changing ItemSizingStrategy
		var headerElementAfter = App.FindElement("GroupHeader");
		var headerRectAfter = headerElementAfter.GetRect();
		
		Console.WriteLine($"Header size AFTER strategy change: Width={headerRectAfter.Width}, Height={headerRectAfter.Height}");
		Assert.That(headerRectAfter.Height, Is.GreaterThan(0), "Header should have a height after strategy change");

		// The header size should remain the same (within a small tolerance for rendering differences)
		// Allow for small rounding differences but not significant changes
		var heightDifference = Math.Abs(headerRectBefore.Height - headerRectAfter.Height);
		Console.WriteLine($"Height difference: {heightDifference}");
		
		// Assert that the height difference is minimal (less than 5 pixels tolerance)
		Assert.That(heightDifference, Is.LessThan(5.0), 
			$"Header height should not change significantly. Before: {headerRectBefore.Height}, After: {headerRectAfter.Height}, Difference: {heightDifference}");
	}
}
