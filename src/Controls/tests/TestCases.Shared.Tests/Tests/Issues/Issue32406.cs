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
		// Wait for the page to load and verify the success label.
		App.WaitForElement("ResultLabel", timeout: TimeSpan.FromSeconds(30));

		var text = App.FindElement("ResultLabel").GetText();
		Assert.That(text, Is.EqualTo("Success"));
	}
}
