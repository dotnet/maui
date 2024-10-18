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

	//	[Test]
	//[FailsOnIOS]
	//public void TabbedPageWithListViewIssueTestsAllElementsPresent()
	//{
	//	RunningApp.WaitForElement(q => q.Marked("Tab Two"));
	//	RunningApp.WaitForElement(q => q.Marked("List Page"));
	//	RunningApp.Screenshot("All elements present");
	//}

	//[Test]
	//[FailsOnIOS]
	//public void TabbedPageWithListViewIssueTestsNavigateToAndVerifyListView()
	//{
	//	RunningApp.Tap(q => q.Marked("List Page"));

	//	RunningApp.WaitForElement(q => q.Marked("Jason"));
	//	RunningApp.WaitForElement(q => q.Marked("Ermau"));
	//	RunningApp.WaitForElement(q => q.Marked("Seth"));

	//	RunningApp.Screenshot("ListView correct");
	//}
}