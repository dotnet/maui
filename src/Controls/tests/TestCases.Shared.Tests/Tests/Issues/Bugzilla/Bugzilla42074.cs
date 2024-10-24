#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla42074 : _IssuesUITest
{
	public Bugzilla42074(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] Clicking cancel on a TimePicker does not cause it to unfocus";

	// [FailsOnAndroidWhenRunningOnXamarinUITest]
	// [Test]
	// [Category(UITestCategories.TimePicker)]
	// public void TimePickerCancelShouldUnfocus()
	// {
	// 	App.Tap("TimePicker");

	// 	App.Back();
	// 	App.WaitForElement("focusbtn");

	// 	App.Tap("focusbtn");
	// 	App.Back();
	// }
}
#endif