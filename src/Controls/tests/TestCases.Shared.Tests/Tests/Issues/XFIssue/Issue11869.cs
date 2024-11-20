using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11869 : _IssuesUITest
{
	public Issue11869(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] ShellContent.IsVisible issue on Android";

	// [Test]
	// [Category(UITestCategories.Shell)]
	// public void IsVisibleWorksForShowingHidingTabs()
	// {
	// 	App.WaitForElement("TopTab2");
	// 	App.Tap("HideTop2");
	// 	App.WaitForNoElement("TopTab2");

	// 	App.WaitForElement("TopTab3");
	// 	App.Tap("HideTop3");
	// 	App.WaitForNoElement("TopTab3");

	// 	App.WaitForElement("Tab 2");
	// 	App.Tap("HideBottom2");
	// 	App.WaitForNoElement("Tab 2");

	// 	App.WaitForElement("Tab 3");
	// 	App.Tap("HideBottom3");
	// 	App.WaitForNoElement("Tab 3");

	// 	App.Tap("ShowAllTabs");
	// 	App.WaitForElement("TopTab2");
	// 	App.WaitForElement("TopTab3");
	// 	App.WaitForElement("Tab 2");
	// 	App.WaitForElement("Tab 3");
	// }
}