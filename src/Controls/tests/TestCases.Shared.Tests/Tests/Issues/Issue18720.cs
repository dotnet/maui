#if ANDROID //This test case verifies "The background property of AppCompatEditText (Entry) works" exclusively on the Android platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18720 : _IssuesUITest
	{
		public Issue18720(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting the background property of AppCompatEditText (Entry) in a handler mapping does not work";

		[Test]
		[Category(UITestCategories.Entry)]
		public async Task SettingEntryBackgroundFromHandler()
		{
			App.WaitForElement("TestButton");
			App.Tap("TestButton");
			App.WaitForElement("CustomEntry1");

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}
#endif