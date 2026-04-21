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
	}
}
