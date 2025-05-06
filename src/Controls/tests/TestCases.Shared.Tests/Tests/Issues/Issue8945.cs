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
			Exception? exception = null;
			
			App.WaitForElement("WaitForStubControl");
			App.WaitForElement("OpenDatePickerButton");
			App.Tap("OpenDatePickerButton");
			Assert.That(App.WaitForElement("DatePickerStatusLabel", timeout: TimeSpan.FromSeconds(2))?.GetText(), Is.EqualTo("DatePicker Opened"));
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Opened");
			
			App.WaitForElement("CloseDatePickerButton");
			App.Tap("CloseDatePickerButton");
			Assert.That(App.WaitForElement("DatePickerStatusLabel", timeout: TimeSpan.FromSeconds(2))?.GetText(), Is.EqualTo("DatePicker Closed"));
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Closed");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Category(UITestCategories.TimePicker)]
		public void OpenCloseTimePicker()
		{
			Exception? exception = null;

			App.WaitForElement("WaitForStubControl");
			App.WaitForElement("OpenTimePickerButton");
			App.Tap("OpenTimePickerButton");
			Assert.That(App.WaitForElement("TimePickerStatusLabel", timeout: TimeSpan.FromSeconds(2))?.GetText(), Is.EqualTo("TimePicker Opened"));
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Opened");

			App.WaitForElement("CloseTimePickerButton");
			App.Tap("CloseTimePickerButton");
			Assert.That(App.WaitForElement("TimePickerStatusLabel", timeout: TimeSpan.FromSeconds(2))?.GetText(), Is.EqualTo("TimePicker Closed"));
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Closed");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Category(UITestCategories.Picker)]
		public void OpenClosePicker()
		{
			Exception? exception = null;

			App.WaitForElement("WaitForStubControl");
			App.WaitForElement("OpenPickerButton");
			App.Tap("OpenPickerButton");
			Assert.That(App.WaitForElement("PickerStatusLabel", timeout: TimeSpan.FromSeconds(2))?.GetText(), Is.EqualTo("Picker Opened"));
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Opened");

			App.WaitForElement("ClosePickerButton");
			App.Tap("ClosePickerButton");
			Assert.That(App.WaitForElement("PickerStatusLabel", timeout: TimeSpan.FromSeconds(2))?.GetText(), Is.EqualTo("Picker Closed"));
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Closed");

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}