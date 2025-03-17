using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class CollectionView_HeaderFooterFeatureTests : UITest
	{
		public const string HeaderFooterFeatureMatrix = "CollectionView Feature Matrix";
		protected override bool ResetAfterEachTest => true;

		public CollectionView_HeaderFooterFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(HeaderFooterFeatureMatrix);
		}


		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderString_FirstSetItemsSourceList_AndItemsSourceList()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderString_FirstSetHeaderString_AndItemsSourceList()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderView_FirstSetItemsSourceList_AndItemsSourceList()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderView_FirstSetHeaderView_AndItemsSourceList()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
		}
#if TEST_FAILS_ON_WINDOWS //related issues:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithEmptyViewView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("No Items Available(Grid View)");
		}
#endif
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithFooterString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithFooterView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithFooterView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithFooterString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("CollectionView Footer(String)");
		}

#if TEST_FAILS_ON_ANDROID //Header string/view is visible but footer template is not visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In windows While tap the IsGrouped, the app is crashing.
		//In CV2, Group header/footer template is not visible, but header string/view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenGroupHeaderTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenGroupHeaderTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenGroupFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenGroupFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS  //InCV2 and Windows Header String/View is visible not Header Template.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenHeaderTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Header(String)");
			App.WaitForElement("Header Template(Grid View)");
		}
#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2 Header View is visible not Header Template.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenHeaderTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Header(Grid View)");
			App.WaitForElement("Header Template(Grid View)");
		}
#endif
#endif


#if TEST_FAILS_ON_WINDOWS //While tap the IsGrouped, the app is crashing.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenBasicDataTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForElement("Vegetables.jpg, 3");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenBasicDataTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForElement("Vegetables.jpg, 3");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWhenDataTemplateSelectorView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateSelector");
			App.Tap("ItemTemplateSelector");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("Template 1");
			App.WaitForElement("Template 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWhenDataTemplateSelectorView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateSelector");
			App.Tap("ItemTemplateSelector");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Template 1");
			App.WaitForElement("Template 2");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplate_FirstHeaderTemplate_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
		}

#if TEST_FAILS_ON_ANDROID //Header template is not visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplate_FirstItemsSourceList_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In android Both EmptyView and Header template is not visible. In windows related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenEmptyViewView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("No Items Available(Grid View)");
		}
#endif

#if TEST_FAILS_ON_ANDROID //Both header and footer template are not visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTempalteWhenFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In windows While tap the IsGrouped, the app is crashing.
		//In CV2, Group header/footer template is not visible, but header string/view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenGroupFooterTemplate()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenGroupHeaderTemplate()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS  //In CV2 and Windows, Heder template is not visible, Header String/View is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenHeaderString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Header(String)");
			App.WaitForElement("Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenHeaderView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForNoElement("CollectionView Header(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID //In windows While tap the IsGrouped, the app is crashing. In android, when IsGrouped Set False Header template is not visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenBasicDataTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForElement("Vegetables.jpg, 3");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterString_FirstItemsSourceListSet_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterString_FirstFooterStringSet_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterView_FirstItemsSourceListSet_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterView_FirstFooterViewSet_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In android footer template is not visible, EmptyView string is visible.In windows related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForElement("No Items Available(String)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenEmptyViewView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenEmptyViewView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("No Items Available(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //In CV2 and Windows footer template is not visible, but footer string/view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(String)");
			App.WaitForElement("Footer Template(Grid View)");
		}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2 footer template is not visible, but footer view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In Windows While tap the IsGrouped, the app is crashing.
//In CV2, Group header/footer template is not visible, but header string/view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenGroupFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenGroupFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenGroupHeaderTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenGroupHeaderTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenHeaderString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenHeaderView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

#if TEST_FAILS_ON_ANDROID //Header template is not visible but footer string/view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderTemplate()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForElement("Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenHeaderTemplate()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Header Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //In Windows While tap the IsGrouped, the app is crashing.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenBasicDataTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForElement("Vegetables.jpg, 3");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenBasicDataTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForElement("Vegetables.jpg, 3");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplate_FirstFooterTemplateSet_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
		}

#if TEST_FAILS_ON_ANDROID //Footer template is not visible, while ItemsSourceList is tapped first.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplate_FirstItemsSourceListSet_WhenItemsSourceListView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS //In android, Both EmptyView and Footer template is not visible. In windows related issue:https://github.com/dotnet/maui/issues/28022
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(String)");
			App.WaitForElement("Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenEmptyViewView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS //In Windows, Footer template is not visible, but footer string/view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenFooterString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForNoElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenFooterView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In Windows While tap the IsGrouped, the app is crashing.
//In CV2, Group header/footer template is not visible, but header string/view is visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenGroupFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}


		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenGroupHeaderTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}
#endif

	#if TEST_FAILS_ON_ANDROID //Footer template is not visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenHeaderString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenHeaderView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenHeaderTemplate()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID //In Windows While tap the IsGrouped, the app is crashing. In android when IsGrouped Set False Footer template is not visible.
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
		}
#endif

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenBasicDataTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForElement("Vegetables.jpg, 3");
		}


	}
}