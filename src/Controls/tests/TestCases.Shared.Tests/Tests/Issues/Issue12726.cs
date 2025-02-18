using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12726 : _IssuesUITest
	{
		public Issue12726(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Drag and Drop Gesture Fails on Runtime Changes of CanDrag and AllowDrop in Windows";

		[Test]
		[Category(UITestCategories.DragAndDrop)]
		public void DragAndDropShouldWorkRunTime()
		{
			App.WaitForElement("EnableDragAndDrop");
			App.Tap("EnableDragAndDrop");
			App.DragAndDrop("DragElement", "DropTarget");
			App.WaitForElement("DragEventTriggered");
		}
	}
}