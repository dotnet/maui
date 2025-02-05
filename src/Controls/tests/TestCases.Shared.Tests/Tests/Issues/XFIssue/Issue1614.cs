#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms Android and iOS.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1614 : _IssuesUITest
{
#if ANDROID
	const string Done = "Cancel";
#else
	const string Done = "Done";
#endif

	public Issue1614(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS 11 prevents InputAccessoryView from showing in landscape mode";

	[Test]
	[Category(UITestCategories.Picker)]
	public void Issue1614Test()
	{
		TapPicker("Picker");
		TapPicker("DatePicker");
		TapPicker("TimePicker");
	}

	void TapPicker(string picker)
	{
		App.WaitForElement(picker);
		App.Tap(picker);
		App.WaitForElement(Done);
		App.SetOrientationLandscape();
		App.WaitForElement(Done);
		App.SetOrientationPortrait();
		App.WaitForElement(Done);
		App.Tap(Done);
	}
}
#endif