using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34848 : _IssuesUITest
{
	public Issue34848(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "DatePicker Opened and Closed events are not raised on MacCatalyst";

	[Test]
	[Category(UITestCategories.DatePicker)]
	public void DatePickerOpenedAndClosedEventsAreRaised()
	{
		App.WaitForElement("Issue34848OpenStatusLabel");
		App.WaitForElement("Issue34848CloseStatusLabel");
		App.WaitForElement("Issue34848TestDatePicker");

		// Open the DatePicker
		App.Tap("Issue34848TestDatePicker");

#if IOS
		// iOS DatePicker uses a wheel picker, so we can just tap the "Done" button to close it
		App.Tap("Done");
#elif WINDOWS
		// On Windows, we can tap a date to close the DatePicker
		App.Tap("16");
#elif ANDROID
		// On Android, we can tap the "Cancel" button to close the DatePicker
		App.Tap("Cancel");
#else
		// On MacCatalyst, we can tap outside the DatePicker to close it
		App.TapCoordinates(30, 30);
#endif

		Assert.That(App.WaitForElement("Issue34848OpenStatusLabel").GetText(), Is.EqualTo("Opened"));
		Assert.That(App.WaitForElement("Issue34848CloseStatusLabel").GetText(), Is.EqualTo("Closed"));
	}
}
