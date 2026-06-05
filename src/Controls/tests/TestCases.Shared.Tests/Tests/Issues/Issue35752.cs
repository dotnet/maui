using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35752 : _IssuesUITest
{
	public override string Issue => "Android DragGestureRecognizer DragStarting fires prematurely on tap";

	public Issue35752(TestDevice device)
		: base(device)
	{ }

	[Test]
	[Category(UITestCategories.Gestures)]
	public void DragStartingShouldNotFireOnTapButShouldFireOnDrag()
	{
		App.WaitForElement("TestLoaded");

		App.Tap("DragBox");

		Thread.Sleep(400);

		var dragCount = App.WaitForElement("DragStartCount").GetText();
		Assert.That(dragCount, Is.EqualTo("0"),
			"DragStarting should not fire on a quick tap");

		// Initiate drag - DragStarting SHOULD fire
		App.DragAndDrop("DragBox", "DropBox");

		dragCount = App.WaitForElement("DragStartCount").GetText();
		Assert.That(dragCount, Is.EqualTo("1"),
			"DragStarting should fire when drag is initiated");
	}
}
