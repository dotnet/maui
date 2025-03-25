#if ANDROID //This test case verifies "The background property of AppCompatEditText (Editor) works" exclusively on the Android platform.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18720Editor : _IssuesUITest
	{
		public Issue18720Editor(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Setting the background property of AppCompatEditText (Editor) in a handler mapping does not work";

		[Test]
		[Category(UITestCategories.Editor)]
		public async Task SettingEditorBackgroundFromHandler()
		{
			App.WaitForElement("TestButton");
			App.Tap("TestButton");
			App.WaitForElement("CustomEditor1");

			await Task.Delay(500);

			VerifyScreenshot();
		}
	}
}
#endif