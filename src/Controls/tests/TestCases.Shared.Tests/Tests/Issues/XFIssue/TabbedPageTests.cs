using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.TabbedPage)]
public class TabbedPageTests : _IssuesUITest
{
#if ANDROID
	const string Page1 = "PAGE 1";
	const string Page2 = "PAGE 2";
#else
	const string Page1 = "Page 1";
	const string Page2 = "Page 2";
#endif
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
		App.WaitForElement(Page1);
		App.WaitForElement(Page2);
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
		App.WaitForElement(Page2);
		App.Tap(Page2);
		App.WaitForElement(Pop2);

		App.Tap(Pop2);
		App.WaitForElement(HomePage);

	}
}