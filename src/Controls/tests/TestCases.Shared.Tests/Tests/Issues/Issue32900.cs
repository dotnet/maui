using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32900 : _IssuesUITest
{
	public Issue32900(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "'Search Recipe' and 'My Recipe' tab are missing on MacOS Tahoe 26.1";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldBeVisibleOnMacOS()
	{
		// Wait for the first tab to be visible
		App.WaitForElement("Search Recipe");

		// Verify both tabs are present and visible
		App.WaitForElement("Search Recipe");
		App.WaitForElement("My Recipe");

		// Tap the second tab
		App.Tap("My Recipe");

		// Verify the second tab content is displayed
		App.WaitForElement("MyRecipeLabel");
	}
}
