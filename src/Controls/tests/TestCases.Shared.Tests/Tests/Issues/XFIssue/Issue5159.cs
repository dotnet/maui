using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Picker)]
public class Issue5159 : _IssuesUITest
{
	public Issue5159(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Calling Focus on all Pickers running an API 28 devices no longer opens Picker";

	//[Test]
	//public void InvisibleDatepickerShowsDialogOnFocus()
	//{
	//	RunningApp.WaitForElement(DatePickerButton);
	//	RunningApp.Screenshot("Issue 5159 page is showing in all it's glory");
	//	RunningApp.Tap(DatePickerButton);

	//	RunningApp.WaitForElement(x => x.Class("DatePicker"));

	//	RunningApp.Screenshot("DatePicker is shown");
	//	RunningApp.TapCoordinates(5, 100);
	//}

	//[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnAndroid]
	//[Test]
	//public void InvisibleTimepickerShowsDialogOnFocus()
	//{
	//	RunningApp.WaitForElement(TimePickerButton);
	//	RunningApp.Screenshot("Issue 5159 page is showing in all it's glory");
	//	RunningApp.Tap(TimePickerButton);

	//	RunningApp.WaitForElement(x => x.Class("timePicker"));

	//	RunningApp.Screenshot("TimePicker is shown");
	//	RunningApp.TapCoordinates(5, 100);
	//}

	//[FailsOnAndroid]
	//[Test]
	//public void InvisiblePickerShowsDialogOnFocus()
	//{
	//	RunningApp.WaitForElement(PickerButton);
	//	RunningApp.Screenshot("Issue 5159 page is showing in all it's glory");
	//	RunningApp.Tap(PickerButton);

	//	RunningApp.WaitForElement("Foo");

	//	RunningApp.Screenshot("Picker is shown");

	//	RunningApp.Tap("Foo");

	//	RunningApp.WaitForNoElement("Foo");

	//}
}