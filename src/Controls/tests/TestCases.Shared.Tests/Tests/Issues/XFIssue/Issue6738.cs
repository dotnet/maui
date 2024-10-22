using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6738 : _IssuesUITest
{
	public Issue6738(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout Navigation fails when coupled with tabs that have a stack";

	//[Test]
	//[Category(UITestCategories.Shell)]
	//[FailsOnIOS]
	//public void FlyoutNavigationBetweenItemsWithNavigationStacks()
	//{
	//	RunningApp.WaitForElement(pushAutomationId);
	//	RunningApp.Tap(pushAutomationId);
	//	RunningApp.WaitForElement(insertAutomationId);
	//	RunningApp.Tap(insertAutomationId);

	//	TapInFlyout(flyoutOtherTitle, timeoutMessage: flyoutOtherTitle);
	//	TapInFlyout(flyoutMainTitle, timeoutMessage: flyoutMainTitle);

	//	RunningApp.WaitForElement(returnAutomationId);
	//	RunningApp.Tap(returnAutomationId);
	//	RunningApp.NavigateBack();
	//	RunningApp.NavigateBack();
	//}
}