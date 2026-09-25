#if MACCATALYST || WINDOWS  // Native window controls (minimize, maximize, close) are applicable only on Windows and MacCatalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32476 : _IssuesUITest
{
	public Issue32476(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Binding RTL FlowDirection in Shell causes Flyout MenuIcon and native window controls to overlap";
	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnMacWhenRunningOnXamarinUITest("Flaky in CI (failed->passed on same SHA on net11.0; see ui-flake-quarantine-20260922.csv). Re-enable after flakiness investigation.")]
	public void ShellRTLFlowDirectionShouldNotCauseOverlap()
	{
		App.WaitForElement("ToggleButton");
		App.Tap("ToggleButton");
		App.Tap("ToggleButton");

		// wait for window layout to update after flow direction change.
		Task.Delay(500).Wait();
		VerifyScreenshot(includeTitleBar: true);
	}
}
#endif
