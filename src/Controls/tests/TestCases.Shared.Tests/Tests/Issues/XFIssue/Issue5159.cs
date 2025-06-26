#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST // The Programmatic Focus doesn't open the dialog for picker controls. Issue: https://github.com/dotnet/maui/issues/8946 
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Trait("Category", UITestCategories.Picker)]
public class Issue5159 : _IssuesUITest
{
	const string DatePickerButton = "DatePickerButton";
	const string TimePickerButton = "TimePickerButton";
	const string PickerButton = "PickerButton";
	public Issue5159(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Calling Focus on all Pickers running an API 28 devices no longer opens Picker";

	[Fact]
	public void InvisibleDatepickerShowsDialogOnFocus()
	{
		App.WaitForElement("DatePickerButton");
		App.Tap(DatePickerButton);
		App.WaitForElement("Done");
		App.TapCoordinates(5, 100);
	}

	[Fact]
	public void InvisibleTimepickerShowsDialogOnFocus()
	{
		App.WaitForElement(TimePickerButton);
		App.Tap(TimePickerButton);
		App.WaitForElement("Done");
		App.TapCoordinates(5, 100);
	}

	[Fact]
	public void InvisiblePickerShowsDialogOnFocus()
	{
		App.WaitForElement(PickerButton);
		App.Tap(PickerButton);
		App.WaitForElement("Done");
	}
}
#endif