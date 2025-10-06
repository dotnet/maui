#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/31869
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30575 : _IssuesUITest
{
	public Issue30575(TestDevice device) : base(device) { }

	public override string Issue => "FlowDirection RightToLeft causes mirrored content in WebView";

	[Test]
	[Category(UITestCategories.WebView)]
	[FlakyTest("Temporarily disabled due to flakiness in CI. Tracking issue: https://github.com/dotnet/maui/issues/31869")]
	public void WebViewShouldNotMirrored()
	{
		VerifyInternetConnectivity();
		App.WaitForElement("WebViewLabel");
		Thread.Sleep(3000);
		VerifyScreenshot();
	}
}
#endif