using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17554 : _IssuesUITest
	{
		const string DropTarget = "DropTarget";
		const string DragTarget = "DragTarget";

		public Issue17554(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] DragGestureRecognizer.DropCompleted event not firing";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void DropCompletedEventShouldFire()
		{
			App.WaitForElement(DropTarget);
			App.DragAndDrop(DragTarget, DropTarget);
			Thread.Sleep(500); // Wait for the event to fire
			var text = App.WaitForElement("eventText").GetText();
			Assert.That(text, Is.EqualTo("DropCompleted"));
		}
	}
}