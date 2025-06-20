#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29092Flyout : _IssuesUITest
{
	public Issue29092Flyout(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Flyout - Auto Resize chrome icons on iOS to make it more consistent with other platforms - hamburger icon";

	[Test]
	[Category(UITestCategories.FlyoutPage)]
	public void FlyoutIconShouldAutoscale()
	{
		App.WaitForElement("HelloWorldLabel");
		VerifyScreenshot();
	}
}
#endif