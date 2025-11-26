#if TEST_FAILS_ON_WINDOWS // The transparent background color is not working on Windows, refer to https://github.com/microsoft/microsoft-ui-xaml/issues/6527
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28714 : _IssuesUITest
{
	public override string Issue => "[iOS] WebView BackgroundColor is not setting correctly";

	public Issue28714(TestDevice device)
	: base(device)
	{ }

	[Test, Order(1)]
	[Category(UITestCategories.WebView)]
	public void VerifyWebViewBackgroundColor()
	{
		App.WaitForElement("button");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.WebView)]
	public void VerifyWebViewDynamicBackgroundColor()
	{
		App.WaitForElement("button");
		App.Tap("button");
		VerifyScreenshot();
	}
}
#endif