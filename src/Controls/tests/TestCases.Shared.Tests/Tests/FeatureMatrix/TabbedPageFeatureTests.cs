using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class TabbedPageFeatureTests : UITest
{
	const string TabbedPageFeatureMatrix = "TabbedPage Feature Matrix";

	public TabbedPageFeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(TabbedPageFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_InitialState_VerifyVisualState()
	{
		App.WaitForElement("TabbedPageControl");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/14572

	[Test, Order(2)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_InitialState_VerifyFunctionalState()
	{
		App.WaitForElement("TAB 3");
		App.Tap("TAB 3");
		App.WaitForElement("Tab3Label");
		VerifyScreenshot();
		Thread.Sleep(2000);
		App.WaitForElement("TAB 5");
		App.Tap("TAB 5");
		App.WaitForElement("Tab5Label");
		VerifyScreenshot();
	}
#endif

	[Test, Order(3)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarBackground_Gradient_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("GradientBarBackgroundButton");
		App.Tap("GradientBarBackgroundButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/14572

	[Test, Order(4)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarBackground_Solid_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SolidBarBackgroundButton");
		App.Tap("SolidBarBackgroundButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		App.WaitForElement("TAB 2");
		App.Tap("TAB 2");
		VerifyScreenshot();
	}

	[Test, Order(5)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarBackground_And_BarTextColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SolidBarBackgroundButton");
		App.Tap("SolidBarBackgroundButton");
		App.WaitForElement("GreenBarTextButton");
		App.Tap("GreenBarTextButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		App.WaitForElement("TAB 3");
		App.Tap("TAB 3");
		VerifyScreenshot();
	}
#endif

	[Test, Order(6)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarBackground_With_SelectedTabColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SolidBarBackgroundButton");
		App.Tap("SolidBarBackgroundButton");
		App.WaitForElement("PurpleSelectedButton");
		App.Tap("PurpleSelectedButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarBackground_With_UnselectedTabColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SolidBarBackgroundButton");
		App.Tap("SolidBarBackgroundButton");
		App.WaitForElement("DarkGrayUnselectedButton");
		App.Tap("DarkGrayUnselectedButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarTextColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("GreenBarTextButton");
		App.Tap("GreenBarTextButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarTextColor_And_SelectedTabColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("GreenBarTextButton");
		App.Tap("GreenBarTextButton");
		App.WaitForElement("PurpleSelectedButton");
		App.Tap("PurpleSelectedButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_BarTextColor_And_UnselectedTabColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("GreenBarTextButton");
		App.Tap("GreenBarTextButton");
		App.WaitForElement("DarkGrayUnselectedButton");
		App.Tap("DarkGrayUnselectedButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_SelectedTabColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("OrangeSelectedButton");
		App.Tap("OrangeSelectedButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

	[Test, Order(12)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_UnselectedTabColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("DarkGrayUnselectedButton");
		App.Tap("DarkGrayUnselectedButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/14572

	[Test, Order(13)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_SelectedAndUnselectedTabColor_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("PurpleSelectedButton");
		App.Tap("PurpleSelectedButton");
		App.WaitForElement("LightGrayUnselectedButton");
		App.Tap("LightGrayUnselectedButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
		App.WaitForElement("TAB 2");
		App.Tap("TAB 2");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link - https://github.com/dotnet/maui/issues/31121

	[Test, Order(14)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_FlowDirection_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("RightToLeftFlowRadio");
		App.Tap("RightToLeftFlowRadio");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		var rect = App.WaitForElement("ContentPageOne").GetRect();
		App.DragCoordinates(rect.X, rect.CenterY(), rect.Width - 10, rect.CenterY());
		Thread.Sleep(1000);
		App.WaitForElement("Tab2Label");
		VerifyScreenshot();
	}
#endif

	[Test, Order(15)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_ItemTemplate_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TemplateTwoButton");
		App.Tap("TemplateTwoButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_ItemTemplate_And_ItemSource_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TemplateTwoButton");
		App.Tap("TemplateTwoButton");
		App.WaitForElement("ItemsSourceTwoButton");
		App.Tap("ItemsSourceTwoButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("AppleLabel");
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_ItemSource_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceTwoButton");
		App.Tap("ItemsSourceTwoButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("AppleLabel");
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_ItemSource_And_SelectedItems_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("ItemsSourceTwoButton");
		App.Tap("ItemsSourceTwoButton");
		App.WaitForElement("SelectedItemEntry");
		App.ClearText("SelectedItemEntry");
		App.EnterText("SelectedItemEntry", "2");
		App.WaitForElement("SelectItemButton");
		App.Tap("SelectItemButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("BananaLabel");
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_SelectedItems_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("SelectedItemEntry");
		App.ClearText("SelectedItemEntry");
		App.EnterText("SelectedItemEntry", "3");
		App.WaitForElement("SelectItemButton");
		App.Tap("SelectItemButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab3Label");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Android does not show the More button, Issue Link - https://github.com/dotnet/maui/issues/31377, https://github.com/dotnet/maui/issues/14572

	[Test, Order(20)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_MoreButton_Verify()
	{
		App.WaitForElement("Tab1Label");
		App.Tap("Tab1Label");
		App.WaitForElement("More");
		App.Tap("More");
		VerifyScreenshot();
		App.WaitForElement("TAB 6");
		App.Tap("TAB 6");
		App.WaitForElement("Tab6Label");
		VerifyScreenshot();
	}
#endif

	[Test, Order(21)]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPage_IsEnabled_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("IsEnabledFalseRadio");
		App.Tap("IsEnabledFalseRadio");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("Tab1Label");
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForNoElement("ApplyButton");
	}
}