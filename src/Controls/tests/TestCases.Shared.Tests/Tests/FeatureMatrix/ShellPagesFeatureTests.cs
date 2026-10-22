using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class ShellPagesFeatureTests : _GalleryUITest
{
	public const string ShellPagesFeatureMatrix = "Shell Feature Matrix";
	public override string GalleryPageName => ShellPagesFeatureMatrix;
	protected override string GallerySubPageButton => "ShellPageButton";
	public const string Options = "Options";
	public const string Apply = "Apply";

	public ShellPagesFeatureTests(TestDevice device)
		: base(device)
	{
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32125
	[Test, Order(5)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_BackgroundColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_UnselectedColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_IsEnabledTrue()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsEnabledTrue");
		App.Tap("IsEnabledTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Tab3");
		App.Tap("Tab3");
		App.WaitForElement("Tab3Label");
		App.WaitForElement("Tab3GoToHomeButton");
		App.Tap("Tab3GoToHomeButton");
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/5161
	[Test, Order(6)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_ShowTitleViewWithBackgroundColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_ShowTitleViewHidden()
	{
		App.WaitForElement("HideTitleViewButton");
		App.Tap("HideTitleViewButton");
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/17550
	[Test, Order(10)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_NavBarVisibilityHide()
	{
		App.WaitForElement("NavBarHideButton");
		App.Tap("NavBarHideButton");
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_NavBarVisibilityShow()
	{
		App.WaitForElement("NavBarShowButton");
		App.Tap("NavBarShowButton");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/6399
	[Test, Order(12)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_PresentationModeNotAnimated()
	{
		App.WaitForElement("NotAnimatedButton");
		App.Tap("NotAnimatedButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_ForegroundColorAndUnselectedColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_BackgroundColorAndForegroundColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_BackgroundColorAndTitleColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_BackgroundColorAndUnselectedColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_DisabledColorAndUnselectedColor()
	{
		if (App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
		{
			Assert.Ignore("Ignored due to a bug issue in iOS 26"); // Issue Link: https://github.com/dotnet/maui/issues/32125
		}
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
	[Test, Order(30)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
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
		VerifyShellScreenshot();
	}
#endif 

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS  // Issue Link:https://github.com/dotnet/maui/issues/32993
	[Test, Order(29)]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void ShellPages_FlowDirectionRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

	public void VerifyShellScreenshot()
	{
#if WINDOWS
		VerifyScreenshot(includeTitleBar: true);
#else
		VerifyScreenshot();
#endif
	}
}
