using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19500 : _IssuesUITest
	{
		public override string Issue => "[iOS] Editor is not be able to scroll if IsReadOnly is true";

		public Issue19500(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Editor)]
		public void TextInEditorShouldScroll()
		{
			_ = App.WaitForElement("editor");
			App.ScrollDown("editor");

			// The test passes if the text inside the editor scrolls down
			VerifyScreenshot();
		}
	}
}
