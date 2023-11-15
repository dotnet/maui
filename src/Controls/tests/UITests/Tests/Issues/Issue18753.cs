using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18753 : _IssuesUITest
	{
		public Issue18753(TestDevice device) : base(device) { }

		public override string Issue => "[D8] Editor IsTextPredictionEnabled works";

		[Test]
		public void Issue18753Test()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Enter the string 'hi my name is james' exactly (case-sensitive).
			App.EnterText("EditorIsTextPredictionDisabled", "hi my name is james");
			// 2. The test fails if the keyboard presents word suggestions.

			if(App.IsKeyboardShown())
			{
				VerifyScreenshot("Issue18753IsTextPredictionDisabled");
				App.DismissKeyboard();
			}

			// 3. Enter the string 'hi my name is james' exactly (case-sensitive).
			App.EnterText("EditorIsTextPredictionEnabled", "hi my name is james");
			// 4. The test fails if the keyboard does not present word suggestions.
			
			if (App.IsKeyboardShown())
			{
				VerifyScreenshot("Issue18753IsTextPredictionEnabled");
				App.DismissKeyboard();
			}
		}
	}
}
