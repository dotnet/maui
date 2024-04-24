using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18623 : _IssuesUITest
	{
		public Issue18623(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Entry IsPassword obscure the text";

		[Test]
		[Category(UITestCategories.Entry)]
		public async Task EntryIsPasswordObscureText()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/18981");

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
