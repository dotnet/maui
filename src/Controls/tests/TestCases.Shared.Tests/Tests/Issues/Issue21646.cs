#if ANDROID || WINDOWS // Temporary net11 branch divergence: PR #35604 on main reverted this scenario.
// When net11 syncs with main: either remove this guard if the fix is reintroduced, or delete this test if the full revert lands here.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21646: _IssuesUITest
{
	public Issue21646(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout icon should remain visible when a page is pushed onto a NavigationPage with the back button disabled.";

	[Test]
	[Category(UITestCategories.Navigation)]
	public void FlyoutIconShouldBeVisibleWithBackButtonDisabledInNavigationPage()
	{
		App.WaitForElement("NavigateToNextPageButton");
		App.Tap("NavigateToNextPageButton");
		App.WaitForElement("SecondPageLabel");
		App.WaitForFlyoutIcon(FlyoutIconAutomationId, false);
	}
}
#endif