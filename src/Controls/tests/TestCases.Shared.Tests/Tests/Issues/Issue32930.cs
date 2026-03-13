#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32930 : _IssuesUITest
{
	public Issue32930(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] SearchHandler suggestions list does not follow the search bar upward movement on iPhone, creating a layout gap";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerSuggestionsListFollowsSearchBar()
	{
		// Wait for the page to load
		App.WaitForElement("Instructions");

		// Enter text in the search handler to trigger suggestions display
		App.EnterText(AppiumQuery.ByXPath("//XCUIElementTypeSearchField"), "A");

		// Wait for suggestions to appear
		App.WaitForElement("Apple");

		// Verify the suggestions list appears correctly
		// If the fix works, the suggestions list should be visible and positioned directly under the search bar
		// We use screenshot verification to ensure there is no visible gap between the search bar and suggestions
		VerifyScreenshot();
	}
}
#endif
