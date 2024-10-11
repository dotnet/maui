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
	// 	RunningApp.Screenshot("I am at Issue 2927");
	// 	RunningApp.WaitForElement(q => q.Marked("Cell1 0"));
	// 	RunningApp.Tap(q => q.Marked("Cell1 0"));
	// 	RunningApp.WaitForElement(q => q.Marked("Cell1 1"));
	// 	RunningApp.Screenshot("Tapped Once");
	// 	RunningApp.Tap(q => q.Marked("Cell1 1"));
	// 	RunningApp.WaitForElement(q => q.Marked("Cell1 2"));
	// 	RunningApp.Screenshot("Tapped Twice");
	// 	RunningApp.Tap(q => q.Marked("Cell3 0"));
	// 	RunningApp.WaitForElement(q => q.Marked("Cell3 1"));
	// 	RunningApp.Screenshot("Click other cell");
	// 	RunningApp.Tap(q => q.Marked("Cell1 2"));
	// 	RunningApp.WaitForElement(q => q.Marked("Cell1 3"));
	// 	RunningApp.Screenshot("Click first cell again");
	// }
}