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
	[FlakyTest("Temporarily disabled due to flakiness in CI. Tracking issue will be created to re-enable.")]
	public void WebViewShouldNotMirrored()
	{
		App.WaitForElement("WebViewLabel");
		Thread.Sleep(3000);
		VerifyScreenshot();
	}
}