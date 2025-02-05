#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
// On MacCatalyst, pressing the ESC key during screenshot capture clears the text. This causes the image generated in CI to differ from local runs.
//In IOS App.EnterText not working with iOS password for more information : https://github.com/dotnet/maui/issues/18981
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18623 : _IssuesUITest
	{
		public Issue18623(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Entry IsPassword obscure the text";

		[Test]
		[Category(UITestCategories.Entry)]
		[FailsOnIOSWhenRunningOnXamarinUITest("Currently fails on iOS; see https://github.com/dotnet/maui/issues/18981")]
		public async Task EntryIsPasswordObscureText()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. In the Entry control below, input some text. The test passes if the text is not obscured.
			App.EnterText("NoPasswordEntry", "Test");

			// 2. In the Entry control below, input some text. The test passes if the text is obscured.
			App.EnterText("PasswordEntry", "Test");

			// Wait to capture the snapshot after enter the text and
			// closed the keyboard.
			await Task.Delay(1000);

			// 3. Verify the result  comparing snapshots.
			VerifyScreenshot();
		}
	}
}
#endif