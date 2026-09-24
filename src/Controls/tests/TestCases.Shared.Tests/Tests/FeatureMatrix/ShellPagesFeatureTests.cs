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
	bool IsIndependentTest => TestContext.CurrentContext.Test.MethodName is
		"ShellPages_ShowTitleView" or "ShellPages_ShowTitleViewHidden" or
		"ShellPages_IsVisibleFalse" or "ShellPages_IsVisibleTrue" or
		"ShellPages_PresentationModeNotAnimated" or "ShellPages_PresentationModeAnimated" or
		"ShellPages_PresentationModeModal" or "ShellPages_PresentationModeModalAnimated" or
		"ShellPages_PresentationModeModalNotAnimated" or "ShellPages_FlowDirectionRTL";
	protected override string? GallerySubPageButton => IsIndependentTest ? "ShellPageButton" : null;

	public override void TestSetup()
	{
		base.TestSetup();
		if (IsIndependentTest)
			FixtureSetup();
	}

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
		App.WaitForElement("Tab3GoToHomeButton");
		App.Tap("Tab3GoToHomeButton");
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
	[Category(UITestCategories.Shell)]
	public void ShellPages_ShowTitleView()
	{
		App.WaitForElement(Options); // to reset the old value
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ShowTitleViewButton");
		App.Tap("ShowTitleViewButton");
		App.WaitForElement("ShellTitleViewText");
		App.WaitForElement("ShellTitleViewImage");
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_ShowTitleViewHidden()
	{
		App.WaitForElement("ShowTitleViewButton");
		App.Tap("ShowTitleViewButton");
		App.WaitForElement("ShellTitleViewText");
		App.WaitForElement("ShellTitleViewImage");
		App.WaitForElement("HideTitleViewButton");
		App.Tap("HideTitleViewButton");
		App.WaitForNoElement("ShellTitleViewText");
		App.WaitForNoElement("ShellTitleViewImage");
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
		ShellFeatureTestActions.WaitForBottomTab(App, "Tab2");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.WaitForNoElement(() => ShellFeatureTestActions.FindBottomTab(App, "Tab2"));
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_IsVisibleTrue()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.WaitForNoElement(() => ShellFeatureTestActions.FindBottomTab(App, "Tab2"));
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleTrue");
		App.Tap("IsVisibleTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		ShellFeatureTestActions.WaitForBottomTab(App, "Tab2");
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeNotAnimated()
	{
		App.WaitForElement("NotAnimatedButton");
		App.Tap("NotAnimatedButton");
		App.WaitForElement("GoBackButton");
		ShellFeatureTestActions.WaitForBottomTab(App, "Home");
		ShellFeatureTestActions.TapPageBack(App, "Home");
		App.WaitForElement("NotAnimatedButton");
		App.WaitForNoElement("GoBackButton");
	}

	[Test, Order(17)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeAnimated()
	{
		App.WaitForElement("AnimatedButton");
		App.Tap("AnimatedButton");
		App.WaitForElement("GoBackButton");
		ShellFeatureTestActions.WaitForBottomTab(App, "Home");
		ShellFeatureTestActions.TapPageBack(App, "Home");
		App.WaitForElement("AnimatedButton");
		App.WaitForNoElement("GoBackButton");
	}

	[Test, Order(18)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeModal()
	{
		App.WaitForElement("ModalButton");
		App.Tap("ModalButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeModalAnimated()
	{
		App.WaitForElement("ModalAnimatedButton");
		App.Tap("ModalAnimatedButton");
		App.WaitForElement("GoBackButton");
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_PresentationModeModalNotAnimated()
	{
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
	[Category(UITestCategories.Shell)]
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
	[Category(UITestCategories.Shell)]
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
	[Category(UITestCategories.Shell)]
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
		VerifyShellScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS  // Issue Link:https://github.com/dotnet/maui/issues/32993
	[Test, Order(29)]
	[Category(UITestCategories.Shell)]
	public void ShellPages_FlowDirectionRTL()
	{
		AssertShellLayoutDirection(((AppiumApp)App).Driver.PageSource, rightToLeft: false);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.RetryAssert(() => AssertShellLayoutDirection(((AppiumApp)App).Driver.PageSource, rightToLeft: true),
			timeout: TimeSpan.FromSeconds(10));
	}

	static void AssertShellLayoutDirection(string pageSource, bool rightToLeft)
	{
		// WinAppDriver /location uses mirrored window coordinates in RTL; source bounds remain screen-relative.
		var document = System.Xml.Linq.XDocument.Parse(pageSource);
		var show = document.Descendants("Button").Single(element => (string?)element.Attribute("AutomationId") == "NavBarShowButton");
		var hide = document.Descendants("Button").Single(element => (string?)element.Attribute("AutomationId") == "NavBarHideButton");
		var home = document.Descendants("TabItem").Single(element => (string?)element.Attribute("Name") == "Home");
		var lastTab = document.Descendants("TabItem").Single(element => (string?)element.Attribute("Name") == "Tab3");

		foreach (var (first, last) in new[] { (show, hide), (home, lastTab) })
		{
			foreach (var element in new[] { first, last })
			{
				Assert.That((string?)element.Attribute("IsOffscreen"), Is.EqualTo("False"));
				Assert.That((int)element.Attribute("width")!, Is.GreaterThan(0));
				Assert.That((int)element.Attribute("height")!, Is.GreaterThan(0));
			}

			var left = rightToLeft ? last : first;
			var right = rightToLeft ? first : last;
			Assert.That((int)left.Attribute("x")! + (int)left.Attribute("width")!,
				Is.LessThanOrEqualTo((int)right.Attribute("x")!),
				$"Native {first.Name} order must be {(rightToLeft ? "RTL" : "LTR")}.");
		}
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

internal static class ShellFeatureTestActions
{
	internal static IUIElement? FindBottomTab(IApp app, string title)
	{
		// Android uppercases top tabs, not bottom tabs; Windows exposes the title as Name, not AutomationId.
		var query = app switch
		{
			AppiumAndroidApp => AppiumQuery.ByXPath($"//android.widget.FrameLayout[@content-desc='{title}']"),
			AppiumWindowsApp => AppiumQuery.ByXPath($"//TabItem[@Name='{title}']"),
			AppiumIOSApp or AppiumCatalystApp => AppiumQuery.ByXPath($"//XCUIElementTypeTabBar//XCUIElementTypeButton[@label='{title}' or @name='{title}']"),
			_ => throw new NotSupportedException("Unsupported Shell test platform.")
		};
		return app.FindElements(query).FirstOrDefault(element => element.IsDisplayed());
	}

	internal static IUIElement WaitForBottomTab(IApp app, string title) =>
		app.WaitForElement(() => FindBottomTab(app, title));

	internal static void TapPageBack(IApp app, string previousTitle)
	{
		if (app is AppiumIOSApp iosApp && !HelperExtensions.IsIOS26OrHigher(iosApp))
			app.TapBackArrow(previousTitle);
		else
			app.TapBackArrow();
	}
}
