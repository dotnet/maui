using Microsoft.Maui.Controls;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;
namespace Microsoft.Maui.TestCases.Tests;

public class WebViewFeatureTests : UITest
{
	public const string WebViewFeatureMatrix = "WebView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string WebViewControl = "WebViewControl";
	public const string HtmlSourceButton = "HtmlSourceButton";
	public const string GithubUrlButton = "GithubUrlButton";
	public const string EvaluateJSButton = "EvaluateJSButton";
	public const string GoBackButton = "GoBackButton";
	public const string CanGoBackLabel = "CanGoBackLabel";
	public const string CanGoForwardLabel = "CanGoForwardLabel";
	public const string NavigatingStatusLabel = "NavigatingStatusLabel";
	public const string NavigatedStatusLabel = "NavigatedStatusLabel";
	public const string JSResultLabel = "JSResultLabel";
	public const string AddTestCookieButton = "AddTestCookieButton";
	public const string ClearCookiesButton = "ClearCookiesButton";
	public const string CookieStatusMainLabel = "CookieStatusMainLabel";
	public WebViewFeatureTests(TestDevice device)
		: base(device)
	{
	}
	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(WebViewFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.WebView)]
	public void WebView_ValidateDefaultValues_VerifyInitialState()
	{
		App.WaitForElement(Options);
		Assert.That(App.FindElement(CanGoBackLabel).GetText(), Is.EqualTo("False"));
		Assert.That(App.FindElement(CanGoForwardLabel).GetText(), Is.EqualTo("False"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/30381
	[Test, Order(2)]
	[Category(UITestCategories.WebView)]
	public void WebView_VerifyCanGoBackForward()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("HtmlSourceButton");
		App.Tap("HtmlSourceButton");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("MicrosoftUrlButton");
		App.Tap("MicrosoftUrlButton");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("GithubUrlButton");
		App.Tap("GithubUrlButton");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(CanGoBackLabel, timeout: TimeSpan.FromSeconds(3));
		Assert.That(App.FindElement(CanGoBackLabel).GetText(), Is.EqualTo("True"));
		App.WaitForElement(GoBackButton);
		App.Tap(GoBackButton);
		Thread.Sleep(2000); // Allow time to update the state
		App.WaitForElement(CanGoForwardLabel, timeout: TimeSpan.FromSeconds(5));
		Assert.That(App.FindElement(CanGoForwardLabel).GetText(), Is.EqualTo("True"));
	}
#endif

	[Test, Order(3)]
	[Category(UITestCategories.WebView)]
	public void WebView_SetHtmlSource_VerifyJavaScript()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HtmlSourceButton);
		App.Tap(HtmlSourceButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(EvaluateJSButton);
		App.Tap(EvaluateJSButton);
		App.WaitForElement(JSResultLabel);
		var jsResult = App.FindElement(JSResultLabel).GetText();
		Assert.That(jsResult, Is.EqualTo("JS Result: HTML WebView Source"));
	}

	[Test, Order(4)]
	[Category(UITestCategories.WebView)]
	public void WebView_SetUrlSource_VerifyNavigatingEvent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GithubUrlButton);
		App.Tap(GithubUrlButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		var navigatingText = App.FindElement(NavigatingStatusLabel).GetText();
		Assert.That(navigatingText, Is.Not.Null.And.Not.Empty);
	}

	[Test, Order(5)]
	[Category(UITestCategories.WebView)]
	public void WebView_SetUrlSource_VerifyNavigatedEvent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GithubUrlButton);
		App.Tap(GithubUrlButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		var navigatedText = App.FindElement(NavigatedStatusLabel).GetText();
		Assert.That(navigatedText, Is.EqualTo("Navigated: Success"));
	}

	[Test, Order(6)]
	[Category(UITestCategories.WebView)]
	public void WebView_SetHtmlSource_VerifyNavigatingEvent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HtmlSourceButton);
		App.Tap(HtmlSourceButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		var navigatingText = App.FindElement(NavigatingStatusLabel).GetText();
		Assert.That(navigatingText, Is.Not.Null.And.Not.Empty);
	}

	[Test, Order(7)]
	[Category(UITestCategories.WebView)]
	public void WebView_SetHtmlSource_VerifyNavigatedEvent()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HtmlSourceButton);
		App.Tap(HtmlSourceButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		var navigatedText = App.FindElement(NavigatedStatusLabel).GetText();
		Assert.That(navigatedText, Is.EqualTo("Navigated: Success"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_TestCookieManagement_VerifyAddCookie()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(AddTestCookieButton);
		App.Tap(AddTestCookieButton);
		App.WaitForElement(HtmlSourceButton);
		App.Tap(HtmlSourceButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		var cookiesStatusText = App.FindElement(CookieStatusMainLabel).GetText();
		Assert.That(cookiesStatusText, Does.Contain("Domain: localhost").And.Contain("Count: 1").And.Contain("DotNetMAUICookie = My cookie"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_TestCookieManagement_VerifyAddCookieWithUrlSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GithubUrlButton);
		App.Tap(GithubUrlButton);
		App.WaitForElement(AddTestCookieButton);
		App.Tap(AddTestCookieButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		var cookiesStatusText = App.FindElement(CookieStatusMainLabel).GetText();
		Assert.That(cookiesStatusText, Does.Contain("Domain: github.com").And.Contain("Count: 1").And.Contain("DotNetMAUICookie = My cookie"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_TestCookieManagement_VerifyAddCookieAndEvaluateJavaScript()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(AddTestCookieButton);
		App.Tap(AddTestCookieButton);
		App.WaitForElement(HtmlSourceButton);
		App.Tap(HtmlSourceButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		App.WaitForElement(EvaluateJSButton);
		App.Tap(EvaluateJSButton);
		App.WaitForElement(JSResultLabel);
		var jsResult = App.FindElement(JSResultLabel).GetText();
		Assert.That(jsResult, Is.EqualTo("JS Result: HTML WebView Source"));
		var cookiesStatusText = App.FindElement(CookieStatusMainLabel).GetText();
		Assert.That(cookiesStatusText, Does.Contain("Domain: localhost").And.Contain("Count: 1").And.Contain("DotNetMAUICookie = My cookie"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_TestClearCookies_VerifyCookiesCleared()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ClearCookiesButton);
		App.Tap(ClearCookiesButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		var clearCookiesText = App.FindElement(CookieStatusMainLabel).GetText();
		Assert.That(clearCookiesText, Is.EqualTo("No cookies available."));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_TestReloadMethod_VerifyReloadFunctionality()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GithubUrlButton);
		App.Tap(GithubUrlButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ReloadButton");
		App.Tap("ReloadButton");
		var navigatedText = App.FindElement(NavigatedStatusLabel).GetText();
		Assert.That(navigatedText, Is.EqualTo("Navigated: Success"));
	}

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // Issue Link: https://github.com/dotnet/maui/issues/30515
	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_VerifyReloadFunctionalityForHtmlWebViewSource()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(HtmlSourceButton);
		App.Tap(HtmlSourceButton);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("ReloadButton");
		App.Tap("ReloadButton");
		var navigatedText = App.FindElement(NavigatedStatusLabel).GetText();
		Assert.That(navigatedText, Is.EqualTo("Navigated: Success"));
	}
#endif

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_TestEvaluateJavaScriptAsync_VerifyJavaScriptExecution()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("LoadPage1Button");
		App.Tap("LoadPage1Button");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(EvaluateJSButton);
		App.Tap(EvaluateJSButton);
		App.WaitForElement(JSResultLabel);
		var jsResult = App.FindElement(JSResultLabel).GetText();
		Assert.That(jsResult, Is.EqualTo("JS Result: Navigation Test - Page 1"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_TestEvaluateJavaScriptAsync_VerifyJavaScriptExecutionWithMultiplePages()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("LoadMultiplePagesButton");
		App.Tap("LoadMultiplePagesButton");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(EvaluateJSButton);
		App.Tap(EvaluateJSButton);
		App.WaitForElement(JSResultLabel);
		var jsResult = App.FindElement(JSResultLabel).GetText();
		Assert.That(jsResult, Is.EqualTo("JS Result: Multiple Pages Navigation"));
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebView_SetIsVisibleFalse_VerifyWebViewHidden()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(GithubUrlButton);
		App.Tap(GithubUrlButton);
		App.WaitForElement("IsVisibleFalse");
		App.Tap("IsVisibleFalse");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options);
		App.WaitForNoElement(WebViewControl);
	}

#if TEST_FAILS_ON_WINDOWS // Issue Link: https://github.com/dotnet/maui/issues/29812
	[Test]
	[Category(UITestCategories.WebView)]
	public void VerifyWebViewWithShadow()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ShadowTrue");
		App.Tap("ShadowTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElementTillPageNavigationSettled(Options, timeout: TimeSpan.FromSeconds(3));
		VerifyScreenshot();
	}
#endif
}