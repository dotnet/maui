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

	//[Test]
	//[Category(UITestCategories.Editor)]
	//[FailsOnIOS]
	//public void VerifyEditorTextChangeEventsAreFiring()
	//{
	//	App.WaitForElement("EditorAutomationId");
	//	App.EnterText("EditorAutomationId", "Test 1");

	//	Assert.AreEqual("Test 1", App.WaitForElement("EditorAutomationId")[0].ReadText());
	//	Assert.AreEqual("Test 1", App.WaitForElement("EntryAutomationId")[0].ReadText());

	//	App.ClearText("EntryAutomationId");
	//	App.EnterText("EntryAutomationId", "Test 2");

	//	Assert.AreEqual("Test 2", App.WaitForElement("EditorAutomationId")[0].ReadText());
	//	Assert.AreEqual("Test 2", App.WaitForElement("EntryAutomationId")[0].ReadText());
	//}
}