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
	// 	App.SetOrientationPortrait();

	// 	App.WaitForElement(x => x.Class("UITextField"));
	// 	App.Tap(x => x.Class("UITextField").Index(0));
	// 	CheckPickerAccessory("UIPickerView");
	// 	App.SetOrientationLandscape();
	// 	CheckPickerAccessory("UIPickerView");
	// 	App.SetOrientationPortrait();
	// 	App.DismissKeyboard();

	// 	App.Tap(x => x.Class("UITextField").Index(1));
	// 	CheckPickerAccessory("UIDatePicker");
	// 	App.SetOrientationLandscape();
	// 	CheckPickerAccessory("UIDatePicker");
	// 	App.SetOrientationPortrait();
	// 	App.DismissKeyboard();

	// 	App.Tap(x => x.Class("UITextField").Index(2));
	// 	CheckPickerAccessory("UIDatePicker");
	// 	App.SetOrientationLandscape();
	// 	CheckPickerAccessory("UIDatePicker");
	// 	App.SetOrientationPortrait();
	// 	App.DismissKeyboard();
	// }

	// private void CheckPickerAccessory(string className)
	// {
	// 	App.WaitForElement(x => x.Class("UIButtonLabel"));
	// 	var buttonRect = App.Query(x => x.Class("UIButtonLabel"))[0].Rect;
	// 	var pickerRect = App.Query(x => x.Class(className))[0].Rect;

	// 	var buttonBottom = buttonRect.Y + buttonRect.Height;
	// 	var pickerTop = pickerRect.Y;

	// 	Assert.IsTrue(buttonBottom <= pickerTop);
	// }
}
#endif