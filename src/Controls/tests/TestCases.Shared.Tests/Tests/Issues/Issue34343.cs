
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34343 : _IssuesUITest
{
	public override string Issue => "TabBar displays wrong tabs after first tab becomes invisible";

	public Issue34343(TestDevice device) : base(device) { }

#if ANDROID
	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldDisplayCorrectTabsAfterFirstTabBecomesInvisible()
	{
		// Wait for the first tab to load
		App.WaitForElement("HideAndNavigateButton");

		// Hide Tab1 and navigate to Tab5 (reproduces the issue)
		App.Tap("HideAndNavigateButton");

		VerifyScreenshot();
	}
#endif

#if IOS
	[Test]
	[Category(UITestCategories.Shell)]
	public void SubPageNavigationShouldWorkAfterFirstTabBecomesInvisible()
	{
		// Wait for the first tab to load
		App.WaitForElement("HideAndNavigateButton");

		// Hide Tab1 and navigate to Tab5
		App.Tap("HideAndNavigateButton");

		// Verify Tab5 content is shown
		App.WaitForElement("Tab5Content");

		// Navigate to Page51 from Tab5
		// Bug: IsInMoreTab was not recalculated after Tab1 was removed, so Tab5
		// incorrectly routes sub-page navigation through the More tab controller.
		App.Tap("NavigateToPage51Button");
		VerifyScreenshot();
	}
#endif
}