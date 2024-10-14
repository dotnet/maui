using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2964 : _IssuesUITest
{
	public Issue2964(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "TabbedPage toolbar item crash";

	// [Test]
	// [Category(UITestCategories.ToolbarItem)]
	// public void Issue2964Test()
	// {
	// 	RunningApp.Screenshot("I am at Issue 2964");

	// 	RunningApp.Tap(q => q.Marked("FlyoutButton"));
	// 	RunningApp.Screenshot("Create new Detail instance");

	// 	RunningApp.Tap(q => q.Marked("Page1PushModalButton"));
	// 	RunningApp.Screenshot("Modal is pushed");

	// 	RunningApp.Tap(q => q.Marked("ModalPagePopButton"));
	// 	RunningApp.Screenshot("Modal is popped");

	// 	RunningApp.WaitForElement(q => q.Marked("Page1Label"));
	// 	RunningApp.Screenshot("Didn't blow up! :)");
	// }
}