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

	// TODO: Migration from Xamarin.UITest
	// Find out how to do this advanced stuff with Appium
	// [Test]
	// [Category(UITestCategories.TableView)]
	// [FailsOnIOSWhenRunningOnXamarinUITest]
	// public void TableViewEntryCellShowsDefaultKeyboardThenNumericKeyboardAfterScrolling()
	// {
	// 	App.ScrollDown("table");
	// 	App.ScrollDown("table");
	// 	App.Tap("entryNumeric");
	// 	var e = App.Query(c => c.Marked("0").Parent("UITextField").Index(0).Invoke("keyboardType"))[0];
	// 	//8 DecimalPad
	// 	Assert.AreEqual(8, e);
	// 	App.DismissKeyboard();
	// 	App.Tap(x => x.Marked("Enter text here").Index(0).Parent());
	// 	App.ScrollUp();
	// 	App.Tap(x => x.Marked("Enter text here 1"));
	// 	App.Tap(x => x.Marked("Enter text here 2").Index(0).Parent());
	// 	var e1 = App.Query(c => c.Marked("Enter text here 2").Parent("UITextField").Index(0).Invoke("keyboardType"))[0];
	// 	Assert.AreEqual(0, e1);
	// }
}
#endif