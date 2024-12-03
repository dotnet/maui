#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla41424 : _IssuesUITest
{
	public Bugzilla41424(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Clicking cancel on a DatePicker does not cause it to unfocus";

	// TODO from Xamarin.UITest migration: replace DialogIsOpened calls with another way of detecting the dialog
	// Maybe see Bugzilla42074 which seems to do the same?
	// [FailsOnAndroidWhenRunningOnXamarinUITest]
	// [Test]
	// [Category(UITestCategories.DatePicker)]
	// public void DatePickerCancelShouldUnfocus()
	// {
	// 	App.Tap("DatePicker");
	// 	//Assert.IsTrue(DialogIsOpened(),"Tap Picker");

	// 	App.WaitForElement("getfocusstate");
	// 	App.Tap("getfocusstate");
	// 	App.WaitForElement("focusstate", "unfocused");

	// 	App.Tap("Click to focus DatePicker");
	// 	//Assert.IsTrue(DialogIsOpened(),"Call Focus Picker");

	// 	App.WaitForElement("getfocusstate");
	// 	App.Tap("getfocusstate");
	// 	App.WaitForTextToBePresentInElement("focusstate", "unfocused");
	// }

	// bool DialogIsOpened()
	// {
	// 	Thread.Sleep(1500);
	// 	var frameLayouts = App.Query(q => q.Class("FrameLayout");
	// 	foreach (var layout in frameLayouts)
	// 	{
	// 		if (layout.Rect.X > 0 && layout.Rect.Y > 0 && layout.Description.Contains(@"id/content"))
	// 		{
	// 			// close dialog
	// 			App.Back();
	// 			Thread.Sleep(1500);
	// 			return true;
	// 		}
	// 	}
	// 	return false;
	// }
}
#endif