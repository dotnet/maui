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

	[Test]
	public void TabbedPageWithModalIssueTestsAllElementsPresent()
	{
		App.WaitForElement("Page 1");
		App.WaitForElement("Page 2");
		App.WaitForElement("Pop");
	}

	[Test]
	public void TabbedPageWithModalIssueTestsPopFromFirstTab()
	{
		App.Tap("Pop");
		App.WaitForElement("Bug Repro's");
	}

	[Test]
	public void TabbedPageWithModalIssueTestsPopFromSecondTab()
	{
		App.Tap("Page 2");
		App.WaitForElement("Pop 2");
		App.Screenshot("On second tab");

		App.Tap("Pop 2");
		App.WaitForElement("Bug Repro's");

	}
}