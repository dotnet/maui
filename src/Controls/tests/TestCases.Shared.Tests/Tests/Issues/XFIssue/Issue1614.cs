#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1614 : _IssuesUITest
{
	public Issue1614(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS 11 prevents InputAccessoryView from showing in landscape mode";
	
	// [Test]
	// [FailsOnIOS]
	// public void Issue1614Test()
	// {
	// 	RunningApp.SetOrientationPortrait();

	// 	RunningApp.WaitForElement(x => x.Class("UITextField"));
	// 	RunningApp.Tap(x => x.Class("UITextField").Index(0));
	// 	CheckPickerAccessory("UIPickerView");
	// 	RunningApp.SetOrientationLandscape();
	// 	CheckPickerAccessory("UIPickerView");
	// 	RunningApp.SetOrientationPortrait();
	// 	RunningApp.DismissKeyboard();

	// 	RunningApp.Tap(x => x.Class("UITextField").Index(1));
	// 	CheckPickerAccessory("UIDatePicker");
	// 	RunningApp.SetOrientationLandscape();
	// 	CheckPickerAccessory("UIDatePicker");
	// 	RunningApp.SetOrientationPortrait();
	// 	RunningApp.DismissKeyboard();

	// 	RunningApp.Tap(x => x.Class("UITextField").Index(2));
	// 	CheckPickerAccessory("UIDatePicker");
	// 	RunningApp.SetOrientationLandscape();
	// 	CheckPickerAccessory("UIDatePicker");
	// 	RunningApp.SetOrientationPortrait();
	// 	RunningApp.DismissKeyboard();
	// }

	// private void CheckPickerAccessory(string className)
	// {
	// 	RunningApp.WaitForElement(x => x.Class("UIButtonLabel"));
	// 	var buttonRect = RunningApp.Query(x => x.Class("UIButtonLabel"))[0].Rect;
	// 	var pickerRect = RunningApp.Query(x => x.Class(className))[0].Rect;

	// 	var buttonBottom = buttonRect.Y + buttonRect.Height;
	// 	var pickerTop = pickerRect.Y;

	// 	Assert.IsTrue(buttonBottom <= pickerTop);
	// }
}
#endif