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
			App.Click("VerticalList");
			App.WaitForElement("TestCollectionView");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewHorizontalList()
		{
			App.Click("HorizontalList");
			App.WaitForElement("TestCollectionView");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewVerticalGrid()
		{
			App.Click("VerticalGrid");
			App.WaitForElement("TestCollectionView");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewHorizontalGrid()
		{
			App.Click("HorizontalGrid");
			App.WaitForElement("TestCollectionView");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
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
		public void RemoveItemsCollectionViewList()
		{
			App.Click("AddRemoveItemsList");
			App.WaitForElement("TestCollectionView");

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
			App.Click("AddRemoveItemsList");
			App.WaitForElement("TestCollectionView");

			// 1. With a snapshot we verify the CollectionView items.
			VerifyScreenshot("ReplaceItemsCollectionViewListBefore");

			// 2. Add a new Item in the Index 0
			App.EnterText("entryReplace", "0");
			App.Click("btnReplace");

			// 3. Check if the item has been added correctly.			
			VerifyScreenshot("ReplaceItemsCollectionViewListAfter");
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
		public void RemoveItemsCollectionViewGrid()
		{
			App.Click("AddRemoveItemsGrid");
			App.WaitForElement("TestCollectionView");

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
			App.Click("AddRemoveItemsGrid");
			App.WaitForElement("TestCollectionView");

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
			App.Click("EmptyViewString");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the String EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void FilterCollectionViewNoCrash()
		{
			App.Click("EmptyViewString");

			// 1. Filter the items with an existing term.
			App.EnterText("FilterSearchBar", "a");

			// 2. Without exceptions, the test has passed.
			Assert.NotNull(App.AppState);
		}

		[Test]
		public void RemoveStringEmptyView()
		{
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
			App.Click("EmptyViewView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the View EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void RemoveViewEmptyView()
		{
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
			App.Click("EmptyViewTemplateView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the Templated EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void RemoveTemplateViewEmptyView()
		{
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
			App.Click("PreselectedItem");

			// 1. Check the preselected item.
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}

		[Test]
		public void PreselectedItemsCollectionView()
		{
			App.Click("PreselectedItems");

			// 1. Check the preselected items.
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}

		[Test]
		public void ListGrouping()
		{
			App.ScrollTo("ListGrouping", true);
			App.Click("ListGrouping");
			App.WaitForElement("TestCollectionView");

			// 1. Check the grouped CollectionView layout.
			VerifyScreenshot();
		}

		[Test]
		public void GridGrouping()
		{
			App.ScrollTo("GridGrouping", true);
			App.Click("GridGrouping");
			App.WaitForElement("TestCollectionView");

			// 1. Check the grouped CollectionView layout.
			VerifyScreenshot();
		}

		[Test]
		public void StringHeaderFooter()
		{
			App.ScrollTo("HeaderFooterString", true);
			App.Click("HeaderFooterString");

			// 1. Check CollectionView header and footer using a string.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}

		[Test]
		public void ViewHeaderFooter()
		{
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
			App.ScrollTo("HeaderFooterTemplate", true);
			App.Click("HeaderFooterTemplate");

			// 1. Check CollectionView header and footer using a TemplatedView.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}
	}
}