#if IOS || MACCATALYST // The floating glass tab bar is a UIKit-only feature introduced in iOS/MacCatalyst 26. This issue does not affect Android or Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35490 : _IssuesUITest
{
	public Issue35490(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS 26] TabbedPage with NavigationPage children clips content above floating glass tab bar";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	[FailsOnIOSWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void NavigationPageChildContentExtendsUnderFloatingTabBar()
	{
		App.WaitForElement("Tab1Label");
		VerifyScreenshot();
	}
}
#endif
