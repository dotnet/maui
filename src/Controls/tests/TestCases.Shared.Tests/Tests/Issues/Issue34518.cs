using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34518 : _IssuesUITest
{
	public Issue34518(TestDevice device) : base(device) { }

	public override string Issue => "WebView background color has changed after update, can't override";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewBackgroundColorShouldBeApplied()
	{
		App.WaitForElement("WebViewGreen");
		VerifyScreenshot();
	}
}
