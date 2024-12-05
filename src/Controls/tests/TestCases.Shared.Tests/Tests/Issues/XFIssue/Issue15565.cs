using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TitleView)]
public class Issue15565 : _IssuesUITest
{
#if ANDROID
	const string Page1 = "PAGE 1";
	const string Page2 = "PAGE 2";
	const string Page3 = "PAGE 3";
#else
	const string Page1 = "page 1";
	const string Page2 = "page 2";
	const string Page3 = "page 3";
#endif

	public Issue15565(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] Shell TitleView and ToolBarItems rendering strange display on iOS 16";

	[Test]
	public void TitleViewHeightIsNotZero()
	{
		TapTopTab(Page1);
#if WINDOWS // In Windows the Page 1 items are inside the root navViewItem which shows in popup, so we need to tap it once to make them visible..
		App.Tap("navViewItem");
#endif
        var titleView = App.WaitForElement("title 1").GetRect();
        var topTab = App.WaitForElement(Page1).GetRect();
 
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
		TapTopTab(Page1);
		TapTopTab(Page2);
		TapTopTab(Page3);
		ValidateElementsCount("title 3");
	}

	void TapTopTab(string tab)
	{
#if WINDOWS
		App.Tap("navViewItem");
#endif
		App.Tap(tab);		
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