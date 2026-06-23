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
		App.WaitForElement("LTRFormattedLabel");

		var rtlRect = App.FindElement("RTLFormattedLabel").GetRect();
		var ltrRect = App.FindElement("LTRFormattedLabel").GetRect();

		// Both labels have the same width container; with RTL FlowDirection the text
		// should be right-aligned, so the right edge (X + Width) of the RTL label
		// should be further right (or the label layout should differ visually).
		// We verify via screenshot that RTL renders differently from LTR.
		VerifyScreenshot();
	}
}
