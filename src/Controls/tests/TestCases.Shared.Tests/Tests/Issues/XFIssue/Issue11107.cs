using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue11107 : _IssuesUITest
{
	public Issue11107(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug][iOS] Shell Navigation implicitly adds Tabbar";

	[Test]
	[Category(UITestCategories.Shell)]
	[FailsOnIOS]
	[FailsOnMac]
	public void TabShouldntBeVisibleWhenThereIsOnlyOnePage()
	{
		RunTests();
		RunningApp.Tap("RunTestTabBarIsVisible");
		RunTests();
		RunningApp.Tap("RunTestTwoTabs");
		RunTests();

		RunningApp.Back();

		void RunTests()
		{
			RunningApp.WaitForElement("SecondPageLoaded");
			RunningApp.WaitForNoElement("Tab1AutomationId");
			RunningApp.Back();
			RunningApp.WaitForElement("Page1Loaded");
			RunningApp.WaitForNoElement("Tab1AutomationId");
		}
	}
}