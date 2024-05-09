using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

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

	[TestCase("SwapNavigationPage")]
	[TestCase("SwapShellPage")]
	public void NavBarEntryDoesNotTriggerKeyboardScroll(string scenario)
	{
		try
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
			Assert.AreEqual(navBarLocation, newNavBarEntryLocation);

			var newHeaderEntry = App.WaitForElement(HeaderEntry);
			var newHeaderLocation = newHeaderEntry.GetRect();

			Assert.AreEqual(headerLocation, newHeaderLocation);

			App.WaitForElement(RestoreButton);
			App.Click(RestoreButton);
		}
		catch
		{
			// Just in case these tests leave the app in an unreliable state
			App.ResetApp();
			FixtureSetup();
			throw;
		}
	}
}
