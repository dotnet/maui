#if !IOS // iOS DatePicker uses a wheel picker, not a calendar dialog - cannot verify disabled dates visually
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33583 : _IssuesUITest
{
#if ANDROID
	const string DismissDatePicker = "Cancel";
#elif WINDOWS
	const string DismissDatePicker = "16"; // Tap a date to close
#elif MACCATALYST
    const string DismissDatePicker = "ChangeMaxDateButton"; // Tap another element to dismiss/unfocus
#endif

    public Issue33583(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "DatePicker does not update MinimumDate / MaximumDate in the Popup after first opening";

    [Test]
    [Category(UITestCategories.DatePicker)]
    public void VerifyMaximumDateInRuntime()
    {
        // Wait for the page to load
        App.WaitForElement("TestDatePicker");

        // Step 1: Open the date picker the first time by tapping it directly
        App.Tap("TestDatePicker");

        // Dismiss the dialog
        App.Tap(DismissDatePicker);

        // Step 2: Change the MaximumDate to 2027
        App.WaitForElement("ChangeMaxDateButton");
        App.Tap("ChangeMaxDateButton");

        // Step 3: Open the date picker again - this is where the bug would occur
        // Before the fix, the dialog would still show the old MaximumDate constraints
        App.Tap("TestDatePicker");

        // Verify the DatePicker dialog shows updated constraints via screenshot
        // With the fix, dates beyond Jan 28 should now be selectable
        VerifyScreenshot();
    }
}
#endif
