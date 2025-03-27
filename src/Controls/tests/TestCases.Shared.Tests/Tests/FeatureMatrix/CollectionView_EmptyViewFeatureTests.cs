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
		public void ValidateEmptyViewstringSetFirst_AndEmptyObservableCollection()
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
		public void ValidateECustomEmptyViewDisplayedSetFirst_AndEmptyObservableCollection()
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
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Items Available(Grid View)");
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
		public void ValidateCustomEmptyViewDisplayed_WhenBaicItemTemplateSetFirst()
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
		public void ValidateCustomEmptyViewDisplayedSetFirst_WhenBaicItemTemplate()
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
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("No Template Items Available(Grid View)");
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
		public void ValidateEmptyViewTemplateDisplayedSetFirst_AndBaicItemTemplate()
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
		public void ValidateEmptyViewSize()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewCustomSize");
			App.Tap("EmptyViewCustomSize");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			var emptyViewBounds = App.WaitForElement("EmptyViewLabel").GetRect();
			 
			double scaleFactorWidth = emptyViewBounds.Width / 300.0;
			double scaleFactorHeight = emptyViewBounds.Height / 200.0;
			double expectedWidth = 300 * scaleFactorWidth;
			double expectedHeight = 200 * scaleFactorHeight;
			Assert.That(emptyViewBounds, Is.Not.Null);
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
			double scaleFactorWidth = emptyViewBounds.Width / 300.0;
			double scaleFactorHeight = emptyViewBounds.Height / 200.0;
			double expectedWidth = 300 * scaleFactorWidth;
			double expectedHeight = 200 * scaleFactorHeight;
			Assert.That(emptyViewBounds, Is.Not.Null);
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

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS
////Footer Not Displayed at the Bottom When EmptyView is Active in CV2 Issue Link: https://github.com/dotnet/maui/issues/28604
        [Test]
		[Category(UITestCategories.CollectionView)]
		public void ValidateEmptyViewstringDisplayed_AndFooterString()
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
//TargetInvocationException occurs on cv2 when selecting hroizontal list.
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
		public void ValidateCustomEmptyView_WhenItemsLayout_Is_VerticalGrid()
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
		public void ValidateEmptyViewTemplate_WhenItemsLayout_Is_VerticalList()
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
		public void ValidateEmptyViewTemplate_WhenItemsLayout_Is_HorizontalList()
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
		public void ValidateEmptyViewTemplate_WhenItemsLayout_Is_VerticalGrid()
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
		public void ValidateEmptyViewTemplate_WhenItemsLayout_Is_HorizontalGrid()
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
#endif
	}
}
#endif