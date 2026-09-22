using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7396 : _IssuesUITest
{
	public Issue7396(TestDevice testDevice) : base(testDevice)
	{
	}

	const string CreateTopTabButton = "CreateTopTabButton";
	const string CreateBottomTabButton = "CreateBottomTabButton";
	const string ChangeShellColorButton = "ChangeShellBackgroundColorButton";

	public override string Issue => "Setting Shell.BackgroundColor overrides all colors of TabBar";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void BottomTabColorTest()
	{
		App.WaitForElement(CreateBottomTabButton);
		App.Tap(CreateBottomTabButton);
		App.Tap(CreateBottomTabButton);
		App.Tap(ChangeShellColorButton);
		VerifyScreenshot();
	}
}