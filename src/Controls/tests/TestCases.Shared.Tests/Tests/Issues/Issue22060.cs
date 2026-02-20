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
		App.EnterTextInShellSearchHandler("Hello");
#if IOS // When the search handler is focused, the Cancel button is displayed on the right side of the search bar only on the iOS platform.
		App.Tap("Cancel");
#endif
		VerifyScreenshot();
	}
}