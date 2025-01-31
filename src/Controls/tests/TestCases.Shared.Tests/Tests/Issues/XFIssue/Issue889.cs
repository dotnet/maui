using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue889 : _IssuesUITest
{
	string _tab2Title = "Tab 2 Title";

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
		var initialPageQuery = AppiumQuery.ByName("Initial Page");
		App.WaitForElement(initialPageQuery);
		App.Tap(initialPageQuery);
#else

#if WINDOWS
		App.TapBackArrow();
#else
		App.Back();
#endif

#endif

#if ANDROID
		_tab2Title = _tab2Title.ToUpperInvariant();
#endif
		App.WaitForElement(_tab2Title);
		App.TapTab(_tab2Title);
		App.WaitForElement("SecondTabPageButton");
	}
}
