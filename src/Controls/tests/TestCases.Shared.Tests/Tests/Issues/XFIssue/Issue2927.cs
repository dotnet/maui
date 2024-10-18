using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2927 : _IssuesUITest
{
	public Issue2927(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "ListView item tapped not firing multiple times";

	// [Test]
	// [Category(UITestCategories.ListView)]
	// [FailsOnIOS]
	// public void Issue2927Test()
	// {
	// 	App.Screenshot("I am at Issue 2927");
	// 	App.WaitForElement(q => q.Marked("Cell1 0"));
	// 	App.Tap(q => q.Marked("Cell1 0"));
	// 	App.WaitForElement(q => q.Marked("Cell1 1"));
	// 	App.Screenshot("Tapped Once");
	// 	App.Tap(q => q.Marked("Cell1 1"));
	// 	App.WaitForElement(q => q.Marked("Cell1 2"));
	// 	App.Screenshot("Tapped Twice");
	// 	App.Tap(q => q.Marked("Cell3 0"));
	// 	App.WaitForElement(q => q.Marked("Cell3 1"));
	// 	App.Screenshot("Click other cell");
	// 	App.Tap(q => q.Marked("Cell1 2"));
	// 	App.WaitForElement(q => q.Marked("Cell1 3"));
	// 	App.Screenshot("Click first cell again");
	// }
}