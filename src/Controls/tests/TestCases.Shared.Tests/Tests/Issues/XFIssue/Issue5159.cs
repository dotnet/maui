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
	//	App.WaitForElement(DatePickerButton);
	//	App.Screenshot("Issue 5159 page is showing in all it's glory");
	//	App.Tap(DatePickerButton);

	//	App.WaitForElement(x => x.Class("DatePicker"));

	//	App.Screenshot("DatePicker is shown");
	//	App.TapCoordinates(5, 100);
	//}

	//[Microsoft.Maui.Controls.Compatibility.UITests.FailsOnAndroid]
	//[Test]
	//public void InvisibleTimepickerShowsDialogOnFocus()
	//{
	//	App.WaitForElement(TimePickerButton);
	//	App.Screenshot("Issue 5159 page is showing in all it's glory");
	//	App.Tap(TimePickerButton);

	//	App.WaitForElement(x => x.Class("timePicker"));

	//	App.Screenshot("TimePicker is shown");
	//	App.TapCoordinates(5, 100);
	//}

	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//[Test]
	//public void InvisiblePickerShowsDialogOnFocus()
	//{
	//	App.WaitForElement(PickerButton);
	//	App.Screenshot("Issue 5159 page is showing in all it's glory");
	//	App.Tap(PickerButton);

	//	App.WaitForElement("Foo");

	//	App.Screenshot("Picker is shown");

	//	App.Tap("Foo");

	//	App.WaitForNoElement("Foo");

	//}
}