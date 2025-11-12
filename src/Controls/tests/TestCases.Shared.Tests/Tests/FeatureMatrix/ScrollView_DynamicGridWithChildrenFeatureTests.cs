using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ScrollView)]

public class ScrollView_DynamicGridWithChildrenFeatureTests : UITest
{

	public const string ScrollViewDynamicGridWithChildrenFeatureTests = "ScrollView With LayoutOptions Feature Matrix";
	public ScrollView_DynamicGridWithChildrenFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ScrollViewDynamicGridWithChildrenFeatureTests);
	}

	[Test, Order(1)]
	public void VerifyGridWithAddChildren_RowWise()
	{
		App.WaitForElement("DynamicGridLayoutButton");
		App.Tap("DynamicGridLayoutButton");
		App.WaitForElement("ScrollViewWithDynamicGridLayout");
		App.WaitForElement("RowButton");
		App.Tap("RowButton");
		App.WaitForElement("AddButton");
		App.Tap("AddButton");
		App.WaitForElement("Label 4");
	}

	[Test, Order(2)]
	public void VerifyGridWithRemoveChildren_RowWise()
	{
		App.WaitForElement("ScrollViewWithDynamicGridLayout");
		App.WaitForElement("RowButton");
		App.Tap("RowButton");
		App.WaitForElement("RemoveButton");
		App.Tap("RemoveButton");
		App.WaitForNoElement("Label 4");
	}

	[Test, Order(3)]
	public void VerifyGridWithAddChildren_ColumnWise()
	{
		App.WaitForElement("ScrollViewWithDynamicGridLayout");
		App.WaitForElement("ColumnButton");
		App.Tap("ColumnButton");
		App.WaitForElement("AddButton");
		App.Tap("AddButton");
		App.WaitForElement("Label 4");
	}

	[Test, Order(4)]
	public void VerifyGridWithRemoveChildren_ColumnWise()
	{
		App.WaitForElement("ScrollViewWithDynamicGridLayout");
		App.WaitForElement("ColumnButton");
		App.Tap("ColumnButton");
		App.WaitForElement("RemoveButton");
		App.Tap("RemoveButton");
		App.WaitForNoElement("Label 4");
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //related issue link:https://github.com/dotnet/maui/issues/32221
	[Test, Order(5)]
	public void VerifyGridWithResizesCorrectly_AfterAddingAndRemovingChildrenWithRowWise()
	{
		App.WaitForElement("ScrollViewWithDynamicGridLayout");
		App.WaitForElement("RowButton");
		App.Tap("RowButton");
		App.WaitForElement("AddButton");

		for (int i = 0; i < 9; i++)
		{
			App.Tap("AddButton");
		}

		App.WaitForElement("RemoveButton");

		for (int i = 0; i < 10; i++)
		{
			App.Tap("RemoveButton");
		}
		VerifyScreenshot();
	}

	[Test, Order(6)]
	public void VerifyGridWithResizesCorrectly_AfterAddingAndRemovingChildrenWithColumnWise()
	{
		App.WaitForElement("ScrollViewWithDynamicGridLayout");
		App.WaitForElement("ColumnButton");
		App.Tap("ColumnButton");
		App.WaitForElement("AddButton");

		for (int i = 0; i < 9; i++)
		{
			App.Tap("AddButton");
		}

		App.WaitForElement("RemoveButton");

		for (int i = 0; i < 10; i++)
		{
			App.Tap("RemoveButton");
		}
		VerifyScreenshot();
	}
#endif
}