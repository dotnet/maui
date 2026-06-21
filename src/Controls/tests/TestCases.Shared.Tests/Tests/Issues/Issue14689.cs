using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14689 : _IssuesUITest
{
	public Issue14689(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "TabbedPage Back button not updated";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageBackButtonUpdated()
	{
		App.TapTab("HasNavigationPage");
		App.WaitForElement("button");
		App.Tap("button");
		App.TapTab("NoNavigationPage");
#if WINDOWS
		VerifyScreenshot(includeTitleBar: true); // On Windows, the back button is visible in the title bar
#else
		VerifyScreenshot();
#endif
	}
}