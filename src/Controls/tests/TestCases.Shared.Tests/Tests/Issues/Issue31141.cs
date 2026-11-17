using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31141 : _IssuesUITest
{
	public Issue31141(TestDevice device) : base(device)
	{
	}

	public override string Issue => "IndicatorView template remains visible with right-to-left flow direction";

	[Test, Order(1)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorViewWithRightToLeftFlowDirection()
	{
		App.WaitForElement("ToggleFlowDirection");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.IndicatorView)]
	public void VerifyIndicatorViewWithLeftToRightFlowDirection()
	{
		App.WaitForElement("ToggleFlowDirection");
		App.Tap("ToggleFlowDirection");
		VerifyScreenshot();
	}
}