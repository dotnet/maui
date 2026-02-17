using NUnit.Framework;
using OpenQA.Selenium.DevTools.V131.Runtime;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ShellFeatureTests : _GalleryUITest
{
	public const string ShellFeatureMatrix = "Shell Feature Matrix";
	public override string GalleryPageName => ShellFeatureMatrix;
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string Header = "Header";
	public const string Footer = "Footer";
	public const string HeaderTemplate = "HeaderTemplate";
	public const string FooterTemplate = "FooterTemplate";
	public const string OpenFlyout = "OpenFlyout";
	public const string ShellFlyoutButton = "ShellFlyoutButton";

	public ShellFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_Header()
	{
		App.WaitForElement(ShellFlyoutButton);
		App.Tap(ShellFlyoutButton);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Header);
		App.Tap(OpenFlyout);
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_HeaderTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeaderTemplate");
		App.Tap("FlyoutHeaderTemplate");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(HeaderTemplate);
		App.Tap(OpenFlyout);
	}

	[Test, Order(3)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_Footer()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Footer);
		App.Tap(OpenFlyout);
	}

	[Test, Order(4)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FooterTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutFooterTemplate");
		App.Tap("FlyoutFooterTemplate");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(FooterTemplate);
		App.Tap(OpenFlyout);
	}

	[Test, Order(5)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutHeaderTemplateAndFooterTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeaderTemplate");
		App.Tap("FlyoutHeaderTemplate");
		App.WaitForElement("FlyoutFooterTemplate");
		App.Tap("FlyoutFooterTemplate");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(FooterTemplate);
		App.WaitForElement(HeaderTemplate);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(6)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutHeaderAndFooterTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutFooterTemplate");
		App.Tap("FlyoutFooterTemplate");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(FooterTemplate);
		App.WaitForElement(Header);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(7)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutHeaderTemplateAndFooter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeaderTemplate");
		App.Tap("FlyoutHeaderTemplate");
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Footer);
		App.WaitForElement(HeaderTemplate);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(8)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutHeaderFooter_Width()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement("FlyoutWidthEntry");
		App.ClearText("FlyoutWidthEntry");
		App.EnterText("FlyoutWidthEntry", "250");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Footer);
		App.WaitForElement(Header);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(9)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutHeaderFooter_Height()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement("FlyoutHeightEntry");
		App.ClearText("FlyoutHeightEntry");
		App.EnterText("FlyoutHeightEntry", "350");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Footer);
		App.WaitForElement(Header);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(10)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutHeaderFooterTemplate_Width()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeaderTemplate");
		App.Tap("FlyoutHeaderTemplate");
		App.WaitForElement("FlyoutFooterTemplate");
		App.Tap("FlyoutFooterTemplate");
		App.WaitForElement("FlyoutWidthEntry");
		App.ClearText("FlyoutWidthEntry");
		App.EnterText("FlyoutWidthEntry", "250");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(FooterTemplate);
		App.WaitForElement(HeaderTemplate);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(11)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutHeaderFooterTemplate_Height()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeaderTemplate");
		App.Tap("FlyoutHeaderTemplate");
		App.WaitForElement("FlyoutFooterTemplate");
		App.Tap("FlyoutFooterTemplate");
		App.WaitForElement("FlyoutHeightEntry");
		App.ClearText("FlyoutHeightEntry");
		App.EnterText("FlyoutHeightEntry", "350");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(FooterTemplate);
		App.WaitForElement(HeaderTemplate);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(12)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutVerticalScrollModeEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutVerticalScrollModeEnabled");
		App.Tap("FlyoutVerticalScrollModeEnabled");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem4");
		var initialMenuItem4Rect = App.WaitForElement("MenuItem4").GetRect();
		float initialMenuItem4Y = initialMenuItem4Rect.Y;
		App.ScrollDown("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollDown("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		var afterScrollMenuItem4Rect = App.WaitForElement("MenuItem4").GetRect();
		float afterScrollMenuItem4Y = afterScrollMenuItem4Rect.Y;
		Assert.That(initialMenuItem4Y, Is.GreaterThan(afterScrollMenuItem4Y),
			$"MenuItem4 should move up after scrolling. Initial Y: {initialMenuItem4Y}, After scroll Y: {afterScrollMenuItem4Y}");
		App.ScrollUp("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollUp("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement(OpenFlyout); 
		App.Tap(OpenFlyout);
	}

	[Test, Order(13)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutHeaderBehaviorFixed()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutHeaderBehaviorFixed");
		App.Tap("FlyoutHeaderBehaviorFixed");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Header);
		float startingHeight = App.WaitForElement(Header).GetRect().Y; 
		App.ScrollDown("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollDown("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement(Header);
		float endHeight = App.WaitForElement(Header).GetRect().Y;
		Assert.That(startingHeight, Is.EqualTo(endHeight).Within(1),
			$"Starting: {startingHeight}, End: {endHeight}");
		App.ScrollUp("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollUp("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}
 
	[Test, Order(14)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutHeaderBehaviorScroll()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutHeaderBehaviorScroll");
		App.Tap("FlyoutHeaderBehaviorScroll");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Header);
		float startingHeight =	App.WaitForElement(Header).GetRect().Y;
		App.ScrollDown("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollDown("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement(Header);
		float endHeight = App.WaitForElement(Header).GetRect().Y;
		Assert.That(startingHeight, Is.GreaterThan(endHeight).Within(1),
			$"Starting: {startingHeight}, End: {endHeight}");
		App.ScrollUp("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollUp("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}
 
	[Test, Order(15)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutHeaderBehaviorCollapseOnScroll()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutHeaderBehaviorCollapseOnScroll");
		App.Tap("FlyoutHeaderBehaviorCollapseOnScroll");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		float startingHeight =	App.WaitForElement(Header).GetRect().Y;
		App.ScrollDown("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollDown("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement(Header);
		float endHeight = App.WaitForElement(Header).GetRect().Y;
		Assert.That(startingHeight, Is.GreaterThan(endHeight).Within(1),
			$"Starting: {startingHeight}, End: {endHeight}");
		App.ScrollUp("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollUp("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(16)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutContent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutTemplateFlyoutContent");
		App.Tap("FlyoutTemplateFlyoutContent");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.TapShellFlyoutIcon();
		App.WaitForElement("FlyoutContent");
		App.Tap("CloseFlyoutContentButton");
	}

	[Test, Order(17)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutContentTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutTemplateFlyoutContentTemplate");
		App.Tap("FlyoutTemplateFlyoutContentTemplate");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.TapShellFlyoutIcon();
		App.WaitForElement("FlyoutContentTemplate");
		App.Tap("CloseFlyoutContentTemplateButton");
	}

	[Test, Order(18)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutHeaderAndFooter()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(Footer);
		App.WaitForElement(Header);
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
	}

	[Test, Order(19)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutItemVisibility()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutItemVisibilityFalse");
		App.Tap("FlyoutItemVisibilityFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.TapShellFlyoutIcon();
		App.ScrollUp("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000); // FlyoutItems are scrolled down, when changing the FlyoutContent
		App.ScrollUp("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(20)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_DisplayOptionsAsMultipleItems()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutDisplayOptionsAsMultipleItems");
		App.Tap("FlyoutDisplayOptionsAsMultipleItems");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(21)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_Width()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutWidthEntry");
		App.ClearText("FlyoutWidthEntry");
		App.EnterText("FlyoutWidthEntry", "150");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(22)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_Height()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeightEntry");
		App.ClearText("FlyoutHeightEntry");
		App.EnterText("FlyoutHeightEntry", "300");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(23)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundColor()
	{
		App.WaitForElement(OpenFlyout);
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundColorBlue");
		App.Tap("FlyoutBackgroundColorBlue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(24)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBackDrop()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackdropGradient");
		App.Tap("FlyoutBackdropGradient");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(25)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_DisplayOptionsAsMultipleItemsAndBackgroundColor()
	{
		App.WaitForElement(OpenFlyout); //// To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutDisplayOptionsAsMultipleItems");
		App.Tap("FlyoutDisplayOptionsAsMultipleItems");
		App.WaitForElement("FlyoutBackgroundColorBlue");
		App.Tap("FlyoutBackgroundColorBlue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(26)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageAspect_AspectFit()
	{
		App.WaitForElement(OpenFlyout); //// To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundDotnetImage");
		App.Tap("FlyoutBackgroundDotnetImage");
		App.WaitForElement("FlyoutBackgroundImageAspectFit");
		App.Tap("FlyoutBackgroundImageAspectFit");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(27)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageAspect_Fill()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundDotnetImage");
		App.Tap("FlyoutBackgroundDotnetImage");
		App.WaitForElement("FlyoutBackgroundImageFill");
		App.Tap("FlyoutBackgroundImageFill");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(28)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_HeightAndWidthWithBackgroundImage()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundDotnetImage");
		App.Tap("FlyoutBackgroundDotnetImage");
		App.WaitForElement("FlyoutWidthEntry");
		App.ClearText("FlyoutWidthEntry");
		App.EnterText("FlyoutWidthEntry", "300");
		App.WaitForElement("FlyoutHeightEntry");
		App.ClearText("FlyoutHeightEntry");
		App.EnterText("FlyoutHeightEntry", "400");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(29)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageAndBackDrop()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundDotnetImage");
		App.Tap("FlyoutBackgroundDotnetImage");
		App.WaitForElement("FlyoutBackdropGradient");
		App.Tap("FlyoutBackdropGradient");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/32416
	[Test, Order(30)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutVerticalScrollModeDisabled()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutVerticalScrollModeDisabled");
		App.Tap("FlyoutVerticalScrollModeDisabled");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement(OpenFlyout);
		App.ScrollDown("MenuItem1", ScrollStrategy.Gesture, 0.99, 1000);
		App.ScrollDown("MenuItem2", ScrollStrategy.Gesture, 0.99, 1000);
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(31)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageSetToNull()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundNoneImage");
		App.Tap("FlyoutBackgroundNoneImage");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_MACCATALYST // Issue Link: https://github.com/dotnet/maui/issues/32417
	[Test, Order(32)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_SetItemTemplateAndMenuItemTemplate()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MenuItemTemplateBasic");
		App.Tap("MenuItemTemplateBasic");
		App.WaitForElement("ItemTemplateBasic");
		App.Tap("ItemTemplateBasic");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif

	[Test, Order(33)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorDisabled()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBehaviorDisabled");
		App.Tap("FlyoutBehaviorDisabled");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		FlyoutScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_MACCATALYST && TEST_FAILS_ON_WINDOWS// Issue Link: https://github.com/dotnet/maui/issues/32419, https://github.com/dotnet/maui/issues/32476
	[Test, Order(34)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorDisabledAndRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBehaviorDisabled");
		App.Tap("FlyoutBehaviorDisabled");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		FlyoutScreenshot();
	}
	
	[Test, Order(35)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIconAndRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutIcon");
		App.Tap("FlyoutIcon");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		FlyoutScreenshot();
	}
#endif

	[Test, Order(36)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_CurrentItemWithDisplayOptions()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutDisplayOptionsAsMultipleItems");
		App.Tap("FlyoutDisplayOptionsAsMultipleItems");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.TapShellFlyoutIcon();
		App.WaitForElement("Home");
		App.Tap("Home");
		var text1 = App.WaitForElement("HomeCurrentItemLabel").GetText();
		Assert.That(text1, Is.EqualTo("Current Item: Home"));
	}

	[Test, Order(37)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIcon()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutIconCoffee");
		App.Tap("FlyoutIconCoffee");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		FlyoutScreenshot();
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_MACCATALYST && TEST_FAILS_ON_WINDOWS// Issue Link: https://github.com/dotnet/maui/issues/32419, https://github.com/dotnet/maui/issues/32476
	[Test, Order(38)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		FlyoutScreenshot();
	}
#endif

	[Test, Order(39)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_ShellContentAndBackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundColorBlue");
		App.Tap("FlyoutBackgroundColorBlue");
		App.WaitForElement("FlyoutTemplateFlyoutContent");
		App.Tap("FlyoutTemplateFlyoutContent");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.TapShellFlyoutIcon();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(40)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_ShellContentAndBackgroundImage()
	{
		App.WaitForElement("CloseFlyoutContentButton"); // To close the flyout for previous test
		App.Tap("CloseFlyoutContentButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundDotnetImage");
		App.Tap("FlyoutBackgroundDotnetImage");
		App.WaitForElement("FlyoutTemplateFlyoutContent");
		App.Tap("FlyoutTemplateFlyoutContent");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.TapShellFlyoutIcon();
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(41)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImage()
	{
		App.WaitForElement("CloseFlyoutContentButton"); // To close the flyout for previous test
		App.Tap("CloseFlyoutContentButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundDotnetImage");
		App.Tap("FlyoutBackgroundDotnetImage");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(42)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageWithHeaderAndFooter()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement("FlyoutBackgroundDotnetImage");
		App.Tap("FlyoutBackgroundDotnetImage");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(43)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundColorWithHeaderAndFooter()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement("FlyoutBackgroundColorBlue");
		App.Tap("FlyoutBackgroundColorBlue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(44)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_DisplayOptionsWithHeaderAndFooter()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeader");
		App.Tap("FlyoutHeader");
		App.WaitForElement("FlyoutFooter");
		App.Tap("FlyoutFooter");
		App.WaitForElement("FlyoutDisplayOptionsAsMultipleItems");
		App.Tap("FlyoutDisplayOptionsAsMultipleItems");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(45)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_DisplayOptionsWithHeaderTemplateAndFooterTemplate()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeaderTemplate");
		App.Tap("FlyoutHeaderTemplate");
		App.WaitForElement("FlyoutFooterTemplate");
		App.Tap("FlyoutFooterTemplate");
		App.WaitForElement("FlyoutDisplayOptionsAsMultipleItems");
		App.Tap("FlyoutDisplayOptionsAsMultipleItems");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(46)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_HeightAndWidthWithBackgroundColor()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundColorBlue");
		App.Tap("FlyoutBackgroundColorBlue");
		App.WaitForElement("FlyoutWidthEntry");
		App.ClearText("FlyoutWidthEntry");
		App.EnterText("FlyoutWidthEntry", "300");
		App.WaitForElement("FlyoutHeightEntry");
		App.ClearText("FlyoutHeightEntry");
		App.EnterText("FlyoutHeightEntry", "400");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(47)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIsPresented()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsPresentedTrue");
		App.Tap("IsPresentedTrue");
		App.WaitForElement(OpenFlyout); // verify the flyout is opened
		App.Tap(OpenFlyout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
	}

	[Test, Order(48)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorLocked()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBehaviorLocked");
		App.Tap("FlyoutBehaviorLocked");
		App.WaitForElement(OpenFlyout);
		App.WaitForElement("FlyoutBehavior");
		App.Tap("FlyoutBehavior");
#if IOS || MACCATALYST
        App.WaitForElement(OpenFlyout); // On iOS, tapping the FlyoutBehavior does not close the flyout
		App.Tap(OpenFlyout);
#endif
		App.WaitForElement(Apply);
		App.Tap(Apply);
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_MACCATALYST && TEST_FAILS_ON_WINDOWS  //Issue Link: https://github.com/dotnet/maui/issues/32419, https://github.com/dotnet/maui/issues/32476
	[Test, Order(49)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIsPresentedAndRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("IsPresentedTrue");
		App.Tap("IsPresentedTrue");
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test, Order(50)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorLockedAndRTL()
	{
		App.WaitForElement(OpenFlyout); // To close the flyout for previous test
		App.Tap(OpenFlyout);
		App.WaitForElement(Apply);
		App.Tap(Apply); // Reset previous test settings	
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement("FlyoutBehaviorLocked");
		App.Tap("FlyoutBehaviorLocked");
		App.WaitForElement("MenuItem1");
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
#endif
	public void FlyoutScreenshot() // This method is to show titlebar for Screenshot
	{
#if WINDOWS
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2), includeTitleBar: true);
#else
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
#endif
	}
}