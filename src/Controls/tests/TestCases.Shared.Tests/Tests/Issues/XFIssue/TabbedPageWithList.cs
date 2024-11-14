using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class TabbedPageWithList : _IssuesUITest
{
	public TabbedPageWithList(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage with list";

	[Test]
	public void TabbedPageWithListViewIssueTestsAllElementsPresent()
	{
		App.WaitForElement("Tab Two");
		App.WaitForElement("List Page");
	}

	[Test]
	public void TabbedPageWithListViewIssueTestsNavigateToAndVerifyListView()
	{
		App.Tap("List Page");

		App.WaitForElement("Jason");
		App.WaitForElement("Ermau");
		App.WaitForElement("Seth");
	}
}