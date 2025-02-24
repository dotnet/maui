#if TEST_FAILS_ON_CATALYST // TimePicker not opens the dialog, issue: https://github.com/dotnet/maui/issues/10827 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class TimePickerUITest : _IssuesUITest
	{
		public override string Issue => "TimePicker UI Test";

		public TimePickerUITest(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.TimePicker)]
		public void VerifyTimePickerAppearance()
		{
			App.WaitForElement("TimePicker");
			App.Tap("TimePicker");
			Thread.Sleep(500); // Add some wait for popping up the keyboard to resolve flakiness in CI.
			VerifyScreenshot();
		}
	}
}
#endif
