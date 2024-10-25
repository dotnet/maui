using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue973 : _IssuesUITest
{
	public Issue973(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ActionBar doesn't immediately update when nested TabbedPage is changed";

	//[Test]
	//[Category(UITestCategories.TabbedPage)]
	//[FailsOnAndroid]
	//[Description("Test tab reset when swapping out detail")]
	//public void Issue973TestsTabResetAfterDetailSwap()
	//{
	//	RunningApp.WaitForElement(q => q.Marked("Initial Page Left aligned"));
	//	RunningApp.WaitForElement(q => q.Marked("Tab 1"));

	//	RunningApp.Tap(q => q.Marked("Tab 2"));
	//	RunningApp.WaitForElement(q => q.Marked("Initial Page Right aligned"));
	//	RunningApp.Screenshot("Tab 2 showing");

	//	RunningApp.Tap(q => q.Marked("Present Flyout"));

	//	RunningApp.Tap(q => q.Marked("Page 4"));
	//	RunningApp.Screenshot("Change detail page");

	//	RunningApp.Tap(q => q.Marked("Close Flyout"));

	//	RunningApp.WaitForElement(q => q.Marked("Page 4 Left aligned"));
	//	RunningApp.Screenshot("Tab 1 Showing and tab 1 should be selected");

	//	RunningApp.Tap(q => q.Marked("Tab 2"));
	//	RunningApp.WaitForElement(q => q.Marked("Page 4 Right aligned"));
	//	RunningApp.Screenshot("Tab 2 showing");
	//}
}