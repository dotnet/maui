using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26598 : _IssuesUITest
{
	public override string Issue => "Tabbar disappears when navigating back from page with hidden TabBar in iOS";

	public Issue26598(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarShouldbeVisibleNavigatingBackFromNonTabbedPage()
	{
		// Is a iOS issue; see https://github.com/dotnet/maui/issues/26598
		// Initially TabBar for Issue26598Home is hidden 
		App.WaitForElement("NavigateToInnerTab");
		App.WaitForNoElement("RecentTab");
		App.Click("NavigateToInnerTab");

		// Case 1 - After navigating to Inner Page ,  the TabBar should be visible
		App.WaitForElement("RecentTab");

		// Case 2 - Navigate to the InnerTabPage where the TabBar is hidden
		App.WaitForElement("NavigateToTabBarPage");
		App.Click("NavigateToTabBarPage");
		App.WaitForElement("Issue26589NonTab");
		App.WaitForNoElement("RecentTab");

		// Case 3 - Navigate back to the HomeTab, the TabBar should be visible
		NavigateBack("InnerTab");
		App.WaitForElement("RecentTab");
		App.Click("RecentTab");
		App.WaitForElement("Page Loaded in Recent Tab");
		App.WaitForElement("HomeTab");
		App.Click("HomeTab");
		App.WaitForElement("NavigateToTabBarPage");
		App.WaitForElement("RecentTab");

		NavigateBack("Home");
		App.WaitForElement("NavigateToInnerTab");
		App.WaitForNoElement("RecentTab");
		App.Click("NavigateToInnerTab");
		App.WaitForElement("NavigateToTabBarPage");
		App.WaitForElement("RecentTab");
	}

	void NavigateBack(string title)
	{
		if (App is AppiumAndroidApp || App is AppiumIOSApp iosApp && HelperExtensions.IsIOS26OrHigher(iosApp))
			App.TapBackArrow();
		else
			App.TapBackArrow(title);
	}
}
