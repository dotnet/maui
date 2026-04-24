using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35119 : _IssuesUITest
{
	public Issue35119(TestDevice device) : base(device) { }

	public override string Issue => "[Android] AlertDialog, ActionSheet, and Prompt render with Material 2 styles when Material 3 is enabled";

	[Test]
	[Category(UITestCategories.Material3)]
	public void DialogsRenderWithM3Styling()
	{
		Exception? exception = null;

		App.WaitForElement("ShowAlertButton");

		// Alert Dialog
		App.Tap("ShowAlertButton");
		App.WaitForElement("Alert Title");
		VerifyScreenshotOrSetException(ref exception, "AlertDialog", retryTimeout: TimeSpan.FromSeconds(2));
		App.TapDisplayAlertButton("OK");

		// Action Sheet
		App.Tap("ShowActionSheetButton");
		App.WaitForElement("Action Sheet Title");
		VerifyScreenshotOrSetException(ref exception, "ActionSheet", retryTimeout: TimeSpan.FromSeconds(2));
		App.TapDisplayAlertButton("Cancel", buttonIndex: 1);

		// Prompt Dialog
		App.Tap("ShowPromptButton");
		App.WaitForElement("Prompt Title");
		App.DismissKeyboard();
		VerifyScreenshotOrSetException(ref exception, "PromptDialog", tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
		App.TapDisplayAlertButton("Cancel");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
