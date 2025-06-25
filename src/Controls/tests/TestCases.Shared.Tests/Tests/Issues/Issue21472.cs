using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue21472 : _IssuesUITest
{
	public Issue21472(TestDevice testDevice) : base(testDevice)
	{
	}
	public override string Issue => "Shell FlyoutBackgroundImage doesn't shown";

	[Test, Order(1)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutBackgroundImage()
	{
		App.WaitForElement("button");
		App.ShowFlyout();
		App.WaitForElement("CloseFlyoutButton");
		VerifyScreenshot();
		App.Tap("CloseFlyoutButton");
	}

	[Test, Order(2)]
	[Category(UITestCategories.Shell)]
	public void VerifyShellFlyoutBackgroundImageSetNull()
	{
		App.WaitForElement("button");
		App.Tap("button");
		App.ShowFlyout();
		VerifyScreenshot();
	}
}