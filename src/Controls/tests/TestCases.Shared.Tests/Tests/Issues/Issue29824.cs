#if TEST_FAILS_ON_CATALYST // Tab bar does not appear on Catalyst, therefore unable to tab on tab items, this issue has been resolved in the following PR https://github.com/dotnet/maui/pull/32528
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29824 : _IssuesUITest
{
	public Issue29824(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Colors Do Not Update Correctly When Switching Between TabBar Items";


	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void ShellAppearanceUpdatesWhenChangingShellSectionToTab_1()
	{
		App.WaitForElement("HomePageButton");
		App.Tap("HomePageButton");
		App.Tap("Settings");
		App.Tap("Home");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void ShellAppearanceUpdatesWhenChangingShellSectionToTab_2()
	{
		App.WaitForElement("Settings");
		App.Tap("Settings");
		GoToHomeTab();
#if WINDOWS
		App.TapBackArrow();
#else
		App.Back();
#endif
		GoToSettingTab();
		VerifyScreenshot();
	}

	[Test, Order(3)]
	[Category(UITestCategories.Shell)]
	public void ShellAppearanceUpdatesWhenChangingBetweenTabs()
	{
		App.WaitForElement("SettingsPageButton");
		App.Tap("SettingsPageButton");
		App.Tap("Home");
		GoToSettingTab();
		App.Tap("Home");
		VerifyScreenshot();
	}

	void GoToSettingTab()
	{
		App.WaitForElement("HomePageButton");
		App.Tap("Settings");
	}

	void GoToHomeTab()
	{
		App.WaitForElement("SettingsPageButton");
		App.Tap("Home");
	}
}
#endif