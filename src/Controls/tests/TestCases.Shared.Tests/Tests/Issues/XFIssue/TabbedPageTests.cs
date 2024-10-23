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
	//[FailsOnIOS]
	//public void TabbedPageWithModalIssueTestsAllElementsPresent()
	//{
	//	RunningApp.WaitForElement(q => q.Marked("Page 1"));
	//	RunningApp.WaitForElement(q => q.Marked("Page 2"));
	//	RunningApp.WaitForElement(q => q.Button("Pop"));

	//	RunningApp.Screenshot("All elements present");
	//}

	//[Test]
	//[FailsOnIOS]
	//public void TabbedPageWithModalIssueTestsPopFromFirstTab()
	//{
	//	RunningApp.Tap(q => q.Button("Pop"));
	//	RunningApp.WaitForElement(q => q.Marked("Bug Repro's"));

	//	RunningApp.Screenshot("Popped from first tab");
	//}

	//[Test]
	//[FailsOnIOS]
	//public void TabbedPageWithModalIssueTestsPopFromSecondTab()
	//{
	//	RunningApp.Tap(q => q.Marked("Page 2"));
	//	RunningApp.WaitForElement(q => q.Button("Pop 2"));
	//	RunningApp.Screenshot("On second tab");

	//	RunningApp.Tap(q => q.Button("Pop 2"));
	//	RunningApp.WaitForElement(q => q.Marked("Bug Repro's"));
	//	RunningApp.Screenshot("Popped from second tab");
	//}
}