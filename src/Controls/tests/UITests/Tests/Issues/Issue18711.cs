using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18711 : _IssuesUITest
	{
		public Issue18711(TestDevice device)
			: base(device)
		{ }

		public override string Issue => "Editor IsSpellCheckEnabledDisabled works";

		[Test]
		public void Issue18711Test()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Enter the text 'qweqw hi hi' in the editor below.
			App.EnterText("TestEditor", "qweqw hi hi");
			// 2. The test fails if there is no red underline under the string 'qweqw' to indicate a spelling error.
			VerifyScreenshot("IsSpellCheckEnabledEnabled");
			// 3. Uncheck the 'IsSpellCheckEnabled' checkbox to disable spellcheck.
			App.Click("TestCheckbox");
			// 4. The test fails if the red underline under the string 'qweqw' is still visible.
			VerifyScreenshot("IsSpellCheckEnabledDisabled");
		}
	}
}