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
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void TabbedPageWithListViewIssueTestsAllElementsPresent()
	//{
	//	App.WaitForElement(q => q.Marked("Tab Two"));
	//	App.WaitForElement(q => q.Marked("List Page"));
	//	App.Screenshot("All elements present");
	//}

	//[Test]
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void TabbedPageWithListViewIssueTestsNavigateToAndVerifyListView()
	//{
	//	App.Tap(q => q.Marked("List Page"));

	//	App.WaitForElement(q => q.Marked("Jason"));
	//	App.WaitForElement(q => q.Marked("Ermau"));
	//	App.WaitForElement(q => q.Marked("Seth"));

	//	App.Screenshot("ListView correct");
	//}
}