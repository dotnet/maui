using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12685 : _IssuesUITest
{
	const string StatusLabelId = "StatusLabelId";
	const string ResetStatus = "Path touch event not fired, touch path above.";
	const string ClickedStatus = "Path was clicked, click reset button to start over.";
	public Issue12685(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOs][Bug] TapGestureRecognizer in Path does not work on iOS";

	[Test]
	[Category(UITestCategories.Shape)]
	public void ShapesPathReceiveGestureRecognizers()
	{
		var testLabel = App.WaitForFirstElement(StatusLabelId);
		Assert.That(testLabel.ReadText(), Is.EqualTo(ResetStatus));
		var labelRect = App.WaitForFirstElement(StatusLabelId).GetRect(); // Path element not able get via automationid so getting the rect of the label calculated points to tap on the path
#if MACCATALYST // TapCoordinates is not working on MacCatalyst Issue: https://github.com/dotnet/maui/issues/19754
        App.ClickCoordinates(labelRect.X + 3, labelRect.Y - 10);
#else
		App.TapCoordinates(labelRect.X + 3, labelRect.Y - 10);
#endif
		App.WaitForElement(StatusLabelId);
		Assert.That(testLabel.ReadText(), Is.EqualTo(ClickedStatus));
	}
}