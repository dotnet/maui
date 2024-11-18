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
        // In the Catalyst App, the TapBackArrow() function does not work when the back button icon is replaced with a new image on the navigation page.
#if MACCATALYST
		App.ClickCoordinates(422.24f,223.03f);
#else
		App.TapBackArrow();
#endif
		App.WaitForElement("Tab 1");
	}
}
