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
		App.WaitForElement("HideAndNavigateButton");
		App.Tap("HideAndNavigateButton");
		App.WaitForElement("Tab5Content");
		VerifyScreenshot();
	}
#endif

#if IOS || MACCATALYST
	[Test]
	[Category(UITestCategories.Shell)]
	public void SubPageNavigationShouldWorkAfterFirstTabBecomesInvisible()
	{
		App.WaitForElement("HideAndNavigateButton");
		App.Tap("HideAndNavigateButton");
		App.WaitForElement("Tab5Content");
		App.Tap("NavigateToPage51Button");
		VerifyScreenshot();
	}
#endif
}
