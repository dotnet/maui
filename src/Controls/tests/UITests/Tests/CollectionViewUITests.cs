using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	class CollectionViewUITests : UITest
	{
		const string CarouselViewGallery = "CollectionView Gallery";

		public CollectionViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(CarouselViewGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
		public void CollectionViewVerticalList()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("VerticalList");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewHorizontalList()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("HorizontalList");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewVerticalGrid()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("VerticalGrid");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewHorizontalGrid()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("HorizontalGrid");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void AddItemsCollectionViewList()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("AddRemoveItemsList");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("AddItemsCollectionViewListBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryInsert", "0");
			App.Click("btnInsert");

			// 3. Check if the item has been added correctly.
			VerifyScreenshot("AddItemsCollectionViewListAfter");
		}

		[Test]
		public void RemoveItemsCollectionViewList()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("AddRemoveItemsList");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("RemoveItemsCollectionViewListBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryRemove", "0");
			App.Click("btnRemove");  
			
			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("RemoveItemsCollectionViewListAfter");
		}

		[Test]
		public void ReplaceItemsCollectionViewList()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("AddRemoveItemsList");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("ReplaceItemsCollectionViewListBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryReplace", "0");
			App.Click("btnReplace");   
			
			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("ReplaceItemsCollectionViewListAfter");
		}

		[Test]
		public void AddItemsCollectionViewGrid()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("AddRemoveItemsGrid");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("AddItemsCollectionViewGridBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryInsert", "0");
			App.Click("btnInsert");

			// 3. Check if the item has been added correctly.
			VerifyScreenshot("AddItemsCollectionViewGridAfter");
		}

		[Test]
		public void RemoveItemsCollectionViewGrid()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("AddRemoveItemsGrid");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("RemoveItemsCollectionViewGridBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryRemove", "0");
			App.Click("btnRemove");

			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("RemoveItemsCollectionViewGridAfter");
		}


		[Test]
		public void ReplaceItemsCollectionViewGrid()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("AddRemoveItemsGrid");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("ReplaceItemsCollectionViewGridBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryReplace", "0");
			App.Click("btnReplace");

			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("ReplaceItemsCollectionViewGridAfter");
		}
	}
}
