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

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2 related issue: https://github.com/dotnet/maui/issues/28504
		[Test, Order(1)]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderString_FirstSetItemsSourceList_AndItemsSourceList()
		{
			App.WaitForElementTillPageNavigationSettled("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderString_FirstSetHeaderString_AndItemsSourceList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderView_FirstSetItemsSourceList_AndItemsSourceList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderView_FirstSetHeaderView_AndItemsSourceList()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
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
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithEmptyViewView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("EmptyViewLabel");
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
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

#if TEST_FAILS_ON_ANDROID //Header string/view is visible but footer template is not visible.
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
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

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
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
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
		}

#if TEST_FAILS_ON_WINDOWS  //InCV2 and Windows Header String/View is visible not Header Template.
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
		}
#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2 Header View is visible not Header Template.
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
		}
#endif
#endif


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
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
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
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("cover1.jpg, 0");
			App.WaitForElementTillPageNavigationSettled("oasis.jpg, 1");
			App.WaitForElementTillPageNavigationSettled("photo.jpg, 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("cover1.jpg, 0");
			App.WaitForElementTillPageNavigationSettled("oasis.jpg, 1");
			App.WaitForElementTillPageNavigationSettled("photo.jpg, 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenDataTemplateSelectorView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateSelector");
			App.Tap("ItemTemplateSelector");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Header(String)");
			App.WaitForElementTillPageNavigationSettled("Template 1");
			App.WaitForElementTillPageNavigationSettled("Template 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenDataTemplateSelectorView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateSelector");
			App.Tap("ItemTemplateSelector");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderViewLabel");
			App.WaitForElementTillPageNavigationSettled("Template 1");
			App.WaitForElementTillPageNavigationSettled("Template 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplate_FirstHeaderTemplate_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
		}

#if TEST_FAILS_ON_ANDROID //Header template is not visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplate_FirstItemsSourceList_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In android Both EmptyView and Header template is not visible. In windows related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenEmptyViewView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("EmptyViewLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID //Both header and footer template are not visible.
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
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

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
		}

#if TEST_FAILS_ON_WINDOWS  //In CV2 and Windows, Heder template is not visible, Header String/View is visible.
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

#if TEST_FAILS_ON_ANDROID //In android, when IsGrouped Set False Header template is not visible.
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
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
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
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("cover1.jpg, 0");
			App.WaitForElementTillPageNavigationSettled("oasis.jpg, 1");
			App.WaitForElementTillPageNavigationSettled("photo.jpg, 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterString_FirstItemsSourceListSet_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterString_FirstFooterStringSet_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterView_FirstItemsSourceListSet_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterView_FirstFooterViewSet_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In android footer template is not visible, EmptyView string is visible.In windows related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("EmptyViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenEmptyViewView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("EmptyViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenEmptyViewView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("EmptyViewLabel");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //In CV2 and Windows footer template is not visible, but footer string/view is visible.
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
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2 footer template is not visible, but footer view is visible.
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
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In CV2 groupHeader/Footer template is not visible, related issues:https://github.com/dotnet/maui/issues/28509
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
			App.WaitForElementTillPageNavigationSettled("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
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
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

#if TEST_FAILS_ON_ANDROID //Header template is not visible but footer string/view is visible.
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
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
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
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("HeaderTemplateLabel");
		}
#endif

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
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterString");
			App.Tap("FooterString");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("CollectionView Footer(String)");
			App.WaitForElementTillPageNavigationSettled("cover1.jpg, 0");
			App.WaitForElementTillPageNavigationSettled("oasis.jpg, 1");
			App.WaitForElementTillPageNavigationSettled("photo.jpg, 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenBasicDataTemplateView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterViewLabel");
			App.WaitForElementTillPageNavigationSettled("cover1.jpg, 0");
			App.WaitForElementTillPageNavigationSettled("oasis.jpg, 1");
			App.WaitForElementTillPageNavigationSettled("photo.jpg, 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplate_FirstFooterTemplateSet_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

#if TEST_FAILS_ON_ANDROID //Footer template is not visible, while ItemsSourceList is tapped first.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplate_FirstItemsSourceListSet_WhenItemsSourceListView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In android, Both EmptyView and Footer template is not visible. In windows related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenEmptyViewString()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("No Items Available(String)");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenEmptyViewView()
		{
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("EmptyViewLabel");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //In Windows, Footer template is not visible, but footer string/view is visible.
		[Test]
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
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //IN CV2 groupHeader/Footer template is not visible, related issues: https://github.com/dotnet/maui/issues/28509
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
			App.WaitForElementTillPageNavigationSettled("GroupHeaderTemplateLabel");
		}
#endif

	#if TEST_FAILS_ON_ANDROID //Footer template is not visible.
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
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}

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
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
		}
#endif

#if TEST_FAILS_ON_ANDROID //In android when IsGrouped Set False Footer template is not visible.
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
			App.WaitForElementTillPageNavigationSettled("Options");
			App.Tap("Options");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElementTillPageNavigationSettled("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
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
			App.WaitForElementTillPageNavigationSettled("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElementTillPageNavigationSettled("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElementTillPageNavigationSettled("Apply");
			App.Tap("Apply");
			App.WaitForElementTillPageNavigationSettled("FooterTemplateLabel");
			App.WaitForElementTillPageNavigationSettled("cover1.jpg, 0");
			App.WaitForElementTillPageNavigationSettled("oasis.jpg, 1");
			App.WaitForElementTillPageNavigationSettled("photo.jpg, 2");
		}
#endif
	}

}