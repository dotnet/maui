#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15301TabbedPage : _IssuesUITest
{
	public Issue15301TabbedPage(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage.TabActiveTapped";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void ClickingCurrentTabShouldCallTabActiveTappedEvent()
	{
		App.WaitForElement("Deep0Button");
		App.Click("Deep0Button");
		App.WaitForElement("Deep1Label");
		App.Click("Tab 1");
		App.WaitForElement("Deep0Button");
	}
}

#endif