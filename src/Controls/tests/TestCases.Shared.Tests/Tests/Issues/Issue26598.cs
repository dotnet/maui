using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26598 : _IssuesUITest
{
	public override string Issue => "Tabbar disappears when navigating back from page with hidden TabBar in iOS";

#if ANDROID
    const string back = "";
#else
	const string back = "InnerTab";
#endif

	public Issue26598(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldbeVisibleNavigatingBackFromNonTabbedPage()
	{
		// Is a iOS issue; see https://github.com/dotnet/maui/issues/26598
		App.WaitForElement("NavigateToInnerTab");
		App.Click("NavigateToInnerTab");
		App.WaitForElement("NavigateToTabBarPage");
		App.Click("NavigateToTabBarPage");
		App.WaitForElement("Issue26589NonTab");
		App.TapBackArrow(back);
		App.WaitForElement("Recent");
		App.Click("Recent");
		App.WaitForElement("Home");
		App.Click("Home");
		App.WaitForElement("Home");
	}
}

