using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36933 : _IssuesUITest
{
	public Issue36933(TestDevice device) : base(device)
	{
	}

	public override string Issue => "DatePicker and TimePicker Background set to null at runtime does not clear on iOS/MacCatalyst";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void VerifyDatePickerAndTimePickerBackgroundClearsOnNull()
	{
		App.WaitForElement("TestDatePicker");

		Exception? exception = null;

		// Tap clear button to set Background = null — should clear the initial background
		App.Tap("ClearBackgroundButton");
		VerifyScreenshotOrSetException(ref exception, "Issue36933BackgroundCleared");

		// Tap set button to set a new background color
		App.Tap("SetBackgroundButton");
		VerifyScreenshotOrSetException(ref exception, "Issue36933BackgroundColorSet");

		if (exception is not null)
		{
			throw exception;
		}
	}
}
