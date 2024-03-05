using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12060 : IssuesUITest
	{
		public Issue12060(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Bug] DragGestureRecognizer shows 'Copy' tag when dragging in UWP";

		[Test]
		[Category(UITestCategories.DragAndDrop)]
		public void AppDoesntCrashWhenResettingPage()
		{
			App.WaitForElement("TestLoaded");
			App.DragAndDrop("DragBox", "DropBox");
			App.WaitForNoElement("Success");
		}
	}
}