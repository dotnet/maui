using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18752 : _IssuesUITest
	{
		public Issue18752(TestDevice device) : base(device) { }

		public override string Issue => "[D7] Editor IsSpellCheckEnabled works";

		[Test]
		public void Issue18752Test()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Enter the string 'qweqw hi hi' exactly (case-sensitive) in the editor below.
			App.EnterText("EditorIsSpellCheckDisabled", "qweqw hi hi");
			// 2. The test fails if there is a red underline under the string 'qweqw' to indicate a spelling error.
		
			// 3. Enter the string 'qweqw hi hi' exactly (case-sensitive) in the editor below.
			App.EnterText("EditorIsSpellCheckEnabled", "qweqw hi hi");
			// 4. The test fails if there is no red underline under the string 'qweqw' to indicate a spelling error.
					
			// 5. The test fails if there is no red underline under the string 'qweqw' in the editor below to indicate a spelling error.

			VerifyScreenshot();
		}
	}
}