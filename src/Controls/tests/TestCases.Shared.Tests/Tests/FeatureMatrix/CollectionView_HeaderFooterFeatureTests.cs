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
			App.WaitForElement("Header Header(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderStringWithEmptyView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
		}
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyHeaderViewWithEmptyView()
		{
			App.WaitForElement("HeaderFooterViewButton");
			App.Tap("HeaderFooterViewButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("EmptyViewString");
			App.Tap("EmptyViewString");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Header(Grid View)");
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
	}
}