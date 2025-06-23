using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class ShellSearchHandlerItemSizing : _IssuesUITest
{
	public ShellSearchHandlerItemSizing(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Search Handler Item Sizing";

#if IOS || MACCATALYST
	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerSizesCorrectly()
	{
		App.WaitForElement("Instructions");
		App.EnterText(AppiumQuery.ByXPath("//XCUIElementTypeSearchField"), "Hello");
		var contentSize = App.WaitForElement("searchresult").GetRect();
		Assert.That(contentSize.Height, Is.LessThan(100));
	}
#endif

	// For Windows and Android, the test is failing because it cannot retrieve the search result.
	// Therefore, verify it using VerifyScreenshot.
#if ANDROID || WINDOWS
	[Test]
	[Category(UITestCategories.Shell)]
	public void VerifySearchHandlerItemsAreVisible()
	{
		App.WaitForElement("Instructions");
#if WINDOWS
		App.EnterText("TextBox", "Hello");
#else
		App.EnterText(AppiumQuery.ByXPath("//android.widget.EditText"), "Hello");
#endif
		VerifyScreenshot();
	}
#endif
}