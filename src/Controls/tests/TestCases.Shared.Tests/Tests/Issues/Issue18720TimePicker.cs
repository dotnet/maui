#if ANDROID //This test case verifies "The background property of AppCompatEditText (TimePicker) works" exclusively on the Android platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18720TimePicker : _IssuesUITest
	{
		public Issue18720TimePicker(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting the background property of AppCompatEditText (TimePicker) in a handler mapping does not work";

		[Test]
		[Category(UITestCategories.TimePicker)]
		public async Task SettingTimePickerBackgroundFromHandler()
		{
			App.WaitForElement("TestButton");
			App.Tap("TestButton");
			App.WaitForElement("CustomTimePicker1");

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}
#endif