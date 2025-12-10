using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32971 : _IssuesUITest
{
	public override string Issue => "WebView content does not scroll when placed inside a ScrollView";

	public Issue32971(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewShouldScrollInsideScrollView()
	{
		VerifyInternetConnectivity();
		App.WaitForElement("TestScrollView");

		// The test passes if we can scroll the WebView content inside a ScrollView.
		App.ScrollDown("TestScrollView");
	}
}