using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33909 : _IssuesUITest
{
	public override string Issue => "[iOS, Android, Catalyst] Shell.ForegroundColor does not reset correctly for the back button";

	public Issue33909(TestDevice testDevice) : base(testDevice)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void Issue33909ForegroundColorReset()
	{
		App.WaitForElement("ApplyColorButton");
		App.Tap("ApplyColorButton");
		App.WaitForElement("RemoveColorButton");
		App.Tap("RemoveColorButton");
		App.WaitForElement("NavigateButton");
		App.Tap("NavigateButton");
		VerifyScreenshot();
	}
}