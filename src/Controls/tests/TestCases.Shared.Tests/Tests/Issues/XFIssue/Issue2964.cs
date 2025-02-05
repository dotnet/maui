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

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void Issue2964Test()
	{
		App.WaitForElement("FlyoutButton");
		App.Tap("FlyoutButton");
		App.WaitForElement("Page1PushModalButton");
		App.Tap("Page1PushModalButton");
		App.WaitForElement("ModalPagePopButton");
		App.Tap("ModalPagePopButton");
		App.WaitForElement("Page1Label");

	}
}