using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewAddRemoveItemsGridUITests : CollectionViewUITests
	{
		public CollectionViewAddRemoveItemsGridUITests(TestDevice device) 
			: base(device)
		{
		}

		[Test]
		public async Task AddItemsCollectionViewGrid()
		{
			App.Click("AddRemoveItemsGrid");
			App.WaitForElement("TestCollectionView");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("AddItemsCollectionViewGridBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryInsert", "0");
			App.Click("btnInsert");

			// Wait for the animation to finish when adding item
			await Task.Delay(1000);

			// 3. Check if the item has been added correctly.
			VerifyScreenshot("AddItemsCollectionViewGridAfter");
		}

		[Test]
		public async Task RemoveItemsCollectionViewGrid()
		{
			App.Click("AddRemoveItemsGrid");
			App.WaitForElement("TestCollectionView");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("RemoveItemsCollectionViewGridBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryRemove", "0");
			App.Click("btnRemove");

			// Wait for the animation to finish when removing item
			await Task.Delay(1000);

			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("RemoveItemsCollectionViewGridAfter");
		}


		[Test]
		public async Task ReplaceItemsCollectionViewGrid()
		{
			App.Click("AddRemoveItemsGrid");
			App.WaitForElement("TestCollectionView");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("ReplaceItemsCollectionViewGridBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryReplace", "0");
			App.Click("btnReplace");

			// Wait for the animation to finish when replacing items
			await Task.Delay(1000);

			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("ReplaceItemsCollectionViewGridAfter");
		}
	}
}