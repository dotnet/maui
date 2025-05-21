using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8945 : _IssuesUITest
	{
		const string CancelBtn = "Cancel";
		
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
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Opened");

			App.TapDisplayAlertButton(CancelBtn);
			App.WaitForElement("CloseDatePickerButton");
			App.Tap("CloseDatePickerButton");
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
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Opened");

			App.TapDisplayAlertButton(CancelBtn);
			App.WaitForElement("CloseTimePickerButton");
			App.Tap("CloseTimePickerButton");
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
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Opened");

			App.TapDisplayAlertButton(CancelBtn);
			App.WaitForElement("ClosePickerButton");
			App.Tap("ClosePickerButton");
			VerifyScreenshotOrSetException(ref exception, TestContext.CurrentContext.Test.MethodName + "_Closed");

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}