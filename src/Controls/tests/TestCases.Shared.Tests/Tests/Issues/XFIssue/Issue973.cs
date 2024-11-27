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
	//[FailsOnAndroidWhenRunningOnXamarinUITest]
	//[Description("Test tab reset when swapping out detail")]
	//public void Issue973TestsTabResetAfterDetailSwap()
	//{
	//	App.WaitForElement(q => q.Marked("Initial Page Left aligned"));
	//	App.WaitForElement(q => q.Marked("Tab 1"));

	//	App.Tap(q => q.Marked("Tab 2"));
	//	App.WaitForElement(q => q.Marked("Initial Page Right aligned"));
	//	App.Screenshot("Tab 2 showing");

	//	App.Tap(q => q.Marked("Present Flyout"));

	//	App.Tap(q => q.Marked("Page 4"));
	//	App.Screenshot("Change detail page");

	//	App.Tap(q => q.Marked("Close Flyout"));

	//	App.WaitForElement(q => q.Marked("Page 4 Left aligned"));
	//	App.Screenshot("Tab 1 Showing and tab 1 should be selected");

	//	App.Tap(q => q.Marked("Tab 2"));
	//	App.WaitForElement(q => q.Marked("Page 4 Right aligned"));
	//	App.Screenshot("Tab 2 showing");
	//}
}