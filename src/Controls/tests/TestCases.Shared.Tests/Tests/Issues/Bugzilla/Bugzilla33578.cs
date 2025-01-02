#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla33578 : _IssuesUITest
{
	public Bugzilla33578(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TableView EntryCell shows DefaultKeyboard, but after scrolling down and back a NumericKeyboard (";

	[Test]
	[Category(UITestCategories.TableView)]
	public void TableViewEntryCellShowsDefaultKeyboardThenNumericKeyboardAfterScrolling()
	{		
		App.ScrollDown("table");
		App.Tap(AppiumQuery.ByXPath("//XCUIElementTypeTextField[@value='0']"));
		App.WaitForElement("8");
		App.WaitForNoElement("A");
		App.DismissKeyboard();
		App.Tap(AppiumQuery.ByXPath("//XCUIElementTypeTextField[@value='Enter text here']"));
		App.ScrollUp("table");
		App.Tap(AppiumQuery.ByXPath("//XCUIElementTypeTextField[@value='Enter text here 1']"));
		App.Tap(AppiumQuery.ByXPath("//XCUIElementTypeTextField[@value='Enter text here 2']"));
		App.WaitForElement("A");
	}
}
#endif