using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TitleView)]
public class Issue15565 : _IssuesUITest
{

	const string Page1 = "page 1";
	const string Page2 = "page 2";
	const string Page3 = "page 3";

	public Issue15565(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell TitleView and ToolBarItems rendering strange display on iOS 16";

	[Test]
	public void TitleViewHeightIsNotZero()
	{
		App.TapTab(Page1, true);
#if WINDOWS // In Windows the Page 1 items are inside the root navViewItem which shows in popup, so we need to tap it once to make them visible..
		App.Tap("navViewItem");
#endif
		var titleView = App.WaitForElement("title 1").GetRect();
		var topTab = App.WaitForTabElement(Page1).GetRect();

		var titleViewBottom = titleView.Y + titleView.Height;
		var topTabTop = topTab.Y;

		Assert.That(topTabTop, Is.GreaterThanOrEqualTo(titleViewBottom), "Title View is incorrectly positioned in iOS 16");
	}


	[Test]
	public void ToolbarItemsWithTitleViewAreRendering()
	{
		App.WaitForElement("Item 1");
		App.WaitForElement("Item 2");
	}

	[Test]
	public void NoDuplicateTitleViews()
	{
		App.WaitForElement("title 1");
		ValidateElementsCount("title 1");
		App.TapTab(Page1, true);
		App.TapTab(Page2, true);
		App.TapTab(Page3, true);
		ValidateElementsCount("title 3");
	}


	void ValidateElementsCount(string element)
	{
#if WINDOWS // FindElements without query fails on Mac, with query fails on Windows; below condition ensures cross-platform compatibility
        Assert.That(App.FindElements(element).Count, Is.EqualTo(1));
#else
		Assert.That(App.FindElements(AppiumQuery.ById(element)).Count, Is.EqualTo(1));
#endif
	}

}