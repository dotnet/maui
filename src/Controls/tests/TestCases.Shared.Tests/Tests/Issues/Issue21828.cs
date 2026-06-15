using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21828 : _IssuesUITest
{
	public Issue21828(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Flyout icon disappears after root page replacement and pop";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void FlyoutIconRemainsVisible()
	{
		App.WaitForElement("InsertAndPopButton");
		App.Tap("InsertAndPopButton");
		App.WaitForElement("Page2Label");
		VerifyScreenshot();
	}
}