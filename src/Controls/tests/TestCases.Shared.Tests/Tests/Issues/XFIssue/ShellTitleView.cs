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

	const string Page2 = "page 2";
	const string Page3 = "page 3";
	const string Page4 = "page 4";

#if WINDOWS
    const string TitleViewId="commandBar";
	const string Page= "navViewItem";
#else
	const string TitleViewId = "TitleViewId";
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
		var topTab = App.WaitForTabElement(Page).GetRect();
		var titleViewBottom = titleView.Y + titleView.Height;
		var topTabTop = topTab.Y;
		Assert.That(topTabTop, Is.GreaterThanOrEqualTo(titleViewBottom), "Title View is incorrectly positioned behind tabs");
	}

	[Test]
	public void NoDuplicateTitleViews()
	{
		App.WaitForElement(TitleViewId);
		Assert.That(App.FindElements(TitleViewId).Count, Is.EqualTo(1));
		App.TapTab(Page2, true);
		App.WaitForElement("Instructions");
		App.TapTab(Page3, true);
		App.WaitForElement("Instructions");
		App.TapTab(Page4, true);
		App.WaitForElement(TitleViewId);
		Assert.That(App.FindElements(TitleViewId).Count, Is.EqualTo(1));
	}
}
