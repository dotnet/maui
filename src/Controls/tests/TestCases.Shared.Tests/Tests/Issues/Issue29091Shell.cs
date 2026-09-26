using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29091Shell : _IssuesUITest
{
	public Issue29091Shell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell - Auto Resize chrome icons on iOS to make it more consistent with other platforms - TabBar";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void TabBarIconsShouldAutoscaleShell()
	{
		App.WaitForElement("Tab1");
		VerifyScreenshot();
	}
}