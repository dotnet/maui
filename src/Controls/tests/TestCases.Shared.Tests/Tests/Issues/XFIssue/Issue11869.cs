using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11869 : _IssuesUITest
{

	const string TopTab2 = "TopTab2";
	const string TopTab3 = "TopTab3";

	const string HideTop2 = "HideTop2";
	const string HideTop3 = "HideTop3";
	const string HideBottom2 = "HideBottom2";
	const string HideBottom3 = "HideBottom3";
	const string Tab2 = "Tab 2";
	const string Tab3 = "Tab 3";
	const string ShowAllTabs = "ShowAllTabs";



	public Issue11869(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] ShellContent.IsVisible issue on Android";

	[Test]
	[Category(UITestCategories.Shell)]
	public void IsVisibleWorksForShowingHidingTabs()
	{
		App.TapTab(TopTab2);
		App.Tap(HideTop2);
#if WINDOWS
		App.Tap("navViewItem");
#endif
		App.WaitForTabElement(TopTab2);

		App.WaitForTabElement(TopTab3);
		App.Tap(HideTop3);
		App.WaitForNoElement(TopTab3);


		App.WaitForElement(Tab2);
		App.Tap(HideBottom2);
		App.WaitForNoElement(Tab2);

		App.WaitForElement(Tab3);
		App.Tap(HideBottom3);
		App.WaitForNoElement(Tab3);

		App.Tap(ShowAllTabs);
#if WINDOWS
		App.Tap("navViewItem");
#endif
		App.WaitForTabElement(TopTab2);
		App.WaitForTabElement(TopTab3);

		App.WaitForElement(Tab2);
		App.WaitForElement(Tab3);
	}
}