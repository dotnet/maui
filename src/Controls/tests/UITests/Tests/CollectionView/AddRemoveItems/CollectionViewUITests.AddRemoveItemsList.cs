using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewAddRemoveItemsListUITests : CollectionViewUITests
	{
		public CollectionViewAddRemoveItemsListUITests(TestDevice device)
			: base(device)
		{
		}
				
		[Test]
		[Description("Insert new items in the List works")]
		public async Task AddItemsCollectionViewList()
		{
			App.Click("AddRemoveItemsList");
			App.WaitForElement("TestCollectionView");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("AddItemsCollectionViewListBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryInsert", "0");
			App.Click("btnInsert");

			// Wait for the animation to finish when adding item
			await Task.Delay(1000);

			// 3. Check if the item has been added correctly.
			VerifyScreenshot("AddItemsCollectionViewListAfter");
		}

		[Test]
		[Description("Remove an existing item from the List works")]
		public async Task RemoveItemsCollectionViewList()
		{
			App.Click("AddRemoveItemsList");
			App.WaitForElement("TestCollectionView");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("RemoveItemsCollectionViewListBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryRemove", "0");
			App.Click("btnRemove");

			// Wait for the animation to finish when remove item
			await Task.Delay(1000);

			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("RemoveItemsCollectionViewListAfter");
		}

		[Test]
		[Description("Replace an existing item from the List works")]
		public async Task ReplaceItemsCollectionViewList()
		{
			App.Click("AddRemoveItemsList");
			App.WaitForElement("TestCollectionView");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("ReplaceItemsCollectionViewListBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryReplace", "0");
			App.Click("btnReplace");

			// Wait for the animation to finish when replacing items
			await Task.Delay(1000);

			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("ReplaceItemsCollectionViewListAfter");
		}
	}
}