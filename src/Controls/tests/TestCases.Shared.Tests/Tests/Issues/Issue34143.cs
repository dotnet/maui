using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34143 : _IssuesUITest
{
	public override string Issue => "Tab bar ghosting issue after navigating from modal via GoToAsync";

	public Issue34143(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldBeVisibleAfterNavigatingFromModalViaGoToAsync()
	{
		// Start on Home page
		App.WaitForElement("Issue34143PushModal");

		// Push a modal page
		App.Tap("Issue34143PushModal");

		// Verify modal is shown
		App.WaitForElement("Issue34143GoToTabBar");

		// Navigate from modal to the tab bar using GoToAsync
		App.Tap("Issue34143GoToTabBar");

		// Verify we landed on Tab 1 and the tab bar tabs are interactable (not ghosted)
		App.WaitForElement("Issue34143TabContent");

		VerifyScreenshot();
	}
}
