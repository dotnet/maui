#if IOS // Flyout icon and content page title disappeared when focus on the search handler in iOS platform alone
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22060 : _IssuesUITest
{
	public override string Issue => "Flyout icon and content page title disappeared after focusing on the search handler";

	public Issue22060(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	[Category(UITestCategories.SearchBar)]
	public void ShouldAppearFlyoutIconAndContentPageTitle()
	{
		App.EnterText(AppiumQuery.ByXPath("//XCUIElementTypeSearchField"), "Hello");
		App.Tap("Cancel");
		VerifyScreenshot();
	}
}
#endif