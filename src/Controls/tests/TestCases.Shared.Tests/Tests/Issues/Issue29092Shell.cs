#if ANDROID || IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29092Shell : _IssuesUITest
{
	public Issue29092Shell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell - Auto Resize chrome icons on iOS to make it more consistent with other platforms - hamburger icon";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShellFlyoutIconShouldAutoscale()
	{
		App.WaitForElement("HelloWorldLabel");
		VerifyScreenshot();
	}
}
#endif