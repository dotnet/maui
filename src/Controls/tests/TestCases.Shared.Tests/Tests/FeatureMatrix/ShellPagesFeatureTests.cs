using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ShellPagesFeatureTests : _GalleryUITest
{
	public const string ShellPagesFeatureMatrix = "Shell Feature Matrix";
	public override string GalleryPageName => ShellPagesFeatureMatrix;
	public const string Options = "Options";
	public const string Apply = "Apply";

	public ShellPagesFeatureTests(TestDevice device)
		: base(device)
	{
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32125
	[Test, Order(5)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_TitleColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("RedTitleColor");
		App.Tap("RedTitleColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/32992
	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_BackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("SkyBlueBackgroundColor");
		App.Tap("SkyBlueBackgroundColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/5161
	[Test, Order(3)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_DisabledColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsEnabledFalse");
        App.Tap("IsEnabledFalse");
		App.WaitForElement("VioletDisabledColor");
		App.Tap("VioletDisabledColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32125
	[Test, Order(4)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_UnselectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("BlueUnselectedColor");
		App.Tap("BlueUnselectedColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_IsEnabledTrue()
	{
		App.WaitForElement("ShellPageButton");
		App.Tap("ShellPageButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsEnabledTrue");
		App.Tap("IsEnabledTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Tab3");
		App.Tap("Tab3");
		App.WaitForElement("Tab3Label");
		App.WaitForElement("GoToHomeButton");
		App.Tap("GoToHomeButton");
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/5161
	[Test, Order(6)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_IsEnabledFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Tab3");
		App.Tap("Tab3");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/32992
	[Test, Order(7)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ShowTitleViewWithBackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("SkyBlueBackgroundColor");
		App.Tap("SkyBlueBackgroundColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ShowTitleViewButton");
		App.Tap("ShowTitleViewButton");
		VerifyScreenshot();
	}
#endif

	[Test, Order(8)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ShowTitleView()
	{
		App.WaitForElement(Options); // to reset the old value
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ShowTitleViewButton");
		App.Tap("ShowTitleViewButton");
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ShowTitleViewHidden()
	{
		App.WaitForElement("HideTitleViewButton");
		App.Tap("HideTitleViewButton");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/17550
	[Test, Order(10)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_NavBarVisibilityHide()
	{
		App.WaitForElement("NavBarHideButton");
		App.Tap("NavBarHideButton");
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_NavBarVisibilityShow()
	{
		App.WaitForElement("NavBarShowButton");
		App.Tap("NavBarShowButton");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/6399
	[Test, Order(12)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_NavBarHasShadowTrue()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("WhiteBackgroundColor"); // For visible difference when shadow is applied
		App.Tap("WhiteBackgroundColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ShadowTrue");
		App.Tap("ShadowTrue");
		VerifyScreenshot();
	}

	[Test, Order(13)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_NavBarHasShadowFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("WhiteBackgroundColor"); // For visible difference when shadow is removed
		App.Tap("WhiteBackgroundColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ShadowFalse");
		App.Tap("ShadowFalse");
		VerifyScreenshot();
	}
#endif

	[Test, Order(14)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_IsVisibleFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_IsVisibleTrue()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleTrue");
		App.Tap("IsVisibleTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeNotAnimated()
	{
		App.WaitForElement("NotAnimatedButton");
		App.Tap("NotAnimatedButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeAnimated()
	{
		App.WaitForElement("GoBackButton"); // To go back to controls page
		App.Tap("GoBackButton");
		App.WaitForElement("AnimatedButton");
		App.Tap("AnimatedButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

	[Test, Order(18)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeModal()
	{
		App.WaitForElement("GoBackButton"); // To go back to controls page
		App.Tap("GoBackButton");
		App.WaitForElement("ModalButton");
		App.Tap("ModalButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeModalAnimated()
	{
		App.WaitForElement("GoBackButton"); // To go back to controls page
		App.Tap("GoBackButton");
		App.WaitForElement("ModalAnimatedButton");
		App.Tap("ModalAnimatedButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeModalNotAnimated()
	{ 
		App.WaitForElement("GoBackButton"); // To go back to controls page
		App.Tap("GoBackButton");
		App.WaitForElement("ModalNotAnimatedButton");
		App.Tap("ModalNotAnimatedButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32125
	[Test, Order(21)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ForegroundColor()
	{
		App.WaitForElement("GoBackButton"); // To go back to controls page
		App.Tap("GoBackButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MagentaForegroundColor");
		App.Tap("MagentaForegroundColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ForegroundColorAndTitleColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MagentaForegroundColor");
		App.Tap("MagentaForegroundColor");
		App.WaitForElement("RedTitleColor");
		App.Tap("RedTitleColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif 

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32125
	[Test, Order(23)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ForegroundColorAndUnselectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MagentaForegroundColor");
		App.Tap("MagentaForegroundColor");
		App.WaitForElement("MaroonUnselectedColor");
		App.Tap("MaroonUnselectedColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif


#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS// Issue Link: https://github.com/dotnet/maui/issues/32992
	[Test, Order(24)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_BackgroundColorAndForegroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("LightGreenBackgroundColor");
		App.Tap("LightGreenBackgroundColor");
		App.WaitForElement("MagentaForegroundColor");
		App.Tap("MagentaForegroundColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(25)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_BackgroundColorAndTitleColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("LightGreenBackgroundColor");
		App.Tap("LightGreenBackgroundColor");
		App.WaitForElement("RedTitleColor");
		App.Tap("RedTitleColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(26)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_BackgroundColorAndUnselectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("LightGreenBackgroundColor");
		App.Tap("LightGreenBackgroundColor");
		App.WaitForElement("MaroonUnselectedColor");
		App.Tap("MaroonUnselectedColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/5161
	[Test, Order(27)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ForegroundColorAndDisabledColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MagentaForegroundColor");
		App.Tap("MagentaForegroundColor");
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement("VioletDisabledColor");
		App.Tap("VioletDisabledColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_DisabledColorAndUnselectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MaroonUnselectedColor");
		App.Tap("MaroonUnselectedColor");
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement("VioletDisabledColor");
		App.Tap("VioletDisabledColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif 

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS// Issue Link: https://github.com/dotnet/maui/issues/33909
	[Test, Order(29)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_VerifyForegroundColorResetForBackButton()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MagentaForegroundColor");
		App.Tap("MagentaForegroundColor");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.Tap(Options);
#if WINDOWS
		VerifyScreenshot(includeTitleBar: true);
#else
		VerifyScreenshot();
#endif
	}
#endif 

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS  // Issue Link:https://github.com/dotnet/maui/issues/32993
	[Test, Order(30)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_FlowDirectionRTL()
	{
		App.WaitForElement(Apply); // To reset the old value
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif
}
