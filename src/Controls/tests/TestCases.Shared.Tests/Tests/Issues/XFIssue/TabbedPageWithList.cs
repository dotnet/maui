using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class TabbedPageWithList : _IssuesUITest
{
	const string TabTwo = "Tab Two";
	const string ListPage = "List Page";

	public TabbedPageWithList(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage with list";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithListViewIssueTestsAllElementsPresent()
	{
		App.WaitForTabElement(TabTwo);
		App.WaitForTabElement(ListPage);
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithListViewIssueTestsNavigateToAndVerifyListView()
	{
		App.TapTab(ListPage);

		App.WaitForElement("Jason");
		App.WaitForElement("Ermau");
		App.WaitForElement("Seth");
	}
}