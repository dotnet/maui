using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32578 : _IssuesUITest
	{
		public Issue32578(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Grouped CollectionView doesn't size correctly when ItemSizingStrategy=MeasureFirstItem";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void GroupedCollectionViewMeasureFirstItemShouldSizeCorrectly()
		{
			// Wait for the CollectionView to be loaded
			App.WaitForElement("TestCollection");

			// Wait a moment for items to render
			Task.Delay(2000).Wait();

			// Get all items with AutomationId "ItemImage" to check their heights
			var items = App.FindElements("ItemImage").ToList();
			
			Assert.That(items.Count, Is.GreaterThanOrEqualTo(2), "Should have at least 2 items");
			
			// The bug: with MeasureFirstItem on grouped CollectionView, 
			// the first item measured is the group header, and its size gets
			// incorrectly applied to all data items.
			// 
			// After the fix: data items should all have similar heights (the first data item's size)
			// and should NOT be constrained to the group header's height.
			
			var firstItemRect = items[0].GetRect();
			var secondItemRect = items[1].GetRect();
			
			// Both items should have similar heights (they're using the same template)
			// Tolerance of 10 pixels to account for platform differences
			Assert.That(Math.Abs(firstItemRect.Height - secondItemRect.Height), Is.LessThan(10), 
				$"First and second items should have similar heights when using MeasureFirstItem. " +
				$"First: {firstItemRect.Height}, Second: {secondItemRect.Height}");
			
			// Items should have reasonable height (at least 40dp which is ~120px at 3x density)
			// The actual template specifies 60dp row + 10dp padding = ~240px at 3x
			Assert.That(firstItemRect.Height, Is.GreaterThan(100), 
				$"Items should have proper height based on first data item, not compressed. Height: {firstItemRect.Height}");
			
			// Verify all items in first group have the same height
			if (items.Count >= 2)
			{
				for (int i = 1; i < Math.Min(items.Count, 2); i++)
				{
					var itemRect = items[i].GetRect();
					Assert.That(Math.Abs(itemRect.Height - firstItemRect.Height), Is.LessThan(10),
						$"Item {i} should have same height as first item. Expected: {firstItemRect.Height}, Actual: {itemRect.Height}");
				}
			}
		}
	}
}
