using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2809 : _IssuesUITest
{
	public Issue2809(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Secondary ToolbarItems cause app to hang during PushAsync";

	[Test]
	[Category(UITestCategories.ToolbarItem)]
	public void TestPageDoesntCrash()
	{
		App.WaitForMoreButton();
		App.TapMoreButton();
		App.Tap("Item 1");
	}
}