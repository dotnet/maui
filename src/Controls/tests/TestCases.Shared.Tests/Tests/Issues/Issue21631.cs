#if WINDOWS
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
		[FailsOnWindowsWhenRunningOnXamarinUITest("WebView seems to be unreliably loading the image")]
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