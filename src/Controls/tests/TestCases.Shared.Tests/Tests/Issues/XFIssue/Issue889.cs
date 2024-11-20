using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue889 : _IssuesUITest
{
	public Issue889(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Assigning to FlyoutPage.Detail after construction doesn't work";

	//[Test]
	//[Category(UITestCategories.FlyoutPage)]
	//[Description("Reproduce app crash - Issue #983")]
	//public void Issue899TestsAppCrashWhenSwitchingTabs()
	//{
	//	App.Tap(q => q.Marked("Push new page"));
	//	App.WaitForElement(q => q.Marked("I have been pushed"));
	//	App.Screenshot("Push page");
	//	App.Back();
	//	App.Screenshot("Navigate back");

	//	App.Tap(q => q.Marked("Tab 2 Title"));
	//	App.Screenshot("Go to second tab");
	//}
}