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
		App.WaitForElement("TakeScreenshotButton");
		App.Tap("TakeScreenshotButton");

		Thread.Sleep(1000); // Pause briefly to allow the screenshot operation to complete

		VerifyScreenshot("Issue30010TakeScreenshotFunctionality");
	}
}
#endif