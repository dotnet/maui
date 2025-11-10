using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32416 : _IssuesUITest
{
	public Issue32416(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Shell.FlyoutVerticalScrollMode Disabled does not disable scrolling";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void VerifyFlyoutVerticalScrollModeDisabled()
	{
		App.WaitForElement("Issue32416_Label");
		App.ShowFlyout();
		App.WaitForElement("Item 1");
		App.ScrollDown("Item 1", ScrollStrategy.Gesture, swipePercentage: 0.95, swipeSpeed: 100);
		VerifyScreenshot();
	}
}