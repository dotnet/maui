#if TEST_FAILS_ON_WINDOWS
//The BoxView's AutomationId doesn't work correctly on the Windows platform, and using a Label also doesn't ensure the BoxView's drag-and-drop functionality works.
//for more information: https://github.com/dotnet/maui/issues/27195 
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

		public void AcceptedOperationNoneDisablesDropOperation()
		{
			App.WaitForElement("TestLoaded");
			App.DragAndDrop("DragBox", "DropBox");
			App.WaitForElement("Success");
		}
	}
}
#endif