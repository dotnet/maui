#if TEST_FAILS_ON_CATALYST // TimePicker not opens the dialog, issue: https://github.com/dotnet/maui/issues/10827 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla42074 : _IssuesUITest
{

#if ANDROID // Action button on TimePicker dialog is vary on different platforms
	const string TimePickerActionButton = "Cancel";
#elif IOS
	const string TimePickerActionButton = "Done";
#elif WINDOWS
	const string TimePickerActionButton = "Accept";
#endif

	public Bugzilla42074(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Clicking cancel on a TimePicker does not cause it to unfocus";

	[Test]
	[Category(UITestCategories.TimePicker)]
	public void TimePickerCancelShouldUnfocus()
	{
		App.WaitForElement("TimePicker");
		App.Tap("TimePicker");

		CloseDialog();
		App.WaitForElement("Click to focus TimePicker");
#if !ANDROID && !WINDOWS //Programmatic focus does not open the dialog for picker controls, issue: https://github.com/dotnet/maui/issues/8946 
		App.Tap("focusbtn");
		CloseDialog();
#endif
	}

	void CloseDialog()
	{
		App.WaitForElement(TimePickerActionButton);
		App.Tap(TimePickerActionButton);
	}
}
#endif
