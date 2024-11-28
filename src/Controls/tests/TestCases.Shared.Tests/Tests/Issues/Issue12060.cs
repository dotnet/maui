using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12060 : _IssuesUITest
	{
		public Issue12060(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Bug] DragGestureRecognizer shows 'Copy' tag when dragging in UWP";

		[Test]
		[Category(UITestCategories.DragAndDrop)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnIOSWhenRunningOnXamarinUITest]
		[FailsOnMacWhenRunningOnXamarinUITest]
		public void AcceptedOperationNoneDisablesDropOperation()
		{
			App.WaitForElement("TestLoaded");
			App.DragAndDrop("DragBox", "DropBox");
			App.WaitForNoElement("Success");
		}
	}
}