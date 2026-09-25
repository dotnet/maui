using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30771 : _IssuesUITest
{
	public Issue30771(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "SearchHandler overlaps title and title view";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void SearchHandlerShouldNotOverlap()
	{
		App.EnterText("Search here", "Test Search");
		App.EnterText("SearchEntry", "Test Search");
		VerifyScreenshot();
	}
}
