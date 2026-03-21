#if TEST_FAILS_ON_CATALYST // IsOpen property not implemented on Catalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8945TimePicker : _IssuesUITest
{
#if ANDROID
	const string CancelBtn = "Cancel";
#endif

	public Issue8945TimePicker(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Add Open/Close API to picker controls (TimePicker)";

	[Test]
	[Category(UITestCategories.TimePicker)]
	public void TimePickerOpenedFromConstructor()
	{
#if ANDROID
		App.TapDisplayAlertButton(CancelBtn);
#else
		App.TapCoordinates(10, 10); // Tap outside the date picker to close it
#endif
		Assert.That(App.WaitForElement("TimePickerStatusLabel", timeout: TimeSpan.FromSeconds(2))?.GetText(), Is.EqualTo("Passed"));
	}
}
#endif