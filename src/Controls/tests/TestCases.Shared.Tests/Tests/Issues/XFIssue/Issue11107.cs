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
	public void TabShouldntBeVisibleWhenThereIsOnlyOnePage()
	{
		RunTests();
		App.Tap("RunTestTabBarIsVisible");
		RunTests();
		App.Tap("RunTestTwoTabs");
		RunTests();
		
		App.Back();

		void RunTests()
		{
			App.WaitForElement("SecondPageLoaded");
			App.WaitForNoElement("Tab1AutomationId");
			App.Back();
			App.WaitForElement("Page1Loaded");
			App.WaitForNoElement("Tab1AutomationId");
		}
	}
}