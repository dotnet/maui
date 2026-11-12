#if TEST_FAILS_ON_CATALYST //https://github.com/dotnet/maui/issues/39956
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.DragAndDrop)]
public class Issue17554 : _IssuesUITest
{
	public Issue17554(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "[Android] DragGestureRecognizer.DropCompleted event not firing";

	[Test]
	public void DropCompletedFiresWhenDroppingOnNonDropTarget()
	{
		App.WaitForElement("DragSource");
		App.WaitForElement("NonDropTarget");

		App.DragAndDrop("DragSource", "NonDropTarget");
		Assert.That(
			App.WaitForTextEqualToElement("StatusLabel", "DropCompleted"),
			Is.True,
			"DropCompleted was not raised after dropping on a non-drop target.");
	}
}
#endif
