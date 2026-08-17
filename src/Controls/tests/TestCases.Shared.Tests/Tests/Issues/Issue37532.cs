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

		// Open the flyout and tap "Help"
		App.TapInShellFlyout("Help");

		App.WaitForElement("Issue37532HelpPageLabel");
		VerifyScreenshot();
	}
}
