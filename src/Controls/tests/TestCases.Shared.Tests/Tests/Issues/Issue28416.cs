using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.DragAndDrop)]
public class Issue28416 : _IssuesUITest
{
	public Issue28416(TestDevice testDevice) : base(testDevice)
	{
	}

	protected override bool ResetAfterEachTest => true;

	public override string Issue => "Crash in drag-n-drop with dragged element destroyed before drop is completed";

	[Test]
	public void DragAndDropWithAnElementThatIsRemoved()
	{
		App.WaitForElement("InstructionsLabel");
		App.DragAndDrop("DragObject", "DragTarget");
		App.WaitForElement("DropResult");
		App.WaitForElement("DropCompletedResult");
	}
}
