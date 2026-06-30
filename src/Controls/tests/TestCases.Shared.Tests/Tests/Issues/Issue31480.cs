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
}
