using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class CollectionView_HeaderFooterFeatureTests : UITest
	{
		public const string HeaderFooterFeatureMatrix = "CollectionView Feature Matrix";

		public CollectionView_HeaderFooterFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(HeaderFooterFeatureMatrix);
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2, unintended synchronization between the HeaderTemplate/FooterTemplate and Header/Footer views, related issue: https://github.com/dotnet/maui/issues/28504
		[Test, Order(1)]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithItemsSourceObservableCollection25()
		{
			App.WaitForElementTillPageNavigationSettled("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceObservableCollection25");
			App.Tap("ItemsSourceObservableCollection25");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Pepper");
			App.WaitForNoElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithItemsSourceObservableCollection5()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithItemsSourceNone()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithItemsSourceObservableCollection25()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceObservableCollection25");
			App.Tap("ItemsSourceObservableCollection25");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Pepper");
			App.WaitForNoElement("CollectionView Header(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithItemsSourceObservableCollection5()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceObservableCollection5");
			App.Tap("ItemsSourceObservableCollection5");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithItemsSourceNone()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
		}

#if TEST_FAILS_ON_WINDOWS //related issues:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithFooterString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithFooterView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithFooterView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithFooterString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

#if TEST_FAILS_ON_ANDROID //related issue: https://github.com/dotnet/maui/issues/28334
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In CV2 related issue: https://github.com/dotnet/maui/issues/28509, In windows related issue: https://github.com/dotnet/maui/issues/28824
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenGroupHeaderTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Potato");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenGroupHeaderTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenGroupFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenGroupFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
		}
#endif

#if TEST_FAILS_ON_WINDOWS  //related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenHeaderTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenHeaderTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //relate issue: https://github.com/dotnet/maui/issues/28824
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
		}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In windows, related issue: https://github.com/dotnet/maui/issues/27946 and In CV2, related issue: https://github.com/dotnet/maui/issues/28678
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithItemsLayoutVerticalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithItemsLayoutVerticalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithItemsLayoutHorizontalList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithItemsLayoutHorizontalList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithItemsLayoutHorizontalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithItemsLayoutHorizontalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
		}
#endif


#if TEST_FAILS_ON_ANDROID //related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWithItemsSourceObserableCollection5()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWithItemsSourceObserableCollection25()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceObservableCollection25");
			App.Tap("ItemsSourceObservableCollection25");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Pepper");
			App.WaitForNoElement("Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWithItemsSourceNone()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In windows related issue:https://github.com/dotnet/maui/issues/28022, In related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}
#endif

#if TEST_FAILS_ON_ANDROID //related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTempalteWhenFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2: related issue: https://github.com/dotnet/maui/issues/28824 and In windows: https://github.com/dotnet/maui/issues/28824
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenGroupFooterTemplate()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenGroupHeaderTemplate()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
		}
#endif
#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS  //In CV2, unintended synchronization between the HeaderTemplate/FooterTemplate and Header/Footer views, related issue: https://github.com/dotnet/maui/issues/28504
		//In windows, related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenHeaderString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenHeaderView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForNoElement("HeaderViewLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In all platforms, issue related: https://github.com/dotnet/maui/issues/28824 and CV2, related issues:https://github.com/dotnet/maui/issues/28504
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID //In windows, related issue: https://github.com/dotnet/maui/issues/27946, In CV2, related issue: https://github.com/dotnet/maui/issues/28678 and In android related issue:https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWithItemsLayoutVerticalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWithItemsLayoutHorizontalList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWithItemsLayoutHorizontalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2, unintended synchronization between the HeaderTemplate/FooterTemplate and Header/Footer views, related issue: https://github.com/dotnet/maui/issues/28504
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWithItemsSourceObservableCollection5()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWithItemsSourceNone()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}


		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWithItemsSourceObservableCollection25()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceObservableCollection25");
			App.Tap("ItemsSourceObservableCollection25");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Pepper");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWithItemsSourceObservableCollection5()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWithItemsSourceObservableCollection25()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceObservableCollection25");
			App.Tap("ItemsSourceObservableCollection25");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(Grid View)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Pepper");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWithItemsSourceNone()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In android related issue:https://github.com/dotnet/maui/issues/28622, In windows related issue:https://github.com/dotnet/maui/issues/28022 and In CV2, related issue: https://github.com/dotnet/maui/issues/28604
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //In Windows related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS//In CV2 related issues:https://github.com/dotnet/maui/issues/28509
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenGroupFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Fruits");
			App.WaitForElementTillPageNavigationSettled("Vegetables");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenGroupFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Fruits");
			App.WaitForElementTillPageNavigationSettled("Vegetables");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenGroupHeaderTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenGroupHeaderTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenHeaderString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenHeaderView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

#if TEST_FAILS_ON_ANDROID //related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderTemplate()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenHeaderTemplate()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //related isssue: https://github.com/dotnet/maui/issues/28824
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}
#endif
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In windows, related issue: https://github.com/dotnet/maui/issues/27946 and In CV2, related issue: https://github.com/dotnet/maui/issues/28678
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWithItemsLayoutVerticalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWithItemsLayoutVerticalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWithItemsLayoutHorizontalList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWithItemsLayoutHorizontalList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWithItemsLayoutHorizontalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWithItemsLayoutHorizontalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}
#endif
#endif

#if TEST_FAILS_ON_ANDROID //related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWithItemsSourceObservableCollections5()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}
#endif
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWithItemsSourceObservableCollections25()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceObservableCollection25");
			App.Tap("ItemsSourceObservableCollection25");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Footer Template(Grid View)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Pepper");
			App.WaitForElementTillPageNavigationSettled("Footer Template(Grid View)");
		}
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWithItemsSourceNone()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In windows related issue:https://github.com/dotnet/maui/issues/28022, In android: https://github.com/dotnet/maui/issues/28101 and In CV2, related issue: https://github.com/dotnet/maui/issues/28604 and https://github.com/dotnet/maui/issues/28504

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In Windows, related issue: https://github.com/dotnet/maui/issues/28337 and In CV2, related issue: https://github.com/dotnet/maui/issues/28504
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenFooterString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForNoElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenFooterView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //In CV2 related issues: https://github.com/dotnet/maui/issues/28509 and In windows, related issue: https://github.com/dotnet/maui/issues/28824
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenGroupFooterTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Fruits");
			App.WaitForElementTillPageNavigationSettled("Vegetables");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
			App.WaitForElementTillPageNavigationSettled("GroupFooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenGroupHeaderTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Potato");
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In android,related issue: https://github.com/dotnet/maui/issues/28337 and In CV2, reltaed issue:https://github.com/dotnet/maui/issues/28504
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenHeaderString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenHeaderView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID //In android,related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenHeaderTemplate()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //related issue: https://github.com/dotnet/maui/issues/28824
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("Mango");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("Apple");
		}

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID //In windows, related issue: https://github.com/dotnet/maui/issues/27946, In CV2, related issue: https://github.com/dotnet/maui/issues/28678, In android related issue: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWithItemsLayoutVerticalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutVerticalGrid");
			App.Tap("ItemsLayoutVerticalGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWithItemsLayoutHorizontalList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalList");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWithItemsLayoutHorizontalGrid()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsLayoutHorizontalGrid");
			App.Tap("ItemsLayoutHorizontalList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("Apple");
			App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
			App.WaitForElementTillPageNavigationSettled("Mango");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");

		}
#endif
#endif
	}
}