#if TEST_FAILS_ON_CATALYST // In Catalyst App.TapBackArrow() not working when override the back button icon of the navigation page.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12320 : _IssuesUITest
{
	public Issue12320(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] TabBarIsVisible = True/False doesn't work on Back Navigation When using BackButtonBehavior";

	[Test]
	[Category(UITestCategories.Shell)]
	public void PopLogicExecutesWhenUsingBackButtonBehavior()
	{
		App.WaitForElement("TestReady");
		App.TapBackArrow();
		App.WaitForElement("Tab 1");
	}
}
#endif