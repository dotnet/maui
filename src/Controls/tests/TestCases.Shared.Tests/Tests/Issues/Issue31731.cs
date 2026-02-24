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

		// Wait for a moment to ensure dialog is open, then wait for auto navigation
		// The page will automatically pop after 3 seconds
		// If the bug exists, this would cause a crash
		System.Threading.Thread.Sleep(4000); // Wait longer than the 3-second delay

		// If we reach this point without crashing, the test passes
		// Verify we're back on the main page
		App.WaitForElement("statusLabel");
		App.WaitForElement("navigateButton");
	}
}