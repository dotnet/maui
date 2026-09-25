#if TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/26148
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32867 : _IssuesUITest
{
	public Issue32867(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Shell Flyout Icon is always black";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void ShellFlyoutIconShouldNotBeBlack()
	{
		App.WaitForElement("Issue32867Label");
		VerifyScreenshot();
	}
}
#endif