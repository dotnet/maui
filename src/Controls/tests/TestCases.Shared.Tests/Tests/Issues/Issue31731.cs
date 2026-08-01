using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31731 : _IssuesUITest
{
	public Issue31731(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Picker dialog causes crash when page is popped while dialog is open";

	[Test]
	[Category(UITestCategories.Picker)]
	public void PickerDialogDoesNotCrashWhenPagePoppedWhileDialogOpen()
	{
		// Wait for the main page to load
		App.WaitForElement("statusLabel");
		App.WaitForElement("navigateButton");

		// Navigate to the picker page
		App.Tap("navigateButton");

		// Wait for picker page to load
		App.WaitForElement("colorPicker");
		App.WaitForElement("pageStatusLabel");

		// Open the picker dialog
		App.Tap("colorPicker");

		// Opening the dialog triggers an automatic pop back to the main page.
		// Wait for the main page to reappear instead of sleeping a fixed amount
		// of time; if the bug regresses the app crashes and these waits fail.
		App.WaitForElement("statusLabel");
		App.WaitForElement("navigateButton");
	}
}