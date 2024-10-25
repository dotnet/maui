using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue10608 : _IssuesUITest
{
	const string OpenLeftId = "OpenLeftId";
	const string OpenRightId = "OpenRightId";
	const string OpenTopId = "OpenTopId";
	const string OpenBottomId = "OpenBottomId";
	const string CloseId = "CloseId";

	public Issue10608(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [Shell] [iOS] Locked flyout causes application to freezes when quickly switching between tabs"; 
	
	// [Test]
	// [Category(UITestCategories.Shell)]
	// public void ShellWithTopTabsFreezesWhenNavigatingFlyoutItems()
	// {
	// 	RunningApp.Tap("FlyoutItem6");
	// 	RunningApp.Tap("FlyoutItem0");
	// 	for (int i = 0; i < 5; i++)
	// 	{
	// 		RunningApp.WaitForElement("Tab1AutomationId");
	// 		RunningApp.WaitForElement("LearnMoreButton");
	// 		RunningApp.Tap("FlyoutItem0");
	// 		RunningApp.Tap("FlyoutItem1");
	// 		RunningApp.Tap("FlyoutItem0");
	// 		RunningApp.WaitForElement("LearnMoreButton");
	// 	}

	// 	RunningApp.WaitForElement("Tab1AutomationId");
	// 	RunningApp.WaitForElement("LearnMoreButton");
	// 	RunningApp.Tap("FlyoutItem1");
	// 	RunningApp.WaitForElement("Tab2AutomationId");
	// 	RunningApp.WaitForElement("LearnMoreButton");
	// 	RunningApp.Tap("FlyoutItem0");
	// 	RunningApp.WaitForElement("Tab1AutomationId");
	// 	RunningApp.WaitForElement("LearnMoreButton");
	// }
}
