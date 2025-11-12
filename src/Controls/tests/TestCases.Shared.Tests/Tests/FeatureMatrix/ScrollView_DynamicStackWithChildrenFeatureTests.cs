using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

[Category(UITestCategories.ScrollView)]

public class ScrollView_DynamicStackWithChildrenFeatureTests : UITest
{

	public const string ScrollViewDynamicStackWithChildrenFeatureTests = "ScrollView With LayoutOptions Feature Matrix";
	public ScrollView_DynamicStackWithChildrenFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ScrollViewDynamicStackWithChildrenFeatureTests);
	}

	[Test, Order(1)]
	public void VerifyStackWithAddChildren_OrientationVertical()
	{
		App.WaitForElement("DynamicStackLayoutButton");
		App.Tap("DynamicStackLayoutButton");
		App.WaitForElement("ScrollViewWithDynamicStackLayout");
		App.WaitForElement("VerticalButton");
		App.Tap("VerticalButton");
		App.WaitForElement("AddButton");
		App.Tap("AddButton");
		App.WaitForElement("Label 4");
	}

	[Test, Order(2)]
	public void VerifyStackWithRemoveChildren_OrientationVertical()
	{
		App.WaitForElement("ScrollViewWithDynamicStackLayout");
		App.WaitForElement("VerticalButton");
		App.Tap("VerticalButton");
		App.WaitForElement("RemoveButton");
		App.Tap("RemoveButton");
		App.WaitForNoElement("Label 4");
	}

	[Test, Order(3)]
	public void VerifyStackWithAddChildren_OrientationHorizontal()
	{
		App.WaitForElement("ScrollViewWithDynamicStackLayout");
		App.WaitForElement("HorizontalButton");
		App.Tap("HorizontalButton");
		App.WaitForElement("AddButton");
		App.Tap("AddButton");
		App.WaitForElement("Label 4");
	}

	[Test, Order(4)]
	public void VerifyStackWithRemoveChildren_OrientationHorizontal()
	{
		App.WaitForElement("ScrollViewWithDynamicStackLayout");
		App.WaitForElement("HorizontalButton");
		App.Tap("HorizontalButton");
		App.WaitForElement("RemoveButton");
		App.Tap("RemoveButton");
		App.WaitForNoElement("Label 4");
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //related issue link:https://github.com/dotnet/maui/issues/32221
	[Test, Order(5)]
	public void VerifyVerticalStackLayoutResizesCorrectly_AfterAddingAndRemovingChildren()
	{
		App.WaitForElement("ScrollViewWithDynamicStackLayout");
		App.WaitForElement("VerticalButton");
		App.Tap("VerticalButton");
		App.WaitForElement("AddButton");

		for (int i = 0; i < 6; i++)
		{
			App.Tap("AddButton");
		}

		App.WaitForElement("RemoveButton");

		for (int i = 0; i < 7; i++)
		{
			App.Tap("RemoveButton");
		}
		VerifyScreenshot();
	}

	[Test, Order(6)]
	public void VerifyHorizontalStackLayoutResizesCorrectly_AfterAddingAndRemovingChildren()
	{
		App.WaitForElement("ScrollViewWithDynamicStackLayout");
		App.WaitForElement("HorizontalButton");
		App.Tap("HorizontalButton");
		App.WaitForElement("AddButton");

		for (int i = 0; i < 6; i++)
		{
			App.Tap("AddButton");
		}

		App.WaitForElement("RemoveButton");

		for (int i = 0; i < 7; i++)
		{
			App.Tap("RemoveButton");
		}
		VerifyScreenshot();
	}
#endif
}