using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31480 : _IssuesUITest
{
	public Issue31480(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "RightToLeft does not apply for FormattedText";
	
	[Test]
	[Category(UITestCategories.Label)]
	public void FormattedTextToggleFlowDirectionTest()
	{
		Exception? exception = null;
		
		// Verify initial state (LTR)
		App.WaitForElement("FormattedTextLabel");
		App.WaitForElement("ToggleFlowDirection");
		VerifyScreenshotOrSetException(ref exception, "Issue81_InitialLTR");

		// Toggle to RTL
		App.Tap("ToggleFlowDirection");
		VerifyScreenshotOrSetException(ref exception, "Issue81_ToggledRTL");

		// Toggle back to LTR
		App.Tap("ToggleFlowDirection");
		VerifyScreenshotOrSetException(ref exception, "Issue81_ToggledBackLTR");
		
		if (exception != null)
		{
			throw exception;
		}
	}
}