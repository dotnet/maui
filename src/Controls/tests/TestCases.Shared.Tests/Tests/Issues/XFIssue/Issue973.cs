using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue973 : _IssuesUITest
{
	public Issue973(TestDevice testDevice) : base(testDevice)
	{
	}
	const string Tab1 = "Tab 1";
	const string Tab2 = "Tab 2";

	public override string Issue => "ActionBar doesn't immediately update when nested TabbedPage is changed";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	[Description("Test tab reset when swapping out detail")]
	public void Issue973TestsTabResetAfterDetailSwap()
	{
		App.WaitForElement("Initial Page Left aligned");
		App.TapTab(Tab2);
		App.WaitForElement("Initial Page Right aligned");
		App.Tap("Present Flyout");
		App.Tap("Page 4");
		App.WaitForElement("Close Flyout");
		App.Tap("Close Flyout");
		App.WaitForElement("Page 4 Left aligned");
		App.TapTab(Tab2);
		App.WaitForElement("Page 4 Right aligned");
	}
}
