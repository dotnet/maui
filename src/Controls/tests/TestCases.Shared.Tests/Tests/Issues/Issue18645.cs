using Xunit;
using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue18645 : _IssuesUITest
	{
		public Issue18645(TestDevice device) : base(device)
		{
		}

		public override string Issue => "Editor MaxLength property works as expected";

		[Fact]
		[Trait("Category", UITestCategories.Editor)]
		public void EditorMaxLengthWorks()
		{
			App.WaitForElement("WaitForStubControl");

			// 1. Input characters in the editor until it won't accept more input.
			App.EnterText("IssueEditor", "123456789");

			// 2. The test fails if the maximum number of characters is more
			// or less than 5 characters.
			var editorText = App.FindElement("IssueEditor").GetText();

			// MaxLines = 5
			Assert.Equal("12345", editorText);
		}
	}
}
