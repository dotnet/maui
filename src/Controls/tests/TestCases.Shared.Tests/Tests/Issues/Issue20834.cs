using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20834 : _IssuesUITest
{
	const string DragTarget = "DragTarget";
	const string DropTarget1 = "DropTarget1";
	const string DropTarget2 = "DropTarget2";

	public Issue20834(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] GraphicsView can also be visible outside the canvas";
	protected override bool ResetAfterEachTest => true;

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewShouldNotDrawOutsideCanvasBounds()
	{
		App.WaitForElement(DragTarget);
		App.DragAndDrop(DragTarget, DropTarget1);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewShouldNotDrawOutsideCanvasBounds_2()
	{
		App.WaitForElement(DragTarget);
		App.DragAndDrop(DragTarget, DropTarget2);
		VerifyScreenshot();
	}

#if !IOS //setting the toY value more than 100 in iOS opens the notification center
	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewShouldDrawWithInCanvasBounds()
	{
		var graphicsView = App.WaitForElement(DragTarget);
		var graphicsViewRect = graphicsView.GetRect();
		App.DragCoordinates(graphicsViewRect.CenterX(), graphicsViewRect.Y, graphicsViewRect.CenterX(), graphicsViewRect.Y + 600);
		VerifyScreenshot();
	}
#endif
}
