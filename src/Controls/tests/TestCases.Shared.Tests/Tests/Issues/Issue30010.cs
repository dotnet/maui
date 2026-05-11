#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30010 : _IssuesUITest
{
	public Issue30010(TestDevice device) : base(device)
	{
	}

	public override string Issue =>
		"Loading the captured screenshot from webview content to Image control does not visible";

	[Test]
	[Category(UITestCategories.WebView)]
	public void Issue30010_TakeScreenshotFunctionality()
	{
		// Wait for WebView to finish loading
		App.WaitForElement("StatusLabel");
		App.WaitForElement("TakeScreenshotButton");

		// The button is enabled only after WebView.Navigated fires
		App.WaitForElement("TakeScreenshotButton");
		App.Tap("TakeScreenshotButton");

		// Wait for the screenshot to be captured and displayed
		var statusLabel = App.WaitForElement("StatusLabel");
		App.WaitForElement("ResultImage");

		VerifyScreenshot("Issue30010TakeScreenshotFunctionality");
	}
}
#endif
