using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33772 : _IssuesUITest
{
	public Issue33772(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "Shell SearchHandler SearchBoxVisibility does not update when changed dynamically";
	protected override bool ResetAfterEachTest => true;

	IQuery SearchFieldQuery => App switch
	{
		AppiumAndroidApp => AppiumQuery.ByXPath("//android.widget.EditText"),
		AppiumWindowsApp => AppiumQuery.ByAccessibilityId("TextBox"),
		_ => AppiumQuery.ByXPath("//XCUIElementTypeSearchField")
	};

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerVisibilityChangesToExpanded()
	{
		App.WaitForElement("TitleLabel");
		App.Tap("ExpandButton");
		App.WaitForElement(() => App.FindElements(SearchFieldQuery).FirstOrDefault(e => e.IsDisplayed()));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerVisibilityChangesToCollapsible()
	{
		// Wait for the page to load
		App.WaitForElement("TitleLabel");
		App.Tap("ExpandButton");
		App.WaitForElement(() => App.FindElements(SearchFieldQuery).FirstOrDefault(e => e.IsDisplayed()));
		App.Tap("CollapsibleButton");
		VerifyScreenshot();
	}
}
