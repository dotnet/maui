#if TEST_FAILS_ON_WINDOWS // EmptyView Elements not accessible via Automation on Windows, Issue Link: https://github.com/dotnet/maui/issues/28022
// EmptyViewTemplate not displayed on Windows Issue Link: https://github.com/dotnet/maui/issues/28334
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
		public void ValidateEmptyViewStringDisplayed()
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
		public void ValidateCustomEmptyViewDisplayed()
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
		public void ValidateCustomEmptyViewSizeDisplayed()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewStringDisplayed_AndItemSourceEmptyObservableCollectionSetFirst()
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
		public void ValidateEmptyViewStringSetFirst_AndEmptyObservableCollection()
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
		public void ValidateEmptyViewStringDisplayed_AndObservableCollectionSetFirst()
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
		public void ValidateEmptyViewStringSetFirst_AndObservableCollection()
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
		public void ValidateEmptyViewStringDisplayed_AndEmptyGroupedListSetFirst()
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
		public void ValidateEmptyViewStringDisplayedSetFirst_AndItemSourceEmptyGroupedList()
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
		public void ValidateEmptyViewStringDisplayed_GroupedListSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewStringDisplayedSetFirst_AndGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewDisplayed_AndEmptyObservableCollectionSetFirst()
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
		public void ValidateCustomEmptyViewDisplayedSetFirst_AndEmptyObservableCollection()
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
		public void ValidateCustomEmptyViewDisplayed_AndObservableCollectionSetFirst()
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
		public void ValidateCustomEmptyViewDisplayedSetFirst_AndObservableCollection()
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
		public void ValidateCustomEmptyViewDisplayed_EmptyGroupedListSetFirst()
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
		public void ValidateCustomEmptyViewDisplayedSetFirst_AndEmptyGroupedList()
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
		public void ValidateCustomEmptyViewDisplayed_GroupedListSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewDisplayedSetFirst_AndGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_AndEmptyObservableCollectionSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayedSetFirst_AndEmptyObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_AndObservableCollectionSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayedSetFirst_AndObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_EmptyGroupedListSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayedSetFirst_AndEmptyGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_GroupedListSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayedSetFirst_AndGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewStringDisplayed_AndEmptyViewTemplate()
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
		public void ValidateCustomEmptyViewDisplayed_AndEmptyViewTemplate()
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
		public void ValidateCustomEmptyViewSizeDisplayed_AndEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_AndCustomEmptyViewTemplateSize()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewStringDisplayed_AndBasicItemTemplateSetFirst()
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
		public void ValidateEmptyViewStringDisplayedSetFirst_AndBasicItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewDisplayed_WhenBasicItemTemplateSetFirst()
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
		public void ValidateCustomEmptyViewDisplayedSetFirst_WhenBasicItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_WhenBasicItemTemplateSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayedSetFirst_WhenBasicItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed()
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
		public void ValidateCustomSizeEmptyViewTemplateDisplayed()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_EmptyObservableCollectionSetFirst()
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
		public void ValidateEmptyViewTemplateDisplayedSetFirst_AndEmptyObservableCollection()
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
		public void ValidateEmptyViewTemplateDisplayed_AndeObservableCollectionSetFirst()
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
		public void ValidateEmptyViewTemplateDisplayedSetFirst_AndObservableCollection()
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
		public void ValidateEmptyViewTemplateDisplayed_AndEmptyGroupedListSetFirst()
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
		public void ValidateEmptyViewTemplateDisplayedSetFirst_AndEmptyGroupedList()
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
		public void ValidateEmptyViewTemplateDisplayed_AndGroupedListSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayedSetFirst_AndGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_EmptyObservableCollectionSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayedSetFirst_AndEmptyObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsSourceEmptyObservableCollection");
			App.Tap("ItemsSourceEmptyObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_AndeObservableCollectionSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayedSetFirst_AndObservableCollection()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsSourceObservableCollection");
			App.Tap("ItemsSourceObservableCollection");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_AndEmptyGroupedListSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayedSetFirst_AndEmptyGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsSourceEmptyGroupedList");
			App.Tap("ItemsSourceEmptyGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_AndGroupedListSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayedSetFirst_AndGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_AndEmptyViewString()
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
		public void ValidateEmptyViewTemplateDisplayed_AndCustomEmptyView()
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
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_AndCustomSizeEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayedSetFirst_AndBasicItemTemplate()
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
		public void ValidateEmptyViewTemplateDisplayed_AndBasicTemplateSetFirst()
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
		public void ValidateCustomSizeEmptyViewTemplateDisplayedSetFirst_AndBasicItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplateDisplayed_AndBasicTemplateSetFirst()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
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
			var emptyViewBounds = App.WaitForElement("EmptyViewLabel").GetRect();
			Assert.That(emptyViewBounds, Is.Not.Null);
#if ANDROID
			int expectedWidth = 788;
			int expectedHeight = 525;
#elif MACCATALYST
			int expectedWidth = 235;
			int expectedHeight = 150; 
#else
			int expectedWidth = 300;
			int expectedHeight = 200;
#endif
			Assert.That(emptyViewBounds.Width, Is.EqualTo(expectedWidth).Within(5));
			Assert.That(emptyViewBounds.Height, Is.EqualTo(expectedHeight).Within(5));
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
		//When setting HeightRequest and WidthRequest in the EmptyViewTemplate, as the values were not applied, preventing proper resizing. Issue Link: https://github.com/dotnet/maui/issues/28605
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateSize()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			var emptyViewBounds = App.WaitForElement("EmptyViewTemplateLabel").GetRect();
#if ANDROID
			int expectedWidth = 788;
			int expectedHeight = 525;
#elif MACCATALYST
			int expectedWidth = 235;
			int expectedHeight = 150; 
#else
			int expectedWidth = 300;
			int expectedHeight = 200;
#endif
			Assert.That(emptyViewBounds.Width, Is.EqualTo(expectedWidth).Within(5));
			Assert.That(emptyViewBounds.Height, Is.EqualTo(expectedHeight).Within(5));
		}
#endif

#if TEST_FAILS_ON_ANDROID
// CollectionView Footer Becomes Scrollable When EmptyView is Active on Android. Issue Link: https://github.com/dotnet/maui/issues/28350
// HeaderTemplate and FooterTemplate are not displayed when ItemsSource is initially set to null on Android. Issue Link: https://github.com/dotnet/maui/issues/28337
// Header and footer are not displayed when emptyview selected first Issue Link: https://github.com/dotnet/maui/issues/28351		
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewStringDisplayed_AndHeaderString()
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
		public void ValidateEmptyViewStringDisplayed_AndCustomViewHeader()
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
		public void ValidateCustomEmptyViewSizeDisplayed_AndHeaderString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_AndCustomViewHeader()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
//Footer Not Displayed at the Bottom When EmptyView is Active in CV2 Issue Link: https://github.com/dotnet/maui/issues/28604
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewStringDisplayed_AndFooterString()
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
		public void ValidateEmptyViewStringDisplayed_AndCustomViewFooter()
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
		public void ValidateCustomEmptyViewDisplayed_AndFooterString()
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
		public void ValidateCustomEmptyViewDisplayed_AndCustomViewFooter()
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
		public void ValidateCustomEmptyViewSizeDisplayed_AndFooterString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewSizeDisplayed_AndCustomViewFooter()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

//In EmptyView, without explicitly setting HeightRequest and WidthRequest values, the Header and Footer do not appear Issue Link: https://github.com/dotnet/maui/issues/28605
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyViewDisplayed_AndHeaderString()
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
		public void ValidateCustomEmptyViewDisplayed_AndCustomViewHeader()
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
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
//CollectionView Header and Footer Do Not Align with Horizontal ItemsLayout When EmptyView is Displayed on https://github.com/dotnet/maui/issues/28622
//ItemsLayout does not change its default value on windows Issue Link: https://github.com/dotnet/maui/issues/27946
//Footer Not Displayed at the Bottom When EmptyView is Active in CV2 Issue Link: https://github.com/dotnet/maui/issues/28604
//TargetInvocationException Occurs When Selecting Header/Footer After Changing ItemsLayout in CV2 Issue Link : https://github.com/dotnet/maui/issues/28678
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WithHeaderFooterString_WhenVerticalList()
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
		public void ValidateEmptyViewString_WithHeaderFooterString_WhenHorizontalList()
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
		public void ValidateEmptyViewString_WithHeaderFooterString_WhenVerticalGrid()
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
		public void ValidateEmptyViewString_WithHeaderFooterString_WhenHorizontalGrid()
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
		public void ValidateCustomEmptyView_WithHeaderFooterString_WhenVerticalList()
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
		public void ValidateCustomEmptyView_WithHeaderFooterString_WhenHorizontalList()
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
		public void ValidateCustomEmptyView_WithHeaderFooterString_WhenVerticalGrid()
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
		public void ValidateCustomEmptyView_WithHeaderFooterString_WhenHorizontalGrid()
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
		public void ValidateCustomSizeEmptyView_WithHeaderFooterString_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyView_WithHeaderFooterString_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyView_WithHeaderFooterString_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyView_WithHeaderFooterString_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterString_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterString_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterString_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterString_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterString_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterString_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterString_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterString_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WithHeaderFooterView_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WithHeaderFooterView_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WithHeaderFooterView_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewString_WithHeaderFooterView_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyView_WithHeaderFooterView_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyView_WithHeaderFooterView_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyView_WithHeaderFooterView_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomEmptyView_WithHeaderFooterView_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyView_WithHeaderFooterView_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyView_WithHeaderFooterView_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyView_WithHeaderFooterView_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyView_WithHeaderFooterView_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom Empty View (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterView_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterView_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterView_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplate_WithHeaderFooterView_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterView_WhenVerticalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutVerticalList");
			App.Tap("ItemsLayoutVerticalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterView_WhenHorizontalList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterView_WhenVerticalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateCustomSizeEmptyViewTemplate_WithHeaderFooterView_WhenHorizontalGrid()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateCustomSize");
			App.Tap("EmptyViewTemplateCustomSize");
			App.WaitForElement("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Custom EmptyViewTemplate (Sized)");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}
#endif
	}
}
#endif