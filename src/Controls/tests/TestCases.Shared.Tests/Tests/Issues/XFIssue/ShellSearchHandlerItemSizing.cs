#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST // In Android and Catalyst AutomationId is not working for SearchHandler, in Windows searchresults was not shown More Information: https://github.com/dotnet/maui/issues/26477
using UITest.Appium;
using NUnit.Framework; 
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellSearchHandlerItemSizing : _IssuesUITest
{
	public ShellSearchHandlerItemSizing(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Search Handler Item Sizing";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerSizesCorrectly()
	{
		App.WaitForElement("Instructions");
		App.EnterText(AppiumQuery.ByXPath("//XCUIElementTypeSearchField"),"Hello");
		var contentSize = App.WaitForElement("searchresult").GetRect();
		Assert.That(contentSize.Height, Is.LessThan(100));

	}
}
#endif