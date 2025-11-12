using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ScrollView)]

public class ScrollView_DynamicFlexWithChildrenFeatureTests : UITest
{

	public const string ScrollViewDynamicFlexWithChildrenFeatureTests = "ScrollView With LayoutOptions Feature Matrix";
	public ScrollView_DynamicFlexWithChildrenFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ScrollViewDynamicFlexWithChildrenFeatureTests);
	}

	[Test, Order(1)]
	public void VerifyFlexWithAddChildren_RowDirection()
	{
		App.WaitForElement("DynamicFlexLayoutButton");
		App.Tap("DynamicFlexLayoutButton");
		App.WaitForElement("ScrollViewWithDynamicFlexLayout");
		App.WaitForElement("RowButton");
		App.Tap("RowButton");
		App.WaitForElement("AddButton");
		App.Tap("AddButton");
		App.WaitForElement("Label 4");
	}

	[Test, Order(2)]
	public void VerifyFlexWithRemoveChildren_RowDirection()
	{
		App.WaitForElement("ScrollViewWithDynamicFlexLayout");
		App.WaitForElement("RowButton");
		App.Tap("RowButton");
		App.WaitForElement("RemoveButton");
		App.Tap("RemoveButton");
		App.WaitForNoElement("Label 4");
	}

	[Test, Order(3)]
	public void VerifyFlexWithAddChildren_ColumnDirection()
	{
		App.WaitForElement("ScrollViewWithDynamicFlexLayout");
		App.WaitForElement("ColumnButton");
		App.Tap("ColumnButton");
		App.WaitForElement("AddButton");
		App.Tap("AddButton");
		App.WaitForElement("Label 4");
	}

	[Test, Order(4)]
	public void VerifyFlexWithRemoveChildren_ColumnDirection()
	{
		App.WaitForElement("ScrollViewWithDynamicFlexLayout");
		App.WaitForElement("ColumnButton");
		App.Tap("ColumnButton");
		App.WaitForElement("RemoveButton");
		App.Tap("RemoveButton");
		App.WaitForNoElement("Label 4");
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //related issue link:https://github.com/dotnet/maui/issues/32221
	[Test, Order(5)]
	public void VerifyFlexWithResizesCorrectly_AfterAddingAndRemovingWithRowDirection()
	{
		App.WaitForElement("ScrollViewWithDynamicFlexLayout");
		App.WaitForElement("RowButton");
		App.Tap("RowButton");
		App.WaitForElement("AddButton");

		for (int i = 0; i < 8; i++)
		{
			App.Tap("AddButton");
		}

		App.WaitForElement("RemoveButton");

		for (int i = 0; i < 9; i++)
		{
			App.Tap("RemoveButton");
		}
		VerifyScreenshot();
	}

	[Test, Order(6)]
	public void VerifyFlexWithResizesCorrectly_AfterAddingAndRemovingWithColumnDirection()
	{
		App.WaitForElement("ScrollViewWithDynamicFlexLayout");
		App.WaitForElement("ColumnButton");
		App.Tap("ColumnButton");
		App.WaitForElement("AddButton");

		for (int i = 0; i < 8; i++)
		{
			App.Tap("AddButton");
		}

		App.WaitForElement("RemoveButton");

		for (int i = 0; i < 8; i++)
		{
			App.Tap("RemoveButton");
		}
		VerifyScreenshot();
	}
#endif
}