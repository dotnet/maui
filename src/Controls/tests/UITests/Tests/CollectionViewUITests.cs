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

		[Test]
		public void StringEmptyViewAfterFilter()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("EmptyViewString");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the String EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void FilterCollectionViewNoCrash()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("EmptyViewString");

			// 1. Filter the items with an existing term.
			App.EnterText("FilterSearchBar", "a");

			// 2. Without exceptions, the test has passed.
			Assert.NotNull(App.AppState);
		}

		[Test]
		public void RemoveStringEmptyView()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("EmptyViewString");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Clear filter .
			App.EnterText("FilterSearchBar", "");

			// 3. Check if the CollectionView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void ViewEmptyViewAfterFilter()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("EmptyViewView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the View EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void RemoveViewEmptyView()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("EmptyViewView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Clear filter .
			App.EnterText("FilterSearchBar", "");

			// 3. Check if the CollectionView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void TemplateEmptyViewAfterFilter()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("EmptyViewTemplateView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the Templated EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void RemoveTemplateViewEmptyView()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("EmptyViewTemplateView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Clear filter .
			App.EnterText("FilterSearchBar", "");

			// 3. Check if the CollectionView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void PreselectedItemCollectionView()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("PreselectedItem");

			// 1. Check the preselected item.
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}

		[Test]
		public void PreselectedItemsCollectionView()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("PreselectedItems");

			// 1. Check the preselected items.
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}

		[Test]
		public void ListGrouping()
		{
			App.WaitForElement("WaitForStubControl");
			App.ScrollTo("ListGrouping", true);
			App.Click("ListGrouping");

			// 1. Check the grouped CollectionView layout.
			VerifyScreenshot();
		}

		[Test]
		public void GridGrouping()
		{
			App.WaitForElement("WaitForStubControl");
			App.ScrollTo("GridGrouping", true);
			App.Click("GridGrouping");

			// 1. Check the grouped CollectionView layout.
			VerifyScreenshot();
		}

		[Test]
		public void StringHeaderFooter()
		{
			App.WaitForElement("WaitForStubControl");
			App.ScrollTo("HeaderFooterString", true);
			App.Click("HeaderFooterString");

			// 1. Check CollectionView header and footer using a string.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}

		[Test]
		public void ViewHeaderFooter()
		{
			App.WaitForElement("WaitForStubControl");
			App.ScrollTo("HeaderFooterView", true);
			App.Click("HeaderFooterView");

			// 1. Both Header and Footer must be visible with or without items.
			// Let's add items.
			App.Click("AddButton");

			// 2. Clear the items.
			App.Click("ClearButton");

			// 3. Repeat the previous steps.
			App.Click("AddButton");
			App.Click("ClearButton");

			// 3. Check CollectionView header and footer using a View.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}

		[Test]
		public void TemplateHeaderFooter()
		{
			App.WaitForElement("WaitForStubControl");
			App.ScrollTo("HeaderFooterTemplate", true);
			App.Click("HeaderFooterTemplate");

			// 1. Check CollectionView header and footer using a TemplatedView.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}
	}
}
