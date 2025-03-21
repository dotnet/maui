using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests
{
	public class GroupingFeatureTests : UITest
	{
		public const string GroupingFeatureMatrix = "CollectionView Feature Matrix";
		public GroupingFeatureTests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(GroupingFeatureMatrix);
		}

		[Test, Order(1)]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithFooterString()
		{
			App.WaitForElement("GroupingButton");
			App.Tap("GroupingButton");
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithHeaderString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithDataTemplateSelectorItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("ItemTemplateSelector");
			App.Tap("ItemTemplateSelector");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Template 1");
			App.WaitForElement("Template 2");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithBasicItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithFooterString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_withHeaderString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("IsGroupedFalse");
			App.Tap("IsGroupedFalse");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithBasicItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("ItemTemplateBasic");
			App.Tap("ItemTemplateBasic");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("cover1.jpg, 0");
			App.WaitForElement("oasis.jpg, 1");
			App.WaitForElement("photo.jpg, 2");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithDataTemplateSelectorItemTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("ItemTemplateSelector");
			App.Tap("ItemTemplateSelector");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Template 1");
			App.WaitForElement("Template 2");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_IsGroupedTrueSetFirst_AndItemSourceList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_ItemSourceListSetFirst_AndIsGroupedTrue()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_IsGroupedFalseSetFirst_AndItemSourceGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_ItemSourceGroupedListSetFirst_AndIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_IsGroupedFalseSetFirst_AndItemSourceList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_ItemSourceListSetFirst_AndIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceList");
			App.Tap("ItemsSourceList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithFooterString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_withHeaderString()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithGroupHeaderTemplateWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");

		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithGroupFooterTemplateWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_withHeaderStringWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderString");
			App.Tap("HeaderString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(String)");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithFooterStringWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterString");
			App.Tap("FooterString");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(String)");
		}

		
#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS
//CanMixGroups Set to False Still Allows Reordering Between Groups in CollectionView on Catalyst Issue Link : https://github.com/dotnet/maui/issues/28530
//Test fails on CV2 . GroupHeader template is not visible  Issue Link: https://github.com/dotnet/maui/issues/28509
//.NET MAUI CollectionView does not reorder when grouped on windows Issue Link:  https://github.com/dotnet/maui/issues/13027
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyCanMixGroupsFalseWithCanReorderItems()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("CanReorderItemsTrue");
			App.Tap("CanReorderItemsTrue");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.DragAndDrop("cover1.jpg, 0", "oasis.jpg, 1");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyCanMixGroupsTrueWithCanReorderItems()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("CanMixGroupsTrue");
			App.Tap("CanMixGroupsTrue");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("CanReorderItemsTrue");
			App.Tap("CanReorderItemsTrue");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.DragAndDrop("cover1.jpg, 0", "oasis.jpg, 1");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyCanReorderItemsTrueWithCanMixGroups()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("CanReorderItemsTrue");
			App.Tap("CanReorderItemsTrue");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("CanMixGroupsTrue");
			App.Tap("CanMixGroupsTrue");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.DragAndDrop("cover1.jpg, 0", "oasis.jpg, 1");
			VerifyScreenshot();
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyCanReorderItemsFalseWithCanMixGroups()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("CanMixGroupsTrue");
			App.Tap("CanMixGroupsTrue");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.DragAndDrop("cover1.jpg, 0", "oasis.jpg, 1");
			VerifyScreenshot();
		}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
//Test fails on CV2 . GroupHeader/Footer template is not visible  Issue Link: https://github.com/dotnet/maui/issues/28509
//Header/Footer Template and View Synchronization Issue in .NET MAUI CollectionView (CV2) Issue Link : https://github.com/dotnet/maui/issues/28504
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithFooterView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithHeaderView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithFooterView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithHeaderView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithFooterView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithHeaderView()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithFooterViewWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterGrid");
			App.Tap("FooterGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Footer(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithHeaderViewWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderGrid");
			App.Tap("HeaderGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("CollectionView Header(Grid View)");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_GroupFooterTemplateSetFirst_AndItemSourceGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Footer Template(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_ItemSourceGroupedListSetFirst_AndGroupFooterTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Footer Template(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithIsGroupedTrue()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Footer Template(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_GroupHeaderTemplateSetFirst_AndItemSourceGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Header Template(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_ItemSourceGroupedListSetFirst_AndGroupHeaderTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Header Template(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithIsGroupedTrue()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Header Template(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithGroupHeaderTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Header Template(Grid View)");
			App.WaitForElement("Group Header Template(Grid View)");

		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithGroupFooterTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group Footer Template(Grid View)");
			App.WaitForElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_IsGroupedTrueSetFirst_AndItemSourceGroupedList()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group A");
			App.WaitForElement("Group B");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_ItemSourceGroupedListSetFirst_AndIsGroupedTrue()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("ItemsSourceGroupedList");
			App.Tap("ItemsSourceGroupedList");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Group A");
			App.WaitForElement("Group B");
		}

#if TEST_FAILS_ON_ANDROID  // Header and footer template is not visible on Android Issue Link: https://github.com/dotnet/maui/issues/28337
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithHeaderTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupFooterTemplate_WithFooterTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupFooterTemplateGrid");
			App.Tap("GroupFooterTemplateGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
			App.WaitForNoElement("Group Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithFooterTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyGroupHeaderTemplate_WithHeaderTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("GroupHeaderTemplateGrid");
			App.Tap("GroupHeaderTemplateGrid");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
			App.WaitForNoElement("Group Header Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithFooterTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithHeaderTemplate()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("IsGroupedTrue");
			App.Tap("IsGroupedTrue");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithFooterTemplateWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("FooterTemplateGrid");
			App.Tap("FooterTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Footer Template(Grid View)");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}
 
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void VerifyIsGrouped_WithHeaderTemplateWhenIsGroupedFalse()
		{
			App.WaitForElement("Options");
			App.Tap("Options");
			App.WaitForElement("HeaderTemplateGrid");
			App.Tap("HeaderTemplateGrid");
			App.WaitForElement("Apply");
			App.Tap("Apply");
			App.WaitForElement("Header Template(Grid View)");
			App.WaitForNoElement("Group A");
			App.WaitForNoElement("Group B");
		}
#endif
#endif
	}
}