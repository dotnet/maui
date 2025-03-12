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
#if ANDROID
		string tab1Title = "HASNAVIGATIONPAGE";
		string tab2Title = "NONAVIGATIONPAGE";
#else
		string tab1Title = "HasNavigationPage";
		string tab2Title = "NoNavigationPage";
#endif
		App.WaitForElement(tab1Title);
		App.Tap(tab1Title);
		App.WaitForElement("button");
		App.Tap("button");
		App.WaitForElement(tab2Title);
		App.Tap(tab2Title);
#if WINDOWS
		VerifyScreenshot(includeTitleBar: true); // On Windows, the back button is visible in the title bar
#else
		VerifyScreenshot();
#endif
	}
}