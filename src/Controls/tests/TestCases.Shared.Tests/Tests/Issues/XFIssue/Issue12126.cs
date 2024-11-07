using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue12126 : _IssuesUITest
{
	public Issue12126(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[iOS] TabBarIsVisible = True/False breaking for multiple nested pages";

	
	[Test]
	[Category(UITestCategories.Shell)]
	public void NavigatingBackFromMultiplePushPagesChangesTabVisibilityCorrectly()
	{
		App.WaitForElement("TestReady");
#if ANDROID
		App.Tap(AppiumQuery.ByXPath("//android.widget.ImageButton[@content-desc='Navigate up']"));App.Tap("Navigate up");
#else
		App.Tap("NavigationViewBackButton");
#endif

#if WINDOWS
		App.WaitForElement("navViewItem");
#else
		App.WaitForElement("Tab 1");
#endif
	}
}