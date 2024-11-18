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
	// 	App.Tap("FlyoutItem6");
	// 	App.Tap("FlyoutItem0");
	// 	for (int i = 0; i < 5; i++)
	// 	{
	// 		App.WaitForElement("Tab1AutomationId");
	// 		App.WaitForElement("LearnMoreButton");
	// 		App.Tap("FlyoutItem0");
	// 		App.Tap("FlyoutItem1");
	// 		App.Tap("FlyoutItem0");
	// 		App.WaitForElement("LearnMoreButton");
	// 	}

	// 	App.WaitForElement("Tab1AutomationId");
	// 	App.WaitForElement("LearnMoreButton");
	// 	App.Tap("FlyoutItem1");
	// 	App.WaitForElement("Tab2AutomationId");
	// 	App.WaitForElement("LearnMoreButton");
	// 	App.Tap("FlyoutItem0");
	// 	App.WaitForElement("Tab1AutomationId");
	// 	App.WaitForElement("LearnMoreButton");
	// }
}
