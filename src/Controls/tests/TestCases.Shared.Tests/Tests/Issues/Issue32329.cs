using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32329 : _IssuesUITest
{
	public Issue32329(TestDevice device) : base(device)
	{
	}

	public override string Issue => "TabBar not visible on Mac Catalyst";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void TabBarShouldBeVisibleOnMacCatalyst()
	{
		App.WaitForElement("HomePageLabel");
		VerifyScreenshot();
	}
}
