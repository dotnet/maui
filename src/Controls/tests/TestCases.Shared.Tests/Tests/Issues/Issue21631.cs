#if WINDOWS //This test case is only applicable for Windows because this test sample uses the Windows specific dpi path to load the image - appiconLogo.scale-100.png.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue21631 : _IssuesUITest
	{
		public Issue21631(TestDevice device) : base(device) { }

		public override string Issue => 
			"Injecting base tag in Webview2 works";

		[Test]
		[Category(UITestCategories.WebView)]
		public async Task NavigateToStringWithWebviewWorks()
		{
			App.WaitForElement("WaitForWebView");
			await Task.Delay(500);
			VerifyScreenshot();
		}
	}
}
#endif