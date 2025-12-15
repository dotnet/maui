using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33130 : _IssuesUITest
{
	public override string Issue => "CollectionView group header size changes with ItemSizingStrategy";

	public Issue33130(TestDevice device) : base(device) { }
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupHeaderSizeShouldNotChangeWithItemSizingStrategy()
	{
		// Wait for the CollectionView to load
		App.WaitForElement("TestCollectionView");
		App.WaitForElement("GroupHeader");

		// Get the initial header size (before changing ItemSizingStrategy)
		var headerElementBefore = App.FindElement("GroupHeader");
		var headerRectBefore = headerElementBefore.GetRect();

		Assert.That(headerRectBefore.Height, Is.GreaterThan(0), "Header should have a height before strategy change");

		// Switch ItemSizingStrategy
		App.WaitForElement("SwitchStrategyButton");
		App.Tap("SwitchStrategyButton");

		// Get the header size after changing ItemSizingStrategy
		var headerElementAfter = App.FindElement("GroupHeader");
		var headerRectAfter = headerElementAfter.GetRect();

		Assert.That(headerRectAfter.Height, Is.GreaterThan(0), "Header should have a height after strategy change");

		// The header size should remain the same (within a small tolerance for rendering differences)
		// Allow for small rounding differences but not significant changes
		var heightDifference = Math.Abs(headerRectBefore.Height - headerRectAfter.Height);

		// Assert that the height difference is minimal (less than 5 pixels tolerance)
		Assert.That(heightDifference, Is.EqualTo(0),
			$"Header height should not change significantly. Before: {headerRectBefore.Height}, After: {headerRectAfter.Height}, Difference: {heightDifference}");
	}
}