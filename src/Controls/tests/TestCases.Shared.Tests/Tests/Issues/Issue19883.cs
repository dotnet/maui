using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19883 : _IssuesUITest
{
	public override string Issue => "Switch OnColor not applied correctly and ThumbColor not reset when toggled off";

	public Issue19883(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Switch)]
	public void ThumbColorPersistsAcrossToggleAndResetsWhenNull()
	{
		Exception? exception = null;
		App.WaitForElement("TestSwitch");

		// Initial off state: the custom blue ThumbColor is applied.
		VerifyScreenshotOrSetException(ref exception, "Issue19883_ThumbColorBlue");

		// Toggle on: the track uses the Orange OnColor while the thumb stays blue.
		App.Tap("TestSwitch");
		VerifyScreenshotOrSetException(ref exception, "Issue19883_SwitchOn");

		// Toggle off again: the custom blue ThumbColor must be restored and must not
		// revert to the system default (this is the regression reported in the issue).
		App.Tap("TestSwitch");
		VerifyScreenshotOrSetException(ref exception, "Issue19883_SwitchOff");

		// Reset ThumbColor to null at runtime: the thumb must revert to the
		// system default color instead of keeping the previous custom color.
		// This step is performed last because it permanently clears the color.
		App.Tap("ResetThumbColorButton");
		VerifyScreenshotOrSetException(ref exception, "Issue19883_ThumbColorNull");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
