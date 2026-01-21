#if TEST_FAILS_ON_IOS // iOS DatePicker uses a wheel picker, not a calendar view - cannot verify disabled dates visually
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19256 : _IssuesUITest
{
#if ANDROID
	const string DismissDatePicker = "Cancel";
#elif WINDOWS
	const string DismissDatePicker = "25"; // Tap the enabled MinimumDate to close
#elif MACCATALYST
	const string DismissDatePicker = "SetEarlierDateButton"; // Tap another element to dismiss/unfocus
#endif

	public Issue19256(TestDevice device) : base(device)
	{
	}

	public override string Issue => "DatePicker control minimum date issue";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerMinimumDateShouldUpdateDynamically()
	{
		Exception? exception = null;

		App.WaitForElement("LeftDatePicker");
		App.WaitForElement("RightDatePicker");

		// Step 1: Set LEFT to future date (June 25) - RIGHT MinimumDate becomes June 25
		App.Tap("SetFutureDateButton");

		// Step 2: Open RIGHT DatePicker to see MinimumDate is June 25
		App.Tap("RightDatePicker");

		// Screenshot 1: Shows dates before June 25 are disabled
		VerifyScreenshotOrSetException(ref exception, "DatePickerMinimumDateShouldUpdateDynamically_FutureDate");

		// Dismiss the dialog
		App.Tap(DismissDatePicker);

		// Step 3: Set LEFT to earlier date (June 20) - RIGHT MinimumDate should become June 20
		App.Tap("SetEarlierDateButton");

		// Step 4: Open RIGHT DatePicker again to verify MinimumDate updated
		App.Tap("RightDatePicker");

		// Screenshot 2: Should show dates from June 20 are now enabled (not June 25)
		VerifyScreenshotOrSetException(ref exception, "DatePickerMinimumDateShouldUpdateDynamically_EarlierDate");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
#endif