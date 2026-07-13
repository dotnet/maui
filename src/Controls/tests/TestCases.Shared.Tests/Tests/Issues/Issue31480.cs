using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31480 : _IssuesUITest
{
	public Issue31480(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Label FormattedText does not respect FlowDirection RightToLeft";

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelFormattedTextShouldRespectRTLFlowDirection()
	{
		App.WaitForElement("RTLFormattedLabel");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Label)]
	public void LabelFormattedTextShouldUpdateWhenFlowDirectionChanges()
	{
		App.WaitForElement("RTLFormattedLabel");

		// Toggle the flow direction after the initial render so the dynamic
		// MapFlowDirection rebuild path is exercised (the RTL label becomes
		// left-aligned and the LTR label becomes right-aligned).
		App.Tap("ToggleFlowDirectionButton");
		App.WaitForElement("RTLFormattedLabel");
		VerifyScreenshot();
	}
}
