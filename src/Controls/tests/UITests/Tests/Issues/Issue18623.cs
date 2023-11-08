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
		public void EntryIsPasswordObscureText()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. In the Entry control below, input some text. The test passes if the text is not obscured.
			App.EnterText("NoPasswordEntry", "Test");

			// 2. In the Entry control below, input some text. The test passes if the text is obscured.
			App.EnterText("PasswordEntry", "Test");

			// 3. Verify the result  comparing snapshots.
			VerifyScreenshot();
		}
	}
}
