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
	// 	App.Screenshot("I am at Issue 2964");

	// 	App.Tap(q => q.Marked("FlyoutButton"));
	// 	App.Screenshot("Create new Detail instance");

	// 	App.Tap(q => q.Marked("Page1PushModalButton"));
	// 	App.Screenshot("Modal is pushed");

	// 	App.Tap(q => q.Marked("ModalPagePopButton"));
	// 	App.Screenshot("Modal is popped");

	// 	App.WaitForElement(q => q.Marked("Page1Label"));
	// 	App.Screenshot("Didn't blow up! :)");
	// }
}