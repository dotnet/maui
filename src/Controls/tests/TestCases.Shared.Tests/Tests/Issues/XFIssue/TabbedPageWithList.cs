using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class TabbedPageWithList : _IssuesUITest
{
#if ANDROID
	const string TabTwo = "TAB TWO";
	const string ListPage = "LIST PAGE";
#else
	const string TabTwo = "Tab Two";
	const string ListPage = "List Page";
#endif

	public TabbedPageWithList(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage with list";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithListViewIssueTestsAllElementsPresent()
	{
		App.WaitForElement(TabTwo);
		App.WaitForElement(ListPage);
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithListViewIssueTestsNavigateToAndVerifyListView()
	{
		App.WaitForElement(ListPage);
		App.Tap(ListPage);

		App.WaitForElement("Jason");
		App.WaitForElement("Ermau");
		App.WaitForElement("Seth");
	}
}