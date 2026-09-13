using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37532 : _IssuesUITest
{
	public Issue37532(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell Sometimes Shows Hamburger Icon Instead of Arrow";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellBackButtonShowsArrowNotHamburger()
	{
		// Wait for the main page to be ready
		App.WaitForElement("Issue37532MainPageLabel");

		// Open the flyout and tap "Help" (1st navigation)
		App.TapInShellFlyout("HelpMenuItem");
		App.WaitForElementTillPageNavigationSettled("Issue37532HelpPageLabel");

		// Go back to the main page
#if WINDOWS
		App.TapBackArrow();
#elif MACCATALYST
		App.TapBackArrow("Main Page");
#elif IOS
		if (HelperExtensions.IsIOS26OrHigher((AppiumIOSApp)App))
			App.Back();
		else
			App.TapBackArrow("Main Page");
#else
		App.Back();
#endif
		App.WaitForElementTillPageNavigationSettled("Issue37532MainPageLabel");

		// Navigate to Help again — the reported regression only reproduces on the
		// 2nd+ navigation to the Help page, not the first.
		App.TapInShellFlyout("HelpMenuItem");
		App.WaitForElementTillPageNavigationSettled("Issue37532HelpPageLabel");

		VerifyScreenshot();
	}
}
