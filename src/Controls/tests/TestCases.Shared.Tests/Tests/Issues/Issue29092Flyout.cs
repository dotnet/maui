#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS
//IconImageSource is not properly updated in Android and Windows, so the test is not applicable for these platforms.	
//https://github.com/dotnet/maui/issues/15211
//https://github.com/dotnet/maui/issues/15211#issuecomment-1557569889
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