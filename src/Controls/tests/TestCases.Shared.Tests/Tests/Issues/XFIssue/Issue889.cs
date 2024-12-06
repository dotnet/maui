using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue889 : _IssuesUITest
{

#if ANDROID
	const string Tab2Title = "TAB 2 TITLE";
#else
	const string Tab2Title = "Tab 2 Title";
#endif
	public Issue889(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Assigning to FlyoutPage.Detail after construction doesn't work";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void Issue899TestsAppCrashWhenSwitchingTabs()
	{
		App.WaitForElement("PushPage");
		App.Tap("PushPage");
		App.WaitForElement("PushedPageLabel");
		
#if IOS || MACCATALYST
		App.Tap(AppiumQuery.ByName("Initial Page"));
#else
		App.TapBackArrow();
#endif

		App.Tap(Tab2Title);
		App.WaitForElement("SecondTabPageButton");
	}
}
