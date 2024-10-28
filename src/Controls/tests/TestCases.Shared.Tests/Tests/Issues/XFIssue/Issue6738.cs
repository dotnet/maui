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
	//	App.WaitForElement(pushAutomationId);
	//	App.Tap(pushAutomationId);
	//	App.WaitForElement(insertAutomationId);
	//	App.Tap(insertAutomationId);

	//	TapInFlyout(flyoutOtherTitle, timeoutMessage: flyoutOtherTitle);
	//	TapInFlyout(flyoutMainTitle, timeoutMessage: flyoutMainTitle);

	//	App.WaitForElement(returnAutomationId);
	//	App.Tap(returnAutomationId);
	//	App.NavigateBack();
	//	App.NavigateBack();
	//}
}