using NUnit.Framework;
using OpenQA.Selenium.DevTools.V131.Runtime;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ShellFeatureTests : UITest
{
	public const string ShellFeatureMatrix = "Shell Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string ShellFlyoutButton = "ShellFlyoutButton";

	public ShellFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ShellFeatureMatrix);
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
		App.WaitForElement("Header");
		App.Tap("OpenFlyout");
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
		App.WaitForElement("HeaderTemplate");
		App.Tap("OpenFlyout");
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
		App.WaitForElement("Footer");
		App.Tap("OpenFlyout");
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
		App.WaitForElement("FooterTemplate");
		App.Tap("OpenFlyout");
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
		App.WaitForElement("FooterTemplate");
		App.WaitForElement("HeaderTemplate");
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(6)]
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
		App.WaitForElement("Footer");
		App.WaitForElement("Header");
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(7)]
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
		App.EnterText("FlyoutHeightEntry", "250");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("Footer");
		App.WaitForElement("Header");
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(8)]
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
		App.WaitForElement("FooterTemplate");
		App.WaitForElement("HeaderTemplate");
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(9)]
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
		App.EnterText("FlyoutHeightEntry", "250");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("FooterTemplate");
		App.WaitForElement("HeaderTemplate");
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

#if TEST_FAILS_ON_ANDROID
	//[Test, Order(10)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutVerticalScrollModeDisabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutVerticalScrollModeDisabled");
		App.Tap("FlyoutVerticalScrollModeDisabled");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		App.WaitForElement("OpenFlyout");
		var openFlyout = App.WaitForElement("OpenFlyout").GetRect();
		var startX = openFlyout.X + (openFlyout.Width / 2);
		var endX = startX;
		var startY = openFlyout.Y + (openFlyout.Height * 8);
		var endY = openFlyout.Y - (openFlyout.Height * 14);
		App.DragCoordinates((float)startX, (float)startY, (float)endX, (float)endY);
		App.WaitForNoElement("MenuItem19", "MenuItem19 not visible after upward drag", TimeSpan.FromSeconds(4));
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}
#endif

	[Test, Order(11)]
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
		App.WaitForElement("OpenFlyout");
		var openFlyout = App.WaitForElement("MenuItem10").GetRect();
		var startX = openFlyout.X + (openFlyout.Width / 2);
		var endX = startX;
		var startY = openFlyout.Y + (openFlyout.Height * 8);
		var endY = openFlyout.Y - (openFlyout.Height * 14);
		App.DragCoordinates((float)startX, (float)startY, (float)endX, (float)endY);
		App.WaitForElement("MenuItem19", "MenuItem19 not visible after upward drag", TimeSpan.FromSeconds(4));
		var bottomItem = App.WaitForElement("MenuItem19").GetRect();
		var reverseStartY = bottomItem.Y - (bottomItem.Height * 4);
		var reverseEndY = bottomItem.Y + (bottomItem.Height * 15);
		App.DragCoordinates((float)endX, (float)reverseStartY, (float)startX, (float)reverseEndY);
		App.WaitForElement("OpenFlyout", "OpenFlyout not visible after reverse drag", TimeSpan.FromSeconds(4));
		App.Tap("OpenFlyout");
	}


	[Test, Order(12)]
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
		var openFlyout = App.WaitForElement("OpenFlyout").GetRect();
		var startX = openFlyout.X + (openFlyout.Width / 2);
		var endX = startX;
		var startY = openFlyout.Y + (openFlyout.Height * 8);
		var endY = openFlyout.Y - (openFlyout.Height * 14);
		App.DragCoordinates((float)startX, (float)startY, (float)endX, (float)endY);
		App.WaitForElement("Header");
		var bottomItem = App.WaitForElement("MenuItem20").GetRect();
		var reverseStartY = bottomItem.Y - (bottomItem.Height * 4);
		var reverseEndY = bottomItem.Y + (bottomItem.Height * 15);
		App.DragCoordinates((float)endX, (float)reverseStartY, (float)startX, (float)reverseEndY);
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(13)]
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
		var openFlyout = App.WaitForElement("OpenFlyout").GetRect();
		var startX = openFlyout.X + (openFlyout.Width / 2);
		var endX = startX;
		var startY = openFlyout.Y + (openFlyout.Height * 8);
		var endY = openFlyout.Y - (openFlyout.Height * 14);
		App.DragCoordinates((float)startX, (float)startY, (float)endX, (float)endY);
		App.WaitForNoElement("Header");
		var bottomItem = App.WaitForElement("MenuItem20").GetRect();
		var reverseStartY = bottomItem.Y - (bottomItem.Height * 4);
		var reverseEndY = bottomItem.Y + (bottomItem.Height * 15);
		App.DragCoordinates((float)endX, (float)reverseStartY, (float)startX, (float)reverseEndY);
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(14)]
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
		var openFlyout = App.WaitForElement("OpenFlyout").GetRect();
		var startX = openFlyout.X + (openFlyout.Width / 2);
		var endX = startX;
		var startY = openFlyout.Y + (openFlyout.Height * 8);
		var endY = openFlyout.Y - (openFlyout.Height * 14);
		App.DragCoordinates((float)startX, (float)startY, (float)endX, (float)endY);
		App.WaitForElement("Header");
		var bottomItem = App.WaitForElement("MenuItem20").GetRect();
		var reverseStartY = bottomItem.Y - (bottomItem.Height * 4);
		var reverseEndY = bottomItem.Y + (bottomItem.Height * 15);
		App.DragCoordinates((float)endX, (float)reverseStartY, (float)startX, (float)reverseEndY);
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(15)]
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
		App.WaitForElement("FlyoutContent Applied");
		App.Tap("CloseFlyoutContentButton");
	}

	[Test, Order(16)]
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
		App.WaitForElement("FlyoutContentTemplate Applied");
		App.Tap("CloseFlyoutContentTemplateButton");
	}

	[Test, Order(17)]
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
		App.WaitForElement("Footer");
		App.WaitForElement("Header");
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
	}

	[Test, Order(18)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutItemVisibility()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutItemVisibilityTrue");
		App.Tap("FlyoutItemVisibilityTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}

	[Test, Order(19)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_DisplayOptionsAsMultipleItems()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutDisplayOptionsAsMultipleItems");
		App.Tap("FlyoutDisplayOptionsAsMultipleItems");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}

	[Test, Order(20)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_Width()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutWidthEntry");
		App.ClearText("FlyoutWidthEntry");
		App.EnterText("FlyoutWidthEntry", "150");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}

	[Test, Order(21)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_Height()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutHeightEntry");
		App.ClearText("FlyoutHeightEntry");
		App.EnterText("FlyoutHeightEntry", "150");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}

	[Test, Order(22)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundColor()
	{
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundColorBlue");
		App.Tap("FlyoutBackgroundColorBlue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}

	[Test, Order(23)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBackDrop()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackdropGradient");
		App.Tap("FlyoutBackdropGradient");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}

	[Test, Order(24)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_DisplayOptionsAsMultipleItemsAndBackgroundColor()
	{
		App.WaitForElement("OpenFlyout"); //// To close the flyout for previous test
		App.Tap("OpenFlyout");
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
		VerifyScreenshot();
	}

	[Test, Order(25)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIsPresentedAndRTL()
	{
		App.WaitForElement("OpenFlyout"); //// To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsPresentedTrue");
		App.Tap("IsPresentedTrue");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(26)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorDisabledAndRTL()
	{
		App.WaitForElement("OpenFlyout"); //// To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBehaviorDisabled");
		App.Tap("FlyoutBehaviorDisabled");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(27)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorLockedAndRTL()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBehaviorLocked");
		App.Tap("FlyoutBehaviorLocked");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(28)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIconAndRTL()
	{
		App.WaitForElement("FlyoutBehavior"); // To close the flyout for previous test
		App.Tap("FlyoutBehavior");
#if IOS
		App.WaitForElement("OpenFlyout");
		App.Tap("OpenFlyout");
#endif
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutIcon");
		App.Tap("FlyoutIcon");
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(29)]
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

	[Test, Order(30)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIsPresented()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsPresentedTrue");
		App.Tap("IsPresentedTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("OpenFlyout");
		VerifyScreenshot();
	}

	[Test, Order(31)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorDisabled()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBehaviorDisabled");
		App.Tap("FlyoutBehaviorDisabled");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(32)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutBehaviorLocked()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBehaviorLocked");
		App.Tap("FlyoutBehaviorLocked");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("OpenFlyout");
		App.WaitForElement("FlyoutBehavior");
		App.Tap("FlyoutBehavior");
	}

	[Test, Order(33)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlyoutIcon()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutIconCoffee");
		App.Tap("FlyoutIconCoffee");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(34)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_FlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(35)]
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
		VerifyScreenshot();
	}

	[Test, Order(36)]
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
		VerifyScreenshot();
	}

	[Test, Order(37)]
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
		VerifyScreenshot();
	}

	[Test, Order(38)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageAspect_AspectFit()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
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
		VerifyScreenshot();
	}

	[Test, Order(39)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageAspect_Fill()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
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
		VerifyScreenshot();
	}

	[Test, Order(40)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_HeightAndWidthWithBackgroundImage()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
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
		VerifyScreenshot();
	}

	[Test, Order(41)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageAndBackDrop()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
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
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_IOS
	[Test, Order(42)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyout_BackgroundImageSetToNull()
	{
		App.WaitForElement("OpenFlyout"); // To close the flyout for previous test
		App.Tap("OpenFlyout");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlyoutBackgroundNoneImage");
		App.Tap("FlyoutBackgroundNoneImage");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.TapShellFlyoutIcon();
		VerifyScreenshot();
	}
#endif
}