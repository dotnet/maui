using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7045 : _IssuesUITest
{
	public Issue7045(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell BackButtonBehavior binding Command to valid ICommand causes back button to disappear";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void ShellBackButtonBehaviorShouldWork()
	{
		App.WaitForElement("NavigateButton");
		App.Click("NavigateButton");
		App.WaitForElement("DetailPageLabel");
		VerifyScreenshot();
	}
}
