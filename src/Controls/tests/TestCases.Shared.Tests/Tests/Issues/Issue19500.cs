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

#if MACCATALYST // In Catalyst scroll down is not effective so here we retry one more time to address the flakyness.
			Thread.Sleep(500);
			App.ScrollDown("editor");
#endif

			// The test passes if the text inside the editor scrolls down
			VerifyScreenshot();
		}
	}
}
