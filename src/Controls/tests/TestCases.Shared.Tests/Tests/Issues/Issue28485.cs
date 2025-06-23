#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28485 : _IssuesUITest
{
	public Issue28485(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Back-navigation with swipe-back navigates back twice";

	[Test]
	[Category(UITestCategories.Shell)]
	[Category(UITestCategories.Navigation)]
	public void SwipeBackGestureShouldNavigateOnce()
	{
		App.WaitForElement("GotoPage2");
		App.Click("GotoPage2");
		App.WaitForElement("GotoPage3");
		App.Click("GotoPage3");
		App.WaitForElement("Page3Label");
		App.Click("Page3Label");
		App.SwipeBackNavigation();
		App.WaitForElement("Page2Label");
		App.WaitForNoElement("GotoPage2");
	}
}
#endif