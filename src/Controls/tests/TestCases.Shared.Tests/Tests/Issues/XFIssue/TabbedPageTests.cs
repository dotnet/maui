using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class TabbedPageTests : _IssuesUITest
{
	public TabbedPageTests(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage nav tests";

	//	[Test]
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void TabbedPageWithModalIssueTestsAllElementsPresent()
	//{
	//	App.WaitForElement(q => q.Marked("Page 1"));
	//	App.WaitForElement(q => q.Marked("Page 2"));
	//	App.WaitForElement(q => q.Button("Pop"));

	//	App.Screenshot("All elements present");
	//}

	//[Test]
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void TabbedPageWithModalIssueTestsPopFromFirstTab()
	//{
	//	App.Tap(q => q.Button("Pop"));
	//	App.WaitForElement(q => q.Marked("Bug Repro's"));

	//	App.Screenshot("Popped from first tab");
	//}

	//[Test]
	//[FailsOnIOSWhenRunningOnXamarinUITest]
	//public void TabbedPageWithModalIssueTestsPopFromSecondTab()
	//{
	//	App.Tap(q => q.Marked("Page 2"));
	//	App.WaitForElement(q => q.Button("Pop 2"));
	//	App.Screenshot("On second tab");

	//	App.Tap(q => q.Button("Pop 2"));
	//	App.WaitForElement(q => q.Marked("Bug Repro's"));
	//	App.Screenshot("Popped from second tab");
	//}
}