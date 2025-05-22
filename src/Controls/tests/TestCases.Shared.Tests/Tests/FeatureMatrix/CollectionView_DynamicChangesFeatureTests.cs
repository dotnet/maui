using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;
public class CollectionView_DynamicChangesFeatureTests : UITest
{
	public const string DynamicChangesFeatureMatrix = "CollectionView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";

	public CollectionView_DynamicChangesFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(DynamicChangesFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicItemTemplateDisplayed()
	{
		App.WaitForElement("DynamicButton");
		App.Tap("DynamicButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ItemTemplateBasic");
		App.Tap("ItemTemplateBasic");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.WaitForElement("Orange");
		App.WaitForElement("ItemTemplateButton");
		App.Tap("ItemTemplateButton");
		App.WaitForElement("Template 1 - Apple");
		App.WaitForElement("Template 1 - Orange");
		App.Tap("ItemTemplateButton");
		App.WaitForElement("Template 2 - Apple");
		App.WaitForElement("Template 2 - Orange");
	}

#if TEST_FAILS_ON_ANDROID
//Dynamic Updates to CollectionView Header/Footer and Templates Are Not Displayed Issue Link: https://github.com/dotnet/maui/issues/28676
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicHeaderStringDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("HeaderString");
		App.Tap("HeaderString");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default Header String");
		App.WaitForElement("HeaderStringButton");
		App.Tap("HeaderStringButton");
		App.WaitForElement("Header String1");
		App.Tap("HeaderStringButton");
		App.WaitForElement("Header String2");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicHeaderGridDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("HeaderGrid");
		App.Tap("HeaderGrid");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default Header Grid");
		App.WaitForElement("HeaderGridButton");
		App.Tap("HeaderGridButton");
		App.WaitForElement("Header Grid1");
		App.Tap("HeaderGridButton");
		App.WaitForElement("Header Grid2");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicHeaderTemplateDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("HeaderTemplateGrid");
		App.Tap("HeaderTemplateGrid");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default HeaderTemplate");
		App.WaitForElement("HeaderTemplateButton");
		App.Tap("HeaderTemplateButton");
		App.WaitForElement("Header Template1");
		App.Tap("HeaderTemplateButton");
		App.WaitForElement("Header Template2");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicFooterStringDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FooterString");
		App.Tap("FooterString");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default Footer String");
		App.WaitForElement("FooterStringButton");
		App.Tap("FooterStringButton");
		App.WaitForElement("Footer String1");
		App.Tap("FooterStringButton");
		App.WaitForElement("Footer String2");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicFooterGridDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FooterGrid");
		App.Tap("FooterGrid");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default Footer Grid");
		App.WaitForElement("FooterGridButton");
		App.Tap("FooterGridButton");
		App.WaitForElement("Footer Grid1");
		App.Tap("FooterGridButton");
		App.WaitForElement("Footer Grid2");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicFooterTemplateDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FooterTemplateGrid");
		App.Tap("FooterTemplateGrid");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default FooterTemplate");
		App.WaitForElement("FooterTemplateButton");
		App.Tap("FooterTemplateButton");
		App.WaitForElement("Footer Template1");
		App.Tap("FooterTemplateButton");
		App.WaitForElement("Footer Template2");
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicGroupHeaderTemplateDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ItemsSourceGroupedList");
		App.Tap("ItemsSourceGroupedList");
		App.WaitForElement("IsGroupedTrue");
		App.Tap("IsGroupedTrue");
		App.WaitForElement("GroupHeaderTemplateGrid");
		App.Tap("GroupHeaderTemplateGrid");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default GroupHeaderTemplate");
		App.WaitForElement("GroupHeaderTemplateButton");
		App.Tap("GroupHeaderTemplateButton");
		App.WaitForElement("GroupHeaderTemplate1");
		App.Tap("GroupHeaderTemplateButton");
		App.WaitForElement("GroupHeaderTemplate2");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicGroupFooterTemplateDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ItemsSourceGroupedList");
		App.Tap("ItemsSourceGroupedList");
		App.WaitForElement("IsGroupedTrue");
		App.Tap("IsGroupedTrue");
		App.WaitForElement("GroupFooterTemplateGrid");
		App.Tap("GroupFooterTemplateGrid");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Default GroupFooterTemplate");
		App.WaitForElement("GroupFooterTemplateButton");
		App.Tap("GroupFooterTemplateButton");
		App.WaitForElement("GroupFooterTemplate1");
		App.Tap("GroupFooterTemplateButton");
		App.WaitForElement("GroupFooterTemplate2");
	}

#if TEST_FAILS_ON_WINDOWS
	//[Testing] EmptyView(null ItemsSource) elements Not Accessible via Automation on Windows Platform Issue Link:  https://github.com/dotnet/maui/issues/28022
	//EmptyViewTemplate not shown in Windows Issue Link: https://github.com/dotnet/maui/issues/28334
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicEmptyViewStringDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("EmptyViewString");
		App.Tap("EmptyViewString");
		App.WaitForElement("ItemsSourceNone");
		App.Tap("ItemsSourceNone");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("No Items Available(String)");
		App.WaitForElement("EmptyViewStringButton");
		App.Tap("EmptyViewStringButton");
		App.WaitForElement("EmptyView String1: No items available");
		App.Tap("EmptyViewStringButton");
		App.WaitForElement("EmptyView String2: No items available");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicEmptyViewGridDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("EmptyViewGrid");
		App.Tap("EmptyViewGrid");
		App.WaitForElement("ItemsSourceNone");
		App.Tap("ItemsSourceNone");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("No Items Available(Grid View)");
		App.WaitForElement("EmptyViewGridButton");
		App.Tap("EmptyViewGridButton");
		App.WaitForElement("EmptyView Grid1: No items available");
		App.Tap("EmptyViewGridButton");
		App.WaitForElement("EmptyView Grid2: No items available");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ValidateDynamicEmptyViewTemplateDisplayed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("EmptyViewTemplateGrid");
		App.Tap("EmptyViewTemplateGrid");
		App.WaitForElement("ItemsSourceNone");
		App.Tap("ItemsSourceNone");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("No Template Items Available(Grid View)");
		App.WaitForElement("EmptyViewTemplateButton");
		App.Tap("EmptyViewTemplateButton");
		App.WaitForElement("EmptyViewTemplate1: No items available");
		App.Tap("EmptyViewTemplateButton");
		App.WaitForElement("EmptyViewTemplate2: No items available");
	}
#endif
}