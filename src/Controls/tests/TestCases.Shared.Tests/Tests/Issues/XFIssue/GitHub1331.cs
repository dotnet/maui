#if ANDROID // This test only makes sense on platforms using Long Press to activate context menus
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class GitHub1331 : _IssuesUITest
{
	public GitHub1331(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Android] ViewCell shows ContextActions on tap instead of long press";

	// [Test]
	// [Category(UITestCategories.Gestures)]
	// public void SingleTapOnCellDoesNotActivateContext()
	// {
	// 	RunningApp.WaitForElement(Action);
		
	// 	RunningApp.Tap(Action);
	// 	RunningApp.WaitForElement(ActionItemTapped);

	// 	// Tapping the part of the cell without a tap gesture should *not* display the context action
	// 	RunningApp.Tap(CellItem);
	// 	RunningApp.WaitForNoElement("Context Action");

	// 	// But a Long Press *should* still display the context action
	// 	RunningApp.TouchAndHold(CellItem);
	// 	RunningApp.WaitForElement("Context Action");
	// }
}
#endif