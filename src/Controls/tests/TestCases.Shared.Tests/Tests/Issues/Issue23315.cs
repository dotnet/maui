using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23315(TestDevice device) : _IssuesUITest(device)
	{
		public override string Issue => "LoadFile in src/Core/src/Platform/iOS/MauiWKWebView.cs ignore directories";

		[Test]
		[Category(UITestCategories.WebView)]
		public void WebViewCanLoadFileFromSubdirectory()
		{
			App.WaitForElement("DescriptionLabel");
			Thread.Sleep(1000); // Allow time for the UI to settle before taking a screenshot
			VerifyScreenshot();
		}
	}
}
