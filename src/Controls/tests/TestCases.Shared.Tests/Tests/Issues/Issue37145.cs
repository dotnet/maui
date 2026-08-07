using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37145 : _IssuesUITest
{
	public Issue37145(TestDevice device) : base(device)
	{
	}

	public override string Issue => "RadioButton border properties not cleared when reset to default values on Android";

	[Test]
	[Category(UITestCategories.RadioButton)]
	public void VerifyRadioButtonBorderClearsWhenResetToDefaults()
	{
		App.WaitForElement("TestRadioButton");

		Exception? exception = null;

		// Initial state — border should be visible (Green, width 3, corner radius 10)
		VerifyScreenshotOrSetException(ref exception, "RadioButtonBorderSet");

		// Tap clear button to reset all border properties to defaults
		App.Tap("ClearBorderButton");
		VerifyScreenshotOrSetException(ref exception, "RadioButtonBorderCleared");

		// Tap set button to re-apply border — verifies border can be set again after clearing
		App.Tap("SetBorderButton");
		VerifyScreenshotOrSetException(ref exception, "RadioButtonBorderReapplied");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
