using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12685 : _IssuesUITest
{
	public Issue12685(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOs][Bug] TapGestureRecognizer in Path does not work on iOS";

	// [Test]
	// [Category(UITestCategories.Shape)]
	// [FailsOnIOS]
	// public void ShapesPathReceiveGestureRecognizers()
	// {
	// 	var testLabel = RunningApp.WaitForFirstElement(StatusLabelId);
	// 	Assert.AreEqual(ResetStatus, testLabel.ReadText());
	// 	var testPath = RunningApp.WaitForFirstElement(PathId);
	// 	var pathRect = testPath.Rect;
	// 	RunningApp.TapCoordinates(pathRect.X + 1, pathRect.Y + 1);
	// 	Assert.AreEqual(ClickedStatus, RunningApp.WaitForFirstElement(StatusLabelId).ReadText());
	// }
}