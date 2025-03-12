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
		public void ValidateEmptyViewDisplayed_ItemSourceEmptyListSetFirst_AndEmptyViewString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyList");
			App.Tap("ItemsSourceEmptyList");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewStringSetFirst_AndItemSourceEmptyList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceEmptyList");
			App.Tap("ItemsSourceEmptyList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
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
		public void ValidateEmptyViewDisplayed_ItemSourceEmptyListSetFirst_AndCustomEmptyView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyList");
			App.Tap("ItemsSourceEmptyList");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyViewSetFirst_AndItemSourceEmptyList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceEmptyList");
			App.Tap("ItemsSourceEmptyList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
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
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndBasicItemTemplate()
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
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndBaicItemTemplate()
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

		// Emptyview template
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
		public void ValidateEmptyViewTemplateDisplayed_ItemSourceEmptyListSetFirst_AndCustomEmptyViewTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceEmptyList");
			App.Tap("ItemsSourceEmptyList");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewTemplateDisplayed_WhenCustomEmptyViewTemplateSetFirst_AndItemSourceEmptyList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewTemplateGrid");
			App.Tap("EmptyViewTemplateGrid");
			App.WaitForElement("ItemsSourceEmptyList");
			App.Tap("ItemsSourceEmptyList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Template Items Available(Grid View)");
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

#if TEST_FAILS_ON_ANDROID  
// Header and Footer not visibile on Android. Issue Link: https://github.com/dotnet/maui/issues/28350
// HeaderTemplate and FooterTemplate are not displayed when ItemsSource is initially set to null on Android. Issue Link: https://github.com/dotnet/maui/issues/28337
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
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndCustomViewFooterTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndCustomViewFooterTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenEmptyViewString_AndCustomViewHeaderTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewDisplayed_WhenCustomEmptyView_AndCustomViewHeaderTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("Header Template(Grid View)");
		}
#endif
	}
}
#endif