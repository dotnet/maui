using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class TabbedPageTests : _IssuesUITest
{
	const string Page1 = "Page 1";
	const string Page2 = "Page 2";

	const string HomePage = "HomePage";
	const string Pop = "Pop";
	const string Pop2 = "Pop 2";
	public TabbedPageTests(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage nav basic tests";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithModalIssueTestsAllElementsPresent()
	{
		App.WaitForElement(HomePage);
		App.Tap(HomePage);
		App.WaitForTabElement(Page1);
		App.WaitForTabElement(Page2);
		App.WaitForElement(Pop);
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithModalIssueTestsPopFromFirstTab()
	{
		App.WaitForElement(Pop);
		App.Tap(Pop);
		App.WaitForElement(HomePage);
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithModalIssueTestsPopFromSecondTab()
	{
		App.WaitForElement(HomePage);
		App.Tap(HomePage);
		App.TapTab(Page2);
		App.WaitForElement(Pop2);

		App.Tap(Pop2);
		App.WaitForElement(HomePage);

	}
}