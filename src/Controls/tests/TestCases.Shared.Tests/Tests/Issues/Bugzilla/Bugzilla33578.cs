#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST // As this test cases ensures the virutal keyboard so it is not applicable for desktop platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Bugzilla33578 : _IssuesUITest
{
	public Bugzilla33578(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TableView EntryCell shows DefaultKeyboard, but after scrolling down and back a NumericKeyboard";

	[Test]
	[Category(UITestCategories.TableView)]
	public void TableViewEntryCellShowsDefaultKeyboardThenNumericKeyboardAfterScrolling()
	{
		WaitForEntryCell("Enter text here 1");
		App.ScrollDown("table");	
		WaitForEntryCell("0");
		TapEntryCell("0");
		VerifyScreenshot("TableViewEntryCellShowsNumberKeyboard");
		App.DismissKeyboard();
		TapEntryCell("Enter text here");
		App.ScrollUp("table");
		WaitForEntryCell("Enter text here 1");
		TapEntryCell("Enter text here 1");
		TapEntryCell("Enter text here 2");
		VerifyScreenshot("TableViewEntryCellShowsDefaultKeyboard");
	}

	void WaitForEntryCell(string name)
	{
#if ANDROID
		App.WaitForElement(name);
#else
		App.WaitForElement(AppiumQuery.ByXPath($"//XCUIElementTypeTextField[@value='{name}']"));
#endif
	}


	void TapEntryCell(string name)
	{
#if ANDROID
		App.Tap(name);
#else
		App.Tap(AppiumQuery.ByXPath($"//XCUIElementTypeTextField[@value='{name}']"));
#endif
	}
}
#endif