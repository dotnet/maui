using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32406 : _IssuesUITest
{
	public override string Issue => "LayoutCycleException caused by nested Borders in ControlTemplates";

	public Issue32406(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Border)]
	public void NestedBordersShouldNotCauseLayoutCycle()
	{
		// If the app crashes with a LayoutCycleException, we'll never reach this point.
		// Wait for the Loaded handler to set "Success" text, avoiding a race with element existence.
		App.WaitForTextToBePresentInElement("ResultLabel", "Success", timeout: TimeSpan.FromSeconds(30));
	}
}
