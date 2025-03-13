#if TEST_FAILS_ON_WINDOWS // Graphics view is not accessible on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20834 : _IssuesUITest
{
	const string graphicsView = "graphicsView";
	public Issue20834(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] GraphicsView can also be visible outside the canvas";

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewShouldNotDrawOutsideCanvasBounds()
	{
		App.WaitForElement(graphicsView);
		App.DragAndDrop(graphicsView, "DropTarget");
		VerifyScreenshot();
	}
}
#endif