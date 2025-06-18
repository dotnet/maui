using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue6738 : _IssuesUITest
{
	const string pushAutomationId = "PushPageButton";
	const string insertAutomationId = "InsertPageButton";
	const string returnAutomationId = "ReturnPageButton";
	const string flyoutMainTitle = "Main";
	const string flyoutOtherTitle = "Other Page";
	public Issue6738(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout Navigation fails when coupled with tabs that have a stack";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutNavigationBetweenItemsWithNavigationStacks()
	{
		App.WaitForElement(pushAutomationId);
		App.Tap(pushAutomationId);
		App.WaitForElement(insertAutomationId);
		App.Tap(insertAutomationId);
		App.TapInShellFlyout(flyoutOtherTitle);
		App.WaitForElement("Go back to main page via the flyout");
		App.TapInShellFlyout(flyoutMainTitle);
		App.WaitForElement(returnAutomationId);
		App.Tap(returnAutomationId);
		App.TapBackArrow();
		App.WaitForElementTillPageNavigationSettled("This is an extra page");
		App.TapBackArrow();
	}
}