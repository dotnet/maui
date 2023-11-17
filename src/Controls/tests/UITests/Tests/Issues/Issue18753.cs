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
		public async Task Issue18753Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows },
				"Currently IsKeyboardShown is not implemented.");

			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android },
				"Currently is failing.");

			App.WaitForElement("WaitForStubControl");

			// 1. Enter the string 'hi my name is james' exactly (case-sensitive).
			App.EnterText("EditorIsTextPredictionDisabled", "hi my name is james");
			// 2. The test fails if the keyboard presents word suggestions.

			// Focus the Editor.
			App.Click("EditorIsTextPredictionDisabled");

			// Delay to auto hide popups with suggestions to copy etc.
			await Task.Delay(500);

			if (App.IsKeyboardShown())
			{
				VerifyScreenshot("Issue18753IsTextPredictionDisabled");
				App.DismissKeyboard();
			}

			// 3. Enter the string 'hi my name is james' exactly (case-sensitive).
			App.EnterText("EditorIsTextPredictionEnabled", "hi my name is james");
			// 4. The test fails if the keyboard does not present word suggestions.

			// Focus the Editor.
			App.Click("EditorIsTextPredictionEnabled");

			// Delay to auto hide popups with suggestions to copy etc.
			await Task.Delay(500);

			if (App.IsKeyboardShown())
			{
				VerifyScreenshot("Issue18753IsTextPredictionEnabled");
				App.DismissKeyboard();
			}
		}
	}
}
