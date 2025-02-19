using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Shell)]
public class ShellTitleView : _IssuesUITest
{
	public ShellTitleView(TestDevice testDevice) : base(testDevice)
	{
	}
#if ANDROID
	const string Page2 = "PAGE 2";
	const string Page3 = "PAGE 3";
	const string Page4 = "PAGE 4";
#else
    const string Page2 = "page 2";
    const string Page3 = "page 3";
    const string Page4 = "page 4";
#endif

#if WINDOWS
    const string TitleViewId="commandBar";
#else
    const string TitleViewId = "TitleViewId";
#endif

#if WINDOWS
    const string Page="navViewItem";
#elif ANDROID
    const string Page = "PAGE 2";
#else
    const string Page = "page 2";
#endif
    public override string Issue => "Shell Title View Tests";

	[Test]
	public void TitleWidthMeasuresCorrectly_13949()
	{
		App.TapInShellFlyout("Width Measure (13949)");
		App.WaitForElement("Text");
		App.WaitForElement("B1");
		App.WaitForElement("B2");
	}

	[Test]
	public void TitleWidthWithToolBarItemMeasuresCorrectly_13949()
	{
		App.TapInShellFlyout("Width Measure and ToolBarItem (13949)");
		App.WaitForElement("Text");
		App.WaitForElement("B1");
		App.WaitForElement("B2");
	}

	[Test]
	public void TitleViewPositionsCorrectly()
	{
		var titleView = App.WaitForElement(TitleViewId).GetRect();
		var topTab = App.WaitForElement(Page).GetRect();
		var titleViewBottom = titleView.Y + titleView.Height;
		var topTabTop = topTab.Y;
		Assert.That(topTabTop, Is.GreaterThanOrEqualTo(titleViewBottom), "Title View is incorrectly positioned behind tabs");
	}

	[Test]
	public void NoDuplicateTitleViews()
	{
		App.WaitForElement(TitleViewId);
		Assert.That(App.FindElements(TitleViewId).Count, Is.EqualTo(1));
		TapTobTab(Page2);
		App.WaitForElement("Instructions");
		TapTobTab(Page3);
		App.WaitForElement("Instructions");
		TapTobTab(Page4);
		App.WaitForElement(TitleViewId);
		Assert.That(App.FindElements(TitleViewId).Count, Is.EqualTo(1));
	}
	void TapTobTab(string tab)
	{
#if WINDOWS
        App.Tap("navViewItem");
#endif
		App.Tap(tab);
	}
}
