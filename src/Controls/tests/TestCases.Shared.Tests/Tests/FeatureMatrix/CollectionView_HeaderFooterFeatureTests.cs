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
		public void VerifyHeaderStringWithSetItemsSourceList()
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
		public void VerifyHeaderViewWithSetItemsSourceList()
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
		public void VerifyHeaderStringWithEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("No Items Available(String)");
		}
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithEmptyViewView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("No Items Available(Grid View)");
		}

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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
		}

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
			App.WaitForElement("CollectionViewItem");
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
			App.WaitForElement("CollectionViewItem");
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
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenItemsSourceListView()
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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
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
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("No Items Available(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenFooterString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenFooterView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderTemplateWhenFooterTemplateView()
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
		}

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
			App.WaitForElement("CollectionViewItem");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenItemsSourceListView()
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
		public void VerifyFooterViewWhenItemsSourceListView()
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
		public void VerifyFooterStringWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForElement("No Items Available(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterViewWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
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
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
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
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("No Items Available(Grid View)");
		}

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
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(String)");
			App.WaitForElement("Footer Template(Grid View)");
		}

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
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}

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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderString()
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
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
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
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
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
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterStringWhenHeaderTemplate()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
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
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Header Template(Grid View)");
		}

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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

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
			App.WaitForElement("CollectionViewItem");
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
			App.WaitForElement("CollectionViewItem");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenItemsSourceListView()
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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenEmptyViewString()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
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
			App.WaitForElement("EmptyViewGrid");
			App.Tap("EmptyViewGrid");
			App.WaitForElement("ItemsSourceNone");
			App.Tap("ItemsSourceNone");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("No Items Available(Grid View)");
			App.WaitForElement("Footer Template(Grid View)");
		}

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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenGroupFooterTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
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
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("CollectionView Footer(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenIsGroupedTrueOrFalse()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyFooterTemplateWhenBasicDataTemplateView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForElement("CollectionViewItem");
		}
	}
}