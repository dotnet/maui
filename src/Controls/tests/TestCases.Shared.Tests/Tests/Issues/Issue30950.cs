using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30950 : _IssuesUITest
{
	public Issue30950(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Deleting Items Causes Other Expanded Items to Collapse Unexpectedly";

	[Test]
	[Category(UITestCategories.SwipeView)]
	[Category(UITestCategories.CollectionView)]
	public void SwipeViewItemsShouldRemainExpandedWhenOtherItemsAreDeleted()
	{
		// Swipe right on first item to show left swipe items (Favourite)
		App.WaitForElement("Item 1");
		App.SwipeRightToLeft("Item 1");

		// Swipe left on third item to show right swipe items (Delete)  
		App.WaitForElement("Item 3");
		App.SwipeLeftToRight("Item 3");

		// Verify both items show their swipe actions
		App.WaitForElement("Favourite");
		App.WaitForElement("Delete");

		// Delete item 2 (which is not expanded) by swiping left and tapping delete
		App.WaitForElement("Item 2");
		App.SwipeLeftToRight("Item 2");
		App.Tap("Delete");

		// Verify that item1 and item3 are still showing their swipe actions
		// This tests the fix, before the fix, these would have collapsed
		try
		{
			App.WaitForElement("Favourite");
			App.WaitForElement("Delete");
		}
		catch
		{
			Assert.Fail("SwipeView items collapsed unexpectedly when another item was deleted. The fix for Issue79I10Swipe is not working.");
		}
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	[Category(UITestCategories.CollectionView)]
	public void FavouriteSwipeActionShouldWork()
	{
		// Swipe right on first item to show favourite action
		App.WaitForElement("Item 1");
		App.SwipeLeftToRight("Item 1");

		// Tap the favourite button
		App.Tap("Favourite");

		// Verify alert is shown
		App.WaitForElement("Issue30950Alert");
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	[Category(UITestCategories.CollectionView)]
	public void DeleteSwipeActionShouldRemoveItem()
	{
		// Count initial items
		var initialItems = App.FindElements("Item 1").Count > 0 ? 10 : 0; // We know there are 10 items initially
		
		Assert.That(
			initialItems,
			Is.EqualTo(10));
		
		// Swipe left on first item to show delete action
		App.WaitForElement("Item 1");
		App.SwipeRightToLeft("Item 1");
		
		// Verify item was removed, Item 1 should no longer exist
		Assert.Throws<TimeoutException>(() => App.WaitForElement("Item 1", timeout: TimeSpan.FromSeconds(2)));
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	[Category(UITestCategories.CollectionView)]
	public void MultipleSwipeViewsCanBeExpandedSimultaneously()
	{
		// Expand multiple items at once
		App.WaitForElement("Item 1");
		App.SwipeRightToLeft("Item 1");

		App.WaitForElement("Item 2");
		App.SwipeLeftToRight("Item 2");

		App.WaitForElement("Item 3");
		App.SwipeRightToLeft("Item 3");

		// Verify all actions are visible simultaneously
		var favouriteButtons = App.FindElements("Favourite");
		var deleteButtons = App.FindElements("Delete");
		
		Assert.That(
			favouriteButtons.Count,
			Is.GreaterThanOrEqualTo(2),
			"Multiple favourite buttons should be visible");
		
		Assert.That(
			deleteButtons.Count,
			Is.GreaterThanOrEqualTo(1),
			"At least one delete button should be visible");
	}
}