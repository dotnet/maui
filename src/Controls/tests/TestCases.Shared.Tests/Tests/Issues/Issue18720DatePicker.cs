#if ANDROID //This test case verifies "The background property of AppCompatEditText (DatePicker) works" exclusively on the Android platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18720DatePicker : _IssuesUITest
	{
		public Issue18720DatePicker(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting the background property of AppCompatEditText (DatePicker) in a handler mapping does not work";

		[Test]
		[Category(UITestCategories.DatePicker)]
		public async Task SettingDatePickerBackgroundFromHandler()
		{
			App.WaitForElement("TestButton");
			App.Tap("TestButton");
			App.WaitForElement("CustomDatePicker1");

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}
#endif