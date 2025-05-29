#if TEST_FAILS_ON_CATALYST // IsOpen property not implemented on Catalyst
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8945 : _IssuesUITest
	{
#if ANDROID
		const string CancelBtn = "Cancel";
#endif
		public Issue8945(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Add Open/Close API to picker controls";

		[Test, Order(1)]
		[Category(UITestCategories.DatePicker)]
		public void OpenCloseDatePicker()
		{ 
			App.WaitForElement("WaitForStubControl");
			App.WaitForElement("OpenDatePickerButton");
			App.Tap("OpenDatePickerButton");
			Assert.That(App.WaitForElement("StatusLabel")?.GetText(), Is.EqualTo("DatePicker Opened"));
#if ANDROID
			App.TapDisplayAlertButton(CancelBtn);
#else
			App.TapCoordinates(10, 10); // Tap outside the date picker to close it
#endif
			App.WaitForElement("CloseDatePickerButton");
			App.Tap("CloseDatePickerButton");
			Assert.That(App.WaitForElement("StatusLabel")?.GetText(), Is.EqualTo("DatePicker Closed"));
		}

		[Test, Order(2)]
		[Category(UITestCategories.TimePicker)]
		public void OpenCloseTimePicker()
		{
			App.WaitForElement("WaitForStubControl");
			App.WaitForElement("OpenTimePickerButton");
			App.Tap("OpenTimePickerButton");
			Assert.That(App.WaitForElement("StatusLabel")?.GetText(), Is.EqualTo("TimePicker Opened"));
#if ANDROID
			App.TapDisplayAlertButton(CancelBtn);
#else
			App.TapCoordinates(10, 10); // Tap outside the date picker to close it
#endif
			App.WaitForElement("CloseTimePickerButton");
			App.Tap("CloseTimePickerButton");
			Assert.That(App.WaitForElement("StatusLabel")?.GetText(), Is.EqualTo("TimePicker Closed"));
		}

		[Test, Order(3)]
		[Category(UITestCategories.Picker)]
		public void OpenClosePicker()
		{
			App.WaitForElement("WaitForStubControl");
			App.WaitForElement("OpenPickerButton");
			App.Tap("OpenPickerButton");
			Assert.That(App.WaitForElement("StatusLabel")?.GetText(), Is.EqualTo("Picker Opened"));
#if ANDROID
			App.TapDisplayAlertButton(CancelBtn);
#else
			App.TapCoordinates(10, 10); // Tap outside the date picker to close it
#endif
			App.WaitForElement("ClosePickerButton");
			App.Tap("ClosePickerButton");
			Assert.That(App.WaitForElement("StatusLabel")?.GetText(), Is.EqualTo("Picker Closed"));
		}
	}
}
#endif