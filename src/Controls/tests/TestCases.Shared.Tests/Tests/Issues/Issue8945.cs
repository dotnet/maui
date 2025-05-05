using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8945 : _IssuesUITest
	{
		public Issue8945(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Add Open/Close API to picker controls";

		[Test]
		[Category(UITestCategories.DatePicker)]
		public void OpenCloseDatePicker()
		{
			App.WaitForElement("WaitForStubControl");
			App.Tap("OpenDatePickerButton");
			Assert.That(App.WaitForElement("DatePickerStatusLabel")?.GetText(), Is.EqualTo("DatePicker Opened"));
			App.Tap("CloseDatePickerButton");
			Assert.That(App.WaitForElement("DatePickerStatusLabel")?.GetText(), Is.EqualTo("DatePicker Closed"));
		}

		[Test]
		[Category(UITestCategories.TimePicker)]
		public void OpenCloseTimePicker()
		{
			App.WaitForElement("WaitForStubControl");
			App.Tap("OpenTimePickerButton");
			Assert.That(App.WaitForElement("TimePickerStatusLabel")?.GetText(), Is.EqualTo("TimePicker Opened"));
			App.Tap("CloseTimePickerButton");
			Assert.That(App.WaitForElement("TimePickerStatusLabel")?.GetText(), Is.EqualTo("TimePicker Closed"));
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void OpenClosePicker()
		{
			App.WaitForElement("WaitForStubControl");
			App.Tap("OpenPickerButton");
			Assert.That(App.WaitForElement("PickerStatusLabel")?.GetText(), Is.EqualTo("Picker Opened"));
			App.Tap("ClosePickerButton");
			Assert.That(App.WaitForElement("PickerStatusLabel")?.GetText(), Is.EqualTo("Picker Closed"));
		}
	}
}