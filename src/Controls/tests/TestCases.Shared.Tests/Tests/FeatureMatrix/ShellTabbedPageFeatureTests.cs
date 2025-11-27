using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class ShellTabbedPageFeatureTests : UITest
{
	public const string ShellTabbedFeatureMatrix = "Shell Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	 
	public ShellTabbedPageFeatureTests(TestDevice device)
		: base(device)
	{
	}
	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ShellTabbedFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarBackgroundColor()
    {
		App.WaitForElement("ShellTabbedButton");
		App.Tap("ShellTabbedButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarBackgroundColorLightGreen");
		App.Tap("TabBarBackgroundColorLightGreen");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
    }

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarForegroundColor()
    {
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarForegroundColorBlue");
		App.Tap("TabBarForegroundColorBlue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
    }

	[Test, Order(3)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarTitleColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarTitleColorPurple");
		App.Tap("TabBarTitleColorPurple");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(4)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarUnselectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarUnselectedColorBrown");
		App.Tap("TabBarUnselectedColorBrown");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}	

	[Test, Order(5)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarBackgroundColorAndForegroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarBackgroundColorLightYellow");
		App.Tap("TabBarBackgroundColorLightYellow");
		App.WaitForElement("TabBarForegroundColorBlue");
		App.Tap("TabBarForegroundColorBlue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(6)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarBackgroundColorAndTitleColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarBackgroundColorLightBlue");
		App.Tap("TabBarBackgroundColorLightBlue");
		App.WaitForElement("TabBarTitleColorPurple");
		App.Tap("TabBarTitleColorPurple");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(7)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarBackgroundColorAndUnSelectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarBackgroundColorLightGreen");
		App.Tap("TabBarBackgroundColorLightGreen");
		App.WaitForElement("TabBarUnselectedColorBrown");
		App.Tap("TabBarUnselectedColorBrown");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(8)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarForegroundColorAndUnSelectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarForegroundColorBlue");
		App.Tap("TabBarForegroundColorBlue");
		App.WaitForElement("TabBarUnselectedColorBrown");
		App.Tap("TabBarUnselectedColorBrown");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(9)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarForegroundColorAndTitleColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarForegroundColorGreen");
		App.Tap("TabBarForegroundColorGreen");
		App.WaitForElement("TabBarTitleColorOrange");
		App.Tap("TabBarTitleColorOrange");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(10)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarTitleColorAndUnSelectedColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarTitleColorPurple");
		App.Tap("TabBarTitleColorPurple");
		App.WaitForElement("TabBarUnselectedColorBrown");
		App.Tap("TabBarUnselectedColorBrown");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(11)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_NavigatingTabs()
    {
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("NavigateToTab3");
		App.Tap("NavigateToTab3");
		var text = App.WaitForElement("Tab3Label").GetText();
		Assert.That(text, Is.EqualTo("Welcome to Tab 3"));
		App.WaitForElement("GoToTab1Button");
		App.Tap("GoToTab1Button");
		App.WaitForElement("Apply");
		App.Tap(Apply);
    }

	[Test, Order(12)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_CurrentItemTitleBinding()
	{
		App.WaitForElement("Tab2");
		App.Tap("Tab2");
		var text = App.WaitForElement("Tab2CurrentItemLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Item: Tab2"));
		App.WaitForElement("GoToTab1Button");
		App.Tap("GoToTab1Button");
	}

	[Test, Order(13)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_IsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(14)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarIsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarIsVisibleTrue");
		App.Tap("TabBarIsVisibleTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(15)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarIsEnabledAndTabBarDisabledColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement("TabBarDisabledColorGold");
		App.Tap("TabBarDisabledColorGold");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(16)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarDisabledColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarDisabledColorMaroon");
		App.Tap("TabBarDisabledColorMaroon");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(17)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBar_FlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

}