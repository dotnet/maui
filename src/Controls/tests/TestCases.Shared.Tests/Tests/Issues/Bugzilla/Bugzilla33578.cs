#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID
// As this test cases ensures the virutal keyboard so it is not applicable for desktop platforms.
// Need a reliable way to verify the keyboard type in the test. follow ups: https://github.com/dotnet/maui/issues/26968.
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
		App.ScrollDown("table");	
		WaitForEntryCell("0");
		TapEntryCell("0");
		VerifyScreenshot("TableViewEntryCellShowsNumberKeyboard");
		App.DismissKeyboard();
		TapEntryCell("Enter text here");

		// Due to SafeArea, ScrollUp will drag the notification window, causing a black window in screenshots
		// The first entry is already accessible without scrolling on iOS			
#if !IOS
		App.ScrollUp("table");
#endif

		WaitForEntryCell("Enter text here 1");
		TapEntryCell("Enter text here 1");
		TapEntryCell("Enter text here 2");
		VerifyScreenshot("TableViewEntryCellShowsDefaultKeyboard");
	}

	void WaitForEntryCell(string name)
	{
		// In iOS, find element by text is not working for EntryCell placed inside the TableView.
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