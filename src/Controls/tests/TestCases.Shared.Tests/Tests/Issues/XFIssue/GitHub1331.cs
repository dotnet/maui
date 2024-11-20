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
	// 	App.WaitForElement(Action);

	// 	App.Tap(Action);
	// 	App.WaitForElement(ActionItemTapped);

	// 	// Tapping the part of the cell without a tap gesture should *not* display the context action
	// 	App.Tap(CellItem);
	// 	App.WaitForNoElement("Context Action");

	// 	// But a Long Press *should* still display the context action
	// 	App.TouchAndHold(CellItem);
	// 	App.WaitForElement("Context Action");
	// }
}
#endif