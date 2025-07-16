using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20834 : _IssuesUITest
{
	public Issue20834(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] GraphicsView can also be visible outside the canvas";
	protected override bool ResetAfterEachTest => true;

	[Test]
	[Category(UITestCategories.GraphicsView)]
	public void GraphicsViewShouldClipCirclesAtEdgePositions()
	{
		App.WaitForElement("MoveCirclesButton");
		App.Tap("MoveCirclesButton");
		VerifyScreenshot();
	}
}
