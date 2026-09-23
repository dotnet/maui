using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32994 : _IssuesUITest
{
	public Issue32994(TestDevice device) : base(device) { }

	public override string Issue => "Shell TabBarIsVisible binding not working on ShellContent";
	protected override bool ResetAfterEachTest => true;

	void SelectPage(string page)
	{
		if (App is AppiumWindowsApp)
			App.TapTab("Tab1");

		App.TapTab(page);
	}

	void AssertTabBarHidden()
	{
		// Tab2 exists only in the native tab bar, not in the current page content.
		App.WaitForNoElement(() => App.FindElements("Tab2").FirstOrDefault(e => e.IsDisplayed()),
			"The native tab bar is still visible.");
	}

	void AssertTabBarWorks()
	{
		App.TapTab("Tab2");
		App.WaitForElement("Tab2Label");
		App.TapTab("Tab1");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarVisibilityHidesOnPage1UsingDirectSet()
	{
		App.WaitForElement("HidePage1TabBar");
		AssertTabBarWorks();
		App.Tap("HidePage1TabBar");
		AssertTabBarHidden();
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarVisibilityShowsOnPage1UsingDirectSet()
	{
		App.WaitForElement("HidePage1TabBar");
		App.Tap("HidePage1TabBar");
		AssertTabBarHidden();
		App.Tap("ShowPage1TabBar");
		AssertTabBarWorks();
		App.WaitForElement("ShowPage1TabBar");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarVisibilityShowsOnPage2UsingBinding()
	{
		App.WaitForElement("HidePage2TabBar");
		App.Tap("HidePage2TabBar");
		App.Tap("ShowPage2TabBar");
		SelectPage("Page2");
		App.WaitForElement("Page2Label");
		AssertTabBarWorks();
		App.WaitForElement("Page2Label");
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void TabBarVisibilityHidesOnPage2UsingBinding()
	{
		App.WaitForElement("HidePage2TabBar");
		App.Tap("HidePage2TabBar");
		SelectPage("Page2");
		App.WaitForElement("Page2Label");
		AssertTabBarHidden();
		VerifyScreenshot();
	}
}
