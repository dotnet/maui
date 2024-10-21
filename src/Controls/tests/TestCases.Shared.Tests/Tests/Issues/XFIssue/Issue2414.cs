using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2414 : _IssuesUITest
{
	public Issue2414(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "NullReferenceException when swiping over Context Actions";

	// [Test]
	// [Category(UITestCategories.TableView)]
	// [FailsOnIOS]
	// public void TestDoesntCrashShowingContextMenu()
	// {
	// 	RunningApp.ActivateContextMenu("Swipe ME");
	// 	RunningApp.WaitForElement(c => c.Marked("Text0"));
	// 	RunningApp.Screenshot("Didn't crash");
	// 	RunningApp.Tap(c => c.Marked("Text0"));
	// }

	// [Test]
	// [FailsOnIOS]
	// public void TestShowContextMenuItemsInTheRightOrder()
	// {
	// 	RunningApp.ActivateContextMenu("Swipe ME");
	// 	RunningApp.WaitForElement(c => c.Marked("Text0"));
	// 	RunningApp.Screenshot("Are the menuitems in the right order?");
	// 	RunningApp.Tap(c => c.Marked("Text0"));
	// }
}
