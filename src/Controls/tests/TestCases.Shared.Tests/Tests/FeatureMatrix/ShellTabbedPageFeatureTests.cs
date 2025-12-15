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
	public void VerifyShell_TabBarTitleColor()
	{
		App.WaitForElement("ShellTabbedButton");
		App.Tap("ShellTabbedButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarTitleColorPurple");
		App.Tap("TabBarTitleColorPurple");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/33158
	[Test, Order(2)]
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

	[Test, Order(3)]
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
#endif

	[Test, Order(4)]
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

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/33158
	[Test, Order(5)]
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
#endif

	[Test, Order(6)]
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

	[Test, Order(7)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_CurrentItemTitleBinding()
	{
		App.WaitForElement("Tab3");
		App.Tap("Tab3");
		var text = App.WaitForElement("Tab3CurrentItemLabel").GetText();
		Assert.That(text, Is.EqualTo("Current Item: Tab3"));
		App.WaitForElement("GoToTab1Button");
		App.Tap("GoToTab1Button");
	}

	[Test, Order(8)]
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

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/32995, https://github.com/dotnet/maui/issues/32996
	[Test, Order(9)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarDisabledColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement("TabBarDisabledColorMaroon");
		App.Tap("TabBarDisabledColorMaroon");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32996, https://github.com/dotnet/maui/issues/33158
	[Test, Order(10)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarIsEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("IsEnabledFalse");
		App.Tap("IsEnabledFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Tab3");
		App.Tap("Tab3");
		App.WaitForNoElement("Tab3Label");
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/32993
	[Test, Order(11)]
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
#endif

	[Test, Order(12)]
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

#if TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/33158
	[Test, Order(13)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarBackgroundColor()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarBackgroundColorLightGreen");
		App.Tap("TabBarBackgroundColorLightGreen");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test, Order(14)]
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

	[Test, Order(15)]
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

	[Test, Order(16)]
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
#endif 

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST// Issue Link: https://github.com/dotnet/maui/issues/32992, https://github.com/dotnet/maui/issues/33158
	[Test, Order(17)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarBackgroundColorSetToNull()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarBackgroundColorLightGreen");
		App.Tap("TabBarBackgroundColorLightGreen");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST //Issue Link:  https://github.com/dotnet/maui/issues/32994
	[Test, Order(18)]
	[Category(UITestCategories.Shell)]
	public void VerifyShell_TabBarIsVisible()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("TabBarIsVisibleTrue");
		App.Tap("TabBarIsVisibleTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Tab3");
		App.Tap("Tab3");
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/6740
   [Test, Order(19)]
   [Category(UITestCategories.Shell)]
   public void VerifyShell_TabBarBackgroundColorWithMoreTabs()
   {
	   App.WaitForElement("GoToTab1Button"); // Navigate to Tab1 to go back to initial tab
	   App.Tap("GoToTab1Button");
	   App.WaitForElement(Options);
	   App.Tap(Options);
	   App.WaitForElement("TabBarBackgroundColorLightBlue");
	   App.Tap("TabBarBackgroundColorLightBlue");
	   App.WaitForElement(Apply);
	   App.Tap(Apply);
	   App.WaitForElement("More");
	   App.Tap("More");
	   VerifyScreenshot();
   }

   [Test, Order(20)]
   [Category(UITestCategories.Shell)]
   public void VerifyShell_TabBarUnSelectedColorWithMoreTabs()
   {
#if IOS || MACCATALYST
      App.WaitForElement("Tab1"); // Navigate to Tab1 to close more button
	  App.Tap("Tab1");
#else
	  App.WaitForElement("Tab11");
	  App.Tap("Tab11");
	  App.WaitForElement("GoToTab1Button");
	  App.Tap("GoToTab1Button");
#endif
	   App.WaitForElement(Options);
	   App.Tap(Options);
	   App.WaitForElement("TabBarUnselectedColorBrown");
	   App.Tap("TabBarUnselectedColorBrown");
	   App.WaitForElement(Apply);
	   App.Tap(Apply);
	   App.WaitForElement("More");
	   App.Tap("More");
	   VerifyScreenshot();
   }
#endif
}