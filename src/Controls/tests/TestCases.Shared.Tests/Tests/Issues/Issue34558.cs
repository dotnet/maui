using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34558 : _IssuesUITest
{
	public Issue34558(TestDevice device) : base(device) { }

	public override string Issue => "[Windows] WebView renders blank when HybridWebView and WebView coexist in same app";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewLoadsWhenCoexistingWithHybridWebView()
	{
		App.WaitForElement("HybridStatusLabel");
		App.WaitForElement("RegularWebView");

		var rect = App.WaitForElement("RegularWebView").GetRect();
		Assert.That(rect.Height, Is.GreaterThan(0), "WebView should render with non-zero height");

		// "WebViewNavigatedLabel" is only added to the layout after the WebView successfully
		// navigates. Waiting for it confirms the WebView rendered content and was not blank.
		App.WaitForElement("WebViewNavigatedLabel");
	}
}
