using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18726 : _IssuesUITest
	{
		public Issue18726(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Editor custom Keyboard works";

		[Test]
		public void EditorCustomKeyboardWorks()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive).
			App.EnterText("EditorCapitalizeSentence", "hi my name is james. nice to meet you.");
			// 2. The test fails if the editor does not display 'Hi My Name Is James. Nice To Meet You.' exactly (case-sensitive).

			// 3. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive).
			App.EnterText("EditorCapitalizeWord", "hi my name is james. nice to meet you.");
			// 4. The test fails if the editor does not display 'HI MY NAME IS JAMES. NICE TO MEET YOU.' exactly (case-sensitive).

			// 5. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive).
			App.EnterText("EditorCapitalizeCharacter", "hi my name is james. nice to meet you.");
			// 6. The test fails if the editor does not display 'HI MY NAME IS JAMES. NICE TO MEET YOU.' exactly (case-sensitive).

			// 7. Enter the string 'hi my name is james. nice to meet you.' exactly (case-sensitive).
			App.EnterText("EditorCapitalizeNone", "hi my name is james. nice to meet you.");
			// 8. The test fails if the editor does not display 'hi my name is james. nice to meet you.' exactly (case-sensitive).

			// 9. Enter the string 'qweqw hi hi' exactly (case-sensitive).
			App.EnterText("EditorSpellcheck", "qweqw hi hi");
			// 10. The test fails if there is no red underline under the string 'qweqw' to indicate a spelling error.

			VerifyScreenshot();
		}
	}
}
