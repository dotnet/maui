using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue18645 : _IssuesUITest
	{
		public Issue18645(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Editor MaxLength property works as expected";

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorMaxLengthWorks()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Input characters in the editor until it won't accept more input.
			App.EnterText("IssueEditor", "123456789");

			// 2. The test fails if the maximum number of characters is more
			// or less than 5 characters.
			var editorText = App.FindElement("IssueEditor").GetText();

			// MaxLines = 5
			Assert.AreEqual("12345", editorText);
		}
	}
}
