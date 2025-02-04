using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue5500 : _IssuesUITest
{
	public Issue5500(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] Editor with material visuals value binding not working on physical device";

	[Test]
	[Category(UITestCategories.Editor)]
	public void VerifyEditorTextChangeEventsAreFiring()
	{
		App.WaitForElement("EditorAutomationId");
		App.EnterText("EditorAutomationId", "Test 1");

		var editorText = App.WaitForElement("EditorAutomationId").ReadText();
		var entryText = App.WaitForElement("EntryAutomationId").ReadText();
		Assert.That(editorText, Is.EqualTo(entryText));

		App.ClearText("EntryAutomationId");
		App.EnterText("EntryAutomationId", "Test 2");

		editorText = App.WaitForElement("EditorAutomationId").ReadText();
		entryText = App.WaitForElement("EntryAutomationId").ReadText();
		Assert.That(editorText, Is.EqualTo(entryText));
	}
}