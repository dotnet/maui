#if TEST_FAILS_ON_WINDOWS // EmptyView Elements not accessible via Automation on Windows, Issue Link: https://github.com/dotnet/maui/issues/28022
// EmptyViewTemplate not displayed when ItemsSource is initially set to a list and then set to Null Issue Link: https://github.com/dotnet/maui/issues/28334
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class CollectionView_EmptyViewFeatureTests : UITest
	{
		public const string CollectionViewFeatureMatrix = "CollectionView Feature Matrix";

		public CollectionView_EmptyViewFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(CollectionViewFeatureMatrix);
		}


		[Test, Order(1)]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString()
		{
			App.WaitForElement("EmptyViewButton");
			App.Tap("EmptyViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceEmptyObservableCollectionSetFirst_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewStringSetFirst_AndEmptyObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceObservableCollectionSetFirst_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewStringSetFirst_AndObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceEmptyGroupedListSetFirst_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewStringSetFirst_AndItemSourceEmptyGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}
	 
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceGroupedListSetFirst_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewStringSetFirst_AndItemSourceGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceEmptyObservableCollectionSetFirst_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyViewSetFirst_AndItemSourceEmptyObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceObservableCollectionSetFirst_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyViewSetFirst_AndItemSourceObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceEmptyGroupedListSetFirst_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyViewSetFirst_AndItemSourceEmptyGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_ItemSourceGroupedListSetFirst_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyViewSetFirst_AndItemSourceGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenBasicItemTemplate_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenBaicItemTemplate_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenCustomView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_ItemSourceEmptyObservableCollectionSetFirst_AndCustomEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenCustomEmptyViewTemplateSetFirst_AndItemSourceEmptyObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_ItemSourceObservableCollectionSetFirst_AndCustomEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenCustomEmptyViewTemplateSetFirst_AndItemSourceObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_ItemSourceEmptyGroupedListSetFirst_AndCustomEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenCustomEmptyViewTemplateSetFirst_AndItemSourceEmptyGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_ItemSourceGroupedListSetFirst_AndCustomEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenCustomEmptyViewTemplateSetFirst_AndItemSourceGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenEmptyViewTemplate_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenEmptyViewTemplate_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenCustomEmptyViewTemplate_AndBaicItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenBaicItemTemplate_CustomEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WhenItemsLayout_Is_VerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}
        


		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WhenItemsLayout_Is_HorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WhenItemsLayout_Is_VerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WhenItemsLayout_Is_HorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewString_WhenItemsLayout_Is_VerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewString_WhenItemsLayout_Is_HorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewString_WhenItemsLayout_Is_VerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyView_WhenItemsLayout_Is_HorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewSize()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			var emptyViewBounds = App.WaitForElement("TestLabel").GetRect();
			Assert.That(emptyViewBounds, Is.Not.Null);
			Assert.That(emptyViewBounds.Height, Is.EqualTo(200).Within(5));
			Assert.That(emptyViewBounds.Width, Is.EqualTo(300).Within(5));
		}

#if TEST_FAILS_ON_ANDROID
// CollectionView Footer Positioned Incorrectly on Android. Issue Link: https://github.com/dotnet/maui/issues/28350
// HeaderTemplate and FooterTemplate are not displayed when ItemsSource is initially set to null on Android. Issue Link: https://github.com/dotnet/maui/issues/28337
// Header and footer are not displayed when emptyview selected first Issue Link: https://github.com/dotnet/maui/issues/28351
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndFooterString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndCustomViewFooter()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndFooterString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndCustomViewFooter()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndHeaderString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndCustomViewHeader()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndHeaderString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndCustomViewHeader()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
		}
#endif
	}
}
#endif