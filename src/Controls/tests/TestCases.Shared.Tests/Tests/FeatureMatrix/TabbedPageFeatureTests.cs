using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class TabbedPageFeatureTests : _GalleryUITest
{
	public const string TabbedPageFeatureMatrix = "TabbedPage Feature Matrix";
	public override string GalleryPageName => TabbedPageFeatureMatrix;

	public TabbedPageFeatureTests(TestDevice testDevice) : base(testDevice)
	{
	}

#if TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/34605

	[Test, Order(1)]
	public void TabbedPage_InitialState_VerifyVisualState()
	{
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/14572

	[Test, Order(2)]
	public void TabbedPage_InitialState_VerifyFunctionalState()
	{
		App.WaitForElement("TAB 3");
		App.Tap("TAB 3");
		App.WaitForElement("Tab3Label");
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_1");
		App.WaitForElement("TAB 4");
		App.Tap("TAB 4");
		App.WaitForElement("Tab4Label");
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_2");
	}
#endif

	[Test, Order(3)]
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
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_1");
		App.WaitForElement("TAB 2");
		App.Tap("TAB 2");
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_2");
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS// Issue Link - https://github.com/dotnet/maui/issues/31121, https://github.com/dotnet/maui/issues/14572

	[Test, Order(14)]
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
		App.WaitForElement("CherryLabel");
		VerifyScreenshot();
	}

	[Test, Order(19)]
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
	public void TabbedPage_MoreButton_Verify()
	{
		App.WaitForElement("Tab1Label");
		App.Tap("Tab1Label");
#if IOS || MACCATALYST
		App.WaitForElement("More");
		App.Tap("More");
#endif
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_1");
		App.WaitForElement("TAB 6");
		App.Tap("TAB 6");
		App.WaitForElement("Tab6Label");
		VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_2");
	}
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link : https://github.com/dotnet/maui/issues/14572
	[Test, Order(21)]
	public void TabbedPage_AddTab_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TemplateTwoButton");
		App.Tap("TemplateTwoButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("AddTabButton");
		App.Tap("AddTabButton");
		App.WaitForElement("AddTabButton");
		App.Tap("AddTabButton");
		App.WaitForElement("Tab1Label");
#if IOS || MACCATALYST
		App.WaitForElement("More");
		App.Tap("More");
#endif
		App.WaitForElement("TAB 7");
		App.Tap("TAB 7");
		App.WaitForElement("Tab7Label");
		App.WaitForElement("TAB 7");
		App.Tap("TAB 7");
		App.WaitForElement("Tab7Label");
	}
#endif
#endif

#if TEST_FAILS_ON_WINDOWS // Issue Link - https://github.com/dotnet/maui/issues/14572

	[Test, Order(22)]
	public void TabbedPage_RemoveTab_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TemplateTwoButton");
		App.Tap("TemplateTwoButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
#if ANDROID
		for (int i = 0; i < 2; i++)
		{
			App.WaitForElement("RemoveTabButton");
			App.Tap("RemoveTabButton");
		}
#else
		for (int i = 0; i < 7; i++)
		{
			App.WaitForElement("RemoveTabButton");
			App.Tap("RemoveTabButton");
		}
#endif
		App.WaitForElement("Tab1Label");
		App.WaitForNoElement("TAB 4");
	}

#if TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/34605
	[Test, Order(23)]
	public void TabbedPage_InsertTabAt_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TemplateTwoButton");
		App.Tap("TemplateTwoButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("RemoveTabButton");
		App.Tap("RemoveTabButton");
		App.WaitForElement("RemoveTabButton");
		App.Tap("RemoveTabButton");
		App.WaitForElement("InsertTabAtButton");
		App.Tap("InsertTabAtButton");
		App.WaitForElement("NEW TAB");
		App.Tap("NEW TAB");
		App.WaitForElement("NewTabLabel");
		VerifyScreenshot();
	}
#endif
#endif

#if TEST_FAILS_ON_IOS // Issue Link - https://github.com/dotnet/maui/issues/34605
	[Test, Order(24)]
	public void TabbedPage_IconImageSource_Change_Verify()
	{
		App.WaitForElement("Options");
		App.Tap("Options");
		App.WaitForElement("TemplateTwoButton");
		App.Tap("TemplateTwoButton");
		App.WaitForElement("ApplyButton");
		App.Tap("ApplyButton");
		App.WaitForElement("ChangeIconButton");
		App.Tap("ChangeIconButton");
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}
#endif
	[Test, Order(25)]
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