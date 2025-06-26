﻿#if ANDROID || IOS // This is purely testing softinput keyboard behavior on mobile platforms
using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21630 : _IssuesUITest
{
	public override string Issue => "Entries in NavBar don't trigger keyboard scroll";

	public Issue21630(TestDevice device)
		: base(device)
	{ }

	string NavBarEntry => "NavBarEntry";
	string HeaderEntry => "HeaderEntry";
	string FocusButton => "FocusButton";
	string RestoreButton => "RestoreMainPageButton";

	protected override bool ResetAfterEachTest => true;

	[Theory]
		[InlineData("SwapNavigationPage", Category = UITestCategories.Entry)]
	[Theory]
		[InlineData("SwapShellPage", Category = UITestCategories.Entry)]

	public void NavBarEntryDoesNotTriggerKeyboardScroll(string scenario)
	{
		var scenarioSuffix = scenario == "SwapNavigationPage" ? "NavigationPage" : "ShellPage";

		App.WaitForElement(scenario);
		App.Click(scenario);

		var navBarEntry = App.WaitForElement(NavBarEntry + scenarioSuffix);
		var navBarLocation = navBarEntry.GetRect();
		var headerEntry = App.WaitForElement(HeaderEntry);
		var headerLocation = headerEntry.GetRect();

		App.Click(FocusButton + scenarioSuffix);

		var newNavBarEntry = App.WaitForElement(NavBarEntry + scenarioSuffix);
		var newNavBarEntryLocation = newNavBarEntry.GetRect();
		Assert.Equal(navBarLocation, newNavBarEntryLocation);

		var newHeaderEntry = App.WaitForElement(HeaderEntry);
		var newHeaderLocation = newHeaderEntry.GetRect();

		Assert.Equal(headerLocation, newHeaderLocation);

		App.WaitForElement(RestoreButton);
		App.Click(RestoreButton);
	}
}
#endif