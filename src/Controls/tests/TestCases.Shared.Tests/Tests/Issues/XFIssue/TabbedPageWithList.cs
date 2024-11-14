using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class TabbedPageWithList : _IssuesUITest
{
	public TabbedPageWithList(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage with list";

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithListViewIssueTestsAllElementsPresent()
	{
		App.WaitForElement("TAB TWO");
		App.WaitForElement("LIST PAGE");
	}

	[Test]
	[Category(UITestCategories.TabbedPage)]
	public void TabbedPageWithListViewIssueTestsNavigateToAndVerifyListView()
	{
		App.Tap("List Page");

		App.WaitForElement("Jason");
		App.WaitForElement("Ermau");
		App.WaitForElement("Seth");
	}
}