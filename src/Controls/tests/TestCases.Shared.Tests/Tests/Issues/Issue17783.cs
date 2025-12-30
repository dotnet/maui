using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17783 : _IssuesUITest
	{
		public Issue17783(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Pasting long text when the editor has a max length does nothing";

		[Test]
		[Category(UITestCategories.Editor)]
		[Category(UITestCategories.Entry)]
		public void TextShouldBeCut()
		{
			App.WaitForElement("PasteToEntryButton");
			App.Click("PasteToEntryButton");
			App.Click("PasteToEditorButton");
			var entryText = App.FindElement("Entry").GetText();
			var editorText = App.FindElement("Editor").GetText();

			Assert.That(entryText, Is.EqualTo("Hello"));
			Assert.That(editorText, Is.EqualTo("Hello"));
		}
	}
}
