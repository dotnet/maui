#if TEST_FAILS_ON_ANDROID // This test fails on Android because of user app theme is not responsive in Hostapp.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34823 : _IssuesUITest
{
	public Issue34823(TestDevice device) : base(device)
	{
	}

	protected override bool ResetAfterEachTest => true;

	public override string Issue => "WebView on Windows Does Not Inherit App Theme";

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewWithLightTheme()
	{
		App.WaitForElement("WebButton");
		App.Tap("WebButton");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewWithDarkTheme()
	{
		App.WaitForElement("WebButton");
		App.Tap("ThemeButton");
		App.Tap("WebButton");
		VerifyScreenshot();
	}
}
#endif